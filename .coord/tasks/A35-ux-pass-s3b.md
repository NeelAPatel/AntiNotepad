# A35-ux-pass-s3b: Settings window (letter B)

Read `.coord/tasks/A35-ux-pass-s3-common.md` first and follow it.

Screen: the **Settings window**, every page (Theme, UI, Editor, Feel, System, Shortcuts, Storage, About), its sidebar,
settings search, close button, expanders, dropdowns, toggles, segmented choices, swatches, preview tiles, the shortcut
recorder (record, clear, reset, restart), Restore default settings, notes-folder actions. Files: `UI/SettingsWindow.cs`,
`UI/SettingsParts.cs`, `UI/HotkeyRecorder.cs`, `UI/Kit.cs`, `UI/MotionPreview.cs` (as a control, not its timing),
`Model/Settings.cs` for defaults.

Focus: labels say what happens (N2, W1 sentence case, US spelling), destructive or hard-to-undo actions such as Restore
defaults, reset shortcuts, moving the notes folder (U1, N3, N5, N9), grouping and section order (G1, H1, P1), Tab order
and focus rings (KB2, C3), every control reachable by keyboard (KB1), search finds what it should (S1), hit boxes (T1,
T2), consistency of the same control kind across pages (N4, G2), any setting that changes nothing visible (N1).
