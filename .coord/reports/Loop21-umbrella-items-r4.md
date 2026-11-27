# Loop21-umbrella-items r4

## 1 Summary
Added a temporary Git fixture that exercises umbrella roles, title hints, brief checks, roadmap nesting and child merge preflight. Reduced docs/loop.md to 7,981 bytes by moving detail to docs/loop-sessions.md.

## 2 Change list
- Added tools/umbrella-mock.ps1, including an optional fixture-preserving -Serve command.
- Moved umbrella rules, status-card detail and the file map into docs/loop-sessions.md with pointers in docs/loop.md.

## 3 Behavior changes
The mock creates and tests a disposable repository; it starts no chats, serves no port and performs no real merges or pushes. -Serve preserves the fixture and prints a command for the user to run.

## 4 Out-of-scope files
No product code, item card or STATUS edits. The offline gate wrote its normal gate report.

## 5 Plan deviations
The fixture copies start-card.ps1 and merge-preflight.ps1 unchanged into its own tools directory because those scripts derive the repository from their own location. The mock itself is the test, so no separate -SelfTest applies. A separate -Serve verification fixture was left in the system temp directory because cleanup was blocked by policy.

## 8 Checks run
- `pwsh -NoProfile -File tools/umbrella-mock.ps1`: exit 0; output below.
- `pwsh -NoProfile -File tools/umbrella-mock.ps1 -Serve -Port 47891`: 12/12 passed; printed the requested command and preserved its fixture.
- `pwsh -NoProfile -File tools/roadmap-data.ps1 -Check`: pass, 188 rows, 0 warnings.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop21-umbrella-items -Offline`: pass; LoopLint warned about existing lessons and item-card budgets.

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

## 9 Hand checks
No chats were started and no server was bound. The -Serve command was printed but not run; visual roadmap inspection remains for the user.
