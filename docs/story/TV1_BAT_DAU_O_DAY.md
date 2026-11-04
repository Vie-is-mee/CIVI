# Bộ bàn giao TV1 - bản biên soạn 0.1

Ngày: 22/10/2026. Nội dung chuẩn: PDF kịch bản v4.1. Trạng thái: đã soạn để kiểm nhận, chưa tích hợp Unity, chưa playtest và chưa khóa phát hành.

## Đọc và làm theo thứ tự

1. [Sự kiện chuẩn và phần thay thế v3](canon-v4.1.md).
2. [Phạm vi MVP](../design/mvp-scope.md), [trạng thái và quyền](../design/state-and-consent.md), [kiến thức NPC](npc-knowledge.md).
3. [Kịch bản đọc thuần](kich-ban-doc-thuan.md) để đọc/góp ý; [bản chi tiết](kich-ban-mvp.md) có ID, điều kiện và hệ quả từng cảnh để bàn giao kỹ thuật.
4. [Bản đồ cảnh](scene-map.md), [quy ước dữ liệu bàn giao](../engineering/tv1-content-contract.md).
5. [Theo dõi công việc TV1](../production/tv1-task-status.md), [kiểm nhận nội dung](dialogue-review.md).

Các gói JSON đặt tại `Game/Assets/_Civi/Content`. Đây là **dữ liệu biên soạn theo contract đề xuất**; TV2 cần duyệt contract và làm loader/runtime. Chạy kiểm tra tham chiếu và mô phỏng nhánh bằng `tools/Content/validate_tv1_content.py`; kết quả không thay cho kiểm Unity/UI/quyền xuất ảnh thật.

## Tôi nên sửa ở đâu?

- Sự kiện và giới hạn: sửa tài liệu canon trước, giữ v4.1 làm căn cứ.
- Thoại cụ thể: nếu thuận tiện, góp ý trực tiếp theo scene/choice trong bản đọc thuần. Trước khi bàn giao, đồng bộ thay đổi vào JSON `Content/Story/v4_1/dialogue/`, giữ `text_id` ổn định và cập nhật bản đọc. JSON là nguồn giao cho runtime; không chạy một bản Markdown và JSON khác nghĩa.
- Lựa chọn/điều kiện/hệ quả: sửa `Content/Story/v4_1/scenes/`, trao đổi TV2 khi đổi trường hoặc loại thao tác.
- Phản hồi Hạ khi nhập tự do: `Content/Intents/S03/responses.json`, `clarifications.json`; TV4 duyệt mapping intent, TV2 duyệt kiểm quyền.
- Hình/âm/video: viết yêu cầu trong [brief tài nguyên TV1](../design/tv1-asset-brief.md), TV3 sản xuất và gắn asset thực.
- Chơi thử: điền quan sát thật vào biểu mẫu review. Không đánh dấu “đã tích hợp” chỉ vì đã có file.

## Bàn giao cho từng người

| Người nhận | Nhận gì | Cần xác nhận |
| --- | --- | --- |
| TV2 | Cảnh, thoại, state mặc định, quyền, sơ đồ nhánh, fixtures | Nạp cùng ID; luật từ dữ liệu được thực hiện đúng; không sửa state bằng lời nhập. |
| TV3 | Asset ID, số người/đạo cụ, cảnh đầu và nơi dùng lại | 0443 chỉ ba người; hai ảnh hỏng đủ bốn; sơ đồ tia chớp và trạng thái bảng đúng nhánh. |
| TV4 | Facts/knowledge, allowlist S03, phản hồi và câu hỏi lại | A/B/C cùng hành động; NPC không lộ nguồn; AI không sinh thoại/sự thật/đồng ý. |
| TV5 | Các ca kiểm, mục tiêu playtest, bản giới thiệu có điều kiện | Có kết quả quan sát thật và xác nhận nhận sản phẩm trước cập nhật tiến độ. |

## Căn cứ

- [Kịch bản v4.1](../../../../CIVI_kich_ban_v4_1.pdf), trang 3–20.
- [AI 2.1](../../../../CIVI_AI_2_1_FIXED.pdf), trang 9–14; ví dụ v3 được thay bằng S03 v4.1.
- [Bảng phân công](../../../../CIVI_Phan_cong_5_thanh_vien.xlsx), `02_Cong_viec!A7:J14`, `03_Ban_giao!A15:E29`.

Thoại nối và chi tiết thao tác mới trong bộ này là đề xuất biên soạn để dựng MVP; chưa mang ý nghĩa các thành viên đã duyệt. Không đổi động cơ nền, tạo thủ phạm, cộng điểm đạo đức hoặc kết luận tác động giáo dục.
