# Serves the live roadmap page and its read-only JSON data on localhost.
# The data builder is reloaded when its script changes.
[CmdletBinding()]
param(
    [ValidateRange(1, 65535)][int]$Port = 47811,
    [string]$Root,
    [string]$Label,
    [switch]$Open,
    [switch]$Stop,
    [switch]$Background
)

$ErrorActionPreference = 'Stop'
$dataScript = Join-Path $PSScriptRoot 'roadmap-data.ps1'
. $dataScript -RequestedRoot $Root
$repository = Resolve-RoadmapRoot -RequestedRoot $Root
$commonDirectory = Invoke-RoadmapGit -Repository (Join-Path $PSScriptRoot '..') -Arguments @('rev-parse', '--path-format=absolute', '--git-common-dir')
$prefsPath = if ($commonDirectory.Success -and -not [string]::IsNullOrWhiteSpace($commonDirectory.Text)) {
    Join-Path ([System.IO.Directory]::GetParent($commonDirectory.Text.Trim()).FullName) '.coord/roadmap-prefs.json'
} else { $null }
$dataScriptStamp = Get-Item -LiteralPath $dataScript -ErrorAction Stop | ForEach-Object { '{0}|{1}' -f $_.Length, $_.LastWriteTimeUtc.Ticks }
$serveScript = if (-not [string]::IsNullOrWhiteSpace($PSCommandPath)) { $PSCommandPath } else { Join-Path $PSScriptRoot 'roadmap-serve.ps1' }
$serveStamp = Get-Item -LiteralPath $serveScript -ErrorAction Stop | ForEach-Object { '{0}|{1}' -f $_.Length, $_.LastWriteTimeUtc.Ticks }
$baseUrl = 'http://localhost:{0}/' -f $Port
$healthUrl = $baseUrl + 'health'
$stopUrl = $baseUrl + 'stop'

function New-RoadmapHttpClient {
    $handler = [System.Net.Http.HttpClientHandler]::new()
    $handler.UseProxy = $false
    $client = [System.Net.Http.HttpClient]::new($handler)
    $client.Timeout = [TimeSpan]::FromSeconds(1)
    return $client
}

function Get-RoadmapHealth {
    param(
        [Parameter(Mandatory)][System.Net.Http.HttpClient]$Client,
        [Parameter(Mandatory)][string]$Url
    )

    $response = $null
    try {
        $response = $Client.GetAsync($Url).GetAwaiter().GetResult()
        if (-not $response.IsSuccessStatusCode) {
            return $null
        }

        $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        return $body | ConvertFrom-Json -AsHashtable -ErrorAction Stop
    }
    catch {
        return $null
    }
    finally {
        if ($null -ne $response) {
            $response.Dispose()
        }
    }
}

function Test-RoadmapHealth {
    param(
        [Parameter(Mandatory)][System.Net.Http.HttpClient]$Client,
        [Parameter(Mandatory)][string]$Url
    )

    $health = Get-RoadmapHealth -Client $Client -Url $Url
    return ($null -ne $health -and $health.app -eq 'antinotepad-roadmap')
}

function Write-RoadmapServerLog {
    param(
        [Parameter(Mandatory)][string]$RepositoryRoot,
        [Parameter(Mandatory)][string]$Message
    )

    try {
        $logFolder = Join-Path $RepositoryRoot 'build'
        [void](New-Item -ItemType Directory -Path $logFolder -Force)
        $logPath = Join-Path $logFolder 'roadmap-serve.log'
        $line = '{0} {1}' -f [DateTimeOffset]::Now.ToString('yyyy-MM-dd HH:mm:ss zzz'), $Message
        Add-Content -LiteralPath $logPath -Value $line -Encoding utf8NoBOM
    }
    catch {
        # Logging is best-effort; the server does not depend on a writable log folder.
    }
}

