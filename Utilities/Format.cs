using System;

namespace InsaneGenius.Utilities;

public static class Format
{
    // ReSharper disable InconsistentNaming
    public const int KiB = 1024;
    public const int MiB = KiB * 1024;
    public const int GiB = MiB * 1024;
    public const long TiB = GiB * 1024L;
    public const long PiB = TiB * 1024L;
    public const long EiB = PiB * 1024L;
    // public const long ZiB = EiB * 1024L;
    // public const long YiB = ZiB * 1024L;
    public const int KB = 1000;
    public const int MB = KB * 1000;
    public const int GB = MB * 1000;
    public const long TB = GB * 1000L;
    public const long PB = TB * 1000L;
    public const long EB = PB * 1000L;
    // public const long ZB = EB * 1000L;
    // public const long YB = ZB * 1000L;
    // ReSharper restore InconsistentNaming

    // https://en.wikipedia.org/wiki/Kibibyte
    // https://en.wikipedia.org/wiki/Kilobyte
    // https://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
    // https://stackoverflow.com/questions/14488796/does-net-provide-an-easy-way-convert-bytes-to-kb-mb-gb-etc
    public static string BytesToKibi(long value, string units = "B")
    {
        if (value < 0)
        {
            return "-" + BytesToKibi(Math.Abs(value));
        }

        if (value == 0)
        {
            return $"0{units}";
        }

        int magnitude = Convert.ToInt32(Math.Floor(Math.Log(value, KiB)));
        double fraction = Math.Round(value / Math.Pow(KiB, magnitude), 1);
        double truncate = Math.Truncate(fraction);
        return fraction.Equals(truncate) ? $"{Convert.ToInt64(truncate):D}{s_kibiSuffix[magnitude]}{units}" : $"{fraction:F}{s_kibiSuffix[magnitude]}{units}";
    }
    private static readonly string[] s_kibiSuffix = ["", "Ki", "Mi", "Gi", "Ti", "Pi", "Ei"];

    public static string BytesToKilo(long value, string units = "B")
    {
        if (value < 0)
        {
            return "-" + BytesToKilo(Math.Abs(value));
        }

        if (value == 0)
        {
            return $"0{units}";
        }

        int magnitude = Convert.ToInt32(Math.Floor(Math.Log(value, KB)));
        double fraction = Math.Round(value / Math.Pow(KB, magnitude), 1);
        double truncate = Math.Truncate(fraction);
        return fraction.Equals(truncate) ? $"{Convert.ToInt64(truncate):D}{s_kiloSuffix[magnitude]}{units}" : $"{fraction:F}{s_kiloSuffix[magnitude]}{units}";
    }
    private static readonly string[] s_kiloSuffix = ["", "K", "M", "G", "T", "P", "E"];
}
