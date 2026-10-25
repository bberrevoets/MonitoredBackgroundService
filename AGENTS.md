# AGENTS.md

Instructions for AI coding agents (Claude Code, Codex and others) working in this repository. `CLAUDE.md` only imports
this file, so edit this one. [README.md](README.md) describes the project for people; this file lists what the code
alone does not tell you.

<!-- repo-init:start -->
## Repository Info

- **Company:** Berrevoets Systems
- **Author:** Bert Berrevoets
- **Contact:** <bert@berrevoets.net>
- **Remote:** <https://github.com/bberrevoets/MonitoredBackgroundService> (public)

## Workflow Rules

- **GitHub Flow:** `main` is the default and only long-lived branch. It is protected by the ruleset "Protect main":
  no direct pushes, no deletion, changes only through a pull request, and all PR conversations must be resolved
  before merging. Merged head branches are deleted automatically.
- After cloning, run `git config pull.rebase true` once, so `git pull` rebases instead of creating merge commits.
- Work on a branch per topic, push it, and open a PR to `main` only when Bert asks for it, assigned to him
  (`gh pr create --assignee bberrevoets`). PRs are squash-merged. After a merge: switch to `main` and pull, then delete the
  local branch with `git branch -D <branch>` (`-d` refuses after a squash merge) once
  `gh pr view <branch> --json state,headRefOid` shows `MERGED` and the same commit as `git rev-parse <branch>`.
- **"Update the documentation"** means `AGENTS.md`, the project's own `README.md` files (not vendored third-party ones)
  and `CHANGELOG.md`; then commit and push.
- [CHANGELOG.md](CHANGELOG.md) follows Keep a Changelog: the Attributors list at the top, and one `Author: *Name*`
  line under each section heading (`### Added`, `### Changed`, ...).
- Markdown must pass `markdownlint-cli2 "**/*.md"` with [.markdownlint.json](.markdownlint.json) (180-character
  lines) before every commit.
- Binaries go through Git LFS: every `filter=lfs` pattern in `.gitattributes` — the standard set (PDF, ZIP, PNG,
  JPEG, STEP/STP, STL, 3MF, F3D/F3Z) plus any pattern this repository adds. Add a pattern there before committing
  another binary type (GIF, BMP, ...). The patterns are lowercase and match `X.PNG` only where `core.ignorecase` is
  true (Windows, macOS); on a case-sensitive system check a new binary with `git check-attr filter -- <file>` before
  committing it.
- Commit messages use the conventional style (`feat:`, `fix:`, `docs:`, `chore:`).
<!-- repo-init:end -->

## Project Overview

`Berrevoets.MonitoredBackgroundService` is a small .NET library, published on NuGet.org under the same id (latest
release 1.1.1, 2024-10-18). It adds health monitoring to a hosted `BackgroundService`: a worker derived from
`MonitorableBackgroundService` runs `NumberOfTasks` parallel tasks, each task reports progress with
`UpdateTaskExecutionTime`, and `MonitorHealth()` returns `Healthy`, `Degraded` (a task has not reported for more than
half of `MaxTaskDuration`) or `Unhealthy` (a task faulted, was cancelled, or has not reported for `MaxTaskDuration`).
Bert's own project; none of his clones under `D:\Repos` references it (checked 2026-10-25). Dormant since 2025-06-19, and `main`
holds unreleased breaking changes (see Conventions).

## Commands

Needs the .NET 8 SDK or newer (checked with SDK 10.0.402).

```sh
dotnet build HealthTests.sln -c Release      # library + demo host; also writes the .nupkg (GeneratePackageOnBuild)
dotnet run --project HealthTests             # demo: MyWorker with 2 tasks, HealthMonitorWorker logs the status every 60 s
npx markdownlint-cli2 "**/*.md"
```

`dotnet test` (run by CI) finds no tests: `HealthTests` is a demo worker host, not a test project.

