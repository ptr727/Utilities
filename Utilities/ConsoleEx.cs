using System;
using System.Globalization;
using System.Threading;

namespace InsaneGenius.Utilities;

public static class ConsoleEx
{
    public static void WriteLineColor(ConsoleColor color, string value)
    {
        // Locking only works when using this function
        // Mixing any other console output may still result in mismatched color output
        lock (s_writeLineLock)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(
                string.IsNullOrEmpty(value)
                    ? $"{value}"
                    : $"{DateTime.Now.ToString(CultureInfo.CurrentCulture)} : {value}"
            );
            Console.ForegroundColor = oldColor;
        }
    }

    public static void WriteLineColor(ConsoleColor color, object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        WriteLineColor(color, value.ToString());
    }

    public static void WriteLineError(string value) => WriteLineColor(ErrorColor, value);

    public static void WriteLineError(object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        WriteLineError(value.ToString());
    }

    public static void WriteLineEvent(string value) => WriteLineColor(EventColor, value);

    public static void WriteLineEvent(object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        WriteLineEvent(value.ToString());
    }

    public static void WriteLineTool(string value) => WriteLineColor(ToolColor, value);

    public static void WriteLineTool(object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        WriteLineTool(value.ToString());
    }

    public static void WriteLine(string value) => WriteLineColor(OutputColor, value);

    public static void WriteLine(object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        WriteLine(value.ToString());
    }

    private static readonly Lock s_writeLineLock = new();

    // Good looking console colors; Green, Cyan, Red, Magenta, Yellow, White
    public const ConsoleColor ToolColor = ConsoleColor.Green;
    public const ConsoleColor ErrorColor = ConsoleColor.Red;
    public const ConsoleColor OutputColor = ConsoleColor.Cyan;
    public const ConsoleColor EventColor = ConsoleColor.Yellow;
}
