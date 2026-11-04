using UnityEngine;
using UnityEngine.UI;

namespace Civi.Preview
{
    public sealed class NotebookActor : MonoBehaviour
    {
        public RawImage image;
        public int person;
        public bool ReducedMotion;
        Vector2 origin;float emphasis;
        void Start(){origin=((RectTransform)transform).anchoredPosition;}
        public void SetSpeaker(int speaker){emphasis=person==speaker?1:0;image.color=person==speaker||speaker<0?Color.white:new Color(.93f,.93f,.93f);}
        void Update()
        {
            var rect=(RectTransform)transform;
            rect.anchoredPosition=origin+new Vector2(0,ReducedMotion?0:Mathf.Sin(Time.unscaledTime*1.35f+person)*1.6f);
            rect.localScale=Vector3.Lerp(rect.localScale,Vector3.one*(1+emphasis*.018f),Time.unscaledDeltaTime*7);
        }
    }
}
