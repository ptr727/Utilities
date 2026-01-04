using System.Diagnostics;
using System.IO;
using System.Reflection;
using Serilog;

// Get the assembly directory
Assembly? entryAssembly = Assembly.GetEntryAssembly();
Debug.Assert(entryAssembly != null);
string? assemblyDirectory = Path.GetDirectoryName(System.AppContext.BaseDirectory);
Debug.Assert(assemblyDirectory != null);
string projectDirectory = Path.GetFullPath(Path.Combine(assemblyDirectory, "../../../../"));
Log.Logger.Information("Project directory: {ProjectDirectory}", projectDirectory);
