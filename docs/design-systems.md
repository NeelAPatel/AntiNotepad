# Design systems study: how tokens are layered, sized and named

Notes only; nothing here is applied and **no values are chosen** (that is the scale proposal, the next step). Read
2026-11-03 from each system's own pages or source files, with the link beside each claim. Where a page does not say,
this doc says **not published**; nothing is filled in from memory. Counts of steps are counted from the linked list.

Systems read: Fluent 2 / Fluent UI React v9 (Microsoft), Material 3 (Google), Apple HIG, IBM Carbon, GitHub Primer.
Our own numbers against Fluent are in `docs/design-fluent.md`; our 15 control kinds are in
`.coord/reports/A4g-control-groups-map.md`.

## 1. Tiers

| System | Tiers (low to high) | Example chain | Source |
| --- | --- | --- | --- |
| Fluent 2 | **Global** (raw values) > **alias** (meaning). The design page names two layers and does not mention control tokens | `colorBrandBackground` (alias) > `brand[80]` (global) > `#0078d4` | [Design tokens](https://fluent2.microsoft.design/design-tokens); [lightColor.ts](https://raw.githubusercontent.com/microsoft/fluentui/master/packages/tokens/src/alias/lightColor.ts) |
| Material 3 | **Reference** (concrete values) > **system** (roles) > **component** (per element) | `--md-filled-button-container-shape` > `md-sys-shape corner-full` > `9999px` | [Theming](https://raw.githubusercontent.com/material-components/material-web/main/docs/theming/README.md); [filled button tokens](https://raw.githubusercontent.com/material-components/material-web/main/tokens/versions/v0_192/_md-comp-filled-button.scss); [shape](https://raw.githubusercontent.com/material-components/material-web/main/tokens/versions/v0_192/_md-sys-shape.scss) |
| Carbon | Scale tokens (`$spacing-01` to `$spacing-13`) used directly. A separate component tier is not stated on the pages read | `$spacing-05` is 16 px | [Spacing](https://carbondesignsystem.com/elements/spacing/overview/) |
| Primer | **Base** (raw sizes) > **functional** (purpose) > component-scoped (control and overlay sizes) | `--stack-gap-normal` maps to `{base.size.16}` | [Size](https://primer.style/foundations/primitives/size) |
| Apple HIG | **No token tiers published.** It publishes named text styles per platform and a few layout numbers | Body is 17 pt on iOS, 13 pt on macOS | [Typography](https://developer.apple.com/design/human-interface-guidelines/typography) (data: [JSON](https://developer.apple.com/tutorials/data/design/human-interface-guidelines/typography.json)) |

- **Pattern:** every token system has a raw layer, a meaning layer, and (Material, Primer) a per-control layer. A control
  token points up to a role; a role points up to a raw step. Nobody types a number at the control.
- **Fluent's control layer:** the Fluent 2 page read describes two layers only, so a control tier there is **not
  published** on that page.
- **Fluent file note:** alias files are generated ("DO NOT EDIT", [source](https://raw.githubusercontent.com/microsoft/fluentui/master/packages/tokens/src/alias/lightColor.ts)); the tokens are a pipeline output, not hand-edited.

## 2. How scales are chosen (step counts)

Counts are of the linked list. "Incl." marks none/circular end stops.

| Category | System | Steps | What the scale is | Source |
| --- | --- | --- | --- | --- |
| Spacing | Fluent React v9 | 11 (incl. none): 0, 2, 4, 6, 8, 10, 12, 16, 20, 24, 32 | Fixed list on a 4 px base; 6 and 10 are "Nudge" half-steps | [spacings.ts](https://raw.githubusercontent.com/microsoft/fluentui/master/packages/tokens/src/global/spacings.ts) |
| Spacing | Carbon | 13: 2, 4, 8, 12, 16, 24, 32, 40, 48, 64, 80, 96, 160 | Fixed list; "multiples of two, four, and eight" | [Spacing](https://carbondesignsystem.com/elements/spacing/overview/) |
| Spacing | Primer | 33 base steps (14 negative, 14 small, 5 large); the functional tier picks from them | Large raw palette, small named subset | [Size](https://primer.style/foundations/primitives/size) |
| Radius | Fluent React v9 | 11: none, 2, 4, 6, 8, 12, 16, 24, 32, 40, circular | Fixed list | [borderRadius.ts](https://raw.githubusercontent.com/microsoft/fluentui/master/packages/tokens/src/global/borderRadius.ts) |
| Radius | Fluent 2 design | 6: none, 2, 4, 8, 12, circle | Shorter design-side list | [Shapes](https://fluent2.microsoft.design/shapes) |
| Radius | Windows 11 | 3 levels: 0, 4, 8 | Two global resources plus none | [Geometry](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/geometry) |
| Radius | Material 3 | 7 steps: none 0, extra-small 4, small 8, medium 12, large 16, extra-large 28, full | Fixed list; extra start/end/top variants (for example `corner-large-top`) | [_md-sys-shape.scss](https://raw.githubusercontent.com/material-components/material-web/main/tokens/versions/v0_192/_md-sys-shape.scss) |
| Type | Material 3 | 15 roles: 5 roles (display, headline, title, body, label) x 3 sizes (large, medium, small) | Role x size grid | [typescale](https://raw.githubusercontent.com/material-components/material-web/main/tokens/versions/v0_192/_md-sys-typescale.scss); [typography](https://raw.githubusercontent.com/material-components/material-web/main/docs/theming/typography.md) |
| Type | Fluent 2 web | 16 styles, 10 to 68 px; weight variants are separate styles (Strong, Stronger) | Fixed list, size and line height paired | [Typography](https://fluent2.microsoft.design/typography) |
| Type | Carbon | 17 sizes, 12 to 92 px | Formula, not a ratio: `Xn = Xn-1 + {INT[(n-2)/4] + 1} * 2` from 12 | [Typography](https://carbondesignsystem.com/elements/typography/overview/) |
| Type | Apple iOS | 11 text styles, 11 to 34 pt (macOS 11 styles, 10 to 26 pt) | Fixed named styles with set leading | [Typography JSON](https://developer.apple.com/tutorials/data/design/human-interface-guidelines/typography.json) |
| Type | Primer | 11 text styles from 6 primitive sizes (xs to 2xl), 4 weights, 5 line heights | Small primitive scales, composed into styles | [Typography](https://primer.style/foundations/primitives/typography) |
| Motion | Fluent React v9 | 8 durations: 50, 100, 150, 200, 250, 300, 400, 500 ms | Fixed list | [durations.ts](https://raw.githubusercontent.com/microsoft/fluentui/master/packages/tokens/src/global/durations.ts) |
| Motion | Windows 11 | 3 durations: 83, 167, 250 ms | Three named buckets | [Timing and easing](https://learn.microsoft.com/en-us/windows/apps/design/motion/timing-and-easing) |
| Motion | Material 3 | 16 durations (50 to 1000 ms, short/medium/long/extra-long x 4) and 10 easings | Grouped ladder | [_md-sys-motion.scss](https://raw.githubusercontent.com/material-components/material-web/main/tokens/versions/v0_192/_md-sys-motion.scss) |

- **Spacing and radius** are fixed lists on a small base unit (2 or 4 px). No system derives them from a ratio.
- **Type** is a fixed list (Fluent, Apple, Material), a formula (Carbon), or a small primitive set composed into styles
  (Primer). A ratio scale is not used by any system read.
- **Motion** ranges from 3 buckets (Windows) to 16 (Material). The smallest published set is Windows, at 3.
- **Elevation, stroke, alpha:** Windows names 6 elevation levels (1, 2, 8, 16, 32, 128) and Fluent 2 four stroke widths
  (1 to 4), per `docs/design-fluent.md`. Alpha/state steps for Apple, Carbon and Primer were **not read** here.
- **Apple spacing:** only tvOS grid numbers and a visionOS 60 pt control spacing are published; iOS and macOS margins
  are **not published** as numbers ([Layout JSON](https://developer.apple.com/tutorials/data/design/human-interface-guidelines/layout.json)).
- **Not obtainable by fetch:** the Material 3 and Apple HIG web pages returned no body text; values above come from
  Material's open-source token files and Apple's page data. Carbon heading token names are **not published** on the page read.

## 3. Exceptions

| System | How a one-off is allowed | Source |
| --- | --- | --- |
| Material 3 | A component token can be "a system token or a concrete value". Hardcoded values stay in the component file, for example `container-height` 40 px and icon size 18 px in the filled button | [Theming](https://raw.githubusercontent.com/material-components/material-web/main/docs/theming/README.md); [filled button tokens](https://raw.githubusercontent.com/material-components/material-web/main/tokens/versions/v0_192/_md-comp-filled-button.scss) |
| Fluent | Some alias tokens use literals instead of global steps, for example `colorSubtleBackground: 'transparent'` and `colorNeutralShadowAmbient: 'rgba(0,0,0,0.12)'` | [lightColor.ts](https://raw.githubusercontent.com/microsoft/fluentui/master/packages/tokens/src/alias/lightColor.ts) |
| Fluent 2 | Rule-based exception: "for shapes smaller than 32 pixels, the corner angle is reduced to 2 pixels" | [Shapes](https://fluent2.microsoft.design/shapes) |
| Primer | Control and overlay sizes are their own component-scoped tokens (`--control-small-paddingInline-normal`, `--overlay-width-large`) rather than raw steps | [Size](https://primer.style/foundations/primitives/size) |
| Carbon, Apple | A documented override mechanism is **not published** on the pages read | [Carbon spacing](https://carbondesignsystem.com/elements/spacing/overview/) |

- **What the exceptions share:** they sit in the control layer, they are named and visible in one file, and the scale
  underneath is not edited. A one-off never changes a scale step.

## 4. Naming

| System | Pattern | Example | Source |
| --- | --- | --- | --- |
| Fluent | `<category><Property><SizeWord>`; t-shirt sizes with "Nudge" half-steps; 2XL and up written as numbers | `spacingHorizontalM`, `borderRadiusMedium`, `borderRadius2XLarge`, `durationNormal` | [spacings.ts](https://raw.githubusercontent.com/microsoft/fluentui/master/packages/tokens/src/global/spacings.ts), [borderRadius.ts](https://raw.githubusercontent.com/microsoft/fluentui/master/packages/tokens/src/global/borderRadius.ts), [durations.ts](https://raw.githubusercontent.com/microsoft/fluentui/master/packages/tokens/src/global/durations.ts) |
| Fluent color alias | Role word plus a number | `colorNeutralForeground1` | [lightColor.ts](https://raw.githubusercontent.com/microsoft/fluentui/master/packages/tokens/src/alias/lightColor.ts) |
| Material | `--md-sys-<group>-<name>`; type is `--md-sys-typescale-<scale>-<size>-<property>`; component tokens have no tier prefix | `--md-sys-shape-corner-medium`, `--md-sys-typescale-body-medium-size` | [Theming](https://raw.githubusercontent.com/material-components/material-web/main/docs/theming/README.md), [typography](https://raw.githubusercontent.com/material-components/material-web/main/docs/theming/typography.md) |
| Material motion | Group word plus number inside the group | `duration-short1`, `duration-extra-long4` | [_md-sys-motion.scss](https://raw.githubusercontent.com/material-components/material-web/main/tokens/versions/v0_192/_md-sys-motion.scss) |
| Carbon | Numbered ladder | `$spacing-01` to `$spacing-13`; type sets end `-01` (productive) or `-02` (expressive) | [Spacing](https://carbondesignsystem.com/elements/spacing/overview/), [Typography](https://carbondesignsystem.com/elements/typography/overview/) |
| Primer | Base is the pixel value; functional is `--<category>-<property>-<variant>`; control is `--control-<size>-<property>-<variant>`; type sizes use xs to 2xl | `--base-size-8`, `--borderRadius-medium`, `--text-body-size-medium` | [Size](https://primer.style/foundations/primitives/size), [Typography](https://primer.style/foundations/primitives/typography) |
| Apple | Named styles, no token names | Large Title, Title 1 to 3, Headline, Body, Callout, Subhead, Footnote, Caption 1, 2 | [Typography JSON](https://developer.apple.com/tutorials/data/design/human-interface-guidelines/typography.json) |

- **Lessons:** raw tiers are named by **step** (number, ladder word, or the pixel value); role tiers are named by
  **meaning** (primary, body, normal); control tiers are named by **element and part** (button container shape).
- **Size words** (small, medium, large) read best when the list is 3 to 7 steps long; numbers are used when it is longer
  (Carbon 13, Primer 33). Fluent mixes both by adding "Nudge" and number prefixes past XXL.

## 5. Template for AntiNotepad

Three tiers, each pointing up only: **scale** (a few raw steps) > **role** (a meaning, one step) > **control** (the 15
kinds, each naming roles). Today we have role-only tokens (`Typography.Caption`, `Shape.Control`) and literals at the
control; the shape below adds the missing two tiers. No value is chosen here.

| Tier | Holds | Name by | Rule |
| --- | --- | --- | --- |
| Scale | The few raw steps per category | Size word (XS to XL) or small number | A step exists only if a role uses it; the user picks every value in the scale proposal |
| Role | Meaning in the UI, mapped to one step | Meaning (`Shape.Control`, `Typography.Body`) | Each role names exactly one scale step. Roles are what controls cite |
| Control | One entry per control kind: which roles it uses for size, radius, font, padding, fill, border, shadow | Element and part (`TextButton.Radius`) | Cites roles only; a literal here is an exception (below) |

**Step counts to aim for** (the user said about 5). These are targets for the scale proposal, set against the smallest
published systems above; the actual numbers are not chosen.

| Category | Aim | Why this count | Reference points |
| --- | --- | --- | --- |
| Spacing | 6 | A 4 px base needs about 6 steps to span 4 to 32 | Fluent React 11, Carbon 13 |
| Radius | 5 | none, small, medium, large, round; Windows needs 3, Fluent 2 uses 6 | Windows 3, Fluent 2 6, Material 7 |
| Type size | 5 | Primer composes 11 styles from 6 sizes; we have 11 text sizes today | Primer 6 sizes, Apple 11 |
| Type role | 6 to 8 | Roles (title, body, label, caption and so on) share the 5 sizes | Material 15, Primer 11 |
| Weight | 2 | Regular and one emphasis; Windows publishes Regular and Semibold only | [Typography](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/typography) |
| Stroke | 2 | thin and thick | Fluent 2 has 4 |
| Elevation (shadow) | 2 | Today: menu and pill shadows; Windows names 6 | Windows 6 |
| Motion | 4 | Windows has 3 durations; ours has 12 | Windows 3, Fluent React 8, Material 16 |
| Alpha (state, scrim) | 5 | Hover, press, selected, scrim, wash; steps are a guess until the alpha study | not read in this study |
| Icon size | 3 | Glyph sizes follow the type steps | Ours today: 13 to 20, six values |

**Control tier:** one entry for each of the 15 kinds from the A4g map.

| Kind | Kind | Kind |
| --- | --- | --- |
| Text button | Icon button | Toggle |
| Dropdown | Text field | Slider (no instance today) |
| Card | Preview tile | Swatch |
| Popup and menu | Badge | Pill |
| Overlay panel | Toast | Text roles |

- **Exceptions, our rule** (taken from the pattern in section 3): a control may name a literal only when the entry says
  why, and only in the control tier; the scale is never edited for a one-off. The pager dots (7 and 9 px sizes) and the
  3.5 radius dot in the A4g map are the likely first exceptions; the proposal decides.
- **Names:** scale steps by size word or number, roles by meaning (keep the existing `Typography.*`, `Shape.*`
  names), controls by `<Kind>.<Part>`. Keep `ChromeTokens` names as aliases so the bar measurement lock holds
  (named in the A4g map, "ChromeTokens alias candidates").
- **Next step (scale proposal):** the user and builder choose the actual step values within these counts, and map each
  A4g disagreement to a role. Nothing is applied before a lab build.

## Not covered
- Fluent control tokens and a Fluent 2 worked chain: not on the pages read.
- Carbon and Primer exception mechanisms, Carbon component-size tokens, Primer radius and control size values: not
  published on the pages fetched.
- Apple iOS/macOS spacing numbers; Material 3 and Apple HIG prose pages (empty on fetch); the other platform text-style tables beyond iOS and macOS.
- Color systems and state-layer alpha steps: out of scope for this study.
