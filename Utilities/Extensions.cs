using System;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace InsaneGenius.Utilities;

/// <summary>
/// Provides extension methods for string compression and logger error handling.
/// </summary>
#pragma warning disable CA1708 // Identifiers should differ by more than case
public static class Extensions
#pragma warning restore CA1708 // Identifiers should differ by more than case
{
    /// <summary>
    /// Extension methods for string compression and decompression.
    /// </summary>
    extension(string uncompressedString)
    {
        /// <summary>
        /// Compresses the string using Deflate compression.
        /// </summary>
        /// <param name="compressionLevel">The compression level to use.</param>
        /// <returns>A Base64-encoded compressed string.</returns>
        public string Compress(CompressionLevel compressionLevel = CompressionLevel.Optimal) =>
            StringCompression.Compress(uncompressedString, compressionLevel);

        /// <summary>
        /// Compresses the string asynchronously using Deflate compression.
        /// </summary>
        /// <param name="compressionLevel">The compression level to use.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Base64-encoded compressed string.</returns>
        public Task<string> CompressAsync(
            CompressionLevel compressionLevel = CompressionLevel.Optimal,
            CancellationToken cancellationToken = default
        ) =>
            StringCompression.CompressAsync(
                uncompressedString,
                compressionLevel,
                cancellationToken
            );

        /// <summary>
        /// Decompresses a Base64-encoded compressed string.
        /// </summary>
        /// <returns>The decompressed string.</returns>
        public string Decompress() => StringCompression.Decompress(uncompressedString);

        /// <summary>
        /// Decompresses a Base64-encoded compressed string asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The decompressed string.</returns>
        public Task<string> DecompressAsync(CancellationToken cancellationToken = default) =>
            StringCompression.DecompressAsync(uncompressedString, cancellationToken);
    }

    /// <summary>
    /// Extension methods for Serilog ILogger error handling.
    /// </summary>
    extension(ILogger logger)
    {
        /// <summary>
        /// Logs an exception and returns false to allow error propagation in catch-when clauses.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="function">The function name (automatically captured).</param>
        /// <returns>Always returns false to allow exception to propagate.</returns>
        public bool LogAndPropagate(
            Exception exception,
            [System.Runtime.CompilerServices.CallerMemberName] string function = "unknown"
        )
        {
            logger.Error(exception, "{Function}", function);
            return false;
        }

        /// <summary>
        /// Logs an exception and returns true to indicate exception handling in catch-when clauses.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="function">The function name (automatically captured).</param>
        /// <returns>Always returns true to indicate exception was handled.</returns>
        public bool LogAndHandle(
            Exception exception,
            [System.Runtime.CompilerServices.CallerMemberName] string function = "unknown"
        )
        {
            logger.Error(exception, "{Function}", function);
            return true;
        }
    }
}
