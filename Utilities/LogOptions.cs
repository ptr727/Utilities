// TODO: Problematic when used in different environments
// using Microsoft.Extensions.Logging;

using Serilog;

namespace InsaneGenius.Utilities;

/// <summary>
/// Provides global logging configuration options.
/// </summary>
public static class LogOptions
{
    /// <summary>
    /// Gets or sets the logger instance used throughout the utilities library.
    /// Defaults to a silent logger that outputs nothing.
    /// </summary>
    /// <remarks>
    /// Configure this property with your application's logger before using FileEx or Download utilities.
    /// </remarks>
    public static ILogger Logger { get; set; } = new LoggerConfiguration().CreateLogger();
}
