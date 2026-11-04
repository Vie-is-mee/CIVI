using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Civi.Preview.Editor
{
    public static class CiviLiuBuilder
    {
        public const string Scene="Assets/_Civi/Scenes/CIVI-LiuQuang-Chapter1.unity";
        const string Art="Assets/_Civi/Art/LiuQuangC1/";
        static Texture2D Texture(string name)
        {
            var path=Art+name;var t=(TextureImporter)AssetImporter.GetAtPath(path);if(t==null)throw new Exception("Missing "+path);
            t.textureType=TextureImporterType.Default;t.npotScale=TextureImporterNPOTScale.None;t.alphaIsTransparency=true;t.isReadable=true;t.mipmapEnabled=false;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.Uncompressed;t.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        [MenuItem("CIVI/Lưu Quang C1/Prepare, verify and build")]
        public static void Build()
        {
            Prepare();Verify();
            PlayerSettings.runInBackground=true;PlayerSettings.resizableWindow=true;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;
            var exe=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"release","CIVI_LiuQuang_C1.exe");Directory.CreateDirectory(Path.GetDirectoryName(exe));
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Scene},locationPathName=exe,target=BuildTarget.StandaloneWindows,options=BuildOptions.None});
            if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Windows build "+result.summary.result);
            Debug.Log("LIUQUANG WINDOWS BUILD PASS "+exe);
        }
        public static void Prepare()
        {
            var cast=Texture("cast-v2.png");var walk=Texture("walk-v2.png");var props=Texture("props.png");var support=Texture("support.png");var evidence=Texture("evidence.png");var city=Texture("city-window.png");
            if(!File.Exists(Scene))
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);camera.GetComponent<Camera>().orthographic=true;
                var env=new GameObject("Lưu Quang modular environment",typeof(CiviLiuEnvironment)).GetComponent<CiviLiuEnvironment>();env.Interior=true;env.CastAtlas=cast;env.WalkAtlas=walk;env.PropAtlas=props;env.SupportAtlas=support;env.EvidenceAtlas=evidence;env.CityAtlas=city;env.LabelFont=AssetDatabase.LoadAssetAtPath<Font>("Assets/_Civi/Fonts/CiviOpenFont.ttf");
                var move=new GameObject("Polished movement / retained",typeof(CiviSchoolStudioV2)).GetComponent<CiviSchoolStudioV2>();move.ChapterMode=true;move.ChapterEnvironment=env;move.ActorLabel="Mai / adult coordinator";move.ExternalLock=true;
                var chapter=new GameObject("C1 story and workbench",typeof(CiviLiuChapter)).GetComponent<CiviLiuChapter>();chapter.Movement=move;chapter.Environment=env;chapter.InterfaceFont=env.LabelFont;chapter.ChapterData=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Civi/Resources/LiuQuangC1/chapter.json");
                EditorSceneManager.SaveScene(scene,Scene);
            }
            var current=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var currentEnv=UnityEngine.Object.FindAnyObjectByType<CiviLiuEnvironment>();currentEnv.CastAtlas=cast;currentEnv.WalkAtlas=walk;currentEnv.CityAtlas=city;EditorSceneManager.SaveScene(current);
            var all=EditorBuildSettings.scenes.Where(s=>s.path!=Scene).Select(s=>new EditorBuildSettingsScene(s.path,false)).ToList();all.Insert(0,new EditorBuildSettingsScene(Scene,true));EditorBuildSettings.scenes=all.ToArray();AssetDatabase.SaveAssets();
        }
        static void Check(bool condition,string name){if(!condition)throw new Exception("C1 check failed: "+name);checks++;}static int checks;
        public static void Verify()
        {
            checks=0;var scene=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,"missing script");
            var c=UnityEngine.Object.FindAnyObjectByType<CiviLiuChapter>();var data=JsonUtility.FromJson<LiuContent>(c.ChapterData.text);CiviLiuChapter.ValidateContent(data);
            var source=Resources.Load<TextAsset>("LiuQuangC1/dialogue");
            Check(source!=null,"compiled reviewed dialogue exists");
            var dlg=JsonUtility.FromJson<LiuDialogueContent>(source.text);LiuDialogueContent.Validate(dlg,data);
            Check(dlg.pages.Length==154&&dlg.segments.Length==13,"complete canonical dialogue");
            var env=c.Environment;env.Build();Check(env.Frames.Length==16&&env.Frames.All(s=>s!=null),"16 walking frames");
            Check(env.WalkFramesSix!=null&&env.WalkFramesSix.Length==24&&env.WalkFramesSix.All(s=>s!=null),"24 walking poses");
            Check(env.Cast.Length==4&&env.Cast.All(s=>s!=null),"adult NPC cast");
            foreach(var task in data.tasks)
            {
                env.ShowRoom(task.room);Check(env.CanStand(env.Spawn),"spawn "+task.id);Check(!env.CanStand(new Vector2(100,100)),"boundary");
                foreach(var r in env.Solids)Check(!env.CanStand(r.center),"solid");Reach(env,env.Spawn,new Vector2(task.x,task.y));
                // Exhaust all subsets: no early approval; all prerequisite combinations roundtrip.
                for(int mask=0;mask<(1<<task.actions.Length);mask++)
                {var s=new LiuSave{step=Array.IndexOf(data.tasks,task),completed=(1<<Array.IndexOf(data.tasks,task))-1,mask=mask,mode=1};s.Validate(data);var clone=JsonUtility.FromJson<LiuSave>(JsonUtility.ToJson(s));clone.Validate(data);Check(clone.mask==mask,"save mask");Check(s.Commit(task)==((mask&task.requiredMask)==task.requiredMask),"prerequisite gate "+task.id);}
            }
            var full=new LiuSave();for(int i=0;i<data.tasks.Length;i++)
            {full.mode=1;for(int j=data.tasks[i].actions.Length-1;j>=0;j--)full.Observe(data.tasks[i],j);Check(full.Commit(data.tasks[i]),"reverse-order actions");for(int j=0;j<data.tasks[i].lines.Length;j++){full.Validate(data);full.Advance(data);}Check(full.step==i+1,"single event progression");full.Validate(data);}
            Check(full.step==13&&full.completed==8191,"chapter end has no C2 route");
            // Complete each reviewed before-action and after-action cue in the
            // retained 13-task progression, without bypassing prerequisites.
            var integrated=new LiuSave{dialogueVersion=1};
            for(int i=0;i<data.tasks.Length;i++)
            {
                int before=dlg.Count(i,3),after=dlg.Count(i,2);
                for(int line=0;line<before;line++)
                {integrated.mode=3;integrated.line=line;integrated.Validate(data,dlg);Check(dlg.At(i,3,line)!=null,"authored before-action cue");}
                integrated.mode=1;integrated.line=0;
                for(int j=0;j<data.tasks[i].actions.Length;j++)integrated.Observe(data.tasks[i],j);
                Check(integrated.Commit(data.tasks[i]),"reviewed dialogue prerequisites "+i);
                for(int line=0;line<after;line++)
                {
                    integrated.Validate(data,dlg);
                    Check(dlg.At(i,2,line)!=null,"authored after-action cue");
                    integrated.Advance(data,after);
                }
                Check(integrated.step==i+1,"reviewed dialogue task progression "+i);
            }
            integrated.Validate(data,dlg);
            Check(integrated.step==13&&integrated.completed==8191,"integrated C1 completion");
            var migrated=new LiuSave{step=3,completed=7,mode=2,line=0,mask=data.tasks[3].requiredMask};
            migrated.Validate(data);migrated.dialogueVersion=1;migrated.Validate(data,dlg);
            Check(migrated.step==3&&migrated.completed==7,"legacy checkpoint preserved");
            var bad=new[]{new LiuSave{version="old"},new LiuSave{step=4},new LiuSave{step=14},new LiuSave{mode=2},new LiuSave{x=float.NaN}};
            foreach(var s in bad){bool rejected=false;try{s.Validate(data);}catch(FormatException){rejected=true;}Check(rejected,"invalid save rejected");}
            EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);Debug.Log("LIUQUANG EDITMODE PASS "+checks+" checks; all prerequisite subsets, reverse observation order, C1 completion, save validation and radius-safe navigation.");
        }
        static void Reach(CiviLiuEnvironment e,Vector2 a,Vector2 b)
        {var start=Vector2Int.RoundToInt(a*2);var queue=new Queue<Vector2Int>();var seen=new HashSet<Vector2Int>();queue.Enqueue(start);seen.Add(start);while(queue.Count>0){var p=queue.Dequeue();if(Vector2.Distance((Vector2)p*.5f,b)<1.5f){checks++;return;}foreach(var d in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right}){var n=p+d;if(!seen.Contains(n)&&e.CanStand((Vector2)n*.5f)){seen.Add(n);queue.Enqueue(n);}}}throw new Exception("Unreachable "+b+" room "+e.Room);}
        public static void VerifyLegacy()
        {
            checks=0;
            foreach(string path in new[]{"Assets/_Civi/Scenes/CIVI-School-DesignStudio-V2.unity","Assets/_Civi/Scenes/CIVI-Chapter1-DesignRoom.unity"})
            {
                EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                var movement=UnityEngine.Object.FindAnyObjectByType<CiviSchoolStudioV2>();
                Check(movement!=null&&movement.ChapterEnvironment==null,"legacy environment remains optional");
                movement.Boot();Check(movement.IsReady&&movement.HasCamera,"legacy boot and camera");
                Check(movement.Walkable(movement.Player.position),"legacy spawn");
                Check(movement.StudentFrames.Length==16&&movement.StudentFrames.All(x=>x!=null),"legacy frames");
                var camera=Camera.main.GetComponent<CiviFollowCamera2D>();Check(camera.MinZoom==6.5f&&camera.MaxZoom==9.4f,"legacy camera limits");
            }
            EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
            Debug.Log("LIUQUANG LEGACY REGRESSION PASS "+checks+" checks in two original scenes.");
        }
    }
}
