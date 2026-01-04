using System;

namespace InsaneGenius.Utilities;

/// <summary>
/// Provides command-line argument parsing utilities.
/// </summary>
public static class CommandLineEx
{
    /// <summary>
    /// Parses a command-line string into individual arguments, respecting quoted strings.
    /// </summary>
    /// <param name="commandLine">The command-line string to parse.</param>
    /// <returns>An array of parsed arguments.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandLine"/> is null.</exception>
    /// <remarks>
    /// This method handles quoted strings correctly, treating spaces within quotes as part of the argument.
    /// Based on: https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp
    /// </remarks>
    public static string[] ParseArguments(string commandLine)
    {
        ArgumentNullException.ThrowIfNull(commandLine);

        char[] paramChars = commandLine.ToCharArray();
        bool inQuote = false;
        for (int index = 0; index < paramChars.Length; index++)
        {
            if (paramChars[index] == '"')
            {
                inQuote = !inQuote;
            }

            if (!inQuote && paramChars[index] == ' ')
            {
                paramChars[index] = '\n';
            }
        }
        return new string(paramChars).Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Gets the command-line arguments for the current process, excluding the process path.
    /// </summary>
    /// <returns>An array of command-line arguments.</returns>
    public static string[] GetCommandLineArgs()
    {
        // Split the arguments
        string[] args = ParseArguments(Environment.CommandLine);

        // Strip the process path from the list of arguments
        if (args.Length == 0)
        {
            return [];
        }

        string[] argsEx = new string[args.Length - 1];
        Array.Copy(args, 1, argsEx, 0, argsEx.Length);
        return argsEx;
    }
}
