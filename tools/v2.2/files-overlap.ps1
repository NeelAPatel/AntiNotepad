<#
Loop v2.2 (DORMANT, not wired into the gate or any skill; see docs/v2.2/README.md).
Compares the `## Files` list of one item card with the cards of every other item in flight (one worktree per item,
branch feat/<ID-slug>). Exit 0 = no overlap or only hot files (warning); exit 1 = a shared non-hot file.
Usage: pwsh -NoProfile -File tools/v2.2/files-overlap.ps1 -Task <ID-slug>
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Task,
    [string[]]$HotFiles = @(
        'product/AntiNotepad/SelfTest/Contracts.cs',
        'product/AntiNotepad/SelfTest.cs',
        'docs/loop.md'
    )
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-CardFiles([string]$CardPath) {
    if (-not (Test-Path -LiteralPath $CardPath -PathType Leaf)) { return $null }
    $text = [IO.File]::ReadAllText($CardPath)
    $section = [regex]::Match($text, '(?ms)^## Files\s*\r?\n(.*?)(?=^##\s|\z)')
    if (-not $section.Success) { return @() }
    @([regex]::Matches($section.Groups[1].Value, '(?m)^\s*-\s+`([^`]+)`') |
        ForEach-Object { $_.Groups[1].Value.Trim().Replace('\', '/') } | Where-Object { $_.Length -gt 0 })
}

$repoRoot = (& git -C $PSScriptRoot rev-parse --show-toplevel).Trim()
$mine = Get-CardFiles (Join-Path $repoRoot ".coord/tasks/$Task.md")
if ($null -eq $mine) { throw "No card .coord/tasks/$Task.md in this checkout." }

$others = @{}
$path = $null
foreach ($line in (& git -C $repoRoot worktree list --porcelain)) {
    if ($line -like 'worktree *') { $path = $line.Substring(9) }
    elseif ($line -like 'branch refs/heads/feat/*') {
        $slug = $line.Substring('branch refs/heads/feat/'.Length)
        if ($slug -ceq $Task) { continue }
        $files = Get-CardFiles (Join-Path $path ".coord/tasks/$slug.md")
        if ($null -ne $files) { $others[$slug] = $files }
    }
}

$hard = 0; $hot = 0
foreach ($slug in ($others.Keys | Sort-Object)) {
    foreach ($f in $mine) {
        if ($others[$slug] -contains $f) {
            if ($HotFiles -contains $f) { $hot++; Write-Host "WARN hot file shared with ${slug}: $f" }
            else { $hard++; Write-Host "FAIL shared with ${slug}: $f" }
        }
    }
}
if ($hard -gt 0) { Write-Host "FILES-OVERLAP: FAIL ($hard shared file(s), $hot hot)"; exit 1 }
Write-Host "FILES-OVERLAP: PASS ($($others.Count) other item(s) in flight, $hot hot file warning(s))"
