using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Civi.Runtime;

namespace Civi.Preview
{
    // Chapter 1 is a playable integration of the AUTHOR-OWNED v4.1 S00 and S01 datasets.
    // It never invents dialogue, grants consent, or changes the content contract.
    [Serializable] public sealed class CiviChapterSave
    {
        public int version=1;
        public string stage, s00, s01;
        public float px,py;
    }

    public static class CiviChapterBridge
    {
        public const string CourtyardScene="CIVI-Chapter1-Courtyard";
        public const string DesignScene="CIVI-Chapter1-DesignRoom";
        public static string CompletedS00="";
        public static CiviChapterSave PendingLoad;
    }

    public sealed class CiviChapterOne : MonoBehaviour
    {
        public bool IsDesignRoom;
        public CiviSchoolWorldV2 Campus;
        public CiviSchoolStudioV2 Studio;
        public TextAsset S00Scene,S00Dialogue,S01Scene,S01Dialogue,S02Scene,S02Dialogue,InitialState;
        public Sprite[] NpcSprites=new Sprite[3];
        // Original CIVI photographs from the current author's art directory:
        // BAD_01, BAD_02, GOOD, 0443; never depict an invented fifth person.
        public Texture2D[] EvidencePhotos=new Texture2D[4];

        sealed class LocalSource : IStorySource
        {
            readonly CiviChapterOne owner;
            public LocalSource(CiviChapterOne o){owner=o;}
            public string SceneJson(string id)
            {
                if(id=="S01")return owner.S01Scene.text;
                if(id=="S02")return owner.S02Scene.text;
                throw new InvalidOperationException("Scene not in Chapter 1 data: "+id);
            }
            public string DialogueJson(string id)
            {
                if(id=="S01")return owner.S01Dialogue.text;
                if(id=="S02")return owner.S02Dialogue.text;
                throw new InvalidOperationException("Dialogue not in Chapter 1 data: "+id);
            }
        }

        enum Phase {Arrival,Opening,WalkToDoor,StudioArrival,DesignDialogue,Complete}
        Phase phase;
        S00Session opening;
        StorySession design;
        Texture2D pixel;
        AudioSource feedback;
        AudioClip shutter;
        GUIStyle title,sub,body,soft,choiceStyle,caption;
        readonly List<Transform> npc=new List<Transform>();
        Transform objective;
        bool booted;
        string notice="";
        float noticeUntil;
        bool showInstructions;
        public bool IsReady {get{return booted;}}
        public bool HasOpening {get{return opening!=null;}}
        public string PhaseName {get{return phase.ToString();}}
        public bool IsChapterComplete {get{return phase==Phase.Complete;}}
        public string StoryScene {get{return design==null?"S00":design.SceneId;}}
        public int EvidenceCount {get{return opening==null?0:opening.State.evidence_seen.Count;}}
        string SavePath {get{return Path.Combine(Application.persistentDataPath,"civi-chapter1-v4_1.json");}}
        Vector3 PlayerPosition {get{return (IsDesignRoom?Studio.Player:Campus.Player).position;}}
        Transform Avatar {get{return IsDesignRoom?Studio.Player:Campus.Player;}}
        bool Locked {get{return phase==Phase.Opening||phase==Phase.DesignDialogue||showInstructions||phase==Phase.Complete;}}
        Vector3 Outer(int x,int y){return new Vector3(x+.5f-76,y+.5f-30,0);}
        Vector3 Inner(int x,int y){return new Vector3(x+.5f-16,y+.5f-11,0);}
        Vector3 Target {get{
            if(phase==Phase.Arrival)return Outer(53,27);
            if(phase==Phase.WalkToDoor)return Outer(119,39);
            return Inner(16,9);
        }}
        float Distance {get{return Vector2.Distance(Avatar.position,Target);}}

        void Start()
        {
            if(!booted)Boot();
            if(Environment.GetCommandLineArgs().Contains("-civi-ch1-autotest"))
                StartCoroutine(AutoPlayerIntegrationTest());
        }

