# CHƯƠNG 7. NHỮNG CON SỐ BIẾT NÓI DỐI

<!-- civi-route: {"node":"C7","next":["II_A","II_B"]} -->
> **Luồng đọc:** Đọc tiếp: [II_A](Nhanh_II-A_Ban_Duyet.md) · [II_B](Nhanh_II-B_Ban_Duyet.md)

*Bản chuyển thể thoại — viết lại lần 2, áp dụng thiết kế quan hệ nhân vật V1.0.*

## Cảnh 1 — Đếm những người không có tên

*[Ba ngày trước cơn bão, Mai và Linh bắt đầu một công việc mà hệ thống GTC chưa từng được thiết kế để làm: đếm những người không có tên trong hồ sơ công dân.]*

*[Họ không thể dựa vào danh sách cư trú hợp lệ, bởi chính danh sách ấy đang bỏ sót người. Thay vào đó, hai người bắt đầu từ lượng nước lấy từ mạng, số lần thu gom rác, suất thực phẩm được phát và số lượt khám bệnh.]*

*[Một căn hộ có thể bị bỏ trống trên giấy tờ. Nhưng nếu trong suốt bảy năm, căn hộ ấy liên tục tiêu thụ nước và thải rác như một gia đình năm người, điều đó ít nhất cũng cần được kiểm tra.]*

*[Linh mở bản đồ sử dụng điện ban đêm. Mai đặt bên cạnh những trang nhật ký bảo trì và danh sách lớp học cộng đồng mà các giáo viên tình nguyện đã giữ lại.]*

**Mai:** Nếu một khu được ghi là không có cư dân, nhưng vẫn sử dụng nước và có rác được thu gom hằng ngày, mình có thể dựa vào đó để ước lượng số người sống ở đây không?

**Linh:** Có thể dùng làm dấu hiệu ban đầu. Nhưng chưa đủ để kết luận.

**Mai:** Vì sao?

*[Linh phóng to một con hẻm trên bản đồ.]*

**Linh:** Cậu nhìn chỗ này đi. Ban đêm đèn sáng liên tục, nhưng đây có thể là một xưởng chạy ca đêm, chưa chắc đã có người ở.

*[Cô chuyển sang biểu đồ cấp nước.]*

**Linh:** Còn chỗ này tiêu thụ nước rất cao. Nhưng nếu đường ống bị rò thì sao? Mình sẽ đếm cả lượng nước thất thoát thành số người mất.

**Mai:** Vậy phải có thêm nguồn đối chiếu.

**Linh:** Ừ. Tớ đang thử kết hợp ba nguồn: lượng tiêu thụ theo giờ, số lần giao thực phẩm và số người có mặt ở các lớp học cộng đồng.

**Mai:** Còn hồ sơ y tế?

**Linh:** Có thể giúp kiểm tra quy mô theo nhóm tuổi. Nhưng chuyện đó phải hỏi bác sĩ Thu trước. Mình không được tự lấy bệnh án của từng người.

*[Mai gật đầu, ghi lại các nguồn cần xác minh.]*

*[Họ tiếp tục đối chiếu với bản đồ ánh sáng, lịch làm việc của đội bảo trì và những danh sách học sinh chưa được đưa vào mạng giáo dục chính thức.]*

*[Mỗi nguồn cho họ một phần sự thật. Không nguồn nào đủ sức kể toàn bộ câu chuyện.]*

## Cảnh 2 — Một con số không phải một gia đình

*[Mai liên hệ với Thu để xin số liệu phục vụ kiểm tra.]*

**Mai:** Bác sĩ Thu, tôi cần đối chiếu số lượt khám bệnh ở các phân khu đang chờ xác minh cư trú. Bệnh viện có thể hỗ trợ không?

**Thu:** Có thể cung cấp số tổng theo nhóm tuổi và khu vực. Nhưng tôi không thể đưa hồ sơ từng bệnh nhân cho cô.

**Mai:** Tôi không cần tên hay chẩn đoán. Chúng tôi đang cố kiểm tra xem số người thực tế có chênh lệch bao nhiêu so với danh sách cư trú.

**Thu:** Nếu chỉ dùng số liệu tổng hợp thì tôi có thể hỗ trợ trong phạm vi được phép. Nhưng cô nhớ đừng lấy số lượt khám làm số người nhé.

**Mai:** Vì một người có thể đến khám nhiều lần?

**Thu:** Đúng. Và cũng có người bệnh mà không dám đến trạm. Số lượt khám không nói được hết ai đang sống ở đâu.

**Mai:** Tôi hiểu. Chúng tôi sẽ ghi rõ giới hạn đó.

*[Thu gửi phần thống kê được phép sử dụng. Không có tên bệnh nhân, địa chỉ riêng hay bệnh án cá nhân.]*

*[Đêm xuống, Linh tiếp tục kiểm tra các mô hình ước lượng.]*

**Linh:** Mai, có một điều tớ muốn cậu nhớ trước khi mình dùng những con số này.

**Mai:** Chuyện gì?

**Linh:** Mình có thể ước lượng đúng rằng thành phố đang đếm thiếu khoảng mười hai nghìn người. Nhưng vẫn có thể đoán sai hoàn toàn về một gia đình cụ thể.

