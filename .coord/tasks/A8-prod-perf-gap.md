# A8-prod-perf-gap: why prod felt choppier and buggier than dev

Tier: Low · findings only (no code change proposed yet)
Branch: none

## Request
2026-11-06 (user via Master 4): installed v0.7.0-rc2 is "so choppy and poor" vs dev; scrolling and background broken. Reframed: "straight up buggier than dev".

## Findings
- **Code identity:** Release publish from 84784f4 (install.ps1 flags) into `build/A8-publish`; SHA-256 of all 36 app/dependency dll, exe and json files equals the installed copy. The code is not the cause.
- **Settings:** prod `index.json` Settings differed from `build/testdata` in about a dozen keys: Material solid (test acrylic), Scrolling lines (rate), ReduceMotion true (false), Chrome fade, Bars docked, OpenAt remember, Taskbar never.
- **Scrolling:** user's own Reduce motion setting (user confirmed, 2026-11-06). Not a bug.
- **Background:** with settings matched 1:1 prod stayed solid. Cause: `Program.cs:108` picks software rendering at launch when Material is solid; `SettingsWindow.cs:385-390` shows "Restart AntiNotepad to get the glass back". Prod had been launched while Solid was saved. After the user restarted prod, the glass worked (user, 2026-11-06). Release build with test settings also showed glass.
- **Perf:** prod hotkey-to-focus 14 ms (log 2026-11-06), fine.

## Verdict
Not a code deviation, not Debug vs Release. Cause: settings state plus render engine fixed at launch. Remaining candidate (design, user's call): switching Solid to Acrylic/Mica needs a restart; the hint text is easy to miss.
