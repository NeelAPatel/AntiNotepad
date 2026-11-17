param(
    [string]$Item,
    [ValidateSet('blocked','needs-you','waiting','working','parked','standby')][string]$State = 'working',
    [switch]$Codex, [string]$Phase, [switch]$Merged, [switch]$Closed, [string]$Name, [switch]$Check
)
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$utf8 = [Text.UTF8Encoding]::new($false)
$root = Split-Path $PSScriptRoot -Parent
function Set-ComputedTitle([string]$Store, [string]$Key, [string]$Title) {
    [IO.Directory]::CreateDirectory($Store) | Out-Null
    $path = Join-Path $Store "$Key.txt"
    if ((Test-Path -LiteralPath $path) -and [IO.File]::ReadAllText($path, $utf8) -ceq $Title) { return }
    [IO.File]::WriteAllText($path, $Title, $utf8)
    "SET TITLE: $Title"
}
function Get-Title([string]$ItemName, [string]$TitleState, [bool]$HasCodex, [string]$TitlePhase) {
    $glyphs = @{ blocked='⛔'; 'needs-you'='❓'; waiting='⏳'; working='🔨'; parked='🅿️'; standby='💤' }
    $badge = if ($HasCodex) { '🤖' } else { '' }
    "$($glyphs[$TitleState])$badge $ItemName · $TitlePhase"
}
if ($Check) {
    $temp = Join-Path ([IO.Path]::GetTempPath()) ('chat-title-' + [guid]::NewGuid().ToString('N'))
    try {
        $expected = @{ blocked='⛔'; 'needs-you'='❓'; waiting='⏳'; working='🔨'; parked='🅿️'; standby='💤' }
        foreach ($s in $expected.Keys) {
            $title = Get-Title 'T1-example' $s $false 'build'
            if ($title -cne "$($expected[$s]) T1-example · build") { throw "state $s" }
            $line = Set-ComputedTitle $temp 'T1-example' $title
            if ($line -cne "SET TITLE: $title") { throw "store $s" }
            if (Set-ComputedTitle $temp 'T1-example' $title) { throw 'duplicate output' }
        }
        $title = Get-Title 'T1-example' 'waiting' $true 'build'
        if ($title -cne '⏳🤖 T1-example · build') { throw 'badge' }
        foreach ($title in @($title, '[Merged] T1-example', '[Closed] director')) {
            if ((Set-ComputedTitle $temp 'T1-example' $title) -cne "SET TITLE: $title") { throw 'form' }
        }
        $bytes = $utf8.GetBytes('⏳🤖 🅿️')
        if ($utf8.GetString($bytes) -cne '⏳🤖 🅿️' -or ($bytes[0] -eq 0xEF)) { throw 'UTF-8' }
        $bad = & pwsh -NoProfile -File $PSCommandPath -Item T1-example -State invalid 2>&1
        if ($LASTEXITCODE -eq 0) { throw 'bad state accepted' }
        'chat-title CHECK PASS'
    } finally { if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp -Recurse -Force } }
    exit 0
}
if ($Merged -and $Closed) { throw 'Choose -Merged or -Closed.' }
if (-not $Item -and -not $Closed) {
    $branch = [string](& git -C (Get-Location).Path rev-parse --abbrev-ref HEAD 2>$null)
    if ($LASTEXITCODE -eq 0 -and $branch -match '^[^/]+/([A-Z][A-Za-z0-9]*(-[a-z0-9]+)+)$') { $Item = $Matches[1] }
    else { exit 0 }
}
if ($Item -and $Item -cnotmatch '^[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+$') { throw 'Invalid item name.' }
if ($Closed -and [string]::IsNullOrWhiteSpace($Name)) { throw '-Closed requires -Name.' }
$common = [string](& git -C $root rev-parse --path-format=absolute --git-common-dir 2>$null)
if ($LASTEXITCODE -ne 0 -or -not $common) { throw 'Cannot resolve main checkout.' }
$main = Split-Path $common.Trim() -Parent
if ($Closed) {
    $title = "[Closed] $Name"
    if (-not $Item) { $Item = 'closed-' + [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($utf8.GetBytes($Name))) }
} elseif ($Merged) { $title = "[Merged] $Item" }
else {
    if (-not $Phase) {
        $card = Join-Path $root ".coord/tasks/$Item.md"
        if (-not (Test-Path -LiteralPath $card)) { $card = Join-Path $main ".coord/tasks/$Item.md" }
        $stateLine = if (Test-Path -LiteralPath $card) { (Get-Content -LiteralPath $card -Encoding utf8 | Where-Object { $_ -match '^State:\s*' } | Select-Object -First 1) } else { '' }
        $value = if ($stateLine) { $stateLine -replace '^State:\s*', '' } else { '' }
        $Phase = switch -Regex ($value) {
            '(?i)hand[ -]?test|accepted' { 'hand test'; break }
            '(?i)check|review' { 'check'; break }
            '(?i)approv|build|rework' { 'build'; break }
            '(?i)plan' { 'plan'; break }
            '(?i)findings|audit' { 'findings'; break }
            default { 'request' }
        }
        if ($value -match '(?i)\br((?:[2-9]|[1-9][0-9]+))\b') { $Phase += " · $($Matches[1].ToLowerInvariant())" }
    }
    $title = Get-Title $Item $State ([bool]$Codex) $Phase
}
Set-ComputedTitle (Join-Path $main '.coord/titles') $Item $title
