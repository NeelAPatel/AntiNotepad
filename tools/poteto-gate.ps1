# Checks director briefs and the first turn of started item chats.
[CmdletBinding()]
param(
    [ValidateSet('Brief', 'Stop')][string]$Mode,
    [switch]$LogOnly,
    [switch]$SelfTest
)
$ErrorActionPreference = 'Stop'
$command = '/pstack:poteto-mode'
$fallback = "Run this command to start poteto mode, then wait for it to load:`n``````text`n/pstack:poteto-mode`n``````"
$briefReason = 'Use the item chat template: /pstack:poteto-mode on line 1 and the explicit Skill tool call on line 2.'

function Write-GateLog([string]$Message) {
    $root = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { Split-Path -Parent $PSScriptRoot }
    $folder = Join-Path $root 'build'
    [void](New-Item -ItemType Directory -Path $folder -Force)
    [IO.File]::AppendAllText((Join-Path $folder 'poteto-gate.log'), "$(Get-Date -Format o)`t$Mode`t$Message`n")
}

function Get-Text($Content) {
    if ($Content -is [string]) { return $Content }
    return ((@($Content) | Where-Object { $_.type -eq 'text' } | ForEach-Object { $_.text }) -join "`n")
}

function Get-Verdict($Hook) {
    if ($Mode -eq 'Brief') {
        if ($null -eq $Hook.tool_input -or $null -eq $Hook.tool_input.prompt) { throw 'Missing prompt' }
        $prompt = [string]$Hook.tool_input.prompt
        if ($prompt.IndexOf('You are the item chat', [StringComparison]::OrdinalIgnoreCase) -lt 0) { return 'unrelated' }
        $lines = $prompt -split "`r?`n"
        if ($lines.Count -lt 2 -or $lines[0] -cne $command -or -not $lines[1].StartsWith('Your first action is a Skill tool call', [StringComparison]::Ordinal)) { return 'bad-brief' }
        if ($prompt -match '(?i)you are the item chat for\s+`?(?<id>[A-Z][A-Za-z0-9]*(?:-[a-z0-9]+)+)`?') {
            $id = $Matches.id
            $project = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { Split-Path -Parent $PSScriptRoot }
            $common = & git -C $project rev-parse --path-format=absolute --git-common-dir 2>$null
            $root = if ($LASTEXITCODE -eq 0 -and $common) { Split-Path -Parent $common.Trim() } else { $project }
            $found = $false
            foreach ($file in @('.coord/STATUS.md', '.coord/pending.md')) {
                $path = Join-Path $root $file
                if (Test-Path -LiteralPath $path -PathType Leaf) {
                    foreach ($line in [IO.File]::ReadLines($path)) {
                        if ($line -match ('^\|\s*' + [regex]::Escape($id) + '\s*\|')) { $found = $true; break }
                    }
                }
            }
            if (-not $found) { return "missing-row:$id" }
        }
        return 'valid-brief'
    }
    if ($Mode -ne 'Stop') { throw 'Mode is required' }
    if ($Hook.stop_hook_active -eq $true) { return 'active' }
    if (-not $Hook.transcript_path) { throw 'Missing transcript path' }
    $firstUser = $false; $item = $false; $success = $false; $launched = $false
    foreach ($line in [IO.File]::ReadLines([string]$Hook.transcript_path)) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $entry = $line | ConvertFrom-Json
        if ($entry.type -eq 'user' -and $entry.isMeta -eq $true) { continue }
        if ($entry.type -eq 'user') {
            $content = $entry.message.content
            # Tool results use the user role but are not a new user turn.
            if ($content -isnot [string] -and @($content | Where-Object { $_.type -eq 'tool_result' }).Count -gt 0) {
                foreach ($result in @($content | Where-Object { $_.type -eq 'tool_result' })) {
                    if ((Get-Text $result.content).Contains('Launching skill: pstack:poteto-mode')) { $launched = $true }
                }
                continue
            }
            if ($firstUser) { return 'past-first-turn' }
            $firstUser = $true
            $userText = Get-Text $content
            $item = $userText -cmatch '(?m)^Task:\s*\r?\n/pstack:poteto-mode\r?$'
            if (-not $item) { return 'unrelated' }
        }
        elseif ($firstUser -and $entry.type -eq 'assistant') {
            foreach ($block in @($entry.message.content)) {
                if ($block.type -eq 'tool_use' -and $block.name -ceq 'Skill' -and $block.input.skill -ceq 'pstack:poteto-mode') { $success = $true }
                if ($block.type -eq 'text' -and [string]$block.text -cmatch '(?ms)^ {0,3}(`{3,}|~{3,})[^\r\n]*\r?\n(?:(?!\1).)*?^/pstack:poteto-mode(?:[ \t\r]|$)(?:(?!\1).)*?^ {0,3}\1[ \t]*\r?$') { $success = $true }
            }
        }
    }
    if (-not $firstUser) { throw 'No user message in transcript' }
    if ($success -and $launched) { return 'first-turn-pass: Launching skill: pstack:poteto-mode' }
    if ($success) { return 'first-turn-pass' }
    return 'first-turn-missing'
}

