# Utilities

Generally useful and not so useful C# .NET utility classes.

## License

Licensed under the [MIT License](./LICENSE)  
![GitHub](https://img.shields.io/github/license/ptr727/Utilities)

## Build Status

Code and Pipeline is on [GitHub](https://github.com/ptr727/Utilities)  
![GitHub Last Commit](https://img.shields.io/github/last-commit/ptr727/Utilities?logo=github)  
![GitHub Workflow Status](https://img.shields.io/github/workflow/status/ptr727/Utilities/Build%20and%20Publish%20Pipeline?logo=github)

## NuGet Package

Packages published on [NuGet](https://www.nuget.org/packages/InsaneGenius.Utilities/)  
![NuGet](https://img.shields.io/nuget/v/InsaneGenius.Utilities?logo=nuget)

## Notes

- ISO 639-3 language data is sourced from the [ISO 639-3 Registration Authority](https://iso639-3.sil.org/code_tables/download_tables).
  - Download UTF8 format [ISO-639-3 file](https://iso639-3.sil.org/sites/iso639-3/files/downloads/iso-639-3.tab).
  - Compile using Visual Studio's built-in [T4 Tool](https://docs.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates).
  - Or compile from the CLI using the [Mono T4](https://github.com/mono/t4) tool.
    - `wget -O ISO-639-3.tab https://iso639-3.sil.org/sites/iso639-3/files/downloads/iso-639-3.tab`
    - `dotnet tool install -g dotnet-t4`
    - `t4 --out=ISO-639-3.cs ISO-639-3.tt`
