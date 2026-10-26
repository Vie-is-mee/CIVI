# ĐỀ XUẤT CẢI TIẾN KỊCH BẢN CIVI (V4.1) — BẢN BIÊN SOẠN TV1
> **Tài liệu:** Đề xuất điều chỉnh cốt truyện, nhịp điệu, tâm lý nhân vật và tương tác  
> **Người thực hiện:** TV5 (Đỗ Phú Vinh - QA/Playtest Lead)  
> **Áp dụng cho:** Kịch bản TV1 (`kich-ban-doc-thuan.md`, `kich-ban-mvp.md`, `dialogue/`)  
> **Thời điểm cập nhật:** 25/10/2026

---

## I. ĐÁNH GIÁ TỔNG QUAN BẢN KỊCH BẢN HIỆN TẠI CỦA TV1

### 1. Điểm mạnh vượt trội (Cần giữ vững):
- **Triết lý nhân văn, không giáo điều:** Rất xuất sắc khi không áp đặt "điểm đạo đức", không có người tốt kẻ xấu một chiều, không giải quyết mâu thuẫn bằng một lời xin lỗi sáo rỗng.
- **Hệ thống Đồng thuận (Consent) chặt chẽ:** Tôn trọng quyền cá nhân đối với hình ảnh và hiện vật. Đây là điểm sáng độc đáo hiếm có trong game tương tác học đường.
- **Cấu trúc nhánh rẽ logic:** Logic phân luồng từ S00 đến E_A, E_B, E_C rất rõ ràng, đã vượt qua toàn bộ 22 test route tự động.

### 2. Những điểm nghẽn khiến kịch bản bị "gãy nhịp" hoặc "kém thú vị":
1. **Chuyển cảnh thời gian bị hẫng (Disjointed Temporal Pacing):** Đặc biệt là cú nhảy từ S00 (Tháng Mười năm ngoái) sang S01 (07/05 năm nay).
2. **Nhân vật An bị "hành chính hóa":** An đôi lúc giống một kiểm toán viên đi điều tra và thu thập giấy phép consent hơn là một người bạn thân đang đau đáu vì nhóm sắp tan rã khi ra trường.
3. **Thoại còn mang hơi hướng "văn bản kỹ thuật":** Một số câu thoại bàn về "quyền", "phạm vi xuất bản", "tư liệu nội bộ" nghe giống biên bản hợp đồng hơn là học sinh lớp 12 nói chuyện với nhau.
4. **Còn nhiều chỗ "Tell" hơn "Show":** Người chơi tiếp nhận manh mối qua lời thoại giải thích của NPC nhiều hơn là tự mình trải nghiệm khoảnh khắc "Eureka" khi tương tác với hiện vật.

---

## II. ĐỀ XUẤT CẢI TIẾN CHI TIẾT THEO TỪNG TRỌNG TÂM

### 1. Cải tiến chuyển cảnh S00 ➔ S01 (Vấn đề người chơi nêu ra)

* **Hiện trạng:**
  - S00: Buổi chụp thử tháng 10 vui vẻ, náo nức, chụp xong 3 tấm ảnh.
  - S01: Đùng một cái nhảy đến ngày 07/05/2026 (7 tháng sau), An nhận bản in thử và phát hiện mất Trang 17.
  - **Cảm giác người chơi:** Rất hụt hẫng! Người chơi chưa kịp hiểu: *Tại sao tháng 10 chụp xong mà tận tháng 5 mới làm kỷ yếu? Suốt 7 tháng qua 4 người có còn chơi với nhau không? Điều gì đã xảy ra ở quãng giữa?*
* **Đề xuất khắc phục (Cầu nối chuyển cảnh - Narrative Bridge):**
  - **Âm thanh & Thị giác làm cầu nối:** Tiếng "Tách!" đanh gọn của chiếc điện thoại chụp xong ở S00 kéo dài thành tiếng thở dài hoặc tiếng máy in offset công nghiệp chạy rầm rập.
  - **Thêm đoạn nối chuyển giao thời gian (Time transition text):**
    > *"Tháng Mười năm ấy, bốn đứa từng nghĩ chụp kỷ yếu chỉ mất đúng ba giây hẹn giờ.*  
    > *Rồi mùa thi ập đến. Tập đề dày lên, những buổi chiều trên sân khấu vắng dần. Lời hẹn 'hôm nào chụp lại cho đủ' bị cuốn trôi theo những lần báo bận.*  
    > *Bảy tháng sau — 07/05/2026, chỉ còn 48 tiếng trước khi xưởng in đóng bản kẽm cuối cùng..."*
  - **Làm rõ bản chất của S00:** Nhấn mạnh ngay từ S00 rằng buổi tháng Mười chỉ là buổi tập dợt văn nghệ kết hợp chụp nháp thử máy, còn Trang 17 là bài tập nhóm nộp cho ban kỷ yếu trường vào tháng 5.

