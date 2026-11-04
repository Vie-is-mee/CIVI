using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Civi.Runtime;

namespace Civi.Preview
{
    // Scroll to zoom, drag to pan. The source Texture2D is NEVER modified.
    [RequireComponent(typeof(RawImage))]
    public sealed class CiviPhotoPanZoom : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler
    {
        RawImage photo;
        float zoom=1f, originX, originY;
        void Awake(){ photo=GetComponent<RawImage>();ResetView(); }
        public void ResetView()
        {
            if(photo==null)photo=GetComponent<RawImage>();
            zoom=1f;originX=originY=0f;Reframe();
        }
        public void ChangeZoom(float delta,Vector2 focus)
        {
            float next=Mathf.Clamp(zoom+delta,1f,3.6f);
            float before=1f/zoom, after=1f/next;
            originX=Mathf.Clamp(originX+focus.x*(before-after),0f,1f-after);
            originY=Mathf.Clamp(originY+focus.y*(before-after),0f,1f-after);
            zoom=next;Reframe();
        }
        public void OnScroll(PointerEventData e)
        {
            Vector2 local;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform,e.position,e.pressEventCamera,out local))return;
            var r=((RectTransform)transform).rect;
            var uv=new Vector2(Mathf.Clamp01((local.x-r.xMin)/r.width),
                Mathf.Clamp01((local.y-r.yMin)/r.height));
            ChangeZoom(Mathf.Sign(e.scrollDelta.y)*.28f,uv);
        }
        public void OnBeginDrag(PointerEventData e){}
        public void OnDrag(PointerEventData e)
        {
            var rect=((RectTransform)transform).rect;
            if(rect.width<=0||rect.height<=0)return;
            float visible=1f/zoom;
            originX=Mathf.Clamp(originX-e.delta.x/rect.width*visible,0f,1f-visible);
            originY=Mathf.Clamp(originY-e.delta.y/rect.height*visible,0f,1f-visible);
            Reframe();
        }
        void Reframe()
        {
            float v=1f/zoom;
            photo.uvRect=new Rect(originX,originY,v,v);
        }
    }

    // Views are owned by the existing Exploration modal and 100% driven by approved photo assets.
    public static class CiviEvidenceWorkbenchUI
    {
        static readonly string[] photoIds={"PHOTO_0441","PHOTO_0442","PHOTO_0443"};

        static string[] Answers(string id)
        {
            switch(id)
            {
                case "PHOTO_0441": return new[]
                {
                    "Nhi đứng ở mép ảnh, đang cầm dây; đèn chưa sáng.",
                    "Ảnh chứng minh Nhi tự sửa toàn bộ thiết bị.",
                    "Nhi hoàn toàn không xuất hiện trong khung."
                };
                case "PHOTO_0442": return new[]
                {
                    "Ảnh đã xác nhận tất cả đồng ý cho in.",
                    "Đèn sáng, ổ chia trên ghế đỏ, Nhi nhìn về phía An.",
                    "Đây là ảnh chụp mới ở buổi hẹn tháng Năm."
                };
                case "PHOTO_0443": return new[]
                {
                    "Có thể nới crop để thấy Nhi trong ảnh.",
                    "Hạ cố ý không cho Nhi chụp cùng.",
                    "Ảnh gốc chỉ có An, Hạ, Minh; không có Nhi."
                };
                case "PHOTO_CLAMP_EMPTY":return new[]
                {
                    "Chiếc kẹp trống tự chứng minh Nhi đã cầm máy.",
                    "Ảnh chứng minh đoạn clip của Vy đã bị xóa.",
                    "Kẹp trống là dấu hiệu cần kiểm chứng bằng nguồn khác."
                };
                default:throw new ArgumentException("Unknown authored photo "+id);
            }
        }
        static int CorrectIndex(string id)
        {
            if(id=="PHOTO_0441")return 0;
            if(id=="PHOTO_0442")return 1;
            if(id=="PHOTO_0443")return 2;
            if(id=="PHOTO_CLAMP_EMPTY")return 2;
            throw new ArgumentException("Unknown photo "+id);
        }

        public static bool AnswerIsCorrect(string id,int index)
        {
            if(!InvestigationNotebook.IsAllowed(id)||index<0||index>2)return false;
            return index==CorrectIndex(id);
        }

        public static void DrawInspector(RectTransform modal,CiviAssetCatalog catalog,
            InvestigationNotebook notebook,string id,Action onObserve,Action onClose)
        {
            if(!InvestigationNotebook.IsAllowed(id))throw new ArgumentException(id);
            InvestigationNotebook.Entry entry=null;
            foreach(var item in InvestigationNotebook.Sources)if(item.Id==id)entry=item;
            Texture2D texture=catalog.Texture(id);
            if(texture==null)throw new InvalidOperationException("Image missing "+id);
            var frame=NotebookUI.Group("Photo viewport",modal,170,142,823,383);
            var cover=frame.gameObject.AddComponent<Image>();
            cover.color=new Color(.13f,.20f,.20f);
            cover.raycastTarget=false;
            frame.gameObject.AddComponent<RectMask2D>();
            var image=NotebookUI.Photo("Original unchanged",frame,texture,0,0,823,383);
            FitAspect(image,texture,823,383);
            image.raycastTarget=true;
            var zoom=image.gameObject.AddComponent<CiviPhotoPanZoom>();
            NotebookUI.Label("Source",modal,entry.Title,174,526,790,34,22,catalog.uiFont,NotebookUI.Teal);
            NotebookUI.Label("Question",modal,"QUAN SÁT TRONG ẢNH, RỒI CHỌN NHẬN ĐỊNH ĐÚNG:",1026,141,367,63,22,catalog.uiFont,NotebookUI.Teal);
            var feedback=NotebookUI.Label("Inspect feedback",modal,
                "Lăn chuột để phóng to; giữ chuột kéo để xem vùng khác.",1028,486,348,79,19,
                catalog.diaryFont,NotebookUI.Muted);
            var answers=Answers(id);
            for(int i=0;i<answers.Length;i++)
            {
                int chosen=i;
                NotebookUI.Button("Inspect answer "+i,modal,answers[i],
                    1020,211+i*91,380,80,catalog.uiFont,()=>
                    {
                        if(AnswerIsCorrect(id,chosen))onObserve();
                        else
                        {
                            feedback.text="Chưa đúng với nguồn ảnh. Hãy xem lại trước khi kết luận.";
                            feedback.color=NotebookUI.Red;
                        }
                    },true);
            }
            NotebookUI.Button("Zoom +",modal,"+ PHÓNG",170,576,203,40,catalog.uiFont,()=>zoom.ChangeZoom(.40f,new Vector2(.5f,.5f)),true);
            NotebookUI.Button("Zoom -",modal,"− THU",391,576,192,40,catalog.uiFont,()=>zoom.ChangeZoom(-.40f,new Vector2(.5f,.5f)),true);
            NotebookUI.Button("Reset",modal,"VỀ TOÀN ẢNH",598,576,185,40,catalog.uiFont,zoom.ResetView,true);
            NotebookUI.Button("Close",modal,"ĐÓNG / ESC",1005,574,378,42,catalog.uiFont,onClose,true);
        }

        static void FitAspect(RawImage image,Texture2D texture,float w,float h)
        {
            float k=Mathf.Min(w/texture.width,h/texture.height);
            float rw=texture.width*k,rh=texture.height*k;
            NotebookUI.Place(image.rectTransform,(w-rw)/2f,(h-rh)/2f,rw,rh);
        }
        static bool Has(InvestigationNotebook notebook,string id) {return notebook.Has(id);}

        public static void DrawBoard(RectTransform modal,CiviAssetCatalog assets,
            InvestigationNotebook notebook,Action compare12,Action compare23,Action deduction,Action close)
        {
            for(int i=0;i<3;i++)
            {
                string id=photoIds[i];
                var card=NotebookUI.Photo("Evidence card",modal,assets.Texture(id),170+i*407,154,378,252);
                var texture=assets.Texture(id);
                if(texture!=null)
                {
                    float factor=Mathf.Min(378f/texture.width,252f/texture.height);
                    NotebookUI.Place(card.rectTransform,170+i*407+(378-texture.width*factor)*.5f,
                        154+(252-texture.height*factor)*.5f,texture.width*factor,texture.height*factor);
                }
                NotebookUI.Label("Evidence status",modal,id+"  /  "+(Has(notebook,id)?"ĐÃ QUAN SÁT":"CHƯA QUAN SÁT"),
                    168+i*407,424,383,46,19,assets.uiFont,Has(notebook,id)?NotebookUI.Teal:NotebookUI.Red);
            }
            NotebookUI.Label("Board conclusion",modal,
                "Quan sát "+notebook.ObservedCount+"/4 nguồn. So sánh chỉ hỗ trợ giả thuyết; lời kể hoặc video mới giúp kiểm chứng.",
                178,478,1190,50,21,assets.diaryFont,NotebookUI.Ink);
            if(Has(notebook,"PHOTO_0441")&&Has(notebook,"PHOTO_0442"))
                NotebookUI.Button("Compare 1-2",modal,"SO 0441 ↔ 0442",172,551,351,53,assets.uiFont,compare12,true);
            if(Has(notebook,"PHOTO_0442")&&Has(notebook,"PHOTO_0443"))
                NotebookUI.Button("Compare 2-3",modal,"SO 0442 ↔ 0443",535,551,351,53,assets.uiFont,compare23,true);
            NotebookUI.Button("Deduce",modal,"THỬ GIẢ THUYẾT",899,551,310,53,assets.uiFont,deduction);
            NotebookUI.Button("Close",modal,"ĐÓNG",1221,551,166,53,assets.uiFont,close,true);
        }

        public static void DrawComparison(RectTransform modal,CiviAssetCatalog assets,
            string left,string right,Action back,Action close)
        {
            var a=assets.Texture(left);var b=assets.Texture(right);
            var imgA=NotebookUI.Photo("Compare left",modal,a,164,154,599,348);
            var imgB=NotebookUI.Photo("Compare right",modal,b,796,154,599,348);
            FitPanel(imgA,a,164,154,599,348);
            FitPanel(imgB,b,796,154,599,348);
            NotebookUI.Label("A ID",modal,left,183,506,500,34,24,assets.uiFont,NotebookUI.Teal);
            NotebookUI.Label("B ID",modal,right,820,506,500,34,24,assets.uiFont,NotebookUI.Teal);
            string insight=left=="PHOTO_0441"
                ?"Ảnh 0441 đèn tắt; ảnh 0442 đèn sáng. Hãy đối chiếu vị trí dây, ổ chia và Nhi. Hai ảnh không chứng minh ai làm toàn bộ."
                :"Ảnh 0442 có Nhi; ảnh 0443 chỉ có An, Hạ, Minh. Sự thay đổi khung hình chưa đủ kết luận ai cố ý loại Nhi.";
            NotebookUI.Label("Comparison note",modal,insight,170,543,1005,69,22,assets.diaryFont,NotebookUI.Ink);
            NotebookUI.Button("Board",modal,"TRỞ LẠI BẢNG",1176,542,212,64,assets.uiFont,back,true);
        }
        static void FitPanel(RawImage image,Texture2D tex,float x,float y,float w,float h)
        {
            if(tex==null)return;
            float k=Mathf.Min(w/tex.width,h/tex.height);
            NotebookUI.Place(image.rectTransform,x+(w-tex.width*k)*.5f,y+(h-tex.height*k)*.5f,
                tex.width*k,tex.height*k);
        }

        public static void DrawDeduction(RectTransform modal,CiviAssetCatalog assets,
            InvestigationNotebook notebook,Action back)
        {
            NotebookUI.Label("Question",modal,
                "TỪ CÁC NGUỒN ĐANG CÓ, CÓ THỂ KẾT LUẬN ĐIỀU GÌ?",
                167,156,1168,64,28,assets.diaryFont,NotebookUI.Ink);
            var response=NotebookUI.Label("Deduction response",modal,
                notebook.HasWorkingHypothesis?
                    "Giả thuyết làm việc đã được ghi chú. Vẫn cần kiểm chứng trực tiếp.":"Hãy chọn một giả thuyết để nhận phản hồi.",
                175,444,1130,85,24,assets.diaryFont,NotebookUI.Teal);
            var options=new[]
            {
                new[]{"intentional","Hạ đã cố ý loại Nhi khỏi ảnh 0443."},
                new[]{"unconfirmed","Nhi vắng khỏi khung 0443; cần nguồn khác để biết vì sao."},
                new[]{"consent","Có ảnh Nhi nghĩa là được phép đưa lên trang in."}
            };
            for(int i=0;i<options.Length;i++)
            {
                string key=options[i][0];
                NotebookUI.Button("Deduction "+key,modal,options[i][1],181,234+i*69,1180,57,
                    assets.uiFont,()=>
                    {
                        var answer=notebook.Propose(key);
                        response.text=answer.Feedback;
                        response.color=answer.Result==InvestigationNotebook.Verdict.PlausibleNeedsVerification?
                            NotebookUI.Teal:NotebookUI.Red;
                    },true);
            }
            NotebookUI.Button("Back to board",modal,"TRỞ LẠI BẢNG",1017,551,343,51,assets.uiFont,back);
        }
    }
}
