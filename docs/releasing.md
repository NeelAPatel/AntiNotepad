# Releasing

Releases require the owner's go. Set the version in `product/AntiNotepad/AppInfo.props`, merge the change through the normal process, and fast-forward `main` according to `docs/dev-loop.md`. Prepare `.github/release-notes-template.md` with the release's highlights and fixes before tagging. From the release commit on `main`, create and push an annotated stable tag, for example:

```powershell
git tag -a vX.Y.Z <main-commit> -m "vX.Y.Z"
git push origin main vX.Y.Z
```

A new `v*.*.*` tag push starts the Windows release workflow. It checks out the tagged commit, installs the .NET 10 SDK, packages a self-contained Windows x64 zip, runs the staged executable's self-test with isolated data, and creates a GitHub Release with the zip and `SHA256SUMS.txt`. The tag version must match the stamped version. The zip is unsigned; GitHub Packages is not used. Review the resulting release and assets.

To dry-run locally with the .NET 10 SDK, from the repository root:

```powershell
pwsh -NoProfile -File tools/package-release.ps1 -OutDir build/release-check
$env:ANTINOTEPAD_DATA = Join-Path $env:TEMP ('antinotepad-selftest-' + [guid]::NewGuid().ToString('N'))
& '<path printed as Staged build>\AntiNotepad.exe' --selftest
```

Each run leaves a uniquely named staging directory in the output directory. The zip and checksum for the same version are replaced on a repeat run. Use a disposable output location, never an app or data folder. Check for `SELFTEST PASS` and exit code zero.

To verify a downloaded zip in PowerShell, run the following in its folder and compare the hash with the matching line in `SHA256SUMS.txt`:

```powershell
Get-FileHash .\AntiNotepad-X.Y.Z-win-x64.zip -Algorithm SHA256
Get-Content .\SHA256SUMS.txt
```
