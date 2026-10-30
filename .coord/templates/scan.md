# Backlog scan: <slug>

Brief for one Haiku scout (role `scout`), read-only, no Codex. Work only in the detached snapshot
`<absolute snapshot path>` at `<full sha>`. Read the idea's Plan row in `.coord/STATUS.md` and any screenshots named
there; report missing screenshots as a question. Use absolute paths for all reads and the one allowed write:
`<absolute main checkout>/.coord/research/<slug>.md`. Do not edit the snapshot or any other file, plan a fix, or commit.
Keep the report short, with evidence over prose. End your turn with the absolute report path.

Write that report in this format:

Scanned at: <full sha>

## Kind
<bug | feature | polish | process, and why>

## Now
<What the app does today, with repo-relative file:line evidence.>

## Where
One backticked repo-relative file path per bullet; put symbols after the path. No folders or globs.
- `path/to/file` — <symbols, callers or shared state>

## Size and tier
<Size guess; Low | Medium | High with a reason slug from docs/loop.md, Tiers.>

## Risk
<What is uncertain or likely to affect other behavior.>

## Links
<Other backlog slugs this overlaps, depends on, or would make easier; say how, or None.>

## Questions
<Questions for the user, or None. No plan and no fix.>
