# P1-project-reevaluation: plan

Planning only (2026-10-28, dev c1e5b6f). Nothing here is implemented. It replaces STATUS "Priority 1" steps 1.1-1.3 and
feeds Priority 2 row 2.7. Full literal dump (file:line, 15 categories): shared notes folder, `P1-token-inventory.txt`
(outside the repo; the A59 audit regenerates it into its report).

## 1. Revised Priority 1
| # | Step | Goal | How it is measured | Decision gate |
|---|---|---|---|---|
| 1.1 | Confirm goals | Original reorg goal (A38): changing one thing must not break another unseen; features modular. New goals: (a) one owner per responsibility, (b) less duplicated code, (c) a central design system, one source of truth per property. Look, feel and behavior identical unless the user approves a change in a lab; speed of show/hide must not regress. | User confirms the four goals and the success targets in section 3. | User: "goals OK" (or edits). Recorded in `docs/decisions.md` when the series starts. |
| 1.2 | Measure | Baseline numbers for every goal. | Section 2 below (manager survey) plus `A59-design-ownership-audit` (read-only Codex director audit) that confirms ownership and duplication with file:line, and `A60-design-token-scan` report mode for literal counts. | Baseline numbers written into STATUS. |
| 1.3 | Decide | Which items in section 4 run, in what order, and the design value choices (radius scale, type ramp, shadow tiers, motion durations). | User picks from the A59 report and the A64 lab. | Per item at loop step 4 (go / change / drop); value choices at the A64 lab. |

## 2. Findings (manager survey, 2026-10-28)
Product: 16.7k lines of C# in 57 files.

**God files** (gate `SizeWarn` is 600)
| File | Lines | Problem | Planned |
|---|---|---|---|
| `SelfTest.cs` | 2,637 | every selftest in one file; only `Contracts.cs` split out | A70 |
| `Calc/Calculator.cs` | 1,383 | one class, but pure and layered cleanly | leave (optional split) |
| `Model/Store.cs` | 1,006 | holds `Note`, `Settings` POCO, `StoreData`, 5 search types and `Store` | A67 |
| `UI/PadWindow*.cs` (5 partials) | 2,027 | still the hub; partials are about 300-530 each | leave; A57/A58 touch Layout |
| `Editor/PadView.cs` | 865 | checkbox/list commands still inside | A42c, A42d (already planned) |
| `UI/SettingsWindow.cs` + `SettingsParts.cs` | 823 + 530 | own private control kit (below) | A62, A66 |

**Ownership problems**
- `Model/Store.cs` owns settings and search types; `SettingsService` lives apart from the `Settings` class it serves.
- `UI/Kit.cs` mixes controls with `Material()` (window transparency), `InputStyles`, `Ago()` (time text) and the icon
  font families (`Glyph.Font`, `Glyph.Fluent`): design data in UI.
- Root namespace has `NotesSession.cs`, `RuntimeLog.cs`, `AppInfo.cs`, `Program.cs` with no folder or stated owner.
- Motion is in three places: `Motion/Anim.cs` (tweens), `Editor/Motion.cs` (scroll and caret physics), `UI/NoteSwipe.cs`
  (swipe physics); four hand-rolled `RequestAnimationFrame` loops sit beside `Anim`.
- Brush helpers: `RenderPaint.Brush` (Editor) plus two private `Brush`/`Pen` copies in `Editor/Renderers.cs:57,66,188`;
  UI makes 114 `new SolidColorBrush(...)` directly.
- No written ownership map; LAYERS covers direction (who may reference whom), not ownership (who owns colors, fonts, paths).

