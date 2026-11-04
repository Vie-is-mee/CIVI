"""Author only C1 adaptation data; never modify canonical novel/campaign files."""
import json
from pathlib import Path
root=Path('artifacts/chapter1-liuquang-2026-11-02/work/Game/Assets/_Civi/Resources/LiuQuangC1')
root.mkdir(parents=True,exist_ok=True)
tasks=[]
def task(id,room,x,title,lead,actions,responses,lines,image=0,mask=None):
    tasks.append(dict(id=id,room=room,x=x,y=(-4.4 if id=='corridor' else -2.0),title=title,lead=lead,actions=actions,responses=responses,lines=lines,image=image,requiredMask=(1<<len(actions))-1 if mask is None else mask))
task('memory',0,-4,'Một vạch xanh',
'Mai, tám tuổi. Mùa lạnh thứ mười bảy sau khi mái vòm phía Tây được dựng xong. Mẹ nắm chặt chiếc thẻ lao động hết hạn. Nhân viên nói gia đình chưa đủ điểm đóng góp.',
['Xem thẻ lao động','Đọc quy định vừa thay đổi','Xét hoàn cảnh y tế'],
['Thẻ đã hết hạn. Nhưng đây không phải toàn bộ hồ sơ.','Y tá áo xám: Trẻ vị thành niên thuộc nhóm phụ thuộc cần bảo vệ.','Y tá: Phải xét hoàn cảnh y tế trước mức đóng góp. Bà nhập bốn dòng dữ liệu.'],
['Y TÁ|Đừng sợ. Người lớn sẽ tìm cách.','KÝ ỨC|Một vạch xanh hiện lên. Mẹ được giữ lại. Đêm ấy, Mai ngủ dưới chân giường bệnh. Sáng ra mẹ gắng vuốt tóc con.','MAI|Mười năm sau, tôi muốn trở thành người làm ra những vạch xanh. Mười năm nữa, tôi ngồi ở tầng bốn mươi hai.'])
task('hospital',1,-5,'Ưu tiên sinh tồn',
'Bệnh viện Số Ba cần hỗ trợ hô hấp. Quảng trường thương mại có phần năng lượng có thể điều chuyển. Kiểm tra chứng cứ trước khi gửi lệnh.',
['Đọc chứng cứ hô hấp','Chuyển: quảng trường → bệnh viện'],
['Số bệnh nhân cần hỗ trợ hô hấp làm căn cứ cho lệnh ưu tiên sinh tồn.','Nguồn: quảng trường thương mại. Nơi nhận: Bệnh viện Số Ba. Không rút điện khỏi trạm khí hoặc đường nước.'],
['HỆ THỐNG|Phê chuẩn trong mười bốn giây. Ưu tiên sinh tồn: hoàn tất.','MAI|Trước kia cần chữ ký của sáu người. Bây giờ chứng cứ giúp lệnh đi thẳng tới nơi cần điện. Tôi thích một buổi sáng bắt đầu như thế.','LƯU QUANG|Sau cửa kính: nhà trắng xếp tầng, tàu trên ray treo, vòng đèn xanh của trạm khí. Phía Đông, Rìa Mưa chìm trong hơi nước.'])
task('evidence',1,6,'Có gì nằm ngoài số không?',
'Linh mang đề nghị sửa trạm lọc 19, bị trả lần thứ sáu vì dân số dưới ngưỡng. Phân khu 8-12: 0 người cần bảo vệ. Chạm các chi tiết trên ảnh rồi kiểm tra truy vấn.',
['Chậu cây','Hai chiếc áo','Đôi giày trẻ em','Kiểm tra truy vấn dân cư'],
['Một chậu cây nhỏ trên bậu cửa. Có dấu hiệu chăm sóc.','Hai chiếc áo đang phơi. Linh xác nhận ảnh chụp hôm qua.','Giày trẻ em được đặt ngay ngắn dưới hiên để hong khô.','Mô-đun và truy vấn không báo lỗi. Phép cộng chỉ gồm hồ sơ đã xác minh; người chờ xác minh bị loại.'],
['MAI|Ảnh chụp hôm qua. Vậy phải có ít nhất một người.','LINH|Đó là lý do tớ đưa cậu.','MAI|Ghi nhận: sai lệch dữ liệu đầu vào. Cần khảo sát thực địa. Một bức ảnh chưa cho phép tôi điền một dân số chính xác.'],1)
task('coordination',1,-5,'Một phương án cho cả hai nơi',
'Hai phòng giữ trẻ cần sưởi; nhà kho cần giữ lạnh thuốc. Tổng đề nghị vượt điện dư của trạm phụ. Hãy nghe hai bên và ghép nguồn lực để thuốc an toàn, trẻ có sưởi.',
['Yêu cầu biên bản nhiệt độ','Hỏi khả năng ghép phòng','Nơi giữ thuốc: kho hiện tại','Vận tải: chờ lịch thường','Trong lúc chờ: điện trạm phụ','Thông báo tài xế: chưa đặt'],
['Nhà kho: nhiệt độ vượt giới hạn thì cả lô thuốc phải hủy. Cần bảo quản liên tục trong khi chờ xe.','Trung tâm: 23 trẻ có nguy cơ hô hấp trong hai phòng tường ẩm; một em nhập viện tuần trước. Mai đã hỏi khả năng ghép tạm, không mặc định việc đó đủ an toàn.','Kho dự phòng gần bệnh viện nhận thuốc, giải phóng điện ở kho cũ.','Vận tải công cộng ưu tiên chuyến hàng thay vì chờ lịch thường.','Kho hiện tại dùng pin dự trữ CỦA CHÍNH HỌ cho đến khi xe tới.','Nếu đổi lịch, báo người điều khiển trước ít nhất 15 phút.'],
['MAI|Chuyển thuốc sang kho dự phòng gần bệnh viện, ưu tiên một chuyến hàng công cộng. Kho dùng pin riêng trong lúc chờ. Phần điện được giải phóng dành cho trung tâm giữ trẻ.','HAI ĐẦU DÂY|Cả hai bên đồng ý. Phương án có chi phí, nhưng không vượt quy chuẩn.','GHI CHÚ|Nếu đổi lịch vận tải, thông báo người điều khiển trước mười lăm phút.'])
task('driver',1,-5,'Một lời hứa nhỏ',
'Tài xế gọi lại. Anh đã đổi giờ nghỉ để đưa chuyến hàng tới bệnh viện. Trong bảng điều phối chỉ có dòng “giao hàng thành công”.',
['Hỏi về giờ làm và hứa chuyển yêu cầu'],
['Tài xế: Việc ấy thuộc bên thầu, chắc không nằm trong chỗ chị xem. Mai định chuyển máy sang bộ phận hợp đồng.'],
['CẢNH BÁO MỚI|Một hồ sơ khẩn chen vào. Mai bảo anh gửi yêu cầu sau, hứa sẽ chuyển giúp.','BUỔI TRƯA|Mai quên mất. Công việc cứ mười phút lại xuất hiện một nguy cơ lớn hơn.','KÝ ỨC CÔNG VIỆC|Người hướng dẫn từng dặn: muốn làm lâu, phải học cách kết thúc một hồ sơ. Đừng đem cả thành phố lên giường ngủ.'])
task('gallery',2,-5,'Ai có thể đọc cách chia?',
'Trên đường tới phòng họp, một đoàn học sinh dừng trước thẻ khí cũ. Mỗi tấm thẻ kim loại từng cho phép hít thở 24 giờ tại một trạm được cấp phép.',
['Xem thẻ sứt cạnh','Nghe câu hỏi về công suất','Nghe câu hỏi về quy tắc'],
['Mất thẻ phải chờ cấp lại rất lâu. Bị trộm thẻ thì rất nguy hiểm. Ảnh lịch sử ghi cảnh người xếp hàng quanh đường ống.','Học sinh: Sao không để ai cũng vào? Mai: Nếu tất cả vào một lúc, trạm quá tải, không khí hỏng và không ai được cứu. Cần chia để nhiều người sống nhất.','Học sinh: Ai đặt ra cách chia? Mai: Người được giao trách nhiệm, có kiểm tra và phải công khai. Em hỏi tiếp: Người không biết chữ có đọc được không?'],
['MAI|Hiện tại có giao diện giọng nói, dịch vụ hỗ trợ và đường dây khiếu nại. Tôi kiểm tra chúng trong báo cáo hằng năm.','MAI|Nhưng lần cuối tôi thử gọi như một người không có số công dân là khi nào?','HÀNH LANG|Đoàn học sinh đi tiếp. Mai đứng lại bên những tấm thẻ.'])
task('lunch',3,0,'Tên ai trên biên bản?',
'Linh gạt đậu sang một bên. Hai người từng chia phòng trọ chật, sửa quạt bằng dây từ máy sấy hỏng; Mai học đêm để lấy học bổng. Họ đã hứa mua một chiếc bàn tử tế.',
['Mở thanh toán tổng nhà thầu','Đối chiếu biên bản ký tay','Xem đề xuất sửa giao thức'],
['Chuyến hàng có người chịu trách nhiệm trong kênh thanh toán tổng. Không có tên từng người lái.','Biên bản thường do quản đốc ký. Dữ liệu không cho biết người ấy tự lái hay có mười người dưới quyền.','Cần đổi giao thức sáu công ty thầu, cấp thiết bị xác minh từng lao động và trả chi phí duy trì. Đề xuất đang xếp sau thay van trạm trung tâm.'],
['LINH|Bố tớ vừa đổi máy oxy. Nhẹ hơn, pin đắt khủng khiếp. May có trợ cấp Vành Đệm. Tớ vẫn giữ máy cũ, biết đâu cần.','MAI|Như vậy dễ bỏ sót đóng góp của tài xế.','LINH|Có cách sửa. Nhưng quý nào cũng có chuyện khẩn hơn.','GIỜ NGHỈ|Hàng vẫn tới đúng giờ ở Lõi Sáng. Người đã đổi giờ nghỉ chỉ là một phần của “giao hàng thành công”. Chuông họp vang lên.'])
task('corridor',2,7,'Một chỗ đứng',
'Đồng nghiệp vui vì được chuyển từ Vành Đệm vào Lõi Sáng. Căn hộ nhỏ hơn, giá cao hơn, nhưng con gái sẽ được học ở trường có phòng thí nghiệm.',
['Chỉ khu gần tuyến tàu'],['Mai gợi ý khu có công viên và nhà trẻ tốt. Đồng nghiệp cảm ơn: Ai cố gắng rồi cũng có chỗ đứng thôi.'],
['MAI|Tôi nhớ đôi giày trong bức ảnh 8-12. Đứa trẻ ấy có biết cần vào hàng nào, giấy tờ gì, nộp cửa sổ nào?','SỔ VIỆC|Chưa thể kết luận chỉ từ cảm giác. Mai lưu câu hỏi, dưới lời nhắc kiểm tra giờ làm của tài xế. Cuối ngày còn thêm hai mươi bảy dòng.'])
task('meeting',4,0,'Báo cáo có thể thực thi',
'Vinh đứng trước bản đồ năng lượng. Bụi mang điện đang tạo một khối xoáy lớn; dự kiến mười hai ngày nữa quét qua mái vòm.',
['Xem dự báo và ưu tiên','Đối chiếu nguồn lực','Gửi hồ sơ 8-12 tới tổ xác minh'],
['Cần giữ bệnh viện, trạm cấp khí và các đường dẫn nước. Mỗi ngày phê duyệt chậm là một ngày mất chuẩn bị.','Dự trữ điện giảm; bộ lọc vùng rìa đã cũ; ngân sách dự phòng dùng quá nửa. Các con số này cũng thật.','Mai: Nếu cắt điện theo bảng dân số báo thiếu, sẽ tính sai khí cần thiết. Vinh đồng ý chuyển hồ sơ cho tổ xác minh.'],
['VINH|Kết thúc ngày thứ ba, tôi cần báo cáo có thể thực thi. Không phải một lời hứa rằng chúng ta sẽ cứu được tất cả.','MAI|Tôi hiểu. Tôi cũng không muốn hứa điều không có cách thực hiện.'])
task('mother',5,6,'Hai việc có thể cùng thật',
'Về nhà, Mai kể chuyện hồ sơ số không. Mẹ đang tỉa hương thảo bên cửa sổ.',
['Ngồi lại nghe mẹ'],['Mẹ hỏi về cô y tá. Sau hôm được cứu, bà vẫn phải làm thêm mười một năm để trả hết nợ thuốc.'],
['MẸ|Cô ấy cứu mẹ. Nhưng con đừng nhầm việc ấy với chuyện mọi thứ đã công bằng.','MAI|Tôi có dữ liệu: tỷ lệ sống tăng, can thiệp đặc quyền giảm, trẻ Vành Đệm thêm suất học. Nhưng mẹ trở lại chậu cây. Không có cuộc tranh luận nào để thắng.'])
task('album',5,3,'Phần còn lại của ký ức',
'Ảnh sau ngày xuất viện: mẹ trong áo khoác mượn, túi thuốc, Mai và người y tá tóc ngắn. Mai đã gửi thư cảm ơn khi tốt nghiệp, không biết thư có tới không.',
['Nhìn mẹ và túi thuốc','Nhớ những khoản phải trả','Nhìn người y tá'],
['Mẹ còn yếu, nhưng cử chỉ vuốt tóc đã khiến Mai tin mọi chuyện sẽ ổn. Được cứu sống vẫn có giá trị.','Nợ thuốc, những buổi dã ngoại phải bỏ vì không có giày mới, chiếc vòng cuối bà ngoại bán. Chúng thường vắng trong câu chuyện vạch xanh Mai kể.','Mắt thâm quầng, tay giữ vai cô bé. Mai muốn hỏi bà đã phải lựa chọn điều gì, đã làm gì để cứu những người chưa được tính đúng.'],
['MAI|Có phải ngày ấy họ cứu mẹ thay một người khác? Con sợ mọi điều tốt đẹp đời mình đều có ai đó trả giá.','MẸ|Có thể. Mẹ không biết. Con không phải trả ơn bằng cách hối hận vì đã sống.','MẸ|Nhưng nếu biết có người bị đối xử sai, con cũng không được lấy chuyện mình từng được cứu làm lý do nhắm mắt. Hai việc có thể cùng thật.'],2)
task('request',5,3,'Gửi một hồ sơ nhỏ',
'Mai mở lại sổ việc. Lời hứa buổi sáng vẫn ở đó. Đối chiếu đúng chuyến hàng trước khi gửi yêu cầu giờ làm.',
['Gắn chuyến hàng kho → bệnh viện','Ghi rõ giờ nghỉ đã đổi'],
['Chuyến hàng đã giúp nhà kho giải phóng điện cho trung tâm giữ trẻ.','Yêu cầu đối chiếu giờ làm của tài xế với bộ phận hợp đồng; chưa có quyết định trả thêm hoặc phê chuẩn.'],
['SỔ VIỆC|Yêu cầu đã gửi. Có thể mất nhiều tuần mới nhận phản hồi.','MAI|Tôi không muốn sự tử tế của một người trở nên vô hình chỉ vì cuối ngày có việc khác quan trọng hơn.'])
task('final',5,3,'Vẫn là số không',
'Trước khi ngủ, Mai mở lại ảnh 8-12. Trời trong ảnh tối; bóng hai chiếc áo đổ dài xuống tường. Màn hình vẫn ghi 0.',
['Đối chiếu ảnh với số không'],['Chứng cứ đã được chuyển xác minh. Dân số trên màn hình chưa đổi. Người tài xế vẫn đang chờ một hồ sơ được đối chiếu.'],
['MAI|Tôi từng nghĩ sai lầm đáng sợ nhất của một phép tính là cho ra đáp án sai.','MAI|Lần đầu tiên, tôi nghĩ đến khả năng có những người chưa bao giờ được đưa vào phép tính.'],1)
data=dict(schema='civi-liuquang-c1-1',source_url='https://drive.google.com/file/d/1Q7kzTn73t3rwkCjAXxl2AXTU7s1ZpvQf/view',source_sha256='e2a7b0f3f257779978825d00bab44cbd0333cc56f2e958dbca24fae057e2acbd',tasks=tasks)
(root/'chapter.json').write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
print(f'Authored {len(tasks)} C1 tasks; canonical source untouched.')
