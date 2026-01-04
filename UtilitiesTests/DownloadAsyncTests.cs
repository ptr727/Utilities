using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace InsaneGenius.Utilities.Tests;

public class DownloadAsyncTests(UtilitiesTests fixture) : IClassFixture<UtilitiesTests>
{
    private readonly UtilitiesTests _fixture = fixture;

    [Fact]
    public async Task GetContentInfoAsync_WithValidUri_ShouldReturnSuccess()
    {
        Uri uri = new(
            "https://www.google.com/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png"
        );

        (bool success, long size, DateTime _) = await Download.GetContentInfoAsync(uri);

        Assert.True(success);
        Assert.True(size > 0);
    }

    [Fact]
    public async Task DownloadStringAsync_WithValidUri_ShouldReturnContent()
    {
        Uri uri = new("https://www.google.com");

        (bool success, string? content) = await Download.DownloadStringAsync(uri);

        Assert.True(success);
        Assert.NotEmpty(content);
        Assert.Contains("google", content, StringComparison.OrdinalIgnoreCase);
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

            Assert.True(result);
            Assert.True(File.Exists(tempFile));
            Assert.True(new FileInfo(tempFile).Length > 0);
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

        Assert.False(success);
    }

    [Fact]
    public async Task DownloadAsync_WithCancellation_ShouldRespectCancellation()
    {
        Uri uri = new("https://httpstat.us/200?sleep=5000"); // Slow endpoint
        using CancellationTokenSource cts = new();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100)); // Cancel after 100ms

        (bool Success, string _) = await Download.DownloadStringAsync(uri, cts.Token);

        // Should either throw cancellation or return false due to cancellation
        Assert.False(Success);
    }

    [Fact]
    public async Task DownloadAsync_WithNullUri_ShouldThrowArgumentNullException()
    {
        Uri? nullUri = null;

        _ = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await Download.GetContentInfoAsync(nullUri!)
        );
    }

    [Fact]
    public void CreateUri_WithCredentials_ShouldIncludeCredentials()
    {
        string url = "https://example.com/resource";
        string username = "testuser";
        string password = "testpass";

        Uri result = Download.CreateUri(url, username, password);

        Assert.Contains(username, result.ToString());
    }

    [Fact]
    public void CreateUri_WithNullUrl_ShouldThrowArgumentNullException()
    {
        string? nullUrl = null;

        _ = Assert.Throws<ArgumentNullException>(() => Download.CreateUri(nullUrl!));
    }
}
