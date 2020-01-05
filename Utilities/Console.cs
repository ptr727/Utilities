using System;
using System.Globalization;

namespace InsaneGenius.Utilities
{
    public static class ConsoleEx
    {
        public static void WriteLineColor(ConsoleColor color, string value)
        {
            lock (WriteLineLock)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(string.IsNullOrEmpty(value) ? $"{value}" : $"{DateTime.Now.ToString(CultureInfo.CurrentCulture)} : {value}");
                Console.ForegroundColor = oldColor;
            }
        }

        /*
                public static void WriteLineColor(ConsoleColor color, Object value)
                {
                    WriteLineColor(color, value.ToString());
                }
        */

        public static void WriteLineError(string value)
        {
            WriteLineColor(ErrorColor, value);
        }

        public static void WriteLineError(object value)
        {
            WriteLineColor(ErrorColor, value.ToString());
        }

        /*
                public static void WriteLineEvent(string value)
                {
                    WriteLineColor(EventColor, value);
                }
        */

        /*
                public static void WriteLineEvent(object value)
                {
                    WriteLineColor(EventColor, value.ToString());
                }
        */

        public static void WriteLineTool(string value)
        {
            WriteLineColor(ToolColor, value);
        }

        /*
                public static void WriteLineTool(object value)
                {
                    WriteLineColor(ToolColor, value.ToString());
                }
        */

        public static void WriteLine(string value)
        {
            // Good looking console colors; Green, Cyan, Red, Magenta, Yellow, White
            WriteLineColor(OutputColor, value);
        }

        /*
                public static void WriteLine(Object value)
                {
                    WriteLineColor(OutputColor, value);
                }
        */

        private static readonly object WriteLineLock = new object();

        public const ConsoleColor ToolColor = ConsoleColor.Green;
        public const ConsoleColor ErrorColor = ConsoleColor.Red;
        public const ConsoleColor OutputColor = ConsoleColor.Cyan;
        public const ConsoleColor EventColor = ConsoleColor.Yellow;
    }
}
