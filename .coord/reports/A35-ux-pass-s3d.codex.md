# A35-ux-pass-s3d: contrast in both themes

## Summary

**Six high-severity findings identified.** The audit is incomplete: the table gives calculated examples, not the requested worst case across every palette, accent, tone, and control.

1. At the Clear tint, body text can fall below 4.5:1 over an opposite-color desktop.
2. Secondary text, hints, editor syntax colors, and links have low-contrast cases in both themes.
3. Several accent-filled buttons use white text that falls below 4.5:1.

## Ratios

Ratios are WCAG relative-luminance calculations, **truncated to two decimals**. Translucent examples composite the canvas over black or white in sRGB before calculating contrast. Background hex values are rounded for display; calculations used the unrounded composite.

Clear is 0.55 and the default is 0.70. The examples below are **not an exhaustive worst-case search**. Sources: [Palette.cs](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/Theme/Palette.cs>), [SettingsWindow.cs:376](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/SettingsWindow.cs:376>), and [PadWindow.cs:650](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/PadWindow.cs:650>).

| Theme | Pair | Foreground | Background (composited) | Ratio | Needs | Result |
|---|---|---|---|---:|---:|---|
| Light, Sage | Body; Clear over black | `#273027` | `#868783` | 3.76:1 | 4.5:1 | Fail |
| Dark, Graphite | Body; Clear over white | `#E3E3E3` | `#838485` | 2.92:1 | 4.5:1 | Fail |
| Light, Paper | Body; default over black | `#2B2A27` | `#B0AEA9` | 6.44:1 | 4.5:1 | Pass |
| Dark, Ink | Body; default over white | `#E8E4DA` | `#605F5E` | 4.99:1 | 4.5:1 | Pass |
| Light, Sage | Muted; Clear over black | `#7F8A7C` | `#868783` | 1.00:1 | 4.5:1 | Fail |
| Dark, Ink | Muted; Clear over white | `#8F897D` | `#828280` | 1.11:1 | 4.5:1 | Fail |
| Light, Paper | Editor hint, Faint; Clear over black | `#B8B2A6` | `#8A8885` | 1.67:1 | 4.5:1 | Fail |
| Dark, Ink | Editor hint, Faint; Clear over white | `#5E5A52` | `#828280` | 1.77:1 | 4.5:1 | Fail |
| Light, Paper | Disabled icon, 50% ink; Clear over black | composited ink | `#8A8885` | 1.98:1 | Exempt | Report only |
| Dark, Ink | Disabled icon, 50% ink; Clear over white | composited ink | `#828280` | 1.83:1 | Exempt | Report only |
| Light, Paper | Accent on canvas; Clear over black | `#D9822B` | `#8A8885` | 1.20:1 | 4.5:1 for text | Fail |
| Dark, Ink | Accent on canvas; Clear over white | `#F2B84B` | `#828280` | 2.15:1 | 4.5:1 for text | Fail |
| Both | Primary button, Amber | `#FFFFFF` | `#D9822B`, solid fill | 2.92:1 | 4.5:1 | Fail |
| Both | Primary button, Blue | `#FFFFFF` | `#0090FF`, solid fill | 3.26:1 | 4.5:1 | Fail |
| Both | Primary button, Violet | `#FFFFFF` | `#8E4EC6`, solid fill | 5.17:1 | 4.5:1 | Pass |
| Light, Paper | Error red; Clear over black | `#C23B32` | `#8A8885` | 1.50:1 | 3:1 for icon | Fail |
| Dark, Ink | Error red; Clear over white | `#F07167` | `#828280` | 1.33:1 | 3:1 for icon | Fail |
| Light, Paper | Link; Clear over black | `#2F6F9F` | `#8A8885` | 1.53:1 | 4.5:1 | Fail |
| Dark, Graphite | Link; Clear over white | `#61AFEF` | `#838485` | 1.58:1 | 4.5:1 | Fail |
| Light, Paper | Resting field border, Faint at 50% | composited Faint | `#FFFDF8`, solid field | 1.40:1 | 3:1 | Fail |
| Dark, Ink | Resting field border, Faint at 50% | composited Faint | `#262522`, solid field | 1.47:1 | 3:1 | Fail |
| Light, Paper | Focus border, Accent at 90% | composited Accent | `#FFFDF8`, solid field | 2.57:1 | 3:1 | Fail |
| Light, Paper | Selected vs filled pager dot | `#D9822B` | `#8A857A` | 1.25:1 | 3:1 comparison | Low contrast; see C4 finding |
| Dark, Ink | Selected vs filled pager dot | `#F2B84B` | `#8F897D` | 1.94:1 | 3:1 comparison | Low contrast; see C4 finding |
| Light, Paper | Toggle accent vs off outline | `#D9822B` | `#B8B2A6` | 1.38:1 | 3:1 comparison | Low contrast; adjacent-surface check outstanding |
| Dark, Ink | Toggle accent vs off outline | `#F2B84B` | `#5E5A52` | 3.83:1 | 3:1 comparison | Passes this comparison |
| Light, Paper | Toast text; Clear over black | `#FBF8F1` | ink at 90% over canvas | 11.83:1 | 4.5:1 | Pass |
| Dark, Ink | Toast action; Clear over white | `#F2B84B` | ink at 90% over canvas | 1.28:1 | 4.5:1 | Fail |
| Light, Paper | Save-failure detail | `#8A857A` | `#FFFDF8`, solid card | 3.61:1 | 4.5:1 | Fail |
| Dark, Ink | Save-failure detail | `#8F897D` | `#262522`, solid card | 4.41:1 | 4.5:1 | Fail |
| Light, Paper | Dev badge, Silver slot; Clear over black | ink/slot mix | slot at 11% over canvas | 1.85:1 | 4.5:1 | Fail |
| Dark, Ink | Dev badge, Orange slot; Clear over white | `#EE7A24` | slot at 15% over canvas | 1.33:1 | 4.5:1 | Fail |
| Light, Paper | Editor comment; Clear over black | `#9A958A` | `#8A8885` | 1.18:1 | 4.5:1 | Fail |
| Dark, Graphite | Editor number; Clear over white | `#D19A66` | `#838485` | 1.52:1 | 4.5:1 | Fail |