**Mai:** Tớ biết số liệu sẽ có sai số.

**Linh:** Tớ không chỉ nói về sai số đâu.

*[Linh chỉ vào một nhóm điểm dữ liệu.]*

**Linh:** Giả sử mình cho rằng khu này có thêm năm hộ, rồi đề xuất cấp nguồn dựa trên giả định ấy. Nếu thực tế số người ít hơn hoặc nhu cầu khác đi, có thể một nơi đang cần điện hơn lại bị thiếu phần.

**Mai:** Vậy phải làm sao để vừa tính được những người bị bỏ sót, vừa không đưa ra kết luận quá chắc chắn?

**Linh:** Mình phải ghi rõ điều gì đã kiểm chứng được, điều gì mới là ước lượng. Và mỗi ước lượng cần có khoảng sai số.

*[Mai nhìn những đường cong trên màn hình.]*

*[Cô từng cho rằng chứng minh sai sót chỉ là tìm một con số chính xác hơn. Nhưng giờ đây, mỗi phép đo đều giống một cửa sổ chỉ nhìn được thành phố từ một phía.]*

*[Người làm ca đêm không bật đèn vào giờ mọi người ngủ. Một người sống cùng họ hàng có thể không nhận suất thực phẩm đứng tên mình. Có gia đình còn cố tình giấu số người đang sống chung vì sợ bị kiểm tra cư trú.]*

*[Công bố dữ liệu thiếu thận trọng có thể khiến chính những người ấy gặp nguy hiểm.]*

## Cảnh 3 — Giới hạn của việc tìm sự thật

*[Hai người bắt đầu thống nhất những giới hạn cho cuộc điều tra.]*

**Mai:** Tớ sẽ ghi vào phương pháp kiểm tra: không lập danh sách người chưa có giấy tờ đến từng căn hộ.

**Linh:** Và không được ghép bệnh án cá nhân với hồ sơ công dân.

**Mai:** Mỗi ước lượng phải ghi khoảng sai số, nguồn đối chiếu và người chịu trách nhiệm kiểm tra.

**Linh:** Còn một việc nữa. Mình phải hỏi người ở địa phương.

**Mai:** Về số cư dân?

**Linh:** Không chỉ thế. Cả đường đi, các phòng trú, nhà nào có người già không tự di chuyển được. Bản đồ hiện tại có thể thiếu những thứ đó.

*[Mai nghĩ một lúc, rồi mở một nguồn dữ liệu khác.]*

**Mai:** Nếu mình dùng hình ảnh từ camera thương mại để đối chiếu số người ra vào thì sao? Có thể kiểm tra được nhiều nơi hơn.

*[Linh lập tức lắc đầu.]*

**Linh:** Không được.

**Mai:** Tớ chỉ định dùng để đối chiếu, không phải công bố hình ảnh của họ.

**Linh:** Nhưng như thế nghĩa là mình đang mở rộng việc theo dõi những người vốn đã phải tìm cách tránh bị kiểm tra.

**Mai:** Còn nếu không dùng thì ước lượng của mình sẽ kém chính xác hơn.

**Linh:** Tớ hiểu. Nhưng mình đang sửa một việc bất công. Không thể lấy đó làm lý do để giám sát thêm tất cả mọi người.

*[Mai tựa lưng vào ghế.]*

**Mai:** Cậu có nhận ra mỗi lần mình thêm một giới hạn là báo cáo lại mất đi một phần độ chính xác không?

**Linh:** Có.

**Mai:** Thế làm sao thuyết phục được Hội đồng?

**Linh:** Bằng cách nói thật cả phần mình chưa chắc. Chứ không phải bằng cách khiến dữ liệu trông chắc chắn hơn khả năng của nó.

*[Mai nhìn lại bản báo cáo.]*

**Linh:** Chính những quy tắc được gọi là “chính xác” ngày trước đã loại những người không vừa với mẫu tính ra ngoài đấy thôi.

*[Cuộc tranh luận kéo dài đến ba giờ sáng.]*

*[Hai người mệt đến mức suýt cãi nhau vì một dấu phẩy trong phần thuyết minh phương pháp.]*

*[Cuối cùng, Mai đứng dậy pha cà phê. Cô đặt một cốc trước mặt Linh rồi ngồi xuống.]*

**Mai:** Này.

**Linh:** Gì thế?

**Mai:** Tớ nghĩ cậu nói đúng về chuyện camera.

*[Linh nhìn cốc cà phê.]*

**Mai:** Tớ không muốn sau này lại có một đứa trẻ như An phải sợ chỉ vì chúng ta biết quá nhiều chuyện về nhà nó.

*[Linh gật đầu.]*

**Linh:** Ừ. Tớ nghĩ đó mới là lý do đáng để hai đứa thức đến giờ này.

*[Họ quay lại màn hình, tiếp tục chỉnh phần giới hạn dữ liệu.]*

## Cảnh 4 — Người từng gửi cảnh báo

*[Sáng hôm sau, Mai và Linh gặp Tường, một nhân viên phân tích từng gửi cảnh báo về sai lệch dân cư từ hai năm trước.]*

