# CIVI — Unity 2D Game

## BẢN HIỆN TẠI — LƯU QUANG / CHƯƠNG 1 (04/11/2026)

Chuyển thể **Chương 1 — Thành phố công bằng** của *Những người không có chỗ dưới bầu trời*. Scene Unity: `Game/Assets/_Civi/Scenes/CIVI-LiuQuang-Chapter1.unity`. Bản build Windows cục bộ: `release/LiuQuangChapter1/CIVI_LiuQuang_C1.exe`, mở bằng `PLAY_CIVI_LIUQUANG_CHAPTER1.bat`. **Bản build không nằm trong Git/PR**; sau khi clone hãy mở Unity 6000.6.6f1 và dùng menu `CIVI > Lưu Quang C1 > Prepare, verify and build` để tạo bản Windows trước khi chạy launcher.

Điều khiển: WASD/mũi tên để di chuyển, E tương tác, phím 1–6 hoặc chuột chọn mục, Enter xác nhận, F5/F9 lưu/tải, H trợ giúp. Nội dung và báo cáo bàn giao: [CHAPTER1_LIUQUANG.md](CHAPTER1_LIUQUANG.md). Bản school story cũ và dữ liệu NewStory vẫn được giữ. **Đây là baseline trước khi tích hợp phần `CIVI_5.zip` của đồng đội**.

---

## LỊCH SỬ — CHƯƠNG 1 TRƯỜNG HỌC (01/11/2026)

**Chạy `PLAY_CIVI_CHAPTER1.bat`** ngay trong repo. Đây là bản Windows x86 playable của hai cảnh theo canon học đường v4.1: **S00 “Đừng bấm vội” → S01 “Trang ở giữa”**. Nhân vật An đi trong sân/liền mạch, tương tác với máy ảnh và ba bạn học, xem ba ảnh buổi tập; đi tới cửa phòng thiết kế để chuyển Unity scene, giải quyết nội dung S01 và nhận màn hoàn thành Chương 1.

Trong Unity 6000.6.6f1: **CIVI > Chapter 1 > Open Chapter 1 → Play**. Phím: WASD/mũi tên di chuyển, **E** tương tác, Enter/Space đọc thoại, phím 1–3 chọn, F5/F9 lưu/tải. **Chỉ bản Chapter 1 mới được bật trong Build Settings**; bản V1/V2, S00/S02 cũ và NewStory không bị xóa.

**Bằng chứng QA:** Unity Editor smoke PASS, standalone Windows-player integration PASS S00→S01→S02 boundary, Story Runtime regression 69/69. **Giới hạn:** pixel-art vẫn là prototype; chưa đạt art production Blue Archive, và chưa manual playtest phím end-to-end. Xem [tài liệu bàn giao Chương 1](docs/production/chapter1-playable-handoff-2026-11-01.md).

---

## LỊCH SỬ — SCHOOL VISUAL V2 (31/10/2026)

## BẢN TEST MỚI NHẤT: SCHOOL VISUAL V2 (31/10/2026)

**Bạn chỉ cần chạy `PLAY_CIVI_SCHOOL_V2.bat`** ở thư mục repo. Không phải import hoặc gán asset. Bản mới nâng cấp mỹ thuật và nhân vật, có 2 scene (khuôn viên trường học + phòng thiết kế) nối qua cửa E; camera cuộn liền mạch trong mỗi scene. Nhân vật có sprite 4 hướng × 4 frame; ground/building/props/foreground độc lập.

Trong Unity 6000.6.6f1: chọn `CIVI > School World V2 > Open Campus`, nhấn Play. Phím **WASD** di chuyển; **E** tương tác và mở cửa; **H** trợ giúp; **F5/F9** lưu/tải. Xem [hướng dẫn test + giới hạn chất lượng](docs/engineering/school-visual-v2-handoff.md).

Bản môi trường mới là art/gameplay prototype; các bản school v1, S00/S02 và Lưu Quang **vẫn được giữ** phục vụ team, không bị ghi đè.

---

# CIVI — Unity Game

## BẢN TEST ĐANG DÙNG — SCHOOL WORLD / BẢN ĐỒ TRƯỜNG HỌC LIỀN MẠCH

**Chạy ngay `PLAY_CIVI_SCHOOL_WORLD.bat`** ở thư mục gốc. Không cần tự import asset, tạo scene, hay gán script. Bản đồ trường học có một không gian 152×60 tile: cổng, bãi xe đạp, sân, hành lang, lớp học, phòng thiết kế và thư viện; camera theo nhân vật liên tục. Không phải tranh nền ghép từng màn.

