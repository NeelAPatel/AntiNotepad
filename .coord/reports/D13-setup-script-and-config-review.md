# D13-setup-script-and-config-review

I reviewed the current worktree files, including both workflows and the untracked release workflow. This is a static review, not a live workflow or hook test. I did not inspect history or files outside the requested review scope, apart from the hook and packaging scripts needed to understand what the configuration runs.

| File | Finding | Severity | Suggested change |
|---|---|---|---|
| `.gitignore` | `.openchamber/` is not ignored by the repository rules; local state could be added accidentally. | must fix | Add `.openchamber/` before contributors create local state, and review any existing tracked content separately. |
| `.gitignore` | `.claude/settings.local.json`, `.coord/logs/` and `build/` match repository ignore rules. | fine | Keep these entries. Ignore rules do not remove files already tracked. |
| `.github/workflows/gate.yml` | Manual dispatch only; no PR trigger, secret reference, private repository name, home path, or interpolated PR input in `run`. Its three GitHub-maintained actions use mutable major-version tags, and token permissions are inherited from repository settings. No third-party action is used. | should fix | Pin actions to reviewed full commit SHAs and set `permissions: contents: read` explicitly. Keep manual dispatch unless a separate PR threat review is done. |
| `.github/workflows/release.yml` | Tag pushes trigger a release with `contents: write`; the built-in token is passed to `gh`, with no hard-coded secret. Actions are SHA-pinned GitHub actions. No PR trigger, private repository name, home path, or direct expression interpolation into shell code was found. The tag is passed through environment variables; the packaging script validates the version before publication. | fine | Keep the token limited to release publishing; restrict who can create matching tags and review workflow changes. This is safe for public use only with trusted tag creation and repository settings. |
| `.claude/settings.json` and `tools/loop-card.ps1` | Every prompt invokes project hooks; the card hook checks a local endpoint and can start a hidden roadmap server. This is unexpected for a contributor who only opens the project. | must fix | Make the server startup opt-in or move this hook to local settings before public use; document any retained hooks. |
| `.claude/settings.json` and `tools/guard-bash.ps1` | The Bash hook logs selected commands and working directories to `build/guard.log`; current `tools/guard.mode` is `log`, so it does not enforce its proposed blocks. Commands can contain sensitive text. | should fix | Make logging opt-in or remove command contents; document the mode, data recorded, location and retention. |
| `.claude/settings.json`, `tools/director-notes.ps1`, `tools/status-line.ps1`, `tools/worktree-check.ps1` | Hooks can read local director notes on compaction, invoke a user-level status-line script, and write worktree diagnostics in the main checkout. These are machine-dependent behaviors even though settings use variable-based paths. | should fix | Make personal integrations opt-in or local-only; document what is read, executed and written. |
| `.claude/settings.json` | It requests approval for edits to two project files and changes attribution, output style and worktree defaults. No allow-list granting access to private areas or hard-coded personal path was found. | should fix | Document the approval prompts and project-specific defaults, or move personal preferences to local settings. |
| `.claude/rules/coding.md` | Product-scoped coding conventions contain no personal path or username. | fine | Keep the rule and explain the conventions in contributor documentation as needed. |
| `.codex/agents/lens-reviewer.toml` | The reviewer configuration uses a specific model and project skill, with project-relative paths and no personal path, username, hook or private-area allow-list. Model availability can vary. | should fix | Document the optional reviewer and model requirement; keep it if that workflow is intended for contributors. |
| `.agents/skills/lens-review/SKILL.md` | Project-specific review instructions use relative paths and include internal workflow expectations; no personal path or username was found. | fine | Keep as optional project guidance and clarify that outside contributors need not run the internal loop. |

## Setup and checks

- Added `tools/setup.ps1`: checks Windows, PowerShell and the selected .NET SDK; restores once, builds without another restore, and checks the self-test exit code and pass marker with a unique temporary data folder. `-SkipSelfTest` reports that verification was skipped.
- Updated the build sections of the README and contributor guide, including isolated data for the manual self-test.
- PowerShell parser: PASS. I did not execute the script, restore, build, self-test, or run any server or browser checks. NuGet network access is restricted in this sandbox; Claude should run the full script in the intended environment.
- `git check-ignore --no-index` matched the three listed ignored paths and did not match `.openchamber/`.

## Items for me to decide

- Address the missing ignore rule and opt-in hook behavior before public use; this review does not change the reviewed files.
- Decide whether to pin the gate actions and set its token permissions explicitly.
- Confirm tag creation and workflow-change controls in GitHub; repository settings were not inspected.
- Run the complete setup script on a Windows 11 fresh clone with .NET 10 and PowerShell 7, and confirm `SELFTEST PASS` and the isolated dev-copy handoff.
