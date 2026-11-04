# CIVI · Tầng 42 — bản demo web Chương 1

Bản MVP để pitching VSIC 2026: game bàn làm việc chạy trên trình duyệt (máy tính và điện thoại), khoảng 7 phút.

## Chạy thử

- **Mở ngay:** mở `dist/index.html` bằng trình duyệt (Chrome/Edge/Safari). Không cần cài gì.
- **Đưa lên mạng:** tải cả thư mục `dist/` lên GitHub Pages, Netlify hoặc bất kỳ host tĩnh nào.
- **Bản có AI thật:** link Claude Artifact (do chủ dự án chia sẻ). Ở đó Kiểm toán viên dùng Claude của người xem để viết báo cáo riêng.

## Cấu trúc

- `index.html` — mã nguồn (nội dung trang, chưa có khung `<!doctype>`; đây là định dạng Claude Artifact cần).
- `assets/` — ảnh cắt và nén từ `Game/Assets/_Civi/Art/LiuQuangC1/` (tranh của đội).
- `dist/` — bản độc lập đã đóng gói. Sau khi sửa `index.html`, chạy lại:

```
python tools/build_web_demo.py
```

## Luồng chơi

1. Ký ức: người chơi là cô y tá, nhập 4 dòng dữ liệu → vạch xanh.
2. Bàn làm việc Tầng 42: 6 hồ sơ có đồng hồ đếm ngược. Mở bằng chứng tốn thời gian. Nhóm chat, bản tin chạy chữ, hồ sơ hài (shop bỉm livestream, mèo xin cư trú).
3. Hồ sơ 8-12 (dân số = 0) có ảnh phóng to với 3 chi tiết. Bài toán kho thuốc / nhà trẻ có phương án thứ ba ẩn.
4. Khủng hoảng: cảnh báo trạm khí B4 cùng lúc với cuộc gọi của anh Tường (tài xế).
5. Bản đồ năng lượng 3D (khoảng 25 giây, nút "Tiếp" để qua): camera nhìn sa bàn thành phố từ trên cao, các quyết định trong ca hiện lại thành dòng năng lượng sáng (H01, H04, B4; từ chối/chờ là đường đứt đỏ rồi tắt). Phân khu 8-12 là vùng tối, dân số 0; nếu đã gửi xác minh, cửa sổ ở đó lần lượt sáng lên. Không có WebGL thì bỏ qua cảnh này.
6. Phòng trưng bày (câu hỏi của cậu học sinh) → tối ở nhà với mẹ → ảnh 8-12 lúc đêm.
7. Báo cáo kiểm toán về chính người chơi: AI (nếu người chơi cho phép) hoặc báo cáo dự phòng không cần mạng, đều dựa trên nhật ký hành vi. Số người bị bỏ sót, câu hỏi ngược, thẻ kết quả để chia sẻ, xem/xóa dữ liệu.

### Chương 2 · Những chiếc ô hỏng (bản thử, phần 1)

Vào từ nút "Chương 2 · bản thử" ở màn đầu, hoặc "Chơi tiếp Chương 2" sau báo cáo / thẻ kết quả Chương 1. Khoảng 4 phút, chuyển thể từ Chương 2 của *Những người không có chỗ dưới bầu trời* (`Game/Assets/_Civi/Resources/NewStory/novel_source.txt`).

- Mai đi theo bản đồ thành phố tới trạm lọc 19 và gặp chỗ ngập: nước che hai bậc thang, bản đồ vẫn ghi "thông". An dẫn đường bằng lối vạch phấn trong sổ của em.
- Bảy chặng trên mô hình 3D Rìa Mưa (cùng scene với màn mở đầu): quầy vá ô, hẻm ống 13, cầu tấm nhựa, tiệm may, chợ mái bạt, trạm lọc 19, căn nhà trong ảnh của Linh.
- Ô đếm ở góc phải: "Hồ sơ 8-12: 0" đặt cạnh số người Mai đã gặp. Mỗi người gặp được ghi vào sổ của Mai, kèm một chi tiết riêng. Họ không được mô tả như một tính cách chung.
- Lựa chọn: hỏi đường hay cãi bản đồ; xin chụp sổ của An (An cho chụp trang vẽ ô, còn trang có tên người thì phải hỏi từng người); trả lời Tín; trả tiền bát cháo hay hỏi; có hỏi ai trực tiếp sửa ống không; dòng "dân số" trong ghi chú hiện trường (giữ 0 / đề nghị xác minh lại / hỏi An trước).
- Đoạn mở đầu đổi theo hồ sơ HS-0019 ở Chương 1 (gửi xác minh hay trả về).
- Không có WebGL thì vẫn chơi được bằng chữ, nền là ảnh tĩnh.

## Lộ trình

Đã xong: Chương 1 trên web; màn mở đầu 3D; bản đồ năng lượng 3D; Chương 2 phần 1.

Tiếp theo, theo thứ tự:

1. **Chương 2 · phần 2 "Bà Dung".** Bà Dung ngã trong căn nhà mất điện: giữ đèn, chèn ống, ghi mã ống, chụp vết nứt. Câu đố dữ liệu: hai phòng ban ghi hai trạng thái mâu thuẫn ("tuyến phân phối chung" và "công trình tự phát"), nên lệnh sửa cứ quay về "chờ". Người chơi chọn báo cáo gửi ai và viết gì.
2. **Chương 2 · phần 3 "Hộp thiếc".** Bà Hạ trở về. Hạ cho chụp giấy tờ nhưng muốn xem Mai chụp gì, nên quyền xin theo từng tư liệu. Chuyện bố An không được ép kể. Sổ lời hứa ghi lời hứa lớn và lời hứa nhỏ làm được. Ở trạm kiểm toán lưu động: mã lao động của Hạ chỉ có trong phụ lục giấy. Kết chương bằng câu "Lần này, cô không nhìn lên.", rồi báo cáo kiểm toán Chương 2 dựa trên sổ của Mai.
3. **Nối hai chương.** Lưu tiến độ trên máy người chơi (có nút xóa), chọn chương ở màn đầu, thẻ chia sẻ gộp cả hai chương.
4. **Tranh nhân vật Chương 2** (An, bà Hạ, Tín, bà Dung, người bán cháo) theo brief cho đội vẽ. Trong lúc chờ, dùng chân dung chữ cái.
5. **Chuẩn bị thi:** âm thanh (mưa máy, chợ), kiểm trên điện thoại thật, playtest có phiếu nhận xét, kịch bản video demo mới cho Lưu Quang (`docs/submission/demo-script.md` vẫn là truyện cũ).
6. **Chương 3–5**, tới Điểm rẽ I ở cuối Chương 5 của tiểu thuyết.

## Ghi chú kỹ thuật

- Thư viện ngoài duy nhất: Three.js 0.147 (CDN jsDelivr) cho màn mở đầu, bản đồ năng lượng và Rìa Mưa ở Chương 2; một renderer/scene dùng chung (`initCity()`, `startMap()`, `startCh2()`). Kịch bản Chương 2 là mảng `C2SCRIPT`. Không tải được thì game dùng ảnh tĩnh và bỏ qua bản đồ. Ngoài ra chỉ có Google Fonts.
- Nhật ký chơi chỉ nằm trong bộ nhớ trang, không lưu lại sau khi tải lại trang; không thu tên hay thông tin cá nhân.
- Báo cáo dự phòng: hàm `localReport()`; prompt AI: hàm `aiPrompt()` trong `index.html`.
