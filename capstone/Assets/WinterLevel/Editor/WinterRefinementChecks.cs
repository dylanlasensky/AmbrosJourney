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
                Time.timeScale=1;Call(l,"RefreshWorld");Call(ui,"LateUpdate");
                Check(!l.IsDialogueOpen&&!ui.CompassObscured&&ui.Arrow.gameObject.activeSelf,"Winter keeps guidance visible without a frozen notice");
                var saved=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(l.Progress));saved.CompassWarningShown=true;
                l.RestoreProgress(saved);Call(l,"RefreshWorld");Call(ui,"LateUpdate");
                Check(!ui.CompassObscured&&ui.Arrow.gameObject.activeSelf,"Old frozen-arrow saves regain guidance");
                Check(Mathf.Abs(WinterWorld.WardenPosition.x-WinterWorld.Entrance.x)<.01f&&WinterWorld.WardenPosition.y<WinterWorld.Entrance.y,"Warden guards the front of the doorway");
                Check(!l.runes[0].sprite&&!l.runes[1].sprite,"Outdoor note crystals removed");
                SeasonIntegrationChecks.Capture("WinterPermanentGuidance");
                Move(l,WinterWorld.WardenPosition+Vector2.down*2);l.Interact(8);Next(l);
                for(int i=0;i<5;i++){ui.Choices[WinterContent.Boss[i].Correct].onClick.Invoke();Next(l);}
                Check(l.Progress.WinterComplete&&!l.Progress.CaveEntryExplained,"Cave tip waits while victory message is open");Next(l);stage=3;
            }
            else if(stage==3)
            {
                Check(!sequence.Exit.Playing&&!l.caveEntrance.GetComponent<SpriteRenderer>(),"Winter never creates portal-building pixels or a portal overlay");
                if(sequence.Dissolve&&sequence.Dissolve.DetachedPixelCount>0)sawDust=true;
                if(!sequence.Exit.Ready||BossPortalSequence.Busy)return;Check(sawDust,"Warden still disintegrates");
                Check(l.Progress.CaveGuideRead&&l.Progress.CanEnterCave&&!l.Progress.InCave&&!l.InputBlocked,"Victory closes into passive cave guidance without a required NPC stop");
                Check(l.Destination()==(Vector2)l.caveEntrance.position,"Next objective points straight to the cave entrance");
                var beforeGuide=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(l.Progress));beforeGuide.CaveGuideRead=false;
                l.RestoreProgress(beforeGuide);Call(l,"OfferCaveGuidance");Check(l.Progress.CaveGuideRead&&!l.IsDialogueOpen,"Older completed-Warden checkpoint receives automatic cave guidance");
                var saved=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(l.Progress));Check(saved.CaveGuideRead&&saved.CanEnterCave&&saved.Valid(),"Cave permission is saved");
                l.RestoreProgress(saved);Move(l,WinterWorld.Entrance+Vector2.down*1.6f);SeasonIntegrationChecks.Capture("WinterNaturalCaveAfterWarden");
                Move(l,WinterWorld.Entrance);stage=4;ready=EditorApplication.timeSinceStartup+.3;
            }
            else if(stage==4)
            {
                Check(l.Progress.InCave,"Walking into doorway after passive guidance automatically enters cave");Call(l,"RefreshWorld");Call(ui,"LateUpdate");
                Check(!ui.CompassObscured&&ui.Arrow.gameObject.activeSelf,"Arrow stays visible inside Echo Cavern");
                var visibility=l.World.GetComponent<CaveVisibility>();Call(visibility,"LateUpdate");
                Check(visibility.IsVisible&&visibility.LanternCount==WinterWorld.CaveLanterns.Length,"Cave veil and all lantern pools activate");
                Check(visibility.Reveals(l.player.transform.position)&&!visibility.Reveals(new Vector2(140,-25)),"Player circle reveals nearby floor but conceals distant cavern");
                Check(visibility.Reveals(WinterWorld.CaveLanterns[0]),"Lanterns reveal their immediate surroundings");
                SeasonIntegrationChecks.Capture("EchoCavernLanternLight");
                Check(ui.CaveLighting==visibility,"Winter labels use the same lighting mask as the scene");
                var beforeBoss=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(l.Progress));
                beforeBoss.ReadNote(2);foreach(var q in WinterContent.Missions[2])beforeBoss.AnswerMission(2,q.Correct);l.RestoreProgress(beforeBoss);
                Move(l,new Vector2(134,18));Call(visibility,"LateUpdate");
                Check(visibility.ChamberLight==0&&!visibility.Reveals(new Vector2(124,31))&&visibility.LabelRange==3.8f,"Approach tunnel keeps the distant room dark and labels local");
                Camera.main.transform.position=new Vector3(134,18,-10);SeasonIntegrationChecks.Capture("EchoChamberApproach");
                Move(l,new Vector2(134,20));Check(visibility.ChamberLight==0,"Doorway starts its fade at the chamber floor");
                Move(l,new Vector2(134,20.625f));Check(Mathf.Abs(visibility.ChamberLight-.5f)<.001f,"Doorway midpoint is half lit");
                Move(l,new Vector2(134,22));Call(visibility,"LateUpdate");
                Check(visibility.ChamberLight==1&&visibility.LabelRange==8,"Inside the room, lighting and normal interaction-label range return");
                Check(visibility.Reveals(new Vector2(124,31))&&visibility.Reveals(new Vector2(145,32))&&visibility.Reveals(l.guardian.transform.position),"Whole boss chamber and Keeper are visible");
                Check(!visibility.Reveals(new Vector2(134,12))&&!visibility.Reveals(new Vector2(104,10)),"Bright chamber does not reveal tunnels or archive");
                var checkpoint=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(l.Progress));var position=l.player.transform.position;l.RestoreProgress(checkpoint);
                Check(l.player.transform.position==position&&visibility.ChamberLight==1,"Reloading an in-room checkpoint restores light without moving the player");
                Time.timeScale=0;Call(visibility,"LateUpdate");Check(visibility.ChamberLight==1,"Paused chamber remains lit");Time.timeScale=1;
                Camera.main.transform.position=new Vector3(134.5f,27,-10);Call(Camera.main.GetComponent<WinterCamera>(),"LateUpdate");
                var bossBounds=l.guardian.GetComponent<SpriteRenderer>().bounds;
                Check(Camera.main.WorldToViewportPoint(bossBounds.min).y>0&&Camera.main.WorldToViewportPoint(bossBounds.max).y<.92f,"Chamber framing fits the Keeper including overhead label clearance");
                SeasonIntegrationChecks.Capture("EchoChamberLit");
                Check(session.Save(),"Save boss-room checkpoint to isolated test storage");
                var disk=JourneyStorage.Read(JourneySession.SavePath,JsonUtility.FromJson<JourneyData>,d=>d.Valid());
                Check(disk!=null&&disk.chapter==4&&disk.chapters[3].inCave&&disk.chapters[3].x==position.x&&disk.chapters[3].y==position.y,"Disk checkpoint retains the exact lit-room position");
                var diskProgress=JsonUtility.FromJson<WinterProgress>(disk.chapters[3].progress);Check(diskProgress.Valid(),"Saved Winter progress passes its validator");
                File.WriteAllText("Logs/WinterRefinements/chamber-profile.json",JsonUtility.ToJson(disk));
                Move(l,new Vector2(134,12));l.RestoreProgress(diskProgress);Move(l,new Vector2(disk.chapters[3].x,disk.chapters[3].y));Call(visibility,"LateUpdate");
                Check(visibility.ChamberLight==1,"Restoring the disk checkpoint immediately illuminates the chamber");
                l.Interact(4);Next(l);Check(l.IsDialogueOpen,"Final quiz opens in the illuminated room");SeasonIntegrationChecks.Capture("EchoChamberQuiz");l.DismissPanel();
                var failed=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(checkpoint));failed.BossStarted=failed.BossFinished=true;failed.BossQuestion=8;failed.BossScore=0;l.RestoreProgress(failed);
                Check(!l.Progress.Complete&&visibility.ChamberLight==1,"A failed quiz does not darken the occupied room");
                Move(l,new Vector2(134,12));Check(visibility.ChamberLight==0,"Leaving before victory restores exploration darkness");
                var won=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(failed));won.BossScore=8;won.Complete=true;l.RestoreProgress(won);Call(visibility,"LateUpdate");
                Check(visibility.ChamberLight==1&&visibility.Reveals(new Vector2(124,31))&&!visibility.Reveals(new Vector2(134,3)),"Victory keeps only the boss chamber illuminated even from a tunnel checkpoint");
                Check(visibility.LabelRange==3.8f,"Completed-room light does not expand tunnel label range");
                Move(l,new Vector2(134,23));Camera.main.transform.position=new Vector3(134.5f,27,-10);SeasonIntegrationChecks.Capture("EchoChamberVictory");
                l.RestoreProgress(checkpoint);Move(l,l.caveExit.position);l.World.ChangeRegion(false);stage=5;ready=EditorApplication.timeSinceStartup+.3;
            }
            else if(stage==5)
            {
                Call(l,"RefreshWorld");Call(ui,"LateUpdate");Check(!l.Progress.InCave&&!ui.CompassObscured&&ui.Arrow.gameObject.activeSelf,"Outdoor guidance remains visible after exiting cave");var visibility=l.World.GetComponent<CaveVisibility>();Call(visibility,"LateUpdate");Check(!visibility.IsVisible,"Cave darkness switches off outside");
                var legacy=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(l.Progress));legacy.CaveGuideRead=false;
                Check(legacy.CaveDiscovered&&legacy.Valid()&&legacy.CanEnterCave,"Older saves which already visited the cave remain usable");
                Finish(true,"Permanent guidance including old saves; centered Warden; no outdoor note crystals; player visibility circle and six lantern pools; localized boss chamber fade, full room visibility, matching labels, pause and checkpoint restore, failed quiz and persistent victory light; Warden dissolves without a portal overlay/build; cave guidance unlocks automatically after victory feedback, survives reload, supports older completed-Warden saves and permits automatic doorway entry.");
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
