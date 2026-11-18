# Kiểm và tích hợp bộ hội thoại — 03/11/2026

## Phạm vi thực hiện

Người dùng xác nhận **chỉnh file Markdown và tích hợp chương 1 vào Unity**. Đã kiểm cấu trúc/điểm nối/format của cả **17 file, 164 cảnh, 3.702 cue**, trong đó có 1.807 lượt nói. Biên tập câu chữ/ngữ cảnh tập trung vào chương 1 đang chơi và một chỗ làm rõ dữ liệu tổng hợp ở chương 6. Không viết lại đại trà lời thoại các chương tương lai hoặc coi lượt kiểm kỹ thuật này là nhóm tác giả đã duyệt lại mọi câu.

Nguồn truyện `STORY.md` được giữ nguyên. Các file trong `D:\VSIC\GAME\CIVI_5\dialogue` đã được sửa trực tiếp; `dialogue.zip` giữ nguyên. Bản gốc từng file ở `artifacts/dialogue-review/original`, diff đầy đủ `artifacts/dialogue-review/review.patch`, hash trước/sau `changes.json` cùng thư mục.

## Các chỉnh sửa trong bản thảo

| Vị trí | Điều chỉnh | Lý do |
|---|---|---|
| Chương 1, cảnh 4 | “Trước cuộc họp buổi sáng” → “Sáng hôm ấy, trước cuộc họp chiều” | Khớp bữa trưa và phiên họp Vinh trong truyện |
| Chương 1, cảnh 4 | Tài xế xác nhận đã đổi giờ nghỉ và giao thuốc xong | Khớp việc Mai kể lại vào bữa trưa và yêu cầu đối chiếu giờ làm cuối ngày |
| Chương 1, cảnh 4 | Mai nhờ hiệu trưởng xác nhận phòng thay thế an toàn | Giữ giọng làm việc cùng nhau, không thêm ý trách người quản trường tự ý gây nguy hiểm |
| Chương 1, cảnh 5/6 | Tách nhãn “Người hướng dẫn thực tập” và “Hướng dẫn viên”; thống nhất “Học sinh” | Tránh nhập nhầm vai, đặc biệt khi gán người nói/portrait |
| Chương 1, cảnh 10–12 | Tách album thành cảnh 11, hồ sơ cuối ngày thành cảnh 12 | Khớp hai tương tác riêng trong căn hộ, rõ chuyển cảnh |
| Chương 6 | Ghi rõ công khai **số liệu tổng hợp**, không danh tính/hồ sơ cá nhân | Tránh làm mờ khác biệt giữa công khai có kiểm soát và tiết lộ vượt đồng thuận |
| Cả bộ | Thêm ID tuyến, liên kết đọc tiếp; đưa metadata vị trí/lựa chọn/cấu trúc vào blockquote | Không đọc hướng dẫn biên tập thành lời nhân vật; tránh nối nhầm A → B → C |

Luồng được xác minh: C1–C5 → I-A/I-B → C6–C7 → II-A/II-B → C8–C9 → A/B/C; chỉ C mới tiếp tục chương 10. Tổng cộng 12 tổ hợp, không vòng lặp, mỗi lượt chỉ đi một tuyến kết thúc. Xem `dialogue/README.md` và catalog máy đọc `docs/qa/dialogue-catalog.json`.

## Nội dung chạy trong Unity

Chương 1 lấy trực tiếp từ `../dialogue/Chuong_1_Ban_Duyet.md`: **148 cue nguồn = 88 lượt nói + 60 chỉ dẫn**, qua **12 cảnh**, chia thành **154 trang tối đa 360 ký tự**. Không cắt mất từ hoặc đảo thứ tự. Mỗi trang giữ ID, người nói, loại speech/direction, cảnh, tiêu đề cảnh và số dòng nguồn.

