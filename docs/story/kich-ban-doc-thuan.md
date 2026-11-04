# Kịch bản đọc và góp ý - CIVI / Trang 17

Bản biên soạn TV1 0.1, 22/10/2026, chờ kiểm nhận. Các nhánh là lựa chọn thay thế, không đọc nối tất cả trong một lượt. Sự kiện theo PDF v4.1; câu nối và lịch gặp/giao bổ sung mẫu là phần biên soạn mới.

Bạn có thể ghi góp ý bằng scene ID và choice ID. Trước giao TV2 phải đồng bộ về JSON. Bản có đầy đủ điều kiện/effect nằm trong [kịch bản kỹ thuật](kich-ban-mvp.md).

## S00 - Đừng bấm vội

Buổi tập tháng Mười; không gán ngày cụ thể. Hai lần chụp hỏng là hiện vật, không màn thất bại.

### S00_MACHINE

*An kéo dây ra khỏi lối đi. Điện thoại nghiêng trên thùng; Nhi đang cài giờ.*

**Nhi:** Khoan, máy đang trượt.

**Hạ:** Đừng kê bằng tập đề của Minh.

**Minh:** Cuối cùng cũng có người hỏi ý kiến tao.

**Chọn: Kê lại thùng và giữ điện thoại** (`S00_BOX`)

**An:** Để tao giữ. Mày cài tiếp đi.

**Nhi:** Giữ bên này, đừng che camera.

→ Bước S00_POSE.

**Chọn: Giữ chân kẹp cho Nhi cài giờ** (`S00_CLAMP`)

**An:** Chân kẹp tao giữ rồi.

**Nhi:** Ừ. Ba giây thôi, đừng buông sớm.

→ Bước S00_POSE.

**Chọn: Nhờ Nhi kiểm kẹp, mình giữ dây** (`S00_ASK`)

**An:** Mày xem kẹp được không? Tao giữ dây.

**Nhi:** Được. Đừng kéo căng quá.

→ Bước S00_POSE.

### S00_POSE

*Bảng chỉ sáng chữ HẸN. Cả ba rủ An thử cách đứng khác nhau.*

**Hạ:** Hay cùng nhìn bên kia?

**Minh:** Hay đứng yên.

**Nhi:** Mỗi đứa làm một chữ đi.

**Chọn: Giơ tay làm nét chữ HẸN** (`S00_POSE1`)

**An:** Tao nhận chữ dễ nhất.

**Minh:** Có chữ nào không cần giơ tay không?

→ Bước S00_CAPTURE.

**Chọn: Cùng nhìn sang một phía** (`S00_POSE2`)

**Hạ:** Bên này. Minh, bên này cơ.

**Minh:** Tao nhìn rồi, tại mắt tao vậy.

→ Bước S00_CAPTURE.

**Chọn: Đứng bình thường, trêu Minh đừng nhìn pin** (`S00_POSE3`)

**An:** Lần này cấm nhìn pin.

**Minh:** Hết pin thì đừng nhìn tao.

→ Bước S00_CAPTURE.

### S00_CAPTURE

*Đặt máy, bấm hẹn giờ, An chạy vào. Lần một đèn tắt; lần hai Minh cúi nhặt bìa.*

**Nhi:** Đủ bốn mới bấm.

**Minh:** Máy tự bấm.

**Nhi:** Thì mày thương lượng với máy đi.

**Chọn: Đặt lại giờ và chạy vào ảnh** (`S00_SHOOT`)

**Dẫn cảnh:** Tiếng bíp. Hạ vẫn đang cười khi máy bấm lần thứ ba.

**Hạ:** Lưu cả hai tấm trước đi.

**Minh:** Tấm tao cúi xuống ấy hả?

**Nhi:** Nhất là tấm đấy.

→ Bước S00_ALBUM.

### S00_ALBUM

*Ba thumbnail nằm cạnh nhau. Người chơi chọn tên album ngắn.*

**Chọn: Một lần đủ bốn** (`S00_ALBUM1`)

→ Bước S00_DIAGRAM.

**Chọn: Đèn chưa chịu sáng** (`S00_ALBUM2`)

→ Bước S00_DIAGRAM.

**Chọn: Minh đừng nhìn pin** (`S00_ALBUM3`)

→ Bước S00_DIAGRAM.

### S00_DIAGRAM

*Nhi vẽ hai ô, bốn tên, dán tia chớp. Ô phải để trống.*

**Nhi:** Tấm này để đây. Trước khi ra trường chụp lại, đúng chỗ này.

**An:** Lưu rồi. Cả hai tấm hỏng.

**Minh:** Cuộn băng ai cầm về?

**Dẫn cảnh:** Không ai trả lời. Nhi kéo sơ đồ khỏi cốc nước rồi cất vào túi bìa.

**Chọn: Cất sơ đồ cùng album** (`S00_KEEP`)

→ Cảnh S01.

## S01 - Trang ở giữa

Cắt từ ảnh đủ bốn sang bản in thử 16/18. Không mở bằng hồ sơ nhân vật. Minh giải thích ngay.

### S01_OPEN

*07/05/2026, 10:00. Tiếng quạt, lật giấy, bạn lớp khác cười ngoài cửa.*

**Hạ:** Trang 17 đâu?

**Minh:** Tao chuyển sang mục tạm giữ. Mày bảo bỏ mà.

**An:** Tao nói câu chú thích.

**Minh:** Trong tin nhắn có chữ chú thích đâu.

**Chọn: Mở tin nhắn tối hôm trước** (`S01_CHAT`)

**Dẫn cảnh:** Minh: “Trang chưa chốt, bỏ khỏi bản thử nhé?” Dưới đó là câu trả lời của An: “Ừ bỏ đi, mai tính”.

**Minh:** Bản cũ tao vẫn giữ. Không mất file.

→ Bước S01_COMPARE.

### S01_COMPARE

*Ảnh buổi tập cạnh ảnh hội chợ ba người; trên bàn là bố cục ba vị trí cho ngày mai.*

**An:** Mai chụp…

**Nhi:** Tụi mày cứ chụp.

**Dẫn cảnh:** Nhi đi tiếp cùng Phương, ôm thùng đồ. Hạ kéo ảnh hội chợ lại gần.

**Hạ:** Bản này chưa ai chốt. Nhi chưa nhận lời ảnh mới.

**Chọn: Mang sơ đồ xuống chỗ kiểm đồ** (`S01_FIND`)

→ Cảnh S02.

## S02 - Chiếc kẹp có cao su mới

Hub xem sơ đồ/kẹp/lịch tùy thứ tự; thông tin cốt lõi không cần giúp đủ lượt. Hỏi trực tiếp được đáp ngay.

### S02_HUB

*Nhi và Phương kiểm đồ. An có thể chạm từng vật hoặc hỏi trong lúc họ làm.*

**Phương:** Hai cuộn dây, một kẹp. Cái nhỏ ở đâu nhỉ?

**Nhi:** Trong túi bên hông. An, tránh cái thùng giúp tao.

**Chọn: Giữ thùng và giúp kiểm đồ** (`S02_HELP`)

*Chỉ hiện khi nhi_contact khác help.*

**An:** Hai cuộn? Cái này nữa là ba.

**Nhi:** Cái đấy dây hỏng. Để sang bên.

**An:** Ừ, tao giữ thùng.

→ Bước S02_HUB.

**Chọn: Xem lại sơ đồ hai ảnh** (`S02_DIAGRAM`)

*Chỉ hiện khi early_nhi_understanding là False.*

**An:** Sơ đồ này mày vẽ nhỉ?

**Nhi:** Ừ. Bản bây giờ có giống cái đó đâu.

→ Bước S02_HUB.

**Chọn: Thử chiếc kẹp vừa sửa** (`S02_CLAMP`)

*Chỉ hiện khi chưa/không (evidence_seen có PROP_CLAMP).*

**An:** Mày sửa cái này à?

**Nhi:** Nó trượt.

**An:** Mai dùng được không?

**Nhi:** Thì thử đi.

→ Bước S02_HUB.

