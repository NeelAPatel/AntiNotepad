# Builds the read-only data document served by the local roadmap server.
# Sources come from the main checkout working copy and local Git metadata.
[CmdletBinding()]
param(
    [Alias('Root')][string]$RequestedRoot,
    [switch]$Check,
    [string]$Snapshot
)

function Invoke-RoadmapGit {
    param(
        [Parameter(Mandatory)][string]$Repository,
        [Parameter(Mandatory)][string[]]$Arguments
    )

    try {
        $lines = @(& git -C $Repository @Arguments 2>$null)
        $exitCode = $LASTEXITCODE
        return [pscustomobject]@{
            Success = ($exitCode -eq 0)
            Lines = @($lines | ForEach-Object { [string]$_ })
            Text = (@($lines | ForEach-Object { [string]$_ }) -join "`n")
        }
    }
    catch {
        return [pscustomobject]@{
            Success = $false
            Lines = @()
            Text = ''
        }
    }
}

function Resolve-RoadmapRoot {
    param(
        [string]$RequestedRoot,
        [string]$ScriptRoot = $PSScriptRoot
    )

    $worktreeRoot = [System.IO.Path]::GetFullPath((Join-Path $ScriptRoot '..'))
    if (-not [string]::IsNullOrWhiteSpace($RequestedRoot)) {
        try {
            return [System.IO.Path]::GetFullPath($RequestedRoot)
        }
        catch {
            return $RequestedRoot
        }
    }

    $commonDirectory = Invoke-RoadmapGit -Repository $worktreeRoot -Arguments @(
        'rev-parse', '--path-format=absolute', '--git-common-dir'
    )
    if ($commonDirectory.Success -and -not [string]::IsNullOrWhiteSpace($commonDirectory.Text)) {
        try {
            return [System.IO.Directory]::GetParent($commonDirectory.Text.Trim()).FullName
        }
        catch {
            # Keep the script's checkout as the fallback if Git returns an unusual common directory.
        }
    }

    return $worktreeRoot
}

function Add-RoadmapWarning {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[string]]$Warnings,
        [Parameter(Mandatory)][string]$Message
    )

    if (-not $Warnings.Contains($Message)) {
        [void]$Warnings.Add($Message)
    }
}

function Get-RoadmapSection {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]]$Lines,
        [Parameter(Mandatory)][string]$Heading
    )

    $headingPattern = '^##\s+' + [regex]::Escape($Heading) + '\s*$'
    $start = -1
    for ($index = 0; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -match $headingPattern) {
            $start = $index + 1
            break
        }
    }

    if ($start -lt 0) {
        return ''
    }

    $end = $Lines.Count
    for ($index = $start; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -match '^##\s+') {
            $end = $index
            break
        }
    }

    if ($end -le $start) {
        return ''
    }

    return (($Lines[$start..($end - 1)] -join "`n").Trim())
}

function Get-RoadmapPlanRows {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]]$Lines,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[string]]$Warnings
    )

    $rows = [System.Collections.Generic.List[object]]::new()
    $groups = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    if ($Lines.Count -eq 0) {
        return [pscustomobject]@{ Rows = @(); Groups = 0; TableRows = 0 }
    }

    $headingPattern = '^###\s+Pending work\s*$'
    $start = -1
    for ($index = 0; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -match $headingPattern) {
            $start = $index + 1
            break
        }
    }

    if ($start -lt 0) {
        Add-RoadmapWarning -Warnings $Warnings -Message 'STATUS.md: the Pending work section is missing.'
        return [pscustomobject]@{ Rows = @(); Groups = 0; TableRows = 0 }
    }

    $end = $Lines.Count
    for ($index = $start; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -match '^##\s+') {
            $end = $index
            break
        }
    }

    $currentGroup = ''
    $tableRows = 0
    for ($index = $start; $index -lt $end; $index++) {
        $line = $Lines[$index].Trim()
        if ($line -notmatch '^\|.*\|$') {
            continue
        }

        $cells = @($line.Trim('|').Split('|') | ForEach-Object { $_.Trim() })
        if ($cells.Count -lt 5) {
            continue
        }

        if ($cells[0] -eq 'Item' -or ($cells | Where-Object { $_ -notmatch '^:?-{2,}:?$' }).Count -eq 0) {
            continue
        }

        $isGroup = $cells[0] -match '^\*\*(.+)\*\*$' -and
            (@($cells | Select-Object -Skip 1 | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }).Count -eq 0)
        if ($isGroup) {
            $currentGroup = $Matches[1].Trim()
            [void]$groups.Add($currentGroup)
            continue
        }

        if ([string]::IsNullOrWhiteSpace($cells[0])) {
            continue
        }

        $tableRows++
        [void]$rows.Add([ordered]@{
            item = $cells[0]
            scope = $cells[1]
            tier = $cells[2]
            effort = $cells[3]
            state = $cells[4]
            section = 'Pending work'
            group = $currentGroup
        })
    }

    return [pscustomobject]@{
        Rows = $rows.ToArray()
        Groups = $groups.Count
        TableRows = $tableRows
    }
}

