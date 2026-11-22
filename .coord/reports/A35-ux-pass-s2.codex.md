# A35-ux-pass-s2: icon inventory

## Summary

The source inventory identifies **77 glyph-use entries**, counting each sidebar entry and menu definition separately, conditional alternatives together, and repeated shortcut rows as one source use. It excludes selftest fixtures and helper definitions. Six additional Settings icons are hand-drawn.

- **Distinct codes used:** 19 Segoe; 35 bundled Fluent; one dev-badge PUA code in Cascadia Mono/Code, with a non-PUA fallback.
- **Raw-code uses:** 44, using the counting convention above.
- **Mismatches:** three clear same-meaning/different-glyph groups and three clear same-glyph/different-meaning groups, listed below.
- The requested Git search did **not** find `0xF6ED` in available product history. The A29 brief documents its former use, but its introduction and exact historical rendering path remain unproven.

**Table conventions:** paths are relative to `product/AntiNotepad`. `S` means Segoe Fluent Icons with Segoe MDL2 Assets fallback, selected by the default `GlyphIcon` font; `F` means bundled Fluent UI System Icons, explicitly passed as `Glyph.Fluent`. `I16` means `ChromeTokens.IconGlyphSize = 16`; `M16`, `L20`, `XL20`, `Input16`, and `Small14` are the corresponding `Typography` tokens. All glyph weights below are `0`, unless stated otherwise. A dash in runtime changes means no icon-specific change found; palette changes still affect colors when controls are rebuilt.

## Icons

