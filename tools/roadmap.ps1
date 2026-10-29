# Renders the STATUS "Plan" section as a table: .coord/roadmap.html (opened), or an HTML fragment on stdout (-Fragment, used
# by the /roadmap skill). -Hook: Claude PostToolUse hook; reads the hook JSON and regenerates only when STATUS.md changed.
param([switch]$Fragment, [switch]$Hook, [switch]$NoOpen)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$status = Join-Path $root '.coord/STATUS.md'
$out = Join-Path $root '.coord/roadmap.html'

if ($Hook) {
    try { $j = [Console]::In.ReadToEnd() | ConvertFrom-Json } catch { exit 0 }
    $p = "$($j.tool_input.file_path)"
    if ($p -notmatch 'STATUS\.md$') { exit 0 }
    $NoOpen = $true
}

$lines = Get-Content $status -Encoding utf8
$updated = ($lines | Where-Object { $_ -match '^Updated: ' } | Select-Object -First 1) -replace '^Updated: ', ''
$start = ($lines | Select-String -Pattern '^## Plan' | Select-Object -First 1).LineNumber
if (-not $start) { throw 'No "## Plan" section in STATUS.md' }

function Esc([string]$s) {
    $s = [System.Net.WebUtility]::HtmlEncode($s.Trim())
    [regex]::Replace($s, '`([^`]+)`', '<code>$1</code>')
}
function StateClass([string]$s) {
    switch -regex ($s.Trim()) {
        '^(done|dropped)$' { 'done'; break }
        '^needs you$' { 'you'; break }
        '^proposed$' { 'prop'; break }
        '^planned$' { 'plan'; break }
        default { 'live' }
    }
}

$sb = [System.Text.StringBuilder]::new()
$n = 0; $counts = @{}
foreach ($l in $lines[$start..($lines.Count - 1)]) {
    if ($l -match '^## ') { break }
    if ($l -match '^### (.+)$') { [void]$sb.Append("<div class=`"rm-p`">$(Esc $Matches[1])</div>"); continue }
    if ($l -notmatch '^\|' -or $l -match '^\|[\s|:-]+\|\s*$') { continue }
    $c = $l.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() }
    if ($c[0] -eq 'Item') { continue }
    if ($c[0] -match '^\*\*(.+)\*\*$') { [void]$sb.Append("<div class=`"rm-g`">$(Esc $Matches[1])</div>"); continue }
    $item, $scope, $tier, $effort, $state = $c[0], $c[1], $c[2], $c[3], $c[4]
    $cls = StateClass $state
    $counts[$state] = 1 + [int]$counts[$state]
    if ($item -match '^Release marker') {
        [void]$sb.Append("<div class=`"rm-m rm-$cls`"><span>Release marker</span><span>$(Esc $scope)</span><span class=`"rm-s`">$(Esc $state)</span></div>")
        continue
    }
    $n++
    $te = if ($tier -eq '-' -or -not $tier) { '' } else { "$(Esc $tier) · $(Esc $effort)" }
    $name = if ($item -match '^Decision: (.+)$') { "<span class=`"rm-d`">Decision</span> $(Esc $Matches[1])" } else { "<code>$(Esc $item)</code>" }
    [void]$sb.Append("<div class=`"rm-r rm-$cls`"><span>$n</span><span>$name</span><span class=`"rm-x`">$(Esc $scope)</span><span class=`"rm-x`">$te</span><span><span class=`"rm-s`">$(Esc $state)</span></span></div>")
}

$summary = ($counts.GetEnumerator() | Sort-Object Name | ForEach-Object { "$($_.Value) $($_.Name)" }) -join ' · '
$css = @'
<style>
.rm{font-size:13px;color:var(--text-primary)}
.rm-r{display:grid;grid-template-columns:28px minmax(0,1.25fr) minmax(0,2fr) 92px 92px;gap:8px;padding:6px 4px;border-bottom:0.5px solid var(--border);align-items:baseline}
.rm-r code,.rm-p code,.rm-x code{font-family:var(--font-mono);font-size:12px;overflow-wrap:anywhere}
.rm-h{color:var(--text-secondary);font-size:12px}.rm-x{color:var(--text-secondary)}
.rm-p{font-size:15px;font-weight:500;padding:16px 4px 6px;border-bottom:0.5px solid var(--border-strong)}
.rm-g{font-size:13px;font-weight:500;color:var(--text-secondary);padding:12px 4px 4px}
.rm-m{display:grid;grid-template-columns:120px minmax(0,1fr) 92px;gap:8px;padding:6px 8px;margin:6px 0;font-size:12px;border-radius:8px;background:var(--bg-warning);color:var(--text-warning)}
.rm-s{display:inline-block;font-size:11px;padding:1px 8px;border-radius:8px;background:var(--surface-1);color:var(--text-secondary)}
.rm-d{font-size:11px;padding:1px 6px;border-radius:8px;background:var(--bg-accent);color:var(--text-accent)}
.rm-you .rm-s{background:var(--bg-warning);color:var(--text-warning)}.rm-live .rm-s{background:var(--bg-accent);color:var(--text-accent)}
.rm-done{color:var(--text-muted)}.rm-done code,.rm-done .rm-x{color:var(--text-muted);text-decoration:line-through}
</style>
'@
$body = "$css<div class=`"rm`"><h2 class=`"sr-only`">AntiNotepad plan in run order from STATUS.md</h2>" +
    "<div class=`"rm-r rm-h`"><span>#</span><span>Item</span><span>Scope</span><span>Tier · effort</span><span>State</span></div>" +
    $sb.ToString() + "<div class=`"rm-h`" style=`"padding:10px 4px`">From .coord/STATUS.md, updated $(Esc $updated) · $(Esc $summary)</div></div>"

if ($Fragment) { [Console]::OutputEncoding = [Text.Encoding]::UTF8; $body; exit 0 }

$page = @"
<!doctype html><html><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>AntiNotepad roadmap</title><style>
:root{--text-primary:#1f1e1d;--text-secondary:#5f5e5a;--text-muted:#9a9893;--border:rgba(0,0,0,.1);--border-strong:rgba(0,0,0,.2);
--surface-1:#f1efe8;--bg-warning:#faeeda;--text-warning:#854f0b;--bg-accent:#e6f1fb;--text-accent:#185fa5;
--font-mono:'Cascadia Mono',Consolas,monospace;color-scheme:light dark}
@media (prefers-color-scheme:dark){:root{--text-primary:#ecebe6;--text-secondary:#b4b2a9;--text-muted:#77756f;--border:rgba(255,255,255,.1);
--border-strong:rgba(255,255,255,.2);--surface-1:#2c2c2a;--bg-warning:#412402;--text-warning:#fac775;--bg-accent:#042c53;--text-accent:#b5d4f4}}
body{margin:0 auto;max-width:980px;padding:24px 16px;font-family:'Segoe UI Variable Text','Segoe UI',sans-serif;background:Canvas}
.sr-only{position:absolute;width:1px;height:1px;overflow:hidden;clip:rect(0 0 0 0)}
</style></head><body>$body</body></html>
"@
Set-Content -Path $out -Value $page -Encoding utf8
if (-not $Hook) { "Wrote $out ($n items; $summary)" }
if (-not $NoOpen -and -not $Hook) { Invoke-Item $out }
