using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Civi.Runtime;

namespace Civi.Preview
{
    // A single continuous three-screen traversal per episode + canonical branching.
    // Visuals reuse FOUR verified reference paintings; text is the 28 Oct novel,
    // not fabricated dialogue or a replacement for the author's source.
    public sealed class CiviFullCampaign : MonoBehaviour
    {
        [Serializable] public sealed class Episode
        {
            public string id,title,region,visual;
            public string[] paragraphs,beats;
        }
        [Serializable] public sealed class Campaign
        {
            public string version,title,source_url,source_sha256;
            public Episode[] episodes;
        }
        [Serializable] sealed class Snapshot
        {
            public string episode,decision1,decision2,decision3;
            public int visited,readerIndex;
            public float x,y;
            public bool finished;
        }

        public TextAsset Content;
        public Sprite[] Backgrounds=new Sprite[4];
        public Sprite[] Foregrounds=new Sprite[4];
        public Sprite[] AvatarViews=new Sprite[3];
        const float ChunkWidth=15.0f;
        const float ChunkHeight=8.44f;
        const float MinY=-2.95f,MaxY=.75f;
        const float ExitX=35.75f;
        const float Trigger=1.55f;

        readonly List<Transform> markers=new List<Transform>();
        readonly HashSet<string> visitedChapters=new HashSet<string>();
        Campaign data;
        Dictionary<string,Episode> episodes;
        Episode current;
        Transform root,actor;
        Civi2DPlayer mover;
        CiviFollowCamera2D follow;
        Rigidbody2D physical;
        GameObject sceneRoot;
        int visitedMask, activeReading, beatCursor, readerIndex;
        string branchI="",branchII="",branchIII="";
        string previewText="";
        bool finished;
        enum Mode {Title,Explore,Beat,FullReader,Choice,Ending,Help}
        Mode mode=Mode.Title,returnMode=Mode.Explore;
        GUIStyle hero,heading,body,minor,choiceStyle,small,dim;
        Texture2D white;
        string SaveFile {get {return Path.Combine(Application.persistentDataPath,"civi-new-story-campaign-v1.json");}}
        public int EpisodeCount { get {return data==null?0:data.episodes.Length;} }
        public string EpisodeId {get {return current==null?"":current.id;}}
        public int ChunkCount {get {return markers.Count;} }
        public bool Finished {get {return finished;} }
        public bool HasController {get {return mover!=null&&follow!=null&&follow.Target==actor;} }

        void Start()
        {
            try{Boot();}
            catch(Exception ex){Debug.LogException(ex);enabled=false;}
        }
        public void Boot()
        {
            if(Content==null)throw new InvalidOperationException("CIVI canonical novel JSON missing.");
            data=JsonUtility.FromJson<Campaign>(Content.text);
            if(data==null||data.episodes==null)throw new FormatException("CIVI campaign content invalid.");
            episodes=data.episodes.ToDictionary(e=>e.id,e=>e,StringComparer.Ordinal);
            CiviCampaignGraph.Validate(episodes.Keys);
            if(Backgrounds==null||Foregrounds==null||Backgrounds.Length!=4||Foregrounds.Length!=4||
               AvatarViews==null||AvatarViews.Length!=3||Backgrounds.Any(s=>s==null)||
               Foregrounds.Any(s=>s==null)||AvatarViews.Any(s=>s==null))
                throw new InvalidOperationException("CIVI campaign art assignments incomplete.");
            var cam=Camera.main;
            if(cam==null)throw new InvalidOperationException("Missing camera");
            cam.orthographic=true;cam.clearFlags=CameraClearFlags.SolidColor;
            cam.backgroundColor=new Color(.030f,.072f,.087f);
            cam.transform.position=new Vector3(0,0,-10);
            sceneRoot=new GameObject("Chapter / multi-screen exploration");
            sceneRoot.transform.SetParent(transform,false);
            root=sceneRoot.transform;
            var obj=new GameObject("MAI / playable placeholder",typeof(SpriteRenderer));
            obj.transform.SetParent(transform,false);
            actor=obj.transform;
            var spr=obj.GetComponent<SpriteRenderer>();spr.sortingOrder=50;
            var rb=obj.AddComponent<Rigidbody2D>();
            rb.gravityScale=0;rb.freezeRotation=true;
            rb.interpolation=RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            physical=rb;
            obj.AddComponent<CircleCollider2D>().radius=.37f;
            obj.AddComponent<SortingGroup>();
            obj.AddComponent<CiviDepthSort2D>().FeetOffset=-.40f;
            mover=obj.AddComponent<Civi2DPlayer>();
            mover.Speed=4.65f;
            var av=obj.AddComponent<CiviNewStoryPreviewAvatar>();
            av.Front=AvatarViews[0];av.Back=AvatarViews[1];av.Side=AvatarViews[2];
            av.Player=mover;av.Visual=spr;

            follow=cam.GetComponent<CiviFollowCamera2D>();
            if(follow==null)follow=cam.gameObject.AddComponent<CiviFollowCamera2D>();
            follow.MinZoom=3.55f;follow.MaxZoom=4.18f;follow.SmoothTime=.27f;
            follow.Configure(actor,new Rect(-ChunkWidth*.5f,-ChunkHeight*.5f,
                ChunkWidth*3f,ChunkHeight),3.75f);
            white=new Texture2D(1,1,TextureFormat.RGBA32,false);
            white.SetPixel(0,0,Color.white);white.Apply();
            current=episodes["C1"];DrawWorld();
            mode=Mode.Title;PauseMovement();
        }

