# Claude Design and skills

## Claude Design (claude.ai/design)
- Chat-to-canvas tool, research preview since April 2026 (third-party reports; Anthropic's pages are authoritative).
- Makes prototypes, pages, decks. Refine by chat, comments and direct edits. Can build a design system from a codebase or design files and apply it to later projects.
- Exports to PDF, PPTX, Canva or standalone HTML, or hands off to Claude Code.
- Plans: Pro, Max, Team, Enterprise (beta); off by default on Enterprise.
- Fit for this project: output is web-shaped. Useful for tokens, mood and layout ideas; it cannot validate Avalonia rendering.
- The `DesignSync` tool is only for the user-started `/design-sync` skill, which writes to the user's claude.ai account. Not used so far; do not use without the user's request.

## Third-party audit skills seen in search (not installed, not run)
- Prism accessibility (kyaulabs), accessibility-audit (cuellarfr), a11y-audit (plugin87).
- Apple-design summary skill (emilkowalski), already cited by `docs/design/research-2026-10-18.md`.
- All are web-oriented. Only the checklist logic carries over to Avalonia.
- Session skills `modern-web-guidance` and `artifact-design` are web-only.
- Rule from AGENTS.md: other libraries only on a concrete need, maintained and documented.
