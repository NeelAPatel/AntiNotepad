[CmdletBinding()]
param(
    [switch]$Fresh,
    [switch]$SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$script:devSlotColors = @('Blue', 'Red', 'Green', 'Purple', 'Orange', 'Silver')

function Resolve-DevSlotColor {
    param([AllowNull()][string]$Color)
    if ([string]::IsNullOrWhiteSpace($Color)) { return $null }
    foreach ($candidate in $script:devSlotColors) {
        if ([string]::Equals($candidate, $Color.Trim(), [StringComparison]::OrdinalIgnoreCase)) {
            return $candidate
        }
    }
    return $null
}

function Get-DevSlotHotkeys {
    param([Parameter(Mandatory)][string]$Color)
    $canonical = Resolve-DevSlotColor $Color
    if ($null -eq $canonical) { throw "Unknown dev slot color '$Color'." }
    $index = [Array]::IndexOf($script:devSlotColors, $canonical) + 1
    return [pscustomobject]@{
        Color = $canonical
        Number = $index
        ShowHide = "Ctrl+Alt+Shift+$index"
        NewNote = "Ctrl+Alt+Shift+F$index"
    }
}

function Get-ObjectStemForId {
    param(
        [Parameter(Mandatory)][string]$ApplicationId,
        [Parameter(Mandatory)][string]$SidHash
    )
    return "Local\$ApplicationId-$SidHash"
}

function Get-DevSlotObjectStem {
    param(
        [Parameter(Mandatory)][string]$Color,
        [Parameter(Mandatory)][string]$SidHash
    )
    $canonical = Resolve-DevSlotColor $Color
    if ($null -eq $canonical) { throw "Unknown dev slot color '$Color'." }
    return Get-ObjectStemForId -ApplicationId "AntiNotepad-dev-$($canonical.ToLowerInvariant())" -SidHash $SidHash
}

function Resolve-DevSlotAllocation {
    <# Pure allocator: no process, mutex, git, or filesystem access. #>
    param(
        [Parameter(Mandatory)][string]$WorktreePath,
        [Parameter(Mandatory)][System.Collections.IDictionary]$Remembered,
        [Parameter(Mandatory)][System.Collections.IDictionary]$Held,
        [Parameter(Mandatory)][string[]]$LiveWorktrees
    )

    $current = [System.IO.Path]::GetFullPath($WorktreePath)
    $live = @{}
    foreach ($path in $LiveWorktrees) {
        if (-not [string]::IsNullOrWhiteSpace($path)) {
            $live[[System.IO.Path]::GetFullPath($path)] = $true
        }
    }
    $pruned = @{}
    foreach ($entry in $Remembered.GetEnumerator()) {
        try { $path = [System.IO.Path]::GetFullPath([string]$entry.Key) }
        catch { continue }
        $color = Resolve-DevSlotColor ([string]$entry.Value)
        if ($live.ContainsKey($path) -and $null -ne $color) { $pruned[$path] = $color }
    }

    $selected = $null
    foreach ($color in $script:devSlotColors) {
        if ($Held.Contains($color) -and
            [string]::Equals([string]$Held[$color], $current, [StringComparison]::OrdinalIgnoreCase)) {
            $selected = $color
            break
        }
    }
    if ($null -eq $selected -and $pruned.ContainsKey($current)) {
        $rememberedColor = [string]$pruned[$current]
        if (-not $Held.Contains($rememberedColor)) { $selected = $rememberedColor }
    }
    if ($null -eq $selected) {
        foreach ($color in $script:devSlotColors) {
            if (-not $Held.Contains($color)) { $selected = $color; break }
        }
    }
    if ($null -ne $selected) { $pruned[$current] = $selected }
    return [pscustomobject]@{ Color = $selected; Remembered = $pruned }
}

function Invoke-DevSlotsSelfTest {
    function Assert-SelfTest([bool]$Condition, [string]$Name) {
        if (-not $Condition) { throw "Dev slot selftest failed: $Name" }
    }

    $root = [System.IO.Path]::GetFullPath((Join-Path ([System.IO.Path]::GetTempPath()) 'T17-selftest-root'))
    $other = [System.IO.Path]::GetFullPath((Join-Path ([System.IO.Path]::GetTempPath()) 'T17-selftest-other'))
    $live = @($root, $other)
    $empty = @{}
    $first = Resolve-DevSlotAllocation $root $empty $empty $live
    Assert-SelfTest ($first.Color -eq 'Blue') 'allocation order starts with Blue'
    $heldOwn = @{ Red = $root }
    $own = Resolve-DevSlotAllocation $root @{ $root = 'Blue' } $heldOwn $live
    Assert-SelfTest ($own.Color -eq 'Red') 'an already held own slot wins over the remembered color'
    $remembered = Resolve-DevSlotAllocation $root @{ $root = 'Purple' } @{ Blue = $other } $live
    Assert-SelfTest ($remembered.Color -eq 'Purple') 'a vacant remembered color is reused'
    $nextFree = Resolve-DevSlotAllocation $root $empty @{ Blue = $other; Green = 'unknown' } $live
    Assert-SelfTest ($nextFree.Color -eq 'Red') 'first vacant color follows slot order'
    $allHeld = @{}
    foreach ($color in $script:devSlotColors) { $allHeld[$color] = 'unknown' }
    $capped = Resolve-DevSlotAllocation $root $empty $allHeld $live
    Assert-SelfTest ($null -eq $capped.Color) 'a full slot set refuses allocation'
    $removed = Resolve-DevSlotAllocation $root @{ $other = 'Orange' } $empty @($root)
    Assert-SelfTest ($removed.Color -eq 'Blue' -and -not $removed.Remembered.ContainsKey($other)) 'removed worktrees are pruned'

    $fixtureHash = '0123456789ABCDEF0123'
    $installedStem = Get-ObjectStemForId 'AntiNotepad' $fixtureHash
    $legacyStem = Get-ObjectStemForId 'AntiNotepad-dev' $fixtureHash
    foreach ($color in $script:devSlotColors) {
        $slotStem = Get-DevSlotObjectStem $color $fixtureHash
        $keys = Get-DevSlotHotkeys $color
        Assert-SelfTest ($slotStem -ne $installedStem -and $slotStem -ne $legacyStem) "$color stem stays distinct from installed and legacy ids"
        $index = [Array]::IndexOf($script:devSlotColors, $color) + 1
        Assert-SelfTest ($keys.ShowHide -eq "Ctrl+Alt+Shift+$index" -and $keys.NewNote -eq "Ctrl+Alt+Shift+F$index") "$color hotkeys"
    }
    [Console]::Out.WriteLine('DEV-SLOTS SELFTEST PASS')
}

if ($SelfTest) {
    try { Invoke-DevSlotsSelfTest; exit 0 }
    catch { [Console]::Error.WriteLine($_.Exception.Message); exit 1 }
}

$buildRoot = [System.IO.Path]::GetFullPath((Join-Path $repoRoot 'build'))
$devBuildPath = [System.IO.Path]::GetFullPath((Join-Path $buildRoot 'dev'))
$testDataPath = [System.IO.Path]::GetFullPath((Join-Path $buildRoot 'testdata'))
$shotsPath = [System.IO.Path]::GetFullPath((Join-Path $buildRoot 'shots'))
$pidFilePath = $null
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
    param([switch]$Legacy)
    $path = if ($Legacy) { [System.IO.Path]::GetFullPath((Join-Path $buildRoot 'dev.pid')) } else { $pidFilePath }
    Assert-NotReparsePoint -LiteralPath $path -Description 'Dev PID file'
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Force
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
function Get-CurrentSidHash {
    $sid = [System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value
    $hash = [System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($sid))
    return [Convert]::ToHexString($hash).Substring(0, 20)
}

# Retain the pre-slot stem for the one shared launcher transaction lock and legacy handoff.
function Get-DevObjectStem {
    return Get-ObjectStemForId -ApplicationId 'AntiNotepad-dev' -SidHash $script:sidHash
}

function Test-InstanceVacant {
    param([Parameter(Mandatory)][string]$ObjectStem)
    $probe = [System.Threading.Mutex]::new($false, "$ObjectStem-Mutex")
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

function Get-InstanceOwner {
    param(
        [Parameter(Mandatory)][string]$ObjectStem,
        [switch]$AllowUnpublished
    )
    $map = [System.IO.MemoryMappedFiles.MemoryMappedFile]::OpenExisting(
        "$ObjectStem-Owner", [System.IO.MemoryMappedFiles.MemoryMappedFileRights]::Read)
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

function Get-VerifiedDevRootFromImage {
    param([Parameter(Mandatory)][string]$ExePath)
    $fullExePath = [System.IO.Path]::GetFullPath($ExePath)
    $installedRoot = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'Programs/AntiNotepad'
    $installedPrefix = [System.IO.Path]::GetFullPath($installedRoot).TrimEnd('\') + '\'
    if ($fullExePath.StartsWith($installedPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Refusing an installed AntiNotepad image.'
    }

    $candidateRoot = [System.IO.Path]::GetDirectoryName([System.IO.Path]::GetDirectoryName(
        [System.IO.Path]::GetDirectoryName($fullExePath)))
    if ([string]::IsNullOrWhiteSpace($candidateRoot)) { throw 'Dev owner image has no checkout root.' }
    $expectedImage = [System.IO.Path]::GetFullPath((Join-Path $candidateRoot 'build/dev/AntiNotepad.exe'))
    $candidateParent = [System.IO.Path]::GetDirectoryName($candidateRoot)
    $worktreesRoot = [System.IO.Path]::GetFullPath((Join-Path $script:mainRoot '.claude/worktrees'))
    # Item worktrees sit at .claude/worktrees/<prefix>/<ID-slug>; a direct child is accepted too.
    $allowedRoot = [string]::Equals($candidateRoot, $script:mainRoot, [StringComparison]::OrdinalIgnoreCase) -or
        [string]::Equals($candidateParent, $worktreesRoot, [StringComparison]::OrdinalIgnoreCase) -or
        [string]::Equals([System.IO.Path]::GetDirectoryName($candidateParent), $worktreesRoot, [StringComparison]::OrdinalIgnoreCase)
    Assert-SafeAncestors $fullExePath
    if (-not $allowedRoot -or
        -not [string]::Equals($fullExePath, $expectedImage, [StringComparison]::OrdinalIgnoreCase) -or
        -not [string]::Equals((Get-CommonGitDirectory $candidateRoot), $script:commonGitDirectory, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Unverified dev image at $fullExePath."
    }
    return $candidateRoot
}

function Get-InstanceStatus {
    param([Parameter(Mandatory)][string]$ObjectStem)
    if (Test-InstanceVacant $ObjectStem) {
        return [pscustomobject]@{ Occupied = $false; Verified = $false; Pid = $null; Root = $null; ExePath = $null; StartTimeUtcTicks = $null }
    }

    $ownerPid = $null
    $resident = $null
    try {
        $ownerPid = Get-InstanceOwner $ObjectStem
        if ($ownerPid -le 0) { throw 'Owner PID is invalid.' }
        $resident = [System.Diagnostics.Process]::GetProcessById($ownerPid)
        $null = $resident.Handle
        $identity = Get-ProcessIdentity $resident
        if ($null -eq $identity) { throw 'Owner exited during identity check.' }
        $root = Get-VerifiedDevRootFromImage $identity.ExePath
        if ((Test-InstanceVacant $ObjectStem) -or (Get-InstanceOwner $ObjectStem) -ne $ownerPid) {
            throw 'Singleton ownership changed during identity check.'
        }
        return [pscustomobject]@{
            Occupied = $true; Verified = $true; Pid = $ownerPid; Root = $root
            ExePath = $identity.ExePath; StartTimeUtcTicks = $identity.StartTimeUtcTicks
        }
    }
    catch {
        return [pscustomobject]@{ Occupied = $true; Verified = $false; Pid = $ownerPid; Root = 'unknown'; ExePath = $null; StartTimeUtcTicks = $null }
    }
    finally {
        if ($null -ne $resident) { $resident.Dispose() }
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

function Get-LiveWorktreePaths {
    param([Parameter(Mandatory)][string]$Checkout)
    $lines = @(& git -C $Checkout worktree list --porcelain)
    if ($LASTEXITCODE -ne 0) { throw "Cannot list live worktrees for $Checkout" }
    $paths = @()
    foreach ($line in $lines) {
        if ($line -match '^worktree (.+)$') { $paths += [System.IO.Path]::GetFullPath($Matches[1]) }
    }
    if (-not ($paths | Where-Object { [string]::Equals($_, $repoRoot, [StringComparison]::OrdinalIgnoreCase) })) {
        throw "Current checkout is missing from git worktree list: $repoRoot"
    }
    return ,$paths
}

function Read-DevSlotRegistry {
    param([Parameter(Mandatory)][string]$RegistryPath)
    Assert-SafeAncestors $RegistryPath
    $remembered = @{}
    $item = Get-ItemIfPresent $RegistryPath
    if ($null -eq $item) { return $remembered }
    if ($item.PSIsContainer) { throw "Dev slot registry must be a file: $RegistryPath" }
    Assert-NotReparsePoint -LiteralPath $RegistryPath -Description 'Dev slot registry'
    $parsed = [System.IO.File]::ReadAllText($RegistryPath) | ConvertFrom-Json -AsHashtable
    if ($parsed -isnot [System.Collections.IDictionary] -or -not $parsed.Contains('worktrees') -or
        $parsed['worktrees'] -isnot [System.Collections.IDictionary]) {
        throw "Dev slot registry has an unsupported format: $RegistryPath"
    }
    foreach ($entry in $parsed['worktrees'].GetEnumerator()) {
        try { $path = [System.IO.Path]::GetFullPath([string]$entry.Key) }
        catch { throw "Dev slot registry contains an invalid worktree path: $($entry.Key)" }
        $color = Resolve-DevSlotColor ([string]$entry.Value)
        if ($null -ne $color) { $remembered[$path] = $color }
    }
    return $remembered
}

function Write-DevSlotRegistry {
    param(
        [Parameter(Mandatory)][string]$RegistryPath,
        [Parameter(Mandatory)][System.Collections.IDictionary]$Remembered
    )
    $directory = [System.IO.Path]::GetDirectoryName($RegistryPath)
    Ensure-Directory $directory
    Assert-SafeAncestors $RegistryPath
    Assert-NotReparsePoint -LiteralPath $RegistryPath -Description 'Dev slot registry'
    $worktrees = [ordered]@{}
    foreach ($path in @($Remembered.Keys | Sort-Object)) { $worktrees[$path] = [string]$Remembered[$path] }
    $record = [ordered]@{ version = 1; worktrees = $worktrees }
    $temporaryPath = "$RegistryPath.$([guid]::NewGuid().ToString('N')).tmp"
    try {
        [System.IO.File]::WriteAllText(
            $temporaryPath,
            (($record | ConvertTo-Json -Depth 4) + [Environment]::NewLine),
            [System.Text.UTF8Encoding]::new($false))
        [System.IO.File]::Move($temporaryPath, $RegistryPath, $true)
    }
    finally {
        if (Test-Path -LiteralPath $temporaryPath) { Remove-Item -LiteralPath $temporaryPath -Force }
    }
}

function Get-DevSlotStates {
    $held = @{}
    $states = @{}
    foreach ($color in $script:devSlotColors) {
        $objectStem = Get-DevSlotObjectStem $color $script:sidHash
        $status = Get-InstanceStatus $objectStem
        $states[$color] = $status
        if ($status.Occupied) {
            $held[$color] = if ($status.Verified) { [string]$status.Root } else { 'unknown' }
        }
    }
    return [pscustomobject]@{ Held = $held; States = $states }
}

function Format-OccupiedDevSlots {
    param([Parameter(Mandatory)][System.Collections.IDictionary]$States)
    $details = @()
    foreach ($color in $script:devSlotColors) {
        $status = $States[$color]
        if ($null -ne $status -and $status.Occupied) {
            $holder = if ($status.Verified) { $status.Root } else { 'unknown (owner could not be verified)' }
            $details += "${color}: $holder"
        }
    }
    return $details -join '; '
}

function Stop-DevOwner {
    param(
        [Parameter(Mandatory)][string]$ObjectStem,
        [Parameter(Mandatory)][string]$ExpectedRoot
    )
    # Local records (including legacy/stale records) are bookkeeping only, never authority.
    $status = Get-InstanceStatus $ObjectStem
    if (-not $status.Occupied) { return }
    if (-not $status.Verified -or
        -not [string]::Equals($status.Root, $ExpectedRoot, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to force-close unverified or foreign dev owner PID $($status.Pid)."
    }
    $ownerPid = [int]$status.Pid
    $resident = [System.Diagnostics.Process]::GetProcessById($ownerPid)
    try {
        # Pin the process handle before inspecting identity; never reacquire by PID to kill.
        $null = $resident.Handle
        $identity = Get-ProcessIdentity $resident
        if ($null -eq $identity) { throw 'Dev owner exited during verification; retry the launcher.' }
        $verifiedRoot = Get-VerifiedDevRootFromImage $identity.ExePath
        if (-not [string]::Equals($verifiedRoot, $ExpectedRoot, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Refusing to stop a dev owner from another worktree: $($identity.ExePath)"
        }
        $again = Get-ProcessIdentity $resident
        if ($null -eq $again -or $again.StartTimeUtcTicks -ne $identity.StartTimeUtcTicks -or
            (Test-InstanceVacant $ObjectStem) -or (Get-InstanceOwner $ObjectStem) -ne $ownerPid) {
            throw 'Dev singleton ownership changed during verification; retry the launcher.'
        }
        Write-Warning "Force-closing verified dev owner PID $ownerPid ($($identity.ExePath)); unsaved dev edits may be lost."
        $resident.Kill()
        if (-not $resident.WaitForExit(5000)) { throw 'Verified dev owner did not exit within 5 seconds.' }
    }
    finally { $resident.Dispose() }
}

function New-DevStartInfo {
    param(
        [Parameter(Mandatory)][string]$Image,
        [AllowNull()][string]$SlotColor
    )
    if ([string]::IsNullOrWhiteSpace($testDataPath) -or -not [System.IO.Path]::IsPathFullyQualified($testDataPath)) {
        throw 'Dev DATA must be an absolute, nonempty path before starting any app process.'
    }
    $hotkeys = if ([string]::IsNullOrWhiteSpace($SlotColor)) { $null } else { Get-DevSlotHotkeys $SlotColor }
    $info = [System.Diagnostics.ProcessStartInfo]::new()
    $info.FileName = $Image
    $info.WorkingDirectory = [System.IO.Path]::GetDirectoryName($Image)
    $info.UseShellExecute = $false
    $info.Environment['ANTINOTEPAD_DEV'] = '1'
    $info.Environment['ANTINOTEPAD_DATA'] = $testDataPath
    if ($null -eq $hotkeys) {
        $info.Environment.Remove('ANTINOTEPAD_SLOT')
        $info.Environment['ANTINOTEPAD_HOTKEY'] = 'Ctrl+Alt+Shift+D'
        $info.Environment['ANTINOTEPAD_NEWNOTE_HOTKEY'] = 'Ctrl+Alt+Shift+N'
    }
    else {
        $info.Environment['ANTINOTEPAD_SLOT'] = $hotkeys.Color
        $info.Environment['ANTINOTEPAD_HOTKEY'] = $hotkeys.ShowHide
        $info.Environment['ANTINOTEPAD_NEWNOTE_HOTKEY'] = $hotkeys.NewNote
    }
    $info.Environment['ANTINOTEPAD_SHOTS'] = $shotsPath
    return $info
}

function Request-InstanceQuit {
    param(
        [Parameter(Mandatory)][string]$ObjectStem,
        [Parameter(Mandatory)][string]$ExpectedRoot,
        [AllowNull()][string]$SlotColor
    )
    # After fallback termination, require a fresh successful quit/vacancy round too.
    for ($attempt = 0; $attempt -lt 2; $attempt++) {
        $status = Get-InstanceStatus $ObjectStem
        if (-not $status.Occupied -and $attempt -eq 0) { return }
        if ($status.Occupied -and (-not $status.Verified -or
            -not [string]::Equals($status.Root, $ExpectedRoot, [StringComparison]::OrdinalIgnoreCase))) {
            throw "Refusing to request quit for unverified or foreign dev owner PID $($status.Pid)."
        }
        $info = New-DevStartInfo (Join-Path $helperBuildPath 'AntiNotepad.exe') $SlotColor
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
            if (-not (Test-InstanceVacant $ObjectStem)) { throw 'Quit returned success but the selected dev singleton is occupied; retry the launcher.' }
            return
        }
        if ($attempt -ne 0) { throw "Dev quit helper still returned $quitExit after force-close; refusing cleanup, build and launch." }
        Write-Warning "Dev quit helper returned $quitExit; checking the dev owner for force-close."
        Stop-DevOwner -ObjectStem $ObjectStem -ExpectedRoot $ExpectedRoot
        if (-not (Test-InstanceVacant $ObjectStem)) { throw 'Dev singleton is still occupied; refusing cleanup, build and launch.' }
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
        "-p:AntiNotepadBranch=$(& git -C $repoRoot branch --show-current)",
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
        slot = $script:selectedSlotColor
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

    $startInfo = New-DevStartInfo $exePath $script:selectedSlotColor

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
            try { $primary = (Get-InstanceOwner $script:selectedObjectStem -AllowUnpublished) -eq $appProcess.Id }
            catch [System.IO.FileNotFoundException] { $primary = $false }
            if ($primary) { break }
            Start-Sleep -Milliseconds 100
        }
        if (-not $primary) { throw 'Launched dev process did not become the published primary within 5 seconds.' }
        Start-Sleep -Milliseconds 750
        $identity = Get-ProcessIdentity $appProcess
        if ($null -eq $identity -or
            -not [string]::Equals($identity.ExePath, $exePath, [StringComparison]::OrdinalIgnoreCase) -or
            (Test-InstanceVacant $script:selectedObjectStem) -or
            (Get-InstanceOwner $script:selectedObjectStem) -ne $appProcess.Id -or $appProcess.HasExited) {
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
    Write-Host "COLOR $($script:selectedSlotColor.ToLowerInvariant()) (id AntiNotepad-dev-$($script:selectedSlotColor.ToLowerInvariant()))"
    Write-Host "HOTKEY $($script:selectedHotkeys.ShowHide)"
    Write-Host "NEWNOTE $($script:selectedHotkeys.NewNote)"
}

$script:sidHash = Get-CurrentSidHash
$legacyObjectStem = Get-DevObjectStem
$launcher = [System.Threading.Mutex]::new($false, "$legacyObjectStem-Launcher")
$launcherOwned = $false
try {
    try { $launcherOwned = $launcher.WaitOne(120000) }
    catch [System.Threading.AbandonedMutexException] { $launcherOwned = $true }
    if (-not $launcherOwned) { throw 'Timed out after 120 seconds waiting for another dev launcher. Retry after it finishes.' }

    $script:commonGitDirectory = Get-CommonGitDirectory $repoRoot
    $script:mainRoot = [System.IO.Path]::GetDirectoryName($script:commonGitDirectory)
    $mainRootCommon = Get-CommonGitDirectory $script:mainRoot
    if (-not [string]::Equals($mainRootCommon, $script:commonGitDirectory, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Git common directory did not resolve to the main checkout: $script:mainRoot"
    }
    $registryPath = [System.IO.Path]::GetFullPath((Join-Path $script:mainRoot 'build/dev-slots.json'))
    $liveWorktrees = Get-LiveWorktreePaths $repoRoot
    $remembered = Read-DevSlotRegistry $registryPath
    $slotSnapshot = Get-DevSlotStates
    $allocation = Resolve-DevSlotAllocation $repoRoot $remembered $slotSnapshot.Held $liveWorktrees
    Write-DevSlotRegistry $registryPath $allocation.Remembered
    if ($null -eq $allocation.Color) {
        $description = Format-OccupiedDevSlots $slotSnapshot.States
        throw "All six dev slots are occupied; refusing to replace another worktree. $description"
    }

    $script:selectedSlotColor = [string]$allocation.Color
    $script:selectedHotkeys = Get-DevSlotHotkeys $script:selectedSlotColor
    $script:selectedObjectStem = Get-DevSlotObjectStem $script:selectedSlotColor $script:sidHash
    $script:pidFilePath = [System.IO.Path]::GetFullPath((Join-Path $buildRoot "dev-$($script:selectedSlotColor.ToLowerInvariant()).pid"))
    $script:exePath = $exePath
    $selectedStatus = $slotSnapshot.States[$script:selectedSlotColor]
    if ($selectedStatus.Occupied -and
        (-not $selectedStatus.Verified -or -not [string]::Equals($selectedStatus.Root, $repoRoot, [StringComparison]::OrdinalIgnoreCase))) {
        throw "Selected slot $($script:selectedSlotColor) is held by a process this worktree cannot verify; refusing replacement."
    }

    # A pre-slot copy from this checkout is upgraded in place. Legacy copies from other worktrees remain untouched.
    $legacyStatus = Get-InstanceStatus $legacyObjectStem
    $legacyOwnedHere = $legacyStatus.Occupied -and $legacyStatus.Verified -and
        [string]::Equals($legacyStatus.Root, $repoRoot, [StringComparison]::OrdinalIgnoreCase)
    $replaceSelected = $selectedStatus.Occupied
    foreach ($path in @($repoRoot, $buildRoot, $devBuildPath, $helperBuildPath, $testDataPath, $shotsPath, $script:pidFilePath)) {
        Assert-SafeAncestors $path
    }
    Ensure-Directory $buildRoot
    Ensure-Directory $testDataPath
    if ($replaceSelected -or $legacyOwnedHere) {
        Ensure-Directory $helperBuildPath
        Invoke-DevBuild -OutputPath $helperBuildPath
        if ($replaceSelected) {
            Request-InstanceQuit -ObjectStem $script:selectedObjectStem -ExpectedRoot $repoRoot -SlotColor $script:selectedSlotColor
        }
        if ($legacyOwnedHere) {
            Request-InstanceQuit -ObjectStem $legacyObjectStem -ExpectedRoot $repoRoot -SlotColor $null
            Remove-PidFile -Legacy
        }
    }
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