Giữ 9 nhiệm vụ và 5 thao tác hiện có. Điểm ngắt nằm trước khi chuyển điện, ghi kết luận hồ sơ, triển khai vận tải, gửi xác minh và gửi đối chiếu giờ làm. Không tự hoàn thành thao tác chỉ vì một đoạn mô tả kết quả đã xuất hiện trước nó.

Nhãn/portrait đổi theo lượt nói; vai phụ/hệ thống/lời dẫn không mượn portrait Mai hoặc Linh. Hồi ức Mai tám tuổi được trình bày riêng, không đặt Mai trưởng thành và Linh vào vai nhân vật trong phòng bệnh. Sổ chuyện dùng cùng bộ thoại đã chỉnh. Các đoạn NPC trích từ tuyến chính được tra theo cảnh/vai thay cho chỉ số văn xuôi cũ.

73 đoạn nguyên văn của STORY.md vẫn nằm trong dữ liệu như tham chiếu canon và thông tin chuyển save. Chúng không còn là nguồn thoại gameplay chính. Hash canon vẫn `68d2c28bd7ce570883168a25901128573648ca92996513995d6386b0a4cb66e8`.

## Lưu cũ và quy trình sửa tiếp

Save văn xuôi của bản trước được kiểm theo schema/hash/cursor/flags cũ rồi mới chuyển. Giữ nguyên nhiệm vụ đã xong, vị trí, các mục hồ sơ đã kiểm và trạng thái hoàn thành. Do số trang thay đổi, đoạn đang đọc được bắt đầu lại từ đầu **đoạn hiện tại**, không reset toàn chương. Save nguồn không nhận diện hoặc cursor hỏng bị từ chối; bản cũ còn trong backup khi ghi nâng cấp.

Sau khi sửa Markdown, chạy **tools/Build-Chapter1.ps1**. Script tự kiểm cả bộ và biên dịch chương 1. Menu build Unity cũng so hash file nguồn với dữ liệu đã biên dịch; nếu khác thì chặn build và hướng dẫn chạy script. Khi không mang thư mục tác giả theo, bản dữ liệu biên dịch vẫn chứa đủ thoại để build/phân phối game.

## Kiểm chứng

- **607 kiểm tra C# PASS:** duyệt toàn chương, cue/person attribution, từng trạng thái/checklist, JSON/save/backup, giữ tiến độ khi nâng cấp bản văn xuôi và từ chối save không hợp lệ.
- **7 test Python PASS:** 17 file/12 tuyến, metadata không thành thoại, heading cấp 3 của Ending C, hash nguồn, đủ từ/thứ tự, role/portrait và format lỗi.
- **Unity compile + Editor smoke + Windows x64 build PASS**, Unity 6000.4.0f1.
- **516 kiểm tra Windows runtime PASS, mã thoát 0**, không có exception/crash trong lượt cuối tại `artifacts/chapter1-dialogue/player-clean`. Bao gồm hết 154 trang, ảnh riêng Mai/Linh/mẹ Mai/tài xế/người hướng dẫn thực tập/hướng dẫn viên, lưu/tải và nâng cấp save cũ trong player. Đã xem ảnh kiểm nhãn người nói, portrait, đoạn dài và hai vai hướng dẫn khác nhau.

Trong quá trình kiểm đã gặp race ở bộ QA: ảnh PNG xuất hiện trước khi ghi xong gây sharing violation, và một lượt thoát ngay sau đổi kích thước bị crash native. Harness đã được sửa để chờ ảnh có IEND đầy đủ, bắt lỗi coroutine con, và chờ xử lý resize trước khi thoát. Không lấy các lượt đó làm kết quả “chạy sạch”; xem kết quả/mã thoát lượt cuối ở thư mục nêu trên.

Đây là QA runtime tự động và kiểm ảnh; không tuyên bố đã manual chơi bàn phím vật lý toàn chương. Các chương/nhánh ngoài chương 1 mới được kiểm bản thảo, chưa được triển khai vào gameplay hiện tại.
