# Adds this checkout's loop context to the user's global Claude status line.
# Opt-in: part of my dev loop. Set ANTINOTEPAD_LOOP=1 to enable (see docs/claude-hooks.md).
if ($env:ANTINOTEPAD_LOOP -cne '1') { exit 0 }
$ErrorActionPreference = 'Stop'
$raw = [Console]::In.ReadToEnd()
$globalPath = $env:STATUSLINE_GLOBAL
if (-not $globalPath) { $globalPath = Join-Path $HOME '.claude/statusline-command.mjs' }
$globalOutput = ''

try {
    $node = Get-Command node -ErrorAction SilentlyContinue
    if ($node -and (Test-Path -LiteralPath $globalPath -PathType Leaf)) {
        $start = [Diagnostics.ProcessStartInfo]::new()
        $start.FileName = $node.Source
        $start.ArgumentList.Add($globalPath)
        $start.RedirectStandardInput = $true
        $start.RedirectStandardOutput = $true
        $start.RedirectStandardError = $true
        $start.UseShellExecute = $false
        $start.StandardOutputEncoding = [Text.Encoding]::UTF8
        $process = [Diagnostics.Process]::new()
        $process.StartInfo = $start
        if ($process.Start()) {
            $process.StandardInput.Write($raw)
            $process.StandardInput.Close()
            $globalOutput = $process.StandardOutput.ReadToEnd()
            $process.WaitForExit()
            if ($process.ExitCode -ne 0) { $globalOutput = '' }
        }
        $process.Dispose()
    }
}
catch {
    $globalOutput = ''
}

if ($globalOutput) { [Console]::Out.Write($globalOutput) }

try {
    $cwd = ''
    if ($raw) {
        $inputData = $raw | ConvertFrom-Json -ErrorAction Stop
        $cwd = [string]$inputData.cwd
        if (-not $cwd) { $cwd = [string]$inputData.workspace.current_dir }
    }
    if (-not $cwd) { $cwd = $env:CLAUDE_PROJECT_DIR }
    if (-not $cwd) { $cwd = Split-Path $PSScriptRoot -Parent }

    $gitInfo = @(& git -C $cwd rev-parse --abbrev-ref HEAD --git-dir --git-common-dir 2>$null)
    if ($LASTEXITCODE -ne 0 -or $gitInfo.Count -lt 3) { throw 'Git metadata unavailable.' }
    $branch = [string]$gitInfo[0]
    $gitDir = [string]$gitInfo[1]
    $commonDir = [string]$gitInfo[2]
    $gitDirPath = if ([IO.Path]::IsPathRooted($gitDir)) { [IO.Path]::GetFullPath($gitDir) } else { [IO.Path]::GetFullPath((Join-Path $cwd $gitDir)) }
    $commonDirPath = if ([IO.Path]::IsPathRooted($commonDir)) { [IO.Path]::GetFullPath($commonDir) } else { [IO.Path]::GetFullPath((Join-Path $cwd $commonDir)) }
    $isWorktree = $gitDirPath -ne $commonDirPath
    $item = ''
    if ($branch -match '/') { $item = $branch.Substring($branch.IndexOf('/') + 1) }
    elseif ($cwd -match '[\\/]\.claude[\\/]worktrees[\\/].+[\\/]([^\\/]+)$') { $item = $Matches[1] }

    $ids = @()
    $statusPath = Join-Path (Split-Path $commonDirPath -Parent) '.coord/STATUS.md'
    $inPlan = $false
    foreach ($line in (Get-Content $statusPath -Encoding utf8)) {
        if ($line -match '^## Plan') { $inPlan = $true; continue }
        if ($inPlan -and $line -match '^## ') { break }
        if (-not $inPlan -or $line -notmatch '^\|' -or $line -match '^\|[\s|:-]+\|\s*$') { continue }
        $cells = $line.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() }
        if ($cells.Count -lt 5 -or $cells[0] -eq 'Item' -or $cells[0] -match '^\*\*|^Release marker') { continue }
        if ($cells[4] -match '^(done|dropped|planned|needs you|proposed|paused)(\b|\s|\()|^-$') { continue }
        if ($cells[0] -match '^[A-Za-z0-9]+(?:-[A-Za-z0-9]+)+$') { $ids += $cells[0] }
    }

    $parts = @()
    if ($isWorktree) { $parts += 'WT' }
    if ($branch) { $parts += $branch }
    if ($item) { $parts += "item $item" }
    if ($ids.Count) { $parts += "in flight $($ids -join ', ')" }
    if ($parts.Count -and ($isWorktree -or $branch -notin @('dev', 'main') -or $ids.Count)) {
        if ($globalOutput -and -not $globalOutput.EndsWith("`n") -and -not $globalOutput.EndsWith("`r")) { [Console]::Out.WriteLine() }
        [Console]::Out.WriteLine("loop: $($parts -join ' | ')")
    }
}
catch {
    # Status-line failures should not interfere with Claude's session.
}
exit 0
