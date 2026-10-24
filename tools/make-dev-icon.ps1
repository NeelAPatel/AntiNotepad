# Builds product/AntiNotepad/antinotepad-dev.ico by tinting every frame in
# antinotepad.ico. RGB changes only; source alpha, frame geometry, and ICO sizes
# remain intact. The warm amber page becomes a saturated cool blue for dev.
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$sourcePath = Join-Path $PSScriptRoot '..\product\AntiNotepad\antinotepad.ico'
$outputPath = Join-Path $PSScriptRoot '..\product\AntiNotepad\antinotepad-dev.ico'
$sourceBytes = [System.IO.File]::ReadAllBytes($sourcePath)

if ($sourceBytes.Length -lt 6 -or
    [System.BitConverter]::ToUInt16($sourceBytes, 0) -ne 0 -or
    [System.BitConverter]::ToUInt16($sourceBytes, 2) -ne 1) {
    throw "Source is not a valid ICO file: $sourcePath"
}

$frameCount = [int][System.BitConverter]::ToUInt16($sourceBytes, 4)
$directoryEnd = 6 + (16 * $frameCount)
if ($frameCount -lt 1 -or $directoryEnd -gt $sourceBytes.Length) {
    throw "Source ICO has an invalid image directory: $sourcePath"
}

$frames = [System.Collections.Generic.List[object]]::new()
$targetHueDegrees = 207.0
$targetHueSector = $targetHueDegrees / 60.0
$targetHueSectorXFactor = 1.0 - [Math]::Abs(($targetHueSector % 2.0) - 1.0)

for ($index = 0; $index -lt $frameCount; $index++) {
    $entryOffset = 6 + (16 * $index)
    $widthByte = [byte]$sourceBytes[$entryOffset]
    $heightByte = [byte]$sourceBytes[$entryOffset + 1]
    $frameLength = [int][System.BitConverter]::ToUInt32($sourceBytes, $entryOffset + 8)
    $frameOffset = [int][System.BitConverter]::ToUInt32($sourceBytes, $entryOffset + 12)

    if ($frameLength -lt 1 -or $frameOffset -lt $directoryEnd -or
        $frameOffset -gt ($sourceBytes.Length - $frameLength)) {
        throw "Source ICO entry $index points outside the file."
    }

    $frameBytes = [byte[]]::new($frameLength)
    [System.Array]::Copy($sourceBytes, $frameOffset, $frameBytes, 0, $frameLength)
    $sourceStream = [System.IO.MemoryStream]::new($frameBytes, $false)
    $sourceImage = $null
    $outputImage = $null
    $outputStream = $null
    try {
        $sourceImage = [System.Drawing.Bitmap]::new($sourceStream)
        $outputImage = [System.Drawing.Bitmap]::new(
            $sourceImage.Width,
            $sourceImage.Height,
            [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)

        for ($y = 0; $y -lt $sourceImage.Height; $y++) {
            for ($x = 0; $x -lt $sourceImage.Width; $x++) {
                $pixel = $sourceImage.GetPixel($x, $y)
                $red = [double]$pixel.R / 255.0
                $green = [double]$pixel.G / 255.0
                $blue = [double]$pixel.B / 255.0
                $maximum = [Math]::Max($red, [Math]::Max($green, $blue))
                $minimum = [Math]::Min($red, [Math]::Min($green, $blue))
                $delta = $maximum - $minimum

                # Keep transparent pixels and near-neutral ink unchanged. For
                # the colored page and its antialiased edges, rotate hue in HSL
                # while retaining source saturation and lightness.
                if ($pixel.A -eq 0 -or $delta -lt 0.025) {
                    $outputImage.SetPixel($x, $y, $pixel)
                    continue
                }

                $lightness = ($maximum + $minimum) / 2.0
                if ($lightness -le 0.5) {
                    $saturation = $delta / ($maximum + $minimum)
                }
                else {
                    $saturation = $delta / (2.0 - $maximum - $minimum)
                }
                if ($saturation -lt 0.10) {
                    $outputImage.SetPixel($x, $y, $pixel)
                    continue
                }

                $chroma = (1.0 - [Math]::Abs((2.0 * $lightness) - 1.0)) * $saturation
                $secondary = $chroma * $targetHueSectorXFactor
                $match = $lightness - ($chroma / 2.0)
                $newRed = $match
                $newGreen = $secondary + $match
                $newBlue = $chroma + $match

                $r8 = [int][Math]::Round($newRed * 255.0, [MidpointRounding]::AwayFromZero)
                $g8 = [int][Math]::Round($newGreen * 255.0, [MidpointRounding]::AwayFromZero)
                $b8 = [int][Math]::Round($newBlue * 255.0, [MidpointRounding]::AwayFromZero)
                $outputImage.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($pixel.A, $r8, $g8, $b8))
            }
        }

        $outputStream = [System.IO.MemoryStream]::new()
        $outputImage.Save($outputStream, [System.Drawing.Imaging.ImageFormat]::Png)
        $frames.Add([pscustomobject]@{
            WidthByte  = $widthByte
            HeightByte = $heightByte
            Reserved   = [byte]$sourceBytes[$entryOffset + 3]
            ColorCount = [byte]$sourceBytes[$entryOffset + 2]
            Planes     = [System.BitConverter]::ToUInt16($sourceBytes, $entryOffset + 4)
            BitCount   = [System.BitConverter]::ToUInt16($sourceBytes, $entryOffset + 6)
            PngBytes   = $outputStream.ToArray()
            Width      = $sourceImage.Width
            Height     = $sourceImage.Height
        })
    }
    finally {
        if ($outputStream) { $outputStream.Dispose() }
        if ($outputImage) { $outputImage.Dispose() }
        if ($sourceImage) { $sourceImage.Dispose() }
        $sourceStream.Dispose()
    }
}

$icoStream = [System.IO.MemoryStream]::new()
$writer = [System.IO.BinaryWriter]::new($icoStream)
try {
    $writer.Write([uint16]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]$frames.Count)

    $nextImageOffset = 6 + (16 * $frames.Count)
    foreach ($frame in $frames) {
        $writer.Write([byte]$frame.WidthByte)
        $writer.Write([byte]$frame.HeightByte)
        $writer.Write([byte]$frame.ColorCount)
        $writer.Write([byte]$frame.Reserved)
        $writer.Write([uint16]$frame.Planes)
        $writer.Write([uint16]$frame.BitCount)
        $writer.Write([uint32]$frame.PngBytes.Length)
        $writer.Write([uint32]$nextImageOffset)
        $nextImageOffset += $frame.PngBytes.Length
    }

    foreach ($frame in $frames) {
        $writer.Write([byte[]]$frame.PngBytes)
    }

    [System.IO.File]::WriteAllBytes($outputPath, $icoStream.ToArray())
}
finally {
    $writer.Dispose()
    $icoStream.Dispose()
}

$dimensions = ($frames | ForEach-Object { '{0}x{1}' -f $_.Width, $_.Height }) -join ', '
Write-Output ("Wrote {0} ({1} frames: {2}; hue {3} degrees)." -f $outputPath, $frames.Count, $dimensions, $targetHueDegrees)
