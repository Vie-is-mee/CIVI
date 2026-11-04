using System.Collections.Generic;
using UnityEngine;

namespace Civi.Preview
{
    // Inherits the polished environment seam; movement itself is unchanged.
    public sealed class CiviLiuEnvironment : CiviChapterEnvironment
    {
        public Texture2D SupportAtlas, EvidenceAtlas, CityAtlas, WalkAtlas;
        public int Room;
        public readonly List<Rect> Solids=new List<Rect>();
        public override Vector2 Spawn=>Room==0?new Vector2(0,-2.5f):new Vector2(0,-6);
        public override Rect Bounds=>new Rect(-16,-11,32,22);
        public override Sprite ContactShadow=>shadow;
        public Sprite House,Album;
        Sprite shadow,pixel,city; Sprite[] props,support; Transform scenery;
        readonly List<Transform> people=new List<Transform>();
        readonly List<Sprite> owned=new List<Sprite>();
        public override bool CanStand(Vector2 p,float radius=.28f)
        {
            if(p.x-radius < -14||p.x+radius>14||p.y-radius< -8.7f||p.y+radius>5)return false;
            foreach(var r in Solids)if(p.x+radius>r.xMin&&p.x-radius<r.xMax&&p.y+radius>r.yMin&&p.y-radius<r.yMax)return false;
            return true;
        }
        Sprite Slice(Texture2D atlas,Rect top,float height,bool trim=true)
        {
            // Trim only transparent padding inside authored cell; preserve the source PNG.
            int x0=Mathf.RoundToInt(top.x),x1=Mathf.Min(atlas.width,Mathf.RoundToInt(top.xMax));
            int y0=Mathf.Max(0,atlas.height-Mathf.RoundToInt(top.yMax)),y1=atlas.height-Mathf.RoundToInt(top.y);
            if(trim){int ax=x1,ay=y1,bx=x0,by=y0;for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(atlas.GetPixel(x,y).a>.12f){ax=Mathf.Min(ax,x);ay=Mathf.Min(ay,y);bx=Mathf.Max(bx,x);by=Mathf.Max(by,y);}if(bx>=ax&&by>=ay){x0=ax;x1=bx+1;y0=ay;y1=by+1;}}
            var s=Sprite.Create(atlas,new Rect(x0,y0,x1-x0,y1-y0),new Vector2(.5f,0),(y1-y0)/height);owned.Add(s);return s;
        }
        public override void Build()
        {
            if(props!=null)return;
            pixel=CiviQuarterWorld2D.Pixel;
            var tex=new Texture2D(64,32,TextureFormat.RGBA32,false);
            for(int y=0;y<32;y++)for(int x=0;x<64;x++){float d=Mathf.Pow((x-31.5f)/31.5f,2)+Mathf.Pow((y-15.5f)/15.5f,2);tex.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01((1-d)*7)));}tex.Apply();
            shadow=Sprite.Create(tex,new Rect(0,0,64,32),new Vector2(.5f,.5f),64);
            // Actual generated cell centers and baselines, reviewed against original atlas.
            float cw=CastAtlas.width/4f,ch=CastAtlas.height/4f;
            float[] centers={cw*.5f,cw*1.5f,cw*2.5f,cw*3.5f}; float[] rows={0,ch,ch*2,ch*3};float[] heights={ch,ch,ch,ch};
            var frames=new Sprite[12];for(int i=0;i<12;i++)frames[i]=Slice(CastAtlas,new Rect(centers[i%4]-105,rows[i/4],210,heights[i/4]),2.65f);
            int[] order={0,1,0,2,3,4,3,5,6,8,6,9,7,10,7,11};Frames=new Sprite[16];for(int i=0;i<16;i++)Frames[i]=frames[order[i]];
            if(WalkAtlas!=null)
            {
                if(WalkAtlas.width%6!=0||WalkAtlas.height%4!=0)
                    throw new System.FormatException("Reviewed 24-frame walking atlas grid mismatch");
                int cellW=WalkAtlas.width/6, cellH=WalkAtlas.height/4;
                WalkFramesSix=new Sprite[24];
                for(int direction=0;direction<4;direction++)
                    for(int frame=0;frame<6;frame++)
                        WalkFramesSix[direction*6+frame]=Slice(WalkAtlas,
                            new Rect(frame*cellW,direction*cellH,cellW,cellH),2.65f);
            }
            Cast=new Sprite[4];for(int i=0;i<4;i++)Cast[i]=Slice(CastAtlas,new Rect(centers[i]-108,ch*3,216,ch),2.65f);
            Rect[] cuts={new Rect(0,0,384,304),new Rect(390,0,210,304),new Rect(618,0,312,304),new Rect(932,0,346,304),
                new Rect(0,305,366,311),new Rect(387,305,216,311),new Rect(630,305,270,311),new Rect(906,305,372,311),
                new Rect(0,616,306,296),new Rect(365,616,235,296),new Rect(620,616,306,280),new Rect(1005,616,232,278),
                new Rect(0,912,300,318),new Rect(303,912,364,318),new Rect(676,912,322,318),new Rect(1000,912,278,318)};
            props=new Sprite[16];for(int i=0;i<16;i++)props[i]=Slice(PropAtlas,cuts[i],3);
            Rect[] supportCuts={new Rect(240,0,260,480),new Rect(640,0,300,480),new Rect(1050,0,320,480),new Rect(240,480,340,544),new Rect(640,480,300,544),new Rect(1050,480,320,544)};
            support=new Sprite[6];for(int i=0;i<6;i++)support[i]=Slice(SupportAtlas,supportCuts[i],i==0?1.7f:i==2?2.1f:2.65f);
            House=Slice(EvidenceAtlas,new Rect(0,0,1024,768),1,false);Album=Slice(EvidenceAtlas,new Rect(1024,0,1024,768),1,false);
            city=Slice(CityAtlas,new Rect(0,0,CityAtlas.width,CityAtlas.height),1,false);
            ShowRoom(Room);
        }
        static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        GameObject Shape(string name,float x,float y,float w,float h,string color,int order,Sprite s=null)
        {
            var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(scenery,false);go.transform.position=new Vector3(x,y,0);
            var r=go.GetComponent<SpriteRenderer>();r.sprite=s??pixel;r.color=C(color);r.sortingOrder=order;go.transform.localScale=new Vector3(w/r.sprite.bounds.size.x,h/r.sprite.bounds.size.y,1);return go;
        }
        void Solid(float x,float y,float w,float h){Solids.Add(new Rect(x-w/2,y-h/2,w,h));var go=new GameObject("Footprint",typeof(BoxCollider2D));go.transform.SetParent(scenery,false);go.transform.position=new Vector2(x,y);go.GetComponent<BoxCollider2D>().size=new Vector2(w,h);}
        void Prop(int id,float x,float y,float h,float w=0,float d=0)
        {var s=props[id];float width=h*s.bounds.size.x/s.bounds.size.y;Shape("Contact",x,y+.08f,width*.8f,.35f,"#263D3433",-450,shadow);Shape("Prop "+id,x,y+h/2,width,h,"#FFFFFF",1500-Mathf.RoundToInt(y*40),s).transform.position=new Vector2(x,y);if(w>0)Solid(x,y+.15f,w,d);}
        GameObject Person(Sprite s,float x,float y,string name){Shape("Shadow",x,y,.95f,.25f,"#163F383F",-450,shadow);var go=new GameObject(name,typeof(SpriteRenderer),typeof(CiviChapterDepth));go.transform.SetParent(scenery,false);go.transform.position=new Vector2(x,y);go.GetComponent<SpriteRenderer>().sprite=s;people.Add(go.transform);Solid(x,y,.62f,.4f);return go;}
        void LabelRoom(string title){Shape("Room plaque",0,4.95f,16,.6f,"#315C57",2190);var go=new GameObject(title,typeof(TextMesh));go.transform.SetParent(scenery,false);go.transform.position=new Vector3(0,4.95f,0);var t=go.GetComponent<TextMesh>();t.text=title;t.font=LabelFont;t.fontSize=64;t.characterSize=.065f;t.anchor=TextAnchor.MiddleCenter;t.color=C("#EBDCB2");var mr=go.GetComponent<MeshRenderer>();mr.sharedMaterial=LabelFont.material;mr.sortingOrder=2200;}
        public void ShowRoom(int room)
        {
            Room=room;if(scenery!=null){scenery.gameObject.SetActive(false);if(Application.isPlaying)Destroy(scenery.gameObject);else DestroyImmediate(scenery.gameObject);}Solids.Clear();people.Clear();scenery=new GameObject("Lưu Quang room "+room).transform;scenery.SetParent(transform,false);
            bool home=room==5;string floor=home?"#B99D7F":"#D3D1BD";
            Shape("Underfloor",0,0,32,22,floor,-1200);
            for(int y=-11;y<6;y++)for(int x=-16;x<16;x+=2){Shape("Floor tile",x+1,y+.5f,1.97f,.97f,(x+y)%3==0?(home?"#BFA385":"#D5D3BF"):floor,-1190);}
            Shape("Wall",0,8,32,6,home?"#3F6664":"#497873",-1000);Shape("Dado",0,5.35f,32,.35f,"#B8A06C",-980);
            // Distant city stays behind glazing; interior props and collisions are independent.
            float cityHeight=29f*CityAtlas.height/CityAtlas.width;
            Shape("Dome city through window",0,5.5f,29,cityHeight,home?"#667C99":"#FFFFFF",-980,city);
            for(int x=-15;x<=15;x+=5)Shape("Window mullion",x,8,.16f,6,"#315C57",-970);
            Shape("Rug border",0,-.6f,14,6.5f,home?"#685F59":"#3D7771",-1100);Shape("Rug",0,-.6f,13.7f,6.2f,home?"#97816C":"#89ADA0",-1099);
            Prop(15,-12,3,3.3f,1,.65f);Prop(15,12,3,3.3f,1,.65f);Prop(14,0,-8.5f,3.2f); // entry is decorative, no blocked spawn
            if(room==0){Prop(4,-4,0,4,3,1.6f);Prop(5,-8,1,3,.9f,.7f);Person(support[1],-4,.35f,"Mother / memory").GetComponent<CiviChapterDepth>().Offset=60;Person(Cast[3],-.5f,.5f,"Nurse");Person(support[0],-1.7f,-.4f,"Mai age8");Person(support[5],5,.7f,"Clerk");LabelRoom("MÙA LẠNH THỨ 17  /  KÝ ỨC");}
            else if(room==1){Prop(0,-5,0,3.3f,4.4f,1.1f);Prop(11,6,.6f,3.8f,1.9f,.55f);Prop(12,-9,-3.3f,2.2f,1,.65f);Prop(10,11,-5,1.8f,1.6f,.8f);Person(Cast[0],3.7f,.1f,"Linh");Prop(13,-11,1,2.6f,2.4f,.65f);LabelRoom("ĐIỀU PHỐI MÁI VÒM  /  TẦNG 42");}
            else if(room==2){Prop(1,-5,0,4.2f,1.8f,.8f);Prop(1,5,1,3.7f,1.8f,.8f);Person(support[2],-1,.2f,"Student");Person(support[3],1.4f,.9f,"Guide");Person(support[4],7,-3,"Colleague");LabelRoom("THẺ KHÍ  /  LỊCH SỬ PHÂN BỔ");}
            else if(room==3){Prop(2,0,0,2.2f,4.2f,1.1f);Prop(2,-10,2,1.8f,3,.9f);Prop(12,7,-2.4f,2.1f,1,.65f);Person(Cast[0],3.5f,.1f,"Linh");Prop(13,10,2,2.5f,2.4f,.7f);LabelRoom("CĂNG TIN  /  GIỜ NGHỈ TRƯA");}
            else if(room==4){Prop(3,0,0,3.4f,5.4f,1.5f);Prop(11,-8,2,4.1f,1.9f,.6f);Person(Cast[1],4,.6f,"Director Vinh");Prop(12,10,-3,2.1f,1,.65f);LabelRoom("DỰ BÁO  /  CÒN MƯỜI HAI NGÀY");}
            else {Prop(7,-6,.3f,2.5f,4.3f,1);Prop(8,3,0,2.4f,2,.8f);Prop(6,9,2,3.1f,1.2f,.7f);Prop(13,-11,2,2.6f,2.5f,.8f);Person(Cast[2],6,1.1f,"Mother");LabelRoom("CĂN HỘ CỦA MAI  /  ĐÊM");}
        }
        void Update(){for(int i=0;i<people.Count;i++)people[i].localScale=new Vector3(1,1+.005f*Mathf.Sin(Time.time*1.7f+i),1);}
        void OnDestroy(){foreach(var s in owned)if(s!=null)Destroy(s);if(shadow!=null){Destroy(shadow.texture);Destroy(shadow);}}
    }
}
