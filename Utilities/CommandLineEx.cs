using System;

namespace InsaneGenius.Utilities
{
    public static class CommandLineEx
    {
        // https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp
        public static string[] ParseArguments(string commandLine)
        {
            char[] parmChars = commandLine.ToCharArray();
            bool inQuote = false;
            for (int index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"')
                    inQuote = !inQuote;
                if (!inQuote && parmChars[index] == ' ')
                    parmChars[index] = '\n';
            }
            return new string(parmChars).Split('\n');
        }

        public static string[] GetCommandlineArgs()
        {
            // Split the arguments
            string[] args = ParseArguments(Environment.CommandLine);

            // Strip the process path from the list of arguments
            string[] argsex = new string[args.Length - 1];
            Array.Copy(args, 1, argsex, 0, argsex.Length);
            return argsex;
        }
    }
}
