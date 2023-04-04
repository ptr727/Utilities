using PlexCleaner.Utilities;
using System.Collections.Generic;
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
    public void Succeed_From_String(string input, string output)
    {
        // Create full list of languages
        Iso6393 iso6393 = new();
        Assert.True(iso6393.Create());

        // Find matching language
        var record = iso6393.FromString(input);
        Assert.NotNull(record);
        Assert.Equal(record.RefName, output);
    }

    [Theory]
    [InlineData("xxx")]
    public void Failed_From_String(string input)
    {
        // Create full list of languages
        Iso6393 iso6393 = new();
        Assert.True(iso6393.Create());

        // Fail to find matching language
        var record = iso6393.FromString(input);
        Assert.Null(record);
    }
}