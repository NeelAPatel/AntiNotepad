[CmdletBinding()]
param(
    [switch]$Fresh
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$buildRoot = [System.IO.Path]::GetFullPath((Join-Path $repoRoot 'build'))
$devBuildPath = [System.IO.Path]::GetFullPath((Join-Path $buildRoot 'dev'))
$testDataPath = [System.IO.Path]::GetFullPath((Join-Path $buildRoot 'testdata'))
$shotsPath = [System.IO.Path]::GetFullPath((Join-Path $buildRoot 'shots'))
$pidFilePath = [System.IO.Path]::GetFullPath((Join-Path $buildRoot 'dev.pid'))
$exePath = [System.IO.Path]::GetFullPath((Join-Path $devBuildPath 'AntiNotepad.exe'))
$expectedTestDataPath = [System.IO.Path]::GetFullPath((Join-Path $buildRoot 'testdata'))

if (-not [System.String]::Equals(
        $testDataPath,
        $expectedTestDataPath,
        [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to use a test data path outside build/testdata: $testDataPath"
}

function Get-ItemIfPresent {
    param([Parameter(Mandatory)][string]$LiteralPath)

    if (Test-Path -LiteralPath $LiteralPath) {
        return Get-Item -LiteralPath $LiteralPath -Force
    }

    return $null
}

function Assert-NotReparsePoint {
    param(
        [Parameter(Mandatory)][string]$LiteralPath,
        [Parameter(Mandatory)][string]$Description
    )

    $item = Get-ItemIfPresent -LiteralPath $LiteralPath
    if ($null -ne $item -and
        (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0)) {
        throw "$Description must not be a symbolic link or reparse point: $LiteralPath"
    }
}

function Ensure-Directory {
    param([Parameter(Mandatory)][string]$LiteralPath)

    $item = Get-ItemIfPresent -LiteralPath $LiteralPath
    if ($null -eq $item) {
        New-Item -ItemType Directory -Path $LiteralPath -Force | Out-Null
        $item = Get-Item -LiteralPath $LiteralPath -Force
    }

    if (-not $item.PSIsContainer) {
        throw "Expected a directory at $LiteralPath"
    }

    Assert-NotReparsePoint -LiteralPath $LiteralPath -Description 'Managed directory'
}

function Remove-TestDataEntry {
    param(
        [Parameter(Mandatory)][string]$LiteralPath,
        [Parameter(Mandatory)][string]$RootPath
    )

    $fullPath = [System.IO.Path]::GetFullPath($LiteralPath)
    $rootPrefix = $RootPath.TrimEnd([System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    if (-not $fullPath.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove a path outside build/testdata: $fullPath"
    }

    $item = Get-Item -LiteralPath $fullPath -Force
    $isReparsePoint = (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0)
    if ($item.PSIsContainer -and -not $isReparsePoint) {
        foreach ($child in @(Get-ChildItem -LiteralPath $fullPath -Force)) {
            Remove-TestDataEntry -LiteralPath $child.FullName -RootPath $RootPath
        }
    }

    # Removing a reparse point removes the link itself; traversal never follows it.
    Remove-Item -LiteralPath $fullPath -Force
}

function Clear-TestData {
    Ensure-Directory -LiteralPath $buildRoot
    Assert-NotReparsePoint -LiteralPath $testDataPath -Description 'Test data directory'

    $dataItem = Get-ItemIfPresent -LiteralPath $testDataPath
    if ($null -ne $dataItem) {
        if (-not $dataItem.PSIsContainer) {
            throw "Test data path is not a directory: $testDataPath"
        }

        foreach ($child in @(Get-ChildItem -LiteralPath $testDataPath -Force)) {
            Remove-TestDataEntry -LiteralPath $child.FullName -RootPath $testDataPath
        }
    }

    Ensure-Directory -LiteralPath $testDataPath
}

function Remove-PidFile {
    Assert-NotReparsePoint -LiteralPath $pidFilePath -Description 'Dev PID file'
    if (Test-Path -LiteralPath $pidFilePath) {
        Remove-Item -LiteralPath $pidFilePath -Force
    }
}

function Get-ProcessIdentity {
    param(
        [Parameter(Mandatory)][System.Diagnostics.Process]$Process,
        [int]$Attempts = 20
    )

    for ($attempt = 0; $attempt -lt $Attempts; $attempt++) {
        if ($Process.HasExited) {
            return $null
        }

        try {
            return [pscustomobject]@{
                StartTimeUtcTicks = $Process.StartTime.ToUniversalTime().Ticks
                ExePath = [System.IO.Path]::GetFullPath($Process.MainModule.FileName)
            }
        }
        catch {
            if ($Process.HasExited) {
                return $null
            }
            if ($attempt -eq ($Attempts - 1)) {
                throw
            }
            Start-Sleep -Milliseconds 100
        }
    }

    return $null
}

function Stop-TrackedDev {
    if (-not (Test-Path -LiteralPath $pidFilePath)) {
        return
    }

    Assert-NotReparsePoint -LiteralPath $pidFilePath -Description 'Dev PID file'
    try {
        $recordText = Get-Content -LiteralPath $pidFilePath -Raw
    }
    catch {
        throw "Cannot read the owned dev PID record at $pidFilePath; leaving every process untouched. $($_.Exception.Message)"
    }

    if ($recordText.Trim() -match '^\d+$') {
        throw "The legacy bare-PID file at $pidFilePath cannot be stopped safely because it has no executable path or start time. Exit the old dev instance manually and remove build/dev.pid once, then retry."
    }

    try {
        $record = $recordText | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        throw "Cannot parse the owned dev PID record at $pidFilePath; leaving every process untouched. $($_.Exception.Message)"
    }

    $targetPid = 0
    if (-not [int]::TryParse([string]$record.pid, [ref]$targetPid) -or $targetPid -le 0) {
        throw "The dev PID record is invalid at $pidFilePath; leaving every process untouched."
    }

    $recordedExe = [string]$record.exePath
    if ([string]::IsNullOrWhiteSpace($recordedExe) -or
        -not [string]::Equals(
            [System.IO.Path]::GetFullPath($recordedExe),
            $exePath,
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "The dev PID record names a different executable; leaving every process untouched."
    }

    $recordedStartTicks = 0L
    if (-not [long]::TryParse([string]$record.startTimeUtcTicks, [ref]$recordedStartTicks) -or
        $recordedStartTicks -le 0) {
        throw "The dev PID record has an invalid start time; leaving every process untouched."
    }

    try {
        $trackedProcess = [System.Diagnostics.Process]::GetProcessById($targetPid)
    }
    catch [System.ArgumentException] {
        Remove-PidFile
        Write-Host "Removed stale dev PID record for exited process $targetPid."
        return
    }

    try {
        $identity = Get-ProcessIdentity -Process $trackedProcess
    }
    catch {
        throw "Cannot verify PID $targetPid against its recorded executable and start time; leaving it untouched. $($_.Exception.Message)"
    }

    if ($null -eq $identity) {
        Remove-PidFile
        Write-Host "Removed stale dev PID record for exited process $targetPid."
        return
    }

    $sameExecutable = [string]::Equals(
        $identity.ExePath,
        $exePath,
        [System.StringComparison]::OrdinalIgnoreCase)
    if (-not $sameExecutable -or $identity.StartTimeUtcTicks -ne $recordedStartTicks) {
        Remove-PidFile
        throw "PID $targetPid no longer matches the recorded dev process. It was left untouched, and this run stopped before data cleanup, build, or launch. Review the running process, then retry."
    }

    try {
        $trackedProcess.Kill()
    }
    catch {
        $alreadyExited = $false
        try { $alreadyExited = $trackedProcess.WaitForExit(0) } catch { }
        if (-not $alreadyExited) {
            throw "Could not stop the verified dev process PID $targetPid. $($_.Exception.Message)"
        }
    }

    if (-not $trackedProcess.WaitForExit(5000)) {
        throw "Verified dev process PID $targetPid did not exit within 5 seconds; its PID record was retained."
    }

    Remove-PidFile
    Write-Host "Stopped the verified dev process PID $targetPid."
}

function Invoke-DevBuild {
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = 'dotnet'
    $startInfo.WorkingDirectory = $repoRoot
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in @(
        'build',
        (Join-Path $repoRoot 'product/AntiNotepad/AntiNotepad.csproj'),
        '-c', 'Debug',
        '-o', $devBuildPath
    )) {
        [void]$startInfo.ArgumentList.Add([string]$argument)
    }
    $startInfo.Environment['AVALONIA_TELEMETRY_OPTOUT'] = '1'

    $buildProcess = [System.Diagnostics.Process]::Start($startInfo)
    if ($null -eq $buildProcess) {
        throw 'Could not start dotnet build.'
    }

    $stdoutTask = $buildProcess.StandardOutput.ReadToEndAsync()
    $stderrTask = $buildProcess.StandardError.ReadToEndAsync()
    $buildProcess.WaitForExit()
    $stdoutText = $stdoutTask.GetAwaiter().GetResult()
    $stderrText = $stderrTask.GetAwaiter().GetResult()
    if (-not [string]::IsNullOrEmpty($stdoutText)) {
        [Console]::Out.Write($stdoutText)
    }
    if (-not [string]::IsNullOrEmpty($stderrText)) {
        [Console]::Error.Write($stderrText)
    }
    if ($buildProcess.ExitCode -ne 0) {
        throw "Debug build failed with exit code $($buildProcess.ExitCode)."
    }
}

function Write-PidRecord {
    param(
        [Parameter(Mandatory)][System.Diagnostics.Process]$Process,
        [Parameter(Mandatory)][long]$StartTimeUtcTicks
    )

    Assert-NotReparsePoint -LiteralPath $pidFilePath -Description 'Dev PID file'
    $record = @{
        pid = $Process.Id
        exePath = $exePath
        startTimeUtcTicks = $StartTimeUtcTicks
    } | ConvertTo-Json -Compress

    $temporaryPath = "$pidFilePath.$([guid]::NewGuid().ToString('N')).tmp"
    try {
        [System.IO.File]::WriteAllText(
            $temporaryPath,
            $record + [Environment]::NewLine,
            [System.Text.UTF8Encoding]::new($false))
        Move-Item -LiteralPath $temporaryPath -Destination $pidFilePath -Force
    }
    finally {
        if (Test-Path -LiteralPath $temporaryPath) {
            Remove-Item -LiteralPath $temporaryPath -Force
        }
    }
}

function Start-DevProcess {
    if (-not (Test-Path -LiteralPath $exePath -PathType Leaf)) {
        throw "The Debug build did not produce $exePath"
    }

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $exePath
    $startInfo.WorkingDirectory = $devBuildPath
    $startInfo.UseShellExecute = $false
    $startInfo.Environment['ANTINOTEPAD_DEV'] = '1'
    $startInfo.Environment['ANTINOTEPAD_DATA'] = $testDataPath
    $startInfo.Environment['ANTINOTEPAD_HOTKEY'] = 'Ctrl+Alt+Shift+D'
    $startInfo.Environment['ANTINOTEPAD_SHOTS'] = $shotsPath

    $appProcess = [System.Diagnostics.Process]::Start($startInfo)
    if ($null -eq $appProcess) {
        throw "Could not launch $exePath"
    }

    try {
        if ($appProcess.HasExited) {
            throw "AntiNotepad exited immediately with code $($appProcess.ExitCode)."
        }

        $identity = Get-ProcessIdentity -Process $appProcess
        if ($null -eq $identity) {
            throw "AntiNotepad exited immediately with code $($appProcess.ExitCode)."
        }

        if (-not [string]::Equals(
                $identity.ExePath,
                $exePath,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "The launched process image did not match $exePath"
        }

        Write-PidRecord -Process $appProcess -StartTimeUtcTicks $identity.StartTimeUtcTicks
    }
    catch {
        $launchFailure = $_
        $hasExited = $false
        try { $hasExited = $appProcess.HasExited } catch { }
        if (-not $hasExited) {
            $cleanupComplete = $false
            $cleanupError = $null
            try {
                $appProcess.Kill()
                $cleanupComplete = $appProcess.WaitForExit(5000)
            }
            catch { $cleanupError = $_.Exception.Message }

            if (-not $cleanupComplete) {
                throw "Dev launch setup failed: $($launchFailure.Exception.Message) The new process could not be confirmed stopped within 5 seconds; no PID record was written. $cleanupError"
            }
        }
        throw $launchFailure
    }

    Write-Host "RUNNING pid=$($appProcess.Id)"
    Write-Host "DATA $testDataPath"
    Write-Host 'HOTKEY Ctrl+Alt+Shift+D'
}

Ensure-Directory -LiteralPath $buildRoot
Ensure-Directory -LiteralPath $devBuildPath
Assert-NotReparsePoint -LiteralPath $shotsPath -Description 'Capture directory'
Stop-TrackedDev

if ($Fresh) {
    Clear-TestData
}

Ensure-Directory -LiteralPath $testDataPath
Ensure-Directory -LiteralPath $shotsPath
Invoke-DevBuild
Start-DevProcess
