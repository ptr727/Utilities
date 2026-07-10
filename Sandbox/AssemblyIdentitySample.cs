using System.Reflection;

namespace ptr727.Utilities.Sandbox;

/// <summary>
/// Validates AOT-safe assembly and application identity resolution. Resolution must be
/// identical whether the Sandbox runs as a regular managed build or a Native AOT binary; this
/// is the definitive test that the GetExecutingAssembly substitute works under AOT.
/// </summary>
internal static class AssemblyIdentitySample
{
    /// <summary>
    /// Runs the sample.
    /// </summary>
    /// <returns>True if the application identity resolved, false otherwise.</returns>
    internal static bool Run()
    {
        Assembly markerAssembly = AssemblyInfo.For<Program>();
        Log.Logger.Information(
            "Identity: MarkerAssembly={MarkerAssembly}, AppName={AppName}, AppVersion={AppVersion}, UserAgent={UserAgent}",
            markerAssembly.GetName().Name,
            AssemblyInfo.AppName,
            AssemblyInfo.AppVersion,
            AssemblyInfo.DefaultUserAgent.ToString()
        );

        // Hard runtime check: Debug.Assert is compiled out of an AOT Release build, so fail
        // explicitly if the consuming-application identity did not resolve.
        if (string.IsNullOrEmpty(AssemblyInfo.AppName) || AssemblyInfo.AppName == "Unknown")
        {
            Log.Logger.Error(
                "Application identity did not resolve: AppName={AppName}",
                AssemblyInfo.AppName
            );
            return false;
        }

        return true;
    }
}
