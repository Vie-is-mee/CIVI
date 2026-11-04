using System;
using System.Collections.Generic;

namespace Civi.Preview
{
    // The reviewed script is compiled separately. Gameplay never needs the source Markdown.
    [Serializable] public sealed class LiuDialoguePage
    {
        public string id,speaker,kind,text,sceneTitle;
        public int scene,sourceLine;
    }
    [Serializable] public sealed class LiuDialogueSegment
    {
        public string taskId;
        public int beforeStart,beforeCount,afterStart,afterCount;
    }
    [Serializable] public sealed class LiuDialogueContent
    {
        public string schema,sourceFile,sourceSha256,pagesSha256;
        public int sceneCount,cueCount;
        public LiuDialoguePage[] pages;
        public LiuDialogueSegment[] segments;

        public static void Validate(LiuDialogueContent d,LiuContent tasks)
        {
            if(d==null||d.schema!="civi-liuquang-dialogue-1"||
               d.cueCount!=148||d.sceneCount!=12||d.pages==null||
               d.pages.Length!=154||d.segments==null||d.segments.Length!=13)
                throw new FormatException("Reviewed Chapter 1 dialogue schema or count changed");

            var ids=new HashSet<string>();
            int cursor=0, scenes=0;
            for(int i=0;i<d.pages.Length;i++)
            {
                var page=d.pages[i];
                if(page==null||String.IsNullOrWhiteSpace(page.id)||
                   String.IsNullOrWhiteSpace(page.text)||
                   String.IsNullOrWhiteSpace(page.speaker)||
                   String.IsNullOrWhiteSpace(page.sceneTitle)||
                   (page.kind!="speech"&&page.kind!="direction")||
                   page.scene<1||page.scene>12||page.sourceLine<1||
                   !ids.Add(page.id))
                    throw new FormatException("Invalid authored dialogue page "+i);
                scenes|=1<<(page.scene-1);
                if(i>0&&page.scene<d.pages[i-1].scene)
                    throw new FormatException("Dialogue scenes out of order");
            }
            if(scenes!=4095)throw new FormatException("Missing one or more dialogue scenes");
            for(int i=0;i<d.segments.Length;i++)
            {
                var s=d.segments[i];
                if(s==null||s.taskId!=tasks.tasks[i].id||
                   s.beforeStart!=cursor||s.beforeCount<0||s.afterCount<1||
                   s.afterStart!=s.beforeStart+s.beforeCount||
                   s.afterStart+s.afterCount>d.pages.Length)
                    throw new FormatException("Invalid dialogue task segment "+i);
                cursor=s.afterStart+s.afterCount;
            }
            if(cursor!=d.pages.Length)throw new FormatException("Dialogue coverage incomplete");
            if(d.segments[3].afterStart>47||d.segments[3].beforeStart+d.segments[3].beforeCount>47||
               d.segments[8].afterStart>119||d.segments[8].beforeStart+d.segments[8].beforeCount>119||
               d.segments[11].afterStart!=150)
                throw new FormatException("Canonical dialogue action gate mismatch");
        }

        public int Count(int step,int mode)
        {
            var s=segments[step];
            return mode==3?s.beforeCount:mode==2?s.afterCount:0;
        }
        public LiuDialoguePage At(int step,int mode,int line)
        {
            if(step<0||step>=segments.Length)throw new ArgumentOutOfRangeException(nameof(step));
            var s=segments[step];
            int n=Count(step,mode);
            if(line<0||line>=n)throw new ArgumentOutOfRangeException(nameof(line));
            return pages[(mode==3?s.beforeStart:s.afterStart)+line];
        }
        public int ReadCount(int step,int mode,int line)
        {
            int count=0;
            for(int i=0;i<step;i++)count+=segments[i].beforeCount+segments[i].afterCount;
            if(step>=segments.Length)return pages.Length;
            if(mode==0)return count;
            count+=segments[step].beforeCount;
            if(mode==3)return count-segments[step].beforeCount+line;
            if(mode==1)return count;
            if(mode==2)return count+line;
            return count;
        }
    }
}
