# Checks live loop instructions and their file references without changing the checkout.
[CmdletBinding()]
param([string]$Root = (Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Reports once per rule per line; exceptions require an exact path and rule plus literal line text.
function Write-Problem([string]$Path, [int]$Number, [string]$Rule, [string]$Line) {
    foreach ($entry in $script:exceptions) {
        if ($entry[0] -ceq $Path -and $entry[1] -ceq $Rule -and $Line.Contains($entry[2])) { return }
    }
    $script:problems++
    Write-Output "${Path}:${Number}: ${Rule}: $Line"
}

try {
    $Root = (Resolve-Path -LiteralPath $Root).Path
    if (-not [IO.Directory]::Exists($Root)) { throw "Root must be an existing directory: $Root" }
    $patterns = @(
        'AGENTS.md', 'docs/loop.md', 'docs/glossary.md', 'docs/agents.md', 'docs/coding.md', 'docs/dev-loop.md',
        '.claude/skills/*/SKILL.md', '.claude/agents/*.md', '.coord/templates/*.md', '.agents/skills/*/SKILL.md',
        '.codex/agents/*.toml', 'tools/*.ps1', 'tools/*.sh', '.coord/tools/*.sh'
    )
    $escapedRoot = [Management.Automation.WildcardPattern]::Escape($Root)
    $files = @($patterns | ForEach-Object { Get-ChildItem -Path (Join-Path $escapedRoot $_) -File -ErrorAction SilentlyContinue } |
        Sort-Object FullName -Unique)
    if ($files.Count -eq 0) { throw "No live loop files found under: $Root" }
    $script:exceptions = @()
    $allowPath = Join-Path $Root 'tools/loop-lint.allow'
    if (Test-Path -LiteralPath $allowPath -PathType Leaf) {
        $script:exceptions = @(foreach ($line in [IO.File]::ReadAllLines($allowPath)) {
            if ([string]::IsNullOrWhiteSpace($line) -or $line.TrimStart().StartsWith('#')) { continue }
            $parts = $line.Split('|', 3)
            if ($parts.Count -ne 3 -or $parts -contains '') { throw "Invalid allow entry: $line" }
            ,$parts
        })
    }
    $script:problems = 0
    foreach ($file in $files) {
        $path = [IO.Path]::GetRelativePath($Root, $file.FullName).Replace('\', '/')
        $number = 0
        foreach ($line in [IO.File]::ReadAllLines($file.FullName)) {
            $number++
            if ($line -match 'tools/v2[.]2/') { Write-Problem $path $number 'stale-path' $line }
            if ($line -match '(?i)(^\s*##\s+Go\b|●\s+Go\b|Plan\s*>\s*Go\b|"[G]o"|\bpast Go\b)' -and
                $line -notmatch '(?i)(era-one|before loop v3)') { Write-Problem $path $number 'stale-phase' $line }
            if ($line -cmatch '\b(C|N|P)[0-9]+[a-z]?-[a-z]|\bS[0-9]+-status\b') {
                Write-Problem $path $number 'stale-id' $line
            }
            if ($line -match '(?i)\b(loop v[12]\b|(?<!/)v2\.[12]\b)') { Write-Problem $path $number 'stale-version' $line }
            if ($line -match '\bfeat[/]' -and $line -notmatch '(fix|refactor|docs|chore)/') {
                Write-Problem $path $number 'feat-only' $line
            }

            $missing = $false
            foreach ($match in [regex]::Matches($line, '`((?:tools/|docs/|\.claude/|\.coord/templates/|\.coord/tools/|\.agents/|\.codex/)[^`]+)`')) {
                $target = $match.Groups[1].Value
                if ($target -match '[<*{]' -or $target -notmatch '\.[A-Za-z0-9]+$') { continue }
                if (-not (Test-Path -LiteralPath (Join-Path $Root $target) -PathType Leaf)) { $missing = $true }
            }
            if ($file.Extension -eq '.md') {
                # Balance parentheses in link destinations; titles may use either quote style or parentheses.
                $linkPattern = '\]\(\s*(?:<(?<url>[^>]+)>|(?<url>(?:\\.|[^\s()\\]|\((?<depth>)|\)(?<-depth>))+)(?(depth)(?!)))(?:\s+(?:"[^"]*"|''[^'']*''|\([^)]*\)))?\s*\)'
                foreach ($match in [regex]::Matches($line, $linkPattern)) {
                    $target = $match.Groups['url'].Value
                    if ($target -match '^(?:[A-Za-z][A-Za-z0-9+.-]*:|/|#)') { continue }
                    $target = [regex]::Replace($target, '\\([\x21-\x2f\x3a-\x40\x5b-\x60\x7b-\x7e ])', '$1')
                    $target = [Uri]::UnescapeDataString(($target -split '[#?]', 2)[0])
                    if ($target.Length -gt 0 -and -not (Test-Path -LiteralPath (Join-Path $file.DirectoryName $target))) {
                        $missing = $true
                    }
                }
            }
            if ($missing) { Write-Problem $path $number 'missing-path' $line }
        }
    }
    if ($script:problems -gt 0) { Write-Output "LOOP-LINT: FAIL ($script:problems problems)"; exit 1 }
    Write-Output "LOOP-LINT: PASS ($($files.Count) files)"
    exit 0
}
catch {
    Write-Output "LOOP-LINT: ERROR: $($_.Exception.Message)"
    exit 2
}