**Duplication hot spots**
| What | Where | Fix |
|---|---|---|
| Settings' private control kit (Heading, Group, Card, Seg, Toggle, Dropdown, TextButton, Tile) parallel to `Kit.Toggles/Section/Plain/PanelHeader` | `UI/SettingsParts.cs:24-301` vs `UI/Kit.cs:90-170` | A66 |
| Popup shadow `FromArgb(Dark ? 110 : 40, 0,0,0)` copied | `UI/PadMenu.cs:90`, `UI/SettingsParts.cs:241`; pill shadow tokens (90/28) separate | A61 |
| UI font stack `"Segoe UI Variable Text, Segoe UI"` | `UI/PadWindow.cs:81`, `UI/SettingsWindow.cs:71` | A61 |
| Mono stack `"Cascadia Mono, Consolas"` (4x) plus editor fallback chain (2x) | `HelpOverlay.cs:39`, `SearchOverlay.cs:182`, `SettingsParts.cs:376`, `PadView.cs:180`, `Renderers.cs:183` | A61 |
| Alpha-over-role color `Color.FromArgb(a, c.R, c.G, c.B)`, no helper; `Palette.Mix` is private | `Kit.cs:99`, `SettingsParts.cs:20,392,397`, `PadMenu.cs:90`, `PadWindow.Layout.cs:154` | A61 |
| Path rule `SameOrNested` twice | `NotesSession.cs:97`, `Platform/NotesLocation.cs:255` | A68 |
| `Brush`/`Pen` caches three times | `Editor/Renderers.cs:15,57,188` (+ `Colorizers.cs:17`) | A61 |
| `ApplyLook(Palette, Settings)` restyle pattern three times, no shared contract | `PadView.cs:175`, `MotionPreview.cs:201`, `SettingsWindow.cs:91` | note only (A66 if cheap) |

**Design-token inventory**
What exists: `Theme/Palette.cs` (20 semantic color roles, 8 palettes, accents, tones), `Theme/ChromeTokens.cs` (about 70
pad top-bar and pager measurements, including badge 6 and pill 12 radii and pill shadow), `Motion/Anim.cs` (one easing,
reduced-motion switch), `UI/Kit.cs` (icon button, toggles, section, panel header, glyph fonts).

Gaps: ChromeTokens covers the pad chrome only. Settings, Search, Menu, Help, toast and cards have no tokens. Counts outside
`Theme/` and `SelfTest`:
| Category | Literals | Values in use |
|---|---|---|
| Corner radius | 25 (+1 `CornerRadius = 3` in PadView) | 1, 1.5, 3, 3.5, 5, 7 (x6), 8 (x5), 9 (x2), 10 (x6), 12; pill 12 is duplicated as a literal on the card (`PadWindow.Layout.cs:439`) |
| Font size | 22 | 11, 11.5, 12 (x6), 12.5 (x3), 13 (x5), 14 (x4), 15, 26 |
| Font family | 5 string stacks + 2 glyph fonts in Kit | see duplication |
| Font weight | 31 | SemiBold/Bold/Medium spread across 9 files |
| Color construction | 22 | alpha mixes, black shadows, `#1A1A1A` on-accent text |
| Brush with literal alpha | 14 | .35, .45, ... |
| Shadow | 5 | three definitions |
| Tween durations (`Anim.Run`) | 13 | 120, 130, 140, 150 (x3), 160 (x2), 170, 180 (x2), 190, 200 ms |
| Timers (`FromMilliseconds/Seconds`) | 15 | behavior timings (save 400, debounce 125/140, toast 1300/2400/7000, idle 450/1600/2600); not design, but named nowhere |
| Thickness (margin/padding) | 80 | no spacing scale |
| Spacing / sizes | 31 / 41 | no scale |
| Opacity | 22 | |

## 3. Success targets (proposed, for 1.1)
- **Look and feel identical (first rule, user 2026-10-29).** Centralizing changes where a value is written, never the
  value: `CornerRadius = 10` becomes `CornerRadius = Radius.Card` with `Radius.Card = 10`. Proof per item: the
  `DESIGN tokens-match-legacy-values` selftest plus the user's look in the dev copy (light and dark). The only visible
  changes are ones the user approves in a lab (A64-design-unify-lab), and any of them can be declined.
- Design: hard-coded radius, font size/family/weight, color construction, shadow and tween-duration values outside
  `Theme/`/`Motion/` all move into named tokens (same values), and the gate then blocks new hard-coded ones.
