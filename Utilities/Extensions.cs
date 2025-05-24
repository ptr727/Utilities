using System;
using Serilog;

namespace InsaneGenius.Utilities;

public static class Extensions
{
    // string.Compress() / string.Decompress()
    public static string Compress(this string uncompressedString) =>
        StringCompression.Compress(uncompressedString);

    public static string Decompress(this string compressedString) =>
        StringCompression.Decompress(compressedString);

    // catch (Exception e) when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod()?.Name))
    public static bool LogAndPropagate(this ILogger logger, Exception exception, string function)
    {
        logger.Error(exception, "{Function}", function);
        return false;
    }

    public static bool LogAndHandle(this ILogger logger, Exception exception, string function)
    {
        logger.Error(exception, "{Function}", function);
        return true;
    }
}
