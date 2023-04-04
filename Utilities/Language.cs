using System;
using System.Globalization;

namespace InsaneGenius.Utilities;

public class Language
{
    Language()
    {
        Iso6393 = new();
        Iso6393.Create();
        Rfc5646 = new();
        Rfc5646.Create();
    }

    private Iso6393 Iso6393;
    private Rfc5646 Rfc5646;

    // Get the IETF/RFC-5646/BCP-47 tag from an ISO-639-2B tag
    public string GetRfc5646Tag(string language, bool nullOnFailure)
    {
        if (string.IsNullOrEmpty(language))
        {
            return nullOnFailure ? null : Undefined;
        }

        // Undefined "und"
        if (language.Equals(Undefined, StringComparison.OrdinalIgnoreCase))
        {
            return Undefined;
        }

        // No linguistic content "zxx"
        if (language.Equals(None, StringComparison.OrdinalIgnoreCase))
        {
            return None;
        }

        // Handle "chi" as "zho" for Matroska
        // https://gitlab.com/mbunkus/mkvtoolnix/-/issues/1149
        if (language.Equals("chi", StringComparison.OrdinalIgnoreCase))
        {
            return Chinese;
        }

        // Get ISO-639 record
        var iso6393 = Iso6393.Find(language);
        if (iso6393 == null)
        {
            return nullOnFailure ? null : Undefined;
        }

        // TODO: Look in Rfc5646 records

        // Get a CultureInfo from the ISO639-3 3 letter code
        // E.g. afr -> afr
        // E.g. ger -> deu
        // E.g. fre -> fra
        // If not found try the ISO639-1 2 letter code
        // E.g. afr -> af
        var cultureInfo = CreateCultureInfo(iso6393.Id) ?? CreateCultureInfo(iso6393.Part1);
        if (cultureInfo == null)
        {
            return nullOnFailure ? null : Undefined;
        }

        // Return the IETF
        return cultureInfo.IetfLanguageTag;
    }

    // Get the ISO-639-2B tag from a IETF/RFC-5646/BCP-47 tag
    public string GetIso639Tag(string language, bool nullOnFailure)
    {
        if (string.IsNullOrEmpty(language))
        {
            return nullOnFailure ? null : Undefined;
        }

        // Undefined "und"
        if (language.Equals(Undefined, StringComparison.OrdinalIgnoreCase))
        {
            return Undefined;
        }

        // No linguistic content "zxx"
        if (language.Equals(None, StringComparison.OrdinalIgnoreCase))
        {
            return None;
        }

        // Split the parts and use the first part
        // zh-cmn-Hant -> zh
        var parts = language.Split('-');
        language = parts[0];

        // Handle "chi" as "zho" for Matroska
        // https://gitlab.com/mbunkus/mkvtoolnix/-/issues/1149
        if (language.Equals(Chinese, StringComparison.OrdinalIgnoreCase))
        {
            return "chi";
        }

        // Get ISO639 record
        var iso6393 = Iso6393.Find(language);
        if (iso6393 != null)
        {
            // Return the Part 2B code
            return iso6393.Part2B;
        }

        // Try from CultureInfo
        var cultureInfo = CreateCultureInfo(language);
        if (cultureInfo == null)
        {
            return nullOnFailure ? null : Undefined;
        }

        // Get ISO639 record from cultureInfo ISO code
        iso6393 = Iso6393.Find(cultureInfo.ThreeLetterISOLanguageName);
        if (iso6393 != null)
        {
            // Return the Part 2B code
            return iso6393.Part2B;
        }

        // Not found
        return nullOnFailure ? null : Undefined;
    }

    public static CultureInfo CreateCultureInfo(string language)
    {
        // Get a CultureInfo representation
        try
        {
            // Cultures are created on the fly, we can't rely on an exception
            // https://stackoverflow.com/questions/35074033/invalid-cultureinfo-no-longer-throws-culturenotfoundexception/
            var cultureInfo = CultureInfo.GetCultureInfo(language, true);

            // Make sure the culture was not custom created
            if (cultureInfo == null ||
                cultureInfo.ThreeLetterWindowsLanguageName.Equals(Missing, StringComparison.OrdinalIgnoreCase) ||
                (cultureInfo.CultureTypes & CultureTypes.UserCustomCulture) == CultureTypes.UserCustomCulture)
            {
                return null;
            }
            return cultureInfo;
        }
        catch (CultureNotFoundException)
        {
            // Not found
        }
        return null;
    }

    public static bool IsUndefined(string language)
    {
        return string.IsNullOrEmpty(language) || language.Equals(Undefined, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsMatch(string prefix, string language)
    {
        // TODO: This is a very rudimentary BCP 47 matcher, find something more complete in C#?
        // https://r12a.github.io/app-subtags/
        // https://www.loc.gov/standards/iso639-2/php/langcodes-search.php

        // zh match: zh: zh, zh-Hant, zh-Hans, zh-cmn-Hant
        // zho not: zh
        // zho match: zho
        // zh-Hant match: zh-Hant, zh-Hant-foo

        // The language matches the prefix exactly
        if (language.Equals(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // The language start with the prefix, and the the next character is a -
        if (language.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            language[prefix.Length..].StartsWith('-'))
        {
            return true;
        }

        // No match
        return false;
    }

    public const string Undefined = "und";
    public const string Missing = "zzz";
    public const string None = "zxx";
    public const string Chinese = "zh";
    public const string English = "en";
}
