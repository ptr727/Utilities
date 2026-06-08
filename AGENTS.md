# Instructions for AI Coding Agents

**Utilities** is a C# .NET NuGet library (published as `InsaneGenius.Utilities`). The library ships under [`Utilities/`](./Utilities/), with a `Sandbox/` console app for experimentation and `UtilitiesTests/` for xUnit tests. This file is the cross-cutting source of truth for process rules; the C# style guidance lives in [`.github/copilot-instructions.md`](./.github/copilot-instructions.md).

This repo tracks the [ptr727/ProjectTemplate](https://github.com/ptr727/ProjectTemplate) two-phase release model. It is a **NuGet-only** derivation: it has no Docker, executable, PyPI, or codegen targets, so the template's `build-docker-task.yml`, `build-executable-task.yml`, `build-pypilibrary-task.yml`, and `run-codegen-*.yml` workflows are intentionally absent, and the merge-bot carries only the Dependabot path. Keep the remaining workflow filenames and structure aligned with the template so upstream changes apply as minimal deltas.

## Git and Commit Rules

- **Default to staging, not committing.** Stage changes with `git add` and leave `git commit` to the developer unless explicitly authorized for the current task. Authorization is scope-bound to that task.
- **Never force push** (`git push --force` / `--force-with-lease`) and **never run destructive git commands** (`git reset --hard`, `git checkout .`, `git restore .`, `git clean -f`) without explicit developer instruction.

## Branching Model

- `develop` is the integration branch. Feature branches → `develop` is **squash-only**; develop is kept linear.
- `develop` → `main` is **merge-commit only** (no squash, no rebase). Merge commits preserve develop's commit list as a real second-parent reference on main.
- **`develop` is forward-only — no `main → develop` back-merges.** Each branch absorbs its own Dependabot PRs directly.
- **Both branch rulesets intentionally omit "Require branches to be up to date before merging".** On `main` the graph-based check would fail on every release (main's new merge commit is never back-merged into develop); on `develop` it stalls bot auto-merge when two bot PRs land in the same window.
- **Dependabot targets both `main` and `develop` in parallel.** [`.github/dependabot.yml`](./.github/dependabot.yml) duplicates every ecosystem entry (one per branch). The merge-bot ([`.github/workflows/merge-bot-pull-request.yml`](./.github/workflows/merge-bot-pull-request.yml)) dispatches `--squash` or `--merge` from each PR's base ref via a `case` statement so the form matches the ruleset on either base. Dependabot **security** PRs always open against the default branch (`main`) — the same `case` statement covers them.
- **Maintainer-pushed commits on a bot PR auto-disable auto-merge.** The merge-bot's `merge-dependabot` job only fires on `opened` / `reopened` (auto-merge is enabled once per PR); the `disable-auto-merge-on-maintainer-push` job disables it on a `synchronize` event whose actor isn't Dependabot. Re-enable manually when ready.
- **App-token workflows use Client ID, not App ID.** `actions/create-github-app-token` deprecated the numeric `app-id` input in v3.0.0; use `client-id: ${{ secrets.CODEGEN_APP_CLIENT_ID }}`.

## Release Model

The repo uses a **two-phase model by default**: PRs build fast, publishing is batched.

- **PRs smoke-test only.** [`test-pull-request.yml`](./.github/workflows/test-pull-request.yml) always runs unit tests, then a `dorny/paths-filter` `changes` job gates a smoke build of the library only when it changed (Debug for develop / Release for main), never publishing.
- **Merges don't publish by default.** [`publish-release.yml`](./.github/workflows/publish-release.yml) is the sole publisher: its **weekly schedule** (Mondays 02:00 UTC) and **manual `workflow_dispatch`** always do the full build/publish of **both** `main` and `develop` (a branch matrix). Its `push` trigger publishes only when the **`PUBLISH_ON_MERGE` repository variable** is `true` (opt-in continuous-release). Unset/`false` = two-phase.
- **Idempotent weekly republish.** NBGV can produce the same `SemVer2` on an unchanged branch, so the GitHub release step is skipped when the tag already exists, and the NuGet push uses `--skip-duplicate` — an unchanged week is a no-op.
- **Required check.** The `changes` job is in the `Check pull request workflow status` aggregator's `needs` and **must succeed** (not just "not fail") so a paths-filter error can never let a library-changing PR merge with its smoke build silently skipped. Skipped smoke jobs (no matching change) pass; `failure`/`cancelled` blocks.
- **Reusable-task parameter contract.** Every `build-*-task.yml` and `build-release-task.yml` takes `ref` (git ref to check out/version), `branch` (logical branch driving config/tags/prerelease — `main` ⇒ Release/non-prerelease, else Debug/prerelease), and where relevant `smoke`. **Branch-derived config keys off `inputs.branch`, never `github.ref_name`** — the publisher's matrix builds `develop` from a run whose `github.ref_name` is `main`. Artifact names are branch-suffixed so both matrix legs coexist in one run.

## Build Configuration

- **Central Package Management.** Package versions live in [`Directory.Packages.props`](./Directory.Packages.props); shared build properties (target framework, analyzers, `TreatWarningsAsErrors`) live in [`Directory.Build.props`](./Directory.Build.props). Project files carry no `Version=` on `<PackageReference>`.
- **Versioning.** Nerdbank.GitVersioning reads [`version.json`](./version.json); only `main` is a public release ref. Don't put release-bump magnitude in PR titles — NBGV computes the next version from git history.
- **Analyzer relaxations.** `Directory.Build.props` mirrors the template's strict `AnalysisLevel latest-all` / `AnalysisMode All` / `TreatWarningsAsErrors`. Because this is a pre-existing (brownfield) library, a specific set of rules that would otherwise break the build — or require breaking the published public API — are relaxed back to suggestion in [`.editorconfig`](./.editorconfig) (and `IL3058` via `NoWarn` in the AOT project files). Each relaxation is documented inline; prefer fixing new violations over adding new relaxations.

## Workflow YAML Conventions

- **Action pinning**: pin **every** action to a commit SHA with a trailing `# vX.Y.Z` comment. Documented exception: [`dotnet/nbgv`](./.github/workflows/get-version-task.yml) is consumed via `@master` because the upstream tag stream lags `master` and Dependabot would propose a downgrade.
- **Filename**: reusable workflows (`on: workflow_call`) end in `-task.yml`; entry-point workflows do not use the `-task` suffix.
- **Workflow `name:`**: reusable workflow names end in **"task"**; entry-point names end in **"action"**.
- **Job and step `name:`**: every job's `name:` ends in **"job"**; every step's `name:` ends in **"step"**. **Exception**: the ruleset-bound required-status-check job `Check pull request workflow status` in `test-pull-request.yml` keeps its name verbatim — renaming silently breaks required-status-check enforcement.
- **Concurrency**: top-level workflows use `group: '${{ github.workflow }}-${{ github.ref }}'`, `cancel-in-progress: true`. Documented exceptions: `merge-bot-pull-request.yml` (`cancel-in-progress: false`, to run enable/disable events to completion in arrival order) and `publish-release.yml` (global ref-independent group + `cancel-in-progress: false`, so scheduled and manual publishes serialize instead of double-publishing).
- **Shells**: multi-line bash `run:` blocks start with `set -euo pipefail`.
- **Conditionals**: multi-line `if:` uses folded scalar `if: >-`.
- **Tag pinning on releases**: pass `target_commitish` to `softprops/action-gh-release` explicitly, pinned to NBGV's `GitCommitId` (the exact built commit), not `github.sha` or a branch name.
- There is no CI workflow-lint job — lint workflow edits with `actionlint` locally before pushing.

## Pull Request Title and Commit Message Conventions

- Imperative subject summarizing the change, ≤72 characters, no trailing period.
- Don't write vague titles (`update stuff`, `wip`). Dependabot's default `Bump X from Y to Z` titles are fine.
- Don't add `Co-Authored-By:` lines unless the developer explicitly asks.
- Use US English spelling.

## PR Review Etiquette

The repo runs a review loop on every PR: local agent iteration plus remote automated review (GitHub Copilot is the configured reviewer). Treat this as a contract regardless of which local agent authored the changes.

### Expected Review Loop

1. Push changes to the PR branch.
2. Re-request a review for the **current head SHA**. Auto-trigger is unreliable, so request it explicitly via the `requestReviews` GraphQL mutation (now reliable end-to-end - see the runbook); the UI is only a fallback.
3. Wait for review activity on that head.
4. Triage findings.
5. Apply fixes or write a rationale for declines.
6. Reply to each thread and resolve what was addressed.
7. Re-run the loop after every fix push until no actionable findings remain.

`mergeStateStatus: CLEAN` only checks required statuses; it does not block on bot review comments. Drive the loop to green - review confirmed on the latest head SHA and every actionable finding closed - and then **wait for the maintainer's explicit permission to merge**. The agent does not merge on its own (consistent with "default to staging"; merging is maintainer-authorized).

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

## Maintainer Setup (GitHub)

- **Secrets**: `NUGET_API_KEY` (NuGet.org push); `CODEGEN_APP_CLIENT_ID` + `CODEGEN_APP_PRIVATE_KEY` for the merge-bot's GitHub App token — add these to **both** the Actions and Dependabot secret stores.
- **Repository variable**: `PUBLISH_ON_MERGE` — leave unset for the two-phase model; set to `true` for continuous-release.
- **Rulesets**: `develop` squash-only, `main` merge-only; both require the `Check pull request workflow status` check and signed commits; both omit "Require branches to be up to date before merging".
