using System.Collections.Generic;
using Xunit;

namespace InsaneGenius.Utilities.Tests;

public class Iso6393Tests
{
    [Fact]
    public void Create()
    {
        // Create full list of languages
        List<Iso6393> iso6393List = Iso6393.Create();
        Assert.NotNull(iso6393List);
    }

    [Theory]
    [InlineData("afr", "Afrikaans")]
    [InlineData("af", "Afrikaans")]
    [InlineData("zh", "Chinese")]
    [InlineData("ger", "German")]
    [InlineData("zh-CHS", "Chinese")]
    [InlineData("zh-Hans", "Chinese")]
    [InlineData("zh-CHT", "Chinese")]
    [InlineData("zh-Hant", "Chinese")]
    [InlineData("zho", "Chinese")]
    [InlineData("chi", "Chinese")]
    [InlineData("cmn-Hans", "Mandarin Chinese")]
    [InlineData("cmn-Hant", "Mandarin Chinese")]
    [InlineData("yue", "Yue Chinese")]
    [InlineData("yue-Hant", "Yue Chinese")]
    public void FromString(string input, string output)
    {
        // Create full list of languages
        List<Iso6393> iso6393List = Iso6393.Create();
        Assert.NotNull(iso6393List);

        // Find matching language
        Iso6393 language = Iso6393.FromString(input, iso6393List);
        Assert.NotNull(language);
        Assert.Equal(language.RefName, output);
    }
}