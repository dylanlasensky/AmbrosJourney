using System;

namespace AmbrosJourney.Summer
{
    // Serializable lesson state. JourneySession checkpoints it without depending on Unity types.
    [Serializable]
    public sealed class SummerProgress
    {
        public bool Introduced;
        public bool Valid()
        {
            if(Courage<1||Courage>3||BossQuestion<0||BossQuestion>5||BossScore<0||BossScore>BossQuestion||Wards==null||Wards.Length!=3||RemovedPrivateDetails==null||RemovedPrivateDetails.Length!=3||WorkshopQuestion<0||WorkshopQuestion>2||GardenQuestion<0||GardenQuestion>1||PatternStep<0||PatternStep>3)return false;
            return Wards[0]==(WorkshopQuestion==2)&&PatternComplete==(PatternStep==3)&&Wards[1]==(GardenQuestion==1)&&(!Complete||(BossFinished&&BossScore>=4&&CanFight));
        }
        public bool[] Wards = new bool[3];
        public bool[] RemovedPrivateDetails = new bool[3];
        public int WorkshopQuestion, GardenQuestion, PatternStep, BossQuestion, BossScore;
        public int Courage = 3;
        public bool HintChosen, PatternComplete, PrivacyComplete, BossStarted, BossFinished, Complete;
        public int WardCount { get { int n = 0; foreach (bool ward in Wards) if (ward) n++; return n; } }
        public bool CanFight => Introduced && WardCount == 3;
        public void LoseCourage() { if (--Courage <= 0) Courage = 3; }
        public bool AnswerWorkshop(int choice)
        {
            if (!Introduced || Wards[0] || choice < 0 || choice > 1) return false;
            bool correct = choice == SummerContent.Workshop[WorkshopQuestion].Correct;
            if (correct) { WorkshopQuestion++; if (WorkshopQuestion == 2) Wards[0] = true; }
            else LoseCourage();
            return correct;
        }
        public bool ChooseHint(int choice)
        {
            if (!Introduced || Wards[1] || choice < 0 || choice > 1) return false;
            HintChosen = choice == 0;
            if (!HintChosen) LoseCourage();
            return HintChosen;
        }
        public bool StepOnStone(int index)
        {
            if (!Introduced || !HintChosen || PatternComplete) return false;
            if (index != SummerContent.Pattern[PatternStep]) { PatternStep = 0; return false; }
            PatternStep++; PatternComplete = PatternStep == 3;
            return true;
        }
        public bool AnswerGarden(int choice)
        {
            if (!PatternComplete || Wards[1] || choice < 0 || choice > 1) return false;
            bool correct = choice == 0;
            if (correct) { GardenQuestion = 1; Wards[1] = true; } else LoseCourage();
            return correct;
        }
        public bool AnswerMessage(int choice)
        {
            if (!Introduced || Wards[2] || choice < 0 || choice > (PrivacyComplete ? 1 : 2)) return false;
            if (!PrivacyComplete)
            {
                if (choice >= 0 && choice < 3) RemovedPrivateDetails[choice] = true;
                PrivacyComplete = RemovedPrivateDetails[0] && RemovedPrivateDetails[1] && RemovedPrivateDetails[2];
                return true;
            }
            bool correct = choice == 1;
            if (correct) Wards[2] = true; else LoseCourage();
            return correct;
        }
        public bool BeginBoss()
        {
            if (!CanFight || Complete) return false;
            BossStarted = true; BossQuestion = 0; BossScore = 0; BossFinished = false; return true;
        }
        public bool AnswerBoss(int choice)
        {
            if (!CanFight || !BossStarted || BossFinished || Complete || choice < 0 || choice > 1) return false;
            bool correct = choice == SummerContent.Boss[BossQuestion].Correct;
            if (correct) BossScore++; else LoseCourage();
            BossQuestion++;
            if (BossQuestion == 5) { BossFinished = true; Complete = BossScore >= 4; }
            return correct;
        }
    }
    public sealed class SummerQuestion
    {
        public readonly string Prompt, Explanation;
        public readonly string[] Choices;
        public readonly int Correct;
        public SummerQuestion(string prompt, string a, string b, int correct, string explanation)
        { Prompt = prompt; Choices = new[] { a, b }; Correct = correct; Explanation = explanation; }
    }
    public static class SummerContent
    {
        public static readonly int[] Pattern = { 0, 2, 1 };
        public static readonly string[] Notes = {
            "PICK THE RIGHT KIND OF HELP\nAI can help you come up with ideas or explain a topic. You still need to decide what fits. Some jobs need someone who can look at the real problem, like a builder checking a damaged bridge.",
            "LEAVE ROOM TO PRACTICE\nTry a step yourself, then ask for a hint if you need one. Check that you can explain what you did. Follow your teacher's rules about which help is allowed.",
            "SHARE LESS, ASK PEOPLE TOO\nA useful request doesn't need your password or home address. Use made-up details when you can. If someone is upset or you need personal support, talk to a real person you trust."
        };
        public static readonly SummerQuestion[] Workshop = {
            new SummerQuestion("The village is naming its summer festival. How could AI help?","Suggest a few names for the group to discuss","Choose the name and send it straight to the printer",0,"Suggestions can get you started. The people planning the festival know what fits their village."),
            new SummerQuestion("A plank on the bridge has cracked. What is the best next step?","Describe it to AI and use its repair steps","Ask the builder to look at the bridge",1,"The builder can inspect the real bridge. A chat description can't show everything that matters.")
        };
        public static readonly SummerQuestion[] Boss = {
            new SummerQuestion("You read an AI explanation but still can't start the homework. What would help most?","Ask for a small example, then try a similar one","Ask it to finish this question so you can study the answer",0,"An example can help you get going. Trying a new one shows which parts you understand."),
            new SummerQuestion("Your teacher allows spellcheck but says not to use AI to write the paragraph. What fits that rule?","Ask AI for a paragraph and change a few words","Write your paragraph and use the allowed spellcheck",1,"Use the help the teacher allows. Changing a few words doesn't make an AI paragraph your own work."),
            new SummerQuestion("AI suggests a creature for your story, but it doesn't fit your plot. What should you do?","Keep the parts you like and change what doesn't fit","Keep the whole idea because the tool made it consistent",0,"You know the story you're trying to tell. Suggestions don't have to decide it for you."),
            new SummerQuestion("A story tool asks for your account password to save your ideas. What should you do?","Put it in the chat just for this session","Keep it out of the chat and ask an adult about the request",1,"Don't put a password in a conversation. An adult can help you check how the tool is supposed to work."),
            new SummerQuestion("An AI answer matches the answer key, but you can't explain the steps. Are you ready to hand it in?","Yes, since the result matches","Not yet; try the steps and a similar problem yourself",1,"A right result doesn't show what you've learned. Check that you can do the thinking too.")
        };
    }
}
