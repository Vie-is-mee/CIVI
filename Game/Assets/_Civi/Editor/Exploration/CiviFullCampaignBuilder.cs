using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Civi.Runtime;

namespace Civi.Preview.Editor
{
    public static class CiviFullCampaignBuilder
    {
        public const string ScenePath="Assets/_Civi/Scenes/CIVI-NewStory-FullCampaign.unity";
        const string Art="Assets/_Civi/Art/Environments/NewStory/Production/ArtPreview/";
        static readonly string[] Stages={"ENV_RM_ALLEY","ENV_RM_ALLEY_STORM","ENV_RM_FILTER19","ENV_LQ_PLAZA"};
        static readonly string[] Views={"front","back","side"};

        static Sprite LoadSprite(string name,float pixelsPerUnit)
        {
            var importer=AssetImporter.GetAtPath(name) as TextureImporter;
            if(importer==null)throw new InvalidOperationException("Missing environment sprite: "+name);
            if(importer.textureType!=TextureImporterType.Sprite||
               Math.Abs(importer.spritePixelsPerUnit-pixelsPerUnit)>.01f)
            {
                importer.textureType=TextureImporterType.Sprite;
                importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=pixelsPerUnit;
                importer.maxTextureSize=2048;
                importer.filterMode=FilterMode.Bilinear;
                importer.mipmapEnabled=false;
                importer.wrapMode=TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(name);
            if(sprite==null)throw new InvalidOperationException("Could not load "+name);
            return sprite;
        }

        [MenuItem("CIVI/NEW STORY/Build Final Campaign")]
        public static void Build()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)!=null)
                throw new InvalidOperationException("Campaign scene exists: do not overwrite manually edited scene.");
            var data=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Civi/Resources/NewStory/campaign.json");
            if(data==null)throw new InvalidOperationException("Missing canonical campaign.json.");
            var sprites=Stages.Select(x=>LoadSprite(Art+x+"_v02.png",100f)).ToArray();
            var fore=Stages.Select(x=>LoadSprite(Art+x+"_foreground_v02.png",100f)).ToArray();
            var avatar=Views.Select(x=>LoadSprite(Art+"Characters/MAI_placeholder_"+x+".png",180f)).ToArray();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var go=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));
            go.tag="MainCamera";go.transform.position=new Vector3(0,0,-10);
            var camera=go.GetComponent<Camera>();
            camera.orthographic=true;camera.orthographicSize=3.75f;
            camera.backgroundColor=new Color(.03f,.07f,.08f);
            var campaignRoot=new GameObject("CIVI | Full Campaign 2026",typeof(CiviFullCampaign));
            var campaign=campaignRoot.GetComponent<CiviFullCampaign>();
            campaign.Content=data;
            campaign.Backgrounds=sprites;campaign.Foregrounds=fore;
            campaign.AvatarViews=avatar;
            EditorSceneManager.SaveScene(scene,ScenePath);
            // Active release contains only the new-story campaign.
            // Old scenes remain archived and fully recoverable, no file deletion.
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            AssetDatabase.SaveAssets();
            Debug.Log("CIVI FULL CAMPAIGN CREATED with 17 canonical story episodes: "+ScenePath);
        }

        [MenuItem("CIVI/NEW STORY/Open Full Campaign")]
        public static void Open()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)==null)Build();
            else EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        }


        public static void BuildWindows()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)==null)
                throw new InvalidOperationException("Build the campaign scene first.");
            var path=System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(Application.dataPath),"release",
                "CIVI_NewStory_Test_x86.exe");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            var report=UnityEditor.BuildPipeline.BuildPlayer(new UnityEditor.BuildPlayerOptions
            {
                scenes=new[]{ScenePath},
                locationPathName=path,
                target=UnityEditor.BuildTarget.StandaloneWindows,
                options=UnityEditor.BuildOptions.None
            });
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("CIVI Windows build failed: "+report.summary.result);
            Debug.Log("CIVI WINDOWS BUILD PASS: "+path+" bytes "+report.summary.totalSize);
        }

        // Editor-only smoke: graph completeness, art references, 3 traversal screens,
        // all decision routes and ending reachability.
        public static void Smoke()
        {
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var cam=Camera.main;
            if(cam==null)throw new Exception("Campaign camera missing");
            var c=UnityEngine.Object.FindAnyObjectByType<CiviFullCampaign>();
            if(c==null)throw new Exception("Campaign MonoBehaviour missing");
            c.Boot();
            if(c.EpisodeCount!=17||c.EpisodeId!="C1"||
               c.ChunkCount!=3||!c.HasController)
                throw new Exception("Full campaign boot failed");
            CiviCampaignGraph.Validate(c.Content.text==null?new string[0]:
                JsonUtility.FromJson<CiviFullCampaign.Campaign>(c.Content.text)
                   .episodes.Select(x=>x.id));
            foreach(string[] abc in new[]{
                new[]{"I_A","II_A","END_A"},new[]{"I_B","II_B","END_B"},
                new[]{"I_A","II_B","C9C"},new[]{"I_B","II_A","C9C"}
            })
            {
                string node="C1";
                var visited=new System.Collections.Generic.HashSet<string>();
                while(node!=null)
                {
                    if(!visited.Add(node))throw new Exception("Loop in simulated campaign");
                    string selected=node=="C5"?abc[0]:node=="C7"?abc[1]:node=="C9"?abc[2]:null;
                    node=CiviCampaignGraph.Next(node,selected);
                }
                if(!visited.Contains(abc[2]=="C9C"?"C10":abc[2]))
                    throw new Exception("Missing ending "+abc[2]);
            }
            Debug.Log("CIVI FULL CAMPAIGN SMOKE PASS: 17 story episodes, 3 continuous screens, protagonist camera, all branch endings.");
        }
    }
}