## Repository Layout

- `Berrevoets.MonitoredBackgroundService/` — the library (`net8.0`) and everything packed into the NuGet package:
  - `MonitorableBackgroundService.cs` — the abstract base class with `MonitorHealth()`.
  - `ServiceCollectionExtensions.cs` — the two `AddMonitoredWorker<TWorker>` overloads (delegate or `IConfiguration`).
  - `WorkerOptions.cs` — `NumberOfTasks`, `MaxTaskDuration` (section `WorkerOptions` in `appsettings.json`).
  - `Interfaces/` — `IHealthMonitorable` and the `HealthStatus` enum.
  - `readme.md` (the package readme on NuGet.org), `LICENSE.txt` (GPL v3), `assets/icon.png` (the package icon).
- `HealthTests/` — demo worker host (Worker SDK) that uses the library.
- `HealthTests.sln` — solution with both projects.
- `.github/workflows/CI.yml` — build + `dotnet test` and markdown lint on every push and PR to `main`; on a push to
  `main` it also packs and publishes to NuGet.org.

## Conventions

- **Releasing:** bump `<Version>` in the library's `.csproj` in the PR. After the merge, CI pushes the `.nupkg` and
  `.snupkg` to NuGet.org; without a bump the push is skipped (`--skip-duplicate`). The `x.y.z-preview` packages came
  from the old `Development` branch and stopped with the move to GitHub Flow (2026-10-25).
- **Trusted Publishing, no API key:** the `publish` job gets a NuGet API key valid for 1 hour from `NuGet/login`
  (GitHub OIDC, `id-token: write`). It works only through the policy on nuget.org (account `bberrevoets` → Trusted
  Publishing): repository `bberrevoets/MonitoredBackgroundService`, workflow file `CI.yml`, no environment, scope
  "push only new package versions" of `Berrevoets.MonitoredBackgroundService`. Renaming `CI.yml`, moving the push to
  another workflow, adding a GitHub environment or renaming the repository breaks publishing until the policy is
  updated to match. Never add a stored `NUGET_API_KEY` secret again.
- **Unreleased breaking changes:** commit `2a7f41b` (2025-06-19) renamed the namespaces (`Berrevoets`,
  `Berrevoets.Interfaces`, `Berrevoets.Extensions` → `Berrevoets.MonitoredBackgroundService` and
  `Berrevoets.MonitoredBackgroundService.Interfaces`) and the protected members (`_logger` → `Logger`,
  `_stoppingToken` → `StoppingToken`), made `Options` private and `WorkerOptions.MaxTaskDuration` init-only. Version
  1.1.1 on NuGet.org predates it, so the next release needs a new major version (2.0.0).
- **The package readme is documentation for users:** keep its code samples compiling against the current API (they
  were checked on 2026-10-25 by building them against the library).
- **No history rewrites:** the published packages point at their commits through SourceLink. For the same reason
  `assets/icon.png` is a plain git blob on purpose (exception line after the repo-init block in `.gitattributes`;
  the CI checkout does not fetch LFS files either).
- **Known issues, not fixed yet:**
  - `_tasks` (`List<Task>`) and `_taskLastExecution` (`Dictionary`) are shared between the worker tasks and
    `MonitorHealth()` without locking.
  - The XML doc example of `AddMonitoredWorker(Action<WorkerOptions>)` assigns the init-only `MaxTaskDuration`, which
    does not compile (CS8852); configuration binding does set it.
  - The `.csproj` has `<LicenseExpression>GNU GPLv3</LicenseExpression>`, which is not an MSBuild/NuGet property; the
    package licence comes from `PackageLicenseFile`.
  - `LICENSE.txt` lives only in the project folder, so GitHub shows no licence for the repository.
  - Targets `net8.0`, whose support ends on 2026-11-10; the build and publish jobs still use `actions/checkout@v3`
    and `actions/setup-dotnet@v3`.
