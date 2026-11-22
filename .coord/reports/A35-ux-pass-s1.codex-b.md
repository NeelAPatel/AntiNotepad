# A35-ux-pass-s1b

## Summary

**Partial audit only.** The checks identified the cases below, but I did not finish an exhaustive review of all requested files or every theme declaration. Counts by kind and class, and the top five files by off-scale count, would therefore be misleading.

The clearest token discrepancy is `PagerDotButtonHeight`: code changed from 20 to 32 in A34, after the scale was applied. The scale document appears stale for that token.

## Missed by scanner

These are **confirmed examples, not a complete inventory**. “Layout” denotes a control or drawing size rather than a scale step; it does not imply that the value needs changing.

| File:line | Kind | Literal | Class | Nearest step |
| --- | --- | --- | --- | --- |
| [MotionPreview.cs:271](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/MotionPreview.cs:271) | Stroke | 1.8 | off-scale | 2 |
| [MotionPreview.cs:272](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/MotionPreview.cs:272) | Stroke | 1.8 | off-scale | 2 |
| [SettingsParts.cs:609](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/SettingsParts.cs:609) | Stroke | 1.1 | off-scale | 1 |
| [SettingsParts.cs:678](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/SettingsParts.cs:678) | Stroke | 1.4 | off-scale | 1 |
| [Structure.cs:460](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/Editor/Structure.cs:460) | Stroke | 1 | on-scale | Thin 1 |
| [Structure.cs:463](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/Editor/Structure.cs:463) | Stroke | 1 | on-scale | Thin 1 |
| [Renderers.cs:404](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/Editor/Renderers.cs:404) | Stroke | 1.6 | exception 4 | — |
| [Renderers.cs:405](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/Editor/Renderers.cs:405) | Stroke | 1.9 | exception 4 | — |
| [Renderers.cs:399](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/Editor/Renderers.cs:399) | Checkbox size | font × .86 | exception 4 | — |
| [Renderers.cs:421](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/Editor/Renderers.cs:421) | Checkbox radius | size × .28 | exception 4 | — |
| [Renderers.cs:424](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/Editor/Renderers.cs:424) | Checkbox radius | size × .28 | exception 4 | — |
| [Renderers.cs:254](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/Editor/Renderers.cs:254) | Chip font | font × .93 | exception 5 | — |
| [Renderers.cs:101](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/Editor/Renderers.cs:101) | Corner radius | 9 | off-scale | 8 |
| [Renderers.cs:297](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/Editor/Renderers.cs:297) | Corner radius | 4 | on-scale | Sm 4 |
| [Kit.cs:273](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/Kit.cs:273) | Opacity setter | .5 | on-scale | Mid .5 |
| [SettingsParts.cs:381](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/SettingsParts.cs:381) | Helper radius | 4 | on-scale | Sm 4 |
| [SettingsWindow.cs:187](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/SettingsWindow.cs:187) | Helper size | 32 | layout | — |
| [SettingsWindow.cs:239](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/SettingsWindow.cs:239) | Helper height | 36 | layout | — |
| [SearchOverlay.cs:83](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/SearchOverlay.cs:83) | Helper size | 32 | layout | — |
| [PadWindow.Layout.cs:753](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/PadWindow.Layout.cs:753) | Helper dimensions | 88 × 32 | layout | — |
| [PadWindow.Layout.cs:757](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/docs/A35-ux-pass/product/AntiNotepad/UI/PadWindow.Layout.cs:757) | Helper dimensions | 104 × 32 | layout | — |

## Tokens vs scale

The table is **not yet exhaustive**. “Not in doc” means no named mapping was found in sections 3.1–3.2; it does not by itself mean a token is incorrect.

| Token | Today | R | Stale side | Note |
| --- | --- | --- | --- | --- |
| `ChromeTokens.PagerDotButtonHeight` | 32 | 20 | Doc appears stale | A34 changed it from 20 to `ButtonSize`; the proposal predates that change. |
| `ChromeTokens.PinRingThickness` | 4 | not in doc | Doc predates token | Added by A9 pinned accent ring. |
| `ChromeTokens.DevBadgeLightTextInkMix` | .62 | not in doc | Doc predates token | Added with T17 slot identity. |
| `ChromeTokens.PagerCountButtonHeight` | 32 | not in doc | Doc predates token | Added in A34. |
| `ChromeTokens.BarCornerSize` | 42 | not in doc | Doc predates token | `PillHeight`; added in A34. |
| `ChromeTokens.DeleteDebounceMs` | 500 ms | not in doc | Neither established | Behavioral timing, rather than a visual scale value. |
| `ChromeTokens.PlainIconWeight` | 0 | not in doc | Doc predates token | Added in A34. |
| `Paint.DisabledIcon` | .5 | not in doc | Doc predates token | Added in A34; numerically matches Mid. |
| `Typography.UiFamily` | Segoe UI Variable Text, Segoe UI | not in doc | Neither established | Font-family selection is outside the mapped size ramp. |
| `Typography.MonoFamily` | Cascadia Mono, Consolas | not in doc | Neither established | Font-family selection. |
| `Typography.EditorFallback` | Cascadia Mono, Consolas, monospace fallback | not in doc | Neither established | Editor fallback list. |

## Checks run

Read the item card, scale proposal, scanner patterns, theme declarations and targeted source matches. Inspected Git history for the pager height and several later tokens. No build, app launch, file write or network access was performed.

**READY: no —** the remaining literal review, complete theme-token comparison, counts and top-five calculation are missing.