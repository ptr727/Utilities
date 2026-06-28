# Utilities

Some useful and not so useful C# .NET utility classes.

## Build and Distribution

- **Source Code**: [GitHub][github-link] - Source code, issues, discussions, and CI/CD pipelines.
- **Versioned Releases**: [GitHub Releases][releases-link] - Version tagged source code and build artifacts.
- **NuGet Packages** [NuGet Packages][nuget-link] - .NET libraries published to NuGet.org.

### Build Status

[![Release Status][releasebuildstatus-shield]][actions-link]\
[![Last Commit][lastcommit-shield]][commits-link]

### Releases

[![GitHub Release][releaseversion-shield]][releases-link]\
[![GitHub Pre-Release][prereleaseversion-shield]][releases-link]\
[![NuGet Release][nugetreleaseversion-shield]][nuget-link]

### Release Notes

**Version: 3.6**:

**Summary**:

- Internal CI/CD rework, no library API changes.
  - Branch-scoped self-publishing workflows.
  - Keyless OIDC NuGet publishing.
  - Hardened repository configuration, see [WORKFLOW.md](./WORKFLOW.md).

See [Release History](./HISTORY.md) for complete release notes and older versions.

## Installation

```shell
# Add the package to your project
dotnet add package InsaneGenius.Utilities
```

```csharp
// Include the namespace
using InsaneGenius.Utilities;
```

## Contributing

- **Branching workflow**:
  - The repo uses a two-branch model with ruleset-enforced merge methods.
  - Feature branch -> `develop` via **squash merge** (develop is kept linear).
  - `develop` -> `main` via **merge commit** (preserves develop's commit list on main as the second parent of each release commit).
  - Dependabot targets `main` and `develop` in parallel via separate PRs.
  - See [`WORKFLOW.md`](./WORKFLOW.md) for complete details.
- **Code style**:
  - See [`CODESTYLE.md`](./CODESTYLE.md) and [`.editorconfig`](./.editorconfig) for C# code style rules.
- **Repository setup**:
  - See [`repo-config/README.md`](./repo-config/README.md) for repo configuration details.

## License

Licensed under the [MIT License][license-link]\
![GitHub License][license-shield]

<!-- Shields links -->

[actions-link]: https://github.com/ptr727/Utilities/actions
[commits-link]: https://github.com/ptr727/Utilities/commits/main
[github-link]: https://github.com/ptr727/Utilities
[lastcommit-shield]: https://img.shields.io/github/last-commit/ptr727/Utilities?logo=github&label=Last%20Commit
[license-link]: ./LICENSE
[license-shield]: https://img.shields.io/github/license/ptr727/Utilities?label=License
[nuget-link]: https://www.nuget.org/packages/InsaneGenius.Utilities/
[nugetreleaseversion-shield]: https://img.shields.io/nuget/v/InsaneGenius.Utilities?logo=nuget&label=NuGet%20Release
[prereleaseversion-shield]: https://img.shields.io/github/v/release/ptr727/Utilities?include_prereleases&filter=*-g*&label=GitHub%20Pre-Release&logo=github
[releasebuildstatus-shield]: https://img.shields.io/github/actions/workflow/status/ptr727/Utilities/publish-release.yml?logo=github&label=Releases%20Build
[releases-link]: https://github.com/ptr727/Utilities/releases
[releaseversion-shield]: https://img.shields.io/github/v/release/ptr727/Utilities?logo=github&label=GitHub%20Release
