using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Civi
{
    // Holds 60 fps by trading image quality, never gameplay. It steps down fast when frames run long and climbs back
    // slowly; each failed climb doubles the wait, so a machine settles instead of oscillating.
    public sealed class CiviPerformance : MonoBehaviour
    {
        struct Tier
        {
            public float scale, shadow; public int msaa, cascades, lights; public bool bloom, ao, sun;
            public Tier(float scale, int msaa, float shadow, int cascades, int lights, bool bloom, bool ao, bool sun = true)
            { this.scale = scale; this.msaa = msaa; this.shadow = shadow; this.cascades = cascades; this.lights = lights; this.bloom = bloom; this.ao = ao; this.sun = sun; }
        }

        static readonly Tier[] Tiers =
        {
            new Tier(1f, 4, 42, 2, 4, true, true),
            new Tier(1f, 2, 42, 2, 4, true, true),
            new Tier(1f, 2, 32, 2, 4, true, false),
            new Tier(.85f, 1, 26, 1, 2, true, false),
            new Tier(.75f, 1, 22, 1, 2, false, false),
            new Tier(.67f, 1, 16, 1, 1, false, false),
            new Tier(.6f, 1, 0, 1, 0, false, false, false),
        };

        public const int TargetFps = 60;
        public int Level { get; private set; }
        public float Fps { get; private set; } = TargetFps;
        public float WorstMs { get; private set; }
        public bool Software { get; private set; }
        public bool Locked;
        public string Device { get { return SystemInfo.graphicsDeviceName; } }

        UniversalRenderPipelineAsset urp;
        Bloom bloom;
        ScriptableRendererFeature ao;
        float window, worst, holdUntil, raiseDelay = 25;
        int frames, slowSeconds, goodSeconds, failedLevel = -1;

        void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFps;
            urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            var volume = FindAnyObjectByType<Volume>();
            if (volume != null) volume.sharedProfile.TryGet(out bloom);
            if (urp != null)
                foreach (var feature in urp.rendererDataList[0].rendererFeatures)
                    if (feature is ScreenSpaceAmbientOcclusion) ao = feature;
            string name = SystemInfo.graphicsDeviceName ?? "";
            Software = Array.IndexOf(Environment.GetCommandLineArgs(), "-civi-cpu") >= 0 ||
                name.IndexOf("Basic Render", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("WARP", StringComparison.OrdinalIgnoreCase) >= 0;
            // Integrated GPUs start lower, so the first minute is not spent stepping down.
            bool integrated = name.IndexOf("Intel", StringComparison.OrdinalIgnoreCase) >= 0 || SystemInfo.graphicsMemorySize < 2000;
            Apply(Software ? Tiers.Length - 1 : integrated ? 3 : 0);
            if (Software) { Locked = true; Application.targetFrameRate = 30; }
            Hold(3);
        }

        // Scene swaps and cutscene cuts legitimately cost a frame or two; do not punish quality for them.
        public void Hold(float seconds) { holdUntil = Time.unscaledTime + seconds; ResetWindow(); }

        void ResetWindow() { window = 0; frames = 0; worst = 0; }

        public void Apply(int level)
        {
            Level = Mathf.Clamp(level, 0, Tiers.Length - 1);
            var t = Tiers[Level];
            if (urp != null)
            {
                urp.renderScale = t.scale;
                urp.msaaSampleCount = t.msaa;
                urp.shadowDistance = t.shadow;
                urp.shadowCascadeCount = t.cascades;
                urp.maxAdditionalLightsCount = t.lights;
            }
            if (bloom != null) bloom.active = t.bloom;
            if (ao != null) ao.SetActive(t.ao);
            var sun = RenderSettings.sun;
            if (sun != null) sun.shadows = t.sun ? LightShadows.Soft : LightShadows.None;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (Time.unscaledTime < holdUntil) return;
            window += dt;
            frames++;
            worst = Mathf.Max(worst, dt);
            if (window < 1) return;
            Fps = frames / window;
            WorstMs = worst * 1000;
            ResetWindow();
            if (Locked) return;
            if (Fps < TargetFps - 7)
            {
                goodSeconds = 0;
                if (++slowSeconds >= 2 && Level < Tiers.Length - 1)
                {
                    if (failedLevel < 0) failedLevel = Level;
                    raiseDelay = Mathf.Min(raiseDelay * 2, 300);
                    Apply(Level + 1);
                    slowSeconds = 0;
                    Hold(1);
                }
            }
            else
            {
                slowSeconds = 0;
                if (Fps >= TargetFps - 2 && ++goodSeconds >= raiseDelay && Level > 0)
                {
                    goodSeconds = 0;
                    Apply(Level - 1);
                    Hold(1);
                }
            }
        }
    }
}
