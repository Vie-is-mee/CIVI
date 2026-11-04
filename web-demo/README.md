# CIVI · Tầng 42 — game 3D trên trình duyệt (Chương 1–3)

Bản demo pitching VSIC 2026: một game phiêu lưu 3D góc nhìn thứ ba chạy trên trình duyệt, máy tính và điện thoại. Người chơi điều khiển Mai, thực tập sinh IT ở cơ quan điều phối của thành phố mái vòm Lưu Quang, đi cùng CIVI, một drone AI hơi quá tự tin. Ba chương, khoảng 30 phút.

Câu hỏi của cả game: hệ thống chia điện, khí sạch và nước theo điểm đã ghi Phân khu 8-12 có **0 người**. Ai đã bị đếm sót, vì dòng code nào, và đếm lại thế nào cho đúng mà không lấy dữ liệu của người không đồng ý.

## Chạy thử

- **Mở ngay:** mở `dist/index.html` bằng Chrome/Edge/Safari. Không cần cài gì. Nếu trình duyệt chặn file cục bộ, chạy `python -m http.server` trong `dist/` rồi mở `http://localhost:8000`.
- **Đưa lên mạng:** tải cả thư mục `dist/` lên GitHub Pages, Netlify hoặc host tĩnh bất kỳ.
- **Bản có AI thật:** link Claude Artifact (do chủ dự án chia sẻ). Ở đó nút "Hỏi CIVI" và báo cáo cuối game dùng Claude của người xem; ngoài Claude, CIVI trả lời bằng câu soạn sẵn và báo cáo dùng bản không cần mạng.
- **Bản đọc cũ** (game chữ, có bản đồ năng lượng 3D): `classic.html`, có link ở màn đầu. Máy không chạy được WebGL sẽ được đưa sang bản này.

## Điều khiển

| Máy tính | Điện thoại | Việc |
|---|---|---|
| WASD / phím mũi tên | kéo nửa trái màn hình | đi |
| Shift | nút chạy, hoặc kéo cần hết cỡ | chạy |
| Space | nút mũi tên lên | nhảy |
| kéo chuột, lăn chuột | kéo nửa phải | xoay, thu phóng camera |
| E / Enter / bấm chuột | nút bàn tay, chạm màn hình | nói chuyện, đọc tiếp |
| Q | nút kính | Kính Debug: xem lớp dữ liệu của thành phố |
| 1–4 | chạm | chọn câu trả lời |
| Esc / P | nút tạm dừng | tạm dừng |

## Luồng chơi

**Mở đầu · Ký ức.** Mai tám tuổi. Cô y tá áo xám nhập bốn dòng dữ liệu để giữ mẹ Mai lại bệnh viện. Người chơi bấm từng dòng.

**Chương 1 · Ngày đầu ở Tầng 42** (văn phòng 3D, khoảng 8 phút)
1. CIVI chào Mai. Con mèo Bé Điện đang ngủ trên ghế của Mai.
2. Bàn ánh sáng: đóng dấu bốn yêu cầu (bệnh viện, shop bỉm livestream, mèo xin số cư trú, trạm lọc 19). CIVI chấm điểm sẵn; làm khác CIVI thì nó "ghi chú một cách trung lập".
3. Ảnh của Linh: bật Kính Debug, tìm ba dấu hiệu có người ở căn nhà "dân số 0".
4. Terminal: tìm dòng code làm người biến mất khỏi phép đếm (`h.trang_thai == "đã xác minh"`), chọn cách sửa. `git push` bị chặn vì đóng băng hệ thống trước mùa bão.
5. Sếp Vinh tới. Chọn tự đi xác minh sau giờ làm, hoặc trả hồ sơ đúng quy trình.
6. Báo động trạm khí B4: chạy tới tường đông, vặn ba van trong 40 giây. Giữa chừng anh Tường (tài xế) gọi tới nhờ ghi giờ làm, mất 8 giây.
7. Hoàng hôn bên cửa sổ, nhìn về Rìa Mưa. CIVI: "Tôi đi cùng."

