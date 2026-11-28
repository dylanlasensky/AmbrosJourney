// Defines Winter lesson questions, prerequisites, scores and retry state.
using System;
namespace AmbrosJourney.Winter
{
    [Serializable]
    public sealed class WinterProgress
    {
        // Keep grading and prerequisites separate from rendering so the same rules can be tested without Unity.
        public bool Valid()
        {
            if(BossQuestion<0||BossQuestion>8||BossScore<0||BossScore>BossQuestion||Studied==null||Studied.Length!=3||Steps==null||Steps.Length!=3)return false;
            for(int i=0;i<3;i++)if(Steps[i]<0||Steps[i]>WinterContent.Missions[i].Length)return false;
            if(BossFinished!=(BossQuestion==8))return false;
            if(WinterBossQuestion<0||WinterBossQuestion>5||WinterBossScore<0||WinterBossScore>WinterBossQuestion||WinterBossFinished!=(WinterBossQuestion==5)||WinterComplete&&(!WinterBossFinished||WinterBossScore<4)||InCave&&!WinterComplete)return false;
            return !Complete||(BossFinished&&BossScore>=7&&RuneCount==3);
        }
        public bool WinterBossStarted,WinterBossFinished,WinterComplete;
        public int WinterBossQuestion,WinterBossScore;
        public bool Introduced, CaveDiscovered, InCave, BossStarted, BossFinished, Complete;
        public bool CompassWarningShown, CaveGuideRead;
        public bool CaveEntryExplained=>CaveGuideRead||CaveDiscovered;
        public bool CanEnterCave=>WinterComplete&&CaveEntryExplained;
        public bool ReadCaveGuide(){if(!WinterComplete||InCave)return false;CaveGuideRead=true;return true;}
        public bool[] Studied = new bool[3];
        public int[] Steps = new int[3];
        public int BossQuestion, BossScore;
        public bool MissionComplete(int i)=>Steps[i]==WinterContent.Missions[i].Length;
        public int RuneCount {get {int n=0;for(int i=0;i<3;i++)if(MissionComplete(i))n++;return n;}}
        public bool CanFightWinter=>Introduced&&MissionComplete(0)&&MissionComplete(1)&&!InCave;
        public bool CanFight=>WinterComplete&&Introduced&&CaveDiscovered&&InCave&&RuneCount==3;

        public bool EnterCave(){if(!CanEnterCave)return false;CaveDiscovered=InCave=true;return true;}
        public void LeaveCave(){InCave=false;}
        public bool ReadNote(int m){if(!Introduced||m<0||m>2||(m==2&&!InCave))return false;Studied[m]=true;return true;}
        public bool AnswerMission(int m,int c)
        {
            if(m<0||m>2||c<0||c>2||!Introduced||!Studied[m]||MissionComplete(m)||(m==2&&!InCave))return false;
            bool right=c==WinterContent.Missions[m][Steps[m]].Correct;if(right)Steps[m]++;return right;
        }
        public bool BeginWinterBoss(){if(!CanFightWinter||WinterComplete)return false;WinterBossStarted=true;WinterBossFinished=false;WinterBossQuestion=WinterBossScore=0;return true;}
        public bool AnswerWinterBoss(int c)
        {
            if(!CanFightWinter||!WinterBossStarted||WinterBossFinished||WinterComplete||c<0||c>2)return false;
            bool right=c==WinterContent.Boss[WinterBossQuestion].Correct;if(right)WinterBossScore++;
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
            if(right)BossScore++;
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
                new WinterQuestion("AI says this cave was first mapped in 1840. It says the date came from a history book. How can you check whether AI used the book correctly?","Ask AI to explain its answer in more detail","Find the book and read what it says about when the cave was first mapped","Ask another AI tool whether it gives the same date",1,"Check that the book exists and supports this date. A longer reply or another matching answer isn't the original evidence."),
                new WinterQuestion("A history book says this cave was first mapped in 1890. AI says it was first mapped in 1840. What should you check first?","Whether the book is talking about the same cave and when it was first mapped","Which date appears on more websites","Whether AI gives the same date when you ask again",0,"Even an original source needs context. Make sure it refers to this cave and to the same event.")
            },
            new[]{
                new WinterQuestion("AI shows you three online posts about a storm. You notice that all three copied the same news story. What should you do next?","Count the posts as three separate checks of what happened","Trust the story because three people shared it","Find another source that checked what happened instead of copying that story",2,"Three copies of one story are not three separate checks. Look for another source that checked what happened instead of copying it."),
                new WinterQuestion("AI uses an old guide to say a trail is open. Today's ranger notice says it is closed. Which detail matters most?","The guide explains the route in more detail","The ranger notice is current and covers this exact trail","The guide has been shared more often",1,"For current trail conditions, the date and local responsibility matter more than length or popularity.")
            },
            new[]{
                new WinterQuestion("AI suggests a shortcut, but you cannot check that it exists. What should you tell your friends?","It probably exists because the directions are detailed","It isn't on the map, so it cannot exist","We haven't checked this route, so we shouldn't rely on it yet",2,"You don't have enough evidence to confirm or rule it out. Say what is missing before anyone takes the risk."),
                new WinterQuestion("You shared a wrong date from an AI answer. A reliable source shows the correct date. What should you send?","Explain that your earlier date was wrong, give the correct date, and show the source you checked","Send the correct date without explaining why you changed it","Tell everyone that dates can be hard to check, without correcting your earlier message",0,"Make the correction easy to see. Show why you changed the answer so others can update it too.")
            }
        };
        public static readonly WinterQuestion[] Boss = {
            new WinterQuestion("AI gives you a sentence and says a scientist said it. You find the same sentence on a blog. How can you check whether the scientist really said those words?","Check whether the blog has lots of readers","Find the scientist’s original article or interview and check the words there","Ask AI whether the sentence sounds like something that scientist would say",1,"The blog or AI may have changed the words. Find the original article, interview or recording and check what the scientist actually said."),
            new WinterQuestion("AI gives you two websites to support an answer, but both copied the same person’s post. What would help you check whether the answer is true?","Another website that links to the same post","Another AI tool’s summary of those websites","A report from a trusted source that checked the facts itself",2,"More copies don't add a new check. Look for evidence gathered separately."),
            new WinterQuestion("AI says scientists discovered a new planet and gives you a link. The page opens, but says nothing about that planet. What does this mean?","The page does not show that AI’s statement is true","The discovery must be described somewhere else on the website","The discovery is confirmed because the link works",0,"A working link isn't enough. The source needs to support the specific claim."),
            new WinterQuestion("An AI says a planet has 20 moons. You have not checked a reliable source yet. What should you tell your class?","Scientists proved it has 20 moons","The AI said 20, but I still need to check a reliable source","It must be right because the AI sounded sure",1,"Say what you have checked and what you haven't. Don't make the evidence sound stronger than it is."),
            new WinterQuestion("A route app using AI says the ice crossing is safe. Yesterday's photo looks fine. What should you do?","Use the photo to judge whether the ice is thick enough","Compare several app answers before crossing","Stay back and check current advice from the local ranger",2,"Conditions change. A picture or app answer can't inspect the ice now. Ask the person responsible for local safety.")
        };
        public static readonly FinalChallenge[] Final=FinalChallenges.All;
    }
}
