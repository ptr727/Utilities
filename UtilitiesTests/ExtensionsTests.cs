using System;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;
using Xunit;

namespace InsaneGenius.Utilities.Tests;

public class ExtensionsTests(UtilitiesTests fixture) : IClassFixture<UtilitiesTests>
{
    private readonly UtilitiesTests _fixture = fixture;

    #region String Compression Extension Tests

    [Fact]
    public void StringExtension_Compress_ShouldCompressString()
    {
        string original = "This is a test string that should be compressed successfully.";

        string compressed = original.Compress();

        Assert.NotNull(compressed);
        Assert.NotEmpty(compressed);
        Assert.NotEqual(original, compressed);
    }

    [Fact]
    public void StringExtension_Decompress_ShouldDecompressString()
    {
        string original = "This is a test string that should be compressed and then decompressed.";
        string compressed = original.Compress();

        string decompressed = compressed.Decompress();

        Assert.Equal(original, decompressed);
    }

    [Theory]
    [InlineData(CompressionLevel.Fastest)]
    [InlineData(CompressionLevel.Optimal)]
    [InlineData(CompressionLevel.SmallestSize)]
    public void StringExtension_Compress_WithDifferentLevels_ShouldWork(CompressionLevel level)
    {
        string original = "Test string for compression level testing.";

        string compressed = original.Compress(level);
        string decompressed = compressed.Decompress();

        Assert.Equal(original, decompressed);
    }

    [Fact]
    public async Task StringExtension_CompressAsync_ShouldCompressString()
    {
        string original = "This is a test string for async compression.";

        string compressed = await original.CompressAsync();

        Assert.NotNull(compressed);
        Assert.NotEmpty(compressed);
        Assert.NotEqual(original, compressed);
    }

    [Fact]
    public async Task StringExtension_DecompressAsync_ShouldDecompressString()
    {
        string original = "This is a test string for async decompression.";
        string compressed = await original.CompressAsync();

        string decompressed = await compressed.DecompressAsync();

        Assert.Equal(original, decompressed);
    }

    [Fact]
    public async Task StringExtension_CompressAsync_WithCancellation_ShouldRespectToken()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();
        string original = "Test string";

        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await original.CompressAsync(cancellationToken: cts.Token)
        );
    }

    [Fact]
    public void StringExtension_Compress_WithNullString_ShouldThrow()
    {
        string? nullString = null;

        _ = Assert.Throws<ArgumentNullException>(() => nullString!.Compress());
    }

    #endregion

    #region Logger Extension Tests

    [Fact]
    public void LoggerExtension_LogAndHandle_ShouldReturnTrue()
    {
        Logger logger = new LoggerConfiguration().CreateLogger();
        InvalidOperationException exception = new("Test exception");

        bool result = logger.LogAndHandle(exception);

        Assert.True(result);
    }

    [Fact]
    public void LoggerExtension_LogAndPropagate_ShouldReturnFalse()
    {
        Logger logger = new LoggerConfiguration().CreateLogger();
        InvalidOperationException exception = new("Test exception");

        bool result = logger.LogAndPropagate(exception);

        Assert.False(result);
    }

    [Fact]
    public void LoggerExtension_LogAndHandle_CanBeUsedInCatchWhen()
    {
        Logger logger = new LoggerConfiguration().CreateLogger();
        bool exceptionCaught;

        try
        {
            throw new InvalidOperationException("Test");
        }
        catch (Exception e) when (logger.LogAndHandle(e))
        {
            exceptionCaught = true;
        }

        Assert.True(exceptionCaught);
    }

    [Fact]
    public void LoggerExtension_LogAndPropagate_AllowsExceptionToBubble()
    {
        Logger logger = new LoggerConfiguration().CreateLogger();
        bool exceptionHandled;

        try
        {
            throw new InvalidOperationException("Test");
        }
        catch (Exception e) when (logger.LogAndPropagate(e))
        {
            // This block should not execute because LogAndPropagate returns false
            exceptionHandled = true;
        }
        catch (InvalidOperationException)
        {
            // Exception should propagate here since LogAndPropagate returns false
            exceptionHandled = false;
        }

        Assert.False(exceptionHandled);
    }

    [Fact]
    public void LoggerExtension_LogAndHandle_WithCustomFunction_ShouldLog()
    {
        Logger logger = new LoggerConfiguration().CreateLogger();
        InvalidOperationException exception = new("Test exception");

        // Test that it works with explicit function name
        bool result = logger.LogAndHandle(exception, "CustomFunction");

        Assert.True(result);
    }

    [Fact]
    public void LoggerExtension_LogAndPropagate_WithCustomFunction_ShouldLog()
    {
        Logger logger = new LoggerConfiguration().CreateLogger();
        InvalidOperationException exception = new("Test exception");

        // Test that it works with explicit function name
        bool result = logger.LogAndPropagate(exception, "CustomFunction");

        Assert.False(result);
    }

    #endregion

    #region Null Handling Tests

    [Fact]
    public void StringExtension_Decompress_WithNullString_ShouldThrow()
    {
        string? nullString = null;

        _ = Assert.Throws<ArgumentNullException>(() => nullString!.Decompress());
    }

    [Fact]
    public async Task StringExtension_CompressAsync_WithNullString_ShouldThrow()
    {
        string? nullString = null;

        _ = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await nullString!.CompressAsync()
        );
    }

    [Fact]
    public async Task StringExtension_DecompressAsync_WithNullString_ShouldThrow()
    {
        string? nullString = null;

        _ = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await nullString!.DecompressAsync()
        );
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void StringExtension_CompressDecompress_RoundTrip_ShouldPreserveData()
    {
        string[] testStrings =
        [
            "Short",
            "A medium length string for testing compression",
            new string('A', 10000), // Long repetitive string
            "Special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?",
            "Unicode: 你好世界 🌍🌎🌏",
            string.Empty,
        ];

        foreach (string original in testStrings)
        {
            if (string.IsNullOrEmpty(original))
            {
                continue;
            }

            string compressed = original.Compress();
            string decompressed = compressed.Decompress();

            Assert.Equal(original, decompressed);
        }
    }

    [Fact]
    public async Task StringExtension_CompressDecompressAsync_RoundTrip_ShouldPreserveData()
    {
        string[] testStrings =
        [
            "Short async",
            "A medium length string for async compression testing",
            new string('B', 10000), // Long repetitive string
            "Async special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?",
            "Async Unicode: 你好世界 🌍🌎🌏",
        ];

        foreach (string original in testStrings)
        {
            string compressed = await original.CompressAsync();
            string decompressed = await compressed.DecompressAsync();

            Assert.Equal(original, decompressed);
        }
    }

    #endregion
}
