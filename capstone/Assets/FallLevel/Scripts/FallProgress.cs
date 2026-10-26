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
        public static readonly string[] Notes={
            "GIVE A GOAL + CONTEXT + FORMAT\nA prompt is a request to a tool. Say what you want, give relevant background, and describe a useful response. For example: Suggest three names for our fictional autumn fair. Use a numbered list. Clear requests help, but never guarantee a correct answer.",
            "MATCH YOUR AUDIENCE + SET LIMITS\nTell the tool who the explanation is for and what to include or leave out. Use made-up examples instead of private details. Ask for a hint when you need practice, and follow classroom rules. A role such as 'teacher' does not make an AI an expert.",
            "CHECK + REVISE\nCompare the response with your request. Did it follow the number, topic and format? Ask for a specific change instead of repeating a vague prompt. Clear wording does not prove facts: check important claims with reliable sources or a trusted adult."
        };
        public static readonly FallQuestion[][] Missions={
            new[]{
                new FallQuestion("Build a prompt for the orchard's fictional autumn fair. First choose a clear GOAL.","Do something amazing","Suggest names for our autumn fair","Tell me everything",1,"A goal names the task. You chose 'Suggest names for our autumn fair.' Next add useful background."),
                new FallQuestion("Goal: Suggest names for our autumn fair. Which CONTEXT helps without revealing private details?","My password and home address","Whatever you can find about me","It celebrates apples and welcomes middle-school visitors",2,"The theme and audience help the tool choose suitable ideas. Personal identifiers are unnecessary."),
                new FallQuestion("Now choose the FORMAT for the fair-name suggestions.","Give three short names in a numbered list","Write any amount in any style","Guarantee that everyone will like your answer",0,"Your complete prompt: Suggest three short names for a fictional autumn fair about apples for middle-school visitors. Use a numbered list. You still choose the final name.")
            },
            new[]{
                new FallQuestion("The archive needs an explanation of AI for new students. Which prompt gives the best audience and scope?","Explain AI","Explain AI to a 12-year-old in three short sentences, using a flower-sorting example","Pretend you know everything and never admit mistakes",1,"The audience, length and example give useful direction. Even a clear explanation should be checked."),
                new FallQuestion("A class exercise must show your own thinking. Choose a safe request for permitted help.","Write my whole answer so I can copy it","Use my friend's private messages to make an example","Give one hint about this made-up example, then let me try",2,"Use fictional context and a hint that leaves you something to practice. First check whether your teacher allows AI help.")
            },
            new[]{
                new FallQuestion("You requested THREE short fair names. The device returned TEN long paragraphs. Which follow-up fixes the mismatch?","Revise this to exactly three names, each at most four words, in a numbered list","Try harder","Your answer is automatically correct because my prompt was clear",0,"A useful revision identifies the exact change. Then inspect whether the new response follows your limits."),
                new FallQuestion("A revised response follows your format but claims a local plant is safe to eat. What now?","Eat it because the prompt was detailed","Check with a trusted adult and a reliable plant-safety source; do not eat it based on AI","Ask it to sound more certain",1,"Following a format is different from being factual or safe. Detailed prompts cannot guarantee correct advice.")
            }
        };
        public static readonly FallQuestion[] Boss={
            new FallQuestion("Which request gives a clear task, context and format?","Help!","Tell me everything about festivals","Suggest three short names for a fictional apple fair in a numbered list",2,"Name the goal, relevant background and response format."),
            new FallQuestion("You need a simple explanation for younger students. What should you add?","The audience and a useful length or example","Your account password","A demand that the tool never admit uncertainty",0,"Audience, length and examples can make a response more useful."),
            new FallQuestion("Your prompt asked for a hint, but the tool solved the whole exercise. What should you do?","Copy it without understanding","Ask for one small hint instead, then work through it yourself","Stop practicing forever",1,"Revise the request to preserve your own thinking and practice."),
            new FallQuestion("What context belongs in a story-brainstorming prompt?","Your full name, address and password","Private messages from a classmate","A fictional setting, audience and the kind of story you want",2,"Relevant fictional details help without exposing personal information."),
            new FallQuestion("Does a detailed prompt guarantee true facts?","Yes, if you say 'be accurate'","No. Check important claims with reliable sources","Yes, if the response sounds confident",1,"Clear prompts improve instructions, not certainty. Keep checking facts and making your own decisions.")
        };
    }
}
