using System;
using System.IO;
using System.Linq;
using System.Collections;
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
    public sealed class S00PreviewPlayer : MonoBehaviour
    {
        public CiviAssetCatalog assets;
        public S00Session Session {get;private set;}
        public NotebookDialogue Dialogue {get;private set;}
        public bool Busy {get;private set;}
        public bool ReducedMotion;
        public string CurrentScreen {get;private set;}="";
        RectTransform root,actorsLayer,photoLayer,propsLayer,overlay;
        NotebookChoices choices;NotebookActor[] actors;Text stageNote,toast;CanvasGroup fade;
        RawImage backgroundImage;
        StorySession story;
        AudioSource sfx;GameObject diagram,sign,clamp,battery;bool transition;float toastUntil;
        public string SavePath=>Path.Combine(Application.persistentDataPath,story==null?"civi2-s00-preview-bookmark.json":"civi2-s01-story-bookmark.json");
        void Start()
        {
            try{Initialize();}
            catch(Exception error){Debug.LogException(error);enabled=false;}
        }
        public void Initialize()
        {
            if(Session!=null)return;
            Session=new S00Session(assets.s00Scene.text,assets.s00Dialogue.text);
            var canvasObject=new GameObject("CIVI / S00 Notebook",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform,false);var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
            root=NotebookUI.Group("Reference page",canvasObject.transform,0,0,1600,900);
            root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);root.pivot=new Vector2(.5f,.5f);root.anchoredPosition=Vector2.zero;
            backgroundImage=NotebookUI.Photo("Courtyard",root,assets.Texture("BG_COURTYARD"),0,0,1600,900);
            NotebookUI.Box("Header backing",root,0,0,1600,100,new Color(.08f,.22f,.23f,.93f));
            NotebookUI.Label("Civi title",root,"CIVI / ĐỪNG BẤM VỘI",65,12,940,56,35,assets.handFont,NotebookUI.Paper);
            NotebookUI.Label("Preview scope",root,"THÁNG MƯỜI · BUỔI TẬP · BẢN TÍCH HỢP S00",66,66,1110,25,18,assets.uiFont,NotebookUI.Paper);
            actorsLayer=NotebookUI.Group("Actors",root,30,110,1540,500);actors=new NotebookActor[4];
            for(int i=0;i<4;i++)
            {
                var image=NotebookUI.Photo("Actor "+i,actorsLayer,assets.Texture("Cast"),95+i*345,2,275,500,new Rect(i*.25f,0,.25f,1));
                var actor=image.gameObject.AddComponent<NotebookActor>();actor.image=image;actor.person=i;actors[i]=actor;
            }
            photoLayer=NotebookUI.Group("Polaroids",root,0,105,1600,505);
            propsLayer=NotebookUI.Group("Props",root,0,100,1600,510);
            diagram=Instantiate(assets.diagramPrefab,propsLayer);NotebookUI.Place((RectTransform)diagram.transform,100,132,720,275);
            sign=Instantiate(assets.signPrefab,propsLayer);NotebookUI.Place((RectTransform)sign.transform,1290,395,250,104);
            clamp=Instantiate(assets.clampPrefab,propsLayer);NotebookUI.Place((RectTransform)clamp.transform,32,300,220,200);
            battery=Instantiate(assets.batteryPrefab,propsLayer);NotebookUI.Place((RectTransform)battery.transform,1375,355,168,94);
            NotebookUI.Box("Stage note paper",root,52,108,1495,55,new Color(.979f,.962f,.915f,.91f));
            stageNote=NotebookUI.Label("Stage note",root,"",67,113,1435,48,23,assets.handFont,NotebookUI.Ink);
            Dialogue=Instantiate(assets.dialoguePrefab,root).GetComponent<NotebookDialogue>();
            choices=Instantiate(assets.choicesPrefab,root).GetComponent<NotebookChoices>();
            NotebookUI.Box("Footer",root,0,852,1600,48,NotebookUI.Paper);
            toast=NotebookUI.Label("Progress note",root,"",65,862,790,30,19,assets.handFont,NotebookUI.Teal);
            NotebookUI.Button("Save",root,"Lưu đoạn đang đọc",877,860,216,32,assets.uiFont,Save,true);
            NotebookUI.Button("Restore",root,"Về lần đã lưu",1110,860,209,32,assets.uiFont,Load,true);
            NotebookUI.Button("Reduce motion",root,"Giảm chuyển động",1335,860,214,32,assets.uiFont,()=>{ReducedMotion=!ReducedMotion;foreach(var actor in actors)actor.ReducedMotion=ReducedMotion;Notify(ReducedMotion?"Đã giảm chuyển động":"Đã bật chuyển động nhẹ");},true);
            NotebookUI.Button("Album",root,"Ảnh đã chụp",1335,25,214,43,assets.uiFont,()=>{if(Session.State.opening_photos_created)ShowAlbum();else Notify("Chụp xong mới có ảnh trong album nhé.");},true);
            overlay=NotebookUI.Group("Overlay",root,0,0,1600,900);overlay.gameObject.SetActive(false);
            var veil=NotebookUI.Box("Scene transition",root,0,0,1600,900,NotebookUI.Paper);fade=veil.gameObject.AddComponent<CanvasGroup>();fade.alpha=0;fade.blocksRaycasts=false;
            sfx=gameObject.AddComponent<AudioSource>();sfx.playOnAwake=false;
            if(FindAnyObjectByType<EventSystem>()==null){var system=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));system.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();}
            Notify("Bấm Đọc tiếp. Khi An cần đáp, chọn một việc để làm.");Render();
        }
        void Update()
        {
            if(Session==null)return;
            if(Time.unscaledTime>toastUntil)toast.text=story==null?Session.State.album_name+" / "+Session.State.step_id:story.SceneId+" / "+story.StepId;
            var keyboard=Keyboard.current;
            if(keyboard==null||Busy)return;
            if(keyboard.escapeKey.wasPressedThisFrame&&overlay.gameObject.activeSelf){CloseOverlay();return;}
            if(overlay.gameObject.activeSelf)return;
            if(keyboard.spaceKey.wasPressedThisFrame||keyboard.enterKey.wasPressedThisFrame)Next();
            if(story!=null && story.CanChoose)
            {
                var keys=new[]{keyboard.digit1Key,keyboard.digit2Key,keyboard.digit3Key};
                var available=story.AvailableChoices;
                for(int i=0;i<keys.Length&&i<available.Count;i++)if(keys[i].wasPressedThisFrame)Choose(available[i].Id);
            }
            else if(story==null && Session.CanChoose)
            {
                var keys=new[]{keyboard.digit1Key,keyboard.digit2Key,keyboard.digit3Key};
                for(int i=0;i<keys.Length&&i<Session.Step.choices.Length;i++)if(keys[i].wasPressedThisFrame)Choose(Session.Step.choices[i].choice_id);
            }
        }
        public void Next()
        {
            if(Busy||overlay.gameObject.activeSelf)return;
            if(story!=null)
            {
                if(story.IsComplete||story.CanChoose)return;
                if(!Dialogue.Revealed){Dialogue.Reveal();return;}
                story.Advance();Render();return;
            }
            if(Session.State.completed||Session.CanChoose)return;
            if(!Dialogue.Revealed){Dialogue.Reveal();return;}Session.Advance();Render();
        }
        public void Choose(string id)
        {
            if(Busy||overlay.gameObject.activeSelf)return;
            if(story!=null)
            {
                try{story.Choose(id);Play("CiviClick");Render();}
                catch(Exception error){Debug.LogException(error);Notify("Lựa chọn chưa thực thi được: "+error.Message);}
                return;
            }
            if(id=="S00_SHOOT"){StartCoroutine(Capture());return;}
            Session.Choose(id);Play("CiviClick");Render();
        }
        public void Render()
        {
            if(story!=null){RenderStory();return;}
            NotebookUI.Clear(photoLayer);CurrentScreen=Session.State.completed?"handoff":Session.State.step_id;
            if(Session.State.completed){ShowHandoff();return;}
            actorsLayer.gameObject.SetActive(true);propsLayer.gameObject.SetActive(true);
            diagram.SetActive(Session.State.step_id=="S00_DIAGRAM");sign.SetActive(true);
            clamp.SetActive(Session.State.step_id=="S00_MACHINE");battery.SetActive(Session.State.opening_gesture=="pose_3");
            stageNote.text=Session.State.step_id=="S00_MACHINE"?"Buổi tập hội chợ / Chiếc máy chưa chịu đứng yên":Session.State.step_id=="S00_POSE"?"Một tấm đủ bốn / Thử đứng khác đi một chút":Session.State.step_id=="S00_CAPTURE"?"Hẹn giờ rồi chạy vào khung":Session.State.step_id=="S00_ALBUM"?"Giữ cả mấy tấm chụp hỏng nhé.":"Một lời hẹn trước ngày ra trường";
            choices.gameObject.SetActive(Session.CanChoose);Dialogue.gameObject.SetActive(!Session.CanChoose);
            if(Session.State.step_id=="S00_ALBUM"){actorsLayer.gameObject.SetActive(false);propsLayer.gameObject.SetActive(false);AlbumPhotos(photoLayer,false);}
            if(Session.CanChoose){choices.Show(Session.Step.choices,Choose);return;}
            var line=Session.CurrentLine;
            if(line!=null)
            {
                Dialogue.Show(line,assets,Next);
                int who=Array.IndexOf(new[]{"AN","NHI","HA","MINH"},line.speaker_id);
                foreach(var actor in actors)actor.SetSpeaker(who);
            }
        }
        // W2.2-A playable integration preview. TV3's original S00 QA remains unchanged.
        void StartS01()
        {
            try
            {
                if(!Session.State.completed)throw new InvalidOperationException("S00 is not complete");
                var source=new UnityStorySource(assets);
                var next=new StorySession(source,assets.storyInitialState.text,"S01");
                var state=Session.State;
                next.ImportS00(state.album_name,state.opening_gesture,state.opening_support,
                    state.opening_photos_created,state.evidence_seen,state.facts_discovered);
                story=next;Render();
            }
            catch(Exception error)
            {
                Debug.LogException(error);
                Notify("Chua mo duoc S01: "+error.Message);
            }
        }
        void RenderStory()
        {
            NotebookUI.Clear(photoLayer);
            CurrentScreen=story.SceneId+"/"+story.StepId;
            backgroundImage.texture=assets.Texture("BG_DESIGN_ROOM");
            actorsLayer.gameObject.SetActive(story.SceneId=="S01");
            propsLayer.gameObject.SetActive(false);
            if(story.SceneId!="S01")
            {
                Dialogue.gameObject.SetActive(false);choices.gameObject.SetActive(false);stageNote.text="";
                NotebookUI.Box("S02 boundary",photoLayer,105,65,1390,400,NotebookUI.Paper);
                NotebookUI.Label("Boundary heading",photoLayer,"S01 HOAN TAT / DA CHUYEN SANG "+story.SceneId,135,92,1300,62,35,assets.handFont,NotebookUI.Teal);
                NotebookUI.Label("Boundary detail",photoLayer,"Story Runtime da chuyen canh va giu state. Chon KHAM PHA 2D de dieu khien An o S02, dieu tra S03 va kiem tra evidence. Ban P0 dung truoc gameplay S04.",135,186,1290,130,28,assets.diaryFont,NotebookUI.Ink);
                NotebookUI.Button("Restart",photoLayer,"Choi lai tu S00",140,362,550,58,assets.uiFont,()=>StartCoroutine(Restart()));
                if(story.SceneId=="S02")
                    NotebookUI.Button("Enter 2D",photoLayer,"KHÁM PHÁ 2D / S02",718,362,610,58,assets.uiFont,()=>
                    {
                        StoryHandoffBridge.Payload=story.Save();
                        SceneManager.LoadScene("S02-2D-VerticalSlice");
                    });
                return;
            }
            stageNote.text=story.StepId=="S01_OPEN"?"07/05/2026 / Ban in thu dang thieu trang 17":"Anh hoi cho va mot loi hen chua tron";
            if(story.CanChoose)
            {
                var available=story.AvailableChoices;
                if(available.Count==0 || available.Count>3)
                    throw new InvalidOperationException("S01 choice count unsupported by preview UI: "+available.Count);
                var mapped=available.Select(c=>new ChoiceData{choice_id=c.Id,label=c.Label,next=c.Next,lines=new string[0]}).ToArray();
                Dialogue.gameObject.SetActive(false);choices.gameObject.SetActive(true);
                choices.Show(mapped,Choose);
                return;
            }
            choices.gameObject.SetActive(false);
            var line=story.CurrentLine;
            if(line==null)throw new InvalidOperationException("Story has no available line/choice at "+story.StepId);
            Dialogue.gameObject.SetActive(true);
            Dialogue.Show(new LineData{text_id=line.Id,speaker_id=line.Speaker,text=line.Text,mood=line.Mood},assets,Next);
            int who=Array.IndexOf(new[]{"AN","NHI","HA","MINH"},line.Speaker);
            foreach(var actor in actors)actor.SetSpeaker(who);
        }

        IEnumerator Capture()
        {
            Busy=true;choices.gameObject.SetActive(false);Dialogue.gameObject.SetActive(false);actorsLayer.gameObject.SetActive(false);propsLayer.gameObject.SetActive(false);
            var label=NotebookUI.Label("Countdown",photoLayer,"3",650,125,350,200,100,assets.handFont,NotebookUI.Paper);label.alignment=TextAnchor.MiddleCenter;
            for(int i=3;i>0;i--){label.text=i.ToString();Play("CiviTimer");yield return new WaitForSecondsRealtime(.5f);}
            foreach(var id in new[]{"PHOTO_S00_BAD_01","PHOTO_S00_BAD_02","PHOTO_S00_GOOD"})
            {
                NotebookUI.Clear(photoLayer);Polaroid(photoLayer,id,350,5,900,490);Play("CiviShutter");yield return new WaitForSecondsRealtime(ReducedMotion?.3f:.85f);
            }
            Session.Choose("S00_SHOOT");Busy=false;Render();
        }
        void Polaroid(Transform parent,string id,float x,float y,float w,float h)
        {
            NotebookUI.Box("Photo paper",parent,x,y,w,h,NotebookUI.Paper);
            Texture2D photo=id=="PHOTO_S00_GOOD"?assets.Gesture("S00",Session.State.opening_gesture):assets.Texture(id);
            float aspect=(float)photo.width/photo.height;float available=h-55;float imageW=Mathf.Min(w-30,available*aspect);float imageH=imageW/aspect;
            NotebookUI.Photo(id,parent,photo,x+(w-imageW)/2,y+12,imageW,imageH);
            NotebookUI.Label("Photo caption",parent,id=="PHOTO_S00_BAD_01"?"01 / Đèn vừa tắt":id=="PHOTO_S00_BAD_02"?"02 / Minh còn nhặt bìa":"03 / Một lần đủ bốn",x+20,y+h-39,w-40,31,23,assets.handFont,NotebookUI.Ink);
        }
        void AlbumPhotos(Transform parent,bool wide)
        {
            string[] ids={"PHOTO_S00_BAD_01","PHOTO_S00_BAD_02","PHOTO_S00_GOOD"};
            for(int i=0;i<3;i++)Polaroid(parent,ids[i],wide?60+i*510:65+i*300,wide?140:135,wide?480:280,wide?390:250);
        }
        void BeginOverlay(){NotebookUI.Clear(overlay);overlay.gameObject.SetActive(true);NotebookUI.Box("Overlay paper",overlay,25,25,1550,825,NotebookUI.Paper);}
        public void CloseOverlay(){overlay.gameObject.SetActive(false);NotebookUI.Clear(overlay);}
        void ShowAlbum()
        {
            BeginOverlay();NotebookUI.Label("Album title",overlay,Session.State.album_name,70,55,1210,60,39,assets.handFont,NotebookUI.Teal);AlbumPhotos(overlay,true);
            NotebookUI.Button("Close album",overlay,"Cất album / Esc",1230,762,290,48,assets.uiFont,CloseOverlay);
        }
        public void ShowCallback()
        {
            BeginOverlay();NotebookUI.Label("Callback title",overlay,"Cùng một cử chỉ, hai lần đứng cạnh nhau",65,53,1360,60,38,assets.handFont,NotebookUI.Teal);
            for(int i=0;i<2;i++)
            {
                var image=assets.Gesture(i==0?"S00":"NEW_FOUR",Session.State.opening_gesture);
                NotebookUI.Photo("Gesture callback "+i,overlay,image,65+i*770,150,725,483);
                NotebookUI.Label("Time caption",overlay,i==0?"Buổi tập / lựa chọn đã chọn":"Ảnh chụp lại / ứng viên hình cho S09_A",65+i*770,650,725,50,25,assets.handFont,NotebookUI.Teal);
            }
            NotebookUI.Button("Close callback",overlay,"Cất hai tấm ảnh",1230,762,290,48,assets.uiFont,CloseOverlay);
        }
        public DraftReviewView ShowDrafts(bool simulateRequest=true)
        {
            BeginOverlay();NotebookUI.Clear(overlay);
            var view=Instantiate(assets.draftReviewPrefab,overlay).GetComponent<DraftReviewView>();
            view.Gesture=Session.State.opening_gesture;view.Initialize(assets,CloseOverlay);
            NotebookUI.Label("UI test scope",overlay,"BẢN THỬ UI / xác nhận chỉ ghi lựa chọn thử, chưa gửi trong cốt truyện",58,861,1450,27,18,assets.uiFont,NotebookUI.Muted);
            if(simulateRequest)view.Requested+=request=>StartCoroutine(PreviewDraftRequest(view,request));
            return view;
        }
        IEnumerator PreviewDraftRequest(DraftReviewView view,DraftPreviewRequest request)
        {
            yield return new WaitForSecondsRealtime(.55f);
            if(view!=null)view.CompleteRequest(true,"Đã ghi lựa chọn thử: "+assets.Draft(request.DraftType).title+" · "+DraftReviewView.RecipientName(request.Recipient)+".\nChưa gửi nội dung trong cốt truyện.");
        }
        void ShowHandoff()
        {
            Dialogue.gameObject.SetActive(false);choices.gameObject.SetActive(false);actorsLayer.gameObject.SetActive(false);propsLayer.gameObject.SetActive(false);stageNote.text="";
            NotebookUI.Box("Handoff paper",photoLayer,100,65,1400,410,NotebookUI.Paper);
            NotebookUI.Label("Handoff heading",photoLayer,"S00 ĐÃ ĐỌC XONG / MỘT LỜI HẸN ĐƯỢC CẤT LẠI",135,96,1320,70,38,assets.handFont,NotebookUI.Teal);
            NotebookUI.Label("Handoff scope",photoLayer,"Album “"+Session.State.album_name+"” đã giữ cả ba tấm.\nĐiểm nối tiếp là S01: trang 17 trong bản in thử.\n\nBản thử có thể tiếp tục vào S01; các cảnh S02–S10 chưa được tích hợp vào UI chính.",135,175,1310,180,29,assets.diaryFont,NotebookUI.Ink);
            NotebookUI.Button("Callback",photoLayer,"Xem cặp ảnh cùng cử chỉ",135,378,440,54,assets.uiFont,ShowCallback);
            NotebookUI.Button("Animatic QA",photoLayer,"Kiểm animatic S04",601,378,350,54,assets.uiFont,()=>
            {
                BeginOverlay();NotebookUI.Clear(overlay);var go=new GameObject("VIDEO_VY animatic",typeof(RectTransform),typeof(VyAnimatic));go.GetComponent<VyAnimatic>().ReducedMotion=ReducedMotion;go.GetComponent<VyAnimatic>().Open(assets,overlay,CloseOverlay);
            });
            NotebookUI.Button("Restart S00",photoLayer,"Đọc lại S00",978,378,350,54,assets.uiFont,()=>StartCoroutine(Restart()));
            NotebookUI.Button("Draft UI preview",photoLayer,"Thử ba mẫu nháp",135,444,550,45,assets.uiFont,()=>ShowDrafts(),true);
            NotebookUI.Button("Continue S01",photoLayer,"Tiếp tục S01",704,444,624,45,assets.uiFont,StartS01,true);
        }
        IEnumerator Restart()
        {
            Busy=true;fade.blocksRaycasts=true;
            for(float t=0;t<.2f;t+=Time.unscaledDeltaTime){fade.alpha=t/.2f;yield return null;}
            story=null;Session=new S00Session(assets.s00Scene.text,assets.s00Dialogue.text);Render();
            for(float t=.2f;t>0;t-=Time.unscaledDeltaTime){fade.alpha=t/.2f;yield return null;}
            fade.alpha=0;fade.blocksRaycasts=false;Busy=false;
        }
        public void Save()
        {
            if(Busy){Notify("Đợi chụp xong rồi lưu nhé.");return;}
            try{string temp=SavePath+".tmp";string payload=story==null?Session.Save():story.Save();File.WriteAllText(temp,payload);if(File.Exists(SavePath))File.Replace(temp,SavePath,null);else File.Move(temp,SavePath);Notify("Đã lưu đúng câu đang đọc. Bản lưu game cũ không thay đổi.");}
            catch(Exception error){Notify("Chưa lưu được: "+error.Message);}
        }
        public void Load()
        {
            if(Busy)return;
            try{if(!File.Exists(SavePath)){Notify("Chưa có lần lưu cho cảnh hiện tại.");return;}if(story==null)Session.Restore(File.ReadAllText(SavePath));else story.Restore(File.ReadAllText(SavePath));CloseOverlay();Render();Notify("Đã về lần lưu gần nhất.");}
            catch(Exception error){Notify("Không đọc được bản lưu: "+error.Message);}
        }
        void Notify(string text){if(toast!=null){toast.text=text;toastUntil=Time.unscaledTime+6;}}
        void Play(string id){var clip=assets.Sound(id);if(clip!=null)sfx.PlayOneShot(clip,.3f);}
    }
}
