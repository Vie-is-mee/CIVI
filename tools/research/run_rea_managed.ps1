param(
    [string]$AssemblyPath = ""
)
# REA 5.0.0 is intentionally pinned for Node 24.13.0 on the user's current Windows machine.
# Read-only analysis; does not extract third-party game assets or modify the Unity project.
$ErrorActionPreference = "Stop"
$repo = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
if ([string]::IsNullOrWhiteSpace($AssemblyPath)) {
    $AssemblyPath = Join-Path $repo "release/SchoolWorldV2/CIVI_SchoolWorld_V2_x86_Data/Managed/Civi.TV3.Preview.dll"
}
if (!(Test-Path -LiteralPath $AssemblyPath)) {
    throw "Managed assembly does not exist: $AssemblyPath"
}
$work = Join-Path $env:LOCALAPPDATA "Temp/CIVI_REA_RESEARCH_2026_11_01"
New-Item -ItemType Directory -Force -Path $work | Out-Null
Push-Location $work
try {
    Write-Host "REA inspecting $AssemblyPath"
    & npx.cmd -y rea-agents@5.0.0 inspect-managed-artifact $AssemblyPath --format json | Out-File -FilePath (Join-Path $work "identity.json") -Encoding utf8
    if ($LASTEXITCODE -ne 0) { throw "REA managed identity failed" }
    & npx.cmd -y rea-agents@5.0.0 inspect-managed-members $AssemblyPath --format json | Out-File -FilePath (Join-Path $work "members.json") -Encoding utf8
    if ($LASTEXITCODE -ne 0) { throw "REA managed member inventory failed" }
    Write-Host "PASS: Evidence written to $work"
} finally {
    Pop-Location
}