        // Actual standalone-player integration test; does not depend on OS keyboard focus.
        System.Collections.IEnumerator AutoPlayerIntegrationTest()
        {
            yield return new WaitForSeconds(.5f);
            if(!IsDesignRoom)
            {
                phase=Phase.Opening;UpdateLock();
                int guard=0;
                while(phase==Phase.Opening&&guard++<200)
                {
                    if(opening.CanChoose)ChooseOpening(opening.Step.choices[0].choice_id);
                    else Next();
                    yield return null;
                }
                if(!opening.State.completed||phase!=Phase.WalkToDoor)
                    throw new InvalidOperationException("Standalone S00 progression failed");
                CiviChapterBridge.CompletedS00=opening.Save();
                Debug.Log("CIVI CH1 PLAYER S00 PASS: authored choices, photos, courtyard");
                SceneManager.LoadScene(CiviChapterBridge.DesignScene);
            }
            else
            {
                if(!opening.State.completed)
                    throw new InvalidOperationException("Standalone S01 missing S00 handoff");
                BeginDesign();
                int guard=0;
                while(phase==Phase.DesignDialogue&&guard++<100)
                {
                    if(design.CanChoose)ChooseDesign(design.AvailableChoices[0].Id);
                    else Next();
                    yield return null;
                }
                if(phase!=Phase.Complete||design.SceneId!="S02")
                    throw new InvalidOperationException("Standalone S01 progression failed");
                Debug.Log("CIVI CH1 PLAYER INTEGRATION PASS: S00 -> S01 -> S02 boundary, real scene transition, evidence and saved state");
                Application.Quit(0);
            }
        }
        public void ValidateSources()
        {
            if(new UnityEngine.Object[]{S00Scene,S00Dialogue,S01Scene,S01Dialogue,
                S02Scene,S02Dialogue,InitialState}.Any(x=>x==null))
                throw new InvalidOperationException("Chapter 1 content TextAssets not assigned");
            if(NpcSprites==null||NpcSprites.Length!=3||NpcSprites.Any(x=>x==null))
                throw new InvalidOperationException("Chapter 1 NPC sprites missing");
            if(EvidencePhotos==null||EvidencePhotos.Length!=4||EvidencePhotos.Any(x=>x==null))
                throw new InvalidOperationException("Chapter 1 source photos missing");
            var check=new S00Session(S00Scene.text,S00Dialogue.text);
            var next=new StorySession(new LocalSource(this),InitialState.text,"S01");
            if(check.Step.step_id!="S00_MACHINE"||next.SceneId!="S01")
                throw new InvalidOperationException("Unexpected Chapter 1 entry");
        }
        public void Boot()
        {
            if(booted)return;
            ValidateSources();
            opening=new S00Session(S00Scene.text,S00Dialogue.text);
            if(IsDesignRoom)
            {
                if(Studio==null)Studio=FindAnyObjectByType<CiviSchoolStudioV2>();
                if(Studio==null)throw new InvalidOperationException("DesignRoom missing movement world");
                Studio.ChapterMode=true;
                Studio.Boot();
                phase=Phase.StudioArrival;
                if(!String.IsNullOrEmpty(CiviChapterBridge.CompletedS00))
                    opening.Restore(CiviChapterBridge.CompletedS00);
            }
            else
            {
                if(Campus==null)Campus=FindAnyObjectByType<CiviSchoolWorldV2>();
                if(Campus==null)throw new InvalidOperationException("Courtyard missing movement world");
                Campus.ChapterMode=true;
                Campus.Boot();
                phase=Phase.Arrival;
                DecorateCourtyard();
            }
            pixel=new Texture2D(1,1,TextureFormat.RGBA32,false);
            pixel.SetPixel(0,0,Color.white);pixel.Apply();
            feedback=gameObject.AddComponent<AudioSource>();
            shutter=MakeShutter();
            booted=true;
            if(CiviChapterBridge.PendingLoad!=null)
            {
                var state=CiviChapterBridge.PendingLoad;
                CiviChapterBridge.PendingLoad=null;
                RestoreSnapshot(state);
            }
            UpdateLock();
        }
        void DecorateCourtyard()
        {
            var points=new[]{Outer(49,28),Outer(57,29),Outer(60,25)};
            var names=new[]{"Nhi / phụ trách máy","Hạ / dàn trang","Minh / giữ lịch"};
            for(int i=0;i<points.Length;i++)
            {
                var obj=new GameObject(names[i],typeof(SpriteRenderer));
                obj.transform.SetParent(transform,false);
                obj.transform.position=points[i]+new Vector3(0,1.35f,0);
                var render=obj.GetComponent<SpriteRenderer>();
                render.sprite=NpcSprites[i];render.sortingOrder=710;
                npc.Add(obj.transform);
            }
            objective=new GameObject("Tripod / chapter interaction").transform;
            objective.SetParent(transform,false);
            objective.position=Outer(53,27);
            RectWorld("Tripod column",objective,new Vector3(0,.40f,0),
                new Vector3(.12f,1.25f,1),new Color(.33f,.37f,.42f),720);
            RectWorld("Tripod left foot",objective,new Vector3(-.26f,-.12f,0),
                new Vector3(.52f,.10f,1),new Color(.33f,.37f,.42f),720);
            RectWorld("Tripod right foot",objective,new Vector3(.26f,-.12f,0),
                new Vector3(.52f,.10f,1),new Color(.33f,.37f,.42f),720);
            RectWorld("Phone camera",objective,new Vector3(0,1.08f,0),
                new Vector3(.82f,.45f,1),new Color(.28f,.40f,.47f),722);
            RectWorld("Camera lens",objective,new Vector3(0,1.08f,0),
                new Vector3(.22f,.22f,1),new Color(.85f,.91f,.90f),725);
        }
        void RectWorld(string name,Transform parent,Vector3 pos,Vector3 scale,Color color,int order)
        {
            var part=new GameObject(name,typeof(SpriteRenderer));
            part.transform.SetParent(parent,false);
            part.transform.localPosition=pos;part.transform.localScale=scale;
            var r=part.GetComponent<SpriteRenderer>();r.sprite=CiviQuarterWorld2D.Pixel;
            r.color=color;r.sortingOrder=order;
        }
        AudioClip MakeShutter()
        {
            const int rate=22050;
            var clip=AudioClip.Create("CIVI original camera shutter",rate/5,1,rate,false);
            var samples=new float[rate/5];
            for(int i=0;i<samples.Length;i++)
            {
                float t=(float)i/rate;
                float envelope=Mathf.Exp(-30f*t);
                samples[i]=envelope*(Mathf.Sin(2*Mathf.PI*(880-3800*t)*t)*.11f);
            }
            clip.SetData(samples,0);return clip;
        }
        void UpdateLock()
        {
            if(Campus!=null)Campus.ExternalLock=Locked;
            if(Studio!=null)Studio.ExternalLock=Locked;
        }
        void Update()
        {
            if(!booted||Keyboard.current==null)return;
            for(int i=0;i<npc.Count;i++)
            {
                var p=npc[i].position;
                npc[i].localScale=Vector3.one*(1+.012f*Mathf.Sin(Time.time*1.8f+i));
            }
            var k=Keyboard.current;
            if(k.f5Key.wasPressedThisFrame){Save();return;}
            if(k.f9Key.wasPressedThisFrame){Load();return;}
            if(k.hKey.wasPressedThisFrame){showInstructions=!showInstructions;UpdateLock();return;}
            if(showInstructions)
            {
                if(k.escapeKey.wasPressedThisFrame){showInstructions=false;UpdateLock();}
                return;
            }
            if(phase==Phase.Arrival&&Distance<2.7f&&k.eKey.wasPressedThisFrame)
            {
                phase=Phase.Opening;UpdateLock();
                return;
            }
            if(phase==Phase.WalkToDoor&&Distance<2.65f&&k.eKey.wasPressedThisFrame)
            {
                CiviChapterBridge.CompletedS00=opening.Save();
                Save();
                SceneManager.LoadScene(CiviChapterBridge.DesignScene);
                return;
            }
            if(phase==Phase.StudioArrival&&Distance<2.4f&&k.eKey.wasPressedThisFrame)
            {
                if(!opening.State.completed)
                {
                    Notice("Vui lòng bắt đầu từ sân trường.");
                    SceneManager.LoadScene(CiviChapterBridge.CourtyardScene);
                    return;
                }
                BeginDesign();return;
            }
            if(phase!=Phase.Opening&&phase!=Phase.DesignDialogue)return;
            if(k.enterKey.wasPressedThisFrame||k.spaceKey.wasPressedThisFrame)
                Next();
            var keys=new[]{k.digit1Key,k.digit2Key,k.digit3Key};
            for(int i=0;i<keys.Length;i++)
            {
                if(!keys[i].wasPressedThisFrame)continue;
                if(phase==Phase.Opening&&opening.CanChoose&&i<opening.Step.choices.Length)
                    ChooseOpening(opening.Step.choices[i].choice_id);
                if(phase==Phase.DesignDialogue&&design.CanChoose)
                {
                    var opts=design.AvailableChoices;
                    if(i<opts.Count)ChooseDesign(opts[i].Id);
                }
            }
        }
        void BeginDesign()
        {
            design=new StorySession(new LocalSource(this),InitialState.text,"S01");
            var st=opening.State;
            design.ImportS00(st.album_name,st.opening_gesture,st.opening_support,
                st.opening_photos_created,st.evidence_seen,st.facts_discovered);
            phase=Phase.DesignDialogue;UpdateLock();
            Notice("Trang 17 đang ở đâu?");
        }
        public void Next()
        {
            if(phase==Phase.Opening)
            {
                if(opening.CanChoose)return;
                opening.Advance();
                if(opening.State.completed){phase=Phase.WalkToDoor;UpdateLock();Notice("Mang tư liệu tới phòng thiết kế.");Save();}
            }
            else if(phase==Phase.DesignDialogue)
            {
                if(design==null||design.CanChoose)return;
                design.Advance();
                if(design.SceneId=="S02")FinishChapter();
            }
        }
        public void ChooseOpening(string id)
        {
            if(phase!=Phase.Opening||!opening.CanChoose)return;
            opening.Choose(id);
            if(id=="S00_SHOOT")feedback.PlayOneShot(shutter,.8f);
            if(opening.State.completed){phase=Phase.WalkToDoor;UpdateLock();Notice("Đã giữ ảnh và sơ đồ. Đến phòng thiết kế.");Save();}
        }
        public void ChooseDesign(string id)
        {
            if(phase!=Phase.DesignDialogue||design==null||!design.CanChoose)return;
            design.Choose(id);
            if(design.SceneId=="S02")FinishChapter();
            else Save();
        }
        void FinishChapter()
        {
            phase=Phase.Complete;UpdateLock();Save();
            Debug.Log("CIVI CHAPTER 1 COMPLETE: authored S00 -> S01 -> S02 boundary, evidence="+
                opening.State.evidence_seen.Count);
        }
        void Notice(string s){notice=s;noticeUntil=Time.time+3.2f;}

