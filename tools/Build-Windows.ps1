param([string]$UnityEditorPath='C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.com')
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$project = Join-Path $repoRoot 'Game'
$output = Join-Path $repoRoot 'release\Windows'
if (-not (Test-Path -LiteralPath $UnityEditorPath -PathType Leaf)) { throw 'Unity 6000.6.5f1 not found. Pass -UnityEditorPath explicitly.' }
New-Item -ItemType Directory -Force -Path $output | Out-Null
$oldOutput = $env:CIVI41_OUTPUT
try {
    $env:CIVI41_OUTPUT = $output
    & $UnityEditorPath -batchmode -nographics -projectPath $project -executeMethod Civi41Editor.Build -quit -logFile (Join-Path $repoRoot 'release\build.log')
    if ($LASTEXITCODE -ne 0) { throw 'Unity build failed; see release/build.log.' }
    if (-not (Test-Path -LiteralPath (Join-Path $output 'CIVI-Trang17.exe'))) { throw 'Unity returned without producing an executable.' }
} finally { $env:CIVI41_OUTPUT = $oldOutput }
Write-Output "Built $output"
