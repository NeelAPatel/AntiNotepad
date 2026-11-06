# Decisions (locked unless the user reopens them)

Newest first. History before 2026-10-23 (overnight build log and write-up) is in git history and the tags `v0.1`,
`antipad-v*` and `baseline/m8c-v1-compact`.

## 2026-11-06
- **Merge policy: tidy commits plus a `--no-ff` merge commit, worktree by default** (Loop8-merge-policy, loop v5.2.0;
  supersedes the squash rule of the 2026-10-29 entry below). User to Master 4: "i prefer #2's bubbles ... so that i can
  atleast trace out which incremental change broke something", "i'm more worried about slop commits than high number of
  comits" and "let this be the standard as well. worktree preferred for parallelization unless explicitly said not to".
  To Loop Hub 2: "doesnt need to stay limited to 5, we can go by ear IMO. before merging, lets have a step to check the
  list of commits made on it and see if any of them can be squashed together, such as minute tiny changes, or a quick
  fix, etc." and "I'm okay with meaningful commits not commits for fixing 3 lines with a bit of back and forth".
  Rule: before Merge the item's session reviews `git log dev..<branch>` and folds tiny and back-and-forth commits
  (no fixed count, every commit builds); the branch is rebased onto dev (catch-up rebases, pushing the item branch only
  with `--force-with-lease`); the master lands it with `git merge --no-ff --no-commit`, adds STATUS and lessons, and
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
  where the numbers above differ, the Recommended column of `docs/design-scale.md` section 3.2 wins (for example
  leftmost item 12, dot hover radius 8, dots 8, dot hit height 20, dividers 8). ChromeTokens stays the one owner;
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
