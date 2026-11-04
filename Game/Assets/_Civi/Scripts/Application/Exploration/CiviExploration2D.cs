using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using Civi.Runtime;

namespace Civi.Preview
{
    // The handoff keeps the verified S00 -> S01 -> S02 state in memory.
    // No user/AI-provided effect is ever applied outside StorySession.
    public static class StoryHandoffBridge
    {
        public static string Payload;
    }

    public sealed class Civi2DPlayer : MonoBehaviour
    {
        public bool AllowMovement = true;
        public float Speed = 3.7f;
        public Vector2 MoveDirection { get { return AllowMovement ? direction : Vector2.zero; } }
        Rigidbody2D body;
        SpriteRenderer sprite;
        Vector2 direction;
        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            sprite = GetComponent<SpriteRenderer>();
        }
        void Update()
        {
            direction = Vector2.zero;
            var k = Keyboard.current;
            if (k == null || !AllowMovement) return;
            if (k.aKey.isPressed || k.leftArrowKey.isPressed) direction.x -= 1;
            if (k.dKey.isPressed || k.rightArrowKey.isPressed) direction.x += 1;
            if (k.wKey.isPressed || k.upArrowKey.isPressed) direction.y += 1;
            if (k.sKey.isPressed || k.downArrowKey.isPressed) direction.y -= 1;
            direction = Vector2.ClampMagnitude(direction, 1f);
            if (direction.x != 0) sprite.flipX = direction.x < 0;
        }
        void FixedUpdate()
        {
            body.linearVelocity = AllowMovement ? direction * Speed : Vector2.zero;
        }
        public void Teleport(Vector2 point)
        {
            if(body==null)body=GetComponent<Rigidbody2D>();
            body.position = point;
            transform.position = point;
            body.linearVelocity = Vector2.zero;
        }
    }

    public sealed class Civi2DSpot : MonoBehaviour
    {
        public string SceneId;
        public string[] Choices;
        public string DisplayName;
        public SpriteRenderer Visual;
        public Color Original;
        public void Highlight(bool on)
        {
            if (Visual != null) Visual.color = on ? Color.Lerp(Original, Color.white, .42f) : Original;
        }
    }

    // P0 greybox. Replace world geometry with approved sprites/Tilemaps later.
    // UI and authoring data are not coupled to greybox art.
    public sealed class CiviExploration2D : MonoBehaviour
    {
        [Serializable]
        sealed class Snapshot
        {
            public string story;
            public float x, y;
            public string[] visitedIntros;
            public string[] investigationObserved;
            public string hypothesis;
            public bool workingHypothesis;
        }

        public CiviAssetCatalog catalog;
        readonly List<Civi2DSpot> spots = new List<Civi2DSpot>();
        readonly HashSet<string> viewedIntros = new HashSet<string>(StringComparer.Ordinal);
        readonly InvestigationNotebook notebook = new InvestigationNotebook();
        StorySession story;
        Civi2DPlayer player;
        Civi2DSpot nearest;
        CiviQuarterWorld2D roomVisual;
        CiviFollowCamera2D cameraRig;
        RectTransform canvasRoot, modal;
        Text sceneLabel, statusLabel, promptLabel, progressLabel;
        bool paused, menuActive, boardActive;
        string lastScene;
        string note = "";
        float noteUntil;

        string SavePath { get { return Path.Combine(Application.persistentDataPath, "civi-p0-exploration.json"); } }

        void Start()
        {
            try { Boot(); }
            catch (Exception ex) { Debug.LogException(ex); enabled = false; }
        }

        void Boot()
        {
            if (catalog == null || catalog.storyInitialState == null)
                throw new InvalidOperationException("CIVI catalog not prepared. Run CIVI > TV3 > Chuan bi ban tich hop S00.");
            story = new StorySession(new UnityStorySource(catalog), catalog.storyInitialState.text, "S02");
            if (!String.IsNullOrEmpty(StoryHandoffBridge.Payload))
            {
                string handoff = StoryHandoffBridge.Payload;
                StoryHandoffBridge.Payload = null;
                story.Restore(handoff);
            }

            var camera = Camera.main;
            if (camera == null) throw new InvalidOperationException("Camera missing from S02 exploration scene");
            camera.orthographic = true;
            camera.orthographicSize = 3.75f;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.backgroundColor = new Color(.10f, .18f, .20f);
            camera.clearFlags = CameraClearFlags.SolidColor;

            BuildWorld();
            SyncNotebookFromStory();
            cameraRig=camera.GetComponent<CiviFollowCamera2D>();
            if(cameraRig==null)cameraRig=camera.gameObject.AddComponent<CiviFollowCamera2D>();
            cameraRig.Configure(player.transform,new Rect(-9.25f,-5.72f,18.5f,11.45f),3.75f);
            BuildHud();
            SyncStage(true);
            if (story.CurrentLine != null) ShowNext();
            else if (story.StepId == "S03_REASON") ShowNext();
            SetNote("WASD: di chuyển  |  E: tương tác  |  B: bảng ảnh  |  F5/F9: lưu/tải");
        }

        static Sprite Square()
        {
            return Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
        }
        GameObject Block(string name, Vector2 pos, Vector2 size, Color color, int order, bool solid, Transform parent)
        {
            var node = new GameObject(name, typeof(SpriteRenderer));
            node.transform.SetParent(parent, false);
            node.transform.localPosition = new Vector3(pos.x, pos.y, 0);
            node.transform.localScale = new Vector3(size.x, size.y, 1);
            var sr = node.GetComponent<SpriteRenderer>();
            sr.sprite = Square(); sr.color = color; sr.sortingOrder = order;
            if (solid) node.AddComponent<BoxCollider2D>();
            return node;
        }
        void WorldText(Transform parent, string label, Vector2 pos, float width, float size, Color color)
        {
            var go = new GameObject("Label "+label, typeof(TextMesh));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(pos.x, pos.y, -0.1f);
            var t = go.GetComponent<TextMesh>();
            t.text = label; t.anchor = TextAnchor.MiddleCenter; t.alignment = TextAlignment.Center;
            t.font = catalog.uiFont; t.fontSize = 64; t.characterSize = size;
            t.color = color;
            var renderer=t.GetComponent<MeshRenderer>();renderer.sharedMaterial=catalog.uiFont.material;
            renderer.sortingOrder = 12;
            go.transform.localScale = Vector3.one;
        }
        Civi2DSpot Spot(string stage, string name, string[] choices, Vector2 pos, Color color, string caption)
        {
            var obj = Block(name, pos, new Vector2(1.1f, .78f), color, 7, false, transform);
            var mark = obj.AddComponent<Civi2DSpot>();
            mark.SceneId = stage; mark.Choices = choices;
            mark.DisplayName = name; mark.Visual = obj.GetComponent<SpriteRenderer>(); mark.Original = color;
            obj.AddComponent<UnityEngine.Rendering.SortingGroup>();
            obj.AddComponent<CiviDepthSort2D>().ApplyNow();
            var depthShadow=Block("Object shadow",pos+new Vector2(.08f,-.16f),new Vector2(1.18f,.73f),new Color(.14f,.25f,.24f,.26f),6,false,transform);
            depthShadow.transform.SetParent(obj.transform,true);
            var icon = Block("Object inset", pos + new Vector2(0, .05f), new Vector2(.63f, .35f),
                new Color(.96f, .94f, .82f), 8, false, transform);
            icon.transform.SetParent(obj.transform, true);
            WorldText(obj.transform, caption, new Vector2(0, -.33f), 1f, .0078f, new Color(.12f, .20f, .21f));
            spots.Add(mark);
            return mark;
        }
        void BuildWorld()
        {
            // Independent S02 and S03 3/4 environments. All logic stays in StorySession.
            var room = new GameObject("CIVI / 3-quarter explorable rooms");
            roomVisual = room.AddComponent<CiviQuarterWorld2D>();
            roomVisual.Build(transform);

            var an = Block("AN / playable character",new Vector2(-3.5f,-2.25f),
                new Vector2(.62f,.75f),new Color(.20f,.48f,.46f),11,false,transform);
            an.transform.localScale = new Vector3(.62f, .75f, 1);
            var collider = an.AddComponent<CircleCollider2D>(); collider.radius=.49f;
            var rb = an.AddComponent<Rigidbody2D>();
            rb.gravityScale=0; rb.freezeRotation=true; rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            rb.interpolation=RigidbodyInterpolation2D.Interpolate;
            player = an.AddComponent<Civi2DPlayer>();
            var avatar = an.AddComponent<CiviActorVisual2D>();
            avatar.Build();
            WorldText(an.transform,"AN",new Vector2(0,-.68f),1,.010f,new Color(.08f,.22f,.22f));
            var teal = new Color(.22f,.49f,.47f);
            var brown = new Color(.63f,.40f,.29f);
            var yellow = new Color(.76f,.61f,.28f);
            Spot("S02","Sơ đồ",new[]{"S02_DIAGRAM"},new Vector2(-4.55f,1.14f),teal,"DIAGRAM");
            Spot("S02","Chiếc kẹp",new[]{"S02_CLAMP"},new Vector2(-2.1f,.85f),yellow,"CLAMP");
            Spot("S02","Lịch hẹn",new[]{"S02_CLOCK"},new Vector2(.5f,1.12f),yellow,"SCHEDULE");
            Spot("S02","Chú thích",new[]{"S02_CAPTION"},new Vector2(3.1f,.65f),brown,"CAPTION");
            Spot("S02","Nhi",new[]{"S02_HELP","S02_ASK_CAMERA"},new Vector2(-.75f,-1.35f),teal,"NHI");
            Spot("S02","Cửa sang phòng ảnh",new[]{"S02_LEAVE"},new Vector2(6.3f,-1.3f),teal,"NEXT");
            Spot("S03","Ảnh 0441",new[]{"S03_LOOK_0441"},new Vector2(-5.1f,1.1f),yellow,"0441");
            Spot("S03","Ảnh 0442",new[]{"S03_LOOK_0442"},new Vector2(-2.8f,1.1f),yellow,"0442");
            Spot("S03","Ảnh 0443",new[]{"S03_LOOK_0443"},new Vector2(-.5f,1.1f),yellow,"0443");
            Spot("S03","Kẹp trống",new[]{"S03_EMPTY"},new Vector2(2.0f,1.1f),brown,"EMPTY");
            Spot("S03","Hạ",new[]{"S03_WHY"},new Vector2(4.6f,-1.25f),teal,"HA");
            Spot("S03","Nguồn video",new[]{"S03_SOURCE"},new Vector2(6.2f,1.15f),teal,"VIDEO");
        }

        void BuildHud()
        {
            var root = new GameObject("CIVI / Exploration UI",
                typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1600,900); scaler.matchWidthOrHeight=.5f;
            canvasRoot=NotebookUI.Group("HUD",root.transform,0,0,1600,900);
            canvasRoot.anchorMin=canvasRoot.anchorMax=new Vector2(.5f,.5f);
            canvasRoot.pivot=new Vector2(.5f,.5f);canvasRoot.anchoredPosition=Vector2.zero;
            NotebookUI.Box("Top bar",canvasRoot,0,0,1600,110,new Color(.09f,.22f,.24f,.95f));
            sceneLabel=NotebookUI.Label("Scene title",canvasRoot,"CIVI / EXPLORATION",55,12,1350,45,32,catalog.handFont,NotebookUI.Paper);
            progressLabel=NotebookUI.Label("Progress",canvasRoot,"",57,61,1320,35,20,catalog.uiFont,NotebookUI.Paper);
            NotebookUI.Box("Bottom bar",canvasRoot,0,800,1600,100,new Color(.08f,.20f,.21f,.94f));
            promptLabel=NotebookUI.Label("Prompt",canvasRoot,"",52,806,1450,40,26,catalog.handFont,NotebookUI.Paper);
            statusLabel=NotebookUI.Label("Status",canvasRoot,"WASD: Đi  /  E: Xem  /  B: Bảng ảnh  /  F5: Lưu  /  F9: Tải",55,848,1490,31,18,catalog.uiFont,NotebookUI.Paper);
            NotebookUI.Button("Save shortcut",canvasRoot,"LƯU",1392,23,153,49,catalog.uiFont,Save,true);
            modal=NotebookUI.Group("Dialog and evidence",canvasRoot,0,110,1600,690);
            modal.gameObject.SetActive(false);
            if (FindAnyObjectByType<EventSystem>()==null)
            {
                var ev=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
                ev.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
        }

        void SetNote(string message)
        {
            note=message; noteUntil=Time.unscaledTime+4f;
            if(statusLabel!=null)statusLabel.text=message;
        }
        void SyncStage(bool force=false)
        {
            if (!force && lastScene==story.SceneId) return;
            lastScene=story.SceneId;
            if(roomVisual!=null)roomVisual.SetStage(story.SceneId);
            SyncNotebookFromStory();
            if (story.SceneId=="S02" || story.SceneId=="S03")
                player.Teleport(new Vector2(-3.5f,-2.25f));
            if(cameraRig!=null)cameraRig.Snap();
            foreach(var item in spots) item.gameObject.SetActive(item.SceneId==story.SceneId);
            if(sceneLabel!=null)sceneLabel.text=story.SceneId=="S02"?"CIVI / GÓC KIỂM ĐỒ":story.SceneId=="S03"?"CIVI / BÀN ĐIỀU TRA":"CIVI / ĐÃ ĐẾN "+story.SceneId;
        }

        void Update()
        {
            if (story==null || player==null) return;
            var k=Keyboard.current;
            if (k==null)return;
            if (k.f5Key.wasPressedThisFrame)Save();
            if (k.f9Key.wasPressedThisFrame)Load();
            if (paused)
            {
                if(k.escapeKey.wasPressedThisFrame && (menuActive || boardActive)) CloseModal();
                else if(!menuActive && !boardActive &&
                        (k.enterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame)) NextLine();
                return;
            }
            if(Time.unscaledTime>noteUntil)statusLabel.text="WASD: đi  •  E: tương tác  •  B: bảng ảnh  •  F5: lưu  •  F9: tải";
            if(story.SceneId!="S02" && story.SceneId!="S03")return;
            nearest=null;
            var distance=1.42f;
            foreach(var spot in spots)
            {
                if(!spot.gameObject.activeSelf)continue;
                float current=Vector2.Distance(player.transform.position,spot.transform.position);
                if(current<distance){nearest=spot;distance=current;}
            }
            foreach(var spot in spots)if(spot.gameObject.activeSelf)spot.Highlight(spot==nearest);
            bool available = nearest!=null && story.CanChoose &&
                story.AvailableChoices.Any(c=>nearest.Choices.Contains(c.Id));
            promptLabel.text=nearest==null?"ĐI TÌM MANH MỐI  ·  LẠI GẦN ĐỒ VẬT ĐỂ TƯƠNG TÁC":
                available?"E  /  "+nearest.DisplayName:"ĐÃ KIỂM TRA / CHƯA ĐỦ THÔNG TIN: "+nearest.DisplayName;
            if(k.eKey.wasPressedThisFrame && nearest!=null)Interact(nearest);
            if(k.bKey.wasPressedThisFrame)ShowBoard();
        }

        void Interact(Civi2DSpot spot)
        {
            if(!story.CanChoose){SetNote("Đọc hết đoạn thoại trước khi tương tác.");return;}
            var eligible=story.AvailableChoices.Where(c=>spot.Choices.Contains(c.Id)).ToList();
            if(eligible.Count==0){SetNote("Đã kiểm tra hoặc chưa đủ thông tin cho hành động này.");return;}
            if(eligible.Count==1) BeginChoice(eligible[0].Id);
            else ShowMenu(eligible);
        }

        void BeginChoice(string id)
        {
            if (id.StartsWith("S03_LOOK_",StringComparison.Ordinal))
            {
                ShowPhoto("PHOTO_"+id.Substring("S03_LOOK_".Length),"QUAN SÁT ẢNH",()=>CommitChoice(id));
                return;
            }
            if (id=="S03_EMPTY")
            {
                ShowPhoto("PHOTO_CLAMP_EMPTY","ĐỐI CHIẾU CHI TIẾT",()=>CommitChoice(id));
                return;
            }
            CommitChoice(id);
        }
        void CommitChoice(string id)
        {
            try {story.Choose(id);SyncNotebookFromStory();menuActive=false;boardActive=false;ShowNext();}
            catch(Exception ex){Debug.LogException(ex);SetNote("Chưa thực hiện được: "+ex.Message);CloseModal();}
        }
        void ShowNext()
        {
            SyncStage();
            RefreshObjective();
            if (story.SceneId!="S02" && story.SceneId!="S03")
            {
                ShowBoundary();return;
            }
            if(story.CurrentLine!=null)
            {
                string entry=story.SceneId+"/"+story.StepId;
                if(story.LineIndex==0 && (story.CurrentLine.Id=="S02_L001" || story.CurrentLine.Id=="S03_L001"))
                {
                    if(viewedIntros.Contains(entry))
                    {
                        for(int i=0;i<12 && !story.CanChoose && story.CurrentLine!=null;i++)story.Advance();
                        if(story.CanChoose){CloseModal();return;}
                    }
                    else viewedIntros.Add(entry);
                }
                ShowDialogue();return;
            }
            if(story.StepId=="S03_REASON" && story.CanChoose)
            {
                ShowMenu(story.AvailableChoices);return;
            }
            CloseModal();
        }
        void SyncNotebookFromStory()
        {
            if(story==null) return;
            var seen=story.ReadState("evidence_seen") as List<object>;
            if(seen==null)return;
            foreach(object id in seen)
                if(id is string str && InvestigationNotebook.IsAllowed(str))notebook.Observe(str);
        }
        void RefreshObjective()
        {
            if(story==null||progressLabel==null)return;
            var all=story.ReadState("evidence_seen") as List<object>;
            if(all==null)return;
            string[] required=story.SceneId=="S02"?
                new[]{"PROP_CLAMP","PROP_SCHEDULE","PROP_CAPTION_OLD"}:
                new[]{"PHOTO_0441","PHOTO_0442","PHOTO_0443","PHOTO_CLAMP_EMPTY"};
            int count=story.SceneId=="S03" ? notebook.ObservedCount : required.Count(x=>all.Contains(x));
            progressLabel.text=(story.SceneId=="S02"?"KIỂM TRA ĐẠO CỤ":"ĐIỀU TRA ẢNH")+
                "  /  "+count+" trong "+required.Length+" nguồn  •  Camera bám theo An / lăn chuột để zoom";
        }
        void NextLine()
        {
            try{story.Advance();ShowNext();}
            catch(Exception ex){Debug.LogException(ex);SetNote("Lỗi bước hội thoại: "+ex.Message);}
        }

        void OpenModal(string heading)
        {
            paused=true;menuActive=false;boardActive=false;player.AllowMovement=false;
            NotebookUI.Clear(modal);
            modal.gameObject.SetActive(true);
            NotebookUI.Box("Panel",modal,125,40,1350,580,NotebookUI.Paper);
            NotebookUI.Box("Panel header",modal,125,40,1350,76,NotebookUI.Teal);
            NotebookUI.Label("Panel heading",modal,heading,166,62,1240,52,30,catalog.handFont,NotebookUI.Paper);
        }
        void CloseModal()
        {
            paused=false;menuActive=false;boardActive=false;player.AllowMovement=true;
            NotebookUI.Clear(modal);modal.gameObject.SetActive(false);
        }
        void ShowDialogue()
        {
            var line=story.CurrentLine;
            if(line==null){ShowNext();return;}
            OpenModal("HỘI THOẠI  /  "+line.Speaker);
            NotebookUI.Label("Dialogue",modal,line.Text,175,180,1210,290,33,catalog.diaryFont,NotebookUI.Ink);
            NotebookUI.Button("Continue",modal,"ĐỌC TIẾP  /  ENTER",930,520,490,70,catalog.uiFont,NextLine);
        }
        void ShowMenu(List<StoryChoice> options)
        {
            OpenModal("CHỌN CÁCH TƯƠNG TÁC");menuActive=true;
            int n=Mathf.Min(options.Count,5);
            for(int i=0;i<n;i++)
            {
                string id=options[i].Id;
                NotebookUI.Button("Choice "+id,modal,options[i].Label,177,160+i*75,1190,60,
                    catalog.uiFont,()=>BeginChoice(id));
            }
            NotebookUI.Button("Cancel",modal,"QUAY LẠI / ESC",990,558,377,50,catalog.uiFont,CloseModal,true);
        }
        static void FitImage(RawImage image, Texture2D texture, float x, float y, float w, float h)
        {
            if(texture==null)return;
            float scale=Mathf.Min(w/texture.width,h/texture.height);
            float rw=texture.width*scale, rh=texture.height*scale;
            NotebookUI.Place(image.rectTransform,x+(w-rw)*.5f,y+(h-rh)*.5f,rw,rh);
        }
        void ShowPhoto(string id,string title,Action confirm)
        {
            OpenModal(title+" / "+id);boardActive=true;
            CiviEvidenceWorkbenchUI.DrawInspector(modal,catalog,notebook,id,()=>
            {
                // Only StorySession can commit an observation; sync from authored effects.
                confirm();
            },CloseModal);
        }
        void ShowBoard()
        {
            if(story.SceneId!="S03"){SetNote("Bảng đối chiếu chỉ dùng tại S03.");return;}
            SyncNotebookFromStory();
            OpenModal("BẢNG CHỨNG CỨ / ĐỐI CHIẾU ẢNH");boardActive=true;
            CiviEvidenceWorkbenchUI.DrawBoard(modal,catalog,notebook,
                ()=>ShowComparison("PHOTO_0441","PHOTO_0442"),
                ()=>ShowComparison("PHOTO_0442","PHOTO_0443"),ShowDeduction,CloseModal);
        }
        void ShowComparison(string left,string right)
        {
            if(!notebook.Has(left)||!notebook.Has(right))
            {
                SetNote("Bạn cần quan sát cả hai ảnh trước khi so sánh.");return;
            }
            OpenModal("SO SÁNH ẢNH GỐC");boardActive=true;
            CiviEvidenceWorkbenchUI.DrawComparison(modal,catalog,left,right,ShowBoard,CloseModal);
        }
        void ShowDeduction()
        {
            OpenModal("THỬ GIẢ THUYẾT / KHÔNG TỰ SUY DIỄN");boardActive=true;
            CiviEvidenceWorkbenchUI.DrawDeduction(modal,catalog,notebook,ShowBoard);
        }
        void ShowBoundary()
        {
            OpenModal("VERTICAL SLICE / ĐIỂM DỪNG");
            NotebookUI.Label("Next stage",modal,"Đã chuyển sang "+story.SceneId+
                ". Story state và evidence được giữ lại.\nBản P2 dừng tại đây; hãy kiểm chứng giả thuyết bằng nguồn video S04 ở bản tiếp theo.",
                192,192,1140,220,29,catalog.diaryFont,NotebookUI.Ink);
            NotebookUI.Button("Save at boundary",modal,"LƯU TIẾN ĐỘ",190,505,535,65,catalog.uiFont,Save);
            NotebookUI.Button("Restart slice",modal,"CHƠI LẠI BẢN 2D",784,505,565,65,catalog.uiFont,
                ()=>SceneManager.LoadScene("S02-2D-VerticalSlice"),true);
        }
        void Save()
        {
            if(story==null||player==null)return;
            try
            {
                var state=new Snapshot{story=story.Save(),x=player.transform.position.x,y=player.transform.position.y,
                    visitedIntros=viewedIntros.OrderBy(x=>x).ToArray(),
                    investigationObserved=notebook.ObservedIds,hypothesis=notebook.LastHypothesis,
                    workingHypothesis=notebook.HasWorkingHypothesis};
                string json=JsonUtility.ToJson(state);
                string temp=SavePath+".tmp";File.WriteAllText(temp,json);
                if(File.Exists(SavePath))File.Replace(temp,SavePath,null);
                else File.Move(temp,SavePath);
                SetNote("Đã lưu trạng thái khám phá.");
            }
            catch(Exception ex){Debug.LogException(ex);SetNote("Lưu thất bại: "+ex.Message);}
        }
        void Load()
        {
            try
            {
                if(!File.Exists(SavePath)){SetNote("Chưa có save cho bản 2D.");return;}
                var data=JsonUtility.FromJson<Snapshot>(File.ReadAllText(SavePath));
                if(data==null||String.IsNullOrEmpty(data.story))throw new FormatException("Save thiếu story");
                // Validate the notebook before mutating the authoritative StorySession.
                var candidate=new InvestigationNotebook();
                candidate.Restore(data.investigationObserved,data.hypothesis,data.workingHypothesis);
                story.Restore(data.story);
                notebook.Restore(candidate.ObservedIds,candidate.LastHypothesis,candidate.HasWorkingHypothesis);
                SyncNotebookFromStory();
                viewedIntros.Clear();
                if(data.visitedIntros!=null)foreach(var entry in data.visitedIntros)viewedIntros.Add(entry);
                SyncStage(true);
                player.Teleport(new Vector2(data.x,data.y));
                ShowNext();
                SetNote("Đã tải tiến độ 2D.");
            }
            catch(Exception ex){Debug.LogException(ex);SetNote("Tải thất bại: "+ex.Message);}
        }
    }
}
