# CIVI — Architecture

## High-level data flow

```text
STORY.md + approved dialogue/
             │
             ▼
tools/compile_chapter_dialogue.py
tools/compile_chapters.py
             │
             ▼
Game/Assets/_Civi/Resources/Chapter{1,2,3}/ (JSON)
             │
             ▼
Game/Assets/_Civi/Scripts/ (C# domain + game systems)
             │
             ▼
Unity editor/builder ──────────────► release/CIVI/ (Windows player)
             ▲
             │
tools/blender/*.py ──► Models/ (generated FBX/materials)
             ▲
             │
character-design/ + source-assets/art/

Authorized AUDIO/ packages ──► tools/import_audio.py (FFmpeg)
                            └─► Resources/Audio/ (local, ignored)
```

## Main responsibilities

| Location | Responsibility |
| --- | --- |
| `Game/Assets/_Civi/Scripts/Domain` | Narrative progression, state and side-quest logic |
| `Game/Assets/_Civi/Scripts/` | Gameplay behavior, interactions and presentation |
| `Game/Assets/_Civi/Editor` | Scene/build tooling |
| `Game/Assets/_Civi/Resources/Chapter*` | Compiled story and quest data |
| `Game/Assets/_Civi/Models` | Generated 3D objects included in Unity source |
| `tools/blender/` | Reproducible procedural world/character creation |
| `tools/ChapterTests/` | Isolated logic test harness with linked domain sources |
| `docs/qa/` | Reviewed dialogue/QA source references |

## Reproducibility boundary

The Git repository stores the playable prototype's source assets and code, including the imported runtime Ogg audio, cues.json and Unity metadata. Locally generated Unity `Library` data, temporary render/debug artifacts, editor state, and release build outputs remain outside version control.

The Windows build is validated separately from the pure source tests. Regenerating runtime audio from original WAV production packs requires the separately stored `AUDIO/` archives and FFmpeg; normal cloning/building does not.