| File:line | Screen | Code | Font | Name and intended meaning | Size | Weight | Runtime changes |
|---|---|---|---|---|---|---|---|
| `UI/PadBar.cs:127` | Pad bar | E74D | S | Delete; delete note | I16 | 0 | Error color on hover/focus; disabled alpha 0.5 |
| `UI/PadBar.cs:128` | Pad bar | E710 | S | Add; new note | I16 | 0 | Accent on hover/focus; disabled alpha 0.5 |
| `UI/PadBar.cs:210` | Pager | E76B | S | Back; previous note | I16 | 0 | Disabled alpha 0.5 |
| `UI/PadBar.cs:219` | Pager | E76C | S | Forward; next note/new note | I16 | 0 | Disabled alpha 0.5 |
| `UI/PadWindow.Layout.cs:292` | Pad bar | E721 | S | Search; search notes | I16 | 0 | Disabled alpha 0.5 |
| `UI/PadWindow.Layout.cs:293` | Pad bar | E89F | S | Split; split view | I16 | 0 | Disabled alpha 0.5 |
| `UI/PadWindow.Layout.cs:295`, `UI/Kit.cs:120` | Pad bar | E718 + E842 | S | Pin + PinFilled; keep on top | I16 | 0 | Pinned adds fill behind outline, alpha 110/255; tooltip changes; disabled alpha 0.5 |
| `UI/PadWindow.Layout.cs:297` | Pad bar | E713 | S | Settings | I16 | 0 | Disabled alpha 0.5 |
| `UI/PadWindow.Layout.cs:298` | Pad bar | E921 | S | Hide | I16 | 0 | Disabled alpha 0.5 |
| `UI/PadWindow.Bar.cs:56,65` | Pad bar | EA37 / F0E2 | S | List / Grid; switch between notes and slots | I16 | `PlainIconWeight = 0` | Changes with `Data.ViewSlot`; button disabled alpha 0.5 |
| `UI/SearchOverlay.cs:79` | Search | EA37 / F0E2 | S | List / Grid; switch result view | I16 | 0 | Changes with `GridView`; disabled alpha 0.5 |
| `UI/SearchOverlay.cs:81` | Search | E76B / E74D | S | Back / Delete; leave trash / open trash | I16 | 0 | Changes with trash view; disabled alpha 0.5 |
| `UI/SearchOverlay.cs:83` | Search | E711 | S | Close overlay | I16 | 0 | Disabled alpha 0.5 |
| `UI/SettingsWindow.cs:187` | Settings | E711 | S | Close Settings | I16 | 0 | Disabled alpha 0.5 |
| `UI/Kit.cs:230` | Shared close control | E711 | S | Close | I16 | 0 | Disabled alpha 0.5 |
| `UI/PadWindow.Layout.cs:237` | Pad menu | E710 | S | `MenuGlyphs.NewNote`; new note | M16 | 0 | Optional colored menu icon |
| `UI/PadWindow.Layout.cs:238` | Pad menu | E721 | S | Search | M16 | 0 | Optional colored menu icon |
| `UI/PadWindow.Layout.cs:239` | Pad menu | E89F | S | Split; open/close split view | M16 | 0 | Label changes with split state |
| `UI/PadWindow.Layout.cs:240` | Pad menu | E718 | S | Pin; keep on top/unpin | M16 | 0 | Label changes with pin state; optional accent |
| `UI/PadWindow.Layout.cs:242` | Pad menu | E713 | S | Settings | M16 | 0 | Optional colored menu icon |
| `Editor/PadView.cs:915` | Editor menu | E7A7 | S | Undo | M16 | 0 | Disabled when undo unavailable |
| `Editor/PadView.cs:916` | Editor menu | E7A6 | S | Redo | M16 | 0 | Disabled when redo unavailable |
| `Editor/PadView.cs:917` | Editor menu | E8B3 | S | SelectAll | M16 | 0 | — |
| `Editor/PadView.cs:919` | Editor menu/action row | E8C6 | S | Cut | M16 | 0 | Disabled without selection; action row is optional |
| `Editor/PadView.cs:920` | Editor menu/action row | E8C8 | S | Copy | M16 | 0 | Disabled without selection; action row is optional |
| `Editor/PadView.cs:921` | Editor menu/action row | E77F | S | Paste | M16 | 0 | Action row is optional |
| `Editor/PadView.cs:922` | Editor menu/action row | E74D | S | Delete selection | M16 | 0 | Disabled without selection; optional error color/action row |
| `UI/SettingsWindow.cs:33` | Settings sidebar | F2F5 | F | Theme | L20 | 0 | Selected `IconInk`, otherwise muted |
| `UI/SettingsWindow.cs:33` | Settings sidebar | F592 | F | UI | L20 | 0 | Selected color |
| `UI/SettingsWindow.cs:33` | Settings sidebar | F386 | F | Editor | L20 | 0 | Selected color |
| `UI/SettingsWindow.cs:34` | Settings sidebar | EC8D | F | Feel | L20 | 0 | Selected color |
| `UI/SettingsWindow.cs:34` | Settings sidebar | F359 | F | System | L20 | 0 | Selected color |
| `UI/SettingsWindow.cs:34` | Settings sidebar | F4B8 | F | `KeyboardGlyph`; Shortcuts | L20 | 0 | Selected color |
| `UI/SettingsWindow.cs:35` | Settings sidebar | F418 | F | Storage | L20 | 0 | Selected color |
| `UI/SettingsWindow.cs:35` | Settings sidebar | F4A3 | F | About | L20 | 0 | Selected color |
| `UI/SettingsWindow.cs:205` | Settings search | F68F | F | Search input | Input16 | 0 | — |
| `UI/SettingsWindow.cs:338` | Settings row | F8A1 | F | Light mode palette | XL20 | 0 | Expander |
| `UI/SettingsWindow.cs:340` | Settings row | F87D | F | Dark mode palette | XL20 | 0 | Expander |
| `UI/SettingsWindow.cs:342` | Settings row | F2F5 | F | Accent | XL20 | 0 | — |
| `UI/SettingsWindow.cs:356` | Settings row | E0CD | F | Glide between positions | XL20 | 0 | — |
| `UI/SettingsWindow.cs:360` | Settings row | E8B0 | F | Side pane palette | XL20 | 0 | — |
| `UI/SettingsWindow.cs:375` | Settings row | F592 | F | Background | XL20 | 0 | — |
| `UI/SettingsWindow.cs:376` | Settings row | F595 | F | Tint | XL20 | 0 | Hidden for solid material |
| `UI/SettingsWindow.cs:417` | Settings row | F7E4 | F | Font | XL20 | 0 | — |
| `UI/SettingsWindow.cs:419` | Settings row | F7E6 | F | Font size | XL20 | 0 | — |
| `UI/SettingsWindow.cs:423` | Settings row | F7F6 | F | Line spacing | XL20 | 0 | — |
| `UI/SettingsWindow.cs:435` | Settings row | F2F9 | F | Paper tone | XL20 | 0 | — |
| `UI/SettingsWindow.cs:437` | Settings row | F462 | F | Backing | XL20 | 0 | Expander |
| `UI/SettingsWindow.cs:448` | Settings row | F20A | F | Answers | XL20 | 0 | — |
| `UI/SettingsWindow.cs:451` | Settings row | E7DB | F | Clean markdown | XL20 | 0 | — |
| `UI/SettingsWindow.cs:453` | Settings row | ECF4 | F | Folding | XL20 | 0 | — |
| `UI/SettingsWindow.cs:454` | Settings row | F4E4 | F | Short links | XL20 | 0 | — |
| `UI/SettingsWindow.cs:455` | Settings row | F2EF | F | Code languages | XL20 | 0 | Expander |
| `UI/SettingsWindow.cs:467` | Settings row | E077 | F | Vertical Scroll Style | XL20 | 0 | Expander |
| `UI/SettingsWindow.cs:474` | Settings row | F18D | F | Horizontal Scroll Style | XL20 | 0 | Expander |
| `UI/SettingsWindow.cs:483` | Settings row | F103 | F | Reduce motion | XL20 | 0 | — |
| `UI/SettingsWindow.cs:516` | Settings row | F4B8 | F | `KeyboardGlyph`; reset shortcuts | XL20 | 0 | — |
| `UI/SettingsWindow.cs:522` | Settings rows | F4B8 | F | `KeyboardGlyph`; each shortcut action | XL20 | 0 | Generated once per shortcut definition |
| `UI/SettingsWindow.cs:634` | Settings row | F4B8 | F | Show or hide app hotkey | XL20 | 0 | — |
| `UI/SettingsWindow.cs:635` | Settings row | F8BD | F | Open on a new note hotkey | XL20 | 0 | — |
| `UI/SettingsWindow.cs:650` | Settings row | EE77 | F | Open at | XL20 | 0 | Expander |
| `UI/SettingsWindow.cs:651` | Settings row | F359 | F | Taskbar icon | XL20 | 0 | — |
| `UI/SettingsWindow.cs:653` | Settings row | F2DD | F | Fresh note after | XL20 | 0 | — |
| `UI/SettingsWindow.cs:659` | Settings row | F601 | F | Keep on top | XL20 | 0 | — |
| `UI/SettingsWindow.cs:660` | Settings row | F165 | F | Hide when I click away | XL20 | 0 | — |
| `UI/SettingsWindow.cs:661` | Settings row | F60E | F | Start with Windows | XL20 | 0 | — |
| `UI/SettingsWindow.cs:690` | Settings row | F4A3 | F | App/version information | XL20 | 0 | — |
| `UI/SettingsWindow.cs:691` | Settings row | F4B8 | F | `KeyboardGlyph`; keyboard shortcuts | XL20 | 0 | — |
| `UI/SettingsWindow.cs:692` | Settings row | F19F | F | Restore default settings | XL20 | 0 | — |
| `UI/SettingsWindow.cs:734` | Settings card | F418 | F | Notes folder | XL20 | 0 | — |
| `UI/SettingsWindow.cs:758` | Settings card | F418 | F | Recovered notes | XL20 | 0 | — |
| `UI/SettingsWindow.cs:761` | Settings card | F359 | F | App data | XL20 | 0 | — |
| `UI/SettingsParts.cs:148` | Settings expander | F2A3 | F | Chevron; expand/collapse | M16 | 0 | Rotates 180° when open |
| `UI/SettingsParts.cs:357` | Settings control | F2A3 | F | Chevron | Small14 | 0 | — |
| `UI/PadWindow.Layout.cs:489` | Dev badge | E0A0; fallback U+2387 | Cascadia Mono/Code if glyph exists; otherwise text font | Branch badge | `BadgeFontSize = 12` | `Typography.Regular` | Hover expands and crossfades labels; slot can change color |

