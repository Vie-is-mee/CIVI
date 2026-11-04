"""Promote only the reviewed chapter files and validated player; retain original scenes/builds."""
from pathlib import Path
import hashlib,json,shutil
ROOT=Path(__file__).resolve().parents[1]
A=ROOT/'artifacts/chapter1-polish-2026-11-01'
WORK=A/'work/Game'
TARGET=ROOT/'Game'
BACKUP=A/'backup-main-before-sync'
files=[
    'Assets/_Civi/Scripts/Application/Exploration/CiviSchoolWorldV2.cs',
    'Assets/_Civi/Scripts/Application/Exploration/CiviSchoolStudioV2.cs',
    'Assets/_Civi/Scripts/Application/Exploration/CiviChapterEnvironment.cs',
    'Assets/_Civi/Scripts/Application/Exploration/CiviChapterPolished.cs',
    'Assets/_Civi/Scripts/Application/Exploration/CiviChapterPolishedUI.cs',
    'Assets/_Civi/Editor/Exploration/CiviChapterPolishBuilder.cs',
    'Assets/_Civi/Editor/Exploration/CiviChapterGameViewQA.cs',
    'Assets/_Civi/Scenes/CIVI-Chapter1-Polished-Courtyard.unity',
    'Assets/_Civi/Scenes/CIVI-Chapter1-Polished-DesignRoom.unity',
    'ProjectSettings/EditorBuildSettings.asset',
    'ProjectSettings/ProjectSettings.asset',
]
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def copy(relative):
    src=WORK/relative;dst=TARGET/relative
    assert src.is_file(),src
    if dst.exists():
        backup=BACKUP/relative;backup.parent.mkdir(parents=True,exist_ok=True)
        if not backup.exists():shutil.copy2(dst,backup)
    dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
    assert digest(src)==digest(dst)
    manifest.append({'path':relative,'sha256':digest(dst)})
baseline=json.loads((A/'QA/canon-and-legacy-hashes.json').read_text())
for row in baseline:
    assert digest(TARGET/'Assets/_Civi'/row['path'])==row['sha256'], 'Original source changed; re-review before syncing'
assert 'POLISH WINDOWS BUILD PASS' in (A/'QA/unity-build.log').read_text(encoding='utf-8-sig')
for name in ['release-720p','release-1080p','release-1440p']:
    trace=json.loads((A/'QA'/name/'input-trace.json').read_text(encoding='utf-8-sig'))
    assert trace[-1]['test']=='PASS-real-input-end-to-end', name
manifest=[]
for f in files:
    copy(f)
    if (WORK/(f+'.meta')).exists():copy(f+'.meta')
art='Assets/_Civi/Art/Chapter1Polish'
copy(art+'.meta')
for p in (WORK/art).rglob('*'):
    if p.is_file():copy(str(p.relative_to(WORK)))
release=ROOT/'release/Chapter1Polished'
shutil.copytree(WORK/'release',release,dirs_exist_ok=True)
assert digest(WORK/'release/CIVI_Chapter1_Polished.exe')==digest(release/'CIVI_Chapter1_Polished.exe')
launcher=ROOT/'PLAY_CIVI_CHAPTER1.bat'
backup=BACKUP/'PLAY_CIVI_CHAPTER1.bat'
if not backup.exists():shutil.copy2(launcher,backup)
launcher.write_text('@echo off\nsetlocal\ncd /d "%~dp0release\\Chapter1Polished"\nif not exist "CIVI_Chapter1_Polished.exe" (\n  echo [CIVI] Khong tim thay ban Chapter 1 Polished.\n  pause\n  exit /b 1\n)\nstart "CIVI - Chapter 1" "CIVI_Chapter1_Polished.exe" -screen-width 1280 -screen-height 720 -screen-fullscreen 0\nendlocal\n',encoding='ascii')
(A/'QA/synced-files.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print('SYNC PASS:',len(manifest),'source/config/asset files; original scenes and releases retained.')
