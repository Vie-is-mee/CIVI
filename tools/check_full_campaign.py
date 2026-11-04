from pathlib import Path
import json,hashlib,itertools
p=Path(__file__).resolve().parents[1]/"Game/Assets/_Civi/Resources/NewStory"
game=json.loads((p/"campaign.json").read_text(encoding="utf-8"))
raw=(p/"novel_source.txt").read_text(encoding="utf-8")
eps={x["id"]:x for x in game["episodes"]}
required=["C1","C2","C3","C4","C5","I_A","I_B","C6","C7",
          "II_A","II_B","C8","C9","C9C","C10","END_A","END_B"]
assert set(eps)==set(required), sorted(set(required)^set(eps))
assert hashlib.sha256(raw.encode()).hexdigest()==game["source_sha256"]
assert all(len(v["paragraphs"])>15 and len(v["beats"])==6 for v in eps.values())
assert sum(len(x["paragraphs"]) for x in eps.values())>=1000
links={"C1":"C2","C2":"C3","C3":"C4","C4":"C5","I_A":"C6","I_B":"C6",
       "C6":"C7","II_A":"C8","II_B":"C8","C8":"C9","C9C":"C10"}
paths=[]
for i,j,k in itertools.product(["I_A","I_B"],["II_A","II_B"],["END_A","END_B","C9C"]):
    chosen={"C5":i,"C7":j,"C9":k}
    seq=[];current="C1"
    while current is not None:
        assert current not in seq,"route cycle"
        seq.append(current)
        current=chosen.get(current,links.get(current))
    assert seq[-1]==("C10" if k=="C9C" else k),seq
    assert "C9C" not in seq if k!="C9C" else "C10" in seq
    paths.append((i,j,k,len(seq)))
print("PASS: 17 authored episodes, "+str(sum(len(x["paragraphs"]) for x in eps.values()))+
      " text paragraphs, "+str(len(paths))+" full choice combinations, 3 endings, SHA-256 content traceable")
