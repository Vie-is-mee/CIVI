using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Civi.Preview
{
    public sealed partial class CiviLiuChapter
    {
        RectTransform ui,modal,marker;Text titleText,objectiveText,promptText,markerText,noticeText,progressText;Image fadeImage;float qaNext;
        static Color Ink=>new Color(.075f,.19f,.20f);static Color Paper=>new Color(.98f,.96f,.89f);static Color Gold=>new Color(.94f,.76f,.43f);static Color Muted=>new Color(.69f,.81f,.76f);
        RectTransform Node(string name,Transform parent,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 size)
        {var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=pos;r.sizeDelta=size;return r;}
        RectTransform Panel(string name,Transform parent,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 size,Color color,bool block=false)
        {var r=Node(name,parent,anchor,pivot,pos,size);var i=r.gameObject.AddComponent<Image>();i.color=color;i.raycastTarget=block;return r;}
        RectTransform Full(string name,Transform parent,Color color,bool block=false)
        {var r=Panel(name,parent,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,color,block);r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        Text Txt(Transform parent,string name,string value,float x,float y,float w,float h,int size,Color color,bool bold=false,TextAnchor align=TextAnchor.UpperLeft)
        {var r=Node(name,parent,new Vector2(0,1),new Vector2(0,1),new Vector2(x,-y),new Vector2(w,h));var t=r.gameObject.AddComponent<Text>();t.font=InterfaceFont;t.text=value;t.fontSize=size;t.color=color;t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;t.alignment=align;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;t.raycastTarget=false;return t;}
        Button Btn(Transform parent,string label,float x,float y,float w,float h,Action action,bool primary=false)
        {var r=Panel("Button "+label,parent,new Vector2(0,1),new Vector2(0,1),new Vector2(x,-y),new Vector2(w,h),primary?Gold:Paper,true);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<Image>();b.navigation=new Navigation{mode=Navigation.Mode.None};var c=b.colors;c.highlightedColor=new Color(.73f,.88f,.80f);c.pressedColor=new Color(.57f,.75f,.69f);b.colors=c;b.onClick.AddListener(()=>{action();EventSystem.current?.SetSelectedGameObject(null);});Txt(r,"Label",label,14,5,w-28,h-10,21,Ink,true,TextAnchor.MiddleCenter);return b;}
        void Picture(Transform parent,Sprite sprite,float x,float y,float w,float h)
        {var r=Node("Evidence picture",parent,new Vector2(0,1),new Vector2(0,1),new Vector2(x,-y),new Vector2(w,h));var im=r.gameObject.AddComponent<Image>();im.sprite=sprite;im.preserveAspect=true;im.raycastTarget=false;}
        void CreateInterface()
        {
            var go=new GameObject("C1 responsive interface",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var c=go.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=100;
            var sc=go.GetComponent<CanvasScaler>();sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;sc.referenceResolution=new Vector2(1600,900);sc.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;ui=go.GetComponent<RectTransform>();
            if(EventSystem.current==null)new GameObject("C1 input",typeof(EventSystem),typeof(InputSystemUIInputModule));
            var header=Panel("Objective",ui,new Vector2(0,1),new Vector2(0,1),new Vector2(24,-20),new Vector2(1190,108),Ink);
            titleText=Txt(header,"Chapter","CIVI   /   THÀNH PHỐ CÔNG BẰNG",24,12,1130,30,19,Gold,true);
            objectiveText=Txt(header,"Objective text","",24,48,1130,45,28,Paper,true);
            var utility=Panel("Utility",ui,new Vector2(1,1),new Vector2(1,1),new Vector2(-24,-20),new Vector2(340,108),Paper);
            Btn(utility,"H  ·  Cách chơi",0,0,340,48,()=>{if(!Once())return;help=true;Lock();RenderUI();});progressText=Txt(utility,"Progress","",12,65,316,30,20,Ink,true,TextAnchor.MiddleCenter);
            var footer=Panel("Controls",ui,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,18),new Vector2(1400,62),Ink);promptText=Txt(footer,"Prompt","",16,12,1368,38,22,Paper,true,TextAnchor.MiddleCenter);
            marker=Panel("Destination",ui,new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(340,50),Gold);markerText=Txt(marker,"Destination text","",8,4,324,42,20,Ink,true,TextAnchor.MiddleCenter);
            modal=Node("Current interaction",ui,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero);modal.anchorMax=Vector2.one;modal.offsetMin=modal.offsetMax=Vector2.zero;
            var noticePanel=Panel("Save feedback",ui,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-130),new Vector2(1120,29),Paper);noticeText=Txt(noticePanel,"Notice","",10,0,1100,29,18,Ink,true,TextAnchor.MiddleCenter);
            fadeImage=Full("Room transition",ui,Color.clear,true).GetComponent<Image>();
        }
        void RenderUI()
        {
            foreach(Transform child in modal){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            if(help){HelpUI();return;}if(Complete){CompleteUI();return;}
            if(state.mode==1)WorkUI();else if(state.mode==2)DialogueUI();
        }
        RectTransform Card(string name,float width,float height)
        {return Panel(name,modal,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,-38),new Vector2(width,height),Ink,true);}
        void HelpUI()
        {
            Full("Pause veil",modal,new Color(.03f,.10f,.11f,.78f),true);var p=Card("Welcome",1180,620);
            Txt(p,"Eyebrow","NHỮNG NGƯỜI KHÔNG CÓ CHỖ DƯỚI BẦU TRỜI",44,30,1090,35,22,Gold,true);
            Txt(p,"Title","Chương 1. Thành phố công bằng",44,85,1090,70,40,Paper,true);
            Txt(p,"Introduction","Một ngày của Mai ở Cơ quan Điều phối Mái vòm. Tìm những gì con số nhìn thấy — và những gì còn nằm ngoài phép tính.",44,173,1090,100,27,Paper);
            Txt(p,"Controls","WASD / Mũi tên: đi lại     E: tương tác khi đến gần\nChuột hoặc 1–6: xem chứng cứ / ghép phương án\nEnter: xác nhận / tiếp lời     Esc: đóng bảng / tạm dừng\nF5: lưu     F9: tiếp tục bản lưu     H: hướng dẫn     M: bật/tắt âm thanh",44,285,1090,158,24,Paper);
            Txt(p,"IME hint","Nếu bật bộ gõ tiếng Việt, hãy dùng phím mũi tên. Tiến độ tự lưu sau mỗi thao tác và phân đoạn.",44,459,1090,52,20,Muted);
            Btn(p,state.step==0&&state.mask==0?"Bắt đầu":"Trở lại",44,542,332,54,Begin,true);
            Btn(p,"Tiếp tục bản lưu",404,542,332,54,Load);
            Btn(p,state.muted?"Bật âm thanh":"Tắt âm thanh",764,542,372,54,()=>{if(!Once())return;state.muted=!state.muted;UpdateAmbient();RenderUI();});
        }
        string ActionLabel(int i)
        {
            bool done=(state.mask&(1<<i))!=0;string label=Task.actions[i];
            if(Task.id=="coordination"&&i>=2&&done){string[] good={"Kho dự phòng gần bệnh viện","Chuyến hàng công cộng ưu tiên","Pin dự trữ của chính nhà kho","Báo tài xế trước 15 phút"};label=good[i-2];}
            return (i+1)+"  ·  "+(done?"✓  ":"")+label;
        }
        void WorkUI()
        {
            var p=Card("Task workbench",1480,700);Txt(p,"Task title",Task.title,28,20,1424,44,31,Gold,true);Txt(p,"Situation",Task.lead,28,77,1424,105,22,Paper);
            if(Task.image!=0)
            {
                Picture(p,Task.image==1?Environment.House:Environment.Album,28,190,610,366);
                if(Task.id=="evidence")
                { // Hotspots are genuine UI buttons on the photograph; keyboard equivalents remain available.
                    Btn(p,"1",217,286,42,38,()=>Observe(0),true);Btn(p,"2",423,292,42,38,()=>Observe(1),true);Btn(p,"3",425,493,42,38,()=>Observe(2),true);
                }
                Txt(p,"Caption",Task.image==1?"8-12  •  ẢNH CHỤP HÔM QUA":"ALBUM GIA ĐÌNH",28,559,610,28,18,Muted);
            }
            else Diagram(p);
            for(int i=0;i<Task.actions.Length;i++){int index=i;Btn(p,ActionLabel(i),670,190+i*59,782,51,()=>Observe(index),(state.mask&(1<<i))!=0);}
            string msg=feedback.Length>0?feedback:"Chọn từng mục để quan sát. Bạn có thể kiểm tra theo thứ tự mình muốn.";
            Txt(p,"Observation",msg,28,594,1020,82,21,Paper);
            Btn(p,Task.id=="request"?"Gửi yêu cầu đối chiếu":Task.id=="coordination"?"Kiểm tra phương án":"Xác nhận  →",1090,624,362,54,Submit,true);
            if(state.step>0)Btn(p,"Esc  ·  Trở lại phòng",1090,561,362,46,()=>{if(!Once())return;state.mode=0;Lock();RenderUI();});
        }
        void Diagram(RectTransform parent)
        {
            var p=Panel("Constraint diagram",parent,new Vector2(0,1),new Vector2(0,1),new Vector2(28,-190),new Vector2(610,366),new Color(.17f,.32f,.31f));
            if(Task.id=="coordination")
            {
                Txt(p,"Diagram heading","ĐIỆN DƯ < TỔNG HAI ĐỀ NGHỊ",20,16,570,32,22,Gold,true);
                string[] labels={"TRUNG TÂM GIỮ TRẺ\n23 trẻ / cần sưởi","KHO HIỆN TẠI\n"+(((state.mask&16)!=0)?"Pin riêng giữ lạnh":"Cần giữ lạnh liên tục"),"VẬN TẢI CÔNG CỘNG\n"+(((state.mask&8)!=0)?"Ưu tiên chuyến hàng":"Chưa điều phối chuyến"),"KHO GẦN BỆNH VIỆN\n"+(((state.mask&4)!=0)?"Nhận lô thuốc":"Chưa chọn nơi nhận")};
                for(int i=0;i<4;i++){float x=20+(i%2)*290,y=68+(i/2)*128;var card=Panel("Resource "+i,p,new Vector2(0,1),new Vector2(0,1),new Vector2(x,-y),new Vector2(278,107),Paper);Txt(card,"Resource label",labels[i],12,14,254,83,20,Ink,true,TextAnchor.MiddleCenter);}
                Txt(p,"Timeline",(state.mask&32)!=0?"≥ 15 PHÚT BÁO TRƯỚC  →  XE TỚI  →  CHUYỂN THUỐC":"GIỮ LẠNH TRONG LÚC CHỜ XE LÀ BẮT BUỘC",18,327,574,31,18,Gold,true,TextAnchor.MiddleCenter);
            }
            else
            {
                string top="HỒ SƠ ĐANG MỞ",main="",bottom="";
                switch(Task.id)
                {
                    case "memory":top="QUY ĐỊNH MỚI";main="PHỤ THUỘC\nCẦN BẢO VỆ";bottom="Y tế được xét trước mức đóng góp.";break;
                    case "hospital":top="QUẢNG TRƯỜNG THƯƠNG MẠI";main="↓\nBỆNH VIỆN SỐ BA";bottom="Chứng cứ hô hấp → phê chuẩn 14 giây";break;
                    case "driver":top="GIAO HÀNG THÀNH CÔNG";main="Tên tài xế\n—";bottom="Giờ nghỉ đã đổi nằm ngoài bảng tổng.";break;
                    case "gallery":top="THẺ KHÍ KIM LOẠI";main="24 GIỜ";bottom="Có quyền vào trạm. Nhưng ai có thẻ?";break;
                    case "lunch":top="THANH TOÁN TỔNG";main="NHÀ THẦU\n≠ TỪNG LAO ĐỘNG";bottom="Biên bản ký tay → chữ ký quản đốc";break;
                    case "corridor":top="ĐIỀU CHUYỂN ĐƯỢC DUYỆT";main="VÀNH ĐỆM\n→ LÕI SÁNG";bottom="Căn hộ nhỏ hơn. Trường có phòng thí nghiệm.";break;
                    case "meeting":top="DỰ BÁO BÃO BỤI MANG ĐIỆN";main="12 NGÀY";bottom="Báo cáo thực thi: kết thúc ngày thứ ba";break;
                    case "mother":top="SAU NGÀY ĐƯỢC CỨU";main="11 NĂM\nTRẢ NỢ THUỐC";bottom="Sự sống được giữ lại. Chi phí vẫn còn.";break;
                    case "request":top="CHUYẾN HÀNG BUỔI SÁNG";main="GIỜ LÀM\nCẦN ĐỐI CHIẾU";bottom="Yêu cầu có thể mất nhiều tuần để phản hồi.";break;
                }
                Txt(p,"Diagram label",top,22,28,566,48,22,Gold,true,TextAnchor.MiddleCenter);Txt(p,"Diagram value",main,24,108,562,150,36,Paper,true,TextAnchor.MiddleCenter);Txt(p,"Diagram note",bottom,24,285,562,60,21,Muted,false,TextAnchor.MiddleCenter);
            }
        }
        void DialogueUI()
        {
            string raw=Task.lines[state.line];int split=raw.IndexOf('|');string who=raw.Substring(0,split),line=raw.Substring(split+1);
            var p=Panel("Dialogue",modal,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,22),new Vector2(1460,285),Ink,true);
            Txt(p,"Speaker",who,30,24,1050,36,24,Gold,true);Txt(p,"Dialogue text",line,30,78,1400,120,28,Paper);
            Txt(p,"Dialogue controls","ENTER / SPACE  ·  TIẾP TỤC     |     F5 / F9  ·  LƯU / TẢI",30,230,1050,30,18,Muted);
            Btn(p,"Tiếp tục  →",1120,218,310,50,Next,true);
            if(Task.id=="memory"&&state.line==1){var strip=Panel("Green retention",modal,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,20),new Vector2(830,84),new Color(.22f,.48f,.35f));Txt(strip,"Retained","ĐƯỢC GIỮ LẠI  •  QUY ĐỊNH ĐÃ ĐƯỢC ÁP DỤNG",24,24,782,36,23,Paper,true,TextAnchor.MiddleCenter);}
            if(Task.image!=0){var pic=Panel("Photo during reflection",modal,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-166),new Vector2(640,385),Paper);Picture(pic,Task.image==1?Environment.House:Environment.Album,8,8,624,369);}
        }
        void CompleteUI()
        {
            Full("Ending veil",modal,new Color(.025f,.07f,.09f,.90f),true);var p=Card("Ending",1440,680);
            Picture(p,Environment.House,30,50,690,518);
            Txt(p,"End chapter","HẾT CHƯƠNG 1",770,45,620,45,24,Gold,true);
            Txt(p,"Ending title","Thành phố công bằng",770,111,620,112,40,Paper,true);
            Txt(p,"Zero","0",770,230,620,100,86,Gold,true);
            Txt(p,"Ending body","Số người cần bảo vệ trong phân khu 8-12.\n\nHồ sơ đã chuyển xác minh. Yêu cầu giờ làm đã gửi. Chưa có câu trả lời.",770,348,620,160,25,Paper);
            Txt(p,"Ending line","Có những người chưa bao giờ được đưa vào phép tính.",30,595,1030,62,27,Paper);
            Btn(p,"Lưu và khép lại",1090,599,320,54,()=>{Save(true);Application.Quit();},true);
        }
        void TickUI()
        {
            titleText.text="CIVI   /   CHƯƠNG 01   •   "+(Complete?"ĐÊM Ở LƯU QUANG":new[]{"KÝ ỨC","TẦNG 42","PHÒNG TRƯNG BÀY","CĂNG TIN","PHÒNG HỌP","CĂN HỘ"}[Task.room]);
            objectiveText.text=Complete?"Thành phố công bằng":Task.title;progressText.text="PHÂN ĐOẠN  "+Mathf.Min(state.step+1,13)+" / 13";
            bool explore=!Locked;marker.gameObject.SetActive(explore);promptText.transform.parent.gameObject.SetActive(explore);
            promptText.text=Distance<=2.2f?"E  ·  "+(Task==null?"":Task.title):"WASD / Mũi tên  ·  Đi lại     |     E  ·  Tương tác     |     F5 / F9  ·  Lưu / Tải";
            if(explore)
            {var v=Camera.main.WorldToViewportPoint((Vector3)Target+new Vector3(0,Task.id=="mother"?6.8f:5,0));var raw=new Vector2((v.x-.5f)*ui.rect.width,(v.y-.5f)*ui.rect.height);var p=new Vector2(Mathf.Clamp(raw.x,-ui.rect.width/2+190,ui.rect.width/2-190),Mathf.Clamp(raw.y,-ui.rect.height/2+150,ui.rect.height/2-210));marker.anchoredPosition=p;string arrow=(raw-p).sqrMagnitude>1?(Mathf.Abs(raw.x-p.x)>Mathf.Abs(raw.y-p.y)?(raw.x>p.x?"→ ":"← "):(raw.y>p.y?"↑ ":"↓ ")):"◆ ";markerText.text=arrow+Task.title+" · "+Distance.ToString("0.0")+" m";float h=Mathf.Max(42,markerText.preferredHeight);markerText.rectTransform.sizeDelta=new Vector2(324,h);marker.sizeDelta=new Vector2(340,h+8);}
            noticeText.text=Time.unscaledTime<noticeUntil?notice:"";noticeText.transform.parent.gameObject.SetActive(Time.unscaledTime<noticeUntil);
            fadeImage.color=new Color(.025f,.07f,.08f,fade);fadeImage.raycastTarget=transitioning;
        }
        [Serializable] sealed class QAButton{public string label;public float x,y;}
        [Serializable] sealed class QAState
        {public string scene,task,savePath,feedback;public int step,mask,mode,line,frame,width,height;public bool help,locked,complete,transition;public float x,y,targetX,targetY,distance;public QAButton[] buttons;public string[] textOverflows;}
        void Telemetry()
        {
            if(Time.unscaledTime<qaNext)return;qaNext=Time.unscaledTime+.1f;
            var buttons=new List<QAButton>();foreach(var b in ui.GetComponentsInChildren<Button>()){if(!b.isActiveAndEnabled)continue;var r=(RectTransform)b.transform;var screen=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center));buttons.Add(new QAButton{label=b.GetComponentInChildren<Text>().text,x=screen.x/Screen.width*1600,y=(1-screen.y/Screen.height)*900});}
            var over=ui.GetComponentsInChildren<Text>().Where(t=>t.isActiveAndEnabled&&t.text.Length>0&&t.preferredHeight>t.rectTransform.rect.height+2).Select(t=>t.name+" "+t.preferredHeight+">"+t.rectTransform.rect.height).ToArray();
            var s=new QAState{scene="LiuQuang-C1",task=Task?.id??"complete",step=state.step,mask=state.mask,mode=state.mode,line=state.line,frame=Time.frameCount,width=Screen.width,height=Screen.height,help=help,locked=Locked,complete=Complete,transition=transitioning,x=Movement.Player.position.x,y=Movement.Player.position.y,targetX=Target.x,targetY=Target.y,distance=Distance,buttons=buttons.ToArray(),textOverflows=over,savePath=SavePath,feedback=feedback};
            try{File.WriteAllText(Path.Combine(qaDir,"state.json"),JsonUtility.ToJson(s,true));}catch(IOException){}
        }
    }
}
