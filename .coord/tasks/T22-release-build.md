# T22-release-build (Codex build in this worktree; do not commit)

Role: terra, a few files with reasoning (PowerShell packager plus one workflow). Sandbox workspace-write.
Inputs: `.coord/research/public-prep/T21-release-kit-report.md` (read sections 3-7 first), `tools/install.ps1`, `tools/uninstall.ps1`, `tools/dev.ps1`, `docs/dev-loop.md`, `product/AntiNotepad/AntiNotepad.csproj`, `.github/workflows/*`.

## Owner decisions (fixed)
- First release is a PORTABLE zip: download, unzip, run `AntiNotepad.exe`. Self-contained win-x64. Unsigned. No installer in this step. GitHub Package stays empty.
- Tags that stay public: `v0.7.0`, `v0.7.1` only. The workflow triggers on new `v*.*.*` tags.
- Never touch the installed copy (`%LOCALAPPDATA%\Programs\AntiNotepad`) or real data. Never run `AntiNotepad.exe --quit` without `ANTINOTEPAD_DATA` set. Do not run the packager against the user's data folders.
- Write ONLY the files named below. Do not edit product code, AGENTS.md, README, `.github/workflows` files that exist, or anything under `.coord/research/`.

## Write
1. `tools/package-release.ps1`: parameters `-Version` (default read from the csproj or the stamped version), `-OutDir` (default `build/release`). Steps: `dotnet publish product/AntiNotepad -c Release -r win-x64 --self-contained true` into a staging folder (no single-file trim unless the report says safe; keep it simple), copy LICENSE, THIRD-PARTY-NOTICES.md, a short `README-portable.txt` (run the .exe, unsigned warning, where data lives, how to back up and uninstall by deleting the folder), zip to `AntiNotepad-<Version>-win-x64.zip`, write `SHA256SUMS.txt`. Fail clearly on missing .NET 10 SDK. Set `AVALONIA_TELEMETRY_OPTOUT=1`.
2. `.github/workflows/release.yml`: on push of tags `v*.*.*`, windows-latest, setup .NET 10, run the packager, run `AntiNotepad.exe --selftest` from the staged build, then create a GitHub Release with the zip and checksums using `gh release create` (or `softprops/action-gh-release` pinned by full commit SHA). Least-privilege `permissions: contents: write`. Release notes from a `.github/release-notes-template.md` (write it; sections Highlights, Fixes, Install, Requirements, Checksums from the report).
3. `docs/releasing.md`: how to cut a release (tag from `main`, what the workflow does, how to verify the checksum, how to dry-run the packager locally).
4. A report at `.coord/reports/T22-release-build.md`: files written, what you ran, what you could not verify.

## Done means (offline)
- `pwsh -NoProfile -File tools/package-release.ps1 -OutDir build/release-check` produces the zip and SHA256SUMS.txt; the staged `AntiNotepad.exe --selftest` prints `SELFTEST PASS` (run with `ANTINOTEPAD_DATA` pointing to a temp folder). If the sandbox blocks a step, say which and why; do not work around it.
- The workflow YAML parses (check with a YAML parse), has no secrets, and pins third-party actions by SHA or avoids them.
- No home paths or private repo names in the new files.
- Never brief or run port, server or browser checks; Codex cannot bind ports here.
- Final message: files written, commands run with results, open items for the owner.
