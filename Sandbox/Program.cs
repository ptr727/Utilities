using System.Diagnostics;
using System.IO;
using System.Reflection;
using InsaneGenius.Utilities;

// Get the assembly directory
Assembly entryAssembly = Assembly.GetEntryAssembly();
Debug.Assert(entryAssembly != null);
string assemblyDirectory = Path.GetDirectoryName(entryAssembly.Location);
Debug.Assert(assemblyDirectory != null);
string projectDirectory = Path.GetFullPath(Path.Combine(assemblyDirectory, "../../../../"));

string rfc5646File = Path.GetFullPath(
    Path.Combine(projectDirectory, "Data/language-subtag-registry")
);
Rfc5646 rfc5646 = new();
rfc5646.Load(rfc5646File);

string iso6393File = Path.GetFullPath(Path.Combine(projectDirectory, "Data/iso-639-3.tab"));
Iso6393 iso6393 = new();
iso6393.Load(iso6393File);
