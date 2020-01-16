# Utilities

Generally useful and not so useful C# .NET utility classes.

## License

[![GitHub](https://img.shields.io/github/license/ptr727/Utilities)](https://github.com/ptr727/Utilities/blob/master/LICENSE)

## Project

Code is on [GitHub](https://github.com/ptr727/Utilities).   
CI is on [Azure DevOps](https://dev.azure.com/pieterv/Utilities).

## Build Status

[![Build Status](https://dev.azure.com/pieterv/Utilities/_apis/build/status/Utilities-YAML-CI?branchName=master)](https://dev.azure.com/pieterv/Utilities/_build/latest?definitionId=29&branchName=master)

## NuGet Package

[![NuGet](https://img.shields.io/nuget/v/InsaneGenius.Utilities?logo=nuget)](https://www.nuget.org/packages/InsaneGenius.Utilities/)

## Usage

Packages are on [NuGet](https://www.nuget.org/packages/InsaneGenius.Utilities/).

## Notes

- ISO 639-3 language data is sourced from the [ISO 639-3 Registration Authority](https://iso639-3.sil.org/code_tables/download_tables).

## TODO

- Add more unit tests.
- Add ability to conditionally output to the console, e.g. file retry operations.
- Build the TT file in the build pipeline, currently only visual Studio will run the TT generator.
- Add file and directory permission recovery logic when delete fails.
- Figure out why the `ConsoleEx.WriteLine()` output sometimes ignores the newline.
- Figure out how to use the same version number for the build and the NuGet package.
