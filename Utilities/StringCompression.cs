using System.IO;
using System.IO.Compression;
using System.Text;

namespace InsaneGenius.Utilities;

// https://stackoverflow.com/questions/7343465/compression-decompression-string-with-c-sharp

public static class StringCompression
{
    public static string Compress(string uncompressedString)
    {
        byte[] compressedBytes;
        using MemoryStream uncompressedStream = new(Encoding.UTF8.GetBytes(uncompressedString));
        {
            using MemoryStream compressedStream = new();
            {
                using DeflateStream compressorStream = new(
                    compressedStream,
                    CompressionMode.Compress
                );
                {
                    uncompressedStream.CopyTo(compressorStream);
                }
            }
            compressedBytes = compressedStream.ToArray();
        }

        return System.Convert.ToBase64String(compressedBytes);
    }

    public static string Decompress(string compressedString)
    {
        byte[] decompressedBytes;
        using MemoryStream compressedStream = new(
            System.Convert.FromBase64String(compressedString)
        );
        {
            using DeflateStream decompressorStream = new(
                compressedStream,
                CompressionMode.Decompress
            );
            {
                using MemoryStream decompressedStream = new();
                {
                    decompressorStream.CopyTo(decompressedStream);
                    decompressedBytes = decompressedStream.ToArray();
                }
            }
        }

        return Encoding.UTF8.GetString(decompressedBytes);
    }
}
