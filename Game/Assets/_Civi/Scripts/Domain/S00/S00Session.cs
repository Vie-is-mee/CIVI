using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Civi.Preview
{
    [Serializable] public sealed class SceneData { public string schema_version, scene_id, title, entry_step; public StepData[] steps; }
    [Serializable] public sealed class StepData { public string step_id, stage_direction; public string[] lines; public ChoiceData[] choices; }
    [Serializable] public sealed class ChoiceData { public string choice_id, label, next; public string[] lines; }
    [Serializable] public sealed class DialogueData { public string scene_id; public LineData[] lines; }
    [Serializable] public sealed class LineData { public string text_id, speaker_id, text, mood; }
    [Serializable] public sealed class S00Bookmark
    {
        public int version = 1;
        public string scene_id = "S00", step_id = "S00_MACHINE", pending_next = "", active_choice = "";
        public int line_index;
        public string opening_support = "self", opening_gesture = "pose_1", album_name = "Một lần đủ bốn";
        public bool opening_photos_created, completed;
        public List<string> evidence_seen = new List<string>(), facts_discovered = new List<string>();
    }

    // Deliberately scoped to S00: unrecognized commands cannot alter later consent/ending state.
    public sealed class S00Session
    {
        public readonly SceneData Scene;
        public readonly Dictionary<string, LineData> Lines;
        public S00Bookmark State { get; private set; } = new S00Bookmark();
        public StepData Step => Scene.steps.Single(s => s.step_id == State.step_id);
        public string[] CurrentLines => string.IsNullOrEmpty(State.active_choice) ? Step.lines : Step.choices.Single(c => c.choice_id == State.active_choice).lines;
        public LineData CurrentLine => State.line_index < CurrentLines.Length ? Lines[CurrentLines[State.line_index]] : null;
        public bool CanChoose => !State.completed && string.IsNullOrEmpty(State.active_choice) && State.line_index >= Step.lines.Length;
        public S00Session(string scene, string dialogue)
        {
            Scene = JsonUtility.FromJson<SceneData>(scene);
            var words = JsonUtility.FromJson<DialogueData>(dialogue);
            if (Scene.scene_id != "S00" || words.scene_id != "S00" || Scene.schema_version != "tv1-authoring-0.1") throw new InvalidOperationException("Unsupported S00 authoring contract");
            Lines = words.lines.ToDictionary(l => l.text_id);
            foreach (var step in Scene.steps)
            {
                foreach (var id in step.lines.Concat(step.choices.SelectMany(c => c.lines)))
                    if (!Lines.ContainsKey(id)) throw new InvalidOperationException("Missing line " + id);
                foreach (var choice in step.choices) ValidateCommand(choice.choice_id);
            }
            State.step_id = Scene.entry_step;
        }
        static readonly string[] Commands = {"S00_BOX","S00_CLAMP","S00_ASK","S00_POSE1","S00_POSE2","S00_POSE3","S00_SHOOT","S00_ALBUM1","S00_ALBUM2","S00_ALBUM3","S00_KEEP"};
        static void ValidateCommand(string id) { if (!Commands.Contains(id)) throw new InvalidOperationException("Command requires TV2 review: " + id); }
        public void Choose(string id)
        {
            if (!CanChoose) throw new InvalidOperationException("Read the current lines first");
            var choice = Step.choices.Single(c => c.choice_id == id);
            ValidateCommand(id);
            switch (id)
            {
                case "S00_BOX": State.opening_support = "self"; break;
                case "S00_CLAMP": State.opening_support = "clamp"; break;
                case "S00_ASK": State.opening_support = "ask"; break;
                case "S00_POSE1": State.opening_gesture = "pose_1"; break;
                case "S00_POSE2": State.opening_gesture = "pose_2"; break;
                case "S00_POSE3": State.opening_gesture = "pose_3"; break;
                case "S00_SHOOT": State.opening_photos_created = true; foreach (var photo in new[]{"PHOTO_S00_BAD_01","PHOTO_S00_BAD_02","PHOTO_S00_GOOD"}) Add(State.evidence_seen,photo); break;
                case "S00_ALBUM1": State.album_name = "Một lần đủ bốn"; break;
                case "S00_ALBUM2": State.album_name = "Đèn chưa chịu sáng"; break;
                case "S00_ALBUM3": State.album_name = "Minh đừng nhìn pin"; break;
                case "S00_KEEP": Add(State.evidence_seen,"PROP_DIAGRAM"); Add(State.facts_discovered,"F01"); Add(State.facts_discovered,"F02"); break;
            }
            State.active_choice = id; State.pending_next = choice.next; State.line_index = 0;
            if (choice.lines.Length == 0) Follow();
        }
        static void Add(List<string> list, string value) { if (!list.Contains(value)) list.Add(value); }
        public void Advance()
        {
            if (State.completed || CanChoose) return;
            State.line_index++;
            if (State.line_index >= CurrentLines.Length && !string.IsNullOrEmpty(State.active_choice)) Follow();
        }
        void Follow()
        {
            var next = State.pending_next;
            State.active_choice = ""; State.pending_next = ""; State.line_index = 0;
            if (next == "scene:S01") { State.completed = true; return; }
            if (!next.StartsWith("step:") || !Scene.steps.Any(s => s.step_id == next.Substring(5))) throw new InvalidOperationException("Unknown transition " + next);
            State.step_id = next.Substring(5);
        }
        public string Save() => JsonUtility.ToJson(State, true);
        public void Restore(string json)
        {
            var candidate = JsonUtility.FromJson<S00Bookmark>(json);
            if (candidate == null || candidate.version != 1 || candidate.scene_id != "S00" || candidate.evidence_seen == null || candidate.facts_discovered == null) throw new InvalidOperationException("Invalid S00 bookmark");
            if (!new[]{"self","clamp","ask"}.Contains(candidate.opening_support) || !new[]{"pose_1","pose_2","pose_3"}.Contains(candidate.opening_gesture)) throw new InvalidOperationException("Invalid opening choices");
            var step = Scene.steps.Single(s => s.step_id == candidate.step_id);
            var choice = string.IsNullOrEmpty(candidate.active_choice) ? null : step.choices.Single(c => c.choice_id == candidate.active_choice);
            int length = choice == null ? step.lines.Length : choice.lines.Length;
            if (candidate.line_index < 0 || candidate.line_index > length || (choice == null && candidate.pending_next != "") || (choice != null && candidate.pending_next != choice.next)) throw new InvalidOperationException("Invalid bookmark position");
            if (candidate.completed && (candidate.step_id != "S00_DIAGRAM" || !candidate.facts_discovered.Contains("F02"))) throw new InvalidOperationException("Invalid completed bookmark");
            State = candidate;
        }
    }
}
