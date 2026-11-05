# Builds release/CIVI/CIVI.exe from the Unity project. Models in Game/Assets are already generated;
# regenerate them with tools/rebuild.sh only after changing tools/blender.
$root = Split-Path $PSScriptRoot -Parent
$version = (Get-Content "$root/Game/ProjectSettings/ProjectVersion.txt" | Select-String 'm_EditorVersion:').Line.Split(' ')[1]
$unity = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"
if (-not (Test-Path $unity)) { Write-Error "Can Unity $version (kem Windows Build Support). Cai qua Unity Hub."; exit 1 }
$log = "$root/Game/unity-batch.log"
$args = @('-batchmode', '-nographics', '-quit', '-projectPath', "`"$root/Game`"", '-executeMethod', 'Civi.EditorTools.CiviBuild.Build', '-logFile', "`"$log`"")
$p = Start-Process -FilePath $unity -ArgumentList $args -Wait -PassThru -NoNewWindow
if ($p.ExitCode -ne 0) { Write-Error "Unity build failed, see $log"; exit $p.ExitCode }
Write-Host "Da build xong: release/CIVI/CIVI.exe"
