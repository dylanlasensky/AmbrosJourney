using System;

namespace AmbrosJourney.Summer
{
    // Session-only state. No changes to Spring's progression or save format.
    [Serializable]
    public sealed class SummerProgress
    {
        public bool Introduced;
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
            "CHOOSE THE RIGHT TOOL\nAI can suggest ideas or explain unfamiliar topics. People still choose what fits. A damaged bridge needs a qualified person to inspect it; a chatbot cannot establish its safety.",
            "KEEP YOUR PRACTICE\nTry first. Ask for a hint. Explain the answer in your own words. Help should leave you something to work out. Follow your teacher's rules about AI.",
            "ASK SAFELY\nShare only what the task needs. Keep your full name, address, passwords and private photos private. Use fictional examples. Talk with real people and trusted adults when you need support."
        };
        public static readonly SummerQuestion[] Workshop = {
            new SummerQuestion("The village needs names for its summer festival. Which use of AI fits the task?", "Brainstorm names, then choose together", "Send everyone's private messages", 0, "AI can offer possibilities. People decide what fits, without sharing private messages."),
            new SummerQuestion("The bridge looks damaged. What should the village do?", "Cross because a chatbot says it is safe", "Ask the village builder to inspect it", 1, "A chatbot cannot establish whether this particular bridge is safe. Ask a qualified person.")
        };
        public static readonly SummerQuestion[] Boss = {
            new SummerQuestion("You do not understand a homework topic. What could help you learn?", "Ask for an explanation, then try an example yourself", "Copy a finished answer without understanding it", 0, "Use explanations to support your own practice."),
            new SummerQuestion("Your teacher says this assignment must be completed without AI. What should you do?", "Use AI anyway and hide it", "Follow the rule; ask your teacher for help", 1, "Follow classroom rules. Your teacher can explain what help is allowed."),
            new SummerQuestion("You want ideas for a fictional creature. What could you do?", "Ask for suggestions, then develop your own idea", "Let AI make every choice because your ideas do not matter", 0, "Brainstorming can help. Your choices and creativity still matter."),
            new SummerQuestion("An AI tool asks for your password to help with a story. What should you do?", "Send it to get a better story", "Keep it private and stop sharing sensitive information", 1, "A story does not need your password. Ask a trusted adult if a tool requests private information."),
            new SummerQuestion("AI completed a problem, but you cannot explain it. What next?", "Submit it without reading it", "Work through the steps and practice a similar problem", 1, "Practice and explaining the steps help you build understanding.")
        };
    }
}
