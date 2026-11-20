#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using AmbrosJourney.Journey;
using AmbrosJourney.Presentation;
using AmbrosJourney.Summer;
using AmbrosJourney.Fall;
using AmbrosJourney.Winter;
[InitializeOnLoad]
public static class BossExitChecks
{
    const string Key="BossExitChecks.";const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    static double next;static int checks;static bool sawDust,sawAssembly;static Vector2 rPosition;static int rActor;
    static readonly MethodInfo SignalTick=typeof(EditorApplication).GetMethod("SignalTick",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
    static BossExitChecks(){EditorApplication.update+=Tick;Application.logMessageReceived+=(m,s,t)=>{if(SessionState.GetBool(Key+"active",false)&&(t==LogType.Error||t==LogType.Exception||t==LogType.Assert))SessionState.SetString(Key+"error",m);};}
    public static void Run()
    {
        Directory.CreateDirectory("Logs/BossExitChecks");Directory.CreateDirectory("Logs/SeasonChecks");
        SessionState.SetBool(Key+"active",true);SessionState.SetInt(Key+"stage",0);SessionState.SetString(Key+"error","");SessionState.SetString(Key+"deadline",DateTime.UtcNow.AddMinutes(10).ToString("O"));
        EditorSceneManager.OpenScene("Assets/Scenes/Spring.unity");EditorApplication.isPlaying=true;
    }
    static void Check(bool ok,string message){checks++;if(!ok)throw new Exception(message);}
    static void Call(object obj,string method,params object[] args)=>obj.GetType().GetMethod(method,Flags).Invoke(obj,args);
    static void Set(object obj,string name,object value)=>obj.GetType().GetField(name,Flags).SetValue(obj,value);
    static void Next(object level){var callback=(Action)level.GetType().GetField("next",Flags).GetValue(level);Check(callback!=null,"Continue callback");callback();}
    static void Choice(MonoBehaviour level,int n)=>level.GetComponentInChildren<SeasonOverlay>().Choices[n].onClick.Invoke();
    static void Advance(int s,float delay=.1f){SessionState.SetInt(Key+"stage",s);next=EditorApplication.timeSinceStartup+delay;Debug.Log("BOSS_EXIT_STAGE: "+s);}
    static T Find<T>()where T:UnityEngine.Object=>UnityEngine.Object.FindAnyObjectByType<T>();
    static void Move(Vector2 p){var player=JourneySession.Instance.Player();player.position=p;player.GetComponent<Rigidbody2D>().position=p;player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
    static void RDown(int actor){rActor=actor;rPosition=JourneySession.Instance.Player().position;InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(UnityEngine.InputSystem.Key.R));InputSystem.Update();var p=JourneySession.Instance.Player();var controller=p.GetComponents<MonoBehaviour>().First(c=>c is SpringPlayer||c is SummerActor||c is FallActor||c is WinterActor);Call(controller,"Update");}
    static void RUp(){InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.Update();Check(Vector2.Distance(rPosition,JourneySession.Instance.Player().position)<.01f,"R does not reset actor "+rActor);}
    static void Tick()
    {
        if(!SessionState.GetBool(Key+"active",false))return;
        try
        {
            Check(SessionState.GetString(Key+"error","")=="",SessionState.GetString(Key+"error",""));
            Check(DateTime.UtcNow<DateTime.Parse(SessionState.GetString(Key+"deadline","")).ToUniversalTime(),"Timed out at stage "+SessionState.GetInt(Key+"stage",0));
            Application.runInBackground=true;EditorApplication.QueuePlayerLoopUpdate();SignalTick?.Invoke(null,null);
            if(!EditorApplication.isPlaying)return;JourneySession.TestMode=true;JourneySession.SavePathOverride=Path.GetFullPath("Logs/BossExitChecks/save.json");
            if(next==0)next=EditorApplication.timeSinceStartup+3;if(EditorApplication.timeSinceStartup<next)return;
            var session=JourneySession.Instance;var seq=Find<BossPortalSequence>();int stage=SessionState.GetInt(Key+"stage",0);
            if(stage==0)
            {
                if(!seq)return;session.BeginTestProfile();session.Resume();var g=Find<SpringGuide>();
                Check(!seq.Exit.Ready&&!seq.Exit.Portal.gameObject.activeSelf,"Spring portal hidden before victory");
                g.RestoreProgress(new SpringState{lessonComplete=true,explored=true,missions=3,cleared=new[]{true,true,true},studied=new[]{true,true,true},trials=new[]{2,2,2}});
                Move(new Vector2(0,-25));RDown(1);Advance(1);
            }
            else if(stage==1)
            {
                RUp();var g=Find<SpringGuide>();Set(g,"target",3);typeof(SpringGuide).GetProperty("DialogueOpen").SetValue(g,true);Set(g,"bossResult",false);Set(g,"feedback","");Set(g,"bossQuestion",0);
                foreach(int a in new[]{0,0,1,1,0}){Call(g,"Answer",a);Call(g,"Continue");}
                Check(!g.SpringComplete&&g.BossScore==3&&!seq.Exit.Ready,"Spring 3/5 keeps boss and gate locked");Call(g,"Continue");
                foreach(int a in new[]{0,0,1,0,0}){Call(g,"Answer",a);Call(g,"Continue");}
                Check(g.SpringComplete&&g.BossScore==4,"Spring 4/5 unchanged");Check(!seq.Exit.Ready,"Passing does not open portal early");
                Call(g,"Continue");sawDust=sawAssembly=false;Advance(2);
            }
            else if(stage==2||stage==6||stage==10||stage==14)
            {
                if(seq.Dissolve&&seq.Dissolve.AnimationElapsed<1.30f&&seq.Dissolve.DetachedPixelCount>0){sawDust=true;Check(!seq.Exit.Ready,"Entry blocked during boss crumble");Check(!seq.Exit.Portal.gameObject.activeSelf,"Portal hidden during crumble");}
                if(seq.Exit.Playing){sawAssembly=true;Check(JourneySession.Blocked&&!seq.Exit.Ready,"Movement/entry blocked during portal assembly");Check(!seq.Exit.Portal.gameObject.activeSelf,"Real portal waits until build finishes");}
                if(!seq.Exit.Ready)return;
                Check(sawDust&&(stage==14?!sawAssembly:sawAssembly),"Boss dissolve and the correct seasonal exit effect for "+SceneManager.GetActiveScene().name);Check(!JourneySession.Blocked,"Controls restored after reveal");
                Check(BossPortalSequence.AllowsEntry(seq.Exit.Portal),"Portal entry permitted only when ready");
                if(stage==2)Check(!Find<SpringGuide>().ExitBarrier.activeSelf,"Spring physical barrier removed");

                Move(seq.Exit.Portal.position+Vector3.down*1.5f);Advance(stage+1);
            }
            else if(stage==3||stage==7||stage==11){SeasonIntegrationChecks.Capture(SceneManager.GetActiveScene().name+"PortalReady");Move(seq.Exit.Portal.position);Advance(stage+1,2);}
            else if(stage==4)
            {
                if(SceneManager.GetActiveScene().name!="Summer"||!seq)return;var l=Find<SummerLevel>();Check(l&&l.IsDialogueOpen,"Summer introduction preserved");
                Check(!seq.Exit.Ready&&!seq.Exit.Portal.gameObject.activeSelf&&l.shields.Length==0,"Summer starts without portal or flags");
                l.RestoreProgress(new SummerProgress{Introduced=true,GuideArrived=true,Wards=new[]{true,true,true},WorkshopQuestion=2,HintChosen=true,PatternStep=3,GardenQuestion=1,PatternComplete=true,PrivacyComplete=true,RemovedPrivateDetails=new[]{true,true,true}});
                Move(new Vector2(0,-20));RDown(2);Advance(5);
            }
            else if(stage==5)
            {
                RUp();var l=Find<SummerLevel>();l.Interact(4);Next(l);for(int i=0;i<5;i++){Choice(l,SummerContent.Boss[i].Correct);Next(l);}Check(l.Progress.Complete&&l.IsDialogueOpen,"Summer result message shown");Check(!seq.Exit.Ready,"Summer result does not reveal portal");Next(l);sawDust=sawAssembly=false;Advance(6);
            }
            else if(stage==8)
            {
                if(SceneManager.GetActiveScene().name!="Fall"||!seq)return;var l=Find<FallLevel>();Check(l&&l.IsDialogueOpen,"Fall introduction preserved");Check(!seq.Exit.Ready&&l.shields.Length==0,"Fall portal hidden and no flags");
                l.RestoreProgress(new FallProgress{Introduced=true,GuideArrived=true,Studied=new[]{true,true,true},Steps=FallContent.Missions.Select(q=>q.Length).ToArray()});Move(new Vector2(0,-38));RDown(3);Advance(9);
            }
            else if(stage==9)
            {
                RUp();var l=Find<FallLevel>();l.Interact(4);Next(l);for(int i=0;i<5;i++){Choice(l,FallContent.Boss[i].Correct);Next(l);}Check(l.Progress.Complete,"Fall quiz passed");Next(l);sawDust=sawAssembly=false;Advance(10);
            }
            else if(stage==12)
            {
                if(SceneManager.GetActiveScene().name!="Winter"||!seq)return;var l=Find<WinterLevel>();Check(l&&!seq.Exit.Ready&&!seq.Exit.Portal.gameObject.activeSelf&&l.shields.Length==0,"Winter starts without portal or flags");
                l.RestoreProgress(new WinterProgress{Introduced=true,CompassWarningShown=true,Studied=new[]{true,true,false},Steps=new[]{WinterContent.Missions[0].Length,WinterContent.Missions[1].Length,0}});Move(WinterWorld.WardenPosition+Vector2.down*2);RDown(4);Advance(13);
            }
            else if(stage==13)
            {
                RUp();var l=Find<WinterLevel>();l.Interact(8);Next(l);for(int i=0;i<5;i++){Choice(l,WinterContent.Boss[i].Correct);Next(l);}Check(l.Progress.WinterComplete,"Winter quiz passed");Next(l);sawDust=sawAssembly=false;Advance(14);
            }
            else if(stage==15)
            {
                var l=Find<WinterLevel>();Check(!l.caveEntrance.GetComponent<SpriteRenderer>(),"Natural cave has no portal overlay");
                Move(l.caveEntrance.position);Call(l,"LateUpdate");Check(!l.Progress.InCave&&!l.CanEnterCave,"Cave rejects entry before talking to Mage");
                Move(l.littleMage.transform.position);l.Interact(0);Check(!l.Progress.CaveEntryExplained,"Opening the message alone does not finish it");Next(l);
                Check(l.Progress.CaveGuideRead,"Finishing the Mage conversation unlocks cave");
                SeasonIntegrationChecks.Capture("WinterPortalReady");Move(seq.Exit.Portal.position);Advance(16,1);
            }
            else if(stage==16)
            {
                var l=Find<WinterLevel>();Check(l.Progress.InCave,"Winter portal automatically enters cave");Check(!seq.Home.Ready&&!seq.Home.Portal.gameObject.activeSelf,"No home portal before certificate exit");
                // Finish the existing final activities through grading data; preserve the existing final reward.
                l.Progress.Studied[2]=true;l.Progress.Steps[2]=WinterContent.Missions[2].Length;l.Progress.BeginBoss();
                Move(WinterWorld.BossPosition+Vector2.down*4);l.Interact(4);Next(l);
                foreach(var q in WinterContent.Final)
                {
                    var panel=l.GetComponent<FinalActivityPanel>();Check(panel&&panel.IsOpen,"Final activity is interactive");
                    if(q.Kind==FinalActivityKind.Arrange){for(int i=0;i<q.Expected.Length;i++)panel.Drop(q.Expected[i],i);}
                    else if(q.Kind==FinalActivityKind.Match){for(int i=0;i<q.Expected.Length;i++)for(int j=0;j<=q.Expected[i];j++)panel.Select(i);}
                    else foreach(int c in q.Expected)panel.Select(c);
                    panel.Submit();Next(l);
                }
                Check(l.Progress.Complete&&l.Transitioning,"Final pass starts original reward crumble");sawDust=false;Advance(17);
            }
            else if(stage==17)
            {
                var l=Find<WinterLevel>();var reward=l.CertificateReward;
                if(reward.DetachedPixelCount>0)sawDust=true;
                if(!reward.Ready||l.Transitioning)return;
                Check(sawDust,"Final Keeper still uses progressive dissolve");Check(!seq.Home.Ready,"Home portal waits for certificate Exit");
                Move(reward.Position);Check(reward.TryCollect(),"Certificate pickup opens reward");Check(session.Paused&&!session.Data.certificatePending,"Certificate stays modal and claimed");
                var ui=session.GetComponentInChildren<JourneyUI>();Check(ui.GetComponentsInChildren<UnityEngine.UI.Button>().Any(b=>b.GetComponentInChildren<TMPro.TMP_Text>().text=="Download PDF certificate"),"PDF download button retained");
                JourneyCertificate.OutputDirectoryOverride=Path.GetFullPath("Logs/BossExitChecks");JourneyCertificate.Export(session.Data,ui);Advance(18,2);
            }
            else if(stage==18)
            {
                Check(File.Exists(JourneyCertificate.LastExportPath),"Certificate PDF exports");
                var ui=session.GetComponentInChildren<JourneyUI>();ui.GetComponentsInChildren<UnityEngine.UI.Button>().First(b=>b.GetComponentInChildren<TMPro.TMP_Text>().text=="Exit").onClick.Invoke();
                Check(!session.Paused&&Find<WinterLevel>().Progress.InCave&&SceneManager.GetActiveScene().name=="Winter","Exit resumes cave without loading menu");
                Check(BossPortalSequence.Busy&&!seq.Home.Ready&&session.Data.homePortalOpened,"Exit starts saved home portal assembly");sawAssembly=false;Advance(19);
            }
            else if(stage==19)
            {
                if(seq.Home.Playing){sawAssembly=true;Check(!seq.Home.Portal.gameObject.activeSelf&&!seq.Home.Ready,"Home portal disabled throughout animation");}
                if(!seq.Home.Ready)return;Check(sawAssembly&&!JourneySession.Blocked,"Home portal assembles then restores control");
                Advance(190,.4f);
            }
            else if(stage==190)
            {
                SeasonIntegrationChecks.Capture("HomePortalReady");session.ShowCertificate();session.HandleBack();Check(!session.Paused&&seq.Home.Ready&&!BossPortalSequence.Busy,"Certificate Escape resumes cave without replay");
                session.Save();session.ReturnToMenu();Advance(20,2);
            }
            else if(stage==20){Check(SceneManager.GetActiveScene().name=="Intro","Save and return reaches menu");session.LoadChapter(4);Advance(201,3);}
            else if(stage==201)
            {
                if(!seq||!seq.Home)return;Check(seq.Exit.Ready&&seq.Home.Ready&&!BossPortalSequence.Busy,"Completed seasonal and home portals restore instantly");
                Check(!Find<WinterLevel>().winterGuardian.gameObject.activeSelf,"Defeated Warden remains gone after reload");
                Check(!Find<WinterLevel>().guardian.GetComponent<SpriteRenderer>().enabled,"Defeated final boss remains gone after reload");
                Move(seq.Home.Portal.position+Vector3.down*3);Advance(21);
            }
            else if(stage==21){Move(seq.Home.Portal.position);Advance(22,2);}
            else if(stage==22)
            {
                Check(SceneManager.GetActiveScene().name=="Intro","Walking through home portal loads main menu");session.ShowCertificate();session.HandleBack();Check(SceneManager.GetActiveScene().name=="Intro"&&!session.Paused,"Certificate from main menu returns to menu");
                Finish(true,"All four seasonal boss quizzes, failure/pass thresholds, progressive boss dissolves, pixel portal builds, real automatic transitions, R removal in all controllers, final interactive quiz, certificate PDF and cave Exit/Escape, saved completed portals and home-to-menu ending passed.");
            }
        }
        catch(Exception e){Finish(false,e.ToString());}
    }
    static void Finish(bool pass,string message)
    {
        SessionState.SetBool(Key+"active",false);File.WriteAllText("Logs/BossExitChecks/Results.txt",(pass?"PASS":"FAIL")+"\n"+message+"\nChecks: "+checks);Debug.Log("BOSS_EXIT_"+(pass?"PASS":"FAIL")+": "+message);EditorApplication.Exit(pass?0:1);
    }
}
#endif
