using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Civi.Runtime;

static class Program
{
    static int assertions;
    static void Is(bool condition,string text)
    {
        assertions++;
        if(!condition)throw new Exception("FAILED "+text);
    }
    static void Throws<T>(Action action,string label) where T:Exception
    {
        bool caught=false;
        try{action();}catch(T){caught=true;}
        Is(caught,label);
    }
    static void Main(string[] args)
    {
        try
        {
            var folder=args.Length>0?args[0]:Path.GetFullPath("../Game/Assets/_Civi/Content/Story/v4_1/scenes");
            var book=new InvestigationNotebook();
            Is(InvestigationNotebook.Sources.Length==4,"exactly four authored sources");
            Is(book.ObservedCount==0 && !book.HasWorkingHypothesis,"empty notebook");
            foreach(var entry in InvestigationNotebook.Sources)
            {
                Is(InvestigationNotebook.IsAllowed(entry.Id),"author whitelist "+entry.Id);
                Is(!String.IsNullOrEmpty(entry.Question) && !String.IsNullOrEmpty(entry.Observation),"inspection text "+entry.Id);
            }
            Throws<ArgumentException>(()=>book.Observe("PHOTO_FAKE"),"no invented source");
            Is(book.Propose("unconfirmed").Result==InvestigationNotebook.Verdict.NeedMoreSources,"not enough evidence");
            Is(book.Observe("PHOTO_0442"),"first observation");
            Is(!book.Observe("PHOTO_0442"),"duplicate ignored");
            Is(book.ObservedCount==1,"duplicate leaves count unchanged");
            Is(book.Propose("unconfirmed").Result==InvestigationNotebook.Verdict.NeedMoreSources,"without 0443 not supported");
            Is(book.Observe("PHOTO_0443"),"photo 0443 recorded");
            Is(book.Propose("unconfirmed").Result==InvestigationNotebook.Verdict.PlausibleNeedsVerification,"provisional hypothesis");
            Is(book.HasWorkingHypothesis,"working hypothesis only");
            Is(book.Propose("intentional").Result==InvestigationNotebook.Verdict.Unsupported,"no intent attribution");
            Is(!book.HasWorkingHypothesis,"reject intent as working");
            Is(book.Propose("consent").Result==InvestigationNotebook.Verdict.Unsupported,"no implicit consent");
            Throws<ArgumentException>(()=>book.Propose("NHI_DID_IT"),"reject untrusted hypothesis");
            Is(book.Propose("unconfirmed").Result==InvestigationNotebook.Verdict.PlausibleNeedsVerification,"may return to provisional");
            string[] ids=book.ObservedIds;
            var copy=new InvestigationNotebook();
            copy.Restore(ids,book.LastHypothesis,book.HasWorkingHypothesis);
            Is(copy.ObservedIds.SequenceEqual(ids) && copy.HasWorkingHypothesis,"notebook restore");
            var original=copy.ObservedIds;
            Throws<FormatException>(()=>copy.Restore(new[]{"PHOTO_0442","BAD"},"unconfirmed",true),"unknown rejected");
            Is(copy.ObservedIds.SequenceEqual(original),"restore failure atomic");
            Throws<FormatException>(()=>copy.Restore(new[]{"PHOTO_0443","PHOTO_0443"},"unconfirmed",true),"duplicates rejected");
            Throws<FormatException>(()=>copy.Restore(new[]{"PHOTO_0441","PHOTO_0442"},"unconfirmed",true),"working hypothesis must include 0443");
            Throws<FormatException>(()=>copy.Restore(original,"consent",true),"consent cannot be working");
            copy.Restore(null,null,false);
            Is(copy.ObservedCount==0 && !copy.HasWorkingHypothesis,"old save compatible");

            using var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(folder,"S03.json")));
            Is(doc.RootElement.GetProperty("scene_id").GetString()=="S03","authored scene");
            var steps=doc.RootElement.GetProperty("steps");
            var choices=steps[0].GetProperty("choices").EnumerateArray().ToList();
            string[] commands={"S03_LOOK_0441","S03_LOOK_0442","S03_LOOK_0443","S03_EMPTY"};
            for(int i=0;i<commands.Length;i++)
            {
                string command=commands[i];
                var item=choices.First(x=>x.GetProperty("choice_id").GetString()==command);
                Is(item.GetProperty("effects").EnumerateArray().Any(e=>
                    e.GetProperty("op").GetString()=="add_unique" &&
                    e.GetProperty("field").GetString()=="evidence_seen" &&
                    e.GetProperty("value").GetString()==InvestigationNotebook.Sources[i].Id),
                    "source mapping remains canonical "+command);
            }
            var sourceChoice=choices.First(x=>x.GetProperty("choice_id").GetString()=="S03_SOURCE");
            Is(sourceChoice.GetProperty("next").GetString()=="scene:S04","continue to verification");
            Is(sourceChoice.GetProperty("when").GetProperty("all").GetArrayLength()==4,"video gated by four authored sources");
            Console.WriteLine("PASS "+assertions+" P2 assertions: canonical observations, provisional reasoning, safety, atomic save, S03 mapping");
        }
        catch(Exception e)
        {
            Console.Error.WriteLine(e);
            Environment.ExitCode=1;
        }
    }
}
