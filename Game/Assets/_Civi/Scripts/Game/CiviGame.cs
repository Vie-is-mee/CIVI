using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using Civi.Story;

namespace Civi
{
    // Chapter 1 of STORY.md in 3D. The story flow, save rules and dialogue are CIVI_5's, unchanged in behaviour;
    // the world, camera and interaction layers are new.
    public sealed partial class CiviGame : MonoBehaviour
    {
        const string Root = "Chapter1/";
        [Serializable] sealed class Credits { public string teamName, note; public string[] members; }
        [Serializable] sealed class Journey { public bool started, completed; public int highestStage; }

        ChapterContent content;
        SideData sideData;
        ChapterCheckpoint cp;
        ChapterSaveStore saves;
        Journey journey = new Journey();
        Credits credits;
        CiviAudio hub;
        CiviPerformance perf;
        CiviAi ai;
        Font font;
        Texture2D menuArt, chapterPreview;
        bool ready, inGame, hasResume, reducedMotion, muted, showFps, transitioning, takingPreview;
        string overlay = "", notice = "";
        float noticeUntil, volume = .8f, textSize = 25, mouseSpeed = 1, fade;
        int previewStage = -1;

        public bool IsReady { get { return ready; } }
        string SaveFolder
        {
            get
            {
                var args = Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length - 1; i++)
                    if (args[i] == "-civi-qa") return Path.GetFullPath(args[i + 1]);
                return Application.persistentDataPath;
            }
        }
        string SavePath { get { return Path.Combine(SaveFolder, "civi-chapter1-v2.json"); } }
        ChapterStep Step { get { return cp.stage < content.steps.Length ? content.steps[cp.stage] : null; } }
        SideProgress Side { get { return cp.side; } }
        int LineIndex { get { return cp.Lines(content)[cp.line]; } }

        void Start()
        {
            try
            {
                Boot();
                if (Array.IndexOf(Environment.GetCommandLineArgs(), "-civi-qa") >= 0) StartCoroutine(RunQA());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                notice = "Không thể tải chương 1: " + e.Message;
                if (Array.IndexOf(Environment.GetCommandLineArgs(), "-civi-qa") >= 0) Application.Quit(1);
            }
        }

        void Boot()
        {
            content = Load<ChapterContent>("chapter");
            ValidateContent(content);
            sideData = Load<SideData>("quests");
            credits = Load<Credits>("credits");
            font = Resources.Load<Font>(Root + "CiviFont");
            menuArt = Resources.Load<Texture2D>(Root + "Art/menu");
            if (font == null || menuArt == null) throw new InvalidOperationException("Missing font or menu art");
            volume = Mathf.Clamp01(PlayerPrefs.GetFloat("CIVI.Volume", .8f));
            textSize = Mathf.Clamp(PlayerPrefs.GetFloat("CIVI.TextSize", 25), 22, 30);
            mouseSpeed = Mathf.Clamp(PlayerPrefs.GetFloat("CIVI.Mouse", 1), .3f, 2.5f);
            reducedMotion = PlayerPrefs.GetInt("CIVI.ReducedMotion", 0) == 1;
            muted = PlayerPrefs.GetInt("CIVI.Muted", 0) == 1;
            AudioListener.volume = muted ? 0 : volume;
            saves = new ChapterSaveStore(SavePath);
            hub = gameObject.AddComponent<CiviAudio>();
            perf = gameObject.AddComponent<CiviPerformance>();
            ai = gameObject.AddComponent<CiviAi>();
            ai.Prepare(SaveFolder);
            BootWorld();
            BootPlayer();
            BootSide();
            hasResume = ReadCheckpoint() != null;
            LoadJourney();
            Application.wantsToQuit += SaveBeforeWindowClose;
            ready = true;
            EnterTitle();
        }

        T Load<T>(string name)
        {
            var json = Resources.Load<TextAsset>(Root + name);
            if (json == null) throw new InvalidOperationException("Missing " + name + ".json");
            return JsonUtility.FromJson<T>(json.text);
        }

        public static void ValidateContent(ChapterContent data)
        {
            if (data == null || data.steps == null || data.steps.Length != 9 ||
                data.paragraphs == null || data.paragraphs.Length == 0 || string.IsNullOrEmpty(data.sourceHash))
                throw new FormatException("Chapter 1 adaptation content is incomplete");
            int count = data.paragraphs.Length;
            if (data.speakers == null || data.speakers.Length != count || data.kinds == null || data.kinds.Length != count ||
                data.cueIds == null || data.cueIds.Length != count || data.sceneTitles == null || data.sceneTitles.Length != count ||
                data.sceneIds == null || data.sceneIds.Length != count || data.sourceSceneCount != 12)
                throw new FormatException("Chapter dialogue attribution is incomplete");
            int cursor = 0;
            foreach (int n in data.intro) if (n != cursor++) throw new FormatException("Invalid intro order");
            foreach (var step in data.steps)
            {
                if (step.before == null || step.after == null) throw new FormatException("Invalid chapter step");
                foreach (int n in step.before) if (n != cursor++) throw new FormatException("Before text order");
                foreach (int n in step.after) if (n != cursor++) throw new FormatException("After text order");
            }
            if (cursor != data.paragraphs.Length) throw new FormatException("Chapter text omitted");
        }

        ChapterCheckpoint ParseCheckpoint(string json)
        {
            try
            {
                var c = JsonUtility.FromJson<ChapterCheckpoint>(json);
                return c != null && c.IsValid(content) && c.side.IsValid(sideData) ? c : null;
            }
            catch (Exception) { return null; }
        }

        ChapterCheckpoint ReadCheckpoint()
        {
            string json = saves.Read(s => ParseCheckpoint(s) != null);
            return json == null ? null : ParseCheckpoint(json);
        }

        public void StartNewGame()
        {
            cp = ChapterCheckpoint.New(content.sourceHash);
            var spawn = Mark("M_SPAWN_start");
            cp.x = spawn.position.x; cp.y = spawn.position.y; cp.z = spawn.position.z; cp.yaw = 318;
            inGame = true; overlay = ""; pauseUnderlay = "";
            alarmUntil = 0; ResetSideWorld();
            RefreshWorld(true);
            SaveCheckpoint();
        }

        public bool ContinueGame()
        {
            var restored = ReadCheckpoint();
            hasResume = restored != null;
            if (restored == null) { Notify("Không tìm thấy bản lưu hợp lệ."); return false; }
            cp = restored; inGame = true; overlay = ""; pauseUnderlay = "";
            alarmUntil = 0; ResetSideWorld();
            RefreshWorld(true);
            SaveCheckpoint();
            return true;
        }

        bool SaveCheckpoint(bool announce = false)
        {
            if (!inGame || cp == null) return false;
            try
            {
                if (Location != Place.Ward)
                {
                    var p = player.position;
                    cp.x = p.x; cp.y = p.y; cp.z = p.z; cp.yaw = camYaw;
                }
                if (!cp.IsValid(content) || !cp.side.IsValid(sideData)) throw new FormatException("Checkpoint failed validation");
                saves.Write(JsonUtility.ToJson(cp, true));
                hasResume = true;
                RecordJourney();
                if (announce) Notify("Đã lưu tiến độ.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("CIVI checkpoint: " + e.Message);
                Notify("Chưa lưu được tiến độ. Kiểm tra quyền ghi hoặc dung lượng ổ đĩa.");
                return false;
            }
        }

        void LoadJourney()
        {
            try
            {
                string data = new ChapterSaveStore(SavePath + ".journey").Read(json =>
                {
                    try { var j = JsonUtility.FromJson<Journey>(json); return j != null && j.highestStage >= 0 && j.highestStage <= 9; }
                    catch { return false; }
                });
                if (data != null) journey = JsonUtility.FromJson<Journey>(data);
                var existing = ReadCheckpoint();
                if (existing != null) MergeJourney(existing);
                if (File.Exists(SavePath + ".chapter1.jpg"))
                {
                    var image = new Texture2D(2, 2);
                    if (ImageConversion.LoadImage(image, File.ReadAllBytes(SavePath + ".chapter1.jpg"))) { chapterPreview = image; uiTextures.Add(image); }
                    else Destroy(image);
                }
            }
            catch (Exception e) { Debug.LogWarning("Journey read: " + e.Message); }
        }

        void MergeJourney(ChapterCheckpoint c)
        {
            journey.started = true;
            journey.completed |= c.phase == "complete";
            journey.highestStage = Math.Max(journey.highestStage, c.stage);
        }

        void RecordJourney()
        {
            MergeJourney(cp);
            // Optional history never invalidates an already committed gameplay checkpoint.
            try { new ChapterSaveStore(SavePath + ".journey").Write(JsonUtility.ToJson(journey)); }
            catch (Exception e) { Debug.LogWarning("Journey write: " + e.Message); }
            if (!takingPreview && cp.stage > previewStage && cp.phase == "explore" && string.IsNullOrEmpty(cp.travelFrom))
                StartCoroutine(CapturePreview());
        }

        IEnumerator CapturePreview()
        {
            takingPreview = true;
            yield return new WaitForEndOfFrame();
            if (inGame && overlay.Length == 0 && !transitioning && cp.phase == "explore")
            {
                try
                {
                    var shot = ScreenCapture.CaptureScreenshotAsTexture();
                    if (chapterPreview != null) { uiTextures.Remove(chapterPreview); Destroy(chapterPreview); }
                    chapterPreview = shot; uiTextures.Add(shot); previewStage = cp.stage;
                    // Encoding on a worker keeps this save from showing up as a frame spike.
                    byte[] raw = shot.GetRawTextureData();
                    var format = shot.graphicsFormat; uint w = (uint)shot.width, h = (uint)shot.height;
                    string path = SavePath + ".chapter1.jpg";
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try { File.WriteAllBytes(path, ImageConversion.EncodeArrayToJPG(raw, format, w, h, 0, 80)); }
                        catch (Exception e) { Debug.LogWarning("Chapter preview: " + e.Message); }
                    });
                }
                catch (Exception e) { Debug.LogWarning("Chapter preview: " + e.Message); }
            }
            takingPreview = false;
        }

        bool SaveBeforeWindowClose()
        {
            SaveSettings();
            if (!inGame) return true;
            bool saved = SaveCheckpoint();
            if (!saved) overlay = "saveFailure";
            return saved;
        }

        void SaveSettings()
        {
            PlayerPrefs.SetFloat("CIVI.Volume", volume);
            PlayerPrefs.SetFloat("CIVI.TextSize", textSize);
            PlayerPrefs.SetFloat("CIVI.Mouse", mouseSpeed);
            PlayerPrefs.SetInt("CIVI.ReducedMotion", reducedMotion ? 1 : 0);
            PlayerPrefs.SetInt("CIVI.Muted", muted ? 1 : 0);
            PlayerPrefs.Save();
            AudioListener.volume = muted ? 0 : volume;
        }

        void QuitGame()
        {
            if (inGame && !SaveCheckpoint()) { overlay = "saveFailure"; return; }
            SaveSettings();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void ReturnToTitle()
        {
            if (!SaveCheckpoint()) { overlay = "saveFailure"; return; }
            inGame = false; overlay = ""; pauseUnderlay = "";
            EnterTitle();
        }

        void BeginTransition(Action action)
        {
            if (!transitioning) StartCoroutine(Transition(action));
        }

        IEnumerator Transition(Action action)
        {
            transitioning = true;
            float duration = reducedMotion ? .1f : .42f;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime) { fade = Mathf.SmoothStep(0, 1, t / duration); yield return null; }
            fade = 1;
            perf.Hold(2.5f);
            try { action(); }
            catch (Exception e) { Debug.LogException(e); Notify("Không thể chuyển cảnh. Tiến độ gần nhất vẫn được giữ."); }
            yield return null;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime) { fade = 1 - Mathf.SmoothStep(0, 1, t / duration); yield return null; }
            fade = 0; transitioning = false;
        }

        void Notify(string message) { notice = message; noticeUntil = Time.unscaledTime + 6; hub.Ui("UI_12_WARNING"); }

        // ---- story flow, as in CIVI_5 -------------------------------------------------------------------------

        Vector3 ObjectivePos()
        {
            switch (cp.stage)
            {
                case 0: case 2: return Mark("M_OBJ_audit").position;
                case 1: return Mark("M_NPC_linh_office").position;
                case 3: return Mark("M_OBJ_case").position;
                case 4: return Mark("M_OBJ_linh_lunch").position;
                case 5: return Mark("M_OBJ_vinh").position;
                case 6: return Mark("M_OBJ_rosemary").position;
                case 7: return Mark("M_OBJ_album").position;
                default: return Mark("M_OBJ_desk").position;
            }
        }

        bool ObjectiveOpen { get { return inGame && cp.phase == "explore" && Step != null && string.IsNullOrEmpty(cp.travelFrom); } }

        bool BeginObjective()
        {
            cp.phase = Step.before.Length == 0 ? "action" : "before";
            cp.line = 0;
            lineShownAt = Time.unscaledTime;
            SaveCheckpoint();
            return true;
        }

        public void AdvanceDialogue()
        {
            if (cp == null || !cp.IsReading || transitioning) return;
            if (RevealLeft()) { lineShownAt = -100; return; }
            var next = JsonUtility.FromJson<ChapterCheckpoint>(JsonUtility.ToJson(cp));
            int oldScene = content.sceneIds[LineIndex];
            next.AdvanceLine(content);
            bool sceneChange = next.IsReading && content.sceneIds[next.Lines(content)[next.line]] != oldScene;
            if (sceneChange) BeginTransition(ApplyAdvance);
            else ApplyAdvance();
        }

        void ApplyAdvance()
        {
            int stageBefore = cp.stage;
            cp.AdvanceLine(content);
            lineShownAt = Time.unscaledTime;
            RefreshWorld(false);
            if (cp.stage != stageBefore) OnStageChanged();
            SaveCheckpoint();
        }

        public bool CommitAction()
        {
            string kind = Step.action;
            int stageBefore = cp.stage;
            if (!cp.Commit(content)) { Notify("Hãy kiểm tra đủ các mục trước khi xác nhận."); return false; }
            hub.Ui("UI_11_CONFIRM");
            PlayActionCue(kind);
            lineShownAt = Time.unscaledTime;
            if (cp.stage != stageBefore) OnStageChanged();
            RefreshWorld(false);
            SaveCheckpoint();
            return true;
        }

        void PlayActionCue(string kind)
        {
            switch (kind)
            {
                case "hospital": hub.Sfx("C01_02__power_approved"); break;
                case "dossier": hub.Sfx("C01_03__write_field_note"); break;
                case "logistics": hub.Sfx("C01_04__send_proposal"); break;
                case "report": hub.Sfx("C01_09__forward_for_verification"); break;
                case "followup": hub.Sfx("C01_11__submit_timesheet_request"); break;
            }
        }

        void ToggleInspection(int bit)
        {
            cp.inspected ^= bit;
            hub.Ui((cp.inspected & bit) != 0 ? "UI_09_TOGGLE_ON" : "UI_10_TOGGLE_OFF");
            SaveCheckpoint();
        }

        void OnApplicationPause(bool paused) { if (paused && inGame) SaveCheckpoint(); }
        void OnApplicationFocus(bool focused) { if (!focused && inGame) SaveCheckpoint(); }
        void OnApplicationQuit() { if (inGame) SaveCheckpoint(); }

        void OnDestroy()
        {
            Application.wantsToQuit -= SaveBeforeWindowClose;
            Shader.SetGlobalFloat("_CiviLens", 0);
            foreach (var texture in uiTextures) if (texture != null) Destroy(texture);
        }

        void Update()
        {
            if (!ready) return;
            var kb = Keyboard.current;
            var mouse = Mouse.current;
            bool alt = kb != null && (kb.leftAltKey.isPressed || kb.rightAltKey.isPressed);
            ApplyCursor(alt);
            TickPresentation();
            if (kb != null && !qaDriving) HandleInput(kb, mouse, alt);
            else if (!qaDriving) playerBody.moveSpeed = 0;
            TickWorld();
            TickSide();
        }

        void LateUpdate()
        {
            if (ready) TickCamera();
        }

        void HandleInput(Keyboard kb, Mouse mouse, bool alt)
        {
            if (transitioning) { playerBody.moveSpeed = 0; return; }
            bool typing = overlay == "civi";
            if (kb.escapeKey.wasPressedThisFrame || (!typing && kb.pKey.wasPressedThisFrame && inGame)) { Escape(); return; }
            if (typing) { Idle(); return; }
            if (kb.f3Key.wasPressedThisFrame) showFps = !showFps;
            if (!inGame) return;
            bool interact = kb.eKey.wasPressedThisFrame || kb.fKey.wasPressedThisFrame;
            bool advance = kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame;
            bool click = mouse != null && mouse.leftButton.wasPressedThisFrame && !alt;
            if (overlay == "map" && kb.mKey.wasPressedThisFrame) { CloseOverlay(); return; }
            if (overlay == "bag" && kb.bKey.wasPressedThisFrame) { CloseOverlay(); return; }
            if ((overlay == "resident" || overlay == "place") && (interact || advance)) { CloseOverlay(); return; }
            if (overlay.Length > 0) { HandleOverlayKeys(kb); Idle(); return; }
            if (cp.phase == "action")
            {
                Idle();
                int mask = ChapterCheckpoint.RequiredMask(Step.action);
                for (int i = 0; i < 3; i++)
                    if (NumberKey(kb, i) && (mask & (1 << i)) != 0) ToggleInspection(1 << i);
                if (advance && cp.inspected == mask) CommitAction();
                return;
            }
            if (cp.IsReading) { Idle(); if (interact || advance || click) AdvanceDialogue(); return; }
            if (cp.phase != "explore") { Idle(); return; }
            if (kb.mKey.wasPressedThisFrame) { OpenOverlay("map"); return; }
            if (kb.bKey.wasPressedThisFrame || kb.jKey.wasPressedThisFrame) { OpenOverlay("bag"); return; }
            if (kb.cKey.wasPressedThisFrame) { OpenChat(); return; }
            if (kb.qKey.wasPressedThisFrame) ToggleLens();
            if (interact) Interact();
            Vector2 move = Vector2.zero;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move.x--;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move.x++;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) move.y++;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) move.y--;
            MovePlayer(Vector2.ClampMagnitude(move, 1), kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed, kb.spaceKey.wasPressedThisFrame, Time.deltaTime);
        }

        static bool NumberKey(Keyboard kb, int i)
        {
            switch (i)
            {
                case 0: return kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame;
                case 1: return kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame;
                case 2: return kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame;
                default: return kb.digit4Key.wasPressedThisFrame || kb.numpad4Key.wasPressedThisFrame;
            }
        }

        void Escape()
        {
            if (overlay == "settings") { SaveSettings(); overlay = inGame ? "pause" : ""; }
            else if (overlay == "progress" || overlay == "journal") overlay = inGame ? "pause" : "";
            else if (overlay == "pause") ResumeFromPause();
            else if (overlay == "stamp" || overlay == "terminal" || overlay == "call") { pauseUnderlay = overlay; SaveCheckpoint(); overlay = "pause"; }
            else if (overlay.Length > 0) CloseOverlay();
            else if (inGame) { pauseUnderlay = ""; SaveCheckpoint(); OpenOverlay("pause"); }
            hub.Ui("UI_05_BACK");
        }

        void ResumeFromPause() { overlay = pauseUnderlay; pauseUnderlay = ""; hub.Ui("UI_07_MENU_CLOSE"); }

        void OpenOverlay(string name) { overlay = name; hub.Ui("UI_06_MENU_OPEN"); }
        void CloseOverlay() { overlay = ""; talking = null; hub.Ui("UI_07_MENU_CLOSE"); }
    }
}
