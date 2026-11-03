# CIVI — SCHOOL WORLD / Connected Campus v1

## Bản test hiện tại

Mục tiêu: thay các tranh background ghép nối bằng **một khuôn viên trường học 2D liên tục**. Bản này độc lập với *CIVI New Story Campaign* (truyện Lưu Quang) và không thay thế script S00–S02 của nhóm trước đó. Chỉ phục vụ thiết kế môi trường theo yêu cầu game học đường; không tự sửa canon của nhóm.

**Chạy ngay, không mở Unity:** double-click `PLAY_CIVI_SCHOOL_WORLD.bat` tại thư mục gốc. Build Windows x86 tại `release/SchoolWorld/CIVI_SchoolWorld_x86.exe` (giữ cả thư mục kèm Data và UnityPlayer).

**Trong Unity:** dùng **Unity 6000.6.6f1**, chọn menu `CIVI > School World > Open Seamless Campus`, sau đó Play. Scene: `Assets/_Civi/Scenes/CIVI-School-SeamlessCampus.unity`. Đây là scene được đánh dấu enabled đầu tiên trong Build Settings; campaign cũ được giữ nhưng disabled cho bản build thử.

## Cấu trúc bản đồ

Dùng **152 × 60 tiles, mỗi tile 16 × 16 pixel**, một hệ tọa độ Unity duy nhất; camera orthographic trượt liên tục theo An, không có cutscene slide hay tải ảnh khi đi qua mỗi khu.
- Cổng trường, bãi để xe đạp
- Sân chính, cột cờ, cây/ghế
- Hành lang tầng 1 liền mạch
- Lớp học với bàn ghế và lối vào
- Phòng câu lạc bộ / thiết kế
- Thư viện với kệ sách

Tất cả được dựng bằng một bộ **Kenney RPG Urban Pack (CC0-1.0)** và các mô-đun gốc bổ sung; bốn layer độc lập là ground, architecture, props, foreground, kèm navigation mask, marker tương tác và chuyển động có va chạm. Không dùng ảnh AI lớn ghép lại làm level. Source có thể build lại ở `tools/school_modular/build_school_assets.py`; 486 tile PNG nguồn được lưu tại `source-assets/kenney-rpg-urban-pack/Tiles`.

## Điều khiển

- WASD / mũi tên: di chuyển trong cùng bản đồ; camera tự follow, không đổi background khi qua khu vực.
- Con lăn chuột: zoom.
- **E**: mở thông tin tại viên kim cương vàng. Cần khám phá đủ 6 địa điểm.
- **F5 / F9**: lưu / tải tiến độ, file lưu độc lập `civi-school-world-v1.json` trong `Application.persistentDataPath`.
- **H**: hướng dẫn; **Esc/E** đóng bảng tương tác.

## Cách test thực tế

1. Chạy `PLAY_CIVI_SCHOOL_WORLD.bat`. Xem chữ tiếng Việt và khu cổng.
2. Đi **về phía phải** qua bãi xe và sân trường, tiếp tục vào hành lang. Camera phải **cuộn liên tục**, không nhìn thấy đường tiếp giáp ảnh hoặc màn hình loading.
3. Đi lên cửa lớp từ hành lang, di chuyển quanh bàn ghế; thử cố xuyên qua bàn/kệ/tường.
4. Tiếp tục tới phòng thiết kế và thư viện, đi lại ngược về sân trường; tất cả nằm trong một level.
5. Ở mỗi hotspot vàng, nhấn E, kiểm tra popup, số 0/6 đến 6/6.
6. F5 → thoát → mở lại → F9. Xem vị trí, điểm đã khám phá.
7. Test tại 1280×720 và 2560×1440; ghi screenshot và hiện tượng camera/collision/UI nếu thấy lỗi.

## QA đã thực hiện

- Unity 6000.6.6f1 **compile PASS**, tạo scene PASS.
- **Editor smoke PASS**: 1 map 152×60, 6 hotspots, player, camera, art layers, nav.
- **Python BFS**: 6/6 địa điểm đi tới được từ cổng.
- Windows standalone **x86 build PASS**.
- Đã chạy standalone trên GPU Intel Iris Xe, xem Game View tại cổng; không phát hiện Unity exception trong Player log khởi tạo.
- Đã tự đánh giá về art và chỉnh thêm cây lớn, nhà trường/biển hiệu, xe đạp, cờ, tông mặt sàn giảm lặp trước khi chuyển vào repo chính.
- **Chưa xác minh** thao tác con người đi hết bản đồ bằng bàn phím và va chạm mọi vật; cần tester thực tế.

## Chưa phải production release

Pixel art đã có visual consistency và camera seamless, nhưng vẫn là **môi trường v1**: chưa có mọi NPC/animation, school storyline S00–S10 chưa gắn nhiệm vụ vào toàn bộ map, chưa có soundtrack, VFX thời tiết, level art handmade chất lượng cao, gamepad và QA hiệu năng/thiết bị. Chưa có scene tầng 2 chân thực (thang chỉ ở mức điểm nối mở rộng), vì vậy không gọi là full school production.

## Nguồn art / quyền sử dụng

Kenney RPG Urban Pack, Creative Commons Zero 1.0, cho phép sử dụng thương mại và chỉnh sửa. Nguồn: https://kenney.nl/assets/rpg-urban-pack. Bản dùng tilesource lấy từ bản sao GitHub CC0; `Assets/_Civi/Art/SchoolWorld/LICENSE.txt` ghi rõ provenance.

## Bảo toàn dữ liệu cũ

Trước khi đổi Project Settings/Build Settings/README, đã lưu bản cũ tại `artifacts/school-world-handoff-2026-10-31`. Các scene `CIVI-NewStory-FullCampaign`, `S00-TV3-Preview`, `S02-2D-VerticalSlice` và Windows release trước vẫn còn, không xóa. Repo không có `.git` nên không commit/push.