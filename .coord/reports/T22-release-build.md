# T22-release-build

## Files written

- `tools/package-release.ps1` publishes a self-contained Windows x64 build, stages the license, notices and portable instructions, and produces a zip and SHA-256 manifest. The requested version must match the version stamped in `AppInfo.props`.
- `.github/workflows/release.yml` packages and self-tests on stable-looking tag pushes, then creates a GitHub Release. Checkout and setup-dotnet are pinned to full commit SHAs; the workflow uses the built-in GitHub token with contents write permission.
- `.github/release-notes-template.md` supplies the requested sections.
- `docs/releasing.md` describes tagging, the workflow, a local dry run and checksum verification.
- This report.

## Checks

- `dotnet --list-sdks`: .NET SDK 10.0.404 is present.
- `pwsh -NoProfile -File tools/package-release.ps1 -OutDir build/release-check`: blocked during restore with NU1301 because the sandbox refused the NuGet connection. The still-running restore was stopped after the errors. The output contains an incomplete staging directory; no zip or checksum was produced.
- PowerShell parser check of `tools/package-release.ps1`: passed.
- Search of the new implementation and documentation for home paths and the private repository name: no matches.
- YAML parse was not performed: PyYAML, Ruby and a PowerShell YAML module were unavailable locally. No packages were installed.

## Owner follow-up

- Run the packaging command in an environment where the required NuGet dependencies can be restored. Run the staged executable with `ANTINOTEPAD_DATA` set to a disposable folder and confirm exit zero and `SELFTEST PASS`; inspect the zip and verify its checksum.
- Parse the workflow with a YAML parser and validate the workflow in GitHub Actions. A tag push, release creation, uploaded assets and the pinned action revisions were not exercised here.
- Fill in release-specific highlights and fixes before creating the release tag. No tags or GitHub Releases were created.
