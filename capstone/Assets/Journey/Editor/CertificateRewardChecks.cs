#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using TMPro;
using AmbrosJourney.Winter;

namespace AmbrosJourney.Journey.Editor
{
    [InitializeOnLoad]
    public static class CertificateRewardChecks
    {
        const string Key="CertificateRewardChecks.Active";
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        static int stage;static double waitUntil,deadline;static string date;static int startFrame;static bool sawDrop;static float nextCapture;static int captureIndex;
        static readonly MethodInfo Signal=typeof(EditorApplication).GetMethod("SignalTick",BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public);
        static CertificateRewardChecks()
        {
            EditorApplication.update+=Tick;
            if(SessionState.GetBool(Key,false)){JourneySession.TestMode=true;JourneySession.SavePathOverride=Path.GetFullPath("Logs/CertificateChecks/save.json");}
            Application.logMessageReceived+=(message,stack,type)=>{if(stack.Contains("UnityEditor.Search.SearchDatabase"))return;if(SessionState.GetBool(Key,false)&&EditorApplication.isPlaying&&(type==LogType.Error||type==LogType.Exception||type==LogType.Assert))SessionState.SetString(Key+"Error",message);};
        }
        public static void Run()
        {
            Directory.CreateDirectory("Logs/CertificateChecks");Directory.CreateDirectory("Logs/CertificateChecks/Animation");SessionState.SetString(Key+"Error","");SessionState.SetBool(Key,true);
            const string path="Assets/Journey/Resources/Journey/CertificateBackground.png";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            EditorSceneManager.OpenScene("Assets/WinterLevel/Winter.unity");
            var gameViewType=Type.GetType("UnityEditor.GameView,UnityEditor");var view=EditorWindow.GetWindow(gameViewType);view.Focus();
            var assembly=typeof(UnityEditor.Editor).Assembly;var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);var sizes=singleton.GetProperty("instance").GetValue(null);
            var groupType=assembly.GetType("UnityEditor.GameViewSizeGroupType");var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new[]{Enum.Parse(groupType,"Standalone")});
            var sizeType=assembly.GetType("UnityEditor.GameViewSize");var kind=assembly.GetType("UnityEditor.GameViewSizeType");
            var size=Activator.CreateInstance(sizeType,new object[]{Enum.Parse(kind,"FixedResolution"),1280,720,"Certificate validation"});
            group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});int index=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null)-1;
            gameViewType.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(view,index);
            EditorApplication.isPlaying=true;
        }
        static void Check(bool value,string why){if(!value)throw new Exception("CERTIFICATE CHECK: "+why);}
        static void Call(object obj,string name)=>obj.GetType().GetMethod(name,Hidden).Invoke(obj,null);
        static JourneyUI UI=>JourneySession.Instance.GetComponentInChildren<JourneyUI>();
        static void Tick()
        {
            if(!SessionState.GetBool(Key,false))return;
            try
            {
                string error=SessionState.GetString(Key+"Error","");Check(error=="",error);
                EditorApplication.QueuePlayerLoopUpdate();Signal?.Invoke(null,null);
                if(!EditorApplication.isPlaying)return;
                JourneySession.TestMode=true;JourneySession.SavePathOverride=Path.GetFullPath("Logs/CertificateChecks/save.json");Application.runInBackground=true;
                if(deadline==0){deadline=EditorApplication.timeSinceStartup+150;waitUntil=EditorApplication.timeSinceStartup+3;}
                Check(EditorApplication.timeSinceStartup<deadline,"Timed out at stage "+stage+" frame "+Time.frameCount);
                if(stage>=1&&stage<=2)
                {
                    var animated=UnityEngine.Object.FindAnyObjectByType<WinterLevel>()?.CertificateReward;
                    if(animated&&animated.Dropping){sawDrop=true;Check(!animated.Ready,"Reward cannot be picked up while falling");}
                    if(Time.unscaledTime>=nextCapture){ScreenCapture.CaptureScreenshot(Path.GetFullPath("Logs/CertificateChecks/Animation/"+(captureIndex++).ToString("D3")+".png"));nextCapture=Time.unscaledTime+.075f;}
                }
                if(EditorApplication.timeSinceStartup<waitUntil)return;
                var session=JourneySession.Instance;var level=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();Check(session&&level,"Winter and session available");
                if(stage==0)
                {
                    session.BeginTestProfile();session.Resume();Call(level,"Close");
                    var p=new WinterProgress{Introduced=true};
                    for(int m=0;m<2;m++){p.ReadNote(m);foreach(var q in WinterContent.Missions[m])p.AnswerMission(m,q.Correct);}
                    p.BeginWinterBoss();foreach(var q in WinterContent.Boss)p.AnswerWinterBoss(q.Correct);p.EnterCave();p.ReadNote(2);foreach(var q in WinterContent.Missions[2])p.AnswerMission(2,q.Correct);
                    p.BeginBoss();foreach(var q in WinterContent.Final)p.AnswerFinal(q.Expected);
                    Check(p.Complete&&p.Valid(),"Genuine final pass state");level.Progress=p;level.World.RestoreRegion(true);
                    level.player.transform.position=WinterWorld.BossPosition+Vector2.down*4;level.player.GetComponent<Rigidbody2D>().position=level.player.transform.position;Camera.main.transform.position=level.player.transform.position+Vector3.back*10;
                    session.RecordFinalVictory();date=session.Data.completedOn;Check(session.Data.certificatePending&&date==DateTime.Now.ToString("yyyy-MM-dd"),"Victory date saved before pickup");
                    session.ShowCertificate();Check(!session.Paused,"Certificate unavailable before pickup");
                    Call(level,"BossQuestion");Check(level.Transitioning&&level.CertificateReward&&!level.CertificateReward.Ready,"Victory starts crumble before item appears");
                    startFrame=Time.frameCount;stage++;waitUntil=EditorApplication.timeSinceStartup+.7;
                }
                else if(stage==1)
                {
                    Check(Time.frameCount>startFrame,"Game is advancing");Check(!level.guardian.GetComponent<SpriteRenderer>().enabled,"Original boss replaced by texture-backed pixels");
                    Check(level.CertificateReward.DetachedPixelCount>0&&level.CertificateReward.DetachedPixelCount<level.CertificateReward.TotalPixelCount,"Pixels detach progressively rather than disappearing together");
                    Check(GameObject.Find("Falling Keeper dust"),"Boss-colored dust is visible");ScreenCapture.CaptureScreenshot(Path.GetFullPath("Logs/CertificateChecks/KeeperCrumble.png"));
                    stage++;waitUntil=EditorApplication.timeSinceStartup+3;
                }
                else if(stage==2)
                {
                    Check(!level.Transitioning&&level.CertificateReward.Ready&&sawDrop,"Animation drops certificate, bounces and restores movement");
                    Check(!level.guardian.GetComponent<SpriteRenderer>().enabled,"Boss is gone");
                    var position=level.CertificateReward.Position;level.player.transform.position=position+Vector2.down*4;level.player.GetComponent<Rigidbody2D>().position=level.player.transform.position;
                    Check(!level.CertificateReward.TryCollect(),"Remote pickup rejected");
                    session.Data.completedOn="2026-10-01";date=session.Data.completedOn;session.RecordFinalVictory();Check(session.Data.completedOn==date,"Completion date never changes on replay");
                    session.Save();var loaded=JsonUtility.FromJson<JourneyData>(File.ReadAllText(JourneySession.SavePath));Check(loaded.certificatePending&&loaded.completedOn==date,"Pending pickup survives save");
                    level.enabled=false;UnityEngine.Object.Destroy(level.CertificateReward);stage++;waitUntil=EditorApplication.timeSinceStartup+.2;
                }
                else if(stage==3)
                {
                    level.RestoreProgress(level.Progress);level.enabled=true;Check(level.CertificateReward.Ready&&!level.Transitioning,"Restored victory recreates item without repeating crumble");
                    var position=level.CertificateReward.Position;level.player.transform.position=position+Vector2.down*.7f;level.player.GetComponent<Rigidbody2D>().position=level.player.transform.position;Physics2D.SyncTransforms();
                    Call(level,"RefreshWorld");ScreenCapture.CaptureScreenshot(Path.GetFullPath("Logs/CertificateChecks/DroppedCertificate.png"));stage++;waitUntil=EditorApplication.timeSinceStartup+.2;
                }
                else if(stage==4)
                {
                    Check(level.CertificateReward.CanCollect,"Item reachable");
                    InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(UnityEngine.InputSystem.Key.E));InputSystem.Update();Call(level,"Update");InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.Update();
                    Check(session.Paused&&!session.Data.certificatePending&&!level.CertificateReward.Ready,"E collects exactly once and opens certificate");
                    Check(!level.CertificateReward.TryCollect(),"Duplicate pickup rejected");Check(UI.transform.Find("Certificate backdrop").GetComponent<UnityEngine.UI.Image>().raycastTarget,"Certificate is modal");Inspect("Zoë Explorer");
                    ScreenCapture.CaptureScreenshot(Path.GetFullPath("Logs/CertificateChecks/PersonalizedCertificate.png"));
                    JourneyCertificate.OutputDirectoryOverride=Path.GetFullPath("Logs/CertificateChecks");JourneyCertificate.Export(session.Data,UI);stage++;waitUntil=EditorApplication.timeSinceStartup+2;
                }
                else if(stage==5)
                {
                    Check(JourneyCertificate.LastExportPath!=null&&File.Exists(JourneyCertificate.LastExportPath),"Personalized PDF exported");
                    session.Data.playerName="Alexandria Catherine Montgomery";session.ShowCertificate();Inspect(session.Data.playerName);ScreenCapture.CaptureScreenshot(Path.GetFullPath("Logs/CertificateChecks/LongNameCertificate.png"));
                    Check(session.Data.completedOn==date,"Certificate retains victory date");stage++;waitUntil=EditorApplication.timeSinceStartup+.4;
                }
                else if(stage==6)
                {
                    session.Resume();level.enabled=false;UnityEngine.Object.Destroy(level.CertificateReward);stage++;waitUntil=EditorApplication.timeSinceStartup+.2;
                }
                else if(stage==7)
                {
                    level.RestoreProgress(level.Progress);level.enabled=true;Check(!level.CertificateReward.Ready&&!level.guardian.GetComponent<SpriteRenderer>().enabled,"Claimed reward does not respawn; boss remains defeated");
                    session.ShowCertificate();Inspect(session.Data.playerName);Check(session.Data.completedOn==date,"Reopened certificate retains name and date");
                    Debug.Log("CERTIFICATE_REWARD_PASS: real final-pass state, advancing crumble/dust, dropped item, E pickup, reload recovery, duplicate protection, saved date, long/accented names and PDF export.");Finish(0);
                }
            }
            catch(Exception e){Debug.LogException(e);Finish(1);}
        }
        static void Inspect(string name)
        {
            Canvas.ForceUpdateCanvases();var field=Array.Find(UI.GetComponentsInChildren<TMP_Text>(),text=>text.name=="Certificate player name");Check(field.text==name,"Saved name populated");
            var dateField=Array.Find(UI.GetComponentsInChildren<TMP_Text>(),text=>text.name=="Certificate completion date");Check(dateField.text==date,"Saved date populated");
            foreach(var text in UI.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,"Text fits: "+text.name);}
        }
        static void Finish(int code){SessionState.SetBool(Key,false);JourneySession.TestMode=false;EditorApplication.Exit(code);}
    }
}
#endif
