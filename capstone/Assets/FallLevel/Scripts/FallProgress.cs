using System;

namespace AmbrosJourney.Fall
{
    // Serializable lesson state; missions survive mistakes, retries and chapter revisits.
    [Serializable]
    public sealed class FallProgress
    {
        public bool Introduced;
        public bool Valid()
        {
            if(Courage<1||Courage>3||BossQuestion<0||BossQuestion>5||BossScore<0||BossScore>BossQuestion||Studied==null||Studied.Length!=3||Steps==null||Steps.Length!=3)return false;
            for(int i=0;i<3;i++)if(Steps[i]<0||Steps[i]>FallContent.Missions[i].Length)return false;
            if(BossFinished!=(BossQuestion==5))return false;
            return !Complete||(BossFinished&&BossScore>=4&&RuneCount==3);
        }
        public bool[] Studied = new bool[3];
        public int[] Steps = new int[3];
        public int Courage = 3, BossQuestion, BossScore;
        public bool BossStarted, BossFinished, Complete;
        public bool MissionComplete(int i) => Steps[i] == FallContent.Missions[i].Length;
        public int RuneCount { get { int n=0; for(int i=0;i<3;i++) if(MissionComplete(i))n++; return n; } }
        public bool CanFight => Introduced && RuneCount == 3;
        public void LoseCourage() { if(--Courage<=0) Courage=3; }
        public bool ReadNote(int mission)
        { if(!Introduced || mission<0 || mission>=3)return false; Studied[mission]=true;return true; }
        public bool AnswerMission(int mission,int choice)
        {
            if(!Introduced || mission<0 || mission>=3 || !Studied[mission] || MissionComplete(mission) || choice<0 || choice>=3)return false;
            bool right=choice==FallContent.Missions[mission][Steps[mission]].Correct;
            if(right)Steps[mission]++;else LoseCourage();return right;
        }
        public bool BeginBoss()
        { if(!CanFight || Complete)return false;BossStarted=true;BossFinished=false;BossQuestion=BossScore=0;return true; }
        public bool AnswerBoss(int choice)
        {
            if(!CanFight || !BossStarted || BossFinished || Complete || choice<0 || choice>=3)return false;
            bool right=choice==FallContent.Boss[BossQuestion].Correct;
            if(right)BossScore++;else LoseCourage();
            if(++BossQuestion==FallContent.Boss.Length){BossFinished=true;Complete=BossScore>=4;}
            return right;
        }
    }
    public sealed class FallQuestion
    {
        public readonly string Prompt,Explanation;
        public readonly string[] Choices;
        public readonly int Correct;
        public FallQuestion(string prompt,string a,string b,string c,int correct,string explanation)
        {Prompt=prompt;Choices=new[]{a,b,c};Correct=correct;Explanation=explanation;}
    }
    public static class FallContent
    {
        public static readonly string[] Landmarks={"Harvest Orchard","Lantern Archive","Revision Kiln"};
        public static readonly string[] Notes = {
            "SAY WHAT YOU NEED\nA prompt is the request you give an AI tool. Say what you want, add the background it needs, and say how you'd like the answer set out. Leave out personal details that don't help with the task. Clear instructions help, but you still need to check the answer.",
            "THINK ABOUT WHO IT IS FOR\nAn explanation for a new student should sound different from one for a specialist. Say who it is for and set useful limits, like the length or kind of help you want. Calling an AI an expert doesn't make its advice reliable.",
            "READ IT, THEN MAKE A CHANGE\nCompare the reply with what you asked for. If it misses something, say what needs changing. A reply can follow every instruction and still get a fact wrong. Check important claims before using them."
        };
        public static readonly FallQuestion[][] Missions = {
            new[]{
                new FallQuestion("The orchard needs a name for its autumn fair. Which opening gives the tool a clear job?","Help with our fair","Suggest names for our autumn fair","Tell me about autumn fairs",1,"A clear job tells the tool what to make. 'Help with our fair' leaves it guessing."),
                new FallQuestion("What background would help it suggest names for this fair?","The organizer's home address","A list of the visitors' names","It's an apple fair for middle-school students",2,"The theme and audience matter here. The visitors' names and an address don't help name the fair."),
                new FallQuestion("You want a few names you can compare quickly. What should you add?","Give three short names in a numbered list","Include everything that might be useful","Make the answer detailed and creative",0,"The number, length and layout make the reply easier to compare. You still pick the name.")
            },
            new[]{
                new FallQuestion("New students need a short introduction to AI. Which request gives enough direction?","Give a detailed overview of AI","Explain AI to a 12-year-old in three sentences with one example","Explain AI clearly and include all the main ideas",1,"An audience, length and example help narrow the request. You should still read it for mistakes."),
                new FallQuestion("Your teacher allows hints, but the work must be yours. Which request fits?","Show the full method so I can follow it","Rewrite my friend's answer in different words","Give me one clue for this made-up example, then let me try",2,"One clue leaves you a step to work out. Check the rules before asking for more help.")
            },
            new[]{
                new FallQuestion("You asked for three short fair names and got ten paragraphs. Which follow-up is most useful?","Cut this to three names of four words or fewer, in a list","Make this shorter and better","Try the original request again",0,"A specific change is easier to follow than 'better.' Check whether the next reply actually fits."),
                new FallQuestion("The reply now fits your format, but it calls a wild plant edible. What should you do?","Use it because the tool followed the other instructions","Check with a trusted adult and a reliable plant guide before acting","Add 'be accurate' and use the next reply",1,"Following your format doesn't prove a plant is safe. Don't eat it based on a chatbot answer.")
            }
        };
        public static readonly FallQuestion[] Boss = {
            new FallQuestion("A club wants short slogans for a recycling poster. Which request will be easiest to use?","Write something catchy about recycling","Explain recycling and suggest some poster ideas","Give four recycling slogans for a school poster, each under six words",2,"This gives a task, audience and limit. The club still needs to choose what works."),
            new FallQuestion("Your reply uses words a younger class won't know. What is the most useful change?","Name the age group and ask for familiar words with an example","Ask it to explain the topic in more detail","Ask it to sound like a teacher",0,"Tell it what the readers need. A teacher-like tone doesn't guarantee a suitable explanation."),
            new FallQuestion("You asked for a hint, but got the full solution. What should your next request say?","Explain that solution so I can rewrite it","Give only the first clue, then wait for my attempt","Give the same solution with fewer steps",1,"Set a limit that leaves you something to practice. Don't just make a full answer easier to copy."),
            new FallQuestion("Which background would help a tool brainstorm a mystery story?","The real names and schedules of your classmates","Private messages showing how your friends talk","A made-up setting, the reader's age and the kind of mystery",2,"Give details about the story you want. Real people's private information isn't needed."),
            new FallQuestion("A careful prompt gets a clear reply with a few facts. What still needs checking?","Only the spelling, because you gave clear instructions","Whether the facts are supported by reliable sources","Only the facts the tool says it is unsure about",1,"Clear writing and good instructions don't prove the facts. Check important claims even when the tool sounds sure.")
        };
    }
}
