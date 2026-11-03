using System;
using System.Collections.Generic;
using System.IO;
using Civi.Runtime;

class Source : IStorySource
{
    readonly string root;
    public Source(string basePath) { root = basePath; }
    public string SceneJson(string scene) => File.ReadAllText(Path.Combine(root, "scenes", scene + ".json"));
    public string DialogueJson(string scene) => File.ReadAllText(Path.Combine(root, "dialogue", scene + ".json"));
}

static class Program
{
    static int checks;
    static void Check(bool pass, string message)
    {
        checks++;
        if (!pass) throw new Exception("FAIL: " + message);
    }
    static void Throws<T>(Action action, string message) where T : Exception
    {
        bool thrown = false;
        try { action(); } catch (T) { thrown = true; }
        Check(thrown, message);
    }
    static void Drain(StorySession s)
    {
        for (int i = 0; i < 200 && !s.CanChoose && !s.IsComplete; i++) s.Advance();
        Check(s.CanChoose || s.IsComplete, "dialogue reaches a choice");
    }
    static bool Has(StorySession s, string id)
    {
        foreach (var choice in s.AvailableChoices) if (choice.Id == id) return true;
        return false;
    }
    static bool Contains(StorySession s, string field, string item)
    {
        foreach (object value in (List<object>)s.ReadState(field)) if ((string)value == item) return true;
        return false;
    }
    static void TestJson()
    {
        var value = JsonTree.Parse("{\"x\":[true,0,4.5,\"Nhi — ✨\",null,{\"a\":\"\\u00E1\"}]}");
        var round = JsonTree.Obj(JsonTree.Parse(JsonTree.Write(value)), "round");
        var items = JsonTree.Arr(round["x"], "array");
        Check((bool)items[0] && (long)items[1] == 0L && (string)items[3] == "Nhi — ✨", "typed JSON roundtrip");
        Throws<FormatException>(() => JsonTree.Parse("{\"a\":1,\"a\":2}"), "duplicate key rejected");
        Throws<FormatException>(() => JsonTree.Parse("{\"a\":}"), "invalid JSON rejected");
        Throws<FormatException>(() => JsonTree.Parse("true garbage"), "trailing JSON rejected");
    }
    static void Main(string[] args)
    {
        string projectRoot = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        string content = Path.Combine(projectRoot, "Game", "Assets", "_Civi", "Content", "Story", "v4_1");
        var src = new Source(content);
        string initial = File.ReadAllText(Path.Combine(content, "initial-state.json"));
        try
        {
            TestJson();
            var s = new StorySession(src, initial, "S01");
            s.ImportS00("Minh đừng nhìn pin", "pose_3", "clamp", true,
                new[] { "PHOTO_S00_GOOD", "PROP_DIAGRAM" }, new[] { "F01", "F02" });
            Check((string)s.ReadState("opening_gesture") == "pose_3", "S00 gesture survives");
            Check((string)s.ReadState("album_name") == "Minh đừng nhìn pin", "S00 album survives");
            Check(Contains(s,"evidence_seen","PROP_DIAGRAM"),"opening evidence survives");
            Check(s.CurrentLine.Id == "S01_L001", "S01 entry line");
            Throws<InvalidOperationException>(() => s.Choose("S01_CHAT"), "cannot skip dialogue");
            Drain(s);
            Check(Has(s, "S01_CHAT"), "S01 initial choice");
            string beforeBad = s.Save();
            Throws<InvalidOperationException>(() => s.Choose("INVALID"), "invalid choice blocked");
            Check(beforeBad == s.Save(), "blocked choice leaves state untouched");

            s.Choose("S01_CHAT");
            Check(Contains(s, "facts_discovered", "F03"), "S01_CHAT adds F03");
            Check(Contains(s, "evidence_seen", "PROP_CHAT_REMOVE"), "S01_CHAT adds chat");
            Check(Contains(s, "evidence_seen", "PROP_PROOF_16_18"), "S01_CHAT adds print proof");
            string mid = s.Save();
            var restored = new StorySession(src, initial, "S01");
            restored.Restore(mid);
            Check(mid == restored.Save(), "save restores mid-choice");
            Drain(restored);
            Check(restored.StepId == "S01_COMPARE", "transition to compare");
            Drain(restored);
            Check(Has(restored, "S01_FIND"), "S01_FIND unlocked");
            restored.Choose("S01_FIND");
            Check(restored.SceneId == "S02", "transition into S02");
            Check(Contains(restored, "evidence_seen", "PROP_THREE_LAYOUT"), "transition carries evidence");
            Check((string)restored.ReadState("opening_gesture") == "pose_3", "S00 state retained across scene");
            Drain(restored);
            Check(!Has(restored, "S02_LEAVE"), "S02_LEAVE gated by evidence");

            restored.Choose("S02_HELP");
            Drain(restored);
            Drain(restored);
            Check(!Has(restored, "S02_HELP"), "S02_HELP disappears after action");
            foreach (var id in new[] { "S02_CLAMP", "S02_CLOCK", "S02_CAPTION" })
            {
                Check(Has(restored, id), id + " initially visible");
                restored.Choose(id);
                Drain(restored); Drain(restored);
            }
            Check(Has(restored, "S02_LEAVE"), "S02_LEAVE unlocked by 3 evidence objects");
            Check(Contains(restored, "evidence_seen", "PROP_CLAMP"), "S02 clamp evidence");
            Check(Contains(restored, "evidence_seen", "PROP_SCHEDULE"), "S02 schedule evidence");
            Check(Contains(restored, "evidence_seen", "PROP_CAPTION_OLD"), "S02 caption evidence");
            string beforeRestore = restored.Save();
            string corrupt = beforeRestore.Replace("\"scene_id\":\"S02\"", "\"scene_id\":\"INVALID\"");
            Throws<Exception>(() => restored.Restore(corrupt), "corrupt bookmark blocked");
            Check(beforeRestore == restored.Save(), "failed restore does not modify active session");

            // P0 exploration slice: test the authored interaction gates before building UI.
            Check(Has(restored, "S02_LEAVE"), "S02 exit available after investigation");
            restored.Choose("S02_LEAVE");
            Drain(restored);
            Check(restored.SceneId == "S03", "S02 exit enters S03");
            Drain(restored);
            Check(!Has(restored, "S03_SOURCE"), "S03 video remains gated before investigation");
            foreach (string choiceId in new[] {"S03_LOOK_0441", "S03_LOOK_0442", "S03_LOOK_0443", "S03_EMPTY"})
            {
                Check(Has(restored, choiceId), choiceId + " available at photo desk");
                restored.Choose(choiceId);
                Drain(restored); Drain(restored);
            }
            Check(Has(restored, "S03_SOURCE"), "four observations unlock the video lead");
            Check(Contains(restored, "evidence_seen", "PHOTO_0443"), "seen photo preserved");
            var sliceBookmark = restored.Save();
            var sliceReload = new StorySession(src, initial, "S02");
            sliceReload.Restore(sliceBookmark);
            Check(sliceReload.Save() == sliceBookmark, "exploration S03 save/load roundtrip");
            sliceReload.Choose("S03_SOURCE");
            Drain(sliceReload);
            Check(sliceReload.SceneId == "S04", "S03 exit reaches S04 boundary");

            // Unauthorized grant must fail closed and not commit earlier effects.
            var grantSource = new FakeSource();
            var rejected = new StorySession(grantSource, initial, "TEST");
            Drain(rejected);
            var beforeGrant = rejected.Save();
            Throws<NotSupportedException>(() => rejected.Choose("TRY_GRANT"), "grant requires domain validator");
            Check(beforeGrant == rejected.Save(), "effect batch atomic on grant error");
            Console.WriteLine("PASS " + checks + " assertions — S01-S04 slice, conditions, evidence, save/load, guardrails, atomic effects");
        }
        catch (Exception error)
        {
            Console.Error.WriteLine("FAILED after " + checks + " assertions: " + error);
            Environment.ExitCode = 1;
        }
    }

    sealed class FakeSource : IStorySource
    {
        public string SceneJson(string sceneId) => "{\"schema_version\":\"tv1-authoring-0.1\",\"scene_id\":\"TEST\",\"entry_step\":\"A\",\"steps\":[{\"step_id\":\"A\",\"stage_direction\":\"\",\"lines\":[],\"choices\":[{\"choice_id\":\"TRY_GRANT\",\"label\":\"x\",\"when\":{\"all\":[]},\"lines\":[],\"effects\":[{\"op\":\"set\",\"field\":\"album_name\",\"value\":\"WRONG\"},{\"op\":\"grant\",\"asset_id\":\"PHOTO_0443\"}],\"next\":\"end\",\"duration_minutes\":0}]}]}";
        public string DialogueJson(string sceneId) => "{\"scene_id\":\"TEST\",\"lines\":[]}";
    }
}
