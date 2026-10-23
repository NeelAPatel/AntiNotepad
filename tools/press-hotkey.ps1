param([string]$Key = 'D')  # D = dev instance (Ctrl+Alt+Shift+D); S = the real one
# Presses Ctrl+Alt+Shift+<Key> system-wide (dev aid for testing the AntiNotepad hotkey while it is hidden).
Add-Type @"
using System.Runtime.InteropServices;
public static class K { [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, System.UIntPtr extra); }
"@
$keys = 0x11, 0x12, 0x10   # Ctrl, Alt, Shift
foreach ($k in $keys) { [K]::keybd_event($k, 0, 0, [UIntPtr]::Zero) }
$vk = [byte][char]$Key.ToUpper()
[K]::keybd_event($vk, 0, 0, [UIntPtr]::Zero); [K]::keybd_event($vk, 0, 2, [UIntPtr]::Zero)
foreach ($k in $keys) { [K]::keybd_event($k, 0, 2, [UIntPtr]::Zero) }
