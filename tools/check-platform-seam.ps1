# Reject native interop tokens in AntiNotepad C# source outside Platform/, while ignoring comments and literal text.
function Mask-CSharpCode([string] $Text) {
    $script:sourceText = $Text
    $script:maskedChars = $Text.ToCharArray()
    [void](Read-CSharpCode 0 0 $false)
    return -join $script:maskedChars
}

function Hide-CSharpRange([int] $Start, [int] $End) {
    for ($position = $Start; $position -lt $End; $position++) {
        if ($script:sourceText[$position] -ne "`r" -and $script:sourceText[$position] -ne "`n") {
            $script:maskedChars[$position] = [char]' '
        }
    }
}

function Read-CSharpString([int] $PrefixStart, [int] $DollarCount, [bool] $Verbatim, [bool] $Raw, [int] $QuoteCount, [int] $ContentStart) {
    $text = $script:sourceText
    $length = $text.Length
    $segmentStart = $PrefixStart
    $position = $ContentStart

    while ($position -lt $length) {
        if ($Raw) {
            if ($text[$position] -eq '"') {
                $quoteRun = 0
                while ($position + $quoteRun -lt $length -and $text[$position + $quoteRun] -eq '"') { $quoteRun++ }
                if ($quoteRun -ge $QuoteCount) {
                    Hide-CSharpRange $segmentStart ($position + $QuoteCount)
                    return ($position + $QuoteCount)
                }
                $position += $quoteRun
                continue
            }

            if ($DollarCount -gt 0 -and $text[$position] -eq '{') {
                $braceRun = 0
                while ($position + $braceRun -lt $length -and $text[$position + $braceRun] -eq '{') { $braceRun++ }
                if ($braceRun -ge $DollarCount) {
                    $openAt = $position + $braceRun - $DollarCount
                    Hide-CSharpRange $segmentStart ($openAt + $DollarCount)
                    $holeEnd = Read-CSharpCode ($openAt + $DollarCount) $DollarCount $true
                    if ($holeEnd -ge $length) { return $length }
                    Hide-CSharpRange $holeEnd ($holeEnd + $DollarCount)
                    $position = $holeEnd + $DollarCount
                    $segmentStart = $position
                    continue
                }
                $position += $braceRun
                continue
            }

            $position++
            continue
        }

        if (-not $Verbatim -and $text[$position] -eq '\') {
            $position = [Math]::Min($length, $position + 2)
            continue
        }

        if ($text[$position] -eq '"') {
            if ($Verbatim -and $position + 1 -lt $length -and $text[$position + 1] -eq '"') {
                $position += 2
                continue
            }
            Hide-CSharpRange $segmentStart ($position + 1)
            return ($position + 1)
        }

        if ($DollarCount -gt 0 -and $text[$position] -eq '{') {
            $braceRun = 0
            while ($position + $braceRun -lt $length -and $text[$position + $braceRun] -eq '{') { $braceRun++ }
            $literalRun = $braceRun - ($braceRun % 2)
            if ($braceRun % 2 -eq 1) {
                $openAt = $position + $braceRun - 1
                Hide-CSharpRange $segmentStart ($openAt + 1)
                $holeEnd = Read-CSharpCode ($openAt + 1) 1 $false
                if ($holeEnd -ge $length) { return $length }
                Hide-CSharpRange $holeEnd ($holeEnd + 1)
                $position = $holeEnd + 1
                $segmentStart = $position
                continue
            }
            $position += $literalRun
            continue
        }

        if ($DollarCount -gt 0 -and $text[$position] -eq '}') {
            $braceRun = 0
            while ($position + $braceRun -lt $length -and $text[$position + $braceRun] -eq '}') { $braceRun++ }
            $position += $braceRun
            continue
        }

        $position++
    }

    Hide-CSharpRange $segmentStart $length
    return $length
}