        public void BeginCampaign()
        {
            finished=false;branchI=branchII=branchIII="";
            visitedChapters.Clear();current=episodes["C1"];visitedMask=0;
            readerIndex=0;DrawWorld();mode=Mode.Beat;activeReading=-1;beatCursor=0;
            previewText="Bắt đầu hành trình của Mai. Ba điểm sáng là ba khoảnh khắc được trích nguyên văn từ chương này. Đi qua các màn bằng WASD, đọc bằng E; nhấn R để mở toàn văn.";
            PauseMovement();
        }
        static int ArtIndex(string visual,int stage)
        {
            switch(visual)
            {
                case "rain":return stage==2?2:0;
                case "storm":return stage==1?2:1;
                case "filter":return stage==2?0:2;
                default:return stage==2?2:3;
            }
        }
        static GameObject CreateArt(Transform parent,string label,Sprite src,float x,int order)
        {
            var go=new GameObject(label,typeof(SpriteRenderer));
            go.transform.SetParent(parent,false);
            go.transform.localPosition=new Vector3(x,0,0);
            var s=go.GetComponent<SpriteRenderer>();s.sprite=src;s.sortingOrder=order;
            float ratio=ChunkWidth/src.bounds.size.x;
            go.transform.localScale=Vector3.one*ratio;
            return go;
        }
        void DrawWorld()
        {
            for(int i=root.childCount-1;i>=0;i--)
                if(Application.isPlaying)Destroy(root.GetChild(i).gameObject);
                else DestroyImmediate(root.GetChild(i).gameObject);
            markers.Clear();
            for(int stage=0;stage<3;stage++)
            {
                int index=ArtIndex(current.visual,stage);
                float center=ChunkWidth*stage;
                var segment=new GameObject("SCREEN "+(stage+1)+" / "+current.region);
                segment.transform.SetParent(root,false);
                CreateArt(segment.transform,"Backdrop / 3-quarter painting",Backgrounds[index],center,-1000);
                CreateArt(segment.transform,"Foreground / near occlusion",Foregrounds[index],center,4500);
                // A dark seam disguised as a gate separates authored background plates.
                // Unlike NS02, the camera moves THROUGH all 3 screens with the player.
                if(stage>0)
                {
                    var seam=new GameObject("Threshold shadow",typeof(SpriteRenderer));
                    seam.transform.SetParent(segment.transform,false);
                    seam.transform.position=new Vector3(center-ChunkWidth*.5f,0,0);
                    var sr=seam.GetComponent<SpriteRenderer>();
                    sr.sprite=CiviQuarterWorld2D.Pixel;sr.sortingOrder=35;
                    sr.color=new Color(.07f,.11f,.14f,.25f);
                    seam.transform.localScale=new Vector3(.19f,6.5f,1);
                }
                var marker=new GameObject("Evidence/story beat "+(stage+1));
                marker.transform.SetParent(segment.transform,false);
                marker.transform.position=new Vector3(center-1.15f,-1.15f,-.08f);
                var diamond=new GameObject("Marker diamond",typeof(SpriteRenderer));
                diamond.transform.SetParent(marker.transform,false);
                diamond.transform.localPosition=new Vector3(0,.31f,0);
                diamond.transform.localRotation=Quaternion.Euler(0,0,45);
                diamond.transform.localScale=new Vector3(.20f,.20f,1);
                var ds=diamond.GetComponent<SpriteRenderer>();
                ds.sprite=CiviQuarterWorld2D.Pixel;ds.sortingOrder=3500;
                ds.color=new Color(.98f,.78f,.49f,.95f);
                markers.Add(marker.transform);
            }
            actor.position=new Vector3(-5.8f,-1.25f,0);
            physical.position=actor.position;
            physical.linearVelocity=Vector2.zero;
            follow.Snap();
        }
        Transform Nearest()
        {
            Transform found=null;float min=Trigger;
            foreach(var target in markers)
            {
                var distance=Vector2.Distance(actor.position,target.position);
                if(distance<min){min=distance;found=target;}
            }
            return found;
        }
        int NearestIndex()
        {
            var t=Nearest();
            return t==null?-1:markers.IndexOf(t);
        }
        void PauseMovement(){if(mover!=null)mover.AllowMovement=mode==Mode.Explore;}
        void BeginBeat(int marker)
        {
            if(marker<0||marker>=3||current==null)return;
            mode=Mode.Beat;activeReading=marker;beatCursor=0;
            previewText=current.beats[Mathf.Min(marker*2,current.beats.Length-1)];
            PauseMovement();
        }
        void NextBeat()
        {
            if(activeReading<0){mode=Mode.Explore;PauseMovement();return;}
            int index=activeReading*2+beatCursor+1;
            if(beatCursor==0&&index<current.beats.Length){beatCursor++;previewText=current.beats[index];return;}
            visitedMask|=1<<activeReading;
            mode=Mode.Explore;PauseMovement();
            if(visitedMask==7&&actor.position.x>ExitX-3f)
                previewText="Đã đọc các điểm nhấn. Đi tới phía phải của màn thứ ba để sang phần tiếp theo.";
        }
        bool AtExit()
        {return actor.position.x>=ExitX-1.3f && Mathf.Abs(actor.position.y+1.25f)<2f;}
        void EndEpisode()
        {
            if(visitedMask!=7)return;
            if(CiviCampaignGraph.IsDecision(current.id))
            {mode=Mode.Choice;PauseMovement();return;}
            var next=CiviCampaignGraph.Next(current.id);
            if(next==null)
            {
                finished=true;mode=Mode.Ending;PauseMovement();return;
            }
            Enter(next);
        }
        void Choose(string id)
        {
            string next=CiviCampaignGraph.Next(current.id,id); // validates option.
            if(current.id=="C5")branchI=id;
            if(current.id=="C7")branchII=id;
            if(current.id=="C9")branchIII=id;
            Enter(next);
        }
        void Enter(string id)
        {
            visitedChapters.Add(current.id);
            current=episodes[id];
            visitedMask=0;readerIndex=0;
            DrawWorld();mode=Mode.Beat;activeReading=-1;beatCursor=0;
            previewText=current.title+"\n\n"+current.beats[0];
            PauseMovement();
            Save();
        }
        void FullText()
        {
            returnMode=mode;mode=Mode.FullReader;readerIndex=0;PauseMovement();
        }
        void RestoreReader()
        {
            mode=returnMode;PauseMovement();
        }
        void Save()
        {
            try
            {
                if(current==null)return;
                var snapshot=new Snapshot{episode=current.id,decision1=branchI,
                    decision2=branchII,decision3=branchIII,visited=visitedMask,
                    readerIndex=readerIndex,x=actor.position.x,y=actor.position.y,finished=finished};
                string target=SaveFile;
                var temp=target+".tmp";
                File.WriteAllText(temp,JsonUtility.ToJson(snapshot));
                if(File.Exists(target))File.Delete(target);
                File.Move(temp,target);
            }catch(Exception ex){Debug.LogError("CIVI save failed: "+ex.Message);}
        }
        bool Load()
        {
            try
            {
                if(!File.Exists(SaveFile))return false;
                var dataSave=JsonUtility.FromJson<Snapshot>(File.ReadAllText(SaveFile));
                if(dataSave==null||!episodes.ContainsKey(dataSave.episode)||
                   dataSave.visited<0||dataSave.visited>7)
                    throw new FormatException("Invalid campaign save.");
                current=episodes[dataSave.episode];
                branchI=dataSave.decision1??"";branchII=dataSave.decision2??"";
                branchIII=dataSave.decision3??"";
                visitedMask=dataSave.visited;readerIndex=dataSave.readerIndex;
                finished=dataSave.finished;
                DrawWorld();
                actor.position=new Vector3(Mathf.Clamp(dataSave.x,-6.8f,36.5f),
                    Mathf.Clamp(dataSave.y,MinY,MaxY),0);
                physical.position=actor.position;
                follow.Snap();
                mode=finished?Mode.Ending:Mode.Explore;PauseMovement();
                return true;
            }catch(Exception ex){Debug.LogError("CIVI load failed: "+ex.Message);return false;}
        }
        void FixedUpdate()
        {
            if(physical==null||mode!=Mode.Explore)return;
            var p=physical.position;
            var q=new Vector2(Mathf.Clamp(p.x,-6.85f,36.6f),Mathf.Clamp(p.y,MinY,MaxY));
            if(q!=p){physical.position=q;physical.linearVelocity=Vector2.zero;}
        }
        void Update()
        {
            if(current==null)return;
            var k=Keyboard.current;
            if(k==null)return;
            if(k.f5Key.wasPressedThisFrame)Save();
            if(k.f9Key.wasPressedThisFrame)Load();
            if(k.rKey.wasPressedThisFrame&&mode!=Mode.FullReader&&mode!=Mode.Title)
            {FullText();return;}
            if(mode==Mode.Title)
            {
                if(k.enterKey.wasPressedThisFrame||k.spaceKey.wasPressedThisFrame)BeginCampaign();
                if(k.lKey.wasPressedThisFrame)Load();
            }
            else if(mode==Mode.Explore)
            {
                int i=NearestIndex();
                if(k.eKey.wasPressedThisFrame)
                {
                    if(i>=0)BeginBeat(i);
                    else if(AtExit()&&visitedMask==7)EndEpisode();
                }
            }
            else if(mode==Mode.Beat)
            {
                if(k.enterKey.wasPressedThisFrame||k.spaceKey.wasPressedThisFrame||k.eKey.wasPressedThisFrame)
                    NextBeat();
            }
            else if(mode==Mode.Choice)
            {
                var options=CiviCampaignGraph.Options(current.id);
                if(k.digit1Key.wasPressedThisFrame)Choose(options[0]);
                else if(k.digit2Key.wasPressedThisFrame)Choose(options[1]);
                else if(options.Length==3&&k.digit3Key.wasPressedThisFrame)Choose(options[2]);
            }
            else if(mode==Mode.FullReader)
            {
                if(k.escapeKey.wasPressedThisFrame)RestoreReader();
                else if(k.rightArrowKey.wasPressedThisFrame||k.enterKey.wasPressedThisFrame||
                    k.spaceKey.wasPressedThisFrame||k.pageDownKey.wasPressedThisFrame)
                    readerIndex=Mathf.Min(current.paragraphs.Length-1,readerIndex+1);
                else if(k.leftArrowKey.wasPressedThisFrame||k.pageUpKey.wasPressedThisFrame)
                    readerIndex=Mathf.Max(0,readerIndex-1);
            }
            else if(mode==Mode.Ending&&k.enterKey.wasPressedThisFrame)
                mode=Mode.Title;
            PauseMovement();
        }

