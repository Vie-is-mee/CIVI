# CHƯƠNG 6. MỘT HỆ THỐNG ĐÚNG ĐẮN

<!-- civi-route: {"node":"C6","next":["C7"]} -->
> **Luồng đọc:** Đọc tiếp: [C7](Chuong_7_Ban_Duyet.md)

*Bản chuyển thể thoại — viết lại lần 2, áp dụng thiết kế quan hệ nhân vật V1.0.*

## Cảnh 1 — Kho lưu trữ dưới lòng đất

*[Bốn ngày trước cơn bão, giáo sư Khang đồng ý gặp Mai tại kho lưu trữ dưới lòng đất.]*

*[Ông không giống một người đã từng góp phần thiết kế trật tự của hai triệu dân. Áo khoác cũ, đôi giày có dây buộc không cùng loại, bàn tay run nhẹ khi mở ổ khóa.]*

*[Trên tường treo một bức ảnh Lưu Quang những năm đầu: mái vòm còn chưa khép kín, những chiếc lều dựng quanh trạm khí thở. Trong ảnh, không ai cười.]*

**Khang:** Cô Mai, cô muốn biết tại sao ngày trước chúng tôi lại tạo ra hệ thống Điểm Giá trị Công dân, đúng không?

**Mai:** Vâng. Nhưng điều tôi muốn biết hơn là tại sao bây giờ một người phải có điểm thì mới được công nhận là đang sống ở thành phố này.

*[Khang dừng tay trước chiếc tủ hồ sơ.]*

**Khang:** Đó không phải điều chúng tôi định làm khi bắt đầu.

**Mai:** Vậy ban đầu các ông định xây dựng một hệ thống như thế nào?

*[Khang kéo ra một hộp tài liệu cũ, đặt lên bàn.]*

*[Những tờ giấy đã sẫm màu ghi lại việc phân bổ điện thời thành phố chưa có hệ thống chung. Có năm, một bệnh viện mất điện liên tục, trong khi khu nhà của các thành viên Hội đồng được sưởi ấm suốt đêm. Người quản kho bán suất khí cho những gia đình giàu có. Ai quen biết người có quyền thì được đưa tên lên đầu danh sách cấp thuốc.]*

**Khang:** Trước đây, chuyện phân bổ tài nguyên hỗn loạn hơn bây giờ nhiều.

*[Ông lật một trang hồ sơ.]*

**Khang:** Chúng tôi từng thử cho mọi người xếp hàng. Nhưng cuối cùng người khỏe vẫn chen lên trước, còn người yếu thì bị đẩy xuống sau.

**Khang:** Rồi thử bốc thăm. Kết quả có lúc bệnh viện lại được cấp một suất điện ngang với nhà tắm công cộng.

**Mai:** Còn cách giao cho một người chịu trách nhiệm xét từng trường hợp?

**Khang:** Cũng thử rồi. Nhưng một người dù công tâm đến đâu vẫn có bạn bè, người thân. Chỉ cần không có ai kiểm tra, đặc quyền lại quay về.

*[Mai nhìn bảng cấp điện cũ. Những con số được viết tay, chỗ bị sửa chồng lên nhau đến mức gần như không đọc được.]*

**Mai:** Vậy các ông mới quyết định dùng một hệ thống chung?

**Khang:** Đúng. Nhưng trước khi nói đến điểm đóng góp, chúng tôi đã đặt ra hai nguyên tắc.

**Khang:** Thứ nhất, các dịch vụ thiết yếu phải được bảo đảm. Mọi người đều phải có một mức bảo vệ tối thiểu để sống.

**Khang:** Thứ hai, phần tài nguyên còn lại mới được phân bổ theo nhu cầu đã xác minh và đóng góp thực tế. Việc phân bổ phải được công khai, và người dân phải có quyền khiếu nại nếu phát hiện sai sót.

*[Ông mở một bản báo cáo khác.]*

**Khang:** Trong ba năm đầu, số ca tử vong vì mất điện giảm gần một nửa. Những gia đình từng không có tiền mua suất học cũng bắt đầu cho con đến trường được.

**Khang:** Trước đó, người giàu có thể mua gần hết những chỗ học tốt. Sau khi hệ thống được áp dụng, họ không còn làm thế dễ dàng nữa.

