using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Civi.Story;

namespace Civi
{
    // Exploration layer: pickups, side quests and the web demo's minigames. None of it can block or change the main story.
    public sealed partial class CiviGame
    {
        sealed class Spot
        {
            public string label;
            public Func<string> labelFn;
            public Func<Vector3> at;
            public float range = 2.2f;
            public Func<bool> on;
            public Action use;
            public int rank = 3;
            public string Text { get { return labelFn != null ? labelFn() : label; } }
        }

        sealed class Pickup { public string id, item; public GameObject go; public Vector3 home; public bool lensOnly, orb; }
        sealed class Toast { public string title, detail; public float until; }

        readonly List<Spot> spots = new List<Spot>();
        readonly List<Pickup> pickups = new List<Pickup>();
        readonly List<Toast> toasts = new List<Toast>();
        readonly Dictionary<string, GameObject> chests = new Dictionary<string, GameObject>();
        Spot focus;
        Resident talking;
        SideQuest talkQuest;
        string talkText = "", placeTitle = "", placeText = "", bubble = "";
        float bubbleUntil, alarmUntil, lens, nextBark = 30;
        bool lensOn;
        const float AlarmSeconds = 40;
        const int OrbTotal = 12;

        void BootSide()
        {
            foreach (Transform t in propsRoot) t.gameObject.SetActive(false);
            spots.Add(new Spot
            {
                labelFn = () => StoryDoorId() == "L" ? "Đi thang máy về nhà" : "Sang " + RoomName(ChapterPlaces.Room(cp.stage)),
                at = () => StoryDoorId() == "L" ? Mark("M_LIFT").position : doors[StoryDoorId()].pos,
                range = 3.4f, rank = 0, on = () => StoryDoorId() != null, use = UseStoryDoor
            });
            spots.Add(new Spot { labelFn = () => Step.title, at = ObjectivePos, range = 2.4f, rank = 1, on = () => ObjectiveOpen, use = () => BeginObjective() });
            foreach (var r in residents)
            {
                var who = r;
                spots.Add(new Spot { labelFn = () => "Trò chuyện với " + who.name, at = () => who.body.transform.position, range = 2.3f, rank = 4, on = () => who.on, use = () => TalkTo(who) });
            }
            AddPlace("M_SEE_office_board", "Bảng điều phối", "Những vòng đèn xanh đánh dấu các trạm lọc đang hoạt động. Từng yêu cầu được đối chiếu với nhu cầu của con người phía sau hồ sơ.");
            AddPlace("M_SEE_office_window", "Góc cửa kính", "Xa về phía Đông, những mái nhà thấp khuất trong hơi nước. Trên bản đồ quy hoạch, nơi ấy mang tên Rìa Mưa.");
            AddPlace("M_SEE_gallery_card", "Thẻ khí cũ", "Một chiếc thẻ từng quyết định quyền được thở trong hai mươi bốn giờ. Những hiện vật này được giữ lại để người đến sau nhớ cách thành phố đã sống.");
            AddPlace("M_SEE_gallery_book", "Sổ trưng bày", "Những mốc thay đổi quy định được đặt cạnh các hiện vật đời thường. Học sinh đang nghe câu chuyện của một thời trước.");
            AddPlace("M_SEE_canteen_tea", "Quầy trà", "Những cốc trà ấm chờ người đến lấy. Giờ nghỉ trưa là khoảng lặng ngắn giữa các hồ sơ điều phối.");
            AddPlace("M_SEE_canteen_window", "Cửa nhìn ra mái vòm", "Bên ngoài kính, các tuyến xe điện vẫn nối những tầng nhà. Thành phố tiếp tục chuyển động trong giờ nghỉ.");
            AddPlace("M_SEE_meeting_map", "Bản đồ năng lượng", "Các vùng dự trữ và trạm lọc được đánh dấu trên cùng một bản đồ. Cảnh báo thời tiết vẫn cần được đối chiếu với những gì đang xảy ra ở từng khu phố.");
            AddPlace("M_SEE_meeting_file", "Tập biên bản", "Hồ sơ phân khu 8-12 vẫn ở đây, giữa những con số đang chờ được kiểm chứng.");
            AddPlace("M_SEE_home_plant", "Chậu hương thảo", "Mẹ Mai tỉa lại những nhánh cây bên cửa. Mùi hương thảo rất nhẹ trong căn hộ buổi tối.");
            AddPlace("M_SEE_home_window", "Khung cửa buổi tối", "Những tầng nhà sáng đèn dưới mái vòm. Ngày làm việc đã khép lại, những câu hỏi của Mai vẫn còn.");
            foreach (var pair in marks)
            {
                string name = pair.Key;
                if (name.StartsWith("M_PICK_")) AddPickup(name.Substring(7), pair.Value);
                else if (name.StartsWith("M_ORB_")) AddOrb(name.Substring(2).ToLowerInvariant(), pair.Value);
            }
            for (int i = 0; i < 4; i++) AddNote("note_" + i, Mark("M_NOTE_" + i));
            AddChest("chest_0", Mark("M_CHEST_0"), 6);
            AddChest("chest_1", Mark("M_CHEST_1"), OrbTotal);
            AddStation("ca_truc", "M_STATION_stamp", "Mở ca điều phối", "stamp");
            AddStation("dong_code", "M_STATION_terminal", "Mở máy trạm của Linh", "terminal");
            for (int i = 0; i < 3; i++)
            {
                int n = i;
                var mark = Mark("M_VALVE_" + i);
                spots.Add(new Spot { labelFn = () => "Vặn van " + (n + 1), at = () => mark.position, range = 1.9f, rank = 2, on = () => alarmUntil > 0 && !Side.Taken("valve_" + n), use = () => TurnValve(n) });
            }
            spots.Add(new Spot
            {
                labelFn = () => Side.active.Contains("be_dien") && !Side.Has("be_dien.done") ? "Gọi Bé Điện" : "Vuốt ve Bé Điện",
                at = () => cat.position, range = 1.9f, rank = 2, on = () => cat.gameObject.activeSelf && Location == Place.Floor, use = PetCat
            });
        }

