# CIVI · Trang 17 không tồn tại

Một trang kỷ yếu bị rút khỏi bản in thử. Ảnh có ba người, bản nháp ghi bốn tên — còn lời hẹn hôm đó là gì?

Bản chơi thử kể chuyện tương tác bằng tiếng Việt, có tư liệu để xem, lựa chọn đối thoại và các kết quả khác nhau. Nội dung hiện được rút gọn từ kịch bản v4.1; chưa tích hợp dịch vụ AI hội thoại.

## Chơi trên Windows

**[Tải Choi-CIVI.bat](https://github.com/Vie-is-mee/CIVI/releases/download/v0.41.1/Choi-CIVI.bat)** từ [bản phát hành 0.41.1](https://github.com/Vie-is-mee/CIVI/releases/tag/v0.41.1), rồi bấm đúp file vừa tải.

Lần đầu, BAT tải và kiểm tra gói game, cài vào thư mục người dùng, tạo lối tắt trên Desktop rồi mở game. Những lần sau, bấm lối tắt **CIVI - Trang 17** hoặc mở lại BAT; không cần mạng nếu bản cài vẫn đầy đủ.

Chọn **Mở trang đầu** để bắt đầu. Không cần cài Unity, Git, Python hay Node. Bản này dành cho Windows 10/11 64-bit; chưa có bản macOS, Linux hoặc điện thoại. Lần đầu cần mạng và khoảng 250 MB dung lượng trống trong lúc cài.

Nếu muốn tự giải nén: tải **CIVI-Trang17-0.41.1-Windows.zip** trong cùng release, giải nén toàn bộ rồi mở **CIVI-Trang17.exe**. Đừng chỉ lấy riêng EXE hoặc tải “Source code” để chơi. Có thể đặt ZIP cạnh BAT để cài không cần mạng.

## Điều khiển

| Thao tác | Cách dùng |
|---|---|
| Đọc tiếp | Bấm **Tiếp tục** hoặc Space / Enter; khi chữ đang chạy, lần bấm đầu hiện hết câu |
| Chọn hành động | Bấm nút; phím 1–3 dùng khi bảng lựa chọn đã mở |
| Xem tư liệu / lời vừa nói | J / H |
| Tạm dừng, âm thanh, nhịp chữ | Esc |
| Lưu và quay lại | **Đặt dấu trang** / **Về dấu đã lưu** |

Lưu lại giữ cả vị trí và những lựa chọn đã thực hiện. Chơi lại không tự xóa dấu đã lưu. Cài lại game không xóa tiến trình: dữ liệu nằm riêng tại `%USERPROFILE%\AppData\LocalLow\CIVI Prototype\CIVI Trang 17`.

## Gửi nhận xét

Ghi giúp nhóm đoạn nào muốn đọc tiếp hoặc muốn bỏ qua, câu nào khó hiểu, và lựa chọn nào chưa tạo khác biệt. Nếu gặp lỗi, gửi kèm ảnh và thao tác ngay trước đó. Có thể dùng [Issues](https://github.com/Vie-is-mee/CIVI/issues); đừng đăng thông tin riêng tư trong báo lỗi công khai.

## Cho người phát triển

- `Game/`: mã và tài nguyên cần cho bản Trang 17, tách gọn khỏi các bản thử cũ. Dùng Unity **6000.6.5f1**. Mở thư mục này bằng Unity Hub; menu **CIVI > Trang 17 v4.1 > Chơi bản UI mới** để xem thử.
- `tools/Build-Windows.ps1`: xuất bản Windows vào `release/Windows` bằng `Civi41Editor.Build`; build cũng chạy kiểm tra mô hình lựa chọn.
- `tools/Package-Windows.ps1`: tạo ZIP, BAT và SHA256SUMS trong `release/` sau khi build. Đổi phiên bản trong mã build trước khi phát hành một bản mới.
- `tools/Launcher/`: nguồn bộ tải, công cụ sinh BAT từ ZIP và bài kiểm tự động. [Chi tiết](tools/Launcher/README.md).
- `Choi-CIVI.bat`: launcher một file, ghim đúng phiên bản 0.41.1 và SHA-256. Gói chạy nằm trong GitHub Releases, không nằm trong lịch sử Git.
- [Ghi chú phát hành](docs/RELEASE-0.41.1.md) · [Tài nguyên và giấy phép](docs/ASSETS.md).

Không đưa cache Unity, bản cũ, dữ liệu lưu, token hoặc khóa API vào repo. Bản nguồn dùng Built-in để tái tạo bản chạy này; không thay thế cấu hình URP của dự án làm việc gốc.
