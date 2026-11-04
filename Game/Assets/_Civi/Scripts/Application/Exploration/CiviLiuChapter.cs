using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Civi.Preview
{
    [Serializable] public sealed class LiuTask
    {public string id,title,lead;public int room,image,requiredMask;public float x,y;public string[] actions,responses,lines;}
    [Serializable] public sealed class LiuContent
    {public string schema,source_url,source_sha256;public LiuTask[] tasks;}
    [Serializable] public sealed class LiuSave
    {
        public string version="civi-liuquang-c1-save-1";
        public string source="e2a7b0f3f257779978825d00bab44cbd0333cc56f2e958dbca24fae057e2acbd";
        public int step,mask,completed,mode,line;
        public float x=0,y=-6;
        public bool muted;
        public void Validate(LiuContent data)
        {
            if(version!="civi-liuquang-c1-save-1"||source!=data.source_sha256||step<0||step>data.tasks.Length||completed!=(1<<step)-1||mode<0||mode>2||float.IsNaN(x)||float.IsNaN(y)||float.IsInfinity(x)||float.IsInfinity(y))throw new FormatException("Save version, progress or position invalid");
            if(step==data.tasks.Length){if(mode!=0||mask!=0||line!=0)throw new FormatException("Completion state invalid");return;}
            var t=data.tasks[step];if(mask<0||(mask&~((1<<t.actions.Length)-1))!=0||line<0||line>=t.lines.Length||(mode==2&&(mask&t.requiredMask)!=t.requiredMask))throw new FormatException("Save prerequisites invalid");
            if(mode!=2&&line!=0)throw new FormatException("Unexpected dialogue bookmark");
        }
        public bool Ready(LiuTask t)=>(mask&t.requiredMask)==t.requiredMask;
        public void Observe(LiuTask t,int index)
        {if(mode!=1||index<0||index>=t.actions.Length)throw new InvalidOperationException("Not an available observation");if(t.id=="coordination"&&index>=2)mask^=1<<index;else mask|=1<<index;}
        public bool Commit(LiuTask t){if(mode!=1||!Ready(t))return false;mode=2;line=0;return true;}
        public bool Advance(LiuContent data)
        {if(mode!=2)return false;if(++line<data.tasks[step].lines.Length)return false;completed|=1<<step;step++;mask=0;line=0;mode=0;return true;}
    }
    [DefaultExecutionOrder(50)]
    public sealed partial class CiviLiuChapter : MonoBehaviour
    {
        public CiviSchoolStudioV2 Movement;
        public CiviLiuEnvironment Environment;
        public Font InterfaceFont;
        public TextAsset ChapterData;
        LiuContent content; LiuSave state=new LiuSave();
        bool help=true,transitioning;float fade,noticeUntil;string feedback="",notice="";int actionFrame=-1;
        string qaDir;bool qa;AudioSource ambience,fx;AudioClip clickSound;
        public LiuTask Task=>state.step<content.tasks.Length?content.tasks[state.step]:null;
        public bool Complete=>Task==null;
        public string SavePath=>qa?Path.Combine(qaDir,"c1-save.json"):Path.Combine(Application.persistentDataPath,"civi-liuquang-c1-v1.json");
        Vector2 Target=>Task==null?new Vector2(3,-2):new Vector2(Task.x,Task.y);
        float Distance=>Vector2.Distance(Movement.Player.position,Target);
        bool Locked=>help||transitioning||Complete||state.mode!=0||state.step==0;
        void Start()
        {
            content=JsonUtility.FromJson<LiuContent>(ChapterData.text);ValidateContent(content);
            var args=System.Environment.GetCommandLineArgs();qa=args.Contains("-civi-qa");int n=Array.IndexOf(args,"-civi-qa-dir");qaDir=n>=0&&n+1<args.Length?args[n+1]:Path.Combine(Application.persistentDataPath,"LiuQuangQA");if(qa)Directory.CreateDirectory(qaDir);
            Environment.Build();Movement.Boot();Camera.main.GetComponent<CiviFollowCamera2D>().VerticalComposition=4f;CreateInterface();CreateAudio();SetPlayerVisibility();Lock();RenderUI();
        }
        public static void ValidateContent(LiuContent c)
        {
            if(c==null||c.schema!="civi-liuquang-c1-1"||c.tasks==null||c.tasks.Length!=13)throw new FormatException("C1 content schema");
            string[] ids={"memory","hospital","evidence","coordination","driver","gallery","lunch","corridor","meeting","mother","album","request","final"};
            for(int i=0;i<ids.Length;i++){var t=c.tasks[i];if(t.id!=ids[i]||t.actions.Length!=t.responses.Length||t.lines.Length<1||t.actions.Length>6||t.room<0||t.room>5)throw new FormatException("C1 task "+i);}
        }
        void Lock(){Movement.ExternalLock=Locked;}
        bool Once(){if(actionFrame==Time.frameCount||transitioning)return false;actionFrame=Time.frameCount;return true;}
        void Begin(){if(!Once())return;help=false;if(state.step==0)state.mode=1;Lock();RenderUI();}
        void OpenTask(){if(!Once()||Locked||Distance>2.2f)return;state.mode=1;feedback="";Lock();RenderUI();Tone();}
        void Observe(int i){if(!Once()||help||state.mode!=1)return;state.Observe(Task,i);feedback=Task.responses[i];if(Task.id=="coordination"&&i>=2&&(state.mask&(1<<i))==0)feedback="Đã bỏ phương án này. Ràng buộc tương ứng vẫn cần được giải quyết.";Tone();Save(false);RenderUI();}
        void Submit()
        {
            if(!Once()||help)return;
            if(!state.Commit(Task)){feedback=Task.id=="coordination"?"Phương án cần bảo vệ cả hai nơi." : "Cần kiểm tra đủ các mục trước khi xác nhận.";if(Task.id=="coordination")feedback="Chưa đủ: thuốc phải được giữ lạnh liên tục, chuyển đến kho dự phòng bằng chuyến ưu tiên; giải phóng điện cho trẻ và báo tài xế trước 15 phút.";RenderUI();return;}
            feedback="";Tone();Save(false);Lock();RenderUI();
        }
        void Next()
        {
            if(!Once()||help||state.mode!=2)return;
            int oldRoom=Task.room;bool changed=state.Advance(content);feedback="";
            if(changed&&Task!=null&&Task.room!=oldRoom){StartCoroutine(Travel(Task.room));return;}
            if(changed&&state.step==1){StartCoroutine(Travel(1));return;}
            Save(false);SetPlayerVisibility();Lock();RenderUI();
        }
        IEnumerator Travel(int room)
        {
            transitioning=true;Lock();RenderUI();for(float t=0;t<.35f;t+=Time.unscaledDeltaTime){fade=t/.35f;yield return null;}
            Environment.ShowRoom(room);Teleport(Environment.Spawn);SetPlayerVisibility();UpdateAmbient();
            // Let the retained follow camera settle behind the opaque fade.
            fade=1;yield return new WaitForSecondsRealtime(.3f);
            for(float t=0;t<.35f;t+=Time.unscaledDeltaTime){fade=1-t/.35f;yield return null;}
            fade=0;transitioning=false;Lock();Save(false);RenderUI();
        }
        void Teleport(Vector2 p){var body=Movement.Player.GetComponent<Rigidbody2D>();body.position=p;Movement.Player.position=p;}
        void SetPlayerVisibility(){foreach(var sr in Movement.Player.GetComponentsInChildren<SpriteRenderer>())if(sr.gameObject!=Movement.Player.gameObject)sr.enabled=state.step!=0;}
        void Save(bool notify)
        {
            if(transitioning)return;
            try{state.x=Movement.Player.position.x;state.y=Movement.Player.position.y;state.Validate(content);var temp=SavePath+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(state,true));if(File.Exists(SavePath))File.Replace(temp,SavePath,SavePath+".bak");else File.Move(temp,SavePath);if(notify)Notice("Đã lưu tiến độ Chương 1.");}
            catch(Exception e){Notice("Không lưu được. Tiến độ trong phiên vẫn còn.");Debug.LogWarning("C1 save: "+e.Message);}
        }
        void Load()
        {
            if(!Once())return;
            try
            {
                if(!File.Exists(SavePath)){Notice("Chưa có bản lưu cho Chương 1 này.");return;}
                var candidate=JsonUtility.FromJson<LiuSave>(File.ReadAllText(SavePath));candidate.Validate(content);state=candidate;
                Environment.ShowRoom(Task==null?5:Task.room);Teleport(Environment.CanStand(new Vector2(state.x,state.y))?new Vector2(state.x,state.y):Environment.Spawn);
                help=false;if(state.step==0&&state.mode==0)state.mode=1;feedback="";SetPlayerVisibility();UpdateAmbient();Lock();RenderUI();Notice("Đã tiếp tục đúng tiến độ đã lưu.");
            }
            catch(Exception e){Notice("Bản lưu không hợp lệ; phiên hiện tại được giữ nguyên.");Debug.LogWarning("C1 load: "+e.Message);}
        }
        void Notice(string text){notice=text;noticeUntil=Time.unscaledTime+3;}
        void Update()
        {
            if(content==null||!Movement.IsReady)return;
            var k=Keyboard.current;
            if(k!=null&&!transitioning)
            {
                if(k.f5Key.wasPressedThisFrame)Save(true);
                else if(k.f9Key.wasPressedThisFrame)Load();
                else if(k.hKey.wasPressedThisFrame){help=!help;if(!help&&state.step==0&&state.mode==0)state.mode=1;Lock();RenderUI();}
                else if(k.mKey.wasPressedThisFrame){state.muted=!state.muted;UpdateAmbient();Notice(state.muted?"Âm thanh đã tắt.":"Âm thanh đã bật.");}
                else if(k.escapeKey.wasPressedThisFrame){if(help){help=false;if(state.step==0)state.mode=1;}else if(state.mode==1&&state.step>0)state.mode=0;else help=true;Lock();RenderUI();}
                else if(!help&&!Complete)
                {
                    if(state.mode==0&&k.eKey.wasPressedThisFrame)OpenTask();
                    else if(state.mode==2&&(k.enterKey.wasPressedThisFrame||k.spaceKey.wasPressedThisFrame))Next();
                    else if(state.mode==1){if(k.enterKey.wasPressedThisFrame)Submit();else for(int i=0;i<Task.actions.Length;i++)if(k[(Key)((int)Key.Digit1+i)].wasPressedThisFrame){Observe(i);break;}}
                }
                if(qa&&k.f12Key.wasPressedThisFrame)ScreenCapture.CaptureScreenshot(Path.Combine(qaDir,"capture-"+DateTime.UtcNow.Ticks+".png"));
            }
            TickUI();if(qa)Telemetry();
        }
        void CreateAudio()
        {
            ambience=gameObject.AddComponent<AudioSource>();ambience.loop=true;ambience.volume=.07f;
            var data=new float[48000*4];for(int i=0;i<data.Length;i++){float t=i/48000f;data[i]=.15f*Mathf.Sin(2*Mathf.PI*110*t)+.07f*Mathf.Sin(2*Mathf.PI*165*t)+.035f*Mathf.Sin(2*Mathf.PI*220*t)*(1+Mathf.Sin(2*Mathf.PI*.25f*t));}
            var hum=AudioClip.Create("Dome machinery / original synthesis",data.Length,1,48000,false);hum.SetData(data,0);ambience.clip=hum;ambience.Play();
            var note=new float[4800];for(int i=0;i<note.Length;i++)note[i]=.13f*Mathf.Sin(i*2*Mathf.PI*660/48000)*Mathf.Pow(1-i/(float)note.Length,2);
            clickSound=AudioClip.Create("Confirmation / original synthesis",note.Length,1,48000,false);clickSound.SetData(note,0);fx=gameObject.AddComponent<AudioSource>();UpdateAmbient();
        }
        void UpdateAmbient(){if(ambience==null)return;ambience.mute=state.muted;fx.mute=state.muted;ambience.volume=Environment.Room==5?.025f:.07f;ambience.pitch=Environment.Room==0?.8f:Environment.Room==5?.75f:1;}
        void Tone(){if(fx!=null)fx.PlayOneShot(clickSound);}
        void OnApplicationQuit(){if(content!=null&&!help&&!transitioning)Save(false);}
    }
}
