# Utilities

Some useful and not so useful C# .NET utility classes.

## Release History

- v3.5:
  - Re-synced the repository structure and agent documentation with the upstream ProjectTemplate: added this `HISTORY.md` and a `CODESTYLE.md` .NET style guide, narrowed `.github/copilot-instructions.md` to the Copilot review runbook, and refreshed `AGENTS.md` conventions.
  - Corrected the versioning policy to bump `version.json` only for functional changes.
  - Swapped the recommended Todo VS Code add-on from Todo Tree to Better Todo Tree.
- v3.4:
  - .NET 10 and AOT support.
  - Removed `ProcessEx` process wrapper classes, use [CliWrap](https://github.com/Tyrrrz/CliWrap) instead.
  - Code cleanup with help from Copilot.
- v3.3:
  - Language tags moved to a dedicated [repo](https://github.com/ptr727/LanguageTags).
- v3.2 and earlier:
  - Utility classes for downloads, file and directory operations with retry logic, string compression, byte-size formatting, console helpers, and command-line parsing.
