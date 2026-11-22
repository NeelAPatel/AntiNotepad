[CmdletBinding()]
param([switch]$Write)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$doc = Join-Path $root 'docs/design-contract.md'
$sizeLimit = 600
$start = '<!-- design-tokens:start -->'
$end = '<!-- design-tokens:end -->'
$rows = [Collections.Generic.List[string]]::new()
foreach ($file in Get-ChildItem (Join-Path $root 'product/AntiNotepad/Theme') -Filter '*.cs' | Sort-Object Name) {
    $source = [IO.File]::ReadAllText($file.FullName)
    $source = [regex]::Replace($source, '(?m)//[^\r\n]*', '')
    foreach ($declaration in [regex]::Matches($source, '(?s)\bpublic\s+const\s+\w+\s+([^;]+);')) {
        foreach ($name in [regex]::Matches($declaration.Groups[1].Value, '(?:^|,)\s*(\w+)\s*=\s*("[^"]*"|[^,]+)')) {
            $value = $name.Groups[2].Value.Trim().Replace('|', '&#124;')
            $rows.Add('| `' + $file.BaseName + '.' + $name.Groups[1].Value + '` | `' + $value + '` | `' + $file.Name + '` |')
        }
    }
}
$icons = [IO.File]::ReadAllText((Join-Path $root 'product/AntiNotepad/Icons.cs'))
foreach ($match in [regex]::Matches($icons, '\bpublic\s+static\s+readonly\s+Icon\s+(\w+)\s*=\s*(Segoe|Fluent)\("[^"]*",\s*(0x[0-9A-Fa-f]+)')) {
    $rows.Add('| `Icons.' + $match.Groups[1].Value + '` | `' + $match.Groups[2].Value + ' ' + $match.Groups[3].Value + '` | `Icons.cs` |')
}
$generated = "`n| Name | Value (source expression) | Source |`n| --- | --- | --- |`n" + (($rows | Sort-Object -CaseSensitive) -join "`n") + "`n"
$text = [IO.File]::ReadAllText($doc).Replace("`r`n", "`n")
if (([regex]::Matches($text, [regex]::Escape($start))).Count -ne 1 -or ([regex]::Matches($text, [regex]::Escape($end))).Count -ne 1) { throw 'Contract requires exactly one pair of table markers.' }
$a = $text.IndexOf($start) + $start.Length; $b = $text.IndexOf($end)
if ($b -lt $a) { throw 'Contract table markers are out of order.' }
$expected = $text.Substring(0, $a) + $generated + $text.Substring($b)
$sizeRows = Import-Csv (Join-Path $root 'tools/size-exceptions.tsv') -Delimiter "`t"
$sizeTableRows = @($sizeRows | ForEach-Object { '| `' + $_.file + '` | ' + $_.'max lines' + ' | ' + $_.reason + ' |' })
$sizeStart = '<!-- design-size-exceptions:start -->'; $sizeEnd = '<!-- design-size-exceptions:end -->'
if (([regex]::Matches($expected, [regex]::Escape($sizeStart))).Count -ne 1 -or ([regex]::Matches($expected, [regex]::Escape($sizeEnd))).Count -ne 1) { throw 'Contract requires exactly one pair of size exception markers.' }
$sa = $expected.IndexOf($sizeStart) + $sizeStart.Length; $sb = $expected.IndexOf($sizeEnd)
if ($sb -lt $sa) { throw 'Size exception markers are out of order.' }
$sizeTable = "`n| File | Max lines | Reason |`n| --- | ---: | --- |`n" + ($sizeTableRows -join "`n") + "`n"
$expected = $expected.Substring(0, $sa) + $sizeTable + $expected.Substring($sb)
$exceptions = Import-Csv (Join-Path $root 'tools/design-exceptions.tsv') -Delimiter "`t"
$exceptionRows = @($exceptions | ForEach-Object { '| `' + $_.file + '` | `' + $_.category + '` | ' + $_.count + ' | ' + $_.reason + ' |' })
$exceptionStart = '<!-- design-exceptions:start -->'; $exceptionEnd = '<!-- design-exceptions:end -->'
if (([regex]::Matches($expected, [regex]::Escape($exceptionStart))).Count -ne 1 -or ([regex]::Matches($expected, [regex]::Escape($exceptionEnd))).Count -ne 1) { throw 'Contract requires exactly one pair of exception markers.' }
$ea = $expected.IndexOf($exceptionStart) + $exceptionStart.Length; $eb = $expected.IndexOf($exceptionEnd)
if ($eb -lt $ea) { throw 'Exception markers are out of order.' }
$exceptionTable = "`n| File | Category | Count | Reason |`n| --- | --- | --- | --- |`n" + ($exceptionRows -join "`n") + "`n"
$expected = $expected.Substring(0, $ea) + $exceptionTable + $expected.Substring($eb)
if ($Write) { [IO.File]::WriteAllText($doc, $expected, [Text.UTF8Encoding]::new($false)); Write-Output 'CONTRACT tables regenerated'; exit 0 }
if ($text -cne $expected) { Write-Output 'CONTRACT FAIL: token or icon table differs; run check-design-contract.ps1 -Write'; exit 1 }

function Get-FileLines {
    param([AllowEmptyString()][string]$Content)
    if ([string]::IsNullOrEmpty($Content)) { return 0 }
    $lineBreaks = [regex]::Matches($Content, "`r`n|`n|`r").Count
    if ($Content -match '(?:\r\n|\n|\r)$') { return $lineBreaks }
    return $lineBreaks + 1
}

$known = @{}
foreach ($row in $sizeRows) {
    $path = [string]$row.file
    $maximum = 0
    if ([string]::IsNullOrWhiteSpace($path) -or $path -notmatch '^product/.+\.cs$') { throw "Invalid size exception path: '$path'." }
    if ($known.ContainsKey($path)) { throw "Duplicate size exception: $path." }
    if (-not [int]::TryParse([string]$row.'max lines', [ref]$maximum) -or $maximum -le $sizeLimit) { throw "Invalid max lines for $path; it must be an integer greater than $sizeLimit." }
    if ([string]::IsNullOrWhiteSpace([string]$row.reason)) { throw "Missing size exception reason for $path." }
    $fullPath = Join-Path $root ($path.Replace('/', [IO.Path]::DirectorySeparatorChar))
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) { throw "Listed size exception file is missing: $path." }
    $count = Get-FileLines ([IO.File]::ReadAllText($fullPath))
    $known[$path] = $maximum
    if ($count -gt $maximum) { Write-Output "SIZE FAIL: $path has $count lines; max is $maximum."; exit 1 }
    if ($count -lt $maximum) { Write-Output "SIZE NOTE: $path has shrunk to $count lines; lower the max from $maximum." }
}
$oversized = @()
foreach ($file in Get-ChildItem (Join-Path $root 'product') -Recurse -File -Filter '*.cs') {
    $relative = [IO.Path]::GetRelativePath($root, $file.FullName).Replace('\', '/')
    if ((Get-FileLines ([IO.File]::ReadAllText($file.FullName))) -gt $sizeLimit) { $oversized += $relative }
}
$unlisted = @($oversized | Where-Object { -not $known.ContainsKey($_) })
if ($unlisted.Count -gt 0) { Write-Output ('SIZE FAIL: product C# file(s) over ' + $sizeLimit + ' lines must be listed: ' + ($unlisted -join ', ')); exit 1 }
Write-Output 'CONTRACT PASS'
