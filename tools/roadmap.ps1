# Renders the STATUS "Plan" section as the local roadmap page, .coord/roadmap.html, which reloads itself every few seconds
# when opened from disk. -Hook: Claude PostToolUse/Stop hook; regenerates the page silently. -Open: also opens it.
# The same page can still be published by hand as the roadmap Artifact (tools/roadmap.url).
# Acts only in the main checkout: worktrees carry a branch copy of STATUS that would overwrite the live plan.
param([switch]$Hook, [switch]$Open, [string]$Root = (Split-Path $PSScriptRoot -Parent))
$ErrorActionPreference = 'Stop'
$status = Join-Path $Root '.coord/STATUS.md'
$out = Join-Path $Root '.coord/roadmap.html'

if ($Hook) {
    try { $j = [Console]::In.ReadToEnd() | ConvertFrom-Json } catch { $j = $null }
    if ($Root -match '[\\/]\.claude[\\/]worktrees[\\/]' -or -not (Test-Path $status)) { exit 0 }
}

$lines = Get-Content $status -Encoding utf8
$updated = ($lines | Where-Object { $_ -match '^Updated: ' } | Select-Object -First 1) -replace '^Updated: ', ''
$start = ($lines | Select-String -Pattern '^## Plan' | Select-Object -First 1).LineNumber
if (-not $start) { if ($Hook) { exit 0 }; throw 'No "## Plan" section in STATUS.md' }

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
$n = 0; $parent = $null; $inIdea = $false; $stepNo = 0; $counts = @{}
foreach ($l in $lines[$start..($lines.Count - 1)]) {
    if ($l -match '^## ') { break }
    if ($l -match '^### (.+)$') { [void]$sb.Append("<h2 class=`"rm-p`">$(Esc $Matches[1])</h2>"); continue }
    if ($l -notmatch '^\|' -or $l -match '^\|[\s|:-]+\|\s*$') { continue }
    $c = $l.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() }
    if ($c[0] -eq 'Item') { continue }
    if ($c[0] -match '^\*\*(.+)\*\*$') { [void]$sb.Append("<h3 class=`"rm-g`">$(Esc $Matches[1])</h3>"); continue }
    $item, $scope, $tier, $effort, $state = $c[0], $c[1], $c[2], $c[3], $c[4]
    $cls = StateClass $state
    $counts[$state] = 1 + [int]$counts[$state]
    if ($item -match '^Release marker') {
        [void]$sb.Append("<div class=`"rm-m rm-$cls`"><span class=`"rm-ml`">Release marker</span><span>$(Esc $scope)</span><span class=`"rm-s`">$(Esc $state)</span></div>")
        continue
    }
    # Steps sit right under their idea, indented, numbered by letter: A53a-... under A53-..., or "step: x" under "idea: y".
    $step = ($parent -and $item -match "^$([regex]::Escape($parent))([a-z])-") -or ($inIdea -and $item -match '^step: ')
    if ($step) { $stepNo++; $num = "$n$([char](96 + $stepNo))"; $cls += ' rm-sub' }
    else {
        $n++; $num = "$n"; $stepNo = 0
        $parent = if ($item -match '^([A-Z]\d+)-') { $Matches[1] } else { $null }
        $inIdea = $item -match '^idea: '
    }
    $te = if ($tier -eq '-' -or -not $tier) { '' } else { "$(Esc $tier) · $(Esc $effort)" }
    $name = if ($item -match '^Decision: (.+)$') { "<span class=`"rm-d`">Decision</span> $(Esc $Matches[1])" }
        elseif ($item -match '^(idea|step): (.+)$') { "<span class=`"rm-i`">$($Matches[1]) · no ID</span> $(Esc $Matches[2])" }
        else { "<code>$(Esc $item)</code>" }
    [void]$sb.Append("<div class=`"rm-r rm-$cls`"><span class=`"rm-n`">$num</span><span class=`"rm-it`">$name</span><span class=`"rm-x`">$(Esc $scope)</span><span class=`"rm-t`">$te</span><span class=`"rm-st`"><span class=`"rm-s`">$(Esc $state)</span></span></div>")
}

$chips = ($counts.GetEnumerator() | Sort-Object Name | ForEach-Object {
    "<span class=`"rm-c rm-$(StateClass $_.Name)`"><b>$($_.Value)</b> $(Esc $_.Name)</span>" }) -join ''
$page = @"
<title>AntiNotepad Roadmap</title>
<meta name="claude:viewport-fit-sides" content="cover">
<script>
// Opened from disk: reload every 3 s to pick up hook rewrites, keeping the scroll position.
if (location.protocol === 'file:') {
  addEventListener('load', () => { try { scrollTo(0, +sessionStorage.getItem('rmY') || 0) } catch {} })
  setInterval(() => { try { sessionStorage.setItem('rmY', scrollY) } catch {}; location.reload() }, 3000)
}
</script>
<style>
/* One long ledger: priority headings, block labels, then rows (#, item, scope, tier, state); stacks to two lines on phones. */
:root{--bg:#fbfaf8;--fg:#1f1e1d;--fg2:#5f5e5a;--muted:#9a9893;--line:rgba(40,30,20,.1);--line2:rgba(40,30,20,.22);
--chip:#f1efe8;--warn-bg:#faeeda;--warn:#854f0b;--acc-bg:#e6f1fb;--acc:#185fa5;
--sans:'Segoe UI Variable Text','Segoe UI',system-ui,sans-serif;--mono:'Cascadia Mono',Consolas,ui-monospace,monospace}
@media (prefers-color-scheme:dark){:root:not([data-theme="light"]){--bg:#1c1b1a;--fg:#ecebe6;--fg2:#b4b2a9;--muted:#77756f;
--line:rgba(255,255,255,.1);--line2:rgba(255,255,255,.22);--chip:#2c2c2a;--warn-bg:#412402;--warn:#fac775;--acc-bg:#042c53;--acc:#b5d4f4;color-scheme:dark}}
:root[data-theme="dark"]{--bg:#1c1b1a;--fg:#ecebe6;--fg2:#b4b2a9;--muted:#77756f;--line:rgba(255,255,255,.1);
--line2:rgba(255,255,255,.22);--chip:#2c2c2a;--warn-bg:#412402;--warn:#fac775;--acc-bg:#042c53;--acc:#b5d4f4;color-scheme:dark}
body{background:var(--bg);color:var(--fg);font-family:var(--sans);font-size:13px}
.rm{max-width:1000px;margin:0 auto;padding-block:24px 40px;padding-left:max(16px,env(safe-area-inset-left,0px));padding-right:max(16px,env(safe-area-inset-right,0px))}
h1{font-size:20px;font-weight:600;margin:0;text-wrap:balance}
.rm-meta{color:var(--fg2);font-size:12px;margin:4px 0 12px}
.rm-cs{display:flex;flex-wrap:wrap;gap:6px;margin-bottom:8px}
.rm-c{font-size:12px;padding:2px 10px;border-radius:10px;background:var(--chip);color:var(--fg2);font-variant-numeric:tabular-nums}
.rm-c b{color:var(--fg);font-weight:600}
.rm-r{display:grid;grid-template-columns:32px minmax(0,1.25fr) minmax(0,2fr) 96px 96px;gap:8px;padding:6px 4px;border-bottom:1px solid var(--line);align-items:baseline}
.rm-h{color:var(--fg2);font-size:12px;position:sticky;top:env(safe-area-inset-top,0px);background:var(--bg)}
.rm-n{font-variant-numeric:tabular-nums;color:var(--fg2)}
.rm code{font-family:var(--mono);font-size:12px;overflow-wrap:anywhere}
.rm-x{color:var(--fg2);min-width:0;overflow-wrap:anywhere}.rm-t{color:var(--fg2);font-size:12px}
.rm-p{font-size:15px;font-weight:600;margin:0;padding:18px 4px 6px;border-bottom:1px solid var(--line2)}
.rm-g{font-size:13px;font-weight:600;color:var(--fg2);margin:0;padding:12px 4px 4px}
.rm-m{display:grid;grid-template-columns:120px minmax(0,1fr) auto;gap:8px;padding:6px 8px;margin:6px 0;font-size:12px;border-radius:8px;background:var(--warn-bg);color:var(--warn);align-items:baseline}
.rm-ml{font-weight:600}
.rm-s{display:inline-block;font-size:11px;padding:1px 8px;border-radius:8px;background:var(--chip);color:var(--fg2)}
.rm-d{font-size:11px;padding:1px 6px;border-radius:8px;background:var(--acc-bg);color:var(--acc)}
.rm-i{font-size:11px;padding:1px 6px;border-radius:8px;background:var(--chip);color:var(--muted)}
.rm-you .rm-s,.rm-c.rm-you{background:var(--warn-bg);color:var(--warn)}.rm-live .rm-s,.rm-c.rm-live{background:var(--acc-bg);color:var(--acc)}
.rm-c.rm-you b,.rm-c.rm-live b{color:inherit}
.rm-sub .rm-it{padding-left:16px}.rm-sub{font-size:12px}
.rm-r.rm-done{color:var(--muted)}.rm-r.rm-done code,.rm-r.rm-done .rm-x{color:var(--muted);text-decoration:line-through}
.rm-foot{color:var(--fg2);font-size:12px;padding:12px 4px}
@media (max-width:640px){
.rm-h{display:none}
.rm-r{grid-template-columns:32px minmax(0,1fr) auto;grid-template-areas:"n it st" ". x x" ". t t";row-gap:2px}
.rm-n{grid-area:n}.rm-it{grid-area:it;min-width:0}.rm-st{grid-area:st}.rm-x{grid-area:x}.rm-t{grid-area:t}.rm-t:empty{display:none}
.rm-m{grid-template-columns:1fr auto}.rm-ml{grid-column:1/-1}}
</style>
<main class="rm">
<h1>AntiNotepad roadmap</h1>
<p class="rm-meta">Plan from <code>.coord/STATUS.md</code>, in run order · STATUS updated $(Esc $updated)</p>
<div class="rm-cs">$chips</div>
<div class="rm-r rm-h" aria-hidden="true"><span>#</span><span>Item</span><span>Scope</span><span>Tier · effort</span><span>State</span></div>
$($sb.ToString())
<p class="rm-foot">Rebuilt by a Claude hook whenever STATUS changes. Rows are numbered by position; IDs are the item names.</p>
</main>
"@

Set-Content -Path $out -Value $page -Encoding utf8 -NoNewline
if ($Hook) { exit 0 }
"Wrote $out ($n items; $(($counts.GetEnumerator() | Sort-Object Name | ForEach-Object { "$($_.Value) $($_.Name)" }) -join ', '))"
if ($Open) { Invoke-Item $out }
