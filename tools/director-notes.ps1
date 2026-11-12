# SessionStart hook after a context compaction: prints ONLY the active director's notes file, and only in director chats.
# Director chats run in the main checkout; item sub-chats run in a worktree under .claude/worktrees and print nothing.
# The notes live untracked in the main checkout (.coord/directors/<slug>.md). Own file = $env:ANTINOTEPAD_DIRECTOR (slug)
# when set, else the most recently written notes file (a director keeps its notes current). Files in archive/ are never read.
$ErrorActionPreference = 'SilentlyContinue'
[void][Console]::In.ReadToEnd()

$project = [string]$env:CLAUDE_PROJECT_DIR
if ($project.Replace('\', '/') -like '*/.claude/worktrees/*') { exit 0 }

$commonDirectory = git -C $PSScriptRoot rev-parse --path-format=absolute --git-common-dir
$root = if ($commonDirectory) { Split-Path -Parent $commonDirectory.Trim() } else { Split-Path -Parent $PSScriptRoot }
$folder = Join-Path $root '.coord/directors'
$notes = @(Get-ChildItem -LiteralPath $folder -Filter '*.md' -File)
if ($notes.Count -eq 0) { exit 0 }

$own = $null
if ($env:ANTINOTEPAD_DIRECTOR) { $own = $notes | Where-Object { $_.BaseName -ceq $env:ANTINOTEPAD_DIRECTOR } | Select-Object -First 1 }
if (-not $own) { $own = $notes | Sort-Object LastWriteTime -Descending | Select-Object -First 1 }

Write-Output 'Director notes (.coord/directors/): your own record if you are this director; keep it current.'
Write-Output ('=== {0} ===' -f $own.Name)
Get-Content -LiteralPath $own.FullName -Raw
exit 0
