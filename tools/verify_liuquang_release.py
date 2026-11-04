"""Summarize already executed native input runs and verify preservation and image sizes."""
from pathlib import Path
import json,hashlib,re
from PIL import Image
root=Path(__file__).resolve().parents[1]
artifact=root/'artifacts/chapter1-liuquang-2026-11-02'
summary={}
for label,w,h in [('720p',1280,720),('1080p',1920,1080),('1440p',2560,1440)]:
    folder=artifact/'QA'/('release-'+label)
    trace=json.loads((folder/'input-trace.json').read_text(encoding='utf-8'))
    assert trace[-1]['test']=='PASS',label
    shots=[e for e in trace if e['test']=='screenshot']
    for e in shots:
        image=folder/(e['label']+'.png')
        assert Image.open(image).size==(w,h),(label,image)
        assert not e['state']['textOverflows'],(label,e['label'])
    assert len({e.get('task') for e in trace if e['test']=='missing-prerequisites-blocked'})==13
    assert len({e.get('task') for e in trace if e['test']=='workbench-movement-lock'})==13
    for log in ['player.log','player-first-run.log']:
        text=(folder/log).read_text(encoding='utf-8-sig')
        assert not re.search(r'(NullReferenceException|ArgumentException|IndexOutOfRangeException|MissingReferenceException|Error:)',text),log
    summary[label]={'pass':True,'resolution':[w,h],'native_screenshots':len(shots),'all_13_prerequisite_gates':True,'all_13_panel_movement_locks':True,'relaunch':True,'text_overflows':0}
for manifest,base in [('baseline-sha256.json',root/'artifacts/chapter1-polish-2026-11-01/work/Game'),('main-protected-sha256.json',root/'Game')]:
    data=json.loads((artifact/manifest).read_text())
    assert all(hashlib.sha256((base/p).read_bytes()).hexdigest()==v for p,v in data.items()),manifest
    summary[manifest]={'unchanged_files':len(data)}
for label,w,h in [('720p',1280,720),('1080p',1920,1080)]:
    folder=artifact/'QA'/('visual-final-'+label)
    trace=json.loads((folder/'input-trace.json').read_text(encoding='utf-8'))
    assert trace[-1]['test']=='VISUAL-PASS',label
    shots=[e for e in trace if e['test']=='screenshot']
    assert len(shots)==4
    for e in shots:
        assert Image.open(folder/(e['label']+'.png')).size==(w,h)
        assert not e['state']['textOverflows']
    summary['visual-final-'+label]={'pass':True,'genuine_checkpoint_captures':len(shots)}
summary['movement']=json.loads((artifact/'QA/movement-regression.json').read_text())
summary['canon']=json.loads((artifact/'canon/comparison.json').read_text())
(artifact/'QA/release-summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(summary,ensure_ascii=True,indent=2))