*[Tường không còn giữ công việc cũ. Sau cảnh báo ấy, anh bị chuyển sang vị trí không có quyền truy vấn dữ liệu.]*

*[Anh xem giấy giới thiệu của Mai, rồi đặt xuống bàn.]*

**Tường:** Tôi biết hai người muốn tìm lại cảnh báo cũ. Nhưng tôi nói trước, tôi không muốn chuyện này trở thành một cuộc điều tra để tìm xem ai đáng bị đổ lỗi nhất.

**Mai:** Tôi hiểu. Chúng tôi cần biết anh đã phát hiện sai lệch gì, và cảnh báo đó được xử lý ra sao.

**Tường:** Tôi đã thử nhiều cách để báo rồi.

*[Anh nhìn xuống cuốn sổ trước mặt.]*

**Tường:** Ban đầu, tôi dùng cụm “rủi ro sai số dân cư”. Họ bảo nhận định quá chung chung.

**Tường:** Tôi sửa thành “nguy cơ đếm thiếu cư dân”. Họ lại bảo chưa có đủ kiểm chứng.

**Mai:** Vậy sau đó anh làm gì?

**Tường:** Tôi đưa một số trường hợp cụ thể để đối chiếu. Nhưng lần này họ nói tôi có nguy cơ vi phạm quy định bảo mật.

*[Tường cười nhạt, không có vẻ thấy chuyện ấy buồn cười.]*

**Tường:** Cuối cùng tôi không còn biết phải viết thế nào nữa. Tôi làm đơn xin điều chuyển.

**Linh:** Cảnh báo đó có được lưu lại trong hệ thống không?

**Tường:** Có. Nhưng trước khi bàn đến chuyện lấy lại nó, tôi muốn hai người hiểu vì sao hồ sơ bị hoãn.

**Mai:** Anh cứ nói.

**Tường:** Hồi đó những người duyệt hồ sơ còn phải xử lý một vụ đứt cáp điện và một đợt dịch bệnh nhỏ.

**Tường:** Họ đã ưu tiên những chuyện đang trực tiếp đe dọa tính mạng người dân trước mắt.

**Mai:** Nhưng điều đó không có nghĩa sai lệch dân cư không nguy hiểm.

**Tường:** Tôi biết. Tôi là người gửi cảnh báo mà.

*[Anh nhìn Mai.]*

**Tường:** Nhưng nếu cô chỉ nhìn kết quả rồi bảo những người đó cố tình làm ngơ, cô sẽ bỏ qua hoàn cảnh họ phải quyết định lúc ấy.

**Mai:** Tôi sẽ ghi rõ những gì có thể xác minh được. Tôi không định kết luận động cơ khi chưa có bằng chứng.

*[Tường ngồi im một lúc.]*

**Mai:** Vậy tại sao cuối cùng anh lại đồng ý gặp chúng tôi?

*[Anh nhìn về phía cửa sổ.]*

**Tường:** Tôi vừa thấy thông báo cắt điện của phân khu nơi mẹ tôi từng sống.

**Tường:** Tôi không muốn sau cơn bão mới phải đọc tên những người ở đó trong một báo cáo tổng kết thiệt hại.

*[Anh lấy ra một mảnh giấy ghi mã tham chiếu.]*

**Tường:** Đây là mã của cảnh báo cũ. Với quyền truy vấn hợp lệ, cô có thể tìm lại bản đã được lưu trong hệ thống.

**Mai:** Anh không giữ bản sao riêng sao?

**Tường:** Tôi có giữ bản gốc. Nhưng tôi không định trao nó ra ngoài theo một cách mà sau này không ai giải trình được nguồn dữ liệu.

**Mai:** Tôi hiểu. Cảm ơn anh đã cho chúng tôi mã tham chiếu.

*[Tường gật đầu.]*

*[Không có thủ thuật phá khóa hay dữ liệu bí mật nào được trao tay. Việc truy tìm hồ sơ vì thế chậm hơn, nhưng những gì Mai và Linh làm vẫn có thể được giải trình trước một phiên điều trần.]*

## Cảnh 5 — Mười hai nghìn người

*[Linh đặt ngưỡng sai số rất chặt khi bắt đầu dựng lại mô hình.]*

*[Cô không muốn một con số thiếu chắc chắn trở thành lý do để chuyển nguồn điện từ nơi đang cần sang nơi không thực sự có nhu cầu như ước lượng.]*

*[Nhưng mỗi lần mở rộng vùng kiểm tra, họ lại phát hiện thêm những phân khu giống 8-12.]*

*[Sau hai đêm, kết quả tổng hợp xuất hiện trên màn hình.]*

**Mai:** Linh... cậu xem lại con số này giúp tớ.

*[Linh ngồi xuống, kiểm tra các nguồn đối chiếu một lần nữa.]*

**Linh:** Tớ đã kiểm tra rồi.

**Mai:** Khoảng mười hai nghìn người?

**Linh:** Ừ. Đó là mức thiếu ước tính hiện tại. Nhưng cậu nhớ đọc cả khoảng bất định trong phụ lục nhé.