function Get-RoadmapNowItems {
    param([Parameter(Mandatory)][AllowEmptyString()][string]$Section)

    $items = [System.Collections.Generic.List[string]]::new()
    $current = $null
    foreach ($line in ($Section -split "\r?\n")) {
        if ($line -match '^\s*[-*+]\s+(.*)$') {
            if ($null -ne $current) {
                [void]$items.Add($current.Trim())
            }

            $current = $Matches[1].Trim()
        }
        elseif ($null -ne $current -and -not [string]::IsNullOrWhiteSpace($line)) {
            $current += ' ' + $line.Trim()
        }
    }

    if ($null -ne $current) {
        [void]$items.Add($current.Trim())
    }

    return ,$items.ToArray()
}

function Get-RoadmapCardSection {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]]$Lines,
        [Parameter(Mandatory)][string]$Heading
    )

    $headingPattern = '^##\s+' + [regex]::Escape($Heading) + '\s*$'
    $start = -1
    for ($index = 0; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -match $headingPattern) {
            $start = $index + 1
            break
        }
    }

    if ($start -lt 0) {
        return ''
    }

    $end = $Lines.Count
    for ($index = $start; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -match '^##\s+') {
            $end = $index
            break
        }
    }

    if ($end -le $start) {
        return ''
    }

    return (($Lines[$start..($end - 1)] -join "`n").Trim())
}

function Get-RoadmapCards {
    param(
        [Parameter(Mandatory)][string]$Repository,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[string]]$Warnings
    )

    $cards = [ordered]@{}
    $folder = Join-Path $Repository '.coord/tasks'
    try {
        if (-not (Test-Path -LiteralPath $folder -PathType Container)) {
            Add-RoadmapWarning -Warnings $Warnings -Message '.coord/tasks: folder is missing or unreadable.'
            return $cards
        }

        $files = @(Get-ChildItem -LiteralPath $folder -Filter '*.md' -File -ErrorAction Stop | Sort-Object Name)
    }
    catch {
        Add-RoadmapWarning -Warnings $Warnings -Message '.coord/tasks: folder is missing or unreadable.'
        return $cards
    }

    foreach ($file in $files) {
        try {
            $content = Get-Content -LiteralPath $file.FullName -Raw -ErrorAction Stop
            $lines = @($content -split "\r?\n")
            $titleLine = $lines | Where-Object { $_ -match '^#\s+' } | Select-Object -First 1
            $branchMatch = $lines | Where-Object { $_ -match '^Branch:\s*' } | Select-Object -First 1
            $tierMatch = $lines | Where-Object { $_ -match '^Tier:\s*' } | Select-Object -First 1
            if ([string]::IsNullOrWhiteSpace($titleLine) -or $null -eq $branchMatch -or $null -eq $tierMatch) {
                continue
            }

            $title = $titleLine.TrimStart('#').Trim()
            $keyMatch = [regex]::Match($title, '^([A-Za-z][A-Za-z0-9]*(?:-[A-Za-z0-9]+)+)(?::|\s|$)')
            if (-not $keyMatch.Success) {
                continue
            }

            $key = $keyMatch.Groups[1].Value
            if ($file.BaseName -ine $key) {
                continue
            }

            $branch = [regex]::Replace($branchMatch, '^Branch:\s*', '').Trim().Trim('`')
            $tier = [regex]::Replace($tierMatch, '^Tier:\s*', '').Trim().Trim('`')
            $cards[$key] = [ordered]@{
                title = $title
                branch = $branch
                tier = $tier
                request = Get-RoadmapCardSection -Lines $lines -Heading 'Request'
                plan = Get-RoadmapCardSection -Lines $lines -Heading 'Plan'
                check = Get-RoadmapCardSection -Lines $lines -Heading 'Check'
                hand = Get-RoadmapCardSection -Lines $lines -Heading 'Hand test'
            }
        }
        catch {
            Add-RoadmapWarning -Warnings $Warnings -Message ('.coord/tasks/{0}: file is unreadable.' -f $file.Name)
        }
    }

    return $cards
}

function Get-RoadmapUsage {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[string]]$Warnings
    )

    $entries = [System.Collections.Generic.List[object]]::new()
    try {
        if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
            Add-RoadmapWarning -Warnings $Warnings -Message '.coord/logs/usage.tsv: file is missing or unreadable.'
            return ,$entries.ToArray()
        }

        $lines = Get-Content -LiteralPath $Path -ErrorAction Stop
    }
    catch {
        Add-RoadmapWarning -Warnings $Warnings -Message '.coord/logs/usage.tsv: file is missing or unreadable.'
        return ,$entries.ToArray()
    }

    $malformed = $false
    foreach ($line in $lines) {
        if ([string]::IsNullOrWhiteSpace($line) -or $line -match '^\s*#' -or $line -match '^day\t') {
            continue
        }

        $fields = @($line -split "`t", 7)
        if ($fields.Count -ne 7) {
            $malformed = $true
            continue
        }

        [void]$entries.Add([ordered]@{
            day = $fields[0]
            at = $fields[1]
            role = $fields[2]
            model = $fields[3]
            task = $fields[4]
            exit = $fields[5]
            secs = $fields[6]
        })
    }

    if ($malformed) {
        Add-RoadmapWarning -Warnings $Warnings -Message '.coord/logs/usage.tsv: one or more rows are malformed.'
    }

    return ,$entries.ToArray()
}

