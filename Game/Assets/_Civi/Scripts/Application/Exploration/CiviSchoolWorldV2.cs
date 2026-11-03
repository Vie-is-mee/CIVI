using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Civi.Preview
{
    public sealed class CiviSchoolWorldV2 : MonoBehaviour
    {
        [Serializable] sealed class SchoolSave
        {
            public string version;
            public float x,y;
            public string[] explored;
        }
        public TextAsset MapJson;
        public Sprite Ground,Architecture,Props,Foreground;
        public Sprite[] WalkFrames=new Sprite[16];
        public bool ChapterMode;
        public bool ExternalLock;

        CiviSchoolMapData map;
        Transform playerTransform;
        Rigidbody2D body;
        SpriteRenderer playerSprite;
        CiviFollowCamera2D follow;
        readonly Dictionary<string,Transform> markers=new Dictionary<string,Transform>();
        readonly HashSet<string> explored=new HashSet<string>(StringComparer.Ordinal);
        Vector2 motion=Vector2.zero;
        CiviSchoolHotspot focused;
        bool panel,help;
        string toast="";
        float toastExpiry;
        GUIStyle large,medium,small,accent,mini,button;
        Texture2D pixel;
        int lastDirection=0;
        bool booted;

        public bool IsBooted {get{return booted;}}
        public int HotspotCount {get{return map==null?0:map.hotspots.Length;}}
        public int ExploredCount {get{return explored.Count;}}
        public Transform Player {get{return playerTransform;}}
        public int Width {get{return map==null?0:map.width;}}
        public int Height {get{return map==null?0:map.height;}}
        public string[] VisitedIds {get{return explored.OrderBy(x=>x).ToArray();}}
        string SavePath {get{return Path.Combine(Application.persistentDataPath,"civi-school-world-v2.json");}}

        void Start()
        {
            if(booted)return;
            try{Boot();}
            catch(Exception e){Debug.LogException(e);enabled=false;}
        }
        public void Boot()
        {
            if(booted)return;
            if(MapJson==null)throw new InvalidOperationException("School map data missing");
            map=JsonUtility.FromJson<CiviSchoolMapData>(MapJson.text);
            if(map==null||map.width!=152||map.height!=60||
               map.navigationRows==null||map.navigationRows.Length!=map.height||
               map.hotspots==null||map.hotspots.Length!=6)
                throw new FormatException("School navigation schema mismatch");
            for(int y=0;y<map.height;y++)
                if(map.navigationRows[y]==null||map.navigationRows[y].Length!=map.width)
                    throw new FormatException("Malformed school navigation row "+y);
            if(new Sprite[]{Ground,Architecture,Props,Foreground}.Any(s=>s==null) || WalkFrames==null || WalkFrames.Length!=16 || WalkFrames.Any(s=>s==null))
                throw new InvalidOperationException("School sprite pack missing");
            if(map.hotspots.Any(h=>!CellWalkable(h.x,h.y)))
                throw new InvalidOperationException("School hotspot unreachable tile");
            var cam=Camera.main;
            if(cam==null)throw new InvalidOperationException("Camera not tagged MainCamera");
            cam.orthographic=true;
            cam.clearFlags=CameraClearFlags.SolidColor;
            cam.backgroundColor=new Color(.10f,.17f,.19f);
            cam.transform.position=new Vector3(0,0,-10);
            SpawnCanvas("Ground / seamless modular tile layout",Ground,-1200);
            SpawnCanvas("Architecture / cutaway interior walls",Architecture,-100);
            SpawnCanvas("Props / school desks and vegetation",Props,120);
            SpawnCanvas("Foreground / perimeter and gate",Foreground,800);
            var obj=new GameObject("An / school-world playable avatar",typeof(SpriteRenderer));
            obj.transform.SetParent(transform,false);
            obj.transform.position=ChapterMode?CellToWorld(50,27):CellToWorld(map.spawn.x,map.spawn.y);
            playerTransform=obj.transform;
            var visual=new GameObject("Animated Student",typeof(SpriteRenderer));
            visual.transform.SetParent(playerTransform,false);
            visual.transform.localPosition=new Vector3(0,1.35f,0);
            playerSprite=visual.GetComponent<SpriteRenderer>();
            playerSprite.sprite=WalkFrames[0];
            obj.GetComponent<SpriteRenderer>().enabled=false;
            playerSprite.sortingOrder=500;
            var physics=obj.AddComponent<Rigidbody2D>();
            physics.bodyType=RigidbodyType2D.Kinematic;
            physics.interpolation=RigidbodyInterpolation2D.Interpolate;
            physics.freezeRotation=true;
            body=physics;
            obj.AddComponent<CircleCollider2D>().radius=.25f;
            var shadow=new GameObject("Feet shadow",typeof(SpriteRenderer));
            shadow.transform.SetParent(playerTransform,false);
            shadow.transform.localPosition=new Vector3(0,-.40f,.01f);
            shadow.transform.localScale=new Vector3(.70f,.20f,1);
            var s=shadow.GetComponent<SpriteRenderer>();
            s.sprite=CiviQuarterWorld2D.Pixel;
            s.color=new Color(.1f,.20f,.15f,.38f);s.sortingOrder=-1;

            follow=cam.GetComponent<CiviFollowCamera2D>();
            if(follow==null)follow=cam.gameObject.AddComponent<CiviFollowCamera2D>();
            follow.MinZoom=8.25f;follow.MaxZoom=13.0f;
            follow.SmoothTime=.28f;follow.LookAheadDistance=0;
            follow.VerticalComposition=.15f;
            follow.Configure(playerTransform,new Rect(-map.width*.5f,-map.height*.5f,
                map.width,map.height),10.45f);
            foreach(var hotspot in map.hotspots)
            {
                var marker=new GameObject("Hotspot / "+hotspot.id);
                marker.transform.SetParent(transform,false);
                marker.transform.position=CellToWorld(hotspot.x,hotspot.y);
                var objectIcon=new GameObject("Diamond",typeof(SpriteRenderer));
                objectIcon.transform.SetParent(marker.transform,false);
                objectIcon.transform.localPosition=new Vector3(0,.42f,0);
                objectIcon.transform.localScale=new Vector3(.23f,.23f,1);
                objectIcon.transform.localRotation=Quaternion.Euler(0,0,45f);
                var renderer=objectIcon.GetComponent<SpriteRenderer>();
                renderer.sprite=CiviQuarterWorld2D.Pixel;
                renderer.color=new Color(.98f,.78f,.36f);
                renderer.sortingOrder=1100;
                markers.Add(hotspot.id,marker.transform);
            }
            pixel=new Texture2D(1,1,TextureFormat.RGBA32,false);
            pixel.SetPixel(0,0,Color.white);pixel.Apply();
            booted=true;
            if(CiviSchoolV2Bridge.Returning)
            {
                explored.Clear();
                foreach(var id in CiviSchoolV2Bridge.Explored??Array.Empty<string>())
                    if(markers.ContainsKey(id))explored.Add(id);
                playerTransform.position=CiviSchoolV2Bridge.ReturnPosition;
                body.position=CiviSchoolV2Bridge.ReturnPosition;
                follow.Snap();
                CiviSchoolV2Bridge.Returning=false;
            }
        }
        void SpawnCanvas(string label,Sprite source,int order)
        {
            var go=new GameObject(label,typeof(SpriteRenderer));
            go.transform.SetParent(transform,false);
            go.transform.localPosition=Vector3.zero;
            var renderer=go.GetComponent<SpriteRenderer>();
            renderer.sprite=source;
            renderer.sortingOrder=order;
        }
        Vector3 CellToWorld(int x,int y)
        {
            return new Vector3(x+.5f-map.width*.5f,y+.5f-map.height*.5f,0);
        }
        bool CellWalkable(int x,int y)
        {
            return x>=0&&x<map.width&&y>=0&&y<map.height&&map.navigationRows[y][x]=='1';
        }
        public bool IsWalkable(Vector2 p)
        {
            // Feet are radius .22 world units; do not let the avatar cut corners.
            foreach(float dx in new[]{-.22f,.22f})
            foreach(float dy in new[]{-.20f,.20f})
            {
                int tx=Mathf.FloorToInt(p.x+dx+map.width*.5f);
                int ty=Mathf.FloorToInt(p.y+dy+map.height*.5f);
                if(!CellWalkable(tx,ty))return false;
            }
            return true;
        }
        void FixedUpdate()
        {
            if(!booted||panel||help||(ChapterMode&&ExternalLock))return;
            Vector2 current=body.position;
            Vector2 step=motion*6.4f*Time.fixedDeltaTime;
            var next=new Vector2(current.x+step.x,current.y);
            if(IsWalkable(next))current=next;
            next=new Vector2(current.x,current.y+step.y);
            if(IsWalkable(next))current=next;
            body.MovePosition(current);
        }
        void Update()
        {
            if(!booted||Keyboard.current==null)return;
            var key=Keyboard.current;
            if(ChapterMode&&ExternalLock){motion=Vector2.zero;return;}
            if(!ChapterMode&&key.f5Key.wasPressedThisFrame)Save();
            if(!ChapterMode&&key.f9Key.wasPressedThisFrame)Load();
            if(!ChapterMode&&key.hKey.wasPressedThisFrame){help=!help;panel=false;}
            if((panel||help)&&key.escapeKey.wasPressedThisFrame){panel=false;help=false;return;}
            if(panel)
            {
                if(key.eKey.wasPressedThisFrame)panel=false;
                return;
            }
            if(help)return;
            motion=Vector2.zero;
            if(key.aKey.isPressed||key.leftArrowKey.isPressed)motion.x--;
            if(key.dKey.isPressed||key.rightArrowKey.isPressed)motion.x++;
            if(key.wKey.isPressed||key.upArrowKey.isPressed)motion.y++;
            if(key.sKey.isPressed||key.downArrowKey.isPressed)motion.y--;
            motion=Vector2.ClampMagnitude(motion,1f);
            if(motion.sqrMagnitude>.01f)
            {
                lastDirection=Mathf.Abs(motion.x)>Mathf.Abs(motion.y)?
                    (motion.x>0?3:2):(motion.y>0?1:0);
            }
            playerSprite.sprite=WalkFrames[lastDirection*4+(motion.sqrMagnitude>.01f?1+((int)(Time.time*7f)%3):0)];
            if(ChapterMode)return;
            bool nearDoor=Vector2.Distance(playerTransform.position,CellToWorld(119,39))<2.45f;
            if(nearDoor&&key.eKey.wasPressedThisFrame)
            {
                CiviSchoolV2Bridge.ReturnPosition=new Vector2(
                    playerTransform.position.x,playerTransform.position.y);
                CiviSchoolV2Bridge.Explored=VisitedIds;
                CiviSchoolV2Bridge.Returning=false;
                UnityEngine.SceneManagement.SceneManager.LoadScene(CiviSchoolV2Bridge.StudioScene);
                return;
            }
            focused=null;float closest=1.55f;
            foreach(var h in map.hotspots)
            {
                var target=markers[h.id];
                float delta=Vector2.Distance(playerTransform.position,target.position);
                if(delta<closest){closest=delta;focused=h;}
                var icon=target.Find("Diamond");
                if(icon!=null)icon.localScale=Vector3.one*(explored.Contains(h.id)?.20f:.28f);
            }
            if(key.eKey.wasPressedThisFrame&&focused!=null)
            {
                explored.Add(focused.id);panel=true;motion=Vector2.zero;
                toast="ĐÃ KHÁM PHÁ: "+focused.title;
                toastExpiry=Time.time+2.5f;
            }
        }
        void Save()
        {
            try
            {
                var state=new SchoolSave{version=map.worldVersion,
                    x=playerTransform.position.x,y=playerTransform.position.y,
                    explored=VisitedIds};
                var p=SavePath;
                File.WriteAllText(p,JsonUtility.ToJson(state));
                toast="ĐÃ LƯU";toastExpiry=Time.time+3;
            }catch(Exception e){Debug.LogError(e);toast="LỖI LƯU";}
        }
        void Load()
        {
            try
            {
                if(!File.Exists(SavePath)){toast="CHƯA CÓ FILE LƯU";toastExpiry=Time.time+3;return;}
                var loaded=JsonUtility.FromJson<SchoolSave>(File.ReadAllText(SavePath));
                if(loaded==null||loaded.version!=map.worldVersion||
                    loaded.explored==null||loaded.explored.Any(id=>!markers.ContainsKey(id))||
                    !IsWalkable(new Vector2(loaded.x,loaded.y)))
                    throw new FormatException("Save incompatible");
                explored.Clear();
                foreach(var id in loaded.explored)explored.Add(id);
                body.position=new Vector2(loaded.x,loaded.y);
                playerTransform.position=body.position;
                motion=Vector2.zero;panel=false;help=false;
                follow.Snap();
                toast="ĐÃ TẢI TIẾN ĐỘ";toastExpiry=Time.time+3;
            }catch(Exception e){Debug.LogError(e);toast="FILE LƯU KHÔNG HỢP LỆ";}
        }
        string Region()
        {
            float x=playerTransform.position.x+map.width*.5f;
            float y=playerTransform.position.y+map.height*.5f;
            if(y>40)
            {
                if(x<108)return "LỚP HỌC";
                if(x<138)return "PHÒNG THIẾT KẾ";
                return "THƯ VIỆN";
            }
            if(x<30)return "CỔNG TRƯỜNG / BÃI XE";
            if(x<74)return "SÂN TRƯỜNG";
            return "HÀNH LANG TẦNG 1";
        }
        void Skin()
        {
            if(large!=null)return;
            large=new GUIStyle(GUI.skin.label){fontSize=31,fontStyle=FontStyle.Bold,wordWrap=true};
            large.normal.textColor=Color.white;
            medium=new GUIStyle(GUI.skin.label){fontSize=24,wordWrap=true};
            medium.normal.textColor=Color.white;
            small=new GUIStyle(GUI.skin.label){fontSize=19,wordWrap=true};
            small.normal.textColor=new Color(.78f,.89f,.85f);
            accent=new GUIStyle(large){fontSize=26};
            accent.normal.textColor=new Color(.98f,.79f,.45f);
            mini=new GUIStyle(small){fontSize=15};
            button=new GUIStyle(GUI.skin.button){fontSize=24,alignment=TextAnchor.MiddleCenter};
        }
        void Fill(Rect rect,Color c)
        {
            GUI.color=c;GUI.DrawTexture(rect,pixel);GUI.color=Color.white;
        }
        void Minimap()
        {
            const float mx=1280,my=31,mw=280,mh=140;
            Fill(new Rect(mx,my,mw,mh),new Color(.07f,.18f,.18f,.94f));
            // Overview of one real connected campus; no per-screen montage.
            Fill(new Rect(mx+15,my+18,250,110),new Color(.30f,.47f,.32f));
            Fill(new Rect(mx+20,my+62,240,23),new Color(.71f,.70f,.67f));
            Fill(new Rect(mx+64,my+40,81,65),new Color(.71f,.70f,.67f));
            Fill(new Rect(mx+142,my+20,116,42),new Color(.78f,.76f,.65f));
            float vx=mx+15+(playerTransform.position.x+map.width*.5f)/map.width*250f;
            float vy=my+128-(playerTransform.position.y+map.height*.5f)/map.height*110f;
            Fill(new Rect(vx-4,vy-4,8,8),new Color(1,.58f,.23f));
        }
        void OnGUI()
        {
            if(!booted||pixel==null||ChapterMode)return;
            Skin();
            GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/1600f,Screen.height/900f,1));
            Fill(new Rect(0,0,1600,137),new Color(.05f,.13f,.14f,.88f));
            GUI.Label(new Rect(31,17,1200,48),"CIVI  /  KHUÔN VIÊN TRƯỜNG HỌC",large);
            GUI.Label(new Rect(34,69,950,36),Region()+"  ·  BẢN ĐỒ LIỀN MẠCH",medium);
            GUI.Label(new Rect(35,107,1100,28),"KHÁM PHÁ "+explored.Count+"/"+map.hotspots.Length+
                " ĐỊA ĐIỂM    •    ĐẾN PHÒNG THIẾT KẾ BẰNG CỬA",small);
            Minimap();
            Fill(new Rect(0,820,1600,80),new Color(.05f,.13f,.14f,.93f));
            GUI.Label(new Rect(30,826,1530,38),
                Vector2.Distance(playerTransform.position,CellToWorld(119,39))<2.45f?
                "E  /  MỞ CỬA VÀO PHÒNG THIẾT KẾ":
                focused==null?"WASD / MŨI TÊN  ĐI    ·    H  HƯỚNG DẪN    ·    CUỘN CHUỘT  ZOOM":
                "E  TƯƠNG TÁC: "+focused.title,medium);
            GUI.Label(new Rect(31,863,1450,26),
                "F5  LƯU    F9  TẢI    •    TẤT CẢ CÁC KHU NẰM TRONG CÙNG MỘT BẢN ĐỒ",small);
            if(Time.time<toastExpiry)GUI.Label(new Rect(520,200,730,40),toast,accent);
            if(panel&&focused!=null)
            {
                Fill(new Rect(220,246,1160,418),new Color(.065f,.16f,.18f,.97f));
                Fill(new Rect(220,246,1160,8),new Color(.96f,.76f,.42f));
                GUI.Label(new Rect(263,284,1070,72),focused.title,large);
                GUI.Label(new Rect(267,382,1050,150),focused.note,medium);
                GUI.Label(new Rect(265,587,1060,46),"NHẤN E HOẶC ESC ĐỂ QUAY LẠI",accent);
            }
            if(help)
            {
                Fill(new Rect(180,225,1230,430),new Color(.06f,.15f,.17f,.97f));
                GUI.Label(new Rect(225,266,1160,56),"ĐIỀU KHIỂN",large);
                GUI.Label(new Rect(230,343,1110,232),
                    "WASD / phím mũi tên: đi tự do qua khuôn viên\n"+
                    "E: tương tác khi đến gần dấu vàng\n"+
                    "Lăn chuột: zoom; camera bám theo nhân vật liên tục\n"+
                    "F5 / F9: lưu và tải; H / Esc: đóng hướng dẫn",medium);
                GUI.Label(new Rect(230,588,1120,42),"TRONG MỖI SCENE CAMERA ĐI CÙNG NHÂN VẬT",accent);
            }
        }
    }
}
