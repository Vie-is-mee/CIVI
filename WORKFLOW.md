# WORKFLOW — CODE, SCENE VÀ RENDER CHO UNITY 3D

**Phiên bản:** 1.0 — 04/11/2026.  
**Thiết kế cần thực hiện:** [PLAN_1.md](PLAN_1.md).  
**Truyện chuẩn:** [STORY.md](STORY.md); bản thoại tác giả: [dialogue/README.md](../dialogue/README.md).  
**Project đích dự kiến:** `D:\VSIC\GAME\CIVI_5\CIVI\Game3D`.  
**Trạng thái:** tài liệu thi công; các class, scene, prefab, script build và QA 3D bên dưới là đầu việc cần tạo, chưa được coi là đã tồn tại hoặc đã PASS.

## Cách dùng tài liệu

Đi theo thứ tự W00 → W15. Mỗi bước ghi đầu vào, phần code, phần scene/render và điều kiện ra. Không bỏ qua điều kiện ra để làm art hàng loạt khi motor, dữ liệu hoặc nhiệm vụ còn sai. Một vòng làm việc nhỏ luôn có: sửa code/data → compile → Play Mode → xem khung hình Unity → kiểm hành vi → build ở mốc tích hợp → ghi bằng chứng.

Kiến trúc và chỉ số trong đây là quyết định thiết kế cho bản 3D, không phải mô tả runtime đang có. Tất cả đường dẫn dưới `Game3D`, `tools/Build-Unity3D.ps1`, `release/Unity3D`, `artifacts/unity3d` là quy ước đích.

## W00 — Kiểm kê và khóa đầu vào

### Đầu vào thực tế của repository

| Đã có | Cách sử dụng khi chuyển thể |
| --- | --- |
| `Game`, Unity 6000.4.0f1 | Bản đang chơi dùng sprite 2D. Giữ làm baseline kiểm thoại/nhiệm vụ; không sao chép controller/world renderer làm motor 3D. |
| `Game/Packages/manifest.json` | Hiện có Input System 1.20.0, uGUI và các module của bản cũ; chưa có URP, Cinemachine, AI Navigation trong manifest đã kiểm. |
| `STORY.md`, `PLAN.md` | Canon và nguồn thiết kế chuyển thể; giữ nguyên hai file. PLAN_1.md là bản Unity 3D. |
| `../dialogue` | 17 bản thảo, có tuyến và liên kết. Chỉ chương 1 đã biên dịch vào gameplay hiện tại. |
| `tools/dialogue_content.py`, `compile_chapter_dialogue.py`, `review_dialogue_files.py` | Đọc/kiểm dữ liệu, giữ source provenance. Cần adapter xuất cho runtime 3D. |
| `.../LuuQuangChapterOne/chapter.json` | Baseline chương 1: 148 cue, 154 trang, 12 cảnh, 9 nhiệm vụ, 5 thao tác. |
| `LuuQuangChapterState.cs` | Tham khảo quy tắc/checkpoint; các vị trí và travel field gắn bản cũ cần được ánh xạ lại. |
| Menu/pause/map/progress đã thống nhất | Giữ hành vi sản phẩm, viết view/controller Unity UI mới cho 3D. |

Chạy từ gốc `CIVI` để ghi baseline; không cần commit tự động:

```powershell
git -c safe.directory=D:/VSIC/GAME/CIVI_5/CIVI status --short
Get-Content Game/ProjectSettings/ProjectVersion.txt
Get-Content Game/Packages/manifest.json
Get-FileHash PLAN.md,STORY.md -Algorithm SHA256
py -3 tools/review_dialogue_files.py
```

**Đầu ra:** `docs/unity3d/baseline.md`: đường dẫn, Editor/OS, file nguồn, tình trạng Git, snapshot thiết kế. Ghi nguồn canon và phần action bổ sung riêng. Credits vẫn do người dùng tự chỉnh.

**Điều kiện ra:** biết code nào tái dùng được; file nào chỉ là prototype cũ; không dùng kết quả test 620/580 của bản hiện tại để tuyên bố bản 3D đã đạt.

## W01 — Khởi tạo project 3D và package

1. Trong Unity Hub, dùng Editor đã cài và template Universal 3D để tạo `Game3D` ở đường dẫn đích. Không tạo project lồng bên trong `Game/Assets`.
2. Bắt đầu từ mốc Editor 6000.4.0f1 đang có; kiểm license, Windows Build Support và package resolve. Nếu không tương thích, ghi rõ kết quả thử trên project mới trước khi chọn Editor khác.
3. Chọn Universal Renderer cho scene 3D. Kiểm URP Asset được gán trong Graphics và từng Quality level; không gán nhầm 2D Renderer.
4. Cài Input System, Cinemachine, AI Navigation, Timeline, uGUI/TextMeshPro và Unity Test Framework qua Package Manager ở version tương thích. Đọc version thực sự resolve, khóa manifest/lockfile; không copy số “latest” vào JSON theo phỏng đoán.
5. Kiểm Physics 3D, Animation, AI, Audio, UI, JSON và ScreenCapture theo module mà code sử dụng. Scene có Camera phối cảnh, AudioListener và cube/floor có collider là smoke đầu.
6. Version Control: Visible Meta Files, Asset Serialization Force Text. Commit `.meta` cùng asset; không đưa Library/Temp/cache vào Git. Kiểm Git LFS/chính sách asset lớn trước khi nhập model hàng loạt.
7. Tạo đúng thư mục ở PLAN_1.md và scene `Scenes/Bootstrap/Bootstrap.unity`, `Scenes/Menu/MainMenu.unity`, `Scenes/Tests/RenderLab.unity`, `Scenes/Tests/CombatLab.unity`.

