# Reports linked worktrees that do not follow the item naming and base conventions.
# SessionStart must remain silent and must never prevent a session from starting.
# Opt-in: part of my dev loop. Set ANTINOTEPAD_LOOP=1 to enable (see docs/claude-hooks.md).
if ($env:ANTINOTEPAD_LOOP -cne '1') { exit 0 }
$ErrorActionPreference = 'Stop'

try {
    $inputData = [Console]::In.ReadToEnd() | ConvertFrom-Json
    if ([string]::IsNullOrWhiteSpace($inputData.cwd)) { exit 0 }

    $root = & git -C $inputData.cwd rev-parse --path-format=absolute --show-toplevel 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $root) { exit 0 }
    $common = & git -C $root rev-parse --path-format=absolute --git-common-dir 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $common) { exit 0 }
    $root = [IO.Path]::GetFullPath($root.Trim()).TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
    $mainRoot = [IO.Path]::GetFullPath((Split-Path -Parent $common.Trim())).TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
    if ($root -ieq $mainRoot) { exit 0 }

    $problems = [Collections.Generic.List[string]]::new()
    $worktreeRoot = Join-Path $mainRoot '.claude/worktrees'
    $relative = [IO.Path]::GetRelativePath($worktreeRoot, $root).Replace('\', '/')
    if ($relative -cnotmatch '^(feat|fix|refactor|docs|chore)/[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+$') {
        $problems.Add('path does not match .claude/worktrees/<prefix>/<ID-slug>')
    }

    $branch = & git -C $root branch --show-current 2>$null
    if ($LASTEXITCODE -eq 0 -and $branch -cne $relative) {
        $problems.Add("branch '$branch' does not match path '$relative'")
    }

    # Heuristic: a branch cut from main sits at main's tip while dev has moved on; one cut from dev does not.
    $main = & git -C $root rev-parse --verify refs/heads/main 2>$null
    $mainOk = $LASTEXITCODE -eq 0
    $dev = & git -C $root rev-parse --verify refs/heads/dev 2>$null
    $devOk = $LASTEXITCODE -eq 0
    if ($mainOk -and $devOk -and $main -ne $dev) {
        $base = & git -C $root merge-base HEAD refs/heads/dev 2>$null
        if ($LASTEXITCODE -eq 0 -and $base -eq $main) {
            $problems.Add('worktree branches from main while dev has moved on; it may not be based on dev')
        }
    }

    if ($problems.Count -gt 0) {
        $logFolder = Join-Path $mainRoot '.coord/logs'
        [void](New-Item -ItemType Directory -Path $logFolder -Force)
        $log = Join-Path $logFolder 'worktree-check.log'
        foreach ($problem in $problems) {
            Add-Content -LiteralPath $log -Value ('{0} {1}: {2}' -f (Get-Date -Format 'o'), $root, $problem)
        }
    }
} catch {
    # A diagnostic hook must not interrupt startup, even when git or logging fails.
}
exit 0
