# Contract nội dung TV1 - đề xuất 0.1

Chờ TV2/TV4 kiểm nhận. Đây là quy ước biên soạn để giao dữ liệu, chưa là schema/runtime chính thức của Unity. Không thay âm thầm `scene.schema.json` đã được nhóm duyệt trong tương lai; ghi version khi chuyển đổi.

## Cấu trúc

- Scene: `schema_version`, `content_version`, `status`, `scene_id`, `title`, `source_pages`, `background_id`, `entry_step`, `asset_ids`, `steps`.
- Step: `step_id`, `stage_direction`, `lines` (text IDs), `choices`. Mỗi step chỉ vào một lần trừ hub có ghi rõ.
- Choice: ID duy nhất toàn gói, label cho người chơi, `when`, `lines`, `effects`, `next`, `action_note`, `duration_minutes`.
- `next` là `step:ID`, `scene:S03` hoặc `end`. Step hub cho quan sát không tốn giờ và không tự tăng lần gửi.
- Thoại tách theo scene. Mỗi line có `text_id`, `speaker_id`, `text`, `mood`; narrator dùng `NARRATOR`. Các line là biến thể biên soạn giữ nghĩa v4.1, không mặc định nguyên văn nguồn.
- `when` dùng `all`/`any`/`not` hoặc lá `field, op, value`; op `eq`, `ne`, `in`, `contains`, `gte`, `lt`. Danh sách rỗng ở `all` nghĩa luôn đúng.
- Effect dùng `set`, `add_unique`, `advance_minutes`, `grant`; grant có asset/subject/scope/status/source_event_id. `asset_field` lấy asset đang được chọn từ state; `scope_field=final_scope` lấy đúng một phạm vi của bản đang chốt, không cấp cả in sách lẫn bổ sung. Những effect này mô tả kết quả dự kiến của lệnh game, không cho AI trả effect.
- Placeholder `{album_name}`, `{opening_gesture}` chỉ lấy state có allowlist. Ngày giao lấy print_slot theo timeline, không ghép câu sai lịch.

## Ranh giới thực thi

TV2 phải ánh xạ từng interaction sang command kiểm luật. Các điều kiện/effect trong JSON không thay quyền kiểm của runtime. Trước capture kiểm lại attendance/giờ/máy; trước publish kiểm grants; trước scene E_C kiểm later_status. Không thực thi state patch từ input người chơi.

Các choice mang trả lời NPC thể hiện một sự kiện thoại đã được tác giả viết theo tình huống trước; không phải menu để người chơi tự chọn trạng thái accepted. Bộ kiểm giả lập áp dụng effect tương ứng sau thoại cho mục đích kiểm logic nội dung.

Chế độ A/B/C chỉ ở S03, cùng `allowed-intents.json` và `responses.json`. Request không chứa toàn bộ kho facts. Response lạ, nhiều nghĩa, target không mở hoặc stale phải về câu hỏi rõ/nút; không tác động scene/consent/ending.

## Bàn giao và kiểm

Chạy `python tools/Content/validate_tv1_content.py` từ root CIVI_2. Script chuẩn thư viện, kiểm ID/đường dẫn/nội dung nguồn và mô phỏng các đường mẫu. Nó không render, gọi AI hay chứng minh thời lượng 12–20 phút. TV2 vẫn cần nạp thử một scene trước khi nhận toàn gói.
