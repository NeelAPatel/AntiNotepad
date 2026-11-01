<#
Loop v3.1: compares an item's Files with other item checkouts, including the main checkout.
Usage: pwsh -NoProfile -File tools/files-overlap.ps1 -Task <ID-slug>
Exit 0 = no overlap or hot-file warnings only; exit 1 = a shared non-hot file or Git failure.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Task,
    [string[]]$HotFiles = @(
        'product/AntiNotepad/SelfTest/Contracts.cs',
        'product/AntiNotepad/SelfTest/*.cs',
        'docs/loop.md',
        'docs/glossary.md',
        '.claude/skills/*/SKILL.md'
    )
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$prefixes = @('feat', 'fix', 'refactor', 'docs', 'chore')
$branchPattern = '^branch refs/heads/(' + ($prefixes -join '|') + ')/(?<slug>[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+)$'

# Reads only the Files section so other card lists cannot cause false overlaps.
function Get-CardFiles([string]$CardPath) {
    if (-not (Test-Path -LiteralPath $CardPath -PathType Leaf)) { return $null }
    $text = [IO.File]::ReadAllText($CardPath)
    $section = [regex]::Match($text, '(?ms)^## Files\s*\r?\n(.*?)(?=^##\s|\z)')
    if (-not $section.Success) { return @() }
    @([regex]::Matches($section.Groups[1].Value, '(?m)^\s*-\s+`([^`]+)`') |
        ForEach-Object { $_.Groups[1].Value.Trim().Replace('\', '/') } | Where-Object { $_.Length -gt 0 })
}

$repoRoot = & git -C $PSScriptRoot rev-parse --show-toplevel
if ($LASTEXITCODE -ne 0) { Write-Host 'FILES-OVERLAP: FAIL (cannot find repository root)'; exit 1 }
$repoRoot = $repoRoot.Trim()
$mine = Get-CardFiles (Join-Path $repoRoot ".coord/tasks/$Task.md")
if ($null -eq $mine) { throw "No card .coord/tasks/$Task.md in this checkout." }

$others = @{}
$path = $null
$worktrees = @(& git -C $repoRoot worktree list --porcelain)
if ($LASTEXITCODE -ne 0) { Write-Host 'FILES-OVERLAP: FAIL (cannot list worktrees)'; exit 1 }
foreach ($line in $worktrees) {
    if ($line -like 'worktree *') { $path = $line.Substring(9) }
    elseif ($line -cmatch $branchPattern) {
        $slug = $Matches.slug
        if ($slug -ceq $Task) { continue }
        $files = Get-CardFiles (Join-Path $path ".coord/tasks/$slug.md")
        if ($null -ne $files) { $others[$slug] = $files }
    }
}

$hard = 0; $hot = 0
foreach ($slug in ($others.Keys | Sort-Object)) {
    foreach ($f in $mine) {
        if ($others[$slug] -contains $f) {
            if (@($HotFiles | Where-Object { $f -like $_ }).Count -gt 0) { $hot++; Write-Host "WARN hot file shared with ${slug}: $f" }
            else { $hard++; Write-Host "FAIL shared with ${slug}: $f" }
        }
    }
}
if ($hard -gt 0) { Write-Host "FILES-OVERLAP: FAIL ($hard shared file(s), $hot hot)"; exit 1 }
Write-Host "FILES-OVERLAP: PASS ($($others.Count) other item(s) in flight, $hot hot file warning(s))"
exit 0
