# Interactive full-spike launcher. No build or network.
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$exe = Join-Path $PSScriptRoot 'target/debug/antiquickpad-spike-b.exe'
$running = Get-Process antiquickpad-spike-b -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe }
if ($running) { throw 'Spike B is already running. Use Ctrl+Alt+Space or tray Show; quit before relaunching.' }
$env:ANTIQUICKPAD_PROBE_LOG = Join-Path $repoRoot 'docs/spikes/B/runtime.tsv'
$process = Start-Process -FilePath $exe -WorkingDirectory $PSScriptRoot -RedirectStandardOutput (Join-Path $repoRoot 'docs/spikes/B/runtime-stdout.log') -RedirectStandardError (Join-Path $repoRoot 'docs/spikes/B/runtime-stderr.log') -PassThru
"pid=$($process.Id) trace=$env:ANTIQUICKPAD_PROBE_LOG"
