from pathlib import Path
root=Path(__file__).resolve().parents[1]/'artifacts/chapter1-polish-2026-11-01/work/Game'
base=root/'Assets/_Civi/Scripts/Application/Exploration'
s=(base/'CiviChapterOne.cs').read_text(encoding='utf-8-sig')
s=s.replace('CiviChapterBridge','CiviPolishBridge').replace('CiviChapterOne','CiviChapterPolished').replace('public sealed class CiviChapterPolished','public sealed partial class CiviChapterPolished')
s=s.replace('CIVI-Chapter1-Courtyard','CIVI-Chapter1-Polished-Courtyard').replace('CIVI-Chapter1-DesignRoom','CIVI-Chapter1-Polished-DesignRoom')
s=s.replace('public bool IsDesignRoom;','public bool IsDesignRoom;\n        public CiviChapterEnvironment EnvironmentArt;\n        public Font InterfaceFont;\n        public Texture2D[] Documents;')
s=s.replace('string SavePath {get{return Path.Combine(Application.persistentDataPath,"civi-chapter1-v4_1.json");}}','string SavePath {get{return Path.Combine(Application.persistentDataPath,Environment.GetCommandLineArgs().Contains("-civi-qa")?"civi-chapter1-polish-qa.json":"civi-chapter1-polish-v4_1.json");}}')
s=s.replace('bool Locked {get{return phase==Phase.Opening||phase==Phase.DesignDialogue||showInstructions||phase==Phase.Complete;}}','bool Locked {get{return phase==Phase.Opening||phase==Phase.DesignDialogue||showInstructions||phase==Phase.Complete||transitioning||photoIndex>=0;}}')
s=s.replace('if(phase==Phase.Arrival)return Outer(53,27);\n            if(phase==Phase.WalkToDoor)return Outer(119,39);\n            return Inner(16,9);','if(phase==Phase.Arrival)return EnvironmentArt.PhotoPoint;\n            if(phase==Phase.WalkToDoor)return EnvironmentArt.DoorPoint;\n            return EnvironmentArt.DeskPoint;')
s=s.replace('Studio.ChapterMode=true;','Studio.ChapterMode=true;Studio.ChapterEnvironment=EnvironmentArt;')
s=s.replace('Campus.ChapterMode=true;','Campus.ChapterMode=true;Campus.ChapterEnvironment=EnvironmentArt;')
s=s.replace('DecorateCourtyard();','showInstructions=true;')
s=s.replace('            UpdateLock();\n        }\n        void DecorateCourtyard()', '            CreateInterface();\n            UpdateLock();\n        }\n        void DecorateCourtyard()')
# Old procedural tripod/NPC decoration is no longer used; original authored environment handles it.
a=s.index('        void DecorateCourtyard()');b=s.index('        AudioClip MakeShutter()',a);s=s[:a]+s[b:]
a=s.index('        void Update()');b=s.index('        void BeginDesign()',a)
s=s[:a]+'''        bool transitioning;
        float fadeIn=1,flash;
        int photoIndex=-1,lastActionFrame=-1;
        public string[] ChoiceLabels => phase==Phase.Opening&&opening.CanChoose?opening.Step.choices.Select(c=>c.label).ToArray():
            phase==Phase.DesignDialogue&&design!=null&&design.CanChoose?design.AvailableChoices.Select(c=>c.Label).ToArray():Array.Empty<string>();
        public string CurrentLineId => phase==Phase.Opening?(opening.CurrentLine?.text_id??""):phase==Phase.DesignDialogue?(design.CurrentLine?.Id??""):"";
        bool CanAct(){if(lastActionFrame==Time.frameCount||transitioning||showInstructions||photoIndex>=0)return false;lastActionFrame=Time.frameCount;return true;}
        void Update()
        {
            if(!booted)return;
            fadeIn=Mathf.MoveTowards(fadeIn,0,Time.unscaledDeltaTime*2.5f);
            flash=Mathf.MoveTowards(flash,0,Time.unscaledDeltaTime*3);
            TickInterface();
            WriteQaTelemetry();
            if(Keyboard.current==null||transitioning)return;
            var k=Keyboard.current;
            if(k.f12Key.wasPressedThisFrame)CaptureQa();
            if(photoIndex>=0){if(k.escapeKey.wasPressedThisFrame||k.enterKey.wasPressedThisFrame)ClosePhoto();return;}
            if(k.f9Key.wasPressedThisFrame){Load();return;}
            if(k.hKey.wasPressedThisFrame){showInstructions=!showInstructions;UpdateLock();RefreshInterface();return;}
            if(showInstructions)
            {
                if(k.escapeKey.wasPressedThisFrame||k.enterKey.wasPressedThisFrame||k.spaceKey.wasPressedThisFrame)CloseHelp();
                return;
            }
            if(k.f5Key.wasPressedThisFrame){Save();return;}
            if(k.eKey.wasPressedThisFrame)Interact();
            if(phase!=Phase.Opening&&phase!=Phase.DesignDialogue)return;
            if(k.enterKey.wasPressedThisFrame||k.spaceKey.wasPressedThisFrame)Next();
            var keys=new[]{k.digit1Key,k.digit2Key,k.digit3Key};
            for(int i=0;i<keys.Length;i++)if(keys[i].wasPressedThisFrame)ChooseIndex(i);
        }
        void Interact()
        {
            if(Distance>2.2f||!CanAct())return;
            if(phase==Phase.Arrival){phase=Phase.Opening;UpdateLock();RefreshInterface();}
            else if(phase==Phase.WalkToDoor)
            {
                CiviPolishBridge.CompletedS00=opening.Save();
                Save();StartCoroutine(Transition(CiviPolishBridge.DesignScene));
            }
            else if(phase==Phase.StudioArrival)
            {
                if(!opening.State.completed){Notice("Bắt đầu buổi chụp tại sân trường trước nhé.");return;}
                BeginDesign();Save();RefreshInterface();
            }
        }
        System.Collections.IEnumerator Transition(string scene)
        {
            if(transitioning)yield break;
            transitioning=true;UpdateLock();float elapsed=0;
            while(elapsed<.35f){elapsed+=Time.unscaledDeltaTime;SetFade(elapsed/.35f);yield return null;}
            SceneManager.LoadScene(scene);
        }
        void ChooseIndex(int index)
        {
            if(phase==Phase.Opening&&opening.CanChoose&&index<opening.Step.choices.Length)ChooseOpening(opening.Step.choices[index].choice_id);
            else if(phase==Phase.DesignDialogue&&design.CanChoose&&index<design.AvailableChoices.Count)ChooseDesign(design.AvailableChoices[index].Id);
        }
''' + s[b:]
s=s.replace('        public void Next()\n        {','        public void Next()\n        {\n            if(!CanAct())return;')
s=s.replace('        public void ChooseOpening(string id)\n        {','        public void ChooseOpening(string id)\n        {\n            if(!CanAct())return;')
s=s.replace('if(id=="S00_SHOOT")feedback.PlayOneShot(shutter,.8f);','if(id=="S00_SHOOT"){feedback.PlayOneShot(shutter,.8f);flash=1;}')
s=s.replace('        public void ChooseDesign(string id)\n        {','        public void ChooseDesign(string id)\n        {\n            if(!CanAct())return;')
s=s.replace('Notice("Mang tư liệu tới phòng thiết kế.");Save();','Save();Notice("Buổi chụp hoàn thành · Mang tư liệu đến phòng thiết kế.");')
s=s.replace('Notice("Đã giữ ảnh và sơ đồ. Đến phòng thiết kế.");Save();','Save();Notice("Buổi chụp hoàn thành · Đã giữ ba ảnh và sơ đồ.");')
s=s.replace('void Notice(string s){notice=s;noticeUntil=Time.time+3.2f;}','void Notice(string s){notice=s;noticeUntil=Time.time+4.5f;}')
s=s.replace('try{File.WriteAllText(SavePath,JsonUtility.ToJson(Snapshot(),true));Notice("Đã lưu tiến độ Chương 1");}', '''try{
                var tmp=SavePath+".tmp";File.WriteAllText(tmp,JsonUtility.ToJson(Snapshot(),true));
                if(File.Exists(SavePath))File.Replace(tmp,SavePath,SavePath+".bak");else File.Move(tmp,SavePath);
                Notice("Đã lưu tiến độ Chương 1");
            }''')
