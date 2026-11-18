# Source assets and distribution

CIVI's GitHub repository includes the assets required by the current Unity scenes, including imported runtime audio. The source layout separates playable content from generated build output and optional production archives.

## Included in Git

- Unity scripts, packages, project settings, scenes, materials and their `.meta` GUID files.
- All character and environment FBX models needed by the scene builder.
- Chapter 1–3 dialogue JSON, fonts, images and other Unity Resources.
- **283 runtime `.ogg` clips**, `Game/Assets/_Civi/Resources/Audio/cues.json`, and associated `.meta` files.
- Narrative/source tests, tooling, documentation and nine game screenshots.

## Not included in Git

| Path | Reason |
| --- | --- |
| `AUDIO/` | Original high-resolution WAV production archives (`3 GB); not used by the existing game build |
| `source-assets/audio/` | Previously duplicated production audio archives |
| `release/` | Generated Windows player/build output; distribute the full build directory separately |
| `Game/Library/`, `Game/Temp/`, `Game/Logs/` | Unity caches, temporary data and editor logs |
| `artifacts/` | QA images, output and test runs; curated screenshots are in `docs/screenshots/` |
| `tools/vendor/` | Unused legacy character-generation experiment assets |

## Verification and release

1. Run `python tools/check_runtime_assets.py` to ensure build-critical assets and audio references exist.
2. Run the Python dialogue tests and .NET narrative/domain tests.
3. Build and play in Unity 6000.4.0f1 to verify scenes, shaders, audio and progression.
4. For a direct-play release, distribute the entire Windows build folder via GitHub Releases, not just the executable.

The CIVI team has indicated its game assets may be published as part of this project's demo. This does not imply a blanket license for unrelated reuse.
