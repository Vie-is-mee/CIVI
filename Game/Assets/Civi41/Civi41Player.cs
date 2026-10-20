using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;
using UnityEngine.TextCore.LowLevel;

namespace Civi41
{
    public static class Bootstrap
    {
        public static bool Enabled
        {
            get
            {
                var args=Environment.GetCommandLineArgs();
                if(Array.IndexOf(args,"--civi-qa")>=0 || Array.IndexOf(args,"--civi-board-preview")>=0)return false;
                if(Application.productName=="CIVI Trang 17" || Array.IndexOf(args,"--civi41")>=0)return true;
                #if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool("CIVI41Preview",true);
                #else
                return false;
                #endif
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Start(){if(Enabled && UnityEngine.Object.FindAnyObjectByType<Player>()==null)new GameObject("CIVI / Trang 17 / UI v4.1").AddComponent<Player>();}
    }

    public class Player:MonoBehaviour
    {
        public Run State=new Run();
        public bool Testing {get;private set;}
        public bool Busy {get{return transition || capture;}}
        public bool Complete {get{return visible>=full.Length;}}
        string EffectiveMode {get{return State.line<Story.Lines(State).Length-1?"next":Story.Mode(State);}}
        public string SavePath {get{return Path.Combine(Testing?Path.Combine(Application.dataPath,"../QA"):Application.persistentDataPath,"civi-v41-bookmark.json");}}
        public string QAFolder {get{return Path.GetFullPath(Path.Combine(Application.dataPath,"../QA"));}}
        readonly Color paper=new Color(.982f,.966f,.920f), ink=new Color(.105f,.16f,.20f), teal=new Color(.13f,.32f,.32f), red=new Color(.65f,.28f,.23f), gray=new Color(.42f,.46f,.46f);
        TMP_FontAsset ui,diary,hand;
        Sprite rounded;
        Texture2D court,room,atlas;
        Sprite[] bodies=new Sprite[4], warm=new Sprite[4], concerned=new Sprite[4],neutral=new Sprite[4];
        Sprite[] photos=new Sprite[6];Sprite retakeThree;
        CanvasGroup actionAlpha;
        RectTransform root;
        RawImage backdrop;
        GameObject stage,dialogueBox,actions,cover,modal,shade,headPanel;
        CanvasGroup veil;
        TextMeshProUGUI spoken,speaker,chapter,place,nextLabel,toast,stamp;
        Image portrait;
        Button next;
        Actor[] actors=new Actor[4];
        string full="",scene="",recordKey="";
        int visible;
        float textClock,delay,lastInput=-10,toastUntil;
        bool transition,capture,reduceMotion,instant,muted,choosing;
        int evidenceIndex,videoFrame;
        int proofFocus=-1;
        AudioSource ambience,sfx;
        readonly List<Action> shortcuts=new List<Action>();
        readonly string[] names={"AN","NHI","HẠ","MINH"};

        void Awake()
        {
            Testing=Array.IndexOf(Environment.GetCommandLineArgs(),"--civi41-qa")>=0;
            Application.targetFrameRate=60;
            ui=MakeFont("CiviOpenFont");diary=MakeFont("CiviDiary");hand=MakeFont("CiviHand");
            diary.fallbackFontAssetTable=new List<TMP_FontAsset>{ui};hand.fallbackFontAssetTable=new List<TMP_FontAsset>{ui};
            court=Resources.Load<Texture2D>("Civi41/Courtyard");room=Resources.Load<Texture2D>("Civi41/Room");atlas=Resources.Load<Texture2D>("Civi41/Cast");
            if(court==null || room==null || atlas==null)throw new InvalidOperationException("Missing CIVI41 art resources.");
            for(int i=0;i<4;i++)bodies[i]=Sprite.Create(atlas,new Rect(atlas.width*i/4f,0,atlas.width/4f,atlas.height),new Vector2(.5f,0),100);
            var expressions=Resources.Load<Texture2D>("Civi41/Expressions");
            for(int i=0;i<4;i++)
            {
                warm[i]=Sprite.Create(expressions,new Rect(i*expressions.width/4f,expressions.height*.035f,expressions.width/8f,expressions.height*.22f),new Vector2(.5f,.5f),100);
                concerned[i]=Sprite.Create(expressions,new Rect((i*2+1)*expressions.width/8f,expressions.height*.035f,expressions.width/8f,expressions.height*.22f),new Vector2(.5f,.5f),100);
            }
            var faces=new[]{new Rect(180,50,151,176),new Rect(489,121,184,180),new Rect(851,60,164,183),new Rect(1251,12,155,181)};
            for(int i=0;i<4;i++){var f=faces[i];neutral[i]=Sprite.Create(expressions,new Rect(f.x/1536*expressions.width,expressions.height-(f.y+f.height)/1024*expressions.height,f.width/1536*expressions.width,f.height/1024*expressions.height),Vector2.one*.5f,100);}
            var photoAtlas=Resources.Load<Texture2D>("Civi41/Photos");var newThree=Resources.Load<Texture2D>("Civi41/RetakeThree");
            if(photoAtlas==null || newThree==null)throw new InvalidOperationException("Missing story photographs.");
            for(int i=0;i<6;i++)photos[i]=Sprite.Create(photoAtlas,new Rect(i%2*photoAtlas.width/2f,(2-i/2)*photoAtlas.height/3f,photoAtlas.width/2f,photoAtlas.height/3f),Vector2.one*.5f,100);
            retakeThree=Sprite.Create(newThree,new Rect(0,0,newThree.width,newThree.height),Vector2.one*.5f,100);
            rounded=Rounded();
            reduceMotion=PlayerPrefs.GetInt("CIVI41_ReduceMotion",0)==1;
            instant=PlayerPrefs.GetInt("CIVI41_Instant",0)==1;
            muted=PlayerPrefs.GetInt("CIVI41_Muted",0)==1;
            Build();Audio();ShowCover();
            if(Testing)StartCoroutine(RuntimeChecks());
        }
        TMP_FontAsset MakeFont(string resource)
        {
            Font f=Resources.Load<Font>(resource);if(f==null)throw new InvalidOperationException("Missing font: "+resource);
            var result=TMP_FontAsset.CreateFontAsset(f,90,4,GlyphRenderMode.SDFAA,2048,2048);
            result.atlasPopulationMode=AtlasPopulationMode.Dynamic;result.isMultiAtlasTexturesEnabled=true;return result;
        }
        void Build()
        {
            if(Camera.main==null){var camera=new GameObject("CIVI Paper Camera",typeof(Camera)).GetComponent<Camera>();camera.tag="MainCamera";camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ink;camera.cullingMask=0;}
            var co=new GameObject("CIVI41 Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));co.transform.SetParent(transform,false);
            co.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;co.GetComponent<Canvas>().sortingOrder=250;
            var scale=co.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1280,720);scale.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            var bg=co.AddComponent<Image>();bg.color=ink;bg.raycastTarget=false;
            root=Group("1280 stage",co.transform).GetComponent<RectTransform>();root.anchorMin=root.anchorMax=root.pivot=Vector2.one*.5f;root.sizeDelta=new Vector2(1280,720);root.anchoredPosition=Vector2.zero;
            if(EventSystem.current==null)
            {
                var ev=new GameObject("CIVI41 Input",typeof(EventSystem),typeof(InputSystemUIInputModule));
                var module=ev.GetComponent<InputSystemUIInputModule>();module.AssignDefaultActions();module.submit=null;module.cancel=null;module.move=null;
            }
            var existingModule=EventSystem.current==null?null:EventSystem.current.GetComponent<InputSystemUIInputModule>();
            if(existingModule!=null){existingModule.submit=null;existingModule.cancel=null;existingModule.move=null;}
            backdrop=Picture("Place",root,court,0,0,1280,720);
            Box("Atmosphere",root,0,0,1280,720,new Color(.94f,.93f,.86f,.13f));
            stage=Group("Persistent actors",root);
            for(int i=0;i<4;i++)
            {
                var im=SpriteImage("Actor "+names[i],stage.transform,bodies[i],68+278*i,102,304,575);
                actors[i]=im.gameObject.AddComponent<Actor>();actors[i].Setup(im,i);
            }
            Box("Top paper",root,0,0,1280,85,new Color(.982f,.966f,.920f,.96f));
            Label("Brand",root,"CIVI",32,14,111,43,32,ink,hand);
            Box("Header divider",root,160,18,1,42,new Color(.22f,.36f,.36f,.35f));
            chapter=Label("Chapter",root,"",182,16,650,24,18,teal,ui);
            place=Label("Place",root,"",183,46,660,21,12,gray,ui);
            Btn("Kỷ yếu  / J",root,916,23,153,39,()=>Notebook());
            Btn("Tạm dừng  / Esc",root,1082,23,170,39,()=>Pause());
            dialogueBox=Group("Dialogue",root);
            Box("Paper shadow",dialogueBox.transform,35,482,1212,185,new Color(.12f,.18f,.20f,.25f));
            Box("Dialogue paper",dialogueBox.transform,28,475,1224,185,paper);
            Box("Left margin",dialogueBox.transform,183,498,1,125,new Color(.67f,.35f,.28f,.28f));
            headPanel=Group("Expression portrait",dialogueBox.transform);
            Box("Portrait paper",headPanel.transform,48,488,116,132,Color.white);
            portrait=SpriteImage("Persistent expression",headPanel.transform,warm[0],48,488,116,132);
            speaker=Label("Speaker",dialogueBox.transform,"",207,492,795,27,20,teal,hand);
            spoken=Label("Spoken line",dialogueBox.transform,"",207,530,994,83,27,ink,diary);
            spoken.lineSpacing=3;spoken.richText=false;
            next=Btn("Tiếp tục  →",dialogueBox.transform,1007,615,213,32,()=>Continue());nextLabel=next.GetComponentInChildren<TextMeshProUGUI>();
            Label("Dialogue margin",dialogueBox.transform,"12A4 / NHỮNG ĐIỀU CÒN CHƯA NÓI",207,628,740,18,10,gray,ui);
            actions=Group("Actions",root);actionAlpha=actions.AddComponent<CanvasGroup>();
            Box("Footer",root,0,681,1280,39,new Color(.982f,.966f,.920f,.96f));
            stamp=Label("Hint",root,"Space: hiện hết câu / đọc tiếp · 1–3: chọn · J: kỷ yếu",32,691,885,19,12,gray,ui);
            Btn("Đặt dấu trang",root,1081,686,169,28,()=>Save(false),true);
            toast=Label("Feedback",root,"",35,653,1100,25,14,ink,ui);toast.gameObject.SetActive(false);
            var v=Group("Scene transition",root);veil=v.AddComponent<CanvasGroup>();
            Box("Transition paper",v.transform,0,0,1280,720,paper);veil.alpha=0;veil.blocksRaycasts=false;
        }
        void Audio()
        {
            ambience=gameObject.AddComponent<AudioSource>();ambience.clip=Resources.Load<AudioClip>("CiviAmbience");ambience.loop=true;ambience.volume=muted?0:.075f;if(!Testing)ambience.Play();
            sfx=gameObject.AddComponent<AudioSource>();sfx.volume=.23f;sfx.mute=muted;
        }
        void Click(){var c=Resources.Load<AudioClip>("CiviClick");if(c!=null && !Testing)sfx.PlayOneShot(c,.35f);}
        void Notify(string message){toast.text=message;toast.gameObject.SetActive(true);toast.transform.SetAsLastSibling();toastUntil=Time.unscaledTime+4.5f;}
        void Update()
        {
            if(toast!=null && toast.gameObject.activeSelf && Time.unscaledTime>toastUntil)toast.gameObject.SetActive(false);
            bool blocked=cover!=null || modal!=null || Busy;
            actionAlpha.alpha=reduceMotion?1:Mathf.MoveTowards(actionAlpha.alpha,1,Time.unscaledDeltaTime*8);
            for(int i=0;i<actors.Length;i++)actors[i].Paused=blocked || reduceMotion;
            if(!blocked && !Complete)
            {
                textClock+=Time.unscaledDeltaTime;
                if(delay>0)delay-=Time.unscaledDeltaTime;
                else while(textClock>=.026f && visible<full.Length)
                {
                    textClock-=.026f;visible++;char c=full[visible-1];if(c=='.'||c=='?'||c=='!'||c=='…'){delay=.19f;break;}if(c==','){delay=.08f;break;}
                }
                spoken.maxVisibleCharacters=visible;
                if(Complete)ReadingFinished();
            }
            var k=Keyboard.current;if(k==null || Testing)return;
            if(k.escapeKey.wasPressedThisFrame){if(modal!=null)CloseModal();else if(cover==null)Pause();return;}
            if(blocked)return;
            if(k.jKey.wasPressedThisFrame){Notebook();return;}
            if(k.hKey.wasPressedThisFrame){History();return;}
            if(k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame)Continue();
            for(int i=0;i<Math.Min(3,shortcuts.Count);i++)if((i==0?k.digit1Key:i==1?k.digit2Key:k.digit3Key).wasPressedThisFrame && Complete && choosing)shortcuts[i]();
        }
        public void Reveal(){visible=full.Length;spoken.maxVisibleCharacters=visible;ReadingFinished();}
        void ReadingFinished()
        {
            string mode=EffectiveMode;nextLabel.text=mode=="next"?"Tiếp tục  →":mode=="end"?"Trang đã khép":"Chọn hành động  →";
            next.interactable=mode!="end";
            actions.SetActive(choosing && mode!="next" && mode!="end");
        }
        public void Continue()
        {
            if(Busy || cover!=null || modal!=null || (!Testing && Time.unscaledTime-lastInput<.14f))return;
            lastInput=Time.unscaledTime;
            if(!Complete){Reveal();return;}
            if(choosing && EffectiveMode!="next" && EffectiveMode!="end")return;
            if(EffectiveMode!="next" && EffectiveMode!="end"){choosing=true;dialogueBox.SetActive(false);actions.SetActive(true);actionAlpha.alpha=reduceMotion?1:0;return;}
            if(Story.Advance(State))Present();
        }
        public void Act(string action)
        {
            if(Busy || cover!=null || modal!=null || !Complete)return;
            if(Story.Act(State,action))Present();
        }
        string SceneKey(){return State.node==0?"proof":State.node<3?"opening":State.node==5?"equipment":State.node<15?"room":State.node==18 && State.line>=2?"later":State.node>=20?"page":"court";}
        public void Present(bool immediate=false)
        {
            if(State.line>=Story.Lines(State).Length)State.line=Story.Lines(State).Length-1;
            string target=SceneKey();
            if(!immediate && scene!="" && scene!=target && !reduceMotion && !Testing){StartCoroutine(Transition(target));return;}
            scene=target;RenderBeat();
        }
        IEnumerator Transition(string target)
        {
            transition=true;veil.transform.SetAsLastSibling();veil.blocksRaycasts=true;
            for(float t=0;t<.22f;t+=Time.unscaledDeltaTime){veil.alpha=Mathf.SmoothStep(0,1,t/.22f);yield return null;}
            veil.alpha=1;scene=target;RenderBeat();yield return new WaitForSecondsRealtime(.12f);
            for(float t=0;t<.36f;t+=Time.unscaledDeltaTime){veil.alpha=1-Mathf.SmoothStep(0,1,t/.36f);yield return null;}
            veil.alpha=0;veil.blocksRaycasts=false;transition=false;
        }
        void RenderBeat()
        {
            Clear(actions.transform);shortcuts.Clear();actions.SetActive(false);choosing=false;
            chapter.text=Story.Chapter(State);place.text=Story.Place(State);backdrop.texture=scene=="room" || scene=="proof"?room:court;
            backdrop.color=scene=="opening"?new Color(1,.98f,.91f):Color.white;
            stage.SetActive(State.node>0 && State.node<20);dialogueBox.SetActive(true);
            bool laterNow=State.node==18 && State.line>=2 || State.node==19 && State.photoCount==2;
            bool aloneWithNhi=laterNow || State.node==5;
            bool absent=State.node>=3 && State.node<=14 && State.node!=5 || State.node>=15 && State.node<=19 && !laterNow && State.invite!="accepted";
            for(int i=0;i<4;i++)
            {
                actors[i].gameObject.SetActive(!(i==1 && absent) && !(aloneWithNhi && (i==2 || i==3)));
                actors[i].Focus=false;
                actors[i].SetStageX(aloneWithNhi?(i==0?286:650):absent?(i==0?130:i==2?470:810):68+278*i);
            }
            var beat=Story.Lines(State)[State.line];int actor=Array.IndexOf(names,beat.who);
            headPanel.SetActive(actor>=0);speaker.text=beat.who;
            dialogueBox.transform.Find("Left margin").gameObject.SetActive(actor>=0);
            if(actor<0)speaker.text="BÊN LỀ TRANG SỔ";
            if(actor==1 && absent && (State.node==13 || State.node==18 || State.node==19) || State.node==19 && laterNow && (actor==2 || actor==3))speaker.text=beat.who+" / TIN NHẮN";
            if(actor>=0){portrait.sprite=beat.mood=="concerned"?concerned[actor]:beat.mood=="warm"?warm[actor]:neutral[actor];actors[actor].Focus=true;actors[actor].Cue(State.node+":"+State.line);}
            float left=actor>=0?207:64;
            Place(spoken.rectTransform,left,530,1211-left,83);Place(speaker.rectTransform,left,492,1150-left,34);
            full=beat.who==""?beat.text:"“"+beat.text+"”";spoken.text=full;visible=instant || Testing?full.Length:0;textClock=0;delay=.10f;spoken.maxVisibleCharacters=visible;
            next.interactable=true;nextLabel.text=Complete?"Tiếp tục  →":"Hiện hết câu  →";
            string key=State.node+":"+State.line;
            if(recordKey!=key){State.history.Add((beat.who==""?"Bên lề":beat.who)+": "+beat.text);recordKey=key;}
            BuildActions();if(Complete)ReadingFinished();
            if(State.node==22){dialogueBox.SetActive(false);OpenFinal();}
            if(State.node==0)Proof();
            Canvas.ForceUpdateCanvases();
        }
        void BuildActions()
        {
            string mode=EffectiveMode;
            if(mode=="hook")ActionCard("TRANG 17 / BẢN IN THỬ","Hạ giữ ngón tay ở trang trắng. File ảnh vẫn còn.","Mở bản in trên bàn",Proof);
            if(mode=="gesture")Choices("Trước khi vào khung ảnh…",new[]{"Siết lại kẹp điện thoại","Chỉnh bảng HẸN cho ngay ngắn","Trêu Minh để cả nhóm bật cười"},new[]{"Cái kẹp đang tuột.","Chữ HẸN hơi nghiêng.","Minh cứ nhìn đi chỗ khác."},new Action[]{()=>Act("gesture:0"),()=>Act("gesture:1"),()=>Act("gesture:2")});
            if(mode=="opening")ActionCard("BỐN NGƯỜI / MƯỜI GIÂY","Đặt hẹn giờ rồi vào cùng mọi người.","Đặt máy và vào khung",()=>StartCoroutine(Capture("capture_opening",4)));
            if(mode=="investigate")Choices("Bắt đầu từ điều mình chưa hỏi.",new[]{"Hỏi Nhi ai đã cầm máy","Mở thư mục ảnh hội chợ"},new[]{"Nghe người đã ở đó kể lại.","So ba tấm ảnh, rồi nhìn lại chín giây."},new Action[]{()=>Act("ask_nhi"),()=>Act("photos")});
            if(mode=="evidence")ActionCard("THƯ MỤC HỘI CHỢ","0441 · 0442 · 0443 · Chín giây trước đó","Mở các tư liệu",()=>Evidence());
            if(mode=="draft")ActionCard("BÀN KỶ YẾU / TRANG 17","Đổi bố cục và xem trước. Bản này chưa gửi cho ai.","Dựng trang nháp",()=>Draft());
            if(mode=="ownership")Choices("Con trỏ vẫn nằm trên nút xóa.",new[]{"Tao đã đưa máy cho Nhi. Để tao nói với nó.","Mày sửa câu chú thích trước đi. Ảnh để tính sau."},new[]{"",""},new Action[]{()=>Act("own"),()=>Act("deflect")});
            if(mode=="negotiate")Choices("Tấm 0443 vẫn nằm giữa hai đứa.",new[]{"Giữ tấm đó. Phần chữ tao viết lại.","Dùng ảnh khác. Tao tự làm trang mới.","Để nguyên đã. Tao hỏi Nhi trước."},new[]{"","",""},new Action[]{()=>Act("negotiate:keep"),()=>Act("negotiate:replace"),()=>Act("negotiate:ask")});
            if(mode=="invite")Choices("Ô nhắn tin đã mở. Bạn muốn hỏi điều gì?",new[]{"Mai 16:45 vào chụp với bọn tao nhé? Tao lo máy.","Mai mày có rảnh cầm máy giúp bọn tao không?","Tao giữ tấm buổi tập trong kỷ yếu được không?"},new[]{"","",""},new Action[]{()=>Act("invite:accepted"),()=>Act("invite:declined"),()=>Act("invite:not_invited")});
            if(mode=="shoot")
            {
                bool present=State.invite=="accepted";
                Choices(present?"Nhi nhìn đồng hồ. Bạn vẫn đang đứng cạnh máy.":"Chiếc ghế thứ tư đang vướng lối đi.",new[]{present?"Đặt hẹn giờ rồi chạy vào khung":"Cất chiếc ghế, chụp ba người","Cất máy. Hôm nay không chụp nữa.","Nhắn Nhi, hỏi một ngày khác."},new[]{"","",""},new Action[]{()=>StartCoroutine(Capture("capture",present?4:3)),()=>Act("keep_old"),()=>Act("propose_later")});
            }
            if(mode=="later")ActionCard("LỜI HẸN MỚI / ĐÃ NHẬN LỜI","An và Nhi gặp lúc 16:30. Hạ và Minh có việc riêng.","Đặt máy, chụp tấm hai người",()=>StartCoroutine(Capture("capture_later",2)));
            if(mode=="publish")ActionCard("TRANG 17","Tôi xóa chữ “final” khỏi tên file.","Mở trang vừa làm xong",()=>FinalPreview());
        }
        void Choices(string title,string[] labels,string[] notes,Action[] callback)
        {
            Box("Choice shadow",actions.transform,35,454,1212,210,new Color(.10f,.17f,.19f,.25f));
            Box("Choices paper",actions.transform,28,448,1224,212,paper);
            Label("Choice title",actions.transform,title,54,462,1172,28,21,teal,hand);
            float w=(1172-(labels.Length-1)*14)/labels.Length;
            for(int i=0;i<labels.Length;i++)
            {
                int j=i;float x=54+i*(w+14);
                var b=Btn((i+1)+" / "+labels[i],actions.transform,x,509,w,77,()=>callback[j](),true);var t=b.GetComponentInChildren<TextMeshProUGUI>();t.fontSize=21;t.alignment=TextAlignmentOptions.TopLeft;
                Label("Choice note",actions.transform,notes[i],x+3,599,w-6,45,14,gray,ui);
                shortcuts.Add(callback[j]);
            }
        }
        void ActionCard(string heading,string detail,string button,Action run)
        {
            Box("Action paper",actions.transform,260,483,760,177,paper);
            Label("Action heading",actions.transform,heading,286,499,708,24,13,teal,ui);
            Label("Action detail",actions.transform,detail,286,531,708,47,22,ink,hand);
            Btn(button,actions.transform,286,600,708,40,run);shortcuts.Add(run);
        }
        IEnumerator Capture(string action,int count)
        {
            if(Busy || !Complete)yield break;capture=true;actions.SetActive(false);
            var overlay=Group("Camera countdown",root);
            Box("Camera top",overlay.transform,435,115,410,77,paper);
            var num=Label("Countdown",overlay.transform,"",453,125,374,56,32,teal,hand);num.alignment=TextAlignmentOptions.Center;
            for(int i=3;i>0;i--){num.text=i+"… vào khung nhé";if(!Testing)yield return new WaitForSecondsRealtime(.68f);}
            if(!muted && !Testing)sfx.PlayOneShot(Resources.Load<AudioClip>("CiviShutter"),.65f);
            var flash=Box("Shutter",overlay.transform,0,0,1280,720,Color.white);flash.raycastTarget=false;
            for(float t=0;t<.25f;t+=Time.unscaledDeltaTime){flash.color=new Color(1,1,1,1-t/.25f);yield return null;}
            Destroy(overlay);capture=false;Act(action);
        }
        void BeginModal(string name)
        {
            CloseModal();modal=Group(name,root);
            Box("Modal shade",modal.transform,0,0,1280,720,new Color(.05f,.12f,.15f,.62f)).raycastTarget=true;
        }
        public void CloseModal(){if(modal!=null){modal.SetActive(false);Destroy(modal);modal=null;}}
        void InspectProof(int which){State.hookClues|=1<<which;proofFocus=which;Proof();}
        void Proof()
        {
            BeginModal("Proof on the desk");Box("Proof paper",modal.transform,42,48,1196,618,paper);
            Label("Proof heading",modal.transform,"“Khoan gửi. Trang 17 đâu?”",74,69,1045,51,32,ink,hand);
            Btn("Gấp lại ×",modal.transform,1094,78,113,33,CloseModal,true);
            Photo(modal.transform,75,139,647,393,3,false,"0443 / ẢNH HẠ ĐÃ CHỌN");
            Label("Proof instruction",modal.transform,"Chạm vào những thứ đang nằm trên bàn.",758,140,440,44,21,ink,diary);
            Btn("Tấm ảnh 0443",modal.transform,756,203,446,47,()=>InspectProof(0),proofFocus!=0);
            Btn("Tờ nháp có bốn cái tên",modal.transform,756,266,446,47,()=>InspectProof(1),proofFocus!=1);
            Btn("Tin nhắn: “Ừ, bỏ đi. Mai tính.”",modal.transform,756,329,446,47,()=>InspectProof(2),proofFocus!=2);
            string reaction=proofFocus==0?"Hạ: “Tao chọn tấm này. Mày bảo bỏ cái gì thì nói rõ đi.”":proofFocus==1?"An · Nhi · Hạ · Minh.\n\nMinh: “Nhi ghi từ buổi tập. Mày không nhớ à?”":proofFocus==2?"Minh: “Tao rút trang đó rồi. Mày nhắn mỗi thế này, tao hiểu là bỏ trang.”":"Ảnh có ba người.\nTờ nháp ghi bốn cái tên.\nTrang in thử thì trắng.";
            Label("Proof response",modal.transform,reaction,758,407,442,142,24,ink,diary);
            Label("Proof margin",modal.transform,"16     17     18\nTrang ở giữa đã được rút khỏi bản thử.",90,555,615,65,20,gray,hand);
            var go=Btn("Hôm đó mình đã hẹn chuyện gì?  →",modal.transform,756,580,446,49,()=>{CloseModal();Reveal();Act("hook_done");});go.interactable=State.hookClues!=0;
        }
        public void Evidence()
        {
            BeginModal("Photo desk");Box("Evidence sheet",modal.transform,51,69,1178,581,paper);
            Label("Evidence title",modal.transform,"CHÍN GIÂY NGOÀI ẢNH",77,90,865,39,30,ink,hand);
            Btn("Đóng  ×",modal.transform,1102,91,99,33,CloseModal,true);
            string[] titles={"0441 / đèn chưa sáng","0442 / đèn đã sáng","0443 / ba người","9 giây / trước 0443"};
            for(int i=0;i<4;i++){int j=i;Btn(titles[i],modal.transform,78+i*282,147,268,37,()=>{evidenceIndex=j;videoFrame=0;Evidence();},evidenceIndex!=i);}
            if(evidenceIndex<3)
            {
                Photo(modal.transform,80,207,666,391,evidenceIndex==2?3:4,false,evidenceIndex==2?"0443":evidenceIndex==1?"0442":"0441");
                if(evidenceIndex==2)State.saw0443=true;
            }
            else
            {
                int[] frames={0,1,2};
                if(videoFrame==2)Photo(modal.transform,80,207,666,346,3,false,"0443");
                else
                {
                    Box("Video storyboard",modal.transform,80,207,666,346,Color.white);
                    Label("Storyboard heading",modal.transform,videoFrame==0?"AN ĐƯA ĐIỆN THOẠI CHO NHI":"NHI NHẮC ĐẾN KẸP HẸN GIỜ",103,219,618,29,17,teal,ui);
                    SpriteImage("An in memory",modal.transform,bodies[0],126,252,145,255);
                    SpriteImage("Nhi in memory",modal.transform,bodies[1],542,252,145,255);
                    Box("Remembered phone",modal.transform,videoFrame==0?362:519,323,27,47,ink);
                    Box("Remembered phone glass",modal.transform,videoFrame==0?366:523,328,19,34,paper);
                    Label("Handoff arrow",modal.transform,videoFrame==0?"→":"←",302,320,194,62,48,teal,ui).alignment=TextAlignmentOptions.Center;
                    Label("Storyboard caption",modal.transform,"KHUNG DỰNG THỬ "+(videoFrame+1)+" / 3",105,516,610,24,15,gray,hand);
                }
                for(int i=0;i<3;i++){int j=i;Btn("Khung "+(i+1),modal.transform,90+i*214,555,204,32,()=>{videoFrame=j;if(j==2)State.sawVideo=true;DrawEvidenceFrame();},videoFrame!=i);}
            }
            string detail=evidenceIndex==0?"Đèn chưa sáng. Nhi ở mép ảnh, tay vẫn giữ cuộn dây.\n\nMinh chưa cất xong chồng giấy trên bàn.":evidenceIndex==1?"Đèn đã sáng. Nhi vẫn ở cạnh quầy.\n\nMấy đứa còn đang làm, chưa ai nhìn vào máy.":evidenceIndex==2?"An, Hạ, Minh.\n\nTôi phóng to, kéo ra mép ảnh. Như thể nhìn kỹ hơn thì sẽ thấy Nhi.\n\nVẫn chỉ có ba đứa.":VideoWords();
            Label("Evidence details",modal.transform,detail,785,216,400,219,24,ink,diary);
            Label("Evidence source",modal.transform,evidenceIndex==3?"Nguồn: đoạn của Vy, trình bày bằng ba khung dựng thử. Xem để đối chiếu không có nghĩa được đăng.":"Nguồn: thư mục ảnh hội chợ. Mỗi tấm giữ một khoảnh khắc khác nhau.",785,449,400,70,14,gray,ui);
            bool enough=State.saw0443 && (State.askedNhi || State.sawVideo);
            var done=Btn(enough?"Trở lại với Hạ  →":"Xem 0443 và làm rõ ai cầm máy",modal.transform,785,550,402,53,()=>{CloseModal();Act("evidence_done");});done.interactable=enough;
            Label("Reading note",modal.transform,"Bạn đang xem tư liệu. Thời gian trong câu chuyện không trôi.",80,612,1040,19,12,gray,ui);
        }
        void DrawEvidenceFrame()
        {
            int preserve=videoFrame;Evidence(); // Rebuild only the open desk, never the persistent stage.
            videoFrame=preserve;
        }
        string VideoWords(){return videoFrame==0?"An đưa điện thoại cho Nhi.\n\n“Chụp giúp cái. Một tấm thôi.”":videoFrame==1?"Nhi chỉ về phía kẹp máy.\n\nAn: “Lát chụp đủ. Nhanh lên, có người tới rồi.”":"Khách bước vào.\n\nTôi bấm lại.\n\nĐến lần thứ hai, câu “lát chụp đủ” vẫn nghe vô hại như hôm đó.";}
        public void Draft()
        {
            BeginModal("Draft desk");Box("Draft paper",modal.transform,73,70,1134,581,paper);
            Label("Draft title",modal.transform,"TRANG 17 / BẢN CHƯA GỬI",100,92,880,39,29,ink,hand);
            Btn("Đóng ×",modal.transform,1082,94,96,32,CloseModal,true);
            Photo(modal.transform,104,166,525,345,State.draft==1?3:4,false,State.draft==1?"0443 / ẢNH HỘI CHỢ":"S00 / ẢNH BUỔI TẬP");
            string[] label={"Hai ô: buổi tập và cuộc hẹn mới","Giữ 0443, sửa câu chú thích","Thử một trang “Cảm ơn Nhi”"};
            for(int i=0;i<3;i++){int j=i;Btn(label[i],modal.transform,670,175+i*79,494,62,()=>{State.draft=j;Draft();},State.draft!=i);}
            Label("Draft caption",modal.transform,State.draft==0?"Một tấm đã có. Một tấm còn chờ mọi người đồng ý.":State.draft==1?"Hội chợ tháng Mười. Ba người trước máy; câu chuyện còn ở ngoài khung.":"Một bản nháp có thể đẹp mà vẫn chưa đúng điều người được nhắc tới muốn.",112,529,509,77,21,ink,diary);
            Label("Draft privacy",modal.transform,"Đang sửa riêng · Chưa đăng nhóm · Chưa gửi in",674,440,488,47,15,gray,ui);
            Btn("Dùng bản nháp này để nói chuyện",modal.transform,670,546,494,56,()=>{int v=State.draft;CloseModal();Act("draft:"+v);});
        }
        void Photo(Transform parent,float x,float y,float w,float h,int count,bool later,string caption)
        {
            Box("Photo backing",parent,x,y,w,h,Color.white);
            var frame=Group("Illustrated photograph",parent).GetComponent<RectTransform>();Place(frame,x+5,y+5,w-10,h-47);
            frame.gameObject.AddComponent<RectMask2D>();
            Sprite picture=caption=="0441"?photos[1]:caption=="0442"?photos[2]:caption=="0443" || caption.StartsWith("0443 /")?photos[3]:later || count==2?photos[5]:caption.StartsWith("08.05")?count==3?retakeThree:photos[4]:photos[0];
            if(caption.StartsWith("VY /"))picture=videoFrame==2?photos[3]:photos[2];
            if(caption.StartsWith("0443 /"))picture=photos[3];
            var art=SpriteImage("Story photograph",frame,picture,0,0,w-10,h-47).rectTransform;art.pivot=Vector2.one*.5f;art.anchoredPosition=new Vector2((w-10)*.5f,-(h-47)*.5f);
            Label("Photo caption",parent,caption,x+13,y+h-35,w-26,23,Mathf.Clamp(w/26,11,16),ink,hand);
        }
        public void FinalPreview()
        {
            BeginModal("Final page preview");DrawFinal(false);
        }
        void OpenFinal(){BeginModal("Yearbook final page");DrawFinal(true);}
        void DrawFinal(bool final)
        {
            Box("Final album shadow",modal.transform,52,44,1184,631,new Color(.07f,.14f,.17f));
            Box("Final album",modal.transform,44,36,1184,631,paper);
            Box("Final spine",modal.transform,630,57,2,577,new Color(.69f,.67f,.57f,.4f));
            Label("Final running title",modal.transform,"12A4 / "+State.album,77,61,1090,26,15,gray,ui);
            Label("Final heading",modal.transform,State.photoCount==2?"Một cuộc hẹn, cuối cùng.":State.photoCount==0?"Những gì được giữ lại.":"Trước. Và bây giờ.",77,103,520,45,30,teal,hand);
            Photo(modal.transform,80,166,514,State.kept0443?246:365,4,false,"THÁNG MƯỜI / BUỔI TẬP / ĐỦ BỐN");
            if(State.kept0443)
            {
                Photo(modal.transform,80,425,180,106,3,false,"0443 / HỘI CHỢ");
                Label("Kept promise",modal.transform,"Hạ: “Tấm này để lại nhé.”\n\nAn đã nhận phần viết lại chữ.",279,434,306,92,20,ink,diary);
            }
            if(State.photoCount>0)Photo(modal.transform,665,166,514,365,State.photoCount,State.photoCount==2,State.photoCount==2?"11.05 / TỜ BỔ SUNG / AN, NHI":State.photoCount==4?"08.05 / AN, NHI, HẠ, MINH":"08.05 / AN, HẠ, MINH");
            else
            {
                Box("Kept plan",modal.transform,666,166,513,365,new Color(.93f,.92f,.86f));
                Label("Kept plan title",modal.transform,"KHÔNG CHỤP LẠI",700,205,447,45,31,ink,hand);
                Label("Kept plan text",modal.transform,"Ảnh buổi tập.\nSơ đồ bốn chỗ của Nhi.\nBản hội chợ chưa kịp chốt.\n\nKhông có ảnh chụp lại.",700,272,446,195,23,ink,diary);
            }
            Label("Final relation",modal.transform,State.haCollaborates?"File ảnh của Hạ. Phần chữ của An.":State.negotiation=="ask"?"Hạ vẫn giữ file cũ. Trang mới chưa có phần dàn của Hạ.":"Hạ đã gửi file gốc. An tự làm lại trang.",82,548,509,55,20,ink,diary);
            Label("Final date",modal.transform,State.photoCount==2?"Tờ bổ sung đặt vào sách sau buổi gặp ở ghế đá.":State.photoCount==0?"Cái máy được cất đi trước khi hết buổi chiều.":State.photoCount==4?"Minh vẫn đang cúi nhặt bìa. Không chụp lại tấm này nữa.":"Chiếc ghế thứ tư đã được cất vào quầy.",667,548,512,55,20,ink,diary);
            if(final)
            {
                Label("Final seal",modal.transform,"17 / ĐÃ ĐẶT VÀO SỔ",81,620,440,30,16,teal,ui);
                Btn("Lưu dấu trang",modal.transform,669,613,217,39,()=>Save(false));
                Btn("Khép sổ",modal.transform,903,613,276,39,()=>{CloseModal();ShowCover();});
            }
            else
            {
                Btn("Xem lại",modal.transform,81,613,252,39,CloseModal,true);
                var b=Btn("Đặt trang 17 vào sổ",modal.transform,669,613,510,39,()=>{CloseModal();Act("publish");});b.interactable=State.CanFinish;
            }
        }
        void Notebook()
        {
            if(Busy || cover!=null)return;
            BeginModal("Notebook");Box("Notebook paper",modal.transform,95,79,1090,564,paper);
            Label("Notebook title",modal.transform,"NHỮNG ĐIỀU ĐÃ NHÌN THẤY",125,104,933,45,31,ink,hand);
            Btn("Đóng ×",modal.transform,1068,106,88,32,CloseModal,true);
            string[] titles={"Lời hẹn","Chiếc máy","Một cuộc hẹn mới"};
            string[] facts={State.openingPhoto?"Nhi đề nghị chụp đủ bốn từ buổi tập. Máy đã từng tự bấm được.":"Chưa có tấm ảnh buổi tập.",State.askedNhi || State.sawVideo?"An đã đưa điện thoại cho Nhi, rồi hẹn lát nữa chụp đủ. Hôm đó không chụp lại.":"Ai đã cầm máy hôm hội chợ? Có thể hỏi Nhi, hoặc xem tư liệu.",State.invite=="accepted"?"Nhi nhận lời lúc 16:45 và phải đi lúc 17:10.":State.invite=="declined"?"Nhi bận và từ chối buổi chụp. Nhóm không chờ hay gọi Nhi về.":State.invite=="not_invited"?"Chỉ xin dùng ảnh buổi tập. Chưa hẹn Nhi chụp mới.":"Chưa gửi lời mời."};
            for(int i=0;i<3;i++){Box("Note card",modal.transform,125+i*341,182,316,244,new Color(.94f,.93f,.87f));Label("Note title",modal.transform,titles[i],143+i*341,200,280,39,27,teal,hand);Label("Note fact",modal.transform,facts[i],143+i*341,258,280,147,23,ink,diary);}
            Label("Notebook intent",modal.transform,"Tôi gấp mép tờ giấy này, để còn mở lại.",127,466,1018,43,23,gray,diary);
            Btn("Lời vừa nói / H",modal.transform,126,553,399,47,History,true);
            Btn("Trở lại câu chuyện",modal.transform,750,553,400,47,CloseModal);
        }
        void History()
        {
            if(Busy || cover!=null)return;
            BeginModal("Dialogue history");Box("History paper",modal.transform,140,55,1000,611,paper);
            Label("History title",modal.transform,"LỜI VỪA NÓI",172,77,820,42,32,ink,hand);Btn("Đóng ×",modal.transform,1020,78,86,32,CloseModal,true);
            var viewport=Group("History viewport",modal.transform).GetComponent<RectTransform>();Place(viewport,176,137,925,479);viewport.gameObject.AddComponent<RectMask2D>();var hit=viewport.gameObject.AddComponent<Image>();hit.color=Color.clear;
            var content=Group("History content",viewport).GetComponent<RectTransform>();Place(content,0,0,917,State.history.Count*70+30);
            var t=Label("History text",content,string.Join("\n\n",State.history.ToArray()),0,0,912,Mathf.Max(500,State.history.Count*100),23,ink,diary);t.ForceMeshUpdate();content.sizeDelta=new Vector2(917,t.preferredHeight+30);t.rectTransform.sizeDelta=new Vector2(912,t.preferredHeight+10);
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=35;scroll.verticalNormalizedPosition=0;
            Label("History hint",modal.transform,"Cuộn để đọc lại. Việc đọc không làm trôi thời gian trong truyện.",174,631,918,20,13,gray,ui);
        }
        void Pause()
        {
            if(Busy || cover!=null)return;
            BeginModal("Pause");Box("Pause paper",modal.transform,359,97,562,529,paper);
            Label("Pause title",modal.transform,"Gấp một góc trang.",393,126,490,51,36,ink,hand);
            Label("Pause explanation",modal.transform,"Dấu trang giữ đúng chỗ bạn đang đọc.\nChơi lại không xóa dấu đã lưu.",393,188,490,65,23,ink,diary);
            Btn("Đọc tiếp",modal.transform,394,279,491,44,CloseModal);
            Btn("Lưu dấu trang",modal.transform,394,336,237,42,()=>Save(false),true);
            Btn("Về dấu đã lưu",modal.transform,647,336,237,42,()=>Confirm("Trở về dấu đã lưu?","Đoạn chưa lưu của lượt này sẽ được bỏ.",()=>Load()),true);
            Btn(instant?"Chữ: hiện ngay":"Chữ: có nhịp nghỉ",modal.transform,394,391,491,39,()=>{instant=!instant;PlayerPrefs.SetInt("CIVI41_Instant",instant?1:0);Pause();},true);
            Btn(reduceMotion?"Chuyển động: nhẹ tối đa":"Chuyển động: tự nhiên",modal.transform,394,443,491,39,()=>{reduceMotion=!reduceMotion;PlayerPrefs.SetInt("CIVI41_ReduceMotion",reduceMotion?1:0);Pause();},true);
            Btn(muted?"Âm thanh: tắt":"Âm thanh: bật",modal.transform,394,495,237,39,()=>{muted=!muted;ambience.volume=muted?0:.075f;sfx.mute=muted;PlayerPrefs.SetInt("CIVI41_Muted",muted?1:0);Pause();},true);
            Btn("Về bìa sổ",modal.transform,647,495,237,39,()=>{CloseModal();ShowCover();},true);
            Btn("Thoát bản thử",modal.transform,394,554,491,38,()=>Confirm("Khép sổ và thoát?","Bạn có thể lưu dấu trang trước khi thoát.",()=>Application.Quit()),true);
        }
        void Confirm(string title,string detail,Action yes)
        {
            BeginModal("Confirm");Box("Confirm paper",modal.transform,365,219,551,281,paper);
            Label("Confirm title",modal.transform,title,394,243,491,61,29,ink,hand);Label("Confirm detail",modal.transform,detail,394,314,491,69,23,ink,diary);
            Btn("Để sau",modal.transform,394,420,231,44,CloseModal,true);Btn("Đồng ý",modal.transform,643,420,242,44,()=>{CloseModal();yes();});
        }
        public void ShowCover()
        {
            CloseModal();if(cover!=null)Destroy(cover);cover=Group("Cover",root);
            Box("Cover paper",cover.transform,0,0,1280,720,paper).raycastTarget=true;
            Picture("Cover courtyard",cover.transform,court,529,0,751,720);
            Box("Cover right wash",cover.transform,529,0,751,720,new Color(.96f,.95f,.89f,.16f));
            for(int i=0;i<4;i++)SpriteImage("Cover actor",cover.transform,bodies[i],570+i*145,205,191,420);
            Box("Cover spine",cover.transform,523,0,6,720,teal);
            Label("Cover series",cover.transform,"CIVI / MỘT CUỐN KỶ YẾU CÒN DANG DỞ",43,45,445,40,13,teal,ui);
            Label("Cover title",cover.transform,"TRANG 17\nKHÔNG TỒN TẠI",40,131,452,150,51,ink,hand);
            Box("Cover red underline",cover.transform,46,301,199,3,red);
            Label("Cover hook",cover.transform,"Bọn mình hẹn chụp đủ bốn.\nTrong tấm đẹp nhất hôm ấy,\nchỉ có ba người.",46,334,433,124,27,ink,diary);
            Btn("Mở trang đầu  →",cover.transform,46,490,432,54,()=>{if(State.node>0 || State.line>0)Confirm("Bắt đầu lượt mới?","Dấu trang đã lưu vẫn được giữ.",()=>NewRun());else NewRun();});
            var resume=Btn("Đọc tiếp dấu trang",cover.transform,46,559,432,46,()=>Load(),true);resume.interactable=File.Exists(SavePath);
            Label("Cover short guide",cover.transform,"Bấm để đọc. Chọn khi An cần đáp.\nCó những việc cần tự tay làm.",46,631,432,51,18,gray,hand);
            Label("Cover badge",cover.transform,"BẢN THỬ TƯƠNG TÁC / v4.1\nNét mực, ảnh và những lời chưa nói",574,43,630,60,18,ink,hand);
            Label("Cover promise",cover.transform,"Một tấm ảnh không kể hết\nnhững gì ở ngoài khung.",575,631,618,57,23,ink,hand);
        }
        public void NewRun()
        {
            CloseModal();if(cover!=null){cover.SetActive(false);Destroy(cover);cover=null;}
            if(!Testing)StopAllCoroutines();transition=capture=false;veil.alpha=0;veil.blocksRaycasts=false;scene="";State=new Run();recordKey="";proofFocus=-1;Present(true);
        }
        public void Save(bool confirmed)
        {
            if(Busy){Notify("Chờ chuyển cảnh xong rồi đặt dấu trang nhé.");return;}
            if(!Testing && File.Exists(SavePath) && !confirmed){Confirm("Thay dấu trang đã lưu?","Lần sau sẽ đọc tiếp từ vị trí hiện tại.",()=>Save(true));return;}
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));string tmp=SavePath+".tmp";
                File.WriteAllText(tmp,JsonUtility.ToJson(State,true));if(File.Exists(SavePath))File.Replace(tmp,SavePath,null);else File.Move(tmp,SavePath);
                Notify("Đã đặt dấu trang: "+Story.Chapter(State)+".");
            }catch(Exception e){Notify("Chưa lưu được dấu trang. Lượt đang chơi vẫn được giữ.");Debug.LogWarning("CIVI41 save: "+e.Message);}
        }
        public void Load()
        {
            try
            {
                var restored=JsonUtility.FromJson<Run>(File.ReadAllText(SavePath));
                if(restored!=null && restored.version==1){restored.version=2;if(restored.node>11){restored.negotiationDone=true;restored.negotiation="legacy";}restored.line=Mathf.Clamp(restored.line,0,Story.Lines(restored).Length-1);}
                if(restored==null || !restored.Valid() || restored.line>=Story.Lines(restored).Length)throw new InvalidDataException("Bookmark invalid");
                CloseModal();if(cover!=null){Destroy(cover);cover=null;}if(!Testing)StopAllCoroutines();transition=capture=false;veil.alpha=0;veil.blocksRaycasts=false;State=restored;recordKey=State.node+":"+State.line;scene="";Present(true);Notify("Đã trở về đúng dấu trang đã lưu.");
            }catch(Exception e){Notify("Chưa đọc được dấu trang. Bạn có thể tiếp tục lượt hiện tại.");Debug.LogWarning("CIVI41 load: "+e.Message);}
        }
        static void Clear(Transform t){for(int i=t.childCount-1;i>=0;i--){var go=t.GetChild(i).gameObject;go.SetActive(false);UnityEngine.Object.Destroy(go);}}
        static GameObject Group(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=Vector2.zero;r.sizeDelta=new Vector2(1280,720);return go;}
        static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        Image Box(string name,Transform parent,float x,float y,float w,float h,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);Place(go.GetComponent<RectTransform>(),x,y,w,h);var im=go.GetComponent<Image>();im.sprite=rounded;im.type=Image.Type.Sliced;im.color=color;im.raycastTarget=false;return im;
        }
        TextMeshProUGUI Label(string name,Transform parent,string text,float x,float y,float w,float h,float size,Color color,TMP_FontAsset f)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);Place(go.GetComponent<RectTransform>(),x,y,w,h+(f==hand?size*.28f:0));
            var t=go.GetComponent<TextMeshProUGUI>();t.font=f;t.text=text;t.fontSize=size;t.color=color;t.alignment=TextAlignmentOptions.TopLeft;t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Overflow;t.raycastTarget=false;t.margin=Vector4.zero;return t;
        }
        Button Btn(string text,Transform parent,float x,float y,float w,float h,Action action,bool secondary=false)
        {
            var im=Box(text,parent,x,y,w,h,secondary?new Color(.91f,.925f,.887f):teal);im.raycastTarget=true;
            var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;b.navigation=new Navigation{mode=Navigation.Mode.None};
            var c=b.colors;c.normalColor=Color.white;c.highlightedColor=new Color(1.12f,1.12f,1.08f);c.pressedColor=new Color(.78f,.86f,.83f);c.disabledColor=new Color(1,1,1,.40f);c.fadeDuration=.12f;b.colors=c;
            var t=Label("Button label",im.transform,text,11,3,w-22,h-6,h>48?22:17,secondary?ink:paper,ui);t.alignment=TextAlignmentOptions.Center;t.enableAutoSizing=false;
            b.onClick.AddListener(()=>{Click();action();});im.gameObject.AddComponent<ButtonMotion>();return b;
        }
        RawImage Picture(string name,Transform parent,Texture tex,float x,float y,float w,float h)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(RawImage));go.transform.SetParent(parent,false);Place(go.GetComponent<RectTransform>(),x,y,w,h);var im=go.GetComponent<RawImage>();im.texture=tex;im.raycastTarget=false;return im;
        }
        Image SpriteImage(string name,Transform parent,Sprite sprite,float x,float y,float w,float h){var im=Box(name,parent,x,y,w,h,Color.white);im.sprite=sprite;im.type=Image.Type.Simple;im.preserveAspect=true;return im;}
        Sprite Rounded()
        {
            var t=new Texture2D(48,48,TextureFormat.RGBA32,false);var pix=new Color[48*48];
            for(int y=0;y<48;y++)for(int x=0;x<48;x++){float dx=Mathf.Max(9-x,0,x-38),dy=Mathf.Max(9-y,0,y-38);pix[y*48+x]=new Color(1,1,1,1-Mathf.Clamp01(Mathf.Sqrt(dx*dx+dy*dy)-8));}
            t.SetPixels(pix);t.Apply();return Sprite.Create(t,new Rect(0,0,48,48),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(12,12,12,12));
        }

        // Exercises the real UI and save store; failures make the QA player exit nonzero.
        IEnumerator RuntimeChecks()
        {
            Directory.CreateDirectory(QAFolder);var errors=new List<string>();
            Application.logMessageReceived+=(message,trace,type)=>{if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert)errors.Add(message+"\n"+trace);};
            yield return null;yield return new WaitForEndOfFrame();
            CheckText(errors,"cover");yield return Snapshot("01-cover.png");
            NewRun();yield return null;Reveal();CheckText(errors,"opening");
            yield return Snapshot("02-proof.png");
            FindButton("Tin nhắn: “Ừ, bỏ đi. Mai tính.”").onClick.Invoke();yield return null;CheckText(errors,"proof-inspected");
            FindButton("Hôm đó mình đã hẹn chuyện gì?  →").onClick.Invoke();yield return null;
            yield return Snapshot("02-dialogue.png");
            while(State.line<Story.Lines(State).Length-1){Reveal();Continue();yield return null;}
            Reveal();Continue();actionAlpha.alpha=1;yield return null;yield return Snapshot("03-choices.png");CheckText(errors,"choices");
            // UI desks are also checked at the actual corresponding story states.
            State.node=6;State.line=0;State.openingPhoto=true;Present(true);Reveal();Evidence();yield return null;Canvas.ForceUpdateCanvases();CheckText(errors,"evidence");
            yield return Snapshot("04-evidence.png");CloseModal();
            State.node=8;Present(true);Reveal();Draft();yield return null;Canvas.ForceUpdateCanvases();CheckText(errors,"draft");
            yield return Snapshot("05-draft.png");CloseModal();
            for(int route=0;route<4;route++)
            {
                NewRun();int guard=0;
                while(State.node!=20 && guard++<220)
                {
                    Reveal();CheckText(errors,"route-"+route+"/"+State.node+":"+State.line);
                    if(State.node==0){FindButton("Tấm ảnh 0443").onClick.Invoke();yield return null;FindButton("Hôm đó mình đã hẹn chuyện gì?  →").onClick.Invoke();yield return null;continue;}
                    if(State.line<Story.Lines(State).Length-1 || Story.Mode(State)=="next"){if(!next.interactable)errors.Add("Next disabled before the last line at "+State.node+":"+State.line);next.onClick.Invoke();yield return null;continue;}
                    Continue();yield return null;
                    var buttons=actions.GetComponentsInChildren<Button>();int choice=0;
                    if(State.node==1)choice=route%3;
                    if(State.node==4)choice=route%2==0?0:1;
                    if(State.node==10)choice=route%2;
                    if(State.node==11)choice=route==0 || route==3?0:route==1?1:2;
                    if(State.node==11 && State.line==Story.Lines(State).Length-1 && route==0){yield return new WaitForSecondsRealtime(.24f);float settled=actionAlpha.alpha;Continue();if(settled<.99f || actionAlpha.alpha<.99f)errors.Add("Choice panel fades again on repeated continue");yield return Snapshot("05-negotiation.png");}
                    if(State.node==12)choice=route==0?0:route==2?2:1;
                    if(State.node==16)choice=route<2?0:route==2?1:2;
                    if(!buttons[choice].interactable)errors.Add("Story action button disabled at "+State.node);CheckPointer(buttons[choice],errors);buttons[choice].onClick.Invoke();yield return null;
                    while(Busy)yield return null;
                    if(State.node==6 && modal!=null)
                    {
                        FindButton("0443 / ba người").onClick.Invoke();yield return null;
                        if(!State.askedNhi){FindButton("9 giây / trước 0443").onClick.Invoke();yield return null;FindButton("Khung 2").onClick.Invoke();yield return null;FindButton("Khung 3").onClick.Invoke();yield return null;}
                        FindButton("Trở lại với Hạ  →").onClick.Invoke();
                    }
                    if(State.node==8 && modal!=null)
                    {
                        string[] options={"Hai ô: buổi tập và cuộc hẹn mới","Giữ 0443, sửa câu chú thích","Thử một trang “Cảm ơn Nhi”"};
                        FindButton(options[route%3]).onClick.Invoke();yield return null;FindButton("Dùng bản nháp này để nói chuyện").onClick.Invoke();
                    }
                    yield return null;if(!State.Valid())errors.Add("Invalid runtime state route "+route+" at "+State.node);
                }
                if(guard>=220)errors.Add("Runtime route stuck: "+route);
                Reveal();FinalPreview();yield return null;Canvas.ForceUpdateCanvases();CheckText(errors,"final-"+route);
                yield return Snapshot("06-ending-"+route+".png");CloseModal();
                Save(true);string before=JsonUtility.ToJson(State);State=new Run();Load();if(JsonUtility.ToJson(State)!=before)errors.Add("Save roundtrip differs route "+route);
            }
            NewRun();CloseModal();State.node=1;State.line=0;Present(true);int initial=State.line;visible=0;spoken.maxVisibleCharacters=0;Continue();if(State.line!=initial || !Complete)errors.Add("First click skipped unread line");Continue();if(State.line!=initial+1)errors.Add("Second click did not advance");
            File.WriteAllText(SavePath,JsonUtility.ToJson(new Run{version=1,node=3,line=1,openingPhoto=true}));Load();if(State.version!=2 || State.node!=3 || State.line!=1)errors.Add("Legacy bookmark failed to migrate");
            State=Checks.Route(0);State.node=10;State.line=0;Present(true);Reveal();yield return null;for(int i=0;i<30;i++)yield return null;
            if(!stage.activeSelf || !actors[2].gameObject.activeInHierarchy)errors.Add("Persistent stage disappeared");
            History();yield return null;CheckText(errors,"history");CloseModal();Notebook();yield return null;CheckText(errors,"notebook");CloseModal();Pause();yield return null;CheckText(errors,"pause");
            File.WriteAllText(Path.Combine(QAFolder,"runtime-result.txt"),errors.Count==0?"PASS: cold open, four UI routes, negotiation, kept-photo page, save/restore + legacy migration, persistent stage, modals.":string.Join("\n",errors.ToArray()));
            yield return new WaitForSecondsRealtime(.8f);Application.Quit(errors.Count==0?0:1);
        }
        Button FindButton(string text)
        {
            foreach(var b in GetComponentsInChildren<Button>())if(b.GetComponentInChildren<TextMeshProUGUI>().text==text)return b;
            throw new InvalidOperationException("Missing UI button: "+text);
        }
        void CheckPointer(Button button,List<string> errors)
        {
            var r=button.GetComponent<RectTransform>();var center=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center));
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=center},hits);
            if(hits.Count==0 || !(hits[0].gameObject==button.gameObject || hits[0].gameObject.transform.IsChildOf(button.transform)))errors.Add("Button is occluded: "+button.name+" / "+(hits.Count==0?"no raycast":hits[0].gameObject.name));
        }
        IEnumerator Snapshot(string filename)
        {
            yield return new WaitForEndOfFrame();
            // A dedicated offscreen render makes QA independent of a hidden/minimized window.
            var camera=Camera.main;var cv=GetComponentInChildren<Canvas>();
            var rt=new RenderTexture(1280,720,24);var oldMode=cv.renderMode;var oldCamera=cv.worldCamera;var oldTarget=camera.targetTexture;
            camera.targetTexture=rt;camera.cullingMask=~0;camera.orthographic=true;camera.orthographicSize=360;
            cv.renderMode=RenderMode.ScreenSpaceCamera;cv.worldCamera=camera;cv.planeDistance=1;
            Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(QAFolder,filename),image.EncodeToPNG());
            RenderTexture.active=previous;camera.targetTexture=oldTarget;camera.cullingMask=0;cv.renderMode=oldMode;cv.worldCamera=oldCamera;
            rt.Release();Destroy(rt);Destroy(image);Canvas.ForceUpdateCanvases();
        }
        void CheckText(List<string> errors,string context)
        {
            Canvas.ForceUpdateCanvases();foreach(var t in GetComponentsInChildren<TextMeshProUGUI>())
            {
                t.ForceMeshUpdate();if(t.name=="History text")continue;
                if(t.isTextOverflowing)errors.Add(context+": text overflow "+t.name+" / "+t.text);
            }
        }
    }
    public class Actor:MonoBehaviour
    {
        public bool Focus,Paused;
        Image image;RectTransform r;Vector2 position;float phase,lean;string cue="";
        public void Setup(Image im,int index){image=im;r=im.rectTransform;r.pivot=new Vector2(.5f,0);r.anchoredPosition+=new Vector2(r.sizeDelta.x*.5f,-r.sizeDelta.y);position=r.anchoredPosition;phase=index*1.79f;}
        public void Cue(string id){if(cue==id)return;cue=id;lean=.45f;}
        public void SetStageX(float left){position.x=left+r.sizeDelta.x*.5f;}
        void Update()
        {
            if(image==null)return;lean=Mathf.MoveTowards(lean,0,Time.unscaledDeltaTime*.9f);
            // The feet remain planted. Small torso-scale changes avoid a floating-card bob.
            float breath=Paused?0:Mathf.Sin(Time.unscaledTime*1.35f+phase)*.0017f;
            r.localScale=Vector3.Lerp(r.localScale,new Vector3(1,1+breath,1),Time.unscaledDeltaTime*5);
            r.anchoredPosition=Vector2.Lerp(r.anchoredPosition,position+new Vector2(Focus?2:0,0),Time.unscaledDeltaTime*4);
            r.localEulerAngles=new Vector3(0,0,Paused?0:(Focus?lean:0));
            image.color=Color.Lerp(image.color,Focus?Color.white:new Color(.89f,.90f,.87f),Time.unscaledDeltaTime*5);
        }
    }
    public class ButtonMotion:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler
    {
        bool hover,down;
        public void OnPointerEnter(PointerEventData e){hover=true;}
        public void OnPointerExit(PointerEventData e){hover=false;down=false;}
        public void OnPointerDown(PointerEventData e){down=true;}
        public void OnPointerUp(PointerEventData e){down=false;}
        void Update(){float target=down?.985f:hover?1.006f:1;transform.localScale=Vector3.Lerp(transform.localScale,Vector3.one*target,Time.unscaledDeltaTime*16);}
        void OnDisable(){hover=down=false;transform.localScale=Vector3.one;}
    }
}
