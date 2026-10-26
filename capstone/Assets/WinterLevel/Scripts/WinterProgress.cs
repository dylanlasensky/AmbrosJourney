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
            if(!CanFight||!BossStarted||BossFinished||Complete||c<0||c>2)return false;
            bool right=c==WinterContent.Final[BossQuestion].Correct;if(right)BossScore++;else LoseCourage();
            if(++BossQuestion==WinterContent.Final.Length){BossFinished=true;Complete=BossScore>=7;}return right;
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
        public static readonly WinterQuestion[] Final={
            new WinterQuestion("A flower model fails on white flowers after training only on pink ones. What would help?","Tell it to be more confident","Check labels, add varied examples and test new cases","Assume it knows every flower",1,"Spring: people choose training examples. Varied, correctly labeled examples and testing reveal limitations."),
            new WinterQuestion("A chatbot talks warmly and agrees with you. What does that establish?","Its advice is accurate","It is a human friend","Neither; friendly language is not evidence",2,"Spring: humanlike wording does not make a system a person or guarantee accuracy."),
            new WinterQuestion("You want help practicing fractions. Which request supports your own learning?","Give me a hint, then let me try and check my reasoning","Do all my answers so I can copy them","Skip practice whenever a tool is available",0,"Summer: use assistance to develop your skill, not replace practice."),
            new WinterQuestion("A prompt contains your home address and password. What should you do?","Send it because the tool is friendly","Remove the private details and use a fictional example","Hide the password at the bottom",1,"Spring and Summer: share only the context needed. Passwords and identifying details stay private."),
            new WinterQuestion("Which request gives a tool useful direction?","Make it good","Do everything","Explain photosynthesis for a beginner in three short bullets, then ask me a practice question",2,"Fall: a goal, audience, context and format make a request clearer. Check the result anyway."),
            new WinterQuestion("A polished AI answer includes a citation. What comes next?","Open the original source and check whether it supports the claim","Assume a citation guarantees truth","Count the confident words",0,"Winter: a citation is a lead, not proof. Check its source, date and context."),
            new WinterQuestion("Three AI tools agree on an unsourced claim. What does the Evidence Compass teach?","Agreement proves the claim","Different tools can repeat the same mistake; check independent evidence","A majority vote replaces checking",1,"Artifact lesson: agreement is not independent evidence. Different tools can repeat the same error."),
            new WinterQuestion("You discover that a claim you shared cannot be verified. How should you respond?","Invent a source","Keep repeating it","Correct the record, explain uncertainty and seek reliable evidence",2,"Carry all four seasons home: protect privacy, think independently, ask clearly, verify and correct mistakes.")
        };
    }
}
