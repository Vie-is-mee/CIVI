# CIVI new-story art v2 — Tự đánh giá (2026-10-30)

## Context
Screenshot NS01 was a flat primitive blockout: large tiled rectangles, UI with unreadable tiny text at QHD, character indistinct, no usable art background. We replaced that as the *visual demo* with NS02; NS01 is preserved for comparison.

## Acceptance

| Gate | Assessment | Evidence / caveat |
|---|---|---|
| Named new-story background in Unity | PASS | Four imported scene previews: Rìa Mưa normal + storm, Trạm lọc 19, Lõi Sáng |
| Scene independent from old Trang 17 | PASS | NS02 separate .unity, no old StorySession references |
| Actor, camera, move controls | PASS in EditMode smoke | Controller boot, four stages, camera target |
| Visible occlusion layer | PASS in EditMode smoke | PNG alpha foreground overlay per stage, linked asset refs |
| Interactive landmarks | PASS in EditMode smoke | Rìa Mưa 4, other scenes 3 hotspots, live keyboard PlayMode not observed |
| Branch story / quests implemented | NOT DONE | Only environment exploration / location descriptions, author-approved gameplay not yet adapted |
| High-resolution native background | NOT DONE | Canva export exposed 595x335 thumbnail; Lanczos upscaled to 1672x941, no recovered details |
| Fully accurate 3/4 geometry / tilemaps / collisions | NOT DONE | Temporary rectangular foot-navigation regions; layered photograph not a real modular world |
| Game View readability at 2560x1440 | NOT YET VISUALLY VERIFIED | HUD has resolution-scaled font, should be user-tested in PlayMode |
| Full release quality | NOT DONE | Requires original-resolution source, real sprite layers, proper weather VFX and external playtest |

## Test protocol for person running Unity

Run CIVI > New Story > Open Art Vertical Slice (v2), press Play.
A) Zoom and move with WASD across the floor; confirm no roof-walking or sudden stop beyond intended perimeter.
B) Walk near gold/teal diamonds, press E; open and close panels.
C) Press Q while at Rìa Mưa to swap storm background and confirm overlays match.
D) Press 2 then 3 to switch Trạm lọc 19 and Lõi Sáng; confirm no pink missing sprites, UI clear, correct spawn.
E) Watch Unity Console for red errors and compare against NS01 old blockout.
F) Test at 1280x720 and 2560x1440 to inspect HUD readability and any stretched/blurred textures.
Report a screenshot and observed glitches only; developer will handle fixes.

## Production blocker — no false PASS

Only **Canva thumbnail images** could be retrieved through the connected app + remote transport. The 1672x941 images are present in the ChatGPT file workspace but were not accessible as original-resolution bytes on the authorized Windows machine through the connected Canva image API. Therefore the new scene is a useful visual prototype, NOT a production-quality art handoff. The imported PNGs have native dimensions 1672x941 but limited visual detail from upscaling. Do not claim full resolution fidelity merely based on file dimensions.

Verified tests: Unity 6000.6.6f1 batchmode NS02 scene creation PASS, EditMode smoke PASS (all four scenes), legacy 69/69 tests, P2 38/38, TV1 story content validator PASS. No commit/push; root folder has no .git.
