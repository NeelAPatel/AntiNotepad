param(
  [string]$Key = 'D', # D = legacy dev instance; S = the installed instance
  [string]$Slot,
  [switch]$NewNote
)
# Presses a Ctrl+Alt+Shift dev hotkey system-wide while the pad is hidden.
$slotColors = @('Blue', 'Red', 'Green', 'Purple', 'Orange', 'Silver')
if (-not [string]::IsNullOrWhiteSpace($Slot)) {
  $slotIndex = -1
  for ($index = 0; $index -lt $slotColors.Count; $index++) {
    if ([string]::Equals($slotColors[$index], $Slot.Trim(), [StringComparison]::OrdinalIgnoreCase)) {
      $slotIndex = $index
      break
    }
  }
  if ($slotIndex -lt 0) { throw "-Slot must be one of: $($slotColors -join ', ')." }
  $slotNumber = $slotIndex + 1
  $keyName = if ($NewNote) { "F$slotNumber" } else { [string]$slotNumber }
  $vk = if ($NewNote) { 0x70 + $slotIndex } else { 0x30 + $slotNumber }
}
else {
  if ($NewNote) { throw '-NewNote requires -Slot.' }
  if ($Key -notmatch '\A[A-Za-z0-9]\z') {
    throw '-Key must be exactly one letter A-Z or digit 0-9.'
  }
  $keyName = $Key.ToUpperInvariant()
  $vk = [byte][char]$keyName
}
Add-Type @"
using System.Runtime.InteropServices;
public static class K { [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, System.UIntPtr extra); }
"@
$keys = 0x11, 0x12, 0x10   # Ctrl, Alt, Shift
try {
  foreach ($k in $keys) { [K]::keybd_event($k, 0, 0, [UIntPtr]::Zero) }
  [K]::keybd_event([byte]$vk, 0, 0, [UIntPtr]::Zero)
}
finally {
  [K]::keybd_event([byte]$vk, 0, 2, [UIntPtr]::Zero)
  foreach ($k in $keys) { [K]::keybd_event($k, 0, 2, [UIntPtr]::Zero) }
}
