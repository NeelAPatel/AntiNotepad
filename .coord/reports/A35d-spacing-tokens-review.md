**Verdict: fixes needed.** Report content follows; `.coord/reports/A35d-spacing-tokens-review.md` was not written because this session prohibits file writes.

### 1. Value drift

None. Token values and derives preserve the original numbers outside approved snaps, including editor hints, overlay bottom margins, menu icon size, and toggle travel.

### 2. Wrong snaps

None. Rows 1–19 match the approved sites and values.

The existing before/after dumps have the reported hashes and identical control paths. Their differences are exactly 200 margins and 1,880 corner radii.

### 3. Lost logic

None. The survival script exited 1 and listed six missing lines; each is accounted for:

| Flagged line | Judgment |
|---|---|
| `PadMenu.cs` null-icon placeholder | Preserved at line 273; `16 + 2` retains 18×18. |
| `SettingsParts.cs` dropdown chevron | Preserved at line 357; margin remains 12. |
| `SettingsWindow.cs` search icon | Preserved at line 203; margin changes 10→12 under snap row 3. |
| `UI.cs` Fluent-theme guard | Moved into `Program.cs:209`. |
| `UI.cs` Fluent-theme insertion | Moved into `Program.cs:211`. |
| `UI.cs` AvaloniaEdit style insertion | Moved into `Program.cs:212`. |

`DataSafetyFixture` invokes that shared setup at `SelfTest/UI.cs:33`.

### 4. Fluent corner setup

None. App initialization and both selftest files use `App.ConfigureFluentStyles`. Theme/style insertion remains guarded. The app calls it once from `Initialize`; the resource assignment does not override local `Shape` radii.

### 5. Scanner

**1. [minor] Stroke scanning misses the product’s existing pen factory.**  
`tools/check-design-tokens.ps1:314` recognizes only `new Pen(...)` for strokes. Consequently, actual literal thicknesses at `product/AntiNotepad/Editor/Renderers.cs:215`, `:216`, and `:359`—all `RenderPaint.Pen(...)` calls—cannot produce stroke hits. The new fixtures exercise only the constructor, leaving this blind spot unchecked.

**Fix:** Recognize `RenderPaint.Pen` thickness arguments and add literal-hit/token-no-hit fixtures, then recount the baseline.

The supplied fixtures are real cases and pass, including `Thickness(0, 4, 0, 0)`.

### 6. Coding standard

**2. [minor] Required headers and summaries remain missing in touched files.**  
Seven files still open with `using` rather than the required purpose header: `Editor/Completion.cs:1`, `Editor/Renderers.cs:1`, `Platform/TaskbarPresence.cs:1`, `UI/HelpOverlay.cs:1`, `UI/PadMenu.cs:1`, `UI/SearchOverlay.cs:1`, and `UI/SettingsParts.cs:1`. Missing summaries include the class at `Program.cs:190` and non-trivial methods at `UI/Kit.cs:32` and `UI/SearchOverlay.cs:111`. The coding standard explicitly applies to entire files on touch.

**Fix:** Add concise purpose headers and summaries throughout the touched files.

No additional findings on history comments or token naming.

### Checks

- Survival check: six flags, all explained above.
- Scanner self-check: `TOKENS SELFCHECK PASS`.
- Token ratchet: `TOKENS PASS`.
- Diff whitespace check: passed.
- Existing design dumps independently compared; no build or application launch performed.
- No files edited.

