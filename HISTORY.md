# Utilities

Some useful and not so useful C# .NET utility classes.

## Release History

- v3.7:
  - Renamed the NuGet package and root namespace from `InsaneGenius.Utilities` to `ptr727.Utilities` (a breaking change), aligning with the `ptr727.*` package naming used by the sibling `LanguageTags` project; consumers must update their package reference and change `using InsaneGenius.Utilities;` directives to `using ptr727.Utilities;`. The assembly is now named `Utilities`.
  - Replaced the Serilog-coupled logging model with the backend-agnostic `Microsoft.Extensions.Logging` abstraction, matching the sibling `LanguageTags` project.
  - Removed the global Serilog `LogOptions.Logger` property (a breaking API change) in favor of a thread-safe, injectable `ILoggerFactory` configured via `LogOptions.SetFactory(...)` / `TrySetFactory(...)`; the library now depends only on `Microsoft.Extensions.Logging.Abstractions`.
  - Reworked `FileEx` and `Download` to resolve per-class cached loggers through `LogOptions.CreateLogger(...)` and to emit source-generated `[LoggerMessage]` messages, keeping the build clean under `AnalysisMode=All` and `TreatWarningsAsErrors`.
  - Moved the `LogAndHandle` / `LogAndPropagate` helpers onto `Microsoft.Extensions.Logging.ILogger` as internal extensions (exposed to tests via `InternalsVisibleTo`).
  - Renamed the public `Extensions` class to `CompressExtensions` (a breaking API change for direct references; instance-style extension calls such as `value.Compress()` are unaffected), resolving the naming clash with the `Microsoft.Extensions` namespace.
  - Updated the `Sandbox` example to configure a Serilog console logger and inject it through a `SerilogLoggerFactory`, borrowing the pattern from `LanguageTagsCreate`.
  - Dropped the library's Serilog dependency and its `IL3058` AOT warning suppression.
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
