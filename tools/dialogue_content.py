"""Parse/review the author's Markdown dialogue without rewriting the manuscript."""
from dataclasses import dataclass
from pathlib import Path
from collections import Counter
import hashlib
import json
import re

ROOT = Path(__file__).resolve().parents[1]
DIALOGUE = ROOT / "dialogue"


@dataclass
class Cue:
    file: str
    line: int
    scene: int
    speaker: str
    text: str
    kind: str


def parse(path):
    cues, scenes, errors = [], [], []
    scene = 0
    editorial = False
    for number, raw in enumerate(path.read_text(encoding="utf-8-sig").splitlines(), 1):
        line = raw.strip()
        if not line or line == "---" or line.startswith("<!--"):
            continue
        if line.startswith("# "):
            continue
        heading = re.match(r"#{2,3} Cảnh (\d+)\s*[—–-]\s*(.+)", line)
        if heading:
            scene = int(heading[1]); editorial = False; scenes.append((scene, heading[2], number)); continue
        if re.match(r"#{2,3} Điểm rẽ", line):
            editorial = True; continue
        if line.startswith("##"):
            continue
        if not scene or editorial or line.startswith("**—") or line.startswith("*Tiếp nối:"):
            continue
        speech = re.fullmatch(r"\*\*(.+?):\*\*\s+(.+)", line)
        if speech:
            cues.append(Cue(path.name, number, scene, speech[1], speech[2], "speech")); continue
        if line.startswith("*[") and line.endswith("]*"):
            cues.append(Cue(path.name, number, scene, "Lời kể", line[2:-2].replace("**", ""), "direction")); continue
        if line.startswith(("**", "*[")):
            errors.append((number, "Unparsed dialogue/direction", line))
        elif line.startswith("*") and line.endswith("*"):
            # Written objects/letters belong to narration, not an invented speaking character.
            cues.append(Cue(path.name, number, scene, "Lời kể", line[1:-1], "direction"))
        else:
            errors.append((number, "Editorial or unclassified line", line))
    return scenes, cues, errors


def inventory():
    result = []
    for path in sorted(DIALOGUE.glob("*_Ban_Duyet.md")):
        scenes, cues, errors = parse(path)
        result.append(dict(file=path.name, sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
            scenes=scenes, cues=len(cues), spoken=sum(c.kind == "speech" for c in cues),
            speakers=dict(Counter(c.speaker for c in cues if c.kind == "speech")), errors=errors))
    return result


if __name__ == "__main__":
    import sys
    sys.stdout.reconfigure(encoding="utf-8")
    print(json.dumps(inventory(), ensure_ascii=False, indent=2))
