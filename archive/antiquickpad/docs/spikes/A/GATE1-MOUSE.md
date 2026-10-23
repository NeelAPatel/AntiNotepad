> HISTORICAL: Original probe/tuning notes. Current wheel requirement is spec 4b page switching; touchpad is optional/untested. Use REPORT.md and RETEST.md for the current build.

# Gate 1 mouse input evidence

Source: user-operated spikes/A/logs/input-probe-20261017-013136.log (2026-10-17).

- 824 total log lines, 65 native WM_MOUSEHWHEEL messages.
- Native +120: 37, native -120: 28.
- Matching Avalonia horizontal wheel values: (-1,0): 37, (+1,0): 28; pointer Mouse.
- Vertical Avalonia wheel values: (0,-1): 6, (0,+1): 5.
- No fractional wheel values in this sample. No hi-res wheel/driver inertia or touchpad capability claim.
- Zero focused=True and zero human-phase marker records. The log does not prove keyboard editing. Native wheel delivery does not require editor keyboard focus.
- Hardware: notched horizontal mouse wheel per user; exact mouse model/driver unknown (request during interactive acceptance).

Result under revised spec 4a: PASS for discrete horizontal wheel delivery. Mouse drag/release, focus/typing and full carousel behavior need the new executable and interactive acceptance.

Implementation tuning starts at 120 logical pixels per notch and a 130 ms idle gap. Because observed inter-notch gaps sometimes exceed this threshold, tune against actual repeated-notch behavior during user acceptance; pauses may form separate wheel sequences. A normalized wheel unit is an application step, not physical motion or a hi-res input claim.