Trong Unity 6000.6.6f1: `CIVI > School World > Open Seamless Campus` → Play. Điều khiển **WASD**, tương tác **E**, **H** hướng dẫn, **F5/F9** lưu/tải; camera zoom bằng con lăn chuột.

Bản School World là môi trường gameplay độc lập đang được ưu tiên để test; chưa phải phần chuyển thể đã gắn toàn bộ nhiệm vụ của các kịch bản khác. [Hướng dẫn và QA](docs/engineering/school-connected-world-v1.md). Nguồn art tile **Kenney CC0**. Bản cũ và bản campaign Lưu Quang vẫn được giữ an toàn.

---

## LỊCH SỬ / BẢN TEST CŨ — FULL CAMPAIGN (31/10/2026)

**Chỉ cần chạy `PLAY_CIVI_NEW_STORY.bat` ở thư mục gốc**. Không phải mở Unity, không cần tạo scene, không cần sửa code.

Nếu dùng Unity 6000.6.6f1: `CIVI > NEW STORY > Open Full Campaign` rồi nhấn Play. Build Settings chỉ chứa `CIVI-NewStory-FullCampaign.unity`.

Game dựa trên bản phân nhánh 28/10/2026 của *Những người không có chỗ dưới bầu trời*: **10 chương, 4 cảnh rẽ, tuyến C, 2 tuyến kết thúc thay thế; 12 tổ hợp lựa chọn và 3 kết thúc**. Mỗi phần truyện là 3 screen nối nhau có camera theo nhân vật; nhấn E để đọc điểm chuyện, R để mở toàn văn 1.033 đoạn; F5/F9 lưu/tải. Quyết định bằng phím 1/2/3 ở cuối chương 5, 7, 9.

**Cảnh báo:** đây là bản kiểm thử campaign hoàn chỉnh về nội dung và phân nhánh, **chưa phải bản game production** về art/gameplay. Nền 2D vẫn có độ phân giải nguồn giới hạn và được tái sử dụng; NPC, puzzle riêng, collision và animating chưa hoàn thiện.

[Hướng dẫn test, QA và giới hạn](docs/production/full-campaign-test-2026-10-31.md) · `tools/check_full_campaign.py` · bản sao lưu `artifacts/campaign-handoff-2026-10-31`.

---

## ARCHIVE — Những bản prototype không còn dùng để test cuối

Các mục bên dưới mô tả tiến trình kỹ thuật trước đây, **không phải hướng chạy campaign mới**. NS01/NS02 đã chuyển khỏi Assets sang `artifacts/legacy-prototypes-2026-10-31`. Các scene S00/S02 thuộc tuyến kỷ yếu cũ vẫn được giữ phục vụ team nhưng bị loại khỏi build hiện tại.

## OLD NS02 — Art Playtest (30/10/2026)

Mở project `Game` bằng Unity 6000.6.6f1, chọn **CIVI > New Story > Open Art Vertical Slice (v2)**, rồi nhấn Play. Bốn nền concept có thể chuyển bằng `1`, `Q` (bão), `2`, `3`; di chuyển bằng WASD, E để xem địa điểm, con lăn để zoom. Bản NS01 cũ còn nguyên để so sánh. Xem [hướng dẫn test](docs/engineering/new-story-art-vertical-slice-v2.md) và [bảng tự đánh giá/giới hạn chất lượng ảnh](docs/production/new-story-art-v2-self-review.md). Nền mới vẫn là preview 595x335 được upscale, **không phải native-resolution art production**.


## P2 — Điều tra & suy luận tương tác (27/10/2026)

Đã bổ sung chế độ xem ảnh S03 với zoom/kéo, câu hỏi quan sát theo từng ảnh, so sánh cặp 0441–0442/0442–0443 và bảng giả thuyết. Chứng cứ canon chỉ do StorySession ghi nhận; suy luận không tự cấp quyền ảnh. Chơi bằng scene 2D sẵn có, không cần tải thêm. Xem [hướng dẫn P2](docs/engineering/p2-investigation-deduction.md).


## P1 mới: Camera top-down 3/4 (27/10/2026)

Bản S02–S03 hiện có **camera bám theo An, zoom bằng con lăn, phòng 3/4 riêng cho S02/S03, lớp trước/sau đồ vật và avatar An có hướng đứng/đi placeholder**. Mở lại scene 2D như P0, không cần dựng scene mới hoặc tải asset. Xem [hướng dẫn P1](docs/engineering/p1-quarter-camera-view.md). Đây vẫn là bản greybox để kiểm tra gameplay trước khi thay bằng bộ art chính thức.


