using System.Diagnostics;

namespace ptr727.Utilities.Tests;

public class DownloadAsyncTests
{
    [Fact]
    public async Task GetContentInfoAsync_WithValidUri_ShouldReturnSuccess()
    {
        using LoopbackServer server = new();

        (bool success, long size, DateTime _) = await Download.GetContentInfoAsync(
            server.OkUri,
            TestContext.Current.CancellationToken
        );

        _ = success.Should().BeTrue();
        _ = size.Should().Be(LoopbackServer.ContentLength);
    }

    [Fact]
    public async Task DownloadStringAsync_WithValidUri_ShouldReturnContent()
    {
        using LoopbackServer server = new();

        (bool success, string? content) = await Download.DownloadStringAsync(
            server.OkUri,
            TestContext.Current.CancellationToken
        );

        _ = success.Should().BeTrue();
        _ = content.Should().Be(LoopbackServer.Content);
    }

    [Fact]
    public async Task DownloadFileAsync_WithValidUri_ShouldCreateFile()
    {
        using LoopbackServer server = new();
        string tempFile = Path.GetTempFileName();

        try
        {
            bool result = await Download.DownloadFileAsync(
                server.OkUri,
                tempFile,
                TestContext.Current.CancellationToken
            );

            _ = result.Should().BeTrue();
            _ = File.Exists(tempFile).Should().BeTrue();
            _ = new FileInfo(tempFile).Length.Should().Be(LoopbackServer.ContentLength);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task DownloadFileAsync_OverALongerExistingFile_ShouldReplaceIt()
    {
        using LoopbackServer server = new();
        string tempFile = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(
                tempFile,
                new string('x', LoopbackServer.ContentLength * 4),
                TestContext.Current.CancellationToken
            );

            bool result = await Download.DownloadFileAsync(
                server.OkUri,
                tempFile,
                TestContext.Current.CancellationToken
            );

            _ = result.Should().BeTrue();

            // Opening rather than creating the file would leave the seeded bytes after the body.
            string written = await File.ReadAllTextAsync(
                tempFile,
                TestContext.Current.CancellationToken
            );
            _ = written.Should().Be(LoopbackServer.Content);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task DownloadFileAsync_WhenTheRequestFails_ShouldLeaveTheDestination()
    {
        using LoopbackServer server = new();
        string tempFile = Path.GetTempFileName();
        string existing = new('x', LoopbackServer.ContentLength * 4);

        try
        {
            await File.WriteAllTextAsync(tempFile, existing, TestContext.Current.CancellationToken);

            bool result = await Download.DownloadFileAsync(
                server.MissingUri,
                tempFile,
                TestContext.Current.CancellationToken
            );

            _ = result.Should().BeFalse();

            // The destination is opened only once the response is accepted.
            // A request that never gets that far leaves it alone.
            string written = await File.ReadAllTextAsync(
                tempFile,
                TestContext.Current.CancellationToken
            );
            _ = written.Should().Be(existing);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task DownloadFileAsync_WithAnUnusableDestination_ShouldReturnFalse()
    {
        using LoopbackServer server = new();
        string missingDirectory = Path.Combine(
            Path.GetTempPath(),
            Path.GetRandomFileName(),
            "file.txt"
        );

        // A destination that cannot hold the file reports failure rather than throwing.
        bool result = await Download.DownloadFileAsync(
            server.OkUri,
            missingDirectory,
            TestContext.Current.CancellationToken
        );

        _ = result.Should().BeFalse();

        // The body is fetched before the destination is opened.
        // The served request is what proves the failure came from the file rather than the request.
        _ = server.RequestCount.Should().Be(1);
    }

    [Fact]
    public async Task GetContentInfoAsync_WithNotFoundUri_ShouldReturnFalse()
    {
        using LoopbackServer server = new();

        (bool success, long _, DateTime _) = await Download.GetContentInfoAsync(
            server.MissingUri,
            TestContext.Current.CancellationToken
        );

        _ = success.Should().BeFalse();

        // A refused connection returns false too, so the failure is the route's only if it ran.
        _ = server.RequestCount.Should().Be(1);
    }

    [Fact]
    public async Task DownloadAsync_ConcurrentRequests_ShouldBothComplete()
    {
        using LoopbackServer server = new();

        // One request must never queue behind another on the server's accept loop.
        Task<(bool Success, string Value)> first = Download.DownloadStringAsync(
            server.OkUri,
            TestContext.Current.CancellationToken
        );
        Task<(bool Success, string Value)> second = Download.DownloadStringAsync(
            server.OkUri,
            TestContext.Current.CancellationToken
        );

        (bool Success, string Value)[] results = await Task.WhenAll(first, second)
            .WaitAsync(s_signalTimeout, TestContext.Current.CancellationToken);

        _ = results.Should().AllSatisfy(result => result.Success.Should().BeTrue());
        _ = server.RequestCount.Should().Be(2);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2007:Consider calling ConfigureAwait on the awaited task",
        Justification = "xUnit1030 forbids ConfigureAwait in a test method, and the request has to be started before it is cancelled."
    )]
    [Fact]
    public async Task DownloadAsync_WithCancellation_ShouldRespectCancellation()
    {
        using LoopbackServer server = new();
        using CancellationTokenSource cts = new();

        Task<(bool Success, string Value)> download = Download.DownloadStringAsync(
            server.SlowUri,
            cts.Token
        );

        // Cancelling only once the server holds the request proves the route was reached.
        // Any other failure would produce the same false result and prove nothing.
        await server.SlowRequestStarted.WaitAsync(
            s_signalTimeout,
            TestContext.Current.CancellationToken
        );
        long startedAt = Stopwatch.GetTimestamp();
        await cts.CancelAsync();

        (bool success, string _) = await download;
        TimeSpan elapsed = Stopwatch.GetElapsedTime(startedAt);

        _ = success.Should().BeFalse();

        // Returning far inside the route's own delay separates a cancelled request from a served one.
        _ = elapsed.Should().BeLessThan(LoopbackServer.SlowResponseDelay / 2);
    }

    [Fact]
    public async Task DownloadAsync_WithNullUri_ShouldThrowArgumentNullException()
    {
        Uri? nullUri = null;

        _ = await FluentActions
            .Awaiting(() => Download.GetContentInfoAsync(nullUri!))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    // A signal that never arrives is a defect to report, not a test left hanging in CI.
    private static readonly TimeSpan s_signalTimeout = TimeSpan.FromSeconds(30);

    [Fact]
    public void CreateUri_WithCredentials_ShouldIncludeCredentials()
    {
        string url = "https://example.com/resource";
        string username = "testuser";
        string password = "testpass";

        Uri result = Download.CreateUri(url, username, password);

        _ = result.ToString().Should().Contain(username);
    }

    [Fact]
    public void CreateUri_WithNullUrl_ShouldThrowArgumentNullException()
    {
        string? nullUrl = null;

        _ = FluentActions
            .Invoking(() => Download.CreateUri(nullUrl!))
            .Should()
            .Throw<ArgumentNullException>();
    }
}
