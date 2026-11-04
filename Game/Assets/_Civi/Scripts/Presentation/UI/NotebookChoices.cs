using System;
using UnityEngine;
using UnityEngine.UI;

namespace Civi.Preview
{
    public sealed class NotebookChoices : MonoBehaviour
    {
        public Button[] buttons;
        public Text title;
        public void Show(ChoiceData[] choices,Action<string> choose)
        {
            if(choices.Length>buttons.Length)throw new InvalidOperationException("S00 choice count exceeds prefab capacity");
            for(int i=0;i<buttons.Length;i++)
            {
                var button=buttons[i];button.gameObject.SetActive(i<choices.Length);button.onClick.RemoveAllListeners();
                if(i>=choices.Length)continue;var id=choices[i].choice_id;
                button.GetComponentInChildren<Text>().text=choices[i].label;button.onClick.AddListener(()=>choose(id));
            }
        }
        public static NotebookChoices Create(Transform parent,CiviAssetCatalog assets)
        {
            var backing=NotebookUI.Box("NotebookChoices",parent,65,616,1470,226,NotebookUI.Paper);
            var view=backing.gameObject.AddComponent<NotebookChoices>();
            view.title=NotebookUI.Label("Choice heading",backing.transform,"An sẽ làm gì?",22,10,1390,42,28,assets.handFont,NotebookUI.Teal);
            view.buttons=new Button[3];
            for(int i=0;i<3;i++)view.buttons[i]=NotebookUI.Button("Choice "+(i+1),backing.transform,"",22+i*474,64,449,130,assets.uiFont,null,true);
            return view;
        }
    }
}
