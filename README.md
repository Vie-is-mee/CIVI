# CIVI · Những người không có chỗ dưới bầu trời

Game 3D góc nhìn thứ ba làm bằng Unity, chuyển thể **Chương 1 · Thành phố công bằng** từ `STORY.md`.
Bản này lấy CIVI_5 làm gốc (cốt truyện, thoại, âm thanh, thiết kế nhân vật) và gộp các tính năng của CIVI_6.

## Chạy game

Bấm **`CHOI_GAME.bat`**. Đây là launcher duy nhất.

- Lần đầu sau khi clone sẽ tự build bằng Unity 6000.4.0f1 (cần module Windows Build Support), mất vài phút.
- Bản chơi nằm ở `release/CIVI`. Khi gửi cho người khác, gửi cả thư mục, không gửi riêng file exe.
- Máy có card rời: launcher xin Windows chạy game trên card mạnh. Không có GPU dùng được: game vẽ bằng CPU ở 30 FPS.

## Điều khiển

| Phím | Việc |
|---|---|
| WASD / phím mũi tên | Đi |
| Chuột | Xoay camera, không cần giữ nút. Con lăn để thu phóng |
| Shift / Space | Chạy / nhảy |
| E hoặc F | Trò chuyện, nhặt đồ, xem vật thể, qua cửa, đọc tiếp |
| Q | Kính Debug: xem lớp dữ liệu của thành phố, thấy vật ẩn |
| M / B | Bản đồ / túi đồ và nhiệm vụ |
| C | Hỏi CIVI |
| Giữ Alt | Hiện con trỏ, thả ra để chơi tiếp |
| 1–4 | Chọn mục trong hồ sơ và bàn ánh sáng |
| Esc / P | Tạm dừng |
| F3 | Hiện FPS và mức hình ảnh |

## Bật AI cho "Hỏi CIVI"

Game tự chọn nguồn theo thứ tự, không nguồn nào thì dùng câu soạn sẵn:

1. **Claude:** đặt biến môi trường `ANTHROPIC_API_KEY`, hoặc tạo file `civi-ai-key.txt` chứa key trong thư mục lưu game (`%USERPROFILE%\AppData\LocalLow\VSIC\CIVI`).
2. **Ollama:** nếu Ollama đang chạy trên máy, game tự dùng, ưu tiên model `llama3`. Câu hỏi đầu tiên mất khoảng một phút để nạp model.

Không đóng gói key vào bản build vì ai cũng có thể mở file game ra đọc.

## Có gì trong game

- **Cốt truyện:** toàn bộ 9 nhiệm vụ, 5 thao tác hồ sơ, 154 trang thoại đã duyệt của CIVI_5.
- **Thế giới 3D:** Tầng 42 liên thông (văn phòng, phòng lịch sử, căng tin, phòng họp, sân thượng, hành lang kỹ thuật), căn hộ của Mai, phòng bệnh hồi ức, thành phố mái vòm. Ánh sáng đổi theo giờ trong ngày.
- **Nhân vật:** 14 người dựng từ bảng thiết kế bằng Blender, có xương, hoạt ảnh và biểu cảm mặt. Drone CIVI và mèo Bé Điện.
- **Khám phá:** 8 nhiệm vụ phụ, vật phẩm nhặt được, túi đồ, 12 hạt sáng, ghi chú, hộp khóa, minimap, dấu chỉ đường.
- **Từ CIVI_6:** Kính Debug, Bàn ánh sáng, terminal tìm dòng code, báo động trạm B4 có đếm giờ, cuộc gọi của anh Tường, Bé Điện, Hỏi CIVI, báo cáo kiểm toán cuối chương, thẻ kết quả.
- **Âm thanh:** 14 lớp ambience, 57 hiệu ứng theo cảnh, 14 âm giao diện từ gói `VSIC26_CH1_AUDIO`.

## Thư mục

| Đường dẫn | Nội dung |
|---|---|
| `Game/` | Project Unity. Code ở `Assets/_Civi/Scripts`, dữ liệu ở `Assets/_Civi/Resources/Chapter1` |
| `Game/Assets/_Civi/Resources/Chapter1/quests.json` | Vật phẩm, nhiệm vụ phụ, ghi chú. Sửa được mà không cần đụng code |
| `Game/Assets/_Civi/Resources/Chapter1/credits.json` | Tên nhóm và thành viên |
| `tools/blender/` | Script sinh toàn bộ model. Model không sửa tay |
| `tools/` | Build, QA, nhập âm thanh, kiểm thoại |
| `dialogue/`, `STORY.md`, `character-design/`, `source-assets/` | Nguồn gốc từ CIVI_5 |

## Dành cho người sửa game

```bash
bash tools/rebuild.sh            # sinh lại model từ Blender rồi build
bash tools/rebuild.sh world      # chỉ sinh lại môi trường
bash tools/unity.sh Civi.EditorTools.CiviBuild.Build   # chỉ build
bash tools/qa.sh artifacts/qa    # chơi tự động hết chương trong bản build thật
dotnet run --project tools/ChapterTests                # kiểm logic truyện và nhiệm vụ
py -3.12 tools/test_dialogue_content.py                # kiểm nguồn thoại
```

Scene `CIVI-Main.unity` do `CiviSceneBuilder` sinh ra, không sửa tay. Thoại sửa ở `dialogue/`, rồi chạy `tools/compile_chapter_dialogue.py`.

## Giới hạn hiện tại

- Mới có Chương 1. Chương 2–3 của CIVI_6 chưa chuyển sang.
- Nhân vật và môi trường dựng bằng code, chưa phải model do họa sĩ làm, chưa có lồng tiếng.
- Mốc thời gian trong `CUE_SHEET.csv` chỉ là minh họa, hiệu ứng âm thanh đang rải đều theo tiến độ đọc thoại, cần căn lại bằng tai.
- Gói âm thanh ghi rõ chưa xác nhận giấy phép nguồn bên thứ ba, cần kiểm trước khi phát hành thương mại.
- Bản lưu của CIVI_5 không chuyển sang được: định dạng mới có tọa độ 3D và nhiệm vụ phụ.
