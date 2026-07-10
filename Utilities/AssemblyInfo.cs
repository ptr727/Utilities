using System.Net.Http.Headers;
using System.Reflection;

namespace ptr727.Utilities;

/// <summary>
/// Provides AOT and trim safe access to assembly and consuming-application identity.
/// </summary>
/// <remarks>
/// Native AOT does not support <see cref="Assembly.GetExecutingAssembly"/> reliably
/// (see https://github.com/dotnet/runtime/issues/94200). Use <see cref="For{T}"/> as the
/// AOT safe substitute, passing a marker type from the assembly of interest. The
/// application identity members make no assumptions about the hosting environment and
/// fall back gracefully when the managed entry assembly is unavailable.
/// </remarks>
public static class AssemblyInfo
{
    /// <summary>
    /// Gets the assembly that defines the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">A marker type from the assembly of interest.</typeparam>
    /// <returns>The assembly that defines <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// This is the AOT safe replacement for <see cref="Assembly.GetExecutingAssembly"/>:
    /// <c>typeof(T).Assembly</c> resolves at compile time with no stack walk or reflection.
    /// </remarks>
    public static Assembly For<T>() => typeof(T).Assembly;

    /// <summary>
    /// Gets the name of the consuming application (the entry assembly, never this library).
    /// </summary>
    /// <remarks>
    /// Assembly to file metadata is unreliable under Native AOT, so the managed entry
    /// assembly name is tried first, then the OS process executable name, then a generic value.
    /// </remarks>
    public static string AppName
    {
        get
        {
            string? processPath = Environment.ProcessPath;
            string? processName = string.IsNullOrEmpty(processPath)
                ? null
                : Path.GetFileNameWithoutExtension(processPath);
            return Assembly.GetEntryAssembly()?.GetName().Name ?? processName ?? "Unknown";
        }
    }

    /// <summary>
    /// Gets the version of the consuming application (the entry assembly), or a fallback value.
    /// </summary>
    /// <remarks>
    /// Uses the AOT safe <see cref="AssemblyName.Version"/> rather than reflecting over
    /// informational-version attributes, which the trimmer or AOT compiler may strip.
    /// </remarks>
    public static string AppVersion =>
        Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0";

    /// <summary>
    /// Gets a User-Agent header value identifying the consuming application.
    /// </summary>
    /// <remarks>
    /// The derived name may not be a valid HTTP token (for example a process name with spaces),
    /// so the value falls back to a guaranteed-valid token when parsing fails.
    /// </remarks>
    public static ProductInfoHeaderValue DefaultUserAgent
    {
        get
        {
            string appVersion = AppVersion;
            return ProductInfoHeaderValue.TryParse(
                $"{AppName}/{appVersion}",
                out ProductInfoHeaderValue? userAgent
            )
                ? userAgent
                : new ProductInfoHeaderValue("Unknown", appVersion);
        }
    }
}
