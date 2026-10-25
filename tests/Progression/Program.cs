using System;
using AmbrosJourney.Summer;
using AmbrosJourney.Fall;

static class Program
{
    static int checks;
    static void Check(bool condition, string message)
    {
        checks++;
        if (!condition) throw new Exception(message);
    }
    static SummerProgress Summer()
    {
        var p = new SummerProgress();
        Check(!p.BeginBoss() && !p.AnswerWorkshop(0) && !p.AnswerMessage(0), "Summer requires introduction");
        p.Introduced = true;
        Check(!p.AnswerWorkshop(1) && p.WorkshopQuestion == 0, "Wrong answer preserves workshop step");
        Check(p.AnswerWorkshop(0) && p.AnswerWorkshop(1), "Workshop completes");
        Check(!p.BeginBoss(), "One ward cannot unlock boss");
        Check(!p.StepOnStone(0), "Stones require hint");
        Check(!p.ChooseHint(1) && p.ChooseHint(0), "Hint retry");
        Check(p.StepOnStone(0) && !p.StepOnStone(1) && p.PatternStep == 0, "Wrong stone resets only pattern");
        foreach (int i in new[]{0,2,1}) Check(p.StepOnStone(i), "Sun, seed, leaf order");
        Check(!p.AnswerGarden(1) && p.AnswerGarden(0), "Garden retry");
        Check(!p.BeginBoss(), "Two wards cannot unlock boss");
        p.AnswerMessage(0); p.AnswerMessage(0);
        Check(!p.PrivacyComplete, "Duplicate privacy selection cannot bypass exercise");
        p.AnswerMessage(1); p.AnswerMessage(2);
        Check(p.PrivacyComplete && !p.AnswerMessage(0) && p.AnswerMessage(1), "Privacy and human support complete");
        Check(p.WardCount == 3, "Three wards earned");
        return p;
    }
    static FallProgress Fall()
    {
        var p = new FallProgress();
        Check(!p.BeginBoss() && !p.ReadNote(0) && !p.AnswerMission(0,1), "Fall requires introduction");
        p.Introduced = true;
        Check(!p.ReadNote(-1) && !p.ReadNote(3), "Invalid notes rejected");
        for (int m = 0; m < 3; m++)
        {
            Check(!p.BeginBoss(), "Fall requires all three runes");
            Check(!p.AnswerMission(m, FallContent.Missions[m][0].Correct), "Each mission requires its note");
            Check(p.ReadNote(m), "Note accepted");
            foreach (var q in FallContent.Missions[m])
            {
                Check(q.Correct >= 0 && q.Correct < q.Choices.Length, "Valid content answer index");
                int step = p.Steps[m];
                Check(!p.AnswerMission(m, (q.Correct+1)%3) && p.Steps[m] == step, "Mistakes preserve mission progress");
                Check(p.AnswerMission(m, q.Correct), "Correct mission choice");
            }
            Check(p.MissionComplete(m) && !p.AnswerMission(m,0), "No duplicate rune rewards");
        }
        return p;
    }
    static void Main()
    {
        for (int mask = 0; mask < 32; mask++)
        {
            var p = Summer(); Check(p.BeginBoss(), "Summer boss begins"); int score = 0;
            for (int q = 0; q < 5; q++)
            {
                int answer = (mask >> q) & 1;
                if (answer == SummerContent.Boss[q].Correct) score++;
                p.AnswerBoss(answer);
            }
            Check(p.BossScore == score && p.Complete == (score >= 4) && p.BossFinished, "Summer exact 80 percent threshold");
            Check(!p.AnswerBoss(0) && p.BossScore == score, "Summer finished attempt cannot accept extra answers");
            if (!p.Complete)
            {
                Check(p.BeginBoss() && p.WardCount == 3 && p.BossScore == 0, "Summer retry retains wards and resets score");
                foreach (var q in SummerContent.Boss) p.AnswerBoss(q.Correct);
            }
            Check(p.Complete && !p.BeginBoss(), "Summer success is final");
            for(int i=0;i<10;i++) p.LoseCourage();
            Check(p.Complete && p.WardCount == 3 && p.Courage > 0, "Summer courage recovery retains learning");
        }
        for (int mask = 0; mask < 243; mask++)
        {
            var p = Fall(); Check(!p.AnswerBoss(0), "Fall boss must be started"); Check(p.BeginBoss(), "Fall boss begins");
            Check(!p.AnswerBoss(-1) && !p.AnswerBoss(3) && p.BossQuestion == 0, "Invalid guardian choices cannot consume a question");
            int code = mask, score = 0;
            for(int q=0;q<5;q++) { int answer=code%3; code/=3; if(answer==FallContent.Boss[q].Correct)score++; p.AnswerBoss(answer); }
            Check(p.BossScore == score && p.Complete == (score >= 4) && p.BossFinished, "Fall exact 80 percent threshold");
            Check(!p.AnswerBoss(0) && p.BossScore == score, "Fall finished attempt cannot accept extra answers");
            if(!p.Complete) { Check(p.BeginBoss() && p.RuneCount == 3 && p.BossScore == 0, "Fall retry preserves runes"); foreach(var q in FallContent.Boss)p.AnswerBoss(q.Correct); }
            Check(p.Complete && !p.BeginBoss(), "Fall success is final");
            for(int i=0;i<10;i++)p.LoseCourage();
            Check(p.Complete && p.RuneCount==3 && p.Courage>0, "Fall courage recovery retains learning");
        }
        Console.WriteLine($"PASS: {checks} assertions, all 32 Summer and 243 Fall guardian outcomes.");
    }
}
