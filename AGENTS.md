# Instructions for AI Coding Agents

**Utilities** is a C# .NET NuGet library (published as `InsaneGenius.Utilities`). The library ships under [`Utilities/`](./Utilities/), with a `Sandbox/` console app for experimentation and `UtilitiesTests/` for xUnit tests. This file is the cross-cutting source of truth for process rules and this repo's project-specific conventions and public-API contracts; the code-style rules live in [`CODESTYLE.md`](./CODESTYLE.md) at the repo root - one guide with a General section that applies repo-wide plus a droppable .NET language section.

This repo tracks the [ptr727/ProjectTemplate](https://github.com/ptr727/ProjectTemplate) two-phase release model. It is a **NuGet-only** derivation: it has no Docker, executable, PyPI, or codegen targets, so the template's `build-docker-task.yml`, `build-executable-task.yml`, `build-pypilibrary-task.yml`, and `run-codegen-*.yml` workflows are intentionally absent, and the merge-bot carries only the Dependabot path. Keep the remaining workflow filenames and structure aligned with the template so upstream changes apply as minimal deltas.

## Template Adaptations

Intentional, documented deviations from the template. Anything here differs on purpose; anything not here is expected to match the template.

- **NuGet-only target set.** No Docker, executable, PyPI, or codegen targets, so the corresponding `build-*-task.yml` / `run-codegen-*.yml` workflows are absent and the merge-bot carries only the Dependabot path (see the repo overview above). `publish-release.yml` keeps its repo-specific `date-badge` per-branch matrix and omits the template's PyPI/Docker jobs accordingly.
- **Husky-driven clean-compile is the CI style gate.** The template ships no hook runner and lints style only in CI. This repo wires Husky.Net (`.husky/` ships) as a local pre-commit gate, and the `test-pull-request.yml` `unit-test` job runs the same checks via `dotnet husky run` (`dotnet tool restore` -> `dotnet husky install` -> `dotnet husky run`) rather than invoking `dotnet csharpier check` + `dotnet format style --verify-no-changes` directly. This is the intentional equivalent of the template's CI style step - same CSharpier + `dotnet format` checks, driven through the repo's hook runner so the local gate and CI run identical commands. Keeping the working gate is sanctioned by [CODESTYLE.md](./CODESTYLE.md) "Clean-Compile Verification", not drift.
- **`.husky/pre-commit` LF pin in `.editorconfig`.** The carried `.editorconfig` adds a `[.husky/pre-commit]` block pinning the extensionless hook to LF (the template's `[*.sh]` rule does not match it). Required because this repo ships the Husky hook.
- **Brownfield analyzer relaxations.** A pre-existing set of analyzer rules are relaxed to suggestion in `.editorconfig` / project `.editorconfig`s (and `IL3058` via `NoWarn`) to keep the published public API stable; each is documented inline (see "Build Configuration").

## Git and Commit Rules

- **Default to staging, not committing.** Stage changes with `git add` and leave `git commit` to the developer unless the developer has explicitly authorized the agent to commit for the current ask ("commit this", "open a PR", etc.). Authorization is scope-bound - it covers the commits needed for that specific task, not a blanket commit license for the rest of the session.
- **All commits must be cryptographically signed (SSH or GPG).** Branch protection enforces this on both branches; unsigned commits are rejected on push. Signing depends on environment configuration - `git config commit.gpgsign true`, a configured `user.signingkey`, and a working signing agent (loaded `ssh-agent` for SSH, or `gpg-agent` for GPG). If signing is not configured in the environment, **do not commit** - surface the missing config to the developer and stop at `git add`. Verify before any agent-authored commit (`git config --get commit.gpgsign && ssh-add -L` or the GPG equivalent). **Signing must be live before the *first* commit, not retrofitted.** Turning on `Require signed commits` against a branch that already has unsigned commits forces a rewrite of that entire history to re-sign it - changing every commit SHA and making whoever does the rewrite the committer and signer of every commit (a rebase preserves the `author` field but not the original signatures; you cannot sign another contributor's commits for them). During new-repo setup, never create commits until signing is verified.
- **Never force push.** Do not run `git push --force` or `git push --force-with-lease` under any circumstances. Force pushing rewrites shared history and can cause data loss.
- **Never run destructive git commands** (`git reset --hard`, `git checkout .`, `git restore .`, `git clean -f`) without explicit developer instruction.

## Branching Model

- `develop` is the integration branch. Feature branches -> `develop` is **squash-only**; develop is kept linear.
- `develop` -> `main` is **merge-commit only** (no squash, no rebase). Merge commits preserve develop's commit list as a real second-parent reference on main.
- All commits on both branches must be cryptographically signed (SSH or GPG). Squash and merge commits created via the GitHub UI are signed by GitHub's web-flow key.
- **`develop` is forward-only - no `main -> develop` back-merges.** Each branch absorbs its own Dependabot PRs directly.
- **Both branch rulesets intentionally omit "Require branches to be up to date before merging".** On `main` the graph-based check would fail on every release (main's new merge commit is never back-merged into develop); on `develop` it stalls bot auto-merge when two bot PRs land in the same window.
- **Dependabot targets both `main` and `develop` in parallel.** [`.github/dependabot.yml`](./.github/dependabot.yml) duplicates every ecosystem entry (one per branch). The merge-bot ([`.github/workflows/merge-bot-pull-request.yml`](./.github/workflows/merge-bot-pull-request.yml)) dispatches `--squash` or `--merge` from each PR's base ref via a `case` statement so the form matches the ruleset on either base. Dependabot **security** PRs always open against the default branch (`main`) - the same `case` statement covers them.
- **Maintainer-pushed commits on a bot PR auto-disable auto-merge.** The merge-bot's `merge-dependabot` job only fires on `opened` / `reopened` (auto-merge is enabled once per PR); the `disable-auto-merge-on-maintainer-push` job disables it on a `synchronize` event whose actor isn't Dependabot. Re-enable manually when ready.
- **App-token workflows use Client ID, not App ID.** `actions/create-github-app-token` deprecated the numeric `app-id` input in v3.0.0; use `client-id: ${{ secrets.CODEGEN_APP_CLIENT_ID }}`.

## Release Model

The repo uses a **two-phase model by default**: PRs build fast, publishing is batched.

- **PRs smoke-test only.** [`test-pull-request.yml`](./.github/workflows/test-pull-request.yml) always runs unit tests, then a `dorny/paths-filter` `changes` job gates a smoke build of the library only when it changed (Debug for develop / Release for main), never publishing.
- **Merges don't publish by default.** [`publish-release.yml`](./.github/workflows/publish-release.yml) is the sole publisher: its **weekly schedule** (Mondays 02:00 UTC) and **manual `workflow_dispatch`** always do the full build/publish of **both** `main` and `develop` (a branch matrix). Its `push` trigger publishes only when the **`PUBLISH_ON_MERGE` repository variable** is `true` (opt-in continuous-release). Unset/`false` = two-phase.
- **Idempotent weekly republish.** NBGV can produce the same `SemVer2` on an unchanged branch, so the GitHub release step is skipped when the tag already exists, and the NuGet push uses `--skip-duplicate` - an unchanged week is a no-op.
- **Required check.** The `changes` job is in the `Check pull request workflow status` aggregator's `needs` and **must succeed** (not just "not fail") so a paths-filter error can never let a library-changing PR merge with its smoke build silently skipped. Skipped smoke jobs (no matching change) pass; `failure`/`cancelled` blocks.
- **Reusable-task parameter contract.** Every `build-*-task.yml` and `build-release-task.yml` takes `ref` (git ref to check out/version), `branch` (logical branch driving config/tags/prerelease - `main` => Release/non-prerelease, else Debug/prerelease), and where relevant `smoke`. **Branch-derived config keys off `inputs.branch`, never `github.ref_name`** - the publisher's matrix builds `develop` from a run whose `github.ref_name` is `main`. Artifact names are branch-suffixed so both matrix legs coexist in one run.
- **Versioning is semantic and maintainer-controlled.** The `version` (major.minor) in [`version.json`](./version.json) is the version floor; NBGV appends the git height (the SemVer patch position) for the build version. `main` (the public release ref) builds a stable `X.Y.<height>`; `develop` builds a prerelease `X.Y.<height>-g<sha>`. The maintainer edits `version.json`; dependency bumps, CI/workflow fixes, doc edits, and template re-syncs leave it untouched.
  - **Bump `version.json` only for functional changes, by maintainer instruction.** Raise the major/minor when the work being introduced warrants a new semantic version - a new feature, a behavior or API change, a breaking change - and do it in the PR that introduces that work (typically on `develop`). Do **not** bump on a fixed cadence or mechanically after a release. NBGV advances the patch (git height) on every commit automatically, so a release always gets a fresh build version without any `version.json` edit.
  - **No post-release bump; no develop-ahead requirement.** NBGV advances the patch (git height) on every commit, so a release always gets a fresh build version with no `version.json` edit and there is no `bump-version-X.Y` PR after a release. A `develop -> main` promotion carries whatever `version.json` is current: a promotion with a functional bump releases that new version on `main`; a maintenance-only promotion carries the unchanged `version.json` and `main` advances only its NBGV height.

## Build Configuration

- **Central Package Management.** Package versions live in [`Directory.Packages.props`](./Directory.Packages.props); shared build properties (target framework, analyzers, `TreatWarningsAsErrors`) live in [`Directory.Build.props`](./Directory.Build.props). Project files carry no `Version=` on `<PackageReference>`.
- **Versioning.** Nerdbank.GitVersioning reads [`version.json`](./version.json); only `main` is a public release ref. Don't put release-bump magnitude in PR titles - NBGV computes the next version from git history.
- **Analyzer relaxations.** `Directory.Build.props` mirrors the template's strict `AnalysisLevel latest-all` / `AnalysisMode All` / `TreatWarningsAsErrors`. Because this is a pre-existing (brownfield) library, a specific set of rules that would otherwise break the build - or require breaking the published public API - are relaxed back to suggestion in [`.editorconfig`](./.editorconfig) (and `IL3058` via `NoWarn` in the AOT project files). Each relaxation is documented inline; prefer fixing new violations over adding new relaxations.

## Pull Request Title and Commit Message Conventions

### Format

- Imperative subject summarizing the change, <=72 characters, no trailing period. ("Add async download overloads", not "Added X" or "Adds X".)
- Optional body, blank-line separated, explaining *why* the change is being made when that's non-obvious. The diff shows *what*.

### Rules

- Don't write `update stuff`, `wip`, or other vague titles. (Dependabot's default `Bump X from Y to Z` titles are fine - keep them.)
- Don't add `Co-Authored-By:` lines unless the developer explicitly asks.
- Don't put release-bump magnitude in the title - no "minor", "patch", "release v3.5", etc. Nerdbank.GitVersioning computes the next release version from `version.json` + git history. Dependency versions in dependency-bump titles are fine and expected.
- Use US English spelling and match the existing heading style of the file you're editing: title case with lowercase short bind words (a, an, the, and, but, or, of, in, on, at, to, by, for, from); hyphenated compounds capitalize both parts unless the second is a short preposition (*Built-in*, *EPA-Corrected*, *24-Hour*).

### Examples

```text
Add structured logging extensions to library
Pin softprops/action-gh-release to commit SHA
Drop ProcessEx wrapper in favor of CliWrap
Bump xunit.v3 from 3.2.2 to 3.3.0
Clarify release model in README
```

## Documentation Style Conventions

### Markdown

- Use reference-style links for any URL referenced more than once or appearing in lists; alphabetize the reference definitions block. Inline single-use relative links (e.g. `[CODESTYLE.md](./CODESTYLE.md)`) are fine.
- One logical paragraph per line; no hard-wrap line-length limit. For an intentional hard line break within a block - stacked badges, status, or license lines - end the line with a trailing backslash (`\`); this explicit form is preferred over trailing whitespace and is not treated as a paragraph split.
- Headings follow the title-case-with-short-bind-words rule from the PR-title section.
- **Write docs in the current state, not as a change from a prior one.** Describe what *is*: "X does Y", never "X *now* does Y" or "changed/switched to Y". Before/after framing belongs in changelogs, commit messages, and PR descriptions - not in `README.md` or other living docs.

### Comments

Applies to code and workflow (`#`) comments alike.

- Comment only when the code does not explain itself or the logic is genuinely complex. Self-evident code needs no comment.
- Write for the human reading *this* project's code now: state what the code does and only the non-obvious *why*. No cross-project references (do not name other repos), no historic or design narrative, no rule citations - governance lives in this file, not echoed inline.
- Match the surrounding code's line length (typically ~120), not an 80-column wrap.

### Character Set

- **Write ASCII in all agent-authored text** - documentation, code, comments, commit messages, and PR descriptions. Replace typographic Unicode with its ASCII equivalent on sight:
  - em dash and en dash -> hyphen `-` (use a spaced ` - ` for an em-dash-style clause break)
  - right arrow -> `->`; double arrow -> `=>`; `<=` and `>=` for the inequality symbols
  - curly quotes -> straight `'` and `"`; ellipsis -> `...`
- **Allowed non-ASCII**: scientific/technical symbols with no clean ASCII equivalent (ohm, micro, degree, pi), and Unicode the developer deliberately typed (emoji callout markers in `README.md`). Preserve those; never strip the developer's own characters.

### Line Endings

- **[`.editorconfig`](./.editorconfig) defines the correct line ending per file type:** **CRLF** for `.md`, `.cs`, XML/`.csproj`/`.props`/`.targets`, `.yml`/`.yaml`, `.json`, and `.cmd`/`.bat`/`.ps1`; **LF** for `.sh`. `.gitattributes` is `* -text`, so git stores the exact bytes you commit and will **not** normalize endings for you.
- **New files:** create them with the `.editorconfig`-mandated ending. **Editing an existing file:** preserve the file's current line endings - do not reflow them as a side effect of a content change. After any programmatic edit, verify with `git diff --stat` that only the lines you changed are touched; if a diff balloons to the whole file, you flipped the endings - restore them and re-stage.

### Quantitative Claims

- Any quantitative claim in `README.md` (counts, sizes, version floors, supported platforms) must be verified against current code. If a doc number is derived from a code constant, mark the dependency in a source-code comment so the next editor knows to update both.

## PR Review Etiquette

> **Mandatory in every derived repo.** This entire "PR Review Etiquette" section is the provider-agnostic review-loop *contract* and must be carried **verbatim** into every repo derived from this template, alongside the [`.github/copilot-instructions.md`](./.github/copilot-instructions.md) "GitHub Copilot Review Runbook" that implements it. Without both in-repo, an agent working in the derived repo has no pointer to the reliable Copilot mechanics and falls back to ad-hoc (and known-broken) behavior.

The repo runs a review loop on every PR: local agent iteration plus remote automated review (GitHub Copilot is the configured reviewer). Treat this as a contract regardless of which local agent authored the changes.

### Merge Gate (read this first)

**Do not merge - and do not enable auto-merge - unless ALL of these hold:**

1. Required status checks are green (`mergeStateStatus: CLEAN`), **and**
2. A Copilot review is confirmed on the **current head SHA** (not an earlier push), **and**
3. **Every** Copilot finding on that head SHA is closed out - all review threads resolved, **and** any issue-level Copilot comments (which have no resolve action) triaged and replied to - so zero outstanding findings remain, **and**
4. The maintainer has given **explicit** permission to merge.

`mergeStateStatus: CLEAN` reflects **only** required statuses - it never reflects open bot review comments, so `CLEAN` alone is **never** sufficient to merge. A green/`CLEAN` PR with an unresolved Copilot finding fails this gate; treat it as "not mergeable" no matter what the merge-state field says. The agent never merges on its own (consistent with "default to staging"; merging is maintainer-authorized).

**Merging is not releasing.** A merge to a release branch does **not** by itself publish; publishing is a separate step in the repo's release pipeline (a scheduled run or a manual dispatch), not an automatic consequence of merging. Never describe a merge as cutting a release, and never trigger a publish without explicit maintainer instruction.

### Expected Review Loop

1. Push changes to the PR branch.
2. Re-request a review for the **current head SHA**. Auto-trigger is unreliable, so request it explicitly via the `requestReviews` GraphQL mutation (now reliable end-to-end - see the runbook); the UI is only a fallback.
3. Wait for review activity on that head. A completed review that raises **no findings** is a valid terminal outcome for that head - proceed; do not re-trigger it or treat the absence of comments as a missing review.
4. Triage findings.
5. Apply fixes or write a rationale for declines.
6. Reply to each thread and resolve what was addressed.
7. Re-run the loop after every fix push until no actionable findings remain.

Drive the loop to green - review confirmed on the latest head SHA and every actionable finding closed - then stop and apply the **Merge Gate** above: all four preconditions must hold, and `mergeStateStatus: CLEAN` alone never satisfies it.

For provider-specific mechanics (how to request review, query review state, post replies, resolve threads), see the **GitHub Copilot Review Runbook** in [.github/copilot-instructions.md](./.github/copilot-instructions.md). This file owns the contract; that file owns the mechanics.

### Triaging Review Comments

For each comment, classify before responding:

- **Bug** - wrong behavior, missing test coverage, or a real divergence between code and docs. Fix it. Reply with the fixing commit SHA when done.
- **Style/convention** - the comment cites a rule from this file or a language-specific style guide. Two cases:
  - The cited rule matches what the existing codebase already does -> fix the offending code.
  - The cited rule contradicts what's in the tree, or industry norm -> **update the rule instead of the code**. The rule is wrong, not the code. Bouncing the same code across rounds is the symptom of a wrong rule. Heuristic: three rounds on the same style category means the rule needs adjusting and the user should authorize the rule change.
- **Architectural opinion** - the comment proposes a different design ("constrain this to disabled-by-default", "move it elsewhere", "add a runtime guardrail"). This is judgment, not a bug. Surface it to the user with a recommendation; don't apply unilaterally.

### Responding and Resolution Expectations

Reply inline with either the fixing commit SHA (for accepted issues) or a concise rationale (for declines). Resolve review threads when addressed or intentionally declined with rationale. Issue-level comments (those at `repos/.../issues/<N>/comments` rather than tied to a specific line) have no resolution action - acknowledge with a reply if needed and move on.

After the final push on a PR, sweep older threads from earlier rounds whose code paths no longer exist; otherwise stale unresolved markers remain in the review UI.

### Escalating to the User

Bring the user in when:

- **Genuine design trade-off** surfaces (fail-open vs fail-closed, narrow vs broad refactor scope, "should we add a guardrail or trust the docstring"). Triage, recommend, ask.
- **Repeated friction** across rounds without convergence - that's the rule-needs-updating signal. Stop, summarize the pattern, and let the user authorize the rule change.
- **Architectural redesign** is requested rather than a bug fix. Surface with a recommendation; never apply unilaterally.

Anti-pattern: don't keep flipping the code on the same style point. Flip the rule once and stick to the rule.

## Workflow YAML Conventions

- **Action pinning**: pin **every** action to a commit SHA with a trailing `# vX.Y.Z` comment. Documented exception: [`dotnet/nbgv`](./.github/workflows/get-version-task.yml) is consumed via `@master` because the upstream tag stream lags `master` and Dependabot would propose a downgrade.
- **Filename**: reusable workflows (`on: workflow_call`) end in `-task.yml`; entry-point workflows do not use the `-task` suffix.
- **Workflow `name:`**: reusable workflow names end in **"task"**; entry-point names end in **"action"**.
- **Job and step `name:`**: every job's `name:` ends in **"job"**; every step's `name:` ends in **"step"**. **Exception**: the ruleset-bound required-status-check job `Check pull request workflow status` in `test-pull-request.yml` keeps its name verbatim - renaming silently breaks required-status-check enforcement.
- **Concurrency**: top-level workflows use `group: '${{ github.workflow }}-${{ github.ref }}'`, `cancel-in-progress: true`. Documented exceptions: `merge-bot-pull-request.yml` (`cancel-in-progress: false`, to run enable/disable events to completion in arrival order) and `publish-release.yml` (global ref-independent group + `cancel-in-progress: false`, so scheduled and manual publishes serialize instead of double-publishing).
- **Shells**: multi-line bash `run:` blocks start with `set -euo pipefail`.
- **Conditionals**: multi-line `if:` uses folded scalar `if: >-`.
- **Artifact retention**: intermediate build artifacts (`actions/upload-artifact`) are consumed by a later job in the same run, so set `retention-days: 1` - the default 90-day retention otherwise piles up against the account-wide artifact-storage quota. The durable copies live on the GitHub release, not in workflow artifacts.
- **Tag pinning on releases**: pass `target_commitish` to `softprops/action-gh-release` explicitly, pinned to NBGV's `GitCommitId` (the exact built commit), not `github.sha` or a branch name.
- There is no CI workflow-lint job - lint workflow edits with `actionlint` locally before pushing.

### Running the Linters Locally (Known-Working Invocations)

There is no CI lint job for workflow YAML or Markdown - the gate is local. Prefer the Docker invocations below; they need no local toolchain and auto-discover their targets from the working directory.

- **actionlint** (run after any `.github/workflows/` edit, since workflow-only changes are not smoke-built):

  ```sh
  docker run --rm -v "$PWD":/repo --workdir /repo rhysd/actionlint:latest -color
  ```

  The `rhysd/actionlint` image bundles `shellcheck`, so it also validates `run:` shell blocks.

- **markdownlint-cli2** (mirrors the davidanson VS Code extension via the shared [`.markdownlint-cli2.jsonc`](./.markdownlint-cli2.jsonc)):

  ```sh
  docker run --rm -v "$PWD":/workdir davidanson/markdownlint-cli2:latest "**/*.md"
  ```

When pulling a public image fails on a Docker-Desktop/WSL credential-helper error (`docker-credential-desktop.exe: exec format error`), retry with an empty Docker config: `DOCKER_CONFIG=$(mktemp -d) docker run ...` after writing `{}` to `$DOCKER_CONFIG/config.json`.

## Project Structure

- **.NET projects** (build with `dotnet build`, test with `dotnet test`):
  - `Utilities/` - the reusable .NET NuGet library (published as `InsaneGenius.Utilities`)
  - `Sandbox/` - console app for experimentation
  - `UtilitiesTests/` - xUnit tests
  - **Style guide: [`CODESTYLE.md`](./CODESTYLE.md) ".NET" section**.
- **Cross-cutting**:
  - `.github/` - workflows, Dependabot, Copilot instructions
  - `.vscode/` - debug configs and tasks; the `.NET` clean-compile task group is carried verbatim (see [`CODESTYLE.md`](./CODESTYLE.md))

After editing code, the `.NET` clean-compile (the `.NET Format` task) must pass before commit, and brownfield status never licenses relaxing analyzer severities or silencing newly surfaced diagnostics - both rules live in [`CODESTYLE.md`](./CODESTYLE.md) "General".

## Library API Conventions

Project-specific public-API conventions for the library (these are behavioral contracts, so they live here rather than in `CODESTYLE.md`):

- **I/O methods return `bool`** for success/failure; additional outputs use `out` parameters.
- **Async methods carry the `Async` suffix** and an optional `CancellationToken cancellationToken = default`, passed through to the underlying call.
- **`Download`** reuses a thread-safe `Lazy<HttpClient>` and uses `HttpCompletionOption.ResponseHeadersRead`; async overloads return tuples for multiple values.
- **`FileEx`** wraps I/O in retry logic configured via `Options`, with cancellation via `Options.Cancel` and the method parameter.
- **`StringCompression`** uses Deflate, supports configurable compression levels, and passes `leaveOpen` so the caller retains stream ownership.
- **`Extensions`** uses the C# `extension` syntax (inside a static class) for logger and string helpers.

## Files and Sections Derived Repos Must Carry Verbatim

These artifacts are the template's cross-cutting contract; this repo carries each of them. Re-sync them from the template when it changes, adapting only the noted placeholders.

- **[`AGENTS.md`](./AGENTS.md) "PR Review Etiquette" section** - the provider-agnostic review-loop contract. Carried verbatim (it names no owner/repo).
- **[`.github/copilot-instructions.md`](./.github/copilot-instructions.md)** - the whole file is a drop-in; its "GitHub Copilot Review Runbook" carries the provider mechanics. Only the `<owner>` / `<repo>` placeholders are adapted (to `ptr727` / `Utilities`). Keep it **narrow** - provider mechanics plus the inline commit/PR-title summary; project-specific conventions and API contracts belong in this file instead.
- **[`.markdownlint-cli2.jsonc`](./.markdownlint-cli2.jsonc)** - the shared lint config read by both the davidanson `markdownlint` IDE extension and CLI `markdownlint-cli2`, so the IDE and command line stay in lock-step. Carried verbatim (it is repo-agnostic).
- **[`.editorconfig`](./.editorconfig) and [`.gitattributes`](./.gitattributes)** - line-ending governance. The defaults + per-extension EOL block is always-verbatim; the `[*.cs]` + ReSharper style block at the end is .NET-only (the file marks the boundary).
- **[`CODESTYLE.md`](./CODESTYLE.md)** - the single code-style guide. Its **General** section is always carried; each language section is droppable, but this repo carries the file whole (General plus both the .NET and Python sections) so re-sync is a wholesale replace - only the .NET section is consumed here; the Python section is inert. **Repo-root placement is load-bearing** - `AGENTS.md` links it as `./CODESTYLE.md` and `.github/copilot-instructions.md` as `../CODESTYLE.md`, so moving it breaks those links. The file is genericized with neutral placeholders, so re-sync is a clean wholesale overwrite with nothing to hand-adapt.
- **[`.vscode/tasks.json`](./.vscode/tasks.json)** - carry the **named clean-compile definitions verbatim**: the `.NET Build`, `CSharpier Format`, and `.NET Format` tasks. Their names are owned by the `CODESTYLE.md` ".NET" section and their command sequence + arguments are the canonical clean-compile spec. Convenience tasks (`.NET Tool Update`, `.NET Publish`, `Husky.Net Run`) are the adapt zone.

## Staying in Sync and Reporting Drift Upstream

This repo re-syncs against [`ptr727/ProjectTemplate`](https://github.com/ptr727/ProjectTemplate) periodically, not just at creation: pull the current version of each verbatim-carry artifact above and re-apply it (adapting only the noted placeholders). For [`CODESTYLE.md`](./CODESTYLE.md), re-sync the whole file from the template and then drop the language section(s) this repo doesn't ship (always keeping the General section) - replacing the file wholesale and trimming whole sections is simpler to keep current than hand-editing snippets.

**Drift flows back upstream as an issue, not a private fix.** When re-syncing, if you find a discrepancy that should be fixed in the **template itself** - a gap, an outdated instruction, a missing rule, something that bit this repo and would bite the next derived repo too - **open an issue in [`ptr727/ProjectTemplate`](https://github.com/ptr727/ProjectTemplate)** describing it, rather than only patching it locally. A local fix realigns *this* repo; an upstream issue (then fix) corrects it for every future derived repo and keeps the template the single source of truth. This upstream-issue rule is this repo's sole cross-repo obligation: do not name sibling or downstream repos in this repo's docs, comments, or AGENTS - a reader here cares only about this project.

## Maintainer Setup (GitHub)

- **Secrets**: `NUGET_API_KEY` (NuGet.org push); `CODEGEN_APP_CLIENT_ID` + `CODEGEN_APP_PRIVATE_KEY` for the merge-bot's GitHub App token - add these to **both** the Actions and Dependabot secret stores.
- **Repository variable**: `PUBLISH_ON_MERGE` - leave unset for the two-phase model; set to `true` for continuous-release.
- **Rulesets**: `develop` squash-only, `main` merge-only; both require the `Check pull request workflow status` check and signed commits; both omit "Require branches to be up to date before merging".
