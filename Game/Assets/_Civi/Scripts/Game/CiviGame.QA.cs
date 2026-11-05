using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Civi
{
    // Runs inside the real Windows player: plays the whole chapter and the side content through the same methods input
    // calls, writes screenshots and frame-time numbers per area. `-civi-qa <dir>` keeps it away from the real save.
    public sealed partial class CiviGame
    {
        bool qaDriving;
        readonly List<string> qaLog = new List<string>();
        readonly StringBuilder qaFps = new StringBuilder();
        string qaDir;
        readonly HashSet<string> qaFaces = new HashSet<string>();

        void Check(bool ok, string label)
        {
            if (!ok) throw new Exception("QA: " + label);
            qaLog.Add("PASS " + label);
        }

        IEnumerator Shot(string name)
        {
            yield return new WaitForSecondsRealtime(.35f);
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(qaDir, name + ".jpg"), tex.EncodeToJPG(88));
            Destroy(tex);
        }

        // Real frame cost in one place. The frame cap is lifted by -civi-uncapped so the number shows the headroom.
        IEnumerator Sample(string label, float seconds = 3)
        {
            perf.Hold(0);
            var times = new List<float>();
            yield return new WaitForSecondsRealtime(.5f);
            float until = Time.unscaledTime + seconds;
            while (Time.unscaledTime < until) { times.Add(Time.unscaledDeltaTime); yield return null; }
            times.Sort();
            float sum = 0;
            foreach (float t in times) sum += t;
            float avg = times.Count / sum, low = 1 / times[(int)(times.Count * .99f)];
            qaFps.AppendLine(label + ": " + avg.ToString("0") + " fps avg, " + low.ToString("0") + " fps 1% low, worst " + (times[times.Count - 1] * 1000).ToString("0") + " ms, level " + perf.Level + ", frames " + times.Count);
        }

        void Goto(Vector3 pos, float yaw) { TeleportPlayer(pos, yaw); }

        IEnumerator WalkFor(Vector2 input, float seconds, bool run = false)
        {
            float until = Time.time + seconds;
            while (Time.time < until) { MovePlayer(input, run, false, Time.deltaTime); yield return null; }
            playerBody.moveSpeed = 0;
        }

        IEnumerator RunQA()
        {
            var args = Environment.GetCommandLineArgs();
            qaDir = SaveFolder;
            Directory.CreateDirectory(qaDir);
            qaDriving = true;
            Application.runInBackground = true;
            if (Array.IndexOf(args, "-civi-uncapped") >= 0) { Application.targetFrameRate = -1; perf.Locked = true; }
            var stack = new Stack<IEnumerator>();
            stack.Push(Scenario());
            while (stack.Count > 0)
            {
                object next = null; bool more;
                try { more = stack.Peek().MoveNext(); if (more) next = stack.Peek().Current; }
                catch (Exception e)
                {
                    File.WriteAllText(Path.Combine(qaDir, "qa.txt"), string.Join("\n", qaLog) + "\nFAIL " + e);
                    File.WriteAllText(Path.Combine(qaDir, "fps.txt"), qaFps.ToString());
                    Debug.LogException(e);
                    Application.Quit(1);
                    yield break;
                }
                if (!more) { stack.Pop(); continue; }
                if (next is IEnumerator nested) stack.Push(nested);
                else yield return next;
            }
            File.WriteAllText(Path.Combine(qaDir, "qa.txt"), string.Join("\n", qaLog));
            File.WriteAllText(Path.Combine(qaDir, "fps.txt"), "device " + SystemInfo.graphicsDeviceName + " " + SystemInfo.graphicsDeviceType + " " + Screen.width + "x" + Screen.height + " software " + perf.Software + "\n" + qaFps);
            Debug.Log("CIVI QA PASS " + qaLog.Count);
            Application.Quit();
        }

        IEnumerator Scenario()
        {
            Check(ready, "boot");
            Check(!hasResume, "fresh folder has no Continue");
            Check(Cursor.visible && Cursor.lockState == CursorLockMode.None, "title shows the cursor");
            titleOpenedAt = -10;
            titleClock = 3;
            yield return Sample("title terrace");
            yield return Shot("title_terrace");
            titleClock = 24;
            yield return Sample("title flyover");
            yield return Shot("title_city");

            StartNewGame();
            Check(Location == Place.Ward && cp.IsReading, "new game opens on the ward flashback");
            yield return Shot("ward");
            while (cp.IsReading && cp.phase == "intro")
            {
                lineShownAt = -100;
                yield return null;
                AdvanceDialogue();
                while (transitioning) yield return null;
                yield return null;
            }
            Check(Location == Place.Floor && cp.phase == "explore", "flashback hands over to the office");
            yield return new WaitForSecondsRealtime(.6f);
            yield return Shot("office_start");
            yield return Sample("office");

            // Movement and collision: walking into a wall must stop at the wall, and the room must hold the player.
            var start = player.position;
            yield return WalkFor(Vector2.up, 1.2f, true);
            Check((player.position - start).magnitude > 2, "WASD moves Mai");
            Check(playerBody.moveSpeed == 0, "speed resets");
            Goto(new Vector3(-20, 0, -6), 180);
            yield return WalkFor(Vector2.up, 3, true);
            Check(player.position.z > -9.4f, "south wall holds");
            Goto(new Vector3(-5, 0, 0), 90);
            yield return WalkFor(Vector2.up, 2.5f, true);
            Check(player.position.x < -.8f, "locked gallery door holds before its stage");
            Goto(Mark("M_SPAWN_start").position, 200);
            yield return null;

            yield return RealInput();

            // Side content that opens at stage 0.
            lensOn = false;
            ToggleLens();
            yield return new WaitForSecondsRealtime(.5f);
            Check(pickups.Find(p => p.lensOnly).go.activeSelf, "lens reveals sensors");
            yield return Shot("lens");
            ToggleLens();
            yield return new WaitForSecondsRealtime(.5f);
            Check(!pickups.Find(p => p.lensOnly).go.activeSelf, "lens off hides sensors");
            foreach (var q in new[] { "cam_bien" })
            {
                var truc = ResidentOf("truc");
                Goto(truc.body.transform.position + new Vector3(1.4f, 0, 0), 270);
                yield return null; yield return null;
                TalkTo(truc);
                Check(talkQuest != null && overlay == "resident", "NPC offers a quest");
                yield return Shot("quest_offer");
                AcceptTalkQuest();
                Check(Side.active.Contains(q), "quest accepted");
            }
            lensOn = true;
            yield return new WaitForSecondsRealtime(.45f);
            foreach (var p in pickups.FindAll(x => x.lensOnly))
            {
                Goto(p.home + new Vector3(0, 0, .9f) - Vector3.up * p.home.y, 180);
                yield return null; yield return null;
                Check(focus != null && focus.Text.Contains("Cảm biến"), "focus on sensor " + p.id);
                Interact();
                yield return null;
                Check(Side.Taken(p.id), "pickup " + p.id);
            }
            yield return Shot("pickup_toast");
            lensOn = false;
            TalkTo(ResidentOf("truc"));
            Check(Side.finished.Contains("cam_bien") && Side.Count("sensor") == 0, "turn-in consumes items");
            CloseOverlay();

            // Bàn ánh sáng.
            Goto(Mark("M_STATION_stamp").position + new Vector3(0, 0, .6f), 180);
            yield return null; yield return null;
            Check(focus != null && focus.Text == "Mở ca điều phối", "stamp desk reachable");
            Interact();
            yield return Shot("stamp");
            foreach (var c in Cards) Stamp(c, c.id == "h19" ? false : c.aiYes);
            Check(Side.finished.Contains("ca_truc"), "stamp shift completes");
            Check(overlay.Length == 0, "stamp closes");

            // Orbs by walking through them.
            var orb = pickups.Find(p => p.orb && Mathf.Abs(p.home.x + 16) < .1f);
            Goto(orb.home - Vector3.up * orb.home.y, 0);
            yield return new WaitForSecondsRealtime(.2f);
            Check(Side.orbs == 1, "orb collects on touch");

            // Main story to the end, side quests on the way.
            int guard = 0;
            while (cp.phase != "complete")
            {
                if (++guard > 3000) throw new Exception("QA: chapter did not finish");
                if (cp.IsReading)
                {
                    string who = content.speakers[LineIndex];
                    if (who != "Lời kể" && !cp.IsReading.Equals(false) && qaFaces.Add(who))
                    {
                        lineShownAt = Time.unscaledTime;
                        yield return new WaitForSecondsRealtime(.9f);
                        yield return Shot("face_" + qaFaces.Count);
                    }
                    lineShownAt = -100;
                    yield return null;
                    AdvanceDialogue();
                    while (transitioning) yield return null;
                    yield return null;
                }
                else if (cp.phase == "action")
                {
                    yield return Shot("action_" + Step.action);
                    cp.inspected = Civi.Story.ChapterCheckpoint.RequiredMask(Step.action);
                    Check(CommitAction(), "action " + cp.stage);
                }
                else if (StoryDoorId() != null)
                {
                    if (cp.stage == 3) { yield return Stage3Side(); }
                    if (cp.stage == 4) { yield return Sample("gallery route"); }
                    string id = StoryDoorId();
                    Goto(id == "L" ? Mark("M_LIFT").position : doors[id].pos + (id == "L" ? Vector3.zero : new Vector3(-1.6f, 0, 0)), 90);
                    yield return null; yield return null;
                    Check(focus != null && focus.rank == 0, "story door has focus at stage " + cp.stage);
                    Interact();
                    while (transitioning) yield return null;
                    yield return new WaitForSecondsRealtime(.4f);
                }
                else
                {
                    yield return StageShots();
                    Vector3 at = ObjectivePos();
                    Goto(at + new Vector3(0, 0, -.2f), 180);
                    yield return null; yield return null;
                    Check(focus != null && focus.rank <= 1, "objective has focus at stage " + cp.stage);
                    Interact();
                    yield return null;
                }
            }
            Check(cp.stage == 9, "chapter complete");
            yield return Shot("complete");
            BuildLocalReport();
            OpenOverlay("report");
            yield return Shot("report");
            CloseOverlay();

            // Save, reload, corruption.
            Check(SaveCheckpoint(), "final save");
            var again = ReadCheckpoint();
            Check(again != null && again.stage == 9 && again.side.finished.Count == Side.finished.Count, "save roundtrip keeps quests");
            ReturnToTitle();
            yield return new WaitForSecondsRealtime(.5f);
            Check(!inGame && Location == Place.Title, "back to title");
            Check(hasResume, "Continue is offered");
            Check(ContinueGame() && cp.phase == "complete", "Continue restores");
            ReturnToTitle();
            yield return new WaitForSecondsRealtime(.3f);
            StartNewGame();
            Check(cp.stage == 0 && Side.orbs == 0 && pickups.Find(p => p.orb).go.activeSelf, "new game resets the world");
            yield return Chat();
        }

        // The paths a player's hands take: real Input System events through HandleInput, not direct calls.
        IEnumerator RealInput()
        {
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            var kb = Keyboard.current; var mouse = Mouse.current;
            Check(kb != null && mouse != null, "keyboard and mouse present");
            qaDriving = false;
            yield return null;
            Check(!Cursor.visible && Cursor.lockState == CursorLockMode.Locked, "cursor hidden and locked while exploring");
            float yaw = camYaw;
            for (int i = 0; i < 12; i++) { InputSystem.QueueDeltaStateEvent(mouse.delta, new Vector2(40, 0)); yield return null; }
            Check(Mathf.Abs(Mathf.DeltaAngle(yaw, camYaw)) > 5, "moving the mouse turns the view with no button held");

            InputSystem.QueueStateEvent(kb, new KeyboardState(Key.LeftAlt));
            yield return null; yield return null;
            Check(Cursor.visible && Cursor.lockState == CursorLockMode.None, "Alt shows the cursor");
            yaw = camYaw;
            for (int i = 0; i < 8; i++) { InputSystem.QueueDeltaStateEvent(mouse.delta, new Vector2(40, 0)); yield return null; }
            Check(Mathf.Approximately(yaw, camYaw), "view holds still while Alt is down");
            InputSystem.QueueStateEvent(kb, new KeyboardState());
            yield return null; yield return null;
            Check(!Cursor.visible && Cursor.lockState == CursorLockMode.Locked, "releasing Alt hides the cursor again");

            Goto(new Vector3(-14, 0, 2), 90);
            yield return null;
            var from = player.position;
            InputSystem.QueueStateEvent(kb, new KeyboardState(Key.W));
            yield return new WaitForSecondsRealtime(.6f);
            InputSystem.QueueStateEvent(kb, new KeyboardState());
            yield return null;
            Check((player.position - from).magnitude > .5f, "W walks");
            yield return new WaitForSecondsRealtime(.3f);
            from = player.position;
            InputSystem.QueueStateEvent(kb, new KeyboardState(Key.Space));
            yield return null;
            InputSystem.QueueStateEvent(kb, new KeyboardState());
            float peak = from.y, jumpEnd = Time.unscaledTime + .5f;
            while (Time.unscaledTime < jumpEnd) { peak = Mathf.Max(peak, player.position.y); yield return null; }
            Check(peak > from.y + .4f, "Space jumps");
            yield return new WaitForSecondsRealtime(.9f);

            bool before = lensOn;
            InputSystem.QueueStateEvent(kb, new KeyboardState(Key.Q));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(kb, new KeyboardState());
            yield return null;
            Check(lensOn != before, "Q toggles the Debug lens");
            if (lensOn) ToggleLens();

            Goto(Mark("M_STATION_stamp").position + new Vector3(0, 0, .6f), 180);
            yield return null; yield return null;
            InputSystem.QueueStateEvent(kb, new KeyboardState(Key.E));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(kb, new KeyboardState());
            yield return null;
            Check(overlay == "stamp", "E uses what is in front of Mai");
            yield return null;
            Check(Cursor.visible, "panels bring the cursor back");
            InputSystem.QueueStateEvent(kb, new KeyboardState(Key.Escape));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(kb, new KeyboardState());
            yield return null;
            Check(overlay == "pause", "Esc pauses over a panel");
            ResumeFromPause();
            CloseOverlay();
            InputSystem.QueueStateEvent(kb, new KeyboardState(Key.M));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(kb, new KeyboardState());
            yield return null;
            Check(overlay == "map", "M opens the map");
            yield return Shot("map");
            CloseOverlay();
            InputSystem.QueueStateEvent(kb, new KeyboardState(Key.B));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(kb, new KeyboardState());
            yield return null;
            Check(overlay == "bag", "B opens the bag");
            yield return Shot("bag");
            CloseOverlay();
            qaDriving = true;
            yield return new WaitForSecondsRealtime(.3f);

            // Force long frames and watch the governor give up quality, then hand it back.
            if (perf.Software) yield break; // already at the lowest tier, nothing left to give up
            bool wasLocked = perf.Locked;
            int target = Application.targetFrameRate, level = perf.Level;
            perf.Locked = false;
            perf.Hold(0);
            Application.targetFrameRate = 30;
            yield return new WaitForSecondsRealtime(7);
            Check(perf.Level > level, "governor lowers quality when frames run long (level " + perf.Level + ")");
            qaLog.Add("governor went from level " + level + " to " + perf.Level + " at 30 fps");
            Application.targetFrameRate = target;
            perf.Apply(level);
            perf.Locked = wasLocked;
            perf.Hold(1);
        }

        IEnumerator StageShots()
        {
            if (cp.stage == 1) { Goto(Mark("M_NPC_linh_office").position + new Vector3(1, 0, -2.4f), 340); yield return Shot("linh_office"); yield return Sample("office with cast"); }
            if (cp.stage == 3) { Goto(Mark("M_OBJ_case").position + new Vector3(-1, 0, -3), 20); yield return Shot("gallery"); yield return Sample("gallery"); }
            if (cp.stage == 4)
            {
                Goto(Mark("M_NPC_linh_lunch").position + new Vector3(4, 0, 3), 230);
                yield return Shot("canteen"); yield return Sample("canteen");
                // Terrace: the widest view in the game, city and all.
                Goto(new Vector3(24, 0, 12), 0);
                yield return Shot("terrace"); yield return Sample("terrace");
                foreach (var p in pickups.FindAll(x => x.item == "herb")) { Goto(p.home + new Vector3(0, -p.home.y, -1.6f), 0); yield return null; yield return null; Interact(); yield return null; }
                Check(Side.Count("herb") == 3 && Side.active.Contains("huong_thao"), "rosemary quest auto-started and filled");
                var a = ResidentOf("dong_nghiep_a");
                Goto(a.body.transform.position + new Vector3(0, 0, 1.2f), 180);
                yield return null;
                TalkTo(a); Check(talkQuest != null && talkQuest.id == "be_dien", "cat quest offered"); AcceptTalkQuest();
                for (int i = 0; i < 3; i++) { MoveCat(); Goto(cat.position + new Vector3(.9f, -cat.position.y, 0), 270); yield return null; yield return null; PetCat(); yield return null; }
                Check(Side.Has("be_dien.done"), "cat caught three times");
                TalkTo(a); Check(Side.finished.Contains("be_dien"), "cat quest handed in"); CloseOverlay();
            }
            if (cp.stage == 5) { Goto(Mark("M_NPC_vinh").position + new Vector3(-4, 0, -4), 45); yield return Shot("meeting"); yield return Sample("meeting"); }
            if (cp.stage == 6) { yield return new WaitForSecondsRealtime(.4f); yield return Shot("home"); yield return Sample("home"); }
            if (cp.stage == 7)
            {
                var me = ResidentOf("me_mai");
                Goto(me.body.transform.position + new Vector3(.9f, 0, -1), 320);
                yield return null;
                TalkTo(me); Check(Side.finished.Contains("huong_thao"), "rosemary delivered at home"); CloseOverlay();
            }
        }

        IEnumerator Stage3Side()
        {
            // Stage 3 is the first point the south corridor can open: run the B4 alarm for real.
            var truc = ResidentOf("truc");
            Goto(truc.body.transform.position + new Vector3(1.4f, 0, 0), 270);
            yield return null; yield return null;
            TalkTo(truc);
            Check(talkQuest != null && talkQuest.id == "van_b4", "alarm quest offered");
            AcceptTalkQuest();
            Check(alarmUntil > 0, "alarm running");
            Goto(new Vector3(-14, 0, -8.2f), 180);
            yield return WalkFor(Vector2.up, 1.1f, true);
            Check(player.position.z < -9.5f, "corridor door opens during the alarm");
            yield return Shot("corridor");
            for (int i = 0; i < 3; i++)
            {
                Goto(Mark("M_VALVE_" + i).position, 180);
                yield return null; yield return null;
                Check(focus != null && focus.Text.StartsWith("Vặn van"), "valve " + i);
                Interact();
                if (overlay == "call") { yield return Shot("call"); callRecording = 7.9f; yield return new WaitForSecondsRealtime(.3f); }
            }
            Check(Side.Has("tuong.record") && Side.Has("van_b4.done") && alarmUntil == 0, "valves stop the alarm");
            TalkTo(truc); Check(Side.finished.Contains("van_b4"), "alarm quest handed in"); CloseOverlay();

            termStage = 0; OpenOverlay("terminal");
            Side.Accept(sideData.Quest("dong_code"), cp.stage);
            PickCodeLine(0); Check(termStage == 0, "wrong line rejected");
            PickCodeLine(3); Check(termStage == 1, "bug line accepted");
            PickFix(1); Check(termStage == 1, "bad fix rejected");
            PickFix(0);
            termOutput = 99;
            yield return Shot("terminal");
            FinishTerminal();
            Check(Side.finished.Contains("dong_code"), "terminal quest done");
        }

        IEnumerator Chat()
        {
            // No network assumed: a dead provider must still produce a line.
            OpenChat();
            chatInput = "Mục tiêu hiện tại là gì?";
            SendChat();
            float until = Time.unscaledTime + 180;
            while (chatBusy && Time.unscaledTime < until) yield return null;
            Check(!chatBusy && chat[chat.Count - 1].Key == "assistant" && chat[chat.Count - 1].Value.Length > 0, "chat answers (" + ai.Provider + ")");
            qaLog.Add("chat said: " + chat[chat.Count - 1].Value.Replace('\n', ' '));
            yield return Shot("chat");
            CloseOverlay();
        }
    }
}
