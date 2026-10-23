# M9-baseline (before any M9-function-contract-separation code)

Taken 2026-10-21 (night) on feat/function-contract-separation at de8d991 (dev 94c0f6f + F37-hidden-selftest-windows). The manager ran it on the interactive desktop (the user was away, the screen unlocked), using `.coord/scratch/gate.sh baseline measure`.

## Results
| Check | Result |
| --- | --- |
| `--quit` | exit 0 (resident pad saved and closed) |
| Release build | 0 warnings, 0 errors |
| `--selftest` | PASS: Quiet + Card + retained Classic; F37 hidden-window assertions pass (foreground unchanged, about 1,470 off-screen checks) |
| Selftest internal show-to-focus | Quiet 3.7-7.9 ms, Card 3.6-5.2 ms, Classic 2.3-3.5 ms (hidden window, internal only) |
| `--measure` (default shell = Quiet, scale 1, 30 samples, 2 runs) | see below, gaps = 0 both runs |

| `--measure` metric | Run 1 | Run 2 |
| --- | --- | --- |
| Show to visible-and-focused, median | 22.5 ms | 22.7 ms |
| Show to visible-and-focused, p95 | 24.2 ms | 24.9 ms |
| Heavy (20 notes x 200 lines), median / p95 | 21.9 / 24.2 ms | 23.0 / 24.8 ms |
| Hide, median | 13.0 ms | 12.3 ms |
| Idle CPU hidden / shown | 0.47% / 1.2% of one core | similar |
| Cold start to first frame | 1,225 ms | n/a |

## Reading
- Budget 100 ms: met with a wide margin.
- M5 reference (11 ms median, 15 ms p95, October). A control run on the retained Classic shell tonight (`ANTIQUICKPAD_SHELL=classic --measure`) gave 21.5 ms median / 23.0 ms p95, so the shell does **not** explain the roughly 2x gap since M5. It predates tonight's work: dev 94c0f6f plus F37 already measured 22.6 ms, and the gap is unexplained (machine or monitor setup, OS or runtime changes since M5 are candidates; not investigated). The branch is flat against its own baseline and far inside the 100 ms budget. Same-branch noise is small: about 0.2 ms between runs.
- **Gate for M9 part b**: median visible-and-focused at most 27 ms (22.6 + 20%), p95 at most 30 ms. A breach within 1 ms is rerun once before it counts as a failure. A measure run that fails with foreground or timeout errors and no number means the environment was unavailable, not a regression.

Raw JSON: `.coord/scratch/gate-baseline/measure{1,2}.json` (gitignored).
