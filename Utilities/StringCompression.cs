using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InsaneGenius.Utilities;

// https://stackoverflow.com/questions/7343465/compression-decompression-string-with-c-sharp

/// <summary>
/// Provides string compression and decompression using Deflate algorithm.
/// </summary>
public static class StringCompression
{
    /// <summary>
    /// Compresses a string using Deflate compression and returns a Base64-encoded string.
    /// </summary>
    /// <param name="uncompressedString">The string to compress.</param>
    /// <param name="compressionLevel">The compression level to use. Default is <see cref="CompressionLevel.Optimal"/>.</param>
    /// <returns>A Base64-encoded compressed string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uncompressedString"/> is null.</exception>
    public static string Compress(
        string uncompressedString,
        CompressionLevel compressionLevel = CompressionLevel.Optimal
    )
    {
        ArgumentNullException.ThrowIfNull(uncompressedString);

        byte[] compressedBytes;
        using MemoryStream uncompressedStream = new(Encoding.UTF8.GetBytes(uncompressedString));
        using MemoryStream compressedStream = new();
        using (
            DeflateStream compressorStream = new(
                compressedStream,
                compressionLevel,
                leaveOpen: true
            )
        )
        {
            uncompressedStream.CopyTo(compressorStream);
        }
        compressedBytes = compressedStream.ToArray();

        return Convert.ToBase64String(compressedBytes);
    }

    /// <summary>
    /// Compresses a string asynchronously using Deflate compression and returns a Base64-encoded string.
    /// </summary>
    /// <param name="uncompressedString">The string to compress.</param>
    /// <param name="compressionLevel">The compression level to use. Default is <see cref="CompressionLevel.Optimal"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Base64-encoded compressed string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uncompressedString"/> is null.</exception>
    public static async Task<string> CompressAsync(
        string uncompressedString,
        CompressionLevel compressionLevel = CompressionLevel.Optimal,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(uncompressedString);

        byte[] compressedBytes;
        using MemoryStream uncompressedStream = new(Encoding.UTF8.GetBytes(uncompressedString));
        using MemoryStream compressedStream = new();
        await using (
            DeflateStream compressorStream = new(
                compressedStream,
                compressionLevel,
                leaveOpen: true
            )
        )
        {
            await uncompressedStream
                .CopyToAsync(compressorStream, cancellationToken)
                .ConfigureAwait(false);
        }
        compressedBytes = compressedStream.ToArray();

        return Convert.ToBase64String(compressedBytes);
    }

    /// <summary>
    /// Decompresses a Base64-encoded compressed string.
    /// </summary>
    /// <param name="compressedString">The Base64-encoded compressed string to decompress.</param>
    /// <returns>The decompressed string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="compressedString"/> is null.</exception>
    /// <exception cref="FormatException">Thrown when <paramref name="compressedString"/> is not a valid Base64 string.</exception>
    /// <exception cref="InvalidDataException">Thrown when the compressed data is invalid or corrupted.</exception>
    public static string Decompress(string compressedString)
    {
        ArgumentNullException.ThrowIfNull(compressedString);

        byte[] decompressedBytes;
        using MemoryStream compressedStream = new(Convert.FromBase64String(compressedString));
        using MemoryStream decompressedStream = new();
        using (DeflateStream decompressorStream = new(compressedStream, CompressionMode.Decompress))
        {
            decompressorStream.CopyTo(decompressedStream);
        }
        decompressedBytes = decompressedStream.ToArray();

        return Encoding.UTF8.GetString(decompressedBytes);
    }

    /// <summary>
    /// Decompresses a Base64-encoded compressed string asynchronously.
    /// </summary>
    /// <param name="compressedString">The Base64-encoded compressed string to decompress.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The decompressed string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="compressedString"/> is null.</exception>
    /// <exception cref="FormatException">Thrown when <paramref name="compressedString"/> is not a valid Base64 string.</exception>
    /// <exception cref="InvalidDataException">Thrown when the compressed data is invalid or corrupted.</exception>
    public static async Task<string> DecompressAsync(
        string compressedString,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(compressedString);

        byte[] decompressedBytes;
        using MemoryStream compressedStream = new(Convert.FromBase64String(compressedString));
        using MemoryStream decompressedStream = new();
        await using (
            DeflateStream decompressorStream = new(compressedStream, CompressionMode.Decompress)
        )
        {
            await decompressorStream
                .CopyToAsync(decompressedStream, cancellationToken)
                .ConfigureAwait(false);
        }
        decompressedBytes = decompressedStream.ToArray();

        return Encoding.UTF8.GetString(decompressedBytes);
    }
}