function Get-RoadmapShortHash {
    param([AllowNull()][string]$Hash)

    if ([string]::IsNullOrWhiteSpace($Hash)) {
        return ''
    }

    return $Hash.Substring(0, [Math]::Min(7, $Hash.Length))
}

function Get-RoadmapEraNumber {
    param(
        [Parameter(Mandatory)][string]$Commit,
        [Parameter(Mandatory)][object[]]$Eras,
        [Parameter(Mandatory)][hashtable]$UntilSets,
        [Parameter(Mandatory)][hashtable]$FromSets
    )

    for ($index = 0; $index -lt $Eras.Count; $index++) {
        $key = [string]$index
        if ($UntilSets.ContainsKey($key) -and $UntilSets[$key].Contains($Commit)) {
            $nameMatch = [regex]::Match([string]$Eras[$index].name, '\d+')
            if ($nameMatch.Success) {
                return [int]$nameMatch.Value
            }

            return $index
        }
    }

    for ($index = $Eras.Count - 1; $index -ge 0; $index--) {
        $key = [string]$index
        if ($FromSets.ContainsKey($key) -and $FromSets[$key].Contains($Commit)) {
            $nameMatch = [regex]::Match([string]$Eras[$index].name, '\d+')
            if ($nameMatch.Success) {
                return [int]$nameMatch.Value
            }

            return $index
        }
    }

    return $null
}

function Get-RoadmapEraSets {
    param(
        [Parameter(Mandatory)][string]$Repository,
        [Parameter(Mandatory)][AllowEmptyCollection()][object[]]$Eras,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[string]]$Warnings
    )

    $untilSets = @{}
    $fromSets = @{}
    for ($index = 0; $index -lt $Eras.Count; $index++) {
        $key = [string]$index
        $until = [string]$Eras[$index].until
        $from = [string]$Eras[$index].from

        if (-not [string]::IsNullOrWhiteSpace($until)) {
            $result = Invoke-RoadmapGit -Repository $Repository -Arguments @('rev-list', $until)
            if ($result.Success) {
                $untilSets[$key] = [System.Collections.Generic.HashSet[string]]::new([string[]]$result.Lines, [System.StringComparer]::OrdinalIgnoreCase)
            }
            else {
                Add-RoadmapWarning -Warnings $Warnings -Message ('eras.json: the until commit for {0} is unavailable.' -f $Eras[$index].name)
            }
        }

        if (-not [string]::IsNullOrWhiteSpace($from)) {
            $result = Invoke-RoadmapGit -Repository $Repository -Arguments @('rev-list', '--branches', $from, '--not', ($from + '^'))
            if ($result.Success) {
                $fromSets[$key] = [System.Collections.Generic.HashSet[string]]::new([string[]]$result.Lines, [System.StringComparer]::OrdinalIgnoreCase)
            }
            else {
                Add-RoadmapWarning -Warnings $Warnings -Message ('eras.json: the from commit for {0} is unavailable.' -f $Eras[$index].name)
            }
        }
    }

    return [pscustomobject]@{ UntilSets = $untilSets; FromSets = $fromSets }
}

