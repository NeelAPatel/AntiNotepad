# AntiQuickPad

Windows 11 hotkey-summoned, chrome-free translucent scratchpad, inspired by AntiNote (functionality, not a 1:1 copy). Speed of show/hide is the priority. Stack is undecided: spikes A (Avalonia + AvaloniaEdit) and B (GPUI + GPUI Kit) are being compared; C (Tauri + CodeMirror 6) is backup only.

## Rules
- Local git only. No GitHub. Commit per completed task.
- One writer per folder: `spikes/A`, `spikes/B`, `docs/`, `.coord/`.
- Keep this file minimal. Put detail in linked docs.
- Tests are small smoke checks of capability, not app coverage. Disposable.
- First-party libraries first; other stacks/libraries only on a concrete need, and only if maintained and documented.
- Update `.coord/STATUS.md` at every dispatch, milestone and completion. Never rely on chat memory.

## Roles (Codex usage first, Claude manages)
- CEO / manager: whichever Claude session is driving (currently Sonnet 5.5 medium; Opus 5.5 is the intended final reviewer when available). Directors: Codex `gpt-6.1-sol` medium. Workers: Codex `gpt-6-luna` high (backup `gpt-5.6-luna` xhigh). Junior reviewer: Claude Sonnet 5.5 medium (visible session).

## Read when
- Starting or resuming any work: `.coord/STATUS.md`, then `.coord/HANDOFF.md`.
- Launching or messaging agents: `docs/agents.md`.
- Building or judging a spike: `docs/spike-spec.md`.
- Why something was chosen: `docs/decisions-log.md`.
- Your task: `.coord/tasks/<id>.md`; write your result to `.coord/reports/<id>.md`.
