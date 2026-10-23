T-007 COMPLETE — 2026-10-18
1. Strike: TextDecoration removed; Selection-layer overlay. PNG scale 1 = 2 px strike; mean glyph ink plain 0.7869 / strike 0.7953, delta 0.0085 <= 0.10. Zero changed pixels outside predicted strike bands versus renderer-off control. Regular/Bold/Italic assertions PASS.
2. Markers: Look.MarkdownMarker #737F8E; contrast reduction 14.84% versus #7E8B9B.
3. Empty-item indent/outdent: 7 exact text/caret/single-undo cases PASS; numbered parent continuation and task markers preserved.
4. Standard chrome, close hides, wrapping top bar/menu, persistent bottom status/count, Pin/Topmost and normal bounds persistence implemented. Close/pin (including maximized)/settings smoke PASS. Existing drag strip, singleton, persistence, carousel and focus smoke PASS.
Final Release build: 0 warnings, 0 errors, exit 0; no fallback output needed. Selftest exit 0. Internal focus samples 1.939/1.558/1.492 ms. Physical hotkey/tray/native chrome/multi-DPI still need user review.
Report: .coord/reports/T-007.md
Proof: .coord/reports/T-007-strike.png
No git, network, shared STATUS edits, or process termination.
