"""Adapt reviewed chapter-one cues to the existing nine playable objectives."""
from collections import Counter
from pathlib import Path
import hashlib
import json
import re
from dialogue_content import DIALOGUE, parse


def pages(text, limit=360):
    remaining = text.strip()
    result = []
    while len(remaining) > limit:
        endings = list(re.finditer(r"[.!?…][\s]+", remaining[:limit+1]))
        cut = endings[-1].end() if endings and endings[-1].end() > limit//3 else remaining.rfind(" ", 0, limit+1)
        if cut <= 0: cut = limit
        result.append(remaining[:cut].strip()); remaining = remaining[cut:].strip()
    if remaining: result.append(remaining)
    return result


def compile_dialogue(canonical):
    source = DIALOGUE / "Chuong_1_Ban_Duyet.md"
    scenes, cues, errors = parse(source)
    assert not errors, errors
    assert len(scenes) == 12, "Review chapter-one scene mapping before compiling"
    grouped = {n:[i for i,c in enumerate(cues) if c.scene==n] for n,_,_ in scenes}

    def anchor(scene, text):
        found = [i for i in grouped[scene] if text in cues[i].text]
        assert len(found)==1, f"Dialogue action anchor changed: scene {scene}, {text}"
        return found[0]

    transfer = anchor(2,"Buổi sáng hôm ấy, Mai chuyển")
    dossier = anchor(3,"Nhưng không thể cứ thế mà bỏ qua")
    logistics = anchor(4,"Mai trải các phương án lên bàn")
    report = anchor(9,"Vinh không bác bỏ lời cô")
    followup = anchor(12,"Cô mở lại sổ việc")
    ranges = [
        ([], list(range(transfer,grouped[2][-1]+1))),
        (list(range(grouped[3][0],dossier)),list(range(dossier,grouped[3][-1]+1))),
        (list(range(grouped[4][0],logistics)),list(range(logistics,grouped[5][-1]+1))),
        (grouped[6],[]),
        (grouped[7]+grouped[8],[]),
        (list(range(grouped[9][0],report)),list(range(report,grouped[9][-1]+1))),
        (grouped[10],[]), (grouped[11],[]),
        (list(range(grouped[12][0],followup)),list(range(followup,grouped[12][-1]+1))),
    ]
    intro = list(range(transfer))
    order = intro + [i for before,after in ranges for indices in [before,after] for i in indices]
    assert order == list(range(len(cues))), "Dialogue omitted, duplicated or reordered"
    records=[]; expanded={}; ordinal=Counter(); titles={n:t for n,t,_ in scenes}
    portraits={"Mai":0,"Linh":1,"Vinh":2,"Mẹ Mai":3}
    for index,cue in enumerate(cues):
        ordinal[cue.scene]+=1
        cue_id=f"C1.S{cue.scene:02}.{ordinal[cue.scene]:03}"
        expanded[index]=[]
        for page,text in enumerate(pages(cue.text),1):
            expanded[index].append(len(records))
            records.append(dict(id=f"{cue_id}.p{page}", sourceCueId=cue_id, speaker=cue.speaker,
                kind=cue.kind, text=text, portrait=portraits.get(cue.speaker,-1),
                scene=cue.scene, sceneTitle=titles[cue.scene], sourceLine=cue.line))
    def expand(indices): return [page for i in indices for page in expanded[i]]
    steps=[]
    for old,(before,after) in zip(canonical["steps"],ranges):
        step=dict(old); step["before"]=expand(before); step["after"]=expand(after); steps.append(step)
    payload=json.dumps(records,ensure_ascii=False,separators=(",",":"))
    return dict(title=canonical["title"],chapter=canonical["chapter"],
        sourceHash=hashlib.sha256(payload.encode()).hexdigest(),canonHash=canonical["sourceHash"],
        dialogueSourceHash=hashlib.sha256(source.read_bytes()).hexdigest(),dialogueSource=source.name,
        sourceCueCount=len(cues), sourceSceneCount=len(scenes),
        canonParagraphs=canonical["paragraphs"],
        legacyIntroCount=len(canonical["intro"]),legacyBeforeCounts=[len(s["before"]) for s in canonical["steps"]],
        legacyAfterCounts=[len(s["after"]) for s in canonical["steps"]],
        paragraphs=[r["text"] for r in records],speakers=[r["speaker"] for r in records],
        portraits=[r["portrait"] for r in records],kinds=[r["kind"] for r in records],
        cueIds=[r["id"] for r in records],sceneIds=[r["scene"] for r in records],
        sceneTitles=[r["sceneTitle"] for r in records],sourceLines=[r["sourceLine"] for r in records],
        intro=expand(intro),steps=steps)
