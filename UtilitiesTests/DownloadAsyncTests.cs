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
        _ = size.Should().Be(LoopbackServer.Content.Length);
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
            _ = new FileInfo(tempFile).Length.Should().Be(LoopbackServer.Content.Length);
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
    public async Task GetContentInfoAsync_WithNotFoundUri_ShouldReturnFalse()
    {
        using LoopbackServer server = new();

        (bool success, long _, DateTime _) = await Download.GetContentInfoAsync(
            server.MissingUri,
            TestContext.Current.CancellationToken
        );

        _ = success.Should().BeFalse();
    }

    [Fact]
    public async Task DownloadAsync_WithCancellation_ShouldRespectCancellation()
    {
        using LoopbackServer server = new();
        using CancellationTokenSource cts = new();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        // The slow route outlasts the cancellation delay, so the token always wins the race.
        (bool Success, string _) = await Download.DownloadStringAsync(server.SlowUri, cts.Token);

        _ = Success.Should().BeFalse();
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
