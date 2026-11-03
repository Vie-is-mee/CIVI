using System;
using System.Collections.Generic;
using System.Globalization;

namespace Civi.Runtime
{
    public interface IStorySource
    {
        string SceneJson(string sceneId);
        string DialogueJson(string sceneId);
    }

    public sealed class StoryLine
    {
        public readonly string Id, Speaker, Text, Mood;
        public StoryLine(string id, string speaker, string text, string mood)
        { Id = id; Speaker = speaker; Text = text; Mood = mood; }
    }

    public sealed class StoryChoice
    {
        public readonly string Id, Label, Next;
        internal readonly List<string> Lines;
        internal readonly Dictionary<string, object> When;
        internal readonly List<object> Effects;
        public readonly int DurationMinutes;
        internal StoryChoice(Dictionary<string, object> value)
        {
            Id = JsonTree.Str(JsonTree.Need(value, "choice_id"), "choice_id");
            Label = JsonTree.Str(JsonTree.Need(value, "label"), "label");
            Next = JsonTree.Str(JsonTree.Need(value, "next"), "next");
            Lines = TextIds(JsonTree.Need(value, "lines"), Id + ".lines");
            When = JsonTree.Obj(JsonTree.Need(value, "when"), Id + ".when");
            Effects = JsonTree.Arr(JsonTree.Need(value, "effects"), Id + ".effects");
            DurationMinutes = JsonTree.Int(JsonTree.Need(value, "duration_minutes"), Id + ".duration_minutes");
            if (DurationMinutes < 0) throw new FormatException("Negative duration: " + Id);
            if (!Next.StartsWith("step:", StringComparison.Ordinal) &&
                !Next.StartsWith("scene:", StringComparison.Ordinal) && Next != "end")
                throw new FormatException("Invalid transition " + Next);
        }
        internal static List<string> TextIds(object value, string context)
        {
            var result = new List<string>();
            foreach (object item in JsonTree.Arr(value, context)) result.Add(JsonTree.Str(item, context));
            return result;
        }
    }

    public sealed class StoryStep
    {
        public readonly string Id, StageDirection;
        public readonly List<string> Lines;
        public readonly List<StoryChoice> Choices = new List<StoryChoice>();
        internal StoryStep(Dictionary<string, object> value)
        {
            Id = JsonTree.Str(JsonTree.Need(value, "step_id"), "step_id");
            StageDirection = JsonTree.Str(JsonTree.Need(value, "stage_direction"), "stage_direction");
            Lines = StoryChoice.TextIds(JsonTree.Need(value, "lines"), Id + ".lines");
            foreach (object item in JsonTree.Arr(JsonTree.Need(value, "choices"), Id + ".choices"))
                Choices.Add(new StoryChoice(JsonTree.Obj(item, Id + ".choice")));
        }
    }

    sealed class StoryScene
    {
        public readonly string Id, Entry, Version;
        public readonly Dictionary<string, StoryStep> Steps =
            new Dictionary<string, StoryStep>(StringComparer.Ordinal);
        public readonly Dictionary<string, StoryLine> Dialogues =
            new Dictionary<string, StoryLine>(StringComparer.Ordinal);

        public StoryScene(string sceneJson, string dialogueJson)
        {
            var scene = JsonTree.Obj(JsonTree.Parse(sceneJson), "scene");
            Version = JsonTree.Str(JsonTree.Need(scene, "schema_version"), "schema_version");
            if (Version != "tv1-authoring-0.1") throw new FormatException("Unsupported authoring schema: " + Version);
            Id = JsonTree.Str(JsonTree.Need(scene, "scene_id"), "scene_id");
            Entry = JsonTree.Str(JsonTree.Need(scene, "entry_step"), "entry_step");
            var dialogue = JsonTree.Obj(JsonTree.Parse(dialogueJson), "dialogue");
            if (Id != JsonTree.Str(JsonTree.Need(dialogue, "scene_id"), "dialogue.scene_id"))
                throw new FormatException("Dialogue scene mismatch");
            foreach (object item in JsonTree.Arr(JsonTree.Need(dialogue, "lines"), "dialogue.lines"))
            {
                var line = JsonTree.Obj(item, "dialogue.line");
                var id = JsonTree.Str(JsonTree.Need(line, "text_id"), "text_id");
                if (Dialogues.ContainsKey(id)) throw new FormatException("Duplicate line " + id);
                Dialogues.Add(id, new StoryLine(id,
                    JsonTree.Str(JsonTree.Need(line, "speaker_id"), "speaker_id"),
                    JsonTree.Str(JsonTree.Need(line, "text"), "text"),
                    JsonTree.Str(JsonTree.Need(line, "mood"), "mood")));
            }
            var globalChoices = new HashSet<string>(StringComparer.Ordinal);
            foreach (object item in JsonTree.Arr(JsonTree.Need(scene, "steps"), "scene.steps"))
            {
                var step = new StoryStep(JsonTree.Obj(item, "scene.step"));
                if (Steps.ContainsKey(step.Id)) throw new FormatException("Duplicate step " + step.Id);
                Steps.Add(step.Id, step);
                foreach (string id in step.Lines) RequireLine(id);
                foreach (var choice in step.Choices)
                {
                    if (!globalChoices.Add(choice.Id)) throw new FormatException("Duplicate choice " + choice.Id);
                    foreach (string id in choice.Lines) RequireLine(id);
                }
            }
            if (!Steps.ContainsKey(Entry)) throw new FormatException("Entry step missing " + Entry);
            foreach (var step in Steps.Values)
                foreach (var choice in step.Choices)
                    if (choice.Next.StartsWith("step:", StringComparison.Ordinal) &&
                        !Steps.ContainsKey(choice.Next.Substring(5)))
                        throw new FormatException("Dangling step: " + choice.Next);
        }
        void RequireLine(string id)
        {
            if (!Dialogues.ContainsKey(id)) throw new FormatException("Unknown text ID: " + id);
        }
    }

    // Runtime is editor-independent, deterministic and fail-closed for unsupported domain effects.
    // JSON effects are authored instructions, never accepted from user or model output.
    public sealed class StorySession
    {
        readonly IStorySource source;
        readonly Dictionary<string, StoryScene> cache = new Dictionary<string, StoryScene>(StringComparer.Ordinal);
        readonly Dictionary<string, object> initialState;
        Dictionary<string, object> state;
        StoryScene scene;
        string stepId, activeChoice = "", pendingNext = "";
        int lineIndex;
        bool completed;
        readonly string contentVersion;

        public string SceneId { get { return scene.Id; } }
        public string StepId { get { return stepId; } }
        public int LineIndex { get { return lineIndex; } }
        public bool IsComplete { get { return completed; } }
        public string StageDirection { get { return CurrentStep.StageDirection; } }
        StoryStep CurrentStep { get { return scene.Steps[stepId]; } }
        StoryChoice Active
        {
            get
            {
                if (activeChoice.Length == 0) return null;
                foreach (var choice in CurrentStep.Choices) if (choice.Id == activeChoice) return choice;
                throw new InvalidOperationException("Invalid active choice " + activeChoice);
            }
        }
        List<string> CurrentLineIds { get { return Active == null ? CurrentStep.Lines : Active.Lines; } }
        public StoryLine CurrentLine
        {
            get { var ids = CurrentLineIds; return completed || lineIndex >= ids.Count ? null : scene.Dialogues[ids[lineIndex]]; }
        }
        public bool CanChoose
        {
            get { return !completed && activeChoice.Length == 0 && lineIndex == CurrentStep.Lines.Count &&
                         CurrentStep.Choices.Count > 0; }
        }
        public List<StoryChoice> AvailableChoices
        {
            get
            {
                var result = new List<StoryChoice>();
                if (!CanChoose) return result;
                foreach (var c in CurrentStep.Choices) if (Condition(c.When, state)) result.Add(c);
                return result;
            }
        }

        public object ReadState(string key)
        {
            object v;
            if (!state.TryGetValue(key, out v)) throw new KeyNotFoundException("Unknown state " + key);
            return JsonTree.Clone(v);
        }

        public StorySession(IStorySource storySource, string initialJson, string firstScene)
        {
            source = storySource ?? throw new ArgumentNullException(nameof(storySource));
            var doc = JsonTree.Obj(JsonTree.Parse(initialJson), "initial state");
            contentVersion = JsonTree.Str(JsonTree.Need(doc, "content_version"), "content_version");
            initialState = JsonTree.Obj(JsonTree.Need(doc, "state"), "state");
            state = JsonTree.Obj(JsonTree.Clone(initialState), "copy");
            scene = Load(firstScene);
            stepId = scene.Entry;
        }

        // Import only verified S00 result, before the first S01 interaction.
        public void ImportS00(string albumName, string gesture, string support, bool captured,
                              IEnumerable<string> evidence, IEnumerable<string> facts)
        {
            if (scene.Id != "S01" || stepId != scene.Entry || activeChoice.Length != 0 ||
                lineIndex != 0 || completed) throw new InvalidOperationException("S00 handoff already passed");
            if (!new[] { "pose_1", "pose_2", "pose_3" }.ContainsOrdinal(gesture) ||
                !new[] { "self", "clamp", "ask" }.ContainsOrdinal(support) || !captured)
                throw new InvalidOperationException("Invalid S00 handoff");
            var next = JsonTree.Obj(JsonTree.Clone(state), "state");
            next["album_name"] = albumName ?? throw new ArgumentNullException(nameof(albumName));
            next["opening_gesture"] = gesture; next["opening_support"] = support;
            next["opening_photos_created"] = true;
            next["evidence_seen"] = Strings(evidence, "evidence");
            next["facts_discovered"] = Strings(facts, "facts");
            state = next;
        }

        static List<object> Strings(IEnumerable<string> values, string label)
        {
            if (values == null) throw new ArgumentNullException(label);
            var result = new List<object>();
            foreach (var s in values)
            {
                if (String.IsNullOrEmpty(s)) throw new FormatException("Empty " + label);
                if (!result.Contains(s)) result.Add(s);
            }
            return result;
        }

        public void Advance()
        {
            if (completed || CanChoose) return;
            if (lineIndex < CurrentLineIds.Count) lineIndex++;
            if (lineIndex >= CurrentLineIds.Count && activeChoice.Length != 0) Follow();
        }

        public void Choose(string id)
        {
            if (!CanChoose) throw new InvalidOperationException("Finish reading first");
            StoryChoice choice = null;
            foreach (var item in AvailableChoices) if (item.Id == id) { choice = item; break; }
            if (choice == null) throw new InvalidOperationException("Choice blocked or unknown: " + id);
            // Resolve destination before mutation.
            if (choice.Next.StartsWith("scene:", StringComparison.Ordinal)) Load(choice.Next.Substring(6));
            var candidate = JsonTree.Obj(JsonTree.Clone(state), "candidate");
            foreach (object raw in choice.Effects) ApplyEffect(JsonTree.Obj(raw, choice.Id + ".effect"), candidate);
            // Atomic commit: no partial state mutation if any effect is invalid.
            state = candidate;
            activeChoice = id;
            pendingNext = choice.Next;
            lineIndex = 0;
            if (choice.Lines.Count == 0) Follow();
        }

        void ApplyEffect(Dictionary<string, object> effect, Dictionary<string, object> candidate)
        {
            string op = JsonTree.Str(JsonTree.Need(effect, "op"), "effect.op");
            if (op == "grant")
                throw new NotSupportedException("Consent grant requires a validated domain command");
            string field = JsonTree.Str(JsonTree.Need(effect, "field"), "effect.field");
            if (!candidate.ContainsKey(field)) throw new InvalidOperationException("Unknown state field " + field);
            if (op == "set")
            {
                object value = JsonTree.Need(effect, "value");
                object before = candidate[field];
                if (before != null && value != null && before.GetType() != value.GetType())
                    throw new InvalidOperationException("State type mismatch for " + field);
                candidate[field] = JsonTree.Clone(value); return;
            }
            if (op == "add_unique")
            {
                var list = JsonTree.Arr(candidate[field], "state." + field);
                object value = JsonTree.Need(effect, "value");
                bool found = false;
                foreach (object item in list) if (JsonTree.Same(item, value)) found = true;
                if (!found) list.Add(JsonTree.Clone(value));
                return;
            }
            if (op == "advance_minutes")
            {
                long current = (long)candidate[field];
                int delta = JsonTree.Int(JsonTree.Need(effect, "value"), "advance_minutes.value");
                if (delta < 0) throw new InvalidOperationException("Cannot reverse time");
                candidate[field] = checked(current + delta);
                return;
            }
            throw new NotSupportedException("Unsupported effect " + op);
        }

        void Follow()
        {
            var target = pendingNext;
            if (target.StartsWith("step:", StringComparison.Ordinal))
                stepId = target.Substring(5);
            else if (target.StartsWith("scene:", StringComparison.Ordinal))
            {
                scene = Load(target.Substring(6)); stepId = scene.Entry;
            }
            else if (target == "end") completed = true;
            else throw new InvalidOperationException("Invalid transition: " + target);
            activeChoice = ""; pendingNext = ""; lineIndex = 0;
        }

        StoryScene Load(string id)
        {
            if (String.IsNullOrWhiteSpace(id)) throw new FormatException("Missing scene ID");
            StoryScene loaded;
            if (!cache.TryGetValue(id, out loaded))
            {
                loaded = new StoryScene(source.SceneJson(id), source.DialogueJson(id));
                if (loaded.Id != id) throw new FormatException("Scene ID mismatch: " + id);
                cache.Add(id, loaded);
            }
            return loaded;
        }

        static bool Condition(Dictionary<string, object> c, Dictionary<string, object> stateValues)
        {
            object v;
            if (c.TryGetValue("all", out v))
            {
                foreach (object item in JsonTree.Arr(v, "condition.all"))
                    if (!Condition(JsonTree.Obj(item, "condition.item"), stateValues)) return false;
                return true;
            }
            if (c.TryGetValue("any", out v))
            {
                foreach (object item in JsonTree.Arr(v, "condition.any"))
                    if (Condition(JsonTree.Obj(item, "condition.item"), stateValues)) return true;
                return false;
            }
            if (c.TryGetValue("not", out v))
                return !Condition(JsonTree.Obj(v, "condition.not"), stateValues);
            string field = JsonTree.Str(JsonTree.Need(c, "field"), "condition.field");
            string op = JsonTree.Str(JsonTree.Need(c, "op"), "condition.op");
            object actual;
            if (!stateValues.TryGetValue(field, out actual)) throw new FormatException("Unknown condition field " + field);
            object expected = JsonTree.Need(c, "value");
            switch (op)
            {
                case "eq": return JsonTree.Same(actual, expected);
                case "ne": return !JsonTree.Same(actual, expected);
                case "contains":
                {
                    var list = JsonTree.Arr(actual, field);
                    foreach (object item in list) if (JsonTree.Same(item, expected)) return true;
                    return false;
                }
                case "in":
                {
                    foreach (object item in JsonTree.Arr(expected, "condition.in"))
                        if (JsonTree.Same(actual, item)) return true;
                    return false;
                }
                case "gte":
                case "lt":
                {
                    if (!(actual is long || actual is double) || !(expected is long || expected is double))
                        throw new FormatException("Numeric comparison requires numbers: " + field);
                    double lhs = Convert.ToDouble(actual, CultureInfo.InvariantCulture);
                    double rhs = Convert.ToDouble(expected, CultureInfo.InvariantCulture);
                    return op == "gte" ? lhs >= rhs : lhs < rhs;
                }
                default: throw new NotSupportedException("Unsupported condition operator " + op);
            }
        }

        public string Save()
        {
            return JsonTree.Write(new Dictionary<string, object>
            {
                { "save_version", 1L }, { "content_version", contentVersion },
                { "scene_id", scene.Id }, { "step_id", stepId }, { "line_index", (long)lineIndex },
                { "active_choice", activeChoice }, { "pending_next", pendingNext },
                { "completed", completed }, { "state", JsonTree.Clone(state) }
            });
        }

        public void Restore(string json)
        {
            var doc = JsonTree.Obj(JsonTree.Parse(json), "save");
            if (JsonTree.Int(JsonTree.Need(doc, "save_version"), "save_version") != 1)
                throw new FormatException("Unsupported save version");
            if (JsonTree.Str(JsonTree.Need(doc, "content_version"), "content_version") != contentVersion)
                throw new FormatException("Content version mismatch");
            var restoreScene = Load(JsonTree.Str(JsonTree.Need(doc, "scene_id"), "scene_id"));
            string step = JsonTree.Str(JsonTree.Need(doc, "step_id"), "step_id");
            if (!restoreScene.Steps.ContainsKey(step)) throw new FormatException("Unknown saved step");
            string active = JsonTree.Str(JsonTree.Need(doc, "active_choice"), "active_choice");
            string pending = JsonTree.Str(JsonTree.Need(doc, "pending_next"), "pending_next");
            int index = JsonTree.Int(JsonTree.Need(doc, "line_index"), "line_index");
            object done = JsonTree.Need(doc, "completed");
            if (!(done is bool)) throw new FormatException("completed must be boolean");
            var saved = JsonTree.Obj(JsonTree.Need(doc, "state"), "state");
            if (saved.Count != initialState.Count) throw new FormatException("Invalid state field count");
            foreach (var entry in initialState)
            {
                object value;
                if (!saved.TryGetValue(entry.Key, out value)) throw new FormatException("Missing state field " + entry.Key);
                if (entry.Value != null && value != null && entry.Value.GetType() != value.GetType())
                    throw new FormatException("State field type mismatch " + entry.Key);
            }
            StoryChoice choice = null;
            foreach (var c in restoreScene.Steps[step].Choices) if (c.Id == active) choice = c;
            if ((active.Length > 0 && (choice == null || pending != choice.Next)) ||
                (active.Length == 0 && pending.Length != 0))
                throw new FormatException("Invalid pending choice");
            int count = choice == null ? restoreScene.Steps[step].Lines.Count : choice.Lines.Count;
            if (index < 0 || index > count) throw new FormatException("Invalid line index");
            if ((bool)done && (active.Length != 0 || index != 0))
                throw new FormatException("Invalid completion state");
            // Restore atomically only after all checks passed.
            scene = restoreScene; stepId = step; activeChoice = active; pendingNext = pending;
            lineIndex = index; completed = (bool)done; state = JsonTree.Obj(JsonTree.Clone(saved), "state");
        }
    }

    static class StringArrayExtensions
    {
        public static bool ContainsOrdinal(this string[] source, string value)
        {
            foreach (string item in source) if (String.Equals(item, value, StringComparison.Ordinal)) return true;
            return false;
        }
    }
}
