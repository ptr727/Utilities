using System.IO;
using System.Reflection;
using Xunit;

namespace InsaneGenius.Utilities.Tests;

public class Iso6392Tests : IClassFixture<UtilitiesTests>
{
    [Fact]
    public void Create()
    {
        // Create full list of languages
        Iso6392 iso6392 = new();
        Assert.True(iso6392.Create());
    }

    [Fact]
    public void Load()
    {
        // Get the assembly directory
        var entryAssembly = Assembly.GetEntryAssembly();
        var assemblyDirectory = Path.GetDirectoryName(entryAssembly.Location);
        var dataDirectory = Path.GetFullPath(Path.Combine(assemblyDirectory, "../../../../Data"));
        var dataFile = Path.GetFullPath(Path.Combine(dataDirectory, "ISO-639-2_utf-8.txt"));

        // Load list of languages
        Iso6392 iso6392 = new();
        Assert.True(iso6392.Load(dataFile));
    }

    [Theory]
    [InlineData("afr", "Afrikaans")]
    [InlineData("af", "Afrikaans")]
    [InlineData("zh", "Chinese")]
    [InlineData("zho", "Chinese")]
    [InlineData("chi", "Chinese")]
    [InlineData("ger", "German")]
    [InlineData("deu", "German")]
    [InlineData("de", "German")]
    [InlineData("cpe", "Creoles and pidgins, English based")]
    public void Succeed_Find(string input, string output)
    {
        // Create full list of languages
        Iso6392 iso6392 = new();
        Assert.True(iso6392.Create());

        // Find matching language
        var record = iso6392.Find(input, false);
        Assert.NotNull(record);
        Assert.Equal(record.RefName, output);
    }

    [Theory]
    [InlineData("xxx")]
    public void Fail_Find(string input)
    {
        // Create full list of languages
        Iso6392 iso6392 = new();
        Assert.True(iso6392.Create());

        // Fail to find matching language
        var record = iso6392.Find(input, false);
        Assert.Null(record);
    }
}