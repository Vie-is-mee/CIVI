using UnityEngine;

namespace Civi.Preview
{
    // Code-native ink props: actual editable UI objects, not a flattened background.
    public static class RehearsalProps
    {
        public static GameObject Diagram(Transform parent,CiviAssetCatalog a)
        {
            var r=NotebookUI.Box("PROP_DIAGRAM",parent,0,0,720,275,NotebookUI.Paper).rectTransform;
            NotebookUI.Label("Heading",r,"Lần này / Lần trước khi ra trường",24,12,660,42,30,a.handFont,NotebookUI.Teal);
            for(int i=0;i<2;i++)
            {
                float x=26+i*350;
                NotebookUI.Box("Pencil frame",r,x,64,315,155,NotebookUI.Ink);
                NotebookUI.Box("Empty photo slot",r,x+2,66,311,151,new Color(.94f,.92f,.85f));
                NotebookUI.Label("Four names",r,"An · Nhi · Hạ · Minh",x+14,84,285,38,23,a.handFont,NotebookUI.Ink);
                NotebookUI.Label("Slot annotation",r,i==0?"tấm đã chụp":"để dành một lần đủ bốn",x+14,137,285,48,22,a.handFont,NotebookUI.Muted);
            }
            NotebookUI.Stroke(r,355,75,340,116,NotebookUI.Red,4);NotebookUI.Stroke(r,340,116,361,112,NotebookUI.Red,4);NotebookUI.Stroke(r,361,112,348,154,NotebookUI.Red,4);
            NotebookUI.Label("Margin",r,"Minh nhớ đứng thẳng nhé.",26,233,655,31,23,a.handFont,NotebookUI.Ink);return r.gameObject;
        }
        public static GameObject Clamp(Transform parent,CiviAssetCatalog a)
        {
            var r=NotebookUI.Group("PROP_CLAMP",parent,0,0,220,200);
            NotebookUI.Box("Foot",r,40,164,140,8,NotebookUI.Ink);NotebookUI.Box("Stem",r,106,70,8,99,NotebookUI.Ink);
            NotebookUI.Box("Clamp outer",r,58,20,108,61,NotebookUI.Ink);NotebookUI.Box("Empty jaw",r,68,25,88,48,NotebookUI.Paper);
            NotebookUI.Box("Rubber pad",r,68,67,88,8,NotebookUI.Teal);
            NotebookUI.Label("Pad note",r,"miếng cao su",25,88,175,33,22,a.handFont,NotebookUI.Teal);return r.gameObject;
        }
        public static GameObject Battery(Transform parent,CiviAssetCatalog a)
        {
            var r=NotebookUI.Box("PROP_BATTERY",parent,0,0,168,94,NotebookUI.Teal).rectTransform;
            NotebookUI.Label("Battery owner",r,"pin của Minh",12,9,146,32,23,a.handFont,NotebookUI.Paper);
            for(int i=0;i<4;i++)NotebookUI.Box("Charge",r,17+i*31,60,20,8,new Color(.73f,.81f,.60f));return r.gameObject;
        }
        public static GameObject Sign(Transform parent,CiviAssetCatalog a)
        {
            var r=NotebookUI.Box("PROP_SIGN_HEN",parent,0,0,250,104,new Color(.89f,.80f,.61f)).rectTransform;
            NotebookUI.Label("Letters",r,"HẸN",20,5,210,76,49,a.handFont,NotebookUI.Teal).alignment=UnityEngine.TextAnchor.MiddleCenter;
            for(int i=0;i<7;i++)NotebookUI.Box("Bulb "+i,r,14+i*32,88,7,7,new Color(.99f,.87f,.52f));return r.gameObject;
        }
    }
}
