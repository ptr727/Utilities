using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace InsaneGenius.Utilities;

public class ProcessEx : Process
{
    public int ExecuteEx(string executable, string parameters) =>
        ExecuteExAsync(executable, parameters).Result;

    public async Task<int> ExecuteExAsync(string executable, string parameters)
    {
        // Start
        if (!StartEx(executable, parameters))
        {
            return -1;
        }

        // Wait for exit
        return await WaitForExitAsync().ConfigureAwait(true);
    }

    public bool StartEx(string executable, string parameters)
    {
        // Event handlers
        void OutputHandlerEx(object s, DataReceivedEventArgs e) => OutputHandler(e);
        void ErrorHandlerEx(object s, DataReceivedEventArgs e) => ErrorHandler(e);
        void ExitHandlerEx(object s, EventArgs e) => ExitHandler();

        // Output and error handlers
        StartInfo.RedirectStandardOutput = RedirectOutput;
        OutputDataReceived += OutputHandlerEx;
        StartInfo.RedirectStandardError = RedirectError;
        ErrorDataReceived += ErrorHandlerEx;

        // Exit handler
        EnableRaisingEvents = true;
        Exited += ExitHandlerEx;

        // Process name and parameters
        StartInfo.FileName = executable;
        StartInfo.Arguments = parameters;
        StartInfo.UseShellExecute = false;

        try
        {
            // Start process
            if (!Start())
            {
                return false;
            }
        }
        catch (Exception e)
            when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
        {
            return false;
        }

        // Read output and error
        if (RedirectOutput)
        {
            BeginOutputReadLine();
        }

        if (RedirectError)
        {
            BeginErrorReadLine();
        }

        return true;
    }

    public async Task<int> WaitForExitAsync()
    {
        // Wait for exit handler to be signalled
        _ = await _processExitComplete.Task.ConfigureAwait(true);

        // Wait for the process to exit to make sure all output has been written
        WaitForExit();

        // Flush streams
        if (OutputStream != null)
        {
            await OutputStream.FlushAsync();
        }

        if (ErrorStream != null)
        {
            await ErrorStream.FlushAsync();
        }

        return ExitCode;
    }

    protected virtual void OutputHandler(DataReceivedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (string.IsNullOrEmpty(e.Data))
        {
            return;
        }

        // Stream output
        OutputStream?.WriteLine(e.Data);

        // String output
        OutputString?.AppendLine(e.Data);

        // Console output
        if (ConsoleOutput)
        {
            Console.Out.WriteLine(e.Data);
        }
    }

    protected virtual void ErrorHandler(DataReceivedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (string.IsNullOrEmpty(e.Data))
        {
            return;
        }

        // Stream output
        ErrorStream?.WriteLine(e.Data);

        // String output
        ErrorString?.AppendLine(e.Data);

        // Console output
        if (ConsoleError)
        {
            Console.Error.WriteLine(e.Data);
        }
    }

    protected virtual void ExitHandler() =>
        // Signal exit
        _processExitComplete.TrySetResult(true);

    public static int Execute(string executable, string parameters)
    {
        // Create new process
        using ProcessEx process = new();
        return process.ExecuteEx(executable, parameters);
    }

    public static int Execute(string executable, string parameters, bool console)
    {
        // Console output is ok
        if (console)
        {
            return Execute(executable, parameters);
        }

        // Create new process
        // Suppress output and error
        using ProcessEx process = new()
        {
            // Redirect and do not output anything
            RedirectOutput = true,
            ConsoleOutput = false,
            RedirectError = true,
            ConsoleError = false,
        };
        int exitCode = process.ExecuteEx(executable, parameters);

        return exitCode;
    }

    public static int Execute(
        string executable,
        string parameters,
        bool console,
        int lines,
        out string output
    )
    {
        // Create new process
        // Redirect output
        using ProcessEx process = new()
        {
            // Capture output
            RedirectOutput = true,
            ConsoleOutput = console,
            OutputString = new StringHistory(lines, lines),
            // If console is false then redirect error but do not output anything
            RedirectError = !console,
        };
        int exitCode = process.ExecuteEx(executable, parameters);
        output = process.OutputString.ToString();

        return exitCode;
    }

    public static int Execute(
        string executable,
        string parameters,
        bool console,
        int lines,
        out string output,
        out string error
    )
    {
        // Create new process
        // Redirect output and error
        using ProcessEx process = new()
        {
            // Capture output and error
            RedirectOutput = true,
            ConsoleOutput = console,
            OutputString = new StringHistory(lines, lines),
            RedirectError = true,
            ConsoleError = console,
            ErrorString = new StringHistory(lines, lines),
        };
        int exitCode = process.ExecuteEx(executable, parameters);
        output = process.OutputString.ToString();
        error = process.ErrorString.ToString();

        return exitCode;
    }

    public bool RedirectOutput { get; set; }
    public bool ConsoleOutput { get; set; }
    public StreamWriter OutputStream { get; set; }
    public StringHistory OutputString { get; set; }
    public bool RedirectError { get; set; }
    public bool ConsoleError { get; set; }
    public StreamWriter ErrorStream { get; set; }
    public StringHistory ErrorString { get; set; }

    private readonly TaskCompletionSource<bool> _processExitComplete = new(
        TaskCreationOptions.RunContinuationsAsynchronously
    );
}
