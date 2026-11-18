# Tiếp tục CIVI_7

## Hiện trạng (05/11/2026)

Chương 1 đến 3 chơi được từ đầu đến cuối trong bản build Windows 3D. Chi tiết tính năng và lệnh ở `README.md`.

- Unity 6000.4.0f1, URP. Một scene duy nhất, sinh bằng `CiviSceneBuilder`.
- Môi trường sinh bằng script Blender trong `tools/blender`, không sửa tay.
- Nhân vật: phong cách toon, sinh bằng `tools/blender/build_characters.py`. Hoạt ảnh viết bằng code (`CiviBody.cs`), mặt là atlas biểu cảm từ `tools/make_faces.py`.
- Âm thanh: `tools/audio_plan.py` gắn từng cue vào câu trong kịch bản, `tools/import_audio.py` nhập gói từ `AUDIO/`.
- Logic truyện `Scripts/Domain/ChapterState.cs` giữ từ CIVI_5. Chương 2-3 chạy cùng máy trạng thái, dàn cảnh nằm trong `chapter.json`, code ở `CiviGame.Chapters.cs`.
- Chưa có git trong thư mục này. **Chỉ commit khi người dùng yêu cầu.**

## Quyết định đã chốt

- CIVI_5 là gốc; mâu thuẫn với CIVI_6 thì theo CIVI_5. Thoại chỉ lấy từ `dialogue/`.
- **Nhân vật giữ phong cách toon.** Đã thử hướng bán thực kiểu Endfield (MakeHuman, mô phỏng vải, mocap) và người dùng bỏ vì không ưng hình ảnh. Toàn bộ phần đó lưu ở `artifacts/semi-real/`, không nằm trong game.
- Chuột và tốc độ chạy đã được người dùng duyệt, không đổi.
- Phím tương tác là E (giữ F làm phím phụ của CIVI_5).
- Nội dung phụ của Chương 1 không được đổi diễn biến của `STORY.md`, chỉ ảnh hưởng báo cáo cuối chương.
- Giữ IMGUI của CIVI_5 thay vì làm lại UI, vì các luồng menu đã được QA.
- AO lấy mẫu trong shader, không dùng After Opaque, để da nhận rất ít AO. Đây là cách sửa gốc của lỗi mặt đen.

## Việc tiếp theo

1. Người chơi thật xem 7 nhân vật mới và Rìa Mưa, chỉ ra chỗ cần chỉnh.
2. Nghe thử và cân lại âm lượng các lớp ambience, kiểm giấy phép âm thanh trước khi phát hành.
3. Chương 4: `dialogue/Chuong_4_Ban_Duyet.md`, thêm bảng dàn cảnh trong `tools/compile_chapters.py`.
