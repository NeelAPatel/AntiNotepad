# Compares a backlog scan with current integration history so Findings can read only the relevant delta.
# Read-only; exit 0 on a valid comparison, exit 1 on missing input or Git failure.
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Slug,
    [string]$Base = 'dev'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Stop before interpreting partial output from a failed Git command.
function Invoke-Git([string[]]$Arguments) {
    $result = @(& git --no-optional-locks -C $PSScriptRoot -c core.quotePath=false @Arguments)
    if ($LASTEXITCODE -ne 0) { throw "git $($Arguments -join ' ') failed" }
    $result
}

try {
    if ($Slug -cnotmatch '^[a-z0-9]+(-[a-z0-9]+)*$') { throw 'invalid slug' }
    $root = (Invoke-Git @('rev-parse', '--show-toplevel')) -join ''
    $scan = Join-Path $root ".coord/research/$Slug.md"
    if (-not (Test-Path -LiteralPath $scan -PathType Leaf)) { throw "missing scan: $scan" }
    $text = [IO.File]::ReadAllText($scan)
    $stamp = [regex]::Match($text, '(?m)^Scanned at: ([0-9a-fA-F]{40})[^\S\r\n]*\r?$')
    if (-not $stamp.Success) { throw 'missing full Scanned at sha' }
    $sha = $stamp.Groups[1].Value
    $null = Invoke-Git @('rev-parse', '--verify', '--end-of-options', "$sha^{commit}")
    $tip = (Invoke-Git @('rev-parse', '--verify', '--end-of-options', "$Base^{commit}")) -join ''
    $section = [regex]::Match($text, '(?ms)^## Where\s*\r?\n(.*?)(?=^##\s|\z)')
    if (-not $section.Success) { throw 'missing Where section' }
    $where = @([regex]::Matches($section.Groups[1].Value, '(?m)^\s*-\s+`([^`]+)`') |
        ForEach-Object { $_.Groups[1].Value.Trim().Replace('\', '/') } |
        Where-Object { $_.Length -gt 0 } | Sort-Object -Unique)
    $folders = @($where | ForEach-Object { if ($_ -match '/') { $_ -replace '/[^/]+$', '' } else { '' } })

    $range = "$sha..$tip"
    $subjects = @(Invoke-Git @('log', '--first-parent', '--format=%s', $range))
    Write-Host 'COMMITS:'
    $subjects | ForEach-Object { Write-Host $_ }
    $changed = @{}
    $deleted = $false
    Write-Host 'FILES:'
    # Collect every touched path, including changes later reverted; renames count as delete plus add.
    $commits = @(Invoke-Git @('rev-list', '--first-parent', $range))
    foreach ($commit in $commits) {
        foreach ($line in (Invoke-Git @('diff-tree', '--root', '--diff-merges=first-parent', '--no-commit-id', '--name-status', '-r', '--no-renames', $commit))) {
            $parts = $line -split "`t", 2
            if ($parts.Count -ne 2) { continue }
            $file = $parts[1]
            $changed[$file] = $true
            if ($parts[0] -eq 'D' -and $where -ccontains $file) { $deleted = $true }
        }
    }
    $whereCount = 0; $nearCount = 0
    foreach ($file in ($changed.Keys | Sort-Object)) {
        if ($where -ccontains $file) { $whereCount++; Write-Host "WHERE $file" }
        else {
            $folder = if ($file -match '/') { $file -replace '/[^/]+$', '' } else { '' }
            if ($folders -ccontains $folder) { $nearCount++; Write-Host "NEAR $file" }
        }
    }
    Write-Host 'CARDS:'
    $cards = @($subjects | ForEach-Object {
        if ($_ -cmatch '^(?<task>[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+):') {
            $card = ".coord/tasks/$($Matches.task).md"
            if (Test-Path -LiteralPath (Join-Path $root $card) -PathType Leaf) { $card }
        }
    } | Sort-Object -Unique)
    $cards | ForEach-Object { Write-Host $_ }
    $rescan = if ($deleted -or $whereCount -gt ($where.Count / 2)) { 'yes' } else { 'no' }
    Write-Host "RESCAN: $rescan"
    if ($whereCount) { Write-Host "DELTA: where ($whereCount of $($where.Count) Where files changed)" }
    elseif ($nearCount) { Write-Host "DELTA: near ($nearCount)" }
    else { Write-Host 'DELTA: none' }
    exit 0
} catch {
    Write-Host "SCAN-DELTA: FAIL ($($_.Exception.Message))"
    exit 1
}