URP là pipeline render của Unity; cách cài/gán cần theo [tài liệu URP](https://docs.unity3d.com/6000.4/Documentation/Manual/urp/urp-introduction.html). Template có thể cài sẵn một số package; kiểm trước khi thêm.

**Đầu ra:** project mở sạch, package resolve, ảnh cube được chiếu sáng, build Windows rỗng có cửa sổ/ảnh đúng. Ghi cấu hình tại `docs/unity3d/environment.md`.

## W02 — Assembly, boot và hợp đồng dữ liệu

### Dependency dự kiến

| Assembly/thư mục Scripts | Trách nhiệm | Tham chiếu nội bộ |
| --- | --- | --- |
| `Civi.ThreeD.Core` / Core | Quest/choice/evidence state, luật, stable ID, DTO dữ liệu | Không phụ thuộc UnityEngine |
| `Civi.ThreeD.Application` / Application | GameFlow, QuestDirector, use case, interface IO/world | Core |
| `Civi.ThreeD.Infrastructure` / Infrastructure | File save, content loader, clock/asset adapters | Core, Application |
| `Civi.ThreeD.Gameplay` / Gameplay | Motor, physics, combat, NPC, scene bindings | Core, Application |
| `Civi.ThreeD.Presentation` / Presentation | UI, audio, camera/cutscene và render bindings | Core, Application |
| `Civi.ThreeD.Composition` / Composition | Bootstrap, tạo và nối các implementation | Các assembly runtime ở trên |
| `Civi.ThreeD.Editor` / Editor | Validator, importer, build/capture menu | Các assembly runtime cần dùng, Editor-only |

Package reference được thêm đúng nơi sử dụng; ví dụ Input System ở input adapter, Cinemachine ở camera binding. Lấy tên assembly thật từ package `.asmdef`, không suy ra từ tên hiển thị. Test assembly có giới hạn nền tảng/reference riêng, không thành dependency của player.

### File cần viết trước

| File | Hợp đồng tối thiểu |
| --- | --- |
| `Core/Ids/ContentIds.cs` | ID string/struct được validate, không dùng tên object làm khóa |
| `Core/State/GameState.cs` | Chapter/node/quest/evidence/choice/world state; không chứa Transform |
| `Application/GameFlow.cs` | Trạng thái Boot, Title, Loading, Playing, Dialogue, Pause, Cutscene, GameOver |
| `Application/Ports/IContentRepository.cs` | Đọc định nghĩa theo ID; báo thiếu nguồn rõ |
| `Application/Ports/ISaveRepository.cs` | Validate/read/write snapshot, kết quả thành công/lỗi |
| `Application/Ports/IWorldRuntime.cs` | Nạp area, đặt spawn, apply world state, trả dữ liệu map |
| `Composition/CiviBootstrap.cs` | Một instance runtime, nối ports, shutdown và lưu đúng thứ tự |
| `Infrastructure/Content/ContentValidator.cs` | Unique ID, target route, source cue, scene/spawn và quest reference |

Boot scene giữ dịch vụ; area scene giữ geometry/NPC/interactable. Tắt domain reload trong Editor chỉ sau khi đã kiểm reset static/event subscriptions. Đăng ký event có unsubscribe; không spawn bootstrap lần hai khi về menu.

**Điều kiện ra:** vào menu, mở/đóng test scene rồi về menu không nhân đôi camera, listener, input hoặc event. Core test chạy không cần scene.

## W03 — Input, motor và camera

### Code

Tạo `Gameplay/Player/PlayerInputAdapter.cs`, `PlayerMotor.cs`, `PlayerStateMachine.cs`, `InteractionSensor.cs`; `Presentation/Camera/PlayerCameraRig.cs`. `CiviControls.inputactions` có action maps Gameplay / Dialogue / UI / Cutscene và đủ binding theo PLAN_1.md.

Motor chỉ nhận move vector, hướng camera và delta của gameplay clock. Tách đọc input khỏi di chuyển để test được bằng cùng command như người chơi. Camera đọc Look ở runtime khi Gameplay đang bật, không khi menu đang giữ chuột.

Đoạn dưới là công thức tích hợp vận động, cần bọc vào `PlayerMotor` và state gating; không phải controller hoàn chỉnh đã tạo:

```csharp
Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
Vector3 wish = Vector3.ClampMagnitude(forward * move.y + right * move.x, 1f);
if (controller.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
verticalSpeed += gravity * gameplayDelta;
Vector3 velocity = wish * moveSpeed + Vector3.up * verticalSpeed;
controller.Move(velocity * gameplayDelta);
```

Khi camera nhìn gần thẳng đứng, dùng hướng ngang hợp lệ gần nhất để tránh vector gần zero. Nhảy gán vận tốc Y theo luật trạng thái; va trần hủy vận tốc đi lên. `CharacterController.Move` nhận displacement và không tự áp trọng lực. [API CharacterController.Move](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/CharacterController.Move.html).

### Scene/prefab

```text
PF_Mai
  CharacterController
  PlayerMotor / PlayerStateMachine / InteractionSensor
  VisualRoot
    Rig + SkinnedMeshRenderer + Animator
  CameraTarget
  InteractionOrigin
  EquipmentSocket(s)
```

Capsule phải khớp model. Sàn/cầu thang/tường dùng collider 3D; layer matrix tách Player, NPC, Environment, Interactable, Hitbox và Projectile. Tránh collider con làm motor tự va vào đồ cầm.

Main Camera có Cinemachine Brain; CinemachineCamera có Third Person Follow tham chiếu CameraTarget. Script xoay target, giới hạn pitch, sensitivity và đảo trục trong settings. Cấu hình collision mask/radius và camera damping; kiểm camera gần tường. Cinemachine không tự đọc chuột cho target này. [Cinemachine Third Person Follow](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineThirdPersonFollow.html).

**Render/kiểm:** quay clip đi/chạy/dừng/nhảy/cầu thang và camera góc hẹp. Kiểm 30/60/120 FPS, mất focus, pause, mở UI, đứng mép sàn; không nhảy khi Space đang đọc thoại, không đánh khi bấm UI. Chưa tăng art khi motor/camera còn mắc tường.

## W04 — Model, rig, Animator và RenderLab

### Asset cần chuẩn bị

Một model Mai mẫu; một NPC; gậy kỹ thuật; modular kit phòng mẫu. File nguồn model/texture có nguồn, quyền sử dụng và phiên bản trong `source-assets/unity3d/asset-register.md`. File FBX/texture dùng trong game và `.meta` ở Art; file làm việc DCC lưu theo chính sách asset lớn.

1. Chốt đơn vị 1 Unity unit = 1 mét, trục Y lên; model đứng trên sàn ở chân, prefab root scale 1. Không sửa tỉ lệ từng prefab bằng nhiều lớp scale âm.
2. Kiểm topology, normals, UV0 và UV lightmap cho geometry cần bake. Kiểm texture color space, normal map import và material slot.
3. Nhân vật có skin weight sạch tại vai, khuỷu, hông, đầu gối; thử pose cực hạn trước render đẹp.
4. Import Humanoid nếu skeleton hợp lệ và kiểm Avatar mapping/T-pose; dùng Generic khi rig không phù hợp và cung cấp clip tương ứng. Không coi retarget là bảo đảm mọi rig chạy đẹp.
5. Animation clips: idle/walk/run/start/stop/turn, jump/land nếu dùng; attack/guard/dodge/hit/disabled; talk/inspect/use-terminal/sit. Clip loop, root transform và event phải được review.
6. Tạo Animator Controller và Blend Tree dựa trên tốc độ thực; root motion tắt ở locomotion điều khiển bằng motor. Attack root motion chỉ thêm khi có chủ sở hữu displacement rõ và xử lý collision.
7. Tạo socket cầm thiết bị, điểm nhìn và vị trí chân theo cùng convention. IK đặt chân/tay là bước hoàn thiện sau clip cơ bản.

[Mecanim/Avatar của Unity](https://docs.unity3d.com/6000.4/Documentation/Manual/AnimationOverview.html) hỗ trợ controller/clip và retarget humanoid; vẫn cần kiểm deformation trên model thật.

### RenderLab

Scene có sàn, vật chuẩn màu, một nguồn sáng chính, fill môi trường, vùng sáng/tối và ít nhất hai điểm camera cố định. Đặt Mai/NPC cùng vật liệu dự kiến. Kiểm model trong Game view bằng pipeline thật; không chỉ xem thumbnail FBX hoặc render trong phần mềm dựng hình.

**Đầu ra:** prefab, Avatar/rig, Animator, clip sample và ảnh `mai-front`, `mai-side`, `mai-back`, `mai-face`, `walk-contact` trong `artifacts/unity3d/W04`. Ghi material/light/camera settings. Clip phải cho thấy tay/chân/thân thay đổi, chân không trượt rõ khi đứng hoặc chạy.

Ngân sách asset khởi điểm để thử trên máy mục tiêu; đây là giả thuyết sản xuất, không phải giới hạn Unity hay kết quả benchmark:

| Asset | Mức bắt đầu | Cách giảm khi profile quá nặng |
| --- | --- | --- |
| Mai ở camera gameplay | Khoảng 30–50 nghìn tam giác LOD0; 2–3 material; texture chính 2K | LOD theo khoảng cách, giảm material/texture ngoài cận cảnh |
| NPC chính | Khoảng 15–30 nghìn tam giác LOD0; dùng chung rig khi hợp lệ | LOD, giảm bone/animation update theo khoảng cách |
| NPC nền | Model nhẹ hơn NPC chính, vật liệu dùng chung | Giới hạn số đang hoạt động, culling và lịch ở ngoài camera |
| Modular environment | Pivot/grid nhất quán; texture dùng chung/atlas khi hợp lý | LOD/culling, collider đơn giản, kiểm draw và texture memory |

Tỉ lệ màn hình và độ phức tạp shader quyết định chi phí cùng với số polygon. Chỉ tăng asset budget sau khi có ảnh và số đo player tại cùng camera/preset.

## W05 — CombatLab trước khi làm level đẹp

### Code/data

| File/asset dự kiến | Phần cần thực hiện |
| --- | --- |
| `Core/Combat/CombatRules.cs` | Damage, stagger, cooldown và điều kiện hợp lệ |
| `Gameplay/Combat/Combatant.cs` | State runtime, HP/stagger, hit reaction, disable |
| `Gameplay/Combat/AttackExecutor.cs` | Start/active/recovery, hit query, dedupe attack ID |
| `Gameplay/Combat/GuardDodgeController.cs` | Cửa sổ đỡ/né, resource cost và cancel |
| `Gameplay/Combat/AnimationSignalRelay.cs` | Chuyển signal animation sang gameplay có validation |
| `Gameplay/AI/EnemyBrain.cs` | FSM, target sensing, yêu cầu đường đi và chọn đòn |
| `Data/Combat/AttackDefinition.asset` | Thời gian, tầm, damage, stagger, cue và animation ID |

CombatLab: sàn xám, cột che, dummy, drone tuần tra, robot bảo trì và cầu dao. Không đưa crowd/bão dày vào trước khi combat đọc rõ.

### Logic bắt buộc

- Một đòn có `attackInstanceId`; nhiều collider của một NPC quy về một `combatantId`. Clear danh sách hit khi bắt đầu đòn mới.
- Khi bị ngắt/pause/load: đóng hit window đúng cách, không để Animation Event đến muộn phát damage trái trạng thái.
- Đòn có báo trước, active và recovery; phản hồi hit gồm pose, âm thanh, VFX và thông tin vừa đủ.
- Robot đi bộ dùng Navigation; drone có logic bay/độ cao, tránh vật và vùng hoạt động riêng. Không gắn hai motor cùng điều khiển Transform.
- Core timer dùng gameplay clock. Animation gameplay không dùng UnscaledTime trong pause; UI dùng thời gian riêng. Kiểm [Animator.updateMode](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Animator-updateMode.html).

**Điều kiện ra:** combo ba đòn, đỡ/né/xung nhiễu có thể đọc; tấn công nhiều collider không nhân damage; NPC bị vô hiệu hóa không tiếp tục đánh; pause giữa active frame không làm trôi trạng thái. Có clip 30–60 giây từ Unity player và các test tương ứng.

## W06 — Level blockout, cửa và map 3D

### Scene theo chương 1

Tạo scene area cho cơ quan, phòng lịch sử, căng tin, phòng họp và căn hộ; liên kết hành lang/thang máy bằng cửa/spawn có ID. Hồi ức Mai tám tuổi là scene/cutscene có nhân vật đúng tuổi, không dùng model Mai trưởng thành thay vai. Có thể làm khu bệnh viện giới hạn camera để giảm asset.

Blockout bắt đầu bằng geometry đơn giản nhưng có kích thước thật: bàn, lối đi, sàn, trần, cầu thang. Kiểm Mai và camera lọt cửa; tối thiểu có một tuyến đi chính rõ, khoảng NPC nhường đường và tầm nhìn tới mục tiêu. Dựng ngoại cảnh xa bằng mesh đơn giản/LOD phù hợp; đồ ở gần có chiều sâu, vật liệu và collision.

### File/data

- `AreaDefinition`: scene path/ID, map bounds, floor list, door/spawn IDs, anchor chứng cứ.
- `AreaRoot`: đăng ký object, NPC, lights và spawn của scene đã nạp.
- `DoorInteractable`: kiểm điều kiện rồi gọi `SceneTransitionService`.
- `MapProjection`: chuyển tọa độ XZ sang map UV, chọn tầng theo vùng/cao độ đã định nghĩa.
- `MapPresenter`: nhận dữ liệu player/NPC/evidence/door; hiển thị trong UI, không làm nguồn world state.

Ví dụ quy tắc chiếu cho một tầng có bounds đã ghi rõ, cần đảo trục dọc theo UI:

```text
u = (worldX - minX) / (maxX - minX)
v = (worldZ - minZ) / (maxZ - minZ)
uiX = rectX + clamp01(u) * rectWidth
uiY = rectY + (1 - clamp01(v)) * rectHeight
```

Giữ tầng/cao độ trong dữ liệu map; không dùng công thức trên để gộp hai tầng thành một. Hiện đường đi, cầu thang, cửa, vị trí Mai, NPC và điểm chứng cứ. Map cập nhật khi mất nguồn/mở cửa, không cho dịch chuyển bỏ qua quest.

### Quy trình chuyển khu vực

`Kiểm điều kiện → khóa input → lưu snapshot ở cửa nguồn → fade out → nạp area đích bất đồng bộ → kiểm spawn/geometry/world state → đặt Mai và camera → lưu snapshot đích → fade in → trả input`.

Trong lúc nạp, giữ snapshot nguồn để rollback khi scene/spawn/save đích lỗi; không ghi đè nó bằng trạng thái nửa cũ nửa mới. Dùng transaction ID chống double-click F. Xử lý thất bại phải mở lại được menu hoặc trở về nguồn hợp lệ.

**Điều kiện ra:** đi qua rồi load trước/sau cửa đúng area/spawn; không giữ NPC phòng cũ khi quest stage không đổi; không kẹt collider hoặc nhân đôi Player sau nhiều vòng đổi scene.

## W07 — Dialogue, quest và bảng chứng cứ

### Chuỗi dữ liệu

`STORY.md + ../dialogue/*.md → parser/validator → dữ liệu biên dịch có provenance → định nghĩa Unity → Core state → Presenter/Gameplay`.

Tái dùng parser/route validator hiện có. Viết adapter xuất `Game3D/Assets/_Civi3D/Data/...` hoặc một bộ nội dung runtime có version; không sửa tool cũ để nó âm thầm đổi output của bản hiện tại. Nếu dùng ScriptableObject importer, giữ source ID/hash, không nhập từng lời thoại bằng tay vào scene.

### File cần tạo

| File/asset | Hợp đồng |
| --- | --- |
| `Core/Narrative/DialogueNode.cs` | Cue/source ID, speaker, text key, node kế, điều kiện |
| `Core/Quests/QuestState.cs` | Objective trạng thái và điều kiện evidence/action |
| `Application/QuestDirector.cs` | Kiểm command và phát event, chống nhận kết quả hai lần |
| `Application/DialogueService.cs` | Trả cue hiện tại, Advance/Choose hợp lệ, khóa spoiler |
| `Core/Evidence/EvidenceCatalog.cs` | Nguồn, độ chắc chắn, liên kết và điều kiện xác minh |
| `Presentation/Dialogue/DialoguePresenter.cs` | Speaker, chữ, lựa chọn, camera và pose hội thoại |
| `Presentation/Evidence/EvidenceBoardPresenter.cs` | Chọn/đối chiếu evidence; gửi command kết luận |
| `Editor/Content/ChapterImporter.cs` | Import có validate toàn bộ reference và source hash |

Trong chương 1 phải giữ 148 cue/12 cảnh; 154 trang là baseline cách chia hiện tại, có thể chia lại theo layout 3D nhưng kiểm không thiếu/đảo lời. Giữ đúng speaker và tuổi nhân vật. Chín nhiệm vụ, năm thao tác có điều kiện kiểm chứng phải hoàn thành được.

### Chuyển lời văn thành thao tác

Ví dụ hồ sơ 8-12: người chơi xem ảnh, chọn dấu áo/giày/cây, đối chiếu ngày chụp và hỏi Linh. Core chỉ mở kết luận “cần khảo sát, không đủ căn cứ ghi 0”; không tự kết luận ảnh chứng minh một số cư dân chính xác. UI chọn evidence không được tự gọi NextChapter.

**Điều kiện ra:** toàn bộ chương 1 chạy bằng data, đúng cue/speaker/scene order; không thể phê chuẩn khi thiếu điều kiện; nói chuyện phụ không nhảy quest; sau load giữ nguyên evidence đang kiểm và cue hợp lệ.

## W08 — NPC, sinh hoạt và phản ứng môi trường

1. Prefab NPC gồm model/Animator, collider phù hợp, `NpcBrain`, schedule, identity và interaction binding. `NavMeshAgent` dành cho đi bộ; bake NavMeshSurface theo hình học của area.
2. State: Idle, WalkToAnchor, Work, Talk, Yield, Alert, Evacuate; Enemy có FSM riêng, dùng chung services khi phù hợp.
3. Quy tắc một owner vị trí: agent điều khiển locomotion thông thường; nếu dùng animation/root motion thì có adapter đồng bộ, không để agent và Animator giằng Transform.
4. Trò chuyện dừng lịch, quay nhìn hợp lý, đóng hội thoại phục hồi lịch; hội thoại chính chiếm interaction priority rõ ràng.
5. NPC nhường cửa/vùng thao tác, không xếp lịch làm chặn vĩnh viễn điểm nhiệm vụ. Load/save phục hồi bằng anchor/state hợp lệ, không phụ thuộc đường path cũ đã mất.
6. Gắn world event với light, quạt, màn hình, âm thanh và schedule. Khi khu mất điện, cảnh vật phải đổi có chủ đích; không chỉ phủ một lớp đen lên toàn màn hình.

NavMesh và link chỉ giải bài toán đường đi; vẫn phải viết state/schedule. Xem [AI Navigation](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/index.html). Không đưa AI tạo sinh thành điều kiện để NPC có thể làm nhiệm vụ.

**Render/kiểm:** quay một phút sinh hoạt có Mai đi xen qua, bắt đầu/đóng thoại, pause và mất điện. Kiểm hội thoại chính vẫn tiếp cận được khi nhiều NPC trong cảnh.

## W09 — Menu, pause, progress và camera hội thoại

### View cần tạo

`MainMenuView`, `PauseView`, `ProgressView`, `SettingsView`, `GuideView`, `CreditsView`, `DialogueView`, `MapView`, `CombatHudView`, `FadeView`. Tất cả lấy state từ presenter/application; không chứa luật nhiệm vụ.

- MainMenu là scene 3D có Mai/NPC/bối cảnh bên phải, UI tên/nút bên trái. Animator idle, camera dolly nhẹ, particle/ánh sáng có ngân sách.
- Bắt đầu tạo lượt mới ngay; Continue chỉ khi đọc và validate save thành công. Chọn dữ liệu/cảnh được xử lý ở GameFlow.
- Pause theo Esc: Tiếp tục, Tiến trình, Cài đặt, Trở về màn hình đầu. Mở pause giữa dialogue/combat giữ context để quay lại đúng.
- World clock dừng; NavMesh, physics, animation, hit/cooldown và âm thanh gameplay được đưa về chế độ pause nhất quán. UI/fade/transition đã cho phép dùng thời gian không scale. Khôi phục time/audio/input kể cả khi trở về menu.
- Progress lấy danh sách 10 chương từ catalog, không hardcode “đã chơi” theo index. Chưa chơi xám không ảnh; đã chơi có ảnh Unity capture và nhãn đang chơi/hoàn thành. A/B đánh dấu tuyến kết thúc đúng, không ghi chương 10 hoàn thành.
- Panel có Opening/Open/Closing/Closed; không nhận click kép khi đóng/mở. Hover có phản hồi vị trí/màu/viền. Giảm chuyển động giữ đủ thông tin.
- Guide ở màn hình đầu. HUD thường không có tên chương/map, nút pause và dải hướng dẫn. Combat HUD chỉ hiện thông tin cần ra quyết định.
- Đổi camera gameplay → hội thoại bằng blend hoặc fade; Timeline cần skip/cancel cleanup, không được làm kẹt camera/input sau khi bỏ qua.

Canvas dùng scaler/safe area, font tiếng Việt và navigation rõ. Settings có âm lượng, text size, chất lượng/render scale, full screen, camera sensitivity, giảm chuyển động; thay quality không làm mất URP Asset hoặc material.

**Điều kiện ra:** mọi cửa sổ đi/về được bằng chuột và Esc, không có click xuyên UI, chữ dài và tên chương không cắt ở 720p/1080p/4:3/ultrawide đã chọn hỗ trợ. Chụp ảnh cả menu, pause, progress và hội thoại.

## W10 — Save/load, checkpoint và nhánh truyện

### Schema khởi điểm đề xuất

```json
{
  "format": "CIVI-UNITY3D",
  "schemaVersion": 1,
  "contentVersion": "c1-3d-prototype-01",
  "chapterId": "C1",
  "nodeId": "C1.OFFICE.HOSPITAL",
  "cueId": "C1.S02.001",
  "areaId": "core.office42",
  "spawnId": "office.entry",
  "position": { "x": 0.0, "y": 0.0, "z": 0.0 },
  "yaw": 0.0,
  "questStates": [],
  "evidenceStates": [],
  "choices": [],
  "worldStates": [],
  "combatCheckpoint": { "encounterId": "", "state": "inactive" }
}
```

ID ở ví dụ là minh họa contract, phải sinh từ catalog thật trước khi thành fixture. Dùng list DTO có field rõ nếu serializer đã chọn không hỗ trợ Dictionary/polymorphism theo cách cần dùng. Không ghi trực tiếp runtime Unity object/ScriptableObject reference vào JSON.

### File cần tạo và quy tắc

- `Core/Save/SaveSnapshot.cs`: DTO, không có GameObject/Transform.
- `Application/CheckpointService.cs`: chụp trạng thái tại ranh giới hợp lệ; quyết định lúc save đang đánh/tải scene.
- `Infrastructure/Save/JsonSaveRepository.cs`: ghi file tạm, flush/đóng, thay nguyên tử và backup; validate khi đọc. Khả năng replace được kiểm trên filesystem/OS mục tiêu.
- `Infrastructure/Save/SaveMigrator.cs`: registry chuyển schema/content version được nhận diện; không nhận save lạ bằng cách reset im lặng.
- `Application/QuitCoordinator.cs`: cùng một luồng cho về menu, nút thoát và `Application.wantsToQuit`; snapshot save phải hoàn tất trước khi cho thoát. Nếu IO bất đồng bộ, hủy yêu cầu đóng lần đầu, lưu rồi phát lại đúng một lần; có guard chống recursion.
- Đường dẫn save 3D riêng dưới persistentDataPath; QA bắt buộc có override thư mục riêng. Giữ main save, backup, history và preview tách biệt.

Checkpoint combat prototype nên đặt trước/sau encounter và ghi chính sách rõ: load về đầu encounter với trạng thái đã quy định, không giả vờ phục hồi chính xác mid-animation. Nếu sản phẩm cần resume giữa trận, bổ sung enemy HP/phase/RNG/timer/spawn data và test đầy đủ trước khi công bố hỗ trợ.

12 tuyến campaign: I_A/I_B × II_A/II_B × A/B/C; xác nhận đúng route/cue/world payoff. A và B kết ở đoạn riêng; C đi phần cuối chương 9 rồi chương 10. Việc đổi tuyến phải xóa hoặc từ chối state mâu thuẫn theo luật.

**Điều kiện ra:** fresh save, resume, corrupt primary/backup, unknown version, write denied, quit trong dialogue/pause/transition, tải scene thất bại và resume trước/sau encounter đều có kết quả được định nghĩa. Migration tiến độ bản cũ chỉ chạy trên bản sao và ánh xạ spawn 3D; không chuyển thẳng tọa độ 2D.

## W11 — Art pass, ánh sáng, VFX và âm thanh

### Thứ tự dựng render

1. **Geometry pass:** thay blockout từng cụm bằng modular mesh, giữ đường đi/collider và door/spawn IDs. Không đổi kích thước sau khi animation/camera đã duyệt mà không test lại.
2. **Material pass:** URP Lit hoặc shader stylized đã kiểm. Albedo/normal/mask đúng loại import; đồng nhất texel density. Không giữ material hồng do sai pipeline.
3. **Lighting pass:** đặt môi trường, ánh sáng chính, fill, local light; xác định nguồn nào bake, mixed hoặc realtime. Giảm số đèn động có bóng xuất hiện cùng khung hình.
4. **Bake pass:** geometry tĩnh có lightmap UV, lighting settings asset và baking data rõ. Cố định camera trước/sau bake để so sánh seams, leak và vật thể quá tối.
5. **Character lighting:** đặt Light Probes tại vùng nhân vật đi lại và chuyển vùng sáng/tối; kiểm mặt, bàn tay và đồ cầm không bị đen. Reflection Probes cho vùng kính/kim loại có ý nghĩa. [Unity Light Probes](https://docs.unity3d.com/6000.4/Documentation/Manual/LightProbes.html).
6. **World-state pass:** mất điện/bão điều khiển cả direct lights, emissive, VFX và audio. Bake ánh sáng trang trí đang bật vào map sẽ không tự biến mất khi tắt light realtime; chọn ambient bake trung tính hoặc lighting state giải pháp riêng và kiểm chuyển trạng thái.
7. **Post-processing:** exposure/color grading, bloom vừa đủ, anti-aliasing theo preset. Độ sâu trường ảnh/motion blur chỉ dùng khi không che gameplay; có tùy chọn giảm/tắt.
8. **VFX pass:** mưa, hơi nước, bụi, tia điện và hit effect có mức Low/Medium/High; kiểm transparency/overdraw và không che telegraph.
9. **Audio pass:** ambience theo khu, bước chân theo surface, máy móc, phản hồi hit, UI và music snapshots; pause/resume âm thanh đúng và có caption khi cần.

Tạo quality presets với thông số ban đầu, đo rồi chỉnh: resolution/render scale, shadow distance/resolution, additional lights, particles, LOD và post effects. 1080p/60 FPS là mục tiêu trên máy mục tiêu đã ghi cấu hình, không phải kết quả mặc định của URP.

**Đầu ra:** bộ ảnh/clip cùng camera dưới ánh sáng bình thường/mất điện/bão và quality Low/High; ghi shader/material/lighting settings. Mỗi pass art chạy lại đường đi và camera đã có.

## W12 — Render review và profile có thể lặp lại

### Bộ cảnh chụp tối thiểu

| Mã ảnh/clip | Nội dung cần nhìn |
| --- | --- |
| R01 | Menu 3D, tên/nút trái, nhân vật/bối cảnh phải |
| R02 | Mai trước/sau/trái/phải trong RenderLab |
| R03 | Walk/run/turn và tiếp xúc chân khi camera theo |
| R04 | Combo/guard/dodge, báo đòn địch và hit feedback |
| R05 | NPC sinh hoạt, đối thoại và mặt nhân vật |
| R06 | Nội thất cơ quan, map, cửa và cầu thang |
| R07 | Rìa Mưa trước bão, ánh sáng/mật độ |
| R08 | Mất điện/bão và tuyến cứu hộ |
| R09 | Pause/progress/dialogue ở 720p và 1080p |
| R10 | Một lựa chọn thay đổi NPC/đường/nguồn ở lần quay lại |

Tạo `Editor/QA/RenderReviewMenu.cs`, scene/camera presets và cấu hình `RenderReviewProfile` khi tới bước này. Capture filename gồm scene, camera, quality, state và số frame. Ghi Editor/build version, commit nếu có, GPU/driver, resolution và preset.

Chụp gameplay bằng player chạy với thiết bị đồ họa. Nếu tự động hóa dùng camera → RenderTexture → readback, harness phải có GPU và không phụ thuộc Game view đang focus. Chờ scene, animation pose, ánh sáng và output ghi xong trước khi đọc ảnh. Không dùng `-nographics` cho lượt đánh giá render. Với screenshot thông thường xem [ScreenCapture.CaptureScreenshot](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/ScreenCapture.CaptureScreenshot.html); chụp end-of-frame trong Editor phải xử lý riêng các hạn chế batch/Game view.

Profile development player trên máy mục tiêu; chụp CPU/GPU frame và memory ở scene đông nhất đã có. Đo frame time, spike lúc nạp scene/spawn, GC, draw/SetPass, shadow cost, transparency và memory textures/meshes. Ghi khoảng mẫu và preset; phân biệt Editor overhead với player. [Unity Profiler](https://docs.unity3d.com/6000.4/Documentation/Manual/Profiler.html).

Mỗi ảnh phải mở và xem thật: framing, clipping, UI, shader hồng, thiếu texture, light leak, shadow, chân/tay xuyên vật. Test ảnh “không đen” chỉ là smoke, không thay thế review hình ảnh. Tối ưu có số đo trước/sau; không giảm chất lượng toàn game chỉ để tăng con số FPS ở phòng trống.

## W13 — Chương 1 3D rồi vertical slice chương 2–3

### Mốc chương 1

1. Dựng 5 area đúng truyện và phần hồi ức đúng tuổi.
2. Nạp bộ thoại đã duyệt, giữ source/cue order, đúng speaker.
3. Chơi 9 nhiệm vụ và 5 thao tác hồ sơ; F/map/door hoạt động xuyên các scene.
4. NPC chính/phụ có model/rig/lịch; vật thể và môi trường phản hồi.
5. Menu/pause/progress/save được nối đủ; hoàn thành chương rồi tiếp tục đúng.
6. Chạy render review và Windows build; ghi lỗi và sửa trước khi mở nội dung tiếp.

CombatLab tồn tại riêng để kiểm cơ chế; không ép chèn trận đánh vào hồi ức/gia đình. Các đoạn hành động bổ sung trong chapter phải được ghi rõ nguồn thiết kế theo PLAN_1.md.

### Mốc vertical slice theo PLAN.md

Rìa Mưa có hai tuyến, An/Hạ và hai cư dân, nhiệm vụ log/nguồn điện, hai kiểu drone, mini-boss và lựa chọn nhỏ có payoff nhìn thấy sau vài phút. Mục tiêu 45–60 phút phải đo bằng người chơi, không cộng thời lượng cutscene để đạt chỉ tiêu.

Kịch bản chơi thử: vào game → gặp An → khảo sát hai nguồn chứng cứ → xử lý tuyến máy móc → mở trạm → chọn cách hợp tác → quay lại thấy phản hồi → save/quit/continue → hoàn thành slice. Ghi thời điểm người chơi lạc hướng, bỏ qua clue, không hiểu lựa chọn hoặc không đọc được đòn.

**Điều kiện ra:** build đủ thư mục chạy được trên máy không cài Editor; người chơi mô tả được một bất công cụ thể, quan tâm nhân vật, hiểu ít nhất một đánh đổi và nhận thấy hậu quả. Quyết định mở 10 chương dựa trên số liệu sản xuất/playtest của slice.

## W14 — Test, build và đóng gói

### Ma trận kiểm chứng

| Lớp | Kiểm tra có ý nghĩa |
| --- | --- |
| Core/EditMode | Quest/evidence prerequisites; 12 route; damage dedupe; version/migration; stable ID/reference validator |
| PlayMode | Input thật qua Input System; motor/camera/collision; pause tại active hit; dialogue/cutscene skip; AI nhường cửa |
| Save/scene | Trước/sau cửa; lỗi scene/spawn; write denied; corrupt save; quit; load encounter đúng chính sách |
| Render | R01–R10, quality, nhiều độ phân giải, missing material/asset/font |
| Player | Khởi động sạch, hết chapter/slice, resume, backup; process exit code và log |
| Performance | Scene stress có CPU/GPU/memory đo trên cấu hình được ghi |

Tests phải có save/PlayerPrefs override riêng. Không ghi vào tiến độ người chơi. Không đánh dấu manual keyboard PASS khi chỉ gọi trực tiếp method; phân biệt test Input System và thử bàn phím vật lý.

Sau khi test assemblies đã được tạo, có thể chạy EditMode/PlayMode theo lệnh dưới. Chạy tuần tự, không mở hai Editor cùng project; PlayMode có render cần graphics. Không thêm `-quit` vào lượt test nếu việc đó làm runner kết thúc trước khi có XML:

```powershell
$unity3DTestEditor = 'C:\Program Files\Unity\Hub\Editor\6000.4.0f1\Editor\Unity.exe'
$unity3DTestProject = 'D:\VSIC\GAME\CIVI_5\CIVI\Game3D'
$unity3DTestOut = 'D:\VSIC\GAME\CIVI_5\CIVI\artifacts\unity3d\tests'
New-Item -ItemType Directory -Force -Path $unity3DTestOut | Out-Null
foreach ($unity3DTestMode in @('EditMode', 'PlayMode')) {
  $unity3DTestXml = Join-Path $unity3DTestOut ($unity3DTestMode + '.xml')
  $unity3DTestArgs = @('-batchmode', '-projectPath', ('"' + $unity3DTestProject + '"'),
    '-runTests', '-testPlatform', $unity3DTestMode, '-testResults', ('"' + $unity3DTestXml + '"'),
    '-logFile', ('"' + (Join-Path $unity3DTestOut ($unity3DTestMode + '.log')) + '"'))
  $unity3DTestProcess = Start-Process -FilePath $unity3DTestEditor -ArgumentList $unity3DTestArgs -WindowStyle Hidden -Wait -PassThru
  if ($unity3DTestProcess.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $unity3DTestXml)) {
    throw ('Unity test run failed or produced no report: ' + $unity3DTestMode)
  }
}
```

Đọc XML để kiểm số test thực chạy và failed/skipped/inconclusive; exit 0 với không có test chưa đủ nghiệm thu. Lưu log riêng từng lượt để tránh đọc nhầm XML cũ; script production cần dùng thư mục run ID mới và timeout có cleanup cho process do chính nó tạo.

### Build script cần tạo

Tạo `Game3D/Assets/_Civi3D/Editor/Build/Civi3DBuild.cs` với static method `Civi.ThreeD.Editor.Civi3DBuild.BuildWindows`. Method validate content/scene/catalog, lấy danh sách scene của bản 3D, gọi `BuildPipeline.BuildPlayer`, kiểm report và fail rõ nếu không Succeeded. Không phụ thuộc build settings của project `Game`. [API BuildPipeline.BuildPlayer](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html).

Tạo `tools/Build-Unity3D.ps1` bọc method đó, parameter hóa Editor/project/output/log path và kiểm exit code + output. Lệnh dưới **chỉ chạy sau khi project và method đã tạo**; đường Editor là mốc đã cài, không tự cam kết version phù hợp nếu bước W01 chưa đạt:

```powershell
$unity3DEditor = 'C:\Program Files\Unity\Hub\Editor\6000.4.0f1\Editor\Unity.exe'
$unity3DProject = 'D:\VSIC\GAME\CIVI_5\CIVI\Game3D'
$unity3DLogDir = 'D:\VSIC\GAME\CIVI_5\CIVI\artifacts\unity3d\build'
New-Item -ItemType Directory -Force -Path $unity3DLogDir | Out-Null
$unity3DArgs = @('-batchmode', '-nographics', '-projectPath', ('"' + $unity3DProject + '"'),
  '-executeMethod', 'Civi.ThreeD.Editor.Civi3DBuild.BuildWindows', '-quit',
  '-logFile', ('"' + (Join-Path $unity3DLogDir 'build.log') + '"'))
$unity3DProcess = Start-Process -FilePath $unity3DEditor -ArgumentList $unity3DArgs -WindowStyle Hidden -Wait -PassThru
if ($unity3DProcess.ExitCode -ne 0) { throw 'Unity 3D build failed; inspect build.log.' }
```

Đây là build không đánh giá hình ảnh. Nếu target/import/build workflow yêu cầu graphics, dùng cấu hình build đã kiểm trên máy thay vì giữ `-nographics` bằng mọi giá. Bước capture ở W12 có graphics riêng. `-quit` không được kết thúc render coroutine trước khi harness ghi kết quả; capture runner tự quản vòng đời.

Output dự kiến `release/Unity3D/CIVI.exe` cùng `_Data`, UnityPlayer và DLL/phụ thuộc do build sinh. Giao cả thư mục; không copy EXE đơn lẻ. Build QA và release có flags khác nhau; release không mở console/cheat/QA override cho người chơi ngoài ý muốn.

Giữ một launcher gốc. Khi bản 3D được chọn làm bản chơi chính, cập nhật CHOI_GAME.bat sau khi có build đã kiểm; trong giai đoạn thử nghiệm chạy EXE ở output riêng, không tạo thêm nhiều BAT.

**Đầu ra:** build, log, exit code, test report, ảnh/clip và `docs/unity3d/handoff-<moc>.md`. Không cần rebuild bản đang chạy khi chỉ sửa tài liệu này.

## W15 — Nhịp làm việc và bàn giao

### Checklist cho mỗi đầu việc

- [ ] Có ID việc, dependency, file/prefab/scene đích và tiêu chí đạt.
- [ ] Ghi rõ canon hay adaptation; stable ID và data reference đã kiểm.
- [ ] Code compile sạch, không reference UnityEditor trong runtime.
- [ ] Gắn component/reference trong prefab/scene; không Missing Script/null bắt buộc.
- [ ] Chạy hành vi từ input tới kết quả, không chỉ gọi method để chụp ảnh đẹp.
- [ ] Xem render thật trong Unity/player, sửa clipping/animation/light/material.
- [ ] Test phù hợp đã đạt; ghi bằng chứng, giới hạn và lỗi còn lại.
- [ ] Cập nhật TASK.md trước khi dừng hoặc chuyển người; giữ save thật.

### Backlog mở đầu, theo đúng thứ tự

| ID | Việc | Bằng chứng hoàn thành |
| --- | --- | --- |
| U3D-001 | Baseline, tạo Game3D, URP/packages | Environment.md, resolve/compile, cube player |
| U3D-002 | Assembly + Bootstrap/GameFlow | Menu/test scene đi-về, không duplicate |
| U3D-003 | Input/motor/camera | Clip và tests va chạm/pause/UI |
| U3D-004 | Model Mai + rig/Animator/RenderLab | Prefab, locomotion clip, ảnh đa góc |
| U3D-005 | CombatLab + hai máy | Đỡ/né/combo/xung, damage và animation test |
| U3D-006 | Area/door/map | Chuyển scene + spawn + map nhiều tầng đúng |
| U3D-007 | Import chapter/quest/evidence | Toàn bộ nguồn chapter1 được validate |
| U3D-008 | NPC schedule/world reactions | Clip sinh hoạt, tương tác và mất điện |
| U3D-009 | UI menu/pause/progress/settings | Input navigation, ảnh nhiều resolution |
| U3D-010 | Save/quit/rollback/migration | Test lỗi ghi, corrupted, trước/sau cửa |
| U3D-011 | Chapter1 toàn trình + art/render | Chơi hết + build + render review |
| U3D-012 | Vertical slice chương2–3 | Hai tuyến, combat, choice/payoff, player report |
| U3D-013 | Optimize và bàn giao slice | Profile CPU/GPU/memory, release folder |

Không đặt tất cả checkbox thành done chỉ vì có code khung. Nếu bị ngắt, TASK.md ghi: trạng thái project/package, việc đã làm, lệnh đã chạy và kết quả, file đang sửa, lỗi tái hiện, việc kế và prompt tiếp tục. Không có công cụ đọc quota 5h trong phiên hiện tại; cập nhật checkpoint theo mốc công việc để vẫn tiếp tục được.

### Prompt tiếp tục sau giai đoạn tài liệu

> Đọc PLAN_1.md và WORKFLOW.md trong D:\VSIC\GAME\CIVI_5\CIVI. Đây là bản Unity 3D đầy đủ dựa trên PLAN.md, có combat/action và điều tra; STORY.md là canon, drone/boss là phần chuyển thể. Hai tài liệu mới chưa triển khai project Game3D. Khi được giao code, bắt đầu W00/W01 và U3D-001, kiểm file thực tế trước khi tạo, không coi danh sách class/scene là đã có. Dùng model/rig/world 3D, URP/C#/Cinemachine/Animator/Navigation theo version đã resolve. Giữ dữ liệu thoại và save bản đang chơi; không dùng sprite làm nhân vật 3D. Thực hiện từng mốc code + scene + render + test + Windows build, lưu bằng chứng và cập nhật TASK.md.
