using System;
using System.IO;
using System.Text;
namespace AmbrosJourney.Journey
{
    [Serializable] public sealed class SpringState
    {
        public bool explored,lessonComplete,complete;
        public int missions,bossScore,hearts=3;
        public bool[] cleared=new bool[3],studied=new bool[3];
        public int[] trials=new int[3];
        public bool Valid(){if(cleared==null||cleared.Length!=3||studied==null||studied.Length!=3||trials==null||trials.Length!=3||hearts<1||hearts>3||missions<0||missions>3||bossScore<0||bossScore>5)return false;int count=0;for(int i=0;i<3;i++){if(trials[i]<0||trials[i]>2||cleared[i]!=(trials[i]==2))return false;if(cleared[i])count++;}return count==missions&&(!complete||(missions==3&&bossScore>=4));}
    }
    [Serializable] public sealed class ChapterSave
    {
        public bool visited,inCave; public float x,y; public string progress="";
    }
    [Serializable] public sealed class JourneyData
    {
        public int version=1,chapter=1,unlocked=1;
        public string playerName="",completedOn="";
        public bool started,finished,outroSeen,assistedGuidance;
        public bool[] artifacts=new bool[12];
        public ChapterSave[] chapters={new ChapterSave(),new ChapterSave(),new ChapterSave(),new ChapterSave()};
        public int ArtifactCount {get{int n=0;foreach(bool found in artifacts)if(found)n++;return n;}}
        public bool Valid()
        {
            if(version!=1||chapter<1||chapter>4||unlocked<chapter||unlocked>4||playerName==null||playerName.Length>32||artifacts==null||artifacts.Length!=12||chapters==null||chapters.Length!=4)return false;
            foreach(var c in chapters)if(c==null||float.IsNaN(c.x)||float.IsNaN(c.y)||float.IsInfinity(c.x)||float.IsInfinity(c.y)||Math.Abs(c.x)>200||Math.Abs(c.y)>100||c.progress==null||c.progress.Length>16384)return false;
            return true;
        }
        public static string CleanName(string value)
        {
            var b=new StringBuilder();foreach(char c in (value??"").Trim())if(!char.IsControl(c)&&c!='<'&&c!='>'&&b.Length<32)b.Append(c);
            return b.ToString().Trim();
        }
    }
    // Write a complete replacement before touching the last known-good save. Keep a recovery copy.
    public static class JourneyStorage
    {
        public static void Write(string path,string json)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string pending=path+".tmp";File.WriteAllText(pending,json,new UTF8Encoding(false));
            if(File.Exists(path))File.Replace(pending,path,path+".bak");else File.Move(pending,path);
        }
        public static T Read<T>(string path,Func<string,T> parse,Func<T,bool> valid) where T:class
        {
            foreach(var candidate in new[]{path,path+".bak"})
                try{if(!File.Exists(candidate)||new FileInfo(candidate).Length>131072)continue;var data=parse(File.ReadAllText(candidate));if(data!=null&&valid(data))return data;}catch(Exception e)when(e is IOException||e is ArgumentException||e is UnauthorizedAccessException){ }
            return null;
        }
        public static void Reset(string path){foreach(string suffix in new[]{"",".bak",".tmp"})if(File.Exists(path+suffix))File.Delete(path+suffix);}
    }
}