**Mai:** Tớ không nghĩ lại nhiều đến thế.

**Linh:** Tớ cũng vậy.

*[Mai nhìn bản đồ trước mặt.]*

*[Mười hai nghìn không phải con số cô có thể hình dung trọn vẹn.]*

*[Cô biết một đứa trẻ tên An, một người mẹ tên Hạ, ông Lâm thích cằn nhằn và Thắng, người từng ngồi đợi trước cửa phòng bệnh mà không biết mẹ mình phải chờ điều trị đến bao giờ.]*

*[Nhưng mười hai nghìn người nghĩa là còn vô số căn bếp, bậc cửa và đôi giày trẻ nhỏ mà Mai chưa từng nhìn thấy.]*

**Mai:** Cậu đã tìm được nguyên nhân ban đầu chưa?

**Linh:** Có một thay đổi trong dữ liệu ranh giới từ bảy năm trước.

*[Linh mở lịch sử chỉnh sửa.]*

**Linh:** Khi thành phố lập vùng bảo trì đặc biệt ở Rìa Mưa, những người sống trong vùng chuyển tiếp bị đánh dấu là “chờ xác minh”.

**Mai:** Việc xác minh lẽ ra phải hoàn tất khi nào?

**Linh:** Trong sáu tháng. Quyết định ban đầu ghi rất rõ.

**Mai:** Nhưng sau đó không ai thực hiện?

**Linh:** Có gia hạn. Sáu tháng thành một năm, rồi ba năm.

*[Cô chỉ vào dòng cuối của lịch sử cập nhật.]*

**Linh:** Sau đó không còn ai nhắc đến thời hạn nữa.

*[Mai nhìn màn hình.]*

**Mai:** Có ai từng yêu cầu sửa lại số liệu dân cư không?

**Linh:** Có. Ít nhất hai lần.

*[Linh mở một bảng ghi chú nội bộ.]*

**Linh:** Hai yêu cầu điều chỉnh đều bị hoãn.

**Mai:** Lý do?

**Linh:** Nếu bổ sung những người cần bảo vệ, chỉ số hiệu quả năng lượng trên một cư dân hợp lệ của toàn thành phố sẽ giảm. Khi đó báo cáo ngân sách phải được xem xét lại.

*[Mai đọc lại ghi chú.]*

**Mai:** Vậy là đã có người biết dữ liệu bị thiếu.

**Linh:** Có người biết một phần vấn đề. Nhưng tớ chưa có căn cứ để nói họ hiểu hết hậu quả về sau.

**Mai:** Dù sao cũng không thể coi đây là lỗi chưa từng được cảnh báo.

**Linh:** Chuyện đó thì đúng. Mình có hồ sơ để chứng minh.

*[Họ tiếp tục lần theo chữ ký trong các lần xử lý.]*

*[Các cấp quản lý đã thay đổi nhiều lần. Một giám sát viên nghỉ hưu. Một trưởng phòng chuyển sang bệnh viện. Một người chết trong tai nạn đường hầm.]*

*[Mỗi chữ ký thuộc về một thời điểm, một người có trách nhiệm trong phạm vi khác nhau.]*

*[Rồi Mai nhìn thấy một cái tên quen thuộc.]*

## Cảnh 6 — Chữ ký của Mai

*[Đó là tên Mai, nằm ở cuối một báo cáo kiểm toán hai năm trước.]*

*[Tiêu đề báo cáo ghi: “Tỷ lệ đáp ứng nhu cầu bảo vệ: 99,1 phần trăm.”]*

*[Mai nhớ rõ hồ sơ ấy.]*

*[Cô đã kiểm tra hàng trăm phép tính, rà soát các mẫu ngẫu nhiên và phản đối một ngoại lệ dành cho khu nghỉ dưỡng của một viên chức cấp cao.]*

*[Đó từng là một trong những công việc khiến cô tự hào.]*

*[Nhưng trong dữ liệu đầu vào, toàn bộ cư dân đang chờ xác minh đã bị loại khỏi mẫu tính. Khi ấy Mai không kiểm tra điều kiện loại mẫu.]*

*[Cô nhìn chữ ký điện tử nhỏ và gọn của mình ở góc màn hình.]*

**Mai:** Báo cáo này là của tớ.

*[Linh quay lại.]*

**Linh:** Cái nào?

**Mai:** Báo cáo kiểm toán hai năm trước. Chính tớ ký xác nhận tỷ lệ đáp ứng nhu cầu bảo vệ đạt 99,1 phần trăm.

*[Mai mở phần dữ liệu đầu vào.]*

**Mai:** Tớ đã kiểm tra gần như tất cả phép tính trong này.

**Linh:** Nhưng dữ liệu đầu vào đã loại những người chờ xác minh.

**Mai:** Ừ.

*[Mai chạm tay lên chữ ký điện tử. Không có gì xảy ra.]*

**Linh:** Mai, cậu không phải người tạo ra lỗi này.

**Mai:** Nhưng tớ đã ký xác nhận báo cáo không có sai sót.

**Linh:** Cậu xác nhận trong phạm vi kiểm toán được giao. Quy định chọn mẫu đã có từ trước.

