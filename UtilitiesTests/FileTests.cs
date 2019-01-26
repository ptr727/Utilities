using Xunit;

namespace InsaneGenius.Utilities.Tests
{
    public class FileTests
    {
        [Theory]
        [InlineData(@"C:\Path One", @"Path Two", @"C:\Path One\Path Two")]
        [InlineData(@"C:\Path One\", @"\Path Two", @"C:\Path One\Path Two")]
        [InlineData(@"C:\Path One\", @"/Path Two", @"C:\Path One\Path Two")]
        [InlineData(@"\\Server\Path One\", @"\Path Two", @"\\Server\Path One\Path Two")]
        public void CombinePath_RemoveRoots(string path1, string path2, string output)
        {
            // Combine the path
            string path = FileEx.CombinePath(path1, path2);
            Assert.Equal(path, output);
        }
    }
}
