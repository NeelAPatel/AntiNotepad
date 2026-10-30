# Loop v2.2: dormant drop

**Status: DORMANT.** dev and main run **loop v2.1** (`docs/loop.md`). Everything under `docs/v2.2/` and `tools/v2.2/` is
stored, reviewed and versioned, but nothing in the live loop reads it: not `tools/gate.ps1`, not the skills, not the hooks,
not `docs/loop.md` rules. v2.2 starts only when the user says so (user, 2026-10-29: "dev and main will remain on v2.1 until
i explicitly trigger the v2.2 changes").

> **Superseded in part by loop v3** (`Loop1-process-v3`, 2026-10-29, `docs/loop.md`): dev now runs v3, not v2.1; items land on dev as one squashed
> commit (not `--no-ff` plus a status commit), branches are `<prefix>/<ID-slug>` in `.claude/worktrees/<prefix>/<ID-slug>`,
> and up to **3** items may be past Approve (not 2). The rest of v2.2 stays dormant as written.

| File | What it is |
|---|---|
| `docs/v2.2/README.md` | this page: status, contents, how to activate |
| `docs/v2.2/DISCUSSION.md` | summary of the v2.2 discussion: why, evidence, decisions, rejected options, open questions |
| `docs/v2.2/CHANGELOG.md` | structured change log: every change v2.2 makes, step by step, and its state (all "not applied") |
| `docs/v2.2/ACTIVATION.md` | the exact edits to apply per target file when v2.2 is triggered |
| `docs/loop-v2.2-plan.md` | the full plan (Now / Proposed / cost / touches) |
| `docs/wiki/Loop-v2.2-Plan.md` | plain-words version, published on the GitHub wiki |
| `tools/v2.2/files-overlap.ps1` | built and tried; overlap check between in-flight cards. Not called by anything |
| `tools/v2.2/catch-up.ps1` | built and tried; merges dev into a branch and reports CLEAN / CONFLICT. Not called by anything |

## Using the scripts before activation
They are safe to run by hand: `files-overlap.ps1` only reads; `catch-up.ps1` merges `dev` into the checked-out
`feat/<ID-slug>` (refuses on a dirty tree, never pushes, never rebases). Running one does not make the item "v2.2".

## Activating v2.2
Only on the user's explicit word. Then, as one Medium process item (the master runs the loop on it):
1. Work through `ACTIVATION.md` in the order of the CHANGELOG steps; each step is confirmed by the user before it gets an ID.
2. Move the scripts from `tools/v2.2/` to `tools/` and wire them in (gate step, skills) as ACTIVATION.md says.
3. Bump the loop version text in `docs/loop.md` ("Sessions") and, if the user calls it v3, hand over to a new master.
4. Run the rehearsal step before the first real parallel items.
