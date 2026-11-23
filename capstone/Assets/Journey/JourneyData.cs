using System;
using System.Text;
namespace AmbrosJourney.Journey
{
    [Serializable] public sealed class SpringState
    {
        public bool explored,controlsSeen,lessonComplete,complete;
        public int missions,bossScore;
        public bool[] cleared=new bool[3],studied=new bool[3];
        public int[] trials=new int[3];
        public bool Valid(){if(cleared==null||cleared.Length!=3||studied==null||studied.Length!=3||trials==null||trials.Length!=3||missions<0||missions>3||bossScore<0||bossScore>5)return false;int count=0;for(int i=0;i<3;i++){if(trials[i]<0||trials[i]>2||cleared[i]!=(trials[i]==2))return false;if(cleared[i])count++;}return count==missions&&(!complete||(missions==3&&bossScore>=4));}
    }
    [Serializable] public sealed class ChapterState
    {
        public bool visited,inCave; public float x,y; public string progress="";
    }
    [Serializable] public sealed class JourneyData
    {
        public int version=1,chapter=1,unlocked=1;
        public string playerName="",completedOn="";
        public bool started,finished,outroSeen,assistedGuidance;
        public bool certificatePending,homePortalOpened;
        public bool[] artifacts=new bool[12];
        public ChapterState[] chapters={new ChapterState(),new ChapterState(),new ChapterState(),new ChapterState()};
        public int ArtifactCount {get{int n=0;foreach(bool found in artifacts)if(found)n++;return n;}}
        public bool Valid()
        {
            if(certificatePending&&!finished)return false;
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
}
