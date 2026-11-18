# Claude UserPromptSubmit hook: lists the items in flight from the live STATUS plan and warns when the approved-item cap
# is exceeded. Its output is added to Claude's context. Reads STATUS from the main checkout even when run from a worktree.
param([string]$Root = (Split-Path $PSScriptRoot -Parent))
$ErrorActionPreference = 'Stop'
try {
    $common = (& git -C $Root rev-parse --path-format=absolute --git-common-dir 2>$null)
    if ($LASTEXITCODE -eq 0 -and $common) { $Root = Split-Path $common.Trim() -Parent }
} catch { }
$status = Join-Path $Root '.coord/STATUS.md'

$live = @()
$pastApprove = 0
if (Test-Path $status) {
    $inPlan = $false
    foreach ($l in Get-Content $status -Encoding utf8) {
        if ($l -match '^## Plan') { $inPlan = $true; continue }
        if ($inPlan -and $l -match '^## ') { break }
        if (-not $inPlan -or $l -notmatch '^\|' -or $l -match '^\|[\s|:-]+\|\s*$') { continue }
        $c = $l.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() }
        if ($c.Count -lt 5 -or $c[0] -eq 'Item' -or $c[0] -match '^\*\*' -or $c[0] -match '^Release marker') { continue }
        if ($c[4] -match '^(approved|building|rework r[0-9]+|checking|accepted|ask-user|hand test|hand-tested)$') { $pastApprove++ }
        if ($c[4] -match '^(done|dropped|planned|needs you|proposed|paused|-)?$') { continue }
        $live += "$($c[0]) ($($c[4]), $($c[2]))"
    }
}

# ASCII only: hook output passes through the console code page.
if ($live.Count) { "In flight per STATUS: $($live -join '; ')." } else { 'In flight per STATUS: none.' }
if ($pastApprove -gt 3) { "WARNING: $pastApprove items past Approve (cap 3)." }

try {
    $roadmapScript = Join-Path $Root 'tools/roadmap-serve.ps1'
    if (Test-Path -LiteralPath $roadmapScript -PathType Leaf) {
        $handler = [System.Net.Http.HttpClientHandler]::new()
        $handler.UseProxy = $false
        $client = [System.Net.Http.HttpClient]::new($handler)
        $client.Timeout = [TimeSpan]::FromMilliseconds(300)
        $response = $null
        $roadmapRunning = $false
        try {
            $response = $client.GetAsync('http://localhost:47811/health').GetAwaiter().GetResult()
            if ($response.IsSuccessStatusCode) {
                $health = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json -ErrorAction Stop
                $roadmapRunning = ($health.app -eq 'antinotepad-roadmap' -and [int]$health.port -eq 47811)
            }
        }
        catch {
            # The status card must still print when the roadmap is unavailable.
        }
        finally {
            if ($null -ne $response) { $response.Dispose() }
            $client.Dispose()
        }

        if (-not $roadmapRunning) {
            $powerShell = (Get-Process -Id $PID -ErrorAction Stop).Path
            Start-Process -FilePath $powerShell -ArgumentList @('-NoProfile', '-File', ('"{0}"' -f $roadmapScript)) -WindowStyle Hidden | Out-Null
        }
    }
}
catch {
    # Starting the roadmap is best-effort and does not change the status-card hook.
}

exit 0
