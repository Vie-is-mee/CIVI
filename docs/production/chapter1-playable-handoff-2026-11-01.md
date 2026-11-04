# CIVI / Chương 1: Đừng bấm vội → Trang ở giữa

Bàn giao ngày 01/11/2026; kịch bản học đường v4.1 trong repo. Không thay bằng truyện Lưu Quang.

## Cách chạy

- Double-click PLAY_CIVI_CHAPTER1.bat tại root repo.
- Windows standalone x86: release/Chapter1/CIVI_Chapter1_x86.exe (cần toàn bộ thư mục build, không copy riêng exe).
- Unity 6000.6.6f1: CIVI > Chapter 1 > Open Chapter 1 > Play.
- Scene: Assets/_Civi/Scenes/CIVI-Chapter1-Courtyard.unity và CIVI-Chapter1-DesignRoom.unity.

## Nội dung và cách chơi

An bắt đầu ở sân trường, đi tới máy ảnh cùng Nhi, Hạ, Minh. Nhấn E để tương tác, đọc đủ các line S00 và chọn các hành động hỗ trợ kê máy, tư thế, chụp, đặt tên album, giữ sơ đồ. Ảnh buổi tập: PHOTO_S00_GOOD và hai ảnh hỏng PHOTO_S00_BAD_01/BAD_02, tất cả đều đủ bốn người. Sau S00, đi tới cửa phòng thiết kế, nhấn E chuyển sang Unity scene mới. Tới bàn làm việc, E khởi động S01, xem trang 17, tin nhắn, ảnh 0443 (An/Hạ/Minh, không có Nhi). Hoàn thành S01 mở bảng kết Chương 1 và mốc S02.

Phím: WASD/mũi tên di chuyển; E tương tác; Enter/Space đọc tiếp; 1/2/3 chọn hành động; H hướng dẫn; F5/F9 lưu/tải.

Runtime: CiviChapterOne.cs sử dụng S00Session và StorySession theo JSON/thoại gốc; không thay đổi điều kiện/sự kiện; lưu ở civi-chapter1-v4_1.json trong Application.persistentDataPath. Script điều phối chuyển scene qua static handoff; camera, collision, 16-frame nhân vật An lấy lại từ school environment V2. Thêm 3 sprite NPC riêng cùng máy ảnh/tripod.

## Kiểm tra đã thực hiện

- Python check_ch1_scope.py: PASS, S00 5 bước/11 lựa chọn/29 thoại/27 đường đi hợp lệ tới S01, S01 2 bước/2 lựa chọn/10 thoại tới S02.
- Story Runtime regression: PASS 69/69.
- Unity editor Smoke: PASS, controller/textures/NPC/camera/nav và state machine S00-S01.
- Unity Windows x86: PASS build.
- Unity standalone-player autotest: PASS, đi qua S00→S01 bằng LoadScene, có ảnh/chọn state, sang đúng S02 boundary, lưu state. File log: artifacts/chapter1-handoff-2026-11-01/player-integration.log.
- Chạy đồ họa standalone và chụp screenshot thực tế: artifacts/chapter1-handoff-2026-11-01/chapter1-screen.jpg.
- Chưa xác minh đầy đủ phím/cửa và F5/F9 bằng con người từ đầu tới cuối, do remote keyboard injection không xác định chắc focus. Chưa kiểm thử QHD và hiệu năng dài.

## Tự đánh giá

- Nội dung S00/S01 và luật trạng thái: 8/10.
- Gameplay architecture, tương tác, scene transition: 7/10.
- Camera và đường đi: 7/10, cần input playtest thực tế.
- Art environment/NPC: 4.5/10; chưa đạt phong cách Blue Archive hoặc tiêu chuẩn thương mại cao.
- Audio/UI polish: 4/10; mới có âm chụp nhỏ và HUD/caption, thiếu facial animation, ambience và cutscene chất lượng cao.

Bản bàn giao là functional playable Chapter 1, không phải production-art final. REA được dùng để kiểm tra binary CIVI; chưa reverse-engineer Blue Archive vì không có bản game cục bộ hợp lệ để phân tích. Tham khảo quy trình từ NDC25: https://m.inven.co.kr/webzine/wznews.php?idx=306977.

Đã backup script/cấu hình gốc tại artifacts/chapter1-handoff-2026-11-01. Scenes/bản School V1/V2, S00/S02 cũ và full campaign NewStory vẫn tồn tại, chỉ disabled ở Build Settings. Repo không có .git nên không có commit/push.

## QA lần cuối sau khi đồng bộ repo
- Unity smoke trên repo chính: PASS, log artifacts/chapter1-handoff-2026-11-01/main-smoke.log.
- Windows Player integration trên repo chính: PASS S00, scene transition, S01 và S02 boundary, log artifacts/chapter1-handoff-2026-11-01/main-player-test.log; không ghi nhận Exception trong các dòng được lọc.
- File exe ở release/Chapter1/CIVI_Chapter1_x86.exe có SHA256 9571225C141348E4E16E098B07F21B69ABD31E6E0E751EBF299CA932AD1E741E và trùng bản scratch build.