        void AddPlace(string mark, string title, string text)
        {
            var t = Mark(mark);
            spots.Add(new Spot
            {
                label = title, at = () => t.position, range = 2f, rank = 5, on = () => t.gameObject.activeInHierarchy || t.IsChildOf(worldRoot),
                use = () => { placeTitle = title; placeText = text; OpenOverlay("place"); Side.Flag("seen." + mark); }
            });
        }

        GameObject Spawn(string prop, Vector3 at)
        {
            var go = Instantiate(propsRoot.Find(prop).gameObject, at, Quaternion.identity, floorRoot);
            go.SetActive(true);
            return go;
        }

        void AddPickup(string id, Transform mark)
        {
            string item = id.Substring(0, id.IndexOf('_'));
            var go = Spawn("P_" + item, mark.position + Vector3.up * .1f);
            var p = new Pickup { id = id, item = item, go = go, home = go.transform.position, lensOnly = item == "sensor" };
            pickups.Add(p);
            spots.Add(new Spot
            {
                label = "Nhặt " + sideData.Item(item).name, at = () => p.home, range = 2f, rank = 3,
                on = () => !Side.Taken(p.id) && (!p.lensOnly || lensOn), use = () => Collect(p)
            });
        }

        void AddOrb(string id, Transform mark)
        {
            var go = Spawn("P_orb", mark.position);
            pickups.Add(new Pickup { id = id, go = go, home = mark.position, orb = true });
        }

        void AddNote(string id, Transform mark)
        {
            var note = Array.Find(sideData.notes, n => n.id == id);
            var go = Spawn("P_note", mark.position);
            pickups.Add(new Pickup { id = id, go = go, home = mark.position });
            spots.Add(new Spot
            {
                label = "Đọc: " + note.title, at = () => mark.position, range = 2, rank = 3, on = () => !Side.Taken(id),
                use = () => { Side.Take(id, null); placeTitle = note.title; placeText = note.text; OpenOverlay("place"); SaveCheckpoint(); }
            });
        }