function Get-RoadmapFingerprint {
    param([Parameter(Mandatory)][string]$RepositoryRoot)

    $parts = [System.Collections.Generic.List[string]]::new()
    $statusPath = Join-Path $RepositoryRoot '.coord/STATUS.md'
    $pendingPath = Join-Path $RepositoryRoot '.coord/pending.md'
    $archivePath = Join-Path $RepositoryRoot '.coord/status-archive.md'
    $usagePath = Join-Path $RepositoryRoot '.coord/logs/usage.tsv'
    $erasPath = Join-Path $RepositoryRoot 'tools/roadmap/eras.json'
    $dataScriptPath = Join-Path $PSScriptRoot 'roadmap-data.ps1'
    if (-not (Test-Path -LiteralPath $erasPath -PathType Leaf)) {
        $erasPath = Join-Path $PSScriptRoot 'roadmap/eras.json'
    }
    foreach ($path in @($statusPath, $pendingPath, $archivePath, $usagePath, $erasPath, $dataScriptPath)) {
        try {
            $file = Get-Item -LiteralPath $path -ErrorAction Stop
            [void]$parts.Add(('{0}|{1}|{2}' -f $file.FullName, $file.Length, $file.LastWriteTimeUtc.Ticks))
        }
        catch {
            [void]$parts.Add(($path + '|missing'))
        }
    }

    $cardsPath = Join-Path $RepositoryRoot '.coord/tasks'
    try {
        $folder = Get-Item -LiteralPath $cardsPath -ErrorAction Stop
        [void]$parts.Add(('cards|{0}' -f $folder.LastWriteTimeUtc.Ticks))
        $cardFiles = @(Get-ChildItem -LiteralPath $cardsPath -Filter '*.md' -File -ErrorAction Stop | Sort-Object FullName)
        foreach ($file in $cardFiles) {
            [void]$parts.Add(('{0}|{1}|{2}' -f $file.FullName, $file.Length, $file.LastWriteTimeUtc.Ticks))
        }
    }
    catch {
        [void]$parts.Add(($cardsPath + '|missing'))
    }

    $refs = Invoke-RoadmapGit -Repository $RepositoryRoot -Arguments @(
        'for-each-ref', '--format=%(refname)|%(objectname)'
    )
    $head = Invoke-RoadmapGit -Repository $RepositoryRoot -Arguments @('rev-parse', '--verify', 'HEAD')
    $worktrees = Invoke-RoadmapGit -Repository $RepositoryRoot -Arguments @('worktree', 'list', '--porcelain')
    [void]$parts.Add(('refs|{0}' -f $refs.Text))
    [void]$parts.Add(('HEAD|{0}' -f $head.Text))
    [void]$parts.Add(('worktrees|{0}' -f $worktrees.Text))

    return [string]::Join("`n", $parts)
}

function Send-RoadmapResponse {
    param(
        [Parameter(Mandatory)][System.Net.HttpListenerResponse]$Response,
        [Parameter(Mandatory)][int]$StatusCode,
        [Parameter(Mandatory)][string]$ContentType,
        [AllowEmptyString()][string]$Body = '',
        [string]$ETag,
        [string]$PageStamp
    )

    try {
        $Response.StatusCode = $StatusCode
        if (-not [string]::IsNullOrWhiteSpace($ContentType)) {
            $Response.ContentType = $ContentType
        }
        if (-not [string]::IsNullOrWhiteSpace($ETag)) {
            $Response.Headers['ETag'] = $ETag
        }
        if (-not [string]::IsNullOrWhiteSpace($PageStamp)) {
            $Response.Headers['X-Roadmap-Page'] = $PageStamp
        }
        $Response.Headers['Cache-Control'] = 'no-cache'

        if ($StatusCode -ne 304 -and $Body.Length -gt 0) {
            $bytes = [System.Text.Encoding]::UTF8.GetBytes($Body)
            $Response.ContentLength64 = $bytes.Length
            $Response.OutputStream.Write($bytes, 0, $bytes.Length)
        }
        else {
            $Response.ContentLength64 = 0
        }
    }
    finally {
        $Response.Close()
    }
}

function Get-RoadmapClientIsLocal {
    param([Parameter(Mandatory)][System.Net.HttpListenerContext]$Context)

    $hostName = $Context.Request.Url.Host
    if ($hostName -notin @('localhost', '127.0.0.1', '::1')) {
        return $false
    }

    $remote = $Context.Request.RemoteEndPoint
    return ($null -ne $remote -and [System.Net.IPAddress]::IsLoopback($remote.Address))
}

