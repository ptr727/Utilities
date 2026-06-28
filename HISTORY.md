# Utilities

Some useful and not so useful C# .NET utility classes.

## Release History

- v3.6:
  - Reworked the CI/CD pipeline to the branch-scoped self-publishing model: `main` publishes stable releases and `develop` publishes prereleases, each branch publishing itself when a shipped input changes.
  - Switched NuGet publishing to keyless OIDC trusted publishing, removing the `NUGET_API_KEY` secret.
  - Added `WORKFLOW.md`, the canonical CI/CD specification, and `repo-config/`, the rulesets and repository settings as code.
  - Bundled the build output and NuGet packages into a `Utilities.7z` asset on each GitHub release.
- v3.5:
  - Re-synced the repository structure and agent documentation with the upstream ProjectTemplate: added this `HISTORY.md` and a `CODESTYLE.md` .NET style guide, narrowed `.github/copilot-instructions.md` to the Copilot review runbook, and refreshed `AGENTS.md` conventions.
  - Corrected the versioning policy to bump `version.json` only for functional changes.
  - Swapped the recommended Todo VS Code add-on from Todo Tree to Better Todo Tree.
- v3.4:
  - .NET 10 and AOT support.
  - Removed `ProcessEx` process wrapper classes, use [CliWrap](https://github.com/Tyrrrz/CliWrap) instead.
  - Code cleanup with help from Copilot.
- v3.3:
  - Language tags split out into a separate dedicated library.
- v3.2 and earlier:
  - Utility classes for downloads, file and directory operations with retry logic, string compression, byte-size formatting, console helpers, and command-line parsing.
