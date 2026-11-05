using System;
using System.Collections.Generic;
using UnityEngine;
using Civi.Story;

namespace Civi
{
    // IMGUI kept from CIVI_5 (its menu, pause and map flows were already QA'd), repainted in the web demo's night-teal
    // glass. All drawing happens in a 1600x900 virtual canvas.
    public sealed partial class CiviGame
    {
        static readonly Color Night = new Color(.04f, .08f, .095f, .9f);
        static readonly Color Glass = new Color(.05f, .1f, .115f, .72f);
        static readonly Color Mint = new Color(.25f, .94f, .75f);
        static readonly Color MintSoft = new Color(.65f, .97f, .88f);
        static readonly Color Gold = new Color(.94f, .69f, .31f);
        static readonly Color PaperText = new Color(.95f, .93f, .88f);
        static readonly Color Muted = new Color(.56f, .64f, .62f);
        static readonly Color Coral = new Color(1, .42f, .35f);
        readonly List<Texture2D> uiTextures = new List<Texture2D>();
        readonly Dictionary<string, float> warmth = new Dictionary<string, float>();
        GUIStyle titleStyle, labelStyle, bodyStyle, smallStyle, subStyle, buttonStyle, monoStyle, inputStyle;
        Texture2D white, shade, glow;
        string visibleOverlay = "", closingOverlay = "", pauseUnderlay = "";
        float closingAt, overlayOpenedAt, titleOpenedAt, lineShownAt, uiScale = 1, marginX, marginY;
        Vector2 progressScroll, bagScroll;
        bool drawingTitle, lastHoverAny;
        int journalIndex;
        Vector2 dialogueScroll, journalScroll, creditScroll;

        void MakeStyles()
        {
            if (white != null) return;
            white = new Texture2D(1, 1); white.SetPixel(0, 0, Color.white); white.Apply(); uiTextures.Add(white);
            labelStyle = new GUIStyle(GUI.skin.label) { font = font, fontSize = 24, wordWrap = true, richText = false };
            labelStyle.normal.textColor = PaperText;
            titleStyle = new GUIStyle(labelStyle) { fontSize = 45, fontStyle = FontStyle.Bold };
            bodyStyle = new GUIStyle(labelStyle) { fontSize = Mathf.RoundToInt(textSize) };
            smallStyle = new GUIStyle(labelStyle) { fontSize = 18 };
            smallStyle.normal.textColor = Muted;
            subStyle = new GUIStyle(labelStyle) { fontSize = 32, fontStyle = FontStyle.Bold };
            monoStyle = new GUIStyle(labelStyle) { fontSize = 19, wordWrap = false };
            inputStyle = new GUIStyle(GUI.skin.textField) { font = font, fontSize = 22, padding = new RectOffset(14, 14, 10, 10) };
            inputStyle.normal.textColor = inputStyle.focused.textColor = inputStyle.hover.textColor = PaperText;
            inputStyle.normal.background = inputStyle.focused.background = inputStyle.hover.background = null;
            buttonStyle = new GUIStyle(GUI.skin.button) { font = font, fontSize = 23, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(22, 18, 6, 6), wordWrap = true };
            buttonStyle.normal.background = buttonStyle.hover.background = buttonStyle.active.background = buttonStyle.focused.background = null;
            shade = new Texture2D(256, 1, TextureFormat.RGBA32, false);
            for (int i = 0; i < 256; i++) shade.SetPixel(i, 0, new Color(.015f, .05f, .07f, .88f * (1 - Mathf.SmoothStep(0, 1, i / 255f))));
            shade.Apply(); shade.wrapMode = TextureWrapMode.Clamp; uiTextures.Add(shade);
            glow = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 31.5f;
                    glow.SetPixel(x, y, new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - d), 2)));
                }
            glow.Apply(); uiTextures.Add(glow);
        }

        void Box(Rect r, Color color) { Color old = GUI.color; GUI.color = old * color; GUI.DrawTexture(r, white); GUI.color = old; }
        void Glow(Rect r, Color color) { Color old = GUI.color; GUI.color = old * color; GUI.DrawTexture(r, glow); GUI.color = old; }
        void Text(float x, float y, float w, float h, string value, GUIStyle style = null, Color? color = null)
        {
            style = style ?? labelStyle;
            Color old = style.normal.textColor;
            if (color.HasValue) style.normal.textColor = color.Value;
            GUI.Label(new Rect(x, y, w, h), value, style);
            style.normal.textColor = old;
        }

        bool Button(float x, float y, float w, float h, string text, bool primary = false)
        {
            var rect = new Rect(x, y, w, h);
            string key = (drawingTitle ? "title" : overlay) + text;
            warmth.TryGetValue(key, out float heat);
            bool hover = GUI.enabled && rect.Contains(Event.current.mousePosition);
            if (Event.current.type == EventType.Repaint)
            {
                if (hover && heat <= 0) hub.Hover();
                warmth[key] = heat = Mathf.MoveTowards(heat, hover ? 1 : 0, Time.unscaledDeltaTime * 6);
            }
            Color back = primary ? new Color(.1f + .1f * heat, .42f + .2f * heat, .36f + .18f * heat, .92f) : new Color(.07f + .06f * heat, .13f + .1f * heat, .15f + .09f * heat, .8f);
            if (!GUI.enabled) back = new Color(.1f, .12f, .13f, .6f);
            Box(rect, back);
            Box(new Rect(x, y, 2 + heat * 4, h), primary ? Mint : Gold);
            var style = new GUIStyle(buttonStyle);
            style.normal.textColor = style.hover.textColor = style.active.textColor = style.focused.textColor = GUI.enabled ? PaperText : Muted;
            style.padding.left = 22 + Mathf.RoundToInt(heat * 10);
            bool hit = GUI.Button(rect, text, style);
            if (hit) hub.Ui(primary ? "UI_03_CLICK_PRIMARY" : "UI_04_CLICK_SECONDARY");
            return hit;
        }

        void Cap(float x, float y, string key)
        {
            Box(new Rect(x, y, 34, 34), new Color(.95f, .93f, .86f, .95f));
            Text(x + (key.Length > 1 ? 3 : 10), y + 1, 34, 32, key, labelStyle, new Color(.06f, .1f, .11f));
        }

        void OnGUI()
        {
            if (!ready)
            {
                GUI.Label(new Rect(30, 30, Screen.width - 60, 150), notice.Length > 0 ? notice : "Đang tải…");
                return;
            }
            MakeStyles();
            bodyStyle.fontSize = Mathf.RoundToInt(textSize);
            Matrix4x4 previous = GUI.matrix;
            uiScale = Mathf.Min(Screen.width / 1600f, Screen.height / 900f);
            marginX = (Screen.width - 1600 * uiScale) / 2;
            marginY = (Screen.height - 900 * uiScale) / 2;
            GUI.matrix = Matrix4x4.TRS(new Vector3(marginX, marginY, 0), Quaternion.identity, new Vector3(uiScale, uiScale, 1));
            bool oldEnabled = GUI.enabled;
            GUI.enabled = overlay.Length == 0 && !transitioning;
            if (!inGame) DrawTitle();
            else DrawGame();
            GUI.enabled = oldEnabled;
            if (overlay.Length > 0 || closingOverlay.Length > 0) DrawAnimatedOverlay();
            if (notice.Length > 0 && Time.unscaledTime < noticeUntil)
            {
                Box(new Rect(450, 18, 700, 62), new Color(.09f, .16f, .17f, .96f));
                Text(470, 29, 660, 45, notice, smallStyle, PaperText);
            }
            if (fade > 0) Box(new Rect(-marginX / uiScale, -marginY / uiScale, Screen.width / uiScale, Screen.height / uiScale), new Color(.01f, .02f, .03f, fade));
            if (showFps)
            {
                Box(new Rect(1200, 8, 392, 52), new Color(0, 0, 0, .6f));
                Text(1212, 14, 380, 40, Mathf.RoundToInt(perf.Fps) + " FPS · mức " + perf.Level + " · xấu nhất " + Mathf.RoundToInt(perf.WorstMs) + " ms", smallStyle, Mint);
            }
            GUI.matrix = previous;
        }

        void DrawTitle()
        {
            drawingTitle = true;
            if (perf.Software)
            {
                // Without a GPU the live flyover would cost more than the menu is worth: show the painting instead.
                GUI.DrawTexture(new Rect(0, 0, 1600, 900), menuArt, ScaleMode.ScaleAndCrop);
            }
            float s = Time.unscaledTime;
            titleClock += Time.unscaledDeltaTime;
            for (int i = 0; i < 34; i++)
            {
                float x = Mathf.Repeat(i * 193.73f + s * (5 + i % 5), 1700) - 50, y = Mathf.Repeat(i * 83.17f - s * (3 + i % 3), 960) - 30;
                float size = 12 + i % 4 * 7;
                if (!reducedMotion) Glow(new Rect(x, y, size, size), new Color(.63f, .95f, .88f, .1f + .14f * (.5f + .5f * Mathf.Sin(s * .6f + i))));
            }
            GUI.DrawTexture(new Rect(0, 0, 1060, 900), shade, ScaleMode.StretchToFill, true);
            Matrix4x4 matrix = GUI.matrix; Color color = GUI.color;
            float entrance = reducedMotion ? 1 : Mathf.SmoothStep(0, 1, (Time.unscaledTime - titleOpenedAt) / .9f);
            GUI.matrix = GUI.matrix * Matrix4x4.Translate(new Vector3(-32 * (1 - entrance), 0, 0));
            GUI.color = new Color(1, 1, 1, entrance);
            Text(76, 52, 440, 30, "C I V I   /   C H Ư Ơ N G   0 1", smallStyle, MintSoft);
            Box(new Rect(79, 100, 55, 4), Mint);
            Text(75, 122, 520, 200, "NHỮNG NGƯỜI\nKHÔNG CÓ CHỖ\nDƯỚI BẦU TRỜI", titleStyle, new Color(.015f, .04f, .07f, .6f));
            Text(73, 120, 520, 200, "NHỮNG NGƯỜI\nKHÔNG CÓ CHỖ\nDƯỚI BẦU TRỜI", titleStyle, Color.white);
            Text(79, 322, 465, 42, content.chapter.ToUpperInvariant(), smallStyle, MintSoft);
            float y2 = 372;
            if (Button(79, y2, 428, 47, "Bắt đầu", true)) BeginTransition(StartNewGame);
            y2 += 54;
            if (hasResume) { if (Button(79, y2, 428, 47, "Tiếp tục · nhiệm vụ " + Math.Min(journey.highestStage + 1, 9))) BeginTransition(() => ContinueGame()); y2 += 54; }
            if (Button(79, y2, 428, 47, "Hành trình")) { progressScroll = Vector2.zero; OpenOverlay("progress"); }
            y2 += 54;
            if (Button(79, y2, 428, 47, "Hướng dẫn")) OpenOverlay("guide");
            y2 += 54;
            if (Button(79, y2, 428, 47, "Cài đặt")) OpenOverlay("settings");
            y2 += 54;
            if (Button(79, y2, 428, 47, "Âm thanh: " + (muted ? "tắt" : "bật"))) { muted = !muted; SaveSettings(); }
            y2 += 54;
            if (Button(79, y2, 208, 47, "Credits")) { creditScroll = Vector2.zero; OpenOverlay("credits"); }
            if (Button(299, y2, 208, 47, "Thoát")) QuitGame();
            Text(79, 856, 470, 30, "Mỗi con số đều bắt đầu từ một con người.", smallStyle, MintSoft);
            GUI.matrix = matrix; GUI.color = color;
            drawingTitle = false;
        }

        // ---- HUD -----------------------------------------------------------------------------------------------

        Vector2 Screenish(Vector3 world, out bool front)
        {
            Vector3 p = view.WorldToScreenPoint(world);
            front = p.z > 0;
            return new Vector2((p.x - marginX) / uiScale, (Screen.height - p.y - marginY) / uiScale);
        }

        void DrawGame()
        {
            if (cp.IsReading) { DrawDialogue(); return; }
            if (cp.phase == "action") { DrawAction(); return; }
            if (cp.phase == "complete") { DrawComplete(); return; }
            DrawLabels();
            DrawMinimap(new Rect(26, 22, 216, 216));
            float y = 250;
            if (Step != null)
            {
                string goal = string.IsNullOrEmpty(cp.travelFrom) ? Step.objective : "Đi tới " + (StoryDoorId() == "L" ? "thang máy" : "cửa sang " + RoomName(ChapterPlaces.Room(cp.stage))) + ".";
                float h = smallStyle.CalcHeight(new GUIContent(goal), 340) + 42;
                Box(new Rect(26, y, 372, h), Glass);
                Box(new Rect(26, y, 3, h), Mint);
                Text(42, y + 6, 340, 26, "MỤC TIÊU  ·  " + (cp.stage + 1) + "/9", smallStyle, Mint);
                Text(42, y + 32, 340, h, goal, smallStyle, PaperText);
                y += h + 8;
            }
            foreach (string id in Side.active)
            {
                var q = sideData.Quest(id);
                string count = string.IsNullOrEmpty(q.item) ? "" : "  " + Mathf.Min(Side.Count(q.item), q.count) + "/" + q.count;
                Box(new Rect(26, y, 372, 34), new Color(.05f, .1f, .115f, .5f));
                Box(new Rect(26, y, 3, 34), Gold);
                Text(42, y + 4, 346, 28, q.title + count, smallStyle, PaperText);
                y += 38;
            }
            float t = Time.unscaledTime;
            float ty = 620;
            for (int i = toasts.Count - 1; i >= 0; i--)
            {
                var toast = toasts[i];
                if (t > toast.until) { toasts.RemoveAt(i); continue; }
                float alpha = Mathf.Clamp01(toast.until - t);
                Color old = GUI.color; GUI.color = new Color(1, 1, 1, alpha);
                float h = 34 + smallStyle.CalcHeight(new GUIContent(toast.detail), 330);
                Box(new Rect(26, ty - h, 360, h), new Color(.08f, .13f, .13f, .82f));
                Box(new Rect(26, ty - h, 3, h), Gold);
                Text(42, ty - h + 4, 340, 28, toast.title, smallStyle, Gold);
                Text(42, ty - h + 30, 340, h, toast.detail, smallStyle, PaperText);
                GUI.color = old;
                ty -= h + 8;
            }
            if (focus != null)
            {
                string label = focus.Text;
                float w = 90 + labelStyle.CalcSize(new GUIContent(label)).x;
                float x = 800 - w / 2;
                Box(new Rect(x, 744, w, 52), Glass);
                Cap(x + 10, 753, "E");
                Text(x + 58, 753, w - 60, 40, label, labelStyle, PaperText);
            }
            if (t < bubbleUntil)
            {
                float h = smallStyle.CalcHeight(new GUIContent(bubble), 400) + 46;
                Box(new Rect(1170, 690 - h, 410, h), new Color(.05f, .13f, .13f, .86f));
                Box(new Rect(1170, 690 - h, 410, 2), Mint);
                Text(1186, 696 - h, 390, 26, "CIVI", smallStyle, Mint);
                Text(1186, 722 - h, 384, h, bubble, smallStyle, PaperText);
            }
            if (lens > .02f)
            {
                Box(new Rect(690, 20, 220, 38), new Color(.05f, .2f, .2f, .8f * lens));
                Text(735, 25, 170, 30, "KÍNH DEBUG", smallStyle, Mint);
            }
            if (alarmUntil > 0)
            {
                float left = Mathf.Max(0, alarmUntil - Time.time);
                Box(new Rect(560, 66, 480, 66), new Color(.3f, .05f, .05f, .9f));
                Text(580, 72, 440, 30, "CẢNH BÁO ĐỎ · Áp suất trạm khí B4 giảm 12%", smallStyle, PaperText);
                Box(new Rect(580, 106, 440 * left / AlarmSeconds, 12), left < 12 ? Coral : Gold);
                Text(1050, 76, 120, 40, Mathf.CeilToInt(left) + " giây", labelStyle, left < 12 ? Coral : PaperText);
            }
            if (cursorOn && overlay.Length == 0) DrawShortcuts();
        }

        // Alt shows the cursor; these only exist then, so the HUD stays clean while playing.
        void DrawShortcuts()
        {
            float x = 1176;
            if (Button(x, 24, 96, 42, "Hỏi CIVI")) OpenChat();
            if (Button(x + 104, 24, 96, 42, "Túi đồ")) OpenOverlay("bag");
            if (Button(x + 208, 24, 96, 42, "Bản đồ")) OpenOverlay("map");
            if (Button(x + 312, 24, 96, 42, "Nghỉ")) { pauseUnderlay = ""; SaveCheckpoint(); OpenOverlay("pause"); }
        }

        void DrawLabels()
        {
            Vector3 me = player.position;
            foreach (var r in residents)
            {
                if (!r.on) continue;
                float d = (r.body.transform.position - me).magnitude;
                if (d > 11) continue;
                var p = Screenish(r.body.HeadPoint + Vector3.up * .34f, out bool front);
                if (!front) continue;
                bool main = string.IsNullOrEmpty(cp.travelFrom) && ObjectiveResident(r.id);
                var q = QuestFor(r.id);
                string mark = q != null && Side.State(q, cp.stage) != "active" ? "◆ " : "";
                string label = (main ? "◆ " : mark) + r.name;
                float w = smallStyle.CalcSize(new GUIContent(label)).x + 22;
                Box(new Rect(p.x - w / 2, p.y - 15, w, 30), new Color(.03f, .07f, .08f, .66f));
                Text(p.x - w / 2 + 10, p.y - 13, w, 28, label, smallStyle, main || mark.Length > 0 ? Gold : PaperText);
                if (lens > .5f)
                    Text(p.x - 120, p.y + 16, 260, 26, "hồ sơ: đã xác minh · điểm đóng góp đầy đủ", smallStyle, Mint);
            }
            Vector3 goal = Vector3.zero;
            bool hasGoal = true;
            if (StoryDoorId() != null) goal = StoryDoorId() == "L" ? Mark("M_LIFT").position : doors[StoryDoorId()].pos;
            else if (ObjectiveOpen) goal = ObjectivePos();
            else hasGoal = false;
            if (!hasGoal) return;
            float dist = (goal - me).magnitude;
            if (dist < 2.2f) return;
            var g = Screenish(goal + Vector3.up * 1.7f, out bool ahead);
            if (!ahead || g.x < 40 || g.x > 1560 || g.y < 40 || g.y > 860)
            {
                // Off screen: pin the diamond to the edge, pointing the way the player should turn.
                Vector3 local = view.transform.InverseTransformPoint(goal);
                Vector2 dir = new Vector2(local.x, -local.y).normalized;
                g = new Vector2(800, 450) + dir * 400;
                g.x = Mathf.Clamp(800 + dir.x * 740, 40, 1560);
                g.y = Mathf.Clamp(450 + dir.y * 380, 40, 860);
            }
            Text(g.x - 13, g.y - 22, 30, 34, "◆", labelStyle, Gold);
            Text(g.x - 40, g.y + 10, 90, 26, Mathf.RoundToInt(dist) + " m", smallStyle, PaperText);
        }

        bool ObjectiveResident(string id)
        {
            switch (cp.stage)
            {
                case 1: case 4: return id == "linh";
                case 5: return id == "vinh";
                case 6: return id == "me_mai";
                default: return false;
            }
        }

        string RoomName(string id)
        {
            switch (id)
            {
                case "gallery-v2": return "Phòng lịch sử";
                case "cafeteria-v2": return "Căng tin";
                case "meeting-v2": return "Phòng họp";
                case "home": return "Căn hộ của Mai";
                default: return "Cơ quan Điều phối";
            }
        }

        void DrawFloorPlan(Rect area, Vector2 centre, float scale, bool labels)
        {
            Func<Vector2, Vector2> at = p => new Vector2(area.x + area.width / 2 + (p.x - centre.x) * scale, area.y + area.height / 2 - (p.y - centre.y) * scale);
            if (Location == Place.Home)
            {
                Vector2 home = new Vector2(Mark("M_NPC_me_mai").position.x, Mark("M_NPC_me_mai").position.z - 4);
                var a = at(home + new Vector2(-7.5f, 5.5f));
                Box(new Rect(a.x, a.y, 15 * scale, 11 * scale), new Color(.3f, .23f, .19f, .7f));
                return;
            }
            string target = ChapterPlaces.Room(cp.stage);
            foreach (var room in Rooms)
            {
                var a = at(new Vector2(room.r.xMin, room.r.yMax));
                var r = new Rect(a.x, a.y, room.r.width * scale, room.r.height * scale);
                bool here = room.name == roomLabel;
                Box(r, here ? new Color(.16f, .36f, .34f, .78f) : new Color(.1f, .19f, .2f, .66f));
                Box(new Rect(r.x, r.y, r.width, 1), new Color(.5f, .85f, .75f, .4f));
                Box(new Rect(r.x, r.yMax, r.width, 1), new Color(.5f, .85f, .75f, .4f));
                Box(new Rect(r.x, r.y, 1, r.height), new Color(.5f, .85f, .75f, .4f));
                Box(new Rect(r.xMax, r.y, 1, r.height), new Color(.5f, .85f, .75f, .4f));
                if (labels) Text(r.x + 8, r.y + 4, r.width, 28, room.name, smallStyle, PaperText);
            }
        }

        Vector2 Plan(Vector3 p) { return new Vector2(p.x, p.z); }

        void DrawMinimap(Rect r)
        {
            Box(r, Glass);
            GUI.BeginClip(r);
            var local = new Rect(0, 0, r.width, r.height);
            Vector2 me = Plan(player.position);
            float scale = Location == Place.Home ? 11 : 6.5f;
            DrawFloorPlan(local, me, scale, false);
            Func<Vector3, Vector2> at = p => new Vector2(r.width / 2 + (p.x - me.x) * scale, r.height / 2 - (p.z - me.y) * scale);
            foreach (var res in residents)
                if (res.on) { var v = at(res.body.transform.position); Box(new Rect(v.x - 3, v.y - 3, 6, 6), PaperText); }
            foreach (var p in pickups)
                if (p.go.activeSelf && !p.orb && Location == Place.Floor) { var v = at(p.home); Box(new Rect(v.x - 2, v.y - 2, 4, 4), Gold); }
            Vector3 goal;
            if (StoryDoorId() != null || ObjectiveOpen)
            {
                goal = StoryDoorId() != null ? (StoryDoorId() == "L" ? Mark("M_LIFT").position : doors[StoryDoorId()].pos) : ObjectivePos();
                var g = at(goal);
                g.x = Mathf.Clamp(g.x, 8, r.width - 8); g.y = Mathf.Clamp(g.y, 8, r.height - 8);
                Box(new Rect(g.x - 5, g.y - 5, 10, 10), Mint);
            }
            Matrix4x4 m = GUI.matrix;
            GUIUtility.RotateAroundPivot(player.eulerAngles.y, new Vector2(r.width / 2, r.height / 2));
            Box(new Rect(r.width / 2 - 2, r.height / 2 - 12, 4, 14), Mint);
            GUI.matrix = m;
            Box(new Rect(r.width / 2 - 5, r.height / 2 - 5, 10, 10), PaperText);
            GUI.EndClip();
            Text(r.x + 8, r.yMax - 28, r.width, 26, roomLabel, smallStyle, PaperText);
        }

        bool RevealLeft()
        {
            if (cp == null || !cp.IsReading) return false;
            return RevealCount(content.paragraphs[LineIndex].Length) < content.paragraphs[LineIndex].Length;
        }

        int RevealCount(int length)
        {
            if (reducedMotion) return length;
            return Mathf.Min(length, Mathf.FloorToInt((Time.unscaledTime - lineShownAt) * 52));
        }

        void DrawDialogue()
        {
            int index = LineIndex;
            string speaker = content.speakers[index], text = content.paragraphs[index];
            bool narration = content.kinds[index] == "direction" || speaker == "Lời kể";
            GUI.DrawTexture(new Rect(0, 640, 1600, 260), Letter(), ScaleMode.StretchToFill, true);
            if (Location == Place.Ward)
            {
                Text(105, 60, 1250, 42, "HỒI ỨC · MÙA LẠNH THỨ MƯỜI BẢY", smallStyle, MintSoft);
                Text(101, 90, 1300, 80, content.sceneTitles[index], subStyle, Color.white);
            }
            if (!narration)
            {
                Text(200, 672, 1200, 42, speaker, labelStyle, Gold);
                Box(new Rect(200, 710, 120, 2), Gold);
            }
            string shown = text.Substring(0, RevealCount(text.Length));
            var style = new GUIStyle(bodyStyle) { fontStyle = narration ? FontStyle.Italic : FontStyle.Normal };
            float height = Mathf.Max(142, style.CalcHeight(new GUIContent(text), 1200));
            dialogueScroll = GUI.BeginScrollView(new Rect(200, narration ? 690 : 722, 1230, 150), dialogueScroll, new Rect(0, 0, 1200, height));
            GUI.Label(new Rect(0, 0, 1200, height), shown, style);
            GUI.EndScrollView();
            Text(1370, 866, 200, 28, (index + 1) + " / " + content.paragraphs.Length, smallStyle);
        }

        Texture2D letter;
        Texture2D Letter()
        {
            if (letter != null) return letter;
            letter = new Texture2D(1, 64, TextureFormat.RGBA32, false);
            for (int i = 0; i < 64; i++) letter.SetPixel(0, i, new Color(.01f, .03f, .04f, .9f * Mathf.SmoothStep(1, 0, (i - 8) / 56f)));
            letter.Apply(); letter.wrapMode = TextureWrapMode.Clamp; uiTextures.Add(letter);
            return letter;
        }

        void Inspection(int bit, float y, int key, string heading, string detail)
        {
            bool selected = (cp.inspected & bit) != 0;
            if (Button(374, y, 850, 80, (selected ? "✓  " : "○  ") + heading, selected)) ToggleInspection(bit);
            Text(404, y + 47, 790, 29, detail, smallStyle);
            Text(1190, y + 24, 30, 30, key.ToString(), smallStyle, Muted);
        }

        void DrawAction()
        {
            Box(new Rect(325, 150, 950, 610), Night);
            Box(new Rect(325, 150, 950, 3), Mint);
            Text(374, 178, 850, 34, "HỒ SƠ ĐIỀU PHỐI", smallStyle, Mint);
            Text(374, 214, 850, 60, Step.title, subStyle, Color.white);
            string kind = Step.action, summary = "", commit = "Xác nhận";
            if (kind == "hospital")
            {
                summary = "Đối chiếu căn cứ trước khi chuyển điện từ quảng trường thương mại sang bệnh viện.";
                Inspection(1, 373, 1, "Hồ sơ hỗ trợ hô hấp", "Bệnh viện Số Ba cần nguồn điện để duy trì sự sống.");
                Inspection(2, 468, 2, "Nguồn từ quảng trường thương mại", "Chuyển một phần điện; giữ nguyên ưu tiên sinh tồn.");
                commit = "Phê chuẩn lệnh chuyển điện";
            }
            else if (kind == "dossier")
            {
                summary = "Báo cáo phân khu 8-12: SỐ NGƯỜI CẦN BẢO VỆ = 0. Kiểm tra ba chi tiết trong hồ sơ.";
                Inspection(1, 376, 1, "Chậu cây và hai chiếc áo", "Dấu hiệu sinh hoạt trong căn nhà được báo là không có cư dân.");
                Inspection(2, 469, 2, "Đôi giày trẻ con dưới mái hiên", "Không thể kết luận số người cụ thể chỉ từ một bức ảnh.");
                Inspection(4, 562, 3, "Ảnh chụp hôm qua · Linh xác nhận", "Dữ liệu chỉ cộng hồ sơ đã xác minh, bỏ ngoài người đang chờ.");
                commit = "Ghi sai lệch · Đề nghị khảo sát";
            }
            else if (kind == "logistics")
            {
                summary = "Điện dư không đủ cho cả hai yêu cầu. Sắp xếp đủ ba điều kiện để triển khai phương án an toàn.";
                Inspection(1, 376, 1, "Chuyển thuốc sang kho dự phòng gần bệnh viện", "Đối chiếu biên bản nhiệt độ và phương án vận chuyển.");
                Inspection(2, 469, 2, "Ưu tiên chuyến hàng · Báo trước 15 phút", "Thông báo cho người điều khiển khi thay lịch vận tải.");
                Inspection(4, 562, 3, "Kho dùng pin dự trữ trong khi chờ xe", "Giải phóng một phần điện cho hai phòng học có trẻ bệnh hô hấp.");
                commit = "Triển khai phương án đã thống nhất";
            }
            else if (kind == "report")
            {
                summary = "Cơn bão được dự báo tới sau 12 ngày. Đưa hồ sơ có căn cứ vào quy trình xác minh.";
                Inspection(1, 373, 1, "Đính kèm sai lệch dân số phân khu 8-12", "Thiếu người trong bảng tính có thể làm sai nhu cầu khí.");
                Inspection(2, 468, 2, "Gửi tổ xác minh · Yêu cầu báo cáo cuối ngày thứ ba", "Dự trữ giảm và thiết bị cũ cũng là các giới hạn có thật.");
                commit = "Gửi hồ sơ xác minh";
            }
            else if (kind == "followup")
            {
                summary = "Một hồ sơ nhỏ vẫn cần một người theo dõi. Đừng để công sức người tài xế biến thành một dòng tổng.";
                Inspection(1, 373, 1, "Đối chiếu giờ làm và mã chuyến hàng", "Yêu cầu ghi nhận người đã đổi giờ nghỉ để vận chuyển thuốc.");
                Inspection(2, 468, 2, "Giữ việc khảo sát phân khu 8-12 trong sổ", "Số không trên báo cáo chưa phải kết luận về con người.");
                commit = "Gửi yêu cầu · Khép lại ngày đầu";
            }
            Text(374, 286, 850, 73, summary, labelStyle);
            bool old = GUI.enabled;
            GUI.enabled = old && cp.inspected == ChapterCheckpoint.RequiredMask(kind);
            if (Button(374, 665, 850, 56, commit, true)) CommitAction();
            GUI.enabled = old;
            Text(375, 728, 850, 24, "Tiến độ kiểm tra được lưu tự động.  Esc: tạm dừng.", smallStyle);
        }

        void DrawComplete()
        {
            Box(new Rect(0, 0, 1600, 900), new Color(.02f, .05f, .07f, .6f));
            Box(new Rect(360, 200, 880, 500), Night);
            Text(414, 238, 770, 33, "CHƯƠNG 01  /  HOÀN THÀNH", smallStyle, Mint);
            Text(411, 284, 790, 65, "Thành phố công bằng", titleStyle, Color.white);
            Text(414, 371, 765, 112, "Có những người chưa bao giờ được đưa vào phép tính.\n\nMai đã gửi yêu cầu đối chiếu giờ làm và giữ lại hồ sơ phân khu 8-12.", bodyStyle);
            Text(414, 501, 760, 46, "Hành trình tiếp theo: Những chiếc ô hỏng. Chương 2 chưa mở trong bản này.", smallStyle);
            if (Button(414, 560, 770, 52, "Báo cáo của CIVI về bạn", true)) { BuildLocalReport(); OpenOverlay("report"); }
            if (Button(414, 622, 370, 52, "Đọc lại chương 1")) { journalIndex = 0; OpenOverlay("journal"); }
            if (Button(805, 622, 380, 52, "Về màn hình bắt đầu")) BeginTransition(ReturnToTitle);
        }

        // ---- overlays --------------------------------------------------------------------------------------------

        void TickPresentation()
        {
            if (visibleOverlay != overlay)
            {
                if (overlay.Length == 0 && visibleOverlay.Length > 0) { closingOverlay = visibleOverlay; closingAt = Time.unscaledTime; }
                else closingOverlay = "";
                visibleOverlay = overlay;
                overlayOpenedAt = Time.unscaledTime;
            }
            if (Time.unscaledTime - closingAt > .18f) closingOverlay = "";
        }

        void DrawAnimatedOverlay()
        {
            string actual = overlay;
            bool closing = overlay.Length == 0;
            if (closing) overlay = closingOverlay;
            float p = closing ? 1 - Mathf.Clamp01((Time.unscaledTime - closingAt) / .18f) : Mathf.Clamp01((Time.unscaledTime - overlayOpenedAt) / .26f);
            p = reducedMotion ? 1 : Mathf.SmoothStep(0, 1, p);
            Matrix4x4 matrix = GUI.matrix; Color color = GUI.color; bool enabled = GUI.enabled;
            GUI.color = new Color(1, 1, 1, p);
            float size = .975f + .025f * p;
            GUI.matrix = matrix * Matrix4x4.TRS(new Vector3(800 * (1 - size), 450 * (1 - size) + 24 * (1 - p), 0), Quaternion.identity, new Vector3(size, size, 1));
            GUI.enabled = enabled && !closing && !transitioning;
            DrawOverlay();
            GUI.matrix = matrix; GUI.color = color; GUI.enabled = enabled;
            if (closing) overlay = actual;
        }

        void DrawOverlay()
        {
            switch (overlay)
            {
                case "map": Dim(); DrawMap(); return;
                case "bag": Dim(); DrawBag(); return;
                case "progress": Dim(); DrawProgress(); return;
                case "guide": Dim(); DrawGuide(); return;
                case "journal": Dim(); DrawJournal(); return;
                case "place": DrawPlace(); return;
                case "resident": DrawResident(); return;
                case "stamp": Dim(); DrawStamp(); return;
                case "terminal": Dim(); DrawTerminal(); return;
                case "call": DrawCall(); return;
                case "civi": DrawChat(); return;
                case "report": Dim(); DrawReport(); return;
                case "pause": Dim(); DrawPause(); return;
                case "settings": Dim(); DrawSettings(); return;
                case "credits": Dim(); DrawCredits(); return;
                default: Dim(); DrawSaveFailure(); return;
            }
        }

        void Dim() { Box(new Rect(-marginX / uiScale, -marginY / uiScale, Screen.width / uiScale, Screen.height / uiScale), new Color(.01f, .03f, .04f, .62f)); }

        void DrawPause()
        {
            Box(new Rect(180, 125, 1240, 650), Night);
            Text(240, 185, 600, 75, "MỘT KHOẢNG NGHỈ", titleStyle, Color.white);
            Text(240, 281, 470, 90, "Những câu chuyện vẫn ở đây,\nđợi bạn trở lại.", bodyStyle, MintSoft);
            Text(240, 654, 470, 42, "Tiến độ được tự động lưu.", smallStyle, Gold);
            if (Button(820, 196, 520, 62, "Tiếp tục", true)) ResumeFromPause();
            if (Button(820, 270, 520, 62, "Túi đồ và nhiệm vụ")) { pauseUnderlay = ""; overlay = "bag"; }
            if (Button(820, 344, 520, 62, "Tiến trình")) { progressScroll = Vector2.zero; overlay = "progress"; }
            if (Button(820, 418, 520, 62, "Cài đặt")) overlay = "settings";
            if (Button(820, 492, 520, 62, "Trở về màn hình đầu")) BeginTransition(ReturnToTitle);
        }

        void DrawSettings()
        {
            Box(new Rect(380, 90, 840, 720), Night);
            Text(433, 112, 740, 55, "Cài đặt", titleStyle, Color.white);
            Text(434, 188, 720, 40, "Âm lượng chung  ·  " + Mathf.RoundToInt(volume * 100) + "%");
            volume = GUI.HorizontalSlider(new Rect(437, 232, 720, 25), volume, 0, 1);
            AudioListener.volume = muted ? 0 : volume;
            Text(434, 262, 720, 40, "Cỡ chữ hội thoại  ·  " + Mathf.RoundToInt(textSize));
            textSize = GUI.HorizontalSlider(new Rect(437, 306, 720, 25), textSize, 22, 30);
            Text(434, 336, 720, 40, "Độ nhạy chuột  ·  " + mouseSpeed.ToString("0.0"));
            mouseSpeed = GUI.HorizontalSlider(new Rect(437, 380, 720, 25), mouseSpeed, .3f, 2.5f);
            if (Button(434, 416, 720, 52, "Toàn màn hình: " + (Screen.fullScreen ? "Bật" : "Tắt"))) Screen.fullScreen = !Screen.fullScreen;
            if (Button(434, 478, 720, 52, "Giảm chuyển động: " + (reducedMotion ? "Bật" : "Tắt")))
            {
                reducedMotion = !reducedMotion;
                foreach (var body in bodies.Values) body.reducedMotion = reducedMotion;
            }
            if (Button(434, 540, 720, 52, "Hiện FPS (F3): " + (showFps ? "Bật" : "Tắt"))) showFps = !showFps;
            Text(434, 602, 720, 30, "Đồ họa: " + perf.Device + (perf.Software ? " · chế độ CPU" : ""), smallStyle);
            Text(434, 630, 720, 30, "Mức hình ảnh tự động: " + perf.Level + "  ·  Mục tiêu " + (perf.Software ? 30 : CiviPerformance.TargetFps) + " FPS  ·  Hỏi CIVI: " + ai.Provider, smallStyle);
            if (Button(434, 720, 720, 53, "Lưu và quay lại", true)) { SaveSettings(); overlay = inGame ? "pause" : ""; }
        }

        void DrawCredits()
        {
            Box(new Rect(380, 145, 840, 620), Night);
            Text(433, 183, 730, 58, "Credits", titleStyle, Color.white);
            Text(434, 258, 730, 63, string.IsNullOrEmpty(credits.teamName) ? "Tên nhóm của bạn" : credits.teamName, subStyle, Gold);
            string members = credits.members == null ? "" : string.Join("\n", credits.members);
            float height = Mathf.Max(250, bodyStyle.CalcHeight(new GUIContent(members), 680));
            creditScroll = GUI.BeginScrollView(new Rect(434, 332, 730, 250), creditScroll, new Rect(0, 0, 680, height));
            GUI.Label(new Rect(0, 0, 680, height), members, bodyStyle);
            GUI.EndScrollView();
            Text(434, 604, 730, 48, credits.note ?? "", smallStyle);
            if (Button(434, 672, 720, 53, "Quay lại", true)) overlay = inGame ? "pause" : "";
        }

        void DrawSaveFailure()
        {
            Box(new Rect(380, 145, 840, 620), Night);
            Text(433, 204, 730, 95, "Tiến độ chưa được lưu", titleStyle, Color.white);
            Text(434, 333, 720, 170, "Chưa thể ghi tiến độ. Game vẫn đang mở để bạn có thể kiểm tra dung lượng hoặc quyền ghi rồi thử lại.", bodyStyle);
            if (Button(434, 562, 720, 58, "Thử lưu lại", true)) { if (SaveCheckpoint(true)) overlay = "pause"; }
            if (Button(434, 645, 720, 58, "Trở lại game")) overlay = "";
        }

        void DrawJournal()
        {
            Box(new Rect(160, 80, 1280, 740), Night);
            Text(206, 112, 1180, 48, "Sổ chuyện  /  Thành phố công bằng", subStyle, Color.white);
            Text(207, 175, 1180, 32, content.speakers[journalIndex] + "  ·  " + content.sceneTitles[journalIndex] + "  ·  " + (journalIndex + 1) + " / " + content.paragraphs.Length, smallStyle, Mint);
            string paragraph = content.paragraphs[journalIndex];
            float height = Mathf.Max(450, bodyStyle.CalcHeight(new GUIContent(paragraph), 1140));
            journalScroll = GUI.BeginScrollView(new Rect(207, 237, 1180, 438), journalScroll, new Rect(0, 0, 1140, height));
            GUI.Label(new Rect(0, 0, 1140, height), paragraph, bodyStyle);
            GUI.EndScrollView();
            bool old = GUI.enabled;
            GUI.enabled = old && journalIndex > 0;
            if (Button(207, 726, 280, 52, "← Đoạn trước")) { journalIndex--; journalScroll = Vector2.zero; }
            GUI.enabled = old && journalIndex < content.paragraphs.Length - 1;
            if (Button(505, 726, 280, 52, "Đoạn sau →")) { journalIndex++; journalScroll = Vector2.zero; }
            GUI.enabled = old;
            if (Button(1032, 726, 350, 52, "Đóng sổ  ·  Esc", true)) overlay = inGame && cp.phase != "complete" ? "pause" : "";
        }

        void DrawMap()
        {
            Box(new Rect(90, 50, 1420, 800), Night);
            Text(135, 74, 1150, 55, "BẢN ĐỒ KHU VỰC", subStyle, Color.white);
            Text(135, 128, 1150, 38, roomLabel, labelStyle, Gold);
            var area = new Rect(135, 178, 1000, 610);
            Box(area, new Color(.02f, .05f, .06f, .8f));
            GUI.BeginClip(area);
            var local = new Rect(0, 0, area.width, area.height);
            float scale = Location == Place.Home ? 30 : 12.4f;
            Vector2 centre = Location == Place.Home ? new Vector2(Mark("M_NPC_me_mai").position.x, Mark("M_NPC_me_mai").position.z - 4) : new Vector2(10, 5.5f);
            DrawFloorPlan(local, centre, scale, Location != Place.Home);
            Func<Vector3, Vector2> at = p => new Vector2(area.width / 2 + (p.x - centre.x) * scale, area.height / 2 - (p.z - centre.y) * scale);
            foreach (var res in residents)
                if (res.on) { var v = at(res.body.transform.position); Glow(new Rect(v.x - 12, v.y - 12, 24, 24), new Color(.2f, .95f, .9f)); Box(new Rect(v.x - 3, v.y - 3, 6, 6), Color.white); Text(v.x - 50, v.y - 29, 150, 27, res.name, smallStyle, PaperText); }
            if (Location == Place.Floor)
                foreach (var p in pickups)
                {
                    if (!p.go.activeSelf) continue;
                    var v = at(p.home);
                    Box(new Rect(v.x - 3, v.y - 3, 7, 7), p.orb ? MintSoft : Gold);
                }
            if (ObjectiveOpen) { var v = at(ObjectivePos()); Box(new Rect(v.x - 7, v.y - 7, 14, 14), Gold); Text(v.x - 40, v.y + 10, 150, 30, "Mục tiêu", smallStyle, Gold); }
            if (StoryDoorId() != null) { var v = at(StoryDoorId() == "L" ? Mark("M_LIFT").position : doors[StoryDoorId()].pos); Box(new Rect(v.x - 8, v.y - 8, 16, 16), Mint); Text(v.x - 60, v.y + 12, 170, 30, "Lối đi tiếp →", smallStyle, PaperText); }
            var me = at(player.position);
            Glow(new Rect(me.x - 22, me.y - 22, 44, 44), new Color(.4f, .95f, 1, .9f));
            Box(new Rect(me.x - 5, me.y - 5, 10, 10), Color.white);
            Text(me.x - 20, me.y + 10, 80, 28, "Mai", smallStyle, PaperText);
            GUI.EndClip();
            Text(1160, 182, 325, 36, "HẠT SÁNG", labelStyle, Gold);
            Text(1160, 218, 325, 40, Side.orbs + " / " + OrbTotal, subStyle, Color.white);
            Text(1160, 278, 325, 36, "NHIỆM VỤ PHỤ", labelStyle, Gold);
            Text(1160, 314, 325, 40, Side.finished.Count + " / " + sideData.quests.Length, subStyle, Color.white);
            Text(1160, 400, 325, 200, "◆ Mục tiêu\n■ Vật phẩm\n● Người\n▪ Hạt sáng", smallStyle, PaperText);
            if (Button(1160, 738, 310, 50, "Đóng bản đồ", true)) CloseOverlay();
        }

        void DrawBag()
        {
            Box(new Rect(120, 70, 1360, 760), Night);
            Text(165, 96, 700, 55, "NHIỆM VỤ", subStyle, Color.white);
            Text(880, 96, 500, 55, "TÚI ĐỒ", subStyle, Color.white);
            bagScroll = GUI.BeginScrollView(new Rect(165, 156, 680, 590), bagScroll, new Rect(0, 0, 650, 110 * sideData.quests.Length));
            float y = 0;
            foreach (var q in sideData.quests)
            {
                string s = Side.State(q, cp.stage);
                if (s == "locked") continue;
                bool done = s == "done";
                Box(new Rect(0, y, 650, 100), new Color(.08f, .14f, .15f, done ? .4f : .8f));
                Box(new Rect(0, y, 3, 100), done ? Muted : s == "offer" ? Gold : Mint);
                Text(14, y + 6, 620, 34, q.title + (done ? "  ·  xong" : s == "offer" ? "  ·  chưa nhận" : ""), labelStyle, done ? Muted : PaperText);
                Text(14, y + 42, 620, 56, done ? "Danh hiệu: " + q.reward : s == "offer" ? q.offer : q.progress, smallStyle, done ? Gold : PaperText);
                y += 108;
            }
            GUI.EndScrollView();
            y = 160;
            if (Side.bag.Count == 0) Text(880, y, 560, 40, "Chưa nhặt được gì.", smallStyle);
            for (int i = 0; i < Side.bag.Count; i++)
            {
                var item = sideData.Item(Side.bag[i]);
                Box(new Rect(880, y, 560, 90), new Color(.08f, .14f, .15f, .8f));
                Text(894, y + 6, 430, 34, item.name, labelStyle, Gold);
                Text(1380, y + 6, 60, 34, "×" + Side.bagCounts[i], labelStyle, PaperText);
                Text(894, y + 42, 530, 50, item.text, smallStyle, PaperText);
                y += 98;
            }
            Text(880, 700, 560, 30, "Hạt sáng: " + Side.orbs + " / " + OrbTotal, labelStyle, MintSoft);
            if (Button(880, 750, 560, 52, "Đóng  ·  Esc", true)) CloseOverlay();
        }

        void DrawProgress()
        {
            Box(new Rect(100, 55, 1400, 790), Night);
            Text(145, 88, 1250, 60, "HÀNH TRÌNH ĐÃ ĐI QUA", subStyle, Color.white);
            Text(145, 151, 1250, 36, journey.completed ? "Chương 1 đã hoàn thành" : journey.started ? "Đang chơi · " + content.chapter : "Chưa bắt đầu", labelStyle, Gold);
            progressScroll = GUI.BeginScrollView(new Rect(140, 215, 1325, 525), progressScroll, new Rect(0, 0, 1285, 850));
            var cardStyle = new GUIStyle(labelStyle) { fontSize = 22 };
            for (int i = 0; i < 10; i++)
            {
                int col = i % 4, row = i / 4;
                var card = new Rect(col * 322, row * 280, 302, 258);
                bool played = i == 0 && journey.started;
                Box(card, played ? new Color(.12f, .27f, .29f) : new Color(.11f, .13f, .15f));
                if (played)
                {
                    GUI.DrawTexture(new Rect(card.x + 3, card.y + 3, 296, 143), chapterPreview != null ? chapterPreview : menuArt, ScaleMode.ScaleAndCrop);
                    Box(new Rect(card.x, card.y, 302, 4), Gold);
                    Text(card.x + 16, card.y + 110, 270, 30, journey.completed ? "ĐÃ HOÀN THÀNH" : "ĐANG CHƠI", smallStyle, Color.white);
                }
                else Text(card.x + 24, card.y + 49, 255, 55, "CHƯA KHÁM PHÁ", smallStyle, new Color(.42f, .46f, .48f));
                Text(card.x + 16, card.y + 153, 270, 29, "CHƯƠNG " + (i + 1).ToString("00"), smallStyle, played ? Gold : Muted);
                Text(card.x + 16, card.y + 183, 270, 68, ChapterNames[i], cardStyle, played ? Color.white : new Color(.43f, .47f, .49f));
            }
            GUI.EndScrollView();
            if (Button(145, 766, 400, 49, "Quay lại", true)) overlay = inGame ? "pause" : "";
            if (Button(565, 766, 400, 49, "Sổ chuyện")) { journalIndex = 0; overlay = "journal"; }
        }

        void DrawGuide()
        {
            Box(new Rect(200, 60, 1200, 780), Night);
            Text(250, 84, 1000, 65, "HƯỚNG DẪN", titleStyle, Color.white);
            string[] keys = { "W A S D", "CHUỘT", "SHIFT / SPACE", "E", "Q", "M / B", "C", "ALT", "1–4", "ESC / P" };
            string[] actions = { "Di chuyển Mai.", "Xoay camera, không cần giữ nút. Con lăn để thu phóng.", "Chạy / nhảy.", "Trò chuyện, nhặt vật phẩm, xem vật thể, đi qua cửa, đọc tiếp.", "Bật Kính Debug: xem lớp dữ liệu của thành phố.", "Bản đồ / túi đồ và nhiệm vụ.", "Hỏi CIVI.", "Giữ để hiện chuột, thả ra để chơi tiếp.", "Chọn mục trong hồ sơ và bàn ánh sáng.", "Tạm dừng." };
            for (int i = 0; i < keys.Length; i++)
            {
                float y = 164 + i * 56;
                Box(new Rect(250, y, 250, 46), new Color(.1f, .2f, .22f));
                Text(266, y + 6, 230, 38, keys[i], labelStyle, Gold);
                Text(520, y + 6, 840, 44, actions[i], labelStyle, PaperText);
            }
            if (Button(250, 744, 1100, 50, "Quay lại", true)) overlay = "";
        }

        void DrawPlace()
        {
            Box(new Rect(300, 620, 1000, 240), Night);
            Box(new Rect(300, 620, 3, 240), Gold);
            Text(330, 636, 900, 50, placeTitle, subStyle, Gold);
            Text(330, 692, 940, 110, placeText, labelStyle, PaperText);
            if (Button(1090, 808, 190, 42, "Khép lại", true)) CloseOverlay();
        }

        void DrawResident()
        {
            if (talking == null) { overlay = ""; return; }
            GUI.DrawTexture(new Rect(0, 640, 1600, 260), Letter(), ScaleMode.StretchToFill, true);
            Text(200, 660, 1200, 42, talking.name, labelStyle, Gold);
            Box(new Rect(200, 698, 120, 2), Gold);
            Text(200, 708, 1200, 110, talkText, bodyStyle, PaperText);
            if (talkQuest != null)
            {
                if (Button(200, 826, 300, 48, "Nhận việc", true)) AcceptTalkQuest();
                if (Button(512, 826, 300, 48, "Để sau")) CloseOverlay();
            }
            else if (Button(1130, 826, 280, 48, "Kết thúc trò chuyện", true)) CloseOverlay();
        }

        void DrawStamp()
        {
            Box(new Rect(120, 110, 1360, 680), Night);
            float mw = 48;
            foreach (var c in Cards) if (Stamped(c) == "ok") mw -= c.id == "hosp" ? 6 : c.id == "shop" ? .2f : c.id == "h19" ? 1.5f : .1f;
            Text(165, 134, 800, 40, "Ca điều phối · 08:00", smallStyle, Mint);
            Text(165, 164, 800, 55, "Đóng dấu từng yêu cầu", subStyle, Color.white);
            Text(1090, 170, 360, 40, "Điện dư " + mw.ToString("0.0") + " MW", labelStyle, Gold);
            for (int i = 0; i < 4; i++)
            {
                var c = Cards[i];
                float x = 165 + i * 323;
                string state = Stamped(c);
                Box(new Rect(x, 240, 305, 460), new Color(.08f, .14f, .15f, .9f));
                Box(new Rect(x + 16, 256, 64, 32), Mint);
                Text(x + 24, 257, 60, 30, c.tag, smallStyle, new Color(.04f, .1f, .1f));
                Text(x + 16, 298, 275, 74, c.title, labelStyle, PaperText);
                Text(x + 16, 374, 275, 74, c.ask, smallStyle, PaperText);
                Text(x + 16, 452, 275, 74, "CIVI · " + c.ai, smallStyle, MintSoft);
                if (state != "")
                {
                    string word = state == "ok" ? "ĐÃ DUYỆT" : c.id == "h19" ? "GIỮ LẠI" : "TỪ CHỐI";
                    Text(x + 26, 556, 260, 60, word, subStyle, state == "ok" ? Mint : Coral);
                }
                else
                {
                    if (Button(x + 16, 562, 273, 54, "Duyệt", true)) Stamp(c, true);
                    if (Button(x + 16, 626, 273, 54, c.noLabel)) Stamp(c, false);
                }
            }
            Text(165, 716, 1200, 50, "Bấm 1–4 để đóng dấu theo đề xuất của CIVI, hoặc tự chọn bằng chuột.", smallStyle);
        }

        void DrawTerminal()
        {
            Box(new Rect(200, 90, 1200, 720), new Color(.03f, .06f, .07f, .96f));
            Text(240, 112, 800, 30, "terminal · tang42", smallStyle, Mint);
            Text(240, 142, 1100, 44, "dem_dan_so(\"8-12\") → 0", labelStyle, Gold);
            for (int i = 0; i < Code.Length; i++)
            {
                float y = 204 + i * 54;
                bool bug = termStage > 0 && i == 3;
                Box(new Rect(240, y, 1120, 48), bug ? new Color(.35f, .12f, .1f, .9f) : i == termWrong ? new Color(.2f, .12f, .1f, .8f) : new Color(.07f, .12f, .13f, .9f));
                Text(254, y + 10, 40, 30, (i + 1).ToString(), monoStyle, Muted);
                Text(296, y + 10, 1060, 30, Code[i], monoStyle, PaperText);
                if (termStage == 0 && GUI.Button(new Rect(240, y, 1120, 48), GUIContent.none, GUIStyle.none)) PickCodeLine(i);
            }
            Text(240, 484, 1120, 74, termHint, labelStyle, MintSoft);
            if (termStage == 1)
            {
                if (Button(240, 566, 1120, 54, Fixes[0], true)) PickFix(0);
                if (Button(240, 630, 1120, 54, Fixes[1])) PickFix(1);
            }
            if (termStage == 2)
            {
                string shown = string.Join("\n", TermOut, 0, Mathf.Min((int)termOutput, TermOut.Length));
                Text(240, 560, 1120, 170, shown, monoStyle, PaperText);
                if (termOutput >= TermOut.Length && Button(240, 738, 400, 52, "Đóng terminal", true)) FinishTerminal();
            }
        }

        void DrawCall()
        {
            Box(new Rect(420, 240, 760, 380), Night);
            Box(new Rect(420, 240, 760, 3), Gold);
            Text(460, 262, 680, 30, "Cuộc gọi đến", smallStyle, Gold);
            Text(460, 292, 680, 44, "Anh Tường · tài xế xe công cộng tuyến 7", labelStyle, Color.white);
            if (callRecording > 0)
            {
                Text(460, 352, 680, 40, "Đang điền mẫu đối chiếu giờ làm · 8 trường dữ liệu", labelStyle, PaperText);
                Box(new Rect(460, 410, 680, 18), new Color(.1f, .18f, .19f));
                Box(new Rect(460, 410, 680 * Mathf.Clamp01(callRecording / 8), 18), Mint);
                Text(460, 444, 680, 30, "Còi báo động vẫn đang kêu.", smallStyle);
                return;
            }
            Text(460, 346, 680, 130, "“Chị Mai ơi, sáng nay tôi đổi giờ nghỉ để chở hàng khẩn tới Bệnh viện Số Ba. Mà giờ làm thêm này bên thầu bảo không nằm trong chỗ chị xem…”", labelStyle, PaperText);
            if (Button(460, 498, 680, 50, "Ghi nhận ngay · mất 8 giây", true)) callRecording = .01f;
            if (Button(460, 556, 680, 46, "Để sau nhé anh")) { Side.Flag("tuong.later"); CloseOverlay(); SaveCheckpoint(); }
        }

        void DrawChat()
        {
            var panel = new Rect(960, 80, 600, 740);
            Box(panel, Night);
            Box(new Rect(panel.x, panel.y, panel.width, 3), Mint);
            Text(panel.x + 24, panel.y + 14, 300, 40, "Hỏi CIVI", subStyle, Color.white);
            Text(panel.x + 24, panel.y + 60, 540, 28, chatBusy ? "CIVI đang trả lời…" : "drone trợ lý · " + ai.Provider, smallStyle, Mint);
            float width = panel.width - 76, total = 0;
            var heights = new List<float>();
            foreach (var turn in chat)
            {
                float h = smallStyle.CalcHeight(new GUIContent(turn.Value), width - 24) + 22;
                heights.Add(h);
                total += h + 8;
            }
            chatScroll = GUI.BeginScrollView(new Rect(panel.x + 20, panel.y + 96, panel.width - 40, 540), chatScroll, new Rect(0, 0, width, total));
            float y = 0;
            for (int i = 0; i < chat.Count; i++)
            {
                bool me = chat[i].Key == "user";
                Box(new Rect(me ? 40 : 0, y, width - 40, heights[i]), me ? new Color(.12f, .3f, .28f, .8f) : new Color(.08f, .14f, .15f, .9f));
                Text((me ? 40 : 0) + 12, y + 8, width - 64, heights[i], chat[i].Value, smallStyle, PaperText);
                y += heights[i] + 8;
            }
            GUI.EndScrollView();
            var input = new Rect(panel.x + 20, panel.y + 652, panel.width - 150, 56);
            Box(input, new Color(.08f, .14f, .15f, .95f));
            bool enter = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) && GUI.GetNameOfFocusedControl() == "chat";
            GUI.SetNextControlName("chat");
            chatInput = GUI.TextField(input, chatInput, 300, inputStyle);
            if (chatFocus) { GUI.FocusControl("chat"); chatFocus = false; }
            if (enter) { SendChat(); Event.current.Use(); chatFocus = true; }
            if (Button(panel.x + panel.width - 118, panel.y + 652, 98, 56, "Gửi", true)) SendChat();
        }

        void DrawReport()
        {
            Box(new Rect(220, 60, 1160, 780), Night);
            Box(new Rect(220, 60, 1160, 3), Mint);
            Text(270, 84, 1000, 30, "KIỂM TOÁN VIÊN CIVI  ·  BÁO CÁO VỀ CHÍNH BẠN", smallStyle, Mint);
            Text(270, 116, 1060, 66, reportTitle, titleStyle, Color.white);
            float height = Mathf.Max(260, bodyStyle.CalcHeight(new GUIContent(reportBody), 1050));
            GUI.Label(new Rect(270, 200, 1060, 290), reportBody, bodyStyle);
            Text(270, 500, 1060, 30, reportSource, smallStyle, Muted);
            Text(270, 540, 1060, 110, reportQuestion, labelStyle, Gold);
            bool old = GUI.enabled;
            GUI.enabled = old && !reportBusy && ai.Online;
            if (Button(270, 672, 500, 52, ai.Online ? "Để CIVI viết bằng AI" : "AI chưa sẵn sàng · xem hướng dẫn trong README", true)) StartCoroutine(AiWriteReport());
            GUI.enabled = old;
            if (Button(790, 672, 540, 52, "Sao chép thẻ kết quả"))
            {
                GUIUtility.systemCopyBuffer = "Mình vừa chơi CIVI · Chương 1. Danh hiệu: " + reportTitle + ". Mình đã gom được " + Side.orbs + " hạt sáng. Phân khu 8-12 vẫn ghi 0. Bạn sẽ đếm lại thế nào?";
                Notify("Đã sao chép thẻ kết quả.");
            }
            if (Button(270, 740, 1060, 52, "Đóng báo cáo")) CloseOverlay();
        }
    }
}
