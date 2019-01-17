using System.Collections.Generic;
using Xunit;

namespace InsaneGenius.Utilities.Tests
{
    public class Iso6393Tests
    {
        [Fact]
        public void Create_CreateIso6393List()
        {
            // Create full list of languages
            List<Iso6393> iso6393List = Iso6393.Create();
            Assert.NotNull(iso6393List);
        }

        [Theory]
        [InlineData("afr", "Afrikaans")]
        [InlineData("af", "Afrikaans")]
        public void FromString_GetLanguageFromString(string input, string output)
        {
            // Create full list of languages
            List<Iso6393> iso6393List = Iso6393.Create();
            Assert.NotNull(iso6393List);

            // Find matching language
            Iso6393 language = Iso6393.FromString(input, iso6393List);
            Assert.Equal(language.RefName, output);
        }
    }
}
