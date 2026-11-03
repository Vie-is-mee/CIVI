using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Civi.Preview.Editor
{
    // Explicit scene asset, additive to the S00 demo. Never regenerates S00 or its catalog.
    public static class CiviExplorationSceneBuilder
    {
        const string ScenePath = "Assets/_Civi/Scenes/S02-2D-VerticalSlice.unity";
        const string CatalogPath = "Assets/_Civi/Config/TV3-AssetCatalog.asset";

        [MenuItem("CIVI/TV2/Tạo scene khám phá 2D S02-S03")]
        public static void CreateScene()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<CiviAssetCatalog>(CatalogPath);
            if(catalog==null || catalog.storyInitialState==null ||
               catalog.storyScenes==null || catalog.storyScenes.Length!=15)
                throw new InvalidOperationException("Prepare the existing TV3 catalog first. Run CIVI/TV3/Chuẩn bị bản tích hợp S00.");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));
            camera.tag="MainCamera";
            var cam=camera.GetComponent<Camera>();
            cam.orthographic=true;cam.orthographicSize=4.85f;
            cam.transform.position=new Vector3(0,0,-10);
            cam.clearFlags=CameraClearFlags.SolidColor;
            cam.backgroundColor=new Color(.10f,.18f,.20f);
            var root=new GameObject("CIVI / 2D Exploration Controller",typeof(CiviExploration2D));
            root.GetComponent<CiviExploration2D>().catalog=catalog;
            EditorSceneManager.SaveScene(scene,ScenePath);
            var scenes=EditorBuildSettings.scenes.ToList();
            var existing=scenes.FindIndex(x=>x.path==ScenePath);
            if(existing>=0)scenes[existing]=new EditorBuildSettingsScene(ScenePath,true);
            else scenes.Add(new EditorBuildSettingsScene(ScenePath,true));
            EditorBuildSettings.scenes=scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("CIVI P0 ready: Open "+ScenePath+" and press Play. The existing S00 scene is untouched.");
        }

        // Runs in an isolated Editor batchmode copy; not a substitute for PlayMode QA.
        public static void ValidateRuntimeSmoke()
        {
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            try
            {
                var controller=UnityEngine.Object.FindAnyObjectByType<CiviExploration2D>();
                if(controller==null)throw new InvalidOperationException("Missing 2D controller in scene");
                var boot=typeof(CiviExploration2D).GetMethod("Boot",
                    System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
                if(boot==null)throw new InvalidOperationException("Cannot inspect 2D bootstrap");
                boot.Invoke(controller,null);
                var points=controller.GetComponentsInChildren<Civi2DSpot>(true);
                if(points.Length!=12)throw new InvalidOperationException("Expected 12 interactables, found "+points.Length);
                var players=UnityEngine.Object.FindObjectsByType<Civi2DPlayer>();
                if(players.Length!=1)throw new InvalidOperationException("Expected one controllable An");
                var cameraRig=Camera.main.GetComponent<CiviFollowCamera2D>();
                if(cameraRig==null || cameraRig.Target!=players[0].transform)
                    throw new InvalidOperationException("Camera is not following An");
                if(players[0].GetComponent<CiviActorVisual2D>()==null)
                    throw new InvalidOperationException("Missing 4-way placeholder avatar");
                var world=controller.GetComponentInChildren<CiviQuarterWorld2D>(true);
                if(world==null)throw new InvalidOperationException("Quarter room controller missing");
                var stageNames=world.GetComponentsInChildren<Transform>(true).Select(t=>t.name).ToArray();
                if(!stageNames.Contains("S02 / GÓC KIỂM ĐỒ") || !stageNames.Contains("S03 / PHÒNG ẢNH"))
                    throw new InvalidOperationException("Distinct S02 and S03 environments missing");
                if(controller.GetComponentsInChildren<CiviDepthSort2D>(true).Length<13)
                    throw new InvalidOperationException("Depth sorting is missing");
                var state=typeof(CiviExploration2D).GetField("story",
                    System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
                var session=state.GetValue(controller) as Civi.Runtime.StorySession;
                if(session==null||session.SceneId!="S02"||session.CurrentLine==null)
                    throw new InvalidOperationException("Scene failed to load S02 authoring state");
                // P2: confirm the inspected-photo UI, authored answer mapping and the
                // isolated hypothesis notebook without mutating the real story state.
                var notebook=new Civi.Runtime.InvestigationNotebook();
                if(Civi.Runtime.InvestigationNotebook.Sources.Length!=4 ||
                   !CiviEvidenceWorkbenchUI.AnswerIsCorrect("PHOTO_0441",0) ||
                   !CiviEvidenceWorkbenchUI.AnswerIsCorrect("PHOTO_0442",1) ||
                   !CiviEvidenceWorkbenchUI.AnswerIsCorrect("PHOTO_0443",2) ||
                   !CiviEvidenceWorkbenchUI.AnswerIsCorrect("PHOTO_CLAMP_EMPTY",2) ||
                    CiviEvidenceWorkbenchUI.AnswerIsCorrect("PHOTO_0443",0))
                    throw new InvalidOperationException("P2 authored observation mapping is invalid");
                notebook.Observe("PHOTO_0442");
                if(notebook.Propose("unconfirmed").Result!=Civi.Runtime.InvestigationNotebook.Verdict.NeedMoreSources)
                    throw new InvalidOperationException("P2 allowed a deduction without 0443");
                notebook.Observe("PHOTO_0443");
                if(notebook.Propose("unconfirmed").Result!=Civi.Runtime.InvestigationNotebook.Verdict.PlausibleNeedsVerification ||
                   notebook.Propose("consent").Result!=Civi.Runtime.InvestigationNotebook.Verdict.Unsupported)
                    throw new InvalidOperationException("P2 deduction safety validation failed");
                var modal=controller.GetComponentsInChildren<RectTransform>(true)
                    .FirstOrDefault(t=>t.name=="Dialog and evidence");
                if(modal==null)throw new InvalidOperationException("P2 modal root missing");
                CiviEvidenceWorkbenchUI.DrawInspector(modal,controller.catalog,notebook,"PHOTO_0441",()=>{},()=>{});
                if(modal.GetComponentsInChildren<CiviPhotoPanZoom>(true).Length!=1)
                    throw new InvalidOperationException("P2 image zoom/pan viewport did not initialize");
                Debug.Log("CIVI P2 SMOKE PASS: 3/4 world, 1 player, 12 spots, image inspection, correct options, deduction guardrails");
            }
            finally
            {
                EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            }
        }

        [MenuItem("CIVI/TV2/Mở scene khám phá 2D")]
        public static void OpenScene()
        {
            if(!System.IO.File.Exists(ScenePath))
                CreateScene();
            else
                EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        }
    }
}
