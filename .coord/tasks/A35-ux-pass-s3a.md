# A35-ux-pass-s3a: pad screens (letter A)

Read `.coord/tasks/A35-ux-pass-s3-common.md` first and follow it.

Screens: the pad in **Standard** (stack of notes) and **Permanent** (9 slots) views, **split** (two panes), the top bar,
the bottom bar and pager (dots, count, arrows, Delete/Clear, New note, swap), bar styles (docked, overlay, float) and
chrome fade/reveal as behavior (not timing), and the **dev badge**. Files: `UI/PadWindow*.cs`, `UI/PadBar.cs`,
`UI/Kit.cs` (buttons), `UI/NoteSwipe.cs`, `UI/Shortcuts.cs`, `Theme/ChromeTokens.cs`. Context:
`.coord/research/A34-edge-cases.md` (decisions already taken; check they hold, do not re-decide them).

Focus: destructive actions and undo (U1, U2, N3, N5), hover-only controls (DN1), layout stability between states (L1),
hit boxes (T1, T2, F1), tooltips with keys and automation names (N6, KB3, I5), keyboard paths and focus between split
panes (KB1, KB4), feedback after actions (N1, DN3), mapping in split (DN2), the Standard/Permanent model (DN4).
