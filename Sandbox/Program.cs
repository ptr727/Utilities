using System.Diagnostics;
using System.Reflection;
using InsaneGenius.Utilities;
using InsaneGenius.Utilities.Sandbox;
using Serilog;

// Configure logging: build a Serilog console logger and inject it into the library.
Log.Logger = LoggerFactory.Create();
LogOptions.SetFactory(LoggerFactory.CreateLoggerFactory());

try
{
    // Get the assembly directory
    Assembly? entryAssembly = Assembly.GetEntryAssembly();
    Debug.Assert(entryAssembly != null);
    string? assemblyDirectory = Path.GetDirectoryName(AppContext.BaseDirectory);
    Debug.Assert(assemblyDirectory != null);
    string projectDirectory = Path.GetFullPath(Path.Combine(assemblyDirectory, "../../../../"));
    Log.Logger.Information("Project directory: {ProjectDirectory}", projectDirectory);
}
finally
{
    Log.CloseAndFlush();
}
