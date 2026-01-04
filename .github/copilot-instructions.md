# GitHub Copilot Instructions for Utilities Project

## Project Overview

This is a .NET utility library that provides generally useful C# classes and extensions. The project targets .NET 10 and includes AOT (Ahead-of-Time) compilation support for optimized runtime performance.

## Code Style and Standards

### General Guidelines
- Follow C# coding conventions and .NET best practices
- Use meaningful variable and method names
- Keep methods focused and single-purpose
- **Add comprehensive XML documentation comments for ALL public APIs** (required)
- Maintain consistency with existing code style
- Follow the existing patterns in the codebase

### Formatting Requirements

**IMPORTANT:** This project uses **Husky.Net** pre-commit hooks that automatically enforce formatting:

1. **CSharpier** is run first for code formatting
2. **dotnet format** is run second for style enforcement

#### Formatting Workflow
```bash
# Always format with CSharpier FIRST after editing code
dotnet csharpier .

# Then run dotnet format to apply .editorconfig rules
dotnet format

# Verify no changes needed
dotnet format --verify-no-changes
```

#### Key Formatting Rules (from .editorconfig)
- **No `var` keyword**: Use explicit types everywhere
  ```csharp
  // ✅ CORRECT
  string text = "hello";
  List<int> numbers = [];
  
  // ❌ WRONG
  var text = "hello";
  var numbers = new List<int>();
  ```
- **Indentation**: 4 spaces (not tabs)
- **Line endings**: CRLF (Windows)
- **Charset**: UTF-8
- **Final newline**: Required
- **Trailing whitespace**: Not allowed
- **File-scoped namespaces**: Required
- **Collection expressions**: Preferred `[]` over `new List<T>()`

#### Pre-Commit Hook
The Husky.Net pre-commit hook automatically runs:
1. `dotnet csharpier .` - Code formatting
2. `dotnet format` - Style enforcement

**Commits will be rejected if formatting fails!**

### .NET 10 and AOT Considerations
- The project targets .NET 10 with PublishAot enabled
- Avoid reflection where possible (not AOT-friendly)
- Use source generators instead of runtime reflection when applicable
- Be mindful of trim warnings and compatibility
- Ensure all code is AOT-compatible
- Test AOT compatibility with `dotnet publish`

## Project Structure

