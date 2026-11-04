using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Civi.Preview
{
    public sealed class S00RuntimeQA : MonoBehaviour
    {
        [Serializable] public sealed class Result { public string result="PASS",scope="S00 runtime and TV3 draft UI preview; S05 story not integrated";public List<string> errors=new List<string>();public int width,height,draft_cases;public bool team_approval=false; }
        Result result;string folder;S00PreviewPlayer player;bool finished;
        void Awake(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"--civi2-qa")>=0)Invoke(nameof(Timeout),60);}
        void Timeout(){if(result!=null&&!finished){result.errors.Add("Runtime QA exceeded 60 seconds");Finish();}}
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--civi2-qa");if(index<0)yield break;
            folder=index+1<args.Length?args[index+1]:Application.persistentDataPath;Directory.CreateDirectory(folder);
            result=new Result{width=Screen.width,height=Screen.height};Application.logMessageReceived+=Log;
            player=GetComponent<S00PreviewPlayer>();
            for(int i=0;i<120&&player.Session==null;i++)yield return null;
            if(player.Session==null){result.errors.Add("Player did not initialize");Finish();yield break;}
            player.Dialogue.Instant=true;
            yield return new WaitForEndOfFrame();yield return Shot("01-dialogue");
            string firstPortrait=player.Dialogue.portrait.uvRect.ToString();string firstLine=player.Dialogue.LineId;
            yield return new WaitForSecondsRealtime(.6f);
            if(player.Dialogue.LineId!=firstLine||player.Dialogue.portrait.uvRect.ToString()!=firstPortrait)result.errors.Add("Portrait/line reset while reading");
            int guard=0;
            while(!player.Session.State.completed&&guard++<160)
            {
                if(player.Busy){yield return null;continue;}
                if(player.Session.CanChoose)
                {
                    if(player.Session.State.step_id=="S00_POSE")yield return Shot("02-choices");
                    if(player.Session.State.step_id=="S00_ALBUM")yield return Shot("03-album");
                    var choice=player.GetComponentInChildren<NotebookChoices>();choice.buttons[0].onClick.Invoke();
                    if(player.Busy)yield return new WaitForSecondsRealtime(4.5f);
                }
                else player.Dialogue.next.onClick.Invoke();
                yield return null;
            }
            if(!player.Session.State.completed)result.errors.Add("Runtime route did not finish");
            yield return Shot("04-handoff");
            player.ShowCallback();yield return Shot("05-callback");player.CloseOverlay();
            var animaticButton=player.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name=="Animatic QA");
            if(animaticButton==null)result.errors.Add("Missing animatic button");
            else
            {
                animaticButton.onClick.Invoke();yield return null;var animatic=player.GetComponentInChildren<VyAnimatic>();
                if(animatic==null)result.errors.Add("Animatic did not open");
                else
                {
                    foreach(float time in new[]{0f,5f,9f,11f,13f,15f,17f,18f,26f,32f})
                    {
                        animatic.Seek(time);yield return null;
                        if(Mathf.Abs(animatic.Position-time)>.01f)result.errors.Add("Scrub lost position "+time);
                    }
                    animatic.Seek(18);yield return Shot("06-animatic-full-quote");
                }
            }
            player.CloseOverlay();
            string storyBefore=player.Session.Save();
            foreach(var type in new[]{"tribute","paired","memory"})foreach(var recipient in new[]{"held","nhi","group_four"})
            {
                var view=player.ShowDrafts(false);int requests=0;
                view.Requested+=request=>
                {
                    requests++;
                    if(request.DraftType!=type||request.Recipient!=recipient||request.Scope!="internal_review")result.errors.Add("Wrong draft request");
                };
                view.SelectType(type);view.SelectRecipient(recipient);
                if(requests!=0||view.Submitting)result.errors.Add("Template/recipient selection sent automatically");
                view.Confirm();
                if(!view.Reviewing||requests!=0)result.errors.Add("Preview-before-send missing");
                if(recipient=="group_four")yield return Shot("07-draft-"+type);
                view.Confirm();view.Confirm();
                if(!view.Submitting||requests!=1)result.errors.Add("Busy draft request duplicated");
                view.CompleteRequest(false,"Chưa gửi được. Bản nháp và người nhận vẫn được giữ.");
                if(view.Submitting||view.SelectedType!=type||view.SelectedRecipient!=recipient)result.errors.Add("Draft lost after error");
                if(type=="paired"&&recipient=="nhi")yield return Shot("08-draft-error-preserved");
                view.Confirm();view.Confirm();view.CompleteRequest(true,"Đã kiểm thao tác. Cốt truyện chưa thay đổi.");
                if(requests!=2||player.Session.Save()!=storyBefore)result.errors.Add("UI preview changed story or request count");
                result.draft_cases++;player.CloseOverlay();yield return null;
            }
            Finish();
        }
        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();Canvas.ForceUpdateCanvases();
            foreach(var text in player.GetComponentsInChildren<Text>())
                if(text.gameObject.activeInHierarchy&&!string.IsNullOrEmpty(text.text)&&text.preferredHeight>text.rectTransform.rect.height+2)
                    result.errors.Add("Text height overflow: "+text.name+" / "+text.preferredHeight+" > "+text.rectTransform.rect.height);
            // Force a real UI render even when the automated QA window is hidden by Windows.
            var canvas=player.GetComponentInChildren<Canvas>();var camera=Camera.main;
            var previousMode=canvas.renderMode;var previousCamera=canvas.worldCamera;
            var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;
            var target=new RenderTexture(Screen.width,Screen.height,24);var pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
                pixels.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);pixels.Apply();
                var samples=new HashSet<int>();int pink=0,total=0;
                for(int x=16;x<Screen.width;x+=32)for(int y=16;y<Screen.height;y+=32)
                {
                    var pixel=pixels.GetPixel(x,y);samples.Add(((int)(pixel.r*31)<<10)|((int)(pixel.g*31)<<5)|(int)(pixel.b*31));
                    if(pixel.r>.9f&&pixel.g<.1f&&pixel.b>.9f)pink++;total++;
                }
                if(samples.Count<24||pink>total*.20f)result.errors.Add("Rendered UI blank or missing shader: "+name+" / colors "+samples.Count);
                File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());
            }
            finally{canvas.renderMode=previousMode;canvas.worldCamera=previousCamera;camera.targetTexture=previousTarget;RenderTexture.active=previousActive;Destroy(pixels);target.Release();Destroy(target);}
            yield return null;
        }
        void Log(string message,string stack,LogType type){if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert)result.errors.Add(message);}
        void Finish(){if(finished)return;finished=true;CancelInvoke();Application.logMessageReceived-=Log;if(result.errors.Count>0)result.result="FAIL";File.WriteAllText(Path.Combine(folder,"runtime-checks.json"),JsonUtility.ToJson(result,true));Application.Quit(result.errors.Count==0?0:1);}
    }
}
