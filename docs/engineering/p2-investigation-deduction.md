# CIVI / P2 — Investigation & Deduction (27/10/2026)

## Scope

P2 introduces guided evidence inspection and a provisional deduction notebook at S03. The canonical TV1 authoring JSON is unchanged. **StorySession alone** controls evidence_seen, facts_discovered, choice conditions, scene transitions, and any authorized effects. The notebook neither grants consent nor changes story facts.

### Features

- Inspect the original imported images PHOTO_0441, PHOTO_0442, PHOTO_0443, PHOTO_CLAMP_EMPTY. Zoom with mouse wheel or +/- and drag to pan.
- Answer a 3-option observation question based on each image. Only the correct, source-grounded description triggers the existing authored StorySession choice. Wrong answers show feedback without evidence rewards.
- B opens a caseboard showing the four observed sources and compares 0441 ↔ 0442 or 0442 ↔ 0443 if the player has already inspected both.
- The deduction panel provides three theories: intentional exclusion (unsupported), absence from the original frame requiring verification (working hypothesis once 0443 plus one other source observed), and implied publishing permission (unsupported). No permanent new fact is awarded for an inference.
- S03_SOURCE remains gated by all four authored observations and leads only to the S04 preview boundary.
- The existing 2D save records local observation and hypothesis state; old P0 saves remain compatible.

### How to run

Open the Game project in Unity 6000.6.6f1, wait for script compile, select CIVI > TV2 > Mở scene khám phá 2D, and Play. Or go S00 → S01 → KHÁM PHÁ 2D / S02 to carry previous choices into the new level.

Move WASD or arrow keys; E interacts. Complete S02 equipment inspection and exit to S03. Near each of the four sources press E, zoom/pan, select the correct observation. Press B to compare and try a hypothesis; then interact with VIDEO to continue to the S04 boundary. F5/F9 save/load. Esc closes optional inspector or notebook, while Enter/Space advances mandatory dialogue.

### Source files

- Game/Assets/_Civi/Scripts/Domain/Evidence/InvestigationNotebook.cs — pure-C# local evidence notebook and hypothesis guardrails.
- Game/Assets/_Civi/Scripts/Presentation/Evidence/CiviEvidenceWorkbenchUI.cs — interactive image viewer, original evidence textures, comparison and deduction UI.
- Game/Assets/_Civi/Scripts/Application/Exploration/CiviExploration2D.cs — runtime integration, canonical state commit and save/load.
- Game/Assets/_Civi/Editor/Exploration/CiviExplorationSceneBuilder.cs — Unity P2 editor smoke check.
- tools/InvestigationTests — 38 pure-C# assertions; tools/StoryRuntimeTests — 69 prior assertions.
- artifacts/backup-p2-investigation — P1 backup.

### Verification and remaining production work

Unity 6000.6.6f1 isolated-project compilation and Editor smoke: PASS. Pure P2 model tests: 38 PASS. Story 15-scene content validator retained. **Not yet a Unity PlayMode QA approval**, nor a Windows release build.

This P2 implements **guided multiple-choice observation after zoom/pan**, not pixel-accurate hotspot detection, free-form evidence-link drawing or finished visual effects. For a commercial release, develop exact visual landmarks, UI polish, sound/game feel, accessibility and playtest evidence; integrate author-approved interaction flow and consent validation before extending beyond S04. Do not assume attendance or appearing in a photo means permission to publish.