*[Mai nhớ đến mẹ, đến những khoản nợ viện phí và cơ hội học tập mà mình đã có.]*

**Mai:** Tôi là một trong những đứa trẻ được hưởng lợi từ thay đổi đó.

*[Khang nhìn cô. Ánh mắt ông dịu đi.]*

**Khang:** Vậy chắc cô hiểu tại sao tôi không muốn nghe người khác nói rằng cả đời mình chỉ tạo ra một thứ tàn nhẫn.

**Mai:** Tôi không nói công trình của ông chưa từng cứu ai.

**Khang:** Tôi biết.

*[Ông khép tập báo cáo.]*

**Khang:** Nhưng với những gì cô đang thấy, cô có lý do để nghĩ như vậy.

*[Mai không phản bác. Cô nhìn lại bức ảnh thành phố những năm đầu, rồi cúi xuống tập hồ sơ trước mặt.]*

## Cảnh 2 — Điều khoản không còn trên màn hình

*[Khang đưa Mai xem bản dự thảo đầu tiên của hệ thống GTC.]*

*[Giữa những điều khoản về phân bổ điện, khí và dịch vụ công, Mai tìm thấy một nội dung không xuất hiện trong giao diện quản lý hiện tại.]*

*[Điều khoản quy định: Không cá nhân nào bị từ chối mức bảo vệ sinh tồn vì tình trạng hồ sơ cư trú. Khi việc xác minh chưa hoàn tất, phải sử dụng ước lượng cư trú thực tế và có cơ chế giám sát công khai.]*

**Mai:** Khoan đã, điều khoản này...

*[Cô đưa ngón tay chỉ vào đoạn văn.]*

**Mai:** Theo những gì tôi biết, hệ thống hiện tại không cho phép cấp mức bảo vệ sinh tồn nếu hồ sơ cư trú chưa được xác minh.

**Khang:** Tôi biết.

**Mai:** Vậy điều khoản này đã bị bãi bỏ từ bao giờ?

**Khang:** Nó chưa từng bị bãi bỏ.

*[Mai ngẩng lên.]*

**Mai:** Ông nói sao?

**Khang:** Hội đồng chưa bao giờ bỏ phiếu xóa điều khoản đó.

**Mai:** Nhưng nếu nó vẫn có hiệu lực thì tại sao phần mềm lại vận hành ngược lại?

**Khang:** Vì người ta đã sửa cách triển khai. Qua hết lần cập nhật này đến lần cập nhật khác, điều khoản ấy biến mất khỏi giao diện.

*[Ông nhìn chiếc màn hình trên bàn.]*

**Khang:** Rồi lâu dần, mọi người bắt đầu làm việc như thể thứ gì không xuất hiện trên màn hình thì cũng không tồn tại.

*[Mai lật sang trang tiếp theo. Ở mép giấy có dòng ghi chú viết tay của Khang.]*

*“Phải có người chịu trách nhiệm khi con số và con người không khớp.”*

**Mai:** Đây là chữ của ông?

**Khang:** Phải.

**Mai:** Vậy ngay từ đầu ông đã biết hệ thống có thể bỏ sót người.

**Khang:** Tôi đã nghĩ đến nguy cơ đó.

**Mai:** Thế sao nó vẫn xảy ra?

*[Khang chậm rãi ngồi xuống.]*

**Khang:** Vì sau khi giải quyết được nạn cấp phát theo đặc quyền, chúng tôi bắt đầu tin rằng mình đã tìm được cách đúng.

**Khang:** Tôi cũng vậy. Tôi cứ nghĩ thắng được chuyện đó rồi thì những vấn đề còn lại sẽ dễ sửa hơn.

*[Ông nhìn những tập tài liệu xếp chồng.]*

**Khang:** Sau này tôi mới thấy... có những sai lầm không xuất hiện ngay lúc người ta thiết kế một hệ thống. Chúng lớn dần trong những lần mình chấp nhận bỏ qua một điều gì đó.

*[Hai người tiếp tục xem hồ sơ lâu hơn dự định.]*

*[Khang kể mình từng phản đối việc chuyển danh sách kháng nghị về cơ quan xác minh cư trú. Nhưng ông đã chấp nhận thỏa hiệp vì muốn hệ thống được đưa vào sử dụng đúng hạn.]*

