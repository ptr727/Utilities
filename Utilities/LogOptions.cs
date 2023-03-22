using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InsaneGenius.Utilities;

public static class LogOptions
{
    // Create logger from log factory
    public static ILogger CreateLogger(ILoggerFactory loggerFactory)
    {
        // Create a logger using the provided logger factory
        Logger = loggerFactory.CreateLogger("InsaneGeniusUtilities");
        return Logger;
    }

    // Default logger
    public static ILogger Logger { get; set; } = NullLogger.Instance;
}