# Builds product/Antipad/antipad.ico (PNG-compressed entries 256/64/48/32/16): an amber rounded page with ink lines.
Add-Type -AssemblyName System.Drawing
$sizes = 256, 64, 48, 32, 16
$pngs = foreach ($s in $sizes) {
    $bmp = New-Object System.Drawing.Bitmap $s, $s
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $g.Clear([System.Drawing.Color]::Transparent)
    $r = [single]($s * 0.2); $pad = [single]($s * 0.08)
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $x = $pad; $y = $pad * 0.6; $w = $s - 2 * $pad; $h = $s - 1.2 * $pad
    $path.AddArc($x, $y, $r, $r, 180, 90); $path.AddArc($x + $w - $r, $y, $r, $r, 270, 90)
    $path.AddArc($x + $w - $r, $y + $h - $r, $r, $r, 0, 90); $path.AddArc($x, $y + $h - $r, $r, $r, 90, 90); $path.CloseFigure()
    $g.FillPath((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(242, 184, 75))), $path)
    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(43, 42, 39)), ([single][Math]::Max(1.5, $s * 0.075))
    $pen.StartCap = 'Round'; $pen.EndCap = 'Round'
    foreach ($line in @(@(0.30, 0.74), @(0.48, 0.62), @(0.66, 0.46))) {
        $g.DrawLine($pen, [single]($s * 0.27), [single]($s * $line[0]), [single]($s * $line[1]), [single]($s * $line[0]))
    }
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    , $ms.ToArray()
}
$out = New-Object System.IO.MemoryStream
$wr = New-Object System.IO.BinaryWriter $out
$wr.Write([uint16]0); $wr.Write([uint16]1); $wr.Write([uint16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $s = $sizes[$i]; $len = $pngs[$i].Length
    $wr.Write([byte]($(if ($s -ge 256) { 0 } else { $s }))); $wr.Write([byte]($(if ($s -ge 256) { 0 } else { $s })))
    $wr.Write([byte]0); $wr.Write([byte]0); $wr.Write([uint16]1); $wr.Write([uint16]32)
    $wr.Write([uint32]$len); $wr.Write([uint32]$offset); $offset += $len
}
foreach ($p in $pngs) { $wr.Write($p) }
[System.IO.File]::WriteAllBytes((Join-Path $PSScriptRoot '..\product\Antipad\antipad.ico'), $out.ToArray())
"ico written"
