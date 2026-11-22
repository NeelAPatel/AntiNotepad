# A35d-spacing-tokens fix 4: "Open at" tiles reflow into rows

Role: quick. Hand-test finding from the user, in their words: "the Open at option things should reflow into multiple rows".
In a narrow Settings window, the five "Open at" tiles (System page, Opening group) stay in one row and the labels clip.
For example, "Above the tray" is cut off. This branch widened the page's right margin from 28 to 32, so the clipping
starts 4 DIP sooner.

Site: `product/AntiNotepad/UI/SettingsWindow.cs`, `OpenAtTiles()`. It uses `new UniformGrid { Columns = 5 }` and gives
`Tile(..., 104)` a width of 104.

Fix: the tiles keep their look and size. They wrap onto more rows when the card is too narrow to hold all five, and
they stay on one row when it is wide enough. Pick the smallest change that does this. A `WrapPanel` with the existing
tile width is the likely answer. The gaps between tiles must come from `Space` tokens, and a new size goes in
`ChromeTokens.Size`. If tiles stretch to fill the row today, say in the report whether the wrapped layout still fills it,
and keep it left-aligned if it cannot.

Rules: read `AGENTS.md` and `.claude/rules/coding.md`. Edit only `UI/SettingsWindow.cs`, `Theme/ChromeTokens.cs` if a token
is needed, and `DesignHash` in `SelfTest/Design.cs` if the dump moves. Never switch, merge or commit. Never launch or
kill AntiNotepad, never run `--quit`, and never edit `.coord/STATUS.md` or the card. Use offline checks only. Build into
`build/A35d-spacing-tokens-check` with `AVALONIA_TELEMETRY_OPTOUT=1`. Run `--design-dump` before and after, and list
the changed lines. Re-pin `DesignHash`, run `--selftest`, then delete the folder.
Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A35d-spacing-tokens -Base 3b6ecb5 -Offline`. Done means 0 build
warnings, `SELFTEST PASS`, and the gate passing.

Report `.coord/reports/A35d-spacing-tokens-fix-4.md`, short. Include what you changed, the dump diff, the new hash, and
the checks you ran.
