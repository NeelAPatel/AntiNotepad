# Displays the current Windows clipboard formats and text for source captures.
if ([Threading.Thread]::CurrentThread.ApartmentState -ne 'STA') {
    throw 'Run with powershell.exe -STA -File tools/clip-dump.ps1.'
}
Add-Type -AssemblyName System.Windows.Forms
$data = [Windows.Forms.Clipboard]::GetDataObject()
if ($null -eq $data) { Write-Output 'Clipboard is empty.'; return }
foreach ($format in $data.GetFormats($false)) {
    try {
        $value = $data.GetData($format, $false)
        $size = if ($value -is [string]) { [Text.Encoding]::UTF8.GetByteCount($value) } elseif ($value -is [byte[]]) { $value.Length } elseif ($value -is [IO.MemoryStream]) { $value.Length } else { 'unknown' }
        Write-Output ("{0}: {1} bytes" -f $format, $size)
        if ($value -is [string] -and $format -in @('HTML Format', 'UnicodeText', 'Text', 'OEMText')) { Write-Output $value }
    } catch { Write-Output ("{0}: unavailable ({1})" -f $format, $_.Exception.Message) }
}
