using System.IO;
using System.Reflection;
using Xunit;

namespace InsaneGenius.Utilities.Tests;

public class Iso6393Tests : IClassFixture<UtilitiesTests>
{
    [Fact]
    public void Create()
    {
        // Create full list of languages
        Iso6393 iso6393 = new();
        Assert.True(iso6393.Create());
    }

    [Fact]
    public void Load()
    {
        // Get the assembly directory
        var entryAssembly = Assembly.GetEntryAssembly();
        var assemblyDirectory = Path.GetDirectoryName(entryAssembly.Location);
        var dataDirectory = Path.GetFullPath(Path.Combine(assemblyDirectory, "../../../../Data"));
        var dataFile = Path.GetFullPath(Path.Combine(dataDirectory, "iso-639-3.tab"));

        // Load list of languages
        Iso6393 iso6393 = new();
        Assert.True(iso6393.Load(dataFile));
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
    [InlineData("yue", "Yue Chinese")]
    public void Succeed_Find(string input, string output)
    {
        // Create full list of languages
        Iso6393 iso6393 = new();
        Assert.True(iso6393.Create());

        // Find matching language
        var record = iso6393.Find(input, false);
        Assert.NotNull(record);
        Assert.Equal(record.RefName, output);
    }

    [Theory]
    [InlineData("xxx")]
    public void Fail_Find(string input) 
    {
        // Create full list of languages
        Iso6393 iso6393 = new();
        Assert.True(iso6393.Create());

        // Fail to find matching language
        var record = iso6393.Find(input, false);
        Assert.Null(record);
    }
}