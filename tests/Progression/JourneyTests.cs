using System;
using System.Text.Json;
using AmbrosJourney.Journey;
using AmbrosJourney.Summer;
static class JourneyTests
{
    static void Check(bool value,string message){if(!value)throw new Exception("JOURNEY: "+message);}
    public static void Run()
    {
        Check(JourneyData.CleanName(" <Ada>\n ")=="Ada","Names remove markup and controls");
        Check(JourneyData.CleanName(new string('x',60)).Length==32,"Names bounded");
        var data=new JourneyData();
        Check(data.Valid()&&!data.started&&!data.finished&&data.chapter==1&&data.unlocked==1&&data.ArtifactCount==0,"A new run starts at Spring without progress");
        var options=new JsonSerializerOptions{IncludeFields=true};
        var summer=new SummerProgress{Introduced=true,GuideArrived=true,WorkshopQuestion=1};
        data.playerName="Zoë";data.started=true;data.chapter=2;data.unlocked=2;data.artifacts[3]=true;
        data.chapters[1]=new ChapterState{visited=true,x=-18.25f,y=12.5f,progress=JsonSerializer.Serialize(summer,options)};
        var retained=JsonSerializer.Deserialize<SummerProgress>(data.chapters[1].progress,options);
        Check(retained!=null&&retained.Valid()&&retained.WorkshopQuestion==1&&!retained.Wards[0],"Current-run chapter data retains partial lessons without granting a ward");
        data.chapter=1;data.chapter=2;
        Check(data.chapters[1].visited&&data.chapters[1].x==-18.25f&&data.chapters[1].y==12.5f&&data.ArtifactCount==1,"Season revisits retain this run's location and artifacts");
        data.finished=true;data.certificatePending=false;data.completedOn="2026-11-22";
        var fresh=new JourneyData();
        Check(fresh.Valid()&&fresh.playerName==""&&!fresh.started&&!fresh.finished&&!fresh.outroSeen&&!fresh.certificatePending&&!fresh.homePortalOpened&&fresh.chapter==1&&fresh.unlocked==1&&fresh.ArtifactCount==0&&fresh.completedOn=="","The next run does not inherit completion, identity or unlocks");
        Check(!ReferenceEquals(data.artifacts,fresh.artifacts)&&!ReferenceEquals(data.chapters,fresh.chapters),"Runs own independent progress arrays");
        for(int i=0;i<fresh.chapters.Length;i++)
        {
            Check(!ReferenceEquals(data.chapters[i],fresh.chapters[i]),"Runs own independent chapter objects");
            Check(!fresh.chapters[i].visited&&!fresh.chapters[i].inCave&&fresh.chapters[i].progress=="","Fresh chapters have no retained position or lesson state");
            for(int j=0;j<i;j++)Check(!ReferenceEquals(fresh.chapters[i],fresh.chapters[j]),"Chapters do not share mutable state");
        }
        fresh.playerName="Ada";fresh.artifacts[0]=true;fresh.chapters[1].x=7;
        Check(data.playerName=="Zoë"&&data.completedOn=="2026-11-22"&&data.finished&&data.chapters[1].x==-18.25f&&!data.artifacts[0],"Creating a new run cannot rename or mutate the previous completion data");
        data.chapters[1].x=float.NaN;Check(!data.Valid(),"Reject nonfinite chapter position");
        data.chapters[1].x=-18.25f;data.artifacts=new bool[2];Check(!data.Valid(),"Reject truncated collectible array");
        Console.WriteLine("PASS: independent session data, fresh-run defaults, in-memory chapter retention, certificate identity, bounds and names.");
    }
}
