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
5. Phòng trưng bày (câu hỏi của cậu học sinh) → tối ở nhà với mẹ → ảnh 8-12 lúc đêm.
6. Báo cáo kiểm toán về chính người chơi: AI (nếu người chơi cho phép) hoặc báo cáo dự phòng không cần mạng, đều dựa trên nhật ký hành vi. Số người bị bỏ sót, câu hỏi ngược, thẻ kết quả để chia sẻ, xem/xóa dữ liệu.

## Ghi chú kỹ thuật

- Không dùng thư viện ngoài, chỉ Google Fonts.
- Nhật ký chơi chỉ nằm trong bộ nhớ trang, không lưu lại sau khi tải lại trang; không thu tên hay thông tin cá nhân.
- Báo cáo dự phòng: hàm `localReport()`; prompt AI: hàm `aiPrompt()` trong `index.html`.
