# Kiểm dữ liệu TV1 0.1

Ngày chạy: 22/10/2026. Kết quả: PASS.

Đã kiểm 15 scene/nhánh kết, 126 lựa chọn và 326 mục thoại. Mọi ID thoại/asset/state/transition tham chiếu được tìm thấy; không có mục thoại mồ côi.

Mô phỏng 22 đường và 5 ca chặn hành động sai. Đây là kiểm nội dung bằng Python, chưa chạy Unity, không gọi AI và không có người chơi thật.

| Ca | Kết | Kết quả ảnh | Trạng thái |
| --- | --- | --- | --- |
| four | E_A | four | PASS |
| early_understanding | E_A | four | PASS |
| ha_handover | E_A | four | PASS |
| declined_three | E_A | three | PASS |
| pending_three | E_A | three | PASS |
| no_new | E_B | none | PASS |
| three_private | E_B | three | PASS |
| four_not_printed | E_B | four | PASS |
| leave_early | E_A | three | PASS |
| forced_pose_changed | E_A | four | PASS |
| forced_pose_stopped | E_A | three | PASS |
| tribute_retracted | E_A | four | PASS |
| tribute_board_kept | E_A | three | PASS |
| pressure_absence | E_A | three | PASS |
| later_old | E_C | none | PASS |
| later_new | E_C | later | PASS |
| later_new_private | E_C | later | PASS |
| later_pending | E_B | none | PASS |
| old_bad_01 | E_A | four | PASS |
| old_bad_02 | E_A | four | PASS |
| old_diagram | E_A | four | PASS |
| extended | E_A | four | PASS |

Năm ca âm kiểm hai trạng thái absent/left không cho chụp đủ bốn và ba trường hợp thiếu quyền của Nhi/Hạ/Minh cho ảnh cũ không cho công bố.

Đã kiểm riêng phản hồi Hạ trước/sau biết nguyên câu clip; không đưa nguyên văn video vào phản hồi trước nguồn. Mọi quyền được xét đúng phạm vi trang đang chốt.

Chưa chứng minh: đúng UI/media thực, ảnh xuất PNG, save/load Unity, độ dài 12–20 phút, chất lượng hội thoại AI, p95/cost hoặc hiệu quả giáo dục. W1.7/W1.8 chờ bằng chứng thực.

[JSON kết quả chạy](../../artifacts/qa/tv1-content-checks.json) · [Script kiểm](../../tools/Content/validate_tv1_content.py)
