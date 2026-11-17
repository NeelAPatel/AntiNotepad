# Builds and checks an item, recording the results and the tested checkout revision in its gate report.
[CmdletBinding()]
param(
    [string]$Task,
    [string]$Base = 'dev',
    [switch]$LooseScope,
    [ValidateRange(1, 2147483647)][int]$SizeWarn = 600,
    [ValidateRange(0, 2147483647)][int]$SizeGrowth = 25,
    [switch]$Keep,
    [switch]$Offline,
    [switch]$Ci
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($Task)) {
    if ($Ci) { $Task = 'ci' }
    else { throw 'Specify -Task <ID-slug> (or use -Ci for the ci task name).' }
}
if (-not ($Ci -and $Task -ceq 'ci') -and $Task -cnotmatch '^[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+$') {
    throw "Invalid task name '$Task'; expected <ID>-<lowercase-slug>."
}
if ($Base -cnotmatch '^[A-Za-z0-9][A-Za-z0-9._/-]*$') {
    throw "Invalid base ref '$Base'."
}

$script:repoRoot = [IO.Path]::GetFullPath((& git -C $PSScriptRoot rev-parse --show-toplevel).Trim())
$script:buildRoot = [IO.Path]::GetFullPath((Join-Path $script:repoRoot 'build'))
$script:buildPath = [IO.Path]::GetFullPath((Join-Path $script:buildRoot "$Task-gate"))
$script:dataPath = [IO.Path]::GetFullPath((Join-Path $script:buildRoot "$Task-gate-data"))
$script:exePath = Join-Path $script:buildPath 'AntiNotepad.exe'
$script:projectPath = Join-Path $script:repoRoot 'product/AntiNotepad/AntiNotepad.csproj'
$script:seamPath = Join-Path $script:repoRoot 'tools/check-platform-seam.ps1'
$script:baseName = $Base
$script:baseRevision = $null
$script:steps = [Collections.Generic.List[object]]::new()
$script:buildExitCode = $null
$script:reportPath = Join-Path $script:repoRoot ".coord/reports/$Task.gate.md"
$script:head = (& git -C $script:repoRoot rev-parse HEAD)
if ($LASTEXITCODE -ne 0) { throw 'Cannot resolve HEAD for the gate report.' }
$trackedChanges = @(& git -C $script:repoRoot status --porcelain --untracked-files=no)
if ($LASTEXITCODE -ne 0) { throw 'Cannot read tracked changes for the gate report.' }
if (@($trackedChanges | Where-Object { $_ -cnotmatch '^.. (\.coord/(STATUS|pending|lessons|status-archive)\.md|\.claude/settings\.json)$' }).Count -gt 0) {
    $script:head += '-dirty'
}

function Add-Step {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][ValidateSet('PASS', 'FAIL', 'WARN', 'SKIP')][string]$Status,
        [Parameter(Mandatory)][string]$Summary,
        [string[]]$Lines = @()
    )

    if ($Name -eq 'Scope' -and $LooseScope) { $Summary += ' (loose scope)' }
    $entry = [pscustomobject]@{
        Name = $Name
        Status = $Status
        Summary = $Summary
        Lines = @($Lines | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    }
    $script:steps.Add($entry)
    Write-Host "[$Status] ${Name}: $Summary"
    foreach ($line in $entry.Lines) { Write-Host "  $line" }
}

function Invoke-CapturedProcess {
    param(
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [Parameter(Mandatory)][string]$WorkingDirectory,
        [hashtable]$Environment = @{},
        [string[]]$RemoveEnvironment = @()
    )

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    $startInfo.WorkingDirectory = $WorkingDirectory
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in $Arguments) { [void]$startInfo.ArgumentList.Add([string]$argument) }
    foreach ($name in $RemoveEnvironment) { [void]$startInfo.Environment.Remove($name) }
    foreach ($name in $Environment.Keys) { $startInfo.Environment[[string]$name] = [string]$Environment[$name] }

    $process = [Diagnostics.Process]::Start($startInfo)
    if ($null -eq $process) { throw "Could not start $FileName." }
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    $process.WaitForExit()
    [pscustomobject]@{
        ExitCode = $process.ExitCode
        StdOut = $stdoutTask.GetAwaiter().GetResult()
        StdErr = $stderrTask.GetAwaiter().GetResult()
    }
}

function Get-CombinedOutput {
    param([Parameter(Mandatory)]$Result)
    $outputs = @($Result.StdOut, $Result.StdErr) | Where-Object { -not [string]::IsNullOrEmpty($_) }
    return ($outputs -join [Environment]::NewLine)
}