**Chọn: Đọc lịch trên thùng** (`S02_CLOCK`)

*Chỉ hiện khi chưa/không (evidence_seen có PROP_SCHEDULE).*

**Dẫn cảnh:** 16:45 chụp; 17:10 Nhi đi; 17:30 tháo quầy. Sáng thứ Bảy giao file lúc 10:00.

**Nhi:** Lịch tao nhắn từ trước rồi. Mai không chờ thêm được đâu.

→ Bước S02_HUB.

**Chọn: Hỏi Nhi ai cầm máy hôm hội chợ** (`S02_ASK_CAMERA`)

*Chỉ hiện khi knows_handoff là False.*

**An:** Tấm ba đứa hôm hội chợ, ai chụp ấy nhỉ?

**Nhi:** Tao. Mày đưa máy cho tao mà. Bảo một tấm thôi, lát chụp đủ.

**An:** Rồi tao quên.

**Nhi:** Rồi đông khách. Tao cũng không nhắc nữa.

→ Bước S02_HUB.

**Chọn: Hỏi về lời nhắc đừng in** (`S02_CAPTION`)

*Chỉ hiện khi chưa/không (evidence_seen có PROP_CAPTION_OLD).*

**Nhi:** Tao bảo sửa lời tao, không bảo xóa cả trang. Câu kia tao đâu nói thế.

**An:** Còn tấm buổi tập?

**Nhi:** Mày định dùng thế nào thì cho tao xem.

→ Bước S02_HUB.

**Chọn: Mang lịch về bàn ảnh** (`S02_LEAVE`)

*Chỉ hiện khi evidence_seen có PROP_CLAMP và evidence_seen có PROP_SCHEDULE và evidence_seen có PROP_CAPTION_OLD.*

**Dẫn cảnh:** An ghim lịch cạnh album. Chiếc kẹp được đặt riêng trong ngăn đồ sẽ mang ra sân.

→ Cảnh S03.

## S03 - Ba ảnh khác nhau

Thí nghiệm A/B/C duy nhất. B/C dùng responses riêng và cùng luật với các nút. Hub không tự lộ video.

### S03_DESK

*Hạ mở ảnh gốc, kéo chúng cạnh nhau. Có thể phóng to, kéo rộng và hỏi bằng nút hoặc câu nhập.*

**Hạ:** Ảnh gốc ở đây, xem được. Tao chọn 0443 vì cả ba đang cười, bảng cũng sáng.

**An:** Cho tao xem cả mấy tấm trước.

**Chọn: Mở ảnh 0441** (`S03_LOOK_0441`)

*Chỉ hiện khi chưa/không (evidence_seen có PHOTO_0441).*

**Dẫn cảnh:** Đèn chưa sáng. Nhi ở mép, tay cầm dây. Chưa ai nhìn vào máy.

→ Bước S03_DESK.

**Chọn: Mở ảnh 0442** (`S03_LOOK_0442`)

*Chỉ hiện khi chưa/không (evidence_seen có PHOTO_0442).*

**Dẫn cảnh:** Đèn đã sáng. Ổ chia nằm trên ghế đỏ. Nhi đang nhìn sang An.

→ Bước S03_DESK.

**Chọn: Mở ảnh 0443** (`S03_LOOK_0443`)

*Chỉ hiện khi chưa/không (evidence_seen có PHOTO_0443).*

**Dẫn cảnh:** Kéo tới mép ảnh. Vẫn chỉ có An, Hạ và Minh. Không có phần hình nào giấu Nhi bên ngoài.

→ Bước S03_DESK.

**Chọn: So cận chiếc kẹp trống** (`S03_EMPTY`)

*Chỉ hiện khi chưa/không (evidence_seen có PHOTO_CLAMP_EMPTY).*

**An:** Hôm tập mình dùng hẹn giờ được mà.

**Hạ:** Ừ. Kẹp vẫn ở đó.

→ Bước S03_DESK.

**Chọn: Hỏi Hạ vì sao giữ 0443** (`S03_WHY`)

**Hạ:** Tấm này vẫn là chuyện thật của tụi tao. Không phải cứ thiếu một người là hôm đó không ai được vui.

→ Bước S03_REASON.

**Chọn: Tìm đoạn quay của Vy** (`S03_SOURCE`)

*Chỉ hiện khi evidence_seen có PHOTO_0441 và evidence_seen có PHOTO_0442 và evidence_seen có PHOTO_0443 và evidence_seen có PHOTO_CLAMP_EMPTY.*

**Hạ:** Vy có quay ở quầy bên cạnh. Nếu em ấy cho xem thì mình đối chiếu.

→ Cảnh S04.

### S03_REASON

*Ảnh 0443 đứng cạnh sơ đồ; câu hỏi không tự biến thành lời buộc tội được xác nhận.*

**Chọn: Trở lại ba ảnh** (`S03_REASON_BACK`)

→ Bước S03_DESK.

## S04 - Chín giây ngoài ảnh

Animatic/video có âm, phụ đề, thanh kéo. Mở cả nguồn khi được cho xem; không khóa tới cuối game.

### S04_SOURCE

*Vy cho mượn clip để kiểm chứng nội bộ.*

**Vy:** Các anh chị xem ở đây nhé. Nếu định dùng ở đâu khác thì phải hỏi lại những người trong hình.

**Hạ:** Ừ, chỉ xem để đối chiếu.

**Chọn: Xem đoạn trước ảnh 0443 cùng Hạ** (`S04_WATCH`)

**Dẫn cảnh:** Khung trước: Nhi tới muộn, túi dây nối còn trên vai. An trả lời cụt. Nhi nói gắt rồi quay sang chuyển ổ chia khỏi chỗ ướt.

**Minh:** Hôm đó hẹn 16:15, nó đến 16:31.

**Dẫn cảnh:** Khung gần lúc chụp: Minh đưa tay về phía kẹp. An nhấc điện thoại đưa cho Nhi.

**An:** Mày chụp giúp cái. Một tấm thôi.

**Dẫn cảnh:** Nhi chỉ chiếc kẹp.

**An:** Lát chụp đủ. Nhanh lên, có người tới rồi.

**Dẫn cảnh:** Tiếng đếm rồi khách gọi. Máy quay theo quầy, không có tấm đủ bốn trong phần tư liệu này.

**Nhi:** Ảnh thì khỏi cần mặt tao. Nhớ ghi phần tao làm là được.

**Hạ:** Ừ. Nhưng tụi mình đâu chụp nữa.

→ Bước S04_AFTER.

**Chọn: Giữ lời Nhi đã kể, chưa xem clip** (`S04_TESTIMONY`)

*Chỉ hiện khi knows_handoff là True.*

**An:** Tao hỏi rồi. Nhi nói chính tao đưa máy, hẹn lát chụp đủ.

**Hạ:** Ừ, tao nhớ đoạn đưa máy. Nguyên câu còn lại tao không chắc.

→ Bước S04_AFTER.

### S04_AFTER

*Ảnh vui vẫn trên bàn. Sổ chỉ ghi điều có nguồn, không ghi ý đồ của ai.*

**Hạ:** Mày muốn sửa trang thế nào? Nói bằng một bản nháp đi.

**Chọn: Mở các mẫu trang** (`S04_DRAFT`)

→ Cảnh S05.

## S05 - Trang đẹp hơn

Ba mẫu đều đẹp, không tô đúng/sai. Chỉ là nháp riêng, chưa xác nhận quyền in. Không có gửi nhóm lớp mặc định.

### S05_TEMPLATE

*Ba preview đặt ngang hàng; ảnh dùng trong preview được đánh dấu nháp riêng.*

**Hạ:** Mày thử đặt ảnh lên đây. Muốn sửa chỗ nào thì chỉ rõ.

**Minh:** Chốt xong gửi tao bản xem. Đừng gửi mỗi tên file.

**Chọn: Một trang cảm ơn với ảnh hậu trường** (`S05_TYPE_TRIBUTE`)

**An:** Hay làm một trang cảm ơn Nhi.

