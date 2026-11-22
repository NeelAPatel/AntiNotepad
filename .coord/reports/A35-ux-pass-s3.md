# A35-ux-pass-s3: per-screen findings (stage 3) and stage 4 proposal

Item chat, 2026-11-20. Sources: four parallel Codex terra code reads (read-only; raw reports
[pad](A35-ux-pass-s3a.codex.md), [Settings](A35-ux-pass-s3b.codex.md), [overlays and menus](A35-ux-pass-s3c.codex.md),
[contrast](A35-ux-pass-s3d.codex.md)), the rendered-tree dump ([dump checks](A35-ux-pass-s3-dump.md)), stages 1 and 2.
The item chat re-read the code for every 🔴 behavior finding (A-1, C-1) and re-graded severities to the house rules in
`.coord/research/ux-principles.md`. **No live screenshots were taken**: desktop control needs the user's go-ahead, so the
live checks are listed at the end. Principle IDs refer to that file.

## 🔴 High (13)
| ID | Screen | Finding | Principles | Evidence |
|---|---|---|---|---|
| C-1 | Search, trash view | **Plain Delete permanently deletes the selected note, even with the cursor in the search box**, so pressing Delete to edit the query destroys a note. No Undo, no confirmation. (Item chat confirmed in code; worse than the Codex report.) | U1, N5, N3 | `UI/SearchOverlay.cs:337, 376-379` |
| A-1 | Permanent slots | Clear a slot, type new text, then click the toast's Undo button within its 7 s: the old text replaces the new text. (Item chat checked: typing disarms Ctrl+Z, so only the button overwrites.) | U1, N3 | `UI/PadWindow.Navigation.cs:251`; `UI/PadWindow.Layout.cs:267, 704, 718` |
| B-1 | Settings, Theme | Palette chips and accent swatches have no keyboard path (pointer-only `Border`s). | KB1 | `UI/SettingsParts.cs:471, 483, 496, 508` |
| B-5 | Settings, all pages | Segmented choices, language pills and primary text buttons are about 22 px tall with 2 px gaps. That is under the 24 px floor, and neighbors' 24 px circles overlap. | T1, T2 | `UI/SettingsParts.cs:244, 394`; `UI/Kit.cs:201` |
| A-2 | Pad, Permanent | Unselected slot dots are 16 px wide, 20 px apart center to center, so their 24 px circles overlap. | T1, F1 | `Theme/ChromeTokens.cs:48, 60`; `UI/PadBar.cs:230` |
| D-3 | Settings | Text on accent-filled buttons picks dark or white by a non-WCAG brightness formula. White on Amber (2.92:1), Blue (3.26:1), Red, Pink, Teal and Green fails 4.5:1. | C1 | `UI/SettingsParts.cs:398` |
| D-5 | Settings | Resting field borders are 1.40:1 (Paper) and 1.47:1 (Ink) against the field. The Paper focus ring is 2.57:1. Both need 3:1. | C2, C3 | `UI/Kit.cs:249` |
| D-7 | Toasts | Dark Ink toast: the amber Undo action on the light toast is 1.28:1. The action is the one thing on the toast you must read. | C1 | `UI/PadWindow.Layout.cs:692` |
| I-1 | All icons | Two icon fonts; a code passed with the wrong font draws a wrong icon silently (the A29 incident; 32 of 55 codes collide). | I1 | stage 2 |
| I-8 | Gate | No selftest or gate step checks icon codes. | I1 | stage 2 |
| B-2 | Settings, Shortcuts | While recording, the recorder swallows Tab, so the keyboard cannot reach Clear. Enter saves and Esc cancels. | KB1 | `UI/HotkeyRecorder.cs:124, 151, 180` |
| D-2 | Save-failure card | Detail text on the solid card is 3.61:1 (Paper) and 4.41:1 (Ink). | C1 | `UI/PadWindow.Layout.cs:741` |
| C-3 | Save-failure card | Enter always runs Retry, even when Quit anyway has focus. | KB1, DN2 | `UI/PadWindow.Layout.cs:794` |