function Get-OutputLines {
    param([AllowEmptyString()][string]$Text)
    if ([string]::IsNullOrEmpty($Text)) { return @() }
    return @($Text -split '\r?\n')
}

function Get-KeyLines {
    param(
        [AllowEmptyString()][string]$Text,
        [Parameter(Mandatory)][string]$Pattern,
        [int]$Limit = 20
    )

    $matches = @(Get-OutputLines $Text | Where-Object { $_ -match $Pattern } | Select-Object -Last $Limit)
    if ($matches.Count -gt 0) { return $matches }
    return @(Get-OutputLines $Text | Select-Object -Last ([Math]::Min(8, $Limit)))
}

function Get-ItemIfPresent {
    param([Parameter(Mandatory)][string]$LiteralPath)
    if (Test-Path -LiteralPath $LiteralPath) { return Get-Item -LiteralPath $LiteralPath -Force }
    return $null
}

function Assert-NoReparsePoint {
    param([Parameter(Mandatory)][string]$LiteralPath, [Parameter(Mandatory)][string]$Description)
    $item = Get-ItemIfPresent $LiteralPath
    if ($null -ne $item -and (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0)) {
        throw "$Description must not be a symbolic link or reparse point: $LiteralPath"
    }
}

function Ensure-BuildRoot {
    $rootItem = Get-ItemIfPresent $script:repoRoot
    if ($null -eq $rootItem -or -not $rootItem.PSIsContainer) { throw "Repository root is unavailable: $script:repoRoot" }
    Assert-NoReparsePoint -LiteralPath $script:repoRoot -Description 'Repository root'

    $buildItem = Get-ItemIfPresent $script:buildRoot
    if ($null -eq $buildItem) {
        New-Item -ItemType Directory -Path $script:buildRoot -Force | Out-Null
        $buildItem = Get-Item -LiteralPath $script:buildRoot -Force
    }
    if (-not $buildItem.PSIsContainer) { throw "Build path is not a directory: $script:buildRoot" }
    Assert-NoReparsePoint -LiteralPath $script:buildRoot -Description 'Build directory'
}

function Remove-BuildEntry {
    param([Parameter(Mandatory)][string]$LiteralPath, [Parameter(Mandatory)][string]$BuildParent)

    $fullPath = [IO.Path]::GetFullPath($LiteralPath)
    $parentPrefix = $BuildParent.TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $fullPath.StartsWith($parentPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove a path outside build/: $fullPath"
    }

    $item = Get-Item -LiteralPath $fullPath -Force
    $isReparsePoint = (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0)
    if ($item.PSIsContainer -and -not $isReparsePoint) {
        foreach ($child in @(Get-ChildItem -LiteralPath $fullPath -Force)) {
            Remove-BuildEntry -LiteralPath $child.FullName -BuildParent $BuildParent
        }
    }
    # Removing a reparse point removes the link itself; its target is never enumerated.
    Remove-Item -LiteralPath $fullPath -Force
}

function Clear-BuildTarget {
    param([Parameter(Mandatory)][string]$LiteralPath)

    Ensure-BuildRoot
    $fullPath = [IO.Path]::GetFullPath($LiteralPath)
    $buildPrefix = $script:buildRoot.TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $fullPath.StartsWith($buildPrefix, [StringComparison]::OrdinalIgnoreCase) -or
        [IO.Path]::GetDirectoryName($fullPath) -ne $script:buildRoot) {
        throw "Refusing to remove anything outside a direct child of build/: $fullPath"
    }

    $item = Get-ItemIfPresent $fullPath
    if ($null -ne $item) { Remove-BuildEntry -LiteralPath $fullPath -BuildParent $script:buildRoot }
}

function Ensure-BuildDirectory {
    param([Parameter(Mandatory)][string]$LiteralPath)
    Ensure-BuildRoot
    $fullPath = [IO.Path]::GetFullPath($LiteralPath)
    $buildPrefix = $script:buildRoot.TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $fullPath.StartsWith($buildPrefix, [StringComparison]::OrdinalIgnoreCase) -or
        [IO.Path]::GetDirectoryName($fullPath) -ne $script:buildRoot) {
        throw "Refusing to create anything outside a direct child of build/: $fullPath"
    }

    New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
    Assert-NoReparsePoint -LiteralPath $fullPath -Description 'Managed build directory'
}

function Get-GitNames {
    param([Parameter(Mandatory)][string[]]$Arguments)
    $result = Invoke-CapturedProcess -FileName 'git' -Arguments (@('-C', $script:repoRoot) + $Arguments) -WorkingDirectory $script:repoRoot
    if ($result.ExitCode -ne 0) {
        $output = Get-CombinedOutput $result
        throw "git $($Arguments -join ' ') failed with exit $($result.ExitCode): $output"
    }
    return @(Get-OutputLines $result.StdOut | Where-Object { $_.Length -gt 0 })
}

