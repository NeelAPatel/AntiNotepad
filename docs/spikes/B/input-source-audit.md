# Restored input-source audit (T-003 Gate 1)

Exact package: gpui-pre-windows 0.3.9, zed-rev 93d745668805029476e6d7fbb19652116fed51de (restored Cargo.toml metadata).
Original direct_manipulation.rs SHA256: DA83877B3E6C2D3F7D391EB151CC4CC56C70DBC95B261F5BDA2E3E861D8ACA1D
Original package/cache remains unchanged. Probe vendors the Windows backend under spikes/B/vendor/gpui-pre-windows; only direct_manipulation.rs is modified.

Original file references (1-based):
- Lines 41-48: interaction, X/Y translation, inertia, rails, scaling enabled.
- Lines 97-105: SetContact only when GetPointerType returns PT_TOUCHPAD.
- Lines 166-193: end_gesture emits zero-pixel TouchPhase::Ended and resets gesture kind.
- Lines 217-220: RUNNING following INERTIA ends the old sequence.
- Lines 222-249: READY ends gesture, resets viewport transform.
- Lines 283-332: transform divided by DPI scale, incremental pixel deltas, Started then Moved.
- No RUNNING->INERTIA public release event; no Cancelled emission in this handler.

Capture audit: gpui-pre 0.3.9 window.rs 5432-5444 registers mouse listeners during paint; 5944-5964 dispatches Capture before Bubble. The probe installs a non-consuming Capture listener through a canvas painted before the editor. Its focus flag is read from the actual editor focus handle, not inferred from active-window status.

ONE bounded native diagnostic adapter: append timestamped raw status transitions from the existing DM callback to the same trace as public events. Label RUNNING->INERTIA RELEASE_CANDIDATE; RUNNING->READY NO_INERTIA_END_CANDIDATE; READY FINISHED_OR_RESET; INERTIA->RUNNING INERTIA_INTERRUPTED. Preserve original public phases, native inertia and pixel calculations. No new OS recognizer, dependency, timeout-based release or contact synthesis.

These candidates are diagnostic evidence, not yet a production release contract. READY may reflect cancellation/reset; zero-motion lift may supply no recognized scroll; RUNNING->INERTIA timing vs actual finger lift must be physically checked. Timestamp append I/O is probe overhead, so these logs must not be used for product latency benchmarks.

## Revised wheel/mouse gate (2026-10-17)
Spec 4a / final decision supersedes touchpad release as a mandatory gate. Touchpad source findings above are retained as optional/untested evidence.

Restored events.rs routes WM_MOUSEWHEEL at line 148 and WM_MOUSEHWHEEL at 149. Vertical wheel conversion at original lines 568–588 divides raw delta by +120 then scales by Windows scroll lines (or chars with Shift). Horizontal at 612–626 divides by -120, scales by scroll chars, and emits Lines/Moved. Neither wheel path has real contact/release phases.

The SAME bounded diagnostic vendor seam now also logs exact raw wheel messages in events.rs, before conversion. Two files intentionally differ from the original: direct_manipulation.rs and events.rs. All other vendored files remain unchanged. Public logging adds read-only Windows wheel scaling queries for normalized notch steps and capture-phase mouse press/held-button move/release, including editor_focus. Native raw notch_delta is raw_delta/120; public horizontal direction is inverted by the original backend. Idle release threshold must be chosen after trace analysis; no timer or carousel implemented here.