s=s.replace('var savedPhase=(Phase)Enum.Parse(typeof(Phase),candidate.stage);','ValidateSnapshot(candidate);\n                var savedPhase=(Phase)Enum.Parse(typeof(Phase),candidate.stage);')
s=s.replace('SceneManager.LoadScene(needsStudio?CiviPolishBridge.DesignScene:CiviPolishBridge.CourtyardScene);','showInstructions=false;StartCoroutine(Transition(needsStudio?CiviPolishBridge.DesignScene:CiviPolishBridge.CourtyardScene));')
a=s.index('        void RestoreSnapshot(');b=s.index('        static string Person(',a)
s=s[:a]+'''        void ValidateSnapshot(CiviChapterSave state)
        {
            if(state==null||state.version!=1||!Enum.TryParse(state.stage,out Phase saved)||!Enum.IsDefined(typeof(Phase),saved))throw new FormatException("Unsupported chapter save");
            if(float.IsNaN(state.px)||float.IsInfinity(state.px)||float.IsNaN(state.py)||float.IsInfinity(state.py))throw new FormatException("Invalid position");
            var trial=new S00Session(S00Scene.text,S00Dialogue.text);trial.Restore(state.s00);
            bool interior=saved==Phase.StudioArrival||saved==Phase.DesignDialogue||saved==Phase.Complete;
            if((saved==Phase.WalkToDoor||interior)!=trial.State.completed)throw new FormatException("Inconsistent opening progress");
            if(saved==Phase.DesignDialogue||saved==Phase.Complete)
            {
                var test=new StorySession(new LocalSource(this),InitialState.text,"S01");test.Restore(state.s01);
                if(test.SceneId!=(saved==Phase.Complete?"S02":"S01"))throw new FormatException("Inconsistent design progress");
            }
        }
        void RestoreSnapshot(CiviChapterSave state)
        {
            ValidateSnapshot(state);
            var nextOpening=new S00Session(S00Scene.text,S00Dialogue.text);nextOpening.Restore(state.s00);
            var st=(Phase)Enum.Parse(typeof(Phase),state.stage);
            bool interior=st==Phase.StudioArrival||st==Phase.DesignDialogue||st==Phase.Complete;
            if(interior!=IsDesignRoom)throw new FormatException("Save/scene mismatch");
            StorySession nextDesign=null;
            if(st==Phase.DesignDialogue||st==Phase.Complete){nextDesign=new StorySession(new LocalSource(this),InitialState.text,"S01");nextDesign.Restore(state.s01);}
            var restored=new Vector2(state.px,state.py);
            // Old bookmarks and future art revisions cannot strand the player inside furniture.
            if(!EnvironmentArt.CanStand(restored))restored=EnvironmentArt.Spawn;
            opening=nextOpening;design=nextDesign;phase=st;showInstructions=false;photoIndex=-1;
            CiviPolishBridge.CompletedS00=state.s00;
            Avatar.position=restored;var rb=Avatar.GetComponent<Rigidbody2D>();if(rb!=null)rb.position=restored;
            Camera.main.GetComponent<CiviFollowCamera2D>()?.Snap();UpdateLock();RefreshInterface();
        }
''' + s[b:]
a=s.index('        void Styles()');b=s.index('        // Deterministic authoring',a);s=s[:a]+s[b:]
# Automated domain integration remains distinct from the real-input QA runner.
s=s.replace('phase=Phase.Opening;UpdateLock();','showInstructions=false;phase=Phase.Opening;UpdateLock();')
s=s.replace('if(Environment.GetCommandLineArgs().Contains("-civi-ch1-autotest"))','if(Environment.GetCommandLineArgs().Contains("-civi-ch1-autotest"))')
(base/'CiviChapterPolished.cs').write_text(s,encoding='utf8')
