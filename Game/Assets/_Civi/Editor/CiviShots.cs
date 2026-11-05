using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Civi.EditorTools
{
    // Look-dev: renders stills straight from the assembled scene, far faster than a player build.
    public static class CiviShots
    {
        public static void Run()
        {
            var args = System.Environment.GetCommandLineArgs();
            string dir = Path.GetFullPath("../artifacts/shots");
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-shots") dir = args[i + 1];
            Directory.CreateDirectory(dir);
            ShaderUtil.allowAsyncCompilation = false;
            EditorSceneManager.OpenScene(CiviSceneBuilder.ScenePath);
            var marks = new Dictionary<string, Transform>();
            foreach (var m in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (m.name.StartsWith("M_")) marks[m.name] = m;
            Debug.Log("CIVI marks " + marks.Count + " audit=" + marks["M_OBJ_audit"].position + " linh=" + marks["M_NPC_linh_office"].position);
            var cast = GameObject.Find("Cast").transform;
            Transform mai = cast.Find("mai"), linh = cast.Find("linh"), vinh = cast.Find("vinh"), me = cast.Find("me_mai");
            Show(mai, marks["M_OBJ_audit"].position, 0);
            Show(linh, marks["M_NPC_linh_office"].position, 180);
            Show(cast.Find("truc"), marks["M_NPC_truc_office"].position, 90);
            Show(cast.Find("vinh"), marks["M_NPC_vinh"].position, 180);
            Show(cast.Find("huong_dan"), marks["M_NPC_huong_dan"].position, 90);
            Show(cast.Find("hoc_sinh"), marks["M_NPC_hoc_sinh"].position, 180);
            Show(me, marks["M_NPC_me_mai"].position, 0);
            var cam = Camera.main;
            Vector3 a = marks["M_OBJ_audit"].position, l = marks["M_NPC_linh_office"].position;
            foreach (var r in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) Debug.Log("light " + r.name + " " + r.transform.forward);
            Shot(dir, cam, "office_wide", new Vector3(-24.4f, 2.5f, -6), new Vector3(-8, .8f, 5));
            Shot(dir, cam, "mai_close", a + new Vector3(.5f, 1.45f, 1.5f), a + Vector3.up * 1.15f);
            Shot(dir, cam, "mai_back", a + new Vector3(-.8f, 1.5f, -2), a + Vector3.up * 1.1f);
            Shot(dir, cam, "linh_close", l + new Vector3(.3f, 1.4f, -1.4f), l + Vector3.up * 1.25f);
            Vector3 g = marks["M_OBJ_case"].position, c = marks["M_NPC_linh_lunch"].position, v = marks["M_NPC_vinh"].position, t = marks["M_TITLE_an"].position;
            Shot(dir, cam, "gallery", g + new Vector3(-5.5f, 2.2f, -9), g + new Vector3(1, .8f, 0));
            Shot(dir, cam, "canteen", c + new Vector3(-6, 2.6f, 9), c + new Vector3(2, .8f, -2));
            Shot(dir, cam, "meeting", v + new Vector3(4, 2.4f, -8), v + new Vector3(-2, .8f, 2));
            Shot(dir, cam, "terrace", t + new Vector3(-7, 2.1f, -13), t + new Vector3(2, 1, 0));
            Shot(dir, cam, "window_view", new Vector3(-11, 1.6f, 7.5f), new Vector3(40, 1.5f, 300));
            Shot(dir, cam, "city_far", new Vector3(-280, 90, 30), new Vector3(60, -50, 500));
            Debug.Log("CIVI shots done " + dir);
        }

        static void Show(Transform t, Vector3 p, float yaw)
        {
            t.gameObject.SetActive(true);
            t.position = p;
            t.rotation = Quaternion.Euler(0, yaw, 0);
        }

        static void Shot(string dir, Camera cam, string name, Vector3 pos, Vector3 target)
        {
            cam.transform.position = pos;
            cam.transform.rotation = Quaternion.LookRotation(target - pos);
            var rt = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = null;
            RenderTexture.active = rt;
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            File.WriteAllBytes(Path.Combine(dir, name + ".png"), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }
    }
}