*[Không có một người duy nhất để ông đổ lỗi. Chỉ có những nhượng bộ nhỏ, mỗi lần đều tưởng như không quá nghiêm trọng, chất chồng qua ba mươi năm.]*

## Cảnh 3 — Bản đồ từ năm thiếu khí

*[Khang dẫn Mai tới gian lưu trữ cuối cùng. Ở đây ông cất giữ những tấm bản đồ vẽ tay từ thời thành phố thiếu khí.]*

*[Trên một tờ giấy lớn là sơ đồ ba khu trú ẩn, nối với trạm điện chính bằng những mũi tên đen.]*

*[Khang đặt bàn tay lên một dấu mực đã nhòe.]*

**Khang:** Chỗ này từng là khu ở của em gái tôi.

*[Mai đứng lại cạnh ông.]*

**Mai:** Gia đình ông sống ở đây sao?

**Khang:** Em gái tôi sống ở đó. Hồi ấy con bé mới mười bảy tuổi, làm phụ bếp.

*[Ông nhìn vị trí của trạm cấp điện trên bản đồ.]*

**Khang:** Có một đêm trạm phải giảm áp. Người trực đóng van khi mọi người còn đang ngủ.

**Khang:** Sáng hôm sau, người ta bảo đó là một tai nạn kỹ thuật. Đến giờ tôi vẫn không biết ai đã ra lệnh đóng van.

*[Mai nhìn dòng ghi chú cũ bên cạnh sơ đồ. Nét mực đã phai đến mức khó đọc.]*

*[Khang kể em gái mình thường giấu lại một phần bánh vào mỗi tối để mang cho trẻ con ở khu khác.]*

*[Sau sự cố đóng van, nguồn điện được khôi phục cho dãy nhà quản trị trước. Hồ sơ của những người sống ở đó đầy đủ hơn, và họ có người đủ quyền gọi trực tiếp tới phòng vận hành.]*

*[Em gái Khang không chết ngay khi thiếu điện. Cô được đưa vào bệnh viện dã chiến và sống thêm bốn ngày.]*

*[Trong những ngày ấy, cô từng hỏi anh trai một câu mà ông vẫn nhớ.]*

**Khang:** Con bé hỏi tôi... có phải nó là người không đáng được cứu hay không.

*[Mai im lặng. Khang nhìn xuống tấm bản đồ, bàn tay vẫn đặt trên dấu mực đen.]*

**Khang:** Tôi không biết phải trả lời em thế nào.

*[Một lúc sau, ông tiếp tục.]*

**Khang:** Chính từ ngày đó, tôi đã tự hứa phải tìm cách phân bổ tài nguyên mà không phụ thuộc vào việc ai quen biết ai.

**Khang:** Hệ thống đầu tiên công khai tất cả các suất điện. Người dân chỉ cần nhìn vào bảng là biết nơi nào được ưu tiên, nơi nào phải giảm công suất.

**Khang:** Ít nhất, người quản kho không còn dễ dàng giấu nguồn để bán cho người quen nữa.

**Mai:** Ông nghĩ chỉ cần mọi quyết định đều được công khai thì sẽ không còn chuyện ưu ái người có quyền?

**Khang:** Hồi ấy, tôi đã tin như vậy.

*[Khang nhìn Mai.]*

**Khang:** Nhưng giờ cô cũng thấy rồi đấy.

**Mai:** Bảng thống kê hiện tại thậm chí còn không tính đủ người đang sống trong thành phố.

**Khang:** Đúng.

*[Ông khẽ gật đầu.]*

**Khang:** Một bảng thống kê có thể công khai đến từng con số, nhưng vẫn sai. Nhất là khi người ta không thấy được ai đã bị loại khỏi bảng ngay từ đầu.

## Cảnh 4 — Những lần thỏa hiệp

*[Khang lấy ra một tập biên bản khác. Trên bìa ghi thời điểm mười hai năm trước, khi thành phố chuyển từ xác nhận cư trú tại chỗ sang xử lý hồ sơ tự động.]*

**Khang:** Cô xem cuộc họp này đi. Đây là một trong những lần tôi đã nghĩ mình có thể giữ lại những điều quan trọng nhất.

*[Mai nhận tập hồ sơ.]*

**Khang:** Khi đó tôi đề nghị cho phép cộng đồng địa phương xác minh những người không có giấy tờ.

