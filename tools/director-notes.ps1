# SessionStart hook after a context compaction: prints each master's notes file so a master resumes from its own notes.
# The notes live untracked in the main checkout (.coord/directors/<slug>.md), so worktree sessions read the same files.
$ErrorActionPreference = 'SilentlyContinue'
[void][Console]::In.ReadToEnd()

$commonDirectory = git -C $PSScriptRoot rev-parse --path-format=absolute --git-common-dir
$root = if ($commonDirectory) { Split-Path -Parent $commonDirectory.Trim() } else { Split-Path -Parent $PSScriptRoot }
$folder = Join-Path $root '.coord/directors'
$notes = @(Get-ChildItem -LiteralPath $folder -Filter '*.md' -File | Sort-Object Name)
if ($notes.Count -eq 0) { exit 0 }

Write-Output 'Master notes (.coord/directors/). If you are the master named in one of these files, it is your own record:'
Write-Output 'read it before acting and keep it current. Sub-sessions ignore this.'
foreach ($note in $notes) {
    Write-Output ''
    Write-Output ('=== {0} ===' -f $note.Name)
    Get-Content -LiteralPath $note.FullName -Raw
}
exit 0
