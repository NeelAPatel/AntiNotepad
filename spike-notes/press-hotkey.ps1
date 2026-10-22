# Presses Ctrl+Alt+Shift+S system-wide (dev aid for testing the Antipad hotkey while it is hidden).
Add-Type @"
using System.Runtime.InteropServices;
public static class K { [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, System.UIntPtr extra); }
"@
$keys = 0x11, 0x12, 0x10   # Ctrl, Alt, Shift
foreach ($k in $keys) { [K]::keybd_event($k, 0, 0, [UIntPtr]::Zero) }
[K]::keybd_event(0x53, 0, 0, [UIntPtr]::Zero); [K]::keybd_event(0x53, 0, 2, [UIntPtr]::Zero)
foreach ($k in $keys) { [K]::keybd_event($k, 0, 2, [UIntPtr]::Zero) }
