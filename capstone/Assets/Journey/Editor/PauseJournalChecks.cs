#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace AmbrosJourney.Journey.Editor
{
    [InitializeOnLoad]
    public static class PauseJournalChecks
    {
        const string Key="PauseJournalChecks.Active";
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static int stage;static double waitUntil,deadline;static Vector2 winterPosition;
        static readonly MethodInfo Signal=typeof(EditorApplication).GetMethod("SignalTick",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
        static PauseJournalChecks()
        {
            EditorApplication.update+=Tick;
            if(SessionState.GetBool(Key,false)){JourneySession.TestMode=true;JourneySession.SavePathOverride=Path.GetFullPath("Logs/PauseJournalChecks/save.json");}
            Application.logMessageReceived+=(message,stack,type)=>{if(stack.Contains("UnityEditor.Search.SearchDatabase"))return;if(SessionState.GetBool(Key,false)&&EditorApplication.isPlaying&&(type==LogType.Error||type==LogType.Exception||type==LogType.Assert))SessionState.SetString(Key+"Error",message);};
        }
        public static void Run()
        {
            Directory.CreateDirectory("Logs/PauseJournalChecks");SessionState.SetString(Key+"Error","");SessionState.SetBool(Key,true);
            EditorSceneManager.OpenScene("Assets/WinterLevel/Winter.unity");EditorApplication.isPlaying=true;
        }
        static void Check(bool value,string why){if(!value)throw new Exception("JOURNAL CHECK: "+why);}
        static void Call(object target,string method)=>target.GetType().GetMethod(method,Private).Invoke(target,null);
        static JourneyUI UI=>JourneySession.Instance.GetComponentInChildren<JourneyUI>();
        static void Click(string name)
        {
            foreach(var button in UI.GetComponentsInChildren<UnityEngine.UI.Button>())if(button.name==name){Check(button.interactable,"Button enabled: "+name);button.onClick.Invoke();return;}
            throw new Exception("Missing journal action: "+name);
        }
        static bool HasButton(string name)=>Array.Exists(UI.GetComponentsInChildren<UnityEngine.UI.Button>(),button=>button.name==name);
        static void Inspect()
        {
            Check(UI.transform.Find("Left parchment page")&&UI.transform.Find("Right parchment page"),"Two journal pages present");
            Check(UI.GetComponent<UnityEngine.UI.GraphicRaycaster>()&&UnityEngine.EventSystems.EventSystem.current,"Input infrastructure retained");
            Canvas.ForceUpdateCanvases();foreach(var text in UI.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,"Text fits: "+text.name);}
            foreach(var button in UI.GetComponentsInChildren<UnityEngine.UI.Button>())
            {
                Check(button.targetGraphic&&button.GetComponent<UnityEngine.UI.Image>().raycastTarget,"Button receives input: "+button.name);
                var corners=new Vector3[4];button.GetComponent<RectTransform>().GetWorldCorners(corners);
                foreach(var corner in corners){var point=RectTransformUtility.WorldToScreenPoint(null,corner);Check(point.x>=-1&&point.x<=Screen.width+1&&point.y>=-1&&point.y<=Screen.height+1,"Button inside viewport: "+button.name);}
            }
        }
        static void Capture(string name)=>ScreenCapture.CaptureScreenshot(Path.GetFullPath("Logs/PauseJournalChecks/"+name+".png"));
        static void Tick()
        {
            if(!SessionState.GetBool(Key,false))return;
            try
            {
                Check(SessionState.GetString(Key+"Error","")=="",SessionState.GetString(Key+"Error",""));EditorApplication.QueuePlayerLoopUpdate();Signal?.Invoke(null,null);
                if(!EditorApplication.isPlaying)return;JourneySession.TestMode=true;JourneySession.SavePathOverride=Path.GetFullPath("Logs/PauseJournalChecks/save.json");Application.runInBackground=true;
                if(deadline==0){deadline=EditorApplication.timeSinceStartup+180;waitUntil=EditorApplication.timeSinceStartup+3;}
                Check(EditorApplication.timeSinceStartup<deadline,"Timed out at stage "+stage);
                if(EditorApplication.timeSinceStartup<waitUntil)return;
                var session=JourneySession.Instance;Check(session,"Session ready");
                if(stage==0)
                {
                    session.BeginTestProfile();session.Data.unlocked=4;session.Data.artifacts[0]=session.Data.artifacts[4]=session.Data.artifacts[8]=true;
                    session.Resume();var winter=UnityEngine.Object.FindAnyObjectByType<AmbrosJourney.Winter.WinterLevel>();winter.Progress.Introduced=true;Call(winter,"Close");winterPosition=winter.player.transform.position;
                    session.Pause();Check(session.Paused&&Time.timeScale==0&&AudioListener.pause&&winter.InputBlocked,"Pause still freezes world, sound and input");Inspect();
                    Check(!HasButton("Certificate")&&UI.GetComponentsInChildren<UnityEngine.UI.Button>().Length==9,"Original actions and four unlocked chapters only");Capture("Journal-Winter-"+Screen.width+"x"+Screen.height);
                }
                else if(stage==1)
                {
                    bool old=session.Data.assistedGuidance;Click("Guidance: "+(old?"Assisted":"Explorer"));Check(session.Data.assistedGuidance!=old,"Guidance toggles");Check(JsonUtility.FromJson<JourneyData>(File.ReadAllText(JourneySession.SavePath)).assistedGuidance==!old,"Guidance persists");Inspect();
                    Click("Artifacts / 3 of 12");Check(Array.Exists(UI.GetComponentsInChildren<TMP_Text>(),text=>text.text.Contains("3 of 12 recovered")),"Artifact view opens");Click("Return / Esc");Check(session.Paused&&Time.timeScale==0&&AudioListener.pause,"Return restores pause journal");
                    session.Pause();Click("Reset journey");Check(HasButton("Keep my progress"),"Reset still requires confirmation");Click("Keep my progress");Check(session.Data.started&&session.Data.ArtifactCount==3,"Cancel reset preserves progress");Inspect();
                    Click("Resume");Check(!session.Paused&&Time.timeScale==1,"Resume button resumes");
                    session.Pause();Click(JourneySession.ChapterNames[0]);
                }
                else if(stage==2)
                {
                    Check(SceneManager.GetActiveScene().name=="Spring","Spring travel preserved");session.Data.unlocked=1;session.Pause();Inspect();Check(UI.GetComponentsInChildren<UnityEngine.UI.Button>().Length==6,"Locked seasons are decoration, not new actions");Capture("Journal-Spring-Locked");
                }
                else if(stage==3){session.Data.unlocked=4;session.Pause();Click(JourneySession.ChapterNames[1]);}
                else if(stage==4){Check(SceneManager.GetActiveScene().name=="Summer","Summer travel preserved");session.Pause();Inspect();Click(JourneySession.ChapterNames[2]);}
                else if(stage==5){Check(SceneManager.GetActiveScene().name=="Fall","Fall travel preserved");session.Pause();Inspect();Click(JourneySession.ChapterNames[3]);}
                else if(stage==6)
                {
                    Check(SceneManager.GetActiveScene().name=="Winter"&&Vector2.Distance(session.Player().position,winterPosition)<.1f,"Winter travel restores saved location");
                    session.Data.finished=true;session.Data.certificatePending=false;session.Data.completedOn="2026-11-03";session.Pause();Inspect();Check(HasButton("Certificate"),"Earned certificate remains available");Capture("Journal-Completed");
                }
                else if(stage==7)
                {
                    Click("Certificate");Check(UI.transform.Find("Personalized certificate"),"Certificate action unchanged");Click("Return");session.Data.playerName=new string('W',32);session.Pause();Inspect();
                }
                else if(stage==8){Inspect();}
                else if(stage==9){Inspect();Capture("Journal-"+Screen.width+"x"+Screen.height+"-LongName");}
                else if(stage==10){session.Data.playerName="Zoë Explorer";session.Pause();Click("Save and return to menu");}
                else if(stage==11){Check(SceneManager.GetActiveScene().name=="Intro"&&File.Exists(JourneySession.SavePath),"Save and return still opens menu");Click("Continue");}
                else if(stage==12)
                {
                    Check(SceneManager.GetActiveScene().name=="Winter"&&session.Data.ArtifactCount==3&&!session.Paused,"Continue keeps season, artifacts and gameplay");session.Pause();Inspect();Capture("Journal-Returned");
                    Debug.Log("PAUSE_JOURNAL_PASS: Viewport layout, long/accented names, pause/Resume, guidance persistence, artifact view, reset cancellation, all four chapter actions, earned certificate, exact saved location and save/return/continue.");Finish(0);return;
                }
                stage++;waitUntil=EditorApplication.timeSinceStartup+(stage==2||stage==4||stage==5||stage==6||stage==11||stage==12?2.5:.6);
            }
            catch(Exception e){Debug.LogException(e);Finish(1);}
        }
        static void Finish(int code)
        {
            SessionState.SetBool(Key,false);
            EditorApplication.Exit(code);
        }
    }
}
#endif
