using UnityEngine;

namespace Civi
{
    // Procedural skeleton animation in character space (x right, y up, z forward). The stride advances with distance
    // travelled, so feet do not slide at any speed, and bone lengths can differ per character.
    public sealed class CiviBody : MonoBehaviour
    {
        enum B { Hips, Spine, Chest, Neck, Head, Pony, ArmL, ForeL, HandL, ArmR, ForeR, HandR, LegL, ShinL, FootL, LegR, ShinR, FootR, Count }
        public const int Neutral = 0, Blink = 1, Talk = 2, Smile = 3, Sad = 4, Surprised = 5, Serious = 6, Happy = 7;

        public float moveSpeed;
        public bool airborne, talking, reducedMotion;
        public int expression = Neutral;
        public Vector3 lookPoint;
        public bool hasLook;
        public System.Action footfall;

        readonly Transform[] bone = new Transform[(int)B.Count];
        readonly Quaternion[] restLocal = new Quaternion[(int)B.Count], restRel = new Quaternion[(int)B.Count];
        Vector3 hipsRest;
        float amount, run, air, phase, time, seed, lookYaw, lookPitch, blinkAt, blinkUntil, gestureUntil, gestureLength, lastFoot;
        string gesture = "";
        int cell = -1;
        SkinnedMeshRenderer skin;
        MaterialPropertyBlock block;
        static readonly int FaceUV = Shader.PropertyToID("_FaceUV");

        public Vector3 HeadPoint { get { return bone[(int)B.Head].position + Vector3.up * .13f * transform.lossyScale.y; } }
        public Transform RightHand { get { return bone[(int)B.HandR]; } }

        void Awake()
        {
            var names = System.Enum.GetNames(typeof(B));
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                int i = System.Array.IndexOf(names, t.name);
                if (i >= 0 && i < (int)B.Count) bone[i] = t;
            }
            for (int i = 0; i < (int)B.Count; i++)
            {
                restLocal[i] = bone[i].localRotation;
                restRel[i] = Quaternion.Inverse(transform.rotation) * bone[i].rotation;
            }
            hipsRest = transform.InverseTransformPoint(bone[(int)B.Hips].position);
            skin = GetComponentInChildren<SkinnedMeshRenderer>(true);
            block = new MaterialPropertyBlock();
            seed = name.GetHashCode() % 97 * .13f;
            blinkAt = Random.Range(1f, 4f);
        }

        public void Gesture(string kind, float seconds = 1.2f)
        {
            gesture = kind;
            gestureLength = seconds;
            gestureUntil = time + seconds;
        }

        void Pose(B b, float x, float y, float z)
        {
            int i = (int)b;
            bone[i].localRotation = restLocal[i] * (Quaternion.Inverse(restRel[i]) * Quaternion.Euler(x, y, z) * restRel[i]);
        }

        void LateUpdate()
        {
            float dt = Time.deltaTime;
            time += dt;
            if (!skin.isVisible) return;
            amount = Mathf.MoveTowards(amount, Mathf.Clamp01(moveSpeed / 2.4f), dt * 7);
            run = Mathf.MoveTowards(run, Mathf.InverseLerp(3.2f, 5.2f, moveSpeed), dt * 5);
            air = Mathf.MoveTowards(air, airborne ? 1 : 0, dt * 8);
            float stride = Mathf.Lerp(1.2f, 2.2f, run);
            phase = Mathf.Repeat(phase + moveSpeed * dt / stride * Mathf.PI * 2, Mathf.PI * 2);
            float s = Mathf.Sin(phase), c = Mathf.Cos(phase);
            if (footfall != null && amount > .4f && Mathf.Abs(s) > .96f && time - lastFoot > .22f) { lastFoot = time; footfall(); }
            float breath = reducedMotion ? 0 : Mathf.Sin(time * 1.7f + seed);
            float sway = reducedMotion ? 0 : Mathf.Sin(time * .6f + seed);

            float swing = Mathf.Lerp(24, 44, run) * amount, lift = Mathf.Lerp(38, 86, run) * amount;
            float legL = -swing * s, legR = swing * s;
            float kneeL = lift * Mathf.Max(0, Mathf.Cos(phase + .3f)) + 7 * amount;
            float kneeR = lift * Mathf.Max(0, Mathf.Cos(phase + Mathf.PI + .3f)) + 7 * amount;
            float idleWeight = (1 - amount) * sway;

            legL += Mathf.Lerp(0, -32, air); kneeL += Mathf.Lerp(0, 48, air);
            legR += Mathf.Lerp(0, 14, air); kneeR += Mathf.Lerp(0, 26, air);
            Pose(B.LegL, legL, 0, idleWeight * 1.2f - 1.5f);
            Pose(B.LegR, legR, 0, idleWeight * 1.2f + 1.5f);
            Pose(B.ShinL, kneeL, 0, 0);
            Pose(B.ShinR, kneeR, 0, 0);
            Pose(B.FootL, -(legL + kneeL) * .55f, 0, 0);
            Pose(B.FootR, -(legR + kneeR) * .55f, 0, 0);

            float bob = amount * (Mathf.Lerp(-.03f, -.05f, run) + Mathf.Lerp(.03f, .055f, run) * (.5f + .5f * Mathf.Cos(phase * 2)));
            float shift = amount * .014f * -c + (1 - amount) * sway * .012f;
            bone[(int)B.Hips].position = transform.TransformPoint(hipsRest + new Vector3(shift, bob + breath * .002f, 0));
            float lean = amount * 3 + run * 11;
            Pose(B.Hips, lean * .35f, 6 * s * amount, -3 * c * amount + idleWeight);
            Pose(B.Spine, lean * .35f + breath * .35f, -4 * s * amount, 0);
            Pose(B.Chest, lean * .3f + breath * .55f, -9 * s * amount, 2 * c * amount - idleWeight);

            float armSwing = Mathf.Lerp(20, 40, run) * amount;
            float armL = armSwing * s, armR = -armSwing * s;
            float foreL = -(10 + run * 62 + 12 * amount * Mathf.Max(0, s));
            float foreR = -(10 + run * 62 + 12 * amount * Mathf.Max(0, -s));
            float raise = air * 24;
            float zL = -(3 + breath * .5f + raise), zR = 3 + breath * .5f + raise;
            float gestureT = time < gestureUntil ? Mathf.Clamp01(Mathf.Min((gestureUntil - time) * 3, (gestureLength - (gestureUntil - time)) * 4)) : 0;
            if (gestureT > 0)
            {
                float beat = reducedMotion ? 0 : Mathf.Sin(time * 8);
                switch (gesture)
                {
                    case "wave": armR = Mathf.Lerp(armR, -24, gestureT); zR = Mathf.Lerp(zR, 150 + beat * 16, gestureT); foreR = Mathf.Lerp(foreR, -24, gestureT); break;
                    case "reach": armR = Mathf.Lerp(armR, -68, gestureT); foreR = Mathf.Lerp(foreR, -18, gestureT); break;
                    case "turn": armR = Mathf.Lerp(armR, -80, gestureT); armL = Mathf.Lerp(armL, -80, gestureT); foreR = Mathf.Lerp(foreR, -30 + beat * 14, gestureT); foreL = Mathf.Lerp(foreL, -30 - beat * 14, gestureT); break;
                    case "nod": break;
                }
            }
            else if (talking)
            {
                float beat = reducedMotion ? 0 : Mathf.Sin(time * 3.2f + seed);
                foreR = Mathf.Lerp(foreR, -46 + beat * 13, 1 - amount);
                armR = Mathf.Lerp(armR, -12 + beat * 5, 1 - amount);
            }
            Pose(B.ArmL, armL, 0, zL);
            Pose(B.ArmR, armR, 0, zR);
            Pose(B.ForeL, foreL, 0, 0);
            Pose(B.ForeR, foreR, 0, 0);
            Pose(B.HandL, 4, 0, 0);
            Pose(B.HandR, 4, 0, 0);

            float yaw = 0, pitch = 0;
            if (hasLook)
            {
                Vector3 local = transform.InverseTransformPoint(lookPoint) - transform.InverseTransformPoint(bone[(int)B.Head].position);
                yaw = Mathf.Clamp(Mathf.Atan2(local.x, Mathf.Max(local.z, .01f)) * Mathf.Rad2Deg, -62, 62);
                pitch = Mathf.Clamp(-Mathf.Atan2(local.y, new Vector2(local.x, local.z).magnitude) * Mathf.Rad2Deg, -24, 26);
                if (local.z < -.1f) yaw = 0;
            }
            float ease = 1 - Mathf.Exp(-dt * 8);
            lookYaw = Mathf.Lerp(lookYaw, yaw, ease);
            lookPitch = Mathf.Lerp(lookPitch, pitch, ease);
            float nod = gestureT > 0 && gesture == "nod" ? Mathf.Sin(time * 9) * 8 * gestureT : talking && !reducedMotion ? Mathf.Sin(time * 2.6f + seed) * 2.2f : breath * .5f;
            Pose(B.Neck, lookPitch * .4f - lean * .3f, lookYaw * .4f, 0);
            Pose(B.Head, lookPitch * .6f + nod, lookYaw * .6f, talking ? sway * 2 : sway * .8f);
            Pose(B.Pony, -run * 26 - amount * 8 * Mathf.Abs(s) + Mathf.Sin(time * 2 + seed) * 3, Mathf.Sin(phase) * 9 * amount, 0);
            Face();
        }

        void Face()
        {
            if (time > blinkAt) { blinkUntil = time + .11f; blinkAt = time + Random.Range(2.2f, 5.5f); }
            int want = time < blinkUntil ? Blink : talking && Mathf.Repeat(time * 7.5f, 1) < .5f ? Talk : expression;
            if (want == cell) return;
            cell = want;
            skin.GetPropertyBlock(block, 1);
            block.SetVector(FaceUV, new Vector4(.25f, .5f, (cell % 4) * .25f, cell < 4 ? .5f : 0));
            skin.SetPropertyBlock(block, 1);
        }
    }
}
