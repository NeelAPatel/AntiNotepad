# A35-ux-pass-s3b: Settings window

## Summary

**10 findings: 2 high, 7 medium, 1 low.** This is a code-only review; rendered focus, actual tab order, popup behavior and visual contrast need the item chat’s live check.

- **B-1:** Palette and accent choices have pointer handlers but no keyboard path.
- **B-2:** The shortcut recorder intercepts Tab, preventing keyboard access to its Clear button while recording.
- **B-3:** Search omits choices and can return an expander whose contents were filtered out.

## Controls checklist

Hit boxes are calculated from the clickable element where dimensions are explicit; “content-sized” means the final measurement depends on layout. Text buttons and segments use 14 px text with 4 px vertical margins; borders add to the stated minimum where present. “Name” refers to an explicit automation name; ordinary text buttons have visible text as content.

| Screen | Control | File:line | Hit box | Tooltip + key | Automation name | Keyboard path | Result |
|---|---|---|---|---|---|---|---|
| Window | Close | `UI/SettingsWindow.cs:187` | 32 × 32 | Close (Esc) | Close (Esc) | Tab, Enter/Space; Esc | No issue |
| Sidebar | Search | `UI/SettingsWindow.cs:196` | 32 high, rail width | Placeholder; no key shown | Find a setting | Tab, type | B-3; focus ring live check |
| Sidebar | Eight page buttons | `UI/SettingsWindow.cs:230` | 36 high, rail width | Visible names; no key | Page name + “settings” | Tab, Enter/Space | B-7; focus live check |
| Theme | Mode segments | `UI/SettingsWindow.cs:336` | Content-sized, approximately 22 high | Visible labels | Option label | Tab, Enter/Space | B-5 |
| Theme | Light and dark palette expanders | `UI/SettingsWindow.cs:338`; `UI/SettingsParts.cs:141` | Full row, min 64 high | Visible title | Title | Tab, Enter/Space | B-3; focus live check |
| Theme | Palette chips | `UI/SettingsParts.cs:459` | Content-sized; 8 px inner margin | Visible palette name | Palette name | Pointer only | B-1 |
| Theme | Accent swatches | `UI/SettingsParts.cs:490` | 20 × 20; 8 px gap | Color name; no key | Accent + color name | Pointer only | B-1, B-4 |
| Theme | Caret style tiles | `UI/SettingsWindow.cs:346`; `UI/SettingsParts.cs:434` | 64 px wide including padding; picture 32 high plus label | Visible labels | Bar / Block | Tab, Enter/Space | B-7; focus live check |
| Theme | Blink, glide toggles | `UI/SettingsWindow.cs:354` | 40 × 20 track plus word and margins; content-sized | Visible labels | Setting name | Tab, Enter/Space | Focus and state announcement live check |
| Theme | Side pane palette dropdown | `UI/SettingsWindow.cs:360`; `UI/SettingsParts.cs:352` | At least 160 × 48 | Visible current value | Initial value | Tab, Enter/Space to open; popup navigation unverified | B-6 |
| UI | Background tiles | `UI/SettingsWindow.cs:367` | 88 px wide including padding; 48 px picture plus label | Visible labels | Material name | Tab, Enter/Space | B-7; restart feedback present |
| UI | Tint and scale dropdowns | `UI/SettingsWindow.cs:375` | At least 160 × 48 | Visible value | Initial value | Tab, Enter/Space; popup navigation unverified | B-6 |
| UI | Toolbar and pager expander; Show and Fade after | `UI/SettingsWindow.cs:383` | Row min 64; segments ~22 high; dropdown min 160 × 48 | Visible labels | Title / option / initial value | Tab, Enter/Space; dropdown unverified | B-3, B-5, B-6 |
| UI | Bar style and slot dots | `UI/SettingsWindow.cs:387` | Segments content-sized, ~22 high | Visible labels | Option label | Tab, Enter/Space | B-5, B-7 |
| UI | Delete, New note, edge fade, menu row, colored icons, dim toggles | `UI/SettingsWindow.cs:394` | 40 × 20 track plus word and margins | Visible labels | Setting name | Tab, Enter/Space | Focus and state announcement live check |
| Editor | Preview editor | `UI/SettingsWindow.cs:414`; `UI/MotionPreview.cs:111` | Editor 220 high | Visible preview | Inherited editor semantics; verify | Editor keyboard input | Preview interaction and focus live check |
| Editor | Font, size, width, paper tone and answer-position dropdowns | `UI/SettingsWindow.cs:417`; `UI/SettingsWindow.cs:435`; `UI/SettingsWindow.cs:449` | At least 160 × 48 | Visible value | Initial value | Tab, Enter/Space; popup navigation unverified | B-6, B-9 |
| Editor | Line spacing, list type, pattern weight segments | `UI/SettingsWindow.cs:423`; `UI/SettingsWindow.cs:439`; `UI/SettingsWindow.cs:446` | Content-sized, ~22 high | Visible labels | Option label | Tab, Enter/Space | B-5 |
| Editor | Backing expander and pattern tiles | `UI/SettingsWindow.cs:437` | Row min 64; tiles 68 px wide including padding | Visible names | Title / pattern name | Tab, Enter/Space | B-3, B-7 |
| Editor | Answers, markdown, guides, folding, short links toggles | `UI/SettingsWindow.cs:448` | Track 40 × 20 plus word and margins | Visible labels | Setting name | Tab, Enter/Space | Focus and state announcement live check |
| Editor | Code languages expander and language pills | `UI/SettingsWindow.cs:455`; `UI/Kit.cs:201` | Row min 64; pills content-sized, ~22 high | Visible names | Language and on/off | Tab, Enter/Space | B-3, B-5 |
| Feel | Preview editor | `UI/SettingsWindow.cs:464`; `UI/MotionPreview.cs:111` | Editor 220 high | Visible preview | Inherited editor semantics; verify | Editor keyboard input | Preview interaction and focus live check |
| Feel | Vertical style expander and tuning segments | `UI/SettingsWindow.cs:467`; `UI/SettingsWindow.cs:487` | Row min 64; segments ~22 high | Visible labels | Title / option | Tab, Enter/Space | B-3, B-5 |
| Feel | Horizontal style expander and sole choice | `UI/SettingsWindow.cs:474` | Row min 64; segment ~22 high | Visible label | Title / Follow-through | Tab, Enter/Space | B-5, B-8, B-10 |
| Feel | Reduce motion toggle | `UI/SettingsWindow.cs:481` | Track 40 × 20 plus word and margins | Visible label | Reduce motion | Tab, Enter/Space | Focus and state announcement live check |
| Shortcuts | Reset all | `UI/SettingsWindow.cs:515` | Content-sized; approximately 24 high | Visible label | Button text | Tab, Enter/Space | B-5, B-9 |
| Shortcuts | Each Change, Save, Clear, Cancel and Reset | `UI/SettingsWindow.cs:530`; `UI/HotkeyRecorder.cs:104` | Content-sized; ~24 high, primary Save ~22 high | Visible labels | Button text | Tab to Change; recorder commands; see B-2 | B-2, B-5, B-9 |
| Shortcuts | Conflict dialog Cancel / Take shortcut | `UI/SettingsWindow.cs:598` | Content-sized; ~24 / 22 high | Visible labels | Button text | Tab, Enter/Space; Esc behavior unverified | B-5; dialog focus live check |
| System | Show/hide and new-note hotkey recorders | `UI/SettingsWindow.cs:632`; `UI/HotkeyRecorder.cs:124` | Content-sized action buttons | Visible labels | Button text | Tab to Change; recorder commands | B-2, B-5 |
| System | Open at expander and position tiles | `UI/SettingsWindow.cs:641` | Row min 64; tiles 112 px wide including padding | Visible labels | Title / position | Tab, Enter/Space | B-3, B-7 |
| System | Taskbar segments | `UI/SettingsWindow.cs:651` | Content-sized, ~22 high | Visible labels | Option label | Tab, Enter/Space | B-5 |
| System | Fresh note and save-delay dropdowns | `UI/SettingsWindow.cs:653`; `UI/SettingsWindow.cs:667` | At least 160 × 48 | Visible value | Initial value | Tab, Enter/Space; popup navigation unverified | B-6 |
| System | Keep on top, hide on blur, startup toggles | `UI/SettingsWindow.cs:657` | Track 40 × 20 plus word and margins | Visible labels | Setting name | Tab, Enter/Space | B-9; startup failure feedback needs live check |
| Storage | Current path | `UI/SettingsWindow.cs:701`; `UI/SettingsParts.cs:416` | Min 40 high; max 68 | Visible path | No explicit name | Tab, select/copy text | Esc exception: live check |
| Storage | Open, Change location / Choose folder | `UI/SettingsWindow.cs:725` | Content-sized; approximately 24 high | Visible labels | Button text | Tab, Enter/Space | B-5; picker live check |
| Storage | Cancel, Migrate notes / Use this folder | `UI/SettingsWindow.cs:715` | Content-sized; ~24 / 22 high | Visible labels | Button text | Tab, Enter/Space | B-5; migration recovery live check |
| Storage | Open recovered notes / app data | `UI/SettingsWindow.cs:757` | Content-sized; approximately 24 high | Visible labels | Button text | Tab, Enter/Space | B-5 |
| About | Show keyboard shortcuts | `UI/SettingsWindow.cs:691` | Content-sized; approximately 24 high | Visible label | Button text | Tab, Enter/Space | B-5 |
| About | Restore default settings | `UI/SettingsWindow.cs:692` | Content-sized; approximately 24 high | Visible label; says where to undo | Button text | Tab, Enter/Space | B-5; undo is implemented |