function Read-CSharpCode([int] $Start, [int] $InterpolationBraceCount, [bool] $RawInterpolation) {
    $text = $script:sourceText
    $length = $text.Length
    $position = $Start
    $braceDepth = 0
    $parenDepth = 0
    $bracketDepth = 0
    $conditionalDepth = 0
    $inFormatText = $false

    while ($position -lt $length) {
        $current = $text[$position]

        if ($InterpolationBraceCount -gt 0 -and $braceDepth -eq 0 -and $current -eq '}') {
            $braceRun = 0
            while ($position + $braceRun -lt $length -and $text[$position + $braceRun] -eq '}') { $braceRun++ }
            if ($RawInterpolation) {
                if ($braceRun -ge $InterpolationBraceCount) { return ($position + $braceRun - $InterpolationBraceCount) }
            }
            elseif (-not $inFormatText) { return $position }
            else { return $position }
        }

        if ($inFormatText) { $position++; continue }

        if ($current -eq '/' -and $position + 1 -lt $length) {
            if ($text[$position + 1] -eq '/') {
                $end = $position + 2
                while ($end -lt $length -and $text[$end] -ne "`r" -and $text[$end] -ne "`n") { $end++ }
                Hide-CSharpRange $position $end
                $position = $end
                continue
            }
            if ($text[$position + 1] -eq '*') {
                $end = $position + 2
                while ($end + 1 -lt $length -and -not ($text[$end] -eq '*' -and $text[$end + 1] -eq '/')) { $end++ }
                $end = [Math]::Min($length, $end + 2)
                Hide-CSharpRange $position $end
                $position = $end
                continue
            }
        }

        if ($current -eq "'") {
            $end = $position + 1
            while ($end -lt $length) {
                if ($text[$end] -eq '\') { $end = [Math]::Min($length, $end + 2); continue }
                if ($text[$end] -eq "'") { $end++; break }
                $end++
            }
            Hide-CSharpRange $position $end
            $position = $end
            continue
        }

        $prefixStart = $position
        $cursor = $position
        $dollarCount = 0
        while ($cursor -lt $length -and $text[$cursor] -eq '$') { $dollarCount++; $cursor++ }
        $verbatim = $false
        if ($cursor -lt $length -and $text[$cursor] -eq '@') { $verbatim = $true; $cursor++ }
        elseif ($dollarCount -eq 0 -and $current -eq '@') {
            $verbatim = $true
            $cursor++
            while ($cursor -lt $length -and $text[$cursor] -eq '$') { $dollarCount++; $cursor++ }
        }

        if ($cursor -lt $length -and $text[$cursor] -eq '"') {
            $quoteCount = 0
            while ($cursor + $quoteCount -lt $length -and $text[$cursor + $quoteCount] -eq '"') { $quoteCount++ }
            $raw = $quoteCount -ge 3
            if ($raw) {
                $position = Read-CSharpString $prefixStart $dollarCount $verbatim $true $quoteCount ($cursor + $quoteCount)
            }
            elseif ($quoteCount -ge 1) {
                $position = Read-CSharpString $prefixStart $dollarCount $verbatim $false 1 ($cursor + 1)
            }
            else { $position++ }
            continue
        }

        $formatColon = $position + 1 -lt $length -and ($text[$position + 1] -eq ':' -or $position -gt 0 -and $text[$position - 1] -eq ':')
        if ($InterpolationBraceCount -gt 0 -and $braceDepth -eq 0 -and $parenDepth -eq 0 -and $bracketDepth -eq 0 -and $current -eq ':' -and $conditionalDepth -eq 0 -and -not $formatColon) {
            $inFormatText = $true
            $position++
            continue
        }

        if ($current -eq '{') { $braceDepth++ }
        elseif ($current -eq '}') { if ($braceDepth -gt 0) { $braceDepth-- } }
        elseif ($current -eq '(') { $parenDepth++ }
        elseif ($current -eq ')') { if ($parenDepth -gt 0) { $parenDepth-- } }
        elseif ($current -eq '[') { $bracketDepth++ }
        elseif ($current -eq ']') { if ($bracketDepth -gt 0) { $bracketDepth-- } }
        elseif ($current -eq '?' -and -not ($position + 1 -lt $length -and $text[$position + 1] -eq '?')) { $conditionalDepth++ }
        elseif ($current -eq ':' -and $conditionalDepth -gt 0) { $conditionalDepth-- }

        $position++
    }

    return $length
}

$ErrorActionPreference = 'Stop'
$productRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\product\AntiNotepad'))
if (-not [IO.Directory]::Exists($productRoot)) {
    Write-Error "Product source directory not found: $productRoot"
    exit 1
}

$files = [IO.Directory]::EnumerateFiles($productRoot, '*.cs', [IO.SearchOption]::AllDirectories) |
    Where-Object {
        $relative = [IO.Path]::GetRelativePath($productRoot, $_)
        $parts = $relative -split '[\\/]'
        $parts[0] -ne 'Platform'
    } |
    Sort-Object

$tokens = @('DllImport', 'LibraryImport', 'TryGetPlatformHandle', 'Win32Properties', 'DwmSetWindowAttribute', 'IntPtr', 'UIntPtr', 'nint', 'nuint')
$tokenPattern = '(?<![A-Za-z0-9_])(?:' + (($tokens | ForEach-Object { [Regex]::Escape($_) }) -join '|') + ')(?![A-Za-z0-9_])'
$violations = [Collections.Generic.List[string]]::new()

foreach ($file in $files) {
    $source = [IO.File]::ReadAllText($file)
    $masked = Mask-CSharpCode $source
    foreach ($match in [Regex]::Matches($masked, $tokenPattern)) {
        $line = 1
        for ($i = 0; $i -lt $match.Index; $i++) { if ($masked[$i] -eq "`n") { $line++ } }
        $relative = [IO.Path]::GetRelativePath((Split-Path -Parent $PSScriptRoot), $file)
        $violations.Add("${relative}:${line}: $($match.Value)")
    }
}

if ($violations.Count -gt 0) {
    foreach ($violation in $violations) { Write-Output $violation }
    exit 1
}

Write-Output "PLATFORM-SEAM source guard: 0 violations in $(@($files).Count) files"
exit 0
