# CIVI — Chương 1. Thành phố công bằng

Chạy [PLAY_CIVI_LIUQUANG_CHAPTER1.bat](PLAY_CIVI_LIUQUANG_CHAPTER1.bat). Đây là bản chuyển thể Chương1 của **Những người không có chỗ dưới bầu trời**, với Mai trưởng thành ở Lưu Quang. Các launcher School và campaign cũ vẫn dùng bản riêng.

Đi bằng WASD hoặc phím mũi tên; đến gần mục tiêu rồi nhấn E. Dùng chuột hoặc phím1–6 để kiểm tra hồ sơ và phối hợp phương án; Enter để xác nhận hoặc tiếp lời. H mở hướng dẫn, Esc đóng bảng/tạm dừng, M bật/tắt âm thanh. Nếu bộ gõ tiếng Việt đang bật, dùng phím mũi tên.

Tiến độ tự lưu sau mỗi thao tác/phân đoạn. F5 lưu, F9 tải; ở màn đầu chọn **Tiếp tục bản lưu**. Save mới tên `civi-liuquang-c1-v1.json` trong thư mục dữ liệu Unity của ứng dụng, tách khỏi save học đường/campaign. Bắt đầu mới rồi thực hiện thao tác sẽ thay tiến độ hiện tại của riêng C1 này. Game kết thúc ở ảnh phân khu8-12 vẫn ghi0; không mở Chương2.

## Project và source

- Unity6000.6.6f1; mở project `Game`.
- Scene: [CIVI-LiuQuang-Chapter1.unity](Game/Assets/_Civi/Scenes/CIVI-LiuQuang-Chapter1.unity).
- Menu build: **CIVI → Lưu Quang C1 → Prepare, verify and build**.
- Controller và UI: `Game/Assets/_Civi/Scripts/Application/Exploration/CiviLiuChapter*.cs`.
- Môi trường: `CiviLiuEnvironment.cs`; controller di chuyển giữ từ polished qua `CiviSchoolStudioV2`.
- Nội dung chuyển thể: `Game/Assets/_Civi/Resources/LiuQuangC1/chapter.json`.
- Asset và provenance: `Game/Assets/_Civi/Art/LiuQuangC1/`.
- Windows portable: `release/LiuQuangChapter1/`. Giữ cả thư mục cùng nhau khi sao chép, không chỉ lấy tệpexe.

## Bàn giao và bằng chứng

- [Báo cáo QA / giới hạn kiểm chứng](artifacts/chapter1-liuquang-2026-11-02/HANDOFF.md).
- [Reuse / Adapt / Replace / Retire Matrix](artifacts/chapter1-liuquang-2026-11-02/MIGRATION.md).
- [Đối chiếu từng sự kiện với canon](artifacts/chapter1-liuquang-2026-11-02/EVENT-FIDELITY.md).
- [Ảnh gameplay trước/sau](artifacts/chapter1-liuquang-2026-11-02/SCREENSHOTS.md).
- [Đánh giá từng mặt và phần thực sự được cải thiện](artifacts/chapter1-liuquang-2026-11-02/REVIEW.md).

Bản Chapter1-Polished tại `artifacts/chapter1-polish-2026-11-01/work/Game` được giữ nguyên. Bản làm việc Lưu Quang và bản sao các tệp gốc bị thay tại `artifacts/chapter1-liuquang-2026-11-02/work` và `rollback` phục vụ kiểm tra/khôi phục. Nội dung NewStory, source học đường, cảnh cũ và QA cũ được giữ lại.
