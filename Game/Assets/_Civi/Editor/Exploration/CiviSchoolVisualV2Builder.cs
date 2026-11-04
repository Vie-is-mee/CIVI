using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Civi.Preview.Editor
{
    public static class CiviSchoolVisualV2Builder
    {
        const string Art="Assets/_Civi/Art/SchoolVisualV2/";
        public const string Campus="Assets/_Civi/Scenes/CIVI-School-VisualV2.unity";
        public const string Studio="Assets/_Civi/Scenes/CIVI-School-DesignStudio-V2.unity";
        static readonly string[] dirs={"front","back","left","right"};
        static Sprite Sprite(string name,bool actor=false)
        {
            string path=Art+name;
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null)throw new InvalidOperationException("Missing art: "+path);
            if(importer.textureType!=TextureImporterType.Sprite||
                importer.maxTextureSize<4096||importer.filterMode!=FilterMode.Point)
            {
                importer.textureType=TextureImporterType.Sprite;
                importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=16;
                importer.filterMode=FilterMode.Point;
                importer.wrapMode=TextureWrapMode.Clamp;
                importer.maxTextureSize=4096;
                importer.mipmapEnabled=false;
                importer.SaveAndReimport();
            }
            var result=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(result==null)throw new InvalidOperationException("Sprite did not import "+path);
            return result;
        }
        static Sprite[] Frames()
        {
            return dirs.SelectMany(x=>Enumerable.Range(0,4)
                .Select(i=>Sprite("student_"+x+"_"+i+".png",true))).ToArray();
        }
        static void NewCamera(float zoom)
        {
            var go=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));
            go.tag="MainCamera";
            go.transform.position=new Vector3(0,0,-10);
            var camera=go.GetComponent<Camera>();
            camera.orthographic=true;camera.orthographicSize=zoom;
            camera.backgroundColor=new Color(.11f,.17f,.17f);
        }

        [MenuItem("CIVI/School World V2/Build Visual V2")]
        public static void Build()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Campus)!=null||
               AssetDatabase.LoadAssetAtPath<SceneAsset>(Studio)!=null)
                throw new InvalidOperationException("V2 scene exists; refusing to overwrite. Use Open Campus.");
            var map=AssetDatabase.LoadAssetAtPath<TextAsset>(Art+"school_world.json");
            var room=AssetDatabase.LoadAssetAtPath<TextAsset>(Art+"studio_world.json");
            if(map==null||room==null)throw new InvalidOperationException("Map data missing");
            var frames=Frames();
            var exterior=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            NewCamera(10.45f);
            var root=new GameObject("CIVI / Seamless School Campus / V2",typeof(CiviSchoolWorldV2));
            var c=root.GetComponent<CiviSchoolWorldV2>();
            c.MapJson=map;c.WalkFrames=frames;
            c.Ground=Sprite("school_ground.png");
            c.Architecture=Sprite("school_architecture.png");
            c.Props=Sprite("school_props.png");
            c.Foreground=Sprite("school_foreground.png");
            EditorSceneManager.SaveScene(exterior,Campus);
            var interior=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            NewCamera(7.65f);
            var roomRoot=new GameObject("CIVI / Design Studio / V2",typeof(CiviSchoolStudioV2));
            var r=roomRoot.GetComponent<CiviSchoolStudioV2>();
            r.RoomJson=room;
            r.Ground=Sprite("studio_ground.png");
            r.Architecture=Sprite("studio_architecture.png");
            r.Props=Sprite("studio_props.png");
            r.StudentFrames=frames;
            EditorSceneManager.SaveScene(interior,Studio);
            var old=EditorBuildSettings.scenes.ToList();
            foreach(var oldScene in old)oldScene.enabled=false;
            old.Insert(0,new EditorBuildSettingsScene(Studio,true));
            old.Insert(0,new EditorBuildSettingsScene(Campus,true));
            EditorBuildSettings.scenes=old.ToArray();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Campus,OpenSceneMode.Single);
            Debug.Log("CIVI SCHOOL VISUAL V2 CREATED: outdoor seamless map + studio door scene");
        }
        [MenuItem("CIVI/School World V2/Open Campus")]
        public static void Open()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Campus)==null)Build();
            else EditorSceneManager.OpenScene(Campus,OpenSceneMode.Single);
        }
        [MenuItem("CIVI/School World V2/Smoke Test")]
        public static void Smoke()
        {
            EditorSceneManager.OpenScene(Campus,OpenSceneMode.Single);
            var campus=UnityEngine.Object.FindAnyObjectByType<CiviSchoolWorldV2>();
            if(campus==null)throw new Exception("School campus V2 missing");
            campus.Boot();
            if(!campus.IsBooted||campus.HotspotCount!=6||campus.Width!=152||
                campus.Height!=60||campus.Player==null||!campus.IsWalkable(campus.Player.position))
                throw new Exception("V2 campus boot/nav failed");
            if(Camera.main.GetComponent<CiviFollowCamera2D>().Target!=campus.Player)
                throw new Exception("V2 camera lost player");
            EditorSceneManager.OpenScene(Studio,OpenSceneMode.Single);
            var studio=UnityEngine.Object.FindAnyObjectByType<CiviSchoolStudioV2>();
            if(studio==null)throw new Exception("Design studio scene missing");
            studio.Boot();
            if(!studio.IsReady||studio.Width!=32||studio.Player==null||
                !studio.Walkable(studio.Player.position)||!studio.HasCamera)
                throw new Exception("Design studio boot or nav failed");
            var build=EditorBuildSettings.scenes.Where(x=>x.enabled).Select(x=>x.path).ToArray();
            if(build.Length!=2||build[0]!=Campus||build[1]!=Studio)
                throw new Exception("Build Settings not correctly wired for scene transitions.");
            Debug.Log("CIVI SCHOOL VISUAL V2 SMOKE PASS: 2 scenes, 6 locations, 16-frame animated student, doors and camera");
        }
        [MenuItem("CIVI/School World V2/Build Windows Test")]
        public static void BuildWindows()
        {
            var path=System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(Application.dataPath),
                "release","CIVI_SchoolWorld_V2_x86.exe");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=new[]{Campus,Studio},locationPathName=path,
                target=BuildTarget.StandaloneWindows,
                options=BuildOptions.None
            });
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Windows player build failed "+report.summary.result);
            Debug.Log("CIVI SCHOOL V2 WINDOWS PASS "+path);
        }
    }
}
