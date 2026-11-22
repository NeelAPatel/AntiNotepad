# Decisions (locked unless the user reopens them)

Newest first. History before 2026-10-23 (overnight build log and write-up) is in git history and the tags `v0.1`,
`antipad-v*` and `baseline/m8c-v1-compact`.

## Locked choices (short list; detail and quotes below)
1. Product: Windows 11 hotkey-summoned translucent scratchpad, Avalonia 12 + AvaloniaEdit 12 on .NET 10; show/hide speed first.
2. Design changes are lab-first: the user approves the lab before the real change.
3. Never type into or stop the installed copy; dev uses `tools/dev.ps1`; upgrade only on request with `tools/install.ps1`.
4. Codex implements every item in a build chat; Codex checks run offline only; Claude briefs, gates and relays.
5. One chat per item (Findings through hand test; user answers it); it never starts a chat (v5.4.0).
6. Directors (top-level chats) route work, own STATUS, and alone merge to dev and main; the user is the CEO.
7. Item branches are `<prefix>/<ID-slug>` in worktrees made at Approve; dev gets tidy commits plus one `--no-ff` merge per item.
8. `main` is releases only: fast-forwarded on the user's go and tagged semver.
9. IDs: prefixes `A`, `Loop`, `D`, `T`, `L`; a number only on the user's confirmation; always `<ID>-<slug>`.
10. Tests are small `--selftest` capability checks; every fixed bug adds a `CONTRACTS` row.
11. Hot files stay small: STATUS 8 KB, `docs/loop.md` 8 KB, lessons 3 KB, item cards 4 KB; history goes to archive files.
12. Mods are enabled in user scope or the local settings file, never in tracked project settings.
13. Approve cap is 3 items; only the user allows a one-off bypass.
14. Never run `AntiNotepad.exe --quit` without `ANTINOTEPAD_DATA`; no Computer Use without the user's go.
15. US spelling in UI; first-party libraries first.
16. Feel defaults are vertical Rate-matched and horizontal Follow-through; the vertical styles keep their pre-A20 glide and stop at note ends, while horizontal edge-band parameters live in `docs/scroll-feel.md`.

## 2026-11-18
- **Note navigation keys** (user, 2026-11-18): Alt+Left and Alt+Right switch notes, previous and next. They stay as they are; Ctrl+[ and Ctrl+] change list level; the shortcut cleanup item decides the remaining navigation keys.

## 2026-11-15
- **Scroll feel** (A20-overscroll-speed): Settings calls the four existing vertical modes **Vertical scroll style** and adds a one-option **Horizontal scroll style**, Follow-through. Rate-matched remains the default inside notes; Follow-through is the horizontal default. On 2026-11-15, after the round 2 hand test, the user asked to remove vertical overscroll and keep the existing Windows 11 style smooth scrolling; vertical scrolling stops at note ends with no carry-over. Horizontal paging uses projection-based commit and spring return, with edge resistance only at the first and last notes. Locked horizontal values are in `docs/scroll-feel.md`.

