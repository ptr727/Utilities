using System.Globalization;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;

namespace ptr727.Utilities.Sandbox;

/// <summary>
/// Configures a Serilog console logger and exposes it as a
/// <see cref="Microsoft.Extensions.Logging.ILoggerFactory"/> for the library.
/// </summary>
internal static class LoggerFactory
{
    private static readonly Lazy<SerilogLoggerFactory> s_serilogLoggerFactory = new(() =>
    {
        // Use the already configured Log.Logger if set, else create a new logger.
        ILogger logger = ReferenceEquals(Log.Logger, Serilog.Core.Logger.None)
            ? Create()
            : Log.Logger;
        bool disposeLogger = !ReferenceEquals(logger, Log.Logger);
        return new SerilogLoggerFactory(logger, dispose: disposeLogger);
    });

    /// <summary>
    /// Creates a Serilog console logger.
    /// </summary>
    /// <returns>The configured Serilog logger.</returns>
    internal static ILogger Create() =>
        new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(
                theme: AnsiConsoleTheme.Code,
                formatProvider: CultureInfo.InvariantCulture,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();

    /// <summary>
    /// Gets the logger factory wrapping the configured Serilog logger.
    /// </summary>
    /// <returns>The logger factory.</returns>
    internal static Microsoft.Extensions.Logging.ILoggerFactory CreateLoggerFactory() =>
        s_serilogLoggerFactory.Value;
}
