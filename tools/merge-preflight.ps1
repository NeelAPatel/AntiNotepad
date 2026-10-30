# Read-only merge checks: card phases, clean checkouts, dev ancestry and a gate tied to the branch tip.
# Run from the main checkout before squash-merging an item onto dev.
[CmdletBinding()]
param([Parameter(Mandatory)][string]$Task)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$prefixes = @('feat', 'fix', 'refactor', 'docs', 'chore')
$branchPattern = '^(' + ($prefixes -join '|') + ')/(?<slug>[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+)$'

# Disable optional index writes and refuse Git failures rather than treating missing output as clean.
function Invoke-Git([string]$Path, [string[]]$Arguments) {
    $result = @(& git --no-optional-locks -C $Path @Arguments)
    if ($LASTEXITCODE -ne 0) { throw "git $($Arguments -join ' ') failed" }
    $result
}

# Reject empty sections and template values after an optional bullet and bold field label.
function Test-UnfilledSection([string]$Text) {
    [string]::IsNullOrWhiteSpace($Text) -or
        $Text -match '(?m)^[^\S\r\n]*(?:[-*][^\S\r\n]+)?(?:\*\*[^*\r\n]+:\*\*[^\S\r\n]*)?<[^>\r\n]+>(?=\s|$)'
}

try {
    if ($Task -cnotmatch '^[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+$') { throw 'invalid task name' }
    $root = (Invoke-Git $PSScriptRoot @('rev-parse', '--show-toplevel')) -join ''
    $trees = @(Invoke-Git $root @('worktree', 'list', '--porcelain'))
    if ($trees[0].Substring(9) -ne $root.Replace('\', '/')) { throw 'run from the main checkout' }
    $branches = @(Invoke-Git $root @('for-each-ref', '--format=%(refname:short)', 'refs/heads') |
        Where-Object { $_ -cmatch $branchPattern -and $Matches.slug -ceq $Task })
    if ($branches.Count -ne 1) { throw 'expected exactly one local item branch' }
    $branch = $branches[0]
    $checkout = $null; $path = $null
    foreach ($line in $trees) {
        if ($line -like 'worktree *') { $path = $line.Substring(9) }
        elseif ($line -ceq "branch refs/heads/$branch") { $checkout = $path }
    }
    if (-not $checkout) { throw 'item branch has no checkout' }
    $text = (Invoke-Git $root @('show', "${branch}:.coord/tasks/$Task.md")) -join "`n"
    foreach ($phase in @('Request', 'Plan', 'Approve', 'Check', 'Hand test')) {
        $name = if ($phase -eq 'Approve') { '(?:Approve|Go)' } else { [regex]::Escape($phase) }
        $section = [regex]::Match($text, "(?ms)^## $name[^\S\r\n]*\r?\n(.*?)(?=^##\s|\z)")
        if (-not $section.Success -or (Test-UnfilledSection $section.Groups[1].Value)) {
            throw "$phase section missing or unfilled"
        }
        if ($phase -eq 'Check') {
            foreach ($line in $section.Groups[1].Value -split '\r?\n') {
                if ($line -match 'ask the user' -and $line -notmatch 'answered') { throw 'open ask the user line' }
            }
        }
    }
    Write-Host 'CHECK: branch card phases filled; no open ask the user line'
    $changes = @(Invoke-Git $checkout @('status', '--porcelain', '--untracked-files=no'))
    if (@($changes | Where-Object { $_ -cnotmatch '^.. \.coord/STATUS\.md$' }).Count) { throw 'dirty item checkout' }
    & git -C $root merge-base --is-ancestor dev $branch
    if ($LASTEXITCODE -eq 1) { throw "behind dev: run tools/catch-up.ps1 -Task $Task" }
    if ($LASTEXITCODE -ne 0) { throw 'cannot compare dev with item branch' }
    Write-Host 'CHECK: item checkout clean except STATUS; caught up with dev'

    $reports = Join-Path $checkout '.coord/reports'
    $gate = @(Get-ChildItem -LiteralPath $reports -File | Where-Object {
        $_.Name -cmatch "^$([regex]::Escape($Task))(?:-r[0-9]+)?\.gate\.md$"
    } | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1)
    if (-not $gate.Count) { throw 'gate report missing' }
    $report = [IO.File]::ReadAllText($gate[0].FullName)
    $head = (Invoke-Git $root @('rev-parse', $branch)) -join ''
    if ($report -notmatch '(?m)^GATE: PASS\r?$') { throw 'newest gate did not pass' }
    if ($report -cnotmatch "(?m)^Head: $head\r?`$") { throw 'stale Head: re-run the gate' }
    Write-Host "CHECK: newest gate PASS at $head ($($gate[0].Name))"

    & git -C $root diff --cached --quiet
    if ($LASTEXITCODE -ne 0) { throw 'staged changes in main checkout' }
    foreach ($marker in @('SQUASH_MSG', 'MERGE_HEAD')) {
        $path = (Invoke-Git $root @('rev-parse', '--path-format=absolute', '--git-path', $marker)) -join ''
        if (Test-Path -LiteralPath $path) { throw "merge in progress: $marker" }
    }
    $mainBranch = (Invoke-Git $root @('branch', '--show-current')) -join ''
    if ($mainBranch -ceq 'dev') { Write-Host 'Main checkout on dev: squash can run' }
    elseif ($mainBranch -ceq $branch) { Write-Host 'Main checkout on item branch (-NoWorktree): switch to dev first' }
    else { throw 'main checkout must be on dev or the item branch' }
    Write-Host 'MERGE-PREFLIGHT: PASS (ready)'
    exit 0
} catch {
    Write-Host "MERGE-PREFLIGHT: FAIL ($($_.Exception.Message))"
    exit 1
}