## 2026-11-12
- **Motion reference:** User approved the lab on 2026-11-12. Overlay entry uses 167 ms and cubic-bezier(0, 0, 0, 1); exit uses 83 ms and cubic-bezier(1, 0, 1, 1). The reusable Windows 11 durations are 83, 167 and 250 ms. The 16 px slide is our choice; Microsoft does not prescribe a popup offset. Later Settings and window-motion items reuse the `Win11*` tokens. Sources: Microsoft Learn [Timing and easing](https://learn.microsoft.com/en-us/windows/apps/design/motion/timing-and-easing) and [Motion in practice](https://learn.microsoft.com/en-us/windows/apps/design/motion/motion-in-practice).
- **Loop slim-down** (Loop9-loop-slim): usage rose after Level 4 because long chats re-read big contexts, STATUS was 82 KB
  and read by every director, the compaction hook printed every director's notes, project `ask` rules prompted on every
  STATUS edit, and an enabled mod rewrote tracked `.claude/settings.json`. Level 4 itself (about 280 characters per turn)
  was not the cause. Fixes: hot-file budgets, finished history in archives, the compaction hook prints only the active
  director's notes, `ask` only on `docs/loop.md` and `AGENTS.md`, no force-push hints, approve ignores settings drift.
  User: "put it on the roadmap with the correct prefix", run overnight on a branch, merge after reading the report.
- **Rationale moved out of `docs/loop.md`:** priority (user, 2026-10-29): good development and codebase understanding
  first, then saving usage; wasted usage is back-and-forth, so use widgets that cut it. Side tasks (user, 2026-10-31):
  roadmap work may skip the pipeline "as its not a critical piece of the project". Loop freeze (user, 2026-10-31, lifted for
  `Loop4-retro-fixes`): rules change only when lessons show the same miss twice or the user asks. Eras: Era 0
  (2026-10-17 to 2026-10-29) used `A`, `C`, `N`, `P`, `S<n>-status`, all retired.

## 2026-11-06
- **Merge policy: tidy commits plus a `--no-ff` merge commit, worktree by default** (Loop8-merge-policy, loop v5.2.0;
  supersedes the squash rule of the 2026-10-29 entry below). User to Master 4: "i prefer #2's bubbles ... so that i can
  atleast trace out which incremental change broke something", "i'm more worried about slop commits than high number of
  comits" and "let this be the standard as well. worktree preferred for parallelization unless explicitly said not to".
  To Loop Hub 2: "doesnt need to stay limited to 5, we can go by ear IMO. before merging, lets have a step to check the
  list of commits made on it and see if any of them can be squashed together, such as minute tiny changes, or a quick
  fix, etc." and "I'm okay with meaningful commits not commits for fixing 3 lines with a bit of back and forth".
  Rule: before Merge the item's session reviews `git log dev..<branch>` and folds tiny and back-and-forth commits
  (no fixed count, every commit builds); the branch is rebased onto dev (catch-up rebases; later: no push, merge from the local branch;
  originally pushed with `--force-with-lease`); the master lands it with `git merge --no-ff --no-commit`, adds STATUS and lessons, and
  commits once; no separate status commits; the branch is deleted with `git branch -d`. The merge preflight builds and
  selftests each commit that touches product code and names a failing one. Every item gets a worktree unless the user
  says otherwise. Also: the gate-Head check ignores later commits that touch only the card, reports or STATUS; the round
  brief's Files list is required. Routing (user: "routing wise use sonnet, haiku subchat should be used when the task is
  obvious enough that there will be no need for me to 'travel' with the delegation"): Sonnet when the user will refine the
  ask in that chat, Haiku when the brief fully specifies the job. Rejected: keeping squash (hides which change broke
  something), a fixed five-commit limit.

## 2026-10-29
- **Loop v3 process** (Loop1-process-v3; the discussion record `.coord/process-redesign.md` was deleted on 2026-10-31, in git history): IDs `A` app, `Loop`
  dev loop, `D` docs, `T` non-AI tooling, counters reset per era; phase "Go" renamed **Approve**; branches
  `<prefix>/<ID-slug>` with `feat/`, `fix/`, `refactor/`, `docs/`, `chore/`, created at Approve, worktrees mirroring them;
  one squashed commit per item on dev carrying its STATUS edits (no separate status commits; **superseded 2026-11-06**:
  tidy commits plus a `--no-ff` merge commit, see above); items over about 400
  product lines split into lettered steps; `main` fast-forwards at releases with semver tags; no pull requests until the
  release pipeline; at most 3 items past Approve; Claude attribution off. Rejected then: keeping `--no-ff` merges plus
  status commits (bloated history; the 2026-11-06 rule keeps the merge but drops the status commits), PRs now (too much
  overhead for one person).
- **Coding standard** (`docs/coding.md`): guide-style comments (seven rules, incl. the pipeline line), `.editorconfig`
  checked on touch, files split by job with a 600-line review trigger (rejected: hard line caps), folders mirror
  namespaces. Applied on touch; no big reformat pass.

## 2026-10-25
- **Bar insets and chrome tokens** (A35-bar-insets; supersedes the 2026-10-22 bar-style gradient/30 px gap and edge fade 40,
  and the 2026-10-23 4 px icon hover): every top bar and pager measurement lives in `ChromeTokens` (`UI/Chrome.cs`); no
  per-control numbers. Top bar: Hide 32 px at 4 px from top/right, items centered on y 20, badges 20 tall, leftmost item
  10 px, docked row 40. Pager pill: 32 px buttons in a 4 px inset and 1 px border (42 tall), solid Surface in every style
  and material, 4 px from the bottom, float gap 8; docked pager has its own 50 px strip. Hover radii 8 (icons, arrows,
  count), 9 (dots). Text edge fade 12. Overlay/float top bar sits on a blur of the note text (radius 10, Canvas tint 0.4;
  text masked out to 40, half at 46, back by 52), never a solid or gradient band; the window material shows through.
  The overlay/float top gap is 40 px of scrollable whitespace before the document, not part of line 1. Toolbar glyphs
  are centered by their ink. Rejected in the lab: an Acrylic popup window for the pill (glitchy, wrong corners), the
  feathered gradient, Mica for the pill, and blur behind the pill (user picked solid; A36 removed the lab).
  Values updated by A4n-scale-lab (user, 2026-11-05: "Use Recommended, everythign looks good"; hand test "looks good"):
  where the numbers above differ, the current values in `docs/design-contract.md` and `ChromeTokens` win (for example
  leftmost item 12, dot hover radius 8, dots 8, dot hit height 32, dividers 8). ChromeTokens stays the one owner;
  pill bottom gap stays 4 (user tried 8, then "Reduce to 4 and 4").
- **Native window** (user decision; implementation reverted): the pad is the product and must meet every native Windows 11
  window expectation, while its top stays chrome-free (no title text or caption buttons): Windows-drawn rounded corners,
  shadow, border, resize edges and native open/close/minimize/restore animations; Snap, Maximize, Alt+Tab while open or
  minimized, snap layouts; a closed pad invisible to Windows. A32-native-show-hide-animation and A34-native-window-frame
  tried this by forcing caption-window styles onto the frameless window, which lost the corners and shadow and broke all
  three bar styles; dev was reverted to just after A30 (A31-A33 reverted, A34 dropped). Any retry must not change the
  window's frame styles and goes lab-first.

## 2026-10-24
- **Windows session end** (A26-allow-windows-shutdown): synchronously flush panes and save all dirty notes once on Avalonia's Win32 shutdown request, log failures without UI, and let both resident windows close; ordinary closes still stay resident.
- **Taskbar icon** (A20-taskbar-icon-setting, A22-taskbar-button-fixes): Defaults to Never; Settings offers While visible, Always and Never. One stable minimized stand-in supplies the pad's button in both visible and hidden states (Always keeps it while hidden); the real pad stays off the taskbar to avoid Avalonia's owner-change hide/re-show. Clicking the button hides the focused pad or shows/focuses it otherwise. Settings keeps its own button; the tray stays available in every mode.

## 2026-10-23
- **Notes location and recovery** (A7-notes-folder-and-recovery, decision 1): app home remains `%LOCALAPPDATA%\AntiNotepad`
  for the log and `location.json`; notes live in the user-visible folder `Documents\AntiNotepad` by default, using
  `SpecialFolder.MyDocuments` so OneDrive redirection is followed. Settings can move notes to another folder. Moves copy and
  verify the index, backup index, note bodies and recovered files before updating the pointer, then retain the previous copy.
  Unknown Markdown bodies are moved to `recovered\`; uninstall keeps the notes folder even when app home is removed.
- **Branches** (C2-stale-branch-cleanup): only `main` and `dev` are kept long-term; old spike, lab and feature branches are
  deleted. The Icon Lab (`--iconlab`) is gone with its branch and gets remade if needed; the N3 release plan is kept as tag
  `proposal/N3-release-plan`.
- **Archive removed** (C1-remove-archive): the `archive/` folder (old AntiQuickPad project, Antipad spike notes) is deleted
  from the tree; reach it through git history or the tags above.
- **Project**: AntiNotepad replaces AntiQuickPad. The old app, spikes A/B, demos, UI-audit kit and old docs moved to
  `archive/antiquickpad/` (since removed, see above).
- **Icons**: Segoe Fluent Icons, 16 px, palette Ink, drawn once (no faux-bold), snapped to whole pixels. Toolbar and pager
  buttons 32 px square with a 4 px rounded hover. The newer Fluent UI System Icons font stays bundled for Settings icons.
- **Right-click menu**: AntiNotepad's own popup in Windows 11 style (designed in the Icon Lab): 8 px rounded panel, 4 px insets,
  32 px rows, icon column, dimmed shortcuts, content width. Order: Undo, Redo, Select all | New note, Search notes, Split view,
  Keep on top | Settings, Hide AntiNotepad; Cut, Copy, Paste, Delete as an action row on top. Settings: Menu action row (on),
  Colored menu icons (off). The popup window has no system rounding, border or shadow (DWM attributes).
- **Codex does the implementation**; the Claude manager briefs, gates, commits and relays.

## 2026-10-22
- **Bar styles**: Docked (default), Overlay (bars slide in within 20 px of the top/bottom edge), Float below (pager in its own
  popup under the window, 8 px trigger). With overlay/float the first line gets a 30 px gap that scrolls away.
- **Edge fade** on by default (40 px). Text badge and "Saved <time>" label fade with the toolbar icons.
- **Markdown**: `__x__` is underline (`**` bold, `++` also underline); headings h1-h6 in a rainbow; `---`/`***`/`___` draws a
  rule; numbered lists renumber per level on Return/Tab; a heading's fold also ends at two blank lines.
- **Memory**: Settings' live trace runs only while something moves; closing Settings compacts the heap and trims the working set.
- **Scrolling** (user's mouse: MX Master 3S, whole clicks only): vertical modes Lines (eased glide), Free, Rate-matched (default),
  Wheel-driven; note paging by Detent or Progress swipe (default; 220 px of input = one note, fires at 0.35, re-arms after 110 ms).

## Loop version history

v5.2.0 (user, 2026-11-06: `Loop8-merge-policy`, minor bump). v5.1.0 was the same day (`Loop7-top-level-rules`). v5.0.0 was set on 2026-11-04 by `Loop6-loop-v5-wiki` (v4 skipped by the user; based on v3.2 plus `Loop5-reply-style`).

v3.2 (2026-10-31, `Loop4-retro-fixes`: Codex roles by job and no run caps, offline-only Codex checks,
expected time in the plan, a side-task fast lane, masters per area with sub-sessions per phase, master notes after
compaction. v3.1 was 2026-10-30, `Loop2-loop-v3-1`: approve and merge preflights, catch-up with dev, strict scope,
read-only audits, backlog scans with a delta at task time, loop-lint; the dormant v2.2 drop absorbed). v3 was
`Loop1-process-v3`: Approve phase, A/Loop/D/T IDs and eras, type-prefixed branches and nested worktrees, squash merges
with STATUS inside (replaced in v5.2.0 by rebase plus a merge commit), roles, widgets, `docs/coding.md`. v2 was C26-item-card-loop, v2.1
C28-loop-cheap-checks; v1 was C13-dev-loop-kit through C25. v2.2 was never activated; v3.1 absorbed it (its notes were deleted on 2026-10-31; git history keeps them).
A merged item that changes the phases, the roles or who runs which phase bumps the major version (new master at the next
handover); new checks, scripts, routes or research steps bump the minor version and need no new master. Changes to chat output only (reply style, status card or hook wording) bump the patch version (`x.y.1`).

- v5.4.0 (2026-11-12, Loop11-one-chat-flow): one chat per item; it sets the tier, runs Findings to hand test, owns its STATUS row; director only writes Request and merges.
- v5.3.0 (2026-11-08, L6-lv4-docs-diet): docs/loop.md split by audience, coding rules path-scoped, L prefix added.
