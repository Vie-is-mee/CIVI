using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Civi.Preview.Editor
{
    public static class CiviSchoolWorldBuilder
    {
        public const string ScenePath="Assets/_Civi/Scenes/CIVI-School-SeamlessCampus.unity";
        const string Art="Assets/_Civi/Art/SchoolWorld/";
        static Sprite Sprite(string name)
        {
            string path=Art+name;
            var imp=AssetImporter.GetAtPath(path) as TextureImporter;
            if(imp==null)throw new InvalidOperationException("Missing school tile art: "+path);
            if(imp.textureType!=TextureImporterType.Sprite||
                Mathf.Abs(imp.spritePixelsPerUnit-16f)>.01f||
                imp.filterMode!=FilterMode.Point||imp.maxTextureSize<4096)
            {
                imp.textureType=TextureImporterType.Sprite;
                imp.spriteImportMode=SpriteImportMode.Single;
                imp.spritePixelsPerUnit=16;
                imp.filterMode=FilterMode.Point;
                imp.mipmapEnabled=false;
                imp.maxTextureSize=4096;
                imp.wrapMode=TextureWrapMode.Clamp;
                imp.SaveAndReimport();
            }
            var asset=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(asset==null)throw new InvalidOperationException("Could not import school sprite "+path);
            return asset;
        }

        [MenuItem("CIVI/School World/Create Seamless Campus")]
        public static void Build()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)!=null)
                throw new InvalidOperationException("Refusing overwrite of connected school map.");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camGo=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));
            camGo.tag="MainCamera";camGo.transform.position=new Vector3(0,0,-10);
            var cam=camGo.GetComponent<Camera>();
            cam.orthographic=true;cam.orthographicSize=8.0f;
            cam.backgroundColor=new Color(.10f,.16f,.16f);

            var root=new GameObject("CIVI / Connected School Campus",typeof(CiviSchoolWorld));
            var game=root.GetComponent<CiviSchoolWorld>();
            game.MapJson=AssetDatabase.LoadAssetAtPath<TextAsset>(Art+"school_world.json");
            if(game.MapJson==null)throw new InvalidOperationException("Missing navigation JSON.");
            game.Ground=Sprite("school_ground.png");
            game.Architecture=Sprite("school_architecture.png");
            game.Props=Sprite("school_props.png");
            game.Foreground=Sprite("school_foreground.png");
            game.PlayerFront=Sprite("an_front.png");
            game.PlayerBack=Sprite("an_back.png");
            game.PlayerLeft=Sprite("an_left.png");
            game.PlayerRight=Sprite("an_right.png");
            EditorSceneManager.SaveScene(scene,ScenePath);
            var old=EditorBuildSettings.scenes.ToList();
            // The standalone school test is the launch target; other scenes are
            // still preserved and may be reenabled by the team when narrative is settled.
            foreach(var entry in old)entry.enabled=false;
            old.Insert(0,new EditorBuildSettingsScene(ScenePath,true));
            EditorBuildSettings.scenes=old.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("CIVI SCHOOL MAP CREATED: 152x60 modular tiles, no stitched scene backgrounds.");
        }

        [MenuItem("CIVI/School World/Open Seamless Campus")]
        public static void Open()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)==null)
                Build();
            else EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        }

        // EditMode smoke; does not prove physical keyboard PlayMode feel.
        public static void Smoke()
        {
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var game=UnityEngine.Object.FindAnyObjectByType<CiviSchoolWorld>();
            if(game==null)throw new Exception("Connected campus component missing");
            game.Boot();
            if(!game.IsBooted||game.HotspotCount!=6||
                game.Width!=152||game.Height!=60||game.Player==null)
                throw new Exception("Modular campus boot mismatch.");
            if(!game.IsWalkable(game.Player.position))
                throw new Exception("Player spawn unwalkable.");
            if(Camera.main.GetComponent<CiviFollowCamera2D>()==null||
                Camera.main.GetComponent<CiviFollowCamera2D>().Target!=game.Player)
                throw new Exception("Camera follow not configured.");
            if(game.GetComponentsInChildren<SpriteRenderer>().Count()<12)
                throw new Exception("Missing layered art or hotspots");
            Debug.Log("CIVI SCHOOL SMOKE PASS: one connected 152x60 map, 6 hotspots, camera, player, sprite layers, nav");
        }

        public static void BuildWindows()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)==null)
                throw new Exception("School scene does not exist.");
            var target=System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(Application.dataPath),
                "release","CIVI_SchoolWorld_x86.exe");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes=new[]{ScenePath},locationPathName=target,
                target=BuildTarget.StandaloneWindows,
                options=BuildOptions.None
            });
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new Exception("School player build failed: "+report.summary.result);
            Debug.Log("CIVI SCHOOL WINDOWS BUILD PASS: "+target);
        }
    }
}