**Mai:** Nhưng trong quy định hiện tại, xác nhận của cộng đồng không được xem là nguồn hợp lệ.

**Khang:** Vì các phòng quản lý sợ người ta khai khống để nhận thêm suất điện và thuốc.

**Mai:** Nên họ yêu cầu hai nguồn xác minh độc lập?

**Khang:** Đúng. Ban đầu, hai nguồn đó có thể là một nhân viên y tế và một quản đốc khu dân cư.

*[Khang lật đến bản quy định được sửa sau đó.]*

**Khang:** Nhưng rồi qua vài lần cải tổ, cả hai nguồn đều phải là cơ quan đã có mã điện tử.

**Mai:** Nghĩa là nếu một người chưa có mã cư trú...

**Khang:** Thì rất khó để có hai xác nhận mà hệ thống chịu nhận.

**Mai:** Và hồ sơ cứ nằm ở trạng thái chờ.

**Khang:** Phải. Có những người thậm chí không vào được hàng chờ để bắt đầu thủ tục.

*[Mai nhìn chữ ký ở cuối biên bản.]*

**Mai:** Ông đã ký đồng ý với lần sửa này?

**Khang:** Tôi đã bỏ phiếu thuận trong lần sửa cuối.

**Mai:** Tại sao?

*[Ông không trả lời ngay.]*

**Khang:** Tôi nghĩ nguy cơ bỏ sót một số trường hợp vẫn thấp hơn nguy cơ có người làm giả hồ sơ để chiếm suất thuốc của người khác.

**Mai:** Bây giờ ông có hối hận không?

**Khang:** Có.

*[Khang đáp không do dự.]*

**Khang:** Nhưng tôi cũng không thể nói rằng hồi ấy tôi chẳng biết gì. Nói như thế chỉ để mình dễ ngủ hơn thôi.

**Khang:** Tôi biết có nguy cơ. Tôi chỉ không hình dung được nó sẽ lớn đến mức này.

*[Mai mở sổ tay, bắt đầu ghi số hiệu các nghị quyết. Khang đưa tay ngăn lại.]*

**Khang:** Cô Mai, còn một việc nữa. Cô nhớ ghi cả số người chết trước khi có GTC.

**Mai:** Vì sao ông nhắc điều đó?

**Khang:** Vì nếu báo cáo của cô chỉ nói về những người bị hệ thống làm tổn thương, những người từng được nó cứu sẽ nghĩ cô muốn phủ nhận tất cả những gì họ đã trải qua.

**Mai:** Tôi đâu có định phủ nhận chuyện đó.

**Khang:** Tôi biết. Nhưng họ chưa chắc đã biết.

*[Ông nhìn tập hồ sơ trên bàn.]*

**Khang:** Muốn họ nghe phần sai, cô cũng phải kể đúng cả phần mà hệ thống từng làm được.

*[Mai cảm thấy lời ấy có phần giống một sự biện hộ.]*

*[Nhưng cô nghĩ đến mẹ. Đến những người từng được cứu nhờ một quyết định phân bổ, rồi sau này mới phát hiện cùng một cơ chế có thể bỏ quên những người khác.]*

*[Không ai dễ dàng chấp nhận rằng thứ từng cứu mình cũng có thể gây tổn thương cho người khác. Điều đó không khiến hệ thống trở nên đúng đắn; nó chỉ khiến việc nói ra toàn bộ sự thật khó khăn hơn.]*

## Cảnh 5 — Cuộc đối thoại trong hành lang tối

*[Chiều cùng ngày, Mai gặp Vinh trong hành lang tối của Trung tâm Điều phối.]*

*[Ông đứng trước màn hình dự báo, một tay cầm bánh mì, không mặc áo vest.]*

*[Thấy Mai, ông lên tiếng trước.]*

**Vinh:** Tôi đã xem hồ sơ phân khu tám rồi.

**Mai:** Vậy ông cũng thấy sai lệch trong số liệu dân cư?

**Vinh:** Có. Tôi không phủ nhận là dữ liệu đang có vấn đề.

*[Ông đặt chiếc bánh mì xuống.]*

**Vinh:** Nhưng cô Mai, bây giờ chúng ta đang đứng trước hai cách xử lý. Một là công khai toàn bộ sai lệch ngay lúc thành phố cần người dân tin vào phương án điều phối nhất. Hai là giữ hệ thống vận hành qua cơn bão rồi sửa dần những nơi có lỗi.

