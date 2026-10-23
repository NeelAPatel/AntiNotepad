# Retest repair checkpoint

Scope: spikes/B and docs/spikes/B only, per latest user instruction; shared/.coord files are read-only this turn.

Confirmed shell root cause: runtime.tsv line 2 reports RegisterHotKey(Ctrl+Alt+Space), Windows error 1409 (already registered). Existing startup removed the tray after primary failure. Implementing fallback and native registration/dispatch self-check; selected chord will be visible in header and tray tooltip.

Rendering review: duplicate 50ms + per-frame spring ticks; redraw/text sync even for coalesced wheel events; all adjacent editor panes laid out while idle; synchronous detailed wheel log per event. Removing avoidable work, adding per-second observed root render/paint/interval counts. This is not GPU instrumentation or proof of dropped presentation frames.

Native shell worker owns src/shell/mod.rs. Director owns other B code and reports. No material improvements planned. Short retest will be written to RETEST.md.