**Hạ:** Được. Tao thử ảnh lớn, chữ ngắn ở dưới.

→ Bước S05_RECIPIENT.

**Chọn: Hai ảnh trước và bây giờ** (`S05_TYPE_PAIRED`)

**An:** Tao muốn giữ hai ô như sơ đồ.

**Hạ:** Ô mới chưa có ảnh. Cứ để bản nháp, đừng lấy 0443 gọi là đủ bốn.

→ Bước S05_RECIPIENT.

**Chọn: Ảnh hỏng và sơ đồ trong trang ký ức** (`S05_TYPE_MEMORY`)

**An:** Chưa có ảnh mới thì làm từ những tấm đang giữ.

**Hạ:** Ừ. Nhưng chọn ít thôi, để còn nhìn được mặt.

→ Bước S05_RECIPIENT.

### S05_RECIPIENT

*Preview luôn hiện loại nháp, nội dung, phiên bản 1 và người nhận. Người chơi bấm xác nhận gửi.*

**Chọn: Gửi riêng Nhi xem trước** (`S05_SEND_NHI`)

**An:** Tao gửi nháp riêng. Mày xem lúc rảnh, chưa đưa đi in.

**Dẫn cảnh:** Tin đã gửi. Nhi đang trong lớp; phản hồi sẽ đến khi cô có thể xem.

→ Cảnh S06.

**Chọn: Gửi nhóm bốn người** (`S05_SEND_GROUP_TRIBUTE`)

*Chỉ hiện khi draft_type là tribute.*

**Hạ:** Mẫu này in được đấy. Tao còn giấy màu, cắt thử cái bảng nhé.

**Minh:** Vậy mai ai cầm pin?

**Dẫn cảnh:** Hạ lấy giấy, đặt dòng Cảm ơn Nhi lên tấm bảng còn trắng.

→ Cảnh S06.

**Chọn: Gửi nhóm bốn người** (`S05_SEND_GROUP_OTHER`)

*Chỉ hiện khi draft_type khác tribute.*

**Hạ:** Tao mở được rồi. Giữ bản này để bàn, chưa chốt.

**Minh:** Nhớ hỏi người có trong ảnh trước khi gửi in.

→ Cảnh S06.

**Chọn: Giữ trên máy, mang ra nói trực tiếp** (`S05_HOLD`)

**An:** Để tao mang đi hỏi trước. Chưa gửi nhóm.

**Hạ:** Ừ. Mày nhớ nói rõ đây là nháp.

→ Cảnh S06.

## S06 - Hai ký ức trên bàn

Nói và làm là hai bước. Một câu nhận lỗi không tự đổi ha_support. Không khóa nguồn khi chỉ bàn giao.

### S06_TALK

*Hạ đặt 0443 bên cạnh sơ đồ hai ô, chưa xóa tấm nào.*

**Hạ:** Mày nói như cả tấm này phải bỏ đi ấy.

**Hạ:** Hôm đó tụi mình cười thật. Mày muốn tao xóa hết à?

**Chọn: Nói rõ mình đã đưa máy cho Nhi** (`S06_OWN`)

**An:** Tao đưa máy cho Nhi. Phần đó tao phải nói.

**Hạ:** Ừ. Nhưng tấm này thì sao?

→ Bước S06_PLACE.

**Chọn: Bảo Hạ đã chọn ảnh làm thiếu Nhi** (`S06_BLAME`)

**An:** Mày chọn tấm không có nó thì trang mới thành thế này.

**Hạ:** Ảnh gốc ở đây. Đừng bắt tao nhận hết chuyện mày đã quên.

→ Bước S06_PLACE.

**Chọn: Chưa đáp, đặt hai ký ức cạnh nhau** (`S06_COMPARE`)

**Hạ:** Mày muốn giữ cái nào? Hay cả hai, nhưng đừng gọi chúng là cùng một chuyện.

→ Bước S06_PLACE.

### S06_PLACE

*Kéo 0443 vào album riêng hoặc thử góc trang có caption đúng. Chỉ là thỏa thuận làm nháp; quyền in vẫn hỏi sau.*

**Chọn: Thử giữ 0443; An tự viết lại phần chữ** (`S06_KEEP_AGREE`)

*Chỉ hiện khi an_owned_camera_choice khác deflected.*

**An:** Để nó như ảnh hội chợ ba người. Tao viết chữ, gửi mày xem lại.

**Hạ:** Được. Tao thử bố cục một buổi. Nhưng đừng gửi mỗi chữ bỏ nữa.

→ Cảnh S07.

**Chọn: Giữ 0443 ở album; nhờ Hạ thử trang hai ảnh** (`S06_PRIVATE_AGREE`)

*Chỉ hiện khi an_owned_camera_choice khác deflected.*

**An:** Ảnh này vẫn giữ. Trang 17 thử nguồn buổi tập trước, phần chữ để tao.

**Hạ:** Tao gửi cả hai bản. Mình thử rồi hỏi Nhi.

→ Cảnh S07.

**Chọn: Nhận file gốc và tự dàn trang** (`S06_HANDOVER`)

**Hạ:** Tao gửi đủ file. Mẫu đơn giản ở đây, phần còn lại mày làm nhé.

**An:** Ừ. Tối tao gửi bản chốt để mọi người xem.

→ Cảnh S07.

**Chọn: Giữ ảnh để xin dùng sau; nhận file và tự làm** (`S06_KEEP_HANDOVER`)

*Chỉ hiện khi an_owned_camera_choice là deflected.*

**Hạ:** Ảnh thì tao đưa. Nhưng tao không dàn thêm hôm nay. Muốn dùng, gửi bản chữ cuối cho tao xem.

→ Cảnh S07.

## S07 - Một lời mời cụ thể

Phản hồi nháp đi trước lời mời. Câu trả lời NPC là sự kiện, không phải choice tự đặt accepted. Quyền ảnh cũ hỏi riêng.

### S07_SHOW

*Nhi ra khỏi lớp. Nháp đã gửi mới được mở khi cô xem; bản giữ máy được An đưa trực tiếp.*

**An:** Đây là bản nháp tao muốn hỏi mày, chưa in.

**Dẫn cảnh:** Nhi nhìn màn hình, kéo về tấm buổi tập.

**Chọn: Đưa bản tôn vinh cho Nhi xem** (`S07_SHOW_TRIBUTE`)

*Chỉ hiện khi draft_type là tribute.*

**Nhi:** Mai khỏi làm bảng này.

**An:** Mày muốn sửa câu nào?

**Nhi:** Bỏ đoạn cảm ơn. Tao không nhận giải.

→ Bước S07_REACT.

**Chọn: Cùng xem ảnh trong bản nháp** (`S07_SHOW_OTHER`)

*Chỉ hiện khi draft_type khác tribute.*

**Nhi:** Tấm hai ảnh ấy. Tao vẽ cái đó.

**An:** Tao vẫn giữ sơ đồ.

**Nhi:** Ảnh mới ai chụp?

→ Bước S07_INVITE.

### S07_REACT

*Không có cảnh giận bắt buộc khi đã chọn hướng khác. Nếu đã có bảng, việc gỡ sẽ là hành động riêng.*

**Chọn: Dừng bản cũ và báo lại những người đã xem** (`S07_STOP_SENT`)

*Chỉ hiện khi draft_sent_to là group_four.*

**An:** Bản cảm ơn dừng nhé. Hạ, Minh đừng chuẩn bị theo bản đó nữa. Tao đổi sang trang ký ức.

**Hạ:** Tao đọc rồi. Cái bảng đang ở trong túi, ra sân nhớ lấy ra cất.

**Minh:** Ừ. Cho tao bản mới sau.

→ Bước S07_INVITE.

**Chọn: Dừng nháp riêng và đổi sang hai ảnh** (`S07_STOP_PRIVATE`)

*Chỉ hiện khi draft_sent_to khác group_four.*

**An:** Ừ, tao dừng bản này. Để tao làm theo hai ô rồi hỏi mọi người.

**Nhi:** Cho tao xem tấm cũ trước.

