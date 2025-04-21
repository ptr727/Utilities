using System.Runtime.InteropServices;
using Xunit;

namespace InsaneGenius.Utilities.Tests;

public class FileTests(UtilitiesTests fixture) : IClassFixture<UtilitiesTests>
{
    private readonly UtilitiesTests _fixture = fixture;

    [Theory]
    [InlineData(@"C:\Path One", @"Path Two", @"C:\Path One\Path Two")]
    [InlineData(@"C:\Path One\", @"\Path Two", @"C:\Path One\Path Two")]
    [InlineData(@"C:\Path One\", @"/Path Two", @"C:\Path One\Path Two")]
    [InlineData(@"C:\Path One\Path Two\", @"..\Path Three\", @"C:\Path One\Path Three")]
    [InlineData(@"\\server\Path One\", @"\Path Two", @"\\server\Path One\Path Two")]
    [InlineData(@"\\server\Path One\Path Two\", @"..\Path Three", @"\\server\Path One\Path Three")]
    public void CombineAbsolutePath2(string path1, string path2, string output)
    {
        // This only work on Windows
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        string path = FileEx.CombinePath(path1, path2);
        Assert.Equal(path, output);
    }

    [Theory]
    [InlineData(@"C:\Path One", @"Path Two", @"foo.txt", @"C:\Path One\Path Two\foo.txt")]
    [InlineData(@"C:\Path One\", @"\Path Two", @"foo.txt", @"C:\Path One\Path Two\foo.txt")]
    [InlineData(@"C:\Path One\", @"/Path Two", @"foo.txt", @"C:\Path One\Path Two\foo.txt")]
    [InlineData(@"C:\Path One\Path Two\", @"..\Path Three\", @"foo.txt", @"C:\Path One\Path Three\foo.txt")]
    [InlineData(@"\\server\Path One\", @"\Path Two", @"foo.txt", @"\\server\Path One\Path Two\foo.txt")]
    [InlineData(@"\\server\Path One\Path Two\", @"..\Path Three", @"foo.txt", @"\\server\Path One\Path Three\foo.txt")]
    public void CombineAbsolutePath3(string path1, string path2, string path3, string output)
    {
        // This only work on Windows
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        string path = FileEx.CombinePath(path1, path2, path3);
        Assert.Equal(path, output);
    }
}
