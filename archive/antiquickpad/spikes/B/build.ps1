# Pinned hello-window build. Does not install tools or alter user-level settings.
$ErrorActionPreference = 'Stop'
$spikeRoot = $PSScriptRoot
$repoRoot = Split-Path (Split-Path $spikeRoot -Parent) -Parent
$env:PATH = "$env:USERPROFILE\.cargo\bin;$env:PATH"
$env:CARGO_HOME = Join-Path $spikeRoot '.cargo-home'
$env:CARGO_TARGET_DIR = Join-Path $spikeRoot 'target'
$env:CARGO_NET_OFFLINE = 'true'
$env:CARGO_NET_RETRY = '0'
$env:CARGO_HTTP_TIMEOUT = '20'
$logRoot = Join-Path $repoRoot 'docs/spikes/B'
$timer = [Diagnostics.Stopwatch]::StartNew()
& cargo build --offline --locked --manifest-path (Join-Path $spikeRoot 'Cargo.toml') *> (Join-Path $logRoot 'build-probe.log')
$buildExit = $LASTEXITCODE
$timer.Stop()
"exit=$buildExit elapsed_seconds=$($timer.Elapsed.TotalSeconds)" | Set-Content (Join-Path $logRoot 'build-probe-result.txt')
Get-Content (Join-Path $logRoot 'build-probe-result.txt')
Get-Content (Join-Path $logRoot 'build-probe.log') -Tail 35
exit $buildExit


