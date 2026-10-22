# M9d-conformance-check

Implemented the 24-command contract mirror and per-shell conformance smoke. Only the four authorized source/report files were written; no git commands, network calls, layout/palette/Core/ViewModel/Editor changes, or process kills. The manager retains STATUS and git ownership under this task's write-only scope.

## Files
- `src/AntiQuickPad/Contract/FunctionContract.cs`: plain C# immutable command list (ID, Required, exact Reachable by cell); upward contract-file discovery from AppContext.BaseDirectory; Commands-table parser; aggregate mismatch/route diagnostics; named known-gap list.
- `src/AntiQuickPad/Views/Common/ResidentShellWindow.Contract.cs`: single `IReadOnlyList<string> LiveRoutes(string id)` resolver containing every shell route lookup.
- `src/AntiQuickPad/Views/Common/ResidentShellWindow.Smoke.cs`: check inside the Quiet/Card loop and selftest-only missing-toolbar fault injection with finally restoration.
- `.coord/reports/M9d-conformance-check.md`: this report.

## Route rules
- Toolbar: label in `_layout.Buttons` and non-null Command. Menu: label in `_menuCommands`.
- Host keys: small explicit map audited against OnKeyDown; resident initialized. Editor Enter/Tab/Shift+Tab: explicit map audited against EditorFactory.ApplyKey and a writable editor. These handlers are installed unconditionally; input behavior is unchanged.
- Global hotkey: configured factory exists; InstallHotkey wires it to Toggle. Tray: configured factory exists and route is one of the three callbacks wired by SetupTray. Selftests skip creating either service; these checks establish capability, not registration or physical delivery.
- Wheel/grab/drag snap: carousel exists and its View is the active layout page. Card pager: Card shell, attached dot panel, command-bearing dot.
- Checkbox: writable editor with CheckboxRenderer, which EditorFactory installs alongside its pointer handler. Folding: ChevronMargin exists; it replaces the stock FoldingMargin while retaining the manager and pointer handler.
- Native move: actual visual descendant marked TitleBar. Close: initialized non-quitting resident/platform; constructor installs Close -> hide. Second launch/--quit: initialized resident/platform; Program.Attach routes singleton signals to it. Autosave: initialized editor and the wired 350 ms timer, which may be idle.
- Each command needs a live declared route. Required=yes additionally needs a toolbar/menu/hotkey route. Doc ID-set, Required and exact cell-string drift remain fatal even for known gaps. Failures are collected across IDs before throwing; missing doc names all unverified IDs and fails.

## Known gap
`window.move` is declared Required=yes but has only `gesture:title-bar drag region`. The native drag region exists in both shells. `KnownRequiredRouteGaps` waives only the additional toolbar/menu/hotkey rule for this command, prints KNOWN-GAP in each shell, and still requires its gesture to be live. No layout change was made. No other known gaps.

## Verification
- Ran the existing Release DLL with `--quit`; exit 0, no kill.
- Release build with AVALONIA_TELEMETRY_OPTOUT=1, `--no-restore -p:UseSharedCompilation=false`: 0 warnings, 0 errors.
- Negative proof: set `ANTIQUICKPAD_CONFORMANCE_PROBE=missing-minimize` for --selftest. The hook temporarily removes only the Minimize toolbar dictionary entry, calls the real check, and restores the entry in finally. Observed `CONFORMANCE quiet FAIL: window.minimize: no live declared route; window.minimize: yes requires a live toolbar/menu/hotkey route`, SELFTEST FAIL and process exit 1. Removed the environment override before the normal run. Neither doc nor layout was edited.
- Normal selftest with ANTIQUICKPAD_ALIGNMENT_OUTPUT resolved to repository `.coord/scratch/align`: exit 0; `CONFORMANCE quiet PASS (24 commands)` and `CONFORMANCE card PASS (24 commands)`, each preceded by `KNOWN-GAP window.move`. Full Quiet/Card and retained Classic capability suite PASS; hidden-window checks PASS with unchanged foreground.

These are scripted route/capability checks. Physical hotkey/tray/drag delivery and visible desktop behavior are not newly verified.

