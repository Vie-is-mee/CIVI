# CIVI / New Story — Rìa Mưa Environment Test v0.1

## Scope and handoff

This NEW scene is an independent test environment for the new novel *Những người không có chỗ dưới bầu trời*, with Rìa Mưa, the umbrella-repair stall, cooling pipes, makeshift bridge and direction to filtration station 19. It is not yet a chapter-driven game adaptation and does not use the old yearbook S00-S03 script/data.

Unity scene: `Game/Assets/_Civi/Scenes/NS01-RiaMua-EnvironmentTest.unity`.
Editor menu: **CIVI > New Story > Mở Rìa Mưa**.
Unity version: 6000.6.6f1.

## Play test

1. Open the repo's `Game` folder through Unity Hub; allow scripts to recompile.
2. Choose **CIVI > New Story > Mở Rìa Mưa**, then Play.
3. **WASD / arrows**: walk; **E**: interact when close; **Esc**: close the inspection card.
4. **Mouse wheel**: zoom camera; **F1**: cycle nominal environment state: normal, warning, storm, poststorm (the state label/background tone is not a fully realized VFX/lighting system yet).
5. Test four location cards: Umbrella Repair Stall, Cooling Pipes, Makeshift Bridge, Filtration Station 19 direction.
6. Verify no Unity Console errors, collisions at buildings/stall, camera not showing outside level, interactions accessible, and 1080p/720p UI readability.

## Architecture

- `CiviNewStoryEnvironment.cs`: procedural new-story environment prototype using Unity sprites and existing P1 2D actor/camera/depth mechanics; includes new-story location-only text. Art and chapter logic deliberately decoupled.
- `CiviNewStoryBuilder.cs`: editor creation/open and smoke check.
- Scene registered after the two old-story scenes in EditorBuildSettings. Old scenes and assets not changed.
- Art production source: `docs/design/new-story-environment-art-direction.md` and `Assets/_Civi/Art/Environments/NewStory/environment_manifest.json`.

## Validation

Unity 6000.6.6f1 isolated project: batchmode create PASS; Editor smoke PASS: 1 actor, orthographic following camera, depth-sorted structures, 4 locations. No real keyboard PlayMode QA or user playtest recorded yet.

## Art boundary

**The playable scene presently uses clean procedural 3/4 blockout art, NOT the high quality hand-painted concept PNG.** It is important not to warp a cinematic still photo into walkable geometry. Final background must be authored into separate ground/structural/prop/foreground sprite layers and then tested for correct 3/4 gameplay scale. The earlier concept images exist as reference assets outside Windows repo; no claim that they were imported. Do not mark scene as finished art.