        void AddChest(string id, Transform mark, int need)
        {
            var note = Array.Find(sideData.notes, n => n.id == id);
            chests[id] = Spawn("P_chest", mark.position);
            spots.Add(new Spot
            {
                labelFn = () => Side.orbs >= need ? "Mở hộp" : "Hộp khóa · cần " + need + " hạt sáng", at = () => mark.position, range = 2, rank = 3, on = () => !Side.Taken(id),
                use = () =>
                {
                    if (Side.orbs < need) { hub.Ui("UI_13_ERROR"); AddToast("Hộp vẫn khóa", "Cần " + need + " hạt sáng, bạn đang có " + Side.orbs + "."); return; }
                    Side.Take(id, null); hub.Sfx("C01_10__turn_album_page"); hub.Ui("UI_11_CONFIRM");
                    placeTitle = note.title; placeText = note.text; OpenOverlay("place"); SaveCheckpoint();
                }
            });
        }

        void AddStation(string questId, string mark, string label, string overlayName)
        {
            var q = sideData.Quest(questId);
            var t = Mark(mark);
            spots.Add(new Spot
            {
                label = label, at = () => t.position, range = 2.1f, rank = 2, on = () => { string s = Side.State(q, cp.stage); return s == "offer" || s == "active"; },
                use = () =>
                {
                    if (Side.Accept(q, cp.stage)) QuestStarted(q);
                    termStage = 0; termOutput = 0; termHint = "Dòng nào làm người biến mất khỏi phép đếm? Bấm vào dòng đó.";
                    OpenOverlay(overlayName);
                }
            });
        }

        // Reload or new game: rebuild the world from what the save says.
        void ResetSideWorld()
        {
            foreach (var p in pickups) p.go.SetActive(!Side.Taken(p.id) && !p.lensOnly);
            foreach (var chest in chests) chest.Value.SetActive(true);
            lensOn = false; lens = 0; Shader.SetGlobalFloat("_CiviLens", 0);
            toasts.Clear();
            if (Side.active.Contains("van_b4") && !Side.Has("van_b4.done")) alarmUntil = Time.time + AlarmSeconds;
            MoveCat();
        }

        public void Interact()
        {
            if (!inGame || overlay.Length > 0 || transitioning || cp.phase != "explore" || focus == null) return;
            focus.use();
        }

        void Collect(Pickup p)
        {
            if (!Side.Take(p.id, p.item)) return;
            p.go.SetActive(false);
            hub.Ui("UI_14_NOTIFICATION", .8f);
            var item = sideData.Item(p.item);
            AddToast("+1 " + item.name, "Trong túi: " + Side.Count(p.item));
            playerBody.Gesture("reach", .7f);
            SaveCheckpoint();
        }

        void AddToast(string title, string detail)
        {
            toasts.Add(new Toast { title = title, detail = detail, until = Time.unscaledTime + 4.5f });
            if (toasts.Count > 3) toasts.RemoveAt(0);
        }

        void Bark(string text, float seconds = 5) { bubble = text; bubbleUntil = Time.unscaledTime + seconds; }

        void ToggleLens()
        {
            lensOn = !lensOn;
            hub.Ui(lensOn ? "UI_09_TOGGLE_ON" : "UI_10_TOGGLE_OFF");
            if (lensOn && !Side.Has("lens.used")) { Side.Flag("lens.used"); Bark("Kính Debug: tôi thấy những gì bảng số không ghi. Bạn thấy gì?"); }
        }

        SideQuest QuestFor(string residentId)
        {
            foreach (var q in sideData.quests)
            {
                string s = Side.State(q, cp.stage);
                if (s == "ready" && q.Receiver == residentId) return q;
                if ((s == "offer" || s == "active") && q.giver == residentId) return q;
            }
            return null;
        }

