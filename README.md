# Utilities

Some useful and not so useful C# .NET utility classes.

## Build and Distribution

- **Source Code**: [GitHub][github-link] for source, issues, discussions, and CI/CD pipelines.
- **Versioned Releases**: [GitHub Releases][releases-link] for version-tagged source archives and build artifacts.
- **NuGet Packages**: [NuGet Packages][nuget-link] for the published `ptr727.Utilities` library.

### Build Status

[![Release Status][releasebuildstatus-shield]][actions-link]\
[![Last Commit][lastcommit-shield]][commits-link]\
[![Coverage][coverage-shield]][coverage-link]

### Releases

[![GitHub Release][releaseversion-shield]][releases-link]\
[![GitHub Pre-Release][prereleaseversion-shield]][releases-link]\
[![NuGet Release][nugetreleaseversion-shield]][nuget-link]

### Release Notes

**Version: 4.1**:

- Fixed `Download.DownloadFile()` and `DownloadFileAsync()` corrupting the destination when downloading over a longer existing file. The destination is now truncated and rewritten in place, keeping its permissions and any links to it. A download that fails partway leaves a short file rather than a mix of the new content and the old.
- Fixed the `StringHistory` limit properties: `MaxFirstLines` and `MaxLastLines` now honor a limit assigned after lines have been appended, document zero as retaining no lines on that side rather than as no limit, and reject a negative value with `ArgumentOutOfRangeException`.

See [Release History](./HISTORY.md) for complete release notes and older versions.

## Table of Contents

- [Build and Distribution](#build-and-distribution)
  - [Build Status](#build-status)
  - [Releases](#releases)
  - [Release Notes](#release-notes)
- [Installation](#installation)
- [Questions or Issues](#questions-or-issues)
- [Contributing](#contributing)
- [3rd Party Tools](#3rd-party-tools)
- [License](#license)

## Installation

```shell
# Add the package to your project
dotnet add package ptr727.Utilities
```

```csharp
// Include the namespace
using ptr727.Utilities;
```

## Questions or Issues

- Report a defect or request a feature on [GitHub Issues][issues-link].
- Ask a question or start a conversation on [GitHub Discussions][discussions-link].

## Contributing

- **Branching workflow**:
  - The repo uses a two-branch model with ruleset-enforced merge methods.
  - Feature branch -> `develop` via **squash merge** (develop is kept linear).
  - `develop` -> `main` via **merge commit** (preserves develop's commit list on main as the second parent of each release commit).
  - Dependabot targets `main` and `develop` in parallel via separate PRs.
  - See [`WORKFLOW.md`][workflow] for complete details.
- **Code style**:
  - See [`CODESTYLE.md`][codestyle] and [`.editorconfig`][editorconfig] for C# code style rules.
- **Local verification**:
  - See [`OPERATIONS.md`][operations] for the clean-compile, test, and lint commands.
- **Library design**:
  - See [`ARCHITECTURE.md`][architecture] for the project layout and the public-API contracts.

## 3rd Party Tools

The third-party tools, libraries, and actions this project depends on.

- [AwesomeAssertions][awesomeassertions-link]: Assertion library for .NET tests.
- [Codecov][codecov-link]: Coverage reporting service.
- [CSharpier][csharpier-link]: C# code formatter.
- [cspell][cspell-link]: Spell checker.
- [editorconfig-checker][editorconfig-checker-link]: Line-ending and whitespace linter.
- [GitHub Actions][github-actions-link]: CI and automation runner.
- [GitHub Dependabot][dependabot-link]: Dependency update bot.
- [Husky.Net][husky-link]: Git hook manager for .NET.
- [markdownlint-cli2][markdownlint-link]: Markdown linter.
- [Microsoft.Extensions.Http.Resilience][resilience-link]: Resilience pipeline for HttpClient.
- [Microsoft.Testing.Platform][testing-platform-link]: Test runner and extension host for .NET.
- [Nerdbank.GitVersioning][nbgv-link]: Version computation from git height.
- [Polly][polly-link]: Resilience and transient-fault-handling library for .NET.
- [Serilog][serilog-link]: Structured logging library for .NET.
- [xUnit.Net][xunit-link]: Test framework for .NET.

## License

Licensed under the [MIT License][license]\
![GitHub License][license-shield]

<!-- Shields -->

[coverage-shield]: https://img.shields.io/codecov/c/github/ptr727/Utilities?logo=codecov&label=Coverage
[lastcommit-shield]: https://img.shields.io/github/last-commit/ptr727/Utilities?logo=github&label=Last%20Commit
[license-shield]: https://img.shields.io/github/license/ptr727/Utilities?label=License
[nugetreleaseversion-shield]: https://img.shields.io/nuget/v/ptr727.Utilities?logo=nuget&label=NuGet%20Release
[prereleaseversion-shield]: https://img.shields.io/github/v/release/ptr727/Utilities?include_prereleases&filter=*-g*&label=GitHub%20Pre-Release&logo=github
[releasebuildstatus-shield]: https://img.shields.io/github/actions/workflow/status/ptr727/Utilities/publish-release.yml?logo=github&label=Releases%20Build
[releaseversion-shield]: https://img.shields.io/github/v/release/ptr727/Utilities?logo=github&label=GitHub%20Release

<!-- Distribution -->

[actions-link]: https://github.com/ptr727/Utilities/actions
[commits-link]: https://github.com/ptr727/Utilities/commits/main
[discussions-link]: https://github.com/ptr727/Utilities/discussions
[github-link]: https://github.com/ptr727/Utilities
[issues-link]: https://github.com/ptr727/Utilities/issues
[nuget-link]: https://www.nuget.org/packages/ptr727.Utilities/
[releases-link]: https://github.com/ptr727/Utilities/releases

<!-- Repo -->

[architecture]: ./ARCHITECTURE.md
[codestyle]: ./CODESTYLE.md
[editorconfig]: ./.editorconfig
[license]: ./LICENSE
[operations]: ./OPERATIONS.md
[workflow]: ./WORKFLOW.md

<!-- External -->

[awesomeassertions-link]: https://awesomeassertions.org/
[codecov-link]: https://about.codecov.io/
[coverage-link]: https://app.codecov.io/gh/ptr727/Utilities
[csharpier-link]: https://csharpier.com/
[cspell-link]: https://cspell.org
[dependabot-link]: https://github.com/dependabot
[editorconfig-checker-link]: https://github.com/editorconfig-checker/editorconfig-checker
[github-actions-link]: https://github.com/actions
[husky-link]: https://alirezanet.github.io/Husky.Net/
[markdownlint-link]: https://github.com/DavidAnson/markdownlint-cli2
[nbgv-link]: https://github.com/dotnet/Nerdbank.GitVersioning
[polly-link]: https://www.pollydocs.org/
[resilience-link]: https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience
[serilog-link]: https://serilog.net/
[testing-platform-link]: https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-intro
[xunit-link]: https://xunit.net/