- Spacing: the number of hard-coded margin/padding/size values in the code may not grow (watched closely); the spacing on
  screen does not change.
- Ownership: `docs/architecture.md` names one owner per responsibility; every rule it states that can be checked is a
  LAYERS or scan row.
- Duplication: every row in the table above is fixed or explicitly kept with a reason.
- God files: no file over 1,000 lines except `Calc/Calculator.cs` (by choice).
- No behavior change except in the lab-approved A65 item; show/hide timing logged by the dev copy does not regress.

## 4. Loop items
IDs A59-A73 are free (checked STATUS, all branches, tags and `git log --all`). Order is the run order; `||` = can run in
parallel with the item above (no shared files).

| Order | ID-slug | What | Tier | Depends on | Lab-first | Enforcement added |
|---|---|---|---|---|---|---|
| 1 | A59-design-ownership-audit | Read-only Codex director audit: confirm/correct section 2 with file:line, ownership map draft, full duplication sweep (beyond name matches), proposed token catalog (names, current values, users), risks per item. No code. | audit (Medium-style) | - | no | - |
| - | **Decision gate 1.3** | User picks items, order and success targets | | A59 | | |
| 2 | A60-design-token-scan | `tools/check-tokens.ps1`: masked source scan (reuse the string/comment masker from `tools/check-platform-seam.ps1`, extracted to one shared file), categories from section 2, scans `.cs` and `.axaml` (none today), per-file count baseline `tools/token-baseline.txt`; `gate.ps1` fails when a file's count rises or a new file has violations, passes with a note when counts drop. No product change. | Low | A59 | no | the scan itself (ratchet) |
| 3 | A61-theme-tokens-core | `Theme/` only: `Radius`, `Typography` (stacks, sizes, weights; glyph fonts move from Kit), `Elevation` (shadows), `Paint` (`WithAlpha`, `Mix`, one brush cache). Tokens take today's values (role names for now, e.g. `Radius.Card = 10`, `Radius.Menu = 8`). No call sites yet. | Low | A60 | no | `DESIGN tokens-match-legacy-values` selftest rows |
| 4 | A71-tokens-pad-chrome | Migrate `UI/PadWindow*.cs` (toast, card, pill, divider) to the tokens. Pixel-identical. | Medium | A61 | no | baseline shrinks |
| 5 | A72-tokens-overlays | Migrate `PadMenu`, `SearchOverlay`, `HelpOverlay`, `Kit`. Pixel-identical. | Medium | A61 | no | baseline shrinks |
| 6 | A73-tokens-editor | Migrate `Editor/` renderers, colorizers, completion, PadView; the three `Brush`/`Pen` copies use `Theme.Paint`. Pixel-identical. | Medium | A61 | no | LAYERS: `Color.FromArgb/FromRgb/Parse` and `new FontFamily(string)` outside Theme = 0 refs (after A62 too) |
| 7 | A62-settings-tokens | Migrate `SettingsWindow` + `SettingsParts` (most literals live here), plus Settings layout tokens (page margin, card padding, row height). | Medium | A61 | no (pixel-identical) | baseline shrinks; radius/font/color categories at 0 -> switch them from ratchet to ban |
| 8 | A63-motion-tokens | `Motion/MotionTokens`: one named duration per tween role (today's values), easing names; behavior timers get named constants beside their owners (not design tokens). Physics loops stay, documented as owned by `Editor/Motion`. | Medium | A60 | no (timing-identical) | tween-duration category ban; `DESIGN tokens` rows for durations |
| 9 | A64-design-unify-lab | One lab build showing each proposed unification (listed by A59) next to today's: radius scale (proposal: 4 control, 8 overlay/menu/card, 12 pill; Windows 11 uses 4/8), 2 shadow tiers, a type ramp (about 5 sizes), motion durations to the Windows 11 reference (folds in A46-motion-timings). User picks values. | Medium | A71-A73, A62, A63 | **yes** | - |
| 10 | A65-design-unify | Apply the values the user picked, one sub-item per scale (`A65a-radius-scale`, `A65b-type-ramp`, `A65c-shadow-tiers`, `A65d-motion-timings`), never folded into a migration. Visible change. | Medium | A64 approval | approved in A64 | `DESIGN tokens` rows updated to the new scale |
| 11 | A66-ui-kit-merge | One control kit: `UI/Kit/` (Controls, Settings controls) replaces SettingsParts' private kit and Kit's duplicates; `Material()` to Platform/Theme, `Ago()` to Model. | Medium | A62 (A65 if visible) | only if any pixel changes | LAYERS: Settings builds no `Border`/`ToggleSwitch` outside Kit (or scan row) |
| 12 \|\| | A67-model-store-split | Verbatim moves: `Note`, `Settings`, `StoreData` and search types out of `Model/Store.cs` into their own files. | Low (pure move) | A59 | no | LAYERS rows for the moved types |
| 13 \|\| | A70-selftest-split | Split `SelfTest.cs` into `SelfTest/*.cs` parts by area (LAYERS, CONTRACTS, Calc, Store, Editor). | Low (pure move) | A59 | no | gate size check passes for SelfTest |
| 14 | A68-notes-path-dedupe | One `SameOrNested` path rule, used by both NotesSession and NotesLocation. | Medium (notes-folder path) | A67 | no | CONTRACTS row: nested/same/sibling paths, case and trailing separator |
| 15 | A69-ownership-map | `docs/architecture.md` ownership table; root files into `App/` (NotesSession, RuntimeLog, AppInfo); ownership LAYERS rows from the map. | Low | A61-A68 | no | LAYERS ownership rows |

Notes:
- Theme-only definitions (A61) are Low; pixel-identical call-site migrations (A71-A73, A62, A63) are **Medium**, one area per item (dev-loop advice, section 6): they touch many UI/Editor
  files, so a fresh Codex review is worth it. The `DESIGN tokens` selftest proves values did not change; the user gets a
  short look in the dev copy (all surfaces, light and dark).
- Literal bans must be a source scan: `ChromeTokens` members are `const`, which the compiler inlines, so IL-based LAYERS
  cannot tell a token from a literal. LAYERS keeps the reference rules (color and font construction outside Theme).
- Thickness/spacing/size literals stay on the ratchet; a spacing scale is a later lab item only if the user wants it.

## 5. Fit with Priority 2
- Now: 1.1 (goals) and A59 audit (read-only, no conflicts) can run before or alongside the Bugs block.
- File overlaps (never in flight together): A57-split-view-clip and A58-split-divider-height touch `UI/PadWindow.Layout.cs`
  (A71); A51-recovered-folder-always-open and A48-answers-toggle-clarity touch `SettingsWindow`/`SettingsParts` (A62,
  A66); A52-move-old-folder-rename-warning touches `Platform/NotesLocation.cs` (A68).
- Proposed placement: Bugs block (2.1-2.4) -> release marker -> Reorg leftovers: A42c, A42d, then A60-A73 as row 2.7 ->
  Cleanup -> release marker -> Performance -> Features. A46-motion-timings is absorbed by A64/A65. A48 and A51 run after
  A66 so they are built with the merged kit.

## 6. Loop advice and disagreements
Asked the session "Dev loop and agent architecture" (`local_3235d0c4-1772-4769-9bc9-c113864fcddd`) on 2026-10-28.
| Topic | Its advice | This plan |
|---|---|---|
| Split | One read-only audit item, user decision, then one small item per piece | Adopted (A59, gate 1.3, A60-A70) |
| Tiers | Low: tokens defined inside `Theme/`, pure moves with LAYERS rows. Medium: call-site migrations across UI/Editor even if pixel-identical, and anything touching show/hide timings | Adopted: A61 split into Theme-only A61 (Low) plus one migration item per area, A71-A73 and A62 (Medium); A63 Medium. A60, A67, A70 stay Low |
| Who audits | Codex director does the whole sweep, token inventory included; Claude `design-reviewer` reads only the audit report to judge the ownership map and token names | Adopted: A59 = Codex audit, then a `design-reviewer` pass on its report before the decision gate |
| Enforcement | `tools/check-tokens.ps1` from the gate, committed baseline that only shrinks, fail when a file gains literals; LAYERS for reference direction | Adopted (`check-tokens.ps1`, `tools/token-baseline.txt`, covers `.axaml` for the future). Added: the const-inlining reason the ban cannot be IL-based, and ratchet -> hard ban once a category is 0 |
| Lab-first | Pixel-identical: no lab; proof = `tokens-match-legacy-values` row + short user hand test; screenshot diff only if the user allows Computer Use. Each visual unification lab-first as its own item, never in a migration | **Disagree in part:** one lab build (A64) shows all proposed unifications together, since the user judges the look as a whole; applying stays one item per scale (A65a-A65d) |
| Loop gaps | Gate scope check matches exact paths only, so migration briefs must list every file; one STATUS parent line for the series and one release marker after it | **Disagree in part:** keep exact paths (A59 produces the file lists; folder globs weaken the scope check that caught drift before). Propose two release markers: after A63 (all pixel-identical work) and after A69, so the visible A65 ships separately and is easy to revert |

Also adopted: no parallel Claude and Codex audits on the same question; one STATUS parent line for the series; merge each child separately with `/merge-task` and gate on dev each time.

Its open question for the user: should the gate's scope check accept whole folders in a brief's file list? (Decision 7 below.)

## 7. Decisions (user, 2026-10-28/29)
1 goals and reworded targets approved; 2A bugs first, A59-design-ownership-audit alongside; 3 visible changes in scope,
lab-first, with A75-design-system-reference (Claude Design System artifact and comparison canvas) before A64; 4 spacing
count may not grow; 5 no splits for now; 6 A46 folded into A64; 7 exact file paths for now, gate re-checked at releases
(C18-tag-scheme); 8 two release markers plus a tag scheme (C18-tag-scheme).

Original questions:
1. Goals and success targets in sections 1.1 and 3: OK as written?
2. Order: P1 items after the Bugs block (proposed) or before it?
3. Visual unification (A64/A65): in scope now, or keep the design system pixel-identical only (stop after A63)?
4. Spacing scale for margins/padding: ratchet only (proposed), or a lab item too?
5. Optional splits: `Calc/Calculator.cs` (proposed: leave), PadWindow partials (proposed: leave).
6. A46-motion-timings folded into A64 (proposed) or kept separate?
7. Gate scope check: keep exact file paths (proposed) or accept folders in a brief's Files list (dev-loop session's question)?
8. Release markers: one after A69 (dev-loop advice) or two, after A63 and after A69 (proposed)?

## 8. Corrections from A59-design-ownership-audit (user approved 2026-10-29)
These override section 4 where they differ; the STATUS plan rows carry the final scopes.
1. A61-theme-tokens-core defines the full token catalog up front (Settings tokens included); migrations only consume and
   add a DESIGN row for any token they must append. Separate `Paint.WithAlpha(byte)` and `Paint.Faded(opacity)`; no tokens
   that only rename Avalonia enums.
2. A73-tokens-editor shares brush/pen factories; each cache keeps its own limits and keys (no "one brush cache").
3. A68-notes-path-dedupe covers the third copy in `UI/SettingsWindow.cs`, is High tier, and puts the rule in
   `Platform/NotesPathRules` (no dependencies), so it no longer waits on A67-model-store-split.
4. A63-motion-tokens covers decorative tweens only.
5. New A76-behavior-timer-names (optional, High, Cleanup): behavior timers named beside their owners, values unchanged.
6. A62-settings-tokens adds `UI/MotionPreview.cs`.
7. A69-ownership-map keeps `AppInfo` a leaf and adds LAYERS rows for `App/` and `Motion/`.
