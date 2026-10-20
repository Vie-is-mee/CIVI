using System;
using System.Collections.Generic;
namespace Civi41
{
    public static class Checks
    {
        static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException("CIVI41 QA: "+message);}
        static void Last(Run s){s.line=Story.Lines(s).Length-1;}
        public static Run Route(int route)
        {
            var s=new Run();int guard=0;
            while(s.node!=20 && guard++<200)
            {
                Last(s);string mode=Story.Mode(s);
                if(mode=="next"){Require(Story.Advance(s),"Cannot advance "+s.node);continue;}
                string action="";
                switch(s.node)
                {
                    case 0:s.hookClues=1;action="hook_done";break;
                    case 1:action="gesture:"+(route%3);break;
                    case 2:action="capture_opening";break;
                    case 4:action=route%2==0?"ask_nhi":"photos";break;
                    case 6:s.saw0443=true;if(!s.askedNhi)s.sawVideo=true;action="evidence_done";break;
                    case 8:action="draft:"+(route%3);break;
                    case 10:action=route%2==0?"own":"deflect";break;
                    case 11:action=route==0 || route==3?"negotiate:keep":route==1?"negotiate:replace":"negotiate:ask";break;
                    case 12:action=route==0?"invite:accepted":route==2?"invite:not_invited":"invite:declined";break;
                    case 16:action=route<2?"capture":route==2?"keep_old":"propose_later";break;
                    case 18:action="capture_later";break;
                    default:throw new InvalidOperationException("Uncovered node "+s.node);
                }
                Require(Story.Act(s,action),"Rejected route command "+s.node+" "+action);Require(s.Valid(),"Invalid route state");
            }
            Require(guard<200 && s.CanFinish,"Route did not reach a publishable page");return s;
        }
        public static string RunAll()
        {
            var seen=new HashSet<int>();
            for(int route=0;route<4;route++)
            {
                var s=Route(route);seen.Add(s.photoCount);Last(s);Require(Story.Act(s,"publish"),"Cannot complete ending");Require(s.finished && s.Valid(),"Ending invalid");
                Require(s.ending==(route==2?"B":route==3?"C":"A"),"Wrong ending");
                Require(s.photoCount==(route==0?4:route==1?3:route==2?0:2),"Photo participants mismatch");
                Require(!Story.Act(s,"publish"),"Duplicate publication accepted");
            }
            Require(seen.Count==4,"Choices collapse into same photograph");
            var hook=new Run();Require(!Story.Act(hook,"hook_done"),"Empty proof skipped");hook.hookClues=2;Require(Story.Act(hook,"hook_done"),"Proof inspection did not open flashback");
            foreach(string reply in new[]{"own","deflect"})
            {
                var talk=new Run{node=10};Last(talk);Require(Story.Act(talk,reply),"Opening reply rejected");Require(!talk.haCollaborates,"A sentence auto-earned collaboration");Last(talk);Require(Story.Act(talk,"negotiate:keep") && talk.kept0443 && talk.haCollaborates,"A concrete agreement did not change the page");
            }
            var unsafeState=new Run{node=20,openingPhoto=true,photoCount=4};
            Require(!Story.Act(unsafeState,"publish"),"Published without consent");
            var clue=new Run{node=6,saw0443=true};Last(clue);Require(!Story.Act(clue,"evidence_done"),"Unknown camera handoff skipped");
            clue.askedNhi=true;Require(Story.Act(clue,"evidence_done"),"Direct witness incorrectly gated by video");
            var invite=new Run{node=12};Last(invite);Require(!Story.Act(invite,"invite:pending"),"Unsupported invite accepted");
            var wrongScene=new Run();Require(!Story.Act(wrongScene,"capture"),"Action from wrong scene accepted");
            var bad=new Run{photoCount=2};Require(!bad.Valid(),"Two-person photo without agreed later date");
            return "PASS: 4 routes, cold-open inspection, negotiation independent of apology, kept-photo consequence, consent, direct testimony, invalid commands, duplicate protection.";
        }
    }
}