        CiviChapterSave Snapshot()
        {
            return new CiviChapterSave{stage=phase.ToString(),s00=opening.Save(),
                s01=design==null?"":design.Save(),px=PlayerPosition.x,py=PlayerPosition.y};
        }
        public void Save()
        {
            try{File.WriteAllText(SavePath,JsonUtility.ToJson(Snapshot(),true));Notice("Đã lưu tiến độ Chương 1");}
            catch(Exception e){Debug.LogError(e);Notice("Không ghi được save");}
        }
        public void Load()
        {
            try
            {
                if(!File.Exists(SavePath)){Notice("Chưa có tiến độ đã lưu");return;}
                var candidate=JsonUtility.FromJson<CiviChapterSave>(File.ReadAllText(SavePath));
                if(candidate==null||candidate.version!=1||
                    String.IsNullOrEmpty(candidate.s00))throw new FormatException("Unsupported save");
                var savedPhase=(Phase)Enum.Parse(typeof(Phase),candidate.stage);
                bool needsStudio=savedPhase==Phase.StudioArrival||savedPhase==Phase.DesignDialogue||savedPhase==Phase.Complete;
                if(needsStudio!=IsDesignRoom)
                {
                    CiviChapterBridge.PendingLoad=candidate;
                    SceneManager.LoadScene(needsStudio?CiviChapterBridge.DesignScene:CiviChapterBridge.CourtyardScene);
                    return;
                }
                RestoreSnapshot(candidate);
                Notice("Đã tải lại tiến độ");
            }
            catch(Exception e){Debug.LogError(e);Notice("File save không hợp lệ");}
        }
        void RestoreSnapshot(CiviChapterSave state)
        {
            var st=(Phase)Enum.Parse(typeof(Phase),state.stage);
            bool interior=st==Phase.StudioArrival||st==Phase.DesignDialogue||st==Phase.Complete;
            if(interior!=IsDesignRoom)throw new FormatException("Save/scene mismatch");
            opening.Restore(state.s00);
            CiviChapterBridge.CompletedS00=state.s00;
            if(interior&&!opening.State.completed)throw new FormatException("Studio save without completed S00");
            if(st==Phase.DesignDialogue||st==Phase.Complete)
            {
                BeginDesign();
                if(!String.IsNullOrEmpty(state.s01))design.Restore(state.s01);
                else throw new FormatException("Design save without story");
            }
            phase=st;
            var player=Avatar;
            var restored=new Vector2(state.px,state.py);
            if(interior&&!Studio.Walkable(restored))throw new FormatException("Studio restore blocked");
            if(!interior&&!Campus.IsWalkable(restored))throw new FormatException("Courtyard restore blocked");
            player.position=restored;
            var rigidbody=player.GetComponent<Rigidbody2D>();
            if(rigidbody!=null)rigidbody.position=restored;
            var follow=Camera.main.GetComponent<CiviFollowCamera2D>();
            if(follow!=null)follow.Snap();
            UpdateLock();
        }
        static string Person(string id)
        {
            switch(id){case "AN":return "AN";case "NHI":return "NHI";case "HA":return "HẠ";
                case "MINH":return "MINH";case "NARRATOR":return "DẪN TRUYỆN";
                default:return id??"CIVI";}
        }

