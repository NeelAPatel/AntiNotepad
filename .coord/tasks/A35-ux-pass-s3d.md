# A35-ux-pass-s3d: contrast in both themes (letter D)

Read `.coord/tasks/A35-ux-pass-s3-common.md` first and follow it (skip its controls checklist; use the tables below).

Compute WCAG contrast ratios from the palette values, not screenshots: `Theme/Palette.cs` (every palette: light, dark,
high contrast, accents), `Theme/Paint.cs` (alphas), `Theme/ChromeTokens.cs`, `Model/Settings.cs` (tint presets and the
default). You may write and run a small script in memory (PowerShell or Python one-liners printed to stdout) but create
no files. Formula: WCAG 2.x relative luminance; never round a ratio up.

**Translucent window rule (C1):** the pad and its bars sit over the desktop. For each text or icon role, composite the
surface over a white backdrop and over a black backdrop at the **lowest tint preset** and at the default, then compute
the ratio against each. Report the worst case. Where the app has a solid fallback (high contrast, transparency off), say so.

Pairs to check, each in dark and light: body text on surface; muted/secondary text; placeholder or hint text;
disabled text and icons (exempt from WCAG, report only); accent text and accent-filled button text (`SettingsParts.cs:398`
picks dark or white by luminance; check its threshold); error red on surface (Delete hover); link color; selected and
focus rings and field borders against their neighbor (C2, 3:1); pager dots (selected vs not) (C2, C4); toggle track
on vs off (C2); toast text; save-failure card text; dev badge text on its slot colors (`DevSlotTokens`, `DevBadgeLightTextInkMix`);
markdown and code colors in the editor (`Editor/Colorizers.cs` palette roles) at 4.5:1.

Report format: replace the controls checklist with
`## Ratios (table: theme | pair | foreground | background (composited) | ratio | needs | result)`, then Findings,
No issue, Not covered, Checks run, READY. Letter D.