`Glyph` constants are defined at `UI/Kit.cs:18`; `MenuGlyphs` at `Editor/MenuEntry.cs:12`. `Glyph.Restore` E777 is defined but has no current use. The menu renderer is `UI/PadMenu.cs:280`; Settings row glyphs use `UI/SettingsParts.cs:558`.

The Settings rows also use six **drawn**, non-font icons: Mode (`SettingsWindow.cs:336`), Caret style (`:354`), Blink (`:355`), Scale (`:378`), Toolbar and pager (`:383`), and Indent guides (`:452`). Their implementations start at `SettingsParts.cs:566`; `DrawnIcon` measures 20×20 and uses a 1.4-wide pen. They have no code point or font weight.

The Help/shortcut map uses text and keycaps, with no icon-font glyph found. Selftest fixtures also instantiate the Delete button (`SelfTest/Bar.cs:28`) and five pad-menu glyphs (`SelfTest/Design.cs:168`); these are checks, not additional product screens.

## Raw codes

| Uses | Raw values |
|---|---|
| Settings sidebar `SettingsWindow.cs:33–35` | F2F5, F592, F386, EC8D, F359, F418, F4A3 |
| Settings search `SettingsWindow.cs:205` | F68F |
| Settings rows/cards | The 33 `Fl(0x…)` entries listed individually in **Icons** |
| Settings chevrons `SettingsParts.cs:148,357` | F2A3 |
| Dev badge `PadWindow.Layout.cs:489` | E0A0, with U+2387 fallback |

