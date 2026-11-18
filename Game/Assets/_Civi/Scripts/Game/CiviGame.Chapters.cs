using System.Collections.Generic;
using UnityEngine;
using Civi.Story;

namespace Civi
{
    // Chapters 2 and 3. Same story machine as chapter 1, but every step carries its own place, cast and action in chapter.json,
    // so the writing can move without touching code.
    public sealed partial class CiviGame
    {
        readonly ChapterContent[] chapters = new ChapterContent[4];
        readonly List<Light> edgeLights = new List<Light>(), hallLights = new List<Light>(), trainLights = new List<Light>(), classLights = new List<Light>();
        Transform edgeRoot;
        ParticleSystem drips;

        sealed class Walker { public CiviBody body; public Vector3 target; public float yaw; }
        readonly List<Walker> walkers = new List<Walker>();

        bool Episode { get { return content != null && content.number > 1; } }
        ChapterStep EpisodeStep { get { return content.steps[Mathf.Min(cp.stage, content.steps.Length - 1)]; } }

        static readonly Mood EdgeDay = new Mood { dir = new Vector3(.2f, -.9f, .3f), sunColor = new Color(.74f, .84f, .86f), sunPower = .55f, top = new Color(.1f, .13f, .15f), horizon = new Color(.2f, .25f, .27f), bottom = new Color(.08f, .1f, .11f),
            ambSky = new Color(.5f, .57f, .6f), ambMid = new Color(.42f, .45f, .46f), ambLow = new Color(.24f, .24f, .24f), fog = new Color(.26f, .31f, .33f), glow = 1.2f, stars = 0, lamp = 2.2f };
        static readonly Mood EdgeNight = new Mood { dir = new Vector3(.2f, -.9f, .3f), sunColor = new Color(.34f, .42f, .56f), sunPower = .16f, top = new Color(.03f, .04f, .06f), horizon = new Color(.07f, .09f, .12f), bottom = new Color(.03f, .04f, .05f),
            ambSky = new Color(.18f, .21f, .29f), ambMid = new Color(.17f, .18f, .22f), ambLow = new Color(.1f, .09f, .1f), fog = new Color(.08f, .1f, .14f), glow = 1.6f, stars = 0, lamp = 3.6f };
        static readonly Mood Indoors = new Mood { dir = new Vector3(.3f, -.8f, .4f), sunColor = new Color(.94f, .94f, .9f), sunPower = .25f, top = new Color(.4f, .42f, .44f), horizon = new Color(.5f, .52f, .54f), bottom = new Color(.3f, .3f, .3f),
            ambSky = new Color(.74f, .76f, .76f), ambMid = new Color(.66f, .66f, .64f), ambLow = new Color(.44f, .43f, .4f), fog = new Color(.6f, .62f, .62f), glow = 1, stars = 0, lamp = 1.6f };
        static readonly Mood Basement = new Mood { dir = new Vector3(.5f, -.6f, .3f), sunColor = new Color(.95f, .86f, .7f), sunPower = .4f, top = new Color(.2f, .2f, .2f), horizon = new Color(.3f, .28f, .26f), bottom = new Color(.16f, .15f, .14f),
            ambSky = new Color(.46f, .44f, .4f), ambMid = new Color(.42f, .39f, .35f), ambLow = new Color(.26f, .23f, .2f), fog = new Color(.3f, .28f, .26f), glow = 1.4f, stars = 0, lamp = 2.6f };

        void LoadChapters()
        {
            chapters[1] = content;
            for (int n = 2; n <= 3; n++)
            {
                var json = Resources.Load<TextAsset>("Chapter" + n + "/chapter");
                if (json == null) throw new System.InvalidOperationException("Missing chapter " + n);
                chapters[n] = JsonUtility.FromJson<ChapterContent>(json.text);
                ValidateContent(chapters[n]);
            }
        }

        void BootChapters()
        {
            edgeRoot = worldRoot.Find("Edge");
            drips = GameObject.Find("Drips").GetComponent<ParticleSystem>();
            drips.gameObject.SetActive(false);
        }

        // Lights are read by name in BootWorld; only those of the new places land here.
        bool TakeLight(Transform t, Light l)
        {
            string n = t.name;
            if (n.StartsWith("M_LIGHT_edge")) { l.color = new Color(1, .84f, .62f); l.range = 12; edgeLights.Add(l); return true; }
            if (n.StartsWith("M_LIGHT_hall")) { l.color = new Color(1, .98f, .92f); l.range = 12; hallLights.Add(l); return true; }
            if (n.StartsWith("M_LIGHT_train")) { l.color = new Color(1, .97f, .9f); l.range = 12; trainLights.Add(l); return true; }
            if (n.StartsWith("M_LIGHT_class")) { l.color = new Color(1, .86f, .66f); l.range = 8; classLights.Add(l); return true; }
            return false;
        }

        Place EpisodePlace()
        {
            switch (EpisodeStep.place)
            {
                case "edge": return Place.Edge;
                case "hall": return Place.Hall;
                case "train": return Place.Train;
                case "class": return Place.Class;
                default: return Place.Floor;
            }
        }

        void ShowEpisodePlace(Place place)
        {
            Toggle("Edge", place == Place.Edge);
            Toggle("Hall", place == Place.Hall);
            Toggle("Train", place == Place.Train);
            Toggle("Class", place == Place.Class);
            foreach (var l in edgeLights) l.enabled = place == Place.Edge;
            foreach (var l in hallLights) l.enabled = place == Place.Hall;
            foreach (var l in trainLights) l.enabled = place == Place.Train;
            foreach (var l in classLights) l.enabled = place == Place.Class;
            drips.gameObject.SetActive(place == Place.Edge && !reducedMotion);
        }

        void Toggle(string area, bool on)
        {
            foreach (string suffix in new[] { "", "_Glass", "_Roof" })
            {
                var t = edgeRoot.Find(area + suffix);
                if (t != null) t.gameObject.SetActive(on);
            }
        }

        Mood EpisodeMood()
        {
            string mood = EpisodeStep.mood;
            switch (Location)
            {
                case Place.Edge: return mood == "edge_night" ? EdgeNight : EdgeDay;
                case Place.Hall: case Place.Train: return Indoors;
                case Place.Class: return Basement;
                default: return mood == "night" ? NightSky : Morning;
            }
        }

        void PlaceEpisodeCast()
        {
            var wanted = new HashSet<string>();
            foreach (string entry in EpisodeStep.cast)
            {
                int at = entry.IndexOf('@');
                string id = entry.Substring(0, at);
                var mark = Mark(entry.Substring(at + 1));
                wanted.Add(id);
                var body = bodies[id];
                float yaw = -mark.eulerAngles.y;
                // Someone already in the scene walks to the next place instead of popping there.
                if (body.gameObject.activeSelf && (body.transform.position - mark.position).sqrMagnitude < 2500)
                {
                    walkers.RemoveAll(w => w.body == body);
                    if ((body.transform.position - mark.position).sqrMagnitude > .04f) walkers.Add(new Walker { body = body, target = mark.position, yaw = yaw });
                }
                else
                {
                    body.gameObject.SetActive(true);
                    body.transform.SetPositionAndRotation(mark.position, Quaternion.Euler(0, yaw, 0));
                }
            }
            foreach (var r in residents) r.on = false;
            foreach (var pair in bodies)
                if (pair.Value != playerBody && !wanted.Contains(pair.Key)) pair.Value.gameObject.SetActive(false);
            walkers.RemoveAll(w => !w.body.gameObject.activeSelf);
        }

        void TickEpisode(float dt)
        {
            for (int i = walkers.Count - 1; i >= 0; i--)
            {
                var w = walkers[i];
                var t = w.body.transform;
                Vector3 to = w.target - t.position;
                to.y = 0;
                if (to.sqrMagnitude < .02f || cp.IsReading)
                {
                    t.position = w.target;
                    t.rotation = Quaternion.Euler(0, w.yaw, 0);
                    w.body.moveSpeed = 0;
                    walkers.RemoveAt(i);
                    continue;
                }
                t.position += to.normalized * Mathf.Min(2.8f * dt, to.magnitude);
                t.rotation = Quaternion.RotateTowards(t.rotation, Quaternion.LookRotation(to), 360 * dt);
                w.body.moveSpeed = 2.8f;
            }
            if (Location == Place.Edge) drips.transform.position = player.position + Vector3.up * 9;
        }

        void StartEpisodeStep()
        {
            var s = EpisodeStep;
            if (!string.IsNullOrEmpty(s.start))
            {
                var m = Mark(s.start);
                TeleportPlayer(m.position, -m.eulerAngles.y);
            }
            RefreshWorld(false);
            if (cp.phase == "explore" && Step != null && s.auto) BeginObjective();
        }

        void EpisodeStageChanged()
        {
            if (Step == null) return;
            var s = Step;
            if (s.auto || !string.IsNullOrEmpty(s.start)) BeginTransition(StartEpisodeStep);
            else RefreshWorld(false);
        }

        public void BeginChapter(int number)
        {
            BeginTransition(() =>
            {
                var side = cp.side;
                content = chapters[number];
                cp = ChapterCheckpoint.New(content.sourceHash, number);
                cp.side = side;
                foreach (var p in pickups) p.go.SetActive(false);
                foreach (var c in chests) c.Value.SetActive(false);
                lastAudioLine = -1;
                StartEpisodeStep();
                SaveCheckpoint();
            });
        }

        void EpisodeAudio()
        {
            bool reading = cp.IsReading;
            if (reading && LineIndex != lastAudioLine) { lastAudioLine = LineIndex; hub.OnLine(content.number, LineIndex); }
            hub.StoryBeds(content.number, Mathf.Min(cp.stage, content.steps.Length - 1), reading ? LineIndex : -1);
            switch (Location)
            {
                case Place.Edge: roomLabel = "Rìa Mưa"; break;
                case Place.Hall: roomLabel = "Sảnh tiếp dân"; break;
                case Place.Train: roomLabel = "Tàu về Lõi Sáng"; break;
                case Place.Class: roomLabel = "Lớp cộng đồng"; break;
                default: roomLabel = "Cơ quan Điều phối"; break;
            }
        }

        static readonly Rect[] Puddles =
        {
            new Rect(2.5f, 1.5f, 3.4f, 5), new Rect(3.1f, 13, 3, 4), new Rect(-4.5f, 6.7f, 1.8f, 2.6f), new Rect(-.8f, 41.1f, 1.2f, 1.8f), new Rect(2.4f, 54.5f, 2.4f, 3), new Rect(-5.9f, 57.4f, 1.8f, 1.2f),
        };

        // Which recorded surface Mai is on. Numbers are the local layout of tools/blender/build_edge.py.
        string FootBank()
        {
            bool run = playerBody.moveSpeed > 4;
            string hard = run ? "HARD_DRY_RUN" : "HARD_DRY_WALK";
            if (Location != Place.Edge) return hard;
            Vector2 p = new Vector2(player.position.x - 420, player.position.z + 300);
            string wet = run ? "WET_HARD_RUN" : "WET_HARD_WALK";
            if (Mathf.Abs(p.x) < 2.3f && p.y > 24.6f && p.y < 29.4f) return run ? "HARD_DRY_RUN" : "PLASTIC_PANEL_WALK"; // the package has no plastic run
            foreach (var r in Puddles) if (r.Contains(p)) return run ? "WET_HARD_RUN" : "SHALLOW_WATER_WALK";
            if (new Rect(-12.2f, 51.2f, 6.4f, 5.6f).Contains(p) || new Rect(-10.7f, 68.7f, 5.4f, 4.6f).Contains(p)) return hard; // indoors at Hạ's and bà Dung's
            if (new Rect(2.4f, 49, 12, 12).Contains(p)) return hard; // filter station
            if (new Rect(8, 38, 12, 8).Contains(p)) return run ? "WET_HARD_RUN" : "MUD_SOIL_WALK"; // the football yard
            return wet;
        }

        static readonly Dictionary<string, string> Actors = new Dictionary<string, string>
        {
            {"An", "an"}, {"Hạ", "ha"}, {"Tín", "tin"}, {"Bà Dung", "dung"}, {"Bà bán cháo", "ba_chao"}, {"Ông lão", "ong_lao"}, {"Quản đốc ca đêm", "quan_doc"},
            {"Cô Nhi", "nhi"}, {"Người đàn ông", "dan_ong"}, {"Nhân viên hướng dẫn", "truc"}, {"Nhân viên tiếp dân", "truc"}, {"Cán bộ xét duyệt", "dong_nghiep_a"}, {"Một học sinh", "hoc_sinh"},
        };

        // Objectives often sit on a person: nobody starts a conversation standing inside the other one.
        void KeepDistance()
        {
            if (!player.gameObject.activeInHierarchy) return;
            var other = Episode ? NearestCastBody() : NearestResidentBody();
            if (other == null) return;
            Vector3 away = player.position - other.transform.position;
            away.y = 0;
            if (away.sqrMagnitude > 1.2f * 1.2f) return;
            away = away.sqrMagnitude < .01f ? -player.forward : away.normalized;
            controller.enabled = false;
            player.position = new Vector3(other.transform.position.x, player.position.y, other.transform.position.z) + away * 1.2f;
            controller.enabled = true;
        }

        CiviBody NearestCastBody()
        {
            CiviBody best = null; float bestDistance = 64;
            foreach (var body in bodies.Values)
            {
                if (body == playerBody || !body.gameObject.activeSelf) continue;
                float d = (body.transform.position - player.position).sqrMagnitude;
                if (d < bestDistance) { bestDistance = d; best = body; }
            }
            return best;
        }

        // A scene Mai is not part of is watched from a fixed place, cutting to whoever speaks.
        bool WatchedShot(CiviBody speaker, out Vector3 pos, out Quaternion rot)
        {
            pos = default; rot = default;
            if (!Episode || string.IsNullOrEmpty(EpisodeStep.cam)) return false;
            if (speaker != null) lastSpeaker = speaker;
            var main = lastSpeaker != null && lastSpeaker.gameObject.activeSelf ? lastSpeaker : NearestCastBody();
            pos = Mark(EpisodeStep.cam).position;
            if (main == null) { rot = Quaternion.identity; return true; }
            Vector3 head = main.HeadPoint;
            Vector3 toward = (pos - head);
            // pull in along the sight line, so a speaker across the room still fills the frame
            if (toward.magnitude > 2.4f) pos = head + toward.normalized * 2.4f;
            rot = Quaternion.LookRotation(head + Vector3.down * .08f - pos);
            foreach (var body in bodies.Values)
                if (body != main && body.gameObject.activeSelf && body != playerBody) FaceEachOther(body, main);
            return true;
        }
    }
}
