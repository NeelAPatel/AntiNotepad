**L4-lv4-skill-diet — Build debrief**

Updated the four scoped skills: all are now typed-only, their descriptions are shorter, and the three skills with live facts use PowerShell preprocessing. `review-task` names `design-reviewer` for the bounds check. The phase steps were otherwise left unchanged.

**Checks:** The frontmatter mapping shape and required flags passed a structural check; `git diff --check` passed. All three live-facts commands exited successfully in this worktree. The requested gate **failed** because the build reported NU1900 while unable to reach NuGet vulnerability data; selftest, seam, scope, and other reported checks passed. I couldn’t verify Claude’s typed-command, natural-language triggering, or skill fork behavior from this environment.

The diff is limited to the four requested skill files. I did not create the requested report because the instructions also explicitly said never to edit other files. The item is **not ready to mark complete**: the gate did not pass, the skill runs and size measurements remain unverified, and natural-phrase behavior still needs your hand check. I’ll wait for your corrections or go-ahead.