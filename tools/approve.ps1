# Checks approval in the main checkout, then creates the item branch and commits its untracked planning files.
# DryRun runs the same checks and prints the commands without creating or moving anything.
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Task,
    [Parameter(Mandatory)][ValidateSet('feat', 'fix', 'refactor', 'docs', 'chore')][string]$Prefix,
    [switch]$NoWorktree,
    [ValidateRange(1, 2147483647)][int]$Cap = 3,
    [switch]$DryRun
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Git errors stop the preflight before any dependent operation can run.
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
    $branch = (Invoke-Git $root @('branch', '--show-current')) -join ''
    if ($branch -cne 'dev') { throw 'main checkout must be on dev' }
    Write-Host 'CHECK: main checkout on dev'
    $changes = @(Invoke-Git $root @('status', '--porcelain', '--untracked-files=no'))
    if (@($changes | Where-Object { $_ -cnotmatch '^.. \.coord/STATUS\.md$' }).Count) { throw 'dirty checkout' }
    & git -C $root diff --cached --quiet
    if ($LASTEXITCODE -ne 0) { throw 'staged changes in main checkout' }
    foreach ($marker in @('SQUASH_MSG', 'MERGE_HEAD')) {
        $path = (Invoke-Git $root @('rev-parse', '--path-format=absolute', '--git-path', $marker)) -join ''
        if (Test-Path -LiteralPath $path) { throw "merge in progress: $marker" }
    }
    Write-Host 'CHECK: tracked changes limited to unstaged STATUS; no merge in progress'

    $card = ".coord/tasks/$Task.md"
    $untracked = @(Invoke-Git $root @('-c', 'core.quotePath=false', 'ls-files', '--others', '--exclude-standard'))
    if ($untracked -cnotcontains $card) { throw 'card must exist untracked' }
    $text = [IO.File]::ReadAllText((Join-Path $root $card))
    $approve = [regex]::Match($text, '(?ms)^## Approve[^\S\r\n]*\r?\n(.*?)(?=^##\s|\z)')
    if (-not $approve.Success -or (Test-UnfilledSection $approve.Groups[1].Value)) {
        throw 'Approve section missing or unfilled'
    }
    Write-Host 'CHECK: untracked card has a filled Approve section'

    $count = 0; $inPlan = $false
    foreach ($line in Get-Content -LiteralPath (Join-Path $root '.coord/STATUS.md')) {
        if ($line -match '^## Plan') { $inPlan = $true; continue }
        if ($inPlan -and $line -match '^## ') { break }
        if (-not $inPlan -or $line -notmatch '^\|' -or $line -match '^\|[\s|:-]+\|\s*$') { continue }
        $c = $line.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() }
        if ($c.Count -lt 5 -or $c[0] -eq 'Item' -or $c[0] -match '^\*\*|^Release marker' -or $c[0] -ceq $Task) { continue }
        if ($c[4] -match '^(approved|building|rework r[0-9]+|checking|accepted|ask-user|hand test|hand-tested)$') { $count++ }
    }
    Write-Host "CHECK: $count other items past Approve; cap $Cap"
    if ($count -ge $Cap) { throw 'cap' }
    Write-Host "CHECK: tools/files-overlap.ps1 -Task $Task"
    & pwsh -NoProfile -File (Join-Path $root 'tools/files-overlap.ps1') -Task $Task
    if ($LASTEXITCODE -ne 0) { throw 'overlap' }

    $branch = "$Prefix/$Task"
    $destination = if ($NoWorktree) { $root } else { Join-Path $root ".claude/worktrees/$branch" }
    $research = ".coord/research/$($Task.Substring($Task.IndexOf('-') + 1)).md"
    if ((Test-Path -LiteralPath (Join-Path $root $research)) -and $untracked -cnotcontains $research) {
        throw 'research file must be untracked'
    }
    $files = @($untracked | Where-Object {
        $_ -ceq $card -or $_ -ceq $research -or $_ -cmatch "^\.coord/tasks/$([regex]::Escape($Task))-.*\.md$" -or
        $_ -cmatch "^\.coord/reports/$([regex]::Escape($Task))-"
    })
    if ($NoWorktree) { Write-Host "COMMAND: git switch -c $branch dev" }
    else { Write-Host "COMMAND: git worktree add `"$destination`" -b $branch dev" }
    foreach ($file in $files) {
        if (-not $NoWorktree) { Write-Host "COMMAND: Move-Item -LiteralPath `"$(Join-Path $root $file)`" -Destination `"$(Join-Path $destination $file)`"" }
        Write-Host "COMMAND: git -C `"$destination`" add -- `"$file`""
    }
    Write-Host "COMMAND: git -C `"$destination`" commit -m `"${Task}: card`""
    if (-not $DryRun) {
        if ($NoWorktree) { Invoke-Git $root @('switch', '-c', $branch, 'dev') | Out-Host }
        else {
            Invoke-Git $root @('worktree', 'add', $destination, '-b', $branch, 'dev') | Out-Host
            foreach ($file in $files) {
                $target = Join-Path $destination $file
                New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
                Move-Item -LiteralPath (Join-Path $root $file) -Destination $target
            }
        }
        Invoke-Git $destination (@('add', '--') + $files) | Out-Host
        Invoke-Git $destination @('commit', '-m', "${Task}: card") | Out-Host
    }
    $reason = if ($DryRun) { 'dry run' } else { 'card committed' }
    Write-Host "APPROVE: PASS ($reason)"
    exit 0
} catch {
    Write-Host "APPROVE: FAIL ($($_.Exception.Message))"
    exit 1
}