**Mai:** Và tớ đã không kiểm tra lại phạm vi đó.

*[Linh im lặng.]*

**Mai:** Những người ấy không có mặt trong phép tính ngay từ đầu.

**Mai:** Thế mà tớ vẫn ký rằng hệ thống đã đáp ứng đúng nhu cầu bảo vệ.

*[Linh định nói thêm điều gì, rồi thôi.]*

*[Mai nhìn chữ ký một lần nữa. Cô không thể đổ toàn bộ sai lầm lên bản thân, nhưng cũng không thể coi mình chưa từng góp phần xác nhận một kết quả sai lệch.]*

*[Không lời an ủi nào của Linh có thể thay đổi điều ấy.]*

## Cảnh 7 — Cuộc họp trong xưởng bỏ không

*[Ngày hôm sau, Mai mang kết quả kiểm tra xuống Rìa Mưa.]*

*[Cô chuẩn bị mười ba biểu đồ, dự định giải thích rõ quy mô sai lệch để người dân hiểu và ủng hộ một phương án điều phối khẩn cấp.]*

*[Buổi họp được tổ chức trong một xưởng bỏ không. Có thợ bảo trì, người bán hàng, chủ xưởng, người già và những gia đình sống trong vùng chuyển tiếp.]*

*[Mai bắt đầu trình bày bản đồ dân cư ước lượng. Nhưng khi cô chuẩn bị mở phần dữ liệu chi tiết, Hạ lên tiếng ngắt lời.]*

**Hạ:** Khoan đã, cô Mai.

*[Mai dừng lại.]*

**Mai:** Chị có câu hỏi gì sao?

**Hạ:** Cô định đưa cả danh sách nhà cửa của chúng tôi ra cho những người từ Lõi Sáng xem à?

**Mai:** Tôi cần trình bày quy mô sai lệch để chứng minh số dân thực tế lớn hơn rất nhiều so với hồ sơ hiện tại.

**Hạ:** Thế có cần đưa cả địa chỉ từng nhà lên không?

**Mai:** Tôi đã xóa tên trong bản dữ liệu.

**Hạ:** Nhưng người ta vẫn có thể biết được đó là nhà ai nếu cô để cả vị trí chi tiết.

*[Mai nhìn lại màn hình.]*

**Hạ:** Rồi còn bệnh án, tiền nợ, chuyện gia đình chúng tôi nữa. Chẳng lẽ cái gì cũng phải kể ra thì người ta mới chịu tin là chúng tôi có thật?

**Mai:** Tôi không định công bố những thông tin riêng tư đó. Tôi chỉ muốn làm cho bằng chứng đủ sức thuyết phục.

**Hạ:** Nhưng chính chúng tôi mới là người phải sống với hậu quả nếu thông tin ấy bị đưa ra ngoài.

*[Giọng Hạ không lớn, nhưng cả xưởng im lặng.]*

**Hạ:** Cô từng hứa sẽ không làm tổn hại đến An.

*[Mai nhìn bà.]*

**Hạ:** Nếu nhờ con bé mà cô biết đến khu này, thì trước khi quyết định kể chuyện gì cho người khác nghe, cô cũng phải hỏi chúng tôi đã.

*[Mai nhìn những khuôn mặt trước mặt.]*

*[Cô đã chuẩn bị để giải thích cho họ thấy điều gì đang sai. Nhưng cô chưa thực sự hỏi họ muốn xử lý thông tin ấy như thế nào.]*

**Mai:** Chị nói đúng. Tôi đã quá tập trung vào việc chứng minh sai lệch mà chưa hỏi mọi người về phạm vi dữ liệu được phép sử dụng.

*[Cô tắt phần bản đồ chi tiết.]*

**Mai:** Vậy trước khi tiếp tục, tôi muốn nghe ý kiến của mọi người.

## Cảnh 8 — Không ai có cùng một giải pháp

*[Những ý kiến đầu tiên xuất hiện gần như cùng lúc.]*

**Người đàn ông làm việc tại nhà máy:** Nếu chuyện này làm lớn lên, nhà máy có khi bị kiểm tra rồi đóng cửa. Tôi còn phải đi làm nuôi gia đình. Có ai bảo đảm chúng tôi không mất việc không?

**Bà cụ trong khu:** Nhưng cổng trú ẩn phải mở ngay chứ! Người già như tôi đâu có chạy kịp nếu đợi đến lúc bão tới.

**Người thợ bảo trì:** Còn các hợp đồng thầu phụ thì sao? Những người thực sự sửa đường ống phải được ghi nhận đúng công việc. Chúng tôi muốn các hợp đồng đó được kiểm tra.

**Một cư dân khác:** Hay là mình rời khỏi thành phố trước khi bão đến?

**Người đàn ông đứng gần cửa:** Rời bằng cách nào? Tàu nào chở chúng ta đi? Bên ngoài là khí quyển độc, anh định đưa cả nhà đi đâu?

*[Cuộc tranh luận kéo dài.]*

*[Không ai có cùng một phương án. Người sợ mất việc, người cần nơi trú, người muốn sửa hồ sơ, người chỉ muốn gia đình sống qua cơn bão.]*

