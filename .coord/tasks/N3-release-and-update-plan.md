# N3-release-and-update-plan

You are the director (gpt-6.1-sol medium); delegate research or drafting to gpt-6-luna xhigh subagents where useful.
Repo: this folder, branch `feat/N1-antinotepad-identity` (checked out; contains N1 and N2). Never switch branch, merge or commit.
Read `AGENTS.md`, `docs/dev-loop.md`, `docs/decisions.md`, and the N1/N2 reports in `.coord/reports/` first.

## This is a proposal, not an implementation
Write `docs/release-plan.md` for the user to approve. Change no code, scripts or project files. (Lab-first rule: design, then
approval, then build.)

## The user's brief
Design the whole path: dev -> release -> installation / clone -> manual update check (the app checks the project's repo for new
releases). Remove as much friction as possible for three groups:
1. **The developer (the user)**: cutting a release from `dev`/`main` with as few steps as possible (ideally one script), version
   bump in `product/AntiNotepad/AppInfo.props` (the single identity file from N1), changelog, tag, publish.
2. **Other developers** who clone the repo and want to run a dev copy, then send pull requests: what they install (.NET 10 SDK),
   the one command that builds and runs a dev copy (the `tools/dev.ps1` route from N2, dev badge, test data), how they test
   (`--selftest`), branch/PR conventions, what a PR must pass.
3. **End users** who install AntiNotepad as a program and expect upgrades, fixes and change logs: how they install, how they find
   out about updates (a manual "Check for updates" in Settings > About that queries the repo's releases; say what it shows and
   what happens on "update"), how the upgrade keeps their notes in `%LOCALAPPDATA%\AntiNotepad`, how they read the change log.
Both a **portable** build (zip, run anywhere, data next to the exe or in a chosen folder) and a **system install** are wanted;
recommend which to ship first (the easier one) and how the other follows.

## What the plan must cover
- A clear answer to each of the three questions: what the public does to (a) clone and run a dev copy, (b) install the program,
  (c) upgrade it.
- Options compared briefly with a recommendation each: hosting (GitHub Releases is the obvious default; note the repo is
  currently local-only with no remote, so publishing needs the user's go-ahead and an account/repo name), packaging and
  updating (e.g. a plain zip + the N2 install script, Velopack, MSIX, Inno Setup, winget, Scoop; weigh maintenance, signing
  needs, SmartScreen warnings, delta updates, per-user install without admin), the update check (GitHub Releases API, no
  telemetry, manual only, how it compares `AppInfo` versions incl. the pre-release label), changelog format (CHANGELOG.md
  "Keep a Changelog" style + release notes), versioning (semver from 0.5.0 pre-release to 1.0.0), CI (GitHub Actions build +
  selftest on PRs and tag builds) and code signing (what it costs, what happens without it).
- Follow the repo's library rule: first-party first; others only if maintained and documented — state maintenance status.
- A phased rollout: phase 1 the smallest thing that works end to end, then later phases. For each phase, list the files/scripts
  it would add or change and the decisions the user must make.
- Keep it readable: short sections, tables for comparisons, plain language. US spelling.

## Done means
- `docs/release-plan.md` written; nothing else changed except `.coord/STATUS.md` (one line under Now) and the report.
- Short report in `.coord/reports/N3-release-and-update-plan.md`: the recommendation in five bullets and the open decisions.
