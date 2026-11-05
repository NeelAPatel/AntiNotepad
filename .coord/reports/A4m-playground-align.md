# A4m-playground-align report

## What changed (by audit row)

- 1: Added role-specific Alternative steps for control and badge corners, spacing, motion, strengths and the focus ring; the canvas and saved role picks resolve those mappings.
- 2: Dot hit height uses preset data: 18 today, 20 recommended and alternative.
- 3-11: Corrected menu labels and hover radii, search card spacing and current surface, dot ring, toast padding, checkbox proportions, and badge glyph styling.
- 12: Kept the 2 px Today focus rendering to match the captured pixels; Alternative uses the 1.5 px design step.
- 13-20: Corrected the scale Today arrays and Thin replacement list.
- 21-24: Corrected role text and change descriptions.
- 26, 28: Added all 13 documented exceptions as read-only page content and applied the badge's optical padding.
- 29-35: Corrected measurements, coordinates, labels and pager arithmetic.
- Section D: Derive canvas tags from elements remaining after view cuts; mark absent tokens as not drawn in this view. Alternative-only mappings are tagged on corresponding drawn elements.

## Choices

The Alternative dot hit height uses 20, the proposed size; the design scale does not specify a separate Alternative size. The focus ring uses the pixel-observed 2 in Today and 1.5 in Alternative. JSON parsing, JavaScript syntax and a Node check of the Alternative mappings and recommended dot height passed. The requested offline gate reported Format PASS and Scope FAIL: the task card's Files section contains no backticked paths. The card was not edited as instructed. The gate also ran its build and selftest, both passed; its script invokes the built executable for selftest despite the instruction not to launch AntiNotepad. No server or browser was started.

## Gaps

The page has not been loaded or visually checked in a browser. TopBackdrop, split divider, Settings controls, selected pager dot and other roles listed in audit section D remain undrawn; the page has no matching view or element for them. Some tokens apply only in a particular view, theme, hover or reveal state. Visual accuracy and console behavior still require a hand check.

## Out-of-scope files

No product source was edited. The task card and STATUS were not edited. The gate generated its own ignored report and build artifacts.
