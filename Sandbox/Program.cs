// See https://aka.ms/new-console-template for more information

using InsaneGenius.Utilities;
using System.Diagnostics;
using System.IO;
using System.Reflection;

// Get the assembly directory
var entryAssembly = Assembly.GetEntryAssembly();
Debug.Assert(entryAssembly != null);
string assemblyDirectory = Path.GetDirectoryName(entryAssembly.Location);
Debug.Assert(assemblyDirectory != null);
string projectDirectory = Path.GetFullPath(Path.Combine(assemblyDirectory, "../../../../"));

var rfc5646File = Path.GetFullPath(Path.Combine(projectDirectory, "Data/language-subtag-registry"));
var rfc5646 = new Rfc5646();
rfc5646.Load(rfc5646File);

var iso6393File = Path.GetFullPath(Path.Combine(projectDirectory, "Data/iso-639-3.tab"));
var iso6393 = new Iso6393();
iso6393.Load(iso6393File);