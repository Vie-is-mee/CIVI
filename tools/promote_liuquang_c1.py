"""Promote the tested isolated C1 slice additively; preserve old content and QA."""
import hashlib,json,shutil
from pathlib import Path
root=Path(__file__).resolve().parents[1]
artifact=root/'artifacts/chapter1-liuquang-2026-11-02'
source=artifact/'work/Game'
destination=root/'Game'
for resolution in ('720p','1080p','1440p'):
    trace=json.loads((artifact/f'QA/release-{resolution}/input-trace.json').read_text(encoding='utf-8'))
    assert any(e['test']=='PASS' for e in trace),f'Unpassed {resolution}'
snapshot=json.loads((artifact/'baseline-sha256.json').read_text())
baseline=root/'artifacts/chapter1-polish-2026-11-01/work/Game'
assert all(hashlib.sha256((baseline/p).read_bytes()).hexdigest()==h for p,h in snapshot.items()),'Polished baseline changed'
paths=[Path('Assets/_Civi/Art/LiuQuangC1'),Path('Assets/_Civi/Resources/LiuQuangC1')]
paths += [Path('Assets/_Civi/Scripts/Application/Exploration')/name for name in ('CiviChapterEnvironment.cs','CiviSchoolStudioV2.cs','CiviLiuEnvironment.cs','CiviLiuChapter.cs','CiviLiuChapterUI.cs')]
paths += [Path('Assets/_Civi/Editor/Exploration/CiviLiuBuilder.cs'),Path('Assets/_Civi/Scenes/CIVI-LiuQuang-Chapter1.unity')]
files=[]
for p in paths:
    files += [f.relative_to(source) for f in (source/p).rglob('*') if f.is_file()] if (source/p).is_dir() else [p]
    meta=Path(str(p)+'.meta')
    if (source/meta).exists():files.append(meta)
manifest=[]
for rel in files:
    src=source/rel;dst=destination/rel;backup=artifact/'rollback/Game'/rel
    if dst.exists() and not backup.exists():backup.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(dst,backup)
    dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
    manifest.append({'path':str(rel),'sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'had_original':backup.exists()})
settings=destination/'ProjectSettings/EditorBuildSettings.asset';backup=artifact/'rollback/Game/ProjectSettings/EditorBuildSettings.asset'
if not backup.exists():backup.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(settings,backup)
(artifact/'promotion-manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print(f'Promoted {len(manifest)} files; original replacements backed up; old scenes/canon/launchers preserved.')
