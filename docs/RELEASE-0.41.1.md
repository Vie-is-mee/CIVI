# CIVI / Trang 17 — 0.41.1 · 20/10/2026

Bản chơi thử Windows 64-bit, có nội dung rút gọn theo kịch bản v4.1. Chưa kết nối dịch vụ AI hội thoại.

- Mở màn trên bàn tư liệu: ảnh 0443, bốn tên và trang in thử bị rút. Chạm tư liệu trước khi quay về buổi tập.
- Cảnh thương lượng với Hạ có hệ quả giữ/đổi ảnh, nhận phần viết lại hoặc chờ hỏi Nhi. Một lời nhận lỗi không tự giải quyết bất đồng.
- Có các kết quả ảnh mới bốn người, ba người, giữ ảnh cũ hoặc cuộc hẹn khác; lưu dấu trang riêng với bản chơi cũ.
- Thêm launcher Windows một file: tải một lần, kiểm SHA-256 và từng file, tạo lối tắt, mở lại không mạng. Cài vào thư mục người dùng, không cần Unity hoặc quyền admin.
- Không thay đổi cốt truyện hoặc binary game khi thêm bộ cài. ZIP được bổ sung giấy phép phông chữ.

## File tải

- `Choi-CIVI.bat`: tự tải/cài/mở game, hoặc dùng ZIP cùng thư mục để cài ngoại tuyến.
- `CIVI-Trang17-0.41.1-Windows.zip`: gói chạy đầy đủ, cũng có thể tự giải nén và mở EXE.
- `SHA256SUMS.txt`: đối chiếu ZIP và BAT nếu cần.

## Kiểm tra

Bản game trước khi đóng gói đã qua build Unity 6000.6.5f1 với 0 lỗi, 0 cảnh báo, và kiểm bốn đường chơi, lưu/khôi phục, chuyển dấu v1, lựa chọn, ảnh cuối cùng và UI ở 1280×720 trên Intel UHD / Direct3D 11.

Bộ cài kiểm bằng Windows PowerShell 5.1 và CMD thật: cài mới; chạy lại không mạng; file hỏng; sai SHA; lỗi tải; cài đồng thời; đường dẫn có dấu; ZIP traversal; lối tắt Unicode; giữ lối tắt trùng tên của ứng dụng khác; giải nén và kiểm 166 file của ZIP thật. HTTP được giả lập ở bài kiểm tự động; tải GitHub thật cần kiểm sau khi phát hành release.

Chưa có kết quả thử trên máy Windows sạch khác hoặc xác nhận sức hút từ người chơi thật. Chưa có rig nhân vật hay lồng tiếng hoàn chỉnh.
