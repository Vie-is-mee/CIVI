# Bộ tải và mở CIVI trên Windows

Người chơi tải **Choi-CIVI.bat** từ GitHub Releases và bấm đúp. Lần đầu cần mạng để tải game; những lần sau dùng bản đã cài. Có thể đặt ZIP cùng phiên bản cạnh BAT để cài hoàn toàn ngoại tuyến.

## Hành vi

- Cài riêng cho người dùng vào `%LOCALAPPDATA%\CIVI\Trang17\versions\0.41.1`; không cần quyền admin.
- Game đã kèm Mono và DLL. Không cài Unity Editor, Python, Node, Git hoặc đổi Execution Policy của máy.
- BAT chứa mã PowerShell đọc được, URL cố định theo tag, SHA-256 của ZIP và danh sách băm từng file. Không tải rồi thực thi thêm script từ mạng.
- Kiểm tra file đã cài trước khi mở; bản đủ và đúng không cần mạng. ZIP lỗi bị từ chối trước khi thay bản cũ. Tải/giải nén trong thư mục tạm, kiểm tra rồi mới chuyển sang vị trí cài.
- Tạo lối tắt `CIVI - Trang 17` trên Desktop. Nếu Desktop đã có lối tắt trùng tên trỏ tới ứng dụng khác thì giữ nguyên.
- Tiến trình chơi vẫn ở `%USERPROFILE%\AppData\LocalLow\CIVI Prototype\CIVI Trang 17`; bộ cài không sửa hay xóa thư mục này.
- Không tự cập nhật ngầm. Bản mới đi cùng BAT mới; người chơi chủ động tải bản mới. Không tự xóa phiên bản trước.
- Repo và release phải công khai để người chơi dùng BAT mà không đăng nhập GitHub. Không nhúng token vào BAT.

## Sinh BAT cho một release

Chạy từ gốc repository:

```powershell
& tools/Launcher/New-CiviLauncher.ps1 `
  -Archive release/CIVI-Trang17-0.41.1-Windows.zip `
  -OutputPath release/Choi-CIVI.bat `
  -Repository Vie-is-mee/CIVI -Version 0.41.1

& tools/Launcher/Test-CiviLauncher.ps1 `
  -RealArchive release/CIVI-Trang17-0.41.1-Windows.zip
```

Trong repo phân phối, các script này nằm ở `tools/Launcher/`.

Phải tạo lại BAT mỗi khi ZIP thay đổi, kể cả thêm file hướng dẫn. SHA cố định nghĩa là BAT và ZIP luôn phải được phát hành cùng nhau. Release đã phát hành nên giữ nguyên; dùng version/tag mới cho lần sửa tiếp theo. Chỉ có file đã build và dữ liệu công khai được đưa vào ZIP, không kèm dấu trang hay khóa dịch vụ.

## Kiểm thử

`Test-CiviLauncher.ps1` dùng thư mục tạm trong workspace, không cài vào AppData thật, không mở game và không sửa Desktop thật. Kiểm: cài mới; ZIP cục bộ; chạy lại không mạng; sửa file hỏng; URL download chính xác; lỗi mạng; sai SHA; khóa cài đồng thời; chặn đường dẫn vượt gốc/ZIP traversal; thực thi BAT thật qua CMD và Windows PowerShell; tạo lối tắt trong Desktop giả; cài và băm toàn bộ ZIP game thật. HTTP được giả lập trong bài kiểm; tải qua GitHub thật chỉ kiểm được sau khi release được phát hành.

Máy phát triển kiểm được luồng cài trên Windows hiện tại; chưa thay thế kiểm thử trên máy Windows sạch của người khác.
