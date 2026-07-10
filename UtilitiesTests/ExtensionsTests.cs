using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ptr727.Utilities.Tests;

public class ExtensionsTests
{
    #region String Compression Extension Tests

    [Fact]
    public void StringExtension_Compress_ShouldCompressString()
    {
        string original = "This is a test string that should be compressed successfully.";

        string compressed = original.Compress();

        _ = compressed.Should().NotBeNull();
        _ = compressed.Should().NotBeEmpty();
        _ = compressed.Should().NotBe(original);
    }

    [Fact]
    public void StringExtension_Decompress_ShouldDecompressString()
    {
        string original = "This is a test string that should be compressed and then decompressed.";
        string compressed = original.Compress();

        string decompressed = compressed.Decompress();

        _ = decompressed.Should().Be(original);
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

        _ = decompressed.Should().Be(original);
    }

    [Fact]
    public async Task StringExtension_CompressAsync_ShouldCompressString()
    {
        string original = "This is a test string for async compression.";

        string compressed = await original.CompressAsync();

        _ = compressed.Should().NotBeNull();
        _ = compressed.Should().NotBeEmpty();
        _ = compressed.Should().NotBe(original);
    }

    [Fact]
    public async Task StringExtension_DecompressAsync_ShouldDecompressString()
    {
        string original = "This is a test string for async decompression.";
        string compressed = await original.CompressAsync();

        string decompressed = await compressed.DecompressAsync();

        _ = decompressed.Should().Be(original);
    }

    [Fact]
    public async Task StringExtension_CompressAsync_WithCancellation_ShouldRespectToken()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();
        string original = "Test string";

        _ = await FluentActions
            .Awaiting(() => original.CompressAsync(cancellationToken: cts.Token))
            .Should()
            .ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void StringExtension_Compress_WithNullString_ShouldThrow()
    {
        string? nullString = null;

        _ = FluentActions
            .Invoking(() => nullString.Compress())
            .Should()
            .Throw<ArgumentNullException>();
    }

    #endregion

    #region Logger Extension Tests

    [Fact]
    public void LoggerExtension_LogAndHandle_ShouldReturnTrue()
    {
        ILogger logger = NullLogger.Instance;
        InvalidOperationException exception = new("Test exception");

        bool result = logger.LogAndHandle(exception);

        _ = result.Should().BeTrue();
    }

    [Fact]
    public void LoggerExtension_LogAndPropagate_ShouldReturnFalse()
    {
        ILogger logger = NullLogger.Instance;
        InvalidOperationException exception = new("Test exception");

        bool result = logger.LogAndPropagate(exception);

        _ = result.Should().BeFalse();
    }

    [Fact]
    public void LoggerExtension_LogAndHandle_CanBeUsedInCatchWhen()
    {
        ILogger logger = NullLogger.Instance;
        bool exceptionCaught;

        try
        {
            throw new InvalidOperationException("Test");
        }
        catch (Exception e) when (logger.LogAndHandle(e))
        {
            exceptionCaught = true;
        }

        _ = exceptionCaught.Should().BeTrue();
    }

    [Fact]
    public void LoggerExtension_LogAndPropagate_AllowsExceptionToBubble()
    {
        ILogger logger = NullLogger.Instance;
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

        _ = exceptionHandled.Should().BeFalse();
    }

    [Fact]
    public void LoggerExtension_LogAndHandle_WithCustomFunction_ShouldLog()
    {
        ILogger logger = NullLogger.Instance;
        InvalidOperationException exception = new("Test exception");

        // Test that it works with explicit function name
        bool result = logger.LogAndHandle(exception, "CustomFunction");

        _ = result.Should().BeTrue();
    }

    [Fact]
    public void LoggerExtension_LogAndPropagate_WithCustomFunction_ShouldLog()
    {
        ILogger logger = NullLogger.Instance;
        InvalidOperationException exception = new("Test exception");

        // Test that it works with explicit function name
        bool result = logger.LogAndPropagate(exception, "CustomFunction");

        _ = result.Should().BeFalse();
    }

    #endregion

    #region Null Handling Tests

    [Fact]
    public void StringExtension_Decompress_WithNullString_ShouldThrow()
    {
        string? nullString = null;

        _ = FluentActions
            .Invoking(() => nullString!.Decompress())
            .Should()
            .Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task StringExtension_CompressAsync_WithNullString_ShouldThrow()
    {
        string? nullString = null;

        _ = await FluentActions
            .Awaiting(() => nullString.CompressAsync())
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StringExtension_DecompressAsync_WithNullString_ShouldThrow()
    {
        string? nullString = null;

        _ = await FluentActions
            .Awaiting(() => nullString.DecompressAsync())
            .Should()
            .ThrowAsync<ArgumentNullException>();
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

            _ = decompressed.Should().Be(original);
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

            _ = decompressed.Should().Be(original);
        }
    }

    #endregion
}