        void TalkTo(Resident r)
        {
            talking = r;
            Side.Flag("met." + r.id);
            talkQuest = QuestFor(r.id);
            talkText = r.text;
            if (talkQuest != null)
            {
                string s = Side.State(talkQuest, cp.stage);
                if (s == "ready") { talkText = talkQuest.done; FinishQuest(talkQuest); talkQuest = null; }
                else if (s == "active") { talkText = talkQuest.progress; talkQuest = null; }
                else talkText = talkQuest.offer;
            }
            OpenOverlay("resident");
            SaveCheckpoint();
        }

        void AcceptTalkQuest()
        {
            var q = talkQuest;
            talkQuest = null;
            if (q == null || !Side.Accept(q, cp.stage)) return;
            QuestStarted(q);
            CloseOverlay();
            SaveCheckpoint();
        }

        void QuestStarted(SideQuest q)
        {
            hub.Ui("UI_14_NOTIFICATION");
            AddToast("Nhiệm vụ mới", q.title);
            if (q.id == "van_b4")
            {
                alarmUntil = Time.time + AlarmSeconds;
                hub.Sfx("C01_04__new_alert");
                Bark("Áp suất đang tụt. Ba van ở hành lang kỹ thuật. Chạy!", 7);
            }
        }

        void FinishQuest(SideQuest q)
        {
            if (!Side.Finish(q, cp.stage)) return;
            hub.Ui("UI_11_CONFIRM");
            AddToast("Hoàn thành: " + q.title, "Danh hiệu: " + q.reward);
        }

        // Event quests with no giver hand themselves in the moment their flag lands.
        void CheckStationQuest(string id)
        {
            var q = sideData.Quest(id);
            if (string.IsNullOrEmpty(q.Receiver)) { Bark(q.done, 6); FinishQuest(q); }
        }

        void OnStageChanged()
        {
            var herb = sideData.Quest("huong_thao");
            if (cp.stage >= herb.unlock && Side.Accept(herb, cp.stage))
            {
                hub.Ui("UI_14_NOTIFICATION");
                AddToast("Tin nhắn của mẹ", herb.offer);
            }
            lastAudioLine = -1;
        }

        void TurnValve(int n)
        {
            Side.Take("valve_" + n, null);
            playerBody.Gesture("turn", 1.1f);
            hub.Sfx("C01_02__power_keying");
            int done = 0;
            for (int i = 0; i < 3; i++) if (Side.Taken("valve_" + i)) done++;
            AddToast("Van " + (n + 1) + " đã đóng", done + " / 3");
            if (done == 1 && !Side.Has("tuong.called")) { Side.Flag("tuong.called"); callRecording = 0; OpenOverlay("call"); hub.Sfx("C01_04__driver_returns_call"); }
            if (done == 3) EndAlarm(false);
            SaveCheckpoint();
        }

        void EndAlarm(bool late)
        {
            alarmUntil = 0;
            Side.Flag("van_b4.done");
            if (late) Side.Flag("van_b4.late");
            hub.Ui(late ? "UI_12_WARNING" : "UI_11_CONFIRM");
            Bark(late ? "Hệ thống đã tự chuyển sang nguồn dự phòng. Báo lại với nhân viên trực đi." : "Áp suất ổn định. Tôi ghi nhận: bạn chạy nhanh hơn dự đoán.", 6);
            SaveCheckpoint();
        }

        void PetCat()
        {
            hub.Ui("UI_09_TOGGLE_ON", .6f);
            var q = sideData.Quest("be_dien");
            if (Side.active.Contains(q.id) && !Side.Has("be_dien.done"))
            {
                int caught = 0;
                while (Side.Has("cat." + caught)) caught++;
                Side.Flag("cat." + caught);
                if (caught >= 2) { Side.Flag("be_dien.done"); Bark("Mục tiêu đã ngừng di chuyển. Nó vừa nhìn tôi như nhìn một lỗi hệ thống.", 6); }
                else AddToast("Bé Điện lại chạy mất!", "Nó nhảy sang chỗ khác trên sân thượng.");
                MoveCat();
                SaveCheckpoint();
            }
            else Bark("Meo.", 2.5f);
        }

