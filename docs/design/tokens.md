# Design tokens (M6c-design-tokens)

One token set drives the Quiet shell: `src/AntiQuickPad/Tokens/quiet.tokens.json` (DTCG format, embedded in the build), loaded once by `Tokens/DesignTokens.cs`. `ShellPalette` builds the editor `Look` from it; the Quiet layout, menu, pager dots, bar reveal timings and window size read it directly. Nothing in the shell hard-codes colours, sizes or timings any more.

- **Groups:** `color.light` / `color.dark` (canvas, ink, muted, accent, rule, surface, markdown/syntax/heading colours, translucent code panel and active line), `material` (acrylic, tint 0.9), `font`, `space`, `size`, `radius`, `elevation`, `motion` (reveal 120/150/400 ms), `editor` (checkbox, strike).
- **Change a value:** edit the JSON, rebuild, run `--selftest`. No code change needed for value tweaks.
- **Guards in `--selftest` (`M6c-design-tokens TOKENS PASS`):** every token has `$type` and `$value`; ink, muted and accent text >= 4.5:1 and idle dots/focus ring >= 3:1 on the worst-case glass (90 % tint over black or white, both themes); every shell button has an accessible name and a >= 24 DIP target (pager dots: 24 DIP tall, compact width, Previous/Next are the equivalent targets); the focus ring shows on keyboard focus and not on pointer focus.
- **Accessibility values (2026-10-21):** light muted #758079 -> #5A645E (3.1 -> 4.7:1 worst case), light accent #537864 -> #2E6A48 (3.8 -> 4.9:1, clearly greener than muted), dark muted #9CAA9F -> #A3B1A6, idle dot opacity 0.5 -> 1, dot slot 5x18 -> 8x24, focus ring 1 -> 2 DIP, keyboard-only.
- **Exempt (decorative, by design):** markdown markers (F8, deliberately dim), indent guides.