**Chương 2 · Những chiếc ô hỏng** (khu Rìa Mưa 3D, mưa đêm, khoảng 12 phút)
1. Đi theo mũi tên bản đồ chính thức tới trạm lọc 19: đường dẫn thẳng xuống chỗ ngập. An, cô bé sửa ô, gọi lại. Minigame bấm đúng nhịp để gắn nan ô.
2. Đi theo An qua lối không có trên bản đồ: tiệm may (Tín kể chuyện Lõi Sáng), mép sông (sổ đo mực nước của An, xin chụp hay không), cầu tấm nhựa, chợ mái bạt (bát cháo của ông cụ), trạm lọc 19 (sổ giấy của quản đốc), căn nhà trong ảnh của Linh.
3. Nhiệm vụ phụ: chiếc dép đỏ trôi ở rãnh của chú Sáu; xin ghi tên bà Sáu cơm, cô Cúc. Người đồng ý thì được ghi, người từ chối thì CIVI không dùng.
4. Bé Tí báo bà Dung ngã. Chạy qua chỗ ngập: nhảy lên các viên đá kê, nước đẩy người chơi trôi nếu lội.
5. Nhà bà Dung mất điện: Mai cầm đèn pin, soi vào đâu thì người ở đó làm được việc (bà tìm thuốc, An chèn ống rò, Mai ghi mã đoạn ống). Máy tính bảng: tìm hai dòng mâu thuẫn giữa Phòng Hạ tầng và Phòng Cư trú, lý do lệnh sửa cứ quay về "chờ". Chọn lời hứa trong quyền hay ngoài quyền.
6. Bà Hạ về. Hộp thiếc: chụp từng giấy tờ, bà Hạ nhìn. Ảnh gia đình không được chụp. Chuyện bố An.
7. Trạm kiểm toán lưu động: Kính Debug lộ phụ lục giấy, người sửa ống là bà Hạ nhưng điểm và thưởng về công ty thầu. "Cho CIVI học lại" chỉ từ dữ liệu được đồng ý.
8. An đuổi theo Mai ra cổng. Dòng kết: hồ sơ vẫn ghi 0 người; tối nay Mai đã gặp N người.

**Chương 3 · Người có điểm bằng không** (Tầng 42 ngày bão, khoảng 8 phút)
1. CIVI đã học lại: Phân khu 8-12 có N người, nhưng điểm đóng góp bằng 0 nên khi bão được chia 0 điện. Đếm đúng vẫn chưa đủ; Mai đề xuất "sàn sinh tồn".
2. PR #219 cần ba người duyệt, làm theo thứ tự tùy ý:
   - Khoa: chọn đúng ba test cho PR (có test bẫy "Bé Điện có số cư trú" và "if True: đếm tất cả"), chạy `pytest`, "LGTM: Looks Good To Mai".
   - Chị Hằng: phiếu ngoại lệ ĐB-07 "Đơn xin phép được cho phép". Máy in kẹt; CIVI đề xuất tắt bật, vỗ cạnh trái; con mèo Bé Điện ngồi lên thì máy chạy.
   - Linh: chọn bằng chứng đính kèm, chỉ những gì người dân đã cho phép.
3. Sếp Vinh mở khóa đóng băng và nhận trách nhiệm trong nhật ký. Nếu Mai dọa tự merge, Vinh nhắc lại câu của bà Hạ.
4. Bão tới sớm: chia 48 MW ở bàn ánh sáng. CIVI điền sẵn theo điểm (Lõi Sáng 34 MW, Phân khu 8-12 0 MW). Người chơi tự cân bệnh viện, trạm khí B4, sàn 3 MW cho 8-12, đài phun nước của Lõi Sáng, livestream của shop.
5. Cùng đêm ở Rìa Mưa: nếu 8-12 được chia đủ sàn, cửa sổ sáng lên. Dòng kết: "N người. Đã đếm. Đã có điện. Ai cũng có tên."

**Báo cáo.** CIVI kiểm toán chính người chơi từ nhật ký lựa chọn: đã gặp bao nhiêu người, bao nhiêu người đồng ý hay từ chối, lời hứa nào ngoài quyền, việc gì còn dang dở. Trong Claude có thể để AI viết lại báo cáo. Thẻ kết quả để chia sẻ.

## Cấu trúc mã

