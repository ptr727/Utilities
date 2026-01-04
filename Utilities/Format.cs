using System;

namespace InsaneGenius.Utilities;

/// <summary>
/// Provides formatting utilities for byte sizes and size constants.
/// </summary>
public static class Format
{
    // ReSharper disable InconsistentNaming

    /// <summary>Kibibyte (1024 bytes)</summary>
    public const int KiB = 1024;

    /// <summary>Mebibyte (1024 KiB)</summary>
    public const int MiB = KiB * 1024;

    /// <summary>Gibibyte (1024 MiB)</summary>
    public const int GiB = MiB * 1024;

    /// <summary>Tebibyte (1024 GiB)</summary>
    public const long TiB = GiB * 1024L;

    /// <summary>Pebibyte (1024 TiB)</summary>
    public const long PiB = TiB * 1024L;

    /// <summary>Exbibyte (1024 PiB)</summary>
    public const long EiB = PiB * 1024L;

    /// <summary>Kilobyte (1000 bytes)</summary>
    public const int KB = 1000;

    /// <summary>Megabyte (1000 KB)</summary>
    public const int MB = KB * 1000;

    /// <summary>Gigabyte (1000 MB)</summary>
    public const int GB = MB * 1000;

    /// <summary>Terabyte (1000 GB)</summary>
    public const long TB = GB * 1000L;

    /// <summary>Petabyte (1000 TB)</summary>
    public const long PB = TB * 1000L;

    /// <summary>Exabyte (1000 PB)</summary>
    public const long EB = PB * 1000L;

    // ReSharper restore InconsistentNaming

    /// <summary>
    /// Converts bytes to a human-readable string using binary prefixes (KiB, MiB, GiB, etc.).
    /// </summary>
    /// <param name="value">The byte value to convert.</param>
    /// <param name="units">The unit suffix to append (default is "B").</param>
    /// <returns>A formatted string representing the byte value with appropriate binary prefix.</returns>
    /// <remarks>
    /// Based on: https://en.wikipedia.org/wiki/Kibibyte
    /// </remarks>
    public static string BytesToKibi(long value, string units = "B")
    {
        if (value < 0)
        {
            return "-" + BytesToKibi(Math.Abs(value), units);
        }

        if (value == 0)
        {
            return $"0{units}";
        }

        int magnitude = Convert.ToInt32(Math.Floor(Math.Log(value, KiB)));
        double fraction = Math.Round(value / Math.Pow(KiB, magnitude), 1);
        double truncate = Math.Truncate(fraction);
        return fraction.Equals(truncate)
            ? $"{Convert.ToInt64(truncate):D}{s_kibiSuffix[magnitude]}{units}"
            : $"{fraction:F}{s_kibiSuffix[magnitude]}{units}";
    }

    private static readonly string[] s_kibiSuffix = ["", "Ki", "Mi", "Gi", "Ti", "Pi", "Ei"];

    /// <summary>
    /// Converts bytes to a human-readable string using decimal prefixes (KB, MB, GB, etc.).
    /// </summary>
    /// <param name="value">The byte value to convert.</param>
    /// <param name="units">The unit suffix to append (default is "B").</param>
    /// <returns>A formatted string representing the byte value with appropriate decimal prefix.</returns>
    /// <remarks>
    /// Based on: https://en.wikipedia.org/wiki/Kilobyte
    /// </remarks>
    public static string BytesToKilo(long value, string units = "B")
    {
        switch (value)
        {
            case < 0:
                return "-" + BytesToKilo(Math.Abs(value), units);
            case 0:
                return $"0{units}";
            default:
                break;
        }

        int magnitude = Convert.ToInt32(Math.Floor(Math.Log(value, KB)));
        double fraction = Math.Round(value / Math.Pow(KB, magnitude), 1);
        double truncate = Math.Truncate(fraction);
        return fraction.Equals(truncate)
            ? $"{Convert.ToInt64(truncate):D}{s_kiloSuffix[magnitude]}{units}"
            : $"{fraction:F}{s_kiloSuffix[magnitude]}{units}";
    }

    private static readonly string[] s_kiloSuffix = ["", "K", "M", "G", "T", "P", "E"];
}
