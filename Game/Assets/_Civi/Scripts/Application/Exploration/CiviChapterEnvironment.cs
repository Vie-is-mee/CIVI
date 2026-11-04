using System;
using System.Collections.Generic;
using UnityEngine;

namespace Civi.Preview
{
    // Chapter-specific modular scenery. Legacy worlds retain their own art and navigation.
    public class CiviChapterEnvironment : MonoBehaviour
    {
        public bool Interior;
        public Texture2D PropAtlas, CastAtlas;
        public Font LabelFont;
        public Sprite[] Frames;
        public Sprite[] Cast;
        public virtual Vector2 Spawn => Interior ? new Vector2(0,-8) : new Vector2(-13,-5);
        public Vector2 PhotoPoint => new Vector2(-10,-1);
        public Vector2 DoorPoint => new Vector2(10,6);
        public Vector2 DeskPoint => new Vector2(0,-1.8f);
        public virtual Rect Bounds => Interior ? new Rect(-16,-11,32,22) : new Rect(-24,-12,48,28);
        readonly List<Rect> obstacles = new List<Rect>();
        readonly List<Transform> actors = new List<Transform>();
        Sprite[] props;
        Sprite white,ellipse;
        public virtual Sprite ContactShadow=>ellipse;
        bool built;
        public int ObstacleCount => obstacles.Count;
        public IReadOnlyList<Rect> Obstacles => obstacles;
        public virtual bool CanStand(Vector2 p, float radius=.28f)
        {
            Rect walk=Interior ? new Rect(-14.5f,-9.5f,29,15.4f) : new Rect(-22,-10,44,16.8f);
            if(p.x-radius<walk.xMin||p.x+radius>walk.xMax||p.y-radius<walk.yMin||p.y+radius>walk.yMax)return false;
            foreach(var r in obstacles)
                if(p.x+radius>r.xMin&&p.x-radius<r.xMax&&p.y+radius>r.yMin&&p.y-radius<r.yMax)return false;
            return true;
        }
        public virtual void Build()
        {
            if(built)return;
            built=true;
            white=CiviQuarterWorld2D.Pixel;
            var t=new Texture2D(64,32,TextureFormat.RGBA32,false);
            for(int y=0;y<32;y++)for(int x=0;x<64;x++)
            {float d=Mathf.Pow((x-31.5f)/31.5f,2)+Mathf.Pow((y-15.5f)/15.5f,2);t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01((1-d)*7)));}
            t.Apply();ellipse=Sprite.Create(t,new Rect(0,0,64,32),new Vector2(.5f,.5f),64);
            // Hand-selected atlas bounds retain all visible art, including the wider worktable.
            Rect[] cuts={new Rect(0,0,318,314),new Rect(320,85,310,205),new Rect(635,95,310,192),new Rect(968,40,275,250),
                new Rect(0,314,328,310),new Rect(332,326,311,282),new Rect(748,312,110,315),new Rect(962,324,288,305),
                new Rect(0,644,367,263),new Rect(375,656,272,255),new Rect(678,626,228,297),new Rect(932,634,321,286),
                new Rect(12,924,325,318),new Rect(406,964,194,276),new Rect(627,973,335,250),new Rect(1013,956,216,286)};
            props=new Sprite[cuts.Length];
            for(int i=0;i<cuts.Length;i++)
            {var r=cuts[i];r.y=PropAtlas.height-r.y-r.height;props[i]=Sprite.Create(PropAtlas,r,new Vector2(.5f,0),64);}
            Frames=new Sprite[16]; Cast=new Sprite[4];
            // Bounds centered per figure, feet on a shared baseline. Alternate contact frames form a four-beat cycle.
            int[] indices={0,1,0,2,3,4,3,5,6,8,6,9,7,10,7,11};
            for(int i=0;i<16;i++)Frames[i]=Character(indices[i]);
            for(int i=0;i<4;i++)Cast[i]=Character(12+i);
            if(Interior)Room();else Courtyard();
        }
        Sprite Character(int index)
        {
            int col=index%4,row=index/4;
            // The generated rows have equal baselines but columns are slightly inset.
            float[] centers={195,474,752,1036};
            float[] bottom={305,619,921,1277};
            float height=row==3?342:300;
            return Sprite.Create(CastAtlas,new Rect(centers[col]-114,CastAtlas.height-bottom[row],228,height),
                new Vector2(.5f,0),height/2.65f);
        }
        GameObject Shape(string name,Vector2 at,Vector2 size,Color color,int order,Sprite source=null)
        {
            var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(transform,false);
            go.transform.position=at;var sr=go.GetComponent<SpriteRenderer>();sr.sprite=source??white;
            go.transform.localScale=new Vector3(size.x/sr.sprite.bounds.size.x,size.y/sr.sprite.bounds.size.y,1);
            sr.color=color;sr.sortingOrder=order;return go;
        }
        void Box(string name,float x,float y,float w,float h,string color,int order=-1000)
            =>Shape(name,new Vector2(x,y),new Vector2(w,h),ColorOf(color),order);
        static Color ColorOf(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        int Depth(float y)=>1500-Mathf.RoundToInt(y*40);
        void Obstacle(float x,float y,float w,float h)
        {
            obstacles.Add(new Rect(x-w/2,y-h/2,w,h));
            var go=new GameObject("Solid footprint",typeof(BoxCollider2D));go.transform.SetParent(transform,false);
            go.transform.position=new Vector2(x,y);go.GetComponent<BoxCollider2D>().size=new Vector2(w,h);
        }
        void Prop(int id,float x,float y,float height,float footprintW=0,float footprintH=0)
        {
            var s=props[id];float width=height*s.bounds.size.x/s.bounds.size.y;
            Shape("Soft contact shadow",new Vector2(x,y+.10f),new Vector2(width*.87f,.46f),new Color(.12f,.22f,.20f,.20f),-450,ellipse);
            var go=new GameObject("Modular school prop "+id,typeof(SpriteRenderer));go.transform.SetParent(transform,false);
            go.transform.position=new Vector2(x,y);go.transform.localScale=Vector3.one*(height/s.bounds.size.y);
            var sr=go.GetComponent<SpriteRenderer>();sr.sprite=s;sr.sortingOrder=Depth(y);
            if(footprintW>0)Obstacle(x,y+.18f,footprintW,footprintH);
        }
        public void Label(string value,float x,float y,float width=5)
        {
            Box(value+" plaque",x,y,width,.65f,"#285253",2300);
            var go=new GameObject(value,typeof(TextMesh));go.transform.SetParent(transform,false);
            go.transform.position=new Vector3(x,y,-.1f);var text=go.GetComponent<TextMesh>();
            text.text=value;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;
            text.fontSize=64;text.characterSize=.07f;text.color=ColorOf("#FFF0CF");
            if(LabelFont!=null){text.font=LabelFont;go.GetComponent<MeshRenderer>().sharedMaterial=LabelFont.material;}
            go.GetComponent<MeshRenderer>().sortingOrder=2400;
        }
        void Paving(float x,float y,float width,float height,bool wood=false)
        {
            Box("Ground",x,y,width,height,wood?"#CDAE8B":"#D9D4C4",-1100);
            float step=wood?.7f:1f;
            for(float yy=y-height/2;yy<y+height/2;yy+=step)
            {
                int row=Mathf.RoundToInt((yy-y+height/2)/step);
                Box("Joint",x,yy,width,.025f,wood?"#AD8E6F":"#C2C5B8",-1099);
                for(float xx=x-width/2;xx<x+width/2;xx+=wood?4:1.5f)
                {float px=xx+(row%2)*(wood?2:.75f);if(px<x+width/2){
                    float span=Mathf.Min(wood?4:1.5f,x+width/2-px);
                    var tint=ColorOf(wood?"#CDAE8B":"#D9D4C4");float variation=Mathf.Sin(px*73+yy*17)*.025f;
                    tint.r+=variation;tint.g+=variation;tint.b+=variation;
                    Shape("Individual paving block",new Vector2(px+span/2,yy+step/2),new Vector2(Mathf.Max(.01f,span-.035f),step-.035f),tint,-1099);
                    if(wood)Box("Wood grain",px+span/2,yy+.24f,Mathf.Max(.01f,span-.15f),.013f,"#C3A380",-1098);
                }}
            }
        }
        void Courtyard()
        {
            Box("Campus lawn",0,2,48,28,"#8AAE91",-1200);
            Paving(0,-1,44,17);
            Box("Courtyard border",0,-9.1f,44,.25f,"#EEE5CF",-1050);
            Box("Academic wing shadow",0,7,48,1.6f,"#8FA69A",-1040);
            Box("Academic wing plaster",0,11.5f,48,9,"#EDE5D4",-900);
            Box("Academic wing base",0,7.45f,48,.75f,"#B7C5B5",-890);
            Box("Cornice shadow",0,14.4f,48,.35f,"#426E68",-880);
            Box("Teal roof edge",0,15.2f,48,1.3f,"#6D9282",-870);
            for(int x=-21;x<24;x+=7)
            {if(x>6&&x<14)continue;Prop(5,x,9,3.4f);Box("Facade pilaster",x-3,10.7f,.28f,6,"#D1CCB8",-870);}
            Prop(4,10,6.3f,7.4f);
            Label("PHÒNG THIẾT KẾ",10,11.6f,7.3f);
            Label("CIVI  /  CÂU LẠC BỘ KỶ YẾU",-6,14.2f,12);
            Prop(12,-10,1.3f,5.2f,5.2f,.7f);
            Label("HẸN",-10,6.2f,2.6f);
            Prop(13,-10,-.8f,1.5f,.75f,.55f);
            Prop(3,-18,3,4,2.1f,.6f);
            Prop(1,-17,-5.2f,1.8f,3.3f,.8f);
            Prop(1,4,-6.4f,1.8f,3.3f,.8f);
            Prop(1,16,-4,1.8f,3.3f,.8f);
            foreach(var p in new[]{new Vector2(-20,-7),new Vector2(-21,2),new Vector2(21,-6),new Vector2(21,4),new Vector2(1,4)})
                Prop(0,p.x,p.y,6.5f,1.2f,.8f);
            foreach(var p in new[]{new Vector2(-5,3.8f),new Vector2(4,5.6f),new Vector2(16,5.6f),new Vector2(-6,-7),new Vector2(10,-7)})
                Prop(2,p.x,p.y,1.5f,2.7f,.65f);
            Prop(6,-1,-5.8f,4,.3f,.35f);Prop(6,18,3,4,.3f,.35f);
            // Continuous planted edges and school garden details frame the traversable court.
            for(int x=-20;x<=20;x+=5){Prop(2,x,-9.1f,1.2f,2.2f,.55f);}
            for(int x=-22;x<=22;x+=2){Box("Garden boundary post",x,-10.3f,.12f,1,"#D9DDC6",1850);}
            Box("Garden boundary rail",0,-10.1f,44,.10f,"#E4E4CE",1851);
            Box("Garden boundary rail",0,-10.6f,44,.10f,"#D3D8C1",1851);
            // Surface inlay leads from the gathering to the design wing without covering the art.
            for(int i=0;i<8;i++)Box("Route inlay",-4+i*2,-1.8f,.35f,.12f,"#F2E8CE",-1000);
            for(int i=0;i<4;i++)Box("Route inlay",10,i*1.7f,.35f,.12f,"#F2E8CE",-1000);
            Npc(0,-12.6f,.8f,"NHI");Npc(1,-7.1f,.7f,"HẠ");Npc(2,-5.5f,-1.3f,"MINH");
        }
        void Room()
        {
            Paving(0,0,32,22,true);
            Box("Back wall",0,8.4f,32,5.2f,"#EAE3D2",-900);
            Box("Wall dado",0,6.2f,32,.45f,"#688B7D",-890);
            Box("Left wall",-15.4f,0,1.2f,22,"#DFD8C5",-890);
            Box("Right wall",15.4f,0,1.2f,22,"#DFD8C5",-890);
            Box("Entry threshold",0,-10,4,.6f,"#647F75",-880);
            Box("Studio rug edge",0,-1.5f,10,7,"#477C77",-1050);
            Box("Studio rug",0,-1.5f,9.7f,6.7f,"#64948A",-1049);
            Box("Rug inner",0,-1.5f,9.1f,6.1f,"#79A296",-1048);
            Prop(5,-9,6.8f,3.4f);Prop(5,9,6.8f,3.4f);
            Prop(11,0,5.8f,3.7f,4.7f,.6f);
            Prop(10,-12.2f,3.4f,4.4f,2.6f,.8f);Prop(10,12.2f,3.4f,4.4f,2.6f,.8f);
            Prop(8,0,0,3.15f,4.5f,1.4f);
            Prop(9,-9,-1,3.1f,3.4f,1.2f);Prop(9,9,-1,3.1f,3.4f,1.2f);
            Prop(15,-9,-3.2f,2.0f,1.2f,.8f);Prop(15,9,-3.2f,2.0f,1.2f,.8f);
            Prop(14,11,-7.6f,2.3f,4,.8f);
            Prop(7,-12.5f,-7.3f,2.8f,1.2f,.9f);Prop(7,13.3f,1,2.8f,1.2f,.9f);
            Label("BÀN THIẾT KẾ  /  TRANG 17",0,9.6f,10.3f);
            Npc(1,-3.3f,.1f,"HẠ");Npc(2,3.4f,.1f,"MINH");
        }
        void Npc(int index,float x,float y,string name)
        {
            Shape("NPC contact shadow",new Vector2(x,y),new Vector2(1.1f,.32f),new Color(.1f,.2f,.2f,.22f),-450,ellipse);
            var go=new GameObject(name,typeof(SpriteRenderer),typeof(CiviChapterDepth));go.transform.SetParent(transform,false);
            go.transform.position=new Vector2(x,y);go.GetComponent<SpriteRenderer>().sprite=Cast[index];
            actors.Add(go.transform);Obstacle(x,y,.65f,.45f);
        }
        void Update()
        {
            for(int i=0;i<actors.Count;i++)actors[i].localScale=new Vector3(1,1+.006f*Mathf.Sin(Time.time*2+i),1);
        }
    }
    public sealed class CiviChapterDepth:MonoBehaviour
    {
        public int Offset;
        SpriteRenderer render;
        void LateUpdate(){if(render==null)render=GetComponent<SpriteRenderer>();render.sortingOrder=1500-Mathf.RoundToInt(transform.position.y*40)+Offset;}
    }
}
