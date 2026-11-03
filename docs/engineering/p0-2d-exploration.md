# CIVI / P0 — 2D Exploration Greybox (27/10/2026)

## Mục tiêu và phạm vi

Đây là **vertical slice kỹ thuật**, không phải game 2D đã hoàn thiện hoặc bản release thương mại. Người chơi điều khiển **An** trong khu kiểm đồ S02 và bàn ảnh S03. StorySession của TV1 quản lý state, điều kiện lựa chọn và chuyển cảnh; không tự viết lại nội dung canon.

- **S02**: đi đến sơ đồ, kẹp, lịch, chú thích, Nhi và lối ra. Điều kiện S02_LEAVE chỉ được mở sau khi ghi nhận kẹp/lịch/chú thích.
- **S03**: khám phá bốn vật chứng (0441, 0442, 0443, cận kẹp), hỏi Hạ, xem bảng ảnh bằng B. S03_SOURCE chỉ mở khi đủ nguồn theo JSON.
- **S04**: màn thông báo kết thúc vertical slice; không tuyên bố gameplay S04 đã hoàn tất.
- **S00/S01**: giữ bản preview cũ. Sau S01, tại màn nối S02 xuất hiện nút **KHÁM PHÁ 2D / S02**, chuyển state thật sang bản 2D bằng bộ nhớ tạm.
- Chưa thêm grant quyền/consent cho S04; không tự cấp quyền xem/in video.

## Chạy trong Unity

1. Mở project `Game` bằng Unity 6000.6.5f1.
2. Đợi Unity import và compile. Đảm bảo đã chuẩn bị TV3 AssetCatalog trước đó; nếu chưa, dùng `CIVI > TV3 > Chuẩn bị bản tích hợp S00`.
3. Mở scene `Assets/_Civi/Scenes/S02-2D-VerticalSlice.unity` trực tiếp **hoặc** menu `CIVI > TV2 > Mở scene khám phá 2D`.
4. Nhấn Play. Bản mở trực tiếp bắt đầu ở S02 với initial state; muốn đem theo lựa chọn S00/S01, hãy chơi từ scene S00, sau S01 chọn **KHÁM PHÁ 2D / S02**.
5. Nếu scene chưa có, menu `CIVI > TV2 > Tạo scene khám phá 2D S02-S03` dựng lại scene skeleton. **Không chạy lệnh tạo lại khi có thay đổi thủ công chưa lưu trên scene**.

**Phím**: WASD/mũi tên di chuyển; E tương tác khi đủ gần; Enter/Space đọc tiếp; Esc đóng panel; B mở bảng ảnh ở S03; F5 lưu; F9 tải. Nút LƯU trên HUD tương đương F5.

## Mã và cảnh

- `Game/Assets/_Civi/Scenes/S02-2D-VerticalSlice.unity`: scene riêng, đã đăng ký trong Build Settings cùng S00.
- `Game/Assets/_Civi/Scripts/Application/Exploration/CiviExploration2D.cs`: player movement + physics collision, trigger phạm vi, greybox room, UI điều tra, S02/S03 authoring bridge, save/load.
- `Game/Assets/_Civi/Editor/Exploration/CiviExplorationSceneBuilder.cs`: menu dựng/mở scene và smoke-check.
- `Game/Assets/_Civi/Scripts/Application/Preview/S00PreviewPlayer.cs`: chỉ thêm nút chuyển scene tại boundary S02.
- `tools/StoryRuntimeTests/Program.cs`: thêm tests S02/S03: evidence gating, photo 0443, restore, S04 boundary.
- `Game/Assets/_Civi/Editor/Build/S00PreviewBuild.cs`: giữ scene phụ trong Build Settings khi TV3 chạy lại chuẩn bị S00.
- Backup trước tích hợp ở `artifacts/backup-p0-exploration`.

## Không cần tải thủ công ở bước này

Greybox tạo từ primitive Unity (floor, wall, furniture, character marker, interactable marker). Bản đồ này không được coi là art production. Không cần mua template Unity bên ngoài để chạy P0.

**Trước khi tiến tới bản phát hành**: TV3 cần bộ An idle/walk 4 hướng, NPC sprites, tileset phòng/sân trường, lớp foreground, đạo cụ có sprite riêng, animation + SFX, UI evidence board hoàn chỉnh và art direction/bộ giấy phép asset. TV2 cần tích hợp asset/prefab/Tilemap, kiểm soát collision/sorting, rồi thêm một puzzle suy luận có phản hồi trực quan. Không sao chép asset từ game khác khi chưa có quyền.

## Hạn chế và bước nghiệm thu

- P0 dùng thẻ màu greybox; chuyển S02->S03 qua stage trong **cùng scene**, chưa có level art/camera follow/NPC pathfinding.
- Chỉ hỗ trợ bàn phím Windows/desktop, chưa gamepad/mobile.
- Tương tác ghi evidence theo condition/effects tác giả. Việc ghép chứng cứ bằng mechanic độc lập, animation, âm thanh bước chân, UI production, accessibility và nội dung S04–ending chưa hoàn thành.
- Save của P0 tại `Application.persistentDataPath/civi-p0-exploration.json`, không đè save S00/S01. F5 từ khi đang xem thoại cũng lưu lineIndex; khi tải giữ được vị trí An.
- Test logic và Unity compile cần được bổ sung **Unity PlayMode QA trên máy người chơi** và playtest bên ngoài nhóm, gồm ngôn ngữ tiếng Việt, FPS, tỷ lệ 720p/1080p, va chạm, các điều kiện, F5/F9.
- Hiện thư mục gốc chưa là Git worktree (`git status` báo "not a git repository"); chưa commit/push bất cứ thứ gì. Không init Git khi chưa thống nhất với nhóm.

## Production gate

P0 đạt khi Unity Play không có Console error, đi lại và collision ổn định, 12 điểm tương tác hoạt động, S02->S03->S04 boundary đúng, F5/F9 không gây mất state, S00->S01->S02 handoff giữ nguồn/chọn trước đó. P1 bắt đầu sau khi playtest P0: game feel, mechanic nối manh mối (không thay thế lời khai bắt buộc), camera và art import pipeline.
