using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Civi.Preview
{
    [Serializable] public sealed class CiviStudioSaveMap
    {
        public int width,height;
        public string[] navigationRows;
        public CiviSchoolSpawn spawn,exit;
    }
    public sealed class CiviSchoolStudioV2 : MonoBehaviour
    {
        public TextAsset RoomJson;
        public Sprite Ground,Architecture,Props;
        public Sprite[] StudentFrames=new Sprite[16];
        public bool ChapterMode;
        public CiviChapterEnvironment ChapterEnvironment;
        public bool ExternalLock;
        public string ActorLabel="An / design studio";
        CiviStudioSaveMap map;
        Rigidbody2D actorBody;
        SpriteRenderer sprite;
        Transform actor;
        CiviFollowCamera2D cameraFollow;
        Texture2D pixel;
        Vector2 direction;
        int facing;
        bool info,ready;
        float timer;
        GUIStyle label,heading,smaller;
        public bool IsReady {get{return ready;}}
        public int Width{get{return map==null?0:map.width;}}
        public Transform Player{get{return actor;}}
        public bool HasCamera{get{return cameraFollow!=null&&cameraFollow.Target==actor;}}
        void Start()
        {
            if(ready)return;
            try{Boot();}
            catch(Exception e){Debug.LogException(e);enabled=false;}
        }
        Vector3 Cell(int x,int y){return new Vector3(x+.5f-map.width*.5f,y+.5f-map.height*.5f,0);}
        public bool Walkable(Vector2 v)
        {
            if(ChapterEnvironment!=null)return ChapterEnvironment.CanStand(v);
            if(map==null)return false;
            int x=Mathf.FloorToInt(v.x+map.width*.5f);
            int y=Mathf.FloorToInt(v.y+map.height*.5f);
            return x>=0&&x<map.width&&y>=0&&y<map.height&&
                map.navigationRows[y][x]=='1';
        }
        public void Boot()
        {
            if(ready)return;
            if(ChapterEnvironment==null&&(RoomJson==null||Ground==null||Architecture==null||Props==null||
               StudentFrames==null||StudentFrames.Length!=16||StudentFrames.Any(s=>s==null)))
                throw new InvalidOperationException("Missing Civi Design Studio v2 references");
            if(ChapterEnvironment==null){
            map=JsonUtility.FromJson<CiviStudioSaveMap>(RoomJson.text);
            if(map==null||map.width!=32||map.height!=22||map.navigationRows.Length!=22)
                throw new FormatException("Studio map schema mismatch");
            foreach(var s in map.navigationRows)
                if(s.Length!=map.width)throw new FormatException("Studio collision grid broken");
            }
            var cam=Camera.main;
            if(cam==null)throw new InvalidOperationException("Studio camera missing");
            cam.orthographic=true;
            cam.backgroundColor=new Color(.12f,.19f,.20f);
            cam.transform.position=new Vector3(0,0,-10);
            if(ChapterEnvironment!=null)ChapterEnvironment.Build();
            if(ChapterEnvironment==null){
            Render(Ground,"Parquet floor / modular", -1000);
            Render(Architecture,"Room shell and windows", -500);
            Render(Props,"Desks / computers / research notes", 120);
            }
            var go=new GameObject(ActorLabel,typeof(SpriteRenderer));
            go.transform.SetParent(transform,false);
            actor=go.transform;actor.position=ChapterEnvironment!=null?(Vector3)ChapterEnvironment.Spawn:Cell(map.spawn.x,map.spawn.y);
            if(ChapterEnvironment!=null)StudentFrames=ChapterEnvironment.Frames;
            var art=new GameObject("Animated Student",typeof(SpriteRenderer));
            art.transform.SetParent(actor,false);
            art.transform.localPosition=ChapterEnvironment!=null?Vector3.zero:new Vector3(0,1.35f,0);
            if(ChapterEnvironment!=null)art.AddComponent<CiviChapterDepth>();
            sprite=art.GetComponent<SpriteRenderer>();
            sprite.sprite=StudentFrames[0];
            go.GetComponent<SpriteRenderer>().enabled=false;sprite.sortingOrder=750;
            if(ChapterEnvironment!=null)
            {
                var shadow=new GameObject("Contact shadow",typeof(SpriteRenderer));shadow.transform.SetParent(actor,false);
                shadow.transform.localScale=new Vector3(.9f,.5f,1);var sr=shadow.GetComponent<SpriteRenderer>();
                sr.sprite=ChapterEnvironment.ContactShadow;sr.color=new Color(.1f,.2f,.15f,.26f);sr.sortingOrder=-450;
            }
            actorBody=go.AddComponent<Rigidbody2D>();
            actorBody.bodyType=RigidbodyType2D.Kinematic;
            actorBody.interpolation=RigidbodyInterpolation2D.Interpolate;
            go.AddComponent<CircleCollider2D>().radius=.28f;
            cameraFollow=cam.GetComponent<CiviFollowCamera2D>();
            if(cameraFollow==null)cameraFollow=cam.gameObject.AddComponent<CiviFollowCamera2D>();
            cameraFollow.MinZoom=6.5f;cameraFollow.MaxZoom=9.4f;
            cameraFollow.SmoothTime=.22f;
            cameraFollow.Configure(actor,new Rect(-16,-11,32,22),7.65f);
            if(ChapterEnvironment!=null){cameraFollow.MinZoom=8.4f;cameraFollow.MaxZoom=8.4f;cameraFollow.VerticalComposition=2.5f;cameraFollow.Configure(actor,ChapterEnvironment.Bounds,8.4f);}
            pixel=new Texture2D(1,1,TextureFormat.RGBA32,false);
            pixel.SetPixel(0,0,Color.white);pixel.Apply();
            ready=true;
        }
        void Render(Sprite s,string name,int sorting)
        {
            var go=new GameObject(name,typeof(SpriteRenderer));
            go.transform.SetParent(transform,false);
            var sr=go.GetComponent<SpriteRenderer>();
            sr.sprite=s;sr.sortingOrder=sorting;
        }
        void FixedUpdate()
        {
            if(!ready||info||(ChapterMode&&ExternalLock))return;
            var p=actorBody.position;
            var delta=direction*4.25f*Time.fixedDeltaTime;
            var x=new Vector2(p.x+delta.x,p.y);
            if(Walkable(x))p=x;
            var y=new Vector2(p.x,p.y+delta.y);
            if(Walkable(y))p=y;
            actorBody.MovePosition(p);
        }
        void Update()
        {
            if(!ready||Keyboard.current==null)return;
            if(ChapterMode&&ExternalLock){direction=Vector2.zero; if(sprite!=null&&ChapterEnvironment!=null&&ChapterEnvironment.WalkFramesSix!=null) sprite.sprite=StudentFrames[facing*4];return;}
            var k=Keyboard.current;
            if(!ChapterMode&&(k.escapeKey.wasPressedThisFrame||k.eKey.wasPressedThisFrame))
            {
                if(info){info=false;return;}
                if(Vector2.Distance(actor.position,Cell(map.exit.x,map.exit.y))<2.0f)
                {
                    CiviSchoolV2Bridge.Returning=true;
                    SceneManager.LoadScene(CiviSchoolV2Bridge.CampusScene);
                    return;
                }
                if(k.eKey.wasPressedThisFrame)
                {
                    info=true;direction=Vector2.zero;return;
                }
            }
            if(info)return;
            direction=Vector2.zero;
            if(k.aKey.isPressed||k.leftArrowKey.isPressed)direction.x--;
            if(k.dKey.isPressed||k.rightArrowKey.isPressed)direction.x++;
            if(k.wKey.isPressed||k.upArrowKey.isPressed)direction.y++;
            if(k.sKey.isPressed||k.downArrowKey.isPressed)direction.y--;
            direction=Vector2.ClampMagnitude(direction,1);
            if(direction.sqrMagnitude>.01f)
                facing=Mathf.Abs(direction.x)>Mathf.Abs(direction.y)?
                    (direction.x>0?3:2):(direction.y>0?1:0);
            timer+=Time.deltaTime;
            if(ChapterEnvironment!=null&&ChapterEnvironment.WalkFramesSix!=null&&ChapterEnvironment.WalkFramesSix.Length==24)
                sprite.sprite=direction.sqrMagnitude>.01f?
                    ChapterEnvironment.WalkFramesSix[facing*6+((int)(timer*9f)%6)]:
                    StudentFrames[facing*4];
            else
                sprite.sprite=StudentFrames[facing*4+
                    (direction.sqrMagnitude>.01f?(ChapterEnvironment!=null?((int)(timer*8f)%4):1+((int)(timer*7f)%3)):0)];
        }
        void OnGUI()
        {
            if(!ready||pixel==null||ChapterMode)return;
            if(label==null)
            {
                heading=new GUIStyle(GUI.skin.label){fontSize=36,fontStyle=FontStyle.Bold};
                heading.normal.textColor=Color.white;
                label=new GUIStyle(GUI.skin.label){fontSize=25,wordWrap=true};
                label.normal.textColor=new Color(.9f,.95f,.88f);
                smaller=new GUIStyle(label){fontSize=19};
            }
            GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/1600f,
                Screen.height/900f,1));
            GUI.color=new Color(.06f,.16f,.18f,.92f);
            GUI.DrawTexture(new Rect(0,0,1600,114),pixel);
            GUI.DrawTexture(new Rect(0,812,1600,88),pixel);
            GUI.color=Color.white;
            GUI.Label(new Rect(31,12,1420,50),"PHÒNG THIẾT KẾ  /  CIVI",heading);
            GUI.Label(new Rect(34,69,1450,30),
                "MỘT SCENE NỘI THẤT RIÊNG  ·  CAMERA DI CHUYỂN TRONG PHÒNG",smaller);
            var near=Vector2.Distance(actor.position,Cell(map.exit.x,map.exit.y))<2;
            GUI.Label(new Rect(27,826,1520,40),near?
                "E  /  RA HÀNH LANG  ·  CỬA ĐỂ CHUYỂN SCENE":
                "WASD / MŨI TÊN  ĐI LẠI    ·    E  XEM PHÒNG",label);
            if(info)
            {
                GUI.color=new Color(.06f,.16f,.18f,.97f);
                GUI.DrawTexture(new Rect(200,270,1200,350),pixel);
                GUI.color=Color.white;
                GUI.Label(new Rect(246,319,1090,66),"PHÒNG CÂU LẠC BỘ",heading);
                GUI.Label(new Rect(250,416,1070,106),
                    "Không gian dành cho thiết kế, thảo luận và kiểm tra ảnh. "+
                    "Đạo cụ và bố cục sẽ được liên kết với nhiệm vụ chính khi nhóm chốt kịch bản học đường.",label);
                GUI.Label(new Rect(249,561,1030,48),"E / ESC  —  QUAY LẠI",smaller);
            }
        }
    }
}
