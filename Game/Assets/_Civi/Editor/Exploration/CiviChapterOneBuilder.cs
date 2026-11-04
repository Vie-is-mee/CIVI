using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Civi.Preview;

namespace Civi.Preview.Editor
{
    public static class CiviChapterOneBuilder
    {
        public const string Courtyard="Assets/_Civi/Scenes/CIVI-Chapter1-Courtyard.unity";
        public const string Studio="Assets/_Civi/Scenes/CIVI-Chapter1-DesignRoom.unity";
        const string SourceCampus="Assets/_Civi/Scenes/CIVI-School-VisualV2.unity";
        const string SourceDesign="Assets/_Civi/Scenes/CIVI-School-DesignStudio-V2.unity";
        const string Content="Assets/_Civi/Content/Story/v4_1/";
        const string Art="Assets/_Civi/Art/Chapter1/";
        static readonly string[] NpcNames={"nhi","ha","minh"};
        static readonly string[] Photos={"PHOTO_S00_BAD_01","PHOTO_S00_BAD_02","PHOTO_S00_GOOD","PHOTO_0443"};

        static T Asset<T>(string path) where T:UnityEngine.Object
        {
            var asset=AssetDatabase.LoadAssetAtPath<T>(path);
            if(asset==null)throw new InvalidOperationException("Missing Chapter 1 asset "+path);
            return asset;
        }
        static Sprite Npc(string id)
        {
            string path=Art+"npc_"+id+".png";
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null)throw new InvalidOperationException("No NPC sprite at "+path);
            if(importer.textureType!=TextureImporterType.Sprite ||
                Mathf.Abs(importer.spritePixelsPerUnit-16)>.01f||
                importer.filterMode!=FilterMode.Point)
            {
                importer.textureType=TextureImporterType.Sprite;
                importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=16;
                importer.filterMode=FilterMode.Point;
                importer.mipmapEnabled=false;
                importer.SaveAndReimport();
            }
            return Asset<Sprite>(path);
        }
        static void Wire(CiviChapterOne controller,bool studio)
        {
            controller.IsDesignRoom=studio;
            controller.S00Scene=Asset<TextAsset>(Content+"scenes/S00.json");
            controller.S00Dialogue=Asset<TextAsset>(Content+"dialogue/S00.json");
            controller.S01Scene=Asset<TextAsset>(Content+"scenes/S01.json");
            controller.S01Dialogue=Asset<TextAsset>(Content+"dialogue/S01.json");
            controller.S02Scene=Asset<TextAsset>(Content+"scenes/S02.json");
            controller.S02Dialogue=Asset<TextAsset>(Content+"dialogue/S02.json");
            controller.InitialState=Asset<TextAsset>(Content+"initial-state.json");
            controller.NpcSprites=NpcNames.Select(Npc).ToArray();
            controller.EvidencePhotos=Photos.Select(n=>Asset<Texture2D>(
                "Assets/_Civi/Art/Photos/"+n+".png")).ToArray();
            controller.ValidateSources();
        }

