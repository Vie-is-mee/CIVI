using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Civi.Preview
{
    public sealed class VyAnimatic : MonoBehaviour
    {
        public const float MainStart=9f, MainEnd=18f, Duration=32f;
        public bool ReducedMotion;
        public bool Playing {get;private set;}
        public float Position {get;private set;}
        CiviAssetCatalog assets;Text subtitle,phase,clock;Slider scrub;RawImage background;
        RectTransform phone,hand;AudioSource audioSource;int lastCue=-1;GameObject clampProp,castLayer;RectTransform nhiActor;
        public static readonly float[] Times={0,5,9,11,13,15,17,18,26};
        public static readonly string[] Ids={"S04_L003","S04_L004","S04_L005","S04_L006","S04_L007","S04_L008","S04_L009","S04_L010","S04_L011"};
        LineData[] words;
        public int CueIndex(float time){int result=0;for(int i=0;i<Times.Length;i++)if(time>=Times[i])result=i;return result;}
        public void Open(CiviAssetCatalog catalog,Transform parent,Action close)
        {
            assets=catalog;transform.SetParent(parent,false);NotebookUI.Place((RectTransform)transform,0,0,1600,900);
            NotebookUI.Box("Backdrop",transform,0,0,1600,900,new Color(.06f,.12f,.13f,1));
            NotebookUI.Label("Title",transform,"ĐOẠN QUAY CỦA VY / bản dựng có phụ đề",74,33,1210,48,32,assets.handFont,NotebookUI.Paper);
            NotebookUI.Button("Close",transform,"Đóng ×",1320,33,200,44,assets.uiFont,()=>close());
            background=NotebookUI.Photo("Documented courtyard",transform,assets.Texture("BG_COURTYARD"),75,106,925,520);
            phase=NotebookUI.Label("Segment",transform,"",1035,115,495,42,27,assets.handFont,NotebookUI.Paper);
            subtitle=NotebookUI.Label("Full subtitle",transform,"",1035,183,470,330,27,assets.uiFont,NotebookUI.Paper);
            NotebookUI.Label("Scope",transform,"Chỉ để đối chiếu nội bộ. Chưa có quyền phát hành clip.",1035,550,475,90,21,assets.handFont,new Color(.82f,.83f,.73f));
            castLayer=NotebookUI.Group("Four people in animatic before photo",transform,0,0,1600,900).gameObject;
            for(int i=0;i<4;i++)
            {
                var body=NotebookUI.Photo("Animatic person "+i,castLayer.transform,assets.Texture("Cast"),115+i*210,210,175,350,new Rect(i*.25f,0,.25f,1));
                if(i==1)nhiActor=body.rectTransform;
            }
            phone=NotebookUI.Box("Phone handed from An to Nhi",transform,195,395,32,58,NotebookUI.Ink).rectTransform;
            NotebookUI.Box("Phone screen",phone,5,6,32,57,new Color(.67f,.79f,.77f));
            hand=NotebookUI.Box("Nhi points to clamp",transform,600,320,58,8,NotebookUI.Red).rectTransform;
            clampProp=Instantiate(assets.clampPrefab,transform);NotebookUI.Place((RectTransform)clampProp.transform,560,423,200,185);
            var sliderRect=NotebookUI.Group("Timeline",transform,90,680,1425,34);
            var track=NotebookUI.Box("Track",sliderRect,0,13,1425,8,new Color(.43f,.53f,.51f));
            var handle=NotebookUI.Box("Handle",sliderRect,0,0,18,34,NotebookUI.Paper);
            scrub=sliderRect.gameObject.AddComponent<Slider>();scrub.direction=Slider.Direction.LeftToRight;scrub.minValue=0;scrub.maxValue=Duration;
            scrub.handleRect=handle.rectTransform;scrub.targetGraphic=handle;
            scrub.onValueChanged.AddListener(value=>Seek(value));
            clock=NotebookUI.Label("Clock",transform,"",90,730,650,40,25,assets.uiFont,NotebookUI.Paper);
            NotebookUI.Button("Play pause",transform,"Phát / Dừng",75,807,245,48,assets.uiFont,()=>Playing=!Playing,true);
            NotebookUI.Button("Restart",transform,"Xem từ đầu",344,807,245,48,assets.uiFont,()=>{Seek(0);Playing=true;},true);
            NotebookUI.Button("Main nine seconds",transform,"Đến 9 giây chính",613,807,310,48,assets.uiFont,()=>{Seek(MainStart);Playing=false;},true);
            NotebookUI.Label("Reading",transform,"Có thể dừng và kéo lại để đọc. Mốc 09–18 là chín giây chính.",965,802,555,60,22,assets.handFont,NotebookUI.Paper);
            words=JsonUtility.FromJson<DialogueData>(assets.s04Dialogue.text).lines;
            audioSource=gameObject.AddComponent<AudioSource>();audioSource.playOnAwake=false;
            Seek(0);Playing=false;
        }
        public void Seek(float value){Position=Mathf.Clamp(value,0,Duration);Render(false);}
        void Update(){if(!Playing)return;Position=Mathf.Min(Duration,Position+Time.unscaledDeltaTime);Render(true);if(Position>=Duration)Playing=false;}
        void Render(bool playCue)
        {
            if(words==null)return;int cue=CueIndex(Position);
            var line=words.Single(item=>item.text_id==Ids[cue]);
            subtitle.text=NotebookDialogue.Name(line.speaker_id)+"\n\n"+(line.speaker_id=="NARRATOR"?line.text:"“"+line.text+"”");
            phase.text=Position<MainStart?"TRƯỚC KHI GIAO MÁY":Position<MainEnd?"CHÍN GIÂY CHÍNH":"SAU ĐÓ / NGUYÊN CÂU NHI NÓI";
            clock.text=Position.ToString("00.0")+" / 32.0 giây";scrub.SetValueWithoutNotify(Position);
            background.texture=assets.Texture(Position>=17?"PHOTO_0443":"BG_COURTYARD");
            phone.gameObject.SetActive(Position>=MainStart&&Position<17);hand.gameObject.SetActive(Position>=13&&Position<17);
            clampProp.SetActive(Position>=MainStart&&Position<17);castLayer.SetActive(Position<17);
            NotebookUI.Place(nhiActor,ReducedMotion?325:Mathf.Lerp(405,325,Mathf.Clamp01(Position/3)),210,175,350);
            float progress=Mathf.Clamp01((Position-MainStart)/2f);
            NotebookUI.Place(phone,ReducedMotion?(progress<1?195:410):Mathf.Lerp(195,410,progress),395,32,58);
            if(playCue&&cue!=lastCue)
            {
                string sound=cue==2?"CiviPaperRustle":cue==4?"CiviClampClick":cue==6?"CiviShutter":"";
                if(sound!=""&&assets.Sound(sound)!=null)audioSource.PlayOneShot(assets.Sound(sound),.30f);
            }
            lastCue=cue;
        }
    }
}
