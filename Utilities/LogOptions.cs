
// TODO: Problematic when used in different environments
// using Microsoft.Extensions.Logging;

using Serilog;

namespace InsaneGenius.Utilities;

public static class LogOptions
{
    // Default silent logger
    public static ILogger Logger { get; set; } = new LoggerConfiguration().CreateLogger();
}
