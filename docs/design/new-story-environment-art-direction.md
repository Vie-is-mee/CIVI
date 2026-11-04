# CIVI – New Story Environment Art Direction v0.1
Date: 2026-10-29
Source of truth: nhung_nguoi_khong_co_cho_duoi_bau_troi_phien_ban_phan_nhanh.md (Drive, modified 2026-10-28).
Status: art direction + concept production; story-to-level adaptation is NOT approved canon. Existing Trang 17 Unity scenes must remain untouched.

## Decision
Priority: two cohesive single-frame concept images: Ria Mua alley and Loi Sang civic core. These define the visual language for the remaining 11 environments.
Camera: 2D orthographic 3/4 (not 3D isometric camera); 16:9 target, a contiguous wide walkable path and room for player/NPC overlays. Do not paint a playable character or UI into the scene.
Style: original stylized hand-painted adventure background, medium stylization; clear objects at 1080p; avoid excessive micro-details and overly dramatic cinematic vanishing-point angles. Use independent layers, props and colliders for runtime.
World art logic: Loi Sang orderly/ivory/cyan/filtered sunlight, polished but not utopian proof; Ria Mua patched/rust/warm local light/community, condensation from cooling infrastructure (not merely natural rain); storm affects both areas.
Typography: no AI-rendered Vietnamese signs in flattened paintings; leave blank plaques and compose verified text separately in Unity.

## Production registry
[P0] BG_LQ_CITY — full city / opening vista, not walkable; chapters 1,4,8,10.
[P0] ENV_RM_ALLEY — explorable Ria Mua route including umbrella-repair stall and makeshift bridge; chapters 2,7,8,10.
[P0] ENV_LQ_CONTROL — Mai's allocation office and energy map; chapters 1,3,5,7-9.
[P0] ENV_RM_FILTER19 — filter station, maintenance corridor, valve pressure chamber; chapters 2,8,9.
[P0] ENV_RM_CLINIC — Thu's medical station and emergency backup; chapters 5,8-10.
[P1] ENV_STORM_SHELTER — civilian shelter; chapters 8-10.
[P1] BG_EMERGENCY_CONSULT — crisis hearing/remote link room, branching A/B/C.
[P1] ENV_LQ_GATE — control gate and suspended rail, chapters 2,4,8.
[P1] ENV_RM_HA_HOME — Ha family home, chapter 2 and branch follow-ups.
[P1] ENV_VD_PUBLIC_SERVICE — public service waiting hall, chapter 3.
[P2] ENV_LQ_ACADEMY — academy and air-filtration demonstration, chapter 4, ending.
[P2] ENV_LQ_ARCHIVE — basement archive and historic photos, chapter 6.
[P2] ENV_RM_WORKSHOP — abandoned workshop for residents' meeting, chapter 7.

## Asset lifecycle
1. Concept/full-bleed key visual — no gameplay-compatibility claim.
2. Blockout and camera/collision map — 2D fixed-camera depth; test clearance, foot pivot and sightlines.
3. Production sprites: GROUND, STRUCTURES, PROPS, FOREGROUND, LIGHT_VFX; save source layers and export transparent PNG separately.
4. Import Unity into a NEW new-story test scene (not S00/S02 old-story scenes). Set PPU/pivot/SortingLayer, collider and occlusion, then test at 720p/1080p and movement.
5. Variants: NORMAL, WARNING, STORM, POSTSTORM; use shared geometry where possible.

## Current integration boundary (29 Oct)
Only project folders and this design brief are created in the Windows repo. Canva standalone outputs and ChatGPT-generated concept images are NOT yet synced as local image files here. Do not claim a Unity-ready background exists until image bytes are actually verified in this folder. No old story files have been overwritten. The existing 2D controller is reusable infrastructure, not new-story gameplay.

## Quality gates
A. Art consistency: matching perspective, brush language, saturation and human-world scale across Ria Mua/Loi Sang.
B. Gameplay: walkable zone unambiguous; foreground props do not permanently obscure the player; hotspots remain readable at 1080p.
C. Fidelity: no invented narrative facts; correct role of dome, rain condensate, air filters, neighborhood self-repair.
D. Delivery: native source, exported PNG layers, filename and manifest, provenance/license, Unity screenshot and independent tester review.
