# TV3 — Bàn giao theo cây CIVI_2 · 23/10/2026

Hiện có **29 media + 29 .meta** (20 PNG, 6 WAV, 3 TTF), **sáu prefab UI**, **bốn prefab đạo cụ**, **AssetCatalog**, scene và bản Windows S00. Đã thêm ba cặp gesture, sửa 0442 để Nhi nhìn An, rà 0441 và thêm ba mẫu nháp/DraftReview. Ảnh mới dùng built-in image_gen; prompt đầy đủ ở `source-assets/TV3/imagegen-production.json`.

**Đã tích hợp và kiểm kỹ thuật S00**, chưa tích hợp toàn truyện. Model đạt 27 tổ hợp/135 roundtrip dấu trang; build không lỗi/cảnh báo; UI 720p và 1080p đạt. Nhóm chưa xác nhận duyệt hình/thoại; chưa có người chơi thật ghi phiếu.

## Mở và nhận

1. Chơi: mở `01-CHOI-THU-S00.bat` tại gốc hoặc `release/S00-TV3-Windows/CIVI-S00.exe`. Giữ cả thư mục khi gửi bạn.
2. Đọc tiến độ: `00-TIEN-DO-TV3.md`; người nhận mã xem `docs/engineering/tv3-s00-integration.md`.
3. Nhận xét: `docs/qa/TV3-NHAN-XET-S00.md`. Không đánh dấu nhóm đã duyệt từ kết quả kiểm tự động.
4. Tài nguyên theo mã: `tv3-asset-map.json` cạnh tài liệu này. Các trường `variant_files` và `artifact_file` bổ sung phần ảnh/Prefab; không thay contract hay quyền TV1.
5. Giữ .meta/GUID, không nhập hai bản của cùng GUID vào một Unity project. Manifest TV1 vẫn là brief; runtime S00 lấy tham chiếu thực từ AssetCatalog.

## Việc tiếp theo

Nhóm duyệt S00, ba cặp gesture và UI ba mẫu; TV2 nhận bộ đọc S00, nối S01/S03 và DraftReview vào S05 theo domain command đã chốt. TV3 sửa theo phiếu và bổ sung các đạo cụ/nháp/nền chưa có. Không gói demo toàn truyện khi các cảnh sau vẫn chưa tích hợp.

## Đối chiếu brief

Có 12/32 mã có file ứng viên; đây không phải tỷ lệ hoàn thành. Nhiều mã cần prefab, hội thoại hoặc trang dựng động; mọi ứng viên vẫn chờ nhóm duyệt; chỉ các thành phần S00 đã được tích hợp ở mốc hiện tại.

