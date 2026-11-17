# A24-save-delay-cleanup implementation report

## Changed
- Settings and the dropdown share the token-backed delay choices; labels retain invariant formatting.
- Store sets a per-note `BodyWriteInFlight` flag when a body is queued and clears it at the matching-revision acknowledgement; the static hook and revision set are removed. The flag lives on the note, so a deleted note leaves no store state (it keeps its own flag, unused).
- Save callbacks carry the note only; logging uses the current setting. Added save callback and pane transition comments.
- Added a queued-body deletion check to the existing storage selftest (checks the body file is gone).

## Tests run
- Baseline: offline local-cache restore, Debug build and selftest passed before changes (0 warnings, `SELFTEST PASS`).
- Changed build (`dotnet build product/AntiNotepad -c Debug -o build/A24-save-delay-cleanup-check --no-restore`):
  ```
  Build succeeded.
      0 Warning(s)
      0 Error(s)
  ```
- Changed `--selftest`: exit 0, including the deletion check; output tail:
  ```
  CONTRACTS: 30 rows, 29 pass, 1 manual
  SELFTEST PASS
  ```
- `tools/gate.ps1 -Offline -Task A24-save-delay-cleanup`: exit 0; output tail:
  ```
  [PASS] Tokens: check-design-tokens.ps1 exit 0
  [PASS] Format: 8 changed C# file(s), no format problems on changed lines; 0 older one(s) ignored
  [PASS] Scope: 8 changed file(s); all are allowed by the brief
  [PASS] Size: no changed product C# file crossed 600 lines or grew by more than 25 while already over it
  GATE: PASS
  ```
  The gate reports its expected Offline warning (NuGet audit disabled); build reports zero warnings. Tokens reports an existing baseline can be lowered.

## Line counts
Nonblank lines, PowerShell `Measure-Object -Line`.

| File | Before | After |
|---|---:|---:|
| Model/Settings.cs | 78 | 82 |
| Model/Note.cs | 143 | 143 |
| Model/Store.cs | 560 | 552 |
| UI/PadWindow.cs | 668 | 668 |
| UI/PadWindow.Navigation.cs | 408 | 409 |
| UI/SettingsWindow.cs | 686 | 683 |
| SelfTest/UI.cs | 693 | 693 |
| SelfTest/Store.cs | 567 | 579 |

## Risks
- A note removed from the store keeps its flag set; only a window still holding that note could observe it. Concurrent save behavior remains review-sensitive.
- Codex review and design review remain for the item chat; the quick dev-copy hand test remains pending. No dev or installed copy was launched.
