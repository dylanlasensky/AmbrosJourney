// Defines the cumulative AI-safety activities and validates structured responses.
using System;
using System.Collections.Generic;

namespace AmbrosJourney.Winter
{
    public enum FinalActivityKind { Choose, SelectMany, Arrange, Match }

    // Plain lesson data and grading shared by the real UI and the progression checks.
    public sealed class FinalChallenge
    {
        public readonly string Prompt, Explanation, Instruction;
        public readonly string[] Choices, Categories;
        public readonly int[] Expected;
        public readonly FinalActivityKind Kind;
        public int Correct => Expected[0];
        public FinalChallenge(FinalActivityKind kind,string prompt,string instruction,string[] choices,int[] expected,string explanation,string[] categories=null)
        { Kind=kind;Prompt=prompt;Instruction=instruction;Choices=choices;Expected=expected;Explanation=explanation;Categories=categories; }
        public bool TryEvaluate(int[] response,out bool right)
        {
            right=false;if(response==null||response.Length==0)return false;
            int limit=Kind==FinalActivityKind.Match?Categories.Length:Choices.Length;
            var seen=new HashSet<int>();
            foreach(int value in response)if(value<0||value>=limit||(Kind!=FinalActivityKind.Match&&!seen.Add(value)))return false;
            if(Kind==FinalActivityKind.Choose&&response.Length!=1)return false;
            if((Kind==FinalActivityKind.Arrange||Kind==FinalActivityKind.Match)&&response.Length!=Expected.Length)return false;
            if(Kind==FinalActivityKind.SelectMany){right=seen.SetEquals(Expected);return true;}
            right=response.Length==Expected.Length;
            for(int i=0;right&&i<Expected.Length;i++)right=response[i]==Expected[i];
            return true;
        }
    }

    public static class FinalChallenges
    {
        public static readonly FinalChallenge[] All={
            new FinalChallenge(FinalActivityKind.SelectMany,
                "Your school club is planning a trip to a nature park. You want AI to suggest activities, so you write the message below. Some details help explain what you need, but others share private information.",
                "Select the two details you should remove before sending the message.",
                new[]{"Our club wants to visit a nature park","Here is my home address so you can plan where the bus should pick me up","Suggest three activities we could do if it rains","Here is my account password so you can log in and read our club’s trip plans","The activities should be suitable for middle-school students"},
                new[]{1,3},
                "Leave out the address and password. The kind of trip, weather and age group give enough background without exposing an account or home."),
            new FinalChallenge(FinalActivityKind.Arrange,
                "Your lunch club wants a school poster that encourages students to waste less food. You want AI to suggest three short tips that new club members can understand. Start with what AI should do, then give the topic and who it is for. End with how to write the answer.",
                "Order: job, topic, who it is for, how to write the answer.",
                new[]{"about wasting less food","Give three tips, each under ten words.","Suggest tips for a school poster","for students who just joined our lunch club."},
                new[]{2,0,3,1},
                "The request gives a job, useful background, an audience and a format. This is one clear order, not the only way a useful prompt can be written."),
            new FinalChallenge(FinalActivityKind.Choose,
                "Your group is choosing classmates to help with a project. Someone shows AI one short message from a classmate. AI says that person cannot be trusted to help and suggests leaving them out. AI does not know the classmate or what happened before the message.",
                "What should your group do before deciding?",
                new[]{"Talk with the classmate and make a fair decision together as a group","Ask AI to score how helpful each classmate would be","Follow AI’s suggestion, but tell the classmate that AI made the choice"},
                new[]{0},
                "A short message doesn't tell the tool who someone is. Don't let an AI guess decide how you treat a person. Talk to them and get help from a trusted adult if needed."),
            new FinalChallenge(FinalActivityKind.Arrange,
                "Your club plans to visit the town pool. AI writes a message saying the pool will be closed, but you have not checked the date or which pool it means. You need to check the message before sending it to club members.",
                "Put these steps in the order you should follow.",
                new[]{"Compare AI’s message with that notice","Check the pool’s notice, place and date","Fix mistakes; list what needs checking"},
                new[]{1,0,2},
                "Open the relevant official notice first. Then compare the AI draft with it. Use what you found to correct mistakes before sharing; say if something is still unconfirmed."),
            new FinalChallenge(FinalActivityKind.Match,
                "Your group is getting ready for a science fair. You need a name for your table, you need to practice explaining your experiment, and you notice a sparking electrical cable nearby. These jobs need different kinds of help.",
                "Match each job to the best answer below. Use each answer once.",
                new[]{"Find out whether the sparking electrical cable is safe","Think of names for our science-fair table","Practice explaining our experiment to visitors"},
                new[]{1,0,2},
                "AI can help with ideas. A qualified person needs to inspect a real electrical problem. Practicing your explanation is a job you need to take part in.",
                new[]{"Ask AI for ideas, then choose for ourselves","Ask a trained person to check it","Practice it ourselves, then ask for feedback"}),
            new FinalChallenge(FinalActivityKind.SelectMany,
                "An AI has been trained to find litter in beach photos. Most of its practice photos were taken on sunny days. Your club wants to know whether it also works well when the beach looks different.",
                "Select the two checks that would best help your club find mistakes in the AI’s labels.",
                new[]{"Test it with photos taken on cloudy days and near sunset","Ask it to sound more sure when explaining its answers","Ask people who know the beach to look at the photos and check whether its labels are right","Measure how quickly it labels its practice photos"},
                new[]{0,2},
                "Try conditions it hasn't practiced much and compare its labels with people's checks. A confident explanation or fast result doesn't show which mistakes it makes."),
            new FinalChallenge(FinalActivityKind.Match,
                "AI helped write a club article about how many students visit your town’s library each week. Before publishing it, you need to check where its numbers came from and whether they describe your library.",
                "Match each finding to a next step. You can use an answer more than once.",
                new[]{"Three AI tools give the same number, but none says where it came from","AI links to a real report, but the number is for a library in another town","Your library’s recent report confirms the number and explains which week was counted"},
                new[]{0,0,1},
                "Matching AI answers aren't separate evidence. A real link can also be irrelevant. Trace each claim to a source that actually supports it, then cite it with its limits.",
                new[]{"Find a source that checks the number for our town’s library","Use the number, name the source, and explain which library and week it covers","Ask AI to write the number in a clearer sentence"}),
            new FinalChallenge(FinalActivityKind.SelectMany,
                "You used AI to help write a club update and already sent it to club members. Later, you checked a trusted source and discovered that one fact was wrong. You found the correct information, but some other details still need checking. You now need to send a correction to the people who received the original update.",
                "Select the three things your correction should include.",
                new[]{"Tell readers which part of your earlier update was wrong","Give the correct information and show the source you checked","Say which details you still need to check","Fix your own copy without telling the people who saw the mistake","Keep the original information and add “AI can make mistakes” at the bottom"},
                new[]{0,1,2},
                "Tell the people who saw the mistake what changed. Give the evidence for the correction and explain any uncertainty. A general warning doesn't fix the original claim.")
        };
    }
}
