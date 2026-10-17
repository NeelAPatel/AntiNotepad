# # spike — launch-to-frame and idle-memory measurement; interactive user shell only.
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$exe = Join-Path $PSScriptRoot 'target/debug/antiquickpad-spike-b.exe'
if (Get-Process antiquickpad-spike-b -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe }) { throw 'Quit the running B spike before measuring a fresh launch.' }
$env:ANTIQUICKPAD_PROBE_LOG = Join-Path $repoRoot 'docs/spikes/B/runtime.tsv'
$launchTimer = [Diagnostics.Stopwatch]::StartNew()
$process = Start-Process -FilePath $exe -WorkingDirectory $PSScriptRoot -RedirectStandardOutput (Join-Path $repoRoot 'docs/spikes/B/runtime-stdout.log') -RedirectStandardError (Join-Path $repoRoot 'docs/spikes/B/runtime-stderr.log') -PassThru
$frame = $null
while ($launchTimer.Elapsed.TotalSeconds -lt 20 -and -not $process.HasExited) {
    if (Test-Path -LiteralPath $env:ANTIQUICKPAD_PROBE_LOG) {
        $frame = Get-Content -LiteralPath $env:ANTIQUICKPAD_PROBE_LOG -Tail 100 | Where-Object { $_ -match "`tFIRST_EDITABLE_FRAME`t" -and $_ -match "pid=$($process.Id)(`t|$)" } | Select-Object -Last 1
        if ($frame) { break }
    }
    Start-Sleep -Milliseconds 20
}
$launchTimer.Stop()
$metrics = [ordered]@{pid=$process.Id;configuration='debug with diagnostic I/O';launch_to_frame_observed_ms=if($frame){$launchTimer.Elapsed.TotalMilliseconds}else{$null};frame_log=$frame;frame_timeout=($null -eq $frame);working_set_mib=$null;private_memory_mib=$null;sample='10 seconds after first frame; user should leave idle'}
if ($frame) { Start-Sleep -Seconds 10 }
$process.Refresh()
if (-not $process.HasExited) {
    $metrics.working_set_mib = [math]::Round($process.WorkingSet64/1MB,2)
    $metrics.private_memory_mib = [math]::Round($process.PrivateMemorySize64/1MB,2)
}
$path = Join-Path $repoRoot "docs/spikes/B/metrics-$($process.Id).json"
$metrics | ConvertTo-Json | Set-Content -LiteralPath $path
$metrics | ConvertTo-Json
"Metrics saved: $path. Leave the app running for the interaction test."
