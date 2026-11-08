# Claude status line: shows branch, item, worktree, context, cost and live in-flight items.
$ErrorActionPreference = 'Stop'
$branch = '-'
try {
    $raw = [Console]::In.ReadToEnd()
    if (-not $raw) { throw 'No status input.' }
    $inputData = $raw | ConvertFrom-Json -ErrorAction Stop
    $cwd = [string]$inputData.cwd
    if (-not $cwd) { $cwd = [string]$inputData.workspace.current_dir }
    if (-not $cwd) { throw 'No current directory.' }

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

    $parts = @($branch)
    if ($item) { $parts += $item }
    if ($isWorktree) { $parts += 'WT' }
    $percent = $inputData.context_window.used_percentage
    if ($null -ne $percent) { $parts += ('ctx {0}%' -f [math]::Round([double]$percent)) }
    $cost = $inputData.cost.total_cost_usd
    if ($null -ne $cost) { $parts += ('${0:N2}' -f [double]$cost) }
    $firstLine = $parts -join ' | '

    $mainRoot = Split-Path $commonDirPath -Parent
    $statusPath = Join-Path $mainRoot '.coord/STATUS.md'
    $ids = @()
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
    "$firstLine`nin flight: $(if ($ids.Count) { $ids -join ', ' } else { 'none' })"
}
catch {
    $branch
}
exit 0
