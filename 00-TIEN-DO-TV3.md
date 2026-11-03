# TV3 — BẮT ĐẦU Ở ĐÂY

**2026-10-23 · TV3 · ĐÃ TÍCH HỢP VÀ KIỂM S00 · chờ nhóm duyệt**

**Mở [01-CHOI-THU-S00.bat](01-CHOI-THU-S00.bat)** hoặc [CIVI-S00.exe](release/S00-TV3-Windows/CIVI-S00.exe). Giữ cả thư mục bản chạy khi gửi bạn; người chơi không cần cài Unity. Đọc hết S00 rồi chọn **Thử ba mẫu trang nháp** để xem phần W3.4 mới. Truyện dừng tại điểm nối S01; bàn nháp chỉ thử UI, chưa gửi nội dung thật. File lưu riêng, không thay tiến trình bản cũ.

## Nhìn cây là biết đã làm tới đâu

```text
CIVI_2/
├── 00-TIEN-DO-TV3.md                        ← tiến độ hiện tại
├── 01-CHOI-THU-S00.bat                      ← mở bản thử
├── Game/Assets/_Civi/
│   ├── Art/Backgrounds/                     ← 2 nền ứng viên
│   ├── Art/Characters/                      ← 2 atlas; có portrait giữ suốt câu
│   ├── Art/Photos/                          ← 16 ảnh, gồm 3 cặp callback
│   ├── Art/Props/                           ← 4 prefab: sơ đồ, kẹp, pin, HẸN
│   ├── Art/UI/README-TIEN-DO.md              ← UI hiện dựng bằng component, chưa có atlas nút riêng
│   ├── Audio/                              ← 6 WAV; đã gắn hiệu ứng S00/animatic
│   ├── Fonts/                              ← 3 TTF + giấy phép đi kèm
│   ├── Prefabs/UI/                          ← 6 prefab: thoại, lựa chọn, 3 mẫu, DraftReview
│   ├── Config/TV3-AssetCatalog.asset         ← tham chiếu dữ liệu, ảnh, âm, prefab
│   ├── Scenes/S00-TV3-Preview.unity          ← scene tích hợp S00
│   ├── Scripts/                            ← S00Session + bộ trình bày tách riêng
│   ├── Scripts/Presentation/Animatic/       ← VyAnimatic: 32s, mốc chính 09–18
│   ├── Scripts/Presentation/Drafting/       ← ba mẫu nháp và DraftReview
│   ├── Video/README-TIEN-DO.md               ← animatic đã chạy; chưa xuất video phim
│   └── Content/                            ← nguyên dữ liệu TV1
├── release/S00-TV3-Windows/                 ← bản Windows một cảnh
├── docs/production/tv3-task-status.md        ← 8 nhiệm vụ riêng của thành viên 3
├── docs/qa/TV3-NHAN-XET-S00.md               ← chờ nhóm/người chơi nhận xét
├── docs/engineering/tv3-s00-integration.md   ← TV2 nhận mã và nối tiếp
└── artifacts/qa/                           ← kiểm model/build/UI + ảnh kiểm
```

## Bốn bước đã yêu cầu

| Bước | Đã làm được | Còn phải xác nhận |
| --- | --- | --- |
| 1 · Ảnh S00 và gesture | BAD_01 đúng đèn tắt, BAD_02 đủ bốn; ba biến thể pose_1/2/3 ở hai thời điểm; 0442 đã sửa Nhi nhìn An | TV1 duyệt hình; 0441 đã rà, chờ duyệt continuity với 0442 |
| 2 · UI, đạo cụ, animatic | Sáu prefab UI, bốn prefab đạo cụ, portrait giữ suốt câu; animatic 32s; ba mẫu nháp/người nhận/xác nhận/lỗi đã thử 9 tổ hợp ở mỗi độ phân giải | Chưa có diễn xuất toàn thân hoặc giọng thu; UI nháp chưa nối vào S05; các đạo cụ còn lại và UI cảnh sau còn thiếu |
| 3 · Tích hợp S00 | AssetCatalog + scene + bản Windows; 27 tổ hợp, 135 roundtrip dấu trang và UI 720p/1080p đạt | TV2 nhận mã; S01–S10 và các kết chưa tích hợp cây mới |
| 4 · Nhóm kiểm nhận | Đã có [phiếu thử](docs/qa/TV3-NHAN-XET-S00.md), ID câu/cảnh để ghi lỗi và bằng chứng kỹ thuật | Chưa có xác nhận TV1/TV5 hoặc nhận xét người chơi thật; chưa chốt demo toàn game |