## P0 mới: Game 2D điều khiển An (27/10/2026)

Đã thêm scene **`Game/Assets/_Civi/Scenes/S02-2D-VerticalSlice.unity`**: WASD, E để tương tác, bảng ảnh B, F5/F9 lưu/tải, logic chứng cứ từ JSON S02/S03 và cầu nối từ cuối S01. Đây là **greybox kỹ thuật**, chưa có animation/tileset production; không thay thế demo S00.

Hướng dẫn: **[P0 Exploration 2D](docs/engineering/p0-2d-exploration.md)**. Trong Unity có menu `CIVI > TV2 > Mở scene khám phá 2D`.


Thư mục làm việc theo cây mới của nhóm. Bản thử trước đó được giữ riêng để tham khảo; không có thư mục `../CIVI` trong gói giải nén này.

## Xem tiến độ TV3 trước

**[00-TIEN-DO-TV3.md — đã làm gì, còn thiếu gì, làm gì tiếp](00-TIEN-DO-TV3.md)**

**[Tám nhiệm vụ riêng của thành viên 3 — W3.1 đến W3.8](docs/production/tv3-task-status.md)**

**[Mở bản S00](01-CHOI-THU-S00.bat)** · [Phiếu nhận xét TV1/TV5](docs/qa/TV3-NHAN-XET-S00.md) · [TV2 nhận mã tích hợp](docs/engineering/tv3-s00-integration.md)

Ảnh ở `Game/Assets/_Civi/Art`, âm thanh ở `Audio`, phông ở `Fonts`. Có file không đồng nghĩa đã duyệt. Các thư mục UI/prefab/video ghi rõ phần đã làm và phần còn thiếu.

## Bắt đầu với TV1

- [Hướng dẫn bộ bàn giao TV1](docs/story/TV1_BAT_DAU_O_DAY.md).
- [Kịch bản để đọc và góp ý](docs/story/kich-ban-doc-thuan.md).
- [Kịch bản có điều kiện và hệ quả dữ liệu](docs/story/kich-ban-mvp.md).
- [Tình trạng W1.1–W1.8](docs/production/tv1-task-status.md).
- [Cấu trúc và toàn bộ 40 đầu việc](CAU_TRUC_VA_TINH_NANG.md).

## Hiện có gì?

Cây thư mục và gói nội dung TV1 0.1 đã được tạo: canon, phạm vi MVP, state/quyền, kiến thức NPC, lịch, 15 cảnh/nhánh kết cùng thoại, ba template nháp, phản hồi ý định S03 và brief tài nguyên. Nội dung mới chờ người nhận kiểm duyệt.

TV3 đã có 29 media, sáu prefab UI (thoại/lựa chọn/ba mẫu nháp/DraftReview), bốn prefab đạo cụ, AssetCatalog, scene S00 và bản Windows đọc/bấm được. S00 có ba cặp callback gesture và animatic phụ đề/hiệu ứng; UI W3.4 có bàn thử chín tổ hợp mẫu/người nhận. Model/build/UI 720p và 1080p đã kiểm. Xem trang tiến độ và báo cáo hash để xác định kết quả còn khớp bản hiện tại hay không.

Cây này **đã tích hợp S00**, dừng ở điểm nối S01. Chưa tích hợp S01–S10/các kết, chưa AI service, chưa có nhận xét người chơi thật hoặc xác nhận nhóm duyệt. S00Session chỉ thực thi lệnh S00; không phải runtime đầy đủ cho contract TV1/TV4. Bản chạy cũ vẫn giữ riêng.

Sau khi kiểm đã dọn khoảng 422 MiB cache Unity tự sinh. Bản Windows vẫn mở bình thường; lần đầu mở project bằng Unity sẽ nhập lại tài nguyên. Giữ `Assets` và `.meta`, `Packages`, `ProjectSettings`; không cần mang `Library/Temp/Logs` khi bàn giao.

## Kiểm dữ liệu

Từ thư mục này, dùng Python 3:

```powershell
python tools/Content/validate_tv1_content.py
```

Script chỉ dùng thư viện chuẩn; kiểm ID/tham chiếu và mô phỏng nội dung. Báo cáo lần chạy hiện tại nằm ở [kết quả kiểm TV1](docs/qa/tv1-content-validation.md). Việc nạp Unity, UI/media, lưu ảnh thực và hiệu quả AI vẫn phải kiểm riêng.
