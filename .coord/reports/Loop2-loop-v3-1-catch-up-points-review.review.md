# Loop2-loop-v3-1-catch-up-points lens review

## Findings (proven)
1. [major] tools/catch-up.ps1:37,71 - Git-quoted incoming filenames can incorrectly suppress a required hand test. `git diff --name-only` without `-z` quotes non-ASCII paths under Git's default `core.quotePath=true`: an incoming `docs/café.md` is represented as `"docs/caf\303\251.md"`. The card parser produces the literal `docs/café.md`, but line 37 compares the quoted Git output directly, so the CLEAN path prints `RETEST: no` even though that incoming file is listed. Proof: an in-memory probe of the exact parser and Write-Retest function, substituting only card filesystem reads, printed `RETEST: no` for the quoted incoming path and `RETEST: yes (docs/café.md)` for the unquoted control. Git's read-only check-attr output also confirmed this pathname quoting representation. The code path is lines 71 -> 77 -> 37 -> 39; both review-task step 5 and loop.md then preserve the earlier hand-test result. Smallest robust fix: collect incoming paths with `git diff --name-only -z`, preserve and split its NUL-delimited output, and compare literal paths. Add a Unicode filename probe; merely disabling core.quotePath still leaves quotes and escapes for other valid filenames.

## Behavior changes check
Report's list vs the diff: matches the intended changes, except the claim that matching incoming filenames always trigger RETEST is disproved by finding 1.

- Local `refs/heads/<Base>` is authoritative for resolution, ancestry, incoming diff and merge. Missing local base refuses; missing remote ref is allowed; differing remote/local commits produce exactly one NOTE. Remote resolution errors other than the missing-ref result refuse.
- Files-section parsing is identical to tools/files-overlap.ps1, including trimming and backslash-to-slash normalization. ASCII matching, empty incoming and ignoring other card sections passed the probes.
- Successful paths append RETEST. Conflict and refusal paths do not. Git failure branches check exit status before printing CLEAN or UP TO DATE.
- Callers/consumers checked: review-task step 2 (catch up, push, gate; stop on conflict), review-task step 5 (hand-test decision), merge-task step 1 (behind-dev preflight routes back to the item session), and loop.md's catch-up paragraph. Their wording matches the planned script contract; finding 1 undermines the RETEST consumer decision. The behind-dev wording matches tools/merge-preflight.ps1:55.
- No new persistent state, asynchronous work, resource ownership, product layering or product contracts.

## Scope and plan check
In plan. `git diff -- tools .claude docs` changes exactly the brief's four files. In docs/loop.md, the only additions are the catch-up paragraph and the four requested Files-table rows. No unrelated hunks or unlisted implementation deviations. The report discloses its bundle-origin probe deviation. Existing coordination artifacts and STATUS changes were not attributed to this step's implementation diff.

## Unproven risks
- None retained.

## Checks run
- Read the item card, step brief, implementer report, gate report, lens-review skill and coding standard; inspected the complete scoped diff and surrounding scripts/skill instructions. Branch verified as chore/Loop2-loop-v3-1.
- PowerShell AST syntax check: 0 errors. Get-CardFiles function comparison against files-overlap.ps1: identical.
- In-memory parser/RETEST probes (no files created): normalized listed ASCII path -> yes; path listed only after Files -> no; empty incoming -> no; quoted Unicode listed path -> no (FAIL); unquoted Unicode control -> yes.
- Ran the unmodified catch-up script in child PowerShell processes with Git replaced by an in-memory command mock: CLEAN listed -> exit 0/yes; CLEAN unlisted -> exit 0/no; UP TO DATE -> exit 0/no; missing origin ref -> CLEAN without NOTE; equal origin/local -> CLEAN without NOTE; differing origin/local -> exactly one NOTE. Source inspection confirms all base operations use the fully qualified local ref.
- Mocked conflict -> CONFLICT, no RETEST, script exit 2 (explicitly propagated through the child command wrapper). Missing local base, wrong branch, dirty checkout, status/fetch/remote-resolution/ancestry/incoming-diff/merge/conflict-list failures -> refusal, exit 1, no RETEST, no CLEAN/UP TO DATE.
- `git diff --check -- tools .claude docs`: PASS.
- Supplied gate report: GATE PASS; build 0 warnings/errors; SELFTEST PASS; CONTRACTS 13 rows, 12 pass, 1 manual. Not rerun: this review made no product changes. Implementer's real-Git merge probes were read, not independently repeated; reviewer Git-command probes were mocked and do not establish live fetch/merge behavior.
- Only this report was written. No branches switched, commits, merges, fetches, app launches or product edits performed.

VERDICT: rework