## Findings

| ID | Screen | Finding | Principles | Evidence | Sev | Fix idea |
|---|---|---|---|---|---|---|
| B-1 | Theme | Palette chips and accent swatches are `Border`s with pointer handlers and no focus or key handler. Keyboard users cannot choose them. | KB1 | `UI/SettingsParts.cs:471`, `:483`, `:496`, `:508` | High | Use focusable buttons or another keyboard-operable selection control with visible focus and selected state. |
| B-2 | Shortcuts, System | While recording, the tunnel handler marks every key handled. Tab therefore cannot move focus from the recorder to Clear; Enter saves a candidate and Esc cancels, but neither invokes Clear. | KB1, S1 | `UI/HotkeyRecorder.cs:124`, `:151`, `:180` | High | Provide a keyboard command for Clear or allow focus navigation to recorder actions while retaining a way to record Tab combinations. |
| B-3 | Search | Search indexes row titles/descriptions and subrow tags, not option labels. For example, searching “Cascadia” cannot find Font. Searching “Light mode palette” returns its header, but its unnamed subrow has been filtered out, so expanding the result reveals no palette choices. | S1, N1 | `UI/SettingsParts.cs:79`, `:117`, `:141`, `:175`; `UI/SettingsWindow.cs:308`, `:338`, `:417` | Medium | Index option names and include matching expanders’ relevant children, or navigate to the complete setting. |
| B-4 | Theme | Accent swatches are 20 × 20 clickable borders. Their 8 px gap prevents the specific overlapping-circle failure, but the targets remain below the house 24 px floor. | T1 | `UI/SettingsParts.cs:492`, `:498` | Medium | Put each swatch inside a larger clickable target while retaining the visible dot. |
| B-5 | Multiple pages | Segments and language pills have approximately 22 px content height; primary text buttons are also approximately 22 px. Adjacent segments have only 2 px margin, so their 24 px target circles can overlap. Other text buttons are approximately 24 px and below the 32 px comfort target. | T1, T2 | `UI/SettingsParts.cs:244`, `:394`; `UI/Kit.cs:201`; `Theme/Typography.cs:20` | High | Give interactive buttons at least 24 px height and spacing; use 32 px where these actions are frequent. |
| B-6 | Dropdowns | The dropdown’s automation name is set once to the initial value; choosing an item updates visible text but does not update that name. The name also omits the setting’s title. | N1, C3 | `UI/SettingsParts.cs:354`, `:385`, `:389` | Medium | Expose the setting name and current value through an appropriate selection control or update its accessible properties on change. |
| B-7 | Sidebar, tiles, row-changing controls | Page selection rebuilds the rail, and choices marked `rows: true` rebuild the page. The focused button is removed without an explicit focus restoration, so keyboard users may lose their place. | KB2, S1 | `UI/SettingsWindow.cs:158`, `:230`, `:262`; `:349`, `:371`, `:431`, `:646` | Medium | Restore focus to the selected control or a sensible successor after rebuilding; verify actual behavior live. |
| B-8 | Feel | “Horizontal Scroll Style” offers only “Follow-through”; the displayed current value is always `follow`, so choosing it offers no alternative. The description also uses implementation language. | N1, N2, N5 | `UI/SettingsWindow.cs:474`; `Model/Settings.cs:58` | Medium | Remove the one-choice selector or explain the behavior in user-facing terms without presenting it as a choice. |
| B-9 | Shortcuts, Editor, System | Reset and Reset all immediately discard overrides with no undo or confirmation. Reset remains available even when there is no override. Likewise, Answer position remains available when Answers is off, and Hide when I click away remains available while Keep on top prevents it taking effect. | U1, N3, N5 | `UI/SettingsWindow.cs:516`, `:540`, `:582`, `:590`, `:448`, `:659` | Medium | Offer undo for resets and indicate or disable controls whose current prerequisites prevent an effect. |
| B-10 | Feel | “Vertical Scroll Style” and “Horizontal Scroll Style” use title case rather than the requested sentence case. | W1 | `UI/SettingsWindow.cs:467`, `:474` | Low | Use “Vertical scroll style” and “Horizontal scroll style.” |

