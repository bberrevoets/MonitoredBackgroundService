# Berrevoets.MonitoredBackgroundService

A .NET library for hosted background services that run several worker tasks and report their health (`Healthy`,
`Degraded`, `Unhealthy`), published on NuGet.org as
[Berrevoets.MonitoredBackgroundService](https://www.nuget.org/packages/Berrevoets.MonitoredBackgroundService).

[Documentation](https://github.com/bberrevoets/MonitoredBackgroundService/blob/main/Berrevoets.MonitoredBackgroundService/readme.md)

[![Build and Publish Final NuGet Package](https://github.com/bberrevoets/MonitoredBackgroundService/actions/workflows/CI.yml/badge.svg)](https://github.com/bberrevoets/MonitoredBackgroundService/actions/workflows/CI.yml)

## Conventions

- **Branching:** GitHub Flow. `main` is protected and accepts changes only through pull requests.
- **Markdown:** linted with `markdownlint-cli2`, 180 character limit.

  ```sh
  npx markdownlint-cli2 "**/*.md"
  ```

- **Binaries:** tracked with Git LFS; the covered types and any deliberate exceptions are listed in
  [AGENTS.md](AGENTS.md). Run `git lfs install` once per machine before cloning; in a clone made without it, run
  `git lfs install` and then `git lfs pull` to download the files.
- **Agent instructions:** in [AGENTS.md](AGENTS.md), read by Claude Code, Codex and any other coding agent.
  `CLAUDE.md` only imports it, so edit `AGENTS.md`.

## Author

Bert Berrevoets — Berrevoets Systems — <bert@berrevoets.net>
