# Research: Lauren Tan (@poteto) and pstack — 2026-11-15

Read-only web research. Every claim is tagged **[V]** (verified, source given) or **[U]** (unverified / secondary only). Verified = read directly via the GitHub API on 2026-11-15.

## 1. Who she is
- Software engineer, React Compiler core team. **[V]** GitHub profile bio: "Software Engineer @xai-org & @react compiler core team", company @xai-org, 16.8k followers — https://github.com/poteto
- pstack README (her own words): worked "at Meta, Netflix, and Cursor", on the React core team on React Compiler; says she uses the skills daily "at Cursor". **[V]** https://github.com/cursor/plugins/tree/main/pstack/README.md
- Co-author of the React Compiler v1.0 post. **[V]** https://react.dev/blog/2025/04/21/react-compiler-rc (search result, page title/credit only)
- **Where she works now: conflicting.** Profile says xAI (current); pstack lives in Cursor's org and its README says "at Cursor". Secondary blogs say she joined Cursor in early/April 2026 and worked on "Grok Bot" architecture (beta Aug 2026) **[U]** (Korean newsletter / redreamality.com, not checked). Best reading: moved Cursor -> xAI recently; unconfirmed.

## 2. pstack
- Repo: https://github.com/cursor/plugins (folder `pstack/`). It is a plugin marketplace repo: 16.4k stars (whole repo), last push 2026-11-15, latest pstack commit 2026-11-15 ("new default models, one-file why and how…", #1236), previous 2026-10-27. Very active. **[V]**
- Version 0.15.42, license **MIT**, author Lauren Tan. **[V]** (`pstack/plugin.json`)
- Install: Cursor `/add-plugin pstack`; Claude Code `/plugin marketplace add cursor/plugins` then `/plugin install pstack@cursor-plugins` (commands become `/pstack:poteto-mode`). A generated Claude Code copy lives in `harness/pstack/`; CI fails if stale. **[V]**
- Structure: `skills/` (~51 skills), `agents/` (2: comment-sicko, poteto-agent), `automations/benny`, `docs/guide` (10-chapter guide) and `docs/harness/claude-code.md`. **[V]** Third-party mirrors/ports exist (backnotprop/pstack, pi-pstack, potetos-for-everyone) **[U]**.
- Design: one entry skill `/poteto-mode` picks one of 23 **playbooks** (investigation, bug fix, perf, hillclimb, feature, refactoring, prototype, visual parity, eval, babysit, shipping, autonomous run, orchestrate, autopilot, pickup, pause, worktree cleanup, opening a PR…) and runs the other skills as steps. `/setup-pstack` picks which models fill which role (defaults: Opus 5.5 for code/prose, Fable 5.2 for hardest work). **[V]**
- Skill groups (one line each, all **[V]** from README/SKILL.md):
  - Entry/help: `poteto-mode`, `poteto-help`, `setup-pstack`, `in-your-own-words` (play back the goal), `sharpen` (deepen a prompt), `bro` (plain restatement).
  - Understand: `how`, `why` (from commits/PRs), `recall`, `ketchup` (catch-up), `teach`, `blast-radius`.
  - Design/parallel: `architect`, `arena` (N attempts, keep best), `swarm`, `automate-me`, `make-bot-ui`.
  - Quality: `interrogate`, `tdd`, `no-comments`, `unslop`, `technical-writing`, `typescript-best-practices`, `benchmark-checklist`, `correct`, `reflect`.
  - Verification: `create-verification-skill`, `maintain-verification-skill`, `create-formal-verification-skill`, `maintain-formal-verification-skill`, `statechart`, `show-me-your-work`.
  - ~23 `principle-*` skills (prove-it-works, test-behavior-not-implementation, subtract-before-you-add, fix-root-causes, sequence-verifiable-units, guard-the-context-window, never-block-on-the-human, make-operations-idempotent, type-system-discipline, etc.).
- Caveat: several skills use multi-model panels, subagents, Cursor-only features (bugbot, automations, `/loop`); Claude Code adapter skips Cursor-only steps with a note. **[V]**

