# Architecture

How the **Utilities** library is laid out and what its public surface promises. [`AGENTS.md`](./AGENTS.md) is the agent entry point, [`GOVERNANCE.md`](./GOVERNANCE.md) holds the cross-cutting rules, [`CODESTYLE.md`](./CODESTYLE.md) the code style, and [`OPERATIONS.md`](./OPERATIONS.md) how the repo is run.

The library is published as the NuGet package `ptr727.Utilities` and consumed directly from `main`, so every public type and member is a released contract. A change to one is a change to what consumers already depend on, and the conventions below are what a consumer is entitled to assume.

## Projects

- **`Utilities`** ([`Utilities/Utilities.csproj`](./Utilities/Utilities.csproj)): the library, and the only packable project. Target framework .NET 10.0. Its source is the publisher's shipped input, so a change here republishes the package.
- **`Sandbox`** ([`Sandbox/Sandbox.csproj`](./Sandbox/Sandbox.csproj)): a console app for experimentation. Never packaged or published, and never referenced by the library.
- **`UtilitiesTests`** ([`UtilitiesTests/UtilitiesTests.csproj`](./UtilitiesTests/UtilitiesTests.csproj)): the xUnit v3 suite, asserting through AwesomeAssertions and running on native Microsoft.Testing.Platform.

Shared MSBuild configuration lives in `Directory.Build.props` and every package version in `Directory.Packages.props`, both at the solution root. A `.csproj` carries a property only where it is project-specific or overrides the shared default, and a `PackageReference` carries no `Version` attribute.

## Public API Conventions

These are behavioral contracts rather than formatting rules, which is why they live here and not in [`CODESTYLE.md`](./CODESTYLE.md). A new public member follows them, and an existing one changes only as a deliberate, recorded break.

- **I/O methods return `bool`** for success or failure, and hand back any additional result through an `out` parameter. They do not signal ordinary I/O failure by throwing.
- **Async methods carry the `Async` suffix** and take an optional `CancellationToken cancellationToken = default`, passed through to the underlying call rather than ignored. An async overload returning several values returns a tuple, since `out` parameters are unavailable there.
- **`Download`** reuses a thread-safe `Lazy<HttpClient>` and reads with `HttpCompletionOption.ResponseHeadersRead`, so a large response streams rather than buffering whole. A download to a file replaces that file, so a response shorter than the file already there leaves nothing of it behind.
- **`FileEx`** wraps its I/O in retry logic configured through `Options`, and honors cancellation from both `Options.Cancel` and the method's own token parameter.
- **`StringCompression`** uses Deflate, takes a configurable compression level, and passes `leaveOpen` so the caller keeps ownership of the stream it supplied.
- **`StringHistory`** retains at most `MaxFirstLines` from the head and `MaxLastLines` from the tail. Both limits at zero is the one unrestricted mode, and zero on a single side retains no lines on that side. Either limit rejects a negative value, and assigning one re-partitions the lines already stored, so the history never holds more than the limits then in force allow. Re-partitioning only discards: once a line has been dropped the head is closed, so a later, larger `MaxFirstLines` never promotes a retained tail line into it.
- **`Extensions`** uses the C# `extension` block form inside a static class for its logger and string helpers.
- **Logging is a seam, never a dependency.** The library depends on `Microsoft.Extensions.Logging.Abstractions` and takes an `ILoggerFactory` through `LogOptions`. It references no logging framework or sink, so a consumer chooses its own. `Serilog` appears only in `Sandbox` and the tests, where an application legitimately picks one.
