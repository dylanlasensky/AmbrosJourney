using System;
namespace AmbrosJourney.Winter
{
    [Serializable]
    public sealed class WinterProgress
    {
        public bool Valid()
        {
            if(Courage<1||Courage>3||BossQuestion<0||BossQuestion>8||BossScore<0||BossScore>BossQuestion||Studied==null||Studied.Length!=3||Steps==null||Steps.Length!=3)return false;
            for(int i=0;i<3;i++)if(Steps[i]<0||Steps[i]>WinterContent.Missions[i].Length)return false;
            if(BossFinished!=(BossQuestion==8))return false;
            if(WinterBossQuestion<0||WinterBossQuestion>5||WinterBossScore<0||WinterBossScore>WinterBossQuestion||WinterBossFinished!=(WinterBossQuestion==5)||WinterComplete&&(!WinterBossFinished||WinterBossScore<4)||InCave&&!WinterComplete)return false;
            return !Complete||(BossFinished&&BossScore>=7&&RuneCount==3);
        }
        public bool WinterBossStarted,WinterBossFinished,WinterComplete;
        public int WinterBossQuestion,WinterBossScore;
        public bool Introduced, CaveDiscovered, InCave, BossStarted, BossFinished, Complete;
        public bool[] Studied = new bool[3];
        public int[] Steps = new int[3];
        public int Courage=3, BossQuestion, BossScore;
        public bool MissionComplete(int i)=>Steps[i]==WinterContent.Missions[i].Length;
        public int RuneCount {get {int n=0;for(int i=0;i<3;i++)if(MissionComplete(i))n++;return n;}}
        public bool CanFightWinter=>Introduced&&MissionComplete(0)&&MissionComplete(1)&&!InCave;
        public bool CanFight=>WinterComplete&&Introduced&&CaveDiscovered&&InCave&&RuneCount==3;
        public void LoseCourage(){if(--Courage<=0)Courage=3;}
        public bool EnterCave(){if(!WinterComplete)return false;CaveDiscovered=InCave=true;return true;}
        public void LeaveCave(){InCave=false;}
        public bool ReadNote(int m){if(!Introduced||m<0||m>2||(m==2&&!InCave))return false;Studied[m]=true;return true;}
        public bool AnswerMission(int m,int c)
        {
            if(m<0||m>2||c<0||c>2||!Introduced||!Studied[m]||MissionComplete(m)||(m==2&&!InCave))return false;
            bool right=c==WinterContent.Missions[m][Steps[m]].Correct;if(right)Steps[m]++;else LoseCourage();return right;
        }
        public bool BeginWinterBoss(){if(!CanFightWinter||WinterComplete)return false;WinterBossStarted=true;WinterBossFinished=false;WinterBossQuestion=WinterBossScore=0;return true;}
        public bool AnswerWinterBoss(int c)
        {
            if(!CanFightWinter||!WinterBossStarted||WinterBossFinished||WinterComplete||c<0||c>2)return false;
            bool right=c==WinterContent.Boss[WinterBossQuestion].Correct;if(right)WinterBossScore++;else LoseCourage();
            if(++WinterBossQuestion==WinterContent.Boss.Length){WinterBossFinished=true;WinterComplete=WinterBossScore>=4;}return right;
        }
        public bool BeginBoss(){if(!CanFight||Complete)return false;BossStarted=true;BossFinished=false;BossQuestion=BossScore=0;return true;}
        public bool AnswerBoss(int c)
        {
            if(BossQuestion>=WinterContent.Final.Length||WinterContent.Final[BossQuestion].Kind!=FinalActivityKind.Choose)return false;
            return AnswerFinal(new[]{c});
        }
        public bool AnswerFinal(int[] response)
        {
            if(!CanFight||!BossStarted||BossFinished||Complete)return false;
            bool right;if(!WinterContent.Final[BossQuestion].TryEvaluate(response,out right))return false;
            if(right)BossScore++;else LoseCourage();
            if(++BossQuestion==WinterContent.Final.Length){BossFinished=true;Complete=BossScore>=7;}
            return right;
        }
    }
    public sealed class WinterQuestion
    {
        public readonly string Prompt,Explanation;public readonly string[] Choices;public readonly int Correct;
        public WinterQuestion(string prompt,string a,string b,string c,int correct,string explanation){Prompt=prompt;Choices=new[]{a,b,c};Correct=correct;Explanation=explanation;}
    }
    public static class WinterContent
    {
        public static readonly string[] Landmarks={"Wayfarer's Ledger","Frozen Observatory","Echo Archive"};
        public static readonly string[] Notes = {
            "OPEN THE SOURCE\nA chatbot can give you a title, quote or link that looks real but isn't. Open the source and see whether it says what the reply claims. Look at who wrote it, when it was updated and what it was written about.",
            "CHECK WHERE THE EVIDENCE CAME FROM\nSeveral posts can repeat one wrong story. Look for a reliable source that checked the evidence for itself. Several AI tools can repeat the same mistake too. When sources disagree, check their dates and what each one actually measured.",
            "BE CLEAR ABOUT WHAT YOU KNOW\nIf you can't check a claim, say what is still uncertain. If you shared a mistake, correct it and show what changed your mind. For decisions about health or safety, ask someone qualified to help with the real situation."
        };
        public static readonly WinterQuestion[][] Missions = {
            new[]{
                new WinterQuestion("AI says the cave was mapped in 1840 and names a book. What would actually help you check it?","Ask the same tool for a longer explanation","Find the book and read the part about the cave","Find a second chatbot that gives the same date",1,"Check that the book exists and supports this date. A longer reply or another matching answer isn't the original evidence."),
                new WinterQuestion("A dated ledger says 1890, but AI says 1840. Before correcting the date, what should you check?","That the ledger is about this cave and what its date refers to","Which answer appears on more websites","Which date the tool gives when you ask again",0,"Even an original source needs context. Make sure it refers to this cave and to the same event.")
            },
            new[]{
                new WinterQuestion("Three posts describe the same storm using identical sentences. What can you conclude?","The story has been checked three times","Different websites make the reports independent","They may all come from one report; find separate evidence",2,"Different addresses don't mean different evidence. Trace the reports back to where the information came from."),
                new WinterQuestion("An old trail guide says a path is open. Today's ranger notice says it is closed. Which detail matters most?","The guide explains the route in more detail","The ranger notice is current and covers this exact trail","The guide has been shared more often",1,"For current trail conditions, the date and local responsibility matter more than length or popularity.")
            },
            new[]{
                new WinterQuestion("An echo describes a shortcut, but you can't find a record of it. What should you tell the party?","It probably exists because the directions are detailed","It isn't on the map, so it cannot exist","We haven't checked this route, so we shouldn't rely on it yet",2,"You don't have enough evidence to confirm or rule it out. Say what is missing before anyone takes the risk."),
                new WinterQuestion("You shared the wrong cave date. A reliable record now shows the error. What should you send?","A correction with the record and what it shows","The new date without mentioning the earlier message","A note saying dates can be hard to check",0,"Make the correction easy to see. Show why you changed the answer so others can update it too.")
            }
        };
        public static readonly WinterQuestion[] Boss = {
            new WinterQuestion("The Warden quotes a researcher. You find the quote in a blog. What is the next useful check?","Check whether the blog has lots of readers","Find the researcher's original work and read the quote in context","Ask AI whether the quote sounds like that researcher",1,"The blog may have shortened or misquoted it. Check the original work and its context."),
            new WinterQuestion("Two sites cite the same anonymous post. What would add stronger evidence?","A third site that links to that post","A summary from another AI tool","A reliable report based on its own observations",2,"More copies don't add a new check. Look for evidence gathered separately."),
            new WinterQuestion("A link in an AI reply opens a real page, but it doesn't mention the claimed discovery. What does that tell you?","This page doesn't support that claim","The claim is probably elsewhere on the same site","The source supports it because the link works",0,"A working link isn't enough. The source needs to support the specific claim."),
            new WinterQuestion("You have to present a claim today, but haven't confirmed it. What is the fairest wording?","Research proves this, though the details aren't available yet","This is not confirmed; here is what we still need to check","Several people have mentioned this, so it seems settled",1,"Say what you have checked and what you haven't. Don't make the evidence sound stronger than it is."),
            new WinterQuestion("A route app using AI says the ice crossing is safe. Yesterday's photo looks fine. What should you do?","Use the photo to judge whether the ice is thick enough","Compare several app answers before crossing","Stay back and check current advice from the local ranger",2,"Conditions change. A picture or app answer can't inspect the ice now. Ask the person responsible for local safety.")
        };
        public static readonly FinalChallenge[] Final=FinalChallenges.All;
    }
}
