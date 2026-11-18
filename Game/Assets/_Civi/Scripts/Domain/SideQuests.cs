using System;
using System.Collections.Generic;

namespace Civi.Story
{
    [Serializable] public sealed class SideItem { public string id, name, text; }

    // Adaptation content: side quests never change what happens in STORY.md, they only colour the end-of-chapter report.
    [Serializable] public sealed class SideQuest
    {
        public string id, title, giver, turnIn, item, offer, progress, done, reward;
        public int unlock, count;
        public string Receiver { get { return string.IsNullOrEmpty(turnIn) ? giver : turnIn; } }
    }

    [Serializable] public sealed class SideNote { public string id, title, text; }

    [Serializable] public sealed class SideData
    {
        public SideItem[] items;
        public SideQuest[] quests;
        public SideNote[] notes;
        public SideItem Item(string id) { return Array.Find(items, i => i.id == id); }
        public SideQuest Quest(string id) { return Array.Find(quests, q => q.id == id); }
    }

    [Serializable] public sealed class SideProgress
    {
        public List<string> taken = new List<string>(), active = new List<string>(), finished = new List<string>(), flags = new List<string>();
        public List<string> bag = new List<string>();
        public List<int> bagCounts = new List<int>();
        public int orbs;

        public int Count(string item) { int i = bag.IndexOf(item); return i < 0 ? 0 : bagCounts[i]; }
        public void Add(string item, int amount)
        {
            int i = bag.IndexOf(item);
            if (i < 0) { bag.Add(item); bagCounts.Add(amount); }
            else bagCounts[i] += amount;
            i = bag.IndexOf(item);
            if (bagCounts[i] <= 0) { bag.RemoveAt(i); bagCounts.RemoveAt(i); }
        }
        public bool Has(string flag) { return flags.Contains(flag); }
        public void Flag(string flag) { if (!flags.Contains(flag)) flags.Add(flag); }
        public bool Taken(string id) { return taken.Contains(id); }

        // A world pickup can pay out exactly once, however many times the player presses E or reloads.
        public bool Take(string id, string item)
        {
            if (taken.Contains(id)) return false;
            taken.Add(id);
            if (!string.IsNullOrEmpty(item)) Add(item, 1);
            return true;
        }

        public string State(SideQuest q, int stage)
        {
            if (finished.Contains(q.id)) return "done";
            if (active.Contains(q.id)) return Ready(q) ? "ready" : "active";
            return stage >= q.unlock ? "offer" : "locked";
        }
        bool Ready(SideQuest q)
        {
            return string.IsNullOrEmpty(q.item) ? Has(q.id + ".done") : Count(q.item) >= q.count;
        }
        public bool Accept(SideQuest q, int stage)
        {
            if (State(q, stage) != "offer") return false;
            active.Add(q.id);
            return true;
        }
        public bool Finish(SideQuest q, int stage)
        {
            if (State(q, stage) != "ready") return false;
            if (!string.IsNullOrEmpty(q.item)) Add(q.item, -q.count);
            active.Remove(q.id);
            finished.Add(q.id);
            return true;
        }

        public bool IsValid(SideData data)
        {
            if (data == null || taken == null || active == null || finished == null || flags == null || bag == null || bagCounts == null) return false;
            if (bag.Count != bagCounts.Count || orbs < 0) return false;
            for (int i = 0; i < bag.Count; i++)
                if (bagCounts[i] <= 0 || data.Item(bag[i]) == null) return false;
            foreach (string id in active) if (data.Quest(id) == null || finished.Contains(id)) return false;
            foreach (string id in finished) if (data.Quest(id) == null) return false;
            return true;
        }
    }
}
