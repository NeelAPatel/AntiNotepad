# Prior pass: unverified leads only

From a first look at saved PNG renders in `docs/design/m6/built` (Quiet and Card, light and dark). The user has since said real-UI inspection is paused. **Do not cite these as findings.** Re-observe with a method from [observation-methods.md](observation-methods.md) before reporting.

- Small text (footer, eyebrow, empty-state hints, menu section labels) looks faint and about 9-11 px; contrast unmeasured (only headings were measured, 5.6-9.4:1).
- Accent colour used inconsistently on toolbar and status text.
- Footer baselines look misaligned; scrollbar sits on the code-panel edge; a stray `···` mark above the editor.
- Card shell: neighbour peek shows a clipped "Ne"; a stray highlight behind the second card's H1 mid-slide.
- Delete placement differs between shells; pager dots and arrows may be under 24 px.
- No keyboard focus ring seen in any render; icon-only controls may lack names and tooltips.
- Custom title row in captures vs docs saying standard decorations: possible double title bar.
