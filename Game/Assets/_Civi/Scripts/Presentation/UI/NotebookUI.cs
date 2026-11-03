using System;
using UnityEngine;
using UnityEngine.UI;

namespace Civi.Preview
{
    public static class NotebookUI
    {
        public static readonly Color Paper = new Color(.979f,.962f,.915f), Ink = new Color(.13f,.19f,.20f), Teal = new Color(.13f,.32f,.32f), Muted = new Color(.43f,.47f,.44f), Red = new Color(.65f,.31f,.25f);
        public static RectTransform Group(string name, Transform parent, float x,float y,float w,float h)
        {
            var node = new GameObject(name, typeof(RectTransform)); node.transform.SetParent(parent,false);
            var rect = (RectTransform)node.transform; Place(rect,x,y,w,h); return rect;
        }
        public static void Place(RectTransform rect,float x,float y,float w,float h)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0,1); rect.pivot = new Vector2(0,1);
            rect.anchoredPosition = new Vector2(x,-y); rect.sizeDelta = new Vector2(w,h);
        }
        public static Image Box(string name,Transform parent,float x,float y,float w,float h,Color color)
        {
            var rect = Group(name,parent,x,y,w,h); var image = rect.gameObject.AddComponent<Image>(); image.color=color; image.raycastTarget=false; return image;
        }
        public static Text Label(string name,Transform parent,string text,float x,float y,float w,float h,int size,Font font,Color color)
        {
            var rect = Group(name,parent,x,y,w,h); var label = rect.gameObject.AddComponent<Text>(); label.font=font;label.fontSize=size;label.color=color;label.text=text;
            label.supportRichText=false;label.raycastTarget=false;label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Truncate;label.lineSpacing=1.16f; return label;
        }
        public static RawImage Photo(string name,Transform parent,Texture2D texture,float x,float y,float w,float h,Rect? uv=null)
        {
            var rect=Group(name,parent,x,y,w,h);var image=rect.gameObject.AddComponent<RawImage>();image.texture=texture;image.uvRect=uv??new Rect(0,0,1,1);image.raycastTarget=false;return image;
        }
        public static Button Button(string name,Transform parent,string label,float x,float y,float w,float h,Font font,Action action,bool secondary=false)
        {
            var backing=Box(name,parent,x,y,w,h,secondary?Paper:Teal);backing.raycastTarget=true;
            var button=backing.gameObject.AddComponent<Button>();button.targetGraphic=backing;
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(.84f,.91f,.87f);colors.pressedColor=new Color(.62f,.76f,.69f);colors.selectedColor=new Color(.88f,.93f,.86f);colors.fadeDuration=.1f;button.colors=colors;
            if(secondary){var outline=backing.gameObject.AddComponent<Outline>();outline.effectColor=Teal;outline.effectDistance=new Vector2(1,-1);}
            var caption=Label("Label",backing.transform,label,16,4,w-32,h-8,Mathf.Clamp((int)(h*.40f),16,22),font,secondary?Teal:Paper);caption.alignment=TextAnchor.MiddleCenter;
            button.onClick.AddListener(()=>action?.Invoke());return button;
        }
        public static void Stroke(Transform parent,float x1,float y1,float x2,float y2,Color color,float thickness=2)
        {
            var d=new Vector2(x2-x1,y2-y1);var line=Box("Ink stroke",parent,x1,y1,d.magnitude,thickness,color).rectTransform;
            line.pivot=new Vector2(0,.5f);line.localRotation=Quaternion.Euler(0,0,-Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg);
        }
        public static void Clear(Transform parent){foreach(Transform child in parent){child.gameObject.SetActive(false);UnityEngine.Object.Destroy(child.gameObject);}}
    }
}