The literal PUA characters in `Glyph` and `MenuGlyphs` are named constants, so their **call sites** are not counted as raw uses. `SelectAll` is likewise a named constant despite its `\uE8B3` definition. `AppInfo.cs:61` supplies the raw E0A0 badge marker; the badge’s formatting selftests at `SelfTest/App.cs:206–223` repeat that marker.

## Mismatches

| Kind | Meaning or code | Uses |
|---|---|---|
| Same meaning, different glyph | Search | Segoe E721 in pad/menu; bundled F68F in Settings search |
| Same meaning, different glyph | Keep on top | Segoe E718/E842 in pad, E718 in menu; bundled F601 in Settings |
| Same meaning, different glyph | Hide | Segoe E921 for hide app; bundled F165 for hide on click away |
| Same glyph, different meaning | Segoe E74D | Delete note, delete selection, and open trash |
| Same glyph, different meaning | Segoe E76B | Previous note and leave trash |
| Same glyph, different meaning | Bundled F359 / F418 / F4B8 | System/taskbar/app data; Storage/notes folder/recovered notes; Shortcuts/reset/action/hotkey/help respectively |

The same numeric code in different fonts would not establish the same image; comparisons above identify the font.

## A29 cause

The [A29 rework brief](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/.coord/tasks/A29-shortcut-map-r4.md>) says the Shortcuts sidebar and page rows used guessed `0xF6ED`. Its [report](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/.coord/reports/A29-shortcut-map-r4.md>) says they were replaced with bundled Fluent `0xF4B8`.

The available commit `fa982a5` introduces `KeyboardGlyph = 0xF4B8` and routes those uses through it; the existing keyboard-shortcuts row already used `0xF4B8` in its parent. The current bundled-font path is `SettingsWindow.cs:237` for the sidebar and `SettingsParts.cs:558` for rows.

However, `git log --all -S '0xf6ed'` and the uppercase variant returned no product-history matches. Thus the brief supports that the old value was used, but **Git did not establish its introducing commit or independently prove its historical font**. I did not infer its appearance.

## Glyph checks

| File:line | Check |
|---|---|
| `UI/PadWindow.Layout.cs:501–507` | Runtime check that Cascadia Mono or Cascadia Code contains E0A0 before using it for the dev badge; otherwise uses U+2387. This is not a selftest. |
| `SelfTest/App.cs:127–135` | Checks that bundled icon and font resources open and are nonempty; does not check individual glyphs. |
| `SelfTest/Bar.cs:28–34` | Checks icon-button disabled opacity and restoration; does not check glyph existence. |
| `SelfTest/Design.cs:168` | Constructs menu entries for a design check; does not check glyph existence. |

No selftest using `ContainsGlyph` or `CharacterToGlyphMap` was found.

## Checks run

Read-only source searches across `product/AntiNotepad`, including PUA literals, escapes, font helpers and selftests; inspected shared renderers and token values; searched available Git history for `f6ed` and `f4b8`. No app launch, build, file write, or network access.

READY: no - the available Git history did not establish where `0xF6ED` originated or independently prove the font used for that historical draw.