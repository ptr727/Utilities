using System;
using System.Globalization;
using System.Threading;

namespace InsaneGenius.Utilities;

/// <summary>
/// Provides enhanced console output utilities with color and timestamp support.
/// </summary>
public static class ConsoleEx
{
    /// <summary>
    /// Writes a line to the console with the specified color and optional timestamp.
    /// </summary>
    /// <param name="color">The foreground color to use.</param>
    /// <param name="value">The value to write.</param>
    /// <remarks>
    /// This method is thread-safe when used exclusively. Mixing with other console output may result in mismatched colors.
    /// Non-empty values are prefixed with a timestamp.
    /// </remarks>
    public static void WriteLineColor(ConsoleColor color, string value)
    {
        lock (s_writeLineLock)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(
                string.IsNullOrEmpty(value)
                    ? value
                    : $"{DateTime.Now.ToString(CultureInfo.CurrentCulture)} : {value}"
            );
            Console.ForegroundColor = oldColor;
        }
    }

    /// <summary>
    /// Writes a line to the console with the specified color and optional timestamp.
    /// </summary>
    /// <param name="color">The foreground color to use.</param>
    /// <param name="value">The value to write.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void WriteLineColor(ConsoleColor color, object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        WriteLineColor(color, value.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Writes an error message to the console in red.
    /// </summary>
    /// <param name="value">The error message to write.</param>
    public static void WriteLineError(string value) => WriteLineColor(ErrorColor, value);

    /// <summary>
    /// Writes an error message to the console in red.
    /// </summary>
    /// <param name="value">The error message to write.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void WriteLineError(object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        WriteLineError(value.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Writes an event message to the console in yellow.
    /// </summary>
    /// <param name="value">The event message to write.</param>
    public static void WriteLineEvent(string value) => WriteLineColor(EventColor, value);

    /// <summary>
    /// Writes an event message to the console in yellow.
    /// </summary>
    /// <param name="value">The event message to write.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void WriteLineEvent(object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        WriteLineEvent(value.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Writes a tool message to the console in green.
    /// </summary>
    /// <param name="value">The tool message to write.</param>
    public static void WriteLineTool(string value) => WriteLineColor(ToolColor, value);

    /// <summary>
    /// Writes a tool message to the console in green.
    /// </summary>
    /// <param name="value">The tool message to write.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void WriteLineTool(object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        WriteLineTool(value.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Writes a message to the console in cyan.
    /// </summary>
    /// <param name="value">The message to write.</param>
    public static void WriteLine(string value) => WriteLineColor(OutputColor, value);

    /// <summary>
    /// Writes a message to the console in cyan.
    /// </summary>
    /// <param name="value">The message to write.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void WriteLine(object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        WriteLine(value.ToString() ?? string.Empty);
    }

    private static readonly Lock s_writeLineLock = new();

    /// <summary>
    /// The console color used for tool messages.
    /// </summary>
    public const ConsoleColor ToolColor = ConsoleColor.Green;

    /// <summary>
    /// The console color used for error messages.
    /// </summary>
    public const ConsoleColor ErrorColor = ConsoleColor.Red;

    /// <summary>
    /// The console color used for output messages.
    /// </summary>
    public const ConsoleColor OutputColor = ConsoleColor.Cyan;

    /// <summary>
    /// The console color used for event messages.
    /// </summary>
    public const ConsoleColor EventColor = ConsoleColor.Yellow;
}
