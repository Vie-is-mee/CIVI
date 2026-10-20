using System;
using System.Collections.Generic;

namespace Civi41
{
    [Serializable] public class Run
    {
        public int version=2, node, line, gesture, draft=0, photoCount=-1, hookClues;
        public bool openingPhoto, askedNhi, sawVideo, saw0443, ownsChoice, haCollaborates;
        public bool laterConfirmed, publicationConsent, finished;
        public bool kept0443, negotiationDone;
        public string negotiation="";
        public string invite="unasked", ending="", album="Một lần đủ bốn";
        public List<string> history=new List<string>();
        public bool Valid()
        {
            return version==2 && node>=0 && node<=22 && line>=0 && line<20 && hookClues>=0 && hookClues<=7
                && gesture>=0 && gesture<=2 && draft>=0 && draft<=2
                && photoCount>=-1 && photoCount<=4
                && (photoCount!=2 || laterConfirmed)
                && (!finished || publicationConsent && photoCount>=0 && ending.Length>0)
                && (invite=="unasked" || invite=="accepted" || invite=="declined" || invite=="not_invited");
        }
        public bool CanFinish {get{return openingPhoto && publicationConsent && photoCount>=0;}}
    }
    public struct Beat
    {
        public string who,text,mood;
        public Beat(string w,string t,string m="neutral"){who=w;text=t;mood=m;}
    }
    public static class Story
    {
        public static string Chapter(Run s)
        {
            if(s.node==0)return "TRANG 17 / BẢN CHƯA GỬI";
            if(s.node<=2)return "01 / LỜI HẸN";
            if(s.node<=7)return "02 / CHÍN GIÂY NGOÀI ẢNH";
            if(s.node<=11)return "03 / TRANG ĐẸP HƠN";
            if(s.node<=14)return "04 / MỘT LỜI MỜI";
            if(s.node<=18)return "05 / NGÀY MAI ĐÃ TỚI";
            return "06 / TRANG 17";
        }
        public static string Place(Run s)
        {
            if(s.node==0)return "THỨ NĂM, 07.05 · PHÒNG KỶ YẾU";
            if(s.node==5)return "THỨ NĂM, 07.05 · CHỖ TRẢ ĐỒ HỘI CHỢ";
            if(s.node<=2)return "THÁNG MƯỜI · BUỔI TẬP HỘI CHỢ";
            if(s.node<=11)return "THỨ NĂM, 07.05 · PHÒNG KỶ YẾU";
            if(s.node<=14)return "THỨ NĂM · TRƯỚC BUỔI CHỤP";
            if(s.node==18 && s.line<2)return "THỨ SÁU, 08.05 · HẸN MỘT NGÀY KHÁC";
            if(s.node==18 || s.photoCount==2)return "THỨ HAI, 11.05 · SAU GIỜ HỌC";
            if(s.node<=19)return "THỨ SÁU, 08.05 · SÂN TRƯỜNG, 16:45";
            return "THÁNG NĂM · MỘT TRANG ĐƯỢC GIỮ LẠI";
        }
        static Beat B(string w,string t,string m="neutral"){return new Beat(w,t,m);}
        public static Beat[] Lines(Run s)
        {
            switch(s.node)
            {
                case 0:return new[]{B("HẠ","Khoan gửi. Trang 17 đâu?","concerned")};
                case 1:return new[]{B("","Bảy tháng trước. Chữ HẸN trên quầy vẫn chưa chịu sáng."),B("NHI","Ê, vào hết đi. Đủ bốn mới bấm.","warm"),B("MINH","Máy tự bấm mà.","warm"),B("NHI","Thì mày thương lượng với máy đi.","warm")};
                case 2:return new[]{B("HẠ",s.gesture==0?"Chắc chưa? Điện thoại rơi là tao không nhặt đâu đấy.":s.gesture==1?"Người dựng bảng cũng vào ảnh đi. Lệch tí kệ nó.":"Đừng chọc nữa. Minh cười rồi kìa, nhanh!","warm"),B("NHI","Giữ tấm này nhé. Trước khi ra trường chụp lại, đúng chỗ này.","warm"),B("MINH","Lúc đấy nhớ nhắc. Tao quên là tại mày đấy.","warm"),B("NHI","Ừ, để tao nhắc cả năm.","warm")};
                case 3:return new[]{B("","Bản in thử vẫn mở trên bàn. Hạ kéo ghế ngồi xuống, giữ ngón tay ở trang trắng."),B("MINH","Tao rút trang đó. Mày nhắn “bỏ đi, mai tính” mà."),B("AN","Tao nói câu chú thích.","concerned"),B("MINH","Trong tin nhắn có chữ “chú thích” đâu.","concerned"),B("","Tôi mở máy. Tin nhắn của mình vẫn nằm đó, ngắn hơn câu vừa nói.")};
                case 4:return new[]{B("HẠ","Ảnh gốc tao chưa xóa. Nhi đang ở dưới trả đồ cho Phương, mày muốn hỏi thì xuống đi.")};
                case 5:return new[]{B("AN","Hôm hội chợ… tấm ba đứa ấy, ai chụp nhỉ?"),B("NHI","Tao. Mày đưa máy cho tao mà."),B("AN","Ừ nhỉ.","concerned"),B("NHI","Tao có chỉ cái kẹp. Mày bảo chụp một tấm thôi, lát chụp đủ."),B("AN","Rồi tao quên.","concerned"),B("NHI","Rồi đông khách. Tao cũng quên nhắc."),B("","Nhi cuốn nốt sợi dây, không nhìn lên. Tôi vẫn cầm điện thoại, chưa biết cất vào đâu.")};
                case 6:return new[]{B("","Ba tấm ảnh, cùng một buổi chiều. Chạm từng tấm để nhìn kỹ hơn.")};
                case 7:return new[]{B("HẠ","Mày thấy đoạn đó rồi à?"),B("AN","Ừ."),B("HẠ","Thế chắc lại bỏ tấm này.","concerned"),B("AN","Tao chưa bảo bỏ."),B("HẠ","Hôm đó Minh vừa cười được hai giây. Tao còn chưa kịp cất máy. Giờ nhìn đâu cũng thấy mình làm sai.","concerned"),B("MINH","Tao không cười ít đến thế đâu."),B("HẠ","Ừ, thì hiếm khi cười mà không liếc lịch ôn thi.","warm"),B("","Minh định nói gì nữa, rồi thôi. Hạ vẫn giữ tấm ảnh, chưa đưa tôi.")};
                case 8:return new[]{B("HẠ","Mày thử đặt lên trang xem. Đừng chỉ bảo “sửa đi” nữa.")};
                case 9:return new[]{B("HẠ",s.draft==2?"“Cảm ơn Nhi”… Thế mai nó đến chụp, hay đến nhận cái này?":s.draft==1?"Vẫn giữ 0443 à? Tao tưởng mày muốn bỏ nó.":"Hai ô. Thế tấm hội chợ để đâu?",s.draft==2?"concerned":"neutral"),B("AN",s.draft==2?"Tao tưởng nó sẽ đỡ khó chịu.":"Tao chưa biết. Đang thử thôi.","concerned"),B("HẠ","Tao thức hai tối chọn ảnh. Mày không cần thích hết, nhưng nói rõ giúp tao mày muốn giữ cái gì.","concerned")};
                case 10:return new[]{B("","Con trỏ nằm trên nút xóa ảnh. Hạ đặt tay lên cạnh máy, như sợ tôi bấm nhầm.")};
                case 11:return s.ownsChoice?new[]{B("AN","Hôm đó tao đưa máy cho Nhi. Tao còn hẹn nó chụp lại. Phần này để tao nói.","concerned"),B("HẠ","Tao nghe rồi. Nhưng tấm ảnh này thì sao?"),B("AN","Mày vẫn muốn giữ?"),B("HẠ","Ừ. Tao không muốn cứ nhìn nó là phải nhớ mình đã có lỗi với ai.","concerned")}:new[]{B("HẠ","File ở đây. Nhưng mày muốn tao sửa phần chữ, hay muốn tao giả vờ tấm ảnh này chưa từng có?","concerned"),B("AN","Tao đang muốn xong trang này."),B("HẠ","Tao cũng thế. Từ hai tối trước rồi.","concerned")};
                case 12:return new[]{B("HẠ",s.negotiation=="keep"?"Để ảnh lại nhé. Phần chữ tao chừa trống, mày viết. Đừng lại gửi mỗi chữ “bỏ”.":s.negotiation=="replace"?"Ừ. Tao gửi đủ file. Trang mới mày tự chốt nhé.":"Được. Cứ hỏi Nhi trước. Tao chưa sửa tiếp đâu."),B("","Tôi gõ tên Nhi vào ô tìm kiếm. Lần này phải nhắn dài hơn hai chữ.")};
                case 13:
                    var response=s.invite=="accepted"?new[]{B("NHI","16:45 được. 17:10 tao đi học. Nếu chưa chụp xong thì tao vẫn phải đi đấy."),B("AN","Ừ. Đến là vào chụp, tao lo máy."),B("NHI","Đừng bắt cười từ lúc còn đang đếm ngược là được.","warm"),B("AN","Cả tấm buổi tập nữa, cho tao để vào kỷ yếu nhé?"),B("NHI","Tấm Minh nhắm mắt á? Ừ, tấm đó được.","warm")}:s.invite=="declined"?new[]{B("NHI","Tao không cầm máy giúp được. Mai còn phải trả đồ cho Phương."),B("AN","Vậy bọn tao cứ chụp nhé?"),B("NHI","Ừ. Đừng chờ."),B("AN","Tao giữ tấm buổi tập trong kỷ yếu được không?"),B("NHI","Được. Câu chú thích kia thì bỏ nhé.","concerned")}:new[]{B("AN","Tao để tấm buổi tập vào kỷ yếu nhé? Không làm lại cái khung “cảm ơn” nữa."),B("NHI","Ừ. Tấm đó được. Mày vẫn còn giữ à?"),B("AN","Còn. Cả hai tấm hỏng."),B("NHI","Đừng xóa vội.")};
                    if(s.draft!=2)return response;
                    var list=new List<Beat>{B("AN","Tao làm thử trang này. Mày xem nhé?"),B("NHI","Đừng làm cái bảng đó.","concerned"),B("AN","Tao chỉ định cảm ơn—"),B("NHI","Tao biết. Nhưng tao không nhận giải."),B("","Tôi xóa dòng tiêu đề. Phần giấy trắng bỗng to hơn lúc nãy.")};list.AddRange(response);return list.ToArray();
                case 14:return new[]{B("MINH","Pin tao sạc rồi. Chụp xong ai cầm về?"),B("AN","Tao."),B("MINH","Ghi tên mày vào túi đi. Mai đừng gọi tao tìm, tao thi thử."),B("HẠ",s.haCollaborates?"Thẻ nhớ đưa tao. Còn chữ trên trang, An vẫn viết nhé.":"Tao gửi file gốc rồi. Đừng tải nhầm bản cũ."),B("","Tôi viết tên mình lên băng dính. Chữ lem vì đầu bút đã khô.")};
                case 15:return s.invite=="accepted"?new[]{B("","16:45. Cái bảng HẸN đã sáng đủ. Nhi đặt cặp xuống cạnh ghế."),B("AN","Chụp cùng bọn tao nhé? Máy hẹn giờ, ai cũng vào được."),B("NHI","Ừ. Nhưng đừng bắt đứng y hệt hồi trước. Tao đứng chỗ này thôi.")}:new[]{B("","16:45. Tin nhắn của Nhi vẫn nằm trên màn hình: “Tụi mày cứ chụp.”"),B("HẠ","Vậy… mình chụp không?","concerned"),B("MINH","Tao giữ máy thêm một lát được. Rồi phải bàn giao nhé.")};
                case 16:return new[]{B("",s.invite=="accepted"?"Nhi nhìn đồng hồ. Minh đã đeo một quai cặp. Máy vẫn chưa bấm.":"Hạ kéo chiếc ghế thứ tư vào trong. “Để đây hay cất đi?”")};
                case 17:return new[]{B("MINH",s.photoCount==4?"Ơ, tao đang nhặt cái bìa mà!":"Lại nhìn cái pin rồi. Chụp lại không?","warm"),B("HẠ",s.photoCount==4?"Chính thế mới giữ.":"Thôi. Tao thích tấm này.","warm"),B("",s.photoCount==4?"Minh xin xem ảnh thêm một lần, rồi mới chịu nhận lại cặp.":"Tôi định nhắn Nhi “xong rồi”. Gõ xong lại thấy chẳng biết mình đang báo xong chuyện gì.")};
                case 18:return new[]{B("AN","Thứ Hai sau giờ học, mình chụp một tấm ở sân nhé? Không kịp bản in thì làm tờ bổ sung."),B("NHI","16:30 thì được. Tao gặp mày ở ghế đá. Chụp hai đứa thôi cũng được."),B("","Thứ Hai, 11 tháng Năm. Hạ và Minh có việc riêng. An đến ghế đá đúng giờ.")};
                case 19:return new[]{B("HẠ",s.kept0443?"0443 để nguyên nhé. Phần ảnh có tao, tao đồng ý cho in.":"Phần ảnh có tao dùng được. Gửi bản chốt trước khi in nhé."),B("MINH","Tao xem rồi, in được. Gửi PDF cho tao với, đừng gửi cái link hết hạn nữa."),B("NHI",s.photoCount==2 || s.photoCount==4?"Ừ, tao đồng ý tấm này. Mà gửi tao cả tấm buổi tập nhé.":"Tấm buổi tập dùng được. Xong rồi gửi tao một bản nhé.")};
                case 20:return new[]{B("","Tôi gõ tên file. Ngón tay đã đặt lên chữ “final”, rồi xóa đi.")};
                case 21:return s.photoCount==4?new[]{B("NHI","Gửi tao cả tấm hỏng nhé."),B("AN","Mày cũng nhắm mắt mà."),B("NHI","Biết. Thì tao mới xin.","warm")}:s.photoCount==3?new[]{B("HẠ","Tấm thứ Sáu tao lưu riêng rồi."),B("","Điện thoại rung. Nhi hỏi xin ảnh buổi tập. Tôi mở thư mục, lần này chọn cả hai tấm hỏng.")}:s.photoCount==0?new[]{B("HẠ",s.kept0443?"Cứ để tấm đó ở đấy. Tao chưa muốn bỏ nó.":"Tấm hội chợ tao vẫn giữ trong máy nhé."),B("AN","Ừ."),B("","Tôi cất máy. Không ai bảo chụp thêm một tấm cho xong chuyện.")}:new[]{B("NHI","Hạ với Minh biết có tờ này chưa?"),B("AN","Tao gửi rồi. Chưa thấy ai trả lời."),B("","Nhi chỉnh lại mép giấy gấp lệch. Hai đứa ngồi chờ thêm một lúc, không chụp thêm.")};
                default:return new[]{B("","Trang này được giữ lại.")};
            }
        }
        public static string Mode(Run s)
        {
            switch(s.node){case 0:return "hook";case 1:return "gesture";case 2:return "opening";case 4:return "investigate";case 6:return "evidence";case 8:return "draft";case 10:return "ownership";case 11:return "negotiate";case 12:return "invite";case 16:return "shoot";case 18:return "later";case 20:return "publish";case 22:return "end";default:return "next";}
        }
        public static bool Advance(Run s)
        {
            if(s.line<Lines(s).Length-1){s.line++;return true;}
            if(Mode(s)!="next")return false;
            if(s.node==19)s.publicationConsent=true;
            if(s.node==13 && s.draft==2)s.draft=0;
            s.node=s.node==17?19:s.node+1;s.line=0;return true;
        }
        public static bool Act(Run s,string action)
        {
            if(s.line!=Lines(s).Length-1)return false;
            int n=s.node;
            if(n==0 && action=="hook_done" && s.hookClues!=0)s.node=1;
            else if(n==1 && action.StartsWith("gesture:")){int v;if(!int.TryParse(action.Substring(8),out v)||v<0||v>2)return false;s.gesture=v;s.node=2;}
            else if(n==2 && action=="capture_opening"){s.openingPhoto=true;s.node=3;}
            else if(n==4 && action=="ask_nhi"){s.askedNhi=true;s.node=5;}
            else if(n==4 && action=="photos")s.node=6;
            else if(n==6 && action=="evidence_done" && s.saw0443 && (s.askedNhi || s.sawVideo))s.node=7;
            else if(n==8 && action.StartsWith("draft:")){int v;if(!int.TryParse(action.Substring(6),out v)||v<0||v>2)return false;s.draft=v;s.node=9;}
            else if(n==10 && (action=="own" || action=="deflect")){s.ownsChoice=action=="own";s.haCollaborates=false;s.node=11;}
            else if(n==11 && (action=="negotiate:keep" || action=="negotiate:replace" || action=="negotiate:ask")){s.negotiation=action.Substring(10);s.negotiationDone=true;s.kept0443=s.negotiation=="keep";s.haCollaborates=s.kept0443;if(s.kept0443)s.draft=1;else if(s.negotiation=="replace")s.draft=0;s.node=12;}
            else if(n==12 && action.StartsWith("invite:")){string v=action.Substring(7);if(v!="accepted" && v!="declined" && v!="not_invited")return false;s.invite=v;s.node=13;}
            else if(n==16 && action=="capture"){s.photoCount=s.invite=="accepted"?4:3;s.node=17;}
            else if(n==16 && action=="keep_old"){s.photoCount=0;s.node=19;}
            else if(n==16 && action=="propose_later")s.node=18;
            else if(n==18 && action=="capture_later"){s.laterConfirmed=true;s.photoCount=2;s.node=19;}
            else if(n==20 && action=="publish" && s.CanFinish){s.finished=true;s.ending=s.photoCount==0?"B":s.photoCount==2?"C":"A";s.node=21;}
            else return false;
            s.line=0;return true;
        }
    }
}
