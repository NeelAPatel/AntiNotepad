[CmdletBinding()]
param(
    [switch]$DryRun,
    [string]$InstallRoot,
    [switch]$TestFailSelfTest
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$propsPath = Join-Path $repoRoot 'product\AntiNotepad\AppInfo.props'
if (-not (Test-Path -LiteralPath $propsPath -PathType Leaf)) { throw "App identity file not found: $propsPath" }
[xml]$props = [System.IO.File]::ReadAllText($propsPath)
$appName = [string]$props.Project.PropertyGroup.AppName
$appVersion = [string]$props.Project.PropertyGroup.AppVersion
if ($appName -notmatch '^[A-Za-z0-9][A-Za-z0-9.-]*$') { throw "Unsafe AppName in AppInfo.props: '$appName'" }
$envPrefix = $appName.ToUpperInvariant() + '_'
$localAppData = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
if ([string]::IsNullOrWhiteSpace($localAppData)) { throw 'Windows did not provide a Local AppData path.' }
$buildDirectory = Join-Path $repoRoot 'build'
$testMode = -not [string]::IsNullOrWhiteSpace($InstallRoot)

function Assert-DirectChildPath([string]$Path, [string]$Parent, [string]$ExpectedLeaf) {
    $fullPath = [IO.Path]::GetFullPath($Path)
    $fullParent = [IO.Path]::GetFullPath($Parent).TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
    if (-not [string]::Equals([IO.Path]::GetDirectoryName($fullPath), $fullParent, [StringComparison]::OrdinalIgnoreCase) -or
        -not [string]::Equals([IO.Path]::GetFileName($fullPath), $ExpectedLeaf, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing filesystem operation outside the expected direct child path: $fullPath"
    }
}

function Assert-PlainDirectory([string]$Path, [string]$Label) {
    if (-not (Test-Path -LiteralPath $Path)) { return }
    $item = Get-Item -LiteralPath $Path -Force
    if (-not $item.PSIsContainer -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw "Refusing to use $Label because it is not a plain directory: $Path"
    }
}

if ($testMode) {
    if (Test-Path -LiteralPath $buildDirectory) { Assert-PlainDirectory $buildDirectory 'repository build directory' }
    $installDirectory = [IO.Path]::GetFullPath($InstallRoot)
    $installLeaf = [IO.Path]::GetFileName($installDirectory)
    Assert-DirectChildPath $installDirectory $buildDirectory $installLeaf
    if ($installLeaf -notmatch '^A16-staged-upgrade-rollback-[A-Za-z0-9._-]+$') {
        throw 'Test InstallRoot must be a direct child of this repo build folder named A16-staged-upgrade-rollback-<suffix>.'
    }
}
else {
    $installDirectory = Join-Path (Join-Path $localAppData 'Programs') $appName
    $installLeaf = $appName
}

if ($TestFailSelfTest -and -not $testMode) { throw '-TestFailSelfTest requires the safe -InstallRoot test mode.' }
if ($testMode -and $DryRun -and $TestFailSelfTest) { throw '-DryRun cannot be combined with -TestFailSelfTest.' }

$installedExe = Join-Path $installDirectory "$appName.exe"
$dataDirectory = Join-Path $localAppData $appName
$programsDirectory = if ($testMode) { $null } else { [Environment]::GetFolderPath([Environment+SpecialFolder]::Programs) }
$shortcutPath = if ($testMode) { $null } else { Join-Path $programsDirectory "$appName.lnk" }
$runKeyPath = 'Software\Microsoft\Windows\CurrentVersion\Run'

if ($DryRun) {
    return [pscustomobject]@{
        Task = 'Install'
        Product = $appName
        Version = $appVersion
        InstallDirectory = $installDirectory
        DataDirectory = $dataDirectory
        StartMenuShortcut = $shortcutPath
        Publish = "dotnet publish product/AntiNotepad/$appName.csproj -c Release -o build/<unique-install-stage> -r win-x64 --self-contained false"
        Plan = 'Publish to a repository staging directory; on upgrade request a graceful quit, move the current install to a sibling backup, copy and self-test the new files, then restore the backup on any later failure. Keep only the latest successful backup.'
        TestMode = $testMode
        TestModeSkips = 'When -InstallRoot is used: installed process checks and --quit, Start menu shortcut, Run key access or changes, and app launch.'
        FirstTransition = 'Before first transition, save and exit any legacy Antipad or repo copy through its tray menu. This installer controls only the canonical installed AntiNotepad copy. DryRun does not inspect processes or the registry.'
    }
}

if ([string]::IsNullOrWhiteSpace($programsDirectory) -and -not $testMode) { throw 'Windows did not provide a Start menu Programs path.' }

function New-CleanAppProcessInfo([string]$Executable, [string[]]$Arguments, [bool]$CaptureOutput) {
    $info = [System.Diagnostics.ProcessStartInfo]::new()
    $info.FileName = $Executable
    $info.WorkingDirectory = Split-Path -Parent $Executable
    $info.UseShellExecute = $false
    $info.CreateNoWindow = $CaptureOutput
    $info.RedirectStandardOutput = $CaptureOutput
    $info.RedirectStandardError = $CaptureOutput
    foreach ($key in @($info.Environment.Keys)) {
        if ($key.StartsWith($envPrefix, [StringComparison]::OrdinalIgnoreCase)) { [void]$info.Environment.Remove($key) }
    }
    $info.Environment['AVALONIA_TELEMETRY_OPTOUT'] = '1'
    foreach ($argument in $Arguments) { [void]$info.ArgumentList.Add($argument) }
    return $info
}

function Invoke-InstalledCommand([string]$Executable, [string]$Argument, [int]$TimeoutSeconds) {
    $info = New-CleanAppProcessInfo $Executable @($Argument) $true
    $process = [System.Diagnostics.Process]::Start($info)
    if ($null -eq $process) { throw "Could not start $Executable $Argument." }
    try {
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
            throw "$Executable $Argument did not exit within $TimeoutSeconds seconds. No process was force-stopped; installation was left incomplete."
        }
        return [pscustomobject]@{
            ExitCode = $process.ExitCode
            StandardOutput = $stdout.GetAwaiter().GetResult()
            StandardError = $stderr.GetAwaiter().GetResult()
        }
    }
    finally { $process.Dispose() }
}

function Get-InstalledRunningState([string]$Executable) {
    $expected = [IO.Path]::GetFullPath($Executable)
    foreach ($process in @(Get-Process -Name $appName -ErrorAction SilentlyContinue)) {
        try { $path = $process.Path }
        catch { throw "Could not verify whether $appName process $($process.Id) is running from the installed path." }
        if ([string]::IsNullOrWhiteSpace($path)) { throw "Could not verify the executable path for $appName process $($process.Id)." }
        if ([string]::Equals([IO.Path]::GetFullPath($path), $expected, [StringComparison]::OrdinalIgnoreCase)) { return $true }
    }
    return $false
}

function Get-RunValueState {
    $state = [pscustomobject]@{ Exists = $false; Value = $null; Kind = [Microsoft.Win32.RegistryValueKind]::String }
    $runKey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($runKeyPath, $false)
    if ($null -eq $runKey) { return $state }
    try {
        if ($null -ne $runKey.GetValue($appName, $null, [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)) {
            $state.Exists = $true
            $state.Value = $runKey.GetValue($appName, $null, [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)
            $state.Kind = $runKey.GetValueKind($appName)
        }
        return $state
    }
    finally { $runKey.Dispose() }
}

function Restore-RunValueState($State) {
    $runKey = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey($runKeyPath)
    try {
        if ($State.Exists) { $runKey.SetValue($appName, $State.Value, $State.Kind) }
        else { $runKey.DeleteValue($appName, $false) }
    }
    finally { $runKey.Dispose() }
}

function Set-StartMenuShortcut([string]$Executable) {
    $shell = New-Object -ComObject WScript.Shell
    try {
        $shortcut = $shell.CreateShortcut($shortcutPath)
        $shortcut.TargetPath = $Executable
        $shortcut.WorkingDirectory = Split-Path -Parent $Executable
        $shortcut.Description = $appName
        $shortcut.IconLocation = "$Executable,0"
        $shortcut.Save()
    }
    finally { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($shell) }
}

function Get-NotesDisplayPath {
    if ($testMode) { return $dataDirectory }
    try {
        $pointerPath = Join-Path $dataDirectory 'location.json'
        if (Test-Path -LiteralPath $pointerPath -PathType Leaf) {
            $location = [System.IO.File]::ReadAllText($pointerPath) | ConvertFrom-Json -ErrorAction Stop
            $candidate = [string]$location.NotesFolder
            if (-not [string]::IsNullOrWhiteSpace($candidate) -and [IO.Path]::IsPathFullyQualified($candidate)) {
                return [IO.Path]::GetFullPath($candidate)
            }
        }
    }
    catch { }
    return $dataDirectory
}

function Remove-InstallDirectory([string]$Path, [string]$Parent, [string]$Leaf) {
    if (-not (Test-Path -LiteralPath $Path)) { return }
    Assert-DirectChildPath $Path $Parent $Leaf
    Assert-PlainDirectory $Path 'install directory'
    Remove-Item -LiteralPath $Path -Recurse -Force
}

$guid = [guid]::NewGuid().ToString('N')
$stamp = [DateTime]::Now.ToString('yyyyMMdd-HHmmssfff')
$stageDirectory = Join-Path $buildDirectory "$appName-install-stage-$guid"
$stageExe = Join-Path $stageDirectory "$appName.exe"
$installParent = Split-Path -Parent $installDirectory
$backupLeaf = "$installLeaf.previous-$stamp-$guid"
$backupDirectory = Join-Path $installParent $backupLeaf
$installedBefore = Test-Path -LiteralPath $installDirectory
$startupEnabled = $false
$wasRunningBefore = $false
$startupState = $null
$shortcutExistedBefore = $false
$savedShortcutPath = Join-Path $stageDirectory "$appName.previous-shortcut.lnk"
$shortcutChanged = $false
$runValueChanged = $false
$swapStarted = $false
$backupMoved = $false

if ($installedBefore) {
    Assert-DirectChildPath $installDirectory $installParent $installLeaf
    Assert-PlainDirectory $installDirectory 'existing install directory'
    if (-not (Test-Path -LiteralPath $installedExe -PathType Leaf)) { throw "The install folder exists but $appName.exe is missing: $installDirectory" }
}

try {
    if (-not $installedBefore -and -not $testMode) {
        Write-Warning 'Before running this installer for the first transition, save and exit any legacy Antipad or repo copy through its tray menu.'
    }
    New-Item -ItemType Directory -Path $buildDirectory -Force | Out-Null
    $previousTelemetry = [Environment]::GetEnvironmentVariable('AVALONIA_TELEMETRY_OPTOUT', 'Process')
    [Environment]::SetEnvironmentVariable('AVALONIA_TELEMETRY_OPTOUT', '1', 'Process')
    try {
        & dotnet publish (Join-Path $repoRoot "product\AntiNotepad\$appName.csproj") -c Release -o $stageDirectory -r win-x64 --self-contained false
        $publishExitCode = $LASTEXITCODE
    }
    finally { [Environment]::SetEnvironmentVariable('AVALONIA_TELEMETRY_OPTOUT', $previousTelemetry, 'Process') }
    if ($publishExitCode -ne 0) { throw "dotnet publish failed with exit code $publishExitCode." }
    if (-not (Test-Path -LiteralPath $stageExe -PathType Leaf)) { throw "Publish did not produce $stageExe." }

    if (-not $testMode) {
        $startupState = Get-RunValueState
        $startupEnabled = $startupState.Exists
        if ($installedBefore) {
            $wasRunningBefore = Get-InstalledRunningState $installedExe
            $quit = Invoke-InstalledCommand $installedExe '--quit' 15
            if ($quit.ExitCode -ne 0) {
                $detail = if ($quit.StandardError) { $quit.StandardError.Trim() } else { 'The resident copy did not confirm shutdown.' }
                throw "Could not safely upgrade the installed copy: $detail"
            }
        }
        if (Test-Path -LiteralPath $shortcutPath -PathType Leaf) {
            $shortcutExistedBefore = $true
        }
    }

    $swapStarted = $true
    if ($installedBefore) {
        Assert-DirectChildPath $backupDirectory $installParent $backupLeaf
        Move-Item -LiteralPath $installDirectory -Destination $backupDirectory
        $backupMoved = $true
    }
    else {
        New-Item -ItemType Directory -Path $installDirectory -Force | Out-Null
    }

    Get-ChildItem -LiteralPath $stageDirectory -Force | Copy-Item -Destination $installDirectory -Recurse -Force

    if ($TestFailSelfTest) { throw 'Simulated installed selftest failure requested by -TestFailSelfTest.' }
    $selftest = Invoke-InstalledCommand $installedExe '--selftest' 60
    if ($selftest.ExitCode -ne 0 -or $selftest.StandardOutput -notmatch 'SELFTEST PASS') {
        $detail = ($selftest.StandardOutput + $selftest.StandardError).Trim()
        throw "Installed selftest failed (exit $($selftest.ExitCode)): $detail"
    }
    Write-Output $selftest.StandardOutput.Trim()

    if (-not $testMode) {
        if ($shortcutExistedBefore) { Copy-Item -LiteralPath $shortcutPath -Destination $savedShortcutPath -Force }
        $shortcutChanged = $true
        Set-StartMenuShortcut $installedExe
        if ($startupEnabled) {
            $runValueChanged = $true
            $runKey = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey($runKeyPath)
            try { $runKey.SetValue($appName, "`"$installedExe`" --hidden", [Microsoft.Win32.RegistryValueKind]::String) }
            finally { $runKey.Dispose() }
        }
    }

    if ($backupMoved) {
        $olderBackups = @(Get-ChildItem -LiteralPath $installParent -Directory -Filter "$installLeaf.previous-*" -Force |
            Where-Object { -not [string]::Equals($_.FullName, $backupDirectory, [StringComparison]::OrdinalIgnoreCase) })
        foreach ($olderBackup in $olderBackups) {
            $olderLeaf = [IO.Path]::GetFileName($olderBackup.FullName)
            Remove-InstallDirectory $olderBackup.FullName $installParent $olderLeaf
        }
    }

    if (-not $testMode) {
        $launchInfo = New-CleanAppProcessInfo $installedExe @() $false
        $launch = [System.Diagnostics.Process]::Start($launchInfo)
        if ($null -eq $launch) { throw "Installed and self-tested $appName, but could not launch it." }
        $launch.Dispose()
    }

    $notesDisplayPath = Get-NotesDisplayPath
    if ($backupMoved) {
        Write-Output "$appName $appVersion installed at $installDirectory. Latest previous version is backed up at $backupDirectory. Notes are at $notesDisplayPath."
    }
    elseif ($testMode) {
        Write-Output "$appName $appVersion installed and self-tested at $installDirectory. Test mode skipped app launch and user integration."
    }
    else {
        Write-Output "$appName $appVersion installed, self-tested, and launched at $installDirectory. Notes are at $notesDisplayPath."
    }
}
catch {
    $failure = $_.Exception.Message
    $rollbackMessages = [System.Collections.Generic.List[string]]::new()
    if ($swapStarted) {
        if (-not $backupMoved -and $installedBefore -and (Test-Path -LiteralPath $backupDirectory) -and -not (Test-Path -LiteralPath $installDirectory)) {
            $backupMoved = $true
        }
        $previousFilesRestored = $false
        if ($backupMoved) {
            try {
                if (Test-Path -LiteralPath $installDirectory) {
                    Remove-InstallDirectory $installDirectory $installParent $installLeaf
                }
                Assert-DirectChildPath $backupDirectory $installParent $backupLeaf
                Assert-PlainDirectory $backupDirectory 'previous install backup'
                Move-Item -LiteralPath $backupDirectory -Destination $installDirectory
                $backupMoved = $false
                $previousFilesRestored = $true
                $rollbackMessages.Add("Previous files were restored to $installDirectory.")
            }
            catch { $rollbackMessages.Add("Previous files could not be restored: $($_.Exception.Message)") }
        }
        elseif (-not $installedBefore) {
            try {
                Remove-InstallDirectory $installDirectory $installParent $installLeaf
                $rollbackMessages.Add('The partial first install was removed.')
            }
            catch { $rollbackMessages.Add("The partial first install could not be removed: $($_.Exception.Message)") }
        }
        else {
            $previousFilesRestored = Test-Path -LiteralPath $installedExe -PathType Leaf  # never moved, so the old files are still in place
            $rollbackMessages.Add('The existing install was left in place; its move to backup did not complete.')
        }

        if ($runValueChanged -and $null -ne $startupState) {
            try { Restore-RunValueState $startupState; $rollbackMessages.Add('The previous Run key value was restored.') }
            catch { $rollbackMessages.Add("The Run key value could not be restored: $($_.Exception.Message)") }
        }
        if ($shortcutChanged) {
            try {
                if ($shortcutExistedBefore -and (Test-Path -LiteralPath $savedShortcutPath -PathType Leaf)) {
                    Copy-Item -LiteralPath $savedShortcutPath -Destination $shortcutPath -Force
                }
                elseif (-not $shortcutExistedBefore -and (Test-Path -LiteralPath $shortcutPath -PathType Leaf)) {
                    Remove-Item -LiteralPath $shortcutPath -Force
                }
                $rollbackMessages.Add('The previous Start menu shortcut state was restored.')
            }
            catch { $rollbackMessages.Add("The Start menu shortcut could not be restored: $($_.Exception.Message)") }
        }
        if ($previousFilesRestored -and $wasRunningBefore -and -not $testMode) {
            try {
                $oldLaunch = [System.Diagnostics.Process]::Start((New-CleanAppProcessInfo $installedExe @() $false))
                if ($null -eq $oldLaunch) { throw 'Process start returned no process.' }
                $oldLaunch.Dispose()
                $rollbackMessages.Add('The previous app was running before the upgrade and has been relaunched.')
            }
            catch { $rollbackMessages.Add("Previous files were restored, but the old app could not be relaunched: $($_.Exception.Message)") }
        }
        elseif ($previousFilesRestored) { $rollbackMessages.Add('The previous app was not running before the upgrade, so it was left stopped.') }
    }
    [Console]::Error.WriteLine("$appName installation failed: $failure")
    foreach ($message in $rollbackMessages) { [Console]::Error.WriteLine($message) }
    exit 1
}
finally {
    if (Test-Path -LiteralPath $stageDirectory) {
        try {
            $stageLeaf = [IO.Path]::GetFileName($stageDirectory)
            Assert-DirectChildPath $stageDirectory $buildDirectory $stageLeaf
            Assert-PlainDirectory $stageDirectory 'staged publish directory'
            Remove-Item -LiteralPath $stageDirectory -Recurse -Force
        }
        catch { [Console]::Error.WriteLine("Could not remove the staged publish directory $stageDirectory`: $($_.Exception.Message)") }
    }
}
