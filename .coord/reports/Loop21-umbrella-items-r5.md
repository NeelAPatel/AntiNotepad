# Loop21-umbrella-items r5

## Changes

- `tools/survival.py` now supplies explicit `--src-prefix=a/ --dst-prefix=b/` to `git diff`, while retaining `-c core.quotepath=false`. Its selftest enables `diff.noprefix=true` before removing a line and confirms the loss is detected.
- `.claude/skills/merge-task/SKILL.md` now states beside autosquash that umbrella branches use `--rebase-merges` and `catch-up.ps1 -RebaseMerges`. The non-umbrella command is unchanged.

## Checks

- `python -I tools/survival.py --selftest`: `SURVIVAL SELFTEST PASS`.
- `pwsh -NoProfile -File tools/umbrella-mock.ps1`: PASS, 12/12.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop21-umbrella-items`: FAIL at Build. `dotnet` exited 0 with 4 NU1900 warnings because NuGet vulnerability data could not be reached at `https://api.nuget.org/v3/index.json`; all other gate checks passed, including the product selftest. No source build errors.
- `git diff --check`: PASS.

## Mock output (verbatim)

```text
PASS start item: ok
PASS start child: ok
PASS start director: ok
PASS title missing row: ok
PASS title child: ok
PASS title differs: ok
PASS brief missing row: ok
PASS brief plain row: ok
PASS brief indented row: ok
PASS roadmap snapshot: ok
PASS preflight behind: ok
PASS preflight rebased: ok
UMBRELLA MOCK: PASS (12/12)
```