- `index.html` — trang game: giao diện HUD, CSS, nạp Three.js 0.147 (jsDelivr) và năm file trong `game/`. Viết dạng nội dung trang (không có `<!doctype>`), đúng định dạng Claude Artifact.
- `game/engine.js` — renderer và chất liệu toon (cel shading, viền ngoài, ánh sáng viền), điều khiển bàn phím/chuột/cảm ứng, camera người thứ ba (né tường, góc quay hội thoại), di chuyển và va chạm, NPC, bong bóng thoại, lựa chọn, mục tiêu và mũi tên chỉ đường, Kính Debug, hạt, âm thanh tổng hợp bằng WebAudio, lưu tiến độ.
- `game/chars.js` — nhân vật chibi dựng từ khối cơ bản, mặt anime vẽ bằng canvas (11 biểu cảm, chớp mắt, mấp máy môi), hoạt ảnh thủ tục (đi, chạy, nhảy, thở, 18 cử chỉ, 11 tư thế, nhìn theo người nói, tóc đung đưa), drone CIVI, mèo Bé Điện, danh sách 16 nhân vật.
- `game/worlds.js` — ba bối cảnh: Tầng 42, Rìa Mưa (mưa, nước, cầu, chợ, trạm lọc, đá kê qua chỗ ngập, đường chân trời Lõi Sáng), phòng tối của bà Dung.
- `game/story1.js` — hàm dùng chung cho kịch bản, màn đầu, ký ức và Chương 1.
- `game/story2.js` — Chương 2, báo cáo, chat với CIVI, khởi động game.
- `game/story3.js` — Chương 3: duyệt PR, máy in, mở khóa đóng băng, chia điện trong bão, đoạn kết ở Rìa Mưa.
- `classic.html` — bản đọc cũ (Chương 1 và Chương 2 phần 1–2 dạng chữ).
- `assets/` — ảnh nén từ `Game/Assets/_Civi/Art/LiuQuangC1/` (tranh của đội); `city-window.jpg` là cảnh thành phố ngoài cửa sổ Tầng 42.
- `dist/` — bản độc lập đã đóng gói. Sau khi sửa mã, chạy lại:

```
python tools/build_web_demo.py
```

Nhân vật và bối cảnh đều dựng bằng mã (không có file 3D ngoài), nên cả game nhẹ (khoảng 210 KB mã, cộng ảnh). Máy yếu, điện thoại hoặc cửa sổ hẹp tự chuyển sang đồ họa nhẹ: tắt bóng đổ và viền, giảm hạt mưa.

## Góc AI và dấu ấn sinh viên IT

- CIVI là một AI tối ưu hóa thật thà: nó không ác, nó chỉ đếm những gì được cho xem. Câu chốt của nó ở Chương 2: "Tôi không cộng sai. Tôi chỉ chưa từng được cho thấy họ."
- "Cho CIVI học lại" chỉ dùng dữ liệu người dân đồng ý; người từ chối được tôn trọng và không bị đếm thay.
- Kính Debug, terminal có dòng code lỗi, `git push` bị đóng băng mùa bão, bảng KPI "0 lỗi / 2.190 ngày".
- Nút "Hỏi CIVI" (trong Claude): chat với CIVI bằng AI, giữ vai, không tiết lộ diễn biến sau mục tiêu hiện tại.

## Giới hạn hiện tại

- Đồ họa là toon dựng bằng khối cơ bản, chưa tới mức Genshin: chưa có mô hình nhân vật do họa sĩ dựng, chưa có hoạt ảnh motion capture, chưa có lồng tiếng. Muốn lên mức đó cần mô hình GLB có xương (đội vẽ hoặc đặt làm), bộ hoạt ảnh (Mixamo hoặc tự quay), và âm nhạc.
- Chưa thử trên điện thoại thật; mới thử bằng trình duyệt giả lập (xem phần kiểm thử trong lịch sử commit).

## Lộ trình

1. Chương 4–5 theo tiểu thuyết, tới Điểm rẽ I; giữ lời hứa với bà Dung, bà Hạ, An từ Chương 2.
2. Mô hình nhân vật thật cho Mai, An, CIVI, bà Hạ; âm nhạc theo từng khu.
3. Playtest trên điện thoại thật, phiếu nhận xét, video demo mới cho hồ sơ thi.
