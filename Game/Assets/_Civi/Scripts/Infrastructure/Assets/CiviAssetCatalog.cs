using System;
using System.Linq;
using UnityEngine;

namespace Civi.Preview
{
    [Serializable] public sealed class TextureEntry { public string id; public Texture2D texture; }
    [Serializable] public sealed class SoundEntry { public string id; public AudioClip clip; }
    [CreateAssetMenu(menuName="CIVI/TV3 AssetCatalog")]
    public sealed class CiviAssetCatalog : ScriptableObject
    {
        public TextureEntry[] textures;
        public SoundEntry[] sounds;
        public Font uiFont, handFont, diaryFont;
        public TextAsset s00Scene, s00Dialogue, s04Dialogue;
        // TV2 authoring content for the next playable scene. Populated by Editor Prepare.
        public TextAsset storyInitialState;
        public TextAsset[] storyScenes, storyDialogues;
        public TextAsset[] draftBriefs;
        public GameObject[] draftTemplates;
        public GameObject draftReviewPrefab;
        public GameObject dialoguePrefab, choicesPrefab, diagramPrefab, clampPrefab, batteryPrefab, signPrefab;
        public Texture2D Texture(string id)
        {
            var result = textures.FirstOrDefault(item => item.id == id);
            if (result == null || result.texture == null) throw new InvalidOperationException("AssetCatalog texture missing: " + id);
            return result.texture;
        }
        public AudioClip Sound(string id) => sounds.FirstOrDefault(item => item.id == id)?.clip;
        public Texture2D Gesture(string moment, string gesture) => Texture("PHOTO_" + moment + "_" + gesture.ToUpperInvariant());
        public DraftBrief Draft(string type)
        {
            foreach(var file in draftBriefs)
            {
                var brief=JsonUtility.FromJson<DraftBrief>(file.text);
                if(brief.draft_type==type)return brief;
            }
            throw new InvalidOperationException("Draft brief missing: "+type);
        }
    }
}