- **Utilities/**: Main library project containing utility classes
  - `CommandLineEx.cs`: Command-line argument parsing utilities
  - `ConsoleEx.cs`: Console interaction helpers with color support
  - `Download.cs`: HTTP download utilities (sync and async)
  - `Extensions.cs`: Extension methods (string compression, logger error handling)
  - `FileEx.cs`: File and directory operation utilities with retry logic (sync and async)
  - `FileExOptions.cs`: Configuration options for FileEx operations
  - `Format.cs`: Byte size formatting utilities (binary and decimal)
  - `LogOptions.cs`: Global logging configuration
  - `StringCompression.cs`: String compression/decompression using Deflate (sync and async)
  - `StringHistory.cs`: Bounded string history buffer

- **Sandbox/**: Console application for testing and experimentation
- **UtilitiesTests/**: Unit tests using xUnit

## Testing

- Use xUnit for all tests
- Follow AAA pattern (Arrange, Act, Assert)
- Test file names should match the class being tested with "Tests" suffix
- Run tests frequently during development
- Maintain good test coverage for public APIs
- **Add tests for all new async methods**
- Consider edge cases and error conditions in tests

## Dependencies

- **Serilog**: Logging framework (required for LogOptions and error handling)
- **Microsoft.SourceLink.GitHub**: Source linking for debugging
- **xUnit**: Testing framework
- Keep dependencies minimal and well-justified
- Update package references to latest stable versions when appropriate

## Common Tasks

### Building
Run the ".NET Build" task or use: `dotnet build`

### Publishing
Run the ".NET Publish" task or use: `dotnet publish`

### Formatting (REQUIRED before commit)
```bash
# Step 1: Format with CSharpier
dotnet csharpier .

# Step 2: Apply dotnet format rules
dotnet format

# Step 3: Verify (this is what pre-commit hook checks)
dotnet format --verify-no-changes
```

### Running Tests
Use: `dotnet test`

## Commit Guidelines

### Pre-Commit Process (Automated by Husky.Net)
The following happens automatically on every commit:
1. ✅ CSharpier formats all C# files
2. ✅ dotnet format applies .editorconfig rules
3. ✅ Commit proceeds if formatting passes
4. ❌ Commit is rejected if formatting fails

### Manual Pre-Commit Checklist
Before committing, ensure:
- [ ] Code formatted with CSharpier (`dotnet csharpier .`)
- [ ] Style rules applied (`dotnet format`)
- [ ] No formatting issues (`dotnet format --verify-no-changes`)
- [ ] All tests passing (`dotnet test`)
- [ ] Build successful (`dotnet build`)
- [ ] No `var` keywords used
- [ ] XML documentation complete
- [ ] Commit message is clear and descriptive

### Commit Message Format
Follow conventional commit format:
```
<type>(<scope>): <description>

[optional body]

[optional footer]
```
Examples:
- `feat(download): add async download methods`
- `fix(fileex): correct boundary condition in DeleteDirectory`
- `docs(readme): update async method examples`
- `test(compression): add async compression tests`

## Package Information

- **Package ID**: InsaneGenius.Utilities
- **Namespace**: InsaneGenius.Utilities
- **License**: MIT
- **Repository**: https://github.com/ptr727/Utilities
- **Target Framework**: .NET 10
- **C# Version**: 14.0
- **Version**: 3.5 (managed by Nerdbank.GitVersioning)

## When Adding New Features

1. **Consider AOT compatibility from the start**
2. **Add comprehensive XML documentation** (required for all public APIs)
3. **Create corresponding unit tests** (including async versions)
4. Update README.md if adding significant functionality
5. Ensure backward compatibility when modifying existing APIs
6. Consider performance implications
7. **Use async/await for I/O-bound operations** with proper cancellation token support
8. Handle exceptions appropriately with logging via LogOptions.Logger
9. **Follow existing patterns** (e.g., retry logic, bool return values, exception handling)
10. **Format with CSharpier before running dotnet format**

## Code Generation Preferences

### Modern C# Features (C# 14)
- Prefer modern C# language features (pattern matching, records, file-scoped namespaces, etc.)
- Use nullable reference types consistently with `ArgumentNullException.ThrowIfNull()`
- Leverage expression-bodied members where appropriate
- Use collection expressions `[]` for initialization
- Use `extension` keyword for extension methods (inside static class)
- Prefer LINQ for data transformations
- Use primary constructors where appropriate

### Async/Await Patterns
- **Always use `ConfigureAwait(false)` in library code**
- Provide async versions of I/O-bound methods
- Use `CancellationToken` parameters (default to `default`)
- Use `await using` for async disposal
- Replace blocking calls (`.GetAwaiter().GetResult()`) with proper async
- Use `Task.Delay()` instead of `Thread.Sleep()` in async methods
- Use `Memory<T>` and `Span<T>` for async I/O operations

### Input Validation
- Use `ArgumentNullException.ThrowIfNull()` for null checks
- Validate parameters early in methods
- Document all exceptions in XML comments

### Resource Management
- Use `using` statements for proper disposal
- Use `await using` for async disposal
- Avoid explicit `.Close()` calls (using handles it)
- Use `leaveOpen` parameter when appropriate

### Thread Safety
- Use `Lazy<T>` for thread-safe initialization
- Use `Lock` (C# 13+) instead of `object` for locks
- Avoid static mutable state
- Document thread-safety guarantees

## Security Considerations

- Validate all user inputs
- Use secure defaults
- Avoid hardcoding sensitive information
- Follow principle of least privilege
- Use secure random number generation when needed
- Be careful with file path manipulation

## Performance Guidelines

- Profile before optimizing
- Be mindful of allocations
- Use `Span<T>` and `Memory<T>` for performance-critical code
- Consider using object pooling for frequently allocated objects
- Use `ValueTask` for async methods that may complete synchronously
- Leverage AOT benefits for startup time and memory usage
- Avoid unnecessary string allocations
- Use `StringBuilder` for string concatenation in loops

## Common Patterns in This Project

### Error Handling
```csharp
try
{
    // Operation
}
catch (IOException e) when (LogOptions.Logger.LogAndHandle(e))
{
    // Retry or return false
}
catch (Exception e) when (LogOptions.Logger.LogAndHandle(e))
{
    return false;
}
```

### Retry Logic
- Use `Options.RetryCount` for retry attempts
- Use `Options.Cancel.IsCancellationRequested` for cancellation
- Use `Task.Delay()` for async waits
- Log retry attempts with `LogOptions.Logger.Information()`

### Method Signatures
- I/O methods return `bool` for success/failure
- Async methods have `Async` suffix
- Async methods include optional `CancellationToken cancellationToken = default`
- Use `out` parameters for additional return values

### XML Documentation
- Always include `<summary>` for all public members
- Document all `<param>` with descriptions
- Document `<returns>` with descriptions
- Document all possible `<exception>` types
- Use `<remarks>` for additional context
- Reference other types with `<see cref="Type"/>`

## Anti-Patterns to Avoid

❌ Sync-over-async: `.GetAwaiter().GetResult()`, `.Wait()`, `.Result`  
❌ Missing `ConfigureAwait(false)` in library code  
❌ Missing XML documentation on public APIs  
❌ Not using `ArgumentNullException.ThrowIfNull()`  
❌ Explicit `.Close()` calls when using `using`  
❌ Missing cancellation token support in async methods  
❌ Race conditions in static initialization  
❌ Reflection (not AOT-compatible)  
❌ Missing tests for async methods  
❌ Using `var` keyword (explicit types required by .editorconfig)  
❌ Forgetting to run CSharpier before dotnet format  

## File-Specific Notes

### Download.cs
- Uses thread-safe `Lazy<HttpClient>` initialization
- Provides both sync and async versions
- Returns tuples from async methods for multiple values
- Uses `HttpCompletionOption.ResponseHeadersRead` for efficiency

### FileEx.cs
- All I/O methods have async versions
- Uses `Options` for retry configuration
- Returns `bool` for success/failure
- Supports cancellation via `Options.Cancel` and method parameter

### StringCompression.cs
- Supports configurable compression levels
- Has both sync and async versions
- Uses `leaveOpen` for stream management
- Proper error documentation

### Extensions.cs
- Uses C# 14 `extension` keyword
- Must be inside static class
- Provides extension methods for string compression and logger error handling

## Development Workflow

### Making Changes
1. Edit code
2. **Run CSharpier**: `dotnet csharpier .`
3. **Run dotnet format**: `dotnet format`
4. Build: `dotnet build`
5. Test: `dotnet test`
6. Commit (Husky.Net pre-commit hook will verify formatting)

### Before Committing
```bash
# Format code (REQUIRED ORDER)
dotnet csharpier .
dotnet format

# Verify
dotnet format --verify-no-changes
dotnet build
dotnet test

# Commit
git add .
git commit -m "feat: your message"
# Husky.Net hook runs automatically
```

### If Pre-Commit Hook Fails
```bash
# Hook will show formatting errors
# Re-run formatters
dotnet csharpier .
dotnet format

# Try commit again
git commit -m "feat: your message"
```

## Tools Required

- .NET 10 SDK
- CSharpier (installed as dotnet tool)
- Husky.Net (installed as dotnet tool)
- Visual Studio 2022 or VS Code with C# extension

## EditorConfig Integration

The project uses `.editorconfig` for style enforcement. Key rules:
- `csharp_style_var_*` = **false** (no var keyword)
- `csharp_style_namespace_declarations` = **file_scoped**
- `csharp_prefer_system_threading_lock` = **true**
- `dotnet_style_prefer_collection_expression` = **when_types_loosely_match**

Visual Studio and Rider automatically apply these settings. VS Code requires the EditorConfig extension.
