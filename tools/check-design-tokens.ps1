# Inventories literal design values and prevents their counts from increasing.
[CmdletBinding()]
param([switch]$Update, [string]$Inventory, [switch]$SelfCheck)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$baselinePath = Join-Path $PSScriptRoot 'design-tokens-baseline.tsv'
$number = '(?<![\w.])(?:0x[\da-fA-F]+|(?:\d+(?:\.\d+)?|\.\d+)[dfm]?)(?![\w.])'
$byteAlpha = '(?<![\w.])(?:[2-9]|[1-9]\d|1\d\d|2[0-4]\d|25[0-4])(?![\w.])'
$patterns = [ordered]@{
    # Literal corner radii.
    'radius' = "\b(?:new\s+CornerRadius\s*\([^)]*?$number|CornerRadius\s*=\s*$number)"
    # Literal font-family constructor arguments.
    'font-family' = '\b(?:FontFamily|Typeface)\s*\(\s*(?:\$@?|@\$?)?"[^"\r\n]+"'
    # Font sizes in properties, named arguments, and common text helpers.
    'font-size' = "\b(?:FontSize\s*=|(?:fontSize|size)\s*[:=])\s*$number|\b(?:Kit\.)?Text\s*\([^\r\n;]*?,\s*$number(?=\s*[,\)])|\bnew\s+GlyphIcon\s*\([^\r\n;]*?,\s*$number(?=\s*[,\)])"
    # Enum and numeric font weights.
    'font-weight' = "\bFontWeight\.[A-Za-z]+\b|\b(?:new\s+FontWeight\s*\(|FontWeight\s*=\s*)\s*$number"
    # Hex color strings and numeric RGB constructors.
    'color' = '"#[0-9a-fA-F]{6}(?:[0-9a-fA-F]{2})?"' + "|\bColor\.From(?:Argb|Rgb)\s*\([^)]*?$number"
    # Box shadow numeric geometry.
    'shadow' = "\b(?:Blur|OffsetX|OffsetY|Spread)\s*=\s*$number|\bnew\s+BoxShadow\s*\([^)]*?$number"
    # Opacity properties and brush arguments containing a non-endpoint literal.
    'opacity' = '\bOpacity\s*=\s*[^;\r\n,}]*?(?<![\w.])(?:0?\.\d*[1-9]\d*|[2-9]\d*(?:\.\d+)?|1\.\d*[1-9]\d*)(?![\w.])|\b\w*Brush\w*\s*\([^,;\r\n()]*,\s*[^,;\r\n()]*?(?<![\w.])0?\.\d*[1-9]\d*(?:[dDfFmM])?(?![\w.])[^,;\r\n()]*\)'
    # Literal byte alpha arguments, including a conditional pair.
    'alpha' = "\bColor\.FromArgb\s*\(\s*(?:(?:\(\s*byte\s*\)\s*)?$byteAlpha|[^,\r\n?]+\?\s*(?=(?:\(\s*byte\s*\)\s*$byteAlpha\s*:|\(\s*byte\s*\)\s*\d+\s*:\s*\(\s*byte\s*\)\s*$byteAlpha))(?:\(\s*byte\s*\)\s*\d+\s*:\s*\(\s*byte\s*\)\s*\d+))\s*,"
    # Durations in animation properties and helper calls.
    'tween' = "\b(?:Duration|AnimationDuration)\s*=\s*(?:TimeSpan\.From\w+\s*\()?\s*$number|\b(?:Anim\.Run|Tween\w*)\s*\([^,\r\n]+,\s*(?:TimeSpan\.From\w+\s*\()?\s*$number|\b(?:durationMs|animationMs)\s*:\s*$number"
    # Time spans outside animation duration properties.
    'timer' = "\bTimeSpan\.From(?:Milliseconds|Seconds|Minutes|Hours)\s*\(\s*$number"
    # Literal dimensions, spacing properties, and thickness constructors.
    'spacing' = "\b(?:MinWidth|MinHeight|MaxWidth|MaxHeight|Width|Height|Spacing|LetterSpacing|Margin|Padding|BorderThickness)\s*=\s*(?!new\s+Thickness\b)[^;\r\n,}]*?$number|\bnew\s+Thickness\s*\([^)]*?$number"
}

