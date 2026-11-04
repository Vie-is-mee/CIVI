"""Compile reviewed CIVI Ch.1 dialogue to stable cue IDs and 13 gated gameplay tasks.

Canon events and author-approved wording are sourced from
source-assets/chapter1-dialogue/Chuong_1_Ban_Duyet.md.
The 13 task boundaries preserve the existing CiviLiuChapter gameplay order.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import re
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "source-assets/chapter1-dialogue/Chuong_1_Ban_Duyet.md"
GAMEPLAY = ROOT / "Game/Assets/_Civi/Resources/LiuQuangC1/chapter.json"
OUTPUT = ROOT / "Game/Assets/_Civi/Resources/LiuQuangC1/dialogue.json"

# Half-open page ranges: (before_start, before_end, after_start, after_end).
# Explicit narrative action gates: 6 hospital rescue; 13 power transfer;
# 30 population discrepancy verified; 47 logistics dispatch; 119 director
# acknowledges submitted evidence; 150 driver time-sheet request submitted.
# Each page occurs ONCE, in canonical order, never before its required action.
RANGES = [
    ("memory",       0,   6,   6,  12),
    ("hospital",    12,  12,  12,  17),
    ("evidence",    17,  30,  30,  36),
    ("coordination",36,  47,  47,  54),
    ("driver",      54,  62,  62,  67),
    ("gallery",     67,  80,  80,  84),
    ("lunch",       84,  94,  94, 102),
    ("corridor",   102, 110, 110, 113),
    ("meeting",    113, 119, 119, 125),
    ("mother",     125, 131, 131, 137),
    ("album",      137, 144, 144, 149),
    ("request",    149, 150, 150, 151),
    ("final",      151, 153, 153, 154),
]

def pages(text: str, limit: int = 360) -> list[str]:
    remaining = text.strip()
    result = []
    while len(remaining) > limit:
        endings = list(re.finditer(r"[.!?…][\s]+", remaining[: limit + 1]))
        cut = (
            endings[-1].end()
            if endings and endings[-1].end() > limit // 3
            else remaining.rfind(" ", 0, limit + 1)
        )
        if cut <= 0:
            cut = limit
        result.append(remaining[:cut].strip())
        remaining = remaining[cut:].strip()
    if remaining:
        result.append(remaining)
    return result


def parse_markdown(markdown: str):
    current_scene = 0
    scene_titles = {}
    ordinal = Counter()
    cues = []
    editorial = False
    for line_no, raw in enumerate(markdown.splitlines(), start=1):
        line = raw.strip()
        if not line or line == "---" or line.startswith("<!--") or line.startswith("# "):
            continue
        heading = re.match(r"#{2,3} Cảnh (\d+)\s*[—–-]\s*(.+)", line)
        if heading:
            current_scene = int(heading[1])
            scene_titles[current_scene] = heading[2]
            editorial = False
            continue
        if re.match(r"#{2,3} Điểm rẽ", line):
            editorial = True
            continue
        if line.startswith("##") or editorial or line.startswith("**—") or line.startswith("*Tiếp nối:"):
            continue
        if current_scene == 0:
            continue
        speech = re.fullmatch(r"\*\*(.+?):\*\*\s+(.+)", line)
        if speech:
            who, content, kind = speech[1], speech[2], "speech"
        elif line.startswith("*[") and line.endswith("]*"):
            who, content, kind = "Lời kể", line[2:-2].replace("**", ""), "direction"
        elif line.startswith("*") and line.endswith("*") and not line.startswith("**"):
            who, content, kind = "Lời kể", line[1:-1], "direction"
        else:
            raise ValueError(f"Unparsed content at line {line_no}: {line}")
        ordinal[current_scene] += 1
        cue_id = f"C1.S{current_scene:02}.{ordinal[current_scene]:03}"
        cues.append((cue_id, current_scene, scene_titles[current_scene], line_no, who, kind, content))
    return cues, scene_titles


def compile_dialogue() -> dict:
    source_bytes = SOURCE.read_bytes()
    source = source_bytes.decode("utf-8-sig")
    cues, scene_titles = parse_markdown(source)
    if len(cues) != 148 or len(scene_titles) != 12 or set(scene_titles) != set(range(1, 13)):
        raise ValueError(f"Unexpected chapter source: {len(cues)} cues, {len(scene_titles)} scenes")
    records = []
    for cue_id, scene, title, line_no, who, kind, content in cues:
        for number, paragraph in enumerate(pages(content), start=1):
            records.append({
                "id": f"{cue_id}.p{number}",
                "speaker": who,
                "kind": kind,
                "text": paragraph,
                "scene": scene,
                "sceneTitle": title,
                "sourceLine": line_no,
            })
    if len(records) != 154:
        raise ValueError(f"Expected 154 authored pages, got {len(records)}")

    gameplay = json.loads(GAMEPLAY.read_text(encoding="utf-8"))
    task_ids = [task["id"] for task in gameplay["tasks"]]
    if task_ids != [item[0] for item in RANGES]:
        raise ValueError("Current gameplay task order changed; remap dialogue before compiling.")

    segments = []
    coverage = []
    for task_id, bs, be, a_s, ae in RANGES:
        if bs > be or be != a_s or a_s > ae:
            raise ValueError(f"Noncontiguous before/after boundary at {task_id}")
        segments.append({
            "taskId": task_id,
            "beforeStart": bs,
            "beforeCount": be - bs,
            "afterStart": a_s,
            "afterCount": ae - a_s,
        })
        coverage.extend(range(bs, be))
        coverage.extend(range(a_s, ae))
    if coverage != list(range(154)):
        raise ValueError("Missing, repeated, or out-of-order pages.")

    # Causal gates: important outcome narration must not be shown before
    # the player completes the action in the indicated gameplay task.
    gates = {
        "memory": 6,        # Green bar, treatment granted
        "hospital": 12,     # Power allocation must be AFTER gameplay action
        "coordination": 47, # Dispatch and energy relief happen AFTER the action
        "meeting": 119,     # Vinh assigns verification AFTER evidence is inspected
        "request": 150,     # Driver working-hours request is sent AFTER Submit
    }
    for task, page in gates.items():
        seg = next(s for s in segments if s["taskId"] == task)
        if not seg["afterStart"] <= page < seg["afterStart"] + seg["afterCount"]:
            raise ValueError(f"Premature outcome narration in {task} at page {page}")
    if segments[11]["afterStart"] != 150:
        raise ValueError("Driver request action gate must occur before source page 150")

    payload = json.dumps(records, ensure_ascii=False, separators=(",", ":"))
    return {
        "schema": "civi-liuquang-dialogue-1",
        "sourceFile": SOURCE.name,
        "sourceSha256": hashlib.sha256(source_bytes).hexdigest(),
        "pagesSha256": hashlib.sha256(payload.encode("utf-8")).hexdigest(),
        "sceneCount": len(scene_titles),
        "cueCount": len(cues),
        "pages": records,
        "segments": segments,
    }


def main() -> None:
    p = argparse.ArgumentParser()
    p.add_argument("--check", action="store_true", help="Validate generated asset without overwriting it")
    args = p.parse_args()
    compiled = compile_dialogue()
    if args.check:
        existing = json.loads(OUTPUT.read_text(encoding="utf-8"))
        if existing != compiled:
            raise SystemExit("Dialogue asset stale: rerun python tools/compile_liuquang_dialogue.py")
    else:
        OUTPUT.write_text(json.dumps(compiled, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(
        "CIVI CH1 DIALOGUE PASS:"
        f" {compiled['cueCount']} cues, {len(compiled['pages'])} pages,"
        f" {compiled['sceneCount']} scenes, {len(compiled['segments'])} gated tasks."
    )


if __name__ == "__main__":
    main()
