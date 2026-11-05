# Serves the token playground page (tools/playground) and the read-only measurements (docs/playground) on localhost.
# Same switches as roadmap-serve.ps1: -Background, -Open, -Stop; health at /health. The one write is POST /picks.
[CmdletBinding()]
param(
    [ValidateRange(1, 65535)][int]$Port = 47812,
    [switch]$Open,
    [switch]$Stop,
    [switch]$Background
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$pageRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'playground'))
$dataRoot = [IO.Path]::GetFullPath((Join-Path $repository 'docs/playground'))
$appName = 'antinotepad-playground'
$baseUrl = 'http://localhost:{0}/' -f $Port
$healthUrl = $baseUrl + 'health'
$stopUrl = $baseUrl + 'stop'
$contentTypes = @{
    '.html' = 'text/html; charset=utf-8'
    '.json' = 'application/json; charset=utf-8'
    '.md'   = 'text/markdown; charset=utf-8'
    '.png'  = 'image/png'
    '.css'  = 'text/css; charset=utf-8'
    '.js'   = 'text/javascript; charset=utf-8'
    '.svg'  = 'image/svg+xml'
}

function New-PlaygroundHttpClient {
    $handler = [System.Net.Http.HttpClientHandler]::new()
    $handler.UseProxy = $false
    $client = [System.Net.Http.HttpClient]::new($handler)
    $client.Timeout = [TimeSpan]::FromSeconds(1)
    return $client
}

# Returns the parsed /health reply of whatever answers on the port, or $null when nothing does.
function Get-PlaygroundHealth {
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

function Test-PlaygroundHealth {
    param([AllowNull()]$Health)

    return ($null -ne $Health -and $Health.app -eq $appName -and [int]$Health.port -eq $Port)
}

function Write-PlaygroundServerLog {
    param([Parameter(Mandatory)][string]$Message)

    try {
        $logFolder = Join-Path $repository 'build'
        [void](New-Item -ItemType Directory -Path $logFolder -Force)
        $line = '{0} {1}' -f [DateTimeOffset]::Now.ToString('yyyy-MM-dd HH:mm:ss zzz'), $Message
        Add-Content -LiteralPath (Join-Path $logFolder 'playground-serve.log') -Value $line -Encoding utf8NoBOM
    }
    catch {
        # Logging is best-effort; the server does not depend on a writable log folder.
    }
}

function Send-PlaygroundResponse {
    param(
        [Parameter(Mandatory)][System.Net.HttpListenerResponse]$Response,
        [Parameter(Mandatory)][int]$StatusCode,
        [Parameter(Mandatory)][string]$ContentType,
        [byte[]]$Body = @()
    )

    try {
        $Response.StatusCode = $StatusCode
        $Response.ContentType = $ContentType
        $Response.Headers['Cache-Control'] = 'no-cache'
        $Response.ContentLength64 = $Body.Length
        if ($Body.Length -gt 0) {
            $Response.OutputStream.Write($Body, 0, $Body.Length)
        }
    }
    finally {
        $Response.Close()
    }
}

function Send-PlaygroundText {
    param(
        [Parameter(Mandatory)][System.Net.HttpListenerResponse]$Response,
        [Parameter(Mandatory)][int]$StatusCode,
        [Parameter(Mandatory)][string]$Text
    )

    Send-PlaygroundResponse -Response $Response -StatusCode $StatusCode -ContentType 'text/plain; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes($Text))
}

# Maps a request path to a file under the page folder or the read-only docs/playground folder; $null when it is neither.
function Resolve-PlaygroundFile {
    param([Parameter(Mandatory)][string]$UrlPath)

    $relative = [Uri]::UnescapeDataString($UrlPath).TrimStart('/')
    if ($relative -eq '') {
        $relative = 'index.html'
    }

    $root = $pageRoot
    if ($relative.StartsWith('docs/playground/', [StringComparison]::Ordinal)) {
        $root = $dataRoot
        $relative = $relative.Substring('docs/playground/'.Length)
    }

    $full = [IO.Path]::GetFullPath((Join-Path $root $relative))
    $inside = $full.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)
    if (-not $inside -or -not (Test-Path -LiteralPath $full -PathType Leaf)) {
        return $null
    }

    return $full
}

$picksPath = Join-Path $dataRoot 'picks.json'
$maxPicksBytes = 1MB

# Saves the posted token picks to docs/playground/picks.json: only that file, only valid JSON object text, at most 1 MB.
function Save-PlaygroundPicks {
    param([Parameter(Mandatory)][System.Net.HttpListenerContext]$Context)

    $request = $Context.Request
    $response = $Context.Response

    # a page on another origin must not be able to write the file
    $origin = [string]$request.Headers['Origin']
    if ($origin -ne '' -and $origin -notin @("http://localhost:$Port", "http://127.0.0.1:$Port")) {
        Send-PlaygroundText -Response $response -StatusCode 403 -Text 'Same-origin only.'
        return
    }

    if ($request.ContentLength64 -gt $maxPicksBytes) {
        Send-PlaygroundText -Response $response -StatusCode 413 -Text 'Picks are too large.'
        return
    }

    $buffer = [IO.MemoryStream]::new()
    $chunk = [byte[]]::new(8192)
    while (($read = $request.InputStream.Read($chunk, 0, $chunk.Length)) -gt 0) {
        $buffer.Write($chunk, 0, $read)
        if ($buffer.Length -gt $maxPicksBytes) {
            Send-PlaygroundText -Response $response -StatusCode 413 -Text 'Picks are too large.'
            return
        }
    }

    try {
        $text = [Text.UTF8Encoding]::new($false, $true).GetString($buffer.ToArray())
        $parsed = $text | ConvertFrom-Json -ErrorAction Stop
        if ($text.TrimStart()[0] -ne '{' -or $null -eq $parsed) {
            throw 'Not a JSON object.'
        }
    }
    catch {
        Send-PlaygroundText -Response $response -StatusCode 400 -Text 'Picks must be a valid JSON object.'
        return
    }

    $temp = $picksPath + '.tmp'
    [IO.File]::WriteAllText($temp, $text, [Text.UTF8Encoding]::new($false))
    Move-Item -LiteralPath $temp -Destination $picksPath -Force
    $reply = ConvertTo-Json -InputObject ([ordered]@{ saved = $true; bytes = $buffer.Length; at = [DateTimeOffset]::Now.ToString('o') }) -Compress
    Send-PlaygroundResponse -Response $response -StatusCode 200 -ContentType 'application/json; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes($reply))
}

function Test-ClientIsLocal {
    param([Parameter(Mandatory)][System.Net.HttpListenerContext]$Context)

    if ($Context.Request.Url.Host -notin @('localhost', '127.0.0.1', '::1')) {
        return $false
    }

    $remote = $Context.Request.RemoteEndPoint
    return ($null -ne $remote -and [System.Net.IPAddress]::IsLoopback($remote.Address))
}

if ($Stop -and ($Open -or $Background)) {
    Write-Output 'Use -Stop alone.'
    exit 1
}

$httpClient = New-PlaygroundHttpClient
if ($Stop) {
    $health = Get-PlaygroundHealth -Client $httpClient -Url $healthUrl
    if (-not (Test-PlaygroundHealth -Health $health)) {
        Write-Output ('No playground server is running on port {0}.' -f $Port)
        exit 0
    }

    try {
        $httpClient.GetAsync($stopUrl).GetAwaiter().GetResult().Dispose()
    }
    catch {
        Write-Output ('Could not ask the playground server on port {0} to stop.' -f $Port)
        exit 1
    }

    $serverProcessId = [int]$health.pid
    for ($attempt = 0; $attempt -lt 40; $attempt++) {
        if ($null -eq (Get-Process -Id $serverProcessId -ErrorAction SilentlyContinue)) {
            Write-Output ('Playground server stopped on port {0}.' -f $Port)
            exit 0
        }
        Start-Sleep -Milliseconds 250
    }

    Write-Output ('Playground server did not stop on port {0}.' -f $Port)
    exit 1
}

if (Test-PlaygroundHealth -Health (Get-PlaygroundHealth -Client $httpClient -Url $healthUrl)) {
    if ($Open) {
        Start-Process $baseUrl
    }
    Write-Output ('Playground server already running on port {0}.' -f $Port)
    exit 0
}

if ($Background) {
    $powerShell = (Get-Process -Id $PID -ErrorAction Stop).Path
    $arguments = '-NoProfile -File "{0}" -Port {1}' -f $PSCommandPath, $Port
    $backgroundProcess = Start-Process -FilePath $powerShell -ArgumentList $arguments -WindowStyle Hidden -PassThru

    for ($attempt = 0; $attempt -lt 40; $attempt++) {
        if (Test-PlaygroundHealth -Health (Get-PlaygroundHealth -Client $httpClient -Url $healthUrl)) {
            if ($Open) {
                Start-Process $baseUrl
            }
            Write-Output ('Playground server running at {0}' -f $baseUrl)
            exit 0
        }

        $backgroundProcess.Refresh()
        if ($backgroundProcess.HasExited) {
            break
        }
        Start-Sleep -Milliseconds 250
    }

    Write-Output ('Playground server could not bind port {0}.' -f $Port)
    exit 1
}

$listener = [System.Net.HttpListener]::new()
[void]$listener.Prefixes.Add($baseUrl)
try {
    $listener.Start()
}
catch {
    $listener.Close()
    Write-Output ('Playground server could not bind port {0}.' -f $Port)
    exit 1
}

Write-PlaygroundServerLog -Message ('started pid={0} port={1}' -f $PID, $Port)
if ($Open) {
    Start-Process $baseUrl
}

$lastRequest = [DateTimeOffset]::UtcNow
$stopping = $false
try {
    $pending = $null
    while ($listener.IsListening -and -not $stopping) {
        if ($null -eq $pending) {
            $pending = $listener.GetContextAsync()
        }

        if (-not $pending.Wait(1000)) {
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
            $response = $context.Response

            if (-not (Test-ClientIsLocal -Context $context)) {
                Send-PlaygroundText -Response $response -StatusCode 403 -Text 'Localhost only.'
                continue
            }

            if ($context.Request.HttpMethod -eq 'POST' -and $context.Request.Url.AbsolutePath -eq '/picks') {
                Save-PlaygroundPicks -Context $context
                continue
            }

            if ($context.Request.HttpMethod -ne 'GET') {
                $response.Headers['Allow'] = 'GET'
                Send-PlaygroundText -Response $response -StatusCode 405 -Text 'GET only (and POST /picks).'
                continue
            }

            $path = $context.Request.Url.AbsolutePath
            if ($path -eq '/health') {
                $body = ConvertTo-Json -InputObject ([ordered]@{ app = $appName; pid = $PID; port = $Port }) -Compress
                Send-PlaygroundResponse -Response $response -StatusCode 200 -ContentType 'application/json; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes($body))
                continue
            }

            if ($path -eq '/stop') {
                Send-PlaygroundResponse -Response $response -StatusCode 200 -ContentType 'application/json; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes('{"stopping":true}'))
                $stopping = $true
                continue
            }

            $file = Resolve-PlaygroundFile -UrlPath $path
            if ($null -eq $file) {
                Send-PlaygroundText -Response $response -StatusCode 404 -Text 'Not found.'
                continue
            }

            $type = $contentTypes[[IO.Path]::GetExtension($file).ToLowerInvariant()]
            if ($null -eq $type) {
                $type = 'application/octet-stream'
            }
            Send-PlaygroundResponse -Response $response -StatusCode 200 -ContentType $type -Body ([IO.File]::ReadAllBytes($file))
        }
        catch {
            $pending = $null
            try {
                if ($null -ne $context -and $context.Response.OutputStream.CanWrite) {
                    Send-PlaygroundText -Response $context.Response -StatusCode 500 -Text 'Playground request failed.'
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
    Write-PlaygroundServerLog -Message ('stopped pid={0} port={1}' -f $PID, $Port)
    $httpClient.Dispose()
}
