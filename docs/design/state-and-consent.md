# Trạng thái và quyền nội dung - gói TV1 0.1

W1.2; người kiểm nhận TV2/TV4. Mẫu state cụ thể nằm ở `Content/Story/v4_1/initial-state.json`. Các trường là contract đề xuất, không phải code Unity đã có.

## Trạng thái phải tách

| Trường | Giá trị / nghĩa | Nguồn thay đổi |
| --- | --- | --- |
| opening_gesture | pose_1/pose_2/pose_3 | Lựa chọn S00; callback bằng hình/thoại. |
| an_owned_camera_choice | unresolved/yes/deflected | Lời An và việc thương lượng S06; không cộng điểm. |
| draft_type | tribute/paired/memory | Chọn S05; chọn sớm paired/memory không bị ép tribute. |
| draft_sent_to | held/nhi/group_four | Gửi có preview; nhóm lớp không có trong bản MVP. |
| draft_seen_by | Danh sách người đã xem phiên bản gửi | Sự kiện nhận/xem trong cảnh, không phải chỉ chọn recipient. |
| draft_retracted | not_needed/no/yes | Dừng phiên bản; không xóa draft_seen_by. |
| retraction_notified_to | Những người đã được báo dừng | Tin thực sự gửi và/hoặc cuộc nói trực tiếp. |
| tribute_board_prepared/removed | Hai cờ độc lập | Hạ làm bảng sau khi nhận bản; An/Hạ tháo thật. |
| ha_support | collaborate/handover_only | Thỏa thuận cụ thể về ảnh và ai làm phần nào; không từ một câu xin lỗi. |
| nhi_invite_status | unasked/pending/accepted/declined | Câu phản hồi của NPC trong scene, không từ AI nhận intent. |
| attendance | absent/present/left | Sự kiện đến/rời; accepted có thể trở thành left trước capture. |
| nhi_photo_consent | unasked/yes/no | Chỉ trạng thái ảnh mới đang xét; grants theo asset/scope mới là nguồn chính. |
| equipment_ready/support_plan | repairable/ready; assumed/assigned | Kiểm kẹp/pin và giao việc có xác nhận. |
| photo_result | none/four/three/later | Chụp thật và danh sách người ở trong khung. |
| print_slot | normal/extended/renegotiate | Câu trả lời xác nhận từ xưởng, sau kiểm giờ. |
| final_mode | new/old/supplement | Nguồn được chọn cho trang; không suy trực tiếp từ photo_result. |
| later_status | unasked/pending/accepted/declined | Lịch và mục đích trả lời rõ; đề nghị chưa là đồng ý. |

## Quyền theo tư liệu, người và phạm vi

Mỗi grant có `asset_id`, `subject_id`, `scope`, `status`, `source_event_id`. Trạng thái yes/no/unasked. Scope gồm `internal_review`, `private_album`, `yearbook_print`, `supplement_delivery`. Người giữ nguồn video có quyền cho mượn kiểm chứng; việc đó không thay đồng ý xuất bản của các người xuất hiện. Scope không bao hàm nhau.

- Ảnh cũ S00: S07 hỏi Nhi rõ tấm ảnh cụ thể; S10 An/Hạ/Minh cùng kiểm bản và xác nhận. S00_bad_01/02 cần hỏi riêng nếu được chọn thay ảnh tốt; không suy từ quyền PHOTO_S00_GOOD.
- 0443: chỉ An/Hạ/Minh xuất hiện; hỏi đúng ba người và quyền chủ nguồn. Nhi không là người có mặt trong ảnh nhưng caption không được gán lời hay mong muốn cho cô.
- Sơ đồ: do Nhi tạo, phải hỏi quyền riêng nếu đưa vào trang.
- Clip Vy: chỉ mở cho xem nội bộ trong gói MVP; không có lựa chọn xuất clip.
- Ảnh bốn mới: có mặt không tự thành đồng ý dùng; có nhánh Nhi không cho in, người chơi vẫn kết bằng ảnh cũ.
- Ảnh ba mới: An/Hạ/Minh xác nhận dùng; không giả xin quyền Nhi như một người có mặt trong ảnh đó.
- Ảnh bổ sung: quyền `supplement_delivery` được hỏi ở cuộc gặp; không kế thừa quyền in ảnh S00 hay ảnh thứ Sáu.

## Bất biến khi chốt

1. Ảnh mới bốn người chỉ sinh khi Nhi còn ở sân, đồng ý đứng trong khung, máy đã đặt và An vào ảnh.
2. pending/declined/left không thể tạo ảnh có Nhi. Tới giờ đi phải chuyển S09_B.
3. Bản tôn vinh đã gửi rồi dừng phải báo người đã xem. Bảng đã gỡ không tự sinh lại.
4. final_mode=new cần ảnh mới đã có và đủ grant yearbook_print; final_mode=old có thể giữ ảnh mới riêng đúng scope.
5. final_mode=supplement cần lịch accepted và kế hoạch giao rõ. finished của E_C chỉ có sau giao tờ, không sau gửi lời hẹn.
6. Runtime kiểm lại toàn bộ source/subject/scope cho mỗi asset được chọn; UI không chỉ dựa vào nhi_photo_consent.
7. Hạ chỉ bàn giao không làm mất quyền đọc chứng cứ đã cho; template cơ bản vẫn có.

Nếu người giữ nguồn từ chối dùng ảnh, bỏ thành phần đó hoặc hỏi một nguồn khác. Trong bản dữ liệu mẫu, mỗi nhánh có ít nhất nguồn S00 được xác nhận rõ. Các tình huống từ chối mọi tư liệu là biến thể ngoài gói, cần TV1/TV2 thiết kế tiếp; không tự đồng ý thay nhân vật.