---

### 2. Làm dày chiều sâu cảm xúc & Động lực nhân vật

#### A. Nhân vật An (Nhân vật chính):
- **Vấn đề:** An dễ bị biến thành người thụ động chạy việc.
- **Cải tiến:**
  - Thêm **độc thoại nội tâm (Inner Monologue)**: Cho thấy áp lực của An. Năm cuối cấp, ai cũng sắp chọn một con đường riêng (Minh thi Bách Khoa, Hạ học Mỹ thuật, Nhi có thể đi xa). Cuốn kỷ yếu này là cơ hội duy nhất để giữ lại một bằng chứng rằng 4 đứa đã từng có một thanh xuân rực rỡ bên nhau.
  - Thể hiện sự day dứt về lời hứa thất hứa: Khi nhìn bức ảnh `0443`, An nhớ lại cảm giác lúc mình vẫy tay gọi Nhi chụp hộ rồi sau đó bị bạn bè kéo đi mất.

#### B. Nhân vật Nhi:
- **Vấn đề:** Trong kịch bản hiện tại, Nhi xuất hiện khá cộc cằn, phòng thủ, khiến tester dễ có ấn tượng tiêu cực (coi Nhi là nhân vật khó chiều, chấp vặt).
- **Cải tiến:**
  - **"Cho Nhi một khoảnh khắc yếu lòng":** Ở cảnh S02 hoặc S07, cho An thấy một chi tiết nhỏ: Nhi vẫn đeo chiếc móc khóa nhóm, hoặc chiếc hộp kẹp cao su mới được Nhi bọc cẩn thận bằng khăn tay.
  - **Làm rõ tổn thương của Nhi:** Nhi không giận vì bị bắt chụp ảnh. Nhi giận vì cảm giác **bị coi là người ngoài nhưng lại bị biến thành công cụ**: Bị sửa lời nói, bị đem ra làm "bảng tôn vinh" như một đối tượng đáng thương hại. Câu nói của Nhi cần làm bật lên sự cô đơn:
    > *"Tao không cần tụi mày làm bảng vinh danh tao trước sân trường. Tao chỉ từng nghĩ... lúc chụp tấm đó xong, sẽ có đứa nhớ ra là tao vẫn đang đứng sau ống kính."*

#### C. Nhân vật Hạ:
- **Vấn đề:** Hạ dễ bị người chơi hiểu nhầm là người vô tâm hoặc "thủ phạm" muốn gạt Nhi ra.
- **Cải tiến:**
  - Thể hiện sự trân trọng chân thành của Hạ với bức ảnh `0443`: Bức ảnh 3 người lúc đó An và Minh đang cười toe toét ngớ ngẩn — đó là nụ cười thật nhất sau những ngày ôn thi kiệt sức. Hạ giữ lại vì trân trọng khoảnh khắc đó, chứ không phải vì ghét Nhi.
  - Cho thấy sự kiệt sức của Hạ: Hạ thức trắng nhiều đêm để chỉnh màu, dàn trang. Khi An chê trang nháp, Hạ phản ứng vì công sức của mình bị phủ nhận.

#### D. Nhân vật Minh:
- **Cải tiến:** Tăng vai trò "mỏ neo lý trí" của Minh. Minh là người nói ít nhưng nhìn thấu mọi chuyện. Minh giữ chiếc kẹp cũ, nhắc nhở giờ giấc, và chính Minh là người đã ngăn bản nháp tệ hại bị đưa thẳng vào xưởng in.

---

### 3. Tăng tính tương tác: "Show, Don't Just Tell"