function Invoke-SelfTest {
    $root = Join-Path ([IO.Path]::GetTempPath()) ('poteto-' + [guid]::NewGuid().ToString('N'))
    $oldRoot = $env:CLAUDE_PROJECT_DIR
    try {
        [void](New-Item -ItemType Directory -Path $root -Force)
        $env:CLAUDE_PROJECT_DIR = $root
        $shell = (Get-Process -Id $PID).Path
        $transcript = Join-Path $root 'transcript.jsonl'
        $user = @{ type='user'; message=@{ content="wrapper`nTask:`n/pstack:poteto-mode`nYou are the item chat" } }
        $skill = @{ type='assistant'; message=@{ content=@(@{ type='tool_use'; name='Skill'; input=@{ skill='pstack:poteto-mode' } }) } }
        $fenced = @{ type='assistant'; message=@{ content=@(@{ type='text'; text="Please run:`n``````text`n/pstack:poteto-mode Loop16: work the brief`n``````" }) } }
        $plain = @{ type='assistant'; message=@{ content=@(@{ type='text'; text='I will begin.' }) } }
        $second = @{ type='user'; message=@{ content='next turn' } }
        function Check([string]$Name, [string]$TestMode, [string]$InputJson, [int]$Expected, [string]$ExpectedText = '', [bool]$TestLogOnly = $false) {
            $arguments = @('-NoProfile', '-File', $PSCommandPath, '-Mode', $TestMode)
            if ($TestLogOnly) { $arguments += '-LogOnly' }
            $previous = $ErrorActionPreference; $ErrorActionPreference = 'Continue'
            try { $output = $InputJson | & $shell @arguments 2>&1; $code = $LASTEXITCODE } finally { $ErrorActionPreference = $previous }
            if ($code -ne $Expected -or ($ExpectedText -and ($output -join "`n") -notmatch $ExpectedText)) { throw "Selftest failed: $Name (exit $code): $output" }
        }
        function Json($Value) { return ($Value | ConvertTo-Json -Depth 20 -Compress) }
        $valid = "/pstack:poteto-mode`nYour first action is a Skill tool call with skill pstack:poteto-mode`nYou are the item chat"
        Check 'valid brief' Brief (Json @{tool_input=@{prompt=$valid}}) 0
        $named = $valid + ' for `Loop21-test-item`'
        Check 'missing row' Brief (Json @{tool_input=@{prompt=$named}}) 2 'Loop21-test-item'
        Check 'missing row log-only' Brief (Json @{tool_input=@{prompt=$named}}) 0 '' $true
        [void](New-Item -ItemType Directory -Path (Join-Path $root '.coord') -Force)
        [IO.File]::WriteAllText((Join-Path $root '.coord/pending.md'), "| Loop21-test-item | scope | Low | S | request |`n")
        Check 'existing row' Brief (Json @{tool_input=@{prompt=$named}}) 0
        Check 'bad first line' Brief (Json @{tool_input=@{prompt="prefix`n$valid"}}) 2 'item chat template'
        Check 'lowercase marker with bad first line' Brief (Json @{tool_input=@{prompt="you are the item chat for Loop16-poteto-trigger; begin Findings"}}) 2 'item chat template'
        Check 'bad second line' Brief (Json @{tool_input=@{prompt="/pstack:poteto-mode`nwrong`nYou are the item chat"}}) 2 'item chat template'
        Check 'unrelated brief' Brief (Json @{tool_input=@{prompt='hello'}}) 0
        Check 'log-only brief' Brief (Json @{tool_input=@{prompt="wrong`nYou are the item chat"}}) 0 '' $true
        Check 'garbage' Brief '{' 0
        $stop = Json @{ transcript_path=$transcript; stop_hook_active=$false }
        [IO.File]::WriteAllLines($transcript, @((Json $user), (Json $skill)))
        Check 'skill' Stop $stop 0
        [IO.File]::WriteAllLines($transcript, @((Json $user), (Json $fenced)))
        Check 'fenced fallback' Stop $stop 0
        $fencedSuffix = @{ type='assistant'; message=@{ content=@(@{ type='text'; text="Please run:`n``````text`n/pstack:poteto-mode-unavailable`n``````" }) } }
        [IO.File]::WriteAllLines($transcript, @((Json $user), (Json $fencedSuffix)))
        Check 'fenced command suffix' Stop $stop 2 '/pstack:poteto-mode'
        $fencedBare = @{ type='assistant'; message=@{ content=@(@{ type='text'; text="Please run:`r`n``````text`r`n/pstack:poteto-mode`r`n``````" }) } }
        [IO.File]::WriteAllLines($transcript, @((Json $user), (Json $fencedBare)))
        Check 'fenced bare command with CRLF' Stop $stop 0
        [IO.File]::WriteAllLines($transcript, @((Json $user), (Json $plain)))
        Check 'missing first turn' Stop $stop 2 '/pstack:poteto-mode'
        [IO.File]::WriteAllLines($transcript, @((Json $user), (Json $plain), (Json $second), (Json $skill)))
        Check 'later turn' Stop $stop 0
        $meta = @{ type='user'; isMeta=$true; message=@{ content=@(@{ type='text'; text='Base directory for this skill' }) } }
        [IO.File]::WriteAllLines($transcript, @((Json $user), (Json $skill), (Json $meta), (Json $plain)))
        Check 'skill text is not a turn' Stop $stop 0
        [IO.File]::WriteAllLines($transcript, @((Json $user), (Json $meta), (Json $plain)))
        Check 'meta does not hide a missing launch' Stop $stop 2 '/pstack:poteto-mode'
        Check 'active guard' Stop (Json @{ transcript_path=$transcript; stop_hook_active=$true }) 0
        Check 'unreadable transcript' Stop (Json @{ transcript_path=(Join-Path $root 'missing'); stop_hook_active=$false }) 0
        [IO.File]::WriteAllLines($transcript, @((Json @{type='user';message=@{content='hello'}}), (Json $plain)))
        Check 'unrelated chat' Stop $stop 0
        Write-Output 'POTETO GATE SELFTEST PASS'
    }
    finally { $env:CLAUDE_PROJECT_DIR = $oldRoot; Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue }
}

if ($SelfTest) {
    try { Invoke-SelfTest; exit 0 } catch { [Console]::Error.WriteLine($_.Exception.Message); exit 1 }
}
try {
    $hook = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $verdict = Get-Verdict $hook
    try { Write-GateLog "$verdict$(if ($LogOnly) { ' (log-only)' })" } catch { }
    if (-not $LogOnly -and $verdict -eq 'bad-brief') { [Console]::Error.WriteLine($briefReason); exit 2 }
    if (-not $LogOnly -and $verdict -like 'missing-row:*') { [Console]::Error.WriteLine("Missing STATUS or pending row for $($verdict.Substring(12)); add it before dispatch."); exit 2 }
    if (-not $LogOnly -and $verdict -eq 'first-turn-missing') { [Console]::Error.WriteLine($fallback); exit 2 }
    exit 0
}
catch {
    try { Write-GateLog "error: $($_.Exception.Message -replace '[\r\n]+', ' ')" } catch { }
    exit 0
}
