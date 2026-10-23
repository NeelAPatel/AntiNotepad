[CmdletBinding()]
param(
    [switch]$RemoveData,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$propsPath = Join-Path $repoRoot 'product\AntiNotepad\AppInfo.props'
if (-not (Test-Path -LiteralPath $propsPath -PathType Leaf)) { throw "App identity file not found: $propsPath" }
[xml]$props = [System.IO.File]::ReadAllText($propsPath)
$appName = [string]$props.Project.PropertyGroup.AppName
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
        Task = 'Uninstall'
        Product = $appName
        InstallDirectory = $installDirectory
        StartMenuShortcut = $shortcutPath
        RemoveRunValue = $appName
        RemoveData = [bool]$RemoveData
        DataDirectory = if ($RemoveData) { $dataDirectory } else { '(preserved)' }
        Plan = 'Request a graceful quit from the installed executable and wait with a timeout; remove the install folder, Start menu shortcut, and product Run value.'
    }
}

function New-CleanAppProcessInfo([string]$Executable, [string[]]$Arguments) {
    $info = [System.Diagnostics.ProcessStartInfo]::new()
    $info.FileName = $Executable
    $info.WorkingDirectory = Split-Path -Parent $Executable
    $info.UseShellExecute = $false
    $info.CreateNoWindow = $true
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    foreach ($key in @($info.Environment.Keys)) {
        if ($key.StartsWith($envPrefix, [StringComparison]::OrdinalIgnoreCase)) { [void]$info.Environment.Remove($key) }
    }
    $info.Environment['AVALONIA_TELEMETRY_OPTOUT'] = '1'
    foreach ($argument in $Arguments) { [void]$info.ArgumentList.Add($argument) }
    return $info
}

function Invoke-InstalledQuit([string]$Executable, [int]$TimeoutSeconds) {
    $process = [System.Diagnostics.Process]::Start((New-CleanAppProcessInfo $Executable @('--quit')))
    if ($null -eq $process) { throw "Could not start $Executable --quit." }
    try {
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
            throw "$Executable --quit did not exit within $TimeoutSeconds seconds. No process was force-stopped; uninstall was cancelled."
        }
        if ($process.ExitCode -ne 0) {
            $detail = $stderr.GetAwaiter().GetResult()
            throw "The installed copy did not confirm shutdown (exit $($process.ExitCode)): $detail"
        }
        [void]$stdout.GetAwaiter().GetResult()
        [void]$stderr.GetAwaiter().GetResult()
    }
    finally { $process.Dispose() }
}

function Remove-ProductRunValue {
    $runKey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($runKeyPath, $true)
    if ($null -eq $runKey) { return }
    try { $runKey.DeleteValue($appName, $false) }
    finally { $runKey.Dispose() }
}

function Assert-PlainDirectory([string]$Path, [string]$Label) {
    if (-not (Test-Path -LiteralPath $Path)) { return }
    $item = Get-Item -LiteralPath $Path -Force
    if (-not $item.PSIsContainer -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw "Refusing to remove $Label because it is not a plain directory: $Path"
    }
}

function Assert-DirectChildPath([string]$Path, [string]$Parent, [string]$ExpectedLeaf) {
    $fullPath = [IO.Path]::GetFullPath($Path)
    $fullParent = [IO.Path]::GetFullPath($Parent).TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
    if (-not [string]::Equals([IO.Path]::GetDirectoryName($fullPath), $fullParent, [StringComparison]::OrdinalIgnoreCase) -or
        -not [string]::Equals([IO.Path]::GetFileName($fullPath), $ExpectedLeaf, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing filesystem operation outside the expected direct child path: $fullPath"
    }
}

$fullInstallPath = [IO.Path]::GetFullPath($installDirectory).TrimEnd('\')
$fullDataPath = [IO.Path]::GetFullPath($dataDirectory).TrimEnd('\')
if ([string]::Equals($fullInstallPath, $fullDataPath, [StringComparison]::OrdinalIgnoreCase)) { throw 'Install and data paths unexpectedly resolve to the same directory.' }

try {
    if (Test-Path -LiteralPath $installDirectory) {
        Assert-DirectChildPath $installDirectory (Join-Path $localAppData 'Programs') $appName
        Assert-PlainDirectory $installDirectory 'install folder'
        if (-not (Test-Path -LiteralPath $installedExe -PathType Leaf)) {
            throw "The install folder exists but $appName.exe is missing. Close any resident copy manually before removing files."
        }
        Invoke-InstalledQuit $installedExe 15
    }

    Remove-ProductRunValue
    if (Test-Path -LiteralPath $shortcutPath) { Remove-Item -LiteralPath $shortcutPath -Force }
    if (Test-Path -LiteralPath $installDirectory) {
        Assert-DirectChildPath $installDirectory (Join-Path $localAppData 'Programs') $appName
        Remove-Item -LiteralPath $installDirectory -Recurse -Force
    }

    if ($RemoveData -and (Test-Path -LiteralPath $dataDirectory)) {
        Assert-DirectChildPath $dataDirectory $localAppData $appName
        Assert-PlainDirectory $dataDirectory 'data folder'
        Remove-Item -LiteralPath $dataDirectory -Recurse -Force
        Write-Output "$appName uninstalled; application data was removed from $dataDirectory."
    }
    else {
        Write-Output "$appName uninstalled; application data remains at $dataDirectory."
    }
}
catch {
    Write-Error $_
    exit 1
}
