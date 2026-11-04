using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Civi.Preview
{
    public sealed class DraftReviewView : MonoBehaviour
    {
        public event Action<DraftPreviewRequest> Requested;
        public string SelectedType {get;private set;}="paired";
        public string SelectedRecipient {get;private set;}="held";
        public bool Reviewing {get;private set;}
        public bool Submitting {get;private set;}
        public int Version {get;private set;}=1;
        public string Gesture="pose_1";
        public Button confirm,back;
        public Text summary,status;
        CiviAssetCatalog assets;RectTransform pageSlot;Button[] templateButtons,recipientButtons;
        static readonly string[] Types={"tribute","paired","memory"},Recipients={"held","nhi","group_four"};
        public static string RecipientName(string id)=>id=="held"?"Giữ trên máy của An":id=="nhi"?"Chỉ Nhi":"Nhóm bốn người: An, Nhi, Hạ, Minh";
        public void Initialize(CiviAssetCatalog catalog,Action close,int version=1)
        {
            if(version<1)throw new ArgumentOutOfRangeException(nameof(version));Version=version;
            assets=catalog;
            NotebookUI.Button("Close drafts",transform,"Cất bản nháp",1320,28,230,45,assets.uiFont,()=>{if(!Submitting)close();});
            templateButtons=new Button[3];
            for(int i=0;i<3;i++)
            {
                string type=Types[i];float x=55+i*507;
                var button=NotebookUI.Button("Choose "+type,transform,"",x,105,477,132,assets.uiFont,()=>SelectType(type),true);templateButtons[i]=button;
                button.GetComponentInChildren<Text>().gameObject.SetActive(false);
                var miniature=Instantiate(assets.draftTemplates[i],button.transform).GetComponent<RectTransform>();
                NotebookUI.Place(miniature,10,10,850,490);miniature.localScale=Vector3.one*.21f;
                NotebookUI.Label("Template name",button.transform,assets.Draft(type).title,203,22,256,40,27,assets.handFont,NotebookUI.Teal);
                string detail=type=="tribute"?"Ảnh hậu trường, lời cảm ơn":type=="paired"?"Giữ một chỗ cho lần chụp tới":"Ảnh hỏng và điều muốn nhớ";
                NotebookUI.Label("Template description",button.transform,detail,204,67,250,51,20,assets.uiFont,NotebookUI.Ink);
            }
            pageSlot=NotebookUI.Group("Selected draft page",transform,55,270,850,490);
            NotebookUI.Box("Review paper",transform,949,270,596,555,new Color(.94f,.93f,.87f));
            NotebookUI.Label("Recipient heading",transform,"Bản này để ai xem?",975,282,540,44,28,assets.handFont,NotebookUI.Teal);
            recipientButtons=new Button[3];
            for(int i=0;i<3;i++)
            {
                string recipient=Recipients[i];string label=i==0?"Giữ trên máy":i==1?"Gửi riêng Nhi":"Nhóm bốn người";
                recipientButtons[i]=NotebookUI.Button("Recipient "+recipient,transform,label,975,339+i*55,543,44,assets.uiFont,()=>SelectRecipient(recipient),true);
            }
            summary=NotebookUI.Label("Review summary",transform,"",975,514,540,177,22,assets.uiFont,NotebookUI.Ink);
            confirm=NotebookUI.Button("Review or confirm",transform,"Xem lại lựa chọn",975,714,543,54,assets.uiFont,Confirm);
            back=NotebookUI.Button("Back to editing",transform,"Quay lại sửa",975,780,543,35,assets.uiFont,()=>{if(Submitting)return;Reviewing=false;status.text="Bạn vẫn có thể đổi mẫu và người nhận.";Refresh(false);},true);
            status=NotebookUI.Label("Request status",transform,"",57,780,850,82,23,assets.handFont,NotebookUI.Teal);
            Refresh();
        }
        public void SelectType(string type)
        {
            if(Submitting)return;if(!Types.Contains(type))throw new ArgumentException("Unknown template");SelectedType=type;Reviewing=false;Refresh();
        }
        public void SelectRecipient(string recipient)
        {
            if(Submitting)return;if(!assets.Draft(SelectedType).recipient_options.Contains(recipient))throw new ArgumentException("Unknown recipient");
            SelectedRecipient=recipient;Reviewing=false;Refresh(false);
        }
        void Refresh(bool rebuild=true)
        {
            if(rebuild)
            {
                NotebookUI.Clear(pageSlot);int index=Array.IndexOf(Types,SelectedType);
                var view=Instantiate(assets.draftTemplates[index],pageSlot).GetComponent<DraftTemplateView>();NotebookUI.Place((RectTransform)view.transform,0,0,850,490);view.BindGesture(assets,Gesture);
            }
            for(int i=0;i<3;i++)
            {
                templateButtons[i].interactable=!Submitting;recipientButtons[i].interactable=!Submitting;
                templateButtons[i].GetComponent<Image>().color=Types[i]==SelectedType?new Color(.84f,.89f,.81f):NotebookUI.Paper;
                recipientButtons[i].GetComponent<Image>().color=Recipients[i]==SelectedRecipient?new Color(.84f,.89f,.81f):NotebookUI.Paper;
            }
            string sources=SelectedType=="tribute"?"0442 · ảnh hậu trường của Hạ":SelectedType=="paired"?"Ảnh buổi tập; chưa thêm ảnh chụp mới":"Hai ảnh hỏng và sơ đồ của Nhi";
            summary.text="Bản nháp "+Version+" · "+assets.Draft(SelectedType).title+"\nNgười xem: "+RecipientName(SelectedRecipient)+"\nPhạm vi: xem trước; chưa in hoặc đăng.\nNguồn: "+sources;
            confirm.interactable=!Submitting;
            confirm.GetComponentInChildren<Text>().text=Submitting?"Đang chờ phản hồi…":!Reviewing?"Xem lại lựa chọn":SelectedRecipient=="held"?"Xác nhận giữ trên máy":SelectedRecipient=="nhi"?"Xác nhận gửi riêng Nhi":"Xác nhận gửi nhóm bốn người";
            back.gameObject.SetActive(Reviewing&&!Submitting);
            if(!Submitting)status.text=Reviewing?"Đọc lại người nhận và phạm vi bên phải. Xem trước chưa có nghĩa là được in hoặc đăng.":"Chọn một cách kể. Chưa có nội dung nào được gửi.";
        }
        public void Confirm()
        {
            if(Submitting)return;
            if(!Reviewing){Reviewing=true;Refresh(false);return;}
            if(Requested==null){status.text="Bản nháp vẫn ở đây. Chưa có yêu cầu nào được gửi.";return;}
            var request=new DraftPreviewRequest(SelectedType,SelectedRecipient,Version);
            Submitting=true;Refresh(false);
            try{Requested(request);}catch{CompleteRequest(false,"Chưa gửi được. Bản nháp và lựa chọn của bạn vẫn còn.");}
        }
        public void CompleteRequest(bool success,string message)
        {
            if(!Submitting)return;Submitting=false;Reviewing=false;Refresh(false);status.text=message;
            if(success)confirm.interactable=false;
        }
        public static DraftReviewView Create(Transform parent,CiviAssetCatalog assets)
        {
            var paper=NotebookUI.Box("DraftReview",parent,0,0,1600,900,NotebookUI.Paper);
            var view=paper.gameObject.AddComponent<DraftReviewView>();
            NotebookUI.Label("Draft heading",paper.transform,"Trang 17 / Thử một cách kể khác",55,22,1235,63,40,assets.handFont,NotebookUI.Teal);
            NotebookUI.Stroke(paper.transform,55,86,1540,86,new Color(.76f,.69f,.57f),1);
            return view;
        }
    }
}
