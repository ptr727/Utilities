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
    [InlineData("zho", "Chinese")]
    [InlineData("chi", "Chinese")]
    [InlineData("ger", "German")]
    [InlineData("deu", "German")]
    [InlineData("de", "German")]
    [InlineData("yue", "Yue Chinese")]
    public void Succeed_From_String(string input, string output)
    {
        // Create full list of languages
        List<Iso6393> iso6393List = Iso6393.Create();
        Assert.NotNull(iso6393List);

        // Find matching language
        Iso6393 language = Iso6393.FromString(input, iso6393List);
        Assert.NotNull(language);
        Assert.Equal(language.RefName, output);
    }

    [InlineData("xxx")]
    public void Failed_From_String(string input, string output) 
    {
        // Create full list of languages
        List<Iso6393> iso6393List = Iso6393.Create();
        Assert.NotNull(iso6393List);

        // Fail to find matching language
        Iso6393 language = Iso6393.FromString(input, iso6393List);
        Assert.Null(language);
    }
}