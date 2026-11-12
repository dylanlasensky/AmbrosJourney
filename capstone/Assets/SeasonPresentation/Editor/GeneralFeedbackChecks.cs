#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using AmbrosJourney.Journey;
using AmbrosJourney.Summer;
using AmbrosJourney.Fall;
using AmbrosJourney.Winter;
using AmbrosJourney.Presentation;
[InitializeOnLoad]
// Run only in a disposable project copy: enters Play mode, uses a separate save, then exits.
public static class GeneralFeedbackChecks
{
    const string Key="GeneralFeedbackChecks",Report="Logs/SeasonChecks/GeneralFeedback.txt";
    static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    static double ready;static int stage,checks;
    static GeneralFeedbackChecks(){EditorApplication.update+=Tick;}
    public static void Run()
    {
        if(!Application.isBatchMode)throw new InvalidOperationException("Run in a disposable batch-mode project copy.");
        Directory.CreateDirectory("Logs/SeasonChecks");File.WriteAllText(Report,"RUNNING");SessionState.SetBool(Key,true);
        SessionState.SetString(Key+"deadline",DateTime.UtcNow.AddMinutes(8).ToString("O"));
        EditorSceneManager.OpenScene("Assets/Scenes/Spring.unity");EditorApplication.isPlaying=true;
    }
    static void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
    static object Get(object o,string field)=>o.GetType().GetField(field,Flags).GetValue(o);
    static void Set(object o,string field,object value)=>o.GetType().GetField(field,Flags).SetValue(o,value);
    static void Call(object o,string method,params object[] args)=>o.GetType().GetMethod(method,Flags).Invoke(o,args);
    static void Next(SeasonOverlay ui)=>ui.Choices[0].onClick.Invoke();
    static void Choice(SeasonOverlay ui,int answer)=>ui.Choices[answer].onClick.Invoke();
    static void Move(WinterLevel l,Vector2 p){l.player.transform.position=p;l.player.GetComponent<Rigidbody2D>().position=p;Physics2D.SyncTransforms();}
    static void Feedback(SeasonOverlay ui,bool right,int score,int total,string capture=null)
    {
        Check(ui.Copy.text.StartsWith("Correct!")==right,"Explicit correct feedback: "+ui.Copy.text);
        Check(ui.Copy.text.Contains(score+" / "+total+" correct."),"Fixed quiz denominator: "+ui.Copy.text);
        Call(ui,"LateUpdate");Canvas.ForceUpdateCanvases();
        foreach(var text in ui.GetComponentsInChildren<TMP_Text>())
        {text.ForceMeshUpdate();Check(!text.isTextOverflowing,"Text overflow: "+text.text);Check(!text.text.ToLowerInvariant().Contains("courage")&&!text.text.ToLowerInvariant().Contains("heart"),"Removed mechanic appears on screen");}
        if(capture!=null)SeasonIntegrationChecks.Capture(capture);
    }
    static SummerProgress SummerReady()
    {
        var p=new SummerProgress{Introduced=true,GuideArrived=true};foreach(var q in SummerContent.Workshop)p.AnswerWorkshop(q.Correct);p.ChooseHint(0);foreach(int stone in SummerContent.Pattern)p.StepOnStone(stone);p.AnswerGarden(0);p.AnswerMessage(0);p.AnswerMessage(1);p.AnswerMessage(2);p.AnswerMessage(1);return p;
    }
    static FallProgress FallReady()
    {var p=new FallProgress{Introduced=true,GuideArrived=true};for(int m=0;m<3;m++){p.ReadNote(m);foreach(var q in FallContent.Missions[m])p.AnswerMission(m,q.Correct);}return p;}
    static WinterProgress WinterReady()
    {var p=new WinterProgress{Introduced=true};for(int m=0;m<2;m++){p.ReadNote(m);foreach(var q in WinterContent.Missions[m])p.AnswerMission(m,q.Correct);}return p;}
    static void Counter()
    {
        var session=JourneySession.Instance;session.Resume();var ui=session.GetComponentInChildren<JourneyUI>();
        Check(ui.GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="0/12 Artifacts"),"Clear artifact counter label");
        for(int i=0;i<12;i++)session.Data.artifacts[i]=true;session.Resume();Canvas.ForceUpdateCanvases();
        var full=ui.GetComponentsInChildren<TMP_Text>().Single(t=>t.text=="12/12 Artifacts");full.ForceMeshUpdate();Check(!full.isTextOverflowing,"Full artifact count fits");
        for(int i=0;i<12;i++)session.Data.artifacts[i]=false;session.Resume();
    }
    static void Spring()
    {
        var session=JourneySession.Instance;session.BeginTestProfile();session.Data.unlocked=4;session.Resume();Counter();
        var g=UnityEngine.Object.FindAnyObjectByType<SpringGuide>();g.DismissPanel();typeof(SpringGuide).GetProperty("LessonComplete").SetValue(g,true);
        ((bool[])Get(g,"studied"))[0]=true;Set(g,"target",0);typeof(SpringGuide).GetProperty("DialogueOpen").SetValue(g,true);Call(g,"LateUpdate");
        var ui=g.GetComponentInChildren<SeasonOverlay>();var truth=(bool[])Get(g,"truth");int index=(int)typeof(SpringGuide).GetMethod("QuestionIndex",Flags).Invoke(g,new object[]{0});
        Call(g,"Answer",truth[index]?0:1);Call(g,"LateUpdate");Feedback(ui,true,1,2,"FeedbackSpringLesson");Call(g,"Continue");
        index=(int)typeof(SpringGuide).GetMethod("QuestionIndex",Flags).Invoke(g,new object[]{0});Call(g,"Answer",truth[index]?1:0);Call(g,"LateUpdate");Feedback(ui,false,1,2);
        typeof(SpringGuide).GetProperty("MissionsComplete").SetValue(g,3);for(int i=0;i<3;i++){((bool[])Get(g,"cleared"))[i]=true;((int[])Get(g,"trials"))[i]=2;((bool[])Get(g,"studied"))[i]=true;}
        Set(g,"target",3);Set(g,"feedback","");Set(g,"bossQuestion",0);var correct=(int[])Get(g,"correct");Call(g,"Answer",correct[0]);Call(g,"LateUpdate");Feedback(ui,true,1,5,"FeedbackSpringBoss");Call(g,"Continue");
        Call(g,"Answer",1-correct[1]);Call(g,"LateUpdate");Feedback(ui,false,1,5);
    }
    static void Summer()
    {
        var l=UnityEngine.Object.FindAnyObjectByType<SummerLevel>();l.DismissPanel();var ui=l.GetComponentInChildren<SeasonOverlay>();
        l.Interact(1);Next(ui);Choice(ui,SummerContent.Workshop[0].Correct);Feedback(ui,true,1,2,"FeedbackSummerLesson");Next(ui);Choice(ui,1-SummerContent.Workshop[1].Correct);Feedback(ui,false,1,2);
        l.RestoreProgress(SummerReady());l.Interact(4);Next(ui);Choice(ui,SummerContent.Boss[0].Correct);Feedback(ui,true,1,5,"FeedbackSummerBoss");Next(ui);Choice(ui,1-SummerContent.Boss[1].Correct);Feedback(ui,false,1,5);
    }
    static void Fall()
    {
        var l=UnityEngine.Object.FindAnyObjectByType<FallLevel>();l.DismissPanel();var ui=l.GetComponentInChildren<SeasonOverlay>();l.Interact(1);Next(ui);
        Choice(ui,FallContent.Missions[0][0].Correct);Feedback(ui,true,1,FallContent.Missions[0].Length,"FeedbackFallLesson");Next(ui);Choice(ui,(FallContent.Missions[0][1].Correct+1)%3);Feedback(ui,false,1,FallContent.Missions[0].Length);
        l.RestoreProgress(FallReady());l.Interact(4);Next(ui);Choice(ui,FallContent.Boss[0].Correct);Feedback(ui,true,1,5,"FeedbackFallBoss");Next(ui);Choice(ui,(FallContent.Boss[1].Correct+1)%3);Feedback(ui,false,1,5);
    }
    static void Boundaries(WinterLevel l)
    {
        Check(WinterWorld.ConstrainOutdoorCheckpoint(new Vector2(26,31))==WinterWorld.Entrance+Vector2.down*3,"Older save behind ridge recovers in front");
        Check(WinterWorld.ConstrainOutdoorCheckpoint(WinterWorld.Entrance)==WinterWorld.Entrance,"Valid doorway save remains exact");
        var valid=WinterWorld.Entrance+Vector2.down*1.5f;Check(WinterWorld.ConstrainOutdoorCheckpoint(valid)==valid,"Valid outdoor approach save remains exact");
        var body=l.player.GetComponent<Rigidbody2D>();var previous=Physics2D.simulationMode;
        try
        {
            Physics2D.simulationMode=SimulationMode2D.Script;
            foreach(float x in new[]{-36f,-15f,5f,20f,28f,38f})
            {
                Move(l,new Vector2(x,24.5f));for(int i=0;i<40;i++){body.linearVelocity=Vector2.up*5;Physics2D.Simulate(.02f);}
                Check(body.position.y<26.1f,"Cannot walk behind mountain at x="+x+": "+body.position);
            }
            Move(l,new Vector2(24,25));for(int i=0;i<40;i++){body.linearVelocity=Vector2.up*5;Physics2D.Simulate(.02f);}
            Check(body.position.y>26.5f&&body.position.y<28,"Doorway is approachable but its rear is solid: "+body.position);
            Check(SeasonNavigation.FindPath(WinterLevel.Spawn,new Vector2(24,31),WinterWorld.Min,WinterWorld.Max).Count==0,"Cannot slip around the doorway behind the cave");
            Check(!Physics2D.OverlapCircleAll(WinterWorld.Entrance,.5f).Any(c=>!c.GetComponent<WinterActor>()),"Automatic entrance trigger remains clear");
            Check(SeasonNavigation.FindPath(WinterLevel.Spawn,new Vector2(-30,30),WinterWorld.Min,WinterWorld.Max).Count==0,"No route around ridge edge");
        }
        finally{body.linearVelocity=Vector2.zero;Physics2D.simulationMode=previous;}
    }
    static void SubmitActivity(WinterLevel l,int index,bool correct)
    {
        var q=WinterContent.Final[index];var panel=l.GetComponent<FinalActivityPanel>();var response=(int[])q.Expected.Clone();
        if(index==4)
        {
            var note=panel.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Instructions");Check(note.text.Contains("repeatedly"),"Activity 5 explains repeated clicks");
            panel.Select(0);int first=panel.Response[0];panel.Select(0);Check(panel.Response[0]!=first,"Clicking same box cycles answer options");
            SeasonIntegrationChecks.Capture("Activity5ClickInstructions");
        }
        if(!correct)Array.Reverse(response);
        if(q.Kind==FinalActivityKind.Arrange)for(int i=0;i<response.Length;i++)panel.Drop(response[i],i);
        else if(q.Kind==FinalActivityKind.Match){for(int i=0;i<response.Length;i++)while(panel.Response[i]!=response[i])panel.Select(i);}
        else foreach(int choice in response)panel.Select(choice);
        panel.Submit();Feedback(l.GetComponentInChildren<SeasonOverlay>(),correct,l.Progress.BossScore,8,index==0?"FeedbackFinalBoss":null);
    }
    static void Winter()
    {
        var l=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();l.DismissPanel();var ui=l.GetComponentInChildren<SeasonOverlay>();
        l.Interact(1);Next(ui);Choice(ui,WinterContent.Missions[0][0].Correct);Feedback(ui,true,1,2,"FeedbackWinterLesson");Next(ui);Choice(ui,(WinterContent.Missions[0][1].Correct+1)%3);Feedback(ui,false,1,2);Call(l,"Close");Boundaries(l);
        l.RestoreProgress(WinterReady());l.Interact(8);Next(ui);Choice(ui,WinterContent.Boss[0].Correct);Feedback(ui,true,1,5,"FeedbackWinterBoss");Next(ui);Choice(ui,(WinterContent.Boss[1].Correct+1)%3);Feedback(ui,false,1,5);
        var p=WinterReady();p.BeginWinterBoss();foreach(var q in WinterContent.Boss)p.AnswerWinterBoss(q.Correct);l.RestoreProgress(p);Move(l,WinterWorld.Entrance);Call(l,"LateUpdate");Check(p.InCave,"Walk-in cave entrance still works");
        Check(!l.littleMage.GetComponentInChildren<SeasonWorldLabel>(true).enabled,"Little Mage has no E label inside");
        l.Interact(0);Check(!l.IsDialogueOpen,"Little Mage cannot start outdoor dialogue inside");
        Move(l,l.littleMage.transform.position);Check((int)typeof(WinterLevel).GetMethod("Nearest",Flags).Invoke(l,null)!=0,"Little Mage excluded from cave interactions");
        l.Interact(3);Next(ui);Choice(ui,WinterContent.Missions[2][0].Correct);Feedback(ui,true,1,2,"FeedbackArchive");Call(l,"Close");
        while(!p.MissionComplete(2))p.AnswerMission(2,WinterContent.Missions[2][p.Steps[2]].Correct);
        l.Interact(4);Next(ui);for(int i=0;i<8;i++){SubmitActivity(l,i,i!=1);Next(ui);}
        Check(p.Complete&&p.BossScore==7,"Final still requires seven of eight");
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false))return;
        try
        {
            if(DateTime.UtcNow>=DateTime.Parse(SessionState.GetString(Key+"deadline","")).ToUniversalTime())throw new Exception("General feedback checks timed out");
            if(!EditorApplication.isPlaying)return;
            JourneySession.TestMode=true;JourneySession.SavePathOverride=Path.GetFullPath("Logs/SeasonChecks/general-feedback-save.json");Application.runInBackground=true;EditorApplication.QueuePlayerLoopUpdate();
            typeof(EditorApplication).GetMethod("SignalTick",BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public)?.Invoke(null,null);
            if(ready==0)ready=EditorApplication.timeSinceStartup+3;if(EditorApplication.timeSinceStartup<ready)return;
            if(!JourneySession.Instance)return;
            if(stage==0)Spring();else if(stage==1)Summer();else if(stage==2)Fall();else if(stage==3)Winter();
            else if(stage==4)
            {
                var l=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();if(l.Transitioning||!l.CertificateReward.Ready)return;
                Move(l,l.CertificateReward.Position+Vector2.down*.7f);Check(l.CertificateReward.TryCollect(),"Certificate pickup opens certificate");
                var ui=JourneySession.Instance.GetComponentInChildren<JourneyUI>();Check(JourneySession.Instance.Paused,"Certificate modal");
                ui.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="Return").onClick.Invoke();
            }
            else if(stage==5)
            {
                Check(SceneManager.GetActiveScene().name=="Intro","Certificate Return opens main menu scene");
                var ui=JourneySession.Instance.GetComponentInChildren<JourneyUI>();Check(ui.GetComponentsInChildren<UnityEngine.UI.Button>().Any(b=>b.name=="New Journey"),"Main menu is visible");Check(JourneySession.Instance.Data.finished,"Completed progress retained");
                JourneySession.Instance.ShowCertificate();JourneySession.Instance.HandleBack();
            }
            else if(stage==6)
            {
                Check(SceneManager.GetActiveScene().name=="Intro"&&!JourneySession.Instance.Paused,"Certificate Escape also returns to main menu");
                File.WriteAllText(Report,"PASS\n"+checks+" assertions: all-season Correct feedback and fixed totals, no courage/hearts on screen, artifact label/full-count fit, physical ridge boundaries and walk-in entry, no cave mage interaction, Activity 5 cycling note, certificate Return/Escape to menu with completion saved.");
                SessionState.SetBool(Key,false);EditorApplication.Exit(0);return;
            }
            stage++;ready=EditorApplication.timeSinceStartup+(stage==4?1:4);if(stage<4)JourneySession.Instance.LoadChapter(stage+1);
        }
        catch(Exception e){File.WriteAllText(Report,"FAIL\n"+e);Debug.LogException(e);SessionState.SetBool(Key,false);EditorApplication.Exit(1);}
    }
}
#endif
