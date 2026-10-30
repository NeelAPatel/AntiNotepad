# T1-roadmap-overview-r2

## Summary

The graph data now includes short commit parents and lane parent/fork metadata. The Branches table draws one home-lane dot per commit, fork and merge curves, and the requested fork, merge, and squash labels; its legend explains the new marks. The data check and gate pass, but the specified `/data` HTTP probe could not bind port 47890 in this environment, and policy rejected removal of the verified throwaway clone.

## Change list

| File:line-range | Change | Plan line |
|---|---|---|
| `tools/roadmap-data.ps1:691-753` | Adds each lane's parent index and fork hash from the configured merge-base rules. | Data and parent/fork |
| `tools/roadmap-data.ps1:758-803` | Reads `%P` and emits the short `parents` array for each graph commit. | Data |
| `tools/roadmap/index.html:129-143` | Adds row-contained curve and relation-label styles. | Lane drawing and 30 px curves |
| `tools/roadmap/index.html:389-484` | Renders home dots, head rings, fork-to-head lane spans, fork/merge curves, and relation labels; detects the specified deleted-branch squash label. | Home lane, parent/fork, merges, squash merges |
| `tools/roadmap/index.html:617-631` | Adds fork curve, merge curve, `↳ from`, and `⤶` legend rows; retains the dashed-line row. | Legend |

## Behavior changes

- `graph.commits[]` has a short `parents` array; `graph.lanes[]` has `parent` (lane index or null) and `fork` (short hash or null). The sample file and top-level data keys are unchanged.
- Each commit dot is drawn only in the first lane listed in `in`; branch heads retain their own rings. A non-main lane is solid between its fork and head rows and dashed outside that span.
- Fork rows show `↳ from <parent>`; two-parent commits with a live source lane show `⤶ merged <branch>` and a curve. A qualifying one-parent dev commit shows `⤶ squash of <branch>` when its card's branch is absent locally.
- No product code, other tabs, health table, or server behavior changed.

## Out-of-scope files

None.

## Plan deviations

- `roadmap-serve.ps1 -Port 47890 -Root <clone>` could not start: `HttpListener.Start()` threw `HttpListenerException (6): The handle is invalid`; the server reported it could not bind. `/data` was therefore read from `roadmap-data.ps1 -Root <clone>` directly. Port 47811 already had the existing roadmap server (PID 19012), which was left running. `-Stop` reported no server on 47890.
- The probe clone and its temporary bundle remain at `build/T1-r2-probe` and `build/T1-r2-probe.bundle`. After verifying the clone path was inside this worktree's `build/`, the command runner rejected both recursive clone removal and single-file bundle removal as `blocked by policy`. A `git clean` dry run would have removed all of `build/`, so it was not run.

## Contracts

None (tooling and page rendering only).

## Lens review

Worker self-review followed the lens-review checklist for scope, changed meanings, shared state, ordering, missing inputs, cleanup, failure paths, and layers. No proven code findings; all hunks serve the round plan. The HTTP bind and cleanup limitations are recorded above.

VERDICT: accept code diff; round probe cleanup and HTTP serving remain incomplete.

## Checks run

- `pwsh -NoProfile -File tools/roadmap-data.ps1 -Check`: `ROADMAP DATA OK` (rows 70, groups 8, log 169, lanes 6, worktrees 4, tags 34, cards 37, warnings 0).
- `pwsh -NoProfile -File tools/gate.ps1 -Task T1-roadmap-overview -Offline`: `GATE: PASS`; build 0 warnings/errors, `SELFTEST PASS`, scope PASS, loop-lint PASS.
- `git diff --check`: passed.
- Direct clone data check: all six lane `parent`/`fork` values matched `git merge-base` and the first lane containing the fork. `main` was null/null; `dev` was parent 0/fork `8d18f72`; `tmp-x`, `probe-r2-source`, `probe-r2-target`, and `chore/T1-roadmap-overview` were parent 1/fork `01bb862`. `tmp-x` head equaled its fork.
- Parent probe: source commits `0877861` and `ca19141` had parents `[01bb862]` and `[0877861]`; target commit `1638c95` had parent `[01bb862]`; no-ff merge `4b4ebe1` had parents `[1638c95, ca19141]`.
- The 28 lane rows returned by the clone data builder are listed below. `dot`, `rings`, `solid`, `dashed`, `item`, `tags`, and `links` match the Branches table rendering logic.