→ Bước S07_INVITE.

**Chọn: Giải thích cả nhóm làm vậy vì Nhi** (`S07_INSIST`)

**An:** Nhưng mọi người đang làm vì mày mà.

**Nhi:** Vậy tụi mày làm đi. Mai tao không ra chụp.

→ Bước S07_INVITE.

**Chọn: Ghi lại ý Nhi, để bàn cách gỡ với nhóm tại sân** (`S07_DISCUSS_AT_FIELD`)

**An:** Tao biết mày không muốn bảng đó. Tao sẽ nói rõ ở sân, chưa dùng vào ảnh.

**Nhi:** Đừng bắt tao cầm nó.

→ Bước S07_INVITE.

### S07_INVITE

*Lời mời nêu rõ chụp cùng, ai cầm máy và giờ; lựa chọn không là điểm tình bạn.*

**Chọn: Mời 16:45, máy hẹn giờ, không ép tư thế cũ** (`S07_INVITE_CLEAR`)

*Chỉ hiện khi invite_pressure là False.*

**An:** Mai 16:45 chụp cùng nhé? Tao lo máy hẹn giờ, không cần đứng y hệt tấm cũ.

**Nhi:** Được. 17:10 tao phải đi, chưa xong tao vẫn đi đấy.

**An:** Ừ. Mày đến là vào chụp, tao lo máy.

→ Bước S07_OLD_RIGHTS.

**Chọn: Hỏi Nhi có thu xếp được ngày mai không** (`S07_ASK_AVAILABILITY`)

*Chỉ hiện khi invite_pressure là False.*

**An:** Mai mày có thu xếp ra sân được không? Không kịp thì mình tính trang từ ảnh cũ.

**Nhi:** Mai tao còn trả đồ với Phương trước giờ kiểm tra. Tao không ra được.

**An:** Ừ. Tao không chờ mày ở sân.

→ Bước S07_OLD_RIGHTS.

**Chọn: Gửi lời mời rồi để Nhi xem lịch trả lời sau** (`S07_INVITE_SHORT`)

*Chỉ hiện khi invite_pressure là False.*

**An:** Mai ra chụp nhé? Tao gửi giờ trong nhóm, xem lịch rồi báo tao.

**Nhi:** Để tao xem đã.

**Dẫn cảnh:** Chưa có câu nhận lời. An không đánh dấu đã hẹn xong.

→ Bước S07_OLD_RIGHTS.

**Chọn: Dừng chuyện mời và hỏi phần ảnh cũ** (`S07_ACCEPT_DECLINE`)

*Chỉ hiện khi invite_pressure là True.*

**An:** Ừ. Tao không nhắn mày ra nữa. Còn phần ảnh cũ, cho tao hỏi rõ.

→ Bước S07_OLD_RIGHTS.

### S07_OLD_RIGHTS

*An mở đúng PHOTO_S00_GOOD. Đây là câu hỏi quyền ảnh cũ, không là lời mời mới.*

**An:** Tấm buổi tập này, tao để vào trang 17 được không? Chỉ ảnh và ngày, không câu gán kia.

**Nhi:** Được, tấm này dùng được. Gửi tao bản chốt nhé.

**Chọn: Ghi nhận quyền dùng đúng tấm vừa xem** (`S07_OLD_OK`)

→ Cảnh S08.

## S08 - Mang gì ra sân

Cắt cảnh sang 08/05. Bàn giao cụ thể trước sân; thời lượng thao tác là giả định nhịp để thử, không số nguồn.

### S08_ASSIGN

*Sáng thứ Sáu, trước 16:00. Lịch tháo quầy vẫn dán ở đó.*

**Minh:** Tối tao phải ôn. Pin tao mang, còn ai kiểm file?

**Hạ:** File gốc tao gửi rồi. Ai giữ máy thì ghi tên vào túi.

**Chọn: An nhận máy và xuất file; nhờ Minh bàn giao pin** (`S08_ASSIGN`)

**An:** Máy và bản xuất để tao. Minh đưa pin trước, tối kiểm giúp một lượt nếu kịp thôi.

**Minh:** Ừ. Tao chỉ ngồi tới giờ ôn.

**Dẫn cảnh:** An viết tên mình lên băng dính dán túi máy.

→ Bước S08_PRINT.

**Chọn: Chưa phân lại, mang file và pin đã được bàn giao** (`S08_ASSUME`)

**An:** Tao tưởng mọi người lo như cũ.

**Minh:** Tao đưa pin thôi, tối mày tự xuất nhé. Bản hướng dẫn tao gửi rồi.

**An:** Ừ, phần còn lại để tao.

→ Bước S08_PRINT.

### S08_PRINT

*Minh chỉ lịch thường và lịch giữ chỗ. Không có thao tác tự gửi in.*

**Chọn: Giữ lịch thường: giao file 09/05 lúc 10:00** (`S08_NORMAL`)

**An:** Cứ lịch thường. Thiếu ảnh mới thì tao làm từ ảnh cũ.

**Minh:** Ừ. Sách chiều thứ Hai.

→ Bước S08_CHECK.

**Chọn: Gọi giữ lịch dự phòng trước 17:00 thứ Sáu** (`S08_EXTEND`)

*Chỉ hiện khi courtyard_minute nhỏ hơn 60.*

**An:** Nếu cần, bọn em giao trước 12:00 thứ Bảy được không?

**Xưởng in:** Giữ chỗ được. Sách chuyển sang sáng thứ Ba, nhóm xác nhận nhé.

**An:** Dạ, nhóm nhận lịch đó.

→ Bước S08_CHECK.

### S08_CHECK

*16:40 tại sân. Bấm thử kẹp/pin trước khi Nhi tới hoặc quyết định không tới.*

**Hạ:** Kẹp còn trượt không?

**An:** Để tao thử.

**Minh:** Đừng thử bằng cách thả tay luôn.

**Chọn: Lắp máy, kiểm kẹp và pin (5 phút)** (`S08_READY`)

**Dẫn cảnh:** An kê thêm miếng cao su, thử hẹn giờ. Máy đứng vững.

**An:** Được rồi. Máy tự bấm, tao vào cùng.

→ Bước S08_ROUTE.

**Chọn: Mang đồ sang vị trí chụp, kiểm tiếp tại đó (5 phút)** (`S08_REPAIR_LATER`)

**Dẫn cảnh:** Túi máy đủ đồ. Kẹp chưa thử xong; lúc chụp cần kê lại hoặc đổi chỗ.

→ Bước S08_ROUTE.

### S08_ROUTE

*16:45. Chỉ nhận lời có sự kiện đến mới vào đường có Nhi.*

**Chọn: Đón Nhi tại sân** (`S08_ARRIVE`)

*Chỉ hiện khi nhi_invite_status là accepted.*

**Dẫn cảnh:** Nhi đặt cặp cạnh ghế, nhìn đồng hồ.

**Nhi:** Tao tới rồi. Đến 17:10 thôi nhé.

→ Cảnh S09_A.

**Chọn: Bắt đầu với những người đang có mặt** (`S08_ABSENT`)

*Chỉ hiện khi nhi_invite_status khác accepted.*

**Dẫn cảnh:** Hạ đặt pin lên quầy. Tin nhắn không có câu nhận lời mới. Chỗ cạnh máy vẫn trống.

→ Cảnh S09_B.

## S09_A - Cùng chụp một tấm mới

Không ép ảnh trở lại nguyên khuôn. Nếu Nhi đi trước capture, chuyển S09_B. Gỡ bảng không tự xóa lịch sử.

### S09_A_BOARD

*Kiểm đạo cụ trước dựng khung; chỉ hiện bảng nếu đã được chuẩn bị và chưa gỡ.*

**Chọn: Lấy bảng cảm ơn khỏi chỗ chụp** (`S09_A_REMOVE_BOARD`)

*Chỉ hiện khi tribute_board_prepared là True và tribute_board_removed là False.*

**An:** Bản này dừng rồi. Tao cất bảng đi.

**Hạ:** Để vào túi giấy, đừng gấp chữ.