#### A. Tại S02 (Chiếc kẹp có cao su mới):
- **Thay vì chỉ chọn menu đọc chữ:** Cho phép người chơi click tương tác trực tiếp lên đồ vật:
  - Click vào chiếc kẹp: Hiện pop-up 3D/2D quay cận cảnh. Thấy miếng cao su màu xanh mới tinh được dán đè lên vết trầy cũ -> Gợi mở chi tiết ai đó đã cẩn thận sửa nó.
  - Click vào sơ đồ 2 ô: Lật mặt sau tờ giấy nháp, thấy những dòng chữ gạch xóa nguệch ngoạc của Nhi khi cố gắng thiết kế trang cho nhóm.

#### B. Tại S03 (So sánh 3 bức ảnh 0441 - 0442 - 0443):
- **Tạo khoảnh khắc "Eureka" thị giác:**
  - Thay vì để Hạ nói toẹt ra nguyên nhân, cho người chơi một thanh trượt thời gian (Timeline Slider) hoặc công cụ soi kính lúp (Magnifier):
    * Soi góc dưới `0441`: Thấy bàn tay Nhi nắm sợi dây bấm giờ.
    * Soi mép phải `0442`: Thấy chiếc ổ cắm điện màu đỏ và mắt Nhi đang nhìn An.
    * Soi sang `0443`: Góc máy hơi nghiêng lên, độ cao vừa đúng tầm mắt của Nhi.
  - Người chơi tự nhận ra: *"Khoan đã... góc chụp này... chính là góc nhìn của Nhi!"*. Cảm giác tự mình khám phá ra sự thật sẽ tạo ấn tượng mạnh gấp 10 lần việc nghe đọc thoại!

