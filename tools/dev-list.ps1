[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$slotColors = @('Blue', 'Red', 'Green', 'Purple', 'Orange', 'Silver')
$sid = [System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value
$sidHash = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($sid))).Substring(0, 20)

function Get-CommonGitDirectory([string]$Checkout) {
    $common = @(& git -C $Checkout rev-parse --path-format=absolute --git-common-dir)
    if ($LASTEXITCODE -ne 0 -or $common.Count -ne 1) { throw "Cannot verify repository at $Checkout" }
    return [IO.Path]::GetFullPath($common[0])
}

function Get-DevRootFromImage([string]$ExePath, [string]$MainRoot, [string]$CommonGitDirectory) {
    $fullPath = [IO.Path]::GetFullPath($ExePath)
    $root = [IO.Path]::GetDirectoryName([IO.Path]::GetDirectoryName([IO.Path]::GetDirectoryName($fullPath)))
    $expected = [IO.Path]::GetFullPath((Join-Path $root 'build/dev/AntiNotepad.exe'))
    $parent = [IO.Path]::GetDirectoryName($root)
    $worktrees = [IO.Path]::GetFullPath((Join-Path $MainRoot '.claude/worktrees'))
    $allowed = [string]::Equals($root, $MainRoot, [StringComparison]::OrdinalIgnoreCase) -or
        [string]::Equals($parent, $worktrees, [StringComparison]::OrdinalIgnoreCase) -or
        [string]::Equals([IO.Path]::GetDirectoryName($parent), $worktrees, [StringComparison]::OrdinalIgnoreCase)
    if (-not $allowed -or -not [string]::Equals($fullPath, $expected, [StringComparison]::OrdinalIgnoreCase) -or
        -not [string]::Equals((Get-CommonGitDirectory $root), $CommonGitDirectory, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Owner image is not a verified AntiNotepad dev checkout.'
    }
    return $root
}

function Read-OwnerPid([string]$ObjectStem) {
    $owner = [System.IO.MemoryMappedFiles.MemoryMappedFile]::OpenExisting(
        "$ObjectStem-Owner", [System.IO.MemoryMappedFiles.MemoryMappedFileRights]::Read)
    try {
        $view = $owner.CreateViewAccessor(0, 4, [System.IO.MemoryMappedFiles.MemoryMappedFileAccess]::Read)
        try { return $view.ReadInt32(0) }
        finally { $view.Dispose() }
    }
    finally { $owner.Dispose() }
}

function Get-SlotRow([string]$Color, [int]$Index, [string]$MainRoot, [string]$CommonGitDirectory) {
    $id = "AntiNotepad-dev-$($Color.ToLowerInvariant())"
    $objectStem = "Local\$id-$script:sidHash"
    $ownerPid = $null
    $worktree = ''
    $state = 'vacant'
    $mutex = $null
    try {
        try { $mutex = [System.Threading.Mutex]::OpenExisting("$objectStem-Mutex") }
        catch [System.Threading.WaitHandleCannotBeOpenedException] {
            return [pscustomobject]@{ Color = $Color; Hotkey = "Ctrl+Alt+Shift+$Index"; NewNote = "Ctrl+Alt+Shift+F$Index"; State = 'vacant'; PID = ''; Worktree = '' }
        }
        $owned = $false
        try { $owned = $mutex.WaitOne(0) }
        catch [System.Threading.AbandonedMutexException] { $owned = $true }
        if ($owned) {
            $mutex.ReleaseMutex()
            return [pscustomobject]@{ Color = $Color; Hotkey = "Ctrl+Alt+Shift+$Index"; NewNote = "Ctrl+Alt+Shift+F$Index"; State = 'vacant'; PID = ''; Worktree = '' }
        }

        $state = 'held'
        try {
            $ownerPid = Read-OwnerPid $objectStem
            if ($ownerPid -le 0) { throw 'Owner PID is not published.' }
            $process = [System.Diagnostics.Process]::GetProcessById($ownerPid)
            try {
                $null = $process.Handle
                $exe = [IO.Path]::GetFullPath($process.MainModule.FileName)
                $worktree = Get-DevRootFromImage $exe $MainRoot $CommonGitDirectory
                if ((Read-OwnerPid $objectStem) -ne $ownerPid) { throw 'Owner PID changed during inspection.' }
            }
            finally { $process.Dispose() }
        }
        catch { $worktree = 'unknown' }
        return [pscustomobject]@{
            Color = $Color; Hotkey = "Ctrl+Alt+Shift+$Index"; NewNote = "Ctrl+Alt+Shift+F$Index"
            State = $state; PID = if ($null -eq $ownerPid -or $ownerPid -le 0) { '' } else { $ownerPid }; Worktree = $worktree
        }
    }
    catch {
        # Inaccessible mutexes are occupied for allocation purposes; report them as unknown.
        return [pscustomobject]@{ Color = $Color; Hotkey = "Ctrl+Alt+Shift+$Index"; NewNote = "Ctrl+Alt+Shift+F$Index"; State = 'held'; PID = ''; Worktree = 'unknown' }
    }
    finally { if ($null -ne $mutex) { $mutex.Dispose() } }
}

$scriptRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$commonGitDirectory = Get-CommonGitDirectory $scriptRoot
$mainRoot = [System.IO.Path]::GetDirectoryName($commonGitDirectory)
$null = Get-CommonGitDirectory $mainRoot
$rows = for ($index = 0; $index -lt $slotColors.Count; $index++) {
    Get-SlotRow $slotColors[$index] ($index + 1) $mainRoot $commonGitDirectory
}
$rows | Format-Table -AutoSize Color, Hotkey, NewNote, State, PID, Worktree
