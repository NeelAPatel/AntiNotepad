# A39-motion-out-of-ui: make Editor independent of UI, and guard it

Role: worker (gpt-6-luna xhigh). Branch `reorg/A39-motion-out-of-ui` (off dev). You cannot commit.
Product: `product/AntiNotepad`. Read `AGENTS.md` first. Context: the A38-mvvm-audit (on branch `feat/A38-mvvm-audit`,
`docs/A38-mvvm-audit.md`) chose targeted modularity refactors over MVVM. This is the first one.

## Why
The user's pain: changing one thing silently breaks another, found much later. One cause is that `Editor` and `UI` depend on
each other, so an edit in either can reach the other. This task makes the layering one-way **and adds a selftest that fails
the build gate if it ever goes two-way again.** The refactor is behavior-neutral: nothing visible may change.

## Target layering (one-way)
`UI -> Editor -> {Model, Calc, Theme, Platform, root}`. `Editor` must not reference any type in `AntiNotepad.UI`.

## Today's Editor -> UI references (all of them; verify with your own search)
1. `Anim` (`UI/Anim.cs`): `Editor/Motion.cs` (`Anim.Reduced`, `Anim.EaseOut`), `Editor/PadView.cs` (`Anim.Handle`, `Anim.Run`).
2. `ChromeTokens` (`UI/Chrome.cs`): `Editor/PadView.cs` (`PagerStripHeight`, `TextEdgeFadeBand`, `TopBackdrop.*`,
   `TopScrollableWhitespace`).
3. `MenuEntry` and `PadMenu` (`UI/PadMenu.cs`): `Editor/PadView.cs` builds the editor context menu (`PadMenu.Undo` etc. icon
   ids, `MenuEntry.Separator`, `PadMenu.Show(...)`) and exposes `Func<IEnumerable<MenuEntry>>? AppMenu` set by
   `UI/PadWindow.Layout.cs`.
4. `using AntiNotepad.UI` in `Editor/Elements.cs`: check whether it is used; remove it if not.

## Work
1. **Anim**: move to a neutral namespace both layers may use, e.g. `AntiNotepad.Motion` in a new folder `Motion/Anim.cs`
   (or the root namespace). Same code, same name. Update all callers (UI has about 20).
2. **ChromeTokens**: these are layout/design constants. Move them to `Theme` (e.g. `Theme/ChromeTokens.cs`, namespace
   `AntiNotepad.Theme`) unless something in them needs `UI`; if part does, split only that part. Values unchanged.
3. **Context menu**: invert the dependency. Preferred: move the plain data record `MenuEntry` and the icon-id constants out of
   `UI` (e.g. into `Editor` or a neutral namespace), and have `PadView` ask its host to show the menu through a delegate
   or event (e.g. `Action<Control, IReadOnlyList<MenuEntry>>? ShowMenu` set by `PadWindow`, which calls `PadMenu.Show`).
   `PadMenu` (the popup building) stays in `UI`. The menu must look and behave exactly as now, including the app entries
   from `AppMenu`, the action row, colored icons and the Settings it reads.
4. Keep comment density and naming like the surrounding code. No other cleanups, renames or reformatting.
5. **Layering selftest** in `SelfTest.cs`, printed as one line like the others, e.g. `LAYERS Editor->UI 0 refs`:
   - Reflect over the app assembly. For every type in `AntiNotepad.Editor` (including nested and compiler-generated types),
     collect referenced types from base types, interfaces, fields, properties, events, method/ctor signatures, generic
     arguments, **and method bodies** (scan IL with `MethodBody.GetILAsByteArray()` and resolve type/method/field tokens via
     `Module.Resolve*`; lambdas/closures live in nested types, so cover those).
   - Fail the selftest if any referenced type's namespace is `AntiNotepad.UI` (or starts with it). List the offenders.
   - Make the rule table-driven (a small list of forbidden `from -> to` pairs) so later reorg tasks can add rules, and include
     these that already hold today: `Model -> UI`, `Model -> Editor`, `Calc -> *any other AntiNotepad namespace*`,
     `Theme -> UI`, `Theme -> Editor`, `Platform -> UI`, `Platform -> Editor`. If one of those does not hold today, report
     it and leave that rule out rather than refactoring for it.
   - Prove the check works: temporarily add an Editor -> UI reference, confirm the selftest fails and names it, then
     remove it. Say in the report that you did.

## Rules
- Never launch or kill AntiNotepad (not even `--quit`). Build into `build/A39-check`, run `--selftest` there, delete the folder.
- `AVALONIA_TELEMETRY_OPTOUT=1` for builds. 0 warnings required.
- No new packages.

## Done means
- `grep -rn "AntiNotepad.UI" product/AntiNotepad/Editor` finds nothing, and the IL-based selftest confirms 0 Editor -> UI refs.
- Build 0 warnings, 0 errors; `SELFTEST PASS` with the new `LAYERS` line.
- No behavior change (moves and one delegate only).
- Report at `.coord/reports/A39-motion-out-of-ui.md`: files moved/changed, how the menu inversion works, the rules table, the
  proof that a violation fails, anything you left out and why.
