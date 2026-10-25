# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow the NuGet
package ([Berrevoets.MonitoredBackgroundService](https://www.nuget.org/packages/Berrevoets.MonitoredBackgroundService)).

## Attributors

- Bert Berrevoets (<bert@berrevoets.net>) — Berrevoets Systems
- Claude Opus 5.5 — co-author of the 2026 changes

## [Unreleased]

### Added

Author: *Bert Berrevoets*

- AI agent setup: [AGENTS.md](AGENTS.md) as the single instruction file, a [CLAUDE.md](CLAUDE.md) stub that imports it,
  markdownlint (180 characters), Git LFS rules and this changelog. The package icon stays a plain git blob on purpose
  (see AGENTS.md).
- A markdown lint job in `CI.yml`.
- XML documentation comments on the public API (2025-06-19).

### Changed

Author: *Bert Berrevoets*

- **Breaking** (2025-06-19, not yet on NuGet.org): one namespace for the whole library,
  `Berrevoets.MonitoredBackgroundService` (+ `.Interfaces`), instead of `Berrevoets`, `Berrevoets.Interfaces` and
  `Berrevoets.Extensions`; the protected members `_logger` and `_stoppingToken` are now `Logger` and `StoppingToken`;
  `Options` is private; `WorkerOptions.MaxTaskDuration` is init-only (set it through configuration).
- GitHub Flow (2026-10-25): `main` is the only branch and the default; the `Development` branch and its preview-package
  workflow (`CI-development.yml`, which published `x.y.z-preview` versions) are gone.
- The README describes the library and links the documentation on `main`.
- Publishing uses nuget.org Trusted Publishing (2026-10-25): the `publish` job in `CI.yml` gets a 1-hour API key via
  GitHub OIDC (`NuGet/login`) instead of the stored `NUGET_API_KEY` secret, which had expired.

### Fixed

Author: *Bert Berrevoets*

- The package readme matches the current API: namespaces, `StoppingToken`, the inline-options example without the
  init-only `MaxTaskDuration`, `using YourNamespace;` in the `Program.cs` sample, the GPL v3 licence (it said MIT,
  with a dead link) and the contact address (it was a placeholder).

## [1.1.1] - 2024-10-18

### Added

Author: *Bert Berrevoets*

- Package icon (`assets/icon.png`); the repository README gets its title and the documentation link.

## [1.1.0] - 2024-10-18

### Changed

Author: *Bert Berrevoets*

- Version bump only, after the CI work on preview packages (`<Version>` from the `.csproj` with `-preview` appended);
  the library is unchanged since 1.0.3.

## [1.0.3] - 2024-10-18

### Added

Author: *Bert Berrevoets*

- Symbol packages (`.snupkg`), SourceLink (`Microsoft.SourceLink.GitHub`) and deterministic CI builds.

## [1.0.2] - 2024-10-17

### Added

Author: *Bert Berrevoets*

- `HealthStatus` enum (`Healthy`, `Degraded`, `Unhealthy`): `MonitorHealth()` returns it instead of a `bool`.
- The full package readme (configuration, custom worker, health monitor worker, dependency injection, API reference).
- Preview-package workflow for the `Development` branch.

## [1.0.1] - 2024-10-17

### Added

Author: *Bert Berrevoets*

- GNU GPL v3 licence (`LICENSE.txt`), packed into the package.

## [1.0.0] - 2024-10-17

### Added

Author: *Bert Berrevoets*

- First release: `MonitorableBackgroundService`, `WorkerOptions`, `IHealthMonitorable`, the `AddMonitoredWorker<TWorker>`
  extensions, the `HealthTests` demo host and a CI workflow that publishes to NuGet.org.
