# Code Style and Formatting Rules

This is the single code-style guide for the repo. The **General** section applies repo-wide and is always carried. The **.NET** section is the language section for this repo's C# projects, self-contained like the `.editorconfig` `[*.cs]` block. (The upstream [ProjectTemplate](https://github.com/ptr727/ProjectTemplate) also ships a Python section; this NuGet-only repo drops it.)

Cross-cutting *process* rules (PR titles, branching, US English, markdown style, comments philosophy, workflow YAML, PR review etiquette) live in [AGENTS.md](./AGENTS.md) and are not repeated here.

## General

These rules apply to every language in the repo.

### Tooling Names and Casing

Use each tool's official casing in task labels, docs, and prose - `.NET` (not `.Net`), `CSharpier`. Don't invent personal variants.

### Clean-Compile Verification

Each language defines a **clean-compile** verification - the combination of build, formatter, linter, and code-analysis tools that must report clean before a commit. It is exposed as one or more **named** VS Code tasks (or, where a language ships no tasks, documented commands), and those definitions are **carried verbatim** across derived repos. The concrete names live in the language section below.

- **Run it after every code change.** The clean-compile must pass before you commit; CI runs the same checks as a backstop, and this repo's Husky.Net pre-commit hook runs them locally too.
- **The named task definition is the canonical spec** - its exact command sequence, arguments, and strictness. You may run it through the VS Code task **or** by invoking the equivalent native commands directly; either is fine **only if the sequence, arguments, and strictness match exactly**. No shortcuts and no more-lenient options (for example, never drop `--verify-no-changes` or loosen a `--severity`).
- **A local commit/pre-commit gate is the derived repo's choice - the template ships no hook runner only because no single runner fits every language it targets** (a `dotnet`-tool runner like Husky.Net suits .NET but not Python), **not** as a recommendation against commit gates. CI is the authoritative backstop regardless; a local gate is an additive convenience a repo may wire and keep - Husky.Net (and `dotnet husky run` as a style step) for .NET, `pre-commit` for Python. Keeping a working gate is not drift, and "no hooks ship by default" must not be read as "remove your gate to stay aligned".

### Analyzer Diagnostics and Suppressions

- **A new port is not a license to silence diagnostics.** Brownfield / just-ported status never justifies relaxing analyzer or linter severities or muting newly surfaced warnings - fix them. (The only brownfield allowance in this template is the one-time git-signing / line-ending migration described in [AGENTS.md](./AGENTS.md) and [README.md](./README.md), which has nothing to do with code analysis.)
- **Suppress only genuine false-positives or deliberate, documented exceptions**, always at the **narrowest scope that fits**, in this order of preference:
  1. An **in-code annotation on the specific symbol**, with a justification - the language's attribute/comment form, never a blanket pragma spanning a region.
  2. The **owning project's local config** when the exception is project-wide for one project (e.g. a test project's own `.editorconfig`).
  3. The **root / shared config** only when the suppression is genuinely applicable to **every** project in the repo.
- **Never blanket-relax a batch of rules project-wide** to get a port to build. The per-language mechanics (which attribute, which config key) are in the language section.

### Markdown and Spelling

These apply repo-wide, in every directory:

