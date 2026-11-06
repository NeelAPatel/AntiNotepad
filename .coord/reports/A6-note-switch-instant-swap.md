# A6-note-switch-instant-swap

## Summary
Implemented the three approved lab variants, selected by `ANTINOTEPAD_NOTE_SWITCH_LAB=1|2|3` in a compile-time dev build. The pad and Settings preview share the transition path; navigation and document binding remain synchronous so repeated gestures cannot queue stale navigation. No variant has been selected for release, and visible behavior remains unverified.

## Change list
Paths below are relative to `product/AntiNotepad/`.

| File:line-range | Change | Plan line served |
|---|---|---|
| `Editor/PadView.cs:1-60` | Header and per-pane transition state | Three-variant lab |
| `Editor/PadView.cs:137-137` | Cancel motion on detach | Settled-pane contract |
| `Editor/PadView.cs:273-276` | Cancel old motion before any bind, including same-note binds | Repeat and cleanup safety |
| `Editor/PadView.cs:429-432` | Cancel motion on release | Settled-pane contract |
| `Editor/PadView.cs:451-572` | Pull staging; immediate bind; snapshot departure; lazy overlay container; arrival and resource cleanup | Variants 1-3 and re-entrancy |
| `Motion/MotionTokens.cs:1-7` | Describe geometry as well as durations | Lab motion values |
| `Motion/MotionTokens.cs:20-21` | Exit 160 ms; existing arrival geometry named as tokens | Variants 1-3; preserve 200 ms arrival |
| `UI/NoteSwipe.cs:1-7` | Header and editor import | Shared swipe/lab integration |
| `UI/NoteSwipe.cs:179-190` | Cancel previous animation and spring before following new input | Fast repeats |
| `UI/NoteSwipe.cs:217-260` | Stage last pull, read dev-only selector, navigate once, discard unused staging after boundary bumps; detached cancellation | Three variants and settled-pane contract |
| `UI/PadWindow.Navigation.cs:1-34` | Route pane binds and arrivals through SwitchTo | Stack, split and slots |
| `UI/PadWindow.Navigation.cs:174-181` | Pass right-slot arrival through the same path | Split behavior |
| `UI/MotionPreview.cs:1-1` | Header | Coding standard on touch |
| `UI/MotionPreview.cs:115-120` | Use shared SwitchTo | Settings preview |
| `SelfTest/UI.cs:1-9` | Header and check dependencies | Code tests |
| `SelfTest/UI.cs:134-145` | Once-per-gesture and opposite-direction rearm checks | Fired swipe navigates exactly once |
| `SelfTest/UI.cs:158-229` | Six no-frame/reduced-motion cases; seeded in-flight departure interrupted by real input; detached cancellation | Repeat-fire and settled-pane contract |
| `SelfTest/Contracts.cs:1-1` | Header | Coding standard on touch |
| `SelfTest/Contracts.cs:94-99` | Add note-switch-settles | Contract row |

## Behavior changes
- `NoteSwipe.Fire` still calls navigate exactly once, synchronously. It passes the last displayed pull and opacity to a PadView, cancels competing spring motion, and clears unconsumed staging in finally (including boundary bumps). Existing distances, swipe settings, pay-out and rearm values are unchanged.
- `ANTINOTEPAD_NOTE_SWITCH_LAB` is read on each committed swipe, only when `AppInfo.IsDevBuild` is true. Missing, invalid, or out-of-range values select the original arrival. There is no saved setting or production activation.
- Variant 1: a frozen image of the old page exits from its displayed pull over 160 ms; the already-bound new content stays invisible until departure completes, then arrives over the unchanged 200 ms.
- Variant 2: the old image exits over 160 ms while the new content arrives immediately over 200 ms. Variant 3: no image; the new content arrives from the old page's displayed offset and opacity over 200 ms.
- `PadView.SwitchTo` binds immediately. Deferred callbacks only finish visual work; none change the selected note or invoke navigation. Unsupported/invalid snapshot rendering, no top-level, zero bounds, and reduced motion use the normal arrival path. Snapshot allocation/render failures of type NotSupportedException or InvalidOperationException dispose the image and fall back.
- A lazy child Grid separates new content from the departure image only after the lab is used. Its first layout is applied before capture. Ordinary controls retain their original tree and pinned design snapshot.
- `PadView.Arrive` accepts optional start offset/opacity. After lab activation, it animates the child content rather than the entire pane, keeping the departure image independent. Original defaults remain 34 px and 0.25 opacity.
- `Bind`, including a same-note bind, `Release`, detach, a new pull, and cancellation stop obsolete transition handles, remove/dispose the snapshot, and restore pane/content transforms and opacity. A second fire during departure reveals the already-selected first destination before binding the second; it never drops a navigation.
- `NoteSwipe.Cancel` now also restores detached panes and emits Moved(0). Stack, slot and split selection rules remain synchronous and unchanged; right-slot arrival is passed into BindPanes instead of called afterward. The preview uses the same SwitchTo path.