**Mai:** Những người ở phân khu ấy có thể không sống được đến sau bão để chờ chúng ta sửa.

**Vinh:** Còn nếu bây giờ chúng ta thông báo rằng mô hình phân bổ đã tính thiếu hàng nghìn người, cô nghĩ chuyện gì sẽ xảy ra?

**Mai:** Người dân cần được biết họ đang đối mặt với nguy cơ gì.

**Vinh:** Tôi không phản đối chuyện đó. Tôi hỏi chuyện gì sẽ xảy ra sau khi họ biết.

*[Vinh chỉ vào bảng dự trữ năng lượng.]*

**Vinh:** Nếu các khu không còn tin vào hạn mức, người ta có thể tích trữ điện, phá trạm hoặc đòi tự giữ nguồn riêng.

**Vinh:** Khi đó, phương án đang có còn vận hành được không?

*[Mai định phản bác, nhưng dừng lại.]*

*[Cô nhớ đêm ở bệnh viện, khi hai giường bệnh cùng cần một nguồn điện.]*

**Mai:** Tôi không biết chắc.

**Vinh:** Đấy. Tôi cũng không biết chắc.

*[Ông vặn nắp bình nước.]*

**Vinh:** Nhưng công việc của tôi là không để sự thiếu chắc chắn ấy khiến nhiều người chết hơn.

**Mai:** Tôi hiểu trách nhiệm của ông. Nhưng hiện tại ông vẫn đang quyết định thay cả những người phải chịu rủi ro mà họ không hề được biết.

**Vinh:** Có những lúc người điều hành buộc phải đưa ra quyết định trước khi có đủ mọi ý kiến.

**Mai:** Vậy ông định cho họ biết lúc nào?

*[Vinh không đáp.]*

**Mai:** Trước khi cắt điện, hay sau khi mọi chuyện đã xảy ra?

*[Ông quay lại nhìn màn hình.]*

*[Mai chờ thêm một lúc, nhưng không nhận được câu trả lời. Cô hiểu cuộc đối thoại đã kết thúc.]*

## Cảnh 6 — Bố của Linh

*[Cùng chiều hôm ấy, Linh trở về Vành Đệm để thay bộ pin cho bố.]*

*[Ông Hải từng là giáo viên vật lý. Ông thường kể rằng học sinh sợ làm sai vì người lớn chỉ chăm chăm chấm đáp án, ít khi chịu hỏi các em đã nghĩ thế nào.]*

*[Bây giờ ông phải dùng máy oxy khi ngủ, mỗi lần chỉ đi bộ được một đoạn ngắn.]*

*[Khi Linh bước vào, ông đang nhìn chiếc máy báo pin yếu, tỏ vẻ bực bội.]*

**Ông Hải:** Bố biết rồi! Bố vẫn còn ngồi đây cơ mà, có phải đi đâu đâu.

*[Linh nhìn chiếc máy đang phát tiếng báo liên tục, bật cười.]*

**Linh:** Bố lại cãi nhau với máy à?

**Ông Hải:** Nó nhắc mãi một chuyện. Bố nghe thấy rồi mà nó vẫn không chịu im.

**Linh:** Thì pin yếu thật còn gì. Bố có mắng nó cũng chẳng tự đầy pin lên được đâu.

*[Cô tháo pin cũ, lắp bộ mới, đặt thiết bị đo áp suất cạnh giường rồi lục tủ tìm bình oxy dự phòng.]*

**Ông Hải:** Dạo này công việc của con thế nào?

**Linh:** Cũng nhiều chuyện lắm bố ạ.

**Ông Hải:** Nghe giọng là biết không chỉ có mỗi chuyện nhiều việc rồi.

*[Linh ngừng tìm trong tủ.]*

**Linh:** Thành phố sắp phải tiết kiệm điện ở mức sâu hơn. Có thể nhiều khu sẽ phải giảm cả sưởi.

**Ông Hải:** Nhà mình có nằm trong danh sách không?

**Linh:** Dạ, có bố ạ.

*[Ông Hải gật đầu, nhìn chiếc chăn trên giường.]*

**Ông Hải:** Thế con lấy giúp bố thêm mấy cái chăn ra là được.

**Linh:** Bố không thấy sợ à?

