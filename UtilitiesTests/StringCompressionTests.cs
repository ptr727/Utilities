using System;
using System.Text;
using Xunit;

namespace InsaneGenius.Utilities.Tests
{
    public class StringCompressionTests
    {
        [Fact]
        public void CompressDecompress()
        {
            // Create random string
            Random random = new Random();
            int length = random.Next(64 * 1024);
            byte[] buffer = new byte[length];
            random.NextBytes(buffer);
            string text = System.Convert.ToBase64String(buffer);

            // Compress the string
            string compressed = text.Compress();

            // Decompress
            string decompressed = compressed.Decompress();

            // Compare to original string
            Assert.Equal(text, decompressed);
        }
    }
}