        void MoveCat()
        {
            int k = 0;
            while (Side.Has("cat." + k)) k++;
            bool hunting = Side.active.Contains("be_dien") && !Side.Has("be_dien.done");
            cat.position = hunting ? Mark("M_CAT_" + Mathf.Min(k, 2)).position : Mark("M_NPC_cat").position;
        }

        void TickSide()
        {
            float dt = Time.deltaTime, t = Time.time;
            float wanted = lensOn ? 1 : 0;
            if (!Mathf.Approximately(lens, wanted))
            {
                lens = Mathf.MoveTowards(lens, wanted, dt * 3.5f);
                Shader.SetGlobalFloat("_CiviLens", lens);
                foreach (var p in pickups) if (p.lensOnly) p.go.SetActive(lensOn && !Side.Taken(p.id));
            }
            catTail.localRotation = Quaternion.Euler(0, Mathf.Sin(t * 2.4f) * 24, 0);
            if (!inGame) { focus = null; return; }
            Vector3 me = player.position;
            bool floor = Location == Place.Floor;
            foreach (var p in pickups)
            {
                if (!p.go.activeSelf) continue;
                if (!floor) continue;
                float d = (p.home - me).sqrMagnitude;
                if (d > 900) continue; // no reason to spin what nobody can see
                p.go.transform.SetPositionAndRotation(p.home + Vector3.up * (.07f + Mathf.Sin(t * 2 + p.home.x) * .05f), Quaternion.Euler(0, t * 80, 0));
                if (p.orb && (p.home - me - Vector3.up * .9f).sqrMagnitude < 1.2f && cp.phase == "explore" && Side.Take(p.id, null))
                {
                    p.go.SetActive(false);
                    Side.orbs++;
                    hub.Ui("UI_09_TOGGLE_ON");
                    AddToast("Hạt sáng", Side.orbs + " / " + OrbTotal);
                    SaveCheckpoint();
                }
            }
            if (alarmUntil > 0 && t > alarmUntil) EndAlarm(true);
            if (overlay == "call" && callRecording > 0)
            {
                callRecording += dt;
                if (callRecording >= 8) { Side.Flag("tuong.record"); hub.Ui("UI_11_CONFIRM"); CloseOverlay(); AddToast("Đã ghi nhận giờ làm", "Anh Tường, tuyến 7."); SaveCheckpoint(); }
            }
            if (overlay == "terminal" && termStage == 2) TickTerminal(dt);
            focus = null;
            if (cp.phase != "explore" || overlay.Length > 0 || transitioning) return;
            float best = float.MaxValue;
            foreach (var s in spots)
            {
                if (!s.on()) continue;
                float d = (s.at() - me).sqrMagnitude;
                if (d > s.range * s.range) continue;
                float score = s.rank * 100 + d;
                if (score < best) { best = score; focus = s; }
            }
            if (!reducedMotion && Time.unscaledTime > nextBark && Time.unscaledTime > bubbleUntil)
            {
                nextBark = Time.unscaledTime + 70;
                Bark(ai.CannedLine());
            }
        }

        // ---- minigames from the web demo -----------------------------------------------------------------------

        sealed class Card { public string id, tag, title, ask, ai; public bool aiYes; public string noLabel; }
        static readonly Card[] Cards =
        {
            new Card { id = "hosp", tag = "BV", title = "Bệnh viện Số Ba", ask = "6 MW cho máy hỗ trợ hô hấp", ai = "Đề xuất DUYỆT · ưu tiên 98", aiYes = true, noLabel = "Từ chối" },
            new Card { id = "shop", tag = "SHOP", title = "Shop Mẹ Bỉm", ask = "0,2 MW livestream xả kho lúc 0 giờ", ai = "Đề xuất DUYỆT · doanh thu cao", aiYes = true, noLabel = "Từ chối" },
            new Card { id = "cat", tag = "MÈO", title = "Bé Điện, 4 tuổi", ask = "Số cư trú và suất khí 0,3 m³/ngày", ai = "Đề xuất TỪ CHỐI · không phải người", aiYes = false, noLabel = "Từ chối" },
            new Card { id = "h19", tag = "19", title = "Trạm lọc 19 · Phân khu 8-12", ask = "Sửa trạm lọc · đề nghị lần thứ sáu", ai = "TỰ ĐỘNG TRẢ VỀ · dân số 0", aiYes = false, noLabel = "Giữ lại hồ sơ" },
        };

