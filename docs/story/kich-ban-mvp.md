# Kịch bản MVP - bản biên soạn TV1 0.1

Ngày 22/10/2026. Bản đọc tương ứng các JSON cảnh/thoại. Sự kiện theo PDF v4.1; câu nối, lựa chọn cụ thể và lịch bổ sung mẫu là phần biên soạn mới chờ kiểm nhận. Chưa playtest hoặc tích hợp Unity.

Điều kiện và effect là ghi chú sản xuất, không hiển thị nguyên mã cho người chơi. Không đọc mọi nhánh trong cùng một lượt. Các hub cho phép xem tùy thứ tự; các choice không hợp điều kiện bị ẩn/khóa có giải thích hợp lý.

## Mục lục

- [S00 - Đừng bấm vội](#s00)
- [S01 - Trang ở giữa](#s01)
- [S02 - Chiếc kẹp có cao su mới](#s02)
- [S03 - Ba ảnh khác nhau](#s03)
- [S04 - Chín giây ngoài ảnh](#s04)
- [S05 - Trang đẹp hơn](#s05)
- [S06 - Hai ký ức trên bàn](#s06)
- [S07 - Một lời mời cụ thể](#s07)
- [S08 - Mang gì ra sân](#s08)
- [S09_A - Cùng chụp một tấm mới](#s09-a)
- [S09_B - Sân còn ba người](#s09-b)
- [S10 - Trang nào được giữ](#s10)
- [E_A - Tấm mới](#e-a)
- [E_B - Những gì được giữ lại](#e-b)
- [E_C - Một tờ giấy đến sau](#e-c)

<a id="s00"></a>
## S00 - Đừng bấm vội

Nguồn: v4.1 trang 4, 5, 19.

Buổi tập tháng Mười; không gán ngày cụ thể. Hai lần chụp hỏng là hiện vật, không màn thất bại.

### S00_MACHINE

An kéo dây ra khỏi lối đi. Điện thoại nghiêng trên thùng; Nhi đang cài giờ.

**NHI**: Khoan, máy đang trượt.
**HA**: Đừng kê bằng tập đề của Minh.
**MINH**: Cuối cùng cũng có người hỏi ý kiến tao.

**Lựa chọn `S00_BOX`: Kê lại thùng và giữ điện thoại**

**AN**: Để tao giữ. Mày cài tiếp đi.
**NHI**: Giữ bên này, đừng che camera.

Hệ quả: `[{"op": "set", "field": "opening_support", "value": "self"}]`.
Đi tiếp: `step:S00_POSE`.

**Lựa chọn `S00_CLAMP`: Giữ chân kẹp cho Nhi cài giờ**

**AN**: Chân kẹp tao giữ rồi.
**NHI**: Ừ. Ba giây thôi, đừng buông sớm.

Hệ quả: `[{"op": "set", "field": "opening_support", "value": "clamp"}]`.
Đi tiếp: `step:S00_POSE`.

**Lựa chọn `S00_ASK`: Nhờ Nhi kiểm kẹp, mình giữ dây**

**AN**: Mày xem kẹp được không? Tao giữ dây.
**NHI**: Được. Đừng kéo căng quá.

Hệ quả: `[{"op": "set", "field": "opening_support", "value": "ask"}]`.
Đi tiếp: `step:S00_POSE`.
### S00_POSE

Bảng chỉ sáng chữ HẸN. Cả ba rủ An thử cách đứng khác nhau.

**HA**: Hay cùng nhìn bên kia?
**MINH**: Hay đứng yên.
**NHI**: Mỗi đứa làm một chữ đi.

**Lựa chọn `S00_POSE1`: Giơ tay làm nét chữ HẸN**

**AN**: Tao nhận chữ dễ nhất.
**MINH**: Có chữ nào không cần giơ tay không?

Hệ quả: `[{"op": "set", "field": "opening_gesture", "value": "pose_1"}]`.
Đi tiếp: `step:S00_CAPTURE`.

**Lựa chọn `S00_POSE2`: Cùng nhìn sang một phía**

**HA**: Bên này. Minh, bên này cơ.
**MINH**: Tao nhìn rồi, tại mắt tao vậy.

Hệ quả: `[{"op": "set", "field": "opening_gesture", "value": "pose_2"}]`.
Đi tiếp: `step:S00_CAPTURE`.

**Lựa chọn `S00_POSE3`: Đứng bình thường, trêu Minh đừng nhìn pin**

**AN**: Lần này cấm nhìn pin.
**MINH**: Hết pin thì đừng nhìn tao.

Hệ quả: `[{"op": "set", "field": "opening_gesture", "value": "pose_3"}]`.
Đi tiếp: `step:S00_CAPTURE`.
### S00_CAPTURE

Đặt máy, bấm hẹn giờ, An chạy vào. Lần một đèn tắt; lần hai Minh cúi nhặt bìa.

**NHI**: Đủ bốn mới bấm.
**MINH**: Máy tự bấm.
**NHI**: Thì mày thương lượng với máy đi.

**Lựa chọn `S00_SHOOT`: Đặt lại giờ và chạy vào ảnh**

**NARRATOR**: Tiếng bíp. Hạ vẫn đang cười khi máy bấm lần thứ ba.
**HA**: Lưu cả hai tấm trước đi.
**MINH**: Tấm tao cúi xuống ấy hả?
**NHI**: Nhất là tấm đấy.

Hệ quả: `[{"op": "set", "field": "opening_photos_created", "value": true}, {"op": "add_unique", "field": "evidence_seen", "value": "PHOTO_S00_GOOD"}, {"op": "add_unique", "field": "evidence_seen", "value": "PHOTO_S00_BAD_01"}, {"op": "add_unique", "field": "evidence_seen", "value": "PHOTO_S00_BAD_02"}]`.
Đi tiếp: `step:S00_ALBUM`.

Dựng/kiểm: Cho mở cả ba ảnh ngay; luôn đủ bốn trong ảnh gốc.
### S00_ALBUM

Ba thumbnail nằm cạnh nhau. Người chơi chọn tên album ngắn.


**Lựa chọn `S00_ALBUM1`: Một lần đủ bốn**


Hệ quả: `[{"op": "set", "field": "album_name", "value": "Một lần đủ bốn"}]`.
Đi tiếp: `step:S00_DIAGRAM`.

**Lựa chọn `S00_ALBUM2`: Đèn chưa chịu sáng**


Hệ quả: `[{"op": "set", "field": "album_name", "value": "Đèn chưa chịu sáng"}]`.
Đi tiếp: `step:S00_DIAGRAM`.

**Lựa chọn `S00_ALBUM3`: Minh đừng nhìn pin**


Hệ quả: `[{"op": "set", "field": "album_name", "value": "Minh đừng nhìn pin"}]`.
Đi tiếp: `step:S00_DIAGRAM`.
### S00_DIAGRAM

Nhi vẽ hai ô, bốn tên, dán tia chớp. Ô phải để trống.

**NHI**: Tấm này để đây. Trước khi ra trường chụp lại, đúng chỗ này.
**AN**: Lưu rồi. Cả hai tấm hỏng.
**MINH**: Cuộn băng ai cầm về?
**NARRATOR**: Không ai trả lời. Nhi kéo sơ đồ khỏi cốc nước rồi cất vào túi bìa.

**Lựa chọn `S00_KEEP`: Cất sơ đồ cùng album**


Hệ quả: `[{"op": "add_unique", "field": "evidence_seen", "value": "PROP_DIAGRAM"}, {"op": "add_unique", "field": "facts_discovered", "value": "F01"}, {"op": "add_unique", "field": "facts_discovered", "value": "F02"}]`.
Đi tiếp: `scene:S01`.

<a id="s01"></a>
## S01 - Trang ở giữa

Nguồn: v4.1 trang 5, 15.

Cắt từ ảnh đủ bốn sang bản in thử 16/18. Không mở bằng hồ sơ nhân vật. Minh giải thích ngay.

### S01_OPEN

07/05/2026, 10:00. Tiếng quạt, lật giấy, bạn lớp khác cười ngoài cửa.

**HA**: Trang 17 đâu?
**MINH**: Tao chuyển sang mục tạm giữ. Mày bảo bỏ mà.
**AN**: Tao nói câu chú thích.
**MINH**: Trong tin nhắn có chữ chú thích đâu.

**Lựa chọn `S01_CHAT`: Mở tin nhắn tối hôm trước**

**NARRATOR**: Minh: “Trang chưa chốt, bỏ khỏi bản thử nhé?” Dưới đó là câu trả lời của An: “Ừ bỏ đi, mai tính”.
**MINH**: Bản cũ tao vẫn giữ. Không mất file.

Hệ quả: `[{"op": "add_unique", "field": "evidence_seen", "value": "PROP_CHAT_REMOVE"}, {"op": "add_unique", "field": "evidence_seen", "value": "PROP_PROOF_16_18"}, {"op": "add_unique", "field": "facts_discovered", "value": "F03"}]`.
Đi tiếp: `step:S01_COMPARE`.
### S01_COMPARE

Ảnh buổi tập cạnh ảnh hội chợ ba người; trên bàn là bố cục ba vị trí cho ngày mai.

**AN**: Mai chụp…
**NHI**: Tụi mày cứ chụp.
**NARRATOR**: Nhi đi tiếp cùng Phương, ôm thùng đồ. Hạ kéo ảnh hội chợ lại gần.
**HA**: Bản này chưa ai chốt. Nhi chưa nhận lời ảnh mới.

**Lựa chọn `S01_FIND`: Mang sơ đồ xuống chỗ kiểm đồ**


Hệ quả: `[{"op": "add_unique", "field": "evidence_seen", "value": "PROP_THREE_LAYOUT"}]`.
Đi tiếp: `scene:S02`.

<a id="s02"></a>
## S02 - Chiếc kẹp có cao su mới

Nguồn: v4.1 trang 5, 6, 15.

Hub xem sơ đồ/kẹp/lịch tùy thứ tự; thông tin cốt lõi không cần giúp đủ lượt. Hỏi trực tiếp được đáp ngay.

### S02_HUB

Nhi và Phương kiểm đồ. An có thể chạm từng vật hoặc hỏi trong lúc họ làm.

**PHUONG**: Hai cuộn dây, một kẹp. Cái nhỏ ở đâu nhỉ?
**NHI**: Trong túi bên hông. An, tránh cái thùng giúp tao.

**Lựa chọn `S02_HELP`: Giữ thùng và giúp kiểm đồ**

Điều kiện: `{"field": "nhi_contact", "op": "ne", "value": "help"}`.

**AN**: Hai cuộn? Cái này nữa là ba.
**NHI**: Cái đấy dây hỏng. Để sang bên.
**AN**: Ừ, tao giữ thùng.

Hệ quả: `[{"op": "set", "field": "nhi_contact", "value": "help"}]`.
Đi tiếp: `step:S02_HUB`.

**Lựa chọn `S02_DIAGRAM`: Xem lại sơ đồ hai ảnh**

Điều kiện: `{"field": "early_nhi_understanding", "op": "eq", "value": false}`.

**AN**: Sơ đồ này mày vẽ nhỉ?
**NHI**: Ừ. Bản bây giờ có giống cái đó đâu.

Hệ quả: `[{"op": "set", "field": "early_nhi_understanding", "value": true}, {"op": "add_unique", "field": "facts_discovered", "value": "F01"}]`.
Đi tiếp: `step:S02_HUB`.

**Lựa chọn `S02_CLAMP`: Thử chiếc kẹp vừa sửa**

Điều kiện: `{"not": {"field": "evidence_seen", "op": "contains", "value": "PROP_CLAMP"}}`.

**AN**: Mày sửa cái này à?
**NHI**: Nó trượt.
**AN**: Mai dùng được không?
**NHI**: Thì thử đi.

Hệ quả: `[{"op": "add_unique", "field": "evidence_seen", "value": "PROP_CLAMP"}]`.
Đi tiếp: `step:S02_HUB`.

**Lựa chọn `S02_CLOCK`: Đọc lịch trên thùng**

Điều kiện: `{"not": {"field": "evidence_seen", "op": "contains", "value": "PROP_SCHEDULE"}}`.

**NARRATOR**: 16:45 chụp; 17:10 Nhi đi; 17:30 tháo quầy. Sáng thứ Bảy giao file lúc 10:00.
**NHI**: Lịch tao nhắn từ trước rồi. Mai không chờ thêm được đâu.

Hệ quả: `[{"op": "add_unique", "field": "evidence_seen", "value": "PROP_SCHEDULE"}]`.
Đi tiếp: `step:S02_HUB`.

**Lựa chọn `S02_ASK_CAMERA`: Hỏi Nhi ai cầm máy hôm hội chợ**

Điều kiện: `{"field": "knows_handoff", "op": "eq", "value": false}`.

**AN**: Tấm ba đứa hôm hội chợ, ai chụp ấy nhỉ?
**NHI**: Tao. Mày đưa máy cho tao mà. Bảo một tấm thôi, lát chụp đủ.
**AN**: Rồi tao quên.
**NHI**: Rồi đông khách. Tao cũng không nhắc nữa.

Hệ quả: `[{"op": "set", "field": "knows_handoff", "value": true}, {"op": "add_unique", "field": "evidence_seen", "value": "TESTIMONY_NHI"}, {"op": "add_unique", "field": "facts_discovered", "value": "F06"}]`.
Đi tiếp: `step:S02_HUB`.

**Lựa chọn `S02_CAPTION`: Hỏi về lời nhắc đừng in**

Điều kiện: `{"not": {"field": "evidence_seen", "op": "contains", "value": "PROP_CAPTION_OLD"}}`.

**NHI**: Tao bảo sửa lời tao, không bảo xóa cả trang. Câu kia tao đâu nói thế.
**AN**: Còn tấm buổi tập?
**NHI**: Mày định dùng thế nào thì cho tao xem.

Hệ quả: `[{"op": "add_unique", "field": "evidence_seen", "value": "PROP_CAPTION_OLD"}]`.
Đi tiếp: `step:S02_HUB`.

**Lựa chọn `S02_LEAVE`: Mang lịch về bàn ảnh**

Điều kiện: `{"all": [{"field": "evidence_seen", "op": "contains", "value": "PROP_CLAMP"}, {"field": "evidence_seen", "op": "contains", "value": "PROP_SCHEDULE"}, {"field": "evidence_seen", "op": "contains", "value": "PROP_CAPTION_OLD"}]}`.

**NARRATOR**: An ghim lịch cạnh album. Chiếc kẹp được đặt riêng trong ngăn đồ sẽ mang ra sân.

Không tự đổi trạng thái ngoài chuyển bước.
Đi tiếp: `scene:S03`.

<a id="s03"></a>
## S03 - Ba ảnh khác nhau

Nguồn: v4.1 trang 6, 7, 17.

Thí nghiệm A/B/C duy nhất. B/C dùng responses riêng và cùng luật với các nút. Hub không tự lộ video.

### S03_DESK

Hạ mở ảnh gốc, kéo chúng cạnh nhau. Có thể phóng to, kéo rộng và hỏi bằng nút hoặc câu nhập.

**HA**: Ảnh gốc ở đây, xem được. Tao chọn 0443 vì cả ba đang cười, bảng cũng sáng.
**AN**: Cho tao xem cả mấy tấm trước.

**Lựa chọn `S03_LOOK_0441`: Mở ảnh 0441**

Điều kiện: `{"not": {"field": "evidence_seen", "op": "contains", "value": "PHOTO_0441"}}`.

**NARRATOR**: Đèn chưa sáng. Nhi ở mép, tay cầm dây. Chưa ai nhìn vào máy.

Hệ quả: `[{"op": "add_unique", "field": "evidence_seen", "value": "PHOTO_0441"}]`.
Đi tiếp: `step:S03_DESK`.

**Lựa chọn `S03_LOOK_0442`: Mở ảnh 0442**

Điều kiện: `{"not": {"field": "evidence_seen", "op": "contains", "value": "PHOTO_0442"}}`.

**NARRATOR**: Đèn đã sáng. Ổ chia nằm trên ghế đỏ. Nhi đang nhìn sang An.

Hệ quả: `[{"op": "add_unique", "field": "evidence_seen", "value": "PHOTO_0442"}]`.
Đi tiếp: `step:S03_DESK`.

**Lựa chọn `S03_LOOK_0443`: Mở ảnh 0443**

Điều kiện: `{"not": {"field": "evidence_seen", "op": "contains", "value": "PHOTO_0443"}}`.

**NARRATOR**: Kéo tới mép ảnh. Vẫn chỉ có An, Hạ và Minh. Không có phần hình nào giấu Nhi bên ngoài.

Hệ quả: `[{"op": "add_unique", "field": "evidence_seen", "value": "PHOTO_0443"}]`.
Đi tiếp: `step:S03_DESK`.

**Lựa chọn `S03_EMPTY`: So cận chiếc kẹp trống**

Điều kiện: `{"not": {"field": "evidence_seen", "op": "contains", "value": "PHOTO_CLAMP_EMPTY"}}`.

**AN**: Hôm tập mình dùng hẹn giờ được mà.
**HA**: Ừ. Kẹp vẫn ở đó.

Hệ quả: `[{"op": "add_unique", "field": "evidence_seen", "value": "PHOTO_CLAMP_EMPTY"}, {"op": "add_unique", "field": "facts_discovered", "value": "F04"}, {"op": "add_unique", "field": "facts_discovered", "value": "F05"}]`.
Đi tiếp: `step:S03_DESK`.

**Lựa chọn `S03_WHY`: Hỏi Hạ vì sao giữ 0443**

**HA**: Tấm này vẫn là chuyện thật của tụi tao. Không phải cứ thiếu một người là hôm đó không ai được vui.

Không tự đổi trạng thái ngoài chuyển bước.
Đi tiếp: `step:S03_REASON`.

**Lựa chọn `S03_SOURCE`: Tìm đoạn quay của Vy**

Điều kiện: `{"all": [{"field": "evidence_seen", "op": "contains", "value": "PHOTO_0441"}, {"field": "evidence_seen", "op": "contains", "value": "PHOTO_0442"}, {"field": "evidence_seen", "op": "contains", "value": "PHOTO_0443"}, {"field": "evidence_seen", "op": "contains", "value": "PHOTO_CLAMP_EMPTY"}]}`.

**HA**: Vy có quay ở quầy bên cạnh. Nếu em ấy cho xem thì mình đối chiếu.

Không tự đổi trạng thái ngoài chuyển bước.
Đi tiếp: `scene:S04`.
### S03_REASON

Ảnh 0443 đứng cạnh sơ đồ; câu hỏi không tự biến thành lời buộc tội được xác nhận.


**Lựa chọn `S03_REASON_BACK`: Trở lại ba ảnh**


Không tự đổi trạng thái ngoài chuyển bước.
Đi tiếp: `step:S03_DESK`.

<a id="s04"></a>
## S04 - Chín giây ngoài ảnh

Nguồn: v4.1 trang 6, 7, 15.

Animatic/video có âm, phụ đề, thanh kéo. Mở cả nguồn khi được cho xem; không khóa tới cuối game.

### S04_SOURCE

Vy cho mượn clip để kiểm chứng nội bộ.

**VY**: Các anh chị xem ở đây nhé. Nếu định dùng ở đâu khác thì phải hỏi lại những người trong hình.
**HA**: Ừ, chỉ xem để đối chiếu.

**Lựa chọn `S04_WATCH`: Xem đoạn trước ảnh 0443 cùng Hạ**

**NARRATOR**: Khung trước: Nhi tới muộn, túi dây nối còn trên vai. An trả lời cụt. Nhi nói gắt rồi quay sang chuyển ổ chia khỏi chỗ ướt.
**MINH**: Hôm đó hẹn 16:15, nó đến 16:31.
**NARRATOR**: Khung gần lúc chụp: Minh đưa tay về phía kẹp. An nhấc điện thoại đưa cho Nhi.
**AN**: Mày chụp giúp cái. Một tấm thôi.
**NARRATOR**: Nhi chỉ chiếc kẹp.
**AN**: Lát chụp đủ. Nhanh lên, có người tới rồi.
**NARRATOR**: Tiếng đếm rồi khách gọi. Máy quay theo quầy, không có tấm đủ bốn trong phần tư liệu này.
**NHI**: Ảnh thì khỏi cần mặt tao. Nhớ ghi phần tao làm là được.
**HA**: Ừ. Nhưng tụi mình đâu chụp nữa.

Hệ quả: `[{"op": "set", "field": "saw_video", "value": true}, {"op": "set", "field": "knows_handoff", "value": true}, {"op": "add_unique", "field": "evidence_seen", "value": "VIDEO_VY"}, {"op": "add_unique", "field": "facts_discovered", "value": "F06"}, {"op": "add_unique", "field": "facts_discovered", "value": "F07"}, {"op": "add_unique", "field": "facts_discovered", "value": "F08"}, {"op": "add_unique", "field": "ha_shared_facts", "value": "F06"}, {"op": "add_unique", "field": "ha_shared_facts", "value": "F07"}, {"op": "add_unique", "field": "ha_shared_facts", "value": "F08"}, {"op": "grant", "asset_id": "VIDEO_VY", "subject_id": "VY", "scope": "internal_review", "status": "yes", "source_event_id": "S04_WATCH"}]`.
Đi tiếp: `step:S04_AFTER`.

Dựng/kiểm: Giữ thứ tự chứng cứ, khoảng lặng sau lời lát; cho tua/lặp. Đồng ý kiểm chứng không là phát hành.

**Lựa chọn `S04_TESTIMONY`: Giữ lời Nhi đã kể, chưa xem clip**

Điều kiện: `{"field": "knows_handoff", "op": "eq", "value": true}`.

**AN**: Tao hỏi rồi. Nhi nói chính tao đưa máy, hẹn lát chụp đủ.
**HA**: Ừ, tao nhớ đoạn đưa máy. Nguyên câu còn lại tao không chắc.

Hệ quả: `[{"op": "add_unique", "field": "ha_shared_facts", "value": "F06"}]`.
Đi tiếp: `step:S04_AFTER`.

Dựng/kiểm: Không giả vờ chưa biết chỉ vì chưa xem clip; nguyên câu hai vế vẫn chưa mở cho Hạ.
### S04_AFTER

Ảnh vui vẫn trên bàn. Sổ chỉ ghi điều có nguồn, không ghi ý đồ của ai.

**HA**: Mày muốn sửa trang thế nào? Nói bằng một bản nháp đi.

**Lựa chọn `S04_DRAFT`: Mở các mẫu trang**


Không tự đổi trạng thái ngoài chuyển bước.
Đi tiếp: `scene:S05`.

<a id="s05"></a>
## S05 - Trang đẹp hơn

Nguồn: v4.1 trang 7, 17, 19.

Ba mẫu đều đẹp, không tô đúng/sai. Chỉ là nháp riêng, chưa xác nhận quyền in. Không có gửi nhóm lớp mặc định.

### S05_TEMPLATE

Ba preview đặt ngang hàng; ảnh dùng trong preview được đánh dấu nháp riêng.

**HA**: Mày thử đặt ảnh lên đây. Muốn sửa chỗ nào thì chỉ rõ.
**MINH**: Chốt xong gửi tao bản xem. Đừng gửi mỗi tên file.

**Lựa chọn `S05_TYPE_TRIBUTE`: Một trang cảm ơn với ảnh hậu trường**

**AN**: Hay làm một trang cảm ơn Nhi.
**HA**: Được. Tao thử ảnh lớn, chữ ngắn ở dưới.

Hệ quả: `[{"op": "set", "field": "draft_type", "value": "tribute"}, {"op": "add_unique", "field": "draft_seen_by", "value": "AN"}]`.
Đi tiếp: `step:S05_RECIPIENT`.

**Lựa chọn `S05_TYPE_PAIRED`: Hai ảnh trước và bây giờ**

**AN**: Tao muốn giữ hai ô như sơ đồ.
**HA**: Ô mới chưa có ảnh. Cứ để bản nháp, đừng lấy 0443 gọi là đủ bốn.

Hệ quả: `[{"op": "set", "field": "draft_type", "value": "paired"}, {"op": "add_unique", "field": "draft_seen_by", "value": "AN"}]`.
Đi tiếp: `step:S05_RECIPIENT`.

**Lựa chọn `S05_TYPE_MEMORY`: Ảnh hỏng và sơ đồ trong trang ký ức**

**AN**: Chưa có ảnh mới thì làm từ những tấm đang giữ.
**HA**: Ừ. Nhưng chọn ít thôi, để còn nhìn được mặt.

Hệ quả: `[{"op": "set", "field": "draft_type", "value": "memory"}, {"op": "add_unique", "field": "draft_seen_by", "value": "AN"}]`.
Đi tiếp: `step:S05_RECIPIENT`.
### S05_RECIPIENT

Preview luôn hiện loại nháp, nội dung, phiên bản 1 và người nhận. Người chơi bấm xác nhận gửi.


**Lựa chọn `S05_SEND_NHI`: Gửi riêng Nhi xem trước**

**AN**: Tao gửi nháp riêng. Mày xem lúc rảnh, chưa đưa đi in.
**NARRATOR**: Tin đã gửi. Nhi đang trong lớp; phản hồi sẽ đến khi cô có thể xem.

Hệ quả: `[{"op": "set", "field": "draft_sent_to", "value": "nhi"}]`.
Đi tiếp: `scene:S06`.

**Lựa chọn `S05_SEND_GROUP_TRIBUTE`: Gửi nhóm bốn người**

Điều kiện: `{"field": "draft_type", "op": "eq", "value": "tribute"}`.

**HA**: Mẫu này in được đấy. Tao còn giấy màu, cắt thử cái bảng nhé.
**MINH**: Vậy mai ai cầm pin?
**NARRATOR**: Hạ lấy giấy, đặt dòng Cảm ơn Nhi lên tấm bảng còn trắng.

Hệ quả: `[{"op": "set", "field": "draft_sent_to", "value": "group_four"}, {"op": "set", "field": "tribute_board_prepared", "value": true}, {"op": "set", "field": "draft_retracted", "value": "no"}, {"op": "add_unique", "field": "draft_seen_by", "value": "HA"}, {"op": "add_unique", "field": "draft_seen_by", "value": "MINH"}]`.
Đi tiếp: `scene:S06`.

**Lựa chọn `S05_SEND_GROUP_OTHER`: Gửi nhóm bốn người**

Điều kiện: `{"field": "draft_type", "op": "ne", "value": "tribute"}`.

**HA**: Tao mở được rồi. Giữ bản này để bàn, chưa chốt.
**MINH**: Nhớ hỏi người có trong ảnh trước khi gửi in.

Hệ quả: `[{"op": "set", "field": "draft_sent_to", "value": "group_four"}, {"op": "add_unique", "field": "draft_seen_by", "value": "HA"}, {"op": "add_unique", "field": "draft_seen_by", "value": "MINH"}]`.
Đi tiếp: `scene:S06`.

**Lựa chọn `S05_HOLD`: Giữ trên máy, mang ra nói trực tiếp**

**AN**: Để tao mang đi hỏi trước. Chưa gửi nhóm.
**HA**: Ừ. Mày nhớ nói rõ đây là nháp.

Hệ quả: `[{"op": "set", "field": "draft_sent_to", "value": "held"}]`.
Đi tiếp: `scene:S06`.

<a id="s06"></a>
## S06 - Hai ký ức trên bàn

Nguồn: v4.1 trang 8, 18.

Nói và làm là hai bước. Một câu nhận lỗi không tự đổi ha_support. Không khóa nguồn khi chỉ bàn giao.

### S06_TALK

Hạ đặt 0443 bên cạnh sơ đồ hai ô, chưa xóa tấm nào.

**HA**: Mày nói như cả tấm này phải bỏ đi ấy.
**HA**: Hôm đó tụi mình cười thật. Mày muốn tao xóa hết à?

**Lựa chọn `S06_OWN`: Nói rõ mình đã đưa máy cho Nhi**

**AN**: Tao đưa máy cho Nhi. Phần đó tao phải nói.
**HA**: Ừ. Nhưng tấm này thì sao?

Hệ quả: `[{"op": "set", "field": "an_owned_camera_choice", "value": "yes"}]`.
Đi tiếp: `step:S06_PLACE`.

**Lựa chọn `S06_BLAME`: Bảo Hạ đã chọn ảnh làm thiếu Nhi**

**AN**: Mày chọn tấm không có nó thì trang mới thành thế này.
**HA**: Ảnh gốc ở đây. Đừng bắt tao nhận hết chuyện mày đã quên.

Hệ quả: `[{"op": "set", "field": "an_owned_camera_choice", "value": "deflected"}]`.
Đi tiếp: `step:S06_PLACE`.

**Lựa chọn `S06_COMPARE`: Chưa đáp, đặt hai ký ức cạnh nhau**

**HA**: Mày muốn giữ cái nào? Hay cả hai, nhưng đừng gọi chúng là cùng một chuyện.

Hệ quả: `[{"op": "set", "field": "an_owned_camera_choice", "value": "unresolved"}]`.
Đi tiếp: `step:S06_PLACE`.
### S06_PLACE

Kéo 0443 vào album riêng hoặc thử góc trang có caption đúng. Chỉ là thỏa thuận làm nháp; quyền in vẫn hỏi sau.


**Lựa chọn `S06_KEEP_AGREE`: Thử giữ 0443; An tự viết lại phần chữ**

Điều kiện: `{"field": "an_owned_camera_choice", "op": "ne", "value": "deflected"}`.

**AN**: Để nó như ảnh hội chợ ba người. Tao viết chữ, gửi mày xem lại.
**HA**: Được. Tao thử bố cục một buổi. Nhưng đừng gửi mỗi chữ bỏ nữa.

Hệ quả: `[{"op": "set", "field": "photo0443_placement", "value": "page17"}, {"op": "set", "field": "ha_support", "value": "collaborate"}]`.
Đi tiếp: `scene:S07`.

**Lựa chọn `S06_PRIVATE_AGREE`: Giữ 0443 ở album; nhờ Hạ thử trang hai ảnh**

Điều kiện: `{"field": "an_owned_camera_choice", "op": "ne", "value": "deflected"}`.

**AN**: Ảnh này vẫn giữ. Trang 17 thử nguồn buổi tập trước, phần chữ để tao.
**HA**: Tao gửi cả hai bản. Mình thử rồi hỏi Nhi.

Hệ quả: `[{"op": "set", "field": "photo0443_placement", "value": "private_album"}, {"op": "set", "field": "ha_support", "value": "collaborate"}]`.
Đi tiếp: `scene:S07`.

**Lựa chọn `S06_HANDOVER`: Nhận file gốc và tự dàn trang**

**HA**: Tao gửi đủ file. Mẫu đơn giản ở đây, phần còn lại mày làm nhé.
**AN**: Ừ. Tối tao gửi bản chốt để mọi người xem.

Hệ quả: `[{"op": "set", "field": "photo0443_placement", "value": "private_album"}, {"op": "set", "field": "ha_support", "value": "handover_only"}]`.
Đi tiếp: `scene:S07`.

**Lựa chọn `S06_KEEP_HANDOVER`: Giữ ảnh để xin dùng sau; nhận file và tự làm**

Điều kiện: `{"field": "an_owned_camera_choice", "op": "eq", "value": "deflected"}`.

**HA**: Ảnh thì tao đưa. Nhưng tao không dàn thêm hôm nay. Muốn dùng, gửi bản chữ cuối cho tao xem.

Hệ quả: `[{"op": "set", "field": "photo0443_placement", "value": "page17"}, {"op": "set", "field": "ha_support", "value": "handover_only"}]`.
Đi tiếp: `scene:S07`.

<a id="s07"></a>
## S07 - Một lời mời cụ thể

Nguồn: v4.1 trang 7, 8, 9, 18.

Phản hồi nháp đi trước lời mời. Câu trả lời NPC là sự kiện, không phải choice tự đặt accepted. Quyền ảnh cũ hỏi riêng.

### S07_SHOW

Nhi ra khỏi lớp. Nháp đã gửi mới được mở khi cô xem; bản giữ máy được An đưa trực tiếp.

**AN**: Đây là bản nháp tao muốn hỏi mày, chưa in.
**NARRATOR**: Nhi nhìn màn hình, kéo về tấm buổi tập.

**Lựa chọn `S07_SHOW_TRIBUTE`: Đưa bản tôn vinh cho Nhi xem**

Điều kiện: `{"field": "draft_type", "op": "eq", "value": "tribute"}`.

**NHI**: Mai khỏi làm bảng này.
**AN**: Mày muốn sửa câu nào?
**NHI**: Bỏ đoạn cảm ơn. Tao không nhận giải.

Hệ quả: `[{"op": "add_unique", "field": "draft_seen_by", "value": "NHI"}]`.
Đi tiếp: `step:S07_REACT`.

**Lựa chọn `S07_SHOW_OTHER`: Cùng xem ảnh trong bản nháp**

Điều kiện: `{"field": "draft_type", "op": "ne", "value": "tribute"}`.

**NHI**: Tấm hai ảnh ấy. Tao vẽ cái đó.
**AN**: Tao vẫn giữ sơ đồ.
**NHI**: Ảnh mới ai chụp?

Hệ quả: `[{"op": "add_unique", "field": "draft_seen_by", "value": "NHI"}]`.
Đi tiếp: `step:S07_INVITE`.
### S07_REACT

Không có cảnh giận bắt buộc khi đã chọn hướng khác. Nếu đã có bảng, việc gỡ sẽ là hành động riêng.


**Lựa chọn `S07_STOP_SENT`: Dừng bản cũ và báo lại những người đã xem**

Điều kiện: `{"field": "draft_sent_to", "op": "eq", "value": "group_four"}`.

**AN**: Bản cảm ơn dừng nhé. Hạ, Minh đừng chuẩn bị theo bản đó nữa. Tao đổi sang trang ký ức.
**HA**: Tao đọc rồi. Cái bảng đang ở trong túi, ra sân nhớ lấy ra cất.
**MINH**: Ừ. Cho tao bản mới sau.

Hệ quả: `[{"op": "set", "field": "draft_type", "value": "memory"}, {"op": "set", "field": "draft_version", "value": 2}, {"op": "set", "field": "draft_retracted", "value": "yes"}, {"op": "add_unique", "field": "retraction_notified_to", "value": "NHI"}, {"op": "add_unique", "field": "retraction_notified_to", "value": "HA"}, {"op": "add_unique", "field": "retraction_notified_to", "value": "MINH"}]`.
Đi tiếp: `step:S07_INVITE`.

**Lựa chọn `S07_STOP_PRIVATE`: Dừng nháp riêng và đổi sang hai ảnh**

Điều kiện: `{"field": "draft_sent_to", "op": "ne", "value": "group_four"}`.

**AN**: Ừ, tao dừng bản này. Để tao làm theo hai ô rồi hỏi mọi người.
**NHI**: Cho tao xem tấm cũ trước.

Hệ quả: `[{"op": "set", "field": "draft_type", "value": "paired"}, {"op": "set", "field": "draft_version", "value": 2}, {"op": "set", "field": "draft_retracted", "value": "yes"}, {"op": "add_unique", "field": "retraction_notified_to", "value": "NHI"}]`.
Đi tiếp: `step:S07_INVITE`.

**Lựa chọn `S07_INSIST`: Giải thích cả nhóm làm vậy vì Nhi**

**AN**: Nhưng mọi người đang làm vì mày mà.
**NHI**: Vậy tụi mày làm đi. Mai tao không ra chụp.

Hệ quả: `[{"op": "set", "field": "invite_pressure", "value": true}, {"op": "set", "field": "nhi_invite_status", "value": "declined"}]`.
Đi tiếp: `step:S07_INVITE`.

**Lựa chọn `S07_DISCUSS_AT_FIELD`: Ghi lại ý Nhi, để bàn cách gỡ với nhóm tại sân**

**AN**: Tao biết mày không muốn bảng đó. Tao sẽ nói rõ ở sân, chưa dùng vào ảnh.
**NHI**: Đừng bắt tao cầm nó.

Hệ quả: `[{"op": "set", "field": "invite_pressure", "value": false}]`.
Đi tiếp: `step:S07_INVITE`.
### S07_INVITE

Lời mời nêu rõ chụp cùng, ai cầm máy và giờ; lựa chọn không là điểm tình bạn.


**Lựa chọn `S07_INVITE_CLEAR`: Mời 16:45, máy hẹn giờ, không ép tư thế cũ**

Điều kiện: `{"field": "invite_pressure", "op": "eq", "value": false}`.

**AN**: Mai 16:45 chụp cùng nhé? Tao lo máy hẹn giờ, không cần đứng y hệt tấm cũ.
**NHI**: Được. 17:10 tao phải đi, chưa xong tao vẫn đi đấy.
**AN**: Ừ. Mày đến là vào chụp, tao lo máy.

Hệ quả: `[{"op": "set", "field": "nhi_invite_status", "value": "accepted"}]`.
Đi tiếp: `step:S07_OLD_RIGHTS`.

**Lựa chọn `S07_ASK_AVAILABILITY`: Hỏi Nhi có thu xếp được ngày mai không**

Điều kiện: `{"field": "invite_pressure", "op": "eq", "value": false}`.

**AN**: Mai mày có thu xếp ra sân được không? Không kịp thì mình tính trang từ ảnh cũ.
**NHI**: Mai tao còn trả đồ với Phương trước giờ kiểm tra. Tao không ra được.
**AN**: Ừ. Tao không chờ mày ở sân.

Hệ quả: `[{"op": "set", "field": "nhi_invite_status", "value": "declined"}]`.
Đi tiếp: `step:S07_OLD_RIGHTS`.

**Lựa chọn `S07_INVITE_SHORT`: Gửi lời mời rồi để Nhi xem lịch trả lời sau**

Điều kiện: `{"field": "invite_pressure", "op": "eq", "value": false}`.

**AN**: Mai ra chụp nhé? Tao gửi giờ trong nhóm, xem lịch rồi báo tao.
**NHI**: Để tao xem đã.
**NARRATOR**: Chưa có câu nhận lời. An không đánh dấu đã hẹn xong.

Hệ quả: `[{"op": "set", "field": "nhi_invite_status", "value": "pending"}]`.
Đi tiếp: `step:S07_OLD_RIGHTS`.

**Lựa chọn `S07_ACCEPT_DECLINE`: Dừng chuyện mời và hỏi phần ảnh cũ**

Điều kiện: `{"field": "invite_pressure", "op": "eq", "value": true}`.

**AN**: Ừ. Tao không nhắn mày ra nữa. Còn phần ảnh cũ, cho tao hỏi rõ.

Không tự đổi trạng thái ngoài chuyển bước.
Đi tiếp: `step:S07_OLD_RIGHTS`.
### S07_OLD_RIGHTS

An mở đúng PHOTO_S00_GOOD. Đây là câu hỏi quyền ảnh cũ, không là lời mời mới.

**AN**: Tấm buổi tập này, tao để vào trang 17 được không? Chỉ ảnh và ngày, không câu gán kia.
**NHI**: Được, tấm này dùng được. Gửi tao bản chốt nhé.

**Lựa chọn `S07_OLD_OK`: Ghi nhận quyền dùng đúng tấm vừa xem**


Hệ quả: `[{"op": "grant", "asset_id": "PHOTO_S00_GOOD", "subject_id": "NHI", "scope": "yearbook_print", "status": "yes", "source_event_id": "S07_OLD_OK"}]`.
Đi tiếp: `scene:S08`.

Dựng/kiểm: Chưa cấp quyền hai ảnh hỏng, sơ đồ, clip hoặc mọi ảnh tương lai.

<a id="s08"></a>
## S08 - Mang gì ra sân

Nguồn: v4.1 trang 9, 14, 15, 18.

Cắt cảnh sang 08/05. Bàn giao cụ thể trước sân; thời lượng thao tác là giả định nhịp để thử, không số nguồn.

### S08_ASSIGN

Sáng thứ Sáu, trước 16:00. Lịch tháo quầy vẫn dán ở đó.

**MINH**: Tối tao phải ôn. Pin tao mang, còn ai kiểm file?
**HA**: File gốc tao gửi rồi. Ai giữ máy thì ghi tên vào túi.

**Lựa chọn `S08_ASSIGN`: An nhận máy và xuất file; nhờ Minh bàn giao pin**

**AN**: Máy và bản xuất để tao. Minh đưa pin trước, tối kiểm giúp một lượt nếu kịp thôi.
**MINH**: Ừ. Tao chỉ ngồi tới giờ ôn.
**NARRATOR**: An viết tên mình lên băng dính dán túi máy.

Hệ quả: `[{"op": "set", "field": "support_plan", "value": "assigned"}, {"op": "set", "field": "export_owner", "value": "AN"}]`.
Đi tiếp: `step:S08_PRINT`.

**Lựa chọn `S08_ASSUME`: Chưa phân lại, mang file và pin đã được bàn giao**

**AN**: Tao tưởng mọi người lo như cũ.
**MINH**: Tao đưa pin thôi, tối mày tự xuất nhé. Bản hướng dẫn tao gửi rồi.
**AN**: Ừ, phần còn lại để tao.

Hệ quả: `[{"op": "set", "field": "support_plan", "value": "assumed"}, {"op": "set", "field": "export_owner", "value": "AN"}]`.
Đi tiếp: `step:S08_PRINT`.
### S08_PRINT

Minh chỉ lịch thường và lịch giữ chỗ. Không có thao tác tự gửi in.


**Lựa chọn `S08_NORMAL`: Giữ lịch thường: giao file 09/05 lúc 10:00**

**AN**: Cứ lịch thường. Thiếu ảnh mới thì tao làm từ ảnh cũ.
**MINH**: Ừ. Sách chiều thứ Hai.

Hệ quả: `[{"op": "set", "field": "print_slot", "value": "normal"}]`.
Đi tiếp: `step:S08_CHECK`.

**Lựa chọn `S08_EXTEND`: Gọi giữ lịch dự phòng trước 17:00 thứ Sáu**

Điều kiện: `{"field": "courtyard_minute", "op": "lt", "value": 60}`.

**AN**: Nếu cần, bọn em giao trước 12:00 thứ Bảy được không?
**PRINTER**: Giữ chỗ được. Sách chuyển sang sáng thứ Ba, nhóm xác nhận nhé.
**AN**: Dạ, nhóm nhận lịch đó.

Hệ quả: `[{"op": "set", "field": "print_slot", "value": "extended"}, {"op": "set", "field": "extension_confirmed", "value": true}]`.
Đi tiếp: `step:S08_CHECK`.

Dựng/kiểm: PRINTER là vai âm/tin nhắn vận hành, không thêm nhân vật giải quyết xung đột.
### S08_CHECK

16:40 tại sân. Bấm thử kẹp/pin trước khi Nhi tới hoặc quyết định không tới.

**HA**: Kẹp còn trượt không?
**AN**: Để tao thử.
**MINH**: Đừng thử bằng cách thả tay luôn.

**Lựa chọn `S08_READY`: Lắp máy, kiểm kẹp và pin (5 phút)**

**NARRATOR**: An kê thêm miếng cao su, thử hẹn giờ. Máy đứng vững.
**AN**: Được rồi. Máy tự bấm, tao vào cùng.

Hệ quả: `[{"op": "set", "field": "equipment_ready", "value": "ready"}, {"op": "advance_minutes", "value": 5}]`.
Đi tiếp: `step:S08_ROUTE`.

**Lựa chọn `S08_REPAIR_LATER`: Mang đồ sang vị trí chụp, kiểm tiếp tại đó (5 phút)**

**NARRATOR**: Túi máy đủ đồ. Kẹp chưa thử xong; lúc chụp cần kê lại hoặc đổi chỗ.

Hệ quả: `[{"op": "set", "field": "equipment_ready", "value": "repairable"}, {"op": "advance_minutes", "value": 5}]`.
Đi tiếp: `step:S08_ROUTE`.
### S08_ROUTE

16:45. Chỉ nhận lời có sự kiện đến mới vào đường có Nhi.


**Lựa chọn `S08_ARRIVE`: Đón Nhi tại sân**

Điều kiện: `{"field": "nhi_invite_status", "op": "eq", "value": "accepted"}`.

**NARRATOR**: Nhi đặt cặp cạnh ghế, nhìn đồng hồ.
**NHI**: Tao tới rồi. Đến 17:10 thôi nhé.

Hệ quả: `[{"op": "set", "field": "attendance", "value": "present"}, {"op": "set", "field": "nhi_wants_capture", "value": true}]`.
Đi tiếp: `scene:S09_A`.

**Lựa chọn `S08_ABSENT`: Bắt đầu với những người đang có mặt**

Điều kiện: `{"field": "nhi_invite_status", "op": "ne", "value": "accepted"}`.

**NARRATOR**: Hạ đặt pin lên quầy. Tin nhắn không có câu nhận lời mới. Chỗ cạnh máy vẫn trống.

Hệ quả: `[{"op": "set", "field": "attendance", "value": "absent"}, {"op": "set", "field": "nhi_wants_capture", "value": false}]`.
Đi tiếp: `scene:S09_B`.

<a id="s09-a"></a>
## S09_A - Cùng chụp một tấm mới

Nguồn: v4.1 trang 10, 11, 18, 19.

Không ép ảnh trở lại nguyên khuôn. Nếu Nhi đi trước capture, chuyển S09_B. Gỡ bảng không tự xóa lịch sử.

### S09_A_BOARD

Kiểm đạo cụ trước dựng khung; chỉ hiện bảng nếu đã được chuẩn bị và chưa gỡ.


**Lựa chọn `S09_A_REMOVE_BOARD`: Lấy bảng cảm ơn khỏi chỗ chụp**

Điều kiện: `{"all": [{"field": "tribute_board_prepared", "op": "eq", "value": true}, {"field": "tribute_board_removed", "op": "eq", "value": false}]}`.

**AN**: Bản này dừng rồi. Tao cất bảng đi.
**HA**: Để vào túi giấy, đừng gấp chữ.
**NHI**: Ừ. Tao đứng bên này.

Hệ quả: `[{"op": "set", "field": "tribute_board_removed", "value": true}]`.
Đi tiếp: `step:S09_A_FRAME`.

Dựng/kiểm: Nếu chưa báo người nhận, dùng bước notice trước chốt trang; thao tác gỡ không thay tin báo.

**Lựa chọn `S09_A_LEAVE_BOARD`: Giữ bảng trong khung và đề nghị Nhi cầm**

Điều kiện: `{"all": [{"field": "tribute_board_prepared", "op": "eq", "value": true}, {"field": "tribute_board_removed", "op": "eq", "value": false}]}`.

**AN**: Cầm thử một tấm thôi được không?
**NHI**: Tao bảo không rồi. Tấm này tụi mày chụp đi.

Hệ quả: `[{"op": "set", "field": "nhi_wants_capture", "value": false}]`.
Đi tiếp: `step:S09_A_LEAVE`.

**Lựa chọn `S09_A_NO_BOARD`: Đặt sơ đồ cũ cạnh máy, không dựng bảng**

Điều kiện: `{"any": [{"field": "tribute_board_prepared", "op": "eq", "value": false}, {"field": "tribute_board_removed", "op": "eq", "value": true}]}`.

**HA**: Tấm hội chợ vẫn giữ nhé.
**NHI**: Tao đâu có bảo bỏ tấm đó.

Không tự đổi trạng thái ngoài chuyển bước.
Đi tiếp: `step:S09_A_FRAME`.
### S09_A_FRAME

Mọi người tự chọn chỗ; khung cũ không vừa bảng mới và túi sách.

**MINH**: Chụp hay không thì chốt. Tao còn chuyển đồ.

**Lựa chọn `S09_A_OPEN_FRAME`: Nới khung, để mỗi người tự chọn chỗ**

**AN**: Không cần giống tấm trước. Mày đứng đâu tiện thì đứng.
**NHI**: Thế tao đứng đây.
**HA**: Đợi tao hạ góc một chút.

Hệ quả: `[{"op": "set", "field": "framing", "value": "open"}]`.
Đi tiếp: `step:S09_A_CAMERA`.

**Lựa chọn `S09_A_OLD_POSE`: Đề nghị mọi người đứng y hệt tấm cũ**

**AN**: Nhi sang phải chút, đúng chỗ trong ảnh trước.
**NHI**: Mày muốn tấm đó hay muốn chụp chung? Tao không đứng diễn lại đâu.

Hệ quả: `[{"op": "set", "field": "framing", "value": "forced"}]`.
Đi tiếp: `step:S09_A_REFUSE`.

**Lựa chọn `S09_A_CHECK_SCOPE`: Hạ máy, hỏi ảnh mới sẽ dùng ở đâu**

**AN**: Mình chụp một tấm rồi cả nhóm xem. Chưa ai đồng ý thì chưa đưa vào trang.
**NHI**: Ừ. Vậy chụp đi.

Hệ quả: `[{"op": "set", "field": "framing", "value": "open"}]`.
Đi tiếp: `step:S09_A_CAMERA`.
### S09_A_REFUSE

Nhi chưa rời; An có thể đổi cách chụp hoặc dừng. Không cần chọn câu xin lỗi mẫu.


**Lựa chọn `S09_A_CHANGE_FRAME`: Bỏ dấu vị trí cũ, thử khung thoáng hơn**

**NARRATOR**: An kéo băng đánh dấu khỏi sàn.
**AN**: Đứng như bây giờ cũng được.
**NHI**: Ừ, đừng kéo cặp tao vào khung là được.

Hệ quả: `[{"op": "set", "field": "framing", "value": "open"}]`.
Đi tiếp: `step:S09_A_CAMERA`.

**Lựa chọn `S09_A_STOP_FRAME`: Dừng buổi chụp với Nhi**

**AN**: Thôi, hôm nay không chụp chung nữa.
**NHI**: Ừ. Tao qua trả đồ.

Hệ quả: `[{"op": "set", "field": "nhi_wants_capture", "value": false}]`.
Đi tiếp: `step:S09_A_LEAVE`.
### S09_A_CAMERA

An giữ đầu dây, không mặc định giao Nhi. Thử lại kẹp nếu chưa kiểm xong.


**Lựa chọn `S09_A_FIX`: Kê lại kẹp và tự giữ dây (4 phút)**

Điều kiện: `{"field": "equipment_ready", "op": "eq", "value": "repairable"}`.

**AN**: Dây để tao giữ. Minh nhìn giúp máy có nghiêng không?
**MINH**: Bên trái thấp hơn. Rồi, được.

Hệ quả: `[{"op": "set", "field": "equipment_ready", "value": "ready"}, {"op": "advance_minutes", "value": 4}]`.
Đi tiếp: `step:S09_A_CAPTURE`.

**Lựa chọn `S09_A_MOUNT`: Đặt máy đã kiểm lên kẹp**

Điều kiện: `{"field": "equipment_ready", "op": "eq", "value": "ready"}`.

**NARRATOR**: An đặt máy vào kẹp, kéo dây gọn rồi bước thử vào khung.

Không tự đổi trạng thái ngoài chuyển bước.
Đi tiếp: `step:S09_A_CAPTURE`.

**Lựa chọn `S09_A_REBUILD`: Dựng lại toàn bộ quầy theo ảnh trước (25 phút)**

**NARRATOR**: 16:45 đã qua. Khi An kê xong tấm bảng, Nhi đeo cặp lên.
**NHI**: 17:10 rồi. Tao phải đi.
**AN**: Ừ. Đồ còn lại để bọn tao.

Hệ quả: `[{"op": "set", "field": "attendance", "value": "left"}, {"op": "set", "field": "nhi_wants_capture", "value": false}, {"op": "advance_minutes", "value": 25}]`.
Đi tiếp: `step:S09_A_LEAVE`.

Dựng/kiểm: UI báo trước 25 phút và mốc đi, không cho thời gian âm thầm trôi khi đọc.
### S09_A_CAPTURE

Mọi người đã thống nhất khung. Chỉ chụp nếu attendance/giờ vẫn hợp lệ.


**Lựa chọn `S09_A_CAPTURE`: Bấm hẹn giờ rồi An chạy vào**

Điều kiện: `{"all": [{"field": "attendance", "op": "eq", "value": "present"}, {"field": "nhi_wants_capture", "op": "eq", "value": true}, {"field": "courtyard_minute", "op": "lt", "value": 70}, {"field": "equipment_ready", "op": "eq", "value": "ready"}]}`.

**AN**: Bíp rồi, đừng nhìn tao nữa, nhìn máy.
**NARRATOR**: Minh cúi nhặt miếng bìa vừa rơi. Hạ bật cười. Nhi đứng lệch nửa bước so với sơ đồ.
**HA**: Giữ tấm này.

Hệ quả: `[{"op": "set", "field": "photo_result", "value": "four"}, {"op": "set", "field": "captured_asset", "value": "PHOTO_NEW_FOUR"}, {"op": "set", "field": "photo_participants", "value": ["AN", "NHI", "HA", "MINH"]}]`.
Đi tiếp: `step:S09_A_RIGHTS`.

Dựng/kiểm: Hiện chi tiết tương ứng opening_gesture; không đòi pose cụ thể để chụp được.

**Lựa chọn `S09_A_TIMEOUT`: Cất chỗ Nhi trước khi thử chụp tiếp**

Điều kiện: `{"any": [{"field": "courtyard_minute", "op": "gte", "value": 70}, {"field": "attendance", "op": "ne", "value": "present"}]}`.

**NHI**: Tao phải đi rồi.
**AN**: Ừ. Tao không giữ mày thêm.

Hệ quả: `[{"op": "set", "field": "attendance", "value": "left"}, {"op": "set", "field": "nhi_wants_capture", "value": false}]`.
Đi tiếp: `step:S09_A_LEAVE`.
### S09_A_RIGHTS

Mở đúng ảnh mới vừa chụp, hỏi quyền dùng; không mặc định đồng ý từ việc đứng vào ảnh.

**AN**: Tấm này để vào trang 17 nhé? Nếu không thích mình chỉ giữ riêng theo ý mọi người.

**Lựa chọn `S09_A_USE_PHOTO`: Cho cả nhóm xem ảnh và hỏi dùng bản này**

**HA**: Phần có tao in được, giữ album riêng cũng được.
**MINH**: Tao đồng ý cả hai. Gửi tao bản PDF nữa.
**NHI**: Ừ, tấm này in và giữ trong album riêng được. Gửi tao cả tấm buổi tập nhé.

Hệ quả: `[{"op": "set", "field": "nhi_photo_consent", "value": "yes"}, {"op": "set", "field": "private_photo_allowed", "value": true}, {"op": "grant", "asset_id": "PHOTO_NEW_FOUR", "subject_id": "AN", "scope": "yearbook_print", "status": "yes", "source_event_id": "S09_A_USE_PHOTO"}, {"op": "grant", "asset_id": "PHOTO_NEW_FOUR", "subject_id": "NHI", "scope": "yearbook_print", "status": "yes", "source_event_id": "S09_A_USE_PHOTO"}, {"op": "grant", "asset_id": "PHOTO_NEW_FOUR", "subject_id": "HA", "scope": "yearbook_print", "status": "yes", "source_event_id": "S09_A_USE_PHOTO"}, {"op": "grant", "asset_id": "PHOTO_NEW_FOUR", "subject_id": "MINH", "scope": "yearbook_print", "status": "yes", "source_event_id": "S09_A_USE_PHOTO"}, {"op": "set", "field": "new_photo_print_allowed", "value": true}, {"op": "grant", "asset_id": "PHOTO_NEW_FOUR", "subject_id": "AN", "scope": "private_album", "status": "yes", "source_event_id": "S09_A_USE_PHOTO"}, {"op": "grant", "asset_id": "PHOTO_NEW_FOUR", "subject_id": "NHI", "scope": "private_album", "status": "yes", "source_event_id": "S09_A_USE_PHOTO"}, {"op": "grant", "asset_id": "PHOTO_NEW_FOUR", "subject_id": "HA", "scope": "private_album", "status": "yes", "source_event_id": "S09_A_USE_PHOTO"}, {"op": "grant", "asset_id": "PHOTO_NEW_FOUR", "subject_id": "MINH", "scope": "private_album", "status": "yes", "source_event_id": "S09_A_USE_PHOTO"}]`.
Đi tiếp: `step:S09_A_PACK`.

**Lựa chọn `S09_A_PRIVATE_ONLY`: Hỏi giữ riêng tấm này, dùng ảnh cũ cho trang**

**NHI**: Tấm này tao chưa muốn đưa vào kỷ yếu. Giữ trong album riêng thôi.
**HA**: Ừ, bản trang mình dùng tấm khác.
**MINH**: Tao đồng ý giữ riêng.
**AN**: Được, không đưa tấm này đi in.

Hệ quả: `[{"op": "set", "field": "nhi_photo_consent", "value": "no"}, {"op": "set", "field": "private_photo_allowed", "value": true}, {"op": "grant", "asset_id": "PHOTO_NEW_FOUR", "subject_id": "AN", "scope": "private_album", "status": "yes", "source_event_id": "S09_A_PRIVATE_ONLY"}, {"op": "grant", "asset_id": "PHOTO_NEW_FOUR", "subject_id": "NHI", "scope": "private_album", "status": "yes", "source_event_id": "S09_A_PRIVATE_ONLY"}, {"op": "grant", "asset_id": "PHOTO_NEW_FOUR", "subject_id": "HA", "scope": "private_album", "status": "yes", "source_event_id": "S09_A_PRIVATE_ONLY"}, {"op": "grant", "asset_id": "PHOTO_NEW_FOUR", "subject_id": "MINH", "scope": "private_album", "status": "yes", "source_event_id": "S09_A_PRIVATE_ONLY"}, {"op": "grant", "asset_id": "PHOTO_NEW_FOUR", "subject_id": "NHI", "scope": "yearbook_print", "status": "no", "source_event_id": "S09_A_PRIVATE_ONLY"}]`.
Đi tiếp: `step:S09_A_PACK`.
### S09_A_PACK

Giờ đi không phụ thuộc số câu đọc. Thu đồ sau chụp; Nhi đi trước để kịp lớp.

**NHI**: Tao đi nhé.
**AN**: Ừ. Bản chốt tao gửi sau.

**Lựa chọn `S09_A_PACK`: Tháo máy, cất pin và bàn giao kẹp**

**NARRATOR**: An tháo máy. Minh cất pin vào túi đã ghi tên. Vy nhận lại kẹp, Hạ giữ file ảnh.

Hệ quả: `[{"op": "set", "field": "equipment_packed", "value": true}, {"op": "set", "field": "attendance", "value": "left"}]`.
Đi tiếp: `scene:S10`.
### S09_A_LEAVE

Nhi rời phần chụp; khung hình kế tiếp chỉ có người còn ở sân.


**Lựa chọn `S09_A_TO_ABSENT`: Để Nhi đi, quay lại với Hạ và Minh**

**NARRATOR**: Nhi khoác cặp đi về phía chỗ trả đồ. An đặt máy xuống, nhìn Hạ và Minh.

Hệ quả: `[{"op": "set", "field": "attendance", "value": "left"}, {"op": "set", "field": "nhi_wants_capture", "value": false}]`.
Đi tiếp: `scene:S09_B`.

<a id="s09-b"></a>
## S09_B - Sân còn ba người

Nguồn: v4.1 trang 10, 11, 13, 18, 19.

Đường vắng có đủ việc làm và cảnh kết, không là màn báo lỗi. Nhắn nhiều hay chờ không triệu hồi Nhi.

### S09_B_DECIDE

Bảng HẸN sáng, ảnh cũ đủ bốn ở góc màn hình. Hạ giữ chân máy, Minh nhìn giờ.

**HA**: Vậy… mình chụp không?
**MINH**: Tao giữ máy thêm một lát được. Nhưng mấy giờ xong?

**Lựa chọn `S09_B_THREE`: Chụp một tấm ba người hôm nay**

Điều kiện: `{"field": "courtyard_minute", "op": "lt", "value": 90}`.

**AN**: Chụp một tấm của ba đứa đang ở đây. Không gọi là đủ bốn.
**HA**: Ừ. Hạ máy xuống chút, lấy cả cái bảng vào.

Hệ quả: `[{"op": "set", "field": "absence_choice", "value": "three"}]`.
Đi tiếp: `step:S09_B_THREE`.

**Lựa chọn `S09_B_NO_PHOTO`: Không chụp hôm nay, cùng thu đồ**

**AN**: Thôi, hôm nay cất máy. Tao giữ sơ đồ, làm trang từ nguồn cũ.
**MINH**: Ừ. Cái chân bên kia gập trước.

Hệ quả: `[{"op": "set", "field": "absence_choice", "value": "none"}]`.
Đi tiếp: `step:S09_B_PACK`.

**Lựa chọn `S09_B_LATER`: Đề nghị một cuộc gặp khác, không bắt buộc chụp**

**AN**: Tao hỏi một buổi khác, ai được thì gặp. Chưa có trả lời thì hôm nay mình vẫn cất đồ.

Hệ quả: `[{"op": "set", "field": "absence_choice", "value": "later"}]`.
Đi tiếp: `step:S09_B_LATER`.
### S09_B_THREE

An đặt máy vào kẹp, kiểm lại và chạy vào khung. Không sinh hình Nhi từ ảnh cũ.


**Lựa chọn `S09_B_TAKE_THREE`: Kê máy, hẹn giờ và cùng vào ảnh (4 phút)**

Điều kiện: `{"field": "courtyard_minute", "op": "lt", "value": 86}`.

**NARRATOR**: An kéo ghế thừa ra khỏi khung rồi chạy vào. Minh vừa nhìn pin thì máy bấm.
**MINH**: Lại nhìn pin nữa rồi.
**HA**: Tao thích tấm này.

Hệ quả: `[{"op": "set", "field": "equipment_ready", "value": "ready"}, {"op": "set", "field": "photo_result", "value": "three"}, {"op": "set", "field": "captured_asset", "value": "PHOTO_NEW_THREE"}, {"op": "set", "field": "photo_participants", "value": ["AN", "HA", "MINH"]}, {"op": "advance_minutes", "value": 4}]`.
Đi tiếp: `step:S09_B_THREE_RIGHTS`.

**Lựa chọn `S09_B_TOO_LATE`: Tháo quầy đúng lịch, giữ ảnh cũ**

Điều kiện: `{"field": "courtyard_minute", "op": "gte", "value": 86}`.

**VY**: Sắp tháo quầy rồi, em nhận lại kẹp nhé.
**AN**: Ừ. Tao gập chân máy trước.

Không tự đổi trạng thái ngoài chuyển bước.
Đi tiếp: `step:S09_B_PACK`.
### S09_B_THREE_RIGHTS

Xem ảnh vừa chụp; chỉ người thực sự xuất hiện xác nhận.


**Lựa chọn `S09_B_THREE_PRINT`: Hỏi dùng ảnh ba người trong trang kỷ yếu**

**HA**: Dùng tấm này trong trang và album riêng được. Ghi đúng hôm nay và ba người nhé.
**MINH**: Tao đồng ý hai phạm vi đó.
**AN**: Tao cũng đồng ý. Không lấy nó thay lời hẹn đủ bốn.

Hệ quả: `[{"op": "grant", "asset_id": "PHOTO_NEW_THREE", "subject_id": "AN", "scope": "yearbook_print", "status": "yes", "source_event_id": "S09_B_THREE_PRINT"}, {"op": "grant", "asset_id": "PHOTO_NEW_THREE", "subject_id": "HA", "scope": "yearbook_print", "status": "yes", "source_event_id": "S09_B_THREE_PRINT"}, {"op": "grant", "asset_id": "PHOTO_NEW_THREE", "subject_id": "MINH", "scope": "yearbook_print", "status": "yes", "source_event_id": "S09_B_THREE_PRINT"}, {"op": "set", "field": "private_photo_allowed", "value": true}, {"op": "set", "field": "new_photo_print_allowed", "value": true}, {"op": "grant", "asset_id": "PHOTO_NEW_THREE", "subject_id": "AN", "scope": "private_album", "status": "yes", "source_event_id": "S09_B_THREE_PRINT"}, {"op": "grant", "asset_id": "PHOTO_NEW_THREE", "subject_id": "HA", "scope": "private_album", "status": "yes", "source_event_id": "S09_B_THREE_PRINT"}, {"op": "grant", "asset_id": "PHOTO_NEW_THREE", "subject_id": "MINH", "scope": "private_album", "status": "yes", "source_event_id": "S09_B_THREE_PRINT"}]`.
Đi tiếp: `step:S09_B_PACK`.

**Lựa chọn `S09_B_THREE_PRIVATE`: Giữ trong album riêng, chưa dùng cho trang**

**AN**: Mình giữ riêng tấm này nhé. Trang 17 tao dùng ảnh cũ.
**HA**: Ừ, gửi tao một bản.
**MINH**: Được.

Hệ quả: `[{"op": "grant", "asset_id": "PHOTO_NEW_THREE", "subject_id": "AN", "scope": "private_album", "status": "yes", "source_event_id": "S09_B_THREE_PRIVATE"}, {"op": "grant", "asset_id": "PHOTO_NEW_THREE", "subject_id": "HA", "scope": "private_album", "status": "yes", "source_event_id": "S09_B_THREE_PRIVATE"}, {"op": "grant", "asset_id": "PHOTO_NEW_THREE", "subject_id": "MINH", "scope": "private_album", "status": "yes", "source_event_id": "S09_B_THREE_PRIVATE"}, {"op": "set", "field": "private_photo_allowed", "value": true}]`.
Đi tiếp: `step:S09_B_PACK`.
### S09_B_LATER

Gửi lời đề nghị có ngày, chỗ và mục đích. Không quay cảnh gặp khi chưa có xác nhận.


**Lựa chọn `S09_B_LATER_CLEAR`: Hỏi gặp ghế đá 11/05 lúc 16:30 để xem ảnh và giao tờ bổ sung**

Điều kiện: `{"field": "invite_pressure", "op": "eq", "value": false}`.

**AN**: Thứ Hai 11/05, 16:30 ở ghế đá được không? Tao mang ảnh cũ với tờ bổ sung, không bắt buộc chụp.
**NHI**: 16:30 được. Tao chỉ gặp mày thôi, Hạ với Minh có việc thì đừng kéo về. Mang cả tấm buổi tập nhé.
**AN**: Ừ. Tờ bổ sung tao gửi nhóm xem trước khi in.

Hệ quả: `[{"op": "set", "field": "later_status", "value": "accepted"}, {"op": "set", "field": "later_datetime", "value": "2026-05-11T16:30:00"}, {"op": "set", "field": "later_place", "value": "ghế đá sân trường"}, {"op": "set", "field": "later_purpose", "value": "xem ảnh và giao tờ bổ sung"}, {"op": "set", "field": "later_participants", "value": ["AN", "NHI"]}]`.
Đi tiếp: `step:S09_B_PACK`.

Dựng/kiểm: Đây là biến thể được biên soạn, có câu nhận lời thật trong truyện; không mặc định mọi lần đề nghị đều được nhận.

**Lựa chọn `S09_B_LATER_PENDING`: Gửi đề nghị để Nhi xem lịch, không hẹn giờ thay cô**

**AN**: Tuần sau mày muốn nhận ảnh lúc nào thì báo tao nhé. Không cần ra chụp.
**NARRATOR**: Chưa có trả lời nhận lời. An cất máy, không đánh dấu đã có cuộc gặp.

Hệ quả: `[{"op": "set", "field": "later_status", "value": "pending"}]`.
Đi tiếp: `step:S09_B_PACK`.

**Lựa chọn `S09_B_LATER_REFUSED`: Hỏi lại một lần sau cuộc nói chuyện còn căng**

Điều kiện: `{"field": "invite_pressure", "op": "eq", "value": true}`.

**AN**: Tuần sau mình gặp để tao nói rõ hơn nhé?
**NHI**: Tao chưa muốn hẹn. Cứ làm bản từ ảnh cũ rồi gửi tao xem.
**AN**: Ừ. Tao không đặt lịch thay mày.

Hệ quả: `[{"op": "set", "field": "later_status", "value": "declined"}]`.
Đi tiếp: `step:S09_B_PACK`.
### S09_B_PACK

Không cắt ngay tới tổng kết. Người chơi tháo máy, lấy sơ đồ và cất bảng nếu tồn tại.

**MINH**: Tao cất máy nhé?
**AN**: Để tao gập chân. Mày lấy pin.
**HA**: Tờ sơ đồ đừng để dưới đất.

**Lựa chọn `S09_B_PACK`: Gập chân máy, cất bảng và sơ đồ**

**NARRATOR**: An gỡ bản dựng ba vị trí, cuộn dây, trả kẹp cho Vy. Sơ đồ bốn tên được đặt trong túi bìa.
**HA**: Bản cuối mày gửi tao nhé.
**AN**: Ừ, tao làm tiếp.

Hệ quả: `[{"op": "set", "field": "equipment_packed", "value": true}, {"op": "set", "field": "tribute_board_removed", "value": true}]`.
Đi tiếp: `scene:S10`.

Dựng/kiểm: Chỉ animate bảng nếu prepared; set removed không tạo bảng chưa từng có.

<a id="s10"></a>
## S10 - Trang nào được giữ

Nguồn: v4.1 trang 12, 13, 14, 19.

Tối thứ Sáu. Mọi nguồn được hỏi đúng scope. Người chơi nhìn đúng trang, lịch và phần còn chờ trước chốt. Không chấm điểm.

### S10_WORK

File gốc mở được dù Hạ không giúp dàn thêm.


**Lựa chọn `S10_WITH_HA`: Cùng Hạ đặt ảnh vào mẫu**

Điều kiện: `{"field": "ha_support", "op": "eq", "value": "collaborate"}`.

**HA**: Tao làm phần ảnh. Phần chữ mày tự chốt rồi gửi xem lại.
**AN**: Ừ, tao không giữ câu gán cho Nhi.

Không tự đổi trạng thái ngoài chuyển bước.
Đi tiếp: `step:S10_NOTICE`.

**Lựa chọn `S10_ALONE`: An mở mẫu đơn giản và tự dàn**

Điều kiện: `{"field": "ha_support", "op": "eq", "value": "handover_only"}`.

**NARRATOR**: An mở mẫu một trang, kéo ảnh vào đúng ô. File Hạ bàn giao vẫn đủ.
**HA**: Bản chốt có ảnh tao thì gửi xem trước nhé.

Không tự đổi trạng thái ngoài chuyển bước.
Đi tiếp: `step:S10_NOTICE`.
### S10_NOTICE

Kiểm việc dừng bản tôn vinh. Một phiên bản mới không xóa người đã thấy phiên bản cũ.


**Lựa chọn `S10_STOP_REMAINING`: Báo dừng nháp cảm ơn còn chưa xử lý**

Điều kiện: `{"all": [{"field": "draft_type", "op": "eq", "value": "tribute"}, {"field": "draft_retracted", "op": "ne", "value": "yes"}, {"field": "draft_sent_to", "op": "eq", "value": "group_four"}]}`.

**AN**: Bản cảm ơn trước dừng nhé. Trang này không dùng câu đó hay bảng đó. Người đã xem nhận giúp tao bản mới.
**NHI**: Ừ. Gửi bản mới cho tao xem.
**HA**: Tao không dùng bảng cũ nữa.
**MINH**: Tao đổi bản xem trong thư mục rồi.

Hệ quả: `[{"op": "set", "field": "draft_retracted", "value": "yes"}, {"op": "set", "field": "draft_version", "value": 2}, {"op": "set", "field": "tribute_board_removed", "value": true}, {"op": "add_unique", "field": "retraction_notified_to", "value": "NHI"}, {"op": "add_unique", "field": "retraction_notified_to", "value": "HA"}, {"op": "add_unique", "field": "retraction_notified_to", "value": "MINH"}]`.
Đi tiếp: `step:S10_OLD`.

Dựng/kiểm: Chỉ gửi tới người thực sự đã xem; runtime lấy giao của danh sách người này với draft_seen_by. Không thêm lượt đã xem.

**Lựa chọn `S10_NOTICE_OK`: Tiếp tục từ bản hiện tại**

Điều kiện: `{"any": [{"field": "draft_type", "op": "ne", "value": "tribute"}, {"field": "draft_retracted", "op": "eq", "value": "yes"}]}`.

**NARRATOR**: Bản hiện tại được mở cạnh lịch giao sách; bản cũ vẫn nằm trong lịch sử.

Không tự đổi trạng thái ngoài chuyển bước.
Đi tiếp: `step:S10_OLD`.

**Lựa chọn `S10_STOP_PRIVATE_REMAINING`: Báo dừng nháp cảm ơn còn chưa xử lý**

Điều kiện: `{"all": [{"field": "draft_type", "op": "eq", "value": "tribute"}, {"field": "draft_retracted", "op": "ne", "value": "yes"}, {"field": "draft_sent_to", "op": "ne", "value": "group_four"}]}`.

**AN**: Bản cảm ơn riêng trước tao dừng. Nguồn cũ vừa chọn, tao sẽ cho mày xem chữ trước khi chốt.
**NHI**: Ừ, gửi đúng bản mới nhé.

Hệ quả: `[{"op": "set", "field": "draft_retracted", "value": "yes"}, {"op": "set", "field": "draft_version", "value": 2}, {"op": "set", "field": "tribute_board_removed", "value": true}, {"op": "add_unique", "field": "retraction_notified_to", "value": "NHI"}]`.
Đi tiếp: `step:S10_OLD`.

Dựng/kiểm: Dừng nháp riêng và báo Nhi; không tạo người nhận/đã xem mới trong lịch sử.
### S10_OLD

Chọn nguồn cũ cụ thể. Preview riêng và lời xin phép Nhi đi liền nguồn, không cấp quyền cho cả album.


**Lựa chọn `S10_OLD_1`: Ảnh buổi tập đủ bốn**

**AN**: Tao giữ tấm đủ bốn đã hỏi mày, đúng ảnh này trong trang.
**NHI**: Ừ. Tấm này dùng được.

Hệ quả: `[{"op": "set", "field": "final_old_asset", "value": "PHOTO_S00_GOOD"}, {"op": "grant", "asset_id": "PHOTO_S00_GOOD", "subject_id": "NHI", "scope": "yearbook_print", "status": "yes", "source_event_id": "S10_OLD_1"}]`.
Đi tiếp: `step:S10_NEW`.

**Lựa chọn `S10_OLD_2`: Ảnh hỏng khi đèn vừa tắt**

**AN**: Còn tấm đèn tắt này, để trong trang 17 được không?
**NHI**: Tấm đấy mặt tao buồn cười thật. Được, cứ để.

Hệ quả: `[{"op": "set", "field": "final_old_asset", "value": "PHOTO_S00_BAD_01"}, {"op": "grant", "asset_id": "PHOTO_S00_BAD_01", "subject_id": "NHI", "scope": "yearbook_print", "status": "yes", "source_event_id": "S10_OLD_2"}]`.
Đi tiếp: `step:S10_NEW`.

**Lựa chọn `S10_OLD_3`: Ảnh hỏng Minh đang cúi nhặt bìa**

**AN**: Tao muốn dùng tấm Minh cúi xuống này. Mày xem nhé?
**NHI**: Được, tao thích tấm đó. Hỏi cả Minh nữa.

Hệ quả: `[{"op": "set", "field": "final_old_asset", "value": "PHOTO_S00_BAD_02"}, {"op": "grant", "asset_id": "PHOTO_S00_BAD_02", "subject_id": "NHI", "scope": "yearbook_print", "status": "yes", "source_event_id": "S10_OLD_3"}]`.
Đi tiếp: `step:S10_NEW`.

**Lựa chọn `S10_OLD_4`: Sơ đồ hai ô và tia chớp**

**AN**: Dùng sơ đồ mày vẽ như một bản nháp cũ được không? Không coi ô trống là lời trách ai.
**NHI**: Được. Để cả hình tia chớp nhé.

Hệ quả: `[{"op": "set", "field": "final_old_asset", "value": "PROP_DIAGRAM"}, {"op": "grant", "asset_id": "PROP_DIAGRAM", "subject_id": "NHI", "scope": "yearbook_print", "status": "yes", "source_event_id": "S10_OLD_4"}]`.
Đi tiếp: `step:S10_NEW`.
### S10_NEW

Ảnh mới nếu có được đặt cạnh nguồn cũ; ảnh không có quyền in chỉ ở album riêng đã được đồng ý.


**Lựa chọn `S10_NEW_FOUR`: Dùng ảnh mới bốn người trong trang**

Điều kiện: `{"all": [{"field": "captured_asset", "op": "eq", "value": "PHOTO_NEW_FOUR"}, {"field": "nhi_photo_consent", "op": "eq", "value": "yes"}]}`.

**NARRATOR**: Nguồn cũ bên trái, ảnh 08/05 gồm An, Nhi, Hạ, Minh bên phải. Caption ghi đúng ngày và người.

Hệ quả: `[{"op": "set", "field": "final_mode", "value": "new"}, {"op": "set", "field": "final_new_asset", "value": "PHOTO_NEW_FOUR"}, {"op": "set", "field": "final_scope", "value": "yearbook_print"}]`.
Đi tiếp: `step:S10_0443`.

**Lựa chọn `S10_NEW_THREE`: Dùng ảnh mới ba người trong trang**

Điều kiện: `{"all": [{"field": "captured_asset", "op": "eq", "value": "PHOTO_NEW_THREE"}, {"field": "new_photo_print_allowed", "op": "eq", "value": true}]}`.

**NARRATOR**: Ảnh mới ghi An, Hạ, Minh; không gọi là ảnh đủ bốn hay gán lời cho Nhi.

Hệ quả: `[{"op": "set", "field": "final_mode", "value": "new"}, {"op": "set", "field": "final_new_asset", "value": "PHOTO_NEW_THREE"}, {"op": "set", "field": "final_scope", "value": "yearbook_print"}]`.
Đi tiếp: `step:S10_0443`.

**Lựa chọn `S10_ONLY_OLD`: Dùng nguồn cũ, bỏ khung chờ ảnh mới**

**NARRATOR**: An bỏ ô chờ, nới nguồn cũ ra. Nếu có ảnh mới giữ riêng, nó vẫn còn trong album đúng phạm vi.

Hệ quả: `[{"op": "set", "field": "final_mode", "value": "old"}, {"op": "set", "field": "final_new_asset", "value": ""}, {"op": "set", "field": "final_scope", "value": "yearbook_print"}]`.
Đi tiếp: `step:S10_0443`.

**Lựa chọn `S10_SUPPLEMENT`: Dùng trang thông báo ngắn, tự giao tờ bổ sung ở cuộc gặp đã xác nhận**

Điều kiện: `{"field": "later_status", "op": "eq", "value": "accepted"}`.

**AN**: Sách chính để trang thông báo bổ sung phát riêng. Tờ ảnh này tao mang tới buổi đã hẹn, mọi người xem trước nhé.
**NHI**: Ừ. Nguồn cũ vừa xem dùng trong tờ bổ sung được.
**HA**: Tao xem tờ đó trước khi in.
**MINH**: Đừng dời cả lịch phát sách nếu chỉ giao tờ riêng.

Hệ quả: `[{"op": "set", "field": "final_mode", "value": "supplement"}, {"op": "set", "field": "final_new_asset", "value": ""}, {"op": "grant", "asset_field": "final_old_asset", "subject_id": "NHI", "scope": "supplement_delivery", "status": "yes", "source_event_id": "S10_SUPPLEMENT"}, {"op": "set", "field": "supplement_delivery_datetime", "value": "2026-05-11T16:30:00"}, {"op": "set", "field": "supplement_delivery_place", "value": "ghế đá sân trường"}, {"op": "set", "field": "final_scope", "value": "supplement_delivery"}]`.
Đi tiếp: `step:S10_0443`.

Dựng/kiểm: Không tự gia hạn sách hoặc hứa miễn phí. TV5 chốt chi phí in thực trước khi có lựa chọn trả tiền; MVP tự giao tờ riêng.
### S10_0443

0443 có thể ở album hoặc một góc phù hợp của trang/tờ, quyền được hỏi riêng.


**Lựa chọn `S10_ADD_0443`: Thêm 0443 với chữ “Hội chợ tháng Mười - An, Hạ, Minh”**

**AN**: Tao đặt tấm hội chợ ở góc này, đúng ba tên và ngày tháng Mười.
**HA**: Ừ. Phần ảnh tao đồng ý, chữ thế này được.
**MINH**: Tao cũng đồng ý.
**AN**: Tao đồng ý dùng phần có mình.

Hệ quả: `[{"op": "set", "field": "use_0443", "value": true}, {"op": "grant", "asset_id": "PHOTO_0443", "subject_id": "AN", "status": "yes", "source_event_id": "S10_ADD_0443", "scope_field": "final_scope"}, {"op": "grant", "asset_id": "PHOTO_0443", "subject_id": "HA", "status": "yes", "source_event_id": "S10_ADD_0443", "scope_field": "final_scope"}, {"op": "grant", "asset_id": "PHOTO_0443", "subject_id": "MINH", "status": "yes", "source_event_id": "S10_ADD_0443", "scope_field": "final_scope"}]`.
Đi tiếp: `step:S10_RIGHTS`.

**Lựa chọn `S10_KEEP_0443_PRIVATE`: Giữ 0443 ở album, không thêm vào trang cuối**

**AN**: Tấm hội chợ vẫn giữ trong album như đã thống nhất, trang này dùng nguồn vừa chọn.
**HA**: Ừ. Gửi tao bản chốt.

Hệ quả: `[{"op": "set", "field": "use_0443", "value": false}]`.
Đi tiếp: `step:S10_RIGHTS`.
### S10_RIGHTS

Cả nhóm nhận đúng preview nguồn cũ và caption; không xin quyền bằng tên file không rõ nội dung.

**AN**: Đây là bản với nguồn cũ vừa chọn, chữ chỉ ngày và tên. Hạ, Minh xem phần có mình nhé.
**HA**: Bản này tao xem được, phần của tao dùng được.
**MINH**: Ừ, dùng được. Tao không ở lại xuất file tối nay, bản hướng dẫn tao gửi rồi.
**AN**: Phần của tao đồng ý. Tao xuất và gửi bản chốt.

**Lựa chọn `S10_RIGHTS_OK`: Ghi nhận quyền đúng nguồn và phạm vi đang dùng**


Hệ quả: `[{"op": "grant", "asset_field": "final_old_asset", "subject_id": "AN", "status": "yes", "source_event_id": "S10_RIGHTS_OK", "scope_field": "final_scope"}, {"op": "grant", "asset_field": "final_old_asset", "subject_id": "HA", "status": "yes", "source_event_id": "S10_RIGHTS_OK", "scope_field": "final_scope"}, {"op": "grant", "asset_field": "final_old_asset", "subject_id": "MINH", "status": "yes", "source_event_id": "S10_RIGHTS_OK", "scope_field": "final_scope"}]`.
Đi tiếp: `step:S10_PREVIEW`.

Dựng/kiểm: PROP_DIAGRAM chỉ có tác giả Nhi cần grant; các xác nhận dư của người xem không thay quyền tác giả. Runtime kiểm theo subjects của asset catalog.
### S10_PREVIEW

Hiện trang thật, danh sách nguồn/người, lịch theo print_slot và việc tự giao bổ sung nếu có. Không có thông báo đạo đức.

**MINH**: Bản này?
**AN**: Ừ, đúng bản này.
**NARRATOR**: An nhìn lại tên album, xóa chữ final khỏi tên file rồi lưu bản có ngày.

**Lựa chọn `S10_PREVIEW`: Xem đủ trang và lịch giao**


Hệ quả: `[{"op": "set", "field": "page_previewed", "value": true}]`.
Đi tiếp: `step:S10_PUBLISH`.

Dựng/kiểm: Runtime render và kiểm quyền; chưa gửi in chỉ vì mở preview.
### S10_PUBLISH

Chốt riêng sau kiểm quyền; yêu cầu runtime lưu PNG hiện vật, không giả định đã ghi file thành công.


**Lựa chọn `S10_PUBLISH_NEW`: Chốt bản vừa xem và lưu trang cuối**

Điều kiện: `{"all": [{"field": "page_previewed", "op": "eq", "value": true}, {"field": "final_mode", "op": "eq", "value": "new"}]}`.

**NARRATOR**: An xác nhận bản vừa xem để bàn giao. Một bản trang cuối được yêu cầu lưu cùng thông tin nguồn và ngày.

Hệ quả: `[{"op": "set", "field": "print_submitted", "value": true}, {"op": "set", "field": "final_artifact_requested", "value": true}, {"op": "set", "field": "ending_id", "value": "E_A"}]`.
Đi tiếp: `scene:E_A`.

Dựng/kiểm: Command publish phải kiểm toàn bộ grants/attendance ảnh/lịch lại. PNG lỗi được thử lại; không coi yêu cầu export là đã có file.

**Lựa chọn `S10_PUBLISH_OLD`: Chốt bản vừa xem và lưu trang cuối**

Điều kiện: `{"all": [{"field": "page_previewed", "op": "eq", "value": true}, {"field": "final_mode", "op": "eq", "value": "old"}]}`.

**NARRATOR**: An xác nhận bản vừa xem để bàn giao. Một bản trang cuối được yêu cầu lưu cùng thông tin nguồn và ngày.

Hệ quả: `[{"op": "set", "field": "print_submitted", "value": true}, {"op": "set", "field": "final_artifact_requested", "value": true}, {"op": "set", "field": "ending_id", "value": "E_B"}]`.
Đi tiếp: `scene:E_B`.

Dựng/kiểm: Command publish phải kiểm toàn bộ grants/attendance ảnh/lịch lại. PNG lỗi được thử lại; không coi yêu cầu export là đã có file.

**Lựa chọn `S10_PUBLISH_SUPPLEMENT`: Chốt bản vừa xem và lưu trang cuối**

Điều kiện: `{"all": [{"field": "page_previewed", "op": "eq", "value": true}, {"field": "final_mode", "op": "eq", "value": "supplement"}]}`.

**NARRATOR**: An xác nhận bản vừa xem để bàn giao. Một bản trang cuối được yêu cầu lưu cùng thông tin nguồn và ngày.

Hệ quả: `[{"op": "set", "field": "print_submitted", "value": true}, {"op": "set", "field": "final_artifact_requested", "value": true}, {"op": "set", "field": "ending_id", "value": "E_C"}]`.
Đi tiếp: `scene:E_C`.

Dựng/kiểm: Command publish phải kiểm toàn bộ grants/attendance ảnh/lịch lại. PNG lỗi được thử lại; không coi yêu cầu export là đã có file.

<a id="e-a"></a>
## E_A - Tấm mới

Nguồn: v4.1 trang 12, 13, 19.

Ngày nhận sách theo print_slot. Không gọi ảnh ba người là kết kém hơn. Chưa tự hòa giải tất cả.

### E_A_OPEN

An mở đúng trang đã chốt; dùng nguồn cũ đã chọn, không thay bằng ảnh khác ở cutscene.


**Lựa chọn `E_A_FOUR`: Mở trang có ảnh mới bốn người**

Điều kiện: `{"field": "final_new_asset", "op": "eq", "value": "PHOTO_NEW_FOUR"}`.

**NARRATOR**: Nhi nhìn nguồn cũ lâu hơn ảnh mới. Minh kéo cuốn sách về phía mình.
**MINH**: Tấm tao cúi thế này mà tụi mày cũng giữ à?
**HA**: Tại nó thật.
**NHI**: Gửi tao bản ảnh nhé.

Không tự đổi trạng thái ngoài chuyển bước.
Đi tiếp: `step:E_A_CALLBACK`.

**Lựa chọn `E_A_THREE`: Mở trang có ảnh mới ba người**

Điều kiện: `{"field": "final_new_asset", "op": "eq", "value": "PHOTO_NEW_THREE"}`.

**HA**: Tấm hôm thứ Sáu tao lưu riêng rồi.
**NARRATOR**: An gửi Nhi bản xem đúng phần đã được đồng ý. Không có câu hỏi vì sao cô không có mặt.

Không tự đổi trạng thái ngoài chuyển bước.
Đi tiếp: `step:E_A_CALLBACK`.
### E_A_CALLBACK

Chi tiết mở đầu trở lại theo gesture đã chọn, không tự sinh món đồ chưa giữ.


**Lựa chọn `E_A_CALLBACK_POSE_1`: Đặt trang vào cuốn sách**

Điều kiện: `{"field": "opening_gesture", "op": "eq", "value": "pose_1"}`.

**NARRATOR**: An nhìn nét chữ HẸN trong ảnh cũ, bàn tay vẫn làm sai một nét. Minh chỉ vào đúng chỗ đó rồi cười.

Hệ quả: `[{"op": "set", "field": "finished", "value": true}]`.
Đi tiếp: `end`.

**Lựa chọn `E_A_CALLBACK_POSE_2`: Đặt trang vào cuốn sách**

Điều kiện: `{"field": "opening_gesture", "op": "eq", "value": "pose_2"}`.

**HA**: Cuối cùng vẫn mỗi đứa nhìn một hướng.
**AN**: Tấm nào cũng vậy.

Hệ quả: `[{"op": "set", "field": "finished", "value": true}]`.
Đi tiếp: `end`.

**Lựa chọn `E_A_CALLBACK_POSE_3`: Đặt trang vào cuốn sách**

Điều kiện: `{"field": "opening_gesture", "op": "eq", "value": "pose_3"}`.

**MINH**: Có tấm nào tao không nhìn pin không?
**AN**: Có. Tấm đèn tắt.

Hệ quả: `[{"op": "set", "field": "finished", "value": true}]`.
Đi tiếp: `end`.

<a id="e-b"></a>
## E_B - Những gì được giữ lại

Nguồn: v4.1 trang 13, 19.

Trang có nguồn cũ cụ thể đã được phép. Không bắt người chơi chờ tin nhắn tha thứ để kết thúc.

### E_B_OPEN

Không còn ô chờ để trách người vắng. Cuốn sách mở được một trang hoàn chỉnh.

**HA**: Không có ảnh mới trong trang này nhỉ?
**AN**: Ừ. Tao bỏ ô chờ rồi.
**NARRATOR**: Ngón tay An dừng ở nguồn cũ vừa chọn. Ảnh mới nếu có vẫn nằm riêng, không bị xóa như hình phạt.

**Lựa chọn `E_B_SHOW`: Lật lại trang vừa hoàn thành**

**NHI**: Nguồn cũ này dùng được. Gửi tao bản vừa chốt nhé.
**AN**: Ừ, tao gửi đúng bản này.

Không tự đổi trạng thái ngoài chuyển bước.
Đi tiếp: `step:E_B_FINISH`.

Dựng/kiểm: Câu Nhi xác nhận phản ánh grant nguồn cũ; không tự nhận cô đồng ý ảnh mới.
### E_B_FINISH

Người chơi cất sơ đồ/file vào đúng chỗ; không hiện điểm hoặc nhãn thất bại.


**Lựa chọn `E_B_CLOSE`: Lưu bản cho Nhi và khép sổ**

**NARRATOR**: An chọn bản vừa được hỏi, gửi cho Nhi. Màn hình điện thoại tắt. Tiếng quạt ngoài phòng vẫn đều.

Hệ quả: `[{"op": "set", "field": "finished", "value": true}]`.
Đi tiếp: `end`.

<a id="e-c"></a>
## E_C - Một tờ giấy đến sau

Nguồn: v4.1 trang 13, 14, 19.

Chỉ vào khi later_status accepted với ngày/chỗ/người đã có câu xác nhận. Không tái dựng quầy đã tháo.

### E_C_ARRIVE

Ngày và giờ từ later_datetime; ghế đá sân trường. Hạ/Minh không bị triệu hồi. An mang tờ có nguồn cũ đã được duyệt.

**NHI**: Mang cả tấm buổi tập không?
**AN**: Có, trong máy. Tờ bổ sung tao cũng mang rồi.
**NHI**: Hạ với Minh xem chưa?
**AN**: Tao gửi bản trước khi in rồi.

**Lựa chọn `E_C_HAND_OLD`: Cùng xem nguồn cũ, không chụp thêm**

**NARRATOR**: Nhi kéo ảnh trên điện thoại, dừng ở nguồn cũ.
**NHI**: Ừ. Tờ này để tao kẹp vào sách.
**AN**: Để tao chỉnh mép gấp đã.

Hệ quả: `[{"op": "set", "field": "later_purpose", "value": "giao tờ bổ sung từ nguồn cũ"}]`.
Đi tiếp: `step:E_C_DELIVER`.

**Lựa chọn `E_C_ASK_NEW`: Hỏi có muốn chụp một tấm hai người ở ghế đá không**

**AN**: Máy còn hẹn giờ. Mày muốn chụp một tấm ở đây không?
**NHI**: Một tấm thôi, không bắt đứng theo ảnh cũ.
**AN**: Ừ, tao đặt máy chỗ này.

Hệ quả: `[{"op": "set", "field": "later_purpose", "value": "gặp, chụp hai người và giao bổ sung"}]`.
Đi tiếp: `step:E_C_CAPTURE`.
### E_C_CAPTURE

An đặt máy trên vị trí vững và chạy vào; không dùng Nhi như người cầm máy. Đây là lựa chọn sau lời nhận chụp.


**Lựa chọn `E_C_CAPTURE`: Bấm hẹn giờ và đứng vào khung**

**NARRATOR**: Máy bíp. Nhi dời cặp sang bên, An vẫn đang bước vào khi máy chụp.
**NHI**: Tấm này để nguyên đi.

Hệ quả: `[{"op": "set", "field": "photo_result", "value": "later"}, {"op": "set", "field": "captured_asset", "value": "PHOTO_LATER_TWO"}, {"op": "set", "field": "photo_participants", "value": ["AN", "NHI"]}]`.
Đi tiếp: `step:E_C_PHOTO_RIGHTS`.
### E_C_PHOTO_RIGHTS

Mở ảnh hai người mới; hỏi phạm vi tờ bổ sung, không sửa ngược cuốn sách đã phát.


**Lựa chọn `E_C_LATER_USE`: Hỏi dùng ảnh vừa chụp trong tờ bổ sung**

**AN**: Tấm này dùng trong tờ bổ sung được không? Tao gửi bản xem rồi mới giao.
**NHI**: Được, ảnh này dùng được. Gửi tao bản ảnh luôn nhé.
**AN**: Phần của tao cũng đồng ý.

Hệ quả: `[{"op": "set", "field": "later_photo_consent", "value": "yes"}, {"op": "set", "field": "final_new_asset", "value": "PHOTO_LATER_TWO"}, {"op": "grant", "asset_id": "PHOTO_LATER_TWO", "subject_id": "AN", "scope": "supplement_delivery", "status": "yes", "source_event_id": "E_C_LATER_USE"}, {"op": "grant", "asset_id": "PHOTO_LATER_TWO", "subject_id": "NHI", "scope": "supplement_delivery", "status": "yes", "source_event_id": "E_C_LATER_USE"}]`.
Đi tiếp: `step:E_C_PRINT`.

**Lựa chọn `E_C_LATER_PRIVATE`: Giữ riêng ảnh mới, tờ bổ sung dùng nguồn cũ**

**NHI**: Tấm mới giữ riêng thôi nhé. Tờ cũ được rồi.
**AN**: Ừ, tao không thêm tấm mới vào tờ.

Hệ quả: `[{"op": "set", "field": "later_photo_consent", "value": "no"}, {"op": "grant", "asset_id": "PHOTO_LATER_TWO", "subject_id": "AN", "scope": "private_album", "status": "yes", "source_event_id": "E_C_LATER_PRIVATE"}, {"op": "grant", "asset_id": "PHOTO_LATER_TWO", "subject_id": "NHI", "scope": "private_album", "status": "yes", "source_event_id": "E_C_LATER_PRIVATE"}]`.
Đi tiếp: `step:E_C_DELIVER`.
### E_C_PRINT

Chuyển cảnh sau khi bản thêm ảnh được Nhi xem và in xong. Không giả in ngay tại ghế đá.

**NARRATOR**: An gửi bản có ảnh mới cho Nhi xem. Sau khi bản được xác nhận, tờ được in để giao theo cách đã thống nhất.
**NHI**: Bản này được. Nhớ gửi file ảnh cho tao nữa.
**AN**: Tờ có ảnh mới phải in lại. Mai 12/05, 9:30 ở cửa lớp mày nhận được không?
**NHI**: Được. Gửi bản xem trước, mai tao nhận ở cửa lớp.

**Lựa chọn `E_C_PRINT`: Mang tờ đã in tới chỗ giao đã thống nhất**

**NARRATOR**: An mang tờ bổ sung trở lại cuộc giao đã thống nhất. Hạ và Minh nhận bản riêng qua bàn giao, không xuất hiện trong ảnh hai người.

Hệ quả: `[{"op": "set", "field": "supplement_delivery_datetime", "value": "2026-05-12T09:30:00"}, {"op": "set", "field": "supplement_delivery_place", "value": "cửa lớp"}]`.
Đi tiếp: `step:E_C_DELIVER`.

Dựng/kiểm: Cắt cảnh tới mốc giao vừa được cả hai xác nhận; không phát sinh lịch từ lời hứa một phía. TV3 có thể dùng nền hành lang đã có.
### E_C_DELIVER

Tại supplement_delivery_datetime/place đã xác nhận. Giao giấy cho Nhi, người nhận tờ vật lý duy nhất của kế hoạch này; Hạ/Minh đã nhận bản xem.

**NARRATOR**: Một mép giấy gấp lệch. An mở ra gấp lại, Nhi giữ cuốn sách.
**NHI**: Được rồi.
**AN**: File tao gửi cùng bản xem rồi.

**Lựa chọn `E_C_DELIVER`: Chỉnh mép và giao tờ bổ sung**

**NARRATOR**: Tờ ảnh nằm trong cuốn sách của Nhi. Bản xem đã gửi Hạ và Minh; ảnh mới nếu có vẫn ghi đúng người hiện diện.

Hệ quả: `[{"op": "set", "field": "supplement_delivered", "value": true}, {"op": "set", "field": "final_artifact_requested", "value": true}]`.
Đi tiếp: `step:E_C_FINISH`.
### E_C_FINISH

Dừng ở hiện vật đã giao. Không hứa quan hệ trở lại như cũ.


**Lựa chọn `E_C_FINISH`: Khép cuốn sách**

Điều kiện: `{"field": "supplement_delivered", "op": "eq", "value": true}`.

**NARRATOR**: Nhi vuốt phẳng mép giấy rồi khép sách. Không ai đề nghị chụp thêm một tấm cho xong chuyện.

Hệ quả: `[{"op": "set", "field": "finished", "value": true}]`.
Đi tiếp: `end`.
