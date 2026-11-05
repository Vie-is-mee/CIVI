using System;
using System.IO;

namespace Civi.Story
{
    public static class ChapterPlaces
    {
        public static string Room(int stage)
        {
            if (stage >= 6) return "home";
            switch (stage)
            {
                case 3: return "gallery-v2";
                case 4: return "cafeteria-v2";
                case 5: return "meeting-v2";
                default: return "office";
            }
        }
        public static int MainResident(int stage)
        {
            switch (stage)
            {
                case 1: case 4: return 0; // Linh
                case 3: return 3; // guide
                case 5: return 1; // Vinh
                case 6: return 2; // mother
                default: return -1;
            }
        }
        public static string ActiveRoom(ChapterCheckpoint checkpoint)
        { return string.IsNullOrEmpty(checkpoint.travelFrom) ? Room(checkpoint.stage) : checkpoint.travelFrom; }
        public static bool ValidTravel(ChapterCheckpoint checkpoint)
        {
            if (string.IsNullOrEmpty(checkpoint.travelFrom)) return true;
            return checkpoint.phase == "explore" && checkpoint.stage > 0 && checkpoint.stage < 9 &&
                Room(checkpoint.stage - 1) == checkpoint.travelFrom && Room(checkpoint.stage) != checkpoint.travelFrom;
        }
    }

    [Serializable] public sealed class ChapterStep
    {
        public string title, objective, location, speaker, action;
        public int portrait;
        public float x, y;
        public int[] before, after;
    }

    [Serializable] public sealed class ChapterContent
    {
        public string title, chapter, sourceHash, canonHash, dialogueSourceHash, dialogueSource;
        public int sourceCueCount, sourceSceneCount, legacyIntroCount;
        public string[] paragraphs;
        public string[] speakers;
        public int[] portraits;
        public string[] kinds, cueIds, sceneTitles, canonParagraphs;
        public int[] sceneIds, sourceLines, legacyBeforeCounts, legacyAfterCounts;
        public int[] intro;
        public ChapterStep[] steps;
    }

    [Serializable] public sealed class ChapterCheckpoint
    {
        public string format, sourceHash, phase, travelFrom;
        public int version, stage, line, inspected;
        public float x, y, z, yaw;
        public SideProgress side = new SideProgress();

        public static ChapterCheckpoint New(string hash)
        {
            return new ChapterCheckpoint { format = "CIVI-3D-C1", version = 2, sourceHash = hash, phase = "intro" };
        }

        public static int RequiredMask(string action)
        {
            switch (action)
            {
                case "hospital": return 3;
                case "dossier": return 7;
                case "logistics": return 7;
                case "report": return 3;
                case "followup": return 3;
                default: return 0;
            }
        }

        public bool IsValid(ChapterContent content)
        {
            if (content == null || content.steps == null || format != "CIVI-3D-C1" ||
                version != 2 || sourceHash != content.sourceHash ||
                stage < 0 || stage > content.steps.Length || line < 0 || inspected < 0 ||
                !Finite(x) || !Finite(y) || !Finite(z) || !Finite(yaw) || side == null ||
                !ChapterPlaces.ValidTravel(this)) return false;
            if (stage == content.steps.Length)
                return phase == "complete" && line == 0 && inspected == 0;
            var step = content.steps[stage];
            int mask = RequiredMask(step.action);
            if ((inspected & ~mask) != 0) return false;
            switch (phase)
            {
                case "intro": return stage == 0 && line < content.intro.Length && inspected == 0;
                case "explore": return line == 0 && inspected == 0;
                case "before": return line < step.before.Length && inspected == 0;
                case "action": return mask != 0 && line == 0;
                case "after": return line < step.after.Length && inspected == mask;
                default: return false;
            }
        }

        static bool Finite(float v) { return !float.IsNaN(v) && !float.IsInfinity(v); }

        public int[] Lines(ChapterContent content)
        {
            if (phase == "intro") return content.intro;
            if (stage >= content.steps.Length) return Array.Empty<int>();
            return phase == "before" ? content.steps[stage].before :
                phase == "after" ? content.steps[stage].after : Array.Empty<int>();
        }

        public bool IsReading { get { return phase == "intro" || phase == "before" || phase == "after"; } }

        public void AdvanceLine(ChapterContent content)
        {
            if (!IsReading) return;
            if (++line < Lines(content).Length) return;
            line = 0;
            if (phase == "intro") { phase = "explore"; return; }
            if (phase == "before" && RequiredMask(content.steps[stage].action) != 0)
            { phase = "action"; return; }
            NextStep(content);
        }

        public bool Commit(ChapterContent content)
        {
            if (phase != "action" || stage >= content.steps.Length) return false;
            var step = content.steps[stage];
            if (inspected != RequiredMask(step.action)) return false;
            line = 0;
            if (step.after.Length > 0) phase = "after";
            else NextStep(content);
            return true;
        }

        void NextStep(ChapterContent content)
        {
            string previousRoom = ChapterPlaces.Room(stage);
            stage++; line = 0; inspected = 0;
            phase = stage == content.steps.Length ? "complete" : "explore";
            travelFrom = stage < content.steps.Length && previousRoom != ChapterPlaces.Room(stage) ? previousRoom : null;
        }
    }

    // A failed write leaves the previous checkpoint intact. Backup is validated by the caller.
    public sealed class ChapterSaveStore
    {
        readonly string path;
        public ChapterSaveStore(string target) { path = target; }
        public void Write(string json)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temp = path + ".tmp";
            File.WriteAllText(temp, json, new System.Text.UTF8Encoding(false));
            if (File.Exists(path)) File.Replace(temp, path, path + ".bak");
            else File.Move(temp, path);
        }
        public string Read(Func<string, bool> valid)
        {
            foreach (string candidate in new[] { path, path + ".bak" })
            {
                if (!File.Exists(candidate)) continue;
                try { var json = File.ReadAllText(candidate); if (valid(json)) return json; }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            return null;
        }
    }
}