        void Styles()
        {
            if(title!=null)return;
            title=new GUIStyle(GUI.skin.label){fontSize=33,fontStyle=FontStyle.Bold,wordWrap=true};
            title.normal.textColor=new Color(.98f,.96f,.88f);
            sub=new GUIStyle(title){fontSize=23};
            body=new GUIStyle(GUI.skin.label){fontSize=29,wordWrap=true,alignment=TextAnchor.UpperLeft};
            body.normal.textColor=new Color(.98f,.96f,.90f);
            soft=new GUIStyle(GUI.skin.label){fontSize=20,wordWrap=true};
            soft.normal.textColor=new Color(.79f,.88f,.84f);
            caption=new GUIStyle(soft){fontSize=18};
            choiceStyle=new GUIStyle(GUI.skin.button){
                fontSize=24,wordWrap=true,alignment=TextAnchor.MiddleLeft,
                padding=new RectOffset(22,20,12,12)};
        }
        void Fill(Rect box,Color color)
        {
            GUI.color=color;GUI.DrawTexture(box,pixel);GUI.color=Color.white;
        }
        void Photo(Rect area,Texture2D texture,string label)
        {
            Fill(area,new Color(.98f,.95f,.86f,.98f));
            GUI.DrawTexture(new Rect(area.x+12,area.y+12,area.width-24,area.height-46),
                texture,ScaleMode.ScaleToFit,true);
            GUI.Label(new Rect(area.x+12,area.y+area.height-32,area.width-22,25),label,
                caption);
        }
        void DrawPhotos()
        {
            if(phase==Phase.Opening&&opening.State.opening_photos_created)
            {
                Photo(new Rect(230,168,355,245),EvidencePhotos[0],"ẢNH HỎNG 01 · ĐỦ BỐN");
                Photo(new Rect(622,168,355,245),EvidencePhotos[1],"ẢNH HỎNG 02 · ĐỦ BỐN");
                Photo(new Rect(1013,168,355,245),EvidencePhotos[2],"ẢNH ĐỦ BỐN · GIỮ LẠI");
            }
            else if(phase==Phase.DesignDialogue&&design!=null&&design.StepId=="S01_COMPARE")
            {
                Photo(new Rect(945,173,434,270),EvidencePhotos[3],"0443 · AN, HẠ, MINH");
                Fill(new Rect(180,192,555,142),new Color(.08f,.21f,.22f,.94f));
                GUI.Label(new Rect(205,215,540,102),
                    "BẢN THỬ: 16  |  [17 TẠM GIỮ]  |  18",sub);
            }
        }
        void DrawConversation(string person,string dialogue,string stage)
        {
            DrawPhotos();
            Fill(new Rect(0,575,1600,325),new Color(.05f,.15f,.18f,.97f));
            Fill(new Rect(45,585,11,298),new Color(.96f,.75f,.43f));
            GUI.Label(new Rect(86,593,1370,48),stage,soft);
            GUI.Label(new Rect(86,631,520,41),person,sub);
            GUI.Label(new Rect(86,680,1390,118),dialogue,body);
            GUI.Label(new Rect(101,845,1420,31),
                "ENTER / SPACE  ĐỌC TIẾP   ·   1 / 2 / 3  CHỌN   ·   F5 LƯU   ·   H TRỢ GIÚP",soft);
        }
        void DrawChoices(string stage,IEnumerable<string> labels,Action<int> choose)
        {
            DrawPhotos();
            Fill(new Rect(0,555,1600,345),new Color(.05f,.15f,.18f,.97f));
            GUI.Label(new Rect(92,578,1400,45),stage,sub);
            int i=0;
            foreach(var labelText in labels.Take(3))
            {
                int index=i++;
                if(GUI.Button(new Rect(92,633+index*75,1410,65),
                    (index+1)+".  "+labelText,choiceStyle))choose(index);
            }
        }
        void Guide()
        {
            string heading,mission;
            if(phase==Phase.Arrival){heading="01 / SÂN TRƯỜNG";mission="Đến máy ảnh cùng Nhi, Hạ và Minh";}
            else if(phase==Phase.WalkToDoor){heading="01 / RỜI BUỔI TẬP";mission="Tới cửa phòng thiết kế — dữ liệu ảnh đã được giữ";}
            else if(phase==Phase.StudioArrival){heading="01 / PHÒNG THIẾT KẾ";mission="Đến bàn làm việc để tìm trang 17";}
            else {heading="CHƯƠNG 1 / CIVI";mission="Ảnh chụp không kể hết mọi chuyện";}
            Fill(new Rect(0,0,1600,116),new Color(.05f,.15f,.18f,.89f));
            GUI.Label(new Rect(38,11,1170,45),heading,title);
            GUI.Label(new Rect(42,64,1110,34),mission,sub);
            GUI.Label(new Rect(1260,24,290,72),
                phase==Phase.Opening?"ĐANG CHỌN":
                phase==Phase.DesignDialogue?"ĐANG ĐỌC":"CHAPTER 01",sub);
            if(phase==Phase.Arrival||phase==Phase.WalkToDoor||phase==Phase.StudioArrival)
            {
                Fill(new Rect(0,827,1600,73),new Color(.05f,.15f,.18f,.94f));
                GUI.Label(new Rect(34,838,1530,40),
                    Distance<2.65f?"E  /  TƯƠNG TÁC ĐỂ TIẾP TỤC":
                    "WASD / MŨI TÊN  DI CHUYỂN    ·    MỤC TIÊU: "+
                    Mathf.CeilToInt(Distance)+"m    ·    F5/F9  LƯU/TẢI",sub);
            }
        }
        void OnGUI()
        {
            if(!booted||pixel==null)return;
            Styles();
            GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/1600f,Screen.height/900f,1));
            Guide();
            if(phase==Phase.Opening)
            {
                if(opening.CanChoose)
                {
                    var choices=opening.Step.choices;
                    DrawChoices(opening.Step.stage_direction,choices.Select(c=>c.label),
                        index=>ChooseOpening(choices[index].choice_id));
                }
                else
                {
                    var line=opening.CurrentLine;
                    DrawConversation(Person(line==null?"NARRATOR":line.speaker_id),
                        line==null?"Đang chuyển tới phần tiếp theo...":line.text,
                        opening.Step.stage_direction);
                }
            }
            else if(phase==Phase.DesignDialogue&&design!=null)
            {
                if(design.CanChoose)
                {
                    var choices=design.AvailableChoices;
                    DrawChoices(design.StageDirection,choices.Select(c=>c.Label),
                        index=>ChooseDesign(choices[index].Id));
                }
                else
                {
                    var line=design.CurrentLine;
                    DrawConversation(Person(line==null?"NARRATOR":line.Speaker),
                        line==null?"Đang chuyển cảnh...":line.Text,design.StageDirection);
                }
            }
            else if(phase==Phase.Complete)
            {
                Fill(new Rect(120,205,1360,485),new Color(.05f,.16f,.18f,.98f));
                GUI.Label(new Rect(175,239,1230,75),"CHƯƠNG 1 HOÀN THÀNH",title);
                GUI.Label(new Rect(177,344,1210,218),
                    "Buổi tập đã lưu cả ảnh tốt lẫn hai ảnh hỏng. Sơ đồ của Nhi vẫn được giữ.\n"+
                    "Trang 17 chưa bị xóa; Minh chỉ chuyển vào mục tạm giữ.\n"+
                    "Không ai tự động đồng ý dùng ảnh chỉ vì một lời hẹn.",
                    body);
                GUI.Label(new Rect(179,606,1170,41),
                    "PHẦN TIẾP THEO: S02 / CHIẾC KẸP CÓ CAO SU MỚI",sub);
            }
            if(Time.time<noticeUntil)
            {
                Fill(new Rect(565,120,700,48),new Color(.11f,.28f,.28f,.95f));
                GUI.Label(new Rect(582,123,665,38),notice,soft);
            }
            if(showInstructions)
            {
                Fill(new Rect(190,210,1220,450),new Color(.06f,.17f,.18f,.98f));
                GUI.Label(new Rect(239,251,1100,65),"ĐIỀU KHIỂN",title);
                GUI.Label(new Rect(243,350,1055,251),
                    "WASD hoặc phím mũi tên: đi bộ trong cảnh.\n"+
                    "E: tương tác với máy ảnh, bàn làm việc hoặc cửa.\n"+
                    "Enter / Space: đọc thoại. Phím 1–3: chọn hành động.\n"+
                    "F5: lưu tiến độ. F9: tải tiến độ. H / Esc: đóng bảng này.",body);
            }
        }

        // Deterministic authoring + state-machine test. Run by Unity batchmode.
        public void SelfTest()
        {
            ValidateSources();
            var s00=new S00Session(S00Scene.text,S00Dialogue.text);
            int guard=0;
            while(!s00.State.completed&&guard++<200)
            {
                if(s00.CanChoose)s00.Choose(s00.Step.choices[0].choice_id);
                else s00.Advance();
            }
            if(!s00.State.completed||!s00.State.opening_photos_created||
                !s00.State.evidence_seen.Contains("PHOTO_S00_BAD_01")||
                !s00.State.evidence_seen.Contains("PHOTO_S00_BAD_02")||
                !s00.State.evidence_seen.Contains("PHOTO_S00_GOOD"))
                throw new InvalidOperationException("S00 full route or evidence failed");
            var t=new StorySession(new LocalSource(this),InitialState.text,"S01");
            var o=s00.State;
            t.ImportS00(o.album_name,o.opening_gesture,o.opening_support,
                o.opening_photos_created,o.evidence_seen,o.facts_discovered);
            guard=0;
            while(t.SceneId=="S01"&&guard++<100)
            {
                if(t.CanChoose)t.Choose(t.AvailableChoices[0].Id);
                else t.Advance();
            }
            if(t.SceneId!="S02")throw new InvalidOperationException("S01 did not reach S02");
            var resumed=new StorySession(new LocalSource(this),InitialState.text,"S01");
            resumed.Restore(t.Save());
            if(resumed.SceneId!="S02")throw new InvalidOperationException("Chapter state restore failed");
            Debug.Log("CIVI CH1 STORY PASS: canonical S00 -> S01 -> S02, photos, save/restore.");
        }
    }
}
