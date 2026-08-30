# Operations

How this repository is run day to day. [`ARCHITECTURE.md`](./ARCHITECTURE.md) is the design counterpart, [`WORKFLOW.md`](./WORKFLOW.md) the CI/CD contract, and [`GOVERNANCE.md`](./GOVERNANCE.md) the cross-cutting rules.

## Local Verification

The clean-compile gate is the [`.NET Format`](./.vscode/tasks.json) VS Code task, which chains `CSharpier Format`, then `.NET Build`, then `dotnet format style --verify-no-changes`. Run it after every code change and before every commit. Running the native commands directly is equally fine, provided the sequence, arguments, and strictness match the task definition exactly, since that definition is the canonical spec.

```shell
dotnet tool restore
dotnet csharpier format --log-level=debug .
dotnet build "$PWD" --verbosity=diagnostic
dotnet format style --verify-no-changes --severity=info --verbosity=detailed
```

**Tests run on native Microsoft.Testing.Platform.** The .NET 10 SDK dropped the VSTest bridge, so the older `--collect:"XPlat Code Coverage"` form exits with zero tests ran rather than degrading, and it is not an alternative to reach for. This is the invocation, and it is the one CI runs:

```shell
dotnet test --coverage --coverage-output-format cobertura --results-directory ./coverage
```

The output filename is left at its default, a GUID basename, because pinning one name would give every test project in the solution the same path and the last to finish would overwrite the rest. CI prefixes each report with `coverage-` afterward, since the bare GUID name is one the Codecov finder does not match, and a report it cannot find is an upload that reports success having sent nothing.

**The document linters run from their official images**, which is the portable path that needs no local Node or Go install. Each takes its globs directly, and a run reporting zero files checked scanned nothing and is not a pass:

```shell
docker run --rm -v "$PWD":/work -w /work davidanson/markdownlint-cli2 '**/*.md'
docker run --rm -v "$PWD":/work -w /work ghcr.io/streetsidesoftware/cspell README.md HISTORY.md
docker run --rm -v "$PWD":/work -w /work rhysd/actionlint
docker run --rm -v "$PWD":/check --workdir /check mstruebing/editorconfig-checker:latest
```

**The repository gates run from a hub checkout**, because the prose and repository gates are hosted there rather than carried here. Both are read-only, and CI runs them on every pull request. Run them from this repository's root, since both resolve their target from the working directory, and point `hub` at a checkout of the fleet template fetched at `main`:

```shell
hub=/path/to/ProjectTemplate
python3 "$hub/.github/actions/prose-gate/prose_lint.py" --diff origin/develop
python3 "$hub/.github/actions/repo-gate/repo_gate.py" --root .
```

The prose gate covers the character set, comment wrapping, sentence style, and dead paths. The repository gate covers action SHA pinning and the line-ending policy, comparing `.editorconfig` against `.gitattributes` and resolving representative paths through `git check-attr`.

**Shell scripts are linted too**, which reaches [`.husky/pre-commit`](./.husky/pre-commit) here, the only tracked file with a shebang. Both run in CI and neither is in the .NET clean-compile, so a hook edit that passes every command above can still red the build:

```shell
docker run --rm -v "$PWD":/mnt --workdir /mnt koalaman/shellcheck:stable -- .husky/pre-commit
docker run --rm -v "$PWD":/mnt --workdir /mnt mvdan/shfmt:latest -d -- .husky/pre-commit
```

**What CI cannot exercise.** The publisher's NuGet push and GitHub release run only on a real publish, so a pull request proves the package builds and packs and never proves it uploads. A change to the push or release path is verified by reading the run of the release it first ships in, not by a green pull request. The local commit hook is likewise a convenience that can be bypassed or never installed, so CI is the authoritative backstop and a locally green tree is not evidence a push will pass.

## Runbooks

**Cutting a release.** Publishing is two-phase, so a human pull request merge never publishes, whichever branch it lands on. A release to NuGet.org and GitHub Releases is a deliberate `workflow_dispatch` of `publish-release.yml`. The one exception is a bot merge to `main` that touches a shipped input, which the publisher gates on the merging actor rather than on the merge itself, so a Dependabot bump republishes the package with its declared dependencies current. Update [`README.md`](./README.md)'s summary and the full entry in [`HISTORY.md`](./HISTORY.md) in the same change that ships the behavior, not afterward.

