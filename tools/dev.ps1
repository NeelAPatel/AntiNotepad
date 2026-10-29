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
$helperBuildPath = Join-Path $buildRoot 'dev-handoff-helper'
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

# Match SingleInstanceService.ObjectStem: current logon session plus Windows user SID.
function Get-DevObjectStem {
    $sid = [System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value
    $hash = [System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($sid))
    return 'Local\AntiNotepad-dev-' + [Convert]::ToHexString($hash).Substring(0, 20)
}

function Test-DevVacant {
    $probe = [System.Threading.Mutex]::new($false, "$devStem-Mutex")
    $owned = $false
    try {
        try { $owned = $probe.WaitOne(0) }
        catch [System.Threading.AbandonedMutexException] { $owned = $true }
        return $owned
    }
    finally {
        if ($owned) { $probe.ReleaseMutex() }
        $probe.Dispose()
    }
}

function Get-DevOwner {
    param([switch]$AllowUnpublished)
    $map = [System.IO.MemoryMappedFiles.MemoryMappedFile]::OpenExisting(
        "$devStem-Owner", [System.IO.MemoryMappedFiles.MemoryMappedFileRights]::Read)
    try {
        $view = $map.CreateViewAccessor(0, 4, [System.IO.MemoryMappedFiles.MemoryMappedFileAccess]::Read)
        try {
            $ownerPid = $view.ReadInt32(0)
            if ($ownerPid -le 0 -and -not $AllowUnpublished) { throw 'Dev singleton owner has not published a valid PID.' }
            return $ownerPid
        }
        finally { $view.Dispose() }
    }
    finally { $map.Dispose() }
}

function Assert-SafeAncestors {
    param([Parameter(Mandatory)][string]$LiteralPath)
    $cursor = [System.IO.Path]::GetFullPath($LiteralPath)
    while ($cursor) {
        Assert-NotReparsePoint -LiteralPath $cursor -Description 'Dev path ancestor'
        $cursor = [System.IO.Path]::GetDirectoryName($cursor)
    }
}

function Assert-SafeBuildOutput {
    param([Parameter(Mandatory)][string]$LiteralPath)
    Assert-SafeAncestors $LiteralPath
    if (Test-Path -LiteralPath $LiteralPath) {
        # Do not let dotnet overwrite a redirected child file/directory either.
        foreach ($child in @(Get-ChildItem -LiteralPath $LiteralPath -Force -Recurse)) {
            Assert-NotReparsePoint $child.FullName 'Build output entry'
        }
    }
}

function Get-CommonGitDirectory {
    param([Parameter(Mandatory)][string]$Checkout)
    $result = @(& git -C $Checkout rev-parse --path-format=absolute --git-common-dir)
    if ($LASTEXITCODE -ne 0 -or $result.Count -ne 1) { throw "Cannot verify repository at $Checkout" }
    $top = @(& git -C $Checkout rev-parse --show-toplevel)
    if ($LASTEXITCODE -ne 0 -or $top.Count -ne 1 -or
        -not [string]::Equals([System.IO.Path]::GetFullPath($top[0]), $Checkout, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Not an actual repository checkout root: $Checkout"
    }
    return [System.IO.Path]::GetFullPath($result[0])
}

function Stop-DevOwner {
    # Local records (including legacy/stale records) are bookkeeping only, never authority.
    if (Test-DevVacant) { return }
    $ownerPid = Get-DevOwner
    $resident = [System.Diagnostics.Process]::GetProcessById($ownerPid)
    try {
        # Pin the process handle before inspecting identity; never reacquire by PID to kill.
        $null = $resident.Handle
        $identity = Get-ProcessIdentity $resident
        if ($null -eq $identity) { throw 'Dev owner exited during verification; retry the launcher.' }
        $installedRoot = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'Programs/AntiNotepad'
        $installedPrefix = [System.IO.Path]::GetFullPath($installedRoot).TrimEnd('\') + '\'
        if ($identity.ExePath.StartsWith($installedPrefix, [StringComparison]::OrdinalIgnoreCase)) {
            throw 'Refusing to stop an installed AntiNotepad image.'
        }
        $candidateRoot = [System.IO.Path]::GetDirectoryName([System.IO.Path]::GetDirectoryName(
            [System.IO.Path]::GetDirectoryName($identity.ExePath)))
        $expectedImage = [System.IO.Path]::GetFullPath((Join-Path $candidateRoot 'build/dev/AntiNotepad.exe'))
        $common = Get-CommonGitDirectory $repoRoot
        $mainRoot = [System.IO.Path]::GetDirectoryName($common)
        $worktreesRoot = [System.IO.Path]::GetFullPath((Join-Path $mainRoot '.claude/worktrees'))
        $allowedRoot = [string]::Equals($candidateRoot, $mainRoot, [StringComparison]::OrdinalIgnoreCase) -or
            [string]::Equals([System.IO.Path]::GetDirectoryName($candidateRoot), $worktreesRoot, [StringComparison]::OrdinalIgnoreCase)
        Assert-SafeAncestors $identity.ExePath
        if (-not $allowedRoot -or
            -not [string]::Equals($identity.ExePath, $expectedImage, [StringComparison]::OrdinalIgnoreCase) -or
            -not [string]::Equals((Get-CommonGitDirectory $candidateRoot), $common, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Refusing to force-close unverified dev owner PID $ownerPid at $($identity.ExePath)."
        }
        $again = Get-ProcessIdentity $resident
        if ($null -eq $again -or $again.StartTimeUtcTicks -ne $identity.StartTimeUtcTicks -or
            (Test-DevVacant) -or (Get-DevOwner) -ne $ownerPid) {
            throw 'Dev singleton ownership changed during verification; retry the launcher.'
        }
        Write-Warning "Force-closing verified dev owner PID $ownerPid ($($identity.ExePath)); unsaved dev edits may be lost."
        $resident.Kill()
        if (-not $resident.WaitForExit(5000)) { throw 'Verified dev owner did not exit within 5 seconds.' }
    }
    finally { $resident.Dispose() }
}

function New-DevStartInfo {
    param([Parameter(Mandatory)][string]$Image)
    if ([string]::IsNullOrWhiteSpace($testDataPath) -or -not [System.IO.Path]::IsPathFullyQualified($testDataPath)) {
        throw 'Dev DATA must be an absolute, nonempty path before starting any app process.'
    }
    $info = [System.Diagnostics.ProcessStartInfo]::new()
    $info.FileName = $Image
    $info.WorkingDirectory = [System.IO.Path]::GetDirectoryName($Image)
    $info.UseShellExecute = $false
    $info.Environment['ANTINOTEPAD_DEV'] = '1'
    $info.Environment['ANTINOTEPAD_DATA'] = $testDataPath
    $info.Environment['ANTINOTEPAD_HOTKEY'] = 'Ctrl+Alt+Shift+D'
    $info.Environment['ANTINOTEPAD_SHOTS'] = $shotsPath
    return $info
}

function Request-DevQuit {
    # After fallback termination, require a fresh successful quit/vacancy round too.
    for ($attempt = 0; $attempt -lt 2; $attempt++) {
        $info = New-DevStartInfo (Join-Path $helperBuildPath 'AntiNotepad.exe')
        [void]$info.ArgumentList.Add('--quit')
        $helper = [System.Diagnostics.Process]::Start($info)
        if ($null -eq $helper) { throw 'Could not start the dev quit helper.' }
        try {
            if (-not $helper.WaitForExit(8000)) {
                # An unresponsive client could send a delayed Quit to a replacement.
                # Never kill a staging image; only a verified canonical dev owner may be killed.
                throw 'Dev quit helper did not finish within 8 seconds. No replacement launched; wait for the helper to exit before retrying.'
            }
            $quitExit = $helper.ExitCode
        }
        finally { $helper.Dispose() }
        if ($quitExit -eq 0) {
            if (-not (Test-DevVacant)) { throw 'Quit returned success but the dev singleton is occupied; retry the launcher.' }
            return
        }
        if ($attempt -ne 0) { throw "Dev quit helper still returned $quitExit after force-close; refusing cleanup, build and launch." }
        Write-Warning "Dev quit helper returned $quitExit; checking the dev owner for force-close."
        Stop-DevOwner
        if (-not (Test-DevVacant)) { throw 'Dev singleton is still occupied; refusing cleanup, build and launch.' }
    }
}

function Invoke-DevBuild {
    param([Parameter(Mandatory)][string]$OutputPath)
    Assert-SafeBuildOutput $OutputPath
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
        '-p:AntiNotepadDev=true',
        '-o', $OutputPath
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

    $startInfo = New-DevStartInfo $exePath

    $appProcess = [System.Diagnostics.Process]::Start($startInfo)
    if ($null -eq $appProcess) {
        throw "Could not launch $exePath"
    }

    try {
        # Give startup time to publish Owner and expose a secondary that immediately exits.
        $timer = [System.Diagnostics.Stopwatch]::StartNew()
        $primary = $false
        while ($timer.ElapsedMilliseconds -lt 5000 -and -not $appProcess.HasExited) {
            # Do not acquire the product mutex here: that could make the child a secondary.
            try { $primary = (Get-DevOwner -AllowUnpublished) -eq $appProcess.Id }
            catch [System.IO.FileNotFoundException] { $primary = $false }
            if ($primary) { break }
            Start-Sleep -Milliseconds 100
        }
        if (-not $primary) { throw 'Launched dev process did not become the published primary within 5 seconds.' }
        Start-Sleep -Milliseconds 750
        $identity = Get-ProcessIdentity $appProcess
        if ($null -eq $identity -or
            -not [string]::Equals($identity.ExePath, $exePath, [StringComparison]::OrdinalIgnoreCase) -or
            (Test-DevVacant) -or (Get-DevOwner) -ne $appProcess.Id -or $appProcess.HasExited) {
            throw "Launched dev process failed owner/image/survival verification for $exePath"
        }
        Write-PidRecord -Process $appProcess -StartTimeUtcTicks $identity.StartTimeUtcTicks
    }
    catch {
        # Do not kill on failed verification: identity/ownership may be ambiguous.
        throw "Dev launch verification failed; no success record written. Inspect the dev copy before retrying. $($_.Exception.Message)"
    }

    Write-Host "RUNNING pid=$($appProcess.Id)"
    Write-Host "EXE $exePath"
    Write-Host "DATA $testDataPath"
    Write-Host 'HOTKEY Ctrl+Alt+Shift+D'
}

$devStem = Get-DevObjectStem
$launcher = [System.Threading.Mutex]::new($false, "$devStem-Launcher")
$launcherOwned = $false
try {
    try { $launcherOwned = $launcher.WaitOne(120000) }
    catch [System.Threading.AbandonedMutexException] { $launcherOwned = $true }
    if (-not $launcherOwned) { throw 'Timed out after 120 seconds waiting for another dev launcher. Retry after it finishes.' }

    foreach ($path in @($repoRoot, $buildRoot, $devBuildPath, $helperBuildPath, $testDataPath, $shotsPath, $pidFilePath)) {
        Assert-SafeAncestors $path
    }
    Ensure-Directory $buildRoot
    Ensure-Directory $helperBuildPath
    Ensure-Directory $testDataPath
    Invoke-DevBuild -OutputPath $helperBuildPath
    Request-DevQuit
    Remove-PidFile
    if ($Fresh) { Clear-TestData }
    Ensure-Directory $devBuildPath
    Ensure-Directory $testDataPath
    Ensure-Directory $shotsPath
    Invoke-DevBuild -OutputPath $devBuildPath
    Start-DevProcess
}
finally {
    if ($launcherOwned) { $launcher.ReleaseMutex() }
    $launcher.Dispose()
}
