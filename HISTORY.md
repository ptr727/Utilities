# Utilities

Some useful and not so useful C# .NET utility classes.

## Release History

- v4.1:
  - Fixed `Download.DownloadFile()` and `DownloadFileAsync()` corrupting the destination file: both opened it with `File.OpenWrite()`, which does not truncate, so a download over a longer existing file left that file's trailing bytes after the downloaded content and reported success. Both now truncate the destination explicitly and rewrite it in place, which keeps its permissions, ownership, and any links to it. The truncation happens once the response headers are accepted rather than once the body has arrived, so a download that fails partway now leaves a short file where it previously left the original bytes behind the new ones.
  - Added `StringHistory.SetLimits()`, which applies both limits in one re-partition. Assigning `MaxFirstLines` and `MaxLastLines` one after the other re-partitions twice, so the first assignment measures against the other limit's previous value and can discard lines the final pair would have retained, which no ordering of the two assignments avoids.
  - Tightened the `StringHistory` limit contract: `MaxFirstLines` and `MaxLastLines` now document zero as retaining no lines on that side rather than as no limit (both at zero remains the unrestricted mode), reject a negative value with `ArgumentOutOfRangeException` at the constructor and at the property rather than at a later `AppendLine()`, and re-partition the lines already stored when assigned, so a limit set after appending is honored instead of ignored. `AppendLine()` changed with them: what is retained is now always a prefix of the appended lines followed by a suffix of them, so once a line has been discarded the head is trimmed but never refilled, and a later, larger `MaxFirstLines` raises the ceiling without adopting retained tail lines as first lines.
- v4.0:
  - Added `HttpClientFactory`, a reusable resilient HTTP client factory built on `Microsoft.Extensions.Http.Resilience` (Polly) with retry, circuit breaker, and connection pooling, tunable through the new `HttpClientOptions`. It exposes a shared singleton client, caller-owned clients, and the resilience handler for callers that build their own client with a custom base address or headers.
  - Added `AssemblyInfo`, an AOT-safe assembly and application identity helper whose `For<T>()` substitutes for `Assembly.GetExecutingAssembly()` (unreliable under Native AOT), and which supplies the consuming application name, version, and a default User-Agent.
  - Reworked `Download` to build its `HttpClient` through `HttpClientFactory`, so downloads now flow through the shared retry and circuit-breaker pipeline. The `TimeoutSeconds` property and all method signatures are unchanged.
  - Changed public members that exposed `List<T>` to safe collection types (a breaking API change): `FileEx.EnumerateDirectories()` / `EnumerateDirectory()` now return `Collection<T>` out parameters and accept `IEnumerable<string>`, and `StringHistory.StringList` is now a `ReadOnlyCollection<string>`.
  - Gated the library's reference AOT verification behind an explicit `PublishAot` opt-in, and turned the `Sandbox` project into a Native AOT smoke test (published and run as AOT) that proves the resilience pipeline and assembly-identity resolution work under Native AOT.
  - Renamed the NuGet package and root namespace from `InsaneGenius.Utilities` to `ptr727.Utilities`, a breaking change. Consumers must update their package reference and change `using InsaneGenius.Utilities;` directives to `using ptr727.Utilities;`. The assembly is now named `Utilities`.
  - Replaced the Serilog-coupled logging model with the backend-agnostic `Microsoft.Extensions.Logging` abstraction.
  - Removed the global Serilog `LogOptions.Logger` property (a breaking API change) in favor of a thread-safe, injectable `ILoggerFactory` configured via `LogOptions.SetFactory(...)` / `TrySetFactory(...)`. The library now depends only on `Microsoft.Extensions.Logging.Abstractions`.
  - Reworked `FileEx` and `Download` to resolve per-class cached loggers through `LogOptions.CreateLogger(...)` and to emit source-generated `[LoggerMessage]` messages, keeping the build clean under `AnalysisMode=All` and `TreatWarningsAsErrors`.
  - Moved the `LogAndHandle()` / `LogAndPropagate()` helpers onto `Microsoft.Extensions.Logging.ILogger` as internal extensions (exposed to tests via `InternalsVisibleTo`).
  - Renamed the public `Extensions` class to `CompressExtensions` (a breaking API change for direct references, though instance-style extension calls such as `value.Compress()` are unaffected), resolving the naming clash with the `Microsoft.Extensions` namespace.
  - Updated the `Sandbox` example to configure a Serilog console logger and inject it through a `SerilogLoggerFactory`.
  - Dropped the library's Serilog dependency and its `IL3058` AOT warning suppression.
- v3.6:
  - Reworked the CI/CD pipeline to the branch-scoped self-publishing model: `main` publishes stable releases and `develop` publishes prereleases, each branch publishing itself when a shipped input changes.
  - Switched NuGet publishing to keyless OIDC trusted publishing, removing the `NUGET_API_KEY` secret.
  - Added `WORKFLOW.md`, the canonical CI/CD specification, and `repo-config/`, the rulesets and repository settings as code.
  - Bundled the build output and NuGet packages into a `Utilities.7z` asset on each GitHub release.
- v3.5:
  - Re-synced the repository structure and agent documentation: added this `HISTORY.md` and a `CODESTYLE.md` .NET style guide, narrowed `.github/copilot-instructions.md` to the Copilot review runbook, and refreshed `AGENTS.md` conventions.
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