*[Hạ quay về phía mọi người.]*

**Hạ:** Tôi không muốn cô Mai đứng ra quyết định thay cả khu mình.

*[Bà nhìn Mai rồi nhìn những người trong xưởng.]*

**Hạ:** Nhưng tôi cũng muốn chúng ta được biết đầy đủ những gì bên điều phối đang biết.

**Hạ:** Họ hỏi gì, mình phải được hỏi như thế. Họ dựa vào điều gì để quyết định, mình phải được xem điều đó.

*[Mai không chen vào. Cô lật sang trang giấy trắng trong sổ.]*

*[Một người đàn ông đứng lên.]*

**Người đàn ông:** Nếu họ lập lại danh sách dân cư, thông tin của chúng tôi có bị đưa cho chủ lao động không?

**Người phụ nữ bán thực phẩm:** Tôi bán hàng trong khu này nhưng không có hợp đồng chính thức. Nếu khai tên ra, tôi có bị phạt vì bán hàng ở khu chưa đăng ký không?

**Ông Trụ:** Tôi có thẻ công dân, nhưng nhà tôi nằm trong vùng ranh giới bị sửa. Nếu họ xác minh lại thì tiền nước nhà tôi bị thu cao suốt bảy năm có được xem xét không?

**Thanh niên Rìa Mưa:** Thế còn nhà ở? Nếu họ xác định khu mình không đạt chuẩn, liệu có bắt chúng tôi chuyển đi không?

*[Mai nhìn mười ba biểu đồ đã chuẩn bị.]*

*[Không biểu đồ nào có thể trả lời ngay những câu hỏi ấy.]*

**Mai:** Tôi chưa có đủ thông tin để trả lời chắc chắn những việc này. Nhưng tôi sẽ ghi rõ từng yêu cầu, không gộp chúng thành một ý kiến chung.

*[Hạ gật đầu, rồi quay sang mọi người.]*

**Hạ:** Mọi người cứ nói những gì mình cần. Đừng chỉ hỏi cô Mai xem chúng ta nên cần gì.

**Hạ:** Cô ấy hiểu số liệu. Còn đường ống nào sắp vỡ, người nào không tự đi được, nhà nào có trẻ con hay sốt mỗi khi hệ thống lọc khí ngừng thì chính chúng ta biết rõ hơn.

*[Từ cuối xưởng, Tín chen vào.]*

**Tín:** Thế nếu được ghi tên vào hệ thống thì cháu có được vào sân bóng ở trường Lõi Sáng không ạ?

*[Một vài người bật cười.]*

**Người đàn ông gần cửa:** Người lớn đang bàn chuyện điện đóm mà cháu lại chỉ nghĩ đến đá bóng à?

**Tín:** Nhưng cháu muốn biết thật mà!

*[Tín nhìn Hạ.]*

**Tín:** Lần trước thẻ đi cùng của người lớn hết hạn, cháu còn không được vào sân công cộng nữa cơ.

*[An ngồi gần đó hiểu ngay.]*

*[Đối với Tín, sân bóng là điều cụ thể nhất mà quyền cư trú có thể mang lại. Trong khi người lớn sắp tranh luận về những điều khoản lớn, cậu chỉ muốn biết lần sau mình có được vào sân chơi hay không.]*

*[Một ông chủ xưởng đứng dậy.]*

**Ông chủ xưởng:** Tôi không phản đối chuyện ghi nhận cho người lao động. Nhưng nếu công khai toàn bộ hợp đồng thầu phụ rồi bắt kiểm tra ngay, xưởng tôi có thể phải đóng cửa.

**Ông chủ xưởng:** Khi đó hàng chục người mất việc, ai sẽ chịu trách nhiệm?

**Hạ:** Tôi đâu có muốn xưởng chú đóng cửa.

*[Bà nhìn những người thợ đứng cạnh.]*

**Hạ:** Nhưng cũng không thể vì sợ đóng xưởng mà cứ để người ta sửa đường ống đến kiệt sức, cuối cùng điểm đóng góp vẫn bằng không được.

**Ông chủ xưởng:** Vậy chị muốn giải quyết thế nào?

**Hạ:** Phải có cách ghi nhận đúng người làm. Còn nếu cần sửa lại hợp đồng thì cũng phải có thời gian để xưởng chuyển đổi, chứ không phải đóng cửa một cái là xong.

*[Ông chủ xưởng suy nghĩ.]*

**Ông chủ xưởng:** Nếu vậy, tôi muốn cùng ký kiến nghị. Phải ghi rõ thời hạn chuyển đổi hợp đồng, tránh chuyện vừa sửa quy định xong là công nhân bị cho nghỉ hàng loạt.

**Hạ:** Được. Chuyện đó phải ghi rõ.

*[Mai ghi thêm kiến nghị.]*

*[Một bà cụ đề nghị cắt điện khu thương mại ngay để dành nguồn cho nhà dân. Nhưng người có con đang làm ở khu thương mại phản đối: nếu các xưởng ngừng hết, gia đình họ sẽ không còn tiền mua thuốc.]*

*[Không có giải pháp nào hoàn toàn không gây thiệt hại.]*

