# Utilities

Generally useful and not so useful C# .NET utility classes.

## License

Licensed under the [MIT License](./LICENSE)  
![GitHub](https://img.shields.io/github/license/ptr727/Utilities)

## Build Status

Code and Pipeline is on [GitHub](https://github.com/ptr727/Utilities)  
![GitHub Last Commit](https://img.shields.io/github/last-commit/ptr727/Utilities?logo=github)  
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/ptr727/Utilities/BuildPublishPipeline.yml?logo=github)

## NuGet Package

Packages published on [NuGet](https://www.nuget.org/packages/InsaneGenius.Utilities/)  
![NuGet](https://img.shields.io/nuget/v/InsaneGenius.Utilities?logo=nuget)

## Language Matching

- Language Data Sources:
  - ISO 639-2 language data is sourced from the [ISO 639-2 Registration Authority](https://www.loc.gov/standards/iso639-2/langhome.html), [download](https://www.loc.gov/standards/iso639-2/ISO-639-2_utf-8.txt)
  - ISO 639-3 language data is sourced from the [ISO 639-3 Registration Authority](https://iso639-3.sil.org/), [download](https://iso639-3.sil.org/sites/iso639-3/files/downloads/iso-639-3.tab).
  - RFC 5646 / BCP 47 language data is sourced from the [IANA Tags for Identifying Languages RFC 5646](https://www.rfc-editor.org/rfc/rfc5646.html), [download](https://www.iana.org/assignments/language-subtag-registry/language-subtag-registry).
- Manually update language content:
  - If Windows install [wget for Windows](https://winget.run/pkg/JernejSimoncic/Wget) : `winget install -e --id JernejSimoncic.Wget`
  - [Run Task](./.vscode/tasks.json) : `build language files`
- Language matching usage:
  - Refer to [PlexCleaner/Language.cs](https://github.com/ptr727/PlexCleaner/blob/main/PlexCleaner/Language.cs) as an example.
