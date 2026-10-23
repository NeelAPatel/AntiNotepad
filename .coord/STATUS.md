# STATUS (update at every dispatch / milestone / completion)

Updated: 2026-10-23
Product: Antipad (`product/Antipad`). Latest tag `antipad-v8`; the user's copy (`build/antipad`) runs that build.

## Now
- R1-antipad-replaces-antiquickpad: old project moved to `archive/antiquickpad/`; AGENTS.md, docs/dev-loop.md, docs/agents.md,
  docs/decisions.md written for Antipad. On `claude/product-spike`. Next, on the user's say-so: fast-forward `dev` to it, move
  `main` to a release tag, then new work on `feat/<ID-slug>` branches off dev.

## Open
- Settings window can still stay in memory after closing in manual use (log shows `freed=False` twice after
  C3-trace-idle-and-preview-leak); cause not found yet.
- Private memory stays ~200 MB after Settings closes (working set drops to 10-40 MB); not traced.
- Optional: pick final defaults among the bar styles and scroll/swipe modes, and drop the ones not wanted.

## Branches
- `claude/product-spike`: Antipad (to become dev). `claude/icon-lab`: Icon Lab spike, kept on purpose.
- `dev`, `main`, `feat/*`: the retired AntiQuickPad line (`feat/ui-auditor` and `feat/ui-overhaul` never merged into dev).
