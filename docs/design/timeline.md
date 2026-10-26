# Lịch truyện và thời gian thao tác

Nguồn: kịch bản v4.1 tr.9–15. Lịch sản xuất tháng 10–11/2026 không được đưa vào thời gian truyện.

| Mốc | Sự kiện |
| --- | --- |
| Tháng Mười năm trước | Tập gian hàng, rồi một tuần sau hội chợ; không tự gán ngày cụ thể. |
| Hội chợ 16:15 / 16:31 | Giờ hẹn / giờ Nhi đến; đây không phải giờ chụp lại. |
| 07/05/2026 10:00 | Thấy trang 16 sang 18; tin An giải thích việc rút trang. |
| 07/05 trong ngày | Điều tra, nháp, phản hồi, lời mời. Chuyển thời đoạn bằng cắt cảnh có nhãn. |
| 08/05 trước 16:00 | Chuẩn bị và chọn phương án dự phòng. |
| 08/05 16:40 | Đến sân; kiểm máy. |
| 08/05 16:45 | Buổi chụp; có mặt không đồng nghĩa nhận mọi tư thế. |
| 08/05 trước 17:00 | Có thể giữ lịch gia hạn nếu nhận xác nhận xưởng; không tự ghi đã gia hạn từ một lời hứa gọi. |
| 08/05 17:10 | Nhi phải đi; chuyển đường vắng nếu chưa chụp. |
| 08/05 17:30 | Tháo quầy. Không tiếp tục dựng lại nguyên quầy sau mốc này. |
| 08/05 tối | Bàn giao, kiểm trang, cho Minh đi ôn thi. |
| 09/05 10:00 hoặc 12:00 | Hạn thường hoặc đã được gia hạn hợp lệ. |
| 11/05 chiều hoặc 12/05 sáng | Giao sách thường hoặc gia hạn; lịch bổ sung riêng có xác nhận riêng. |

## Quy tắc vận hành đề xuất

Đọc, xem ảnh, nghe lại clip và preview không tiến giờ. Thao tác dài hiển thị số phút trước chọn; các số này là giả định nhịp dựng. State `courtyard_minute` đếm phút từ 16:00 ngày 08/05; 45 là 16:45, 70 là 17:10, 90 là 17:30. Chỉ sử dụng chỉ số này trong S08/S09; không dùng nó làm đồng hồ toàn bộ truyện.

Khi một thao tác chạm 70, Nhi thông báo đi trong điểm ngắt rồi đổi `attendance=left` trước lựa chọn chụp. Runtime phải kiểm lại ngay trước capture, kể cả save/load. Nếu chạm 90 chỉ cho thu đồ/kết bằng nguồn cũ. Cắt cảnh tối không làm nhân vật tiếp tục đứng ở sân.

Biến thể bổ sung trong gói dùng đề nghị gặp ở ghế đá ngày 11/05 lúc 16:30. Đây là **một lịch đề xuất trong thoại mới**, chỉ thành lịch xác nhận khi Nhi trả lời nhận. Nếu cần đổi ngày, sửa cùng request/response, nhãn ảnh và delivery plan; không sửa riêng một câu.