        string Stamped(Card c) { return Side.Has("stamp." + c.id + ".ok") ? "ok" : Side.Has("stamp." + c.id + ".no") ? "no" : ""; }

        void Stamp(Card c, bool yes)
        {
            Side.Flag("stamp." + c.id + (yes ? ".ok" : ".no"));
            hub.Sfx("C01_02__power_submit");
            foreach (var card in Cards) if (Stamped(card) == "") { SaveCheckpoint(); return; }
            int disagree = 0;
            foreach (var card in Cards) if ((Stamped(card) == "ok") != card.aiYes) disagree++;
            Side.Flag("ca_truc.done");
            CheckStationQuest("ca_truc");
            CloseOverlay();
            Bark(disagree > 0 ? "Bạn làm khác tôi " + disagree + " lần. Tôi sẽ ghi chú điều đó... một cách trung lập." : "Bạn đồng ý với tôi cả bốn lần. Chúng ta sẽ là một đội rất nhanh.", 7);
            SaveCheckpoint();
        }

        static readonly string[] Code =
        {
            "# dem_dan_so.py · CIVI v2.190 · “0 lỗi trong 2.190 ngày”",
            "def dem_dan_so(phan_khu):",
            "    ho_so = lay_ho_so(phan_khu)",
            "    da_xac_minh = [h for h in ho_so if h.trang_thai == \"đã xác minh\"]",
            "    return len(da_xac_minh)",
        };
        static readonly string[] Fixes = { "if h.trang_thai in (\"đã xác minh\", \"chờ xác minh\")", "if True  # đếm hết cho nhanh" };
        static readonly string[] TermOut =
        {
            "$ chạy kiểm thử", "✔ Phân khu 8-12: 0 → 3 người (chờ xác minh)", "$ git push origin sua-dem-dan-so", "…",
            "✖ BỊ CHẶN: ĐÓNG BĂNG HỆ THỐNG TRƯỚC MÙA BÃO", "  người duyệt: Vinh · lý do: “Còn 12 ngày nữa là bão.”"
        };
        int termStage, termWrong = -1;
        float termOutput, callRecording;
        string termHint = "";

        void PickCodeLine(int i)
        {
            if (i == 3)
            {
                hub.Ui("UI_11_CONFIRM");
                termStage = 1;
                termHint = "Chỉ hồ sơ “đã xác minh” được đếm. Người đang chờ xác minh bị tính là không tồn tại. Sửa thế nào?";
                return;
            }
            termWrong = i;
            hub.Ui("UI_13_ERROR");
            termHint = i == 0 ? "Đây chỉ là chú thích, khoe thành tích." : "Dòng này chạy đúng. Lỗi nằm ở chỗ chọn ai được đếm.";
        }

        void PickFix(int i)
        {
            if (i == 1)
            {
                hub.Ui("UI_13_ERROR");
                termHint = "✖ Phân khu 8-12: 1.204.880 người (gồm cả chuột). Thử lại với điều kiện có chữ “chờ xác minh”.";
                return;
            }
            termStage = 2;
            termOutput = 0;
        }

        void TickTerminal(float dt)
        {
            int before = (int)termOutput;
            termOutput = Mathf.Min(termOutput + dt * (reducedMotion ? 20 : 1.9f), TermOut.Length);
            if ((int)termOutput != before) hub.Sfx("C01_01__typing_line1", .35f);
        }

        void FinishTerminal()
        {
            Side.Flag("dong_code.done");
            CheckStationQuest("dong_code");
            CloseOverlay();
            SaveCheckpoint();
        }