        [MenuItem("CIVI/Chapter 1/Create Playable S00-S01")]
        public static void Build()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Courtyard)!=null||
                AssetDatabase.LoadAssetAtPath<SceneAsset>(Studio)!=null)
                throw new InvalidOperationException("Chapter scenes already exist. Refusing to overwrite.");

            var scene=EditorSceneManager.OpenScene(SourceCampus,OpenSceneMode.Single);
            var world=UnityEngine.Object.FindAnyObjectByType<CiviSchoolWorldV2>();
            if(world==null)throw new InvalidOperationException("Campus prefab scene missing");
            world.ChapterMode=true;
            var owner=new GameObject("CHAPTER 1 / S00 authored runtime",typeof(CiviChapterOne));
            var controller=owner.GetComponent<CiviChapterOne>();
            controller.Campus=world;
            Wire(controller,false);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene,Courtyard,true);

            scene=EditorSceneManager.OpenScene(SourceDesign,OpenSceneMode.Single);
            var design=UnityEngine.Object.FindAnyObjectByType<CiviSchoolStudioV2>();
            if(design==null)throw new InvalidOperationException("Studio prefab scene missing");
            design.ChapterMode=true;
            owner=new GameObject("CHAPTER 1 / S01 authored runtime",typeof(CiviChapterOne));
            controller=owner.GetComponent<CiviChapterOne>();
            controller.Studio=design;
            Wire(controller,true);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene,Studio,true);

            var old=EditorBuildSettings.scenes.ToList();
            foreach(var x in old)x.enabled=false;
            old.Insert(0,new EditorBuildSettingsScene(Studio,true));
            old.Insert(0,new EditorBuildSettingsScene(Courtyard,true));
            EditorBuildSettings.scenes=old.ToArray();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Courtyard,OpenSceneMode.Single);
            Debug.Log("CIVI CHAPTER 1 CREATED: 2 playable authored environments, S00/S01 StorySession.");
        }
        [MenuItem("CIVI/Chapter 1/Open Chapter 1")]
        public static void Open()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Courtyard)==null)Build();
            else EditorSceneManager.OpenScene(Courtyard,OpenSceneMode.Single);
        }
        public static void Smoke()
        {
            var scenes=EditorBuildSettings.scenes.Where(x=>x.enabled).ToArray();
            if(scenes.Length!=2||scenes[0].path!=Courtyard||scenes[1].path!=Studio)
                throw new Exception("Chapter 1 Build Settings not limited to both Chapter 1 scenes");
            EditorSceneManager.OpenScene(Courtyard,OpenSceneMode.Single);
            var controller=UnityEngine.Object.FindAnyObjectByType<CiviChapterOne>();
            if(controller==null||controller.IsDesignRoom)throw new Exception("No S00 controller");
            controller.ValidateSources();
            controller.SelfTest();
            controller.Boot();
            if(!controller.IsReady||!controller.HasOpening||controller.PhaseName!="Arrival")
                throw new Exception("S00 gameplay boot failed");
            var courtyard=UnityEngine.Object.FindAnyObjectByType<CiviSchoolWorldV2>();
            if(!courtyard.IsBooted||!courtyard.IsWalkable(courtyard.Player.position))
                throw new Exception("Chapter courtyard not walkable");

            EditorSceneManager.OpenScene(Studio,OpenSceneMode.Single);
            controller=UnityEngine.Object.FindAnyObjectByType<CiviChapterOne>();
            if(controller==null||!controller.IsDesignRoom)throw new Exception("No S01 controller");
            controller.ValidateSources();
            controller.Boot();
            if(!controller.IsReady||controller.PhaseName!="StudioArrival")
                throw new Exception("Studio gameplay boot failed");
            var studio=UnityEngine.Object.FindAnyObjectByType<CiviSchoolStudioV2>();
            if(!studio.IsReady||!studio.Walkable(studio.Player.position))
                throw new Exception("Studio spawn blocked");
            Debug.Log("CIVI CHAPTER 1 SMOKE PASS: S00/S01 authoring, 27 choices routes, " +
                "camera, props, 3 NPC, photo refs, save/restore, 2 scenes.");
        }
        public static void BuildWindows()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Courtyard)==null||
                AssetDatabase.LoadAssetAtPath<SceneAsset>(Studio)==null)
                throw new Exception("Chapter scene missing");
            string basePath=System.IO.Path.GetDirectoryName(Application.dataPath);
            string exe=System.IO.Path.Combine(basePath,"release","CIVI_Chapter1_x86.exe");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(exe));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=new[]{Courtyard,Studio},
                locationPathName=exe,
                target=BuildTarget.StandaloneWindows,
                options=BuildOptions.None});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Chapter1 build "+report.summary.result);
            Debug.Log("CIVI CHAPTER 1 WINDOWS PASS "+exe);
        }
    }
}
