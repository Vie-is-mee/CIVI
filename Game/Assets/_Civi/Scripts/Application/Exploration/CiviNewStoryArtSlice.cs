using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Civi.Preview
{
    // Art-led exploratory preview: new novel's locations only. Not a replacement for
    // the yearbook StorySession and not a claim to implement the 10-chapter plot.
    public sealed class CiviNewStoryArtSlice : MonoBehaviour
    {
        public Sprite[] Backgrounds=new Sprite[4]; // RiaMua, Storm, Filter19, LoiSang
        public Sprite[] Foregrounds=new Sprite[4];
        public Sprite[] AvatarViews=new Sprite[3]; // front/back/side

        const float WorldWidth=18f;
        const float WorldHeight=10.13f;
        const float InteractRange=1.45f;
        readonly List<PointOfInterest> points=new List<PointOfInterest>();
        SpriteRenderer worldRenderer,frontRenderer,playerRenderer;
        Rigidbody2D playerPhysics;
        Civi2DPlayer player;
        CiviFollowCamera2D follow;
        CiviNewStoryPreviewAvatar avatar;
        int place=0;
        bool storm;
        bool panel;
        PointOfInterest focused,selected;
        GUIStyle h1,small,copy,button,label;
        Texture2D uiTexture;
        Vector2[][] polygons;
        float markerPulse;
        string[] names={"RÌA MƯA / KHU DÂN CƯ","RÌA MƯA / CẢNH BÁO BÃO",
            "TRẠM LỌC SỐ 19","LÕI SÁNG / QUẢNG TRƯỜNG"};
        string[] chapters={"Chương 2 · Khu dân cư và quầy sửa ô",
            "Chương 8–9 · Khủng hoảng và bão","Chương 2, 8–9 · Hạ tầng lọc",
            "Chương 1, 4, 8, 10 · Trung tâm Lưu Quang"};

        sealed class PointOfInterest
        {
            public string Name,Note;
            public Vector2 Position;
            public GameObject Marker;
        }
        static readonly Color Midnight=new Color(.048f,.097f,.112f);
        static readonly Color Gold=new Color(.98f,.73f,.42f);
        static readonly Color Turquoise=new Color(.42f,.85f,.83f);
        public int CurrentPlace {get {return place;}}
        public int ActiveInteractions {get {return points.Count;}}
        public Transform PlayerTransform {get {return player!=null?player.transform:null;}}
        public Camera MainCamera {get {return Camera.main;}}
        public bool HasForeground {get {return frontRenderer!=null&&frontRenderer.sprite!=null;}}

        void Start()
        {
            try { Boot(); }
            catch(Exception e) {Debug.LogException(e);enabled=false;}
        }
        public void Boot()
        {
            if(Backgrounds==null||Backgrounds.Length!=4||
               Foregrounds==null||Foregrounds.Length!=4||
               AvatarViews==null||AvatarViews.Length!=3)
                throw new InvalidOperationException("CIVI V2 art references missing. Run CIVI > New Story > Rebuild Art Preview.");
            for(int i=0;i<4;i++)
                if(Backgrounds[i]==null||Foregrounds[i]==null)
                    throw new InvalidOperationException("CIVI V2 background/foreground missing "+i);
            foreach(var sprite in AvatarViews)
                if(sprite==null)throw new InvalidOperationException("CIVI V2 playable placeholder missing");

            var camera=Camera.main;
            if(camera==null)throw new InvalidOperationException("No camera");
            camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=Midnight;
            camera.transform.position=new Vector3(0,0,-10);

            var bg=new GameObject("Full-frame authored art",typeof(SpriteRenderer));
            bg.transform.SetParent(transform,false);
            worldRenderer=bg.GetComponent<SpriteRenderer>();
            worldRenderer.sortingOrder=-10000;
            var fg=new GameObject("Foreground mask / authored occlusion",typeof(SpriteRenderer));
            fg.transform.SetParent(transform,false);
            frontRenderer=fg.GetComponent<SpriteRenderer>();frontRenderer.sortingOrder=9000;
            SpriteRenderer r;
            var actor=new GameObject("MAI / Temporary playable stand-in",typeof(SpriteRenderer));
            actor.transform.SetParent(transform,false);
            actor.transform.position=new Vector3(-3,-2,-.1f);
            r=actor.GetComponent<SpriteRenderer>();r.color=Color.white;
            playerRenderer=r;
            var group=actor.AddComponent<SortingGroup>();
            var depth=actor.AddComponent<CiviDepthSort2D>();
            depth.FeetOffset=-.45f;
            var body=actor.AddComponent<Rigidbody2D>();
            body.gravityScale=0;body.freezeRotation=true;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            body.interpolation=RigidbodyInterpolation2D.Interpolate;
            var collision=actor.AddComponent<CircleCollider2D>();
            collision.radius=.35f;collision.offset=new Vector2(0,-.18f);
            playerPhysics=body;
            player=actor.AddComponent<Civi2DPlayer>();
            player.Speed=3.25f;
            avatar=actor.AddComponent<CiviNewStoryPreviewAvatar>();
            avatar.Front=AvatarViews[0];avatar.Back=AvatarViews[1];avatar.Side=AvatarViews[2];
            avatar.Player=player;avatar.Visual=r;
            depth.ApplyNow();

            follow=camera.GetComponent<CiviFollowCamera2D>();
            if(follow==null)follow=camera.gameObject.AddComponent<CiviFollowCamera2D>();
            // At ~16:9, show a comfortably sized section and clamp to entire painting.
            follow.MinZoom=3.18f;follow.MaxZoom=4.80f;
            follow.SmoothTime=.26f;
            follow.Configure(actor.transform,new Rect(-WorldWidth*.5f,-WorldHeight*.5f,
                WorldWidth,WorldHeight),3.95f);
            MakeUITexture();
            SetLocation(0);
        }

        void MakeUITexture()
        {
            uiTexture=new Texture2D(1,1,TextureFormat.RGBA32,false);
            uiTexture.SetPixel(0,0,Color.white);uiTexture.Apply();
        }

        static GameObject Marker(Transform parent,Vector2 location,Color color)
        {
            var holder=new GameObject("Glimmer / interaction");
            holder.transform.SetParent(parent,false);
            holder.transform.localPosition=location;
            var node=new GameObject("Diamond",typeof(SpriteRenderer));
            node.transform.SetParent(holder.transform,false);
            node.transform.localPosition=new Vector3(0,.16f,-.05f);
            node.transform.localRotation=Quaternion.Euler(0,0,45);
            node.transform.localScale=new Vector3(.17f,.17f,1);
            var spr=node.GetComponent<SpriteRenderer>();
            spr.sprite=CiviQuarterWorld2D.Pixel;
            spr.sortingOrder=3000;spr.color=color;
            return holder;
        }
        void AddPoint(string n,string note,float x,float y,Color color)
        {
            points.Add(new PointOfInterest{Name=n,Note=note,
                Position=new Vector2(x,y),Marker=Marker(transform,new Vector2(x,y),color)});
        }

        public void SetLocation(int next)
        {
            if(next<0||next>3)return;
            // Storm and normal share location and walk geometry, not duplicated story facts.
            place=next;
            storm=(place==1);
            if(worldRenderer==null)return;
            worldRenderer.sprite=Backgrounds[place];
            frontRenderer.sprite=Foregrounds[place];
            var bg=worldRenderer.gameObject.transform;
            var fg=frontRenderer.gameObject.transform;
            bg.localPosition=Vector3.zero;fg.localPosition=Vector3.zero;
            // Use a single shared world footprint across all four paintings.
            float px=WorldWidth/worldRenderer.sprite.bounds.size.x;
            bg.localScale=new Vector3(px,px,1);
            float fscale=WorldWidth/frontRenderer.sprite.bounds.size.x;
            fg.localScale=new Vector3(fscale,fscale,1);
            foreach(var item in points)if(item.Marker!=null)DestroyImmediateSafe(item.Marker);
            points.Clear();focused=selected=null;panel=false;

            if(place<=1)
            {
                // The courtyard and bridge are distinct walkable regions in the painting.
                AddPoint("Quầy sửa ô","Chi tiết ở Rìa Mưa: người dân sửa ô và đồ dùng thay vì phụ thuộc hoàn toàn vào nguồn cung.",-5.65f,.45f,Gold);
                AddPoint("Đường ống làm mát","Nước ngưng từ hệ thống làm mát; hình ảnh không được hiểu đơn giản là khu này luôn có mưa tự nhiên.",.6f,1.4f,Turquoise);
                AddPoint("Cầu tạm","Một tuyến đi do cộng đồng tự gia cố, kết nối khu dân cư với hạ tầng công nghiệp.",2.5f,-.25f,Gold);
                AddPoint("Hướng Trạm lọc 19","Chuyển sang bản thử môi trường Trạm lọc 19. Đây chưa phải nhiệm vụ điều tra đã chốt.",5.2f,.4f,Turquoise);
                player.Teleport(new Vector2(-3.0f,-1.1f));
            }
            else if(place==2)
            {
                AddPoint("Hệ thống lọc khí","Thiết bị lọc và đường ống tạo thành một phần hạ tầng sống còn của thành phố.",-1.4f,.85f,Turquoise);
                AddPoint("Bảng bảo trì","Ghi chú kỹ thuật chưa đồng nghĩa đã xác nhận nguyên nhân một sự cố.",-5.5f,-.6f,Gold);
                AddPoint("Van áp lực","Một điểm quan sát tương tác, chưa mô phỏng điều khiển thiết bị thật.",3f,-.5f,Turquoise);
                player.Teleport(new Vector2(-1.75f,-2.85f));
            }
            else
            {
                AddPoint("Hệ thống tàu treo","Tuyến giao thông kết nối các khu dưới mái vòm Lưu Quang.",3.3f,1.32f,Turquoise);
                AddPoint("Bảng phân khu","Đường ranh hành chính và việc tiếp cận dịch vụ sẽ được làm rõ khi chuyển thể các chương.",-2.1f,.5f,Gold);
                AddPoint("Khu vực trung tâm","Không gian hiện đại, trật tự và có hệ thống giám sát nguồn lực.",.2f,-2.05f,Turquoise);
                player.Teleport(new Vector2(-2.7f,-1.65f));
            }
            if(follow!=null)follow.Snap();
        }
        static void DestroyImmediateSafe(GameObject go)
        {
            if(Application.isPlaying)UnityEngine.Object.Destroy(go);
            else UnityEngine.Object.DestroyImmediate(go);
        }

        bool Inside(Vector2 p)
        {
            if(place<=1)
            {
                // High-priority playable courtyard with a side approach to the bridge.
                bool courtyard=(p.x>-7.4f&&p.x<1.25f&&p.y>-1.85f&&p.y<1.35f);
                bool bridge=(p.x>1.0f&&p.x<6.35f&&p.y>-.90f&&p.y<1.20f);
                return courtyard||bridge;
            }
            if(place==2)return p.x>-6.0f&&p.x<5.2f&&p.y>-3.7f&&p.y<1.30f;
            return p.x>-5.55f&&p.x<5.65f&&p.y>-3.25f&&p.y<1.30f;
        }
        void FixedUpdate()
        {
            if(playerPhysics==null)return;
            Vector2 p=playerPhysics.position;
            if(Inside(p))return;
            // Camera-friendly collision against manually authored navigation regions.
            // Constrains feet to the intended floor, rather than permitting roof walks.
            Vector2 target;
            if(place<=1)
            {
                Vector2 a=new Vector2(Mathf.Clamp(p.x,-7.39f,1.24f),
                    Mathf.Clamp(p.y,-1.84f,1.34f));
                Vector2 b=new Vector2(Mathf.Clamp(p.x,1.01f,6.34f),
                    Mathf.Clamp(p.y,-.89f,1.19f));
                target=(p-a).sqrMagnitude<(p-b).sqrMagnitude?a:b;
            }
            else target=place==2?
                new Vector2(Mathf.Clamp(p.x,-5.99f,5.19f),Mathf.Clamp(p.y,-3.69f,1.29f)):
                new Vector2(Mathf.Clamp(p.x,-5.54f,5.64f),Mathf.Clamp(p.y,-3.24f,1.29f));
            playerPhysics.position=target;playerPhysics.linearVelocity=Vector2.zero;
        }
        void Update()
        {
            if(player==null)return;
            var k=Keyboard.current;
            if(k==null)return;
            if(k.digit1Key.wasPressedThisFrame)SetLocation(0);
            if(k.digit2Key.wasPressedThisFrame)SetLocation(2);
            if(k.digit3Key.wasPressedThisFrame)SetLocation(3);
            if(k.qKey.wasPressedThisFrame && place<=1)SetLocation(place==0?1:0);
            if(panel)
            {
                player.AllowMovement=false;
                if(k.escapeKey.wasPressedThisFrame||k.eKey.wasPressedThisFrame){panel=false;selected=null;}
                return;
            }
            player.AllowMovement=true;focused=null;
            float d=InteractRange;
            foreach(var item in points)
            {
                var distance=Vector2.Distance(player.transform.position,item.Position);
                if(distance<d){focused=item;d=distance;}
                if(item.Marker!=null)
                {
                    var diamond=item.Marker.transform.Find("Diamond");
                    if(diamond!=null)diamond.localScale=Vector3.one*(item == focused ? .23f : .17f);
                }
            }
            if(k.eKey.wasPressedThisFrame && focused!=null){selected=focused;panel=true;}
        }
        void Skin()
        {
            if(h1!=null)return;
            h1=new GUIStyle(GUI.skin.label){fontSize=37,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft};
            h1.normal.textColor=Color.white;
            small=new GUIStyle(GUI.skin.label){fontSize=25,wordWrap=true};
            small.normal.textColor=new Color(.82f,.91f,.89f);
            copy=new GUIStyle(GUI.skin.label){fontSize=31,wordWrap=true,alignment=TextAnchor.MiddleLeft};
            copy.normal.textColor=Color.white;
            label=new GUIStyle(GUI.skin.label){fontSize=27,fontStyle=FontStyle.Bold};
            label.normal.textColor=Gold;
            button=new GUIStyle(GUI.skin.button){fontSize=27,fontStyle=FontStyle.Bold,
                alignment=TextAnchor.MiddleCenter};
            button.normal.textColor=Color.white;
            button.normal.background=uiTexture;
        }
        void Fill(Rect rect,Color color)
        {
            GUI.color=color;GUI.DrawTexture(rect,uiTexture);GUI.color=Color.white;
        }
        void OnGUI()
        {
            if(uiTexture==null)return;
            Skin();
            float sx=Screen.width/1600f, sy=Screen.height/900f;
            GUI.matrix=Matrix4x4.Scale(new Vector3(sx,sy,1));
            Fill(new Rect(0,0,1600,118),new Color(.044f,.105f,.13f,.94f));
            GUI.Label(new Rect(37,12,1200,54),names[place],h1);
            GUI.Label(new Rect(39,68,1350,34),chapters[place]+"  •  ENVIRONMENT ART PLAYTEST",small);
            Fill(new Rect(0,790,1600,110),new Color(.04f,.09f,.12f,.93f));
            GUI.Label(new Rect(34,798,1480,44),focused==null?
                "WASD / ↑↓←→  Di chuyển     E  Tương tác     Con lăn  Zoom":("E  Xem: "+focused.Name),copy);
            GUI.Label(new Rect(37,852,1470,36),"1  Rìa Mưa  /  Q  Bão      2  Trạm lọc 19      3  Lõi Sáng      Esc  Đóng bảng",small);
            if(panel&&selected!=null)
            {
                Fill(new Rect(260,275,1080,360),new Color(.04f,.11f,.14f,.96f));
                Fill(new Rect(260,275,1080,9),Gold);
                GUI.Label(new Rect(306,305,972,59),selected.Name,h1);
                GUI.Label(new Rect(306,394,960,138),selected.Note,copy);
                GUI.Label(new Rect(306,573,960,40),"E / Esc  —  Quay lại khám phá",small);
            }
        }
    }
    public sealed class CiviNewStoryPreviewAvatar : MonoBehaviour
    {
        public Sprite Front,Back,Side;
        public SpriteRenderer Visual;
        public Civi2DPlayer Player;
        Vector2 facing=Vector2.down;
        float initialY;
        void Awake(){initialY=transform.localPosition.y;}
        void Update()
        {
            if(Visual==null||Player==null)return;
            var dir=Player.MoveDirection;
            if(dir.sqrMagnitude>.04f)facing=dir;
            if(Mathf.Abs(facing.x)>Mathf.Abs(facing.y))Visual.sprite=Side;
            else Visual.sprite=facing.y>0?Back:Front;
            Visual.flipX=facing.x<-.01f;
            // No movement root animation: only sprite-facing changes, physics stays authoritative.
        }
    }
}
