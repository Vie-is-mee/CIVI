# CIVI — New Story Art Vertical Slice v2

Date: 2026-10-30
Context: implementation request after NS01 screenshot showed blocky gameplay.
Source of truth: "Những người không có chỗ dưới bầu trời", branch narrative dated 2026-10-28. This v2 is only an exploratory art environment, NOT the completed new story script or 10-chapter Unity adaptation.

## Start — tester only

1. Open the existing `Game` Unity project in **Unity 6000.6.6f1**, wait for import/compile.
2. Menu **CIVI > New Story > Open Art Vertical Slice (v2)** (or open `Assets/_Civi/Scenes/NS02-NewStory-ArtVerticalSlice.unity`).
3. Press **Play**.
4. Use WASD / arrow keys to move; mouse wheel zoom; approach a glowing diamond and press E for a brief evidence-neutral location description.
5. 1 — Rìa Mưa normal; Q — toggle its storm variant; 2 — Trạm lọc 19; 3 — Lõi Sáng; E / Esc closes a location detail.
6. Inspect character/world scale, camera bounds, movement navigation, readable HUD and image foreground occlusion. Compare against `NS01-RiaMua-EnvironmentTest.unity` (earlier blocky greybox).

## Delivered

- Four 16:9 preview art environments: Rìa Mưa normal, Rìa Mưa storm, Trạm lọc 19, Lõi Sáng civic square.
- Matching cropped foreground masks for depth occlusion, per scene.
- 3-direction 2D temporary sprite for the playable stand-in labeled Mai (not approved character design).
- Camera follow/zoom adapted from P1, manually authored traversable floor bounds and 3–4 story-grounded hotspots per environment.
- Scale-adaptive IMGUI header, bottom action prompts and location-card popups.
- Old S00, S02 and NS01 scenes retained. A new NS02 art scene appears in Build Settings.
- Canva originals are separately editable in Canva: Rìa Mưa reference document DAHZc8bM4Bk; production concept designs also created for all four image variants.

## Critical image fidelity caveat

The ChatGPT-generated original PNG files are 1672 × 941. The connected Canva/media export path available to the Windows integration provided **595 × 335 thumbnail files**. `tools/prepare_newstory_art.py` upsamples these previews to 1672 × 941 by Lanczos and mild sharpening. **Upsampling does not restore original detail**; they are NOT native-resolution production art. Source size and SHA-256 are documented by `Assets/_Civi/Art/Environments/NewStory/Production/ArtPreview/art_preview_manifest.json`. Avoid calling these production-quality original PNGs. Full-resolution image delivery, layered sprite extraction from full-res source, and artist sign-off remain open.
The bitmap layers are a visual prototype, not a geometrically accurate Tilemap. Navigation rectangles are provisional and must be re-authored from final collision masks. The 4 painted backgrounds do not have story content implemented.

## Source paths

- `Game/Assets/_Civi/Scenes/NS02-NewStory-ArtVerticalSlice.unity` — new scene.
- `Game/Assets/_Civi/Scripts/Application/Exploration/CiviNewStoryArtSlice.cs` — game runtime.
- `Game/Assets/_Civi/Editor/Exploration/CiviNewStoryArtBuilder.cs` — builder, menu and smoke test.
- `Game/Assets/_Civi/Art/Environments/NewStory/Production/ArtPreview/` — preview textures and simple actor sprites.
- `Game/Assets/_Civi/Art/Environments/NewStory/Concepts/` — Canva source thumbnails.
- `tools/prepare_newstory_art.py` — repeatable non-destructive preview conversion script.
- `artifacts/backup-v2-art-slice/EditorBuildSettings.asset` — backup of old Build Settings.
- `docs/design/new-story-environment-art-direction.md` — design / narrative mapping.

## QA gates

- Unity 6000.6.6f1 on an isolated copy: compilation, scene creation and editor smoke **PASS** for four scene variants, non-null textures/foreground layers, actor, camera and hotspots.
- **Not yet verified**: true keyboard PlayMode feel on user's GPU, QHD text clarity, visible masking vs walk regions, reliable E key at all landmarks, FPS and Windows player build.
- Content integration: 69 existing Story Runtime assertions, 38 Investigation assertions, and story authoring validation run separately; NS02 does NOT consume old StorySession or accidentally alter it.
- No git commit/push; the Windows folder currently is not a git worktree.

## Next production requirements

1. Obtain original 1672 × 941 or higher export bytes from Canva/image generation in the Windows project, replace all thumbnail-sourced preview textures without changing the scene schema.
2. Hand-separate near/far foreground structures and props into independent transparent sprites; replace coarse rectangle navigation with per-location collision/waypoint paths.
3. Approve Mai/NPC art direction and rebuild 4-direction animation assets plus footsteps / VFX / weather.
4. Convert 10-chapter story and branching narrative to authored Unity domain data separately; do not mix with legacy yearbook S00–S03.
5. Real PlayMode test on the student's editor and a playable Windows build before release gate.
