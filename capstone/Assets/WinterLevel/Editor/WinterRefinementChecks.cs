#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AmbrosJourney.Journey;
using AmbrosJourney.Presentation;
using AmbrosJourney.Winter;
[InitializeOnLoad]
public static class WinterRefinementChecks
{
    const string Active="WinterRefinementChecks.Active";
    const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    static int stage,count;static double ready;static bool sawDust;static string error;
    static WinterRefinementChecks()
    {
        EditorApplication.update+=Tick;
        Application.logMessageReceived+=(m,s,t)=>{if(SessionState.GetBool(Active,false)&&(t==LogType.Error||t==LogType.Exception||t==LogType.Assert))error=m;};
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Isolate()
    {
        if(SessionState.GetBool(Active,false)){JourneySession.TestMode=true;JourneySession.SavePathOverride=Path.GetFullPath("Logs/WinterRefinements/save.json");}
    }
    public static void Run()
    {
        if(!Application.isBatchMode)throw new InvalidOperationException("Use a disposable batch-mode project.");
        Directory.CreateDirectory("Logs/WinterRefinements");Directory.CreateDirectory("Logs/SeasonChecks");
        File.WriteAllText("Logs/WinterRefinements/Results.txt","RUNNING");SessionState.SetBool(Active,true);
        SessionState.SetString(Active+"Deadline",DateTime.UtcNow.AddMinutes(4).ToString("O"));
        EditorSceneManager.OpenScene("Assets/WinterLevel/Winter.unity");EditorApplication.isPlaying=true;
    }
    static void Check(bool ok,string why){count++;if(!ok)throw new Exception(why);}
    static void Call(object obj,string name)=>obj.GetType().GetMethod(name,Hidden).Invoke(obj,null);
    static void Next(WinterLevel l)=>((Action)typeof(WinterLevel).GetField("next",Hidden).GetValue(l))();
    static void Move(WinterLevel l,Vector2 position){l.player.transform.position=position;l.player.GetComponent<Rigidbody2D>().position=position;l.player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
    static void Tick()
    {
        if(!SessionState.GetBool(Active,false))return;
        try
        {
            Check(error==null,error);Check(DateTime.UtcNow<DateTime.Parse(SessionState.GetString(Active+"Deadline","")).ToUniversalTime(),"Timed out at stage "+stage);
            Application.runInBackground=true;EditorApplication.QueuePlayerLoopUpdate();typeof(EditorApplication).GetMethod("SignalTick",BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public)?.Invoke(null,null);
            if(!EditorApplication.isPlaying)return;
            JourneySession.TestMode=true;JourneySession.SavePathOverride=Path.GetFullPath("Logs/WinterRefinements/save.json");
            if(ready==0)ready=EditorApplication.timeSinceStartup+2;if(EditorApplication.timeSinceStartup<ready)return;
            var session=JourneySession.Instance;var l=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();var sequence=UnityEngine.Object.FindAnyObjectByType<BossPortalSequence>();
            if(!l||!sequence)return;var ui=l.GetComponentInChildren<SeasonOverlay>();
            if(stage==0)
            {
                session.BeginTestProfile();session.Resume();var p=new WinterProgress{Introduced=true};
                for(int m=0;m<2;m++){p.ReadNote(m);foreach(var q in WinterContent.Missions[m])p.AnswerMission(m,q.Correct);}
                Check(!p.ReadCaveGuide()&&!p.CanEnterCave,"Cannot pre-read cave guide before Warden victory");l.RestoreProgress(p);
                Check(!l.caveEntrance.GetComponent<SpriteRenderer>(),"No portal sprite on the natural cave");
                Time.timeScale=12;stage=1;
            }
            else if(stage==1)
            {
                if(!l.Progress.CompassWarningShown)return;Time.timeScale=1;
                Check(l.IsDialogueOpen&&l.CurrentTitle.Contains("frozen"),"Actual snowfall opens frozen-arrow notice");Call(ui,"LateUpdate");
                Check(!ui.Arrow.gameObject.activeSelf&&ui.CompassObscured,"Arrow disappears when notice opens");
                SeasonIntegrationChecks.Capture("WinterFrozenArrowNotice");Next(l);Check(!l.IsDialogueOpen,"Notice continues normally");
                Time.timeScale=16;stage=2;
            }
            else if(stage==2)
            {
                if(Mathf.Repeat(Time.time,48)>12)return;Time.timeScale=1;
                Call(l,"RefreshWorld");Call(ui,"LateUpdate");Check(l.Progress.CompassWarningShown&&ui.CompassObscured&&!ui.Arrow.gameObject.activeSelf,"Arrow remains gone after the weather cycle ends");
                session.Data.assistedGuidance=true;Call(l,"RefreshWorld");Call(ui,"LateUpdate");Check(!ui.Arrow.gameObject.activeSelf,"Guidance toggle cannot revive an already frozen outdoor arrow");session.Data.assistedGuidance=false;
                var saved=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(l.Progress));l.RestoreProgress(saved);Call(l,"RefreshWorld");Call(ui,"LateUpdate");Check(!ui.Arrow.gameObject.activeSelf,"Frozen state survives checkpoint reload");
                Move(l,WinterWorld.WardenPosition+Vector2.down*2);l.Interact(8);Next(l);
                for(int i=0;i<5;i++){ui.Choices[WinterContent.Boss[i].Correct].onClick.Invoke();Next(l);}
                Check(l.Progress.WinterComplete&&!l.Progress.CaveEntryExplained,"Warden victory alone does not unlock first cave entry");Next(l);stage=3;
            }
            else if(stage==3)
            {
                Check(!sequence.Exit.Playing&&!l.caveEntrance.GetComponent<SpriteRenderer>(),"Winter never creates portal-building pixels or a portal overlay");
                if(sequence.Dissolve&&sequence.Dissolve.DetachedPixelCount>0)sawDust=true;
                if(!sequence.Exit.Ready||BossPortalSequence.Busy)return;Check(sawDust,"Warden still disintegrates");
                Move(l,WinterWorld.Entrance);Call(l,"LateUpdate");l.World.ChangeRegion(true);
                Check(!l.Progress.InCave&&!l.CanEnterCave,"Walking or region-change call cannot bypass Mage conversation");
                Check(l.Destination()==(Vector2)l.littleMage.transform.position,"Next objective points to Little Mage");
                Move(l,WinterWorld.CaveGuide);l.Interact(0);Check(l.IsDialogueOpen&&!l.Progress.CaveGuideRead,"Opening cave dialogue alone is insufficient");
                l.DismissPanel();Move(l,WinterWorld.Entrance);Call(l,"LateUpdate");Check(!l.Progress.InCave,"Escape does not finish the Mage conversation");
                Move(l,WinterWorld.CaveGuide);l.Interact(0);Next(l);Check(l.Progress.CaveGuideRead,"Continue finishes cave guidance");
                var saved=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(l.Progress));Check(saved.CaveGuideRead&&saved.CanEnterCave&&saved.Valid(),"Cave permission is saved");
                l.RestoreProgress(saved);Move(l,WinterWorld.Entrance+Vector2.down*1.6f);SeasonIntegrationChecks.Capture("WinterNaturalCaveAfterWarden");
                Move(l,WinterWorld.Entrance);stage=4;ready=EditorApplication.timeSinceStartup+.3;
            }
            else if(stage==4)
            {
                Check(l.Progress.InCave,"Walking into doorway after Mage conversation automatically enters cave");Call(l,"RefreshWorld");Call(ui,"LateUpdate");
                Check(!ui.CompassObscured&&ui.Arrow.gameObject.activeSelf,"Arrow returns inside Echo Chamber");SeasonIntegrationChecks.Capture("EchoChamberArrowRestored");
                Move(l,l.caveExit.position);l.World.ChangeRegion(false);stage=5;ready=EditorApplication.timeSinceStartup+.3;
            }
            else if(stage==5)
            {
                Call(l,"RefreshWorld");Call(ui,"LateUpdate");Check(!l.Progress.InCave&&ui.CompassObscured&&!ui.Arrow.gameObject.activeSelf,"Returning outdoors freezes the arrow again");
                var legacy=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(l.Progress));legacy.CaveGuideRead=false;
                Check(legacy.CaveDiscovered&&legacy.Valid()&&legacy.CanEnterCave,"Older saves which already visited the cave remain usable");
                Finish(true,"Frozen arrow hides immediately, survives clear weather/settings/reload, returns only in cave; Warden dissolves without a portal overlay/build; cave entry requires completed Mage dialogue, survives reload, supports old cave saves and remains automatic afterward.");
            }
        }
        catch(Exception e){Finish(false,e.ToString());}
    }
    static void Finish(bool passed,string message)
    {
        SessionState.SetBool(Active,false);Time.timeScale=1;File.WriteAllText("Logs/WinterRefinements/Results.txt",(passed?"PASS":"FAIL")+"\n"+message+"\nChecks: "+count);Debug.Log("WINTER_REFINEMENTS_"+(passed?"PASS: ":"FAIL: ")+message);EditorApplication.Exit(passed?0:1);
    }
}
#endif