**Ông Hải:** Có chứ. Sao lại không?

*[Ông nhìn con gái.]*

**Ông Hải:** Nhưng bố sợ nhất là con cứ giấu bố vì nghĩ bố già rồi, nghe chuyện không chịu nổi.

*[Linh kéo chiếc ghế ngồi xuống bên giường.]*

*[Người đàn ông trước mặt đã gầy đi nhiều so với những bức ảnh còn treo trong nhà. Hồi nhỏ, Linh từng nghĩ bố giải được mọi bài toán, từ sửa đồng hồ đến làm lành với mẹ sau một trận cãi nhau.]*

*[Giờ đây, ông phải tính xem bộ pin có thể giữ máy oxy hoạt động được bao lâu nếu mất điện.]*

**Linh:** Bố này, con đang gặp một bài toán khó.

**Ông Hải:** Ừ, bố nghe đây.

**Linh:** Có những khu dân cư bị hệ thống đếm thiếu người. Nếu cấp đủ nguồn điện để bảo vệ họ, những khu khác sẽ phải chịu cắt nhiều hơn.

*[Ông Hải không ngắt lời.]*

**Linh:** Nếu giữ nguyên phương án, nhà mình có thể vẫn được bảo vệ tốt hơn. Nhưng ở những nơi như Rìa Mưa, có nhiều người già cũng đang dùng máy như bố mà không có tên trong danh sách dự phòng.

**Ông Hải:** Thế con đã biết phải làm sao chưa?

**Linh:** Chưa ạ.

*[Cô cúi nhìn bộ pin vừa thay.]*

**Linh:** Con sợ nếu sửa cách phân bổ không khéo, cuối cùng lại khiến nhiều người cùng gặp nguy hiểm.

**Ông Hải:** Vậy con phải tính cho thật kỹ.

**Linh:** Con vẫn đang tính mà bố.

**Ông Hải:** Bố biết. Nhưng con nhớ này, mình phải giải bài toán cho đúng. Không phải tìm một đáp án chỉ để bản thân đỡ thấy có lỗi.

*[Linh im lặng một lúc.]*

**Linh:** Nhưng nếu bài toán này không có đáp án đúng thì sao bố?

**Ông Hải:** Thì phải nói rõ mình đang dựa vào những giả định nào, chỗ nào chưa chắc, và mỗi phương án có thể khiến ai chịu rủi ro.

**Ông Hải:** Rồi để những người chịu ảnh hưởng được biết họ đang có những lựa chọn gì.

*[Ông nhìn con gái, hơi mỉm cười.]*

**Ông Hải:** Hồi con còn bé, bố có bao giờ chấm đúng một đáp án chỉ vì nó khiến bố yên lòng đâu.

*[Linh bật cười, dù mắt đã cay.]*

**Linh:** Bố lúc nào cũng lôi chuyện chấm bài ra được.

**Ông Hải:** Tại bố dạy học quen rồi.

*[Ông kéo chăn lên, ra hiệu cho Linh không cần cố cười nữa.]*

*[Trước khi cô ra về, ông đưa cho cô một chiếc chìa khóa nhỏ.]*

**Linh:** Chìa khóa gì thế bố?

**Ông Hải:** Phòng sách cũ của bố. Con cầm lấy.

**Linh:** Để làm gì ạ?

**Ông Hải:** Nếu mấy hôm tới trạm y tế phải chuyển những người dùng máy oxy đến nơi tập trung, con đừng mất công lo dọn đồ cho bố.

**Ông Hải:** Cứ để bố mang theo một cuốn sách. Nhà này chịu lạnh một chút cũng được.

*[Linh nhận chiếc chìa khóa.]*

*[Cô vẫn sợ. Nhưng lần đầu tiên, cô có thể hình dung rằng thay đổi phương án phân bổ không có nghĩa là phản bội bố.]*

*[Giữ cho ông sống không nhất thiết phải khiến một ông Lâm khác chịu thiệt mà chưa từng được hỏi ý kiến.]*

## Cảnh 7 — Người kỹ sư từng được cứu

*[Vinh không phải chưa từng đến Rìa Mưa.]*

*[Hai mươi năm trước, ông từng phụ trách dự án thay một phần đường ống tại đó. Khi còn là kỹ sư trẻ, ông ngủ ngay trên sàn xưởng cùng thợ, tự chui vào khoang kiểm tra để xác nhận các mối nối.]*

