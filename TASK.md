# Tiếp tục CIVI_7

## Hiện trạng (05/11/2026)

Chương 1 chơi được từ đầu đến cuối trong bản build Windows 3D. Chi tiết tính năng và lệnh ở `README.md`.

- Unity 6000.4.0f1, URP. Một scene duy nhất, sinh bằng `CiviSceneBuilder`.
- Model sinh bằng script Blender trong `tools/blender`, không sửa tay.
- Logic truyện `Scripts/Domain/ChapterState.cs` giữ từ CIVI_5. Nhiệm vụ phụ ở `SideQuests.cs` + `quests.json`.
- Chưa có git trong thư mục này. **Chỉ commit khi người dùng yêu cầu.**

## Quyết định đã chốt

- CIVI_5 là gốc; mâu thuẫn với CIVI_6 thì theo CIVI_5.
- Phím tương tác là E (giữ F làm phím phụ của CIVI_5).
- Nội dung phụ không được đổi diễn biến của `STORY.md`, chỉ ảnh hưởng báo cáo cuối chương.
- Giữ IMGUI của CIVI_5 thay vì làm lại UI, vì các luồng menu đã được QA. Không tạo project `Game3D` riêng như `WORKFLOW.md` đề xuất.
- Hoạt ảnh nhân vật viết bằng code (`CiviBody.cs`), không dùng Animator: chân không trượt và dùng chung cho mọi tỉ lệ cơ thể.

## Việc tiếp theo

1. Hỏi người dùng có chuyển Chương 2–3 của CIVI_6 không. Lưu ý truyện CIVI_6 khác canon, nên dùng `dialogue/Chuong_2_Ban_Duyet.md` làm nguồn thoại và chỉ lấy cơ chế từ CIVI_6.
2. Người chơi thật thử cảm giác chuột, tốc độ chạy, độ khó báo động B4.
3. Căn lại thời điểm hiệu ứng âm thanh bằng tai.
4. Kiểm giấy phép âm thanh trước khi phát hành.
