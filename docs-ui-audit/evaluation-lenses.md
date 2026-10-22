# Evaluation lenses

## 1. Heuristic evaluation (Nielsen, 10 heuristics)
- Rules of thumb, not strict guidelines. Cheap, fast, works on specs, prototypes and finished software.
- Best with 3-5 independent evaluators and a severity scale. A single-evaluator pass is a stated limitation.
- Headings include visibility of system status, consistency and standards, aesthetic and minimalist design, help and documentation.

## 2. Cognitive walkthrough
Per task step ask:
1. Will the user see the action is available?
2. Will they recognise it as the one they need?
3. After acting, will they understand the feedback?
This is the closest method to "feeling" the product.

## 3. WCAG 2.2 (AA unless noted)
- Text contrast 4.5:1 (large text 3:1); non-text UI and focus indicators 3:1.
- Target size minimum 24x24 px (44 px is a stricter house rule for touch).
- 2.4.7 visible focus; 2.4.11 focus not obscured (AA); 2.4.13 focus appearance is **AAA**, not AA.
- 2.5.7 dragging movements need a single-pointer alternative.
- Reduced motion: best practice only (2.3.3 is AAA). Pause/stop for moving content is the AA-level check.
- Automated tools catch only part; add manual keyboard and assistive-tech testing.
- Web criteria; apply to a desktop app by intent, not by letter.

## 4. Windows 11 / Fluent 2
- Principles: functional, natural, consistent, appealing. Larger elements get more motion time; reduced-motion variants expected.
- Mica for app windows, Acrylic for transient surfaces. Official token values were not confirmed; check Microsoft Learn.
- Judge native fit: title bar, snap behaviour, focus, backdrop over light and dark wallpapers, DPI changes.

## 5. Motion (Apple HIG as the feel target)
- Interruptible, retargetable motion; springs carry velocity; feedback on every input; crossfade under Reduce Motion.

## Severity scale
| Level | Meaning |
|---|---|
| S0 | Cosmetic, nice to fix. |
| S1 | Minor friction, workaround obvious. |
| S2 | Real friction or exclusion for some users. |
| S3 | Blocks a task, loses data, or breaks the core promise (speed, summon, type). |
