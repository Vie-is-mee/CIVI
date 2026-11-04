using UnityEngine;

namespace Civi.Preview
{
    public sealed class DraftTemplateView : MonoBehaviour
    {
        public string draftType;
        public UnityEngine.UI.RawImage openingPhoto;
        public void BindGesture(CiviAssetCatalog assets,string gesture){if(openingPhoto!=null)openingPhoto.texture=assets.Gesture("S00",gesture);}
        static void Photo(Transform parent,CiviAssetCatalog assets,string id,float x,float y,float w,float h)
        {
            NotebookUI.Box("Photo backing",parent,x-5,y-5,w+10,h+36,Color.white);
            NotebookUI.Photo(id,parent,assets.Texture(id),x,y,w,h);
        }
        public static DraftTemplateView Create(string type,Transform parent,CiviAssetCatalog a)
        {
            var paper=NotebookUI.Box("TEMPLATE_"+type.ToUpperInvariant(),parent,0,0,850,490,NotebookUI.Paper);
            var view=paper.gameObject.AddComponent<DraftTemplateView>();view.draftType=type;
            NotebookUI.Stroke(paper.transform,24,66,826,66,new Color(.79f,.73f,.62f),1);
            NotebookUI.Label("Private draft stamp",paper.transform,"NHÁP RIÊNG / CHƯA IN, CHƯA ĐĂNG",24,447,800,27,17,a.uiFont,NotebookUI.Muted);
            if(type=="tribute")
            {
                NotebookUI.Label("Template title",paper.transform,"Cảm ơn Nhi",25,12,790,48,34,a.handFont,NotebookUI.Teal);
                Photo(paper.transform,a,"PHOTO_0442",27,91,480,320);
                NotebookUI.Label("Source",paper.transform,"0442 · hội chợ · An, Nhi, Hạ, Minh",34,416,476,28,18,a.handFont,NotebookUI.Muted);
                NotebookUI.Label("Caption proposal",paper.transform,"Hôm ấy, đèn đã sáng.\nCó những việc không nằm trong tấm ảnh.\n\nMình muốn hỏi Nhi trước khi giữ trang này.",544,111,275,285,26,a.diaryFont,NotebookUI.Ink);
            }
            else if(type=="paired")
            {
                NotebookUI.Label("Template title",paper.transform,"Trước và bây giờ",25,12,790,48,34,a.handFont,NotebookUI.Teal);
                NotebookUI.Box("Opening photo backing",paper.transform,22,88,390,310,Color.white);
                view.openingPhoto=NotebookUI.Photo("SOURCE_S00",paper.transform,a.Gesture("S00","pose_1"),30,96,374,249);
                NotebookUI.Label("Opening source",paper.transform,"Buổi tập · tháng Mười",33,357,369,34,23,a.handFont,NotebookUI.Teal);
                NotebookUI.Box("Future photo frame",paper.transform,439,88,390,310,new Color(.92f,.91f,.85f));
                NotebookUI.Label("Not captured",paper.transform,"Để dành lần chụp tới",464,172,340,70,30,a.handFont,NotebookUI.Muted);
                NotebookUI.Label("Future source",paper.transform,"Chưa có ảnh mới",464,253,340,45,22,a.uiFont,NotebookUI.Muted);
                NotebookUI.Label("Pair caption",paper.transform,"Hai thời điểm. Không cần đứng lại y hệt ngày trước.",26,405,800,34,23,a.handFont,NotebookUI.Ink);
            }
            else if(type=="memory")
            {
                NotebookUI.Label("Template title",paper.transform,"Những gì được giữ lại",25,12,790,48,34,a.handFont,NotebookUI.Teal);
                Photo(paper.transform,a,"PHOTO_S00_BAD_01",27,88,470,313);
                Photo(paper.transform,a,"PHOTO_S00_BAD_02",545,88,280,187);
                NotebookUI.Label("First failed photo",paper.transform,"01 / Đèn vừa tắt. Cả bốn vẫn ở đây.",35,406,455,33,21,a.handFont,NotebookUI.Teal);
                NotebookUI.Label("Second failed photo",paper.transform,"02 / Minh còn nhặt bìa",551,281,269,30,20,a.handFont,NotebookUI.Teal);
                var sketch=Object.Instantiate(a.diagramPrefab,paper.transform).GetComponent<RectTransform>();
                NotebookUI.Place(sketch,545,320,720,275);sketch.localScale=Vector3.one*.36f;
                NotebookUI.Label("Sketch source",paper.transform,"Sơ đồ Nhi giữ sau buổi tập",545,420,280,25,17,a.handFont,NotebookUI.Teal);
            }
            else throw new System.ArgumentException("Unknown draft type");
            return view;
        }
    }
}