#### C. Trợ giúp cho nhánh C (AI Input ở S03):
- **Hiện trạng:** Ô nhập tự do dễ làm tester bối rối không biết hỏi gì (Writer's Block).
- **Cải tiến:** Thêm các dòng "gợi ý suy nghĩ" mờ dưới ô chat (Placeholder/Prompt hints), ví dụ:
  * *"Gợi ý: Thử hỏi Hạ xem ai là người bấm bức 0443, hoặc hỏi về chiếc dây bấm giờ..."*

---

### 4. Tăng kịch tính thời gian (Tension & Pacing) ở S08 ➔ S09

- **Hiện trạng:** Thời gian chạy bằng cách cộng dồn phút biến số (`courtyard_minute`), người chơi khó cảm nhận được tính cấp bách.
- **Cải tiến trực quan:**
  - Bổ sung thanh hiển thị đồng hồ thời gian thực ở góc màn hình: `16:45` -> `16:55` -> `17:05`.
  - Hiệu ứng ánh sáng nền (Lighting shift): Khi bước sang 17:00, ánh nắng sân trường chuyển từ vàng rực sang cam tím hoàng hôn.
  - Nhịp điệu nhân vật: Ở mốc 17:05, Nhi bắt đầu nhìn đồng hồ, liếc ra phía cổng trường nơi xe buýt sắp đến. Điều này tạo áp lực tự nhiên khiến người chơi phải cân nhắc kỹ từng lựa chọn, không dài dòng giải thích.

---

### 5. Tự nhiên hóa ngôn ngữ đối thoại (Dialogue Naturalization)

Một số câu thoại cần chuyển ngữ từ "hành chính / kỹ thuật" sang "học sinh cấp 3":

| Vị trí | Lời thoại hiện tại (Hơi cứng / Thủ tục) | Đề xuất sửa đổi (Tự nhiên, giàu cảm xúc) |
|---|---|---|
| **S06 (Hạ)** | *"Bản này tớ dàn theo tư liệu nội bộ, quyền phát hành cậu phải tự đi hỏi."* | *"Tớ dàn trang này vì thấy ba đứa cười vui quá thôi. Còn in lên sách cho cả trường xem... cậu phải hỏi Nhi xem nó có chịu không đã."* |
| **S07 (An)** | *"Tao muốn xin phạm vi sử dụng kỷ yếu cho bức ảnh S00 hồi tháng Mười."* | *"Tấm chụp hồi tháng Mười tao giữ kỹ lắm. Bọn mình... dùng lại tấm đó cho Trang 17 nha Nhi?"* |
| **S07 (Nhi)** | *"Tuyên bố tôn vinh của cậu không khớp với mong muốn xuất hiện của tớ."* | *"Tụi mày làm cái bảng vinh danh này làm gì? Nhìn tao giống người ngoài cần được ban ơn lắm hả An?"* |
| **S10 (An)** | *"Kiểm tra lại toàn bộ danh sách chủ thể và phạm vi cấp quyền trước khi bấm xuất bản."* | *"Chắc chắn là ai cũng đồng ý rồi chứ? Bấm gửi xưởng in là không rút lại được nữa đâu."* |

---

### 6. Nâng tầm ý nghĩa của 3 Kết thúc (Endings)

#### E_A (Tấm mới — A New Photograph):
- Thêm đoạn vĩ thanh (Epilogue): Ngày bế giảng nhận sách, bốn đứa chụm đầu mở đúng Trang 17. Bức ảnh mới 4 người tuy không hoàn hảo như chụp studio nhưng đầy ắp kỷ niệm. Nhắc lại câu trêu chọc về tư thế chụp mà An đã chọn từ S00 (`opening_gesture`).

#### E_B (Những gì được giữ lại — What Was Kept):
- **Đây là cái kết cực kỳ giá trị, không được để người chơi cảm thấy là "kết thua/kết dở":**
- Nhấn mạnh thông điệp trưởng thành: Không phải tình bạn nào cũng kết thúc bằng một tấm ảnh chụp chung đông đủ nở nụ cười công nghiệp. Việc dám giữ lại bức ảnh cũ chân thật, dám thừa nhận những khoảng trống và tôn trọng sự lựa chọn không chụp của Nhi — đó chính là sự trưởng thành sâu sắc nhất của tuổi 18.
- Câu kết lắng đọng: *"Trang 17 không có thêm một bức ảnh mới. Nhưng trên trang giấy ấy, không có một lời nói dối nào."*

#### E_C (Một tờ giấy đến sau — A Page That Arrives Later):
- Miêu tả kỹ hơn không gian buổi hẹn sau: Băng ghế đá sân trường chiều vắng lặng, không còn tiếng loa trường, không còn áp lực kỷ yếu.
- An đưa tờ giấy phụ đính (supplement) in riêng cho Nhi. Nhi lật xem, im lặng rất lâu, rồi khẽ mỉm cười cất vào balo. Lời hứa năm xưa không cần phải khoe ra cho cả trường thấy, chỉ cần hai đứa trong cuộc hiểu là đủ.

---

## III. BẢNG TỔNG HỢP HÀNH ĐỘNG CỤ THỂ CHO TV1 (ACTIONABLE TASKS)

| Phân cảnh | Nội dung cần chỉnh sửa / bổ sung | Mục tiêu trải nghiệm |
|---|---|---|
| **S00 ➔ S01** | Viết đoạn dẫn chuyển cảnh (cầu nối âm thanh/chữ viết giải thích quãng hở 7 tháng). | Xóa bỏ cảm giác đứt gãy thời gian. |
| **S02** | Thêm tương tác lật mặt sau sơ đồ của Nhi và xem cận cảnh đệm cao su của kẹp. | Tăng tính trinh thám, "Show don't tell". |
| **S03** | Thêm công cụ soi chi tiết 3 ảnh (phát hiện góc máy của Nhi); thêm gợi ý nhập text cho nhánh C. | Tạo khoảnh khắc Eureka; giảm bỡ ngỡ khi dùng AI. |
| **S06 & S07** | Chuốt lại lời thoại của Hạ và Nhi, giảm thuật ngữ hợp đồng/consent, tăng cảm xúc học đường. | Người chơi đồng cảm với nhân vật hơn. |
| **S08 & S09** | Thêm miêu tả ánh sáng hoàng hôn và cử chỉ nhìn đồng hồ của Nhi khi gần 17:10. | Tạo nhịp điệu gấp gáp, áp lực thời gian tự nhiên. |
| **S10 & Endings** | Bổ sung câu đúc kết triết lý cho kết E_B và đoạn hậu truyện ngày bế giảng cho E_A, E_C. | Dư âm cảm xúc sâu sắc sau khi kết thúc game. |

---
*Tài liệu này được biên soạn để TV5 dùng làm căn cứ góp ý chính thức cho TV1 tại buổi họp nhóm và nghiệm thu nội dung.*