if ($Stop -and ($Open -or $Background)) {
    Write-Output 'Use -Stop alone.'
    exit 1
}

$httpClient = New-RoadmapHttpClient
if ($Stop) {
    $health = Get-RoadmapHealth -Client $httpClient -Url $healthUrl
    if ($null -eq $health -or $health.app -ne 'antinotepad-roadmap' -or [int]$health.port -ne $Port) {
        Write-Output ('No roadmap server is running on port {0}.' -f $Port)
        exit 0
    }

    $stopResponse = $null
    $stopRequestFailed = $false
    try {
        $stopResponse = $httpClient.GetAsync($stopUrl).GetAwaiter().GetResult()
    }
    catch {
        Write-Output ('Could not ask the roadmap server on port {0} to stop.' -f $Port)
        $stopRequestFailed = $true
    }
    finally {
        if ($null -ne $stopResponse) {
            $stopResponse.Dispose()
        }
    }
    if ($stopRequestFailed) {
        exit 1
    }

    $serverProcessId = [int]$health.pid
    for ($attempt = 0; $attempt -lt 40; $attempt++) {
        if ($null -eq (Get-Process -Id $serverProcessId -ErrorAction SilentlyContinue)) {
            Write-Output ('Roadmap server stopped on port {0}.' -f $Port)
            exit 0
        }
        Start-Sleep -Milliseconds 250
    }

    Write-Output ('Roadmap server did not stop on port {0}.' -f $Port)
    exit 1
}

$existingHealth = Get-RoadmapHealth -Client $httpClient -Url $healthUrl
if ($null -ne $existingHealth -and $existingHealth.app -eq 'antinotepad-roadmap' -and [int]$existingHealth.port -eq $Port) {
    if ($Open) {
        Start-Process $baseUrl
    }
    if ($Background) {
        exit 0
    }
    Write-Output ('Roadmap server already running on port {0}.' -f $Port)
    exit 0
}

if ($Background) {
    $hostProcess = Get-Process -Id $PID -ErrorAction Stop
    $powerShell = $hostProcess.Path
    $scriptPath = Join-Path $PSScriptRoot 'roadmap-serve.ps1'
    $arguments = '-NoProfile -File "{0}" -Port {1}' -f $scriptPath, $Port
    if (-not [string]::IsNullOrWhiteSpace($Root)) {
        $arguments += ' -Root "{0}"' -f $Root
    }
    if (-not [string]::IsNullOrWhiteSpace($Label)) {
        $arguments += ' -Label "{0}"' -f $Label
    }

    try {
        $backgroundProcess = Start-Process -FilePath $powerShell -ArgumentList $arguments -WindowStyle Hidden -PassThru
    }
    catch {
        Write-Output ('Roadmap server could not bind port {0}.' -f $Port)
        exit 1
    }

    for ($attempt = 0; $attempt -lt 40; $attempt++) {
        $health = Get-RoadmapHealth -Client $httpClient -Url $healthUrl
        if ($null -ne $health -and $health.app -eq 'antinotepad-roadmap' -and [int]$health.port -eq $Port) {
            if ($Open) {
                Start-Process $baseUrl
            }
            exit 0
        }

        $backgroundProcess.Refresh()
        if ($backgroundProcess.HasExited) {
            break
        }
        Start-Sleep -Milliseconds 250
    }

    Write-Output ('Roadmap server could not bind port {0}.' -f $Port)
    exit 1
}

$listener = $null
$started = $false
for ($attempt = 0; $attempt -lt 20; $attempt++) {
    $listener = [System.Net.HttpListener]::new()
    [void]$listener.Prefixes.Add($baseUrl)
    try {
        $listener.Start()
        $started = $true
        break
    }
    catch {
        $listener.Close()
        if ($attempt -lt 19) {
            Start-Sleep -Milliseconds 250
        }
    }
}
if (-not $started) {
    $found = $false
    for ($attempt = 0; $attempt -lt 8; $attempt++) {
        if (Test-RoadmapHealth -Client $httpClient -Url $healthUrl) {
            $found = $true
            break
        }
        Start-Sleep -Milliseconds 250
    }

    $listener.Close()
    if ($found) {
        if ($Open) {
            Start-Process $baseUrl
        }
        exit 0
    }

    Write-Output ('Roadmap server could not bind port {0}.' -f $Port)
    exit 1
}

