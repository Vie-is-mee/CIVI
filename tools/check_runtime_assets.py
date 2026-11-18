"""Fail fast when a clean CIVI Git clone is missing source/gameplay assets.

This checks files and references only. A real Unity Editor build and playtest
are still required to prove engine/runtime compatibility.
"""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
RES = ROOT / "Game/Assets/_Civi/Resources"
AUDIO = RES / "Audio"

REQUIRED_FILES = [
    "Game/ProjectSettings/ProjectVersion.txt",
    "Game/Packages/manifest.json",
    "Game/Assets/_Civi/Scenes/CIVI-Main.unity",
    "Game/Assets/_Civi/Resources/Chapter1/CiviFont.ttf",
    "Game/Assets/_Civi/Resources/Chapter1/Art/menu.png",
    "Game/Assets/_Civi/Resources/Audio/cues.json",
    "Game/Assets/_Civi/Resources/Audio/cues.json.meta",
    "Game/Assets/_Civi/Resources/Audio.meta",
    *[f"Game/Assets/_Civi/Resources/Chapter{n}/chapter.json" for n in (1, 2, 3)],
    *[f"Game/Assets/_Civi/Models/World/{name}.fbx"
      for name in ("Floor42", "Home", "City", "Props", "Civi", "Cat", "Edge")],
]

def main() -> None:
    errors = []
    for rel in REQUIRED_FILES:
        if not (ROOT / rel).is_file():
            errors.append(f"missing: {rel}")

    if not (AUDIO / "cues.json").exists():
        raise SystemExit("\n".join(errors) or "Audio/cues.json missing")

    plan = json.loads((AUDIO / "cues.json").read_text(encoding="utf-8"))
    for category in ("cues", "beds", "banks"):
        if not isinstance(plan.get(category), list):
            errors.append(f"invalid audio plan key: {category}")

    for item in plan.get("cues", []):
        clip = item.get("clip", "")
        if not any((AUDIO / folder / (clip + ".ogg")).is_file()
                   for folder in ("Sfx", "Amb")):
            errors.append(f"missing cue clip: {clip}")

    for item in plan.get("beds", []):
        for clip in item.get("clips", []):
            if not (AUDIO / "Amb" / (clip + ".ogg")).is_file():
                errors.append(f"missing ambience bed: {clip}")

    for item in plan.get("banks", []):
        name = item.get("name", "")
        count = item.get("count", 0)
        if not isinstance(count, int) or count < 1:
            errors.append(f"invalid footstep bank count: {name}: {count}")
            continue
        for n in range(1, count + 1):
            clip = AUDIO / "Foot" / f"{name}_{n:02}.ogg"
            if not clip.is_file():
                errors.append(f"missing footstep clip: {clip.relative_to(ROOT)}")

    tracks = sorted(AUDIO.rglob("*.ogg"))
    if not tracks:
        errors.append("no runtime Ogg audio tracks found")
    for clip in tracks:
        if not clip.with_name(clip.name + ".meta").is_file():
            errors.append(f"missing Unity meta: {clip.relative_to(ROOT)}")

    for error in errors[:40]:
        print("ERROR:", error)
    if errors:
        raise SystemExit(f"Runtime asset inventory FAILED: {len(errors)} issue(s)")

    print(f"Runtime asset inventory PASS: {len(tracks)} Ogg clips, "
          f"{len(plan['cues'])} cues, {len(plan['beds'])} ambience beds, "
          f"{len(plan['banks'])} footstep banks, all required scene/content assets present")
if __name__ == "__main__":
    main()
