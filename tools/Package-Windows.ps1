param([string]$Version='0.41.1')
$ErrorActionPreference = 'Stop'
if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw 'Version must be x.y.z.' }
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$build = Join-Path $repoRoot 'release\Windows'
$archive = Join-Path $repoRoot ('release\CIVI-Trang17-' + $Version + '-Windows.zip')
if (Test-Path -LiteralPath $archive) { throw 'Release ZIP already exists. Keep published archives immutable; use a new version or choose a fresh local build folder.' }
$files = @()
foreach ($name in @('CIVI-Trang17.exe','UnityPlayer.dll','UnityCrashHandler64.exe','dstorage.dll','dstoragecore.dll')) {
    $path = Join-Path $build $name
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing runtime file: $name" }
    $files += Get-Item -LiteralPath $path
}
foreach ($name in @('CIVI-Trang17_Data','MonoBleedingEdge','D3D12')) {
    $path = Join-Path $build $name
    if (-not (Test-Path -LiteralPath $path -PathType Container)) { throw "Missing runtime directory: $name" }
    $files += Get-ChildItem -LiteralPath $path -Recurse -File
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::Open($archive, 'Create')
try {
    foreach ($file in $files) {
        $name = 'CIVI-Trang17/' + $file.FullName.Substring($build.Length + 1).Replace('\','/')
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, $name) | Out-Null
    }
    foreach ($file in Get-ChildItem -LiteralPath (Join-Path $repoRoot 'Game\Assets\FontLicenses') -Filter '*.txt') {
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, ('CIVI-Trang17/Licenses/' + $file.Name)) | Out-Null
    }
    [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, (Join-Path $repoRoot 'docs\BAT-DAU.txt'), 'CIVI-Trang17/BAT-DAU.txt') | Out-Null
} finally { $zip.Dispose() }
$bat = Join-Path $repoRoot 'release\Choi-CIVI.bat'
& (Join-Path $PSScriptRoot 'Launcher\New-CiviLauncher.ps1') -Archive $archive -OutputPath $bat -Version $Version -Repository 'Vie-is-mee/CIVI'
@($archive, $bat) | ForEach-Object { (Get-FileHash -LiteralPath $_ -Algorithm SHA256).Hash.ToLowerInvariant() + '  ' + [IO.Path]::GetFileName($_) } |
    Set-Content -LiteralPath (Join-Path $repoRoot 'release\SHA256SUMS.txt') -Encoding ASCII
Write-Output 'Release assets ready. Run launcher checks against the ZIP before uploading.'
