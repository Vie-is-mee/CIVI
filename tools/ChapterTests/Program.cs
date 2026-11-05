using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Civi.Story;

class Program
{
    static int checks;
    static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };
    static void Assert(bool value, string label) { if (!value) throw new Exception(label); checks++; }
    static ChapterCheckpoint Clone(ChapterCheckpoint value) { return JsonSerializer.Deserialize<ChapterCheckpoint>(JsonSerializer.Serialize(value, Json), Json); }
    static void SideTests(ChapterContent content)
    {
        var data = JsonSerializer.Deserialize<SideData>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "quests.json")), Json);
        Assert(data.quests.Length == 8 && data.items.Length == 4, "side quest data loads");
        foreach (var q in data.quests)
        {
            Assert(string.IsNullOrEmpty(q.item) == (q.count == 0), q.id + ": item and count agree");
            Assert(string.IsNullOrEmpty(q.item) || data.Item(q.item) != null, q.id + ": item exists");
            Assert(q.offer.Length > 0 && q.progress.Length > 0 && q.done.Length > 0 && q.reward.Length > 0, q.id + ": has text");
            Assert(q.unlock >= 0 && q.unlock < content.steps.Length, q.id + ": unlock inside chapter");
        }
        var s = new SideProgress();
        var quest = data.Quest("the_khi");
        Assert(s.State(quest, 0) == "locked" && !s.Accept(quest, 0), "locked before its stage");
        Assert(s.State(quest, 3) == "offer" && !s.Finish(quest, 3), "cannot finish unaccepted");
        Assert(s.Accept(quest, 3) && !s.Accept(quest, 3), "accept once");
        Assert(s.State(quest, 3) == "active" && !s.Finish(quest, 3), "cannot finish while short");
        Assert(s.Take("card_0", "card") && !s.Take("card_0", "card") && s.Count("card") == 1, "a pickup pays once");
        Assert(s.Take("card_1", "card") && s.Take("card_2", "card") && s.State(quest, 3) == "ready", "ready at the required count");
        Assert(s.IsValid(data), "bag valid");
        Assert(s.Finish(quest, 3) && s.State(quest, 3) == "done" && s.Count("card") == 0 && !s.bag.Contains("card"), "finishing consumes items");
        Assert(!s.Finish(quest, 3), "finish once");
        Assert(s.Taken("card_0") && s.IsValid(data), "taken pickups stay taken after finishing");
        var roundTrip = JsonSerializer.Deserialize<SideProgress>(JsonSerializer.Serialize(s, Json), Json);
        Assert(roundTrip.IsValid(data) && roundTrip.State(quest, 3) == "done" && roundTrip.Taken("card_1"), "progress survives a save");
        var ev = data.Quest("ca_truc");
        Assert(s.Accept(ev, 0) && s.State(ev, 0) == "active", "event quest waits for its flag");
        s.Flag("ca_truc.done"); s.Flag("ca_truc.done");
        Assert(s.State(ev, 0) == "ready" && s.flags.Count == 1 && s.Finish(ev, 0), "event quest finishes on its flag");
        var bad = new SideProgress(); bad.bag.Add("ghost"); bad.bagCounts.Add(1); Assert(!bad.IsValid(data), "reject unknown item");
        bad = new SideProgress(); bad.bag.Add("leaf"); Assert(!bad.IsValid(data), "reject bag length mismatch");
        bad = new SideProgress(); bad.bag.Add("leaf"); bad.bagCounts.Add(0); Assert(!bad.IsValid(data), "reject empty stack");
        bad = new SideProgress(); bad.active.Add("nope"); Assert(!bad.IsValid(data), "reject unknown active quest");
        bad = new SideProgress(); bad.finished.Add("the_khi"); bad.active.Add("the_khi"); Assert(!bad.IsValid(data), "reject quest both active and finished");
        bad = new SideProgress(); bad.orbs = -1; Assert(!bad.IsValid(data), "reject negative orbs");
        var cp = ChapterCheckpoint.New(content.sourceHash); cp.side = null; Assert(!cp.IsValid(content), "reject missing side progress");
        cp = ChapterCheckpoint.New(content.sourceHash); cp.z = float.NaN; Assert(!cp.IsValid(content), "reject NaN z");
        cp = ChapterCheckpoint.New(content.sourceHash); cp.yaw = float.PositiveInfinity; Assert(!cp.IsValid(content), "reject infinite yaw");
    }

    static void Main()
    {
        var content = JsonSerializer.Deserialize<ChapterContent>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "chapter.json")), Json);
        var cp = ChapterCheckpoint.New(content.sourceHash);
        var distinctRooms = new HashSet<string>();
        for (int stage = 0; stage < 9; stage++) distinctRooms.Add(ChapterPlaces.Room(stage));
        Assert(distinctRooms.Count == 5, "chapter traverses five distinct locations");
        Assert(ChapterPlaces.Room(3) == "gallery-v2" && ChapterPlaces.Room(4) == "cafeteria-v2" && ChapterPlaces.Room(5) == "meeting-v2", "history, lunch, meeting follow chapter order");
        Assert(ChapterPlaces.Room(6) == "home" && ChapterPlaces.Room(9) == "home", "family scenes and completion stay at home");
        Assert(ChapterPlaces.MainResident(1) == 0 && ChapterPlaces.MainResident(4) == 0 && ChapterPlaces.MainResident(5) == 1 && ChapterPlaces.MainResident(6) == 2, "canonical NPCs own their conversations");
        Assert(content.speakers.Length == content.paragraphs.Length && content.portraits.Length == content.paragraphs.Length, "all pages have presentation attribution");
        Assert(content.sourceCueCount == 148 && content.sourceSceneCount == 12 && content.canonParagraphs.Length == 73, "reviewed script with retained canon reference");
        int reply = Array.FindIndex(content.paragraphs, p => p == "Loại mới dùng ổn không?");
        Assert(reply >= 0 && content.speakers[reply] == "Mai" && content.portraits[reply] == 0 && content.speakers[reply+1] == "Linh", "alternating Mai/Linh attribution from Markdown");
        int motherReply = Array.FindIndex(content.paragraphs, p => p.StartsWith("Mẹ có nghĩ ngày ấy"));
        Assert(motherReply >= 0 && content.speakers[motherReply] == "Mai" && content.speakers[motherReply+1] == "Mẹ Mai", "family dialogue attribution from Markdown");
        Assert(Array.TrueForAll(content.paragraphs, p => p.Length <= 360), "dialogue pages fit the reading pane");
        Assert(Array.TrueForAll(content.portraits, p => p >= -1 && p <= 3), "unsupported roles do not borrow another character portrait");
        Assert(cp.IsValid(content), "new checkpoint");
        var invalid = Clone(cp); invalid.version = 0; Assert(!invalid.IsValid(content), "reject absent schema");
        invalid = Clone(cp); invalid.sourceHash = "older source"; Assert(!invalid.IsValid(content), "reject incompatible source");
        invalid = Clone(cp); invalid.x = float.NaN; Assert(!invalid.IsValid(content), "reject NaN position");
        invalid = Clone(cp); invalid.y = float.PositiveInfinity; Assert(!invalid.IsValid(content), "reject infinity position");
        invalid = Clone(cp); invalid.stage = 9; Assert(!invalid.IsValid(content), "reject forged completion");
        invalid = Clone(cp); invalid.stage = 3; Assert(!invalid.IsValid(content), "intro only at first step");
        invalid = Clone(cp); invalid.line = 999; Assert(!invalid.IsValid(content), "reject invalid dialogue cursor");
        invalid = Clone(cp); invalid.phase = "injected"; Assert(!invalid.IsValid(content), "reject unknown phase");
        var read = new List<int>(); int actions = 0, transitions = 0;
        while (cp.phase != "complete" && transitions++ < content.paragraphs.Length + 50)
        {
            Assert(cp.IsValid(content), "valid traversal checkpoint");
            var restored = Clone(cp); Assert(restored.IsValid(content) && restored.stage == cp.stage && restored.phase == cp.phase && restored.line == cp.line, "save roundtrip every state");
            if (cp.IsReading) { read.Add(cp.Lines(content)[cp.line]); cp.AdvanceLine(content); }
            else if (cp.phase == "explore")
            {
                if (!string.IsNullOrEmpty(cp.travelFrom)) {
                    Assert(ChapterPlaces.ActiveRoom(cp) == ChapterPlaces.Room(cp.stage-1), "stay in previous room until doorway");
                    Assert(Clone(cp).travelFrom == cp.travelFrom, "pending doorway survives save roundtrip");
                    cp.travelFrom = null;
                }
                cp.phase = content.steps[cp.stage].before.Length == 0 ? "action" : "before";
            }
            else
            {
                Assert(!cp.Commit(content), "action blocked without evidence");
                int mask = ChapterCheckpoint.RequiredMask(content.steps[cp.stage].action);
                for (int flags = 0; flags <= mask; flags++)
                {
                    cp.inspected = flags;
                    Assert(cp.IsValid(content) && Clone(cp).inspected == flags, "partial puzzle flags roundtrip");
                    if (flags != mask) Assert(!cp.Commit(content), "all evidence required");
                }
                cp.inspected = mask; Assert(cp.Commit(content), "completed action accepted"); actions++;
            }
        }
        Assert(cp.IsValid(content) && cp.stage == 9 && actions == 5 && read.Count == content.paragraphs.Length, "reviewed chapter complete");
        var travel = ChapterCheckpoint.New(content.sourceHash); travel.stage=3; travel.phase="explore"; travel.travelFrom="office";
        Assert(travel.IsValid(content) && ChapterPlaces.ActiveRoom(travel)=="office", "valid office to gallery doorway");
        travel.travelFrom="home"; Assert(!travel.IsValid(content), "reject arbitrary travel origin");
        travel.travelFrom="office"; travel.phase="before"; Assert(!travel.IsValid(content), "travel cannot skip into destination dialogue");
        travel.stage=9; travel.phase="complete"; Assert(!travel.IsValid(content), "completed chapter has no pending doorway");
        travel.stage=0; travel.phase="explore"; Assert(!travel.IsValid(content), "intro cannot forge a doorway");
        for (int i = 0; i < read.Count; i++) Assert(read[i] == i, "all paragraphs exactly once in order");
        string directory = Path.Combine(Path.GetTempPath(), "civi-lq-tests-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(directory, "save.json"); var store = new ChapterSaveStore(path);
        Assert(store.Read(_ => true) == null, "fresh user has no Continue");
        string first = JsonSerializer.Serialize(cp, Json);
        store.Write(first); Assert(File.ReadAllText(path) == first, "first save");
        string second = JsonSerializer.Serialize(ChapterCheckpoint.New(content.sourceHash), Json);
        store.Write(second); Assert(File.ReadAllText(path + ".bak") == first, "backup previous checkpoint");
        File.WriteAllText(path, "broken");
        Func<string, bool> valid = value => { try { return JsonSerializer.Deserialize<ChapterCheckpoint>(value, Json).IsValid(content); } catch { return false; } };
        Assert(store.Read(valid) == first, "corrupt primary recovers backup");
        File.WriteAllText(path + ".bak", "broken"); Assert(store.Read(valid) == null, "invalid saves hide Continue");
        store.Write(second); Assert(store.Read(valid) == second, "new game recovers from invalid existing save");
        SideTests(content);
        Console.WriteLine("PASS " + checks + " checks: canonical chapter coverage, state transitions, every partial puzzle, resume, invalid saves, atomic replacement and backup recovery.");
        // Leave only these uniquely named test artifacts for diagnosis; never delete a computed tree.
    }
}
