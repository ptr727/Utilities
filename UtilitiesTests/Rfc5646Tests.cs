using PlexCleaner.Utilities;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace InsaneGenius.Utilities.Tests;

public class Rfc5646Tests : IClassFixture<UtilitiesTests>
{
    [Fact]
    public void Create()
    {
        // Create full list of languages
        Rfc5646 rfc5646 = new();
        Assert.True(rfc5646.Create());
    }

    [Fact]
    public void Load()
    {
        // Get the assembly directory
        var entryAssembly = Assembly.GetEntryAssembly();
        string assemblyDirectory = Path.GetDirectoryName(entryAssembly.Location);
        string dataDirectory = Path.GetFullPath(Path.Combine(assemblyDirectory, "../../../../Data"));
        string dataFile = Path.GetFullPath(Path.Combine(dataDirectory, "language-subtag-registry"));

        // Load list of languages
        Rfc5646 rfc5646 = new();
        Assert.True(rfc5646.Load(dataFile));
    }

    [Theory]
    [InlineData("af", "Afrikaans")]
    [InlineData("zh", "Chinese")]
    [InlineData("de", "German")]
    [InlineData("yue", "Yue Chinese")]
    [InlineData("zh-cmn-Hant", "Mandarin Chinese (Traditional)")]
    [InlineData("cmn-Hant", "Mandarin Chinese (Traditional)")]
    public void Succeed_Find(string input, string output)
    {
        // Create full list of languages
        Rfc5646 rfc5646 = new();
        Assert.True(rfc5646.Create());

        // Find matching language
        var record = rfc5646.Find(input, false);
        Assert.NotNull(record);
        Assert.Contains(record.Description, item => item.Equals(output, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("xxx")]
    public void Failed_From_String(string input)
    {
        // Create full list of languages
        Rfc5646 rfc5646 = new();
        Assert.True(rfc5646.Create());

        // Fail to find matching language
        var record = rfc5646.Find(input, false);
        Assert.Null(record);
    }
}