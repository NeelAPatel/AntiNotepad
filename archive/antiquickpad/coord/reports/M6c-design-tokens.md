# M6c-design-tokens

Done 2026-10-21 by the Claude manager (user: Claude first today). UI task.

- **Step 1 (ca91368):** `Tokens/quiet.tokens.json` (DTCG, embedded) + `Tokens/DesignTokens.cs`. `ShellPalette` builds the Look from tokens (no more dependency on `Design/DesignSpec`). Quiet layout, layout context, menu, dots, bar reveal timings and the resident window size read tokens. All 28 Quiet renders byte-identical to the M8c baseline; selftest PASS.
- **Step 2:** F43 contrast on worst-case glass (light muted -> #5A645E, light accent -> #2E6A48, dark muted -> #A3B1A6, idle dots opacity 1); F42 (dot hit slot 8 x 24, buttons already 32 x 30); F48 (menu items and editor named); F51 (2 DIP accent ring, keyboard/programmatic focus only, also on menu items and dots). New selftest `TOKENS PASS` (86 tokens; worst text contrast 4.69:1; 22 buttons named, >= 24 DIP; ring keyboard-only). No proof expected values changed (proofs read colours from the Look). First accent pick (#456852) was too close to muted for the checkbox raster proof and for selected-vs-idle distinction; replaced with #2E6A48.
- **Gate:** Release 0 warnings; selftest exit 0 (BOUNDARY, CONFORMANCE quiet, REVEAL, PRESENTATION, TOKENS, alignment rasters). Renders `.coord/scratch/M6c/step2-{light,dark}/`.
- **Not done:** no live Acrylic look at the new colours yet (user glance); `--measure` not re-run.