function Get-RoadmapGitData {
    param(
        [Parameter(Mandatory)][string]$Repository,
        [Parameter(Mandatory)][AllowEmptyCollection()][object[]]$Eras,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[string]]$Warnings
    )

    $empty = [ordered]@{
        log = @()
        worktrees = @()
        branches = @()
        tags = @()
        main = ''
        devAhead = 0
        devLine = @()
        graph = [ordered]@{ lanes = @(); commits = @() }
    }

    $probe = Invoke-RoadmapGit -Repository $Repository -Arguments @('rev-parse', '--show-toplevel')
    if (-not $probe.Success) {
        Add-RoadmapWarning -Warnings $Warnings -Message 'git: repository metadata is unavailable.'
        return $empty
    }

    $branchResult = Invoke-RoadmapGit -Repository $Repository -Arguments @(
        'for-each-ref', '--format=%(refname:short)|%(objectname)|%(committerdate:format:%Y-%m-%d %H:%M:%S %z)|%(committerdate:unix)', 'refs/heads'
    )
    $tagResult = Invoke-RoadmapGit -Repository $Repository -Arguments @(
        'for-each-ref', '--format=%(refname:short)|%(objectname)|%(*objectname)|%(creatordate:short)', 'refs/tags'
    )
    $worktreeResult = Invoke-RoadmapGit -Repository $Repository -Arguments @('worktree', 'list', '--porcelain')

    if (-not $branchResult.Success) {
        Add-RoadmapWarning -Warnings $Warnings -Message 'git: local branches could not be read.'
    }
    if (-not $tagResult.Success) {
        Add-RoadmapWarning -Warnings $Warnings -Message 'git: tags could not be read.'
    }
    if (-not $worktreeResult.Success) {
        Add-RoadmapWarning -Warnings $Warnings -Message 'git: worktrees could not be read.'
    }

    $branchRefs = [System.Collections.Generic.List[object]]::new()
    foreach ($line in $branchResult.Lines) {
        $parts = @($line -split '\|', 4)
        if ($parts.Count -lt 4) {
            continue
        }

        [void]$branchRefs.Add([pscustomobject]@{
            Name = $parts[0]
            FullHash = $parts[1]
            Date = $parts[2]
            UnixDate = [long]0
        })
        $branchRefs[$branchRefs.Count - 1].UnixDate = [long]$parts[3]
    }

    $tagRefs = [System.Collections.Generic.List[object]]::new()
    foreach ($line in $tagResult.Lines) {
        $parts = @($line -split '\|', 4)
        if ($parts.Count -lt 4) {
            continue
        }

        $target = if (-not [string]::IsNullOrWhiteSpace($parts[2])) { $parts[2] } else { $parts[1] }
        [void]$tagRefs.Add([pscustomobject]@{
            Name = $parts[0]
            FullHash = $target
            Hash = Get-RoadmapShortHash $target
            Date = $parts[3]
        })
    }

    $worktrees = [System.Collections.Generic.List[object]]::new()
    $worktree = $null
    foreach ($line in $worktreeResult.Lines) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            if ($null -ne $worktree) {
                [void]$worktrees.Add($worktree)
                $worktree = $null
            }
            continue
        }

        if ($line.StartsWith('worktree ')) {
            if ($null -ne $worktree) {
                [void]$worktrees.Add($worktree)
            }
            $worktree = [ordered]@{ worktree = $line.Substring(9); HEAD = ''; branch = '' }
        }
        elseif ($null -ne $worktree -and $line.StartsWith('HEAD ')) {
            $worktree.HEAD = $line.Substring(5)
        }
        elseif ($null -ne $worktree -and $line.StartsWith('branch ')) {
            $worktree.branch = $line.Substring(7) -replace '^refs/heads/', ''
        }
    }
    if ($null -ne $worktree) {
        [void]$worktrees.Add($worktree)
    }

    $worktreeByBranch = @{}
    foreach ($entry in $worktrees) {
        if (-not [string]::IsNullOrWhiteSpace($entry.branch)) {
            $worktreeByBranch[$entry.branch] = $entry.worktree
        }
    }

    $branchRows = [System.Collections.Generic.List[object]]::new()
    foreach ($branch in $branchRefs) {
        $behind = 0
        $ahead = 0
        $fork = ''
        if ($branchRefs.Name -contains 'dev') {
            $counts = Invoke-RoadmapGit -Repository $Repository -Arguments @('rev-list', '--left-right', '--count', ('dev...' + $branch.Name))
            if ($counts.Success -and $counts.Lines.Count -gt 0) {
                $values = @($counts.Lines[0] -split '\s+')
                if ($values.Count -ge 2) {
                    $behind = [int]$values[0]
                    $ahead = [int]$values[1]
                }
            }

            $mergeBase = Invoke-RoadmapGit -Repository $Repository -Arguments @('merge-base', 'dev', $branch.Name)
            if ($mergeBase.Success -and $mergeBase.Lines.Count -gt 0) {
                $fork = Get-RoadmapShortHash $mergeBase.Lines[0]
            }
        }

        [void]$branchRows.Add([ordered]@{
            name = $branch.Name
            head = Get-RoadmapShortHash $branch.FullHash
            date = $branch.Date
            behind = $behind
            ahead = $ahead
            fork = $fork
        })
    }

    $tagRows = @($tagRefs | ForEach-Object {
        [ordered]@{ name = $_.Name; hash = $_.Hash; date = $_.Date }
    })

    $main = ''
    $devAhead = 0
    $mainRef = $branchRefs | Where-Object Name -eq 'main' | Select-Object -First 1
    $devRef = $branchRefs | Where-Object Name -eq 'dev' | Select-Object -First 1
    if ($null -ne $mainRef) {
        $main = Get-RoadmapShortHash $mainRef.FullHash
    }
    else {
        Add-RoadmapWarning -Warnings $Warnings -Message 'git: the main branch is unavailable.'
    }
    if ($null -ne $mainRef -and $null -ne $devRef) {
        $aheadResult = Invoke-RoadmapGit -Repository $Repository -Arguments @('rev-list', '--count', 'main..dev')
        if ($aheadResult.Success -and $aheadResult.Lines.Count -gt 0) {
            $devAhead = [int]$aheadResult.Lines[0]
        }
    }

    $eraSets = Get-RoadmapEraSets -Repository $Repository -Eras $Eras -Warnings $Warnings
    $logRows = [System.Collections.Generic.List[object]]::new()
    $devLineRows = [System.Collections.Generic.List[object]]::new()
    if ($null -ne $devRef) {
        $logResult = Invoke-RoadmapGit -Repository $Repository -Arguments @(
            'log', '--date=format:%Y-%m-%d', '--format=%H%x1f%h%x1f%cs%x1f%s', 'dev'
        )
        if ($logResult.Success) {
            foreach ($line in $logResult.Lines) {
        $parts = @($line.Split([char]31, 4))
                if ($parts.Count -lt 4) {
                    continue
                }

                $item = ''
                $itemMatch = [regex]::Match($parts[3], '^([A-Za-z][A-Za-z0-9]*(?:-[A-Za-z0-9]+)+):')
                if ($itemMatch.Success) {
                    $item = $itemMatch.Groups[1].Value
                }

                $eraNumber = Get-RoadmapEraNumber -Commit $parts[0] -Eras $Eras -UntilSets $eraSets.UntilSets -FromSets $eraSets.FromSets
                $entry = [ordered]@{
                    hash = $parts[1]
                    date = $parts[2]
                    subject = $parts[3]
                    item = $item
                    era = $eraNumber
                }
                [void]$logRows.Add($entry)
                if ($devLineRows.Count -lt 40) {
                    [void]$devLineRows.Add([ordered]@{
                        hash = $parts[1]
                        date = $parts[2]
                        subject = $parts[3]
                    })
                }
            }
        }
        else {
            Add-RoadmapWarning -Warnings $Warnings -Message 'git: the dev log could not be read.'
        }
    }
    else {
        Add-RoadmapWarning -Warnings $Warnings -Message 'git: the dev branch is unavailable.'
    }

    $lanes = [System.Collections.Generic.List[object]]::new()
    $orderedBranches = [System.Collections.Generic.List[object]]::new()
    foreach ($pinned in @('main', 'dev')) {
        $match = $branchRefs | Where-Object Name -eq $pinned | Select-Object -First 1
        if ($null -ne $match) {
            [void]$orderedBranches.Add($match)
        }
    }
    $rest = @($branchRefs | Where-Object { $_.Name -notin @('main', 'dev') } | Sort-Object -Property @(
        @{ Expression = 'UnixDate'; Descending = $true },
        @{ Expression = 'Name'; Descending = $false }
    ))
    foreach ($branch in $rest) {
        [void]$orderedBranches.Add($branch)
    }

    $laneSets = [System.Collections.Generic.List[System.Collections.Generic.HashSet[string]]]::new()
    $laneFirstParentChains = [System.Collections.Generic.List[object]]::new()
    $laneFirstParentSets = [System.Collections.Generic.List[System.Collections.Generic.HashSet[string]]]::new()
    $eraStart = ''
    if ($Eras.Count -gt 1) {
        $eraStart = [string]$Eras[1].from
    }
    if ([string]::IsNullOrWhiteSpace($eraStart)) {
        $eraStartEntry = $Eras | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_.from) } | Select-Object -First 1
        if ($null -ne $eraStartEntry) {
            $eraStart = [string]$eraStartEntry.from
        }
    }

    foreach ($branch in $orderedBranches) {
        $laneDate = ''
        try {
            $laneDate = ([DateTimeOffset]::FromUnixTimeSeconds($branch.UnixDate).ToLocalTime()).ToString('yyyy-MM-dd HH:mm')
        }
        catch {
            $laneDate = ''
        }
        $laneWorktree = if ($worktreeByBranch.ContainsKey($branch.Name)) { $worktreeByBranch[$branch.Name] } else { '' }
        [void]$lanes.Add([ordered]@{
            name = $branch.Name
            head = Get-RoadmapShortHash $branch.FullHash
            date = $laneDate
            worktree = $laneWorktree
            parent = $null
            fork = $null
        })

        $firstParentResult = Invoke-RoadmapGit -Repository $Repository -Arguments @('rev-list', '--first-parent', '--parents', $branch.Name)
        $firstParentChain = [System.Collections.Generic.List[object]]::new()
        $firstParentHashes = [System.Collections.Generic.List[string]]::new()
        if ($firstParentResult.Success) {
            foreach ($line in $firstParentResult.Lines) {
                $parts = @($line -split '\s+')
                if ($parts.Count -eq 0 -or [string]::IsNullOrWhiteSpace($parts[0])) {
                    continue
                }
                $parentHash = if ($parts.Count -gt 1) { [string]$parts[1] } else { '' }
                [void]$firstParentChain.Add([ordered]@{ hash = $parts[0]; parent = $parentHash; merge = $parts.Count -gt 2 })
                [void]$firstParentHashes.Add($parts[0])
            }
        }
        else {
            Add-RoadmapWarning -Warnings $Warnings -Message ('git: first-parent commits for branch {0} could not be read.' -f $branch.Name)
        }
        [void]$laneFirstParentChains.Add($firstParentChain.ToArray())
        [void]$laneFirstParentSets.Add([System.Collections.Generic.HashSet[string]]::new([string[]]$firstParentHashes.ToArray(), [System.StringComparer]::OrdinalIgnoreCase))

        if ([string]::IsNullOrWhiteSpace($eraStart)) {
            [void]$laneSets.Add([System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase))
            continue
        }

        $laneLog = Invoke-RoadmapGit -Repository $Repository -Arguments @('rev-list', $branch.Name, '--not', ($eraStart + '^'))
        if ($laneLog.Success) {
            [void]$laneSets.Add([System.Collections.Generic.HashSet[string]]::new([string[]]$laneLog.Lines, [System.StringComparer]::OrdinalIgnoreCase))
        }
        else {
            Add-RoadmapWarning -Warnings $Warnings -Message ('git: commits for branch {0} could not be read.' -f $branch.Name)
            [void]$laneSets.Add([System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase))
        }
    }

    for ($laneIndex = 0; $laneIndex -lt $orderedBranches.Count; $laneIndex++) {
        $branch = $orderedBranches[$laneIndex]
        if ($branch.Name -eq 'main') {
            continue
        }

        $parentLane = -1
        $forkHash = ''
        $bestDistance = [int]::MaxValue
        if ($branch.Name -eq 'dev') {
            if ($null -ne $mainRef -and $null -ne $devRef) {
                $isMainAncestor = Invoke-RoadmapGit -Repository $Repository -Arguments @('merge-base', '--is-ancestor', 'main', 'dev')
                if ($isMainAncestor.Success) {
                    $forkResult = Invoke-RoadmapGit -Repository $Repository -Arguments @('merge-base', 'main', 'dev')
                    if ($forkResult.Success -and $forkResult.Lines.Count -gt 0) {
                        $forkHash = $forkResult.Lines[0]
                        $parentLane = 0
                    }
                }
            }
        }

        if ($branch.Name -ne 'dev' -or $parentLane -lt 0) {
            $branchChain = @($laneFirstParentChains[$laneIndex])
            for ($candidate = 0; $candidate -lt $orderedBranches.Count; $candidate++) {
                if ($candidate -eq $laneIndex) {
                    continue
                }

                $otherBranch = $orderedBranches[$candidate]
                $mergeBaseResult = Invoke-RoadmapGit -Repository $Repository -Arguments @('merge-base', $branch.Name, $otherBranch.Name)
                if (-not $mergeBaseResult.Success -or $mergeBaseResult.Lines.Count -eq 0) {
                    continue
                }

                $otherFirstParents = $laneFirstParentSets[$candidate]
                # Keep a fork before a branch's own commits, even when another lane later merges them in.
                $commonIndex = -1
                for ($chainIndex = 0; $chainIndex -lt $branchChain.Count; $chainIndex++) {
                    if ($otherFirstParents.Contains([string]$branchChain[$chainIndex].hash)) {
                        $commonIndex = $chainIndex
                        break
                    }
                }

                $candidateFork = ''
                $candidateDistance = [int]::MaxValue
                if ($commonIndex -ge 0) {
                    $oldestOwnCommit = -1
                    for ($chainIndex = 0; $chainIndex -lt $commonIndex; $chainIndex++) {
                        if (-not $branchChain[$chainIndex].merge) {
                            $oldestOwnCommit = $chainIndex
                        }
                    }

                    if ($oldestOwnCommit -lt 0) {
                        if ($commonIndex -gt 0) {
                            continue
                        }
                        $candidateFork = [string]$branchChain[$commonIndex].hash
                        $candidateDistance = $commonIndex
                    }
                    else {
                        $candidateFork = [string]$branchChain[$oldestOwnCommit].parent
                        for ($chainIndex = $oldestOwnCommit + 1; $chainIndex -lt $branchChain.Count; $chainIndex++) {
                            if ($branchChain[$chainIndex].hash -eq $candidateFork) {
                                $candidateDistance = $chainIndex
                                break
                            }
                        }
                        if ($candidateDistance -eq [int]::MaxValue -or -not $otherFirstParents.Contains($candidateFork)) {
                            continue
                        }
                    }
                }
                else {
                    $candidateFork = [string]$mergeBaseResult.Lines[0]
                    $distanceResult = Invoke-RoadmapGit -Repository $Repository -Arguments @('rev-list', '--count', ($candidateFork + '..' + $branch.Name))
                    if (-not $distanceResult.Success -or $distanceResult.Lines.Count -eq 0 -or -not [int]::TryParse($distanceResult.Lines[0], [ref]$candidateDistance)) {
                        continue
                    }
                }

                $candidateParent = -1
                for ($homeLaneIndex = 0; $homeLaneIndex -lt $laneSets.Count; $homeLaneIndex++) {
                    if ($homeLaneIndex -ne $laneIndex -and $laneSets[$homeLaneIndex].Contains($candidateFork)) {
                        $candidateParent = $homeLaneIndex
                        break
                    }
                }
                if ($candidateParent -lt 0) {
                    for ($homeLaneIndex = 0; $homeLaneIndex -lt $laneFirstParentSets.Count; $homeLaneIndex++) {
                        if ($homeLaneIndex -ne $laneIndex -and $laneFirstParentSets[$homeLaneIndex].Contains($candidateFork)) {
                            $candidateParent = $homeLaneIndex
                            break
                        }
                    }
                }
                if ($candidateParent -lt 0) {
                    continue
                }

                if ($candidateDistance -lt $bestDistance) {
                    $bestDistance = $candidateDistance
                    $forkHash = $candidateFork
                    $parentLane = $candidateParent
                }
            }
        }

        if (-not [string]::IsNullOrWhiteSpace($forkHash)) {
            $lanes[$laneIndex]['fork'] = Get-RoadmapShortHash $forkHash
        }
        if ($parentLane -ge 0) {
            $lanes[$laneIndex]['parent'] = $parentLane
        }
    }

    $graphCommits = [System.Collections.Generic.List[object]]::new()
    if (-not [string]::IsNullOrWhiteSpace($eraStart)) {
        $graphResult = Invoke-RoadmapGit -Repository $Repository -Arguments @(
            'log', '--topo-order', '--date-order', '--date=format:%Y-%m-%d %H:%M', '--format=%H%x1f%h%x1f%ad%x1f%s%x1f%P', '--branches', '--not', ($eraStart + '^')
        )
        if ($graphResult.Success) {
            $tagsByCommit = @{}
            foreach ($tag in $tagRefs) {
                if (-not $tagsByCommit.ContainsKey($tag.FullHash)) {
                    $tagsByCommit[$tag.FullHash] = [System.Collections.Generic.List[string]]::new()
                }
                [void]$tagsByCommit[$tag.FullHash].Add($tag.Name)
            }

            foreach ($line in $graphResult.Lines) {
                $parts = @($line.Split([char]31, 5))
                if ($parts.Count -lt 5) {
                    continue
                }

                $in = [System.Collections.Generic.List[int]]::new()
                for ($laneIndex = 0; $laneIndex -lt $laneSets.Count; $laneIndex++) {
                    if ($laneSets[$laneIndex].Contains($parts[0])) {
                        [void]$in.Add($laneIndex)
                    }
                }

                $commitTags = @()
                if ($tagsByCommit.ContainsKey($parts[0])) {
                    $commitTags = $tagsByCommit[$parts[0]].ToArray()
                }

                $parents = @()
                if (-not [string]::IsNullOrWhiteSpace($parts[4])) {
                    $parents = @($parts[4] -split '\s+' | ForEach-Object { Get-RoadmapShortHash $_ })
                }

                [void]$graphCommits.Add([ordered]@{
                    hash = $parts[1]
                    date = $parts[2]
                    subject = $parts[3]
                    in = $in.ToArray()
                    parents = $parents
                    tags = $commitTags
                })
            }
        }
        else {
            Add-RoadmapWarning -Warnings $Warnings -Message 'git: the branch graph could not be read.'
        }
    }

    return [ordered]@{
        log = $logRows.ToArray()
        worktrees = $worktrees.ToArray()
        branches = $branchRows.ToArray()
        tags = $tagRows
        main = $main
        devAhead = $devAhead
        devLine = $devLineRows.ToArray()
        graph = [ordered]@{ lanes = $lanes.ToArray(); commits = $graphCommits.ToArray() }
    }
}

