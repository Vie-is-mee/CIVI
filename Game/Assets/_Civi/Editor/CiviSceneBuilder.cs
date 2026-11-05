using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Civi.EditorTools
{
    // Assembles the one playable scene from the Blender FBX files. Re-running replaces it, so the scene is never hand-edited.
    public static class CiviSceneBuilder
    {
        public const string ScenePath = "Assets/_Civi/Scenes/CIVI-Main.unity";
        const string Rendering = "Assets/_Civi/Rendering/";
        const string Models = "Assets/_Civi/Models/";
        static readonly Dictionary<string, string> FaceStyle = new Dictionary<string, string>
        {
            { "an", "kid" }, { "hoc_sinh", "kid" }, { "mai_nho", "kid" }, { "vinh", "man" }, { "dong_nghiep_a", "man" },
            { "truc", "man" }, { "nv_benh_vien", "man" }, { "me_mai", "old" }, { "ao_xam", "old" }
        };

        static Material Mat(string name, string shader, System.Action<Material> setup = null)
        {
            string path = Rendering + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find(shader)); AssetDatabase.CreateAsset(m, path); }
            m.shader = Shader.Find(shader);
            setup?.Invoke(m);
            EditorUtility.SetDirty(m);
            return m;
        }

        [MenuItem("CIVI/Build scene")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            var env = Mat("Env", "Civi/Toon");
            // Faces and clothes must stay readable when a character is backlit by the windows.
            var body = Mat("Body", "Civi/ToonOutlined", m => m.SetColor("_ShadowTint", new Color(1.34f, 1.2f, 1.18f)));
            var glass = Mat("Glass", "Civi/Glass");
            var dome = Mat("Dome", "Civi/Glass", m => { m.SetColor("_Color", new Color(.74f, .92f, 1, .03f)); m.SetColor("_Edge", new Color(.86f, 1, 1, .2f)); m.SetFloat("_Power", 4); });
            var screen = Mat("Screen", "Civi/Screen");
            var sky = Mat("Sky", "Civi/Sky");
            var faces = new Dictionary<string, Material>();
            foreach (var style in new[] { "adult", "kid", "man", "old" })
            {
                var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Civi/Resources/Chapter1/Faces/face_" + style + ".png");
                faces[style] = Mat("Face_" + style, "Civi/Face", m => m.SetTexture("_BaseMap", atlas));
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var world = new GameObject("World").transform;
            foreach (var file in new[] { "Floor42", "Home", "City", "Props", "Civi", "Cat" })
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Models + "World/" + file + ".fbx");
                var go = (GameObject)PrefabUtility.InstantiatePrefab(asset, world);
                PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                go.name = file;
                foreach (var r in go.GetComponentsInChildren<MeshRenderer>(true)) Dress(r, env, glass, dome, screen, body);
                string boxes = Models + "World/" + file + ".collision.txt";
                if (File.Exists(boxes))
                {
                    var col = new GameObject("Collision");
                    col.transform.SetParent(go.transform, false);
                    foreach (string line in File.ReadAllLines(boxes))
                    {
                        var v = System.Array.ConvertAll(line.Split(' '), x => float.Parse(x, System.Globalization.CultureInfo.InvariantCulture));
                        var box = col.AddComponent<BoxCollider>();
                        box.center = new Vector3(v[0], v[1], v[2]);
                        box.size = new Vector3(v[3], v[4], v[5]);
                    }
                }
                foreach (var t in go.GetComponentsInChildren<Transform>(true))
                    if (t.name.StartsWith("Door_"))
                    {
                        var box = t.gameObject.AddComponent<BoxCollider>();
                        var mf = t.GetComponent<MeshFilter>();
                        if (mf != null) { box.center = mf.sharedMesh.bounds.center; box.size = mf.sharedMesh.bounds.size; }
                    }
            }

            var cast = new GameObject("Cast").transform;
            foreach (var guid in AssetDatabase.FindAssets("t:GameObject", new[] { Models + "Characters" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path);
                var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path), cast);
                PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                go.name = name;
                var animator = go.GetComponent<Animator>();
                if (animator != null) Object.DestroyImmediate(animator);
                var skin = go.GetComponentInChildren<SkinnedMeshRenderer>(true);
                FaceStyle.TryGetValue(name, out var style);
                skin.sharedMaterials = new[] { body, faces[style ?? "adult"] };
                skin.updateWhenOffscreen = false;
                skin.localBounds = new Bounds(new Vector3(0, .9f, 0), new Vector3(2.4f, 2.4f, 2.4f));
                go.SetActive(false);
            }

            var sunGo = new GameObject("Sun");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 1;
            sun.intensity = 1.2f;
            sun.color = new Color(1, .95f, .86f);
            sun.transform.rotation = Quaternion.LookRotation(new Vector3(.46f, -.6f, -.66f));
            RenderSettings.sun = sun;

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.nearClipPlane = .1f;
            cam.farClipPlane = 2600;
            cam.fieldOfView = 52;
            camGo.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
            camGo.GetComponent<UniversalAdditionalCameraData>().antialiasing = AntialiasingMode.None;

            var volumeGo = new GameObject("Post");
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = Profile();

            RenderSettings.skybox = sky;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 160;
            RenderSettings.fogEndDistance = 1500;
            RenderSettings.fogColor = new Color(.8f, .91f, .96f);
            RenderSettings.reflectionIntensity = 0;

            var game = new GameObject("CIVI");
            var type = System.Type.GetType("Civi.CiviGame, Assembly-CSharp");
            if (type != null) game.AddComponent(type);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("CIVI scene built");
        }

        static void Dress(MeshRenderer r, Material env, Material glass, Material dome, Material screen, Material body)
        {
            string n = r.name;
            bool isGlass = n.EndsWith("_Glass"), isScreen = n.EndsWith("_Screen");
            r.sharedMaterial = n == "DomeShell" ? dome : isGlass ? glass : isScreen ? screen : n.StartsWith("P_") || n == "Civi" || n.StartsWith("Cat") ? body : env;
            bool roof = n.EndsWith("_Roof");
            r.shadowCastingMode = isGlass || isScreen || roof || n == "City" || n == "DomeShell" || n == "Tram" ? ShadowCastingMode.Off : ShadowCastingMode.On;
            r.receiveShadows = n != "City";
            r.lightProbeUsage = LightProbeUsage.Off;
            r.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        static VolumeProfile Profile()
        {
            string path = Rendering + "Post.asset";
            AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.value = 1.05f;
            bloom.intensity.value = .55f;
            bloom.scatter.value = .62f;
            bloom.highQualityFiltering.value = false;
            var tone = profile.Add<Tonemapping>(true);
            tone.mode.value = TonemappingMode.Neutral;
            var color = profile.Add<ColorAdjustments>(true);
            color.saturation.value = 8;
            color.contrast.value = 6;
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.value = .16f;
            vignette.smoothness.value = .5f;
            foreach (var c in profile.components) AssetDatabase.AddObjectToAsset(c, profile);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }
    }
}
