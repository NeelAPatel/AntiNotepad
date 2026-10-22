T-011 started. Scope read. Lead owns design host, render pipeline, documentation and verification; Luna owns Design/ChevronMargin.cs. No git/network/STATUS edits. Checkpoint: implementation in progress.

Quiet checkpoint: 10 real RTB PNGs rendered; first visual inspection passed main/carousel. All scales/states to inspect after final refinement. ChevronMargin integrated; first build 0 errors, one naming warning to fix.

Card checkpoint: 10 real RTB PNGs rendered; visual inspection next.

Pro checkpoint: 10 real RTB PNGs rendered; visual inspection next. All three layouts implemented; probe/selftest pending.

Visual refinement: Card last-row clipping found; reducing body to 16 DIP. Pro font face changed to Cascadia Code for lighter body. Menu and empty screenshots reviewed. Probe mode results: None/None, Mica/Mica, Acrylic/AcrylicBlur, Blur/Transparent. Selftest running gracefully (includes four 30-second idle windows).

Final render checkpoint: Quiet/Card/Pro refinements complete; Card entire fixture fits, caret flush added before both scale captures, Pro uses explicit regular Consolas base. Final Release build 0 warnings/0 errors. First selftest exit 0; repeating on final binary. README and motion estimates written; per-PNG final inspection in progress.

QA blocker found and being repaired: direct 144-DPI RTB produced double-scaled nested card content and missing menu labels. Switching to 1.5 vector-scale Viewbox + 96-DPI RTB; all frames will be regenerated/inspected. Caret moved into task text (col 21), outside checkbox marker.

QA milestone: viewed all 30 final PNGs individually (five states x two scales x three directions), 30 correct dimensions/hashes in render-manifest.json. Viewbox vector capture resolved all double-scaling and missing menu contents. Final build clean. Final selftest still running; no further code changes planned.

Validation update: one final-binary selftest hit existing measurement child hide timeout; preserved selftest-timeout.log. Graceful isolated quit returned 0; process inventory confirmed no apphosts remained. Retrying unchanged final binary in isolation. Prior full tests passed; all renders/probes final and reviewed.

COMPLETE: final Release build 0 warnings/0 errors; --selftest exit 0; internal show-to-focus 2.667 / 1.605 / 1.089 ms. Integrated carousel + baseline/heavy measurement smoke passed. All 30 final PNGs individually inspected and correct dimensions; README/comparison/motion estimates/probe reports/manifest written. F25 None=None, Mica=Mica, Acrylic=AcrylicBlur, Blur=Transparent. .coord/reports/T-011.md finalized. User eyes: choose direction; native backdrop appearance, DPI, interaction/motion after integration. No git/network/STATUS edits; no killed processes.
