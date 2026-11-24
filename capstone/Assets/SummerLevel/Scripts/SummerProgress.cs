using System;

namespace AmbrosJourney.Summer
{
    // Serializable lesson state. JourneySession checkpoints it without depending on Unity types.
    [Serializable]
    public sealed class SummerProgress
    {
        public bool GuideBriefed;
        public bool Introduced, GuideArrived, GardenWarriorRead;
        public bool Valid()
        {
            if(BossQuestion<0||BossQuestion>5||BossScore<0||BossScore>BossQuestion||Wards==null||Wards.Length!=3||RemovedPrivateDetails==null||RemovedPrivateDetails.Length!=3||WorkshopQuestion<0||WorkshopQuestion>2||GardenQuestion<0||GardenQuestion>1||PatternStep<0||PatternStep>3)return false;
            return Wards[0]==(WorkshopQuestion==2)&&PatternComplete==(PatternStep==3)&&Wards[1]==(GardenQuestion==1)&&(!Complete||(BossFinished&&BossScore>=4&&CanFight));
        }
        public bool[] Wards = new bool[3];
        public bool[] RemovedPrivateDetails = new bool[3];
        public int WorkshopQuestion, GardenQuestion, PatternStep, BossQuestion, BossScore;

        public bool HintChosen, PatternComplete, PrivacyComplete, BossStarted, BossFinished, Complete;
        public int WardCount { get { int n = 0; foreach (bool ward in Wards) if (ward) n++; return n; } }
        public bool CanFight => Introduced && WardCount == 3;

        public bool AnswerWorkshop(int choice)
        {
            if (!Introduced || Wards[0] || choice < 0 || choice > 1) return false;
            bool correct = choice == SummerContent.Workshop[WorkshopQuestion].Correct;
            if (correct) { WorkshopQuestion++; if (WorkshopQuestion == 2) Wards[0] = true; }

            return correct;
        }
        public bool ChooseHint(int choice)
        {
            if (!Introduced || Wards[1] || choice < 0 || choice > 1) return false;
            HintChosen = choice == 0;

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
            if (!Introduced || Wards[1] || choice < 0 || choice > 1) return false;
            bool correct = choice == 0;
            if (correct) { GardenQuestion = 1; Wards[1] = true; }
            return correct;
        }
        public bool AnswerMessage(int choice)
        {
            if (!Introduced || Wards[2] || choice < 0 || choice > 1) return false;
            if (!PrivacyComplete)
            {
                bool safe=choice==SummerContent.Message[0].Correct;
                if(safe){PrivacyComplete=true;for(int i=0;i<RemovedPrivateDetails.Length;i++)RemovedPrivateDetails[i]=true;}
                return safe;
            }
            bool correct = choice == SummerContent.Message[1].Correct;
            if (correct) Wards[2] = true;
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
            if (correct) BossScore++;
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
            new SummerQuestion("You want to understand a science topic. How can AI help while you still do the learning?","Ask for a simple example, then explain it in your own words","Copy its explanation into your homework without checking it",0,"AI can explain ideas, but you need to check the explanation and practice using it yourself."),
            new SummerQuestion("You tell an AI chatbot that a bridge is cracked. It says the bridge is safe. What should the village do?","Cross the bridge because the AI sounds sure","Ask a trained bridge inspector to check it before anyone crosses",1,"The chatbot has not inspected this bridge. A trained bridge inspector needs to check the real damage before anyone crosses.")
        };
        public static readonly SummerQuestion[] Message = {
            new SummerQuestion("You want AI to help plan a treasure hunt for a birthday party. Which details should you include in your request?","The birthday child’s full name, home address, and the names of their friends","The players’ age group, the party theme, and how many clues you need",1,"The age group, theme and number of clues help AI plan the game. You do not need to share anyone’s full name, home address or list of friends."),
            new SummerQuestion("Your friend sends you a message saying they are upset. You aren’t sure what happened. What should you do first?","Share your friend’s message with AI and ask it to write your reply","Ask your friend what happened and listen to what they want to share",1,"AI cannot know how your friend feels. Listen to your friend and keep their message private. If you are worried about their safety or need help, talk to a trusted adult.")
        };
        public static readonly SummerQuestion[] Boss = {
            new SummerQuestion("Your teacher says you may use AI for hints, but you must solve the homework yourself. You are stuck on a math problem. Which request follows that rule?","Show me a complete solution to this problem so I can learn by reading it","Look at my first step and give me a hint about what to try next",1,"A hint leaves the next step for you to work out. Reading a complete solution does not follow this teacher’s rule."),
            new SummerQuestion("Your teacher allows a tool that checks spelling, but says AI must not write your paragraph. Which choice follows that rule?","Write the paragraph yourself, then use the tool to find spelling mistakes","Ask AI to rewrite your ideas as a paragraph, then edit it to sound like you",0,"Use the spelling help your teacher allows. Editing an AI-written paragraph does not make it follow the rule that you must write it yourself."),
            new SummerQuestion("AI suggests a creature for your story, but its special powers would solve the story’s main problem too easily. What should you do?","Change the main problem so you can keep all of the AI’s idea","Choose the parts that fit your story and change the creature’s powers",1,"You decide what fits your story. You can change an AI suggestion instead of changing your story to fit it."),
            new SummerQuestion("An AI chatbot asks you to type your account password into the conversation. What should you do?","Put it in the chat just for this session","Keep it out of the chat and ask an adult about the request",1,"Don't put a password in a conversation. An adult can help you check how the tool is supposed to work."),
            new SummerQuestion("An AI math answer matches the answer key, but you cannot explain the steps. What is the best way to check whether you understand?","Read the AI’s steps until they look familiar, then move on","Explain the steps in your own words, then try a similar problem without the AI answer",1,"Recognizing the steps is not the same as understanding them. Explaining them and trying another problem help you check that you can do the thinking yourself.")
        };
    }
}