**Nhi:** Ừ. Tao đứng bên này.

→ Bước S09_A_FRAME.

**Chọn: Giữ bảng trong khung và đề nghị Nhi cầm** (`S09_A_LEAVE_BOARD`)

*Chỉ hiện khi tribute_board_prepared là True và tribute_board_removed là False.*

**An:** Cầm thử một tấm thôi được không?

**Nhi:** Tao bảo không rồi. Tấm này tụi mày chụp đi.

→ Bước S09_A_LEAVE.

**Chọn: Đặt sơ đồ cũ cạnh máy, không dựng bảng** (`S09_A_NO_BOARD`)

*Chỉ hiện khi (tribute_board_prepared là False hoặc tribute_board_removed là True).*

**Hạ:** Tấm hội chợ vẫn giữ nhé.

**Nhi:** Tao đâu có bảo bỏ tấm đó.

→ Bước S09_A_FRAME.

### S09_A_FRAME

*Mọi người tự chọn chỗ; khung cũ không vừa bảng mới và túi sách.*

**Minh:** Chụp hay không thì chốt. Tao còn chuyển đồ.

**Chọn: Nới khung, để mỗi người tự chọn chỗ** (`S09_A_OPEN_FRAME`)

**An:** Không cần giống tấm trước. Mày đứng đâu tiện thì đứng.

**Nhi:** Thế tao đứng đây.

**Hạ:** Đợi tao hạ góc một chút.

→ Bước S09_A_CAMERA.

**Chọn: Đề nghị mọi người đứng y hệt tấm cũ** (`S09_A_OLD_POSE`)

**An:** Nhi sang phải chút, đúng chỗ trong ảnh trước.

**Nhi:** Mày muốn tấm đó hay muốn chụp chung? Tao không đứng diễn lại đâu.

→ Bước S09_A_REFUSE.

**Chọn: Hạ máy, hỏi ảnh mới sẽ dùng ở đâu** (`S09_A_CHECK_SCOPE`)

**An:** Mình chụp một tấm rồi cả nhóm xem. Chưa ai đồng ý thì chưa đưa vào trang.

**Nhi:** Ừ. Vậy chụp đi.

→ Bước S09_A_CAMERA.

### S09_A_REFUSE

*Nhi chưa rời; An có thể đổi cách chụp hoặc dừng. Không cần chọn câu xin lỗi mẫu.*

**Chọn: Bỏ dấu vị trí cũ, thử khung thoáng hơn** (`S09_A_CHANGE_FRAME`)

**Dẫn cảnh:** An kéo băng đánh dấu khỏi sàn.

**An:** Đứng như bây giờ cũng được.

**Nhi:** Ừ, đừng kéo cặp tao vào khung là được.

→ Bước S09_A_CAMERA.

**Chọn: Dừng buổi chụp với Nhi** (`S09_A_STOP_FRAME`)

**An:** Thôi, hôm nay không chụp chung nữa.

**Nhi:** Ừ. Tao qua trả đồ.

→ Bước S09_A_LEAVE.

### S09_A_CAMERA

*An giữ đầu dây, không mặc định giao Nhi. Thử lại kẹp nếu chưa kiểm xong.*

**Chọn: Kê lại kẹp và tự giữ dây (4 phút)** (`S09_A_FIX`)

*Chỉ hiện khi equipment_ready là repairable.*

**An:** Dây để tao giữ. Minh nhìn giúp máy có nghiêng không?

**Minh:** Bên trái thấp hơn. Rồi, được.

→ Bước S09_A_CAPTURE.

**Chọn: Đặt máy đã kiểm lên kẹp** (`S09_A_MOUNT`)

*Chỉ hiện khi equipment_ready là ready.*

**Dẫn cảnh:** An đặt máy vào kẹp, kéo dây gọn rồi bước thử vào khung.

→ Bước S09_A_CAPTURE.

**Chọn: Dựng lại toàn bộ quầy theo ảnh trước (25 phút)** (`S09_A_REBUILD`)

**Dẫn cảnh:** 16:45 đã qua. Khi An kê xong tấm bảng, Nhi đeo cặp lên.

**Nhi:** 17:10 rồi. Tao phải đi.

**An:** Ừ. Đồ còn lại để bọn tao.

→ Bước S09_A_LEAVE.

### S09_A_CAPTURE

*Mọi người đã thống nhất khung. Chỉ chụp nếu attendance/giờ vẫn hợp lệ.*

**Chọn: Bấm hẹn giờ rồi An chạy vào** (`S09_A_CAPTURE`)

*Chỉ hiện khi attendance là present và nhi_wants_capture là True và courtyard_minute nhỏ hơn 70 và equipment_ready là ready.*

**An:** Bíp rồi, đừng nhìn tao nữa, nhìn máy.

**Dẫn cảnh:** Minh cúi nhặt miếng bìa vừa rơi. Hạ bật cười. Nhi đứng lệch nửa bước so với sơ đồ.

**Hạ:** Giữ tấm này.

→ Bước S09_A_RIGHTS.

**Chọn: Cất chỗ Nhi trước khi thử chụp tiếp** (`S09_A_TIMEOUT`)

*Chỉ hiện khi (courtyard_minute ít nhất 70 hoặc attendance khác present).*

**Nhi:** Tao phải đi rồi.

**An:** Ừ. Tao không giữ mày thêm.

→ Bước S09_A_LEAVE.

### S09_A_RIGHTS

*Mở đúng ảnh mới vừa chụp, hỏi quyền dùng; không mặc định đồng ý từ việc đứng vào ảnh.*

**An:** Tấm này để vào trang 17 nhé? Nếu không thích mình chỉ giữ riêng theo ý mọi người.

**Chọn: Cho cả nhóm xem ảnh và hỏi dùng bản này** (`S09_A_USE_PHOTO`)

**Hạ:** Phần có tao in được, giữ album riêng cũng được.

**Minh:** Tao đồng ý cả hai. Gửi tao bản PDF nữa.

**Nhi:** Ừ, tấm này in và giữ trong album riêng được. Gửi tao cả tấm buổi tập nhé.

→ Bước S09_A_PACK.

**Chọn: Hỏi giữ riêng tấm này, dùng ảnh cũ cho trang** (`S09_A_PRIVATE_ONLY`)

**Nhi:** Tấm này tao chưa muốn đưa vào kỷ yếu. Giữ trong album riêng thôi.

**Hạ:** Ừ, bản trang mình dùng tấm khác.

**Minh:** Tao đồng ý giữ riêng.

**An:** Được, không đưa tấm này đi in.

→ Bước S09_A_PACK.

### S09_A_PACK

*Giờ đi không phụ thuộc số câu đọc. Thu đồ sau chụp; Nhi đi trước để kịp lớp.*

**Nhi:** Tao đi nhé.

**An:** Ừ. Bản chốt tao gửi sau.

**Chọn: Tháo máy, cất pin và bàn giao kẹp** (`S09_A_PACK`)

**Dẫn cảnh:** An tháo máy. Minh cất pin vào túi đã ghi tên. Vy nhận lại kẹp, Hạ giữ file ảnh.

→ Cảnh S10.

### S09_A_LEAVE

*Nhi rời phần chụp; khung hình kế tiếp chỉ có người còn ở sân.*

**Chọn: Để Nhi đi, quay lại với Hạ và Minh** (`S09_A_TO_ABSENT`)

**Dẫn cảnh:** Nhi khoác cặp đi về phía chỗ trả đồ. An đặt máy xuống, nhìn Hạ và Minh.

→ Cảnh S09_B.

## S09_B - Sân còn ba người

Đường vắng có đủ việc làm và cảnh kết, không là màn báo lỗi. Nhắn nhiều hay chờ không triệu hồi Nhi.

### S09_B_DECIDE

*Bảng HẸN sáng, ảnh cũ đủ bốn ở góc màn hình. Hạ giữ chân máy, Minh nhìn giờ.*

**Hạ:** Vậy… mình chụp không?

**Minh:** Tao giữ máy thêm một lát được. Nhưng mấy giờ xong?

