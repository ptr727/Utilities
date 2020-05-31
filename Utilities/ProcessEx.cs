using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace InsaneGenius.Utilities
{
    public class ProcessEx : Process
    {
        public int ExecuteEx(string executable, string parameters)
        {
            return ExecuteExAsync(executable, parameters).Result;
        }

        public async Task<int> ExecuteExAsync(string executable, string parameters)
        {
            // Start
            if (!StartEx(executable, parameters))
                return -1;

            // Wait for exit
            return await WaitForExitAsync().ConfigureAwait(true);
        }

        public bool StartEx(string executable, string parameters)
        {
            // Init
            OutputString.Clear();
            ErrorString.Clear();

            // Output and error handlers
            void OutputHandlerEx(object s, DataReceivedEventArgs e)
            {
                OutputHandler(e);
            }
            void ErrorHandlerEx(object s, DataReceivedEventArgs e)
            {
                ErrorHandler(e);
            }
            StartInfo.RedirectStandardOutput = RedirectOutput;
            OutputDataReceived += OutputHandlerEx;
            StartInfo.RedirectStandardError = RedirectError;
            ErrorDataReceived += ErrorHandlerEx;

            // Exit handler
            void ExitHandlerEx(object s, EventArgs e)
            {
                ExitHandler(e);
            }
            EnableRaisingEvents = true;
            Exited += ExitHandlerEx;

            // Start process
            StartInfo.FileName = executable;
            StartInfo.Arguments = parameters;
            StartInfo.UseShellExecute = false;
            if (!Start())
                return false;

            // Read output and error
            if (RedirectOutput)
                BeginOutputReadLine();
            if (RedirectError)
                BeginErrorReadLine();

            return true;
        }

        public async Task<int> WaitForExitAsync()
        {
            // Wait for exit
            await ProcessExitComplete.Task.ConfigureAwait(true);
            return ExitCode;
        }

        protected virtual void OutputHandler(DataReceivedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (string.IsNullOrEmpty(e.Data))
                return;

            // Stream output or text output
            if (OutputStream != null)
                OutputStream.Write(e.Data);
            else
                OutputString.AppendLine(e.Data);

            // Console output
            if (PrintOutput)
                ConsoleEx.WriteLineTool($"{ProcessName} : {e.Data}");
        }

        protected virtual void ErrorHandler(DataReceivedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (string.IsNullOrEmpty(e.Data))
                return;

            // Stream output or text output
            if (ErrorStream != null)
                ErrorStream.Write(e.Data);
            else
                ErrorString.AppendLine(e.Data);

            // Console output
            if (PrintError)
                ConsoleEx.WriteLineError($"{ProcessName} : {e.Data}");
        }

        protected virtual void ExitHandler(EventArgs e)
        {
            // Flush stream
            OutputStream?.Flush();

            // Signal exit
            ProcessExitComplete.TrySetResult(true);
        }

        public static int Execute(string executable, string parameters)
        {
            // Create new process
            using ProcessEx process = new ProcessEx();
            return process.ExecuteEx(executable, parameters);
        }

        public static int Execute(string executable, string parameters, out string output)
        {
            // Create new process
            using ProcessEx process = new ProcessEx
            {
                RedirectOutput = true
            };
            int exitcode = process.ExecuteEx(executable, parameters);
            output = process.OutputText;

            return exitcode;
        }

        public static int Execute(string executable, string parameters, out string output, out string error)
        {
            // Create new process
            using ProcessEx process = new ProcessEx
            {
                RedirectOutput = true,
                RedirectError = true
            };
            int exitCode = process.ExecuteEx(executable, parameters);
            output = process.OutputText;
            error = process.ErrorText;

            return exitCode;
        }

        public bool RedirectOutput { get; set; } = false;
        public bool PrintOutput { get; set; } = false;
        public string OutputText => OutputString.ToString();
        public StreamWriter OutputStream { get; set; } = null;
        public bool RedirectError { get; set; } = false;
        public bool PrintError { get; set; } = false;
        public string ErrorText => ErrorString.ToString();
        public StreamWriter ErrorStream { get; set; } = null;

        private readonly StringBuilder OutputString = new StringBuilder();
        private readonly StringBuilder ErrorString = new StringBuilder();
        private readonly TaskCompletionSource<bool> ProcessExitComplete = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}