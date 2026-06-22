# Code Style and Formatting Rules - .NET

This file is the style guide for the .NET projects in this repo: [`Utilities/`](./Utilities/) (the published library), [`Sandbox/`](./Sandbox/) (a console app for experimentation), and [`UtilitiesTests/`](./UtilitiesTests/) (xUnit tests).

Cross-cutting rules (PR titles, branching, US English, markdown style, workflow YAML, PR review etiquette) live in [AGENTS.md](./AGENTS.md) and apply across the repo. This file only documents what's specific to C# / .NET.

## Build Requirements

### Zero Warnings Policy

**CRITICAL**: All builds must complete without warnings. The project enforces this through:

1. **VS Code tasks**
   - `CSharpier Format` -> `.NET Build` -> `.NET Format`
   - `.NET Format` must pass with `--verify-no-changes` before commit
   - Command: `dotnet format --verify-no-changes --severity=info --verbosity=detailed`

2. **Analyzer configuration**
   - `<AnalysisLevel>latest-all</AnalysisLevel>`
   - `<AnalysisMode>All</AnalysisMode>`
   - `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
   - Because this is a pre-existing (brownfield) library, a specific set of rules are relaxed back to suggestion in [`.editorconfig`](./.editorconfig) (each relaxation documented inline); prefer fixing new violations over adding new relaxations.

3. **Pre-commit hooks**
   - Husky.Net pre-commit hooks are wired in this repo (`.husky/` ships) and run CSharpier and `dotnet format` on every commit; commits are rejected if formatting fails.

### Build Tasks

Available VS Code tasks (use via `run_task` tool):

- `.NET Build`: Build the solution
- `.NET Publish`: Publish the library (exercises AOT)
- `.NET Format`: Verify formatting and style (must pass)
- `CSharpier Format`: Auto-format code with CSharpier
- `.Net Tool Update`: Update dotnet tools
- `Husky.Net Run`: Run the pre-commit hook tasks on demand

## Tooling and Editor

### Code Formatting and Tooling

1. **CSharpier**: Primary code formatter
   - Run before committing: `dotnet csharpier format --log-level=debug .`

2. **dotnet format**: Style verification
   - Verify no changes: `dotnet format --verify-no-changes --severity=info --verbosity=detailed`

3. **Other tools**
   - Husky.Net: pre-commit git-hook runner (installed as a dotnet tool)
   - Nerdbank.GitVersioning: version management

The required order is **CSharpier first, then dotnet format**. The Husky.Net pre-commit hook runs `dotnet husky run`, which applies the same checks; restore the tools with `dotnet tool restore` before the first commit.

### Editor Baseline

1. **Required VS Code extensions**: CSharpier, EditorConfig, markdownlint, CSpell
2. **VS Code settings**: Use the workspace settings without overrides

### Markdown Files

1. **Linting**: All `.md` files must be linted with the VS Code `markdownlint` extension (local only; no CI)
2. **Zero warnings**: Markdown linting must be error and warning free
3. **Authoritative config**: [`.markdownlint-cli2.jsonc`](./.markdownlint-cli2.jsonc) at the repo root is the single source of truth - the davidanson `markdownlint` extension and a command-line `markdownlint-cli2` run both read it, so the IDE and CLI stay in lock-step. Rules the config deliberately disables (e.g. `MD013` line-length, `MD033` inline HTML) are **intentional** - do not "fix" them.

### Spelling

1. **CSpell**: All spelling checks must be error free using the CSpell VS Code integration
2. **Accepted spellings**: Words must be correctly spelled in US or UK English
3. **Allowed exceptions**: Project-specific terms must be added to the workspace CSpell config

## Coding Standards and Conventions

Note: Code snippets are illustrative examples only. Replace namespaces/types to match the project.

### C# Language Features

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

### Naming Conventions

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

### Code Structure

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

### Comments and Documentation

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

2. **Code analysis suppressions**
   - Do not use `#pragma` sections to disable analyzers
   - For one-off cases, use suppression attributes with justifications
   - For project-wide suppressions, add rules to `.editorconfig`

   ```csharp
   [System.Diagnostics.CodeAnalysis.SuppressMessage(
       "Design",
       "CA1034:Nested types should not be visible",
       Justification = "https://github.com/dotnet/sdk/issues/51681"
   )]
   ```

### Error Handling and Logging

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

   ```csharp
   try
   {
       // Operation
   }
   catch (IOException e) when (LogOptions.Logger.LogAndHandle(e))
   {
       // Retry or return false
   }
   ```

### Code Patterns

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

### Testing Conventions

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

## Project Configuration

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

## Best Practices

1. **Code reviews**: All changes go through pull requests
