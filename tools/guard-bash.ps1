# Evaluates Bash hook commands against the merge and installed-copy walls.
# The hook starts in log mode so its decisions can be reviewed before enforcement.
[CmdletBinding()]
param([switch]$SelfTest)

$ErrorActionPreference = 'Stop'

# Split shell words and command separators without treating quoted text as commands.
function Get-CommandParts([string]$Text) {
    $parts = New-Object System.Collections.ArrayList
    $words = New-Object System.Collections.ArrayList
    $word = New-Object System.Text.StringBuilder
    $quote = [char]0
    $started = $false
    $heredocs = New-Object System.Collections.ArrayList
    $lines = $Text -split "`n", 0, 'SimpleMatch'
    $bodyEnd = $null
    foreach ($line in $lines) {
        $line = $line.TrimEnd("`r")
        if ($null -ne $bodyEnd) {
            if ($line -ceq $bodyEnd) { $bodyEnd = $null }
            continue
        }
        for ($i = 0; $i -lt $line.Length; $i++) {
            $c = $line[$i]
            if ($quote -ne [char]0) {
                if ($c -eq $quote) { $quote = [char]0 }
                elseif ($c -eq '\' -and $quote -eq '"' -and $i + 1 -lt $line.Length -and $line[$i + 1] -in @('"', '\', '$', '`')) { $i++; [void]$word.Append($line[$i]) }
                else { [void]$word.Append($c) }
                continue
            }
            if ($c -eq "'" -or $c -eq '"') { $quote = $c; $started = $true; continue }
            if ($c -eq '\' -and $i + 1 -lt $line.Length) { $i++; [void]$word.Append($line[$i]); $started = $true; continue }
            if ($c -eq '<' -and $i + 1 -lt $line.Length -and $line[$i + 1] -eq '<') {
                $tail = $line.Substring($i)
                if ($tail -notmatch '^<<-?\s*[''""]?([A-Za-z_][A-Za-z0-9_]*)[''""]?') { throw 'Unparseable heredoc' }
                [void]$heredocs.Add($Matches[1]); $i += $Matches[0].Length - 1; continue
            }
            if ([char]::IsWhiteSpace($c) -or $c -eq ';' -or $c -eq '|' -or ($c -eq '&' -and $i + 1 -lt $line.Length -and $line[$i + 1] -eq '&')) {
                if ($started) { [void]$words.Add($word.ToString()); [void]$word.Clear(); $started = $false }
                if (-not [char]::IsWhiteSpace($c)) {
                    if ($words.Count) { [void]$parts.Add(@($words.ToArray())); $words.Clear() }
                    if ($i + 1 -lt $line.Length -and $line[$i + 1] -eq $c -and ($c -eq '|' -or $c -eq '&')) { $i++ }
                }
                continue
            }
            [void]$word.Append($c); $started = $true
        }
        if ($quote -ne [char]0) { throw 'Unterminated quote' }
        if ($started) { [void]$words.Add($word.ToString()); [void]$word.Clear(); $started = $false }
        if ($words.Count) { [void]$parts.Add(@($words.ToArray())); $words.Clear() }
        if ($heredocs.Count -gt 1) { throw 'Multiple heredocs are not supported' }
        if ($heredocs.Count) { $bodyEnd = $heredocs[0]; $heredocs.Clear() }
    }
    if ($null -ne $bodyEnd) { throw 'Unterminated heredoc' }
    return ,$parts.ToArray()
}

# Classify the effective checkout, resolving Git's relative directory paths.
function Get-GitContext([string]$Directory) {
    $gitDir = & git -C $Directory rev-parse --absolute-git-dir 2>$null
    if ($LASTEXITCODE -ne 0) { return 'other' }
    $common = & git -C $Directory rev-parse --git-common-dir 2>$null
    if ($LASTEXITCODE -ne 0) { return 'other' }
    if (-not [IO.Path]::IsPathRooted($common)) { $common = Join-Path $Directory $common }
    $common = [IO.Path]::GetFullPath($common)
    $gitDir = [IO.Path]::GetFullPath($gitDir)
    if (-not [string]::Equals($gitDir.TrimEnd('\', '/'), $common.TrimEnd('\', '/'), [StringComparison]::OrdinalIgnoreCase)) { return 'worktree' }
    $branch = & git -C $Directory symbolic-ref --quiet --short HEAD 2>$null
    if ($LASTEXITCODE -eq 0 -and $branch -ceq 'dev') { return 'main-dev' }
    return 'other'
}

# Return a verdict without executing the command; auto context resolves each git -C separately.
function Get-GuardVerdict([string]$Command, [string]$Context, [bool]$HasData, [string]$Cwd = '.') {
    $parts = Get-CommandParts $Command
    if (-not $HasData -and $Command -notmatch 'ANTINOTEPAD_DATA' -and $Command -match '(?i)--quit|Programs[\\/]AntiNotepad|%LOCALAPPDATA%[\\/]AntiNotepad') {
        return @{ Guard = 'installed-copy'; Verdict = 'BLOCK'; Reason = 'Installed-copy command requires ANTINOTEPAD_DATA.' }
    }
    $unknown = $false
    foreach ($part in $parts) {
        $tokens = @($part); $i = 0; $dir = $Cwd
        while ($i -lt $tokens.Count -and $tokens[$i] -match '^[A-Za-z_][A-Za-z0-9_]*=') { $i++ }
        if ($i -ge $tokens.Count -or $tokens[$i] -notmatch '(?i)^(git|git\.exe)$') { continue }
        $i++
        while ($i -lt $tokens.Count) {
            if ($tokens[$i] -ceq '-C' -or $tokens[$i] -ceq '-c') {
                $option = $tokens[$i]; $i++
                if ($i -ge $tokens.Count) { throw 'Missing git option value' }
                if ($option -ceq '-C') {
                    $nextDir = $tokens[$i]
                    if (-not [IO.Path]::IsPathRooted($nextDir)) { $nextDir = Join-Path $dir $nextDir }
                    $dir = [IO.Path]::GetFullPath($nextDir)
                }
                $i++; continue
            }
            break
        }
        if ($i -ge $tokens.Count) { throw 'Missing git subcommand' }
        $sub = $tokens[$i]; $i++; $argsLeft = @($tokens | Select-Object -Skip $i)
        $where = $Context
        if ($where -eq 'auto') { $where = Get-GitContext $dir }
        if ($where -eq 'other') { $unknown = $true; continue }
        $reason = $null
        if ($where -eq 'worktree' -and $sub -eq 'merge') { $reason = 'Merging in an item worktree is blocked.' }
        if ($where -eq 'worktree' -and $sub -in @('checkout', 'switch')) {
            foreach ($arg in $argsLeft) { if ($arg -match '^(refs/heads/)?(main|dev)$') { $reason = 'Switching to main or dev in an item worktree is blocked.' } }
        }
        if ($where -eq 'main-dev' -and $sub -eq 'rebase') { $reason = 'Rebasing the main dev checkout is blocked.' }
        if ($sub -eq 'push') {
            foreach ($arg in $argsLeft) {
                if ($where -eq 'main-dev' -and ($arg -match '^--force($|=|-with-lease)' -or $arg -match '^-[^-]*f' -or $arg.StartsWith('+'))) { $reason = 'Force pushing from the main dev checkout is blocked.' }
                if ($where -eq 'worktree' -and $arg -in @('--all', '--mirror', '--branches')) { $reason = 'Pushing all branches from an item worktree is blocked.' }
                $target = ($arg -replace '^\+', '') -replace '^.*:', '' -replace '^refs/heads/', ''
                if (($where -eq 'worktree' -and $target -in @('dev', 'main')) -or ($where -eq 'main-dev' -and $target -eq 'main')) { $reason = 'Pushing a protected branch is blocked.' }
            }
        }
        if ($reason) { return @{ Guard = 'merge'; Verdict = 'BLOCK'; Reason = $reason } }
    }
    if ($unknown) { return @{ Guard = 'unknown context'; Verdict = 'ALLOW'; Reason = 'unknown context' } }
    return @{ Guard = ''; Verdict = 'ALLOW'; Reason = '' }
}

# Append a single line even when the command itself contains line breaks or tabs.
function Write-GuardLog([string]$Root, [string]$Mode, [string]$Guard, [string]$Verdict, [string]$Cwd, [string]$Command) {
    $folder = Join-Path $Root 'build'
    [void](New-Item -ItemType Directory -Path $folder -Force)
    $safeCwd = $Cwd -replace '[\r\n\t]+', ' '
    $safeCommand = $Command -replace '[\r\n\t]+', ' '
    $line = '{0}`t{1}`t{2}`t{3}`t{4}`t{5}' -f (Get-Date -Format 'o'), $Mode, $Guard, $Verdict, $safeCwd, $safeCommand
    [IO.File]::AppendAllText((Join-Path $folder 'guard.log'), ($line.Replace('`t', "`t") + [Environment]::NewLine))
}

# Exercise policy decisions and the external mode and exit-code contract.
function Invoke-GuardSelfTest {
    $cases = @(
        @('lease', 'git push --force-with-lease origin chore/X', 'worktree', $false, 'ALLOW'),
        @('bare push', 'git push', 'worktree', $false, 'ALLOW'),
        @('rebase', 'git rebase dev', 'worktree', $false, 'ALLOW'),
        @('commit', 'git commit', 'worktree', $false, 'ALLOW'),
        @('fetch', 'git fetch', 'worktree', $false, 'ALLOW'),
        @('target dev', 'git -C . push origin HEAD:dev', 'worktree', $false, 'BLOCK'),
        @('target main', 'git push origin HEAD:refs/heads/main', 'worktree', $false, 'BLOCK'),
        @('all', 'git push --all', 'worktree', $false, 'BLOCK'),
        @('mirror', 'git push --mirror', 'worktree', $false, 'BLOCK'),
        @('delete', 'git push origin --delete dev', 'worktree', $false, 'BLOCK'),
        @('delete ref', 'git push origin :dev', 'worktree', $false, 'BLOCK'),
        @('merge', 'git merge x', 'worktree', $false, 'BLOCK'),
        @('chain', 'a && git merge x', 'worktree', $false, 'BLOCK'),
        @('switch', 'git switch main', 'worktree', $false, 'BLOCK'),
        @('checkout', 'git checkout dev', 'worktree', $false, 'BLOCK'),
        @('merge dev', 'git merge --no-ff --no-commit b', 'main-dev', $false, 'ALLOW'),
        @('abort', 'git merge --abort', 'main-dev', $false, 'ALLOW'),
        @('push dev', 'git push origin dev', 'main-dev', $false, 'ALLOW'),
        @('delete item', 'git push origin --delete chore/X', 'main-dev', $false, 'ALLOW'),
        @('branch delete', 'git branch -d chore/X', 'main-dev', $false, 'ALLOW'),
        @('rebase dev', 'git rebase dev', 'main-dev', $false, 'BLOCK'),
        @('force', 'git push --force origin dev', 'main-dev', $false, 'BLOCK'),
        @('force short', 'git push -f origin dev', 'main-dev', $false, 'BLOCK'),
        @('force lease', 'git push --force-with-lease origin dev', 'main-dev', $false, 'BLOCK'),
        @('force ref', 'git push origin +dev', 'main-dev', $false, 'BLOCK'),
        @('push main', 'git push origin main', 'main-dev', $false, 'BLOCK'),
        @('quit', 'echo --quit', 'other', $false, 'BLOCK'),
        @('quit data', 'echo --quit', 'other', $true, 'ALLOW'),
        @('path', 'echo Programs/AntiNotepad', 'other', $false, 'BLOCK'),
        @('path data', 'echo Programs\AntiNotepad', 'other', $true, 'ALLOW'),
        @('local path', 'echo %LOCALAPPDATA%/AntiNotepad', 'other', $false, 'BLOCK'),
        @('echo', 'echo hi', 'worktree', $false, 'ALLOW'),
        @('quoted', 'echo "git merge x"', 'worktree', $false, 'ALLOW'),
        @('heredoc', "cat <<EOF`ngit merge x`nEOF", 'worktree', $false, 'ALLOW')
    )
    foreach ($case in $cases) {
        $result = Get-GuardVerdict -Command $case[1] -Context $case[2] -HasData $case[3]
        if ($result.Verdict -ne $case[4]) { throw "Selftest failed: $($case[0])" }
    }
    $root = Join-Path ([IO.Path]::GetTempPath()) ('guard-' + [guid]::NewGuid().ToString('N'))
    try {
        [void](New-Item -ItemType Directory -Path (Join-Path $root 'tools') -Force)
        $oldRoot = $env:CLAUDE_PROJECT_DIR; $oldData = $env:ANTINOTEPAD_DATA
        $env:CLAUDE_PROJECT_DIR = $root; $env:ANTINOTEPAD_DATA = ''
        $json = @{ cwd = $PSScriptRoot; tool_name = 'Bash'; tool_input = @{ command = 'echo --quit' } } | ConvertTo-Json -Compress
        $shell = (Get-Process -Id $PID).Path
        [IO.File]::WriteAllText((Join-Path $root 'tools/guard.mode'), 'log')
        $output = $json | & $shell -NoProfile -File $PSCommandPath 2>&1
        $logPath = Join-Path $root 'build/guard.log'
        if ($LASTEXITCODE -ne 0 -or $output -or -not (Test-Path $logPath) -or (Get-Content $logPath -Raw) -notmatch "`tlog`tinstalled-copy`tBLOCK`t") { throw 'Selftest failed: log mode' }
        [IO.File]::WriteAllText((Join-Path $root 'tools/guard.mode'), 'block')
        $ErrorActionPreference = 'Continue'
        $output = $json | & $shell -NoProfile -File $PSCommandPath 2>&1
        $ErrorActionPreference = 'Stop'
        if ($LASTEXITCODE -ne 2 -or -not $output) { throw 'Selftest failed: block mode' }
    }
    finally {
        $env:CLAUDE_PROJECT_DIR = $oldRoot; $env:ANTINOTEPAD_DATA = $oldData
        Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue
    }
    Write-Output 'GUARD SELFTEST PASS'
}

if ($SelfTest) {
    try { Invoke-GuardSelfTest; exit 0 } catch { [Console]::Error.WriteLine($_.Exception.Message); exit 1 }
}

$root = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { Split-Path -Parent $PSScriptRoot }
$mode = 'log'; $cwd = ''; $command = ''
try {
    $modePath = Join-Path $root 'tools/guard.mode'
    if (Test-Path -LiteralPath $modePath) {
        $first = (([IO.File]::ReadAllText($modePath)).Trim() -split '\s+')[0]
        if ($first -ceq 'block') { $mode = 'block' }
    }
    $inputText = [Console]::In.ReadToEnd()
    $hook = $inputText | ConvertFrom-Json
    if (-not $hook -or -not $hook.cwd -or -not $hook.tool_input -or $null -eq $hook.tool_input.command -or -not $hook.tool_name) { throw 'Invalid hook input' }
    $cwd = [string]$hook.cwd; $command = [string]$hook.tool_input.command
    $result = Get-GuardVerdict -Command $command -Context 'auto' -HasData ([bool]$env:ANTINOTEPAD_DATA) -Cwd $cwd
    if ($result.Guard) { Write-GuardLog $root $mode $result.Guard $result.Verdict $cwd $command }
    if ($mode -eq 'block' -and $result.Verdict -eq 'BLOCK') { [Console]::Error.WriteLine($result.Reason); exit 2 }
    exit 0
}
catch {
    try { Write-GuardLog $root $mode 'guard-error' 'ALLOW' $cwd $command } catch { }
    exit 0
}
