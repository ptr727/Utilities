using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace InsaneGenius.Utilities;

// ISO-639-3 data source
// http://www-01.sil.org/iso639-3
// https://iso639-3.sil.org/sites/iso639-3/files/downloads/iso-639-3.tab

// IETF language structure references
// https://en.wikipedia.org/wiki/IETF_language_tag
// https://www.rfc-editor.org/info/bcp47
// https://www.rfc-editor.org/info/rfc4647
// https://www.rfc-editor.org/info/rfc5646

// RFC 5646 data sources
// https://www.iana.org/assignments/language-tags/language-tags.xml
// https://www.iana.org/assignments/language-subtag-registry/language-subtag-registry
// https://github.com/mattcg/language-subtag-registry
// https://github.com/DanSmith/languagetags-sharp

// T4 template
// https://docs.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates
// https://github.com/mono/t4
// wget -O ISO-639-3.tab https://iso639-3.sil.org/sites/iso639-3/files/downloads/iso-639-3.tab
// dotnet tool install -g dotnet-t4
// t4 --out=ISO-639-3.cs .\ISO-639-3.tt

// ISO 639-3 class
// Create() method is generated from ISO-639-3.tt
public partial class Iso6393
{
    // The three-letter 639-3 identifier
    public string Id { get; set; }
    // Equivalent 639-2 identifier of the bibliographic applications code set, if there is one
    public string Part2B { get; set; }
    // Equivalent 639-2 identifier of the terminology applications code set, if there is one
    public string Part2T { get; set; }
    // Equivalent 639-1 identifier, if there is one
    public string Part1 { get; set; }
    // I(ndividual), M(acrolanguage), S(pecial)
    public string Scope { get; set; }
    // A(ncient), C(onstructed), E(xtinct), H(istorical), L(iving), S(pecial)
    public string LanguageType { get; set; }
    // Reference language name
    public string RefName { get; set; }
    // Comment relating to one or more of the columns
    public string Comment { get; set; }

    public static Iso6393 FromString(string language, List<Iso6393> iso6393List)
    {
        if (string.IsNullOrEmpty(language))
            throw new ArgumentNullException(nameof(language));

        // Find the matching language entry
        Iso6393 iso6393Lang;

        // 693 3 letter form
        // E.g. zho, chi, afr, ger
        if (language.Length == 3)
        {
            // Try 639-3
            iso6393Lang = iso6393List.FirstOrDefault(item => item.Id.Equals(language, StringComparison.OrdinalIgnoreCase));
            if (iso6393Lang != null)
                return iso6393Lang;

            // Try the 639-2/B
            iso6393Lang = iso6393List.FirstOrDefault(item => item.Part2B.Equals(language, StringComparison.OrdinalIgnoreCase));
            if (iso6393Lang != null)
                return iso6393Lang;

            // Try the 639-2/T
            iso6393Lang = iso6393List.FirstOrDefault(item => item.Part2T.Equals(language, StringComparison.OrdinalIgnoreCase));
            if (iso6393Lang != null)
                return iso6393Lang;
        }

        // 693 2 letter
        // E.g. zh, af, de
        if (language.Length == 2)
        {
            // Try 639-1
            iso6393Lang = iso6393List.FirstOrDefault(item => item.Part1.Equals(language, StringComparison.OrdinalIgnoreCase));
            if (iso6393Lang != null)
                return iso6393Lang;
        }

        // Long form
        // E.g. Zambian Sign Language, Zul
        // Try long form
        iso6393Lang = iso6393List.FirstOrDefault(item => item.RefName.Equals(language, StringComparison.OrdinalIgnoreCase));
        if (iso6393Lang != null)
            return iso6393Lang;

        // Try to lookup from CultureInfo
        // E.g. en-US, zh-Hans
        try
        {
            // Cultures get created on the fly, do not rely on an exception
            // https://stackoverflow.com/questions/35074033/invalid-cultureinfo-no-longer-throws-culturenotfoundexception/

            // Get culture info from OS
            // TODO: Not available in .NET Standard
            // var cultureInfo = CultureInfo.GetCultureInfo(language, true);
            CultureInfo cultureInfo = CultureInfo.GetCultureInfoByIetfLanguageTag(language);

            // Make sure the culture was not dynamically created
            if (cultureInfo != null ||
                !cultureInfo.ThreeLetterWindowsLanguageName.Equals("ZZZ", StringComparison.OrdinalIgnoreCase) &&
                (cultureInfo.CultureTypes & CultureTypes.UserCustomCulture) != CultureTypes.UserCustomCulture) 
            {
                // Recursively call using the ISO 639-2 code
                return FromString(cultureInfo.ThreeLetterISOLanguageName, iso6393List);
            }
        }
        catch (CultureNotFoundException)
        {
            // Try something else
        }

        // Not found
        return null;
    }
}