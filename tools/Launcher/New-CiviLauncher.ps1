param(
    [Parameter(Mandatory=$true)][string]$Archive,
    [Parameter(Mandatory=$true)][string]$OutputPath,
    [string]$Repository = 'Vie-is-mee/CIVI',
    [string]$Version = '0.41.1'
)
$ErrorActionPreference = 'Stop'
if ($Repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$') { throw 'Repository must be OWNER/REPO.' }
if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw 'Version must be x.y.z.' }
$Archive = (Resolve-Path -LiteralPath $Archive).Path
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead($Archive)
try {
    $files = @()
    $seen = @{}
    foreach ($entry in $zip.Entries) {
        if (-not $entry.Name) { continue }
        $name = $entry.FullName
        if (-not $name.StartsWith('CIVI-Trang17/') -or $name.Contains('\') -or $name.Contains(':') -or $name -match '(^|/)\.\.?(/|$)' -or $seen.ContainsKey($name)) {
            throw "Unexpected ZIP entry: $name"
        }
        $seen[$name] = $true
        $stream = $entry.Open()
        $sha = [Security.Cryptography.SHA256]::Create()
        try { $hash = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','') }
        finally { $sha.Dispose(); $stream.Dispose() }
        $files += [ordered]@{ Path=$name.Substring('CIVI-Trang17/'.Length); Size=$entry.Length; Sha256=$hash }
    }
} finally { $zip.Dispose() }
foreach ($required in @('CIVI-Trang17.exe','UnityPlayer.dll','CIVI-Trang17_Data/globalgamemanagers','MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll')) {
    if ($required -notin $files.Path) { throw "Missing runtime file: $required" }
}
$config = [ordered]@{
    Version=$Version; Tag=('v'+$Version); Repository=$Repository
    ArchiveName=[IO.Path]::GetFileName($Archive)
    ArchiveSha256=(Get-FileHash -LiteralPath $Archive -Algorithm SHA256).Hash
    Files=$files
} | ConvertTo-Json -Depth 5 -Compress
$header = @'
@echo off
setlocal
set "CIVI_LAUNCHER_SELF=%~f0"
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoLogo -NoProfile -Command "$ErrorActionPreference='Stop'; try { $text=[IO.File]::ReadAllText($env:CIVI_LAUNCHER_SELF); $parts=$text -split '(?m)^#<CIVI-POWERSHELL>\r?$',2; if($parts.Length -ne 2){throw 'Invalid CIVI launcher'}; & ([scriptblock]::Create($parts[1])) } catch { Write-Host ('CIVI could not start: '+$_.Exception.Message) -ForegroundColor Red; exit 1 }"
set "CIVI_RESULT=%ERRORLEVEL%"
if not "%CIVI_RESULT%"=="0" (
    echo Please check your connection, free disk space, or send this message to the CIVI team.
    pause
)
exit /b %CIVI_RESULT%
#<CIVI-POWERSHELL>
'@
$engine = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'CiviLauncher.ps1'))
$tail = "`r`n`$civiConfig = @'`r`n" + $config + "`r`n'@ | ConvertFrom-Json`r`nInvoke-CiviLauncher -Config `$civiConfig -LauncherPath `$env:CIVI_LAUNCHER_SELF | Out-Null`r`n"
$content = ($header + "`r`n" + $engine + $tail) -replace "`r?`n", "`r`n"
$OutputPath = [IO.Path]::GetFullPath($OutputPath)
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputPath) | Out-Null
[IO.File]::WriteAllText($OutputPath, $content, [Text.Encoding]::ASCII)
Write-Output "Created $OutputPath ($($files.Count) verified files, SHA256 pinned)."
