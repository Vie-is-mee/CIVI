# CIVI — Development guide

## Prerequisites

| Tool | Purpose |
| --- | --- |
| Unity Editor **6000.4.0f1** (URP), **Windows Build Support** | Open/build `Game/` for Windows |
| Python **3.12** | Content compilation and dialogue tests |
| .NET SDK **10.0** | Chapter/domain test harness |
| Git Bash for Windows | `tools/*.sh` scripts and `cygpath` |
| Blender (optional for model regeneration) | `tools/blender/*.py` |
| FFmpeg (for audio import) | Convert authorized source WAV files to Ogg |

The project currently targets Windows. Do **not** upgrade the Unity editor version without testing the scene, render pipeline, packages and build.

## Initial setup

1. Clone the repository or open the prepared local project.
2. Install the Unity version specified in `Game/ProjectSettings/ProjectVersion.txt`.
3. Open `Game/` through Unity Hub. Wait for package/import operations.
4. If you are an authorized team member with cleared media rights, place the original audio packages under `AUDIO/` using the expected structure in `tools/import_audio.py`.
5. For audio recreation, run `python tools/import_audio.py` **after** preparing the Chapter 1–3 JSON data and ensuring `ffmpeg` is on the PATH.

`AUDIO/` and generated `Game/Assets/_Civi/Resources/Audio/` are Git-ignored until the relevant licensing issues are resolved. Without them, source code and story tests still work, but full audio is unavailable.

## Build and play

- Double-click `CHOI_GAME.bat` on Windows to launch an existing `release/CIVI/CIVI.exe` or trigger a build when it is missing.
- Alternatively, run `powershell -ExecutionPolicy Bypass -File tools/Build-CIVI.ps1`.
- The launcher and build scripts use the editor version locked by `ProjectVersion.txt`.
- Output is written into the ignored `release/CIVI/` directory; ship a complete build directory, not only the EXE.

## Content generation

- Approved text belongs in `dialogue/`. Narrative compilation is implemented by `tools/compile_chapter_dialogue.py` and `tools/compile_chapters.py`.
- Blender source scripts live in `tools/blender/`; resulting 3D models live under `Game/Assets/_Civi/Models/`.
- Generated scene `CIVI-Main.unity` is maintained by the editor builder, not by manual scene editing.
- Full regeneration, where prerequisites are installed: `bash tools/rebuild.sh`; selective modes: `world`, `chars`, `story`.
- For Blender discovery, set `BLENDER_BIN` to an executable path if it is not already in your PATH. For Unity's shell wrapper, `UNITY_EDITOR` overrides the default editor path.

The regeneration scripts are not a substitute for checking models, materials, dialogue mapping, and scene composition inside Unity.

## Tests

```powershell
py -3.12 tools/test_dialogue_content.py
dotnet run --project tools/ChapterTests
python -m compileall -q tools
```

On the prepared local Windows copy (18 November 2026), the dialogue suite reported **7 passing tests** and the chapter harness reported **1,094 passing checks**. These checks do not prove that a fresh Unity build or every in-game interaction passes on another device.

The interactive QA script `bash tools/qa.sh artifacts/qa` requires a locally built game; screenshots and logs are saved outside Git tracking.

## Source of truth and Git workflow

- Source snapshot: **Google Drive folder `CIVI_8`** (5 November 2026). The companion `CIVI_8.zip` contains an older internal folder name `CIVI_7`; do not treat that name as a separate release.
- The local source snapshot was checked against the previously extracted source; Unity cache/`artifacts/`/local builds are deliberately excluded from Git.
- Historical notes now live in `docs/history/`. They describe earlier decisions and are **not** necessarily current instructions.
- Git history/remote originated from the earlier CIVI repository; work for the Drive-based 3D project is isolated on a dedicated integration branch.
- **No automatic Drive↔Git sync is configured.** Changes on the machine do not upload back to Google Drive.

Before a push: run source tests, inspect `git status` and `git diff --stat`, check the staged file list and sizes, verify asset licenses and third-party notices, then commit explicitly. Do not publish files ignored for license/security reasons.
