using System;

namespace InsaneGenius.Utilities;

public static class CommandLineEx
{
    // https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp
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
        return new string(paramChars).Split('\n');
    }

    public static string[] GetCommandLineArgs()
    {
        // Split the arguments
        string[] args = ParseArguments(Environment.CommandLine);

        // Strip the process path from the list of arguments
        string[] argsEx = new string[args.Length - 1];
        Array.Copy(args, 1, argsEx, 0, argsEx.Length);
        return argsEx;
    }
}
