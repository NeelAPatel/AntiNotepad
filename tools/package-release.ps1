[CmdletBinding()]
param([string]$Version, [string]$OutDir = 'build/release')

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
[xml]$identity = Get-Content -LiteralPath (Join-Path $repoRoot 'product/AntiNotepad/AppInfo.props') -Raw
$stampedVersion = [string]$identity.Project.PropertyGroup.AppVersion
$label = [string]$identity.Project.PropertyGroup.AppVersionLabel
if ($label) { $stampedVersion += "-$label" }
if (-not $Version) { $Version = $stampedVersion }
if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw 'Version must be a stable version in the form N.N.N.' }
if ($Version -cne $stampedVersion) { throw "Version '$Version' does not match the stamped version '$stampedVersion'." }

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw '.NET 10 SDK is required; dotnet was not found.' }
$sdkList = & dotnet --list-sdks
if ($LASTEXITCODE -ne 0 -or -not ($sdkList | Where-Object { $_ -match '^10\.' })) { throw '.NET 10 SDK is required; install it before packaging.' }

$env:AVALONIA_TELEMETRY_OPTOUT = '1'
if (-not [IO.Path]::IsPathRooted($OutDir)) { $OutDir = Join-Path $repoRoot $OutDir }
$outPath = [IO.Path]::GetFullPath($OutDir)
New-Item -ItemType Directory -Path $outPath -Force | Out-Null
$stage = Join-Path $outPath ("stage-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage | Out-Null
& dotnet publish (Join-Path $repoRoot 'product/AntiNotepad') -c Release -r win-x64 --self-contained true -o $stage
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE." }
# Debug symbols are most of the download size; the portable zip does not need them.
Get-ChildItem -LiteralPath $stage -Filter '*.pdb' -Recurse | Remove-Item -Force
foreach ($name in @('LICENSE', 'THIRD-PARTY-NOTICES.md')) {
    Copy-Item -LiteralPath (Join-Path $repoRoot $name) -Destination $stage
}
@'
AntiNotepad portable for Windows x64

Extract the entire zip to a folder, then run AntiNotepad.exe. The app is
unsigned, so Windows may show a warning when you run it.

App settings and the notes-folder pointer live in %LOCALAPPDATA%\AntiNotepad.
Notes default to the AntiNotepad folder in your Documents folder; if you chose
a different notes folder in the app, back up that folder instead. Exit the app
before backing up your notes folder and %LOCALAPPDATA%\AntiNotepad.

To uninstall this portable copy, exit the app and delete the extracted folder.
Your notes and app settings remain in their separate locations.
'@ | Set-Content -LiteralPath (Join-Path $stage 'README-portable.txt') -Encoding utf8
$zipName = "AntiNotepad-$Version-win-x64.zip"
$zipPath = Join-Path $outPath $zipName
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zipPath -Force
$hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $zipName" | Set-Content -LiteralPath (Join-Path $outPath 'SHA256SUMS.txt') -Encoding utf8
Write-Host "Staged build: $stage"
Write-Host "Release archive: $zipPath"