**Chọn: Chụp một tấm ba người hôm nay** (`S09_B_THREE`)

*Chỉ hiện khi courtyard_minute nhỏ hơn 90.*

**An:** Chụp một tấm của ba đứa đang ở đây. Không gọi là đủ bốn.

**Hạ:** Ừ. Hạ máy xuống chút, lấy cả cái bảng vào.

→ Bước S09_B_THREE.

**Chọn: Không chụp hôm nay, cùng thu đồ** (`S09_B_NO_PHOTO`)

**An:** Thôi, hôm nay cất máy. Tao giữ sơ đồ, làm trang từ nguồn cũ.

**Minh:** Ừ. Cái chân bên kia gập trước.

→ Bước S09_B_PACK.

**Chọn: Đề nghị một cuộc gặp khác, không bắt buộc chụp** (`S09_B_LATER`)

**An:** Tao hỏi một buổi khác, ai được thì gặp. Chưa có trả lời thì hôm nay mình vẫn cất đồ.

→ Bước S09_B_LATER.

### S09_B_THREE

*An đặt máy vào kẹp, kiểm lại và chạy vào khung. Không sinh hình Nhi từ ảnh cũ.*

**Chọn: Kê máy, hẹn giờ và cùng vào ảnh (4 phút)** (`S09_B_TAKE_THREE`)

*Chỉ hiện khi courtyard_minute nhỏ hơn 86.*

**Dẫn cảnh:** An kéo ghế thừa ra khỏi khung rồi chạy vào. Minh vừa nhìn pin thì máy bấm.

**Minh:** Lại nhìn pin nữa rồi.

**Hạ:** Tao thích tấm này.

→ Bước S09_B_THREE_RIGHTS.

**Chọn: Tháo quầy đúng lịch, giữ ảnh cũ** (`S09_B_TOO_LATE`)

*Chỉ hiện khi courtyard_minute ít nhất 86.*

**Vy:** Sắp tháo quầy rồi, em nhận lại kẹp nhé.

**An:** Ừ. Tao gập chân máy trước.

→ Bước S09_B_PACK.

### S09_B_THREE_RIGHTS

*Xem ảnh vừa chụp; chỉ người thực sự xuất hiện xác nhận.*

**Chọn: Hỏi dùng ảnh ba người trong trang kỷ yếu** (`S09_B_THREE_PRINT`)

**Hạ:** Dùng tấm này trong trang và album riêng được. Ghi đúng hôm nay và ba người nhé.

**Minh:** Tao đồng ý hai phạm vi đó.

**An:** Tao cũng đồng ý. Không lấy nó thay lời hẹn đủ bốn.

→ Bước S09_B_PACK.

**Chọn: Giữ trong album riêng, chưa dùng cho trang** (`S09_B_THREE_PRIVATE`)

**An:** Mình giữ riêng tấm này nhé. Trang 17 tao dùng ảnh cũ.

**Hạ:** Ừ, gửi tao một bản.

**Minh:** Được.

→ Bước S09_B_PACK.

### S09_B_LATER

*Gửi lời đề nghị có ngày, chỗ và mục đích. Không quay cảnh gặp khi chưa có xác nhận.*

**Chọn: Hỏi gặp ghế đá 11/05 lúc 16:30 để xem ảnh và giao tờ bổ sung** (`S09_B_LATER_CLEAR`)

*Chỉ hiện khi invite_pressure là False.*

**An:** Thứ Hai 11/05, 16:30 ở ghế đá được không? Tao mang ảnh cũ với tờ bổ sung, không bắt buộc chụp.

**Nhi:** 16:30 được. Tao chỉ gặp mày thôi, Hạ với Minh có việc thì đừng kéo về. Mang cả tấm buổi tập nhé.

**An:** Ừ. Tờ bổ sung tao gửi nhóm xem trước khi in.

→ Bước S09_B_PACK.

**Chọn: Gửi đề nghị để Nhi xem lịch, không hẹn giờ thay cô** (`S09_B_LATER_PENDING`)

**An:** Tuần sau mày muốn nhận ảnh lúc nào thì báo tao nhé. Không cần ra chụp.

**Dẫn cảnh:** Chưa có trả lời nhận lời. An cất máy, không đánh dấu đã có cuộc gặp.

→ Bước S09_B_PACK.

**Chọn: Hỏi lại một lần sau cuộc nói chuyện còn căng** (`S09_B_LATER_REFUSED`)

*Chỉ hiện khi invite_pressure là True.*

**An:** Tuần sau mình gặp để tao nói rõ hơn nhé?

**Nhi:** Tao chưa muốn hẹn. Cứ làm bản từ ảnh cũ rồi gửi tao xem.

**An:** Ừ. Tao không đặt lịch thay mày.

→ Bước S09_B_PACK.

### S09_B_PACK

*Không cắt ngay tới tổng kết. Người chơi tháo máy, lấy sơ đồ và cất bảng nếu tồn tại.*

**Minh:** Tao cất máy nhé?

**An:** Để tao gập chân. Mày lấy pin.

**Hạ:** Tờ sơ đồ đừng để dưới đất.

**Chọn: Gập chân máy, cất bảng và sơ đồ** (`S09_B_PACK`)

**Dẫn cảnh:** An gỡ bản dựng ba vị trí, cuộn dây, trả kẹp cho Vy. Sơ đồ bốn tên được đặt trong túi bìa.

**Hạ:** Bản cuối mày gửi tao nhé.

**An:** Ừ, tao làm tiếp.

→ Cảnh S10.

## S10 - Trang nào được giữ

Tối thứ Sáu. Mọi nguồn được hỏi đúng scope. Người chơi nhìn đúng trang, lịch và phần còn chờ trước chốt. Không chấm điểm.

### S10_WORK

*File gốc mở được dù Hạ không giúp dàn thêm.*

**Chọn: Cùng Hạ đặt ảnh vào mẫu** (`S10_WITH_HA`)

*Chỉ hiện khi ha_support là collaborate.*

**Hạ:** Tao làm phần ảnh. Phần chữ mày tự chốt rồi gửi xem lại.

**An:** Ừ, tao không giữ câu gán cho Nhi.

→ Bước S10_NOTICE.

**Chọn: An mở mẫu đơn giản và tự dàn** (`S10_ALONE`)

*Chỉ hiện khi ha_support là handover_only.*

**Dẫn cảnh:** An mở mẫu một trang, kéo ảnh vào đúng ô. File Hạ bàn giao vẫn đủ.

**Hạ:** Bản chốt có ảnh tao thì gửi xem trước nhé.

→ Bước S10_NOTICE.

### S10_NOTICE

*Kiểm việc dừng bản tôn vinh. Một phiên bản mới không xóa người đã thấy phiên bản cũ.*

**Chọn: Báo dừng nháp cảm ơn còn chưa xử lý** (`S10_STOP_REMAINING`)

*Chỉ hiện khi draft_type là tribute và draft_retracted khác yes và draft_sent_to là group_four.*

**An:** Bản cảm ơn trước dừng nhé. Trang này không dùng câu đó hay bảng đó. Người đã xem nhận giúp tao bản mới.

**Nhi:** Ừ. Gửi bản mới cho tao xem.

**Hạ:** Tao không dùng bảng cũ nữa.

**Minh:** Tao đổi bản xem trong thư mục rồi.

→ Bước S10_OLD.

**Chọn: Tiếp tục từ bản hiện tại** (`S10_NOTICE_OK`)

*Chỉ hiện khi (draft_type khác tribute hoặc draft_retracted là yes).*

**Dẫn cảnh:** Bản hiện tại được mở cạnh lịch giao sách; bản cũ vẫn nằm trong lịch sử.

→ Bước S10_OLD.

**Chọn: Báo dừng nháp cảm ơn còn chưa xử lý** (`S10_STOP_PRIVATE_REMAINING`)

*Chỉ hiện khi draft_type là tribute và draft_retracted khác yes và draft_sent_to khác group_four.*

