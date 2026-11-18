using System;
using System.Collections.Generic;
using UnityEngine;

namespace Civi
{
    // Ambience stems plus one-shots, as the package READMEs ask: never the preview mixes on top of them.
    // cues.json comes from tools/import_audio.py: every sound is tied to its line of the script.
    public sealed class CiviAudio : MonoBehaviour
    {
        [Serializable] sealed class Cue { public int chapter, line; public string clip; public float delay; }
        [Serializable] sealed class Bed { public int chapter, step, line; public string[] clips; }
        [Serializable] sealed class Bank { public string name; public int count; }
        [Serializable] sealed class Plan { public Cue[] cues; public Bed[] beds; public Bank[] banks; }
        sealed class Layer { public AudioSource source; public float target; }
        struct Pending { public float at; public string clip; }

        const float BedLevel = .55f, FadeSeconds = 1.4f;
        const int LayerCount = 8; // up to four layers fading out while four fade in

        readonly Layer[] layers = new Layer[LayerCount];
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        readonly Dictionary<long, List<Cue>> byLine = new Dictionary<long, List<Cue>>();
        readonly Dictionary<string, int> banks = new Dictionary<string, int>();
        readonly List<Pending> pending = new List<Pending>();
        public readonly HashSet<string> Missing = new HashSet<string>();
        AudioSource sfx, ui;
        float hoverAt;
        int lastStep = -1;
        string currentBeds = "";
        Plan plan;

        void Awake()
        {
            for (int i = 0; i < LayerCount; i++)
            {
                var s = gameObject.AddComponent<AudioSource>();
                s.loop = true;
                s.playOnAwake = false;
                s.volume = 0;
                layers[i] = new Layer { source = s };
            }
            sfx = gameObject.AddComponent<AudioSource>();
            ui = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = ui.playOnAwake = false;
            var json = Resources.Load<TextAsset>("Audio/cues");
            plan = JsonUtility.FromJson<Plan>(json.text);
            foreach (var c in plan.cues)
            {
                long key = Key(c.chapter, c.line);
                if (!byLine.TryGetValue(key, out var list)) byLine[key] = list = new List<Cue>();
                list.Add(c);
            }
            foreach (var b in plan.banks) banks[b.name] = b.count;
        }

        static long Key(int chapter, int line) { return chapter * 100000L + line; }

        AudioClip Clip(string folder, string name)
        {
            string key = folder + "/" + name;
            if (!clips.TryGetValue(key, out var clip))
            {
                clip = Resources.Load<AudioClip>("Audio/" + key);
                clips[key] = clip;
                if (clip == null) Missing.Add(key);
            }
            return clip;
        }

        public void Ambience(string name) { Layers(new[] { name }); }

        // Layers already playing carry on untouched; only what changes fades.
        public void Layers(string[] names)
        {
            string joined = string.Join("|", names);
            if (joined == currentBeds) return;
            currentBeds = joined;
            var wanted = new List<string>(names);
            foreach (var layer in layers)
            {
                var clip = layer.source.clip;
                bool keep = clip != null && layer.source.isPlaying && wanted.Remove(clip.name);
                layer.target = keep ? BedLevel : 0;
            }
            foreach (string name in wanted)
            {
                var clip = Clip("Amb", name);
                if (clip == null) continue;
                var free = Array.Find(layers, l => l.target == 0 && l.source.volume <= 0) ?? Array.Find(layers, l => l.target == 0);
                if (free == null) continue;
                free.source.clip = clip;
                free.source.time = 0;
                free.source.Play();
                free.target = BedLevel;
            }
        }

        // Beds of a chapter 2-3 step, including a change part way through the scene once the reader has passed it.
        public void StoryBeds(int chapter, int step, int line)
        {
            Bed chosen = null;
            foreach (var b in plan.beds)
                if (b.chapter == chapter && b.step == step) chosen = b;
            if (chosen == null) return;
            int start = chosen.line, best = -1;
            foreach (var b in plan.beds)
                if (b.chapter == chapter && b.step < 0 && line >= b.line && b.line >= start && b.line > best && InStep(chapter, step, b.line)) { chosen = b; best = b.line; }
            lastStep = step;
            Layers(chosen.clips);
        }

        bool InStep(int chapter, int step, int line)
        {
            foreach (var b in plan.beds)
                if (b.chapter == chapter && b.step == step + 1) return line < b.line;
            return true;
        }

        void Update()
        {
            float fade = Time.unscaledDeltaTime / FadeSeconds * BedLevel;
            foreach (var layer in layers)
            {
                var s = layer.source;
                if (s.volume != layer.target) s.volume = Mathf.MoveTowards(s.volume, layer.target, fade);
                if (layer.target == 0 && s.volume <= 0 && s.isPlaying) { s.Stop(); s.clip = null; }
            }
            for (int i = pending.Count - 1; i >= 0; i--)
                if (Time.unscaledTime >= pending[i].at) { Sfx(pending[i].clip, .8f); pending.RemoveAt(i); }
        }

        public void Sfx(string name, float volume = 1)
        {
            var clip = Clip("Sfx", name);
            if (clip != null) sfx.PlayOneShot(clip, volume);
        }

        public void Ui(string name, float volume = 1)
        {
            var clip = Clip("Ui", name);
            if (clip != null) ui.PlayOneShot(clip, volume);
        }

        public void Hover()
        {
            // Package note: debounce hover by 80-120 ms so a quick sweep across buttons does not rattle.
            if (Time.unscaledTime - hoverAt < .1f) return;
            hoverAt = Time.unscaledTime;
            Ui("UI_01_HOVER");
        }

        int lastVariant;

        // Never the same take twice running: the ear catches that at once.
        public void Foot(string bank, float volume)
        {
            if (!banks.TryGetValue(bank, out int count)) { Missing.Add("Foot/" + bank); return; }
            int pick = UnityEngine.Random.Range(1, count);
            if (pick >= lastVariant) pick++;
            lastVariant = pick;
            var clip = Clip("Foot", bank + "_" + pick.ToString("00"));
            if (clip != null) sfx.PlayOneShot(clip, volume);
        }

        public int Fired;
        public int Planned(int chapter) { int n = 0; foreach (var c in plan.cues) if (c.chapter == chapter) n++; return n; }
        public int LayersPlaying { get { int n = 0; foreach (var l in layers) if (l.target > 0 && l.source.isPlaying) n++; return n; } }

        public void OnLine(int chapter, int line)
        {
            if (!byLine.TryGetValue(Key(chapter, line), out var list)) return;
            foreach (var c in list)
            {
                Fired++;
                if (c.delay <= 0) Sfx(c.clip, .8f);
                else pending.Add(new Pending { at = Time.unscaledTime + c.delay, clip = c.clip });
            }
        }

        // Reader moved on to another page: sounds still waiting belong to the line that just left.
        public void ClearPending() { pending.Clear(); }
    }
}
