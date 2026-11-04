# CIVI — NEW STORY FULL CAMPAIGN / BẢN TEST NGÀY 31-10-2026

## Chạy game

**Cách nhanh nhất (không cần Unity Editor):** mở file `PLAY_CIVI_NEW_STORY.bat` tại thư mục gốc repo. File này chạy bản Windows x86 `release/NewStoryCampaign/CIVI_NewStory_Test_x86.exe`. Giữ nguyên cả folder release, không lấy riêng exe vì sẽ mất data và UnityPlayer.dll.

**Trong Unity:** mở project `Game` với Unity **6000.6.6f1**, vào menu `CIVI > NEW STORY > Open Full Campaign` hoặc mở `Assets/_Civi/Scenes/CIVI-NewStory-FullCampaign.unity`, rồi Play. Chỉ scene này hiện nằm trong Build Settings.

## Thao tác

- Enter hoặc Space: bắt đầu, đọc tiếp câu chuyện.
- WASD hoặc mũi tên: đi qua **3 màn nối nhau** trong từng phần truyện, camera tự bám.
- E: tương tác với viên kim cương sáng; sau khi đọc đủ 3 địa điểm, đến mép phải màn thứ ba để sang chương/nhánh.
- R: mở **toàn văn** đúng đoạn hiện tại; Enter / → / PageDown để đọc đoạn tiếp; ← / PageUp để xem đoạn trước; Esc đóng. Đây là toàn bộ văn bản gốc, không phải bản tóm tắt được AI tự thêm.
- Tại ba điểm rẽ (cuối chương 5, chương 7, chương 9): phím **1, 2, 3** chọn nhánh theo đúng tài liệu gốc.
- F5: lưu; F9: tải. Lưu ở `Application.persistentDataPath/civi-new-story-campaign-v1.json`.

## Truyện và diễn tiến

Bản game dùng `Game/Assets/_Civi/Resources/NewStory/campaign.json`, xây dựng từ nguyên bản `novel_source.txt` thuộc file Google Drive ngày 28/10/2026. Phân nhánh giữ chính xác:

- Chương 1–5 → I-A hoặc I-B → chương 6–7.
- Chương 7 → II-A hoặc II-B → chương 8–9.
- Chương 9 → A hoặc B (kết thúc riêng, **không** đi tiếp chương 10), hoặc tuyến C → cuối chương 9 → chương 10.
- Có 17 đoạn nội dung được lưu, 1.033 đoạn văn, 12 tổ hợp ba quyết định và 3 kết thúc. Các đoạn văn được giữ nguyên nội dung truyện sau khi bỏ Markdown/siêu liên kết biên tập.
- Trong gameplay, mỗi chương có ba điểm ghi nhận, mỗi điểm mở hai trích đoạn quan trọng từ nguyên bản để giữ tốc độ khám phá. Nhấn **R** để đọc toàn bộ đoạn văn của phần hiện tại. Các trích đoạn không thay thế bản gốc.

## Gameplay và môi trường

- **3 screen liên tục mỗi phần truyện**: nhân vật đi bộ nối tiếp qua các màn, camera cuộn theo (không chuyển bằng phím 1/2/3 như NS02).
- Hình ảnh lấy từ bốn bộ environment preview đã import của Rìa Mưa, Rìa Mưa bão, Trạm lọc 19 và Lõi Sáng. Nội dung nhánh trỏ tới khu vực theo truyện, nhưng nhiều địa điểm chưa có background riêng.
- Đây là **campaign narrative test**, không phải build phát hành thương mại: sprite Mai vẫn là placeholder, ảnh môi trường đang dùng nguồn preview Canva 595×335 upscale, màn nối có thể nhìn thấy mép tranh, collider là vùng đi lại đơn giản. Chưa có AI NPC, gameplay giải đố độc lập, âm thanh/hiệu ứng nghệ thuật cuối cùng hay art riêng cho mọi địa điểm.

## Kết quả tự đánh giá và QA

- **PASS** Unity 6000.6.6f1 compile, tạo scene, Editor smoke check (17 nội dung, ba màn, player/camera, tuyến A/B/C).
- **PASS** `python tools/check_full_campaign.py`: 12 tổ hợp nhánh, kiểm chứng SHA-256 nguyên bản, 1.033 đoạn văn.
- **PASS** Windows standalone x86 Player build, do Unity 6000.6.6f1 trên máy không có WindowsPlayer.exe x64.
- **Chưa xác nhận hoàn toàn** thao tác người chơi qua mọi nhánh ở Windows QHD; cần người chơi kiểm tra chuột/phím, viewport, FPS, lối đi, quá trình lưu/tải.
- **Production quality chưa đạt**: thiếu asset gốc độ phân giải thực và không gian modular độc lập của từng chương. Không coi prototype này là game đã hoàn thiện.

## Dọn dẹp repo an toàn

- Hai demo lỗi thời NS01 và NS02, mã Editor builder và nguồn thử nghiệm liên quan đã chuyển sang `artifacts/legacy-prototypes-2026-10-31`, không xóa vĩnh viễn.
- Scene S00/S02 thuộc kịch bản kỷ yếu cũ của team được giữ nguyên phục vụ rollback, nhưng **không nằm trong build mới**.
- Snapshot Build Settings và README trước thay đổi nằm trong `artifacts/campaign-handoff-2026-10-31`.
- Không có .git trong repo, do đó **chưa commit/push**. Không tự init Git.
- Toàn bộ thử nghiệm compile và build đã chạy trên bản sao project, sau đó mới copy các file đã kiểm chứng vào repo chính. Smoke Unity trên repo chính PASS.

## Checklist cho người test

1. Chạy file BAT; thử Enter bắt đầu, WASD qua ba màn của chương 1.
2. Nhấn E ở ba viên kim cương, đọc hết; tới mép phải màn cuối nhấn E để chuyển chương 2. Xác nhận camera không dừng ở một ảnh.
3. Nhấn R để xem toàn văn, lật trang qua lại và Esc trở ra.
4. Ở chương 5 chọn I-A, chương 7 chọn II-B, chương 9 chọn C: phải tới chương 10.
5. Chơi lại lựa chọn cuối A hoặc B: phải dừng ở đúng kết thúc, **không** nhảy sang chương 10.
6. F5 lưu tại chương bất kỳ; đóng mở game, F9 tải lại, đối chiếu chương và vị trí.
7. Ghi lại screenshot, FPS và Console/Player.log nếu có lỗi.

### Chấm chất lượng hiện tại
- Story coverage/branching: **đạt prototype**.
- 2D roaming giữa nhiều khung hình: **đạt về cơ chế**.
- Đồ họa và chiều sâu vật lý thực sự của 10 chương: **chưa đạt yêu cầu production**.
- Tổng thể: **bản game thử nghiệm có đầu–cuối hoàn chỉnh về tuyến đọc/nhánh**, không phải bản final commercial quality.
