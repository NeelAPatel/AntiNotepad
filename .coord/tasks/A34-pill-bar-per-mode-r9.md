# A34-pill-bar-per-mode-r9: rework round, three proven review findings

Role: terra at CODEX_EFFORT=medium (three small fixes in known places; docs/agents.md). Work only in this worktree. Never commit, never launch AntiNotepad, no ports. Set `AVALONIA_TELEMETRY_OPTOUT=1`. Build into `build/A34-check`, run `--selftest` there, delete the folder after.
Read: `.coord/reports/A34-pill-bar-per-mode.review.md` (the three findings with evidence), `.claude/rules/coding.md`, the card's Plan. `Theme/Paint.cs` is now in the card's `## Files`.

## Why this round
The lens review returned `VERDICT: rework`. Fix exactly these lines, nothing else:

1. **[major] Disabled Delete and New note do not look dimmed** (`UI/PadWindow.Bar.cs` where `IsEnabled` is set; `UI/Kit.cs` `IconButton`/`GlyphIcon`). A disabled bar button must read as dimmed: draw the glyph and hover face at a reduced opacity from a new token (`Paint` or `ChromeTokens`, one value, both themes) when the button is disabled, and no hover wash or hover glyph color while disabled. Fix it in the shared `Kit.IconButton` path so every icon button gets it (existing enabled buttons must look unchanged). Add one small `--selftest` capability check in `SelfTest/Bar.cs`: a disabled icon button's rendered glyph opacity is below an enabled one's, and an enabled one is unchanged.
2. **[minor] Save-failure card loses its shadow in Float and Quiet** (`UI/PadWindow.Layout.cs` near line 731 reads `_pill.BoxShadow`, whose meaning changed). Build one palette-based shadow helper from the existing `ChromeTokens.PillShadow*` values and use it for the pill, the toast and the save-failure card, so none of them reads another control's shadow. Pill and toast keep their exact current looks.
3. **[minor] Missing summaries** (`.claude/rules/coding.md`, comments 1 and 2): add a concise `///` summary to `InitializeBar`, `BuildBarButtons`, `ConfigureBarStyle`, `BuildPill`, `SelectBarSlot`, `DeleteFromBar`, `SwapTarget`, `SwapView` and the `BarContent` type in `UI/PadWindow.Bar.cs`, and a 1-3 line file header on `UI/Kit.cs`. No history in comments.

## Done means (offline only)
Build clean (0 warnings), `--selftest` passes, `pwsh -NoProfile -File tools/gate.ps1 -Task A34-pill-bar-per-mode -Offline` passes. Pinned design hash: if (and only if) the design dump changes because of fix 1 or 2, do NOT edit `SelfTest/Design.cs`; say so in the report. Report in `.coord/reports/A34-pill-bar-per-mode-r9.md` with sections: Change list, Behavior changes, Plan deviations, Gate result. Final message 5 lines max.