Write-RoadmapServerLog -RepositoryRoot $repository -Message ('started pid={0} port={1}' -f $PID, $Port)
if ($Open) {
    Start-Process $baseUrl
}

$cacheFingerprint = $null
$cacheJson = ''
$cacheETag = ''
$lastRebuild = [DateTimeOffset]::MinValue
$lastRequest = [DateTimeOffset]::UtcNow
$stopping = $false
$dataUrl = $baseUrl + 'data'
$pagePath = Join-Path $PSScriptRoot 'roadmap/index.html'

try {
    $pending = $null
    while ($listener.IsListening -and -not $stopping) {
        if ($null -eq $pending) {
            $pending = $listener.GetContextAsync()
        }

        if (-not $pending.Wait(1000)) {
            $currentServeStamp = Get-Item -LiteralPath $serveScript -ErrorAction Stop | ForEach-Object { '{0}|{1}' -f $_.Length, $_.LastWriteTimeUtc.Ticks }
            if ($currentServeStamp -ne $serveStamp) {
                Write-RoadmapServerLog -RepositoryRoot $repository -Message 'serve script changed; restarting'
                $listener.Stop()
                $hostProcess = Get-Process -Id $PID -ErrorAction Stop
                $arguments = '-NoProfile -File "{0}" -Port {1}' -f $serveScript, $Port
                if (-not [string]::IsNullOrWhiteSpace($Root)) {
                    $arguments += ' -Root "{0}"' -f $Root
                }
                if (-not [string]::IsNullOrWhiteSpace($Label)) {
                    $arguments += ' -Label "{0}"' -f $Label
                }
                try {
                    [void](Start-Process -FilePath $hostProcess.Path -ArgumentList $arguments -WindowStyle Hidden -PassThru)
                }
                catch {
                    Write-RoadmapServerLog -RepositoryRoot $repository -Message ('serve script restart failed: {0}' -f $_.Exception.Message)
                }
                $stopping = $true
                break
            }
            if ([DateTimeOffset]::UtcNow - $lastRequest -ge [TimeSpan]::FromHours(12)) {
                break
            }
            continue
        }

        $context = $null
        try {
            $context = $pending.GetAwaiter().GetResult()
            $pending = $null
            $lastRequest = [DateTimeOffset]::UtcNow
            if (-not (Get-RoadmapClientIsLocal -Context $context)) {
                Send-RoadmapResponse -Response $context.Response -StatusCode 403 -ContentType 'text/plain; charset=utf-8' -Body 'Localhost only.'
                continue
            }

            $path = $context.Request.Url.AbsolutePath.TrimEnd('/')
            if ($context.Request.HttpMethod -ne 'GET' -and -not ($path -eq '/prefs' -and $context.Request.HttpMethod -eq 'POST')) {
                $context.Response.Headers['Allow'] = 'GET'
                Send-RoadmapResponse -Response $context.Response -StatusCode 405 -ContentType 'text/plain; charset=utf-8' -Body 'GET only.'
                continue
            }

            $path = $context.Request.Url.AbsolutePath.TrimEnd('/')
            if ([string]::IsNullOrWhiteSpace($path)) {
                $path = '/'
            }

            switch ($path) {
                '/prefs' {
                    if (-not $prefsPath) {
                        Send-RoadmapResponse -Response $context.Response -StatusCode 503 -ContentType 'text/plain; charset=utf-8' -Body 'Preferences unavailable.'
                        break
                    }
                    if ($context.Request.HttpMethod -eq 'GET') {
                        if (-not (Test-Path -LiteralPath $prefsPath -PathType Leaf)) {
                            Send-RoadmapResponse -Response $context.Response -StatusCode 404 -ContentType 'text/plain; charset=utf-8' -Body 'No preferences saved.'
                        } else {
                            Send-RoadmapResponse -Response $context.Response -StatusCode 200 -ContentType 'application/json; charset=utf-8' -Body ([System.IO.File]::ReadAllText($prefsPath))
                        }
                        break
                    }
                    $origin = $context.Request.Headers['Origin']
                    if ($origin -and $origin.TrimEnd('/') -ne $baseUrl.TrimEnd('/')) {
                        Send-RoadmapResponse -Response $context.Response -StatusCode 403 -ContentType 'text/plain; charset=utf-8' -Body 'Origin not allowed.'
                        break
                    }
                    $limit = 64KB
                    if ($context.Request.ContentLength64 -gt $limit) {
                        Send-RoadmapResponse -Response $context.Response -StatusCode 413 -ContentType 'text/plain; charset=utf-8' -Body 'Preferences too large.'
                        break
                    }
                    $stream = [System.IO.MemoryStream]::new()
                    try {
                        $buffer = [byte[]]::new(4096)
                        while (($count = $context.Request.InputStream.Read($buffer, 0, $buffer.Length)) -gt 0) {
                            $stream.Write($buffer, 0, $count)
                            if ($stream.Length -gt $limit) { break }
                        }
                        if ($stream.Length -gt $limit) {
                            Send-RoadmapResponse -Response $context.Response -StatusCode 413 -ContentType 'text/plain; charset=utf-8' -Body 'Preferences too large.'
                            break
                        }
                        try {
                            $body = [System.Text.UTF8Encoding]::new($false, $true).GetString($stream.ToArray())
                            $json = [System.Text.Json.JsonDocument]::Parse($body)
                            try { $isObject = $json.RootElement.ValueKind -eq [System.Text.Json.JsonValueKind]::Object } finally { $json.Dispose() }
                            if (-not $isObject) { throw 'Expected an object.' }
                        } catch {
                            Send-RoadmapResponse -Response $context.Response -StatusCode 400 -ContentType 'text/plain; charset=utf-8' -Body 'Expected a JSON object.'
                            break
                        }
                        $temporary = $prefsPath + '.' + [guid]::NewGuid().ToString('N') + '.tmp'
                        try {
                            [System.IO.File]::WriteAllText($temporary, $body, [System.Text.UTF8Encoding]::new($false))
                            [System.IO.File]::Move($temporary, $prefsPath, $true)
                        } finally {
                            if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary -Force }
                        }
                        Send-RoadmapResponse -Response $context.Response -StatusCode 200 -ContentType 'application/json; charset=utf-8' -Body '{"saved":true}'
                    } finally { $stream.Dispose() }
                    break
                }
                '/health' {
                    $body = ConvertTo-Json -InputObject ([ordered]@{ app = 'antinotepad-roadmap'; pid = $PID; port = $Port }) -Compress
                    Send-RoadmapResponse -Response $context.Response -StatusCode 200 -ContentType 'application/json; charset=utf-8' -Body $body
                    break
                }
                '/stop' {
                    Send-RoadmapResponse -Response $context.Response -StatusCode 200 -ContentType 'application/json; charset=utf-8' -Body '{"stopping":true}'
                    $stopping = $true
                    break
                }
                '/data' {
                    $reloadFailed = $false
                    $currentDataStamp = Get-Item -LiteralPath $dataScript -ErrorAction Stop | ForEach-Object { '{0}|{1}' -f $_.Length, $_.LastWriteTimeUtc.Ticks }
                    if ($currentDataStamp -ne $dataScriptStamp) {
                        $previousFunctions = @{}
                        Get-ChildItem Function: | ForEach-Object { $previousFunctions[$_.Name] = $_.ScriptBlock }
                        try {
                            . $dataScript -RequestedRoot $Root
                        }
                        catch {
                            Get-ChildItem Function: | Where-Object { -not $previousFunctions.ContainsKey($_.Name) } | ForEach-Object { Remove-Item -LiteralPath (Join-Path Function: $_.Name) -ErrorAction SilentlyContinue }
                            foreach ($name in $previousFunctions.Keys) {
                                Set-Item -LiteralPath (Join-Path Function: $name) -Value $previousFunctions[$name]
                            }
                            $reloadFailed = $true
                            Write-RoadmapServerLog -RepositoryRoot $repository -Message ('data script reload failed: {0}' -f $_.Exception.Message)
                        }
                        $dataScriptStamp = $currentDataStamp
                    }
                    $fingerprint = Get-RoadmapFingerprint -RepositoryRoot $repository
                    $changed = ($null -eq $cacheFingerprint -or $fingerprint -ne $cacheFingerprint)
                    $due = ([DateTimeOffset]::UtcNow - $lastRebuild) -ge [TimeSpan]::FromSeconds(2)
                    if (-not $reloadFailed -and ($null -eq $cacheFingerprint -or ($changed -and $due))) {
                        $data = Get-RoadmapData -RequestedRoot $repository
                        $cacheJson = ConvertTo-Json -InputObject $data -Depth 100 -Compress
                        $bytes = [System.Text.Encoding]::UTF8.GetBytes($cacheJson)
                        $hash = [System.Security.Cryptography.SHA256]::HashData($bytes)
                        $cacheETag = '"{0}"' -f [Convert]::ToHexString($hash).ToLowerInvariant()
                        $cacheFingerprint = $fingerprint
                        $lastRebuild = [DateTimeOffset]::UtcNow
                    }

                    $pageFile = Get-Item -LiteralPath $pagePath -ErrorAction SilentlyContinue
                    $pageStamp = if ($null -ne $pageFile) { '{0}-{1}' -f $pageFile.Length, $pageFile.LastWriteTimeUtc.Ticks } else { 'missing' }
                    $sentETags = @(([string]$context.Request.Headers['If-None-Match']) -split ',') | ForEach-Object { $_.Trim() }
                    if ($sentETags -contains '*' -or $sentETags -contains $cacheETag) {
                        Send-RoadmapResponse -Response $context.Response -StatusCode 304 -ContentType 'application/json; charset=utf-8' -ETag $cacheETag -PageStamp $pageStamp
                    }
                    else {
                        Send-RoadmapResponse -Response $context.Response -StatusCode 200 -ContentType 'application/json; charset=utf-8' -Body $cacheJson -ETag $cacheETag -PageStamp $pageStamp
                    }
                    break
                }
                '/' {
                    if (Test-Path -LiteralPath $pagePath -PathType Leaf) {
                        try {
                            $page = Get-Content -LiteralPath $pagePath -Raw -ErrorAction Stop
                            if (-not [string]::IsNullOrWhiteSpace($Label)) {
                                $safeLabel = [System.Net.WebUtility]::HtmlEncode($Label)
                                $page = $page.Replace('id="devLabel" class="dev-label" hidden', 'id="devLabel" class="dev-label"')
                                $page = $page.Replace('<!-- ROADMAP_LABEL_SLOT -->', $safeLabel)
                            }
                            Send-RoadmapResponse -Response $context.Response -StatusCode 200 -ContentType 'text/html; charset=utf-8' -Body $page
                        }
                        catch {
                            Send-RoadmapResponse -Response $context.Response -StatusCode 500 -ContentType 'text/plain; charset=utf-8' -Body 'Roadmap page is unreadable.'
                        }
                    }
                    else {
                        Send-RoadmapResponse -Response $context.Response -StatusCode 200 -ContentType 'text/html; charset=utf-8' -Body '<!doctype html><title>AntiNotepad roadmap</title><p>Roadmap page is being built.</p>'
                    }
                    break
                }
                default {
                    Send-RoadmapResponse -Response $context.Response -StatusCode 404 -ContentType 'text/plain; charset=utf-8' -Body 'Not found.'
                }
            }
        }
        catch {
            $pending = $null
            try {
                if ($null -ne $context -and $context.Response.OutputStream.CanWrite) {
                    Send-RoadmapResponse -Response $context.Response -StatusCode 500 -ContentType 'text/plain; charset=utf-8' -Body 'Roadmap request failed.'
                }
            }
            catch {
                # The request may already be closed if the client disconnected.
            }
        }
    }
}
finally {
    if ($listener.IsListening) {
        $listener.Stop()
    }
    $listener.Close()
    Write-RoadmapServerLog -RepositoryRoot $repository -Message ('stopped pid={0} port={1}' -f $PID, $Port)
    $httpClient.Dispose()
}