## 🟡 Medium (23)
| ID | Screen | Finding | Principles |
|---|---|---|---|
| D-1, D-4 | Pad, translucent | At the Clear tint over an opposite-color desktop, body, muted, hint, link, syntax, error and dev-badge colors fall far below 4.5:1 by a plain alpha model (for example, Graphite body on white 2.92:1). Real Acrylic adds blur and a luminosity layer, so this is a **worst-case model, not a measurement**. | C1, C6 |
| D-6 | Pager, toggles | Selected and unselected dot colors are 1.25:1 (Paper). Width also differs, so C4 passes; C2 does not. Paper toggle on/off is 1.38:1. | C2 |
| A-3 | Split | No key moves focus between panes (F6). | KB4 |
| A-4 | Pager | The count opens search but has no tooltip, and its name does not say so. | N6, I5 |
| A-5 | Pager | The selected dot's target widens 16 to 24, so the other dots shift. | L1 |
| A-6 | Split | Swap always acts on the left pane; the tooltip doesn't say so. | DN2 |
| A-7 | Bars | Swap, Settings, Delete/Clear and direct slot picks have no default key, and their tooltips then show none. Hidden overlay/float bars give no cue that they exist. | DN1, N6 |
| A-8 | Dev badge | Full branch text shows on hover only; no keyboard or accessible name. | DN1 |
| A-9 | Float bar | Delete/Clear sits about 13 px from the pager arrows. | U2 |
| B-3 | Settings search | Option labels are not indexed ("Cascadia" finds nothing). A matched expander can open empty. | S1 |
| B-4 | Settings | Accent swatches are 20 x 20 (8 px gaps, so they pass the spacing exception but are under the house floor). | T1 |
| B-6 | Dropdowns | The accessible name is fixed to the first value and omits the setting name. | N1 |
| B-7 | Settings | Page change and row rebuild drop keyboard focus. | KB2 |
| B-8 | Feel | Horizontal scroll style offers one choice. | N1, N5 |
| B-9 | Shortcuts, Editor, System | Reset and Reset all have no undo. Controls whose prerequisite is off stay active (Answer position with Answers off; Hide when I click away with Keep on top on). | U1, N5 |
| C-2 | Search | Deleting a slot result reports "0 notes moved to trash"; mixed selections skip slots silently. | N1, N2 |
| C-4 | Save-failure card | Shows the raw reason and doesn't say what Quit anyway loses. | N9 |
| C-5 | Editor menu | The action row hides accelerator keys and puts Delete next to Paste. | KB3, U2 |
| C-7 | Help | Says `list` always makes checkboxes; the default list type is a setting. | N2, N10 |
| I-2 | Settings | 44 raw icon codes. | I1 |
| I-3 | Icons | One meaning, two glyphs: Search, Keep on top and Hide each look different in the pad and in Settings. | N4 |
| Tokens | All | No spacing tokens exist: 97 on-scale and 39 off-scale spacing literals. | V1 |
| R-1 | Settings | About 60 buttons get the Fluent theme's default radius 3, not our 4. | V3 |

## 🟢 Low (9)
B-10 title case on two Settings rows (W1). C-6 tray menu shows no keys (KB3). I-4 one glyph reused for several
meanings (folder, desktop, keyboard). I-5 20 px icon designs drawn at 14 and 16. I-6 two icon styles (they never sit side by
side). I-7 unused `Glyph.Restore`. R-2 Avalonia caption buttons at 10 px text sit in the Settings tree (live check if ever
visible). Stage 1 strokes 1.1 / 1.4 / 1.8 and radius 9. Stage 1 token doc drift (10 tokens missing from `design-scale.md`).

## No issue (checked and passed)
- Stack Delete goes to trash with a named Undo toast; slot Clear makes a trash copy. The bar guards double clicks. Delete,
  Clear and New dim when they would do nothing (N5, U1, A34 decisions 6-11 hold).
- Restore default settings has Undo. Shortcut conflicts get a dialog naming both actions. Folder migration has Cancel and
  reports failures. Restart-needed changes say so (U1, N5, N9, N1).
- Esc closes Search, Help and Settings and returns focus to the editor (N3, S1).
- Top icons, arrows, count, corner buttons and Settings close are 32 px; Settings close names its key (T2, N4).
- Layout keeps New note's corner when it is hidden (L1); the whole bar fades and reveals together (G2).
- Rendered text sizes and strokes are all on the scale; radii other than the theme-default 3 are explained (V2, V3, V4).
- Every icon code exists in its own font and its glyph name matches its meaning (I1).
- The selected pager dot also differs by width, so state is not color-only (C4).

## Live checks not done (need desktop control or the user)
Tab order and focus rings on every screen; real contrast over a white and a black wallpaper at Clear tint (to settle
D-1/D-4); 320 px clipping; whether R-2's caption buttons ever show; first-use clarity of Standard vs Permanent (DN4).

## Stage 4 proposal: six batches (each its own item and Approve; lab-first where visible)
| # | Batch | Findings | Visible? | Size |
|---|---|---|---|---|
| 1 | **Data safety** | C-1, A-1, C-3, C-2 | small (Delete key, toast text) | small; bug items with repro selftests |
| 2 | **Icon registry and checks** | I-1, I-8, I-2, I-3, I-5, I-7, I-4 | I-3 changes some Settings icons: sheet first | medium |
| 3 | **Targets and keyboard** | B-5, A-2, A-5, B-4, B-1, B-2, A-3, B-7 | yes: lab | medium |
| 4 | **Contrast** | D-3, D-5, D-7, D-2, D-6; live measure for D-1/D-4 | yes: lab, both themes | medium |
| 5 | **Spacing tokens and snaps** | Tokens, R-1, stage 1 strokes, scanner pens, doc drift | no (tokens), yes (snaps): lab | medium |
| 6 | **Labels and discoverability** | A-4, A-6, A-7, A-8, B-3, B-6, B-8, B-9, B-10, C-4, C-5, C-6, C-7, A-9 | text only, mostly | small to medium |