1. **Markdown linting**: All `.md` files must be lint-clean (error and warning free) via the VS Code `markdownlint` extension. [`.markdownlint-cli2.jsonc`](./.markdownlint-cli2.jsonc) at the repo root is the single source of truth - the davidanson `markdownlint` extension and a command-line `markdownlint-cli2` run both read it, so the IDE and CLI stay in lock-step. Rules it deliberately disables (e.g. `MD013` line-length, `MD033` inline HTML) are **intentional** - do not "fix" them. This file is carried verbatim by every derived repo (see [AGENTS.md "Files and Sections Derived Repos Must Carry Verbatim"](./AGENTS.md#files-and-sections-derived-repos-must-carry-verbatim)). Fix violations at the source rather than disabling rules.
2. **Spelling**: All spelling must be clean via the CSpell VS Code integration; words must be correctly spelled in **US English** (the repo-wide convention - see [AGENTS.md](./AGENTS.md)). Project-specific terms go in the workspace CSpell config.

## .NET

This is the style guide for the .NET projects in this repo: [`Utilities/`](./Utilities/) (the published library), [`Sandbox/`](./Sandbox/) (a console app for experimentation), and [`UtilitiesTests/`](./UtilitiesTests/) (xUnit tests).

### Build Requirements

#### Zero Warnings Policy

**CRITICAL**: All builds must complete without warnings. The project enforces this through:

1. **The `.NET Format` clean-compile task** (see [Clean-Compile Verification](#clean-compile-verification))
   - The .NET clean-compile is the **`.NET Format`** VS Code task, which chains `CSharpier Format` -> `.NET Build` -> `dotnet format style --verify-no-changes`. These three task definitions are carried verbatim in [`.vscode/tasks.json`](./.vscode/tasks.json).
   - After any code change it must pass before commit. Run the `.NET Format` task. To run it natively instead, reproduce that task chain from [`.vscode/tasks.json`](./.vscode/tasks.json) exactly - `CSharpier Format`, then `.NET Build`, then the `dotnet format style --verify-no-changes --severity=info ...` verify - without dropping or loosening any argument (tasks.json is the canonical command spec). Bare `dotnet format` alone, skipping CSharpier or the build, is not sufficient.

2. **Analyzer configuration** (in [`Directory.Build.props`](./Directory.Build.props))
   - `<AnalysisLevel>latest-all</AnalysisLevel>`
   - `<AnalysisMode>All</AnalysisMode>`
   - `<EnableNETAnalyzers>true</EnableNETAnalyzers>`
   - `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` - all warnings must be addressed, not relaxed; see [Analyzer Diagnostics and Suppressions](#analyzer-diagnostics-and-suppressions).

3. **Pre-commit hooks**
   - Husky.Net pre-commit hooks are wired in this repo (`.husky/` ships) and run CSharpier and `dotnet format` on every commit; commits are rejected if formatting fails.

#### Build Tasks

Available VS Code tasks (run them from VS Code's task runner - **Terminal -> Run Task** - or an agent's task-running tool). The first three are the clean-compile set, carried verbatim; the rest are convenience/project-specific tasks:

- `.NET Build`: Build with diagnostic verbosity *(clean-compile)*
- `CSharpier Format`: Auto-format code with CSharpier *(clean-compile)*
- `.NET Format`: Run CSharpier and build, then verify formatting and style with `--verify-no-changes` *(clean-compile; the task to run after edits)*
- `.NET Tool Update`: Update dotnet tools *(convenience)*
- `.NET Publish`: Publish the library, exercising AOT *(project-specific)*
- `Husky.Net Run`: Run the pre-commit hook tasks on demand *(convenience)*

### Tooling and Editor

#### Code Formatting and Tooling

1. **CSharpier**: Primary code formatter
   - Invoked by the `CSharpier Format` task / `dotnet csharpier format --log-level=debug .`
2. **dotnet format**: Style verification
   - Verify no changes: `dotnet format style --verify-no-changes --severity=info --verbosity=detailed`
3. **Other tools**
   - Husky.Net: pre-commit git-hook runner (installed as a dotnet tool)
   - Nerdbank.GitVersioning: Version management

Restore the tools with `dotnet tool restore` before the first commit.

#### Editor Baseline

1. **Required VS Code extensions**: CSharpier, EditorConfig, markdownlint, CSpell
2. **VS Code settings**: Use the workspace settings without overrides

### Coding Standards and Conventions

Note: Code snippets are illustrative examples only. Replace namespaces/types to match the project.

#### C# Language Features

1. **File-scoped namespaces**

   ```csharp
   namespace InsaneGenius.Utilities;
   ```

2. **Nullable reference types**: Enabled (`<Nullable>enable</Nullable>`)
   - Use nullable annotations appropriately
   - Use `required` for mandatory properties

3. **Modern C# features**: Prefer modern language constructs
   - Primary constructors when appropriate
   - Pattern matching over traditional checks
   - Collection expressions when types loosely match
   - Extension methods using the `extension` syntax (inside a static class)
   - Implicit object creation when type is apparent
   - Range and index operators

4. **Expression-bodied members**: Use for applicable members
   - Methods, properties, accessors, operators, lambdas, local functions

5. **`var` keyword**: Do NOT use `var` (always use explicit types)

   ```csharp
   // Correct
   int count = 42;
   string name = "test";

   // Incorrect
   var count = 42;
   var name = "test";
   ```

#### Naming Conventions

1. **Private fields**: underscore prefix with camelCase

   ```csharp
   private readonly HttpClient _httpClient;
   private int _counter;
   ```

2. **Static fields**: `s_` prefix with camelCase

   ```csharp
   private static int s_instanceCount;
   ```

3. **Constants**: PascalCase

   ```csharp
   private const int MaxRetries = 3;
   ```

#### Code Structure

1. **Global usings**: Use `GlobalUsings.cs` for common namespaces

   ```csharp
   global using System;
   global using System.Net.Http;
   global using System.Threading.Tasks;
   global using Serilog;
   ```

2. **Usings placement**: Outside namespace, sorted with `System` directives first

3. **Braces**: Allman style

   ```csharp
   public void Method()
   {
       if (condition)
       {
           // code
       }
   }
   ```

4. **Indentation**
   - C# files: 4 spaces
   - XML/csproj files: 2 spaces
   - YAML files: 2 spaces
   - JSON files: 4 spaces

5. **Line endings**
   - C#, XML, YAML, JSON, Windows scripts: CRLF
   - Linux scripts (`.sh`): LF

6. **`#region`**: Do not use regions. Prefer logical file/folder/namespace organization.
7. **Member ordering (StyleCop SA1201)**: const -> static readonly -> static fields -> instance readonly fields -> instance fields -> constructors -> public (events -> properties -> indexers -> methods -> operators) -> non-public in same order -> nested types

#### Comments and Documentation

1. **XML documentation**
   - `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
   - Document all public surfaces.
   - Single-line summaries, additional details in remarks, document input parameters, return values, exceptions, and add crefs

   ```csharp
   /// <summary>
   /// Example of a single line summary.
   /// </summary>
   /// <remarks>
   /// Additional important details about usage.
   /// </remarks>
   /// <param name="cancellationToken">
   /// A <see cref="System.Threading.CancellationToken"/> that can be used to cancel the request.
   /// </param>
   /// <returns>
   /// A <see cref="bool"/> indicating success.
   /// </returns>
   /// <exception cref="System.ArgumentNullException">
   /// Thrown when a required argument is null.
   /// </exception>
   public async Task<bool> DoWorkAsync(CancellationToken cancellationToken) {}
   ```

#### Analyzer Suppressions (.NET)

Follow the scope hierarchy in [Analyzer Diagnostics and Suppressions](#analyzer-diagnostics-and-suppressions). .NET mechanics, narrowest first:

- **Never use `#pragma warning disable`** to silence an analyzer.
- **Symbol-scoped**: a `[System.Diagnostics.CodeAnalysis.SuppressMessage(...)]` attribute with a `Justification`, on the specific member or type:

  ```csharp
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Design",
      "CA1034:Nested types should not be visible",
      Justification = "https://github.com/dotnet/sdk/issues/51681"
  )]
  ```

- **Project-scoped** (e.g. the test project): a `dotnet_diagnostic.<RULE>.severity` entry in *that project's own* `.editorconfig`, with a comment explaining why. This repo's library exceptions live in [`Utilities/.editorconfig`](./Utilities/.editorconfig) and its test exceptions in [`UtilitiesTests/.editorconfig`](./UtilitiesTests/.editorconfig).
- **Repo-wide**: a `dotnet_diagnostic.<RULE>.severity` entry in the root `.editorconfig`, only when the rule is genuinely not applicable to any project. Relaxing a batch of `CA*` rules (or `dotnet_analyzer_diagnostic.severity`) to push a brownfield port through the build is exactly what this forbids.

#### Error Handling and Logging

1. **Serilog logging**: Use structured logging

   ```csharp
   logger.Error(exception, "{Function}", function);
   ```

2. **Library log configuration**: The library exposes logging configuration
   - Provide options or settings to supply an `ILogger`
   - Offer a global fallback logger for static usage when needed (`LogOptions.Logger`)

3. **CallerMemberName**: Use for automatic function name tracking

   ```csharp
   public bool LogAndPropagate(
       Exception exception,
       [CallerMemberName] string function = "unknown"
   )
   ```

4. **Logger extensions**: Use `Extensions.cs` for logger and other extension methods

   ```csharp
   extension(ILogger logger)
   {
       public bool LogAndPropagate(Exception exception, ...) { }
   }
   ```

5. **Exceptions**: Do not swallow exceptions; log and rethrow or translate to a domain-specific exception

#### Code Patterns

1. **Guard clauses**: Prefer early returns for validation; use `ArgumentNullException.ThrowIfNull()` for null checks
2. **Async all the way**: Avoid blocking calls (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`); use `async`/`await`
3. **Cancellation tokens**: Accept `CancellationToken cancellationToken = default` as the last parameter and pass it through
4. **ConfigureAwait**: In library code, use `ConfigureAwait(false)` unless context is required
   - Do not call `ConfigureAwait(false)` in xUnit tests (see xUnit1030)
5. **Disposables**: Use `await using` for async disposables; prefer `using` declarations; pass `leaveOpen` where a caller owns the stream
6. **LINQ vs loops**: Use LINQ for clarity, loops for hot paths or allocations
7. **HTTP**: Reuse `HttpClient` via a thread-safe `Lazy<HttpClient>`; avoid per-request instantiation; use `HttpCompletionOption.ResponseHeadersRead` for streaming downloads
8. **Collections**: Prefer `IReadOnlyList<T>`/`IReadOnlyCollection<T>` for public APIs
9. **Immutability**: Prefer immutable records and init-only setters; prefer immutable or frozen collections for read-only data
10. **Sealing classes**: Seal classes that are not designed for inheritance
11. **Thread safety**: Use `Lazy<T>` for static thread-safe instantiation; use `Lock` (C# 13+) instead of `object` for locks; avoid static mutable state and document thread-safety guarantees
12. **Performance**: Use `Span<T>`/`Memory<T>` for performance-critical I/O; use `ValueTask` for async methods that may complete synchronously; use `StringBuilder` for string concatenation in loops

#### Testing Conventions

1. **Framework**: xUnit

   ```csharp
   [Fact]
   public void MethodName_Scenario_ExpectedBehavior()
   {
       // Arrange
       int expected = 42;

       // Act
       int actual = GetValue();

       // Assert
       Assert.Equal(expected, actual);
   }
   ```

2. **Organization**: Arrange-Act-Assert pattern
3. **Naming**: Descriptive names with underscores; test file names match the class under test with a `Tests` suffix
4. **Theory tests**: Use `[Theory]` with `[InlineData]`
5. **Async coverage**: Add tests for every new async method

### Project Configuration

1. **Target framework**: .NET 10.0 (`<TargetFramework>net10.0</TargetFramework>`)

2. **AOT compatibility**
   - `<IsAotCompatible>true</IsAotCompatible>`
   - Avoid runtime reflection (not AOT-friendly); prefer source generators
   - Be mindful of trim warnings; verify with `dotnet publish`

3. **Assembly information**
   - Use semantic versioning (Nerdbank.GitVersioning)
   - Include SourceLink: `<PublishRepositoryUrl>true</PublishRepositoryUrl>`
   - Embed untracked sources: `<EmbedUntrackedSources>true</EmbedUntrackedSources>`

4. **Internal visibility**: Use `InternalsVisibleTo` for test access

   ```xml
   <ItemGroup>
     <InternalsVisibleTo Include="UtilitiesTests" />
   </ItemGroup>
   ```

### Best Practices

1. **Code reviews**: All changes go through pull requests
