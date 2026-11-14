# Merge checks: card phases, clean checkouts, dev ancestry, the gate and per-commit builds.
# Run from the main checkout before merging an item onto dev with --no-ff.
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Task,
    [switch]$Offline,
    [switch]$SkipBuilds
)
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

# Accept the gate tip or an ancestor whose later tree changes are limited to this item's card, reports and STATUS.
function Test-GateHead([string]$Root, [string]$Task, [string]$Tip, [string]$Report) {
    if ($Report -cmatch "(?m)^Head: $Tip\r?`$") { return $true }
    if ($Report -cnotmatch '(?m)^Head: (?<head>[0-9a-f]{40})\r?$') { return $false }
    $gateHead = $Matches.head
    & git -C $Root merge-base --is-ancestor $gateHead $Tip 2>$null
    if ($LASTEXITCODE -ne 0) { return $false }
    $allowed = '^\.coord/(?:tasks|reports)/' + [regex]::Escape($Task) + '(?:[-.]|$)'
    $files = @(Invoke-Git $Root @('diff', '--name-only', '--no-renames', $gateHead, $Tip))
    @($files | Where-Object { $_ -cnotmatch $allowed -and $_ -cne '.coord/STATUS.md' }).Count -eq 0
}

# Build each product-changing commit in an isolated detached worktree and remove it afterwards.
function Test-CommitBuilds([string]$Root, [string]$Branch, [string]$Task) {
    $commits = @(Invoke-Git $Root @('rev-list', '--reverse', '--no-merges', "dev..$Branch"))
    $scratch = Join-Path $Root "build/$Task-preflight"
    $created = $false
    $built = 0
    $skipped = 0
    $oldTelemetry = $env:AVALONIA_TELEMETRY_OPTOUT
    $oldData = $env:ANTINOTEPAD_DATA
    $oldDev = $env:ANTINOTEPAD_DEV
    try {
        foreach ($commit in $commits) {
            $short = (Invoke-Git $Root @('rev-parse', '--short', $commit)) -join ''
            $subject = (Invoke-Git $Root @('show', '-s', '--format=%s', $commit)) -join ''
            $files = @(Invoke-Git $Root @('diff-tree', '--no-commit-id', '--name-only', '-r', '--root', $commit))
            # Product code and repository-level build inputs can change what builds.
            if (@($files | Where-Object { $_ -cmatch '^(product/|Directory\.|global\.json$|[Nn]u[Gg]et\.config$)' }).Count -eq 0) {
                Write-Host "SKIP ${short} ${subject}: no product code"
                $skipped++
                continue
            }
            if (-not $created) {
                $others = @(Invoke-Git $Root @('worktree', 'list', '--porcelain'))
                $own = [regex]::Escape($scratch.Replace([char]92, '/'))
                for ($i = 0; $i -lt $others.Count; $i++) {
                    if ($others[$i] -match "^worktree $own$" -and ($i + 2) -lt $others.Count -and $others[$i + 2] -like 'branch *') {
                        throw "scratch path is a branch checkout: $scratch"
                    }
                }
                # A leftover from an earlier killed run is ours (build/ is ignored scratch space): clear it.
                if (Test-Path -LiteralPath $scratch) {
                    & git -C $Root worktree remove --force $scratch 2>$null
                    Remove-Item -LiteralPath $scratch -Recurse -Force -ErrorAction SilentlyContinue
                    & git -C $Root worktree prune
                    if (Test-Path -LiteralPath $scratch) { throw "cannot clear scratch worktree: $scratch" }
                }
                New-Item -ItemType Directory -Path (Split-Path $scratch -Parent) -Force | Out-Null
                Invoke-Git $Root @('worktree', 'add', '--detach', $scratch, $commit) | Out-Null
                $created = $true
            }
            Invoke-Git $scratch @('checkout', '--detach', $commit) | Out-Null
            $temp = Join-Path $scratch 'build/preflight-temp'
            if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp -Recurse -Force }
            $out = Join-Path $temp 'out'
            $data = Join-Path $temp 'data'
            $env:AVALONIA_TELEMETRY_OPTOUT = '1'
            Remove-Item Env:ANTINOTEPAD_DEV, Env:ANTINOTEPAD_DATA -ErrorAction SilentlyContinue
            $arguments = @('build', (Join-Path $scratch 'product/AntiNotepad/AntiNotepad.csproj'), '-c', 'Debug', '-o', $out, '-nodeReuse:false', '-p:UseSharedCompilation=false')
            if ($Offline) { $arguments += '-p:NuGetAudit=false' }
            Push-Location $scratch
            try { & dotnet @arguments | Out-Host }
            catch { throw "commit $short `"$subject`" does not build" }
            finally { Pop-Location }
            if ($LASTEXITCODE -ne 0) { throw "commit $short `"$subject`" does not build" }
            $env:ANTINOTEPAD_DATA = $data
            $exe = Join-Path $out 'AntiNotepad.exe'
            if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) { throw "commit $short `"$subject`" fails --selftest" }
            # The exe is a GUI-subsystem program, so run it as a process with redirected output (as gate.ps1 does).
            $logFile = Join-Path $temp 'selftest.log'
            $process = Start-Process -FilePath $exe -ArgumentList '--selftest' -WorkingDirectory $out -NoNewWindow -Wait `
                -PassThru -RedirectStandardOutput $logFile -RedirectStandardError (Join-Path $temp 'selftest.err')
            $output = @(Get-Content -LiteralPath $logFile -ErrorAction SilentlyContinue)
            if ($process.ExitCode -ne 0 -or @($output | Where-Object { $_ -match '^SELFTEST PASS\s*$' }).Count -eq 0) {
                $output | Select-Object -Last 15 | Out-Host
                throw "commit $short `"$subject`" fails --selftest"
            }
            $built++
        }
        if ($built -eq 0) { Write-Host "CHECK: all $skipped commit(s) skipped (no product code)" }
        else { Write-Host "CHECK: $built commit(s) build and pass --selftest ($skipped skipped, no product code)" }
    }
    finally {
        $env:AVALONIA_TELEMETRY_OPTOUT = $oldTelemetry
        $env:ANTINOTEPAD_DATA = $oldData
        $env:ANTINOTEPAD_DEV = $oldDev
        if ($created) {
            # Git cannot always delete the build output on Windows; fall back to a plain delete, then prune.
            & git -C $Root worktree remove --force $scratch 2>$null
            if ($LASTEXITCODE -ne 0) {
                Remove-Item -LiteralPath $scratch -Recurse -Force -ErrorAction SilentlyContinue
                & git -C $Root worktree prune
                if (Test-Path -LiteralPath $scratch) { Write-Host "WARN: scratch worktree left behind: $scratch" }
            }
        }
    }
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
    if (@($changes | Where-Object { $_ -cnotmatch '^.. (\.coord/(STATUS|pending|lessons|status-archive)\.md|\.claude/settings\.json)$' }).Count) { throw 'dirty item checkout' }
    & git -C $root merge-base --is-ancestor dev $branch
    if ($LASTEXITCODE -eq 1) { throw "behind dev: run tools/catch-up.ps1 -Task $Task" }
    if ($LASTEXITCODE -ne 0) { throw 'cannot compare dev with item branch' }
    if (@(Invoke-Git $root @('rev-list', '--merges', "dev..$branch")).Count) {
        throw 'branch has merge commits: rebase onto dev (tools/catch-up.ps1)'
    }
    Write-Host 'CHECK: item checkout clean except STATUS and settings.json drift; caught up with dev'

    $reports = Join-Path $checkout '.coord/reports'
    $gate = @(Get-ChildItem -LiteralPath $reports -File | Where-Object {
        $_.Name -cmatch "^$([regex]::Escape($Task))(?:-r[0-9]+)?\.gate\.md$"
    } | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1)
    if (-not $gate.Count) { throw 'gate report missing' }
    $report = [IO.File]::ReadAllText($gate[0].FullName)
    $head = (Invoke-Git $root @('rev-parse', $branch)) -join ''
    if ($report -notmatch '(?m)^GATE: PASS\r?$') { throw 'newest gate did not pass' }
    if (-not (Test-GateHead $root $Task $head $report)) { throw 'stale Head: re-run the gate' }
    Write-Host "CHECK: newest gate PASS at $head ($($gate[0].Name))"
    if ($report -cnotmatch "(?m)^Head: $head\r?`$") {
        Write-Host 'CHECK: commits after the gate touch only the card, reports and STATUS'
    }

    if ($SkipBuilds) { Write-Host 'CHECK: per-commit builds skipped (-SkipBuilds)' }
    else { Test-CommitBuilds $root $branch $Task }

    & git -C $root diff --cached --quiet
    if ($LASTEXITCODE -ne 0) { throw 'staged changes in main checkout' }
    $path = (Invoke-Git $root @('rev-parse', '--path-format=absolute', '--git-path', 'MERGE_HEAD')) -join ''
    if (Test-Path -LiteralPath $path) { throw 'merge in progress: MERGE_HEAD' }
    $mainBranch = (Invoke-Git $root @('branch', '--show-current')) -join ''
    if ($mainBranch -ceq 'dev') { Write-Host 'Main checkout on dev: merge can run' }
    elseif ($mainBranch -ceq $branch) { Write-Host 'Main checkout on item branch (-NoWorktree): switch to dev first' }
    else { throw 'main checkout must be on dev or the item branch' }
    Write-Host "NEXT: in the merge, move the $Task row from STATUS Pending work to the Finished rows table in .coord/status-archive.md (state done)"
    Write-Host 'MERGE-PREFLIGHT: PASS (ready)'
    exit 0
} catch {
    Write-Host "MERGE-PREFLIGHT: FAIL ($($_.Exception.Message))"
    exit 1
}
