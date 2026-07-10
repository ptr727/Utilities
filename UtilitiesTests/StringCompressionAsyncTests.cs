using System.IO.Compression;
using System.Security.Cryptography;

namespace ptr727.Utilities.Tests;

public class StringCompressionAsyncTests
{
    [Fact]
    public async Task CompressDecompressAsync()
    {
        // Create random string
        int length = RandomNumberGenerator.GetInt32(64 * 1024);
        byte[] buffer = RandomNumberGenerator.GetBytes(length);
        string text = Convert.ToBase64String(buffer);

        // Compress the string asynchronously
        string compressed = await text.CompressAsync();

        // Decompress asynchronously
        string decompressed = await compressed.DecompressAsync();

        // Compare to original string
        _ = decompressed.Should().Be(text);
    }

    [Theory]
    [InlineData(CompressionLevel.Fastest)]
    [InlineData(CompressionLevel.Optimal)]
    [InlineData(CompressionLevel.SmallestSize)]
    public async Task CompressAsync_WithDifferentLevels_ShouldSucceed(CompressionLevel level)
    {
        string text =
            "This is a test string that will be compressed with different compression levels.";

        string compressed = await text.CompressAsync(level);
        string decompressed = await compressed.DecompressAsync();

        _ = decompressed.Should().Be(text);
    }

    [Fact]
    public async Task CompressAsync_WithCancellation_ShouldComplete()
    {
        // For in-memory operations, cancellation might complete before being checked
        // This test verifies the cancellation token parameter is accepted
        using CancellationTokenSource cts = new();
        string text = "Test string";

        string compressed = await text.CompressAsync(cancellationToken: cts.Token);

        _ = compressed.Should().NotBeNull();
        _ = compressed.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DecompressAsync_WithInvalidBase64_ShouldThrow()
    {
        string invalidBase64 = "This is not valid Base64!";

        _ = await FluentActions
            .Awaiting(() => invalidBase64.DecompressAsync())
            .Should()
            .ThrowAsync<FormatException>();
    }

    [Fact]
    public async Task CompressAsync_WithNullString_ShouldThrowArgumentNullException()
    {
        string? nullString = null;

        _ = await FluentActions
            .Awaiting(() => StringCompression.CompressAsync(nullString!))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DecompressAsync_WithNullString_ShouldThrowArgumentNullException()
    {
        string? nullString = null;

        _ = await FluentActions
            .Awaiting(() => StringCompression.DecompressAsync(nullString!))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CompressAsync_LargeString_ShouldCompress()
    {
        // Create a large repetitive string (should compress well)
        string largeText = new('A', 1024 * 1024); // 1MB of 'A's

        string compressed = await largeText.CompressAsync();
        string decompressed = await compressed.DecompressAsync();

        _ = decompressed.Should().Be(largeText);
        _ = (compressed.Length < largeText.Length)
            .Should()
            .BeTrue("Compressed size should be smaller");
    }
}
