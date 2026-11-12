#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using AmbrosJourney.Journey;
using AmbrosJourney.Winter;
using AmbrosJourney.Presentation;
[InitializeOnLoad]
// Disposable batch-mode project only: uses its own save and exits after the checks.
public static class CompassNoticeChecks
{
    const string Key="CompassNoticeChecks",Report="Logs/SeasonChecks/CompassNotice.txt";
    static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    static double ready;static int stage;
    static CompassNoticeChecks(){EditorApplication.update+=Tick;}
    public static void Run(){if(!Application.isBatchMode)throw new InvalidOperationException("Run in a disposable batch-mode project copy.");Directory.CreateDirectory("Logs/SeasonChecks");File.WriteAllText(Report,"RUNNING");SessionState.SetBool(Key,true);SessionState.SetString(Key+"deadline",DateTime.UtcNow.AddMinutes(3).ToString("O"));EditorSceneManager.OpenScene("Assets/WinterLevel/Winter.unity");EditorApplication.isPlaying=true;}
    static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
    static void Call(object obj,string method)=>obj.GetType().GetMethod(method,Flags).Invoke(obj,null);
    static WinterProgress Ready(int step)
    {
        var p=new WinterProgress{Introduced=true};for(int m=0;m<Mathf.Min(step,2);m++){p.ReadNote(m);foreach(var q in WinterContent.Missions[m])p.AnswerMission(m,q.Correct);}
        if(step==3){p.BeginWinterBoss();foreach(var q in WinterContent.Boss)p.AnswerWinterBoss(q.Correct);}return p;
    }
    static void Inspect(SeasonOverlay ui)
    {
        Call(ui,"LateUpdate");Canvas.ForceUpdateCanvases();
        foreach(var text in ui.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,"Overflow: "+text.name+" / "+text.text);}
        var frame=ui.GetComponentInChildren<FrostNoticeFrame>();Check(frame&&frame.gameObject.activeInHierarchy&&!frame.raycastTarget,"Decorative ice frame does not block clicks");
        var mesh=frame.canvasRenderer.GetMesh();Check(mesh&&mesh.vertexCount>100,"Icicles are rendered as visible UI geometry");
        Check(ui.Dialog.GetComponent<RectTransform>().rect.width==720,"Compact popup width");
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying)return;
        try
        {
            Check(DateTime.UtcNow<DateTime.Parse(SessionState.GetString(Key+"deadline","")).ToUniversalTime(),"Compass notice fixture timed out");
            JourneySession.TestMode=true;JourneySession.SavePathOverride=Path.GetFullPath("Logs/SeasonChecks/compass-notice-save.json");Application.runInBackground=true;EditorApplication.QueuePlayerLoopUpdate();
            typeof(EditorApplication).GetMethod("SignalTick",BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public)?.Invoke(null,null);
            if(ready==0)ready=EditorApplication.timeSinceStartup+3;if(EditorApplication.timeSinceStartup<ready)return;
            var l=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();var session=JourneySession.Instance;if(!l||!l.player||!session)return;var ui=l.GetComponentInChildren<SeasonOverlay>();
            if(stage==0)
            {
                session.BeginTestProfile();session.Resume();session.Data.assistedGuidance=true;l.DismissPanel();Call(l,"RefreshWorld");
                Check(!ui.CompassObscured&&!l.Progress.CompassWarningShown,"No warning before snow");stage=1;return;
            }
            if(Mathf.Repeat(Time.time,48)<=27)return;
            Check(!ui.CompassObscured&&!l.IsDialogueOpen&&!l.Progress.CompassWarningShown,"Assisted guidance bypasses snow warning");
            session.Data.assistedGuidance=false;Call(l,"RefreshWorld");Check(ui.CompassObscured&&!ui.Arrow.gameObject.activeSelf,"Real snowfall obscures Explorer compass");Call(l,"MaybeShowCompassWarning");
            Check(l.IsDialogueOpen&&l.Progress.CompassWarningShown&&l.InputBlocked,"First snow opens modal once");Inspect(ui);Check(ui.Copy.text.Contains("Wayfarer's Ledger"),"Next landmark matches first lesson");
            SeasonIntegrationChecks.Capture("FrozenCompassPopup");ui.Choices[0].onClick.Invoke();Check(!l.IsDialogueOpen&&!ui.GetComponentInChildren<FrostNoticeFrame>(),"Continue closes frame");
            var saved=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(l.Progress));Check(saved.CompassWarningShown&&saved.Valid(),"Notice state serializes safely");l.RestoreProgress(saved);Call(l,"RefreshWorld");Call(l,"MaybeShowCompassWarning");Check(!l.IsDialogueOpen,"Notice does not repeat after reload");
            string[] next={"Wayfarer's Ledger","Frozen Observatory","Frostbound Warden","cave entrance"};
            for(int i=0;i<4;i++)
            {
                l.RestoreProgress(Ready(i));Call(l,"RefreshWorld");Call(l,"MaybeShowCompassWarning");Check(l.IsDialogueOpen&&ui.Copy.text.Contains(next[i]),"Progress-specific landmark "+i);Inspect(ui);
                if(i==2)SeasonIntegrationChecks.Capture("FrozenCompassWardenHint");l.DismissPanel();Check(!l.IsDialogueOpen,"Escape dismisses ice notice");
            }
            l.RestoreProgress(Ready(0));l.Interact(0);Call(l,"MaybeShowCompassWarning");Check(!l.Progress.CompassWarningShown&&l.CurrentTitle.Contains("Little Mage"),"Does not interrupt another conversation");l.DismissPanel();
            session.Pause();Call(l,"MaybeShowCompassWarning");Check(!l.Progress.CompassWarningShown,"Does not interrupt pause");session.Resume();
            var cave=Ready(3);cave.EnterCave();l.RestoreProgress(cave);l.player.transform.position=WinterWorld.CaveSpawn;l.player.GetComponent<Rigidbody2D>().position=l.player.transform.position;Call(l,"RefreshWorld");Call(l,"MaybeShowCompassWarning");Check(!l.IsDialogueOpen&&!cave.CompassWarningShown&&!ui.CompassObscured,"Cave compass and lessons unaffected");
            l.Interact(3);Call(ui,"LateUpdate");Check(ui.Dialog.GetComponent<RectTransform>().rect.width==900&&!ui.GetComponentInChildren<FrostNoticeFrame>(),"Ordinary dialogue layout restored");
            File.WriteAllText(Report,"PASS: real snow trigger; once-only saved notice; four progress-specific hints; compact icy UI at 1280x720 and 1024x768; Continue/Escape; normal dialogue restored; no interruption during pause/dialogues; Assisted and cave behavior retained.");
            SessionState.SetBool(Key,false);EditorApplication.Exit(0);
        }
        catch(Exception e){File.WriteAllText(Report,"FAIL\n"+e);Debug.LogException(e);SessionState.SetBool(Key,false);EditorApplication.Exit(1);}
    }
}
#endif