function Resolve-BaseRevision {
    if ($script:baseRevision) { return $script:baseRevision }

    foreach ($candidate in @($script:baseName, "origin/$($script:baseName)")) {
        $result = Invoke-CapturedProcess -FileName 'git' -Arguments @('-C', $script:repoRoot, 'rev-parse', '--verify', '--quiet', "$candidate^{commit}") -WorkingDirectory $script:repoRoot
        if ($result.ExitCode -eq 0 -and -not [string]::IsNullOrWhiteSpace($result.StdOut)) {
            $script:baseRevision = $result.StdOut.Trim()
            return $script:baseRevision
        }
    }
    throw "Cannot resolve base ref '$($script:baseName)' locally or as origin/$($script:baseName)."
}

function Get-ChangedFiles {
    $names = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $baseRevision = Resolve-BaseRevision
    foreach ($name in (Get-GitNames @('diff', '--name-only', "$baseRevision...HEAD", '--'))) { [void]$names.Add($name.Replace('\', '/')) }
    foreach ($name in (Get-GitNames @('diff', '--name-only', '--'))) { [void]$names.Add($name.Replace('\', '/')) }
    foreach ($name in (Get-GitNames @('diff', '--cached', '--name-only', '--'))) { [void]$names.Add($name.Replace('\', '/')) }
    foreach ($name in (Get-GitNames @('ls-files', '--others', '--exclude-standard'))) { [void]$names.Add($name.Replace('\', '/')) }
    return @($names | Where-Object { $_ -notmatch '^\.coord/' } | Sort-Object)
}

function Get-FileLines {
    param([AllowEmptyString()][string]$Text)
    if ([string]::IsNullOrEmpty($Text)) { return 0 }
    $lineBreaks = [regex]::Matches($Text, "`r`n|`n|`r").Count
    if ($Text -match '(?:\r\n|\n|\r)$') { return $lineBreaks }
    return $lineBreaks + 1
}

function Write-GateReport {
    $failed = @($script:steps | Where-Object Status -eq 'FAIL').Count -gt 0 -or
        @($script:steps | Where-Object { $_.Name -eq 'Selftest' -and $_.Status -ne 'PASS' }).Count -gt 0
    $gateStatus = if ($failed) { 'FAIL' } else { 'PASS' }
    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add('## Summary')
    $lines.Add("GATE: $gateStatus")
    $lines.Add("Head: $script:head")
    foreach ($step in $script:steps) { $lines.Add("$($step.Name): $($step.Status) - $($step.Summary)") }
    foreach ($step in $script:steps) {
        $lines.Add('')
        $lines.Add("## $($step.Name)")
        $lines.Add("$($step.Status) - $($step.Summary)")
        foreach ($line in $step.Lines) { $lines.Add("- $line") }
    }

    $reportsDirectory = Split-Path -Parent $script:reportPath
    New-Item -ItemType Directory -Path $reportsDirectory -Force | Out-Null
    [IO.File]::WriteAllText($script:reportPath, ($lines -join [Environment]::NewLine) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
    Write-Host "GATE: $gateStatus"
    Write-Host "Report: $script:reportPath"
    return $failed
}

$buildPassed = $false
$cleanupFailed = $false
if ($Offline) {
    Add-Step -Name 'Offline' -Status 'WARN' -Summary 'NuGet audit disabled for this build only' `
        -Lines @('Offline: NuGet audit off (sandbox)')
}
try {
    try {
        Clear-BuildTarget -LiteralPath $script:buildPath
        Clear-BuildTarget -LiteralPath $script:dataPath
        $buildArguments = @('build', $script:projectPath, '-c', 'Debug', '-o', $script:buildPath)
        if ($Offline) { $buildArguments += '-p:NuGetAudit=false' }
        $build = Invoke-CapturedProcess -FileName 'dotnet' -Arguments $buildArguments `
            -WorkingDirectory $script:repoRoot -Environment @{ AVALONIA_TELEMETRY_OPTOUT = '1' } -RemoveEnvironment @('ANTINOTEPAD_DEV', 'ANTINOTEPAD_DATA', 'ANTINOTEPAD_SLOT')
        $script:buildExitCode = $build.ExitCode
        $buildOutput = Get-CombinedOutput $build
        $warningSummary = [regex]::Matches($buildOutput, '(?im)^\s*(\d+)\s+Warning\(s\)\s*$')
        $errorSummary = [regex]::Matches($buildOutput, '(?im)^\s*(\d+)\s+Error\(s\)\s*$')
        if ($warningSummary.Count -gt 0) {
            $warningCount = [int]$warningSummary[$warningSummary.Count - 1].Groups[1].Value
        }
        else {
            $warningCount = @(Get-OutputLines $buildOutput | Where-Object { $_ -match '(?i)\bwarning(?:\s+[A-Z]+\d+)?\s*:' }).Count
        }
        if ($errorSummary.Count -gt 0) {
            $errorCount = [int]$errorSummary[$errorSummary.Count - 1].Groups[1].Value
        }
        else {
            $errorCount = @(Get-OutputLines $buildOutput | Where-Object { $_ -match '(?i)\berror(?:\s+[A-Z]+\d+)?\s*:' }).Count
        }
        $buildPassed = $build.ExitCode -eq 0 -and $warningCount -eq 0 -and $errorCount -eq 0
        $buildStatus = if ($buildPassed) { 'PASS' } else { 'FAIL' }
        $buildLines = @(Get-KeyLines -Text $buildOutput -Pattern '(?i)(Build (succeeded|FAILED)|Warning\(s\)|Error\(s\)|\bwarning\s+[A-Z]+\d+\s*:|\berror\s+[A-Z]+\d+\s*:)')
        Add-Step -Name 'Build' -Status $buildStatus -Summary "dotnet exit $($build.ExitCode), $warningCount warning(s), $errorCount error(s)" -Lines $buildLines
    }
    catch {
        Add-Step -Name 'Build' -Status 'FAIL' -Summary $_.Exception.Message
    }

    if ($null -ne $script:buildExitCode -and $script:buildExitCode -eq 0 -and (Test-Path -LiteralPath $script:exePath -PathType Leaf)) {
        try {
            Clear-BuildTarget -LiteralPath $script:dataPath
            Ensure-BuildDirectory -LiteralPath $script:dataPath
            $selftest = Invoke-CapturedProcess -FileName $script:exePath -Arguments @('--selftest') -WorkingDirectory $script:buildPath `
                -Environment @{ ANTINOTEPAD_DATA = $script:dataPath } -RemoveEnvironment @('ANTINOTEPAD_DEV', 'ANTINOTEPAD_SLOT')
            $selftestOutput = Get-CombinedOutput $selftest
            $selftestHasPass = $selftestOutput -match '(?m)^SELFTEST PASS\s*$'
            $selftestPassed = $selftest.ExitCode -eq 0 -and $selftestHasPass
            $selftestStatus = if ($selftestPassed) { 'PASS' } else { 'FAIL' }
            $selftestLines = @(Get-KeyLines -Text $selftestOutput -Pattern '^(LAYERS\s|CONTRACTS:|SELFTEST (PASS|FAIL))' -Limit 200)
            if (-not $selftestPassed) { $selftestLines = @(Get-OutputLines $selftestOutput) }
            $selftestNote = if ($selftestPassed) { 'contains SELFTEST PASS' } else { 'requires exit 0 and SELFTEST PASS' }
            Add-Step -Name 'Selftest' -Status $selftestStatus -Summary "exe exit $($selftest.ExitCode); $selftestNote" -Lines $selftestLines
        }
        catch {
            Add-Step -Name 'Selftest' -Status 'FAIL' -Summary $_.Exception.Message
        }
    }
    elseif ($buildPassed) {
        Add-Step -Name 'Selftest' -Status 'FAIL' -Summary 'successful build did not produce a runnable AntiNotepad.exe'
    }
    else {
        Add-Step -Name 'Selftest' -Status 'SKIP' -Summary 'build did not produce a runnable AntiNotepad.exe'
    }

    try {
        $seam = Invoke-CapturedProcess -FileName 'pwsh' -Arguments @('-NoProfile', '-File', $script:seamPath) -WorkingDirectory $script:repoRoot
        $seamOutput = Get-CombinedOutput $seam
        $seamPassed = $seam.ExitCode -eq 0
        $seamStatus = if ($seamPassed) { 'PASS' } else { 'FAIL' }
        Add-Step -Name 'Seam' -Status $seamStatus -Summary "check-platform-seam.ps1 exit $($seam.ExitCode)" -Lines @(Get-KeyLines -Text $seamOutput -Pattern '(?i)(PLATFORM-SEAM|violation|error)')
    }
    catch {
        Add-Step -Name 'Seam' -Status 'FAIL' -Summary $_.Exception.Message
    }

    try {
        $tokens = Invoke-CapturedProcess -FileName 'pwsh' -Arguments @('-NoProfile', '-File', (Join-Path $script:repoRoot 'tools/check-design-tokens.ps1')) -WorkingDirectory $script:repoRoot
        $tokensOutput = Get-CombinedOutput $tokens
        $tokensStatus = if ($tokens.ExitCode -eq 0) { 'PASS' } else { 'FAIL' }
        Add-Step -Name 'Tokens' -Status $tokensStatus -Summary "check-design-tokens.ps1 exit $($tokens.ExitCode)" -Lines @(Get-KeyLines -Text $tokensOutput -Pattern '(?i)(TOKENS|token from|error)')
    }
    catch {
        Add-Step -Name 'Tokens' -Status 'FAIL' -Summary $_.Exception.Message
    }

    # Format: `dotnet format` on the changed product C# files only. A problem on a line this change touched fails;
    # problems on untouched lines are older code and only counted, so existing files never block a gate. In a new file, lines
    # that the same change removed verbatim from product files (a split) count as untouched.
    if ($Ci) {
        Add-Step -Name 'Format' -Status 'SKIP' -Summary 'format is skipped in CI mode'
    }
    else {
        try {
            $csFiles = @(Get-ChangedFiles | Where-Object { $_ -match '^product/.+\.cs$' -and (Test-Path (Join-Path $script:repoRoot $_)) })
            if ($csFiles.Count -eq 0) {
                Add-Step -Name 'Format' -Status 'PASS' -Summary 'no changed product C# files'
            }
            else {
                $baseRevision = Resolve-BaseRevision
                $touched = @{}
                $removed = $null
                foreach ($file in $csFiles) {
                    $set = [Collections.Generic.HashSet[int]]::new()
                    $diff = Invoke-CapturedProcess -FileName 'git' -Arguments @('-C', $script:repoRoot, 'diff', '-U0', $baseRevision, '--', $file) -WorkingDirectory $script:repoRoot
                    $tracked = (Invoke-CapturedProcess -FileName 'git' -Arguments @('-C', $script:repoRoot, 'cat-file', '-e', "${baseRevision}:$file") -WorkingDirectory $script:repoRoot).ExitCode -eq 0
                    if (-not $tracked) {
                        if ($null -eq $removed) {
                            $removed = [Collections.Generic.HashSet[string]]::new()
                            $all = Invoke-CapturedProcess -FileName 'git' -Arguments @('-C', $script:repoRoot, 'diff', '-U0', '--no-renames', $baseRevision, '--', 'product/*.cs') -WorkingDirectory $script:repoRoot
                            foreach ($l in (Get-OutputLines $all.StdOut)) {
                                if ($l.StartsWith('-') -and -not $l.StartsWith('---')) { [void]$removed.Add($l.Substring(1).TrimEnd()) }
                            }
                        }
                        $lineNo = 0
                        foreach ($l in [IO.File]::ReadAllLines((Join-Path $script:repoRoot $file))) {
                            $lineNo++
                            if (-not $removed.Contains($l.TrimEnd())) { [void]$set.Add($lineNo) }
                        }
                    }
                    else {
                        foreach ($m in [regex]::Matches($diff.StdOut, '(?m)^@@ -\S+ \+(\d+)(?:,(\d+))? @@')) {
                            $start = [int]$m.Groups[1].Value
                            $count = if ($m.Groups[2].Success) { [int]$m.Groups[2].Value } else { 1 }
                            for ($i = $start; $i -lt $start + $count; $i++) { [void]$set.Add($i) }
                        }
                    }
                    $touched[[IO.Path]::GetFullPath((Join-Path $script:repoRoot $file))] = $set
                }
                $formatArgs = @('format', $script:projectPath, '--verify-no-changes', '--verbosity', 'quiet', '--include') + $csFiles
                $format = Invoke-CapturedProcess -FileName 'dotnet' -Arguments $formatArgs -WorkingDirectory $script:repoRoot
                $new = [Collections.Generic.List[string]]::new(); $old = 0
                foreach ($line in (Get-OutputLines (Get-CombinedOutput $format))) {
                    $m = [regex]::Match($line, '^(?<path>.+?\.cs)\((?<line>\d+),\d+\): (?:error|warning) (?<code>\S+): (?<msg>.*?)(?: \[.*\])?$')
                    if (-not $m.Success) { continue }
                    $path = [IO.Path]::GetFullPath($m.Groups['path'].Value)
                    $lineNo = [int]$m.Groups['line'].Value
                    $set = $touched[$path]
                    # WHITESPACE problems start at the end of the line before the bad indent, so that line counts too.
                    if ($touched.ContainsKey($path) -and ($null -eq $set -or $set.Contains($lineNo) -or $set.Contains($lineNo + 1))) {
                        $rel = [IO.Path]::GetRelativePath($script:repoRoot, $path).Replace('\', '/')
                        $new.Add("${rel}:${lineNo} $($m.Groups['code'].Value): $($m.Groups['msg'].Value)")
                    }
                    else { $old++ }
                }
                if ($new.Count -gt 0) {
                    Add-Step -Name 'Format' -Status 'FAIL' -Summary "$($new.Count) format problem(s) on changed lines (fix: dotnet format --include <file>); $old older one(s) ignored" -Lines @($new | Select-Object -First 40)
                }
                else {
                    Add-Step -Name 'Format' -Status 'PASS' -Summary "$($csFiles.Count) changed C# file(s), no format problems on changed lines; $old older one(s) ignored"
                }
            }
        }
        catch {
            Add-Step -Name 'Format' -Status 'WARN' -Summary "format check could not run: $($_.Exception.Message)"
        }
    }

    try {
        $branch = (Get-GitNames @('rev-parse', '--abbrev-ref', 'HEAD')) -join ''
        $scopeStatus = if ($Ci -or $LooseScope -or $branch -ceq $Base) { 'WARN' } else { 'FAIL' }
        $changedFiles = @(Get-ChangedFiles)
        $roundMatch = [regex]::Match($Task, '^(?<base>.+)-r(?<number>[0-9]+)$')
        [long]$roundNumber = 0
        $isReworkRound = $roundMatch.Success -and
            [long]::TryParse($roundMatch.Groups['number'].Value, [ref]$roundNumber) -and $roundNumber -ge 2
        if (-not $isReworkRound) {
            $briefPath = Join-Path $script:repoRoot ".coord/tasks/$Task.md"
            if (-not (Test-Path -LiteralPath $briefPath -PathType Leaf)) {
                Add-Step -Name 'Scope' -Status $scopeStatus -Summary "brief not found: .coord/tasks/$Task.md"
            }
            else {
                $brief = [IO.File]::ReadAllText($briefPath)
                $filesSection = [regex]::Match($brief, '(?ms)^## Files\s*\r?\n(.*?)(?=^##\s|\z)')
                if (-not $filesSection.Success) {
                    Add-Step -Name 'Scope' -Status $scopeStatus -Summary 'brief has no ## Files section'
                }
                else {
                    $allowed = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
                    foreach ($match in [regex]::Matches($filesSection.Groups[1].Value, '(?m)^\s*-\s+`([^`]+)`')) {
                        $path = $match.Groups[1].Value.Trim().Replace('\', '/')
                        if ($path.StartsWith('./', [StringComparison]::Ordinal)) { $path = $path.Substring(2) }
                        if ($path.Length -gt 0) { [void]$allowed.Add($path) }
                    }
                    if ($allowed.Count -eq 0) {
                        Add-Step -Name 'Scope' -Status $scopeStatus -Summary '## Files contains no backticked file paths'
                    }
                    else {
                        $outOfScope = @($changedFiles | Where-Object { -not $allowed.Contains($_) })
                        if ($outOfScope.Count -eq 0) {
                            Add-Step -Name 'Scope' -Status 'PASS' -Summary "$($changedFiles.Count) changed file(s); all are allowed by the brief"
                        }
                        else {
                            Add-Step -Name 'Scope' -Status $scopeStatus -Summary "$($outOfScope.Count) changed file(s) outside the brief's Files list" -Lines $outOfScope
                        }
                    }
                }
            }
        }
        else {
            $baseTask = $roundMatch.Groups['base'].Value
            $tasksDirectory = Join-Path $script:repoRoot '.coord/tasks'
            $baseBriefName = "$baseTask.md"
            $baseBriefPath = Join-Path $tasksDirectory $baseBriefName
            if (-not (Test-Path -LiteralPath $baseBriefPath -PathType Leaf)) {
                Add-Step -Name 'Scope' -Status $scopeStatus -Summary "base brief not found: .coord/tasks/$baseBriefName"
            }
            else {
                $briefsToRead = [Collections.Generic.List[object]]::new()
                $briefsToRead.Add([pscustomobject]@{ Number = 0L; Name = $baseBriefName; Path = $baseBriefPath })
                $roundPattern = '^' + [regex]::Escape($baseTask) + '-r(?<number>[0-9]+)\.md$'
                foreach ($candidate in (Get-ChildItem -LiteralPath $tasksDirectory -File)) {
                    $candidateMatch = [regex]::Match($candidate.Name, $roundPattern, [Text.RegularExpressions.RegexOptions]::IgnoreCase)
                    if (-not $candidateMatch.Success) { continue }
                    [long]$candidateNumber = 0
                    if ([long]::TryParse($candidateMatch.Groups['number'].Value, [ref]$candidateNumber) -and
                        $candidateNumber -le $roundNumber) {
                        $briefsToRead.Add([pscustomobject]@{
                            Number = $candidateNumber
                            Name = $candidate.Name
                            Path = $candidate.FullName
                        })
                    }
                }
                $roundBriefName = "$Task.md"
                if (-not (Test-Path -LiteralPath (Join-Path $tasksDirectory $roundBriefName) -PathType Leaf)) {
                    $briefsToRead.Add([pscustomobject]@{ Number = $roundNumber; Name = $roundBriefName; Path = $null })
                }
                $orderedBriefs = @($briefsToRead | Sort-Object Number, Name)
                $allowed = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
                $briefLines = [Collections.Generic.List[string]]::new()
                $briefsWithoutFiles = [Collections.Generic.List[string]]::new()
                foreach ($briefEntry in $orderedBriefs) {
                    $relativeBrief = ".coord/tasks/$($briefEntry.Name)"
                    $briefLines.Add("read $relativeBrief")
                    if ($null -eq $briefEntry.Path) {
                        $briefsWithoutFiles.Add("$relativeBrief not found")
                        continue
                    }
                    $brief = [IO.File]::ReadAllText($briefEntry.Path)
                    $filesSection = [regex]::Match($brief, '(?ms)^## Files\s*\r?\n(.*?)(?=^##\s|\z)')
                    if (-not $filesSection.Success) {
                        $briefsWithoutFiles.Add("$relativeBrief has no ## Files section")
                        continue
                    }
                    $hasPaths = $false
                    foreach ($match in [regex]::Matches($filesSection.Groups[1].Value, '(?m)^\s*-\s+`([^`]+)`')) {
                        $path = $match.Groups[1].Value.Trim().Replace('\', '/')
                        if ($path.StartsWith('./', [StringComparison]::Ordinal)) { $path = $path.Substring(2) }
                        if ($path.Length -gt 0) { [void]$allowed.Add($path); $hasPaths = $true }
                    }
                    if (-not $hasPaths) { $briefsWithoutFiles.Add("$relativeBrief has no backticked ## Files paths") }
                }
                if ($allowed.Count -eq 0) {
                    Add-Step -Name 'Scope' -Status $scopeStatus -Summary 'rework briefs contain no backticked ## Files paths' -Lines @($briefLines + $briefsWithoutFiles)
                }
                else {
                    $outOfScope = @($changedFiles | Where-Object { -not $allowed.Contains($_) })
                    $scopeLines = @($briefLines + $briefsWithoutFiles + $outOfScope)
                    if ($briefsWithoutFiles.Count -gt 0) {
                        Add-Step -Name 'Scope' -Status $scopeStatus -Summary "$($briefsWithoutFiles.Count) brief(s) have no Files list; checked allowed paths from the other briefs" -Lines $scopeLines
                    }
                    elseif ($outOfScope.Count -eq 0) {
                        Add-Step -Name 'Scope' -Status 'PASS' -Summary "$($changedFiles.Count) changed file(s); all are allowed by $($orderedBriefs.Count) rework brief(s)" -Lines $scopeLines
                    }
                    else {
                        Add-Step -Name 'Scope' -Status $scopeStatus -Summary "$($outOfScope.Count) changed file(s) outside the rework briefs' Files lists" -Lines $scopeLines
                    }
                }
            }
        }
    }
    catch {
        Add-Step -Name 'Scope' -Status 'FAIL' -Summary $_.Exception.Message
    }

    try {
        # Keep .coord/ paths here: Scope filters them out, but templates and helpers can affect live loop rules.
        $loopChangedNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        $loopBaseRevision = Resolve-BaseRevision
        foreach ($name in (Get-GitNames @('diff', '--name-only', "$loopBaseRevision...HEAD", '--'))) { [void]$loopChangedNames.Add($name.Replace('\', '/')) }
        foreach ($name in (Get-GitNames @('diff', '--name-only', '--'))) { [void]$loopChangedNames.Add($name.Replace('\', '/')) }
        foreach ($name in (Get-GitNames @('diff', '--cached', '--name-only', '--'))) { [void]$loopChangedNames.Add($name.Replace('\', '/')) }
        foreach ($name in (Get-GitNames @('ls-files', '--others', '--exclude-standard'))) { [void]$loopChangedNames.Add($name.Replace('\', '/')) }
        $loopFiles = @($loopChangedNames | Sort-Object | Where-Object {
            $_ -match '^(AGENTS\.md|docs/(loop|glossary|agents|coding|dev-loop)\.md|\.claude/skills/[^/]+/SKILL\.md|\.claude/agents/[^/]+\.md|\.coord/templates/[^/]+\.md|\.agents/skills/[^/]+/SKILL\.md|\.codex/agents/[^/]+\.toml|tools/[^/]+\.(ps1|sh)|\.coord/tools/[^/]+\.sh)$'
        })
        if ($Ci -or $loopFiles.Count -gt 0) {
            $lint = Invoke-CapturedProcess -FileName 'pwsh' -Arguments @('-NoProfile', '-File', (Join-Path $script:repoRoot 'tools/loop-lint.ps1')) -WorkingDirectory $script:repoRoot
            $lintStatus = if ($lint.ExitCode -eq 0) { 'PASS' } else { 'FAIL' }
            Add-Step -Name 'LoopLint' -Status $lintStatus -Summary "loop-lint.ps1 exit $($lint.ExitCode)" `
                -Lines @(Get-OutputLines (Get-CombinedOutput $lint) | Select-Object -First 40)
        }
        else {
            Add-Step -Name 'LoopLint' -Status 'SKIP' -Summary 'no loop files changed'
        }
    }
    catch {
        Add-Step -Name 'LoopLint' -Status 'FAIL' -Summary $_.Exception.Message
    }

    try {
        $changedFiles = if ($Ci) { @(Get-ChangedFiles) } else { $changedFiles }
        $sizeWarnings = [Collections.Generic.List[string]]::new()
        foreach ($relativePath in $changedFiles) {
            if ($relativePath -notmatch '^product/.+\.cs$') { continue }
            $currentPath = Join-Path $script:repoRoot ($relativePath.Replace('/', [IO.Path]::DirectorySeparatorChar))
            if (-not (Test-Path -LiteralPath $currentPath -PathType Leaf)) { continue }
            $newLines = Get-FileLines ([IO.File]::ReadAllText($currentPath))
            $baseRevision = Resolve-BaseRevision
            $baseResult = Invoke-CapturedProcess -FileName 'git' -Arguments @('-C', $script:repoRoot, 'show', "${baseRevision}:$relativePath") -WorkingDirectory $script:repoRoot
            $oldLines = if ($baseResult.ExitCode -eq 0) { Get-FileLines $baseResult.StdOut } else { 0 }
            $crossedLimit = $oldLines -le $SizeWarn -and $newLines -gt $SizeWarn
            $largeOverLimitGrowth = $oldLines -gt $SizeWarn -and ($newLines - $oldLines) -gt $SizeGrowth
            if ($crossedLimit -or $largeOverLimitGrowth) {
                $sizeWarnings.Add("${relativePath}: $oldLines -> $newLines lines")
            }
        }
        if ($sizeWarnings.Count -eq 0) {
            Add-Step -Name 'Size' -Status 'PASS' -Summary "no changed product C# file crossed $SizeWarn lines or grew by more than $SizeGrowth while already over it"
        }
        else {
            Add-Step -Name 'Size' -Status 'WARN' -Summary "$($sizeWarnings.Count) changed product C# file(s) crossed $SizeWarn lines or grew by more than $SizeGrowth while already over it" -Lines @($sizeWarnings)
        }
    }
    catch {
        Add-Step -Name 'Size' -Status 'FAIL' -Summary $_.Exception.Message
    }
}
finally {
    $alreadyFailed = @($script:steps | Where-Object Status -eq 'FAIL').Count -gt 0
    if ($Keep -or $alreadyFailed) {
        $reason = if ($alreadyFailed) { 'kept build and data folders because the gate failed' } else { 'kept build and data folders by -Keep' }
        Add-Step -Name 'Cleanup' -Status 'SKIP' -Summary $reason
    }
    else {
        try {
            Clear-BuildTarget -LiteralPath $script:buildPath
            Clear-BuildTarget -LiteralPath $script:dataPath
            Add-Step -Name 'Cleanup' -Status 'PASS' -Summary 'removed only the task build and data folders under build/'
        }
        catch {
            $cleanupFailed = $true
            Add-Step -Name 'Cleanup' -Status 'FAIL' -Summary $_.Exception.Message
        }
    }
}

$failed = Write-GateReport
if ($failed -or $cleanupFailed) {
    try {
        $titleScript = Join-Path $script:repoRoot 'tools/chat-title.ps1'
        if (Test-Path -LiteralPath $titleScript) { & $titleScript -State blocked }
    } catch { }
    exit 1
}
exit 0