*[Mai nhìn cuộc họp đang diễn ra.]*

*[Cô đến đây với ý định thuyết phục người dân ủng hộ phương án mình đã chuẩn bị. Bây giờ cô đang ghi những yêu cầu đôi khi mâu thuẫn với chính phương án ấy.]*

*[Nhưng mỗi bất đồng được nói ra đều cho cô biết một điều mà các thuật toán không thể tự tìm được.]*

## Cảnh 9 — Ước mơ không phải bằng chứng

*[Trong giờ giải lao, An mang một chai nước đến cho mẹ.]*

**An:** Mẹ ơi, mẹ uống nước đi ạ.

**Hạ:** Ừ, mẹ cảm ơn con.

*[An đứng bên mẹ một lúc rồi hỏi nhỏ.]*

**An:** Mẹ, lát nữa con có phải lên nói gì không ạ?

**Hạ:** Không, nếu con không muốn thì không cần.

**An:** Nhưng nếu con không nói, người ta có biết con muốn thi vào học viện không mẹ?

**Hạ:** Mẹ sẽ cùng mọi người đề nghị một cách xét hồ sơ công bằng hơn. Chuyện con muốn thi là chuyện riêng của con.

**An:** Nhưng nếu con kể chuyện của mình, có khi người ta sẽ thương rồi giúp con thì sao ạ?

*[Hạ nhìn con gái.]*

**Hạ:** Mẹ không muốn người ta phải thương con thì mới cho con vào học.

**An:** Nhưng con cũng muốn được đi học mà mẹ.

**Hạ:** Mẹ biết chứ.

*[Bà ngồi xuống để nhìn ngang tầm mắt An.]*

**Hạ:** Mẹ muốn có một cách xét hồ sơ để cả những đứa trẻ khác cũng được tham gia. Không phải đứa nào cũng phải đứng trước người lạ kể chuyện buồn của mình thì mới được cho một cơ hội.

*[An im lặng suy nghĩ.]*

*[Em nhìn sang Mai, lúc ấy đang hỏi một người thợ về vị trí các đường ống.]*

*[Ban đầu, An từng muốn cô Mai đứng lên nói cho mọi người biết mình thông minh thế nào, làm bài thi thử tốt ra sao và xứng đáng được vào học viện.]*

*[Nhưng giờ em nghĩ đến chuyện hàng nghìn người lạ có thể nghe cả việc mình đã khóc ở trạm y tế.]*

*[Em không còn thấy điều đó dễ chịu nữa.]*

*[An hiểu mẹ đang cố giữ cho em được sống như một đứa trẻ bình thường, không phải một câu chuyện buồn để người lớn đem ra chứng minh điều gì đó.]*

## Cảnh 10 — Quyền được sửa và quyền được giữ riêng

*[Khi cuộc họp kết thúc, Hạ ký vào biên bản với tư cách một người đại diện cộng đồng. Bàn tay bà vẫn còn những vệt dầu chưa rửa sạch.]*

*[Bên cạnh chữ ký, bà viết thêm một dòng:]*

*“Mọi số liệu công bố phải cho người bị ảnh hưởng quyền sửa và quyền giữ riêng đời sống cá nhân.”*

*[Mai đọc dòng chữ ấy.]*

**Mai:** Tôi sẽ đưa yêu cầu này vào phần đầu của báo cáo.

**Hạ:** Cô đừng chỉ ghi cho có nhé.

**Mai:** Không. Tôi hiểu nếu người dân không được kiểm tra thông tin về mình, việc công bố có thể gây thêm tổn hại.

*[Hạ nhìn cô một lúc rồi gật đầu.]*

*[Mai biết yêu cầu ấy sẽ khiến việc vận động trở nên phức tạp hơn. Nhưng cô vẫn giữ nguyên câu chữ của Hạ trong biên bản.]*

*[Cuối cùng, người dân chọn ra ba đại diện, trong đó có Hạ, để cùng xem các dữ liệu đã được ẩn danh.]*

*[Mai cung cấp phương án phân bổ năng lượng sơ bộ. Những người đại diện bổ sung các đường ống, phòng trú và lịch trực mà bản đồ thành phố chưa hề ghi lại.]*

*[Chỉ trong hai giờ, phần thông tin liên quan tới khu vực được kiểm tra và hoàn thiện nhiều hơn bất kỳ bản khảo sát từ xa nào.]*

*[Trong lúc ấy, An ngồi dưới gầm bàn, sửa chiếc ô cũ.]*

*[Em vẫn giận Mai từ khi nghe mẹ nói về danh sách nhà cửa.]*

*[Khi Mai đến gần, An kéo chiếc ô ra khỏi tầm tay cô.]*

**Mai:** An, cô ngồi đây với cháu một chút được không?

*[An không trả lời ngay. Em tiếp tục vặn một con ốc.]*

**An:** Cô Mai ơi... cô định kể chuyện của cháu cho cả thành phố biết thật ạ?

**Mai:** Không, nếu cháu không muốn.

**An:** Nhưng lúc nãy cô định đưa cả bản đồ lên mà.

*[Mai ngồi xuống cạnh em.]*