## 3. Other notable work (https://github.com/poteto?tab=repositories) — all **[V]**
| Repo | Stars | What |
|---|---|---|
| https://github.com/poteto/how | 970 | skill for explaining architecture (MIT) |
| https://github.com/poteto/noodle | 634 | "Orchestrate agents using skills" (MIT; last push 2026-03) |
| https://github.com/poteto/brainmaxxing | 381 | persistent memory and skill improvement (MIT) |
| https://github.com/poteto/verification-skill-example | 201 | fictional example of a project-local verify skill + feature map |
| https://github.com/poteto/hiring-without-whiteboards | 53k | list of companies without whiteboard hiring (old, not AI) |
Older: Ember/Elixir conference notes and libraries (2015–2021).

## 4. How pstack verifies agent work
Core idea **[V]**: the agent must prove the "real artifact", not a proxy (`principle-prove-it-works`); state the finish condition in the first prompt; ask for evidence as artifacts (failing then passing test, screenshot, trace); reply says "inconclusive" if a check could not run (guide chapter 06, https://github.com/cursor/plugins/blob/main/pstack/docs/guide/06-verify-and-ship.md).

| pstack piece | What it does | Taxonomy mapping |
|---|---|---|
| `tdd` + `principle-test-behavior-not-implementation` | failing test first; a test must fail when behavior breaks and survive refactors (a manual mutation-style check) | unit, regression, (mutation idea, by hand) |
| `create-verification-skill` / `verify-<app>` | generates a repo-local skill to launch, doctor, drive the real app, capture evidence, clean up; plus a feature map for sweeps | smoke, acceptance, UI/UX, regression sweep |
| `interrogate` | adversary agents on different models try to break a diff; only findings a run proves survive | functional, fuzz-ish, penetration-ish |
| `blast-radius` | finds what a small change could break, proves the safety fact by running code | regression |
| `benchmark-checklist`, perf/hillclimb playbooks | vets a measurement (7 questions, repeat runs, tuned build) | benchmark, performance |
| `create-formal-verification-skill`, `statechart` | TLA+ / model-check for races, state machines | formal verification |
| `correct` | turns repeated agent mistakes into architecture, then types, then lint, then tests; checks each rule fails on a real past mistake | static analysis, lint |
| visual-parity playbook | pixel diff against frozen baseline | UI/UX, regression |
| `show-me-your-work` | append-only TSV decision log with evidence pointer | auditing |
| shipping/babysit playbooks | independent re-verify of green stack, then land | acceptance |
Not covered: load/soak/chaos/canary/tracing/RUM/A-B (a team-scale, production topic; irrelevant to a one-user desktop app). Hooks/evals: no verification hooks seen; an `eval` playbook tests skill changes blinded **[V]**, not product code.

## 5. Ideas worth borrowing (AntiNotepad: small app, light loop, fewer gates)
**Cheap (almost no friction):**
1. **Finish condition in the brief** — one line "done = these 2-3 observable checks". Costs a sentence; makes the Codex review and hand test sharper.
2. **"Inconclusive" is an allowed result** — builder reports checks that could not run instead of implying pass. Zero cost, one rule line.
3. **Prove the real artifact** — our selftests plus actually running the dev build for a visible change; reject "it compiles". Already mostly our habit; just write the principle in one line.
4. **Behavior-not-implementation test rule** — one sentence in docs/coding.md for selftests: assert what the user sees; occasionally break the code on purpose to see the test fail. Free.
5. **`/correct` idea** — when the same agent mistake recurs, fix it as a lint rule in the gate (we already have a lint step) rather than another doc paragraph. Only on repetition, so no added gate.

**Medium (do later, once):**
6. **A project verify skill + short feature map** for the real app (launch with ANTINOTEPAD_DATA dev data, doctor, drive, evidence). Would cut hand-test load but costs an upfront item and ongoing upkeep; and our rules forbid touching the installed copy, so isolation must be built in. Our "never type into the installed copy" rule matches pstack's "isolate" step.

**Adds friction — skip or defer:**
- Full `poteto-mode` playbook machinery, multi-model panels, `arena`/`swarm`/`interrogate` on every item: more agents, more tokens, more gates; opposite of "lighter loop". `interrogate` only for a High-tier item if wanted.
- TLA+/formal verification, benchmark checklist: only for a specific risky change (e.g., a store race, perf work).
- `show-me-your-work` TSV log: duplicates .coord/STATUS.md and report files.
- Production-side testing types (canary, chaos, RUM, soak): not applicable.

Taxonomy gap note: for AntiNotepad the useful pre-production types are unit/selftest, static analysis/lint, smoke, regression, UI/UX; the rest of the chart is team-scale.

## 6. Addendum: user-supplied slides and quotes (2026-11-15)
Source: screenshots and quotes the user sent, from a talk by her. Treated as **[U]** for wording (not checked against a transcript), but they match verified pstack skills: `correct` (mistake, then architecture/types/lint/test, docs last) and `no-comments` (strips comments before review) **[V]**, see section 2.

**"The poteto pyramid" (inverted, top = cheapest, bottom = costliest):** codebase (good API design, architecture, refactoring) > static analysis (lints, types, compilers, tests, CI) > verification (formal, runtime) > prose (rules, skills, AGENTS.md) > human review. Rule: fix a recurring agent mistake at the highest layer that works; "rule to prevent new occurrences of anti patterns".

**"Don't automate a bad loop":** low trust (agent writes slop, human verifies; "most people get stuck here") > high trust (agent writes good code, agent verifies, reached after the pyramid) > "factory" (same loop plus an automated trigger; "trust enables scale").

**"Directory structure is a form of context":** her Dune app forbids imports between `electron-renderer/features` and `electron-main/*` (not even types; only `shared/` types cross); side project has `src/sim/tick.hot.ts` where the path itself means "runs every tick, no allocations". Layout and boundary rules tell the agent the constraints without prose.

**Quotes:** (a) when you correct an agent, improve the codebase first: start with a lint rule that blocks new occurrences and clean up old ones; keep verification strong. (b) Comments are liabilities: they give agents an escape hatch, go stale, and cannot be enforced.

### What this means for AntiNotepad (cheap ideas only)
| Idea | Where we stand | Cost |
|---|---|---|
| Correct via lint first, prose last | We already have a gate lint step; AGENTS.md and .claude/rules are heavy on prose (pyramid says prose is costly and weak) | Cheap: when a mistake repeats, add a lint rule or selftest instead of a rules paragraph. Only on repetition. |
| Directory layout as context | AGENTS.md already lists Model/, Editor/, UI/, Calc/, Theme/, Platform/, SelfTest/ | Cheap: add a lint/selftest that bans cross-layer references (e.g. Calc/ must not touch Avalonia/Win32; Theme/ not Platform/). A path-name hot-path marker for show/hide code (speed is our priority: e.g. no allocation in the summon path) is the same idea. |
| Comments as liabilities | Our coding rules may already limit comments; not checked here | Cheap: a lint for stale/explaining comments, optional. Low value; skip unless comments become a problem. |
| Don't automate a bad loop | Matches our Level 4/5 loop-automation (L prefix) work | Free guidance: raise automation only after the cheap layers (codebase, lint, selftests) are strong; do not add agent-verifies-agent gates before then. Argues for fewer prose steps and more mechanical checks, not more gates. |
| Human review is the costliest layer | Our hand test is that layer | Keep hand test for visible changes only; push everything else down the pyramid into lint/selftests. |

## 7. Addendum 2: runtime verification slide (user-supplied) [U wording]
User says all slides/quotes come from her presentation about her work on the Grok Bot team and its Electron app (this also supports the section 1 link between her and Grok Bot, still secondhand **[U]**).

**Slide "runtime verification":** feature map (what features exist > how users reach it > what proves it works) + a CLI that drives the real app (`app aria-click --accessible-name "Settings"`, `app snapshot`, `app screenshot proof.png`) = the agent checks its own work.
**Quotes:** a feature map is like a site map for agents (what exists, how users reach/experience it, what users can do, what proves it works); custom CLIs drive the real app like a real user.

Matches verified pstack pieces: `create-verification-skill` (feature map + Launch/Doctor/Drive/Evidence/Cleanup) and the `verification-skill-example` repo (driver CLI `control-atlas`, omitted) **[V]**, see sections 2-4.

### For AntiNotepad
- Our app is native Avalonia, not Electron, so no DevTools/ARIA-click route. Equivalent driver would be Windows UI Automation (Avalonia exposes automation peers) or a small debug command channel inside the dev build; either is a real build item, not free.
- **Cheap first step:** a one-page feature map file (feature, how reached, what proves it works) per visible area. Costs an hour, lets the hand test and Codex checks share one checklist, no new gate.
- **Defer:** the driver CLI. It only pays off if hand tests become the bottleneck. Constraint stays: it must only ever drive a dev instance with ANTINOTEPAD_DATA, never the installed copy.
- Her loop slide says to build this before automating verification by agents; so the order is feature map, then driver, then agent self-check.

## 8. Motion testing: frame-capture options (2026-11-15)
Our state **[V, local]**: F12 / Ctrl+F12 dev capture writes PNGs to ANTINOTEPAD_SHOTS but "captures miss the Acrylic backdrop" (docs/dev-loop.md:96). ffmpeg is not installed on this machine. pstack ships no frame-recording tool (section 4).

| Option | What it does | Fit | Source |
|---|---|---|---|
| A. In-app frame stepper (dev only) | On a trigger, render N frames to PNGs with Avalonia's own RenderTargetBitmap, advancing time per frame | Deterministic, no dependency; but misses Acrylic backdrop like F12 does; needs a way to step the animation clock (**not documented for Avalonia 12, unverified**) | https://wieslawsoltes.github.io/AvaloniaBook/Chapters/Chapter40.html (community guide) |
| B. Avalonia.Headless + Skia | Headless tests capture rendered frames (CaptureRenderedFrame, ForceRenderTimerTick), compare to baselines | Good for layout/state baselines in selftests; real renderer needed (UseHeadlessDrawing=false); no animation stepping API found | https://docs.avaloniaui.net/docs/concepts/headless/ (official), method names from community guide: unverified |
| C. HeadlessTestingFramework (NuGet) | Has HeadlessScreenRecorder: record frames during an action or a duration; lists Avalonia 12.1.1 | Ready-made, but third-party and headless (no real compositor/Acrylic) | https://www.nuget.org/packages/HeadlessTestingFramework |
| D. ffmpeg screen record + frame extract | Record the real window (gdigrab by title, or gfxcapture filter in newer builds), then `fps,scale,tile` to a contact sheet PNG | Captures the real app incl. translucency; external tool to install; gdigrab can show black for some GPU windows | https://trac.ffmpeg.org/wiki/Capture/Desktop ; tile recipe: https://techearl.com/ffmpeg-contact-sheet-preview (unverified, untested here) |

My read **[U, opinion]**: the real problem for our motion items is the live translucent window, which A-C cannot show. D is the only option that sees what the user sees; A/B/C suit exact state-per-frame assertions. Cheapest first experiment: D on a dev copy (never the installed one), output one contact-sheet PNG per animation that Claude can read; no product code change. Check first whether our ffmpeg build has gfxcapture (if gdigrab shows black).

## 9. Reply from Agentic Levels 4+5 chat (2026-11-15; answered from a summary, did not read this file)
- Overlap: Level 4 already moved prose rules into cheaper layers (hooks, ask rules, typed-only skills, agent files, lint, gate, selftest), matching her pyramid. One chat per item with a human hand test = her "low trust" tier. Level 5 (state tool, headless JSON verdicts, CI on dev, routines) = her "factory" tier, deliberately deferred.
- Missing: finish condition in every brief; "inconclusive" as a result; app verify recipe plus one-page feature map; layer-boundary lint (Platform/Model/UI split is only prose plus tools/check-platform-seam.ps1); decision log for unattended runs.
- Top 3: (1) finish condition + "inconclusive" in card and brief template (text only); (2) layer-boundary lint in the existing gate; (3) one-page feature map doubling as the hand-test checklist.
- Skip for now: swarm/arena, TLA+, UI-driver CLI, frame capture (hand test covers it), Slack-style triage bot. Rule: do not add a gate that costs more tokens than the mistakes it prevents.
- Note: its "skip frame capture" differs from my section 8 suggestion of a cheap trial; user decides.
