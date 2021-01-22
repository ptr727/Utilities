using Microsoft.Extensions.Logging;
using System;

namespace InsaneGenius.Utilities
{
    public static class Extensions
    {
        // string.Compress() / string.Decompress()
        public static string Compress(this string uncompressedString)
        {
            return StringCompression.Compress(uncompressedString);
        }

        public static string Decompress(this string compressedString)
        {
            return StringCompression.Decompress(compressedString);
        }

        // catch (Exception e) when (LogOptions.Logger.LogAndHandle(e, MethodBase.GetCurrentMethod().Name))
        public static bool LogAndPropagate(this ILogger logger, Exception exception, string message, params object[] args)
        {
            logger.LogError(exception, message, args);
            return false;
        }

        public static bool LogAndHandle(this ILogger logger, Exception exception, string message, params object[] args)
        {
            logger.LogError(exception, message, args);
            return true;
        }
    }
}