*[Có lần, một đồng hồ áp suất báo muộn khiến ông suýt chết. Một nữ công nhân chưa có bằng hành nghề đã kéo ông ra bằng dây cáp, rồi mắng ông suốt cả buổi vì không chịu nghe lời người có kinh nghiệm.]*

*[Vinh vẫn nhớ tên người phụ nữ ấy. Trong ngăn kéo văn phòng, ông còn giữ bức ảnh đội sửa chữa cũ, một góc giấy bị gấp.]*

*[Nhưng sau nhiều năm thăng chức, Vinh phải học cách đứng ở nơi không thể trực tiếp nắm từng chiếc van.]*

*[Một quyết định thiếu nguồn cho bệnh viện có thể khiến nhiều người chết hơn một đường ống vỡ. Những báo cáo về thiếu hụt đến với ông mỗi ngày, dày đến mức có lúc ông không còn phân biệt được cảnh báo nào có thể chờ, cảnh báo nào sẽ trở thành thảm họa ngay trước sáng hôm sau.]*

*[Đêm Mai tới chất vấn, sau khi cô rời đi, Vinh gọi cho trưởng bộ phận kỹ thuật.]*

**Vinh:** Tôi muốn anh kiểm tra lại độ tin cậy của các ước lượng dân cư ở vùng rìa. Đặc biệt là những khu đang chờ xác minh.

**Trưởng bộ phận kỹ thuật:** Vâng, thưa giám đốc. Chúng tôi sẽ rà lại dữ liệu. Nhưng kết quả kiểm tra này có được phép công bố không ạ?

**Vinh:** Chưa. Trước mắt cứ xác minh độ tin cậy của số liệu đã.

**Trưởng bộ phận kỹ thuật:** Tôi hiểu.

*[Người kỹ sư ngập ngừng một chút rồi hỏi.]*

**Trưởng bộ phận kỹ thuật:** Thưa giám đốc, vậy mục tiêu cuối cùng của chúng ta là bảo vệ tất cả các khu, bất kể lượng điện dự trữ còn bao nhiêu sao?

**Vinh:** Không.

*[Vinh nhìn bảng dự trữ đang giảm.]*

**Vinh:** Mục tiêu là không để cả thành phố chết vì một quyết định mà chúng ta chưa đủ khả năng thực hiện.

*[Ông ngừng một lúc.]*

**Vinh:** Nhưng anh cũng phải tìm xem có cách nào để không bắt một khu chịu hết phần thiếu hụt hay không.

**Trưởng bộ phận kỹ thuật:** Vâng. Tôi sẽ kiểm tra thêm các phương án.

*[Cuộc gọi kết thúc.]*

*[Vinh mở ngăn kéo, nhìn bức ảnh đội sửa chữa cũ.]*

*[Người nữ công nhân từng cứu ông có lẽ đã nghỉ việc. Có thể bà đã chuyển khu, cũng có thể bà không còn sống.]*

*[Ông nghĩ đến việc yêu cầu tra hồ sơ của bà, rồi lại thôi.]*

*[Không phải vì ông đã quên. Điều ông sợ là một người mình còn nhớ rất rõ lại nằm trong danh sách mà bộ máy dưới quyền ông không thể nhìn thấy.]*

*[Vinh quay lại bàn. Ông ký phê duyệt một phương án dự phòng tăng số lượng trạm trú chung, nhưng vẫn giữ nguyên lịch cắt điện chính thức.]*

*[Ngay cả ông cũng không dám gọi đó là một phương án đủ tốt.]*

*[Một phần trong ông hy vọng sẽ có người tìm ra phép tính khác trước khi bão đến. Phần còn lại sợ chính hy vọng ấy khiến mình chậm quyết định vào lúc chỉ còn vài phút.]*

## Cảnh 8 — Một cách để tất cả được lên tiếng

*[Tối hôm đó, Linh tìm thấy Mai trong văn phòng.]*

*[Quanh cô là những bản báo cáo in kín chữ đỏ.]*

**Linh:** Mai, cậu phải quyết định bước tiếp theo thôi.

*[Mai ngẩng lên.]*

**Mai:** Cậu đã kiểm tra xong các phương án rồi à?