**Kiểm kê:** 29 media + 29 .meta (20 PNG, 6 WAV, 3 TTF); đủ 32 mã brief được đối chiếu. Có 12 mã có file ảnh ứng viên; không quy con số này thành phần trăm hoàn thành.

| Trạng thái mã brief | Số mã |
| --- | ---: |
| Đã có ứng viên · chờ duyệt và tích hợp | 12 |
| Chưa ghép nền và đạo cụ | 1 |
| Chưa có file | 2 |
| Đã có animatic kéo lại được · chờ nhóm duyệt | 1 |
| Nội dung TV1 · chờ nối thoại | 1 |
| Đã có prefab riêng · chờ nhóm duyệt | 7 |
| Chưa có prefab/layer riêng | 6 |
| Chưa có trang dựng động | 2 |

## Làm gì tiếp?

1. Đọc [tám nhiệm vụ TV3](docs/production/tv3-task-status.md). Nhóm chơi bản S00 và bàn thử ba mẫu, rồi điền [phiếu nhận xét](docs/qa/TV3-NHAN-XET-S00.md); ưu tiên câu khó hiểu, gesture chưa rõ, hình lệch ý và nhịp đọc.
2. TV2 nhận [mã tích hợp](docs/engineering/tv3-s00-integration.md), chốt contract rồi nối S01 → S03 và UI nháp vào S05. Không dùng S00Session làm bộ chạy toàn truyện mà bỏ kiểm quyền/điều kiện.
3. TV3 sửa theo nhận xét, bổ sung đạo cụ/mẫu nháp và nền thiếu. Chỉ gói demo toàn truyện sau khi các cảnh nối được và đã thử nhánh.

### Ảnh/nền còn thiếu hoặc cần chỉnh

- `BG_CORRIDOR`: Chưa có nền hành lang/cửa lớp phù hợp. Không đổi tên nền phòng thành hành lang.
- `BG_STONE_BENCH`: Chưa có nền ghế đá trống. PHOTO_LATER_TWO có sẵn hai người nên không dùng làm nền để ghép thêm nhân vật.

## Bằng chứng và cách cập nhật

- [Kiểm tích hợp S00](artifacts/qa/s00-integration-checks.json): phạm vi, hash nguồn/bản chạy, kết quả và phần chưa được nhóm duyệt.
- [Kiểm model](artifacts/qa/s00-model-checks.json), [build](artifacts/qa/s00-build-checks.json), [UI 720p](artifacts/qa/S00-1280x720/runtime-checks.json), [UI 1080p](artifacts/qa/S00-1920x1080/runtime-checks.json).
- [Ảnh UI](artifacts/qa/S00-1280x720/02-choices.png), [cặp callback](artifacts/qa/S00-1280x720/05-callback.png), [phụ đề](artifacts/qa/S00-1280x720/06-animatic-full-quote.png).
- [Kiểm UI nháp](artifacts/qa/draft-ui-integration-checks.json), [ảnh tribute](artifacts/qa/S00-1280x720/07-draft-tribute.png), [paired](artifacts/qa/S00-1280x720/07-draft-paired.png), [memory](artifacts/qa/S00-1280x720/07-draft-memory.png).
- [Bảng tài nguyên](docs/production/tv3-asset-map.json), [bàn giao từng mã](docs/production/TV3-BAN-GIAO.md), [prompt ảnh mới](source-assets/TV3/imagegen-production.json), [nguồn/giấy phép](licenses/TV3-ASSETS.md).
- Chạy `python tools/QA/update_tv3_progress.py --check`. Sau khi sửa asset/mã cần cập nhật map và kiểm lại; hash khác sẽ không tiếp tục ghi S00 đã xác minh. Không đổi tên thư mục/GUID để biểu diễn phần trăm tiến độ.

Chưa push cây này lên GitHub. “Đã kiểm kỹ thuật” không thay “nhóm đã duyệt” hoặc “người chơi đã thử”.
