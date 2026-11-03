# Thành viên 3 — hình ảnh, UI và âm thanh

Nguồn phân công: mục TV3 / W3.1–W3.8 trong `CAU_TRUC_VA_TINH_NANG.md` (hàng 23–30 của bảng nguồn). 47 giờ là ước lượng kế hoạch, không phải thời gian đã làm. Cập nhật 23/10/2026; người nhận chưa xác nhận nghiệm thu.

| Mã | Đầu ra của TV3 | Tình trạng thực tế | Còn để nhận hoàn thành |
| --- | --- | --- | --- |
| W3.1 | Phong cách, danh mục, quy cách, nguồn/quyền | Có map đủ 32 mã, kích thước/hash/GUID và nguồn; 29 media | TV1/TV2 chốt quy cách và nhận danh mục theo mẫu nhóm |
| W3.2 | Ảnh đủ bốn, hai ảnh hỏng, sơ đồ, HẸN và gesture | Có ba cặp gesture, prefab sơ đồ/bảng; S00 đã nạp thử | TV1 duyệt hình/nhận diện, không suy từ số file là đã nghiệm thu |
| W3.3 | 0441/0442/0443, kẹp trống, animatic | 0442 đã sửa; 0441 đã rà; animatic có phụ đề/SFX/dừng/kéo lại | TV1 duyệt continuity; TV2 nối S03/S04 vào tuyến chính |
| W3.4 | UI thoại, ảnh, ba mẫu nháp, xem trước/người nhận | Có UI S00, ba prefab mẫu và DraftReview; đã thử 9 tổ hợp ở 720p/1080p | TV1/TV2 duyệt UI và nối command S05, loading/lỗi của dịch vụ thật |
| W3.5 | Sân, đạo cụ, hình kết hai đường | Có ảnh bốn/ba/hẹn sau và một số đạo cụ | Nền hành lang/ghế đá trống, lịch/chat/bảng cảm ơn và các trạng thái cảnh sau còn thiếu |
| W3.6 | Âm/phụ đề/phông/ghép tài nguyên | Có âm hẹn giờ/SFX, phông và giấy phép; nạp ở bản TV3 | Nạp toàn bộ tài nguyên trong bản tuyến chính và người nhận kiểm |
| W3.7 | Sửa theo người chơi thật | Đã sửa theo kiểm kỹ thuật về chữ, shader và che mặt | Chờ playtest thật của TV5; chưa gọi là đã sửa theo playtest |
| W3.8 | Video demo và hình hồ sơ | Có ảnh kiểm build, chưa có demo nộp | Quay bản khóa, thể hiện hai hệ quả; TV1/TV5 duyệt nội dung/định dạng |

## Rà ảnh 0441–0442

0441: đủ An/Nhi/Hạ/Minh; Nhi ở mép phải cầm dây, mặc quần dài; bóng đèn chưa sáng. 0442: đủ bốn; đèn sáng, ổ chia trên ghế đỏ, Nhi nhìn An sau chỉnh sửa. 0443 vẫn là ảnh gốc ba người, không thêm/cắt Nhi.

Giữ nguyên 0441 sau lần rà này. Vị trí điện thoại và phần việc của các bạn thay đổi giữa hai ảnh; không dùng chúng để khẳng định mọi đồ vật đứng yên hoặc Nhi làm toàn bộ công việc. Nhóm cần duyệt ý nghĩa/chuyển tiếp trong S03, không chỉ duyệt ảnh đẹp.

## Cách mở phần vừa làm

Mở `01-CHOI-THU-S00.bat` ở gốc, đọc hết S00 rồi chọn **Thử ba mẫu trang nháp**. Đây là bàn thử UI W3.4: chọn mẫu → chọn người xem → xem lại → xác nhận. Xác nhận chỉ ghi lựa chọn thử, không gửi nội dung ra ngoài và không làm Nhi tự đồng ý trong cốt truyện.

Trong cây: ba mẫu nằm ở `Game/Assets/_Civi/Prefabs/UI/TEMPLATE_*.prefab`; màn xem trước là `DraftReview.prefab`; mã trình bày ở `Scripts/Presentation/Drafting`. TV2 nhận sự kiện `DraftPreviewRequest`, kiểm domain command và version/quyền trước khi áp dụng vào S05.

Tiến độ và bằng chứng mới nhất: [trang đầu](../../00-TIEN-DO-TV3.md). Việc tiếp theo của TV3: sửa theo nhận xét, hoàn thiện phần đạo cụ W3.5 và UI các cảnh sau; phối hợp TV2 để thử S05 trong tuyến chính.
