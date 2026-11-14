<#
Loop v5.3.0: fetches dev and rebases an item checkout onto it; merge runs from the local branch, so no push is suggested.
Usage: pwsh -NoProfile -File tools/catch-up.ps1 -Task <ID-slug> [-Base dev] [-Push]
Exit 0 = UP TO DATE/CLEAN; 1 = REFUSED; 2 = CONFLICT (hot files marked, rebase left open for resolution).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Task,
    [string]$Base = 'dev',
    [switch]$Push,
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
$branchPattern = '^(' + ($prefixes -join '|') + ')/(?<slug>[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+)$'

# Reads only the Files section so other card lists cannot cause false retests.
function Get-CardFiles([string]$CardPath) {
    if (-not (Test-Path -LiteralPath $CardPath -PathType Leaf)) { return $null }
    $text = [IO.File]::ReadAllText($CardPath)
    $section = [regex]::Match($text, '(?ms)^## Files\s*\r?\n(.*?)(?=^##\s|\z)')
    if (-not $section.Success) { return @() }
    @([regex]::Matches($section.Groups[1].Value, '(?m)^\s*-\s+`([^`]+)`') |
        ForEach-Object { $_.Groups[1].Value.Trim().Replace('\', '/') } | Where-Object { $_.Length -gt 0 })
}

# Report against the card in this checkout after a successful catch-up.
function Write-Retest([string[]]$Incoming) {
    $files = Get-CardFiles (Join-Path $repoRoot ".coord/tasks/$Task.md")
    $touched = @($Incoming | Where-Object { $files -contains $_ })
    if ($touched.Count -gt 0) { Write-Host "RETEST: yes ($($touched -join ', '))" }
    else { Write-Host 'RETEST: no' }
}

# Push only the current item branch when explicitly requested.
function Push-ItemBranch {
    if (-not $Push) { return }
    $output = @(& git -C $repoRoot push --force-with-lease --no-follow-tags origin "refs/heads/${branch}:refs/heads/$branch" 2>&1)
    if ($LASTEXITCODE -ne 0) { Write-Host "REFUSED: push failed: $($output -join [Environment]::NewLine)"; exit 1 }
}

$repoRoot = & git -C $PSScriptRoot rev-parse --show-toplevel
if ($LASTEXITCODE -ne 0) { Write-Host 'REFUSED: cannot find repository root.'; exit 1 }
$repoRoot = $repoRoot.Trim()
foreach ($marker in @('rebase-merge', 'rebase-apply')) {
    $path = & git -C $repoRoot rev-parse --path-format=absolute --git-path $marker
    if ($LASTEXITCODE -ne 0) { Write-Host 'REFUSED: cannot check rebase state.'; exit 1 }
    if (Test-Path -LiteralPath $path) { Write-Host 'REFUSED: a rebase is already in progress'; exit 1 }
}
$branch = & git -C $repoRoot rev-parse --abbrev-ref HEAD
if ($LASTEXITCODE -ne 0) { Write-Host 'REFUSED: cannot determine current branch.'; exit 1 }
$branch = $branch.Trim()
if ($branch -cnotmatch $branchPattern -or $Matches.slug -cne $Task) {
    Write-Host "REFUSED: checkout is on '$branch', expected <prefix>/$Task ($($prefixes -join ', '))."; exit 1
}
$changes = @(& git -C $repoRoot status --porcelain --untracked-files=no)
if ($LASTEXITCODE -ne 0) { Write-Host 'REFUSED: git status failed.'; exit 1 }
if (@($changes | Where-Object { $_ -cnotmatch '^.. (\.coord/(STATUS|pending|lessons|status-archive)\.md|\.claude/settings\.json)$' }).Count -gt 0) {
    Write-Host 'REFUSED: tracked changes outside .coord/STATUS.md and .claude/settings.json; commit or stash your own changes first.'; exit 1
}

$fetchOutput = @(& git -C $repoRoot fetch --quiet origin 2>&1)
if ($LASTEXITCODE -ne 0) { Write-Host "REFUSED: $($fetchOutput -join [Environment]::NewLine)"; exit 1 }
$baseRef = $Base
$baseCommit = & git -C $repoRoot rev-parse --verify --quiet "refs/heads/$Base"
if ($LASTEXITCODE -ne 0) { Write-Host 'REFUSED: cannot resolve base.'; exit 1 }
$remoteCommit = & git -C $repoRoot rev-parse --verify --quiet "refs/remotes/origin/$Base"
if ($LASTEXITCODE -eq 0) {
    if ($remoteCommit -ne $baseCommit) { Write-Host "NOTE: origin/$Base differs from $Base" }
}
elseif ($LASTEXITCODE -ne 1) { Write-Host 'REFUSED: cannot resolve remote base.'; exit 1 }
& git -C $repoRoot merge-base --is-ancestor "refs/heads/$baseRef" HEAD
if ($LASTEXITCODE -eq 0) { Write-Host "UP TO DATE with $baseRef"; Write-Retest @(); Push-ItemBranch; exit 0 }
if ($LASTEXITCODE -ne 1) { Write-Host 'REFUSED: cannot compare base with HEAD.'; exit 1 }

$incoming = @(& git -C $repoRoot diff --name-only -z "HEAD...refs/heads/$baseRef")
if ($LASTEXITCODE -ne 0) { Write-Host 'REFUSED: cannot list incoming files.'; exit 1 }
$incoming = @(($incoming -join "`n").Split([char]0, [StringSplitOptions]::RemoveEmptyEntries))
$rebaseOutput = @(& git -C $repoRoot rebase "refs/heads/$baseRef" 2>&1)
if ($LASTEXITCODE -eq 0) {
    Write-Host "CLEAN: rebased onto $baseRef ($($incoming.Count) incoming file(s)); re-run tools/gate.ps1 -Task $Task; merge from the local branch (no push needed)"
    $incoming | ForEach-Object { Write-Host "  incoming $_" }
    Write-Retest $incoming
    Push-ItemBranch
    exit 0
}
$conflicts = @(& git -C $repoRoot diff --name-only --diff-filter=U -z)
if ($LASTEXITCODE -ne 0) { Write-Host 'REFUSED: cannot list conflicts.'; exit 1 }
$conflicts = @(($conflicts -join "`n").Split([char]0, [StringSplitOptions]::RemoveEmptyEntries))
if ($conflicts.Count -eq 0) { Write-Host "REFUSED: $($rebaseOutput -join [Environment]::NewLine)"; exit 1 }
Write-Host "CONFLICT: $($conflicts.Count) file(s); resolve, git add, git rebase --continue (or git rebase --abort)"
foreach ($c in $conflicts) { Write-Host ("  {0}{1}" -f $(if (@($HotFiles | Where-Object { $c -like $_ }).Count -gt 0) { '[hot] ' } else { '' }), $c) }
exit 2