## Findings

| ID | Screen | Finding | Principles | Evidence | Severity | Fix idea |
|---|---|---|---|---|---|---|
| D-1 | Pad | Clear tint allows body text to fail over contrasting desktop content. | C1, C6 | [PadWindow.cs:650](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/PadWindow.cs:650>); ratios above | 🔴 High | Give text a sufficiently opaque backing or enforce contrast-safe tint and foreground combinations. |
| D-2 | Pad and settings | Muted text, hints, and save-failure details fail in calculated cases, including the solid save-failure card. | C1 | [PadView.cs:200](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/Editor/PadView.cs:200>), [PadWindow.Layout.cs:741](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/PadWindow.Layout.cs:741>) | 🔴 High | Use contrast-checked text roles for each actual backing. |
| D-3 | Settings | The primary-button foreground choice uses weighted encoded RGB at a threshold of 150, not WCAG relative luminance. Amber, Red, Pink, Blue, Teal, and Green select white and fail 4.5:1. | C1 | [SettingsParts.cs:398](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/SettingsParts.cs:398>) | 🔴 High | Calculate contrast against both candidate foregrounds and select one that passes; adjust the fill when neither does. |
| D-4 | Editor and pad | Links, syntax roles, error icons, toast actions, and dev badges have failing examples over translucent backdrops. | C1, C2, C6 | [Colorizers.cs:50](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/Editor/Colorizers.cs:50>), [PadWindow.Layout.cs:414](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/PadWindow.Layout.cs:414>), [PadWindow.Layout.cs:692](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/PadWindow.Layout.cs:692>) | 🔴 High | Contrast-check each role against its rendered backing; add an opaque backing where desktop content otherwise controls readability. |
| D-5 | Settings | Resting field borders fail 3:1 in both examples; the Paper focus border also fails against its field. | C2, C3 | [Kit.cs:249](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/Kit.cs:249>) | 🔴 High | Choose border colors and opacity against the field and neighboring surface. |
| D-6 | Pager and toggles | State colors have low contrast in calculated examples. Selected pager dots do also change width, so color is not their only distinction. Whether each state meets C2 against its actual adjacent surface needs further calculation. | C2, C4 | [PadBar.cs:229](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/PadBar.cs:229>), [SettingsParts.cs:289](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/SettingsParts.cs:289>) | 🔴 High | Give state indicators contrast against their actual neighbors and retain the shape or position distinction. |

## No issue

- **C4:** The selected pager dot is wider than an unselected dot, providing a shape distinction in addition to color. [ChromeTokens.cs:56](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/Theme/ChromeTokens.cs:56>)
- **C1:** The calculated Paper toast body and Paper/Ink body-text examples at the default tint pass in the particular backdrop cases listed above. This does not establish a pass for all palettes or backdrops.
- **C2:** Disabled controls are exempt from the non-text contrast requirement; their calculated values are reported without a failure finding.

## Not covered

- The requested **worst case for every pair** across all palettes, both tint values, both backdrops, accent overrides, and tone overrides remains unfinished. The table must not be treated as a complete pass/fail inventory.
- All six dev slot colors were calculated only for the Paper and Ink Clear examples; the table shows one failing example per theme. Rainbow markdown headings, fenced-code backgrounds, selection/focus rings outside the example field, the settings placeholder, disabled text, and toggle edges against their actual neighbors need separate calculations.
- High-contrast palettes were calculated, but their complete role-by-role results are not included above. System high contrast selects those palettes; it **does not by itself force solid material**. Solid material sets tint to 1, and the window supplies a canvas-colored transparency fallback. [PadWindow.cs:630](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/PadWindow.cs:630>)
- Actual desktop material, blur, and live rendering were not checked. The prescribed black/white compositing model is a source-based calculation, not a screen measurement.

## Checks run

Read the shared brief and source files; ran in-memory Python calculations for the eight base palettes at Clear and default tint over black and white, plus selected solid surfaces, accent buttons, badges, and controls. No files were created, and the app was not built or launched.

**READY: no** — the exhaustive pair-by-pair and customization calculations listed under Not covered remain.