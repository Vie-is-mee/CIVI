"""Validate CIVI school Ch1 authoring data; read-only and independent of Unity."""
import json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
B=ROOT/"Game/Assets/_Civi/Content/Story/v4_1"
def check(s):
    scene=json.loads((B/"scenes"/(s+".json")).read_text(encoding="utf-8"))
    dialogue=json.loads((B/"dialogue"/(s+".json")).read_text(encoding="utf-8"))
    assert scene["scene_id"]==dialogue["scene_id"]==s
    ids={x["step_id"] for x in scene["steps"]}
    lineids={x["text_id"] for x in dialogue["lines"]}
    assert len(lineids)==len(dialogue["lines"])
    referenced=set()
    mapstep={x["step_id"]:x for x in scene["steps"]}
    choiceids=set()
    def visit(node,seen):
        assert node not in seen,(s,"cycle",node)
        step=mapstep[node]
        for line in step.get("lines",[]):
            assert line in lineids;(referenced.add(line))
        endings=[]
        for choice in step.get("choices",[]):
            key=choice["choice_id"]
            choiceids.add(key)
            for line in choice.get("lines",[]):
                assert line in lineids;(referenced.add(line))
            transition=choice["next"]
            if transition.startswith("step:"):
                target=transition[5:]
                assert target in ids
                endings+=visit(target,seen|{node})
            else:
                assert transition.startswith("scene:"),transition
                endings.append(transition)
        return endings
    terminal=visit(scene["entry_step"],set())
    expected="scene:S01" if s=="S00" else "scene:S02"
    assert terminal and set(terminal)=={expected},(s,terminal)
    assert referenced==lineids,(s,"dangling lines",lineids-referenced)
    print(f"PASS {s}: {len(ids)} steps, {len(choiceids)} choices, {len(lineids)} dialogue lines, {len(terminal)} authored routes -> {expected}")
    return scene
def main():
    s00=check("S00")
    s01=check("S01")
    for asset in ("PHOTO_S00_GOOD","PHOTO_S00_BAD_01","PHOTO_S00_BAD_02","PROP_SIGN_HEN","PROP_CLAMP"):
        assert asset in s00["asset_ids"],asset
    assert "PROP_CHAT_REMOVE" in s01["asset_ids"]
    assert s00["background_id"]=="BG_COURTYARD" and s01["background_id"]=="BG_DESIGN_ROOM"
    print("PASS CH1: all declared opening-photo assets and scene backgrounds referenced correctly.")
if __name__=="__main__":main()
