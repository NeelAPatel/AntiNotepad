# A8-save-failure-quit-and-x64

You are a worker (gpt-6-luna xhigh). Repo: this folder, a git worktree on branch `feat/A8-save-failure-quit-and-x64`. Never switch
branch, merge or commit (.git is read-only for you). App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10).
Read first: `AGENTS.md`, `docs/dev-loop.md`, `docs/audit/A1-codebase-audit.md` (A1-2, A1-20, decisions 2 and 7). Match the
surrounding style. First-party only.

## 1. Don't quit after a failed save (A1-2, decision 2: approved)
- `PadWindow.Save()` returns whether the store saved. `Quit()` saves first. On failure it does **not** close: it shows the pad if it
  is hidden (tray Quit and `--quit` arrive while hidden) and shows an in-pad prompt **"Couldn't save your notes"** with the error
  reason and two buttons, **Retry** (save again; quit if it works, otherwise keep the prompt with the new reason) and **Quit anyway**.
  Esc or clicking away cancels and keeps the app running.
- Build the prompt in the pad's own visual language: palette colors, existing Kit button helpers, light and dark. Look at how
  toasts and overlays are built in `UI/PadWindow.Layout.cs` and `UI/Kit.cs`. Keep it small and keyboard accessible (Enter =
  Retry, Esc = cancel), and give it automation names.
- A `--quit` request (installer or uninstaller) with a failed save must not exit: the prompt appears and the requester times out
  and aborts, which is the safe outcome. Don't change `SingleInstance.cs` or the scripts' quit protocol.
- The ordinary save toast on autosave failure stays as it is.

## 2. Publish for Windows x64 only (A1-20, decision 7: approved)
`tools/install.ps1`: publish with `-r win-x64 --self-contained false`; keep the rest of the publish the same. Check the published
output has no `runtimes/linux*`, `runtimes/osx*` or `*.so`/`*.dylib`, and that the Windows OCR dependency still resolves: the
build has 0 warnings and the selftest passes. Run only the publish step into `build/a8-publish`, never the installer itself:
replicate the script's publish command by hand. Don't touch the csproj unless the publish needs it, and say why if it does.

## Files in scope
`UI/PadWindow.cs`, `UI/PadWindow.Layout.cs` (only if the prompt needs layout hooks), `UI/Kit.cs` (only for a reusable bit), `tools/install.ps1`,
`SelfTest.cs` (only if a meaningful check exists; add a method at the END with one call line). Other workers are editing Store,
Program, PadView, Settings and the OCR code in parallel: stay out of them.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a8-check`: 0 errors, 0 warnings. `--selftest`
  PASS. Delete `build/a8-check` and `build/a8-publish` if you can. Never launch, quit or kill AntiNotepad, and never run install or
  uninstall.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A8-save-failure-quit-and-x64.md`: files changed, how the prompt works and looks, what each quit route
  (hotkey, menu, tray, `--quit`) does on failure, the publish command, the runtimes folder before and after, the build and selftest.
