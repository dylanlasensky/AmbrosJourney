// Runs isolated Editor regression checks for season integration.
using System;
using System.IO;
using System.Linq;
using AmbrosJourney.Presentation;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using InputKey=UnityEngine.InputSystem.Key;
using AmbrosJourney.Summer;
using AmbrosJourney.Fall;
using AmbrosJourney.Winter;
using AmbrosJourney.Presentation.Editor;

// Run in a disposable checkout: enters Play mode, exercises the real portals, then exits.
[InitializeOnLoad]
public static class SeasonIntegrationChecks
{
    const string Key = "SeasonIntegrationChecks.";
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static double readyAt;
    static readonly FieldInfo JourneyLoading=typeof(AmbrosJourney.Journey.JourneySession).GetField("loading",Private);
    static readonly MethodInfo SignalTick = typeof(EditorApplication).GetMethod("SignalTick", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
    static SeasonIntegrationChecks()
    {
        EditorApplication.update += Tick;
        Application.logMessageReceived += (message, stack, type) =>
        {
            // Ignore Unity's known startup search-index exception, as the focused journal runner does.
            if (stack.Contains("UnityEditor.Search.SearchDatabase")) return;
            if (SessionState.GetBool(Key + "active", false) &&
                (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
                SessionState.SetString(Key + "error", message);
        };
    }
    public static void Run()
    {
        Directory.CreateDirectory("Logs/SeasonChecks");
        File.WriteAllText("Logs/SeasonChecks/Integration.txt", "RUNNING\n");
        SessionState.SetString(Key + "error", "");
        SessionState.SetInt(Key + "stage", 0);
        SessionState.SetBool(Key + "active", true);
        SessionState.SetString(Key + "deadline", DateTime.UtcNow.AddMinutes(12).ToString("O"));
        AmbrosJourney.Journey.Editor.JourneyTools.Prepare();
        SummerToolsValidate();
        FallToolsValidate();
        PresentationTools.ValidateMath();
        TrailNavigationChecks.Verify();
        CinematicIsolationChecks.Run();
        CinematicPresentationChecks.Run();
        EditorSceneManager.OpenScene("Assets/Scenes/Spring.unity");
        EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();
        Application.runInBackground = true;
        EditorApplication.isPlaying = true;
    }
    static void SummerToolsValidate() => AmbrosJourney.Summer.Editor.SummerTools.Validate();
    static void FallToolsValidate() => AmbrosJourney.Fall.Editor.FallTools.Validate();
    static void Check(bool value, string message)
    { if (!value) throw new Exception(message); }
    static void Set(object obj, string name, object value) => obj.GetType().GetField(name, Private).SetValue(obj, value);
    static void Call(object obj, string name, params object[] args) => obj.GetType().GetMethod(name, Private).Invoke(obj, args);
    // A runtime scene becomes active before its world and level Start methods run.
    // Wait for the actual portal fade to finish, then assert the initialized arrival state.
    static bool ArrivalReady(string scene) => SceneManager.GetActiveScene().name == scene && !SeasonTransition.Active;
    static void Tick()
    {
        if (!SessionState.GetBool(Key + "active", false)) return;
        try
        {
            string error = SessionState.GetString(Key + "error", "");
            Check(error == "", error);
            Check(DateTime.UtcNow < DateTime.Parse(SessionState.GetString(Key + "deadline", "")).ToUniversalTime(), "Timed out at stage " + SessionState.GetInt(Key + "stage", 0) + "; frame " + Time.frameCount);
            Application.runInBackground = true;
            EditorApplication.QueuePlayerLoopUpdate();
            SignalTick?.Invoke(null, null);
            if (!EditorApplication.isPlaying) return;
            AmbrosJourney.Journey.JourneySession.TestMode=true;
            if (readyAt == 0) readyAt = EditorApplication.timeSinceStartup + 3;
            if (EditorApplication.timeSinceStartup < readyAt) return;
            int stage = SessionState.GetInt(Key + "stage", 0);
            if (stage == 0)
            {
                AmbrosJourney.Journey.JourneySession.Instance.BeginTestProfile();
                AmbrosJourney.Journey.JourneySession.Instance.Resume(); // A background Editor may have auto-paused before test mode began.
                var g = UnityEngine.Object.FindAnyObjectByType<SpringGuide>();
                Check(g, "Spring scene has its guide");
                g.DismissPanel(); // Controls and the Mage conversation are separate, deliberate steps.
                Check(!g.LessonComplete&&!g.DialogueOpen,"Closing controls does not skip the Mage lesson or start automatic speech");
                CompanionGuidanceChecks.Spring(g);
                CompleteSpringGreeting(g);
                AmbrosJourney.Journey.Editor.JourneyPerformanceChecks.VerifyKeyboardNavigation();
                DirectGuidanceChecks.Verify(1);
                AmbrosJourney.Journey.Editor.JourneyPerformanceChecks.VerifyCurrentScene();
                BossRefreshChecks.CheckAndCapture(GameObject.Find("Blossom Guardian").GetComponent<SeasonBossVisual>(),"SpringBlossom");
                CheckMotion(UnityEngine.Object.FindAnyObjectByType<SpringPlayer>(), "input", 4);
                var gate = g.ExitPortal;
                Check(!SummerPortalBridge.GateAllowsEntry(g, gate.position, gate.position), "Locked Spring gate blocks entry");
                CheckOverlay(g); Capture("SpringInterface");
                for (int i = 0; i < 3; i++) { Set(g, "target", 5 + i);typeof(SpringGuide).GetProperty("DialogueOpen").SetValue(g,true);Call(g,"LateUpdate");CheckOverlay(g); Call(g, "Continue"); }
                // Read the active claim mapping so wording/claim changes do not leave stale test answers.
                var truth=(bool[])typeof(SpringGuide).GetField("truth",Private).GetValue(g);
                for (int i=0;i<3;i++)
                {
                    Set(g,"target",i);Set(g,"feedback","");
                    for(int attempt=0;attempt<2;attempt++)
                    {
                        int index=(int)typeof(SpringGuide).GetMethod("QuestionIndex",Private).Invoke(g,new object[]{i});
                        Call(g,"Answer",truth[index]?0:1);Call(g,"Continue");
                    }
                }
                Check(g.MissionsComplete==3,"All three myth-bat lessons complete before guardian quiz");
                Set(g, "target", 3); Set(g, "bossQuestion", 0); Set(g, "feedback", ""); Set(g, "bossResult", false);
                foreach (int a in new[]{0,0,1,1,0}) { Call(g, "Answer", a); Call(g, "Continue"); }
                Check(!g.SpringComplete && g.BossScore == 3, "Spring 3/5 fails"); Call(g, "Continue");
                foreach (int a in new[]{0,0,1,0,0}) { Call(g, "Answer", a); Call(g, "Continue"); }
                Check(g.SpringComplete && g.BossScore == 4, "Spring 4/5 passes"); Call(g, "Continue");
                Check(!SummerPortalBridge.GateAllowsEntry(g, gate.position + Vector3.up * 5, gate.position), "Spring gate requires proximity");
                typeof(SpringGuide).GetProperty("DialogueOpen").SetValue(g, true);
                Check(!SummerPortalBridge.GateAllowsEntry(g, gate.position, gate.position), "Spring modal blocks transition");
                typeof(SpringGuide).GetProperty("DialogueOpen").SetValue(g, false);
                g.player.position = gate.position; g.player.GetComponent<Rigidbody2D>().position = gate.position;
                Advance(1);
            }
            else if (stage == 1 && ArrivalReady("Summer"))
            {
                var l = UnityEngine.Object.FindAnyObjectByType<SummerLevel>();
                Check(l && !l.Progress.Introduced && !l.InputBlocked, "Summer arrives silently with free movement until E at the Warriors");
                DirectGuidanceChecks.Verify(2);AmbrosJourney.Journey.Editor.JourneyPerformanceChecks.VerifyCurrentScene();
                BossRefreshChecks.CheckAndCapture(l.orc.GetComponent<SeasonBossVisual>(),"SummerSuncrest");CheckMotion(l.player,"movement",4.6f);Capture("SummerInterface");
                Check(!UnityEngine.Object.FindAnyObjectByType<SpringGuide>(), "Spring unloads after transition");
                Check(!FallPortalBridge.GateAllowsEntry(l), "Incomplete Summer cannot exit");
                PresentationTools.ValidateRoutes();
                PresentationPlayChecks.Summer();
                Check(File.ReadAllText("Temp/PresentationChecks/SummerPresentationChecks.txt").StartsWith("PASS"), "Summer UI checks pass");
                l.player.transform.position = l.fallGate.position; l.player.GetComponent<Rigidbody2D>().position = l.fallGate.position;
                Check(!FallPortalBridge.GateAllowsEntry(l), "Summer gate conversation blocks exit");
                Call(l, "Close"); Call(l, "ToggleJournal");
                Check(!FallPortalBridge.GateAllowsEntry(l), "Summer journal blocks exit"); Call(l, "ToggleJournal");
                l.player.transform.position=l.fallGate.position+Vector3.up*4;l.player.GetComponent<Rigidbody2D>().position=l.player.transform.position;
                Advance(101);
            }
            else if (stage == 2 && ArrivalReady("Fall"))
            {
                var l = UnityEngine.Object.FindAnyObjectByType<FallLevel>();
                Check(l && !l.Progress.Introduced && !l.InputBlocked, "Fall arrives silently with free movement until E at Emberfist");
                DirectGuidanceChecks.Verify(3);AmbrosJourney.Journey.Editor.JourneyPerformanceChecks.VerifyCurrentScene();
                CheckMotion(l.player,"movement",4.6f);Capture("FallInterface");
                Check(!UnityEngine.Object.FindAnyObjectByType<SummerLevel>(), "Summer unloads after transition");
                AmbrosJourney.Fall.Editor.FallTools.ValidateRoutes();
                PresentationTools.ValidateRoutes();
                PresentationPlayChecks.Fall();
                Check(File.ReadAllText("Temp/PresentationChecks/FallPresentationChecks.txt").StartsWith("PASS"), "Fall UI checks pass");
                Check(l.Progress.Complete && l.ember.follow == l.femaleWarrior.transform, "Fall completed and Emberfist recruited");
                Call(l,"Close");
                l.player.transform.position=l.winterGate.position;l.player.GetComponent<Rigidbody2D>().position=l.winterGate.position;
                Check(!WinterPortalBridge.GateAllowsEntry(l),"Fall portal waits for the victory sequence");
                Call(l,"ToggleJournal");Check(!WinterPortalBridge.GateAllowsEntry(l),"Fall journal blocks Winter entry");Call(l,"ToggleJournal");
                l.player.transform.position=l.winterGate.position+Vector3.up*4;l.player.GetComponent<Rigidbody2D>().position=l.player.transform.position;
                Advance(102);
            }
            else if(stage==101||stage==102)
            {
                var sequence=UnityEngine.Object.FindAnyObjectByType<AmbrosJourney.Journey.BossPortalSequence>();
                if(!sequence||!sequence.Exit.Ready)return;
                var player=AmbrosJourney.Journey.JourneySession.Instance.Player();player.position=sequence.Exit.Portal.position;player.GetComponent<Rigidbody2D>().position=player.position;
                if(stage==101)Check(FallPortalBridge.GateAllowsEntry(UnityEngine.Object.FindAnyObjectByType<SummerLevel>()),"Revealed Summer portal permits entry");
                else Check(WinterPortalBridge.GateAllowsEntry(UnityEngine.Object.FindAnyObjectByType<FallLevel>()),"Revealed Fall portal permits entry");
                Advance(stage==101?2:3);
            }
            else if(stage==3&&ArrivalReady("Winter"))
            {
                Check(!UnityEngine.Object.FindAnyObjectByType<FallLevel>(),"Fall unloaded");
                var winter=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();
                Check(winter&&!winter.Progress.Introduced&&!winter.InputBlocked,"Winter arrives silently with free movement until E at Little Mage");
                DirectGuidanceChecks.Verify(4);AmbrosJourney.Journey.Editor.JourneyPerformanceChecks.VerifyCurrentScene();
                CheckMotion(UnityEngine.Object.FindAnyObjectByType<WinterLevel>().player,"movement",4.6f);BossRefreshChecks.CheckAndCapture(UnityEngine.Object.FindAnyObjectByType<WinterLevel>().winterGuardian.GetComponent<SeasonBossVisual>(),"WinterFrostbound");Capture("WinterArrival");Advance(4);
            }
            else if(stage==4){AmbrosJourney.Winter.Editor.WinterPlayChecks.Run();Advance(5);}
            else if(stage==5){if(AmbrosJourney.Winter.Editor.WinterPlayChecks.Running)return;BossRefreshChecks.CheckAndCapture(UnityEngine.Object.FindAnyObjectByType<WinterLevel>().guardian.GetComponent<SeasonBossVisual>(),"FinalEchoKeeper");Capture("WinterCavern");Advance(6);}
            else if(stage==6)
            {
                var l=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();l.player.transform.position=l.caveExit.position;l.player.GetComponent<Rigidbody2D>().position=l.caveExit.position;l.World.ChangeRegion(false);l.player.transform.position=WinterWorld.Entrance+Vector2.down*1.5f;l.player.GetComponent<Rigidbody2D>().position=l.player.transform.position;Advance(7);
            }
            else if(stage==7){Capture("WinterEntrance");Advance(8);}
            else if(stage>=8)
            {
                // LoadChapter activates its scene before restoring the in-memory chapter.
                // Wall-clock delays cannot replace waiting for that operation to finish.
                var session=AmbrosJourney.Journey.JourneySession.Instance;
                if(session&&(bool)JourneyLoading.GetValue(session))return;
                if(AmbrosJourney.Journey.Editor.JourneyPlayChecks.Step(stage-8))Finish(true,"All four seasons, both Winter bosses, menus, current-run chapter revisits, artifacts, certificate and reset passed.");
                else Advance(stage+1);
            }
        }
        catch (Exception e) { Finish(false, e.ToString()); }
    }
    static void CompleteSpringGreeting(SpringGuide guide)
    {
        var originalPosition=guide.player.position;var body=guide.player.GetComponent<Rigidbody2D>();var originalVelocity=body.linearVelocity;
        var previousKeyboard=Keyboard.current;var keyboard=InputSystem.AddDevice<Keyboard>();keyboard.MakeCurrent();
        Action<InputKey> press=key=>{
            var keyDevice=InputSystem.AddDevice<Keyboard>();keyDevice.MakeCurrent();
            try
            {
                InputSystem.QueueStateEvent(keyDevice,new KeyboardState(key));InputSystem.Update();
                Check(keyDevice[key].wasPressedThisFrame,"Mage lesson receives the injected "+key+" edge");
                Call(AmbrosJourney.Journey.JourneySession.Instance,"Update");Call(guide,"Update");
                InputSystem.QueueStateEvent(keyDevice,new KeyboardState());InputSystem.Update();
            }
            finally{InputSystem.RemoveDevice(keyDevice);keyboard.MakeCurrent();}
        };
        try
        {
            guide.player.position=guide.mage.position+Vector3.right*.8f;body.position=guide.player.position;Physics2D.SyncTransforms();
            Call(guide,"Update");Check(!guide.DialogueOpen&&!guide.LessonComplete,"Walking up to the Mage does not start dialogue");
            press(InputKey.E);Check(guide.DialogueOpen&&!guide.LessonComplete,"E starts the Mage lesson");
            for(int page=0;page<3;page++)
            {
                press(InputKey.Enter);
                Check(guide.LessonComplete==(page==2),"Mage lesson completes only after all three pages");
            }
            Check(!guide.DialogueOpen,"Finishing the Mage lesson returns movement control");
        }
        finally
        {
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();InputSystem.RemoveDevice(keyboard);if(previousKeyboard!=null)previousKeyboard.MakeCurrent();
            guide.player.position=originalPosition;body.position=originalPosition;body.linearVelocity=originalVelocity;Physics2D.SyncTransforms();
        }
    }
    static void CheckMotion(MonoBehaviour actor,string input,float speed)
    {
        var body=actor.GetComponent<Rigidbody2D>();var oldVelocity=body.linearVelocity;
        if(actor is SpringPlayer spring)speed=spring.speed;
        var springGuide=actor is SpringPlayer sp?sp.guide:null;bool wasDialogue=springGuide&&springGuide.DialogueOpen;
        var field=actor.GetType().GetField(input,Private);var oldInput=field.GetValue(actor);
        string ownerName=input=="input"?"guide":"level";var owner=actor.GetType().GetField(ownerName);var originalOwner=owner.GetValue(actor);
        var modal=originalOwner!=null&&!springGuide?originalOwner.GetType().GetField("dialog",Private)?.GetValue(originalOwner) as GameObject:null;
        bool modalWasActive=modal&&modal.activeSelf;
        try
        {
            // Exercise the actual controller physics methods without advancing the player's position.
            owner.SetValue(actor,null);body.linearVelocity=Vector2.zero;field.SetValue(actor,Vector2.right);
            Call(actor,"FixedUpdate");Check(body.linearVelocity.x>0&&body.linearVelocity.x<speed,"Movement accelerates without snapping: velocity="+body.linearVelocity+", blocked="+AmbrosJourney.Journey.JourneySession.Blocked+", fixed dt="+Time.fixedDeltaTime);
            for(int i=0;i<20;i++)Call(actor,"FixedUpdate");Check(Mathf.Abs(body.linearVelocity.x-speed)<.001f,"Movement reaches intended speed");
            field.SetValue(actor,Vector2.zero);for(int i=0;i<5;i++)Call(actor,"FixedUpdate");Check(body.linearVelocity==Vector2.zero,"Released movement brakes within five physics ticks");
            owner.SetValue(actor,originalOwner);if(springGuide)typeof(SpringGuide).GetProperty("DialogueOpen").SetValue(springGuide,true);else {Check(modal,"Season motion test has a dialog");modal.SetActive(true);}body.linearVelocity=Vector2.right*speed;Call(actor,"FixedUpdate");Check(body.linearVelocity==Vector2.zero,"Open field-note or quiz modal immediately stops movement");
            Check(body.interpolation==RigidbodyInterpolation2D.Interpolate,"Player physics is interpolated");
            var camera=Camera.main;var originalPosition=camera.transform.position;var damping=Vector3.one;
            camera.transform.position=actor.transform.position+new Vector3(100,100,-10);
            AmbrosJourney.Presentation.SeasonCameraMotion.Follow(camera,actor.transform,new Vector2(-200,-200),new Vector2(200,200),ref damping);
            Check(Vector2.Distance(camera.transform.position,actor.transform.position)<.01f&&damping==Vector3.zero,"Long camera transitions snap and clear damping");camera.transform.position=originalPosition;
            Debug.Log("MOTION_CHECK_PASS: "+actor.GetType().Name);
        }
        finally{if(modal)modal.SetActive(modalWasActive);if(springGuide)typeof(SpringGuide).GetProperty("DialogueOpen").SetValue(springGuide,wasDialogue);owner.SetValue(actor,originalOwner);field.SetValue(actor,oldInput);body.linearVelocity=oldVelocity;}
    }
    static void CheckOverlay(MonoBehaviour level)
    {
        var ui=level.GetComponentInChildren<AmbrosJourney.Presentation.SeasonOverlay>();
        Call(ui,"LateUpdate");Canvas.ForceUpdateCanvases();
        foreach(var t in ui.GetComponentsInChildren<TMPro.TextMeshProUGUI>()){t.ForceMeshUpdate();Check(!t.isTextOverflowing,"UI overflow: "+t.name+" / "+ui.Heading.text);}
    }
    public static void Capture(string name)
    {
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1024,768)})
        {
            var camera=Camera.main;if(!camera)camera=UnityEngine.Object.FindAnyObjectByType<Camera>();Check(camera,"Screenshot camera");
            var canvases=UnityEngine.Object.FindObjectsByType<Canvas>();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var planes=canvases.Select(c=>c.planeDistance).ToArray();var orders=canvases.Select(c=>c.sortingOrder).ToArray();
            // Lift UI above world sprites while preserving the overlay stack (veil < HUD < dialogs).
            var rankedOrders=orders.Distinct().OrderBy(order=>order).ToArray();
            var visibility=UnityEngine.Object.FindAnyObjectByType<CaveVisibility>();
            var follow=camera.GetComponents<MonoBehaviour>().FirstOrDefault(b=>b is SpringCamera||b is SummerCamera||b is FallCamera||b is WinterCamera);
            var smoothing=follow?follow.GetType().GetField("smoothingVelocity",Private):null;var oldSmoothing=smoothing?.GetValue(follow);var oldPosition=camera.transform.position;
            var overlay=UnityEngine.Object.FindAnyObjectByType<SeasonOverlay>();
            var oldTarget=camera.targetTexture;float aspect=camera.aspect;var active=RenderTexture.active;var rt=new RenderTexture(size.x,size.y,24);var pixels=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=rt;camera.aspect=(float)size.x/size.y;
                if(follow)Call(follow,"LateUpdate"); // Use gameplay framing for the requested viewport.
                foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;canvas.sortingOrder=30000+Array.IndexOf(rankedOrders,canvas.sortingOrder);}
                if(visibility)Call(visibility,"LateUpdate");
                Canvas.ForceUpdateCanvases();if(overlay)Call(overlay,"LateUpdate");Canvas.ForceUpdateCanvases();
                foreach(var canvas in canvases)foreach(var t in canvas.GetComponentsInChildren<TMPro.TextMeshProUGUI>()){t.ForceMeshUpdate();Check(!t.isTextOverflowing,name+" overflow at "+size+": "+t.name+" / "+t.text+" rect="+t.rectTransform.rect.size+" preferred="+t.GetPreferredValues()+" pointSize="+t.fontSize);}
                camera.Render();RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,size.x,size.y),0,0);pixels.Apply();File.WriteAllBytes("Logs/SeasonChecks/"+name+"-"+size.x+".png",pixels.EncodeToPNG());
            }
            finally{for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=planes[i];canvases[i].sortingOrder=orders[i];}camera.targetTexture=oldTarget;camera.aspect=aspect;camera.transform.position=oldPosition;if(smoothing!=null)smoothing.SetValue(follow,oldSmoothing);if(visibility)Call(visibility,"LateUpdate");RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(pixels);UnityEngine.Object.DestroyImmediate(rt);Canvas.ForceUpdateCanvases();if(overlay)Call(overlay,"LateUpdate");Canvas.ForceUpdateCanvases();}
        }
    }
    static void Advance(int stage) { Debug.Log("SEASON_CHECK_STAGE: " + stage); SessionState.SetInt(Key + "stage", stage); readyAt = EditorApplication.timeSinceStartup + 4; }
    static void Finish(bool passed, string message)
    {
        SessionState.SetBool(Key + "active", false);
        File.WriteAllText("Logs/SeasonChecks/Integration.txt", (passed ? "PASS" : "FAIL") + "\n" + message);
        Debug.Log("SEASON_INTEGRATION_" + (passed ? "PASS: " : "FAIL: ") + message);
        EditorApplication.Exit(passed ? 0 : 1);
    }
    public static void Build()
    {
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[]{"Assets/Opening/Intro.unity", "Assets/Scenes/Spring.unity"},
            locationPathName = "Logs/SeasonChecks/AmbrosJourney.app", target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        });
        Directory.CreateDirectory("Logs/SeasonChecks");
        File.WriteAllText("Logs/SeasonChecks/Build.txt", report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings);
        Check(report.summary.result == BuildResult.Succeeded, "Connected game build failed");
    }
}
