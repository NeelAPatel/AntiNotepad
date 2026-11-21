# A35-ux-pass: principles, sources and tests for the whole-app audit

Stage 0 of `A35-ux-pass` (whole-app UX audit). Researched 2026-11-20 by the item chat. Each row is one principle, its
source, and a test we can run against an AntiNotepad screen. Stages 1 to 3 cite rows by their ID (`N3`, `F1`, `C2`).
Motion is out of this pass (user, 2026-11-20), so timing and easing rules are listed only to say they are skipped.

**How the sources were read.** "Read" means the page was fetched and read this session. "Search" means only the search
summary of that page was seen, so the wording is a paraphrase. "Book" means no online text was checked; the row states
the idea and names the book. Nothing here is filled in from memory as if it were a quote.

**Screens the tests run on** (from the card): pad Standard, pad Permanent, split, Settings, search overlay, menus
(context and tray), toasts, save-failure card, shortcut map, dev badge. Every test says which screens it touches; "all"
means every one.

**Severity** (ours, used in stage 3): 🔴 High = data loss, a wrong action, or a WCAG AA fail; 🟡 Medium = slows or
confuses a regular user; 🟢 Low = polish. "No issue" is recorded too.

## 1. Behavior: heuristics

| ID | Principle | Source | How to test it here | Screens |
|---|---|---|---|---|
| N1 | Visibility of system status: timely feedback | Nielsen ([NN/g, search](https://www.nngroup.com/articles/ten-usability-heuristics/)) | After each action (save, delete, slot clear, copy, settings change) something visible confirms it within 400 ms (see D1). List actions that change state silently. Saved label and dev badge must say what they claim | all |
| N2 | Match the real world: the user's words | Nielsen (same) | Every label and tooltip says what really happens ("Clear slot 3", not "Delete"); US spelling; no internal terms (stack, slot, pane) unless the UI teaches them | all |
| N3 | User control and freedom: clear exit, undo | Nielsen (same); Shneiderman "easy reversal" ([UMD, search](https://www.cs.umd.edu/~ben/goldenrules.html)) | Esc closes every overlay, menu and window; every destructive action has Undo or goes to trash; walk each exit path once | all |
| N4 | Consistency and standards | Nielsen; Shneiderman "consistency" | Same action, same icon, same words, same place on every screen; Windows conventions (Ctrl+F find, Esc close, Ctrl+Z undo, F1 help) hold | all |
| N5 | Error prevention | Nielsen; Shneiderman "prevent errors" (gray out what cannot apply) | Destructive controls are not next to frequent ones; controls that would do nothing are dimmed, not silently inert; double-clicks on Delete are absorbed | pad bars, menus, Settings |
| N6 | Recognition rather than recall | Nielsen; Shneiderman "reduce short-term memory load" | Icon-only buttons have a tooltip with the bound key; the shortcut map is reachable from the UI, not only by a key the user must know | bars, menus, shortcut map |
| N7 | Flexibility and efficiency: shortcuts for experts | Nielsen; Shneiderman "universal usability" | Every frequent pointer action has a key; every key is shown somewhere (map, tooltip, menu accelerator text) | all |
| N8 | Aesthetic and minimalist design | Nielsen; Rams "as little design as possible" ([Vitsœ, search](https://www.vitsoe.com/us/about/good-design)) | Count elements per screen that the user never needs at that moment; each one is a finding unless it earns its place | all |
| N9 | Recognize, diagnose, recover from errors | Nielsen | Save-failure card: plain words, says what happened, offers a fix; no codes or stack traces shown to the user | save-failure card, toasts |
| N10 | Help and documentation | Nielsen | Help is reachable (F1, map), searchable or scannable, task-shaped | shortcut map |
| S1 | Dialogs that yield closure | Shneiderman (same) | Multi-step flows (rebind a key, search and jump) end in a clear state; focus lands somewhere sensible | Settings, search |
| S2 | Internal locus of control | Shneiderman (same) | Nothing moves, opens or steals focus without a user action (except the hotkey summon) | all |
| K1 | Self-evident: "Don't make me think" | Krug, *Don't Make Me Think* ([O'Reilly ch. 1, search](https://www.oreilly.com/library/view/dont-make-me/9780133597271/ch01.xhtml)) | A first-time user can say what each visible control does without hovering; mark each one that needs a hover or a guess | all |
| U1 | Undo beats "Are you sure?" | NN/g confirmation dialogs ([search](https://www.nngroup.com/articles/confirmation-dialog/)): confirm only serious, hard-to-undo actions; undo is better | List every confirmation dialog and every action with no undo; each pair is checked against this rule | all destructive actions |
| U2 | Keep consequential options apart | NN/g ([search](https://www.nngroup.com/articles/proximity-consequential-options/)) | Delete/Clear is spatially apart from New, arrows and Close; measure the gap | pad bars, toasts, menus |

## 2. Behavior: Norman

| ID | Principle | Source | How to test it here | Screens |
|---|---|---|---|---|
| DN1 | Signifiers show where action is possible | Norman, *The Design of Everyday Things* ([jnd.org, book](https://jnd.org/the-design-of-everyday-things-revised-and-expanded/)) | Hidden bars (fade, edge reveal): is there any cue that a control exists before hover? List every hover-only control | pad bars, pager, split |
| DN2 | Mapping: control layout matches its effect | Norman (same) | Left/right arrows move left/right; split's controls act on the pane they sit in; swap icon names its destination | pager, split |
| DN3 | Feedback on every action | Norman (same) | Every click shows a press state and a result; no dead clicks | all |
| DN4 | Conceptual model | Norman (same) | Standard vs Permanent vs slots: can the UI alone explain the model? Note where it relies on docs | pad, Settings |

## 3. Pointer targets and choice

| ID | Principle | Source | How to test it here | Screens |
|---|---|---|---|---|
| F1 | Fitts's law: time grows with distance, falls with size | NN/g ([search](https://www.nngroup.com/articles/fitts-law/)) | Measure each control's hit box (not its glyph). Frequent actions near where the pointer already is; window edges count as large targets | all |
| T1 | Floor: 24 x 24 px target, or 24 px spacing | WCAG 2.2 SC 2.5.8 AA ([W3C, search](https://www.w3.org/WAI/WCAG22/Understanding/target-size-minimum)) | Any hit box under 24 x 24 whose 24 px circle overlaps a neighbor is 🔴. Read sizes from the window dump, not from screenshots | all |
| T2 | Comfortable size: 32 px pointer, 40 px Windows typical | Windows targeting 40 x 40 (`docs/design-fluent.md`, read 2026-11-03); WCAG 2.5.5 AAA 44 x 44; Material 48 dp touch ([Google, search](https://support.google.com/accessibility/android/answer/7101858)); Apple: padding for comfortable hit regions, no macOS number ([HIG pointing devices, search](https://developer.apple.com/design/human-interface-guidelines/pointing-devices.md)) | Our `ButtonSize` 32 is the house target. Between 24 and 32 is 🟡 when the control is frequent. Touch sizes do not apply (pointer-first desktop app) | all |
| H1 | Hick's law: more choices, slower decisions | Laws of UX ([search](https://lawsofux.com/hicks-law/)) | Count top-level choices in menus and Settings sections; long flat lists are candidates for grouping or progressive disclosure | menus, Settings |
| P1 | Progressive disclosure | Laws of UX Hick's page (same); A34 decision 18 | Rare options sit behind one step; common ones are one click | Settings, bars |
| D1 | Doherty threshold: respond under 400 ms | Laws of UX ([search](https://lawsofux.com/doherty-threshold/)), IBM 1982 | Feedback (not the whole task) under 400 ms. Skipped where it is animation timing (motion is out of scope); kept for "is there feedback at all" | all |

## 4. Visual

| ID | Principle | Source | How to test it here | Screens |
|---|---|---|---|---|
| V1 | 4 px grid for sizes and gaps | Fluent 2 layout (`docs/design-fluent.md`, read); our target `docs/design-scale.md` | Stage 1 token audit: every literal margin, padding, gap and size off the scale is a finding or a named exception | all |
| V2 | Type ramp: few even sizes, 12 px floor Regular, 14 px floor Semibold | Windows typography (`docs/design-fluent.md`, read) | Every text size maps to a ramp step; any text under 12 is 🟡 | all |
| V3 | Corner radius: 4 controls, 8 overlays | Windows geometry (`docs/design-fluent.md`, read) | Every radius maps to a `Shape` role; nested radii shrink inward | all |
| V4 | Whole-pixel strokes, 1 or 2 | Fluent 2 shapes (same) | Fractional strokes are findings unless a named exception | all |
| V5 | Grid and alignment: elements share edges | Müller-Brockmann, *Grid Systems in Graphic Design* (book; [CUNY excerpt, search](https://openlab.citytech.cuny.edu/langecomd3504fa2019/files/2018/10/MullerBrockmann_Grid_Des-Phil.pdf)) | Overlay screenshots with a 4 px grid; list left edges and baselines that miss by 1 to 3 px | all |
| V6 | Hierarchy by size, weight and color; space as a system | Refactoring UI, Wathan and Schoger (book; [summary, search](https://rjseymour66.github.io/webdev/refactoring-ui/)) | Each screen has one clear primary element; secondary text is lighter, not just smaller; spacing comes from the scale | all |
| G1 | Gestalt proximity: near means related | NN/g ([search](https://www.nngroup.com/articles/gestalt-proximity/)) | Gaps inside a group are smaller than gaps between groups; check Settings rows and bar clusters | Settings, bars, menus |
| G2 | Gestalt similarity and common region | NN/g (same); A34 "common fate" | Same-kind controls look alike; cards and pills enclose only what belongs together | all |
| R1 | Restraint and honesty (Rams) | Rams ([Vitsœ, search](https://www.vitsoe.com/us/about/good-design)) | No decoration that implies a function it lacks; nothing looks clickable that is not | all |
| L1 | Layout stability: controls do not move between states | A34 decision table (`.coord/research/A34-edge-cases.md`) | Toggle each state (empty, one note, split, search open); controls that shift position are findings | bars, pager, split |

## 5. Contrast, color, focus, light and dark

| ID | Principle | Source | How to test it here | Screens |
|---|---|---|---|---|
| C1 | Text 4.5:1 (3:1 for large text) | WCAG 1.4.3 AA ([W3C, search](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html)); do not round 4.499 up | Compute from palette values, not screenshots. Translucent window: test at the lowest tint preset over a white and a black backdrop, both themes. Placeholder and hover text count | all |
| C2 | Non-text 3:1: control edges, icons, state | WCAG 1.4.11 AA ([W3C, search](https://w3.org/WAI/WCAG22/Understanding/non-text-contrast)); disabled controls exempt | Icons, field borders, toggle tracks, selected and focus rings against their neighbor color; both themes, lowest tint | all |
| C3 | Focus is visible; ring at least a 2 px perimeter, 3:1 against unfocused | WCAG 2.4.7 AA; 2.4.13 AAA ([W3C, search](https://www.w3.org/WAI/WCAG22/Understanding/focus-appearance)); Microsoft: custom controls need "a clear visual focus indicator" ([keyboard accessibility, read](https://learn.microsoft.com/en-us/windows/apps/design/accessibility/keyboard-accessibility)) | Tab through every screen; every stop shows a ring; ring thickness from `Shape`/stroke tokens | all |
| C4 | Color is never the only signal | Microsoft color ([read](https://learn.microsoft.com/en-us/windows/apps/design/style/color)): no red/green as sole differentiator | Delete red, selected dots, error states: each also differs by shape, text or position | bars, pager, toasts |
| C5 | Accent used sparingly, for state and importance | Microsoft color (same) | Count accent uses per screen; accent at rest on non-primary items is a finding | all |
| C6 | Light and dark parity | Microsoft color (same) | Every screen captured in both themes; anything that exists or reads in one and not the other | all |

## 6. Keyboard

| ID | Principle | Source | How to test it here | Screens |
|---|---|---|---|---|
| KB1 | Everything pointer-operable is keyboard-operable (Enter, Space) | Microsoft keyboard accessibility ([read](https://learn.microsoft.com/en-us/windows/apps/design/accessibility/keyboard-accessibility)) | List controls with no key path | all |
| KB2 | Tab order follows reading order; disabled items leave it | Microsoft (same) | Tab and Shift+Tab through each screen; note jumps | Settings, search, card |
| KB3 | Shortcuts are discoverable: tooltip shows the key; access keys | Microsoft (same): "Always make shortcuts discoverable" | Every icon button tooltip names its key; menu items show accelerators | bars, menus |
| KB4 | F6 moves between panes (must be built explicitly) | Microsoft (same) | Split: is there a key to move focus between panes? | split |

## 7. Icons (feeds stage 2)

| ID | Principle | Source | How to test it here | Screens |
|---|---|---|---|---|
| I1 | One icon font per meaning; codes are font-specific | Segoe Fluent Icons ([read](https://learn.microsoft.com/en-us/windows/apps/design/style/segoe-fluent-icons-font)): most glyphs sit in the Unicode Private Use Area, and that is an interoperability problem when the font is missing; PUA codes "only mean something" for their font ([Microsoft PUA, search](https://learn.microsoft.com/en-us/globalization/encoding/pua)) | Every icon string is resolved by name through one registry (name, font, code). A raw code string in product code is a finding. Selftest: every registered code exists in its font | all |
| I2 | Icon sizes on the font's grid: 16, 20, 24, 32 | Segoe Fluent Icons page (same): "use these specific sizes: 16, 20, 24, 32, 40, 48, and 64"; Windows iconography ([read](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/iconography)): 1 epx stroke, square em footprint | Icon sizes 13, 14, 15, 18 are off the published sizes; each is a finding or a named exception (14 sits with text, see `design-scale.md` 2.4) | all |
| I3 | One style family: Segoe Fluent is monoline; do not mix with filled sets without a rule | Windows iconography (same) | Contact sheet: icons from the bundled Fluent System Icons font next to Segoe glyphs; flag weight and style mismatches | all |
| I4 | Modifiers in a bottom quadrant; state by layering | Windows iconography (same) | Pinned, unsaved, active marks: placement consistent | bars, pager |
| I5 | Icon-only controls need a label | NN/g icon usability ([read](https://www.nngroup.com/articles/icon-usability/)): a label should be present; do not rely on hover | NN/g wants visible labels; our bars are deliberately icon-only. Test the weaker rule: tooltip plus automation name on every icon button, and visible labels wherever space allows (menus, Settings). Each icon-only control is listed with its fallback | bars, pager, search tools |
| I6 | Not in-line with text | Segoe Fluent Icons page: font "not intended for use in-line with text" | Glyphs placed inside text runs (map, toasts) are findings | shortcut map, toasts |

## 8. Writing

| ID | Principle | Source | How to test it here | Screens |
|---|---|---|---|---|
| W1 | Sentence-case labels and titles | Microsoft style guide ([search](https://learn.microsoft.com/en-us/style-guide/capitalization)); Windows writing style ([search](https://learn.microsoft.com/windows/uwp/design/style/writing-style)) | Every label, title, tooltip, toast: sentence case, US spelling (`AGENTS.md`) | all |

## Where sources disagree, and the house rule we test against

- **Target size.** WCAG AA says 24, AAA 44, Windows 40 typical, NN/g 1 cm (touch), Apple gives no macOS number. House
  rule for this pass: under 24 fails (🔴), 24 to 31 is 🟡 when frequent, 32 (`ButtonSize`) passes.
- **Icon labels.** NN/g wants visible labels always; the app's bars are icon-only by design to stay minimal (N8, Rams).
  House rule: tooltip with key plus automation name is required; a missing one is 🟡; visible labels are proposed only
  where they fit without crowding.
- **Dialog radius.** Windows says 8 for dialogs, Fluent 2 says 12 for popovers; `design-scale.md` picked 12 (Lg). Stage 1
  tests against `design-scale.md`.

## Not covered or not verified

- Motion: Windows timing and easing, Doherty as an animation budget. Out of this pass.
- Nielsen's 0 to 4 severity scale: not read; we use the three-level scale above.
- The Refactoring UI and Müller-Brockmann rows rest on the books; no online wording was checked.
- Segoe Fluent Icons fallback when a code is missing in the font: not found on the part of the page read. Stage 2 tests it
  directly by rendering.
- Material state-layer opacities: not found in the sources read; our hover, press and selected washes are compared to
  `design-scale.md` only.