function Get-RoadmapData {
    param([string]$RequestedRoot)

    $repository = Resolve-RoadmapRoot -RequestedRoot $RequestedRoot
    $warnings = [System.Collections.Generic.List[string]]::new()
    $statusPath = Join-Path $repository '.coord/STATUS.md'
    $statusLines = @()
    $statusText = ''
    $statusLoaded = $false
    try {
        if (-not (Test-Path -LiteralPath $statusPath -PathType Leaf)) {
            throw 'missing'
        }
        $statusText = Get-Content -LiteralPath $statusPath -Raw -ErrorAction Stop
        $statusLines = @($statusText -split "\r?\n")
        $statusLoaded = $true
    }
    catch {
        Add-RoadmapWarning -Warnings $warnings -Message '.coord/STATUS.md: file is missing or unreadable.'
    }

    $plan = Get-RoadmapPlanRows -Lines $statusLines -Warnings $warnings
    $nowSection = Get-RoadmapSection -Lines $statusLines -Heading 'Now'
    $whereSection = Get-RoadmapSection -Lines $statusLines -Heading 'Where things are'
    if ($statusLoaded -and [string]::IsNullOrWhiteSpace($nowSection)) {
        Add-RoadmapWarning -Warnings $warnings -Message 'STATUS.md: the Now section is missing or empty.'
    }
    if ($statusLoaded -and [string]::IsNullOrWhiteSpace($whereSection)) {
        Add-RoadmapWarning -Warnings $warnings -Message 'STATUS.md: the Where things are section is missing or empty.'
    }

    $erasPath = Join-Path $repository 'tools/roadmap/eras.json'
    if (-not (Test-Path -LiteralPath $erasPath -PathType Leaf)) {
        $erasPath = Join-Path $PSScriptRoot 'roadmap/eras.json'
    }
    $eras = @()
    try {
        if (-not (Test-Path -LiteralPath $erasPath -PathType Leaf)) {
            throw 'missing'
        }
        $eras = @(Get-Content -LiteralPath $erasPath -Raw -ErrorAction Stop | ConvertFrom-Json -AsHashtable -ErrorAction Stop)
    }
    catch {
        Add-RoadmapWarning -Warnings $warnings -Message 'tools/roadmap/eras.json: file is missing or unreadable.'
    }

    $updated = [DateTimeOffset]::Now.ToString('yyyy-MM-dd HH:mm:ss zzz')
    $usage = Get-RoadmapUsage -Path (Join-Path $repository '.coord/logs/usage.tsv') -Warnings $warnings
    $cards = Get-RoadmapCards -Repository $repository -Warnings $warnings
    $gitData = Get-RoadmapGitData -Repository $repository -Eras $eras -Warnings $warnings

    return [ordered]@{
        updated = $updated
        rows = $plan.Rows
        now = Get-RoadmapNowItems -Section $nowSection
        log = $gitData.log
        worktrees = $gitData.worktrees
        branches = $gitData.branches
        tags = $gitData.tags
        usage = $usage
        main = $gitData.main
        devAhead = $gitData.devAhead
        cards = $cards
        where = $whereSection
        devLine = $gitData.devLine
        graph = $gitData.graph
        eras = $eras
        warnings = $warnings.ToArray()
    }
}

