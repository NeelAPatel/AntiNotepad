# AntiQuickPad

Windows 11 hotkey-summoned, chrome-free translucent scratchpad, inspired by AntiNote (functionality, not a 1:1 copy). Speed of show/hide is the priority. Stack is undecided: spikes A (Avalonia + AvaloniaEdit) and B (GPUI + GPUI Kit) are being compared; C (Tauri + CodeMirror 6) is backup only.

## Rules
- Local git only. No GitHub. Branches: `main` (tagged releases only) > `dev` (integration) > `feat/<name>` (one per task, branched off dev, merged back to dev with --no-ff). Commit per completed task on its feat branch. Merge dev into main only at a release tag, on user say-so.
- One writer per folder: `spikes/A`, `spikes/B`, `docs/`, `.coord/`.
- Keep this file minimal. Put detail in linked docs.
- Name items and tasks as `<ID>-<short-slug>` everywhere (chat, briefs, commits, reports), e.g. `F26-quiet-footer-count-alignment`, `T-015-alignment-fixes`. Never a bare ID.
- Tests are small smoke checks of capability, not app coverage. Disposable.
- First-party libraries first; other stacks/libraries only on a concrete need, and only if maintained and documented.
- Update `.coord/STATUS.md` at every dispatch, milestone and completion. Never rely on chat memory.
- Function vs UI split: function tasks never touch layouts or tokens; UI tasks never touch Core, ViewModels or the controller. Which files are which: `docs/function-contract.md` (Ownership).

## Roles (Codex usage first, Claude manages)
- CEO / manager: whichever Claude session is driving (currently Sonnet 5.5 medium; Opus 5.5 is the intended final reviewer when available). Directors: Codex `gpt-6.1-sol` medium. Workers: Codex `gpt-6-luna` high (backup `gpt-5.6-luna` xhigh). Junior reviewer: Claude Sonnet 5.5 medium (visible session).

## Read when
- Starting or resuming any work: `.coord/STATUS.md`, then `.coord/HANDOFF.md`.
- Launching or messaging agents: `docs/agents.md`.
- Building or judging a spike: `docs/spike-spec.md`.
- Why something was chosen: `docs/decisions-log.md`.
- User feedback status: `docs/feedback/TRACKER.md` is the single live list. Update it in the same step as ANY change (new item, status, priority), never only in STATUS or chat. Keep the `| # | ... | Status | Priority | ... |` table shape. Live board: https://claude.ai/artifact/Q4k56bd74Y4bmtCMLwoG5X (source docs/tracker-board.html). After every TRACKER.md change run `python .coord/tools/tracker-sync.py` (refreshes the board's embedded snapshot), then the Claude manager writes the printed JSON to the board's db doc `tracker/current` (ArtifactData get for its version, then set with file_path + if_version). Codex agents skip the db write; the manager syncs after their run.
- Adding or changing a function, or building/restyling a shell: `docs/function-contract.md` (command IDs, owners, ownership by folder).
- Code layout and dependency rule: `docs/architecture.md` (Views -> ViewModels -> Core).
- Your task: `.coord/tasks/<id>.md`; write your result to `.coord/reports/<id>.md`.
