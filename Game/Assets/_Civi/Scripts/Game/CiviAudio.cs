using System;
using System.Collections.Generic;
using UnityEngine;

namespace Civi
{
    // Ambience and one-shots follow the package README: ambience stems plus SFX, never the preview mixes on top of them.
    public sealed class CiviAudio : MonoBehaviour
    {
        [Serializable] sealed class Cue { public int scene; public float at; public string clip; }
        [Serializable] sealed class CueList { public Cue[] cues; }

        readonly AudioSource[] beds = new AudioSource[2];
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        AudioSource sfx, ui;
        int bed;
        string current;
        float hoverAt, stepIndex;
        CueList cues;
        int lastScene = -1, lastLine = -1;
        const float BedLevel = .55f;

        void Awake()
        {
            for (int i = 0; i < 2; i++)
            {
                beds[i] = gameObject.AddComponent<AudioSource>();
                beds[i].loop = true;
                beds[i].playOnAwake = false;
                beds[i].volume = 0;
            }
            sfx = gameObject.AddComponent<AudioSource>();
            ui = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = ui.playOnAwake = false;
            var json = Resources.Load<TextAsset>("Audio/cues");
            cues = json == null ? new CueList { cues = new Cue[0] } : JsonUtility.FromJson<CueList>(json.text);
        }

        AudioClip Clip(string folder, string name)
        {
            string key = folder + "/" + name;
            if (!clips.TryGetValue(key, out var clip)) { clip = Resources.Load<AudioClip>("Audio/" + key); clips[key] = clip; }
            return clip;
        }

        public void Ambience(string name)
        {
            if (name == current) return;
            var clip = Clip("Amb", name);
            if (clip == null) return;
            current = name;
            bed = 1 - bed;
            beds[bed].clip = clip;
            beds[bed].Play();
        }

        void Update()
        {
            float step = Time.unscaledDeltaTime / 1.4f;
            beds[bed].volume = Mathf.MoveTowards(beds[bed].volume, BedLevel, step);
            var other = beds[1 - bed];
            other.volume = Mathf.MoveTowards(other.volume, 0, step);
            if (other.volume <= 0 && other.isPlaying) other.Stop();
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

        public void Foot()
        {
            stepIndex = (stepIndex + 1) % 4;
            Sfx("foot_" + (int)stepIndex, .32f);
        }

        // line/count: how far through the dialogue scene the reader is. Cues fire once as that point passes them.
        public void OnLine(int scene, int line, int count)
        {
            if (scene != lastScene) { lastScene = scene; lastLine = -1; }
            if (line <= lastLine) return;
            float from = (lastLine + 1) / (float)count, to = (line + 1) / (float)count;
            lastLine = line;
            foreach (var cue in cues.cues)
                if (cue.scene == scene && cue.at >= from && cue.at < (to >= 1 ? 1.01f : to)) Sfx(cue.clip, .8f);
        }
    }
}