function Get-RoadmapCheckResult {
    param(
        [Parameter(Mandatory)][System.Collections.IDictionary]$Data,
        [Parameter(Mandatory)][string]$Repository
    )

    $samplePath = Join-Path $PSScriptRoot '../.coord/tasks/T1-roadmap-overview.sample.json'
    if (-not (Test-Path -LiteralPath $samplePath -PathType Leaf)) {
        $samplePath = Join-Path $Repository '.coord/tasks/T1-roadmap-overview.sample.json'
    }

    try {
        $sample = Get-Content -LiteralPath $samplePath -Raw -ErrorAction Stop | ConvertFrom-Json -AsHashtable -ErrorAction Stop
    }
    catch {
        return [pscustomobject]@{ Success = $false; Reason = 'sample JSON is missing or unreadable.' }
    }

    foreach ($key in $sample.Keys) {
        if (-not $Data.Contains($key)) {
            return [pscustomobject]@{ Success = $false; Reason = ('sample key is missing: {0}.' -f $key) }
        }
    }

    $statusPath = Join-Path $Repository '.coord/STATUS.md'
    try {
        $lines = @(Get-Content -LiteralPath $statusPath -ErrorAction Stop)
    }
    catch {
        $lines = @()
    }
    $tableRows = Get-RoadmapPlanRows -Lines $lines -Warnings ([System.Collections.Generic.List[string]]::new())
    if ($Data.rows.Count -ne $tableRows.TableRows) {
        return [pscustomobject]@{ Success = $false; Reason = ('Plan row count differs from STATUS table ({0} != {1}).' -f $Data.rows.Count, $tableRows.TableRows) }
    }

    return [pscustomobject]@{ Success = $true; Reason = '' }
}