## No issue

- **N3, U1:** Restore default settings supplies an Undo action in the pad notice; the row tells users where to find it. `UI/SettingsWindow.cs:692`; `UI/PadWindow.Keys.cs:313`.
- **N5, S1:** A shortcut already assigned to another action gets a dialog naming both actions and the consequence before transfer. `UI/SettingsWindow.cs:598`.
- **N3, N9:** Folder selection has a pending state and Cancel; migration saves first and displays failures and success messages. `UI/SettingsWindow.cs:703`, `:715`, `:790`.
- **N1:** A background material change that requires a restart displays a restart message. `UI/SettingsWindow.cs:405`.
- **G1, H1, P1:** Pages use named groups and expanders for nested controls; code shows the intended grouping. Visual spacing and whether the grouping works in practice need a live check.
- **N4:** Settings close has the shared 32 px icon button, an automation name and an Esc tooltip. `UI/SettingsWindow.cs:187`; `UI/Kit.cs:109`.
- **Icons:** Existing Settings icon findings are covered by **I-2 through I-6** in stage 2 and are not repeated here.

## Not covered

- Live Tab and Shift+Tab order, visible focus rings, popup arrow-key navigation, screen-reader output, actual rendered hit boxes, and light/dark contrast were not verified.
- The native folder picker, migration and recovery outcomes, shortcut conflict dialog dismissal, restart behavior, and Restore Undo were traced in code only.
- Motion timing and easing are outside this pass. Stage 1 spacing and stage 2 icon findings were not re-audited.

## Checks run

Read the shared brief, principles, stage 2 icon report, Settings construction, shared controls, recorder, defaults, and the restore handler. No app was launched, built, or modified.

**READY: no — the code review is complete; the live checks listed above remain for the item chat.**