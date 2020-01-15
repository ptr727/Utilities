using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace InsaneGenius.Utilities
{
    public class ProcessEx
    {
        public ProcessEx()
        {
            RedirectOutput = false;
            PrintOutput = false;
            OutputString = new StringBuilder();
            RedirectError = false;
            PrintError = false;
            ErrorString = new StringBuilder();
            ProcessName = string.Empty;
        }

        public int ExecuteEx(string executable, string parameters)
        {
            // Create new process
            using Process process = new Process
            {
                StartInfo =
                {
                    FileName = executable,
                    Arguments = parameters,
                    UseShellExecute = false
                }
            };

            // Output handlers
            void OutputHandlerEx(object s, DataReceivedEventArgs e) => OutputHandler(e, this);
            void ErrorHandlerEx(object s, DataReceivedEventArgs e) => ErrorHandler(e, this);
            process.StartInfo.RedirectStandardOutput = RedirectOutput;
            process.OutputDataReceived += OutputHandlerEx;
            process.StartInfo.RedirectStandardError = RedirectError;
            process.ErrorDataReceived += ErrorHandlerEx;
            try
            {
                // Get the EXE name for print purposes
                FileInfo fileInfo = new FileInfo(executable);
                ProcessName = fileInfo.Name;

                // Start process
                OutputString.Clear();
                ErrorString.Clear();
                process.Start();

                // Read output and error
                if (RedirectOutput)
                    process.BeginOutputReadLine();
                if (RedirectError)
                    process.BeginErrorReadLine();

                // Wait for exit
                process.WaitForExit();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return -1;
            }
            return process.ExitCode;
        }

        private static void OutputHandler(DataReceivedEventArgs e, ProcessEx redirected)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            redirected.OutputString.AppendLine(e.Data);
            if (redirected.PrintOutput)
                ConsoleEx.WriteLineTool($"{redirected.ProcessName} : {e.Data}");
        }

        private static void ErrorHandler(DataReceivedEventArgs e, ProcessEx redirected)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            redirected.ErrorString.AppendLine(e.Data);
            if (redirected.PrintError)
                ConsoleEx.WriteLineError($"{redirected.ProcessName} : {e.Data}");
        }

        public bool RedirectOutput { get; set; }
        public bool PrintOutput { get; set; }
        public string OutputText => OutputString.ToString();
        public bool RedirectError { get; set; }
        public bool PrintError { get; set; }
        public string ErrorText => ErrorString.ToString();

        public static int Execute(string executable, string parameters)
        {
            // Create new process
            ProcessEx process = new ProcessEx();
            return process.ExecuteEx(executable, parameters);
        }

        public static int Execute(string executable, string parameters, out string output)
        {
            // Create new process
            ProcessEx process = new ProcessEx
            {
                RedirectOutput = true,
                PrintOutput = false
            };
            int exitcode = process.ExecuteEx(executable, parameters);
            output = process.OutputText;

            return exitcode;
        }

        public static int Execute(string executable, string parameters, out string output, out string error)
        {
            // Create new process
            ProcessEx process = new ProcessEx
            {
                RedirectOutput = true,
                PrintOutput = false,
                RedirectError = true,
                PrintError = false
            };
            int exitCode = process.ExecuteEx(executable, parameters);
            output = process.OutputText;
            error = process.ErrorText;

            return exitCode;
        }

        private readonly StringBuilder OutputString;
        private readonly StringBuilder ErrorString;
        private string ProcessName;
    }
}