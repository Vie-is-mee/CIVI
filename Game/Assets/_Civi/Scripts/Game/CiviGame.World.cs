using System;
using System.Collections.Generic;
using UnityEngine;
using Civi.Story;

namespace Civi
{
    public sealed partial class CiviGame
    {
        enum Place { Title, Floor, Home, Ward }

        sealed class Door
        {
            public string id;
            public Transform a, b;
            public Vector3 aHome, bHome, axisA, pos;
            public float open;
            public bool moving;
        }

        sealed class Resident
        {
            public string id, name, text;
            public CiviBody body;
            public bool on;
        }

        struct Room { public string name; public Rect r; public string amb; public Room(string n, float x0, float y0, float x1, float y1, string a) { name = n; r = Rect.MinMaxRect(x0, y0, x1, y1); amb = a; } }

        // x/z world rectangles. Same numbers as tools/blender/build_world.py.
        static readonly Room[] Rooms =
        {
            new Room("Cơ quan Điều phối", -26, -9, -1, 9, "C01_03_F"),
            new Room("Phòng lịch sử", -1, -9, 15, 9, "C01_06__hall"),
            new Room("Căng tin", 15, -9, 33, 9, "C01_07__cafeteria"),
            new Room("Phòng họp", 33, -9, 48, 9, "C01_09__meeting_office_F"),
            new Room("Sân thượng", 14, 9, 34, 26, "C01_06__air"),
            new Room("Hành lang kỹ thuật", -26, -14.5f, -2, -9, "C01_08__hallway_minus12"),
        };
        static readonly string[] SceneAmbience =
        {
            "", "C01_01__hospital", "C01_02_F_full_68s", "C01_03_F", "C01_04_F", "C01_05_F", "C01_06__hall", "C01_07__cafeteria",
            "C01_08__hallway_minus12", "C01_09__meeting_office_F", "C01_10__home_room", "C01_10__home_room", "C01_11__home_after_hours_minus12"
        };
        static readonly string[] ChapterNames =
        {
            "Thành phố công bằng", "Những chiếc ô hỏng", "Người có điểm bằng không", "Một ngày dưới mái vòm", "Giá của một mạng người",
            "Một hệ thống đúng đắn", "Những con số biết nói dối", "Ngày mái vòm đóng lại", "Quyền được tồn tại", "Sau cơn mưa"
        };

        Transform worldRoot, floorRoot, homeArea, homeGlass, homeRoof, wardArea, wardRoof, cityRoot, tram, drone, cat, catTail, castRoot, propsRoot;
        readonly Dictionary<string, Transform> marks = new Dictionary<string, Transform>();
        readonly Dictionary<string, CiviBody> bodies = new Dictionary<string, CiviBody>();
        readonly Dictionary<string, Door> doors = new Dictionary<string, Door>();
        readonly List<Resident> residents = new List<Resident>();
        readonly List<Light> floorLights = new List<Light>(), homeLights = new List<Light>(), wardLights = new List<Light>();
        Place Location = Place.Title;
        Material env, sky;
        Light sun;
        Vector3 tramCentre, strollA, strollB;
        float tramAngle, strollWait;
        bool strollBack;
        string roomLabel = "";
        int lastAudioLine = -1;

        Transform Mark(string name)
        {
            if (!marks.TryGetValue(name, out var t)) throw new InvalidOperationException("Missing marker " + name);
            return t;
        }

        void BootWorld()
        {
            worldRoot = GameObject.Find("World").transform;
            castRoot = GameObject.Find("Cast").transform;
            sun = RenderSettings.sun;
            sky = RenderSettings.skybox;
            floorRoot = worldRoot.Find("Floor42");
            var home = worldRoot.Find("Home");
            homeArea = home.Find("Home"); homeGlass = home.Find("Home_Glass"); homeRoof = home.Find("Home_Roof");
            wardArea = home.Find("Ward"); wardRoof = home.Find("Ward_Roof");
            cityRoot = worldRoot.Find("City");
            tram = cityRoot.Find("Tram");
            propsRoot = worldRoot.Find("Props");
            drone = worldRoot.Find("Civi"); // a single-object FBX imports with the mesh as its root
            drone.localScale = Vector3.one * .62f;
            cat = worldRoot.Find("Cat/Cat");
            catTail = worldRoot.Find("Cat/CatTail");
            catTail.SetParent(cat, true);
            env = cityRoot.Find("City").GetComponent<Renderer>().sharedMaterial;
            foreach (var t in worldRoot.GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith("M_")) continue;
                if (t.name.StartsWith("M_LIGHT_"))
                {
                    bool warm = t.name.StartsWith("M_LIGHT_warm");
                    var go = new GameObject(t.name, typeof(Light));
                    go.transform.position = t.position;
                    var l = go.GetComponent<Light>();
                    l.type = LightType.Point;
                    l.range = warm ? 11 : 9;
                    l.color = warm ? new Color(1, .82f, .58f) : new Color(.68f, .93f, 1);
                    l.shadows = LightShadows.None;
                    bool ward = t.position.x > 60 && t.position.y < -100;
                    (t.IsChildOf(floorRoot) ? floorLights : ward ? wardLights : homeLights).Add(l);
                }
                else marks[t.name] = t;
            }
            tramCentre = Mark("M_CITY_centre").position;
            foreach (Transform t in floorRoot)
            {
                if (!t.name.StartsWith("Door_") || !t.name.EndsWith("_L")) continue;
                string id = t.name.Substring(5, t.name.Length - 7);
                var b = floorRoot.Find("Door_" + id + "_R");
                var d = new Door { id = id, a = t, b = b, aHome = t.position, bHome = b.position, pos = Mark("M_DOOR_" + id).position };
                d.axisA = (d.aHome - d.pos).normalized;
                doors[id] = d;
            }
            foreach (Transform t in castRoot)
            {
                t.gameObject.SetActive(true);
                var body = t.gameObject.AddComponent<CiviBody>();
                body.reducedMotion = reducedMotion;
                bodies[t.name] = body;
                t.gameObject.SetActive(false);
            }
            AddResident("truc", "Nhân viên trực");
            AddResident("dong_nghiep_b", "Đồng nghiệp");
            AddResident("linh", "Linh");
            AddResident("dong_nghiep_a", "Đồng nghiệp cũ");
            AddResident("huong_dan", "Hướng dẫn viên");
            AddResident("hoc_sinh", "Học sinh");
            AddResident("vinh", "Vinh");
            AddResident("me_mai", "Mẹ Mai");
            strollA = Mark("M_NPC_dong_nghiep_office").position;
            strollB = strollA + new Vector3(-10.5f, 0, 0);
        }

        void AddResident(string id, string name) { residents.Add(new Resident { id = id, name = name, body = bodies[id] }); }

        Resident ResidentOf(string id) { return residents.Find(r => r.id == id); }

        string ReviewedLine(int scene, string speaker, int occurrence = 0)
        {
            for (int i = 0; i < content.paragraphs.Length; i++)
                if (content.sceneIds[i] == scene && content.speakers[i] == speaker && content.cueIds[i].EndsWith(".p1", StringComparison.Ordinal))
                {
                    if (occurrence-- != 0) continue;
                    string text = content.paragraphs[i];
                    while (i + 1 < content.paragraphs.Length && content.sourceLines[i + 1] == content.sourceLines[i]) text += " " + content.paragraphs[++i];
                    return text;
                }
            throw new InvalidOperationException("Missing reviewed NPC dialogue: " + scene + " / " + speaker);
        }

        void Show(string id, bool on, Vector3 pos = default, float yaw = 0)
        {
            var body = bodies[id];
            if (body.gameObject.activeSelf != on) body.gameObject.SetActive(on);
            if (on) { body.transform.position = pos; body.transform.rotation = Quaternion.Euler(0, yaw, 0); body.expression = CiviBody.Neutral; body.hasLook = false; }
        }

        void Put(string id, bool on, string mark, float yaw, string text)
        {
            var r = ResidentOf(id);
            r.on = on;
            r.text = text;
            Show(id, on, on ? Mark(mark).position : default, yaw);
        }

        // Who stands where follows the story stage; rooms stay connected, so the cast is placed once per stage.
        void PlaceCast()
        {
            int s = cp == null ? 0 : cp.stage;
            bool floor = Location == Place.Floor, home = Location == Place.Home;
            Put("truc", floor, "M_NPC_truc_office", 180, "Bảng điều phối vẫn đang cập nhật các yêu cầu mới. Hồ sơ của chị được giữ ở bàn làm việc.");
            Put("dong_nghiep_b", floor, "M_NPC_dong_nghiep_office", 270, "Các yêu cầu mới vẫn đang đến. Chị cứ hoàn tất hồ sơ đang mở trước; tôi sẽ sắp lại các biên bản ở đây.");
            string linhMark = s <= 3 ? "M_NPC_linh_office" : s == 4 ? "M_NPC_linh_lunch" : "M_NPC_linh_meeting";
            string linhText = s == 0 ? "Tớ đang kiểm tra các đề nghị sửa trạm lọc. Xong lệnh điện, cậu ghé qua nhé."
                : s <= 3 ? ReviewedLine(3, "Linh", 2) : s == 4 ? ReviewedLine(7, "Linh") : "Bản đồ dự trữ đã được cập nhật. Các trạm lọc cũ vẫn cần được theo dõi cùng với cảnh báo bão.";
            Put("linh", floor, linhMark, s <= 3 ? 180 : 0, linhText);
            Put("dong_nghiep_a", floor, "M_NPC_dong_nghiep_canteen", 180, "Hồ sơ điều chuyển của tôi cuối cùng cũng được duyệt. Lát nữa trên đường họp, tôi muốn hỏi Mai về tuyến tàu gần khu nhà mới.");
            Put("huong_dan", floor, "M_NPC_huong_dan", 90, ReviewedLine(6, "Hướng dẫn viên"));
            Put("hoc_sinh", floor, "M_NPC_hoc_sinh", 0, "Em đang xem những tấm thẻ khí. Mỗi chiếc thẻ này từng cho một người được thở trong hai mươi bốn giờ ạ?");
            Put("vinh", floor, "M_NPC_vinh", 180, ReviewedLine(9, "Vinh", 2));
            Put("me_mai", home, "M_NPC_me_mai", 180, s <= 6 ? "Con về rồi à? Mẹ đang tỉa lại chậu hương thảo. Ngồi xuống kể mẹ nghe ngày hôm nay nhé." : ReviewedLine(10, "Mẹ Mai", 2));
            bool ward = Location == Place.Ward;
            Show("mai_nho", ward, ward ? Mark("M_WARD_mai").position : default, 270);
            Show("ao_xam", ward, ward ? Mark("M_WARD_grey").position : default, 200);
            Show("nv_benh_vien", ward, ward ? Mark("M_WARD_staff").position : default, 240);
            bool title = Location == Place.Title;
            Show("an", title, title ? Mark("M_TITLE_an").position : default, 188);
            Show("ha", title, title ? Mark("M_TITLE_ha").position : default, 205);
        }

        Place LocationFor()
        {
            if (cp.IsReading && content.sceneIds[LineIndex] == 1) return Place.Ward;
            return ChapterPlaces.ActiveRoom(cp) == "home" ? Place.Home : Place.Floor;
        }

        void SetLocation(Place place)
        {
            Location = place;
            bool floor = place == Place.Floor || place == Place.Title, home = place == Place.Home, ward = place == Place.Ward;
            floorRoot.gameObject.SetActive(floor);
            homeArea.gameObject.SetActive(home); homeGlass.gameObject.SetActive(home); homeRoof.gameObject.SetActive(home);
            wardArea.gameObject.SetActive(ward); wardRoof.gameObject.SetActive(ward);
            cityRoot.gameObject.SetActive(!ward);
            foreach (var l in floorLights) l.enabled = floor;
            foreach (var l in homeLights) l.enabled = home;
            foreach (var l in wardLights) l.enabled = ward;
        }

        void RefreshWorld(bool teleport)
        {
            var next = LocationFor();
            bool changed = next != Location;
            if (changed) { SetLocation(next); PlaceCast(); perf.Hold(2); }
            else PlaceCast();
            player.gameObject.SetActive(next != Place.Ward);
            drone.gameObject.SetActive(next != Place.Ward);
            cat.gameObject.SetActive(next == Place.Floor);
            SetMood();
            if (teleport) TeleportPlayer(new Vector3(cp.x, cp.y, cp.z), cp.yaw);
            else if (changed && next == Place.Home) { var s = Mark("M_SPAWN_home"); TeleportPlayer(s.position, 40); }
            else if (changed && next == Place.Floor) { var s = Mark("M_SPAWN_start"); if (cp.stage == 0) TeleportPlayer(s.position, 200); }
            if (next != Place.Ward) UpdateRoomAudio(true);
        }

        void UpdateRoomAudio(bool force)
        {
            if (!inGame) return;
            if (cp.IsReading)
            {
                int scene = content.sceneIds[LineIndex];
                hub.Ambience(SceneAmbience[scene]);
                int first = LineIndex, last = LineIndex;
                while (first > 0 && content.sceneIds[first - 1] == scene) first--;
                while (last < content.paragraphs.Length - 1 && content.sceneIds[last + 1] == scene) last++;
                if (LineIndex != lastAudioLine) { lastAudioLine = LineIndex; hub.OnLine(scene, LineIndex - first, last - first + 1); }
                return;
            }
            if (Location == Place.Home) { hub.Ambience("C01_10__home_room"); roomLabel = "Căn hộ của Mai"; return; }
            Vector2 p = new Vector2(player.position.x, player.position.z);
            foreach (var room in Rooms)
                if (room.r.Contains(p)) { hub.Ambience(room.amb); roomLabel = room.name; return; }
        }

        struct Mood
        {
            public Vector3 dir; public Color sunColor, top, horizon, bottom, ambSky, ambMid, ambLow, fog; public float sunPower, glow, stars, lamp;
        }

        static readonly Mood Morning = new Mood { dir = new Vector3(.46f, -.6f, -.66f), sunColor = new Color(1, .95f, .86f), sunPower = 1.25f, top = new Color(.24f, .56f, .92f), horizon = new Color(.84f, .94f, .98f), bottom = new Color(.56f, .74f, .82f),
            ambSky = new Color(.78f, .84f, .92f), ambMid = new Color(.74f, .74f, .72f), ambLow = new Color(.56f, .5f, .46f), fog = new Color(.8f, .91f, .96f), glow = .9f, stars = 0, lamp = 1.4f };
        static readonly Mood Noon = new Mood { dir = new Vector3(.18f, -.82f, -.54f), sunColor = new Color(1, .98f, .93f), sunPower = 1.3f, top = new Color(.2f, .52f, .94f), horizon = new Color(.86f, .95f, 1f), bottom = new Color(.58f, .76f, .84f),
            ambSky = new Color(.82f, .88f, .96f), ambMid = new Color(.78f, .78f, .76f), ambLow = new Color(.58f, .52f, .48f), fog = new Color(.82f, .92f, .98f), glow = .8f, stars = 0, lamp = 1.2f };
        static readonly Mood Late = new Mood { dir = new Vector3(-.62f, -.38f, -.68f), sunColor = new Color(1, .74f, .46f), sunPower = 1.25f, top = new Color(.3f, .4f, .72f), horizon = new Color(1f, .78f, .58f), bottom = new Color(.6f, .5f, .52f),
            ambSky = new Color(.66f, .64f, .78f), ambMid = new Color(.74f, .62f, .56f), ambLow = new Color(.48f, .38f, .36f), fog = new Color(.96f, .78f, .64f), glow = 1.5f, stars = 0, lamp = 3f };
        static readonly Mood NightSky = new Mood { dir = new Vector3(.3f, -.6f, -.74f), sunColor = new Color(.5f, .6f, 1f), sunPower = .32f, top = new Color(.03f, .05f, .16f), horizon = new Color(.14f, .22f, .44f), bottom = new Color(.06f, .09f, .18f),
            ambSky = new Color(.3f, .3f, .46f), ambMid = new Color(.32f, .27f, .3f), ambLow = new Color(.18f, .13f, .14f), fog = new Color(.1f, .15f, .3f), glow = 3.4f, stars = 1, lamp = 5.5f };
        static readonly Mood WardNight = new Mood { dir = new Vector3(.3f, -.7f, .6f), sunColor = new Color(.6f, .74f, .9f), sunPower = .15f, top = new Color(.03f, .05f, .1f), horizon = new Color(.05f, .08f, .12f), bottom = new Color(.03f, .05f, .08f),
            ambSky = new Color(.3f, .4f, .5f), ambMid = new Color(.24f, .32f, .38f), ambLow = new Color(.14f, .18f, .22f), fog = new Color(.05f, .08f, .12f), glow = 1.6f, stars = 0, lamp = 3.4f };

        void SetMood()
        {
            int s = cp == null ? 0 : cp.stage;
            var m = Location == Place.Ward ? WardNight : Location == Place.Title ? Morning : s >= 6 ? NightSky : s == 5 ? Late : s >= 3 ? Noon : Morning;
            sun.transform.rotation = Quaternion.LookRotation(m.dir);
            sun.color = m.sunColor; sun.intensity = m.sunPower;
            sky.SetColor("_Top", m.top); sky.SetColor("_Horizon", m.horizon); sky.SetColor("_Bottom", m.bottom);
            sky.SetVector("_SunDir", -m.dir); sky.SetFloat("_Stars", m.stars);
            sky.SetColor("_Cloud", s >= 6 ? new Color(.2f, .24f, .4f, .3f) : new Color(1, 1, 1, .8f));
            RenderSettings.ambientSkyColor = m.ambSky; RenderSettings.ambientEquatorColor = m.ambMid; RenderSettings.ambientGroundColor = m.ambLow;
            RenderSettings.fogColor = m.fog;
            env.SetFloat("_Emission", m.glow);
            foreach (var l in floorLights) l.intensity = m.lamp;
            foreach (var l in homeLights) l.intensity = m.lamp;
            foreach (var l in wardLights) l.intensity = m.lamp;
        }

        bool DoorUnlocked(string id)
        {
            if (!inGame) return false;
            int stage = cp.stage;
            bool arrived = string.IsNullOrEmpty(cp.travelFrom);
            switch (id)
            {
                case "A": return stage > 3 || (stage == 3 && arrived);
                case "B": return stage > 4 || (stage == 4 && arrived);
                case "M": return stage > 5 || (stage == 5 && arrived);
                case "T": return stage >= 4 && arrived || stage > 4;
                case "C": return Side.active.Contains("van_b4") || Side.finished.Contains("van_b4");
                default: return false;
            }
        }

        // Doors look automatic to the player; the story only decides whether they are live.
        string StoryDoorId()
        {
            if (cp == null || string.IsNullOrEmpty(cp.travelFrom)) return null;
            switch (ChapterPlaces.Room(cp.stage))
            {
                case "gallery-v2": return "A";
                case "cafeteria-v2": return "B";
                case "meeting-v2": return "M";
                case "home": return "L";
                default: return null;
            }
        }

