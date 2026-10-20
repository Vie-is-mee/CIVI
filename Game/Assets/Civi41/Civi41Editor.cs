#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using Civi41;

public static class Civi41Editor
{
    [MenuItem("CIVI/Trang 17 v4.1/Chơi bản UI mới")]
    public static void Preview(){EditorPrefs.SetBool("CIVI41Preview",true);if(!EditorApplication.isPlaying)EditorApplication.isPlaying=true;}
    [MenuItem("CIVI/Trang 17 v4.1/Kiểm tra lựa chọn")]
    public static void Check(){Debug.Log(Checks.RunAll());}
    public static void Build()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play before building.");
        string output=Environment.GetEnvironmentVariable("CIVI41_OUTPUT");if(string.IsNullOrEmpty(output))throw new InvalidOperationException("CIVI41_OUTPUT missing.");
        Directory.CreateDirectory(output);File.WriteAllText(Path.Combine(output,"model-checks.txt"),Checks.RunAll());
        // Only this isolated preview build receives these settings.
        PlayerSettings.companyName="CIVI Prototype";PlayerSettings.productName="CIVI Trang 17";PlayerSettings.bundleVersion="0.41.1";
        PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        var shader=Shader.Find("TextMeshPro/Mobile/Distance Field");
        if(shader==null)throw new InvalidOperationException("Missing TMP essential shaders.");
        if(!File.Exists("Assets/Resources/Civi41FontMaterial.mat"))AssetDatabase.CreateAsset(new Material(shader),"Assets/Resources/Civi41FontMaterial.mat");
        var uiShader=Shader.Find("UI/Default");if(uiShader==null)throw new InvalidOperationException("Missing UI shader.");
        if(!File.Exists("Assets/Resources/Civi41UIMaterial.mat"))AssetDatabase.CreateAsset(new Material(uiShader),"Assets/Resources/Civi41UIMaterial.mat");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);Directory.CreateDirectory("Assets/Scenes");EditorSceneManager.SaveScene(scene,"Assets/Scenes/Civi41.unity");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Civi41.unity"},locationPathName=Path.Combine(output,"CIVI-Trang17.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        File.WriteAllText(Path.Combine(output,"build-result.txt"),report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\nUnity "+Application.unityVersion);
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("CIVI41 build failed.");
    }
}
#endif