        void Styles()
        {
            if(hero!=null)return;
            hero=new GUIStyle(GUI.skin.label){fontSize=46,fontStyle=FontStyle.Bold,
                wordWrap=true,alignment=TextAnchor.UpperLeft};
            hero.normal.textColor=new Color(.97f,.93f,.83f);
            heading=new GUIStyle(hero){fontSize=32};
            body=new GUIStyle(GUI.skin.label){fontSize=27,wordWrap=true,
                alignment=TextAnchor.UpperLeft};
            body.normal.textColor=Color.white;
            minor=new GUIStyle(body){fontSize=20};minor.normal.textColor=new Color(.76f,.87f,.86f);
            small=new GUIStyle(body){fontSize=23};small.normal.textColor=new Color(.83f,.93f,.90f);
            dim=new GUIStyle(body){fontSize=22};dim.normal.textColor=new Color(.96f,.74f,.46f);
            choiceStyle=new GUIStyle(GUI.skin.button){fontSize=24,wordWrap=true,
                alignment=TextAnchor.MiddleLeft,padding=new RectOffset(20,20,10,10)};
        }
        void Panel(float x,float y,float w,float h,float alpha=.91f)
        {
            GUI.color=new Color(.026f,.071f,.081f,alpha);
            GUI.DrawTexture(new Rect(x,y,w,h),white);GUI.color=Color.white;
        }
        void Button(float x,float y,float w,float h,string text,Action action)
        {
            if(GUI.Button(new Rect(x,y,w,h),text,choiceStyle))action();
        }
        string PlaceCaption()
        {
            if(current==null)return "";
            string chapter=current.title;
            return chapter+"  /  "+current.region;
        }
        void OnGUI()
        {
            if(white==null||current==null)return;
            Styles();
            float sx=Screen.width/1600f,sy=Screen.height/900f;
            GUI.matrix=Matrix4x4.Scale(new Vector3(sx,sy,1));
            Panel(0,0,1600,117,.95f);
            GUI.Label(new Rect(36,12,1480,52),"CIVI  /  NHỮNG NGƯỜI KHÔNG CÓ CHỖ DƯỚI BẦU TRỜI",heading);
            GUI.Label(new Rect(40,70,1500,33),PlaceCaption(),minor);
            Panel(0,805,1600,95,.95f);
            var progress= "DẤU MỐC "+(visitedMask==7?"3/3":((visitedMask&1)+((visitedMask>>1)&1)+((visitedMask>>2)&1))+"/3");
            var input=mode==Mode.Explore?
                "WASD / MŨI TÊN: ĐI     E: XEM ĐIỂM SÁNG / CỔNG RA     R: ĐỌC TOÀN VĂN":
                "ENTER/SPACE: TIẾP    1/2/3: QUYẾT ĐỊNH     R: ĐỌC TOÀN VĂN";
            GUI.Label(new Rect(34,812,1490,36),input,small);
            GUI.Label(new Rect(38,853,1480,29),progress+"       F5: LƯU     F9: TẢI     "+(mode==Mode.Explore?"CAMERA CUỘN QUA 3 KHÔNG GIAN":"ESC: QUAY LẠI TỪ TOÀN VĂN"),minor);
            if(mode==Mode.Explore)
            {
                int i=NearestIndex();
                if(i>=0)
                    GUI.Label(new Rect(1080,132,485,51),
                        "E  /  "+((visitedMask&(1<<i))!=0?"XEM LẠI":"KHÁM PHÁ")+"  ·  MÀN "+(i+1),dim);
                if(AtExit())
                    GUI.Label(new Rect(980,183,590,54),
                        visitedMask==7?"E  /  SANG PHẦN TIẾP THEO":"KHÁM PHÁ ĐỦ 3 ĐIỂM ĐỂ MỞ LỐI",dim);
                return;
            }
            if(mode==Mode.Title)
            {
                Panel(170,196,1260,538,.97f);
                GUI.Label(new Rect(220,236,1170,122),
                    "THÀNH PHỐ CÔNG BẰNG\nVÀ NHỮNG NGƯỜI BỊ BỎ QUÊN",hero);
                GUI.Label(new Rect(221,394,1140,130),
                    "Một hành trình qua Lưu Quang, Rìa Mưa, khủng hoảng năng lượng và ba lựa chọn về quyền quyết định. Bản chơi thử thống nhất: 10 chương chính, các cảnh rẽ và ba kết thúc.",body);
                Button(221,571,450,90,"BẮT ĐẦU  /  ENTER",BeginCampaign);
                Button(694,571,420,90,"TIẾP TỤC  /  L",()=>{if(!Load())BeginCampaign();});
                return;
            }
            if(mode==Mode.Beat)
            {
                Panel(122,173,1356,567,.96f);
                GUI.Label(new Rect(172,200,1260,52),
                    activeReading<0?current.title:"MÀN "+(activeReading+1)+"  ·  "+current.region,heading);
                GUI.Label(new Rect(178,293,1220,332),previewText,body);
                GUI.Label(new Rect(176,670,1240,38),
                    "ENTER / SPACE / E  →  "+(activeReading<0?"BẮT ĐẦU KHÁM PHÁ":"TIẾP TỤC CÂU CHUYỆN"),dim);
                return;
            }
            if(mode==Mode.Choice)
            {
                Panel(115,173,1370,580,.97f);
                GUI.Label(new Rect(166,208,1260,86),
                    current.id=="C5"?"ĐIỂM RẼ I  ·  TRÁCH NHIỆM VỚI NGƯỜI BỆNH":
                    current.id=="C7"?"ĐIỂM RẼ II  ·  NÓI RA HAY XÁC MINH THÊM?":
                    "ĐIỂM RẼ III  ·  AI QUYẾT ĐỊNH PHẦN ĐIỆN CÒN LẠI?",heading);
                var options=CiviCampaignGraph.Options(current.id);
                for(int i=0;i<options.Length;i++)
                {
                    string id=options[i];
                    Button(178,340+i*101,1223,78,(i+1)+".  "+CiviCampaignGraph.OptionLabel(id),()=>Choose(id));
                }
                GUI.Label(new Rect(174,681,1270,46),"Các hướng đi đều có lợi ích, thiệt hại và trách nhiệm; không có đáp án tốt/xấu tuyệt đối.",minor);
                return;
            }
            if(mode==Mode.FullReader)
            {
                Panel(108,126,1384,636,.975f);
                GUI.Label(new Rect(162,157,1240,55),"TOÀN VĂN  /  "+current.title,heading);
                GUI.Label(new Rect(166,237,1220,375),
                    current.paragraphs[Mathf.Clamp(readerIndex,0,current.paragraphs.Length-1)],body);
                GUI.Label(new Rect(166,665,1180,46),
                    "TRANG "+(readerIndex+1)+"/"+current.paragraphs.Length+
                    "    ← / → / ENTER: LẬT TRANG      ESC: ĐÓNG",dim);
                return;
            }
            if(mode==Mode.Ending)
            {
                Panel(159,167,1282,574,.97f);
                GUI.Label(new Rect(201,215,1150,96),CiviCampaignGraph.EndingLabel(current.id),hero);
                GUI.Label(new Rect(210,357,1130,195),
                    "Đã hoàn thành một tuyến của câu chuyện. Hai lựa chọn trước đó không bị xếp hạng đúng/sai."+
                    "\nNhấn R để đọc toàn văn phần kết, F5 để lưu hoặc ENTER để quay lại tựa game.",body);
                GUI.Label(new Rect(208,643,1120,42),
                    "I: "+branchI+"      II: "+branchII+"      III: "+branchIII,dim);
            }
        }
    }
}
