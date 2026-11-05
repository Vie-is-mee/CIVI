using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Civi.EditorTools
{
    public static class CiviProjectSetup
    {
        const string Dir = "Assets/_Civi/Rendering";

        // Contact shadows are the cheapest way to seat furniture on the floor. "After opaque" needs no shader support.
        static void AddAmbientOcclusion(ScriptableRendererData data)
        {
            ScreenSpaceAmbientOcclusion ao = null;
            foreach (var f in data.rendererFeatures) if (f is ScreenSpaceAmbientOcclusion found) ao = found;
            if (ao == null)
            {
                ao = ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();
                ao.name = "CiviAO";
                AssetDatabase.AddObjectToAsset(ao, data);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(ao, out string _, out long id);
                var rd = new SerializedObject(data);
                var list = rd.FindProperty("m_RendererFeatures");
                var map = rd.FindProperty("m_RendererFeatureMap");
                list.InsertArrayElementAtIndex(list.arraySize);
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = ao;
                map.InsertArrayElementAtIndex(map.arraySize);
                map.GetArrayElementAtIndex(map.arraySize - 1).longValue = id;
                rd.ApplyModifiedPropertiesWithoutUndo();
            }
            var settings = new SerializedObject(ao).FindProperty("m_Settings");
            settings.FindPropertyRelative("AfterOpaque").boolValue = true;
            settings.FindPropertyRelative("Source").enumValueIndex = 0;
            settings.FindPropertyRelative("Downsample").boolValue = true;
            settings.FindPropertyRelative("Intensity").floatValue = 1.1f;
            settings.FindPropertyRelative("Radius").floatValue = .32f;
            settings.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
        }

        [MenuItem("CIVI/Project setup")]
        public static void Run()
        {
            Directory.CreateDirectory(Dir);
            string path = Dir + "/CiviURP.asset";
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (asset == null)
            {
                // URP keeps its default-renderer factory internal; this is the same call its menu item makes.
                var factory = typeof(UniversalRenderPipelineAsset).GetMethod("CreateRendererAsset", BindingFlags.NonPublic | BindingFlags.Static);
                var data = (ScriptableRendererData)factory.Invoke(null, new object[] { Dir + "/CiviRenderer.asset", RendererType.UniversalRenderer, false, "Renderer" });
                asset = UniversalRenderPipelineAsset.Create(data);
                AssetDatabase.CreateAsset(asset, path);
            }
            var so = new SerializedObject(asset);
            so.FindProperty("m_ShadowDistance").floatValue = 42;
            so.FindProperty("m_ShadowCascadeCount").intValue = 2;
            so.FindProperty("m_Cascade2Split").floatValue = .3f;
            so.FindProperty("m_MainLightShadowmapResolution").intValue = 2048;
            so.FindProperty("m_SoftShadowsSupported").boolValue = true;
            so.FindProperty("m_AdditionalLightsPerObjectLimit").intValue = 4;
            so.FindProperty("m_LocalShadowsSupported").boolValue = false;
            so.FindProperty("m_SupportsHDR").boolValue = true;
            so.FindProperty("m_MSAA").intValue = 4;
            so.FindProperty("m_UseSRPBatcher").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            AddAmbientOcclusion(asset.rendererDataList[0]);
            GraphicsSettings.defaultRenderPipeline = asset;
            QualitySettings.renderPipeline = asset;

            PlayerSettings.companyName = "VSIC";
            PlayerSettings.productName = "CIVI";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
            // D3D11 is the one API that also runs on Windows' software rasteriser (see CHOI_GAME.bat).
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11 });
            AssetDatabase.SaveAssets();
            Debug.Log("CIVI setup done");
        }
    }
}