if ($MyInvocation.InvocationName -ne '.') {
    $resolvedRoot = Resolve-RoadmapRoot -RequestedRoot $RequestedRoot
    $data = Get-RoadmapData -RequestedRoot $resolvedRoot
    if (-not [string]::IsNullOrWhiteSpace($Snapshot)) {
        try {
            $pagePath = Join-Path $PSScriptRoot 'roadmap/index.html'
            $snapshotPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Snapshot)
            if ([string]::Equals($pagePath, $snapshotPath, [System.StringComparison]::OrdinalIgnoreCase)) {
                throw 'The snapshot path cannot replace the live roadmap page.'
            }

            $marker = '<!-- ROADMAP_DATA_SLOT -->'
            $page = Get-Content -LiteralPath $pagePath -Raw -ErrorAction Stop
            if ($page.IndexOf($marker, [System.StringComparison]::Ordinal) -lt 0 -or
                $page.IndexOf($marker, [System.StringComparison]::Ordinal) -ne $page.LastIndexOf($marker, [System.StringComparison]::Ordinal)) {
                throw 'The roadmap page must contain one data slot.'
            }

            $json = ConvertTo-Json -InputObject $data -Depth 100 -Compress
            $json = $json.Replace('<', '\u003c').Replace([string][char]0x2028, '\u2028').Replace([string][char]0x2029, '\u2029')
            $dataScript = '<script>window.ROADMAP_DATA = ' + $json + ';</script>'
            $page = $page.Replace($marker, $dataScript)
            [System.IO.File]::WriteAllText($snapshotPath, $page, [System.Text.UTF8Encoding]::new($false))
            Write-Output ('Wrote roadmap snapshot: {0}' -f $snapshotPath)
        }
        catch {
            Write-Output ('ROADMAP SNAPSHOT FAIL: {0}' -f $_.Exception.Message)
            exit 1
        }
    }
    elseif ($Check) {
        $checkResult = Get-RoadmapCheckResult -Data $data -Repository $resolvedRoot
        if (-not $checkResult.Success) {
            Write-Output ('ROADMAP DATA FAIL: {0}' -f $checkResult.Reason)
            exit 1
        }

        $groups = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
        foreach ($row in $data.rows) {
            if (-not [string]::IsNullOrWhiteSpace([string]$row.group)) {
                [void]$groups.Add([string]$row.group)
            }
        }
        $groupCount = $groups.Count
        Write-Output 'ROADMAP DATA OK'
        Write-Output ('rows={0} groups={1} log={2} lanes={3} worktrees={4} tags={5} cards={6} warnings={7}' -f `
            $data.rows.Count, $groupCount, $data.log.Count, $data.graph.lanes.Count, $data.worktrees.Count, $data.tags.Count, $data.cards.Count, $data.warnings.Count)
    }
    else {
        ConvertTo-Json -InputObject $data -Depth 100 -Compress
    }
}
