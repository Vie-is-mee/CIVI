using System;
using UnityEngine;
using UnityEngine.UI;

namespace Civi.Preview
{
    public sealed class NotebookDialogue : MonoBehaviour
    {
        public Text speaker, body, hint;
        public RawImage portrait;
        public Button next;
        public bool Instant;
        string full="";float clock;int visible;
        public bool Revealed => visible>=full.Length;
        public string LineId {get;private set;}
        public void Show(LineData line,CiviAssetCatalog assets,Action advance)
        {
            LineId=line.text_id;full=line.speaker_id=="NARRATOR"?line.text:"“"+line.text+"”";
            body.font=line.speaker_id=="NARRATOR"?assets.diaryFont:assets.uiFont;
            speaker.text=Name(line.speaker_id);hint.text=line.speaker_id=="NARRATOR"?"một dòng bên lề":"Space / Enter để đọc tiếp";
            visible=Instant?full.Length:0;clock=0;body.text=full.Substring(0,visible);
            int person=Array.IndexOf(new[]{"AN","NHI","HA","MINH"},line.speaker_id);
            portrait.gameObject.SetActive(person>=0);
            if(person>=0)
            {
                portrait.texture=assets.Texture("Expressions");
                int face=person*2+(line.mood=="warm"?0:1);
                portrait.uvRect=new Rect(face/8f,0,.125f,.255f);
            }
            next.onClick.RemoveAllListeners();next.onClick.AddListener(()=>{if(!Revealed)Reveal();else advance();});
        }
        public static string Name(string id)=>id=="HA"?"HẠ":id=="NARRATOR"?"GHI CHÉP CỦA AN":id;
        public void Reveal(){visible=full.Length;body.text=full;}
        void Update()
        {
            if(Revealed)return;clock+=Time.unscaledDeltaTime;int count=Mathf.Min(full.Length,(int)(clock*42));
            if(count!=visible){visible=count;body.text=full.Substring(0,visible);}
        }
        public static NotebookDialogue Create(Transform parent,CiviAssetCatalog assets)
        {
            var paper=NotebookUI.Box("NotebookDialogue",parent,65,616,1470,226,NotebookUI.Paper);
            var view=paper.gameObject.AddComponent<NotebookDialogue>();
            NotebookUI.Stroke(paper.transform,174,55,1408,55,new Color(.80f,.75f,.64f),1);
            NotebookUI.Stroke(paper.transform,163,20,163,201,new Color(.76f,.43f,.37f),1);
            view.portrait=NotebookUI.Photo("Persistent portrait",paper.transform,null,19,56,126,150);
            view.speaker=NotebookUI.Label("Speaker",paper.transform,"",184,14,1050,38,25,assets.handFont,NotebookUI.Teal);
            view.body=NotebookUI.Label("Dialogue",paper.transform,"",184,71,1225,100,29,assets.uiFont,NotebookUI.Ink);
            view.hint=NotebookUI.Label("Reading hint",paper.transform,"",185,184,750,25,17,assets.handFont,NotebookUI.Muted);
            view.next=NotebookUI.Button("Continue",paper.transform,"Đọc tiếp  →",1178,176,250,37,assets.uiFont,null);
            return view;
        }
    }
}
