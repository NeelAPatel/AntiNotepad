[CmdletBinding()]
param([switch]$SkipSelfTest)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$dataPath = $null

try {
    Write-Host 'Checking Windows and PowerShell 7...'
    if (-not [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
        throw 'Windows is required.'
    }
    if ($PSVersionTable.PSVersion.Major -lt 7) { throw 'PowerShell 7 or later is required. Install it from https://aka.ms/powershell.' }

    Write-Host 'Checking the .NET SDK...'
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw '.NET 10 SDK was not found. Install it from https://dotnet.microsoft.com/download/dotnet/10.0.'
    }
    Push-Location $repoRoot
    try {
        $version = & dotnet --version
        if ($LASTEXITCODE -ne 0 -or "$version" -notmatch '^10\.\d+\.\d+') {
            throw "The selected .NET SDK must be 10.x (found: $version). Install it from https://dotnet.microsoft.com/download/dotnet/10.0."
        }
        Write-Host "Using .NET SDK $version."
        $env:AVALONIA_TELEMETRY_OPTOUT = '1'
        Write-Host 'Restoring dependencies...'
        & dotnet restore product/AntiNotepad
        if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE." }
        Write-Host 'Building Debug...'
        & dotnet build product/AntiNotepad -c Debug -o build/setup-check --no-restore
        if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE." }

        if ($SkipSelfTest) {
            Write-Host 'Self-test skipped; the build has not been verified by self-test.'
        }
        else {
            Write-Host 'Running self-test with disposable data...'
            $dataPath = Join-Path ([IO.Path]::GetTempPath()) ('antinotepad-setup-' + [guid]::NewGuid().ToString('N'))
            [void](New-Item -ItemType Directory -Path $dataPath)
            $env:ANTINOTEPAD_DATA = $dataPath
            # The app is a Windows GUI program; piping its output makes PowerShell wait for it to finish.
            $output = @(& (Join-Path $repoRoot 'build/setup-check/AntiNotepad.exe') --selftest | Out-String -Stream)
            $testExit = $LASTEXITCODE
            $output | ForEach-Object { Write-Host $_ }
            if ($testExit -ne 0 -or -not ($output | Where-Object { $_ -match 'SELFTEST PASS' })) {
                throw "Self-test failed or did not print SELFTEST PASS (exit code $testExit)."
            }
        }
        Write-Host 'Next: run pwsh -NoProfile -File tools/dev.ps1 for an isolated dev copy.'
        Write-Host 'See CONTRIBUTING.md for build and contribution details.'
    }
    finally { Pop-Location }
}
catch {
    [Console]::Error.WriteLine("Setup failed: $($_.Exception.Message)")
    exit 1
}
finally {
    if ($dataPath -and (Test-Path -LiteralPath $dataPath)) {
        Remove-Item -LiteralPath $dataPath -Recurse -Force -ErrorAction SilentlyContinue
    }
}