**Linh:** Chưa hết. Nhưng tớ biết cậu đang đứng trước hai cách xử lý.

*[Linh kéo ghế ngồi xuống.]*

**Linh:** Cậu có thể báo cáo đây là sai lệch hành chính, giữ quyền truy cập rồi thúc đẩy sửa từng khu.

**Linh:** Hoặc cậu yêu cầu dừng toàn bộ phương án phân bổ. Nếu làm thế, rất có thể ngày mai họ sẽ thu thẻ kiểm toán của cậu.

**Mai:** Nhưng nếu cứ sửa từng khu một thì có kịp trước bão không?

**Linh:** Tớ không dám bảo là kịp.

**Mai:** Vậy chúng ta cứ để phương án hiện tại chạy tiếp à?

**Linh:** Tớ không nói thế.

*[Linh nhìn những tờ báo cáo trên bàn.]*

**Linh:** Tớ chỉ muốn cậu nghĩ kỹ. Can đảm đứng ra phản đối là một chuyện, còn làm sao để hệ thống vẫn vận hành được sau khi mình phản đối lại là chuyện khác.

**Mai:** Cậu nghĩ tớ đang quá liều lĩnh?

**Linh:** Tớ nghĩ cậu đang rất tức giận.

*[Mai quay đi.]*

**Mai:** Cậu muốn tớ im lặng sao?

**Linh:** Không. Tớ muốn cậu tìm được một cách thực sự làm được.

*[Giọng Linh chùng xuống.]*

**Linh:** Bố tớ vẫn phải dùng máy oxy ở Vành Đệm. Nếu mình đưa ra một lệnh thay đổi mà không kiểm tra kỹ, rồi máy của bố tớ ngừng hoạt động, cậu nghĩ tớ sẽ phải làm gì?

*[Mai định nói rằng ở Rìa Mưa cũng có rất nhiều người đang đối mặt với nguy cơ ấy.]*

*[Nhưng cô dừng lại. Linh đã biết điều đó. Cô ấy đang chính là người cố tính toán để không đẩy nguy hiểm từ khu này sang khu khác.]*

**Mai:** Tớ biết cậu cũng đang lo.

**Linh:** Ừ. Tớ lo lắm.

*[Linh thở ra.]*

**Linh:** Nhưng tớ vẫn đang ở đây với cậu. Chỉ là tớ không thể ký vào một phương án mà mình chưa biết nó sẽ khiến những gì ngừng hoạt động.

*[Mai nhìn người bạn ngồi đối diện.]*

*[Cô hiểu rằng hỏi Linh có quan tâm đến người dân Rìa Mưa hay không sẽ là một câu hỏi vừa tàn nhẫn vừa quá dễ dãi.]*

*[Hai người tiếp tục xem tài liệu.]*

*[Đến khuya, Mai tìm lại bộ quy định cũ mà Khang đã đưa. Cô đọc hết các phụ lục điều phối khẩn cấp.]*

*[Trong đó có một thủ tục ít khi được sử dụng: kiểm toán viên và kỹ sư vận hành phải xác minh độc lập; sau đó phải công khai số liệu tổng hợp về các nhóm chịu rủi ro, không công bố danh tính hay hồ sơ cá nhân, để Hội đồng và đại diện các phân khu có thể đưa ý kiến ngay.]*

*[Mai đọc đi đọc lại điều khoản.]*

*[Đây không phải một cách tạo thêm điện. Cũng không bảo đảm mọi người sẽ được cứu.]*

*[Nhưng nó mở ra một quy trình mà những người đang phải chịu rủi ro có thể tham gia, thay vì chỉ chờ người khác quyết định.]*

*[Cô gửi ảnh chụp điều khoản cho Linh.]*

**Mai — Tin nhắn:** Linh, tớ tìm được một thủ tục điều phối khẩn cấp trong phụ lục cũ. Cậu xem giúp tớ, liệu nó còn áp dụng được không?

*[Mười phút sau, màn hình sáng lên.]*

**Linh — Tin nhắn:** Để tớ kiểm tra lại hiệu lực và điều kiện thực hiện đã nhé.

*[Mai nhìn dòng tin nhắn.]*

*[Đó là lần đầu tiên trong nhiều ngày, một câu trả lời không hứa hẹn kết quả lại khiến cô có lý do để hy vọng.]*

**— Hết Chương 6 —**
