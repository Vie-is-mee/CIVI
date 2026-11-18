using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Civi
{
    // "Hỏi CIVI" kept from the web demo, which leaned on the Claude page runtime. A standalone game has none, so it
    // uses the player's own Claude key if set, else a local Ollama, else the same canned lines the web demo used offline.
    // The key is read from the environment or a local file and never compiled in: this is a client anyone can unpack.
    public sealed class CiviAi : MonoBehaviour
    {
        [Serializable] sealed class Block { public string type, text; }
        [Serializable] sealed class ClaudeError { public string type, message; }
        [Serializable] sealed class ClaudeReply { public string stop_reason; public Block[] content; public ClaudeError error; }
        [Serializable] sealed class OllamaMessage { public string role, content; }
        [Serializable] sealed class OllamaReply { public OllamaMessage message; }
        [Serializable] sealed class OllamaModel { public string name; }
        [Serializable] sealed class OllamaTags { public OllamaModel[] models; }
        [Serializable] sealed class Quoted { public string s; }

        const string ClaudeUrl = "https://api.anthropic.com/v1/messages", ClaudeModel = "claude-opus-5-5", OllamaBase = "http://localhost:11434";
        static readonly string[] Canned =
        {
            "Gợi ý của tôi: nhìn mục tiêu ở góc trên bên trái. Nếu vẫn lạc, bật Kính Debug.",
            "Tôi là CIVI. Tôi giỏi cộng trừ. Tôi đang học cách biết ai chưa được cộng.",
            "Mẹo: nói chuyện với mọi người. Dữ liệu tốt nhất thường không nằm trong bảng.",
            "Bản đồ ở phím M. Túi đồ ở phím B. Tôi ở đây, và tôi không tính phí.",
        };

        string key, ollamaModel;
        public string Provider { get { return !string.IsNullOrEmpty(key) ? "Claude" : ollamaModel != null ? "Ollama · " + ollamaModel : "Ngoại tuyến · câu soạn sẵn"; } }
        public bool Online { get { return !string.IsNullOrEmpty(key) || ollamaModel != null; } }
        public string CannedLine() { return Canned[UnityEngine.Random.Range(0, Canned.Length)]; }

        public void Prepare(string keyFolder)
        {
            key = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
            if (string.IsNullOrEmpty(key))
            {
                // Beside the save folder, one line, so a teammate can opt in without touching system settings.
                string file = Path.Combine(keyFolder, "civi-ai-key.txt");
                try { if (File.Exists(file)) key = File.ReadAllText(file).Trim(); }
                catch (IOException) { }
            }
            if (string.IsNullOrEmpty(key)) StartCoroutine(FindOllama());
        }

        IEnumerator FindOllama()
        {
            using (var r = UnityWebRequest.Get(OllamaBase + "/api/tags"))
            {
                r.timeout = 2;
                yield return r.SendWebRequest();
                if (r.result != UnityWebRequest.Result.Success) yield break;
                OllamaTags tags = null;
                try { tags = JsonUtility.FromJson<OllamaTags>(r.downloadHandler.text); }
                catch (ArgumentException) { }
                if (tags == null || tags.models == null || tags.models.Length == 0) yield break;
                ollamaModel = tags.models[0].name;
                foreach (var m in tags.models) if (m.name.StartsWith("llama3")) ollamaModel = m.name;
            }
        }

        static string Q(string text)
        {
            string json = JsonUtility.ToJson(new Quoted { s = text ?? "" });
            return json.Substring(5, json.Length - 6);
        }

        static string Messages(List<KeyValuePair<string, string>> turns)
        {
            var b = new StringBuilder("[");
            for (int i = 0; i < turns.Count; i++)
                b.Append(i == 0 ? "" : ",").Append("{\"role\":").Append(Q(turns[i].Key)).Append(",\"content\":").Append(Q(turns[i].Value)).Append('}');
            return b.Append(']').ToString();
        }

        // turns: (role, text) oldest first, ending with the user. done gets null when no model could answer.
        public IEnumerator Ask(string system, List<KeyValuePair<string, string>> turns, Action<string> done)
        {
            if (!string.IsNullOrEmpty(key)) { yield return Claude(system, turns, done); yield break; }
            if (ollamaModel != null) { yield return Ollama(system, turns, done); yield break; }
            done(null);
        }

        IEnumerator Claude(string system, List<KeyValuePair<string, string>> turns, Action<string> done)
        {
            // Short in-character replies: low effort answers fast, the fallback keeps a declined turn from dead-ending.
            string body = "{\"model\":" + Q(ClaudeModel) + ",\"max_tokens\":2048,\"output_config\":{\"effort\":\"low\"},\"fallbacks\":\"default\",\"system\":" + Q(system) + ",\"messages\":" + Messages(turns) + "}";
            using (var r = new UnityWebRequest(ClaudeUrl, "POST"))
            {
                r.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                r.downloadHandler = new DownloadHandlerBuffer();
                r.SetRequestHeader("content-type", "application/json");
                r.SetRequestHeader("x-api-key", key);
                r.SetRequestHeader("anthropic-version", "2026-10-21");
                r.SetRequestHeader("anthropic-beta", "server-side-fallback-2026-07-01");
                r.timeout = 45;
                yield return r.SendWebRequest();
                if (r.result != UnityWebRequest.Result.Success) { Debug.LogWarning("CIVI AI: HTTP " + r.responseCode); done(null); yield break; }
                done(ReadClaude(r.downloadHandler.text));
            }
        }

        static string ReadClaude(string json)
        {
            ClaudeReply reply;
            try { reply = JsonUtility.FromJson<ClaudeReply>(json); }
            catch (ArgumentException) { return null; }
            if (reply == null || reply.content == null) return null;
            if (reply.stop_reason == "refusal") return "Câu này nằm ngoài những gì tôi được phép xử lý. Hỏi tôi về Lưu Quang nhé.";
            var text = new StringBuilder();
            foreach (var block in reply.content) if (block.type == "text") text.Append(block.text);
            return text.Length == 0 ? null : text.ToString();
        }

        IEnumerator Ollama(string system, List<KeyValuePair<string, string>> turns, Action<string> done)
        {
            var all = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("system", system) };
            all.AddRange(turns);
            string body = "{\"model\":" + Q(ollamaModel) + ",\"stream\":false,\"options\":{\"num_predict\":220},\"messages\":" + Messages(all) + "}";
            using (var r = new UnityWebRequest(OllamaBase + "/api/chat", "POST"))
            {
                r.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                r.downloadHandler = new DownloadHandlerBuffer();
                r.SetRequestHeader("content-type", "application/json");
                r.timeout = 170; // the first question loads the whole model into memory
                yield return r.SendWebRequest();
                if (r.result != UnityWebRequest.Result.Success) { Debug.LogWarning("CIVI AI: Ollama " + r.error); done(null); yield break; }
                OllamaReply reply = null;
                try { reply = JsonUtility.FromJson<OllamaReply>(r.downloadHandler.text); }
                catch (ArgumentException) { }
                done(reply == null || reply.message == null ? null : reply.message.content);
            }
        }
    }
}