```text
4b4ebe1 | Merge branch 'probe-r2-source' into probe-r2-target | dot=probe-r2-target | rings=probe-r2-target | solid=probe-r2-target | dashed=main,dev,probe-r2-source,chore/T1-roadmap-overview,tmp-x | item=- | tags=- | links=merge probe-r2-source->probe-r2-target; merged probe-r2-source
1638c95 | probe-r2-target: target commit | dot=probe-r2-target | rings=- | solid=probe-r2-target | dashed=main,dev,probe-r2-source,chore/T1-roadmap-overview,tmp-x | item=probe-r2-target | tags=- | links=-
ca19141 | probe-r2-source: second commit | dot=probe-r2-source | rings=probe-r2-source | solid=probe-r2-source,probe-r2-target | dashed=main,dev,chore/T1-roadmap-overview,tmp-x | item=- | tags=- | links=-
0877861 | probe-r2-source: first commit | dot=probe-r2-source | rings=- | solid=probe-r2-source,probe-r2-target | dashed=main,dev,chore/T1-roadmap-overview,tmp-x | item=probe-r2-source | tags=- | links=-
c796e1e | T1-roadmap-overview: hand test round 1 and r2 brief | dot=chore/T1-roadmap-overview | rings=chore/T1-roadmap-overview | solid=probe-r2-source,probe-r2-target,chore/T1-roadmap-overview | dashed=main,dev,tmp-x | item=- | tags=- | links=-
0f68d91 | T1-roadmap-overview: card Check | dot=chore/T1-roadmap-overview | rings=- | solid=probe-r2-source,probe-r2-target,chore/T1-roadmap-overview | dashed=main,dev,tmp-x | item=T1-roadmap-overview | tags=- | links=-
1c489c7 | T1-roadmap-overview: T1c switch the roadmap to the live server | dot=chore/T1-roadmap-overview | rings=- | solid=probe-r2-source,probe-r2-target,chore/T1-roadmap-overview | dashed=main,dev,tmp-x | item=T1-roadmap-overview | tags=- | links=-
670da81 | T1-roadmap-overview: T1c step brief | dot=chore/T1-roadmap-overview | rings=- | solid=probe-r2-source,probe-r2-target,chore/T1-roadmap-overview | dashed=main,dev,tmp-x | item=T1-roadmap-overview | tags=- | links=-
f453e9e | T1-roadmap-overview: T1b live overview page | dot=chore/T1-roadmap-overview | rings=- | solid=probe-r2-source,probe-r2-target,chore/T1-roadmap-overview | dashed=main,dev,tmp-x | item=T1-roadmap-overview | tags=- | links=-
5f1b707 | T1-roadmap-overview: T1b step brief | dot=chore/T1-roadmap-overview | rings=- | solid=probe-r2-source,probe-r2-target,chore/T1-roadmap-overview | dashed=main,dev,tmp-x | item=T1-roadmap-overview | tags=- | links=-
d9e2acb | T1-roadmap-overview: T1a roadmap data builder and local server | dot=chore/T1-roadmap-overview | rings=- | solid=probe-r2-source,probe-r2-target,chore/T1-roadmap-overview | dashed=main,dev,tmp-x | item=T1-roadmap-overview | tags=- | links=-
5a1008d | T1-roadmap-overview: T1a step brief | dot=chore/T1-roadmap-overview | rings=- | solid=probe-r2-source,probe-r2-target,chore/T1-roadmap-overview | dashed=main,dev,tmp-x | item=T1-roadmap-overview | tags=- | links=-
a1a8fea | T1-roadmap-overview: approved mockup and data sample | dot=chore/T1-roadmap-overview | rings=- | solid=probe-r2-source,probe-r2-target,chore/T1-roadmap-overview | dashed=main,dev,tmp-x | item=T1-roadmap-overview | tags=- | links=-
c6c6461 | T1-roadmap-overview: card | dot=chore/T1-roadmap-overview | rings=- | solid=probe-r2-source,probe-r2-target,chore/T1-roadmap-overview | dashed=main,dev,tmp-x | item=T1-roadmap-overview | tags=- | links=-
01bb862 | Loop2-loop-v3-1-wiki: wiki rewritten for loop v3.1 with sources | dot=dev | rings=dev,tmp-x | solid=dev,probe-r2-source,probe-r2-target,chore/T1-roadmap-overview,tmp-x | dashed=main | item=- | tags=- | links=fork dev->probe-r2-source; from dev; fork dev->probe-r2-target; from dev; fork dev->chore/T1-roadmap-overview; from dev; fork dev->tmp-x; from dev; squash docs/Loop2-loop-v3-1-wiki
12e9e1c | Loop2-loop-v3-1: close, card Check, STATUS and lessons | dot=dev | rings=- | solid=dev | dashed=main,probe-r2-source,probe-r2-target,chore/T1-roadmap-overview,tmp-x | item=Loop2-loop-v3-1 | tags=- | links=squash chore/Loop2-loop-v3-1
a50c538 | Loop2-loop-v3-1: 3.1i rehearsal | dot=dev | rings=- | solid=dev | dashed=main,probe-r2-source,probe-r2-target,chore/T1-roadmap-overview,tmp-x | item=Loop2-loop-v3-1 | tags=- | links=squash chore/Loop2-loop-v3-1
68b4d9b | Loop2-loop-v3-1: 3.1h rules text says loop v3.1 | dot=dev | rings=- | solid=dev | dashed=main,probe-r2-source,probe-r2-target,chore/T1-roadmap-overview,tmp-x | item=Loop2-loop-v3-1 | tags=- | links=squash chore/Loop2-loop-v3-1
f9240eb | Loop2-loop-v3-1: 3.1g loop-lint | dot=dev | rings=- | solid=dev | dashed=main,probe-r2-source,probe-r2-target,chore/T1-roadmap-overview,tmp-x | item=Loop2-loop-v3-1 | tags=- | links=squash chore/Loop2-loop-v3-1
479afd3 | Loop2-loop-v3-1: 3.1f strict scope and read-only audits | dot=dev | rings=- | solid=dev | dashed=main,probe-r2-source,probe-r2-target,chore/T1-roadmap-overview,tmp-x | item=Loop2-loop-v3-1 | tags=- | links=squash chore/Loop2-loop-v3-1
464abc8 | Loop2-loop-v3-1: 3.1e scan-backlog and scan-delta | dot=dev | rings=- | solid=dev | dashed=main,probe-r2-source,probe-r2-target,chore/T1-roadmap-overview,tmp-x | item=Loop2-loop-v3-1 | tags=- | links=squash chore/Loop2-loop-v3-1
b000381 | Loop2-loop-v3-1: 3.1d conflict route and hand-test turns | dot=dev | rings=- | solid=dev | dashed=main,probe-r2-source,probe-r2-target,chore/T1-roadmap-overview,tmp-x | item=Loop2-loop-v3-1 | tags=- | links=squash chore/Loop2-loop-v3-1
093c4e8 | Loop2-loop-v3-1: re-plan round 2 (trim, scan-backlog, diagram) | dot=dev | rings=- | solid=dev | dashed=main,probe-r2-source,probe-r2-target,chore/T1-roadmap-overview,tmp-x | item=Loop2-loop-v3-1 | tags=- | links=squash chore/Loop2-loop-v3-1
665aa6c | Loop2-loop-v3-1: 3.1c catch-up points | dot=dev | rings=- | solid=dev | dashed=main,probe-r2-source,probe-r2-target,chore/T1-roadmap-overview,tmp-x | item=Loop2-loop-v3-1 | tags=- | links=squash chore/Loop2-loop-v3-1
0909d14 | Loop2-loop-v3-1: 3.1b approve and merge preflights | dot=dev | rings=- | solid=dev | dashed=main,probe-r2-source,probe-r2-target,chore/T1-roadmap-overview,tmp-x | item=Loop2-loop-v3-1 | tags=- | links=squash chore/Loop2-loop-v3-1
ac450d2 | Loop2-loop-v3-1: 3.1a fix-loop-scripts | dot=dev | rings=- | solid=dev | dashed=main,probe-r2-source,probe-r2-target,chore/T1-roadmap-overview,tmp-x | item=Loop2-loop-v3-1 | tags=- | links=squash chore/Loop2-loop-v3-1
c644877 | Loop2-loop-v3-1: card | dot=dev | rings=- | solid=dev | dashed=main,probe-r2-source,probe-r2-target,chore/T1-roadmap-overview,tmp-x | item=Loop2-loop-v3-1 | tags=- | links=squash chore/Loop2-loop-v3-1
8d18f72 | Loop1-process-v3: loop v3 rules, coding standard, glossary, editorconfig | dot=main | rings=main | solid=main,dev | dashed=probe-r2-source,probe-r2-target,chore/T1-roadmap-overview,tmp-x | item=- | tags=- | links=fork main->dev; from main; squash chore/Loop1-process-v3
```

## Hand checks for the user

- The loop chat should check the Branches tab in a browser in both themes and at 375 px, including no page-level horizontal scroll.
- Hand test round 2 should confirm `tmp-x` shows a head ring on its own lane and `↳ from dev`, a two-commit branch shows its fork curve and `↳ from dev`, and the no-ff merge shows a source curve and `⤶ merged probe-r2-source`.
- Confirm a deleted local branch produces the `⤶ squash of <branch>` label on a qualifying one-parent dev commit.

