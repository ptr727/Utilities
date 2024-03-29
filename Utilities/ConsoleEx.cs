﻿using System;
using System.Globalization;

namespace InsaneGenius.Utilities;

public static class ConsoleEx
{
    public static void WriteLineColor(ConsoleColor color, string value)
    {
        // Locking only works when using this function
        // Mixing any other console output may still result in mismatched color output
        lock (WriteLineLock)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(string.IsNullOrEmpty(value) ? $"{value}" : $"{DateTime.Now.ToString(CultureInfo.CurrentCulture)} : {value}");
            Console.ForegroundColor = oldColor;
        }
    }
    public static void WriteLineColor(ConsoleColor color, object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        WriteLineColor(color, value.ToString());
    }

    public static void WriteLineError(string value)
    {
        WriteLineColor(ErrorColor, value);
    }
    public static void WriteLineError(object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        WriteLineError(value.ToString());
    }

    public static void WriteLineEvent(string value)
    {
        WriteLineColor(EventColor, value);
    }
    public static void WriteLineEvent(object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        WriteLineEvent(value.ToString());
    }

    public static void WriteLineTool(string value)
    {
        WriteLineColor(ToolColor, value);
    }
    public static void WriteLineTool(object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        WriteLineTool(value.ToString());
    }

    public static void WriteLine(string value)
    {
        WriteLineColor(OutputColor, value);
    }
    public static void WriteLine(object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        WriteLine(value.ToString());
    }

    private static readonly object WriteLineLock = new();

    // Good looking console colors; Green, Cyan, Red, Magenta, Yellow, White
    public const ConsoleColor ToolColor = ConsoleColor.Green;
    public const ConsoleColor ErrorColor = ConsoleColor.Red;
    public const ConsoleColor OutputColor = ConsoleColor.Cyan;
    public const ConsoleColor EventColor = ConsoleColor.Yellow;
}