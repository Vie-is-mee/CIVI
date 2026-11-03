using System;
using Civi.Runtime;
using UnityEngine;

namespace Civi.Preview
{
    // Adapter from the existing serialized TV3 catalog to the pure C# story runtime.
    // No disk/editor API is required by a packaged Windows build.
    public sealed class UnityStorySource : IStorySource
    {
        readonly CiviAssetCatalog catalog;
        public UnityStorySource(CiviAssetCatalog value)
        {
            catalog = value ?? throw new ArgumentNullException(nameof(value));
            if (catalog.storyInitialState == null ||
                catalog.storyScenes == null || catalog.storyDialogues == null)
                throw new InvalidOperationException("TV2 story assets missing. Run CIVI > TV3 > Prepare S00 in the matching Unity Editor.");
        }
        public string SceneJson(string id) { return Find(catalog.storyScenes, id, "scene"); }
        public string DialogueJson(string id) { return Find(catalog.storyDialogues, id, "dialogue"); }
        static string Find(TextAsset[] assets, string id, string kind)
        {
            TextAsset found = null;
            foreach (var entry in assets)
            {
                if (entry == null) throw new InvalidOperationException("Null " + kind + " TextAsset");
                if (entry.name != id) continue;
                if (found != null) throw new InvalidOperationException("Duplicate " + kind + " asset " + id);
                found = entry;
            }
            if (found == null) throw new InvalidOperationException("Missing " + kind + " asset " + id);
            return found.text;
        }
    }
}
