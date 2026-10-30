**>> T1-roadmap-overview-switch** · Low tier · ● Check · builder  
✓ Request | ✓ Findings | ✓ Plan | ✓ Approve | ✓ Build | ● Check | ○ Hand test | ○ Merge  
**Tier:** Low · docs-tools  
**Goal:** Switch `/roadmap` to the live server and add static snapshots.  
**Now:** Changes and report are in the worktree. The offline gate and loop-lint pass, but real server startup remains unverified here.

---

**Waiting on you:** Should `.gitignore` and `docs/wiki/Who-Does-What.md` be added to scope to remove their legacy references?

Implemented the prompt hook’s health check and hidden asynchronous start, removed the old rebuild hooks and script, added snapshot support, and updated the skill, README, and roadmap documentation. The snapshot was generated and deleted.

- `tools/loop-lint.ps1`: PASS (38 files).
- Offline gate: PASS; build had 0 warnings/errors, selftest passed.
- Hook timings: 0.81 s when the main checkout lacked the server file; with a TCP test stub, stopped start took 1.12 s and three running calls took 0.85/0.81/0.80 s. Status-card text matched and one stub process started. The stub was stopped.
- The real server could not be validated: `HttpListener.Start()` failed with “The handle is invalid.” No server remains running.
- `git grep roadmap.ps1` still finds references in `.gitignore` and `docs/wiki/Who-Does-What.md`, plus protected STATUS/reference files. I left them unchanged because they are outside the listed Files scope or explicitly protected.

Report: [T1-roadmap-overview-switch.md](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/chore/T1-roadmap-overview/.coord/reports/T1-roadmap-overview-switch.md>)