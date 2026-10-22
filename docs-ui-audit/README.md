# UI audit knowledge base

Purpose: how to see AntiQuickPad the way a user sees and feels it, and how to report what is found. Written 2026-10-18.

## Scope and rules
- Known about the product: Windows 11 desktop app, Avalonia + AvaloniaEdit, summoned by a global hotkey, translucent scratchpad. Nothing else is assumed here.
- No inspection of the real UI happens until the user says go. Method first, observation later.
- One writer for this folder (the UI auditor session).
- Every finding is tagged **observed**, **inferred** or **needs-your-eye** (see [finding-template.md](finding-template.md)).
- Pixels show what is drawn, not how it feels. Motion, weight and latency need video or the user's own judgement.

## Files
| File | Read when |
|---|---|
| [perception-model.md](perception-model.md) | Starting any audit: what a user perceives, in order. |
| [evaluation-lenses.md](evaluation-lenses.md) | Choosing what to judge against (heuristics, WCAG 2.2, Fluent, severity). |
| [observation-methods.md](observation-methods.md) | Choosing how to look at a running Avalonia app. |
| [claude-design-and-skills.md](claude-design-and-skills.md) | Deciding whether Claude Design or a third-party skill helps. |
| [finding-template.md](finding-template.md) | Writing up a finding. |
| [prior-pass-observations.md](prior-pass-observations.md) | Only as unverified leads; do not treat as fact. |
| [sources.md](sources.md) | Checking where a claim came from. |
