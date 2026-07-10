namespace ptr727.Utilities.Tests;

public class DownloadAsyncTests
{
    [Fact]
    public async Task GetContentInfoAsync_WithValidUri_ShouldReturnSuccess()
    {
        Uri uri = new(
            "https://www.google.com/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png"
        );

        (bool success, long size, DateTime _) = await Download.GetContentInfoAsync(uri);

        _ = success.Should().BeTrue();
        _ = (size > 0).Should().BeTrue();
    }

    [Fact]
    public async Task DownloadStringAsync_WithValidUri_ShouldReturnContent()
    {
        Uri uri = new("https://www.google.com");

        (bool success, string? content) = await Download.DownloadStringAsync(uri);

        _ = success.Should().BeTrue();
        _ = content.Should().NotBeEmpty();
        _ = content.Should().ContainEquivalentOf("google");
    }

    [Fact]
    public async Task DownloadFileAsync_WithValidUri_ShouldCreateFile()
    {
        Uri uri = new(
            "https://www.google.com/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png"
        );
        string tempFile = Path.GetTempFileName();

        try
        {
            bool result = await Download.DownloadFileAsync(uri, tempFile);

            _ = result.Should().BeTrue();
            _ = File.Exists(tempFile).Should().BeTrue();
            _ = (new FileInfo(tempFile).Length > 0).Should().BeTrue();
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
    public async Task DownloadAsync_WithInvalidUri_ShouldReturnFalse()
    {
        Uri invalidUri = new("https://thisdoesnotexist123456789.com/file.txt");

        (bool success, long _, DateTime _) = await Download.GetContentInfoAsync(invalidUri);

        _ = success.Should().BeFalse();
    }

    [Fact]
    public async Task DownloadAsync_WithCancellation_ShouldRespectCancellation()
    {
        Uri uri = new("https://httpstat.us/200?sleep=5000"); // Slow endpoint
        using CancellationTokenSource cts = new();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100)); // Cancel after 100ms

        (bool Success, string _) = await Download.DownloadStringAsync(uri, cts.Token);

        // Should either throw cancellation or return false due to cancellation
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