| Mã | Tình trạng | Điều cần biết / việc tiếp |
| --- | --- | --- |
| `BG_COURTYARD` | Đã có ứng viên · chờ duyệt và tích hợp | Nền đã có quầy và điện thoại gắn kẹp. Cần biến thể/layer cho lúc chưa giao máy; không mặc định dùng mọi trạng thái S00. |
| `BG_DESIGN_ROOM` | Đã có ứng viên · chờ duyệt và tích hợp | Nền phòng trống; ghép đạo cụ theo trạng thái, kiểm lại bố trí phòng kỷ yếu. |
| `BG_EQUIPMENT_CORNER` | Chưa ghép nền và đạo cụ | Có thể dùng BG_DESIGN_ROOM + đạo cụ kiểm đồ; chưa có prefab/layer riêng. |
| `BG_CORRIDOR` | Chưa có file | Chưa có nền hành lang/cửa lớp phù hợp. Không đổi tên nền phòng thành hành lang. |
| `BG_STONE_BENCH` | Chưa có file | Chưa có nền ghế đá trống. PHOTO_LATER_TWO có sẵn hai người nên không dùng làm nền để ghép thêm nhân vật. |
| `PHOTO_S00_GOOD` | Đã có ứng viên · chờ duyệt và tích hợp | Đã có ba biến thể pose_1/pose_2/pose_3 và cặp callback cùng cử chỉ; chờ nhóm duyệt hình. AssetCatalog chọn theo opening_gesture. |
| `PHOTO_S00_BAD_01` | Đã có ứng viên · chờ duyệt và tích hợp | Đã có ảnh đủ bốn, Nhi chạy vào, đèn tắt ban ngày; đã gắn và kiểm trong S00. Chờ nhóm duyệt hình. |
| `PHOTO_S00_BAD_02` | Đã có ứng viên · chờ duyệt và tích hợp | Minh cúi nhặt bìa, Hạ cười, đủ bốn. Tên A của bản cũ tương ứng BAD_02 của brief mới. |
| `PHOTO_0441` | Đã có ứng viên · chờ duyệt và tích hợp | Đã rà 23/10: đủ bốn, Nhi ở mép cầm dây, đèn chưa sáng. Giữ ảnh; đồ vật có thay vị trí giữa 0441/0442, chờ TV1 duyệt continuity. Không suy toàn bộ phần việc từ ảnh. |
| `PHOTO_0442` | Đã có ứng viên · chờ duyệt và tích hợp | Đã sửa Nhi ở mép phải nhìn sang An bên trái, giữ đèn sáng và ổ chia trên ghế đỏ; đã xem kiểm ảnh, chờ nhóm duyệt. |
| `PHOTO_0443` | Đã có ứng viên · chờ duyệt và tích hợp | Ảnh gốc ba người; tuyệt đối không thêm/cắt Nhi để thay đổi bằng chứng. |
| `PHOTO_CLAMP_EMPTY` | Đã có ứng viên · chờ duyệt và tích hợp | Cận kẹp trống; không dùng một mình để kết luận ai cầm điện thoại. |
| `VIDEO_VY` | Đã có animatic kéo lại được · chờ nhóm duyệt | Đã có animatic 32 giây: trước / chín giây chính 09–18 / sau. Có phụ đề TV1, SFX, dừng và kéo lại; đã kiểm UI hai độ phân giải. Chưa có giọng thu/diễn xuất toàn thân, chờ nhóm duyệt. |
| `TESTIMONY_NHI` | Nội dung TV1 · chờ nối thoại | Lấy lời xác nhận từ dialogue do TV1 cung cấp; không cần ảnh giả, không cấp quyền phát hành clip từ lời chứng. |
| `PROP_DIAGRAM` | Đã có prefab riêng · chờ nhóm duyệt | Đã có prefab riêng, đã gắn vào bản tích hợp S00. Chờ nhóm nhận hình và TV2 tích hợp các trạng thái ở cảnh sau. |
| `PROP_SIGN_HEN` | Đã có prefab riêng · chờ nhóm duyệt | Đã có prefab riêng, đã gắn vào bản tích hợp S00. Chờ nhóm nhận hình và TV2 tích hợp các trạng thái ở cảnh sau. |
| `PROP_CLAMP` | Đã có prefab riêng · chờ nhóm duyệt | Đã có prefab riêng, đã gắn vào bản tích hợp S00. Chờ nhóm nhận hình và TV2 tích hợp các trạng thái ở cảnh sau. |
| `PROP_BATTERY` | Đã có prefab riêng · chờ nhóm duyệt | Đã có prefab riêng, đã gắn vào bản tích hợp S00. Chờ nhóm nhận hình và TV2 tích hợp các trạng thái ở cảnh sau. |
| `PROP_PROOF_16_18` | Chưa có prefab/layer riêng | Dựng UI bản thử trang 16 rồi 18, không giả định trang 17 bị xóa vĩnh viễn. |
| `PROP_CHAT_REMOVE` | Chưa có prefab/layer riêng | Dựng chat từ nội dung TV1, giữ tính mơ hồ; không tự thêm kết luận có tội. |
| `PROP_THREE_LAYOUT` | Chưa có prefab/layer riêng | Bố cục dự kiến ba vị trí phải khác ảnh đã chụp thật PHOTO_NEW_THREE. |
| `PROP_SCHEDULE` | Chưa có prefab/layer riêng | Hiển thị 16:45/17:10/17:30 và hạn sáng thứ Bảy, không dùng ETA khó hiểu. |
| `PROP_CAPTION_OLD` | Chưa có prefab/layer riêng | Chỉ xem đối chiếu; giữ cờ không được xuất công khai của TV1. |
| `PROP_TRIBUTE_BOARD` | Chưa có prefab/layer riêng | Tham khảo RenderDeskProps; chỉ xuất hiện khi đã chuẩn bị và chưa gỡ, không bật cố định trong nền. |
| `TEMPLATE_TRIBUTE` | Đã có prefab riêng · chờ nhóm duyệt | Đã có prefab mẫu và DraftReview: chọn người xem, phạm vi xem trước, xác nhận hai bước; đã thử 9 tổ hợp ở 720p/1080p. Chờ TV1 duyệt và TV2 nối command S05; chưa gửi thật trong truyện. |
| `TEMPLATE_PAIRED` | Đã có prefab riêng · chờ nhóm duyệt | Đã có prefab mẫu và DraftReview: chọn người xem, phạm vi xem trước, xác nhận hai bước; đã thử 9 tổ hợp ở 720p/1080p. Chờ TV1 duyệt và TV2 nối command S05; chưa gửi thật trong truyện. |
| `TEMPLATE_MEMORY` | Đã có prefab riêng · chờ nhóm duyệt | Đã có prefab mẫu và DraftReview: chọn người xem, phạm vi xem trước, xác nhận hai bước; đã thử 9 tổ hợp ở 720p/1080p. Chờ TV1 duyệt và TV2 nối command S05; chưa gửi thật trong truyện. |
| `PHOTO_NEW_FOUR` | Đã có ứng viên · chờ duyệt và tích hợp | Đã có ba biến thể pose_1/pose_2/pose_3 và cặp callback cùng cử chỉ; chờ nhóm duyệt hình. AssetCatalog chọn theo opening_gesture. |
| `PHOTO_NEW_THREE` | Đã có ứng viên · chờ duyệt và tích hợp | Ba người An/Hạ/Minh; không ghép Nhi khi Nhi vắng. |
| `PHOTO_LATER_TWO` | Đã có ứng viên · chờ duyệt và tích hợp | Chỉ An/Nhi ở ghế đá, không quầy HẸN; chỉ dùng sau cuộc gặp đã xác nhận. |
| `PAGE17_FINAL` | Chưa có trang dựng động | TV3 làm bố cục; TV2 gắn nguồn, ngày, người và quyền thực tế. Không thay bằng một ảnh kết cố định. |
| `PAGE17_SUPPLEMENT` | Chưa có trang dựng động | Cần bố cục tờ bổ sung và xác nhận in/giao; chưa giao thì không hiển thị kết thúc đã giao. |

## Kiểm tra đã thực hiện

Hash media/.meta/GUID, đủ 32 mã brief, tham chiếu 15 cảnh/326 dòng/126 lựa chọn; kiểm model, build, UI S00 và 9 tổ hợp mẫu/người nhận ở mỗi độ phân giải. S05 chưa tích hợp tuyến truyện. Chi tiết ở `artifacts/qa/s00-integration-checks.json` và `tv3-handoff-checks.json`. Dữ liệu/contract TV1 giữ nguyên; README cập nhật theo tiến độ thực.

Minh họa dùng công cụ tạo ảnh cho bản thử, không ghi là tranh họa sĩ vẽ tay. Quyền trong truyện không thay giấy phép tài nguyên sản xuất. Xem `licenses/TV3-ASSETS.md`, `licenses/TV3-Fonts`; bản Windows kèm giấy phép phông.