**An:** Bản cảm ơn riêng trước tao dừng. Nguồn cũ vừa chọn, tao sẽ cho mày xem chữ trước khi chốt.

**Nhi:** Ừ, gửi đúng bản mới nhé.

→ Bước S10_OLD.

### S10_OLD

*Chọn nguồn cũ cụ thể. Preview riêng và lời xin phép Nhi đi liền nguồn, không cấp quyền cho cả album.*

**Chọn: Ảnh buổi tập đủ bốn** (`S10_OLD_1`)

**An:** Tao giữ tấm đủ bốn đã hỏi mày, đúng ảnh này trong trang.

**Nhi:** Ừ. Tấm này dùng được.

→ Bước S10_NEW.

**Chọn: Ảnh hỏng khi đèn vừa tắt** (`S10_OLD_2`)

**An:** Còn tấm đèn tắt này, để trong trang 17 được không?

**Nhi:** Tấm đấy mặt tao buồn cười thật. Được, cứ để.

→ Bước S10_NEW.

**Chọn: Ảnh hỏng Minh đang cúi nhặt bìa** (`S10_OLD_3`)

**An:** Tao muốn dùng tấm Minh cúi xuống này. Mày xem nhé?

**Nhi:** Được, tao thích tấm đó. Hỏi cả Minh nữa.

→ Bước S10_NEW.

**Chọn: Sơ đồ hai ô và tia chớp** (`S10_OLD_4`)

**An:** Dùng sơ đồ mày vẽ như một bản nháp cũ được không? Không coi ô trống là lời trách ai.

**Nhi:** Được. Để cả hình tia chớp nhé.

→ Bước S10_NEW.

### S10_NEW

*Ảnh mới nếu có được đặt cạnh nguồn cũ; ảnh không có quyền in chỉ ở album riêng đã được đồng ý.*

**Chọn: Dùng ảnh mới bốn người trong trang** (`S10_NEW_FOUR`)

*Chỉ hiện khi captured_asset là PHOTO_NEW_FOUR và nhi_photo_consent là yes.*

**Dẫn cảnh:** Nguồn cũ bên trái, ảnh 08/05 gồm An, Nhi, Hạ, Minh bên phải. Caption ghi đúng ngày và người.

→ Bước S10_0443.

**Chọn: Dùng ảnh mới ba người trong trang** (`S10_NEW_THREE`)

*Chỉ hiện khi captured_asset là PHOTO_NEW_THREE và new_photo_print_allowed là True.*

**Dẫn cảnh:** Ảnh mới ghi An, Hạ, Minh; không gọi là ảnh đủ bốn hay gán lời cho Nhi.

→ Bước S10_0443.

**Chọn: Dùng nguồn cũ, bỏ khung chờ ảnh mới** (`S10_ONLY_OLD`)

**Dẫn cảnh:** An bỏ ô chờ, nới nguồn cũ ra. Nếu có ảnh mới giữ riêng, nó vẫn còn trong album đúng phạm vi.

→ Bước S10_0443.

**Chọn: Dùng trang thông báo ngắn, tự giao tờ bổ sung ở cuộc gặp đã xác nhận** (`S10_SUPPLEMENT`)

*Chỉ hiện khi later_status là accepted.*

**An:** Sách chính để trang thông báo bổ sung phát riêng. Tờ ảnh này tao mang tới buổi đã hẹn, mọi người xem trước nhé.

**Nhi:** Ừ. Nguồn cũ vừa xem dùng trong tờ bổ sung được.

**Hạ:** Tao xem tờ đó trước khi in.

**Minh:** Đừng dời cả lịch phát sách nếu chỉ giao tờ riêng.

→ Bước S10_0443.

### S10_0443

*0443 có thể ở album hoặc một góc phù hợp của trang/tờ, quyền được hỏi riêng.*

**Chọn: Thêm 0443 với chữ “Hội chợ tháng Mười - An, Hạ, Minh”** (`S10_ADD_0443`)

**An:** Tao đặt tấm hội chợ ở góc này, đúng ba tên và ngày tháng Mười.

**Hạ:** Ừ. Phần ảnh tao đồng ý, chữ thế này được.

**Minh:** Tao cũng đồng ý.

**An:** Tao đồng ý dùng phần có mình.

→ Bước S10_RIGHTS.

**Chọn: Giữ 0443 ở album, không thêm vào trang cuối** (`S10_KEEP_0443_PRIVATE`)

**An:** Tấm hội chợ vẫn giữ trong album như đã thống nhất, trang này dùng nguồn vừa chọn.

**Hạ:** Ừ. Gửi tao bản chốt.

→ Bước S10_RIGHTS.

### S10_RIGHTS

*Cả nhóm nhận đúng preview nguồn cũ và caption; không xin quyền bằng tên file không rõ nội dung.*

**An:** Đây là bản với nguồn cũ vừa chọn, chữ chỉ ngày và tên. Hạ, Minh xem phần có mình nhé.

**Hạ:** Bản này tao xem được, phần của tao dùng được.

**Minh:** Ừ, dùng được. Tao không ở lại xuất file tối nay, bản hướng dẫn tao gửi rồi.

**An:** Phần của tao đồng ý. Tao xuất và gửi bản chốt.

**Chọn: Ghi nhận quyền đúng nguồn và phạm vi đang dùng** (`S10_RIGHTS_OK`)

→ Bước S10_PREVIEW.

### S10_PREVIEW

*Hiện trang thật, danh sách nguồn/người, lịch theo print_slot và việc tự giao bổ sung nếu có. Không có thông báo đạo đức.*

**Minh:** Bản này?

**An:** Ừ, đúng bản này.

**Dẫn cảnh:** An nhìn lại tên album, xóa chữ final khỏi tên file rồi lưu bản có ngày.

**Chọn: Xem đủ trang và lịch giao** (`S10_PREVIEW`)

→ Bước S10_PUBLISH.

### S10_PUBLISH

*Chốt riêng sau kiểm quyền; yêu cầu runtime lưu PNG hiện vật, không giả định đã ghi file thành công.*

**Chọn: Chốt bản vừa xem và lưu trang cuối** (`S10_PUBLISH_NEW`)

*Chỉ hiện khi page_previewed là True và final_mode là new.*

**Dẫn cảnh:** An xác nhận bản vừa xem để bàn giao. Một bản trang cuối được yêu cầu lưu cùng thông tin nguồn và ngày.

→ Cảnh E_A.

**Chọn: Chốt bản vừa xem và lưu trang cuối** (`S10_PUBLISH_OLD`)

*Chỉ hiện khi page_previewed là True và final_mode là old.*

**Dẫn cảnh:** An xác nhận bản vừa xem để bàn giao. Một bản trang cuối được yêu cầu lưu cùng thông tin nguồn và ngày.

→ Cảnh E_B.

**Chọn: Chốt bản vừa xem và lưu trang cuối** (`S10_PUBLISH_SUPPLEMENT`)

*Chỉ hiện khi page_previewed là True và final_mode là supplement.*

**Dẫn cảnh:** An xác nhận bản vừa xem để bàn giao. Một bản trang cuối được yêu cầu lưu cùng thông tin nguồn và ngày.

→ Cảnh E_C.

## E_A - Tấm mới

Ngày nhận sách theo print_slot. Không gọi ảnh ba người là kết kém hơn. Chưa tự hòa giải tất cả.

### E_A_OPEN

*An mở đúng trang đã chốt; dùng nguồn cũ đã chọn, không thay bằng ảnh khác ở cutscene.*

**Chọn: Mở trang có ảnh mới bốn người** (`E_A_FOUR`)

*Chỉ hiện khi final_new_asset là PHOTO_NEW_FOUR.*

**Dẫn cảnh:** Nhi nhìn nguồn cũ lâu hơn ảnh mới. Minh kéo cuốn sách về phía mình.

**Minh:** Tấm tao cúi thế này mà tụi mày cũng giữ à?

**Hạ:** Tại nó thật.

**Nhi:** Gửi tao bản ảnh nhé.

→ Bước E_A_CALLBACK.

