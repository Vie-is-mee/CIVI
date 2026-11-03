# TV3 / S00 — ghi chú để TV2 nhận mã

Phạm vi là bản tích hợp một cảnh S00 và bộ trình bày TV3. Chưa thay runtime toàn truyện, chưa AI, chưa tự nối sang S01, chưa nhận nghiệm thu thay thành viên khác.

## Điểm vào và cách dựng lại

- Mở project `Game` bằng Unity 6000.6.5f1. Chọn `CIVI > TV3 > Chuẩn bị bản tích hợp S00`; scene nằm ở `Assets/_Civi/Scenes/S00-TV3-Preview.unity`.
- Chọn `CIVI > TV3 > Build Windows S00` để tạo `release/S00-TV3-Windows`. Giữ cả thư mục cho người chơi; không chỉ gửi `.exe`.
- Bản hiện có cũng mở bằng `01-CHOI-THU-S00.bat` tại gốc. Không cài thêm môi trường và không chạm file lưu của bản cũ.

## Tách phần để nhận

| Thành phần | Chỗ đặt | Trách nhiệm |
| --- | --- | --- |
| `S00Session` | `Scripts/Domain/S00` | Đọc scene/thoại TV1, tiến câu, chọn lệnh S00, giữ gesture/album/hiện vật và dấu trang |
| `CiviAssetCatalog` | `Scripts/Infrastructure/Assets` + `Config/TV3-AssetCatalog.asset` | Tham chiếu texture, phông, âm thanh, dữ liệu và prefab; không đoán đường dẫn Editor lúc chạy |
| `NotebookDialogue`, `NotebookChoices` | `Scripts/Presentation` + `Prefabs/UI` | Hiện chữ, portrait giữ suốt câu, nút tiếp tục/lựa chọn và nhãn |
| `NotebookActor` | `Scripts/Presentation/Characters` | Nhấn nhẹ người nói, chuyển động nhỏ; có giảm chuyển động |
| `RehearsalProps` | `Scripts/Presentation/Props` + `Art/Props` | Sơ đồ, kẹp/cao su, pin, bảng HẸN là đối tượng riêng; không phải luật nhận quyền |
| `VyAnimatic` | `Scripts/Presentation/Animatic` | Đoạn dựng 32 giây gồm trước / chín giây chính 09–18 / sau, phụ đề TV1 và SFX, kéo lại/dừng |
| `S00PreviewPlayer` | `Scripts/Application/Preview` | Ghép bộ phận cho bản một cảnh, capture và album, điểm nối S01; không dùng làm bộ điều khiển toàn truyện |

## Hợp đồng và giới hạn

- Không sửa JSON tác giả. Runtime S00 chỉ nhận schema `tv1-authoring-0.1`, allowlist 11 choice ID; lệnh ngoài phạm vi phải lỗi rõ để TV2 duyệt trước. Không chạy tùy ý các state patch từ input hoặc AI.
- `S00Session` hiện áp dụng các command S00 cụ thể, không phải bộ thực thi điều kiện/effect cho 15 cảnh. TV2 cần thay bằng domain command chung khi tích hợp tiếp và đối chiếu lại effect TV1.
- Dữ liệu tác giả được tham chiếu dưới dạng TextAsset qua catalog. Trước build, Editor tạo bản sao một chiều ở `StreamingAssets/CiviGenerated`; không sửa bản sinh này. Bản thử chưa triển khai loader StreamingAssets cho toàn truyện.
- `opening_gesture=pose_1/2/3` chọn texture `PHOTO_S00_POSE_1/2/3`; ứng viên callback là `PHOTO_NEW_FOUR_POSE_1/2/3`. Ở cuối bản thử có màn đối chiếu để duyệt; **chưa coi là đã tích hợp S09_A**.
- 0442 đã sửa hướng nhìn Nhi; 0443 vẫn ảnh gốc ba người. Ảnh cũ 0441 vẫn chờ duyệt tính liên tục, nền hành lang/ghế đá trống vẫn còn thiếu.
- Animatic là đồ họa chuyển động đơn giản và phụ đề, chưa có diễn xuất nhân vật/giọng thu hoàn chỉnh. Dùng thoại đã biên soạn S04; xem nội bộ không tự cấp quyền phát hành clip.
- Dấu trang dùng `civi2-s00-preview-bookmark.json` trong persistentDataPath riêng của sản phẩm này, schema version 1. Lưu sau thao tác chụp; không ghi trạng thái chụp dang dở. Không nạp file lưu game cũ vào bản thử.

## Phần UI nháp W3.4 mới bàn giao

`Prefabs/UI` thêm `TEMPLATE_TRIBUTE`, `TEMPLATE_PAIRED`, `TEMPLATE_MEMORY` và `DraftReview`. `CiviAssetCatalog.draftBriefs` tham chiếu ba JSON TV1; `draftTemplates` và `draftReviewPrefab` tham chiếu prefab thật. Mở từ nút **Thử ba mẫu trang nháp** sau S00.

`DraftReviewView.Initialize(catalog, close, version)` nhận phiên bản từ người gọi; `Gesture` nhận cử chỉ đang giữ. Sự kiện `Requested(DraftPreviewRequest)` chỉ đề nghị loại mẫu, nguồn, người nhận, phiên bản và phạm vi `internal_review`. Các choice ID tương ứng là `S05_HOLD`, `S05_SEND_NHI`, `S05_SEND_GROUP_TRIBUTE` hoặc `S05_SEND_GROUP_OTHER`. TV2 kiểm quyền, phiên bản, nguồn và domain command trước khi áp dụng; UI không cập nhật `draft_seen_by`, không dựng bảng cảm ơn và không cấp quyền in/đăng.

Luồng UI: chọn mẫu/người nhận → xem lại đầy đủ → xác nhận. `Submitting` chặn yêu cầu lặp; dùng `CompleteRequest(success, message)` để báo kết quả, khi lỗi giữ nguyên mẫu/người nhận để thử lại. Người gọi cần giữ nội dung nháp khi chuyển cảnh, đóng màn hoặc khôi phục tiến trình; bản thân UI không thay hệ thống lưu game.

Bản thử hiện tại gắn một callback giả lập ghi lựa chọn để kiểm bố cục; không thực thi S05. Mẫu paired không có ảnh chụp mới khi chưa chụp; memory có hai ảnh hỏng và sơ đồ; tribute lấy nguồn 0442. Chữ trên trang là đề xuất trình bày, chờ TV1 duyệt.

Kiểm W3.4: `artifacts/qa/draft-ui-model-checks.json`, `draft-ui-integration-checks.json` và ảnh `07-draft-*.png`/`08-draft-error-preserved.png` trong hai thư mục độ phân giải. Chưa đánh dấu S05 đã tích hợp.

## Kiểm nhận

Xem `artifacts/qa/s00-model-checks.json`, `s00-build-checks.json`, các thư mục `S00-1280x720`, `S00-1920x1080`. Kết quả kiểm code/UI không thay nhận xét người chơi hoặc xác nhận TV1/TV5. Phiếu nhận xét nằm ở `docs/qa/TV3-NHAN-XET-S00.md`.
