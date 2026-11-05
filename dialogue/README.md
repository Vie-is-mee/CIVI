# Bộ hội thoại — Những người không có chỗ dưới bầu trời

Nguồn cốt truyện là `../CIVI/STORY.md`. Các file `*_Ban_Duyet.md` là bản chuyển thể thành lời thoại/chỉ dẫn sân khấu. Nhãn “Bản duyệt” được giữ theo tên file gốc; chỉnh sửa kỹ thuật ở phiên này không thay cho việc nhóm tác giả duyệt lại nội dung.

## Thứ tự đọc

1. Đọc [Chương 1](Chuong_1_Ban_Duyet.md) → [2](Chuong_2_Ban_Duyet.md) → [3](Chuong_3_Ban_Duyet.md) → [4](Chuong_4_Ban_Duyet.md) → [5](Chuong_5_Ban_Duyet.md).
2. Chọn **một** nhánh: [I-A](Nhanh_I-A_Ban_Duyet.md) hoặc [I-B](Nhanh_I-B_Ban_Duyet.md), rồi đọc [Chương 6](Chuong_6_Ban_Duyet.md) → [7](Chuong_7_Ban_Duyet.md).
3. Chọn **một** nhánh: [II-A](Nhanh_II-A_Ban_Duyet.md) hoặc [II-B](Nhanh_II-B_Ban_Duyet.md), rồi đọc [Chương 8](Chuong_8_Ban_Duyet.md) → [9](Chuong_9_Ban_Duyet.md).
4. Chọn **một** kết thúc: [A](Ending_A_Ban_Duyet.md), [B](Ending_B_Ban_Duyet.md), hoặc [C](Ending_C_Ban_Duyet.md). **Chỉ C** tiếp tục sang [Chương 10](Chuong_10_Ban_Duyet.md). Không đọc nối A → B → C như một dòng thời gian.

Tổng cộng có 12 tổ hợp lựa chọn. Dòng **Luồng đọc** và comment `civi-route` đầu mỗi file ghi các điểm nối, không phải lời một nhân vật. Các mục **Vị trí**, **Lựa chọn**, **Cấu trúc**, tiêu đề cảnh và dòng “Hết…” cũng không được đọc thành thoại.

## Những chỉnh sửa ở phiên kiểm tra

- Chương 1: sửa lịch họp thành buổi chiều; tài xế xác nhận đã đổi giờ nghỉ và giao hàng; điều chỉnh cách Mai đề nghị hiệu trưởng kiểm tra phòng thay thế; làm rõ người hướng dẫn thực tập khác hướng dẫn viên phòng lịch sử; thống nhất nhãn Học sinh; tách album thành cảnh 11 và hồ sơ cuối ngày thành cảnh 12.
- Chương 6: làm rõ công khai **số liệu tổng hợp**, không công bố danh tính/hồ sơ cá nhân. Giữ nguyên sự khác nhau giữa các nhánh về thời gian, bằng chứng, quyền riêng tư và lòng tin.
- Cả 17 file: thêm ID tuyến/liên kết đọc tiếp; kiểm thứ tự cảnh, nhãn người nói, format và điểm nối kết thúc. Nội dung tương lai chưa được đưa vào gameplay chương 1.

## Dùng trong Unity

Chương 1 đã được tích hợp từ `Chuong_1_Ban_Duyet.md`: 148 cue gốc gồm 88 lượt nói và 60 chỉ dẫn, chia thành 154 trang ngắn khi hiển thị. Giữ 9 nhiệm vụ hiện có, 5 thao tác và các điểm ngắt trước/sau thao tác. Không nhập toàn bộ văn xuôi vào một nhãn người nói.

Từ thư mục `../CIVI`, chạy `powershell -File tools/Build-Chapter1.ps1`. Script kiểm các tuyến và biên dịch file Markdown trước khi build. Chỉ mở game bằng `../CIVI/CHOI_GAME.bat`.

Bản gốc trước chỉnh ở `../CIVI/artifacts/dialogue-review/original`; diff kiểm tra ở `../CIVI/artifacts/dialogue-review/review.patch`. Báo cáo/catalog có trong `../CIVI/docs/qa/`. Không sửa `STORY.md` để khớp nhầm một câu thoại chuyển thể.
