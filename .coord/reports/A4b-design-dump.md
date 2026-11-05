# A4b-design-dump report

## Summary

Implementation is incomplete. Investigation identified cleanup and menu construction paths that need a safe offline design before the dump can be trusted; no product files were changed.

## Change list

None.

## Behavior changes

None.

## Out-of-scope files

None.

## Plan deviations

No dump or selftest was implemented. `PadWindow` subscribes anonymous handlers to `Completion.OcrRequested` and platform color changes, and its normal close is canceled; its normal `Quit` path can show a window on save failure. `PadMenu.Show` opens a popup, while the non-opening builder is private. Process isolation or another safe construction and cleanup strategy still needs investigation; these observations do not prove a product change is necessary.

## Contracts

None.

## Lens review

No implementation diff to review. The lens checklist was considered against the investigated construction paths; no completed implementation exists to accept.

## Checks run

Read-only source inspection and `git status --short` (clean before this report). Build, selftest, gate, dump comparison and one-value-change proof were not run because no implementation was produced.

## Hand checks for the user

Once implemented, run `--selftest` in the dev build and confirm no window flashes up.
