# A35d-spacing-tokens: snap list (the lab)

Written by the item chat, 2026-11-20, from `A35-ux-pass-s1-classified.tsv` (off-scale and off-grid rows), the stage 1
stroke table and stage 3 R-1. Scale: `docs/design-scale.md` R column (XS 4, S 8, M 12, L 16, XL 24, 2XL 32, Nudge 2).
Each row is **snap** (pixels move), **derive** (written from existing tokens, pixels stay), or **exception** (named token,
pixels stay, listed in `design-scale.md` section 6). Every row the user approves becomes a Codex checklist line.

The design dump records Margin, Padding, CornerRadius and BorderThickness only. Rows marked (no dump) move pixels the
dump cannot show (StackPanel spacing, sizes, pens, drawn radii); they are checked by reading the diff and by hand.

## Snaps (pixels move)
| # | Surface | Site | Before | After | Move |
|---|---|---|---|---|---|
| 1 | Settings | nav column left margin (`SettingsWindow.cs:184`) | 18 | L 16 | -2 |
| 2 | Settings | nav list margin (`:195`) | 10, 4, 10, 10 | 12, 4, 12, 12 | +2 sides and bottom |
| 3 | Settings | nav item margin (`:205`) | 10 left | M 12 | +2 |
| 4 | Settings | page right margin (`:260`) | 28 | 2XL 32 | +4 (page 4 px narrower) |
| 5 | Settings | row stacks (`:348, :532, :715`) | Spacing 6 | S 8 | +2 per gap (no dump) |
| 6 | Settings | dropdown list items (`SettingsParts.cs:361`) | Spacing 1 | Nudge 2 | +1 per gap (no dump) |
| 7 | Settings | swatch row (`SettingsParts.cs:446`) | Spacing 5 | XS 4 | -1 per gap (no dump) |
| 8 | Settings | picture art pens (`SettingsParts.cs:609, 678`) | 1.1, 1.4 | Thin 1 | -0.1, -0.4 (no dump) |
| 9 | Search | overlay margin (`SearchOverlay.cs:47`) | 14, 14, 14 | L 16 | +2 top and sides |
| 10 | Search | field and row margins (`:78, :172`) | 6 | S 8 | +2 |
| 11 | Search | result line stack (`:201`) | Spacing 3 | XS 4 | +1 (no dump) |
| 12 | Help | overlay margin (`HelpOverlay.cs:20`) | 10, 14, 10 | 12, 16, 12 | +2 each |
| 13 | Help | panel right margin (`:22`) | 14 | L 16 | +2 |
| 14 | Completion | row stacks (`Completion.cs:116, 143`) | Spacing 10 | M 12 | +2 (no dump) |
| 15 | Save-failure card | stacks and margin (`PadWindow.Layout.cs:763-766`) | 6, 6, 9 | S 8, 8, 8 | +2, +2, -1 |
| 16 | Motion preview | stacks (`MotionPreview.cs:85, 107, 223`) | 14, 6, 5 | 16, 8, 4 | +2, +2, -1 (no dump) |
| 17 | Motion preview | sample pen (`:271-272`) | 1.8 | Thick 2 | +0.2 (no dump) |
| 18 | Editor | folded block highlight radius (`Renderers.cs:101`) | 9 | Md 8 | -1 (no dump) |
| 19 | Whole app | Fluent `ControlCornerRadius` (about 60 Settings buttons, plus text boxes, combo boxes, toggle and repeat buttons that read it) | 3 | `Shape.Control` 4 | +1 corner |

## Derive (no pixel moves)
| Site | Today | Written as |
|---|---|---|
| Search bottom margin 46 (`SearchOverlay.cs:47`) | 46 | from `ChromeTokens` if it tracks the pager strip; else exception with its reason |
| Help bottom margin 40 (`HelpOverlay.cs:20`) | 40 | same rule |
| Editor hint (`PadView.cs:130`, `:1039`) | 37, 7 | editor text inset + 1, text top + 3 (keeps the hint on the first text line) |
| Menu icon box 18 x 18 (`PadMenu.cs:273`) | 18 | `IconGlyphSize + GlyphIconBoxExtra` |
| Caps letter spacing 0.8 (`Kit.cs:238`) | 0.8 | new `Typography` token |

## Exceptions (named token, no pixel moves)
| Site | Value | Reason |
|---|---|---|
| Editor text inset (`PadView.cs:99`) | 36 left, 30 right | Moves the user's own text; 30 to 32 would rewrap lines. Kept, named. |
| Toggle knob inset (`SettingsParts.cs:298`) | 3 in a 40 x 20 track | Centers the knob; 4 would push it off center. |
| Caret sample art (`SettingsParts.cs:524-526`) | 2 wide, 1 margin | Draws a real caret; art, design-scale exception 11. |
| Completion column widths (`Completion.cs:119, 146`) | 70, 86 | Text-fit columns, like exception 2. |
| Help key column (`HelpOverlay.cs:35, 61`) | 150 | Text-fit column. |
| Motion sample bars (`MotionPreview.cs:135, 226`) | 46; 14 x 3 | Sample art. |
| Hidden taskbar helper (`TaskbarPresence.cs:143-146`) | 1 x 1 | Not drawn. |
| Checkbox pens, chip font (`Renderers.cs:254, 399-424`) | as today | Already exceptions 4 and 5. |

Window and panel sizes already on the 4 grid (Settings 900 x 720, pad 320 x 260, popup widths 224 to 420, rows 40 to 68)
become named size tokens with no pixel move. Out of scope here: the accent text color at `SettingsParts.cs:398` (color)
and the fade ramp at `PadWindow.cs:344` (motion).
