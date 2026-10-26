#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using TMPro;
using AmbrosJourney.Presentation;
using AmbrosJourney.Winter;
namespace AmbrosJourney.Journey.Editor
{
    public static class JourneyPlayChecks
    {
        static Vector2 savedPosition;
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static void Check(bool condition,string why){if(!condition)throw new Exception("JOURNEY CHECK: "+why);}
        static void Call(object target,string name)=>target.GetType().GetMethod(name,Private).Invoke(target,null);
        static JourneyUI UI=>JourneySession.Instance.GetComponentInChildren<JourneyUI>();
        static void Inspect()
        {
            Canvas.ForceUpdateCanvases();foreach(var t in UI.GetComponentsInChildren<TextMeshProUGUI>()){t.ForceMeshUpdate();Check(!t.isTextOverflowing,"UI overflow: "+t.name+" / "+t.text);}
        }
        public static bool Step(int stage)
        {
            var session=JourneySession.Instance;Check(session,"Session exists");
            if(stage==0)
            {
                var winter=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();Check(winter.Progress.Complete&&winter.Progress.WinterComplete,"Both Winter bosses complete");
                var navigation=new SeasonNavigation(WinterWorld.Min,WinterWorld.Max);int count=SeasonNavigation.PhysicsQueryCount;var watch=System.Diagnostics.Stopwatch.StartNew();navigation.Next(WinterLevel.Spawn,WinterWorld.Entrance);watch.Stop();int routeQueries=SeasonNavigation.PhysicsQueryCount-count;count=SeasonNavigation.PhysicsQueryCount;for(int i=0;i<100;i++)navigation.Next(WinterLevel.Spawn,WinterWorld.Entrance);Check(SeasonNavigation.PhysicsQueryCount==count,"Compass samples are cached within the update interval");Debug.Log("JOURNEY_ROUTE_PROFILE: "+watch.Elapsed.TotalMilliseconds.ToString("F2")+" ms, "+routeQueries+" physics queries; 100 cached calls add zero queries");
                JourneySession.ApplyPerformance();double refresh=Screen.currentResolution.refreshRateRatio.value;double limit=QualitySettings.vSyncCount>0?refresh/QualitySettings.vSyncCount:Application.targetFrameRate;Check(limit<=60.01&&!Application.runInBackground,"Rendering limited to at most 60 FPS; background execution off");Application.runInBackground=true;
                savedPosition=winter.player.transform.position;session.Pause();Inspect();Check(Time.timeScale==0&&AudioListener.pause&&winter.InputBlocked,"Pause freezes simulation, audio and input");
                Check(session.Save(),"Save writes");Check(File.Exists(JourneySession.SavePath),"Save file exists");SeasonIntegrationChecks.Capture("PauseMenu");
                session.Resume();winter.Interact(8);Check(winter.IsDialogueOpen,"Conversation opens for pause regression");session.Pause();InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.Escape));InputSystem.Update();Call(session,"Update");Call(winter,"Update");Check(!session.Paused&&winter.IsDialogueOpen,"Esc resumes without also closing the conversation");InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.Update();Call(winter,"Close");session.ReturnToMenu();
            }
            else if(stage==1)
            {
                Check(SceneManager.GetActiveScene().name=="Intro","Return to menu");Inspect();SeasonIntegrationChecks.Capture("MainMenu");
                Check(UI.transform.Find("Blossom valley painting").GetComponent<UnityEngine.UI.RawImage>().texture,"Menu painting imported");
                Check(UI.transform.Find("Saved season").GetComponent<TMP_Text>().text=="Winter · Frostveil","Menu shows actual saved chapter");
                Click("Settings");Inspect();bool assisted=session.Data.assistedGuidance;Click("Guidance: "+(assisted?"Assisted":"Explorer"));Check(session.Data.assistedGuidance!=assisted,"Settings toggle guidance");
                var disk=JsonUtility.FromJson<JourneyData>(File.ReadAllText(JourneySession.SavePath));Check(disk.assistedGuidance==session.Data.assistedGuidance,"Menu settings persist to save");
                Click("Guidance: "+(!assisted?"Assisted":"Explorer"));SeasonIntegrationChecks.Capture("MenuSettings");Click("Back");
                Click("New Journey");Check(session.Data.started,"New Journey asks before discarding a save");Click("Keep my progress");Check(File.Exists(JourneySession.SavePath),"Cancel reset retains save");
                session.Data.finished=true;Call(session,"ShowMenu");Inspect();SeasonIntegrationChecks.Capture("CompletedJourneyMenu");
                var quit=UI.transform.Find("Quit").GetComponent<RectTransform>();var certificate=UI.transform.Find("View Certificate").GetComponent<RectTransform>();Check(-certificate.anchoredPosition.y>=-quit.anchoredPosition.y+quit.rect.height,"Completed menu actions do not overlap");
                session.Data.finished=false;Call(session,"ShowMenu");
                Click("Continue");
            }
            else if(stage==2)
            {
                var winter=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();Check(winter&&winter.Progress.Complete&&winter.Progress.WinterComplete,"Continue retains bosses");Check(Vector2.Distance(winter.player.transform.position,savedPosition)<.01f,"Exact outdoor position restored");
                Check(Time.timeScale==1&&!AudioListener.pause,"Continue unpauses");session.LoadChapter(1);
            }
            else if(stage>=3&&stage<=6)
            {
                int chapter=stage-2;Check(JourneySession.CurrentChapter()==chapter,"Travel loads requested chapter");CollectAll();WorldCohesionChecks.CheckAndCapture(chapter);Inspect();SeasonIntegrationChecks.Capture("Exploration"+chapter);
                if(chapter<4){session.Save();session.LoadChapter(chapter+1);}else {Check(session.Data.ArtifactCount==12,"All artifacts survive chapter loads");session.ShowArtifacts();Inspect();SeasonIntegrationChecks.Capture("ArtifactAchievement");}
            }
            else if(stage==7)
            {
                session.Resume();var w=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();Check(w.Progress.InCave,"Cave artifact collected inside cave");savedPosition=w.player.transform.position;session.Save();session.ReturnToMenu();
            }
            else if(stage==8){session.LoadChapter(4);}
            else if(stage==9)
            {
                var w=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();Check(w.Progress.InCave&&Vector2.Distance(w.player.transform.position,savedPosition)<.01f,"Exact cave location and region restored");Check(session.Data.ArtifactCount==12,"Collection retained through menu");
                JourneyCertificate.OutputDirectoryOverride=Path.GetFullPath("Logs/SeasonChecks");session.FinishJourney();
            }
            else if(stage==10){Check(session.Data.finished&&!session.Data.outroSeen,"Missing-outro fallback reaches certificate without claiming playback");Inspect();SeasonIntegrationChecks.Capture("CertificateScreen");JourneyCertificate.Export(session.Data,UI);}
            else if(stage==11)
            {
                Check(JourneyCertificate.LastExportPath!=null&&File.Exists(JourneyCertificate.LastExportPath),"PDF exported");Check(new FileInfo(JourneyCertificate.LastExportPath).Length>10000,"Certificate contains rendered page");
                session.Resume();session.Pause();Call(session,"ConfirmReset");Inspect();SeasonIntegrationChecks.Capture("ResetConfirmation");
                foreach(var b in UI.GetComponentsInChildren<UnityEngine.UI.Button>())if(b.name=="Reset / Start from the beginning")b.onClick.Invoke();
            }
            else if(stage==12)
            {
                Check(SceneManager.GetActiveScene().name=="Intro"&&!session.Data.started&&session.Data.ArtifactCount==0,"Full reset returns fresh menu");Check(!File.Exists(JourneySession.SavePath)&&!File.Exists(JourneySession.SavePath+".bak"),"Reset deletes recovery copy too");Inspect();
                UI.GetComponentInChildren<TMP_InputField>().text="Zoë Explorer";Click("Settings");Inspect();Click("Guidance: Explorer");Click("Back");Check(UI.GetComponentInChildren<TMP_InputField>().text=="Zoë Explorer","Settings retains draft explorer name");SeasonIntegrationChecks.Capture("NewJourneyMenu");
                var input=UI.GetComponentInChildren<TMP_InputField>();input.text=" ";session.NewJourney();Check(!session.Data.started,"Blank name rejected");
                string normalSave=JourneySession.SavePathOverride;string directory=Path.GetFullPath("Logs/SeasonChecks/menu-save-directory");Directory.CreateDirectory(directory);
                try{JourneySession.SavePathOverride=directory;UI.GetComponentInChildren<TMP_InputField>().text="Test Explorer";session.NewJourney();Check(!session.Data.started&&UI.GetComponentInChildren<TMP_InputField>(),"Failed save stays on name-entry menu");Inspect();}
                finally{JourneySession.SavePathOverride=normalSave;}
                UI.GetComponentInChildren<TMP_InputField>().text="Zoë Explorer";var opening=UnityEngine.Object.FindAnyObjectByType<OpeningCinematic>();Check(!opening.clip&&!Resources.Load<UnityEngine.Video.VideoClip>("Journey/Intro"),"No placeholder intro video ships");Click("New Journey");Check(session.Data.started&&session.Data.playerName=="Zoë Explorer"&&session.Data.assistedGuidance,"Named journey starts with chosen settings");
            }
            else if(stage==13)
            {
                Check(SceneManager.GetActiveScene().name=="Spring","Missing teammate intro starts Spring immediately without an error screen");Check(!UnityEngine.Object.FindAnyObjectByType<UnityEngine.Video.VideoPlayer>(),"Retired opening player is gone");
            }
            else if(stage==14){Check(SceneManager.GetActiveScene().name=="Spring","New Journey enters fresh Spring");Check(!UnityEngine.Object.FindAnyObjectByType<SpringGuide>().LessonComplete,"Reset starts with no lessons");Debug.Log("JOURNEY_PLAY_PASS: menu, exact outdoor/cave save, pause, chapter travel, all 12 F-key pickups, achievement, certificate export, reset and direct-to-Spring start.");return true;}
            return false;
        }
        static void Click(string name)
        {
            foreach(var button in UI.GetComponentsInChildren<UnityEngine.UI.Button>())if(button.name==name){button.onClick.Invoke();return;}
            throw new Exception("Missing menu button: "+name);
        }
        static void CollectAll()
        {
            var session=JourneySession.Instance;var world=UnityEngine.Object.FindAnyObjectByType<JourneyWorld>();Check(world&&world.Artifacts.Count==3,"Three artifacts authored in chapter");var player=session.Player();
            for(int i=0;i<3;i++)
            {
                if(JourneySession.CurrentChapter()==4&&i==2)
                {
                    var w=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();player.position=w.caveEntrance.position;player.GetComponent<Rigidbody2D>().position=player.position;w.World.ChangeRegion(true);Check(w.Progress.InCave,"Can enter cave for artifact");
                }
                var item=world.Artifacts[i];Check(item.activeInHierarchy,"Uncollected artifact visible in correct region");
                Vector2 from=player.position,to=item.transform.position;Vector2 min=JourneySession.CurrentChapter()==4&&i==2?WinterWorld.CaveMin:new Vector2(-49,-48);Vector2 max=JourneySession.CurrentChapter()==4&&i==2?WinterWorld.CaveMax:new Vector2(49,44);
                Physics2D.SyncTransforms();Check(SeasonNavigation.FindPath(from,to,min,max).Count>0,"Artifact has a traversable route: "+item.name);
                player.position=to;player.GetComponent<Rigidbody2D>().position=to;Physics2D.SyncTransforms();
                InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.F));InputSystem.Update();Call(world,"Update");InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.Update();
                Check(session.Data.artifacts[(JourneySession.CurrentChapter()-1)*3+i],"F collects "+item.name);Inspect();session.Resume();
            }
        }
    }
}
#endif
