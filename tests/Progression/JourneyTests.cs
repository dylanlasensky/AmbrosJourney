using System;
using System.IO;
using System.Text.Json;
using AmbrosJourney.Journey;
static class JourneyTests
{
    static void Check(bool value,string message){if(!value)throw new Exception("JOURNEY: "+message);}
    public static void Run()
    {
        Check(JourneyData.CleanName(" <Ada>\n ")=="Ada","Names remove markup and controls");Check(JourneyData.CleanName(new string('x',60)).Length==32,"Names bounded");
        var data=new JourneyData{playerName="Zoë",started=true,chapter=4,unlocked=4,finished=true,certificatePending=true,completedOn="2026-10-30"};data.chapters[3]=new ChapterSave{visited=true,inCave=true,x=108.25f,y=12.5f,progress="{\"Complete\":true}"};data.artifacts[11]=true;
        var options=new JsonSerializerOptions{IncludeFields=true};Func<string,JourneyData> parse=s=>{try{return JsonSerializer.Deserialize<JourneyData>(s,options);}catch(JsonException){return null;}};
        string directory=Path.Combine(Path.GetTempPath(),"ambros-tests-"+Guid.NewGuid().ToString("N"));string path=Path.Combine(directory,"save.json");
        try
        {
            JourneyStorage.Write(path,JsonSerializer.Serialize(data,options));var loaded=JourneyStorage.Read(path,parse,d=>d.Valid());Check(loaded!=null&&loaded.playerName=="Zoë"&&loaded.chapters[3].inCave&&loaded.chapters[3].x==108.25f&&loaded.ArtifactCount==1&&loaded.certificatePending&&loaded.completedOn=="2026-10-30","Round trip retains name, completion date, pending certificate, region, position and artifacts");
            data.artifacts[0]=true;JourneyStorage.Write(path,JsonSerializer.Serialize(data,options));File.WriteAllText(path,"{truncated");loaded=JourneyStorage.Read(path,parse,d=>d.Valid());Check(loaded!=null&&loaded.ArtifactCount==1,"Corrupt primary recovers previous valid save");
            JourneyStorage.Reset(path);Check(!File.Exists(path)&&!File.Exists(path+".bak"),"Reset removes both primary and recovery save");Check(JourneyStorage.Read(path,parse,d=>d.Valid())==null,"Reset cannot recover old progress");
            data.chapters[3].x=float.NaN;Check(!data.Valid(),"Reject nonfinite position");data.chapters[3].x=120;data.artifacts=new bool[2];Check(!data.Valid(),"Reject truncated collectible array");
        }
        finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        Console.WriteLine("PASS: save round trip, recovery, full reset, bounds and names.");
    }
}
