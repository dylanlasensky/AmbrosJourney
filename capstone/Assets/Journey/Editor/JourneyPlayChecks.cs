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
        static Vector2 rememberedPosition;
        static Keyboard testKeyboard;
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
                // Isolate simulated keys from hardware state in a background Editor.
                testKeyboard=InputSystem.AddDevice<Keyboard>();testKeyboard.MakeCurrent();
                var winter=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();Check(winter.Progress.Complete&&winter.Progress.WinterComplete,"Both Winter bosses complete");
                JourneySession.ApplyPerformance();double refresh=Screen.currentResolution.refreshRateRatio.value;double limit=QualitySettings.vSyncCount>0?refresh/QualitySettings.vSyncCount:Application.targetFrameRate;Check(limit<=60.01&&!Application.runInBackground,"Rendering limited to at most 60 FPS; background execution off");Application.runInBackground=true;
                rememberedPosition=winter.player.transform.position;session.Pause();Inspect();Check(Time.timeScale==0&&AudioListener.pause&&winter.InputBlocked,"Pause freezes simulation, audio and input");
                Check(session.CaptureChapter(),"Current chapter is captured in memory");Check(session.Data.chapters[3].visited,"Winter is available for this run's revisits");SeasonIntegrationChecks.Capture("PauseMenu");
                session.Resume();winter.Interact(1);Check(winter.IsDialogueOpen,"Field note opens for pause regression");session.Pause();InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.Escape));InputSystem.Update();Check(Keyboard.current.escapeKey.wasPressedThisFrame,"Synthetic Escape press delivered");Call(session,"Update");Check(!session.Paused,"Escape resumes pause menu");Call(winter,"Update");Check(winter.IsDialogueOpen,"Esc resumes without also closing the field note");InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.Update();Call(winter,"Close");session.CaptureChapter();session.LoadChapter(1);
            }
            else if(stage==1)
            {
                Check(SceneManager.GetActiveScene().name=="Spring"&&session.Data.started,"Earlier season remains available during the current run");
                session.Pause();Check(session.Paused&&Time.timeScale==0,"Ready chapter enters Pause before the end-run confirmation");string beforeEnd=JsonUtility.ToJson(session.Data);Call(session,"ShowConfirmEndRun");Inspect();
                Check(Array.Exists(UI.GetComponentsInChildren<UnityEngine.UI.Button>(),b=>b.name=="Keep playing")&&Array.Exists(UI.GetComponentsInChildren<UnityEngine.UI.Button>(),b=>b.name=="End run / Main menu"),"Ending an active run asks before clearing it");
                Click("Keep playing");Check(session.Data.started&&session.Paused,"Cancel end-run keeps the current journey paused");Check(JsonUtility.ToJson(session.Data)==beforeEnd,"Cancel end-run preserves every chapter, artifact and completion field");
                Check(!UI.transform.Find("Reset journey"),"Pause has one clear end-run action");
                session.Resume();session.LoadChapter(4);
            }
            else if(stage==2)
            {
                var winter=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();Check(winter&&winter.Progress.Complete&&winter.Progress.WinterComplete,"Current-run revisit retains both bosses");Check(Vector2.Distance(winter.player.transform.position,rememberedPosition)<.01f,"Current-run revisit restores the exact Winter position");
                Check(Time.timeScale==1&&!AudioListener.pause,"Chapter travel unpauses");session.LoadChapter(1);
            }
            else if(stage>=3&&stage<=6)
            {
                int chapter=stage-2;Check(JourneySession.CurrentChapter()==chapter,"Travel loads requested chapter");JourneyPerformanceChecks.VerifyCurrentScene();WorldcraftChecks.Verify(chapter);CollectAll();MoonbranchPolishChecks.Verify(chapter);WorldCohesionChecks.CheckAndCapture(chapter);Inspect();SeasonIntegrationChecks.Capture("Exploration"+chapter);
                if(chapter<4){session.CaptureChapter();session.LoadChapter(chapter+1);}else {Check(session.Data.ArtifactCount==12,"All artifacts survive chapter loads");session.ShowArtifacts();Inspect();SeasonIntegrationChecks.Capture("ArtifactAchievement");}
            }
            else if(stage==7)
            {
                session.Resume();var w=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();Check(w.Progress.InCave,"Cave artifact collected inside cave");rememberedPosition=w.player.transform.position;session.CaptureChapter();session.LoadChapter(1);
            }
            else if(stage==8){Check(SceneManager.GetActiveScene().name=="Spring"&&session.Data.ArtifactCount==12,"Current-run travel retains the completed collection");session.LoadChapter(4);}
            else if(stage==9)
            {
                var w=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();Check(w.Progress.InCave&&Vector2.Distance(w.player.transform.position,rememberedPosition)<.01f,"Current-run revisit restores exact cave location and region");Check(session.Data.ArtifactCount==12,"Collection retained across current-run chapter visits");
                JourneyCertificate.OutputDirectoryOverride=Path.GetFullPath("Logs/SeasonChecks");session.FinishJourney();
            }
            else if(stage==10){Check(session.Data.finished&&!session.Data.outroSeen,"Certificate is shown before outro playback");Inspect();SeasonIntegrationChecks.Capture("CertificateScreen");JourneyCertificate.Export(session.Data,UI);}
            else if(stage==11)
            {
                Check(JourneyCertificate.LastExportPath!=null&&File.Exists(JourneyCertificate.LastExportPath),"PDF exported");Check(new FileInfo(JourneyCertificate.LastExportPath).Length>10000,"Certificate contains rendered page");
                session.Resume();session.Pause();Call(session,"ShowConfirmEndRun");Inspect();SeasonIntegrationChecks.Capture("EndRunConfirmation");
                foreach(var b in UI.GetComponentsInChildren<UnityEngine.UI.Button>())if(b.name=="End run / Main menu")b.onClick.Invoke();
            }
            else if(stage==12)
            {
                Check(SceneManager.GetActiveScene().name=="Intro"&&!session.Data.started&&session.Data.ArtifactCount==0&&!session.Data.finished,"Ending a run returns to a fresh menu");
                foreach(var chapter in session.Data.chapters)Check(!chapter.visited,"Ending a run clears every remembered chapter");
                Check(!UI.transform.Find("Continue")&&!UI.transform.Find("Saved season"),"Main menu has no Continue or saved-season entry");
                Check(UI.transform.Find("Blossom valley painting").GetComponent<UnityEngine.UI.RawImage>().texture,"Menu painting imported");Inspect();SeasonIntegrationChecks.Capture("MainMenu");
                UI.GetComponentInChildren<TMP_InputField>().text="Zoë Explorer";Click("Settings");Inspect();Check(UI.transform.Find("Guidance status").GetComponent<TMP_Text>().text=="Trail guidance: always on","Settings explain continuous guidance");Check(!UI.transform.Find("Guidance status").GetComponent<UnityEngine.UI.Button>(),"Guidance has no ineffective toggle");SeasonIntegrationChecks.Capture("MenuSettings");Click("Back");Check(UI.GetComponentInChildren<TMP_InputField>().text=="Zoë Explorer","Settings retains draft explorer name");SeasonIntegrationChecks.Capture("NewJourneyMenu");
                var input=UI.GetComponentInChildren<TMP_InputField>();input.text=" ";session.NewJourney();Check(!session.Data.started,"Blank name rejected");
                UI.GetComponentInChildren<TMP_InputField>().text="Zoë Explorer";var opening=UnityEngine.Object.FindAnyObjectByType<OpeningCinematic>();Check(Resources.Load<UnityEngine.Video.VideoClip>("Journey/Intro"),"Supplied original intro is included");Click("New Journey");Check(session.Data.started&&session.Data.playerName=="Zoë Explorer","Named journey starts with continuous guidance");
                // Playback has its own checks; advance the clip to keep this progression suite focused.
                opening.GetType().GetMethod("Finished",Private).Invoke(opening,new object[]{null});
            }
            else if(stage==13)
            {
                Check(SceneManager.GetActiveScene().name=="Spring","Completing the supplied intro loads Spring");Check(!UnityEngine.Object.FindAnyObjectByType<UnityEngine.Video.VideoPlayer>(),"Opening player is released after the cinematic");
            }
            else if(stage==14){Check(SceneManager.GetActiveScene().name=="Spring","New Journey enters fresh Spring");Check(!UnityEngine.Object.FindAnyObjectByType<SpringGuide>().LessonComplete,"New run starts with no lessons");if(testKeyboard!=null){InputSystem.RemoveDevice(testKeyboard);testKeyboard=null;}Debug.Log("JOURNEY_PLAY_PASS: fresh menu without Continue, exact current-run chapter/cave revisits, pause, all 12 E-key pickups, achievement, certificate export, end-run cancellation/reset and new named journey.");return true;}
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
                InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.E));InputSystem.Update();Call(world,"Update");InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.Update();
                Check(session.Data.artifacts[(JourneySession.CurrentChapter()-1)*3+i],"E collects "+item.name);Inspect();
                InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(i%2==0?Key.Space:Key.Escape));InputSystem.Update();Call(session,"Update");
                Check(!session.Paused&&Time.timeScale==1&&!AudioListener.pause,"Space or Escape closes collected artifact and restores gameplay");
                InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.Update();
            }
        }
    }
}
#endif