        void HandleOverlayKeys(UnityEngine.InputSystem.Keyboard kb)
        {
            if (overlay == "stamp")
                for (int i = 0; i < 4; i++)
                    if (NumberKey(kb, i) && Stamped(Cards[i]) == "") Stamp(Cards[i], Cards[i].aiYes);
        }

        // ---- Hỏi CIVI ------------------------------------------------------------------------------------------

        readonly List<KeyValuePair<string, string>> chat = new List<KeyValuePair<string, string>>();
        string chatInput = "";
        bool chatBusy, chatFocus;
        Vector2 chatScroll;

        void OpenChat()
        {
            if (chat.Count == 0) chat.Add(new KeyValuePair<string, string>("assistant", "Xin chào. Tôi là CIVI. Hỏi tôi về Lưu Quang, về cách chơi, hoặc về việc tại sao bạn lại đứng đó."));
            chatFocus = true;
            OpenOverlay("civi");
        }

        string CiviSystem()
        {
            string objective = Step == null ? "đã xong chương" : !string.IsNullOrEmpty(cp.travelFrom) ? "đi tới lối sang khu vực tiếp theo" : Step.objective;
            return "Bạn là CIVI, drone trợ lý AI của thành phố mái vòm Lưu Quang trong game kể chuyện về công bằng xã hội. Người chơi là Mai, làm ở tầng 42 Cơ quan Điều phối. " +
                "Bạn là AI tối ưu hóa thật thà, hơi quá tự tin, không ác, chỉ đếm những gì được cho xem. Chương hiện tại: Thành phố công bằng. Nhiệm vụ " + (cp.stage + 1) + "/9. " +
                "Mục tiêu hiện tại: " + objective + ". Nơi người chơi đang đứng: " + roomLabel + ". " +
                "Cách chơi: WASD đi, Shift chạy, Space nhảy, chuột xoay camera, E tương tác, Q Kính Debug, M bản đồ, B túi đồ, giữ Alt để hiện chuột. " +
                "Trả lời bằng tiếng Việt, tối đa ba câu ngắn, giữ vai. Không tiết lộ diễn biến sau mục tiêu hiện tại. Không tự nhận mình biết điều chưa xảy ra trong game.";
        }

        void SendChat()
        {
            string q = chatInput.Trim();
            if (q.Length == 0 || chatBusy) return;
            chatInput = "";
            chat.Add(new KeyValuePair<string, string>("user", q));
            chatBusy = true;
            chatScroll = new Vector2(0, 99999);
            hub.Ui("UI_03_CLICK_PRIMARY");
            StartCoroutine(ChatTurn());
        }

        IEnumerator ChatTurn()
        {
            // The API needs the first turn to be the user's, so the greeting stays local.
            var turns = chat.GetRange(1, chat.Count - 1);
            if (turns.Count > 12) turns.RemoveRange(0, turns.Count - 12);
            while (turns.Count > 0 && turns[0].Key != "user") turns.RemoveAt(0);
            string answer = null;
            yield return ai.Ask(CiviSystem(), turns, a => answer = a);
            chat.Add(new KeyValuePair<string, string>("assistant", string.IsNullOrEmpty(answer) ? ai.CannedLine() : answer.Trim()));
            chatBusy = false;
            chatScroll = new Vector2(0, 99999);
            hub.Ui("UI_14_NOTIFICATION", .6f);
        }

        // ---- the audit at the end of the chapter -------------------------------------------------------------------

        string reportTitle, reportBody, reportQuestion, reportSource;
        bool reportBusy;

        void BuildLocalReport()
        {
            int met = 0;
            foreach (var r in residents) if (Side.Has("met." + r.id)) met++;
            var lines = new List<string> { "Bạn đã trò chuyện với " + met + " người trong ngày đầu của chương này, hoàn thành " + Side.finished.Count + "/" + sideData.quests.Length + " việc ngoài nhiệm vụ chính và gom được " + Side.orbs + "/" + OrbTotal + " hạt sáng." };
            if (Side.Has("stamp.cat.ok")) lines.Add("Ở bàn ánh sáng, bạn cấp số công dân cho một con mèo.");
            if (Side.Has("stamp.h19.no")) lines.Add("Bạn giữ lại hồ sơ trạm lọc 19 thay vì để nó tự trả về.");
            if (Side.Has("dong_code.done")) lines.Add("Bạn tìm ra dòng code làm người biến mất khỏi phép đếm, rồi chạm vào lệnh đóng băng.");
            if (Side.Has("tuong.record")) lines.Add("Giữa còi báo động, bạn dành 8 giây ghi giờ làm cho anh Tường.");
            else if (Side.Has("tuong.called")) lines.Add("Anh Tường gọi lúc còi đang kêu, và bạn bảo để sau.");
            if (Side.Has("van_b4.late")) lines.Add("Trạm B4 phải tự chuyển sang nguồn dự phòng.");
            reportTitle = Side.Has("tuong.record") ? "Người ghi lại giờ làm" : Side.Has("dong_code.done") ? "Người đọc từng dòng" : Side.orbs >= OrbTotal ? "Người nhặt ánh sáng" : Side.finished.Count >= 4 ? "Người chịu đi đường vòng" : "Kiểm toán viên Tầng 42";
            reportBody = string.Join(" ", lines);
            reportQuestion = "Phân khu 8-12 vẫn ghi 0 trong hồ sơ chính thức. Trước khi có người xác minh, bạn sẽ gọi con số đó là gì?";
            reportSource = "Báo cáo không cần mạng";
        }

        string ReportFacts()
        {
            return "{\"nguoi_da_gap\":" + Side.flags.FindAll(f => f.StartsWith("met.")).Count + ",\"viec_phu_xong\":" + Side.finished.Count + ",\"hat_sang\":" + Side.orbs +
                ",\"cap_so_cho_meo\":" + (Side.Has("stamp.cat.ok") ? "true" : "false") + ",\"giu_ho_so_19\":" + (Side.Has("stamp.h19.no") ? "true" : "false") +
                ",\"sua_dong_code\":" + (Side.Has("dong_code.done") ? "true" : "false") + ",\"ghi_gio_anh_Tuong\":" + (Side.Has("tuong.record") ? "true" : "false") +
                ",\"tram_B4_tre\":" + (Side.Has("van_b4.late") ? "true" : "false") + "}";
        }

        [Serializable] sealed class AiReport { public string danh_hieu, bao_cao, cau_hoi; }

        IEnumerator AiWriteReport()
        {
            reportBusy = true;
            reportSource = "CIVI đang viết…";
            string system = "Bạn là CIVI, trợ lý AI điều phối của thành phố mái vòm Lưu Quang. Viết báo cáo kiểm toán ngắn về CHÍNH người chơi (vai Mai), bằng tiếng Việt, giọng khô, hơi hài, không phán xét. Chỉ dựa trên nhật ký được cho. Chỉ trả lời một đối tượng JSON với ba khóa danh_hieu, bao_cao, cau_hoi, không thêm chữ nào khác.";
            var turns = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("user", "Nhật ký (JSON): " + ReportFacts()) };
            string answer = null;
            yield return ai.Ask(system, turns, a => answer = a);
            AiReport parsed = null;
            if (!string.IsNullOrEmpty(answer))
            {
                int a = answer.IndexOf('{'), b = answer.LastIndexOf('}');
                if (a >= 0 && b > a)
                    try { parsed = JsonUtility.FromJson<AiReport>(answer.Substring(a, b - a + 1)); }
                    catch (ArgumentException) { }
            }
            if (parsed != null && !string.IsNullOrEmpty(parsed.bao_cao))
            {
                reportTitle = string.IsNullOrEmpty(parsed.danh_hieu) ? reportTitle : parsed.danh_hieu;
                reportBody = parsed.bao_cao;
                reportQuestion = string.IsNullOrEmpty(parsed.cau_hoi) ? reportQuestion : parsed.cau_hoi;
                reportSource = "Viết bởi AI · " + ai.Provider;
            }
            else reportSource = "AI không phản hồi · đang dùng báo cáo không cần mạng";
            reportBusy = false;
        }
    }
}