# Reject native interop tokens in AntiNotepad C# source outside Platform/, while ignoring comments and literal text.
function Mask-DesignCode([string] $Text, [bool] $AllowFont) {
    $script:allowFont = $AllowFont
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

function Read-CSharpString([int] $PrefixStart, [int] $DollarCount, [bool] $Verbatim, [bool] $Raw, [int] $QuoteCount, [int] $ContentStart, [bool] $KeepFont) {
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
                    if (-not $KeepFont) { Hide-CSharpRange $segmentStart ($position + $QuoteCount) }
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
                    if (-not $KeepFont) { Hide-CSharpRange $segmentStart ($openAt + $DollarCount) }
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
            if (-not $KeepFont) { Hide-CSharpRange $segmentStart ($position + 1) }
            return ($position + 1)
        }

        if ($DollarCount -gt 0 -and $text[$position] -eq '{') {
            $braceRun = 0
            while ($position + $braceRun -lt $length -and $text[$position + $braceRun] -eq '{') { $braceRun++ }
            $literalRun = $braceRun - ($braceRun % 2)
            if ($braceRun % 2 -eq 1) {
                $openAt = $position + $braceRun - 1
                if (-not $KeepFont) { Hide-CSharpRange $segmentStart ($openAt + 1) }
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

    if (-not $KeepFont) { Hide-CSharpRange $segmentStart $length }
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

        if ($inFormatText) { Hide-CSharpRange $position ($position + 1); $position++; continue }

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
            $keepFont = $script:allowFont -and $text.Substring(0, $prefixStart) -match '(?s)\b(?:new\s+)?(?:FontFamily|Typeface)\s*\(\s*$'
            if ($raw) {
                $position = Read-CSharpString $prefixStart $dollarCount $verbatim $true $quoteCount ($cursor + $quoteCount) $keepFont
            }
            elseif ($quoteCount -ge 1) {
                $position = Read-CSharpString $prefixStart $dollarCount $verbatim $false 1 ($cursor + 1) $keepFont
            }
            else { $position++ }
            $end = $position
            $literal = $text.Substring($prefixStart, $end - $prefixStart)
            $before = $text.Substring(0, $prefixStart)
            $isFont = $script:allowFont -and $before -match '(?s)\b(?:new\s+)?(?:FontFamily|Typeface)\s*\(\s*$'
            $isColor = $literal -match '^"#[0-9a-fA-F]{6}(?:[0-9a-fA-F]{2})?"$'
            if (($isFont -and $dollarCount -eq 0) -or $isColor) {
                for ($j = $prefixStart; $j -lt $end; $j++) {
                    if ($script:maskedChars[$j] -eq ' ' -and $text[$j] -ne ' ') { $script:maskedChars[$j] = $text[$j] }
                }
            }
            else {
                $script:maskedChars[$cursor] = '"'
                if ($end -gt $cursor + 1 -and $text[$end - 1] -eq '"') { $script:maskedChars[$end - 1] = '"' }
            }
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


# Extracts hits with stable source locations and readable source lines.
function Find-Hits([string]$code, [string]$file) {
    $masked = Mask-DesignCode $code $false
    $fontMasked = Mask-DesignCode $code $true
    $lines = $code -split "`n"
    foreach ($category in $patterns.Keys) {
        $searchText = if ($category -eq 'font-family') { $fontMasked } else { $masked }
        foreach ($match in [regex]::Matches($searchText, $patterns[$category])) {
            if ($category -eq 'timer') {
                $prefix = $masked.Substring([Math]::Max(0, $match.Index - 80), [Math]::Min(80, $match.Index))
                if ($prefix -match '(?s)(?:(?:Duration|AnimationDuration)\s*=\s*|\b(?:Anim\.Run|Tween\w*)\s*\([^,;\r\n]+,\s*)$') { continue }
            }
            $line = 1 + ([regex]::Matches($masked.Substring(0, $match.Index), "`n")).Count
            [pscustomobject]@{ category = $category; file = $file; line = $line; match = ($match.Value -replace '[\t\r\n]+', ' '); code = ($lines[$line - 1].Trim() -replace '\t', ' ') }
        }
    }
}

# Compares file counts and category totals, allowing moves funded by reductions.
function Compare-Counts($old, $new) {
    $keys = @($old.Keys + $new.Keys | Sort-Object -Unique)
    $errors = [Collections.Generic.List[string]]::new()
    $lower = $false
    foreach ($category in $patterns.Keys) {
        $categoryKeys = @($keys | Where-Object { $_.Split("`t")[1] -eq $category })
        $oldTotal = 0; $newTotal = 0; $decreases = 0
        foreach ($key in $categoryKeys) {
            $a = if ($old.ContainsKey($key)) { $old[$key] } else { 0 }; $b = if ($new.ContainsKey($key)) { $new[$key] } else { 0 }
            $oldTotal += $a; $newTotal += $b
            if ($b -lt $a) { $decreases += $a - $b; $lower = $true }
        }
        foreach ($key in $categoryKeys) {
            $a = if ($old.ContainsKey($key)) { $old[$key] } else { 0 }; $b = if ($new.ContainsKey($key)) { $new[$key] } else { 0 }
            if ($b -gt $a -and ($newTotal -gt $oldTotal -or ($b - $a) -gt $decreases)) {
                $errors.Add("$($key.Split("`t")[0]): $category $a -> $b")
            }
        }
    }
    return @{ Errors = $errors; Lower = $lower }
}

if ($SelfCheck) {
    $sample = @'
// CornerRadius = new CornerRadius(9)
var font = new FontFamily("Segoe UI new CornerRadius(99)");
var text = $"{new CornerRadius(5)}";
CornerRadius = ChromeTokens.Radius;
var ignored = "new CornerRadius(5)";
var verbatim = @"new CornerRadius(6)";
var raw = """new CornerRadius(7)""";
var nested = $"{Call("new CornerRadius(8)", /* new CornerRadius(9) */ new CornerRadius(10))}";
var ch = 'x';
var hex = Color.Parse("#123456");
FontWeight = FontWeight.Bold;
Opacity = opacity * 0.5;
var box = new BoxShadow(0, 2, 5);
Width = 42;
Anim.Run(target, TimeSpan.FromMilliseconds(200));
var timer = TimeSpan.FromMilliseconds(30);
'@
    $hits = @(Find-Hits $sample 'fixture.cs')
    if (@($hits | Where-Object category -eq 'radius').Count -ne 2 -or @($hits | Where-Object category -eq 'font-family').Count -ne 1 -or @($hits | Where-Object category -eq 'color').Count -ne 1 -or @($hits | Where-Object category -eq 'font-weight').Count -ne 1 -or @($hits | Where-Object category -eq 'opacity').Count -ne 1 -or @($hits | Where-Object category -eq 'shadow').Count -ne 1 -or @($hits | Where-Object category -eq 'spacing').Count -ne 1 -or @($hits | Where-Object category -eq 'tween').Count -ne 1 -or @($hits | Where-Object category -eq 'timer').Count -ne 1) { throw 'Scanner fixture failed.' }
    $brushFixtures = @('new SolidColorBrush(color, .45)', 'RenderPaint.Brush(color, 0.3)', 'Brush(color, dark ? .2 : .1)')
    foreach ($fixture in $brushFixtures) {
        if (@(Find-Hits $fixture 'fixture.cs' | Where-Object category -eq 'opacity').Count -ne 1) { throw "Brush fixture failed: $fixture" }
    }
    foreach ($fixture in @('Color.FromArgb(220, c.R, c.G, c.B)', 'Color.FromArgb(dark ? (byte)110 : (byte)40, 0, 0, 0)')) {
        if (@(Find-Hits $fixture 'fixture.cs' | Where-Object category -eq 'alpha').Count -ne 1) { throw "Alpha fixture failed: $fixture" }
    }
    foreach ($fixture in @('Brush(color, Token.Opacity)', 'new SolidColorBrush(color, 0)', 'Brush(color, 1.0)', 'Color.FromArgb(0, c.R, c.G, c.B)', 'Color.FromArgb(1, c.R, c.G, c.B)', 'Color.FromArgb(255, c.R, c.G, c.B)', 'Color.FromArgb(dark ? (byte)0 : (byte)255, 0, 0, 0)', 'Color.FromArgb(Token.Alpha, 0, 0, 0)')) {
        if (@(Find-Hits $fixture 'fixture.cs' | Where-Object { $_.category -in @('opacity', 'alpha') }).Count -ne 0) { throw "Endpoint fixture failed: $fixture" }
    }
    $old = @{ "a`tradius" = 1 }; $rise = @{ "a`tradius" = 2 }; $move = @{ "b`tradius" = 1 }
    if ((Compare-Counts $old $rise).Errors.Count -eq 0 -or (Compare-Counts $old $move).Errors.Count -ne 0) { throw 'Ratchet fixture failed.' }
    Write-Output 'TOKENS SELFCHECK PASS'; exit 0
}

$hits = @(Get-ChildItem (Join-Path $root 'product/AntiNotepad') -Recurse -File -Filter '*.cs' | Where-Object { $_.FullName -notmatch '[\\/](?:bin|obj|Theme|SelfTest|Calc)[\\/]' } | ForEach-Object {
    $relative = [IO.Path]::GetRelativePath($root, $_.FullName).Replace('\', '/')
    Find-Hits ([IO.File]::ReadAllText($_.FullName)) $relative
})
$counts = @{}
foreach ($hit in $hits) { $key = "$($hit.file)`t$($hit.category)"; if (-not $counts.ContainsKey($key)) { $counts[$key] = 0 }; $counts[$key]++ }
if ($Inventory) {
    $rows = @("category`tfile`tline`tmatched text`tcode") + @($hits | Sort-Object file, line, category | ForEach-Object { "$($_.category)`t$($_.file)`t$($_.line)`t$($_.match)`t$($_.code)" })
    [IO.File]::WriteAllLines([IO.Path]::GetFullPath($Inventory), [string[]]$rows)
}
if ($Update) {
    $rows = @("file`tcategory`tcount") + @($counts.Keys | Sort-Object | ForEach-Object { "$_`t$($counts[$_])" })
    [IO.File]::WriteAllLines($baselinePath, [string[]]$rows)
    Write-Output 'TOKENS baseline updated'; exit 0
}
if (-not (Test-Path $baselinePath)) { throw 'Tokens baseline is missing; run -Update.' }
$old = @{}
foreach ($row in Import-Csv -Delimiter "`t" $baselinePath) { $old["$($row.file)`t$($row.category)"] = [int]$row.count }
$comparison = Compare-Counts $old $counts
if ($comparison.Errors.Count) {
    $comparison.Errors | ForEach-Object { Write-Output "TOKENS FAIL: $_" }
    Write-Output 'Use a token from Theme/ or Motion/, or raise the baseline with -Update and give the reason on the item card.'
    exit 1
}
if ($comparison.Lower) { Write-Output 'TOKENS baseline can be lowered: run -Update' }
Write-Output 'TOKENS PASS'