**Mai:** Ừ. Cô đã chuẩn bị bản đồ quá chi tiết. Cô chưa nghĩ đủ đến việc người khác có thể nhận ra từng gia đình.

**An:** Cháu muốn mọi người biết là có người sống ở khu mình.

*[An nhìn xuống chiếc ô.]*

**An:** Nhưng cháu không muốn ai cũng biết cháu đã khóc ở trạm y tế đâu ạ.

**Mai:** Cô hiểu.

*[Mai ngừng một lúc.]*

**Mai:** Cô xin lỗi, An. Cô cứ nghĩ chỉ cần làm cho mọi người nhìn thấy sự thật thì sẽ giúp được các cháu.

**Mai:** Nhưng cô quên mất rằng cháu cũng có quyền quyết định người khác được biết những chuyện gì về mình.

*[An không trả lời ngay. Em tiếp tục vặn con ốc còn dang dở.]*

*[Một lúc sau, em ngẩng lên.]*

**An:** Thế... trong bản đồ của cô, nhà cháu đã hiện lên chưa ạ?

**Mai:** Rồi. Có rồi, An ạ.

*[An nhìn Mai thêm một lát, rồi cúi xuống chiếc ô.]*

**An:** Vậy thì được rồi ạ.

*[Mai ngồi cạnh em thêm một lúc.]*

*[Không có lời tha thứ lớn lao nào. Cũng không cần An phải kể thêm một câu chuyện riêng tư để chứng minh mình đã có mặt ở đó.]*

## Cảnh 11 — Thẻ kiểm toán bị đình chỉ

*[Buổi chiều, thông báo đình chỉ quyền truy cập của Mai xuất hiện trên các màn hình của Cơ quan Điều phối.]*

*[Vinh cáo buộc cô đã truy vấn dữ liệu vượt thẩm quyền. Ông yêu cầu bàn giao báo cáo và tạm rời vị trí cho đến khi cuộc điều tra kết thúc.]*

*[Mai nhìn thông báo, rồi kiểm tra lại quyền truy cập. Những mục dữ liệu cô vẫn mở được buổi sáng giờ đã bị khóa.]*

*[Linh giữ lại các bản đối chiếu độc lập trong hồ sơ chuyên môn của mình. Cô không tiếp tục can thiệp vào máy chủ trung tâm.]*

*[Mai đặt thẻ công vụ lên bàn.]*

*[Chiếc thẻ ấy không còn mở được bất kỳ cánh cửa nào trong cơ quan.]*

*[Khi rời tòa nhà, Mai đi ngang quảng trường.]*

*[Màn hình thông báo ngoài trời đột ngột chuyển màu. Một bản tin khẩn xuất hiện.]*

**Hệ thống cảnh báo thành phố:** Cập nhật dự báo bão bụi điện từ. Khối bão ngoài khí quyển đang tăng tốc.

**Hệ thống cảnh báo thành phố:** Thời điểm đỉnh bão được điều chỉnh, đến sớm hơn dự báo trước đó ba mươi sáu giờ.

*[Mai dừng bước.]*

*[Phương án giảm công suất đã được phê duyệt.]*

*[Trên bảng danh sách các phân khu bị điều chỉnh nguồn điện, phân khu 8-12 nằm ngay dòng đầu tiên.]*

*[Mai nhìn những con số.]*

*[Cô đã tìm được bằng chứng cho thấy hàng nghìn người không có mặt trong phép tính. Nhưng quyền truy cập để tiếp tục điều tra vừa bị đình chỉ, còn cơn bão thì đang đến sớm hơn.]*

*[Điều cô phải quyết định tiếp theo không chỉ là có công bố sự thật hay không.]*

*[Cô còn phải quyết định công bố những gì, vào thời điểm nào, và ai có quyền kiểm tra trước khi thông tin ấy được đưa ra ngoài.]*

## Điểm rẽ II — Nói ra ngay, hay xác minh thêm?

*[Mai có trong tay bộ dữ liệu cho thấy thành phố đang đếm thiếu hàng nghìn cư dân. Nhưng mọi ước lượng đều có giới hạn, và chính những người bị bỏ sót có thể gặp nguy hiểm nếu thông tin cá nhân bị công bố thiếu thận trọng.]*

*[Trước khi cơn bão tới, cô phải chọn một trong hai cách tiếp tục.]*

**II-A — Sự thật trên một tờ giấy nhỏ**

*Mai chia sẻ sớm bản tổng hợp đã ẩn danh, kèm cảnh báo về sai số, cho các trạm y tế và đại diện cư dân để họ có thêm thời gian chuẩn bị.*

**II-B — Đêm dành cho những chỗ trống**

*Mai tiếp tục xác minh cùng cư dân, kiểm tra khoảng sai số và bảo vệ thông tin riêng tư trước khi đưa bằng chứng ra phiên điều phối.*

*Hai nhánh đều dẫn đến việc lập hồ sơ khẩn cấp ở Chương 8, nhưng có hậu quả khác nhau đối với thời gian chuẩn bị, chất lượng bằng chứng, quyền riêng tư và lòng tin.*

**— Hết phần tuyến chính Chương 7 —**