**Chọn: Mở trang có ảnh mới ba người** (`E_A_THREE`)

*Chỉ hiện khi final_new_asset là PHOTO_NEW_THREE.*

**Hạ:** Tấm hôm thứ Sáu tao lưu riêng rồi.

**Dẫn cảnh:** An gửi Nhi bản xem đúng phần đã được đồng ý. Không có câu hỏi vì sao cô không có mặt.

→ Bước E_A_CALLBACK.

### E_A_CALLBACK

*Chi tiết mở đầu trở lại theo gesture đã chọn, không tự sinh món đồ chưa giữ.*

**Chọn: Đặt trang vào cuốn sách** (`E_A_CALLBACK_POSE_1`)

*Chỉ hiện khi opening_gesture là pose_1.*

**Dẫn cảnh:** An nhìn nét chữ HẸN trong ảnh cũ, bàn tay vẫn làm sai một nét. Minh chỉ vào đúng chỗ đó rồi cười.

→ Khép truyện.

**Chọn: Đặt trang vào cuốn sách** (`E_A_CALLBACK_POSE_2`)

*Chỉ hiện khi opening_gesture là pose_2.*

**Hạ:** Cuối cùng vẫn mỗi đứa nhìn một hướng.

**An:** Tấm nào cũng vậy.

→ Khép truyện.

**Chọn: Đặt trang vào cuốn sách** (`E_A_CALLBACK_POSE_3`)

*Chỉ hiện khi opening_gesture là pose_3.*

**Minh:** Có tấm nào tao không nhìn pin không?

**An:** Có. Tấm đèn tắt.

→ Khép truyện.

## E_B - Những gì được giữ lại

Trang có nguồn cũ cụ thể đã được phép. Không bắt người chơi chờ tin nhắn tha thứ để kết thúc.

### E_B_OPEN

*Không còn ô chờ để trách người vắng. Cuốn sách mở được một trang hoàn chỉnh.*

**Hạ:** Không có ảnh mới trong trang này nhỉ?

**An:** Ừ. Tao bỏ ô chờ rồi.

**Dẫn cảnh:** Ngón tay An dừng ở nguồn cũ vừa chọn. Ảnh mới nếu có vẫn nằm riêng, không bị xóa như hình phạt.

**Chọn: Lật lại trang vừa hoàn thành** (`E_B_SHOW`)

**Nhi:** Nguồn cũ này dùng được. Gửi tao bản vừa chốt nhé.

**An:** Ừ, tao gửi đúng bản này.

→ Bước E_B_FINISH.

### E_B_FINISH

*Người chơi cất sơ đồ/file vào đúng chỗ; không hiện điểm hoặc nhãn thất bại.*

**Chọn: Lưu bản cho Nhi và khép sổ** (`E_B_CLOSE`)

**Dẫn cảnh:** An chọn bản vừa được hỏi, gửi cho Nhi. Màn hình điện thoại tắt. Tiếng quạt ngoài phòng vẫn đều.

→ Khép truyện.

## E_C - Một tờ giấy đến sau

Chỉ vào khi later_status accepted với ngày/chỗ/người đã có câu xác nhận. Không tái dựng quầy đã tháo.

### E_C_ARRIVE

*Ngày và giờ từ later_datetime; ghế đá sân trường. Hạ/Minh không bị triệu hồi. An mang tờ có nguồn cũ đã được duyệt.*

**Nhi:** Mang cả tấm buổi tập không?

**An:** Có, trong máy. Tờ bổ sung tao cũng mang rồi.

**Nhi:** Hạ với Minh xem chưa?

**An:** Tao gửi bản trước khi in rồi.

**Chọn: Cùng xem nguồn cũ, không chụp thêm** (`E_C_HAND_OLD`)

**Dẫn cảnh:** Nhi kéo ảnh trên điện thoại, dừng ở nguồn cũ.

**Nhi:** Ừ. Tờ này để tao kẹp vào sách.

**An:** Để tao chỉnh mép gấp đã.

→ Bước E_C_DELIVER.

**Chọn: Hỏi có muốn chụp một tấm hai người ở ghế đá không** (`E_C_ASK_NEW`)

**An:** Máy còn hẹn giờ. Mày muốn chụp một tấm ở đây không?

**Nhi:** Một tấm thôi, không bắt đứng theo ảnh cũ.

**An:** Ừ, tao đặt máy chỗ này.

→ Bước E_C_CAPTURE.

### E_C_CAPTURE

*An đặt máy trên vị trí vững và chạy vào; không dùng Nhi như người cầm máy. Đây là lựa chọn sau lời nhận chụp.*

**Chọn: Bấm hẹn giờ và đứng vào khung** (`E_C_CAPTURE`)

**Dẫn cảnh:** Máy bíp. Nhi dời cặp sang bên, An vẫn đang bước vào khi máy chụp.

**Nhi:** Tấm này để nguyên đi.

→ Bước E_C_PHOTO_RIGHTS.

### E_C_PHOTO_RIGHTS

*Mở ảnh hai người mới; hỏi phạm vi tờ bổ sung, không sửa ngược cuốn sách đã phát.*

**Chọn: Hỏi dùng ảnh vừa chụp trong tờ bổ sung** (`E_C_LATER_USE`)

**An:** Tấm này dùng trong tờ bổ sung được không? Tao gửi bản xem rồi mới giao.

**Nhi:** Được, ảnh này dùng được. Gửi tao bản ảnh luôn nhé.

**An:** Phần của tao cũng đồng ý.

→ Bước E_C_PRINT.

**Chọn: Giữ riêng ảnh mới, tờ bổ sung dùng nguồn cũ** (`E_C_LATER_PRIVATE`)

**Nhi:** Tấm mới giữ riêng thôi nhé. Tờ cũ được rồi.

**An:** Ừ, tao không thêm tấm mới vào tờ.

→ Bước E_C_DELIVER.

### E_C_PRINT

*Chuyển cảnh sau khi bản thêm ảnh được Nhi xem và in xong. Không giả in ngay tại ghế đá.*

**Dẫn cảnh:** An gửi bản có ảnh mới cho Nhi xem. Sau khi bản được xác nhận, tờ được in để giao theo cách đã thống nhất.

**Nhi:** Bản này được. Nhớ gửi file ảnh cho tao nữa.

**An:** Tờ có ảnh mới phải in lại. Mai 12/05, 9:30 ở cửa lớp mày nhận được không?

**Nhi:** Được. Gửi bản xem trước, mai tao nhận ở cửa lớp.

**Chọn: Mang tờ đã in tới chỗ giao đã thống nhất** (`E_C_PRINT`)

**Dẫn cảnh:** An mang tờ bổ sung trở lại cuộc giao đã thống nhất. Hạ và Minh nhận bản riêng qua bàn giao, không xuất hiện trong ảnh hai người.

→ Bước E_C_DELIVER.

### E_C_DELIVER

*Tại supplement_delivery_datetime/place đã xác nhận. Giao giấy cho Nhi, người nhận tờ vật lý duy nhất của kế hoạch này; Hạ/Minh đã nhận bản xem.*

**Dẫn cảnh:** Một mép giấy gấp lệch. An mở ra gấp lại, Nhi giữ cuốn sách.

**Nhi:** Được rồi.

**An:** File tao gửi cùng bản xem rồi.

**Chọn: Chỉnh mép và giao tờ bổ sung** (`E_C_DELIVER`)

**Dẫn cảnh:** Tờ ảnh nằm trong cuốn sách của Nhi. Bản xem đã gửi Hạ và Minh; ảnh mới nếu có vẫn ghi đúng người hiện diện.

→ Bước E_C_FINISH.

### E_C_FINISH

*Dừng ở hiện vật đã giao. Không hứa quan hệ trở lại như cũ.*

**Chọn: Khép cuốn sách** (`E_C_FINISH`)

*Chỉ hiện khi supplement_delivered là True.*

**Dẫn cảnh:** Nhi vuốt phẳng mép giấy rồi khép sách. Không ai đề nghị chụp thêm một tấm cho xong chuyện.

→ Khép truyện.
