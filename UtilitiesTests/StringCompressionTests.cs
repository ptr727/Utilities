using System.Security.Cryptography;

namespace ptr727.Utilities.Tests;

public class StringCompressionTests
{
    [Fact]
    public void CompressDecompress()
    {
        // Create random string
        int length = RandomNumberGenerator.GetInt32(64 * 1024);
        byte[] buffer = RandomNumberGenerator.GetBytes(length);
        string text = Convert.ToBase64String(buffer);

        // Compress the string
        string compressed = text.Compress();

        // Decompress
        string decompressed = compressed.Decompress();

        // Compare to original string
        _ = decompressed.Should().Be(text);
    }
}
