# M9d-conformance-check (M9-function-contract-separation, part d)

Director: Codex gpt-6.1-sol medium. Do it yourself (small).
Branch: feat/function-contract-separation (the manager handles ALL git; you run no git commands that write).
Naming: always `<ID>-<slug>`, never a bare ID. Finish in ONE turn.

## Read
AGENTS.md, docs/function-contract.md (the contract; read-only for you), docs/architecture.md, src/AntiQuickPad/Views/Common/*.cs, Views/Quiet, Views/Card, ViewModels/MainViewModel.cs, Shell/AvaloniaTrayService.cs, Carousel/CarouselController.cs (routes only).

## Write only
- New `src/AntiQuickPad/Contract/FunctionContract.cs`: the command ID list mirrored once in code (ID, Required = yes|shared, declared routes as strings exactly like the doc's "Reachable by" cell). Plain C#, no Avalonia types.
- New `src/AntiQuickPad/Views/Common/ResidentShellWindow.Contract.cs`: ONE resolver `IReadOnlyList<string> LiveRoutes(string id)` that reports which declared routes are live in the active shell. Part b will re-point this one function to the new controller, so keep all route lookup in it.
- `src/AntiQuickPad/Views/Common/ResidentShellWindow.Smoke.cs`: call the check.
- `.coord/reports/M9d-conformance-check.md`.
No layout, palette, Core, ViewModel or Editor changes. No docs/. No git, no network, never kill processes.

## The check
1. Parse the Commands table of `docs/function-contract.md` (rows whose first cell is a back-ticked ID; locate the file by walking up from `AppContext.BaseDirectory` until `docs/function-contract.md` exists; missing file = FAIL). Assert the doc's ID set, Required values and route strings equal `FunctionContract`'s. Mismatch = FAIL naming the IDs.
2. For each ID in the active shell, `LiveRoutes(id)` must be non-empty. How each route kind counts as live:
   - `toolbar:<label>`: `_layout.Buttons[label]` exists and has a non-null Command.
   - `menu:<label>`: `_menuCommands[label]` exists.
   - `hotkey:<key>`: the resident host handles that key (Esc, Ctrl+N, Ctrl+Shift+Delete in OnKeyDown; Enter/Tab/Shift+Tab in the editor's key handler; Ctrl+Alt+Shift+A through the hotkey service factory). Prefer a small declared key map the host already uses over reflection. If you must add one, it must not change key behaviour.
   - `gesture:*`, `tray:*`, `system:*`: live if the host or service that provides them exists (carousel/fold margin/checkbox handler present; `ShellServices.CreateTray` non-null; native title-bar drag region present in the layout; Closing handler hides). Keep these cheap and honest. Do not fake them as always true without a reason in a code comment.
   - `yes` IDs additionally need at least one live `toolbar:` or `menu:` route OR a live hotkey route.
3. Print `CONFORMANCE <shell> PASS (<n> commands)` or throw with every failing ID. Run it inside the existing per-shell selftest loop, so it runs for both Quiet and Card.
4. If current code genuinely fails the contract, do NOT change layouts to make it pass. Report the finding, mark that ID with a clearly named known-gap list in `FunctionContract` that the check reports as `KNOWN-GAP`, and continue.

## Build and test
From src/AntiQuickPad: `dotnet bin/Release/net10.0/AntiQuickPad.dll --quit` (never kill), then with `AVALONIA_TELEMETRY_OPTOUT=1`: `dotnet build -c Release --no-restore -p:UseSharedCompilation=false`; `--selftest` with env `ANTIQUICKPAD_ALIGNMENT_OUTPUT` = `.coord/scratch/align`. Prove the check can fail: temporarily rename one toolbar label or drop one doc ID in a scratch copy (or a test-only parameter), see it FAIL, then restore. Say how you proved it in the report.

## Done when
0 warnings; `--selftest` exit 0 with `CONFORMANCE quiet PASS` and `CONFORMANCE card PASS`; report lists files, route rules, proof of failure, any known gaps.
