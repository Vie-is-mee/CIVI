param([string]$RealArchive)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'CiviLauncher.ps1')
$testWorkspace = [IO.DirectoryInfo]$PSScriptRoot
while ($testWorkspace -and -not (Test-Path -LiteralPath (Join-Path $testWorkspace.FullName '.git'))) { $testWorkspace = $testWorkspace.Parent }
if (-not $testWorkspace) { throw 'Run launcher tests from a Git checkout.' }
$testParent = Join-Path $testWorkspace.FullName 'tmp'
$testRoot = Join-Path $testParent ('civi-test-' + [guid]::NewGuid().ToString('N').Substring(0,16))
Assert-CiviChildPath $testParent $testRoot
New-Item -ItemType Directory -Force -Path $testRoot | Out-Null
$script:networkCalls = 0
$script:networkFails = $false
$script:fixtureArchive = ''
function Invoke-WebRequest {
    param($Uri, [switch]$UseBasicParsing, $OutFile, $TimeoutSec, $MaximumRedirection)
    $script:networkCalls++
    if ($Uri -ne 'https://github.com/Vie-is-mee/CIVI/releases/download/v0.41.1/CIVI-Test-0.41.1.zip') { throw "Wrong release URL: $Uri" }
    if ($script:networkFails) { throw 'Simulated offline connection' }
    Copy-Item -LiteralPath $script:fixtureArchive -Destination $OutFile
}
function Assert-Test($Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
function Read-TestConfig([string]$Bat) {
    $text = [IO.File]::ReadAllText($Bat)
    $match = [regex]::Match($text, "(?s)\`$civiConfig = @'\r?\n(.*?)\r?\n'@ \| ConvertFrom-Json")
    Assert-Test $match.Success 'Cannot find BAT configuration'
    return $match.Groups[1].Value | ConvertFrom-Json
}
try {
    $fixture = Join-Path $testRoot 'fixture\CIVI-Trang17'
    foreach ($name in @('CIVI-Trang17.exe','UnityPlayer.dll','CIVI-Trang17_Data\globalgamemanagers','MonoBleedingEdge\EmbedRuntime\mono-2.0-bdwgc.dll')) {
        $path = Join-Path $fixture $name
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $path) | Out-Null
        [IO.File]::WriteAllText($path, 'CIVI launcher fixture; never execute this file.')
    }
    $downloads = Join-Path $testRoot "Downloads thử nghiệm's & files"
    New-Item -ItemType Directory -Path $downloads | Out-Null
    $script:fixtureArchive = Join-Path $downloads 'CIVI-Test-0.41.1.zip'
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $fixtureZip = [IO.Compression.ZipFile]::Open($script:fixtureArchive, 'Create')
    try {
        foreach ($file in Get-ChildItem -LiteralPath $fixture -Recurse -File) {
            $name = 'CIVI-Trang17/' + $file.FullName.Substring($fixture.Length + 1).Replace('\','/')
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($fixtureZip, $file.FullName, $name) | Out-Null
        }
    } finally { $fixtureZip.Dispose() }
    $bat = Join-Path $downloads 'Choi-CIVI.bat'
    & (Join-Path $PSScriptRoot 'New-CiviLauncher.ps1') -Archive $script:fixtureArchive -OutputPath $bat
    $config = Read-TestConfig $bat
    $install = Join-Path $testRoot "AppData thử nghiệm's & files\CIVI"
    $destination = Invoke-CiviLauncher -Config $config -LauncherPath $bat -InstallRoot $install -NoLaunch -NoShortcut
    Assert-Test (Test-CiviInstallation $config $destination) 'First install failed'
    Assert-Test ($script:networkCalls -eq 0) 'Local ZIP unexpectedly required network'
    Write-Output 'PASS: fresh install, local ZIP and paths with spaces, accents, apostrophe and ampersand.'

    # Installed files must be sufficient even when the launcher moves and the ZIP is absent.
    $offlineBat = Join-Path $testRoot 'NoZip\Choi-CIVI.bat'
    $again = Invoke-CiviLauncher -Config $config -LauncherPath $offlineBat -InstallRoot $install -NoLaunch -NoShortcut
    Assert-Test ($again -eq $destination -and $script:networkCalls -eq 0) 'Offline launch downloaded again'
    Write-Output 'PASS: repeated launch uses verified installed files without network.'

    Set-Content -LiteralPath (Join-Path $destination 'UnityPlayer.dll') -Value 'damaged'
    Invoke-CiviLauncher -Config $config -LauncherPath $bat -InstallRoot $install -NoLaunch -NoShortcut | Out-Null
    Assert-Test (Test-CiviInstallation $config $destination) 'Corrupt installation was not repaired'
    Assert-Test (@(Get-ChildItem -LiteralPath $install -Directory -Force | Where-Object Name -Like '.*').Count -eq 0) 'Repair left staging or backup directories'
    Write-Output 'PASS: damaged installed file is detected and repaired; temporary files removed.'

    $downloadRoot = Join-Path $testRoot 'DownloadInstall'
    $downloaded = Invoke-CiviLauncher -Config $config -LauncherPath $offlineBat -InstallRoot $downloadRoot -NoLaunch -NoShortcut
    Assert-Test ($script:networkCalls -eq 1 -and (Test-CiviInstallation $config $downloaded)) 'Pinned GitHub download route failed'
    Write-Output 'PASS: download uses exact repository, release tag and asset name (mocked HTTP boundary).'

    $script:networkFails = $true
    $failed = $false
    try { Invoke-CiviLauncher -Config $config -LauncherPath $offlineBat -InstallRoot (Join-Path $testRoot 'OfflineFresh') -NoLaunch -NoShortcut | Out-Null }
    catch { $failed = $_.Exception.Message -like '*offline*' }
    Assert-Test $failed 'Fresh offline install did not report download failure'
    Assert-Test (Test-CiviInstallation $config $destination) 'Network failure damaged an existing installation'
    $script:networkFails = $false
    Write-Output 'PASS: network failure leaves existing installation intact.'

    $badConfig = $config | ConvertTo-Json -Depth 6 | ConvertFrom-Json
    $badConfig.ArchiveSha256 = '0' * 64
    $failed = $false
    try { Invoke-CiviLauncher -Config $badConfig -LauncherPath $bat -InstallRoot $install -NoLaunch -NoShortcut | Out-Null }
    catch { $failed = $_.Exception.Message -like '*does not match*' }
    Assert-Test ($failed -and (Test-CiviInstallation $config $destination)) 'Bad archive replaced existing installation'
    Write-Output 'PASS: incorrect ZIP hash is rejected before replacement.'

    $lock = [IO.File]::Open((Join-Path $install 'launcher.lock'), 'OpenOrCreate', 'ReadWrite', 'None')
    try {
        $failed = $false
        try { Invoke-CiviLauncher -Config $config -LauncherPath $bat -InstallRoot $install -NoLaunch -NoShortcut | Out-Null }
        catch { $failed = $_.Exception.Message -like '*launcher is busy*' }
        Assert-Test $failed 'Concurrent launcher was not stopped'
    } finally { $lock.Dispose() }
    $failed = $false
    try { Assert-CiviChildPath $install (Join-Path $testRoot 'outside') } catch { $failed = $true }
    Assert-Test $failed 'Outside path accepted'
    Write-Output 'PASS: concurrent installers and paths outside installation root are blocked.'

    $evil = Join-Path $testRoot 'traversal.zip'
    $z = [IO.Compression.ZipFile]::Open($evil, 'Create')
    try { $z.CreateEntry('CIVI-Trang17/../../outside.txt') | Out-Null } finally { $z.Dispose() }
    $badConfig.ArchiveSha256 = (Get-FileHash -LiteralPath $evil -Algorithm SHA256).Hash
    $failed = $false
    try { Assert-CiviArchive $badConfig $evil } catch { $failed = $_.Exception.Message -like '*Unsafe path*' }
    Assert-Test $failed 'ZIP traversal path was accepted'
    Write-Output 'PASS: ZIP traversal is rejected even with a matching archive hash.'

    # Exercise the real CMD -> embedded PowerShell path without opening an application.
    $originalEntry = 'Invoke-CiviLauncher -Config $civiConfig -LauncherPath $env:CIVI_LAUNCHER_SELF | Out-Null'
    $testEntry = 'Invoke-CiviLauncher -Config $civiConfig -LauncherPath $env:CIVI_LAUNCHER_SELF -InstallRoot $env:CIVI_TEST_ROOT -NoLaunch -NoShortcut | Out-Null'
    $content = [IO.File]::ReadAllText($bat)
    Assert-Test ($content.Contains($originalEntry)) 'Production BAT entry point changed unexpectedly'
    [IO.File]::WriteAllText($bat, $content.Replace($originalEntry, $testEntry), [Text.Encoding]::ASCII)
    $oldTestRoot = $env:CIVI_TEST_ROOT
    try {
        $env:CIVI_TEST_ROOT = Join-Path $testRoot 'BatInstall'
        Push-Location $downloads
        try { & $env:ComSpec /d /c Choi-CIVI.bat; Assert-Test ($LASTEXITCODE -eq 0) 'CMD launcher failed' }
        finally { Pop-Location }
        Assert-Test (Test-CiviInstallation $config (Join-Path $env:CIVI_TEST_ROOT 'versions\0.41.1')) 'BAT did not install'
    } finally { $env:CIVI_TEST_ROOT = $oldTestRoot }
    Write-Output 'PASS: actual single BAT bootstrap installs correctly; game launch disabled for test.'

    $desktop = Join-Path $testRoot 'Fake Desktop'
    New-Item -ItemType Directory -Path $desktop | Out-Null
    Set-CiviShortcut $install (Join-Path $destination 'CIVI-Trang17.exe') $desktop
    $link = Join-Path $desktop 'CIVI - Trang 17.lnk'
    Assert-Test ([CiviShortcut]::Read($link) -eq (Join-Path $destination 'CIVI-Trang17.exe')) 'Shortcut target is incorrect'
    # Existing unrelated shortcuts must never be replaced.
    [CiviShortcut]::Write($link, $env:ComSpec, (Split-Path -Parent $env:ComSpec))
    Set-CiviShortcut $install (Join-Path $destination 'CIVI-Trang17.exe') $desktop
    Assert-Test ([CiviShortcut]::Read($link) -eq $env:ComSpec) 'An unrelated shortcut was replaced'
    Write-Output 'PASS: Unicode shortcut target round-trips correctly; unrelated shortcut is preserved.'

    if ($RealArchive) {
        $realBat = Join-Path $testRoot 'Choi-CIVI-real.bat'
        & (Join-Path $PSScriptRoot 'New-CiviLauncher.ps1') -Archive $RealArchive -OutputPath $realBat
        $realConfig = Read-TestConfig $realBat
        $realRoot = Join-Path $testRoot 'RealGame'
        New-Item -ItemType Directory -Path $realRoot | Out-Null
        $realDest = Join-Path $realRoot 'versions\0.41.1'
        Install-CiviPayload $realConfig $realRoot $realDest (Resolve-Path -LiteralPath $RealArchive).Path
        Assert-Test (Test-CiviInstallation $realConfig $realDest) 'Real game installation failed'
        Write-Output ('PASS: real release ZIP installs and verifies all ' + $realConfig.Files.Count + ' files.')
    }
    Write-Output 'ALL LAUNCHER CHECKS PASSED.'
} finally { Remove-CiviTemporary $testParent $testRoot }
