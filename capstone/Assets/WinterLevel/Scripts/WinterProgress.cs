using System;
namespace AmbrosJourney.Winter
{
    [Serializable]
    public sealed class WinterProgress
    {
        public bool Introduced, CaveDiscovered, InCave, BossStarted, BossFinished, Complete;
        public bool[] Studied = new bool[3];
        public int[] Steps = new int[3];
        public int Courage=3, BossQuestion, BossScore;
        public bool MissionComplete(int i)=>Steps[i]==WinterContent.Missions[i].Length;
        public int RuneCount {get {int n=0;for(int i=0;i<3;i++)if(MissionComplete(i))n++;return n;}}
        public bool CanFight=>Introduced&&CaveDiscovered&&InCave&&RuneCount==3;
        public void LoseCourage(){if(--Courage<=0)Courage=3;}
        public bool EnterCave(){if(!Introduced)return false;CaveDiscovered=InCave=true;return true;}
        public void LeaveCave(){InCave=false;}
        public bool ReadNote(int m){if(!Introduced||m<0||m>2||(m==2&&!InCave))return false;Studied[m]=true;return true;}
        public bool AnswerMission(int m,int c)
        {
            if(m<0||m>2||c<0||c>2||!Introduced||!Studied[m]||MissionComplete(m)||(m==2&&!InCave))return false;
            bool right=c==WinterContent.Missions[m][Steps[m]].Correct;if(right)Steps[m]++;else LoseCourage();return right;
        }
        public bool BeginBoss(){if(!CanFight||Complete)return false;BossStarted=true;BossFinished=false;BossQuestion=BossScore=0;return true;}
        public bool AnswerBoss(int c)
        {
            if(!CanFight||!BossStarted||BossFinished||Complete||c<0||c>2)return false;
            bool right=c==WinterContent.Boss[BossQuestion].Correct;if(right)BossScore++;else LoseCourage();
            if(++BossQuestion==5){BossFinished=true;Complete=BossScore>=4;}return right;
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
        public static readonly string[] Notes={
            "FIND THE ORIGINAL SOURCE\nAn AI can invent a convincing title, quote or link. Open the original source. Check who wrote it, when it was updated and whether it actually supports the claim. A confident tone is not evidence.",
            "COMPARE INDEPENDENT EVIDENCE\nLook for a reliable second source that gathered its own evidence. Two pages copying the same claim are not two independent checks. When evidence disagrees, pause, check dates and ask a trusted adult or teacher.",
            "ADMIT UNCERTAINTY + CORRECT MISTAKES\nIf a claim cannot be verified, say so. Do not invent evidence to fill a gap. Correct errors before sharing. For important real-world decisions, ask an appropriate qualified person instead of relying on an AI answer."
        };
        public static readonly WinterQuestion[][] Missions={
            new[]{new WinterQuestion("A device says Frostveil's cave was mapped in 1840, and gives a book title. What should you do first?","Trust it because it gives a date","Open the actual book or archive record and check the claim","Copy the title without looking",1,"A citation is a lead, not proof. Verify that the source exists and says what the tool claims."),new WinterQuestion("The real ledger says the cave was mapped in 1890. The device still insists on 1840. Which evidence should guide your correction?","The original ledger, with its date and context checked","The louder answer","Whichever date looks nicer",0,"Check the original evidence and correct the claim. Repeating an AI answer does not make it true.")},
            new[]{new WinterQuestion("Three travel posts repeat the same weather story word for word. How many independent checks have you found?","Three, because they have different web addresses","Enough to prove the story","You cannot count copies as independent evidence",2,"Copied reports can repeat one mistake. Find a source that checked the evidence independently."),new WinterQuestion("An old sign says a trail is open, but today's ranger notice says it is closed. What should you do?","Ignore the newer notice","Check the current official notice and ask the ranger","Let an AI guess whether it is safe",1,"Dates and context matter. Follow current local guidance and ask the responsible person.")},
            new[]{new WinterQuestion("The cave's echo claims a hidden tunnel leads home. You cannot find any record of it. What should you report?","The tunnel definitely exists","Invent a source so the claim sounds better","The claim is unverified; we need more evidence",2,"Honest uncertainty is useful. A missing source is a reason to investigate, not to invent proof."),new WinterQuestion("You shared an incorrect cave date with the party. What now?","Correct it and show the evidence you checked","Hide the mistake","Ask the device to sound more certain",0,"Updating an answer when evidence changes is a strength. Explain the correction clearly.")}
        };
        public static readonly WinterQuestion[] Boss={
            new WinterQuestion("The Frostbound Warden gives an impressive quote with no source. What is your next step?","Repeat it as a fact","Find and check the original source","Trust the Warden's confidence",1,"Check the original source and whether it supports the claim."),
            new WinterQuestion("Two websites repeat a single anonymous post. Does that establish the claim?","Yes, two pages are enough","Yes, if both look polished","No; seek independent reliable evidence",2,"Copies are not independent corroboration."),
            new WinterQuestion("An AI gives a link, but opening it shows a page about something else. What should you conclude?","That link does not support the claim","Any link proves the answer","The claim must be true because it is detailed",0,"Read the source itself. A real link can still be irrelevant."),
            new WinterQuestion("You cannot verify a claim before presenting it. What is the honest response?","Invent a reference","Say it is uncertain and explain what remains unchecked","Remove the word 'maybe'",1,"State uncertainty and the limits of the evidence."),
            new WinterQuestion("A chatbot says a cracked ice bridge is safe. What should your party do?","Test it by running across","Ask again until it says yes","Stay back and ask the qualified local ranger",2,"An AI response cannot inspect the bridge. Ask the responsible qualified person.")
        };
    }
}
