# A35-ux-pass-s3c: overlays, menus and messages (letter C)

Read `.coord/tasks/A35-ux-pass-s3-common.md` first and follow it.

Screens: the **search overlay** (query, results, list/grid, trash view, restore and delete from trash, close), the
**pad context menu** and **editor context menu** (`UI/PadMenu.cs`, `Editor/MenuEntry.cs`, `Editor/PadView.cs` menu part),
the **tray menu** (`Platform/`), **toasts** (undo toasts, saved label), the **save-failure card**
(`UI/PadWindow.Layout.cs`), the **Help / shortcut map overlay** (`UI/HelpOverlay.cs`), and the **completion popup**
(`Editor/Completion.cs`). Also `UI/SearchOverlay.cs`, `UI/SaveTiming.cs`, `UI/Shortcuts.cs`.

Focus: Esc and other exits (N3), what each toast says and whether Undo works for each destructive path (U1, N1, N9),
error wording on the save-failure card (N9, N2), menu length and grouping (H1, G1), accelerator text shown in menus (KB3,
N7), destructive items apart from frequent ones (U2), help reachable and scannable (N10, N6), focus after close (S1),
glyphs inline with text (I6), hit boxes (T1, T2).