**Bumping the version floor.** [`version.json`](./version.json) carries the NBGV floor. Raise it on `develop` and let the promotion carry it to `main`, since `main` builds the stable version and every other branch a prerelease.

**Updating the dotnet tools.** [`.config/dotnet-tools.json`](./.config/dotnet-tools.json) pins CSharpier and Husky.Net with `rollForward` false, so a tool version is a deliberate edit rather than a floated resolve. Dependabot does not track this manifest, so a CSharpier package bump it proposes has to be matched here by hand or the formatter CI runs will disagree with the one the editor and the hook run.

```shell
dotnet tool update csharpier --local
dotnet tool update husky --local
dotnet tool restore
```

**Installing the commit hook.** Husky.Net runs the formatting and style half of the clean-compile before a commit, meaning CSharpier and `dotnet format style`, and not the build between them. A fresh clone has to install it once:

```shell
dotnet tool restore
dotnet husky install
```

## Backup and Recovery

The repository holds no state of its own, so recovery is a clone. The published artifacts live outside it: the package on NuGet.org and the archive on the GitHub release, both immutable once pushed. A NuGet version cannot be re-pushed, so a bad publish is corrected by shipping a new version rather than by replacing one, and a dependency bump republishes for the same reason.

## Logs and Debugging

CI logs are the Actions run for the branch, and a failing required check names the job that failed in `Check pull request workflow status job`. Publishing runs under `publish-release.yml` and its logs are the only record of a NuGet push, so read the run rather than inferring the outcome from the release page.

The library itself logs through an `ILoggerFactory` seam and configures no sink, so a consumer's own logging configuration decides what surfaces. `Sandbox` is where a logging setup is exercised locally.

## Tool Usage

- **CSharpier** owns C# formatting, which is why `IDE0055` is the one analyzer rule relaxed repo-wide. It is a local dotnet tool restored from the manifest, so `dotnet tool restore` precedes any use of it.
- **`dotnet format style`** is built into the SDK and needs no restore.
- **Husky.Net** runs the local pre-commit gate, defined in [`.husky/task-runner.json`](./.husky/task-runner.json). It carries the two formatting tasks rather than the whole clean-compile, so the build stays a step you run yourself and CI is the backstop.
- **Docker** is required for the document linters and the EditorConfig checker, and for nothing else in this repo.
- The cspell accepted-word list and the path exclusions both live in [`cspell.json`](./cspell.json), the single source the editor extension, the CLI, and CI all read. Do not keep a parallel word list in the `.code-workspace` file.

## Configuration Layout

- [`.editorconfig`](./.editorconfig): the per-file-type line endings and indentation plus the C# and ReSharper style block. The one repo-wide analyzer relaxation is written here with its reason. [`Utilities/.editorconfig`](./Utilities/.editorconfig) and [`UtilitiesTests/.editorconfig`](./UtilitiesTests/.editorconfig) narrow rules to their own project.
- [`.gitattributes`](./.gitattributes): git's own line-ending enforcement, normalizing every detected text file to LF and holding the Windows command-script exceptions.
- [`.markdownlint-cli2.jsonc`](./.markdownlint-cli2.jsonc), [`cspell.json`](./cspell.json), [`.editorconfig-checker.json`](./.editorconfig-checker.json): the document lint configuration the editor, the CLI, and CI share.
- [`global.json`](./global.json): the `dotnet test` runner selection, which is what puts the test run on Microsoft.Testing.Platform.
- [`codecov.yml`](./codecov.yml): coverage reporting, informational so a coverage delta never gates a pull request.
- [`host-tools.json`](./host-tools.json): what a host needs beyond the fleet declaration, layered over it tighten-only.
- [`.github/dependabot.yml`](./.github/dependabot.yml): every ecosystem listed twice, once per target branch, so `main` and `develop` stay current independently.
