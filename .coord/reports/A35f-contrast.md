# A35f-contrast: build

## 1. Summary
Implemented the approved contrast roles and a palette/accent sweep.

## 2. Change list
Added WCAG luminance, ratio, compositing, text selection and contrast adjustment; updated fields, buttons, toast, failure detail, pager and toggles. Updated design documentation and snapshot.

## 3. Behavior changes
Primary text picks white or near-black; field focus falls back to Ink; toast action uses underlined Canvas text; state marks and failure detail adjust against their modeled backing. Pager width and fill cues remain.

## 4. Out-of-scope files
The gate generated its standard report. No other product files outside the brief were changed.

## 5. Plan deviations
Toggle backing uses the 90% Surface card composited over Canvas. Transparent desktop effects remain unmeasured; D-1/D-4 behavior is unchanged.

## 6. Contracts
The sweep covers eight palettes, eight accent overrides and each default. Minimum ratios: primary 4.58; field rest 3.34, focus 3.02; toast 9.21; failure detail 4.52; toggle off 3.01, on 3.00; pager empty/filled/selected on pill 3.01/3.50/3.00 and Canvas 3.00/3.28/3.02. No modeled pair failed its minimum. Values are rounded to two decimals.

## 7. Lens review
Found the translucent Settings card backing; corrected toggle colors and sweep to use its Canvas composite. Follow-up review accepted; no remaining proven findings.

## 8. Checks run
Build to build/A35f-contrast-check passed with zero warnings; selftest passed. Offline gate passed; see its report. Design snapshot re-pinned after deterministic changes.

## 9. Hand checks for the user
In the dev copy, check primary buttons, field rest/focus, toast action, save failure detail, toggle states and pager states in light and dark palettes. Desktop material and Clear tint need separate live measurement.
