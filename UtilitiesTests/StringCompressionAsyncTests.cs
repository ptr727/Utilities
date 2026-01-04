using System;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace InsaneGenius.Utilities.Tests;

public class StringCompressionAsyncTests(UtilitiesTests fixture) : IClassFixture<UtilitiesTests>
{
    private readonly UtilitiesTests _fixture = fixture;

    [Fact]
    public async Task CompressDecompressAsync()
    {
        // Create random string
        Random random = new();
        int length = random.Next(64 * 1024);
        byte[] buffer = new byte[length];
        random.NextBytes(buffer);
        string text = Convert.ToBase64String(buffer);

        // Compress the string asynchronously
        string compressed = await text.CompressAsync();

        // Decompress asynchronously
        string decompressed = await compressed.DecompressAsync();

        // Compare to original string
        Assert.Equal(text, decompressed);
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

        Assert.Equal(text, decompressed);
    }

    [Fact]
    public async Task CompressAsync_WithCancellation_ShouldComplete()
    {
        // For in-memory operations, cancellation might complete before being checked
        // This test verifies the cancellation token parameter is accepted
        using CancellationTokenSource cts = new();
        string text = "Test string";

        string compressed = await text.CompressAsync(cancellationToken: cts.Token);

        Assert.NotNull(compressed);
        Assert.NotEmpty(compressed);
    }

    [Fact]
    public async Task DecompressAsync_WithInvalidBase64_ShouldThrow()
    {
        string invalidBase64 = "This is not valid Base64!";

        _ = await Assert.ThrowsAsync<FormatException>(async () =>
            await invalidBase64.DecompressAsync()
        );
    }

    [Fact]
    public async Task CompressAsync_WithNullString_ShouldThrowArgumentNullException()
    {
        string? nullString = null;

        _ = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await StringCompression.CompressAsync(nullString!)
        );
    }

    [Fact]
    public async Task DecompressAsync_WithNullString_ShouldThrowArgumentNullException()
    {
        string? nullString = null;

        _ = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await StringCompression.DecompressAsync(nullString!)
        );
    }

    [Fact]
    public async Task CompressAsync_LargeString_ShouldCompress()
    {
        // Create a large repetitive string (should compress well)
        string largeText = new('A', 1024 * 1024); // 1MB of 'A's

        string compressed = await largeText.CompressAsync();
        string decompressed = await compressed.DecompressAsync();

        Assert.Equal(largeText, decompressed);
        Assert.True(compressed.Length < largeText.Length, "Compressed size should be smaller");
    }
}