        void UseStoryDoor()
        {
            string id = StoryDoorId();
            if (id == "L")
            {
                BeginTransition(() => { cp.travelFrom = null; RefreshWorld(false); SaveCheckpoint(); hub.Ui("UI_14_NOTIFICATION"); });
                return;
            }
            cp.travelFrom = null;
            hub.Sfx("C01_06__group_leaves", .4f);
            hub.Ui("UI_06_MENU_OPEN", .7f);
            SaveCheckpoint();
        }

        void TickWorld()
        {
            float dt = Time.deltaTime;
            tramAngle += dt * .035f * (reducedMotion ? .3f : 1);
            tram.position = tramCentre + new Vector3(Mathf.Cos(tramAngle), 0, Mathf.Sin(tramAngle)) * 300 + Vector3.up * 6;
            tram.rotation = Quaternion.Euler(0, -tramAngle * Mathf.Rad2Deg, 0);
            if (Location == Place.Floor)
            {
                foreach (var d in doors.Values)
                {
                    bool near = (d.pos - player.position).sqrMagnitude < 25;
                    float want = DoorUnlocked(d.id) && near ? 1 : 0;
                    if (Mathf.Approximately(d.open, want)) continue;
                    d.open = Mathf.MoveTowards(d.open, want, dt * 2.6f);
                    float eased = Mathf.SmoothStep(0, 1, d.open) * 1.5f;
                    d.a.position = d.aHome + d.axisA * eased;
                    d.b.position = d.bHome - d.axisA * eased;
                }
                TickStroll(dt);
            }
            if (inGame && Location != Place.Ward && !cp.IsReading) UpdateRoomAudio(false);
            else if (inGame && cp.IsReading) UpdateRoomAudio(false);
            foreach (var r in residents)
            {
                if (!r.on) continue;
                bool close = (r.body.transform.position - player.position).sqrMagnitude < 12;
                r.body.hasLook = close || r == talking;
                r.body.lookPoint = playerBody.HeadPoint;
            }
            TickDrone(dt);
        }

        void TickStroll(float dt)
        {
            var r = ResidentOf("dong_nghiep_b");
            var t = r.body.transform;
            if (r == talking || (t.position - player.position).sqrMagnitude < 6 || (cp != null && cp.IsReading)) { r.body.moveSpeed = 0; return; }
            if (strollWait > 0) { strollWait -= dt; r.body.moveSpeed = 0; return; }
            Vector3 target = strollBack ? strollA : strollB;
            Vector3 to = target - t.position;
            if (to.sqrMagnitude < .04f) { strollBack = !strollBack; strollWait = 5; r.body.moveSpeed = 0; return; }
            t.position = Vector3.MoveTowards(t.position, target, 1.25f * dt);
            t.rotation = Quaternion.RotateTowards(t.rotation, Quaternion.LookRotation(to), 240 * dt);
            r.body.moveSpeed = 1.25f;
        }

        void TickDrone(float dt)
        {
            if (!drone.gameObject.activeSelf) return;
            float t = Time.time;
            Vector3 anchor = Location == Place.Title ? Mark("M_TITLE_mai").position + new Vector3(.75f, 2.1f, -.3f)
                : player.position + player.forward * .35f + player.right * .78f + Vector3.up * (1.72f + Mathf.Sin(t * 1.7f) * .05f);
            drone.position = Vector3.Lerp(drone.position, anchor, 1 - Mathf.Exp(-dt * 3.2f));
            Vector3 look = cp != null && cp.IsReading ? view.transform.position - drone.position : (Location == Place.Title ? view.transform.position - drone.position : player.forward);
            look.y = 0;
            if (look.sqrMagnitude > .01f) drone.rotation = Quaternion.Slerp(drone.rotation, Quaternion.LookRotation(-look), 1 - Mathf.Exp(-dt * 4));
        }
    }
}
