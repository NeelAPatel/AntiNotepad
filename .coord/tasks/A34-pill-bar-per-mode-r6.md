# A34-pill-bar-per-mode-r6: hover colors, separators toggle, layout per bar style

Role: terra, a rework of one existing HTML lab (docs/agents.md). Edit only `tools/playground/a34-pill-bar-lab.html`. Read `.coord/tasks/A34-pill-bar-per-mode.md` and the current lab first. Keep everything that works (real palettes, tokens, behavior, Created slot, toasts, widths). Same hygiene as before: single file, offline, no top-level identifier named `top`, `node --check` clean.
User (2026-11-20), verbatim:
- "Hovering over delete button should color the icon with our pastel red."
- "Hovering over the new note icon should color it with the app's selected accent color."
- "add toggle for separators in One Pill modes"
- "Permanent mode: Docked / Overlay: E3 Corners, P1 Quiet, Optional P3 quiet (I would have to test it live to see if I prefer this quiet variant). Float: E3 one pill."
- "Standard view: Docked/Overlay: E3 Corners, S1 pill. Float: E3 one pill."
The user wrote "E3 Corners"; in the lab E1 is "corners" and E3 is "one pill". Treat it as E1 corners (Delete bottom-left, New note bottom-right, pager centered) and keep the label "Corners". Say in your final message that you assumed this.

## Changes
1. Hover colors, exactly as the app does it for icon buttons (see `Kit.IconButton` hover/press washes in `UI/Kit.cs`): on hover the Delete icon takes the palette's error/pastel red (`p.Error`, the same color `PadMenu.cs` uses for `MenuGlyphs.Delete`); on hover the New note icon takes the palette accent (`p.Accent`, same as the menu's `MenuGlyphs.NewNote`). Add an "Accent" row to the Settings panel with the 8 real accents from `Palette.Accents` plus "Palette default", applied live (accent also drives the Permanent current-dot and focus rings, as in the app via `Palette.Customize`). Keep the existing hover wash behind the icon.
2. Remove the "Bottom bar layout" row. The layout now follows the bar style: Docked and Overlay use Corners (Delete bottom-left, pager center, New note bottom-right); Float uses One pill (Delete, pager, New note all inside the single floating pill). Show the rule as a one-line caption under the Bar style row. Drop E4 and the old E1/E3 choice.
3. Add a "Separators in one pill" toggle (default on) shown only when the one-pill layout is active (Float), drawing 1 px separators (token `PagerSeparator*`) between Delete | pager | New note and, in Permanent, between the dots and the buttons.
4. Pager rows: Standard has only S1 pill (remove S3). Permanent has "P1 pill (quiet)" and an optional "P3 quiet" choice, defaulting to P1; keep P3 as it is now so the user can test it live. Make the choice visible in Settings as a single row "Permanent pager".
5. Float bar style must show the one-pill layout in a popup below the window, as the app does. Keep Created and the other rows unchanged.
6. Under the window show a short caption of what the current combination is (for example "Standard · Docked · Corners · S1 pill").

## Done means
Valid HTML; script parses; no external requests; console clean. Final message (6 lines max), including the E1/E3 assumption.
