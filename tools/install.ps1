[CmdletBinding()]
param([switch]$DryRun)

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
$installDirectory = Join-Path (Join-Path $localAppData 'Programs') $appName
$installedExe = Join-Path $installDirectory "$appName.exe"
$dataDirectory = Join-Path $localAppData $appName
$programsDirectory = [Environment]::GetFolderPath([Environment+SpecialFolder]::Programs)
$shortcutPath = Join-Path $programsDirectory "$appName.lnk"
$runKeyPath = 'Software\Microsoft\Windows\CurrentVersion\Run'

if ($DryRun) {
    return [pscustomobject]@{
        Task = 'Install'
        Product = $appName
        Version = $appVersion
        InstallDirectory = $installDirectory
        DataDirectory = $dataDirectory
        StartMenuShortcut = $shortcutPath
        Publish = "dotnet publish product/AntiNotepad/$appName.csproj -c Release -o build/<unique-install-stage> --self-contained false"
        Plan = 'Publish to a repository staging directory; on upgrade request a graceful quit from the installed executable; copy staged files; run the installed --selftest; create the Start menu shortcut; retarget the Run entry only when it was enabled; launch the installed app. Preserve the separate data directory.'
        FirstTransition = 'Before first transition, save and exit any legacy Antipad or repo copy through its tray menu. This installer controls only the canonical installed AntiNotepad copy. DryRun does not inspect processes or the registry.'
    }
}

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

function Assert-DirectChildPath([string]$Path, [string]$Parent, [string]$ExpectedLeaf) {
    $fullPath = [IO.Path]::GetFullPath($Path)
    $fullParent = [IO.Path]::GetFullPath($Parent).TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
    if (-not [string]::Equals([IO.Path]::GetDirectoryName($fullPath), $fullParent, [StringComparison]::OrdinalIgnoreCase) -or
        -not [string]::Equals([IO.Path]::GetFileName($fullPath), $ExpectedLeaf, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing filesystem operation outside the expected direct child path: $fullPath"
    }
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

$guid = [guid]::NewGuid().ToString('N')
$buildDirectory = Join-Path $repoRoot 'build'
$stageDirectory = Join-Path $buildDirectory "$appName-install-stage-$guid"
$stageExe = Join-Path $stageDirectory "$appName.exe"
$installedBefore = Test-Path -LiteralPath $installedExe -PathType Leaf
$startupEnabled = $false
$swappedFiles = $false

try {
    if (-not $installedBefore) {
        Write-Warning 'Before running this installer for the first transition, save and exit any legacy Antipad or repo copy through its tray menu.'
    }
    New-Item -ItemType Directory -Path $buildDirectory -Force | Out-Null
    $previousTelemetry = [Environment]::GetEnvironmentVariable('AVALONIA_TELEMETRY_OPTOUT', 'Process')
    [Environment]::SetEnvironmentVariable('AVALONIA_TELEMETRY_OPTOUT', '1', 'Process')
    try {
        & dotnet publish (Join-Path $repoRoot "product\AntiNotepad\$appName.csproj") -c Release -o $stageDirectory --self-contained false
        $publishExitCode = $LASTEXITCODE
    }
    finally { [Environment]::SetEnvironmentVariable('AVALONIA_TELEMETRY_OPTOUT', $previousTelemetry, 'Process') }
    if ($publishExitCode -ne 0) { throw "dotnet publish failed with exit code $publishExitCode." }
    if (-not (Test-Path -LiteralPath $stageExe -PathType Leaf)) { throw "Publish did not produce $stageExe." }

    $runKey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($runKeyPath, $false)
    if ($null -ne $runKey) {
        try { $startupEnabled = $null -ne $runKey.GetValue($appName, $null, [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames) }
        finally { $runKey.Dispose() }
    }

    if ($installedBefore) {
        $quit = Invoke-InstalledCommand $installedExe '--quit' 15
        if ($quit.ExitCode -ne 0) {
            $detail = if ($quit.StandardError) { $quit.StandardError.Trim() } else { 'The resident copy did not confirm shutdown.' }
            throw "Could not safely upgrade the installed copy: $detail"
        }
    }

    New-Item -ItemType Directory -Path $installDirectory -Force | Out-Null
    $swappedFiles = $true
    Get-ChildItem -LiteralPath $stageDirectory -Force | Copy-Item -Destination $installDirectory -Recurse -Force

    $selftest = Invoke-InstalledCommand $installedExe '--selftest' 60
    if ($selftest.ExitCode -ne 0 -or $selftest.StandardOutput -notmatch 'SELFTEST PASS') {
        $detail = ($selftest.StandardOutput + $selftest.StandardError).Trim()
        throw "Installed selftest failed (exit $($selftest.ExitCode)): $detail"
    }
    Write-Output $selftest.StandardOutput.Trim()

    Set-StartMenuShortcut $installedExe
    if ($startupEnabled) {
        $runKey = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey($runKeyPath)
        try { $runKey.SetValue($appName, "`"$installedExe`" --hidden", [Microsoft.Win32.RegistryValueKind]::String) }
        finally { $runKey.Dispose() }
    }

    $launchInfo = New-CleanAppProcessInfo $installedExe @() $false
    $launch = [System.Diagnostics.Process]::Start($launchInfo)
    if ($null -eq $launch) { throw "Installed and self-tested $appName, but could not launch it." }
    $launch.Dispose()
    if ($installedBefore) {
        Write-Output "$appName $appVersion installed at $installDirectory and launched. Data remains at $dataDirectory."
    }
    else {
        Write-Output "$appName $appVersion installed, self-tested, and launched at $installDirectory. Data remains at $dataDirectory."
    }
}
catch {
    if ($swappedFiles) {
        Write-Error "Installation stopped after copying files. Review $installDirectory before launching: $($_.Exception.Message)"
    }
    else { Write-Error $_ }
    exit 1
}
finally {
    if (Test-Path -LiteralPath $stageDirectory) {
        Assert-DirectChildPath $stageDirectory $buildDirectory "$appName-install-stage-$guid"
        $stageItem = Get-Item -LiteralPath $stageDirectory -Force
        if ($stageItem.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Refusing to remove staged reparse point: $stageDirectory" }
        Remove-Item -LiteralPath $stageDirectory -Recurse -Force
    }
}