## Out-of-scope files
None. This required report and the generated gate report are coordination artifacts. STATUS and the item card were not edited.

## Plan deviations
- The sequential variant uses a frozen old-page image and an immediate model/document bind, rather than delaying navigation. Its visible order remains exit then arrival. This removes deferred navigation and its stale-selection/re-entrancy hazard; the second-fire check therefore verifies interruption of visual departure with the first destination already bound.
- The initial dev reproduction and visible lab comparison were not run: the brief forbids Codex from launching the app. They remain hand checks for the sub-session.
- No design choice was made. All three alternatives are opt-in dev behavior awaiting the approved lab comparison.
- The separate check-build folder remains: automatic execution policy rejected both the guarded cleanup command and a direct literal-path Remove-Item. The manager must delete `build/A6-note-switch-instant-swap-check`. No broader deletion was attempted.

## Contracts
Added `note-switch-settles`: the pane/content restore after settling, and departure interruption never loses or repeats navigation. Automated evidence covers no-frame/reduced-motion completion and a seeded departure interrupted by real gesture input; actual animation frames, snapshot pixels and containment remain manual checks.

## Lens review
Ran the `.agents/skills/lens-review/SKILL.md` checklist myself, as directed for a worker without spawning. Checked changed callers, scope, shared state, cancellation, synchronous completion, fallback, resource ownership and layering. Fixed unused swipe staging after boundary bumps and ensured the first lazy content layout exists before capture. No remaining proven findings. This is an implementation self-review; the manager's independent fresh Codex review remains outstanding.

Unproven risks: snapshot quality/capture cost on the user's renderer and DPI; perceived continuity of variant 3; focus and clipping during the first lab reparent. These require the visible lab, not a headless claim.

## Checks run
- Final explicit Debug build into `build/A6-note-switch-instant-swap-check`, `AVALONIA_TELEMETRY_OPTOUT=1`, `NuGetAudit=false`: 0 warnings, 0 errors. Ran its DLL via dotnet with `--selftest`: `SELFTEST PASS`; `CONTRACTS: 14 rows, 13 pass, 1 manual`. Deleting this separate check folder was blocked by automatic execution policy; it remains for manager cleanup.
- Pinned design hash remains `1032fadd9067c61f4ce6299ad46b414464a0fb383a95827eaa79960cdb5c716c`, 16160 lines.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A6-note-switch-instant-swap -Offline`: `GATE: PASS`; Build, Selftest, Seam, Tokens, Format, Scope and Cleanup PASS. Offline audit warning is expected. Size warns that PadView grew from 826 to 947 lines; it remains the existing pane owner, with no unapproved file split. LoopLint SKIP (no loop files changed).
- `git diff --check`: pass. Seven changed product C# files, all on the approved list.
- First checks failed on an overstrict new timer assertion and an unconditional overlay container changing the design hash; both were corrected. One intermediate manual build replayed NU1900 from restore assets; the final offline build restored with NuGetAudit=false and had zero warnings.
- No app UI was launched or killed, no installed data used, and no git writes performed. The online gate is for the manager.

## Hand checks for the user
The sub-session should launch with the existing dev launcher, compile-time dev identity, and disposable data. Before each launch set `$env:ANTINOTEPAD_NOTE_SWITCH_LAB='1'`, then repeat with `'2'` and `'3'`; remove the environment variable for the baseline. The variable must be inherited by the launched process; it is not an in-app switch.

For each variant, compare touchpad and Shift+wheel in both directions; fast repeat swipes (including the 110 ms progress rearm), reversal, and input during departure/arrival. Check stack ends and boundary bumps, creation/removal of blank notes, left slot return to stack, right split-slot cycling, split-divider containment, and the Settings Feel preview. Hide/show and close/reopen Settings during motion; confirm no leftover image, shift, fading or changed focus. Verify reduced motion and a high-DPI screen. Pick the preferred variant before any final production change or merge.
