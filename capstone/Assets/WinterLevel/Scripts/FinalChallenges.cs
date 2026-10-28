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
                "You're asking AI for ideas for a club trip. Pick the parts to leave out before sending this message.",
                "Select the private details, then check your answer.",
                new[]{"Our club wants a nature trip","My house is at 46 Willow Lane","We need three ideas for rainy weather","My login password is CedarTrail42","The ideas should suit middle-school students"},
                new[]{1,3},
                "Leave out the address and password. The kind of trip, weather and age group give enough background without exposing an account or home."),
            new FinalChallenge(FinalActivityKind.Arrange,
                "Build a request for a club poster. Put the job first, then the topic, the audience and the format.",
                "Drag each card into a numbered space. You can also click a card, then a space.",
                new[]{"about cutting food waste","Use three bullets under ten words each.","Suggest tips for a school poster","for students new to our lunch club."},
                new[]{2,0,3,1},
                "The request gives a job, useful background, an audience and a format. This is one clear order, not the only way a useful prompt can be written."),
            new FinalChallenge(FinalActivityKind.Choose,
                "A chatbot says your group should leave a classmate out because they seem 'unreliable.' It has only seen a short message. What should you do?",
                "Choose what you would do next.",
                new[]{"Talk with the classmate and decide fairly with the group","Ask the chatbot to rate each classmate before deciding","Use its suggestion, but explain that it came from AI"},
                new[]{0},
                "A short message doesn't tell the tool who someone is. Don't let an AI guess decide how you treat a person. Talk to them and get help from a trusted adult if needed."),
            new FinalChallenge(FinalActivityKind.Arrange,
                "AI has drafted a notice saying the town pool is closed. Arrange these checks before you share it.",
                "Drag the cards into order, or click a card and then a space.",
                new[]{"Compare it with a separate current notice from the pool","Read the original notice, including its date and location","Fix the draft and say what is still unconfirmed"},
                new[]{1,0,2},
                "First check the original notice in context. Then compare separate current evidence. Update the draft before sharing, and be clear about anything you couldn't confirm."),
            new FinalChallenge(FinalActivityKind.Match,
                "Your group is planning a science fair. Match each job to the help that fits it best.",
                "Click each answer to change it. Fill every row before checking.",
                new[]{"Check whether a sparking cable is safe","Come up with possible stall names","Practice explaining your own experiment"},
                new[]{1,0,2},
                "AI can help with ideas. A qualified person needs to inspect a real electrical problem. Practicing your explanation is a job you need to take part in.",
                new[]{"AI ideas, then our choice","Ask a qualified person","Try it ourselves, then get feedback"}),
            new FinalChallenge(FinalActivityKind.SelectMany,
                "AI labels litter in beach photos. It learned mostly from sunny daytime pictures. Pick the two checks that would best reveal what it misses.",
                "Select two checks, then check your answer.",
                new[]{"Test pictures from cloudy days and dusk","Ask it to describe its answers more confidently","Check the labels with people who know the beach","Count how fast it labels its practice pictures"},
                new[]{0,2},
                "Try conditions it hasn't practiced much and compare its labels with people's checks. A confident explanation or fast result doesn't show which mistakes it makes."),
            new FinalChallenge(FinalActivityKind.Match,
                "A club article needs checking. Match each discovery to the next step that fits.",
                "Click each answer to change it. The same step can fit more than one row.",
                new[]{"Three AI tools repeat a statistic without a source","A real report is linked, but its result is about another town","A dated local report supports the exact claim"},
                new[]{0,0,1},
                "Matching AI answers aren't separate evidence. A real link can also be irrelevant. Trace each claim to a source that actually supports it, then cite it with its limits.",
                new[]{"Find evidence for this exact claim","Use it with its source and limits","Ask AI to rewrite the claim"}),
            new FinalChallenge(FinalActivityKind.SelectMany,
                "You posted an AI-written club update and later found a mistake. Pick everything your follow-up should do.",
                "Select the parts you would include in your correction.",
                new[]{"Say which part of the earlier update was wrong","Share the checked source and the corrected information","Say which details still need checking","Leave the old post alone and only edit your own copy","Keep the claim but add 'AI may be wrong' at the bottom"},
                new[]{0,1,2},
                "Tell the people who saw the mistake what changed. Give the evidence for the correction and explain any uncertainty. A general warning doesn't fix the original claim.")
        };
    }
}
