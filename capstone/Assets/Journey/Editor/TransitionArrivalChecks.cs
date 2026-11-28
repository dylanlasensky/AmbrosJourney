// Runs isolated Editor regression checks for transition arrival.
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using InputKey=UnityEngine.InputSystem.Key;
using AmbrosJourney.Journey;
using AmbrosJourney.Presentation;
using AmbrosJourney.Summer;
using AmbrosJourney.Fall;
using AmbrosJourney.Winter;

// Uses real Resources worlds and portal coroutines in a disposable batch-mode run.
[InitializeOnLoad]
public static class TransitionArrivalChecks
{
    const string Key="TransitionArrivalChecks.";
    const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    static readonly string[] Names={"Spring","Summer","Fall","Winter"};
    static readonly string[] Prefabs={"","SummerLevel","FallLevel","WinterLevel"};
    static readonly MethodInfo SignalTick=typeof(EditorApplication).GetMethod("SignalTick",BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public);
    static int stage,destination,readyFrame,count,lastFrame=-1,expectedPortal;
    static Scene source;
    static AsyncOperation unloading;
    static bool observedPortal;

    static TransitionArrivalChecks()
    {
        EditorApplication.update+=Tick;
        SceneManager.activeSceneChanged+=Changed;
        Application.logMessageReceived+=(message,stack,type)=>{
            if(SessionState.GetBool(Key+"active",false)&&(type==LogType.Error||type==LogType.Exception||type==LogType.Assert))
                SessionState.SetString(Key+"error",message);
        };
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Isolate()
    {
        if(!SessionState.GetBool(Key+"active",false))return;
        JourneySession.TestMode=true;
    }
    public static void Run()
    {
        if(!Application.isBatchMode)throw new InvalidOperationException("Run transition checks in a disposable batch-mode Editor.");
        Directory.CreateDirectory("Logs/TransitionChecks");
        File.WriteAllText("Logs/TransitionChecks/Results.txt","RUNNING\n");
        SessionState.SetBool(Key+"active",true);SessionState.SetString(Key+"error","");
        SessionState.SetString(Key+"deadline",DateTime.UtcNow.AddMinutes(5).ToString("O"));
        EditorSceneManager.OpenScene("Assets/Scenes/Spring.unity");
        EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();
        Application.runInBackground=true;EditorApplication.isPlaying=true;
    }
    static void Check(bool ok,string message){count++;if(!ok)throw new Exception(message);}
    static T InScene<T>(Scene scene) where T:Component
        =>scene.IsValid()&&scene.isLoaded?scene.GetRootGameObjects().SelectMany(root=>root.GetComponentsInChildren<T>()).FirstOrDefault():null;
    static Vector2 Spawn(int chapter)=>chapter==2?SummerLevel.Spawn:chapter==3?FallLevel.Spawn:WinterLevel.Spawn;
    static Transform PlayerInScene(int chapter,Scene scene)
    {
        if(chapter==1)return InScene<SpringPlayer>(scene)?.transform;
        if(chapter==2)return InScene<SummerLevel>(scene)?.player?.transform;
        if(chapter==3)return InScene<FallLevel>(scene)?.player?.transform;
        return InScene<WinterLevel>(scene)?.player?.transform;
    }
    static void Move(Transform player,Vector2 position)
    {
        player.position=position;var body=player.GetComponent<Rigidbody2D>();
        if(body){body.position=position;body.linearVelocity=Vector2.zero;}Physics2D.SyncTransforms();
    }
    static void Changed(Scene previous,Scene next)
    {
        if(!SessionState.GetBool(Key+"active",false)||expectedPortal==0||next.name!=Names[expectedPortal-1])return;
        observedPortal=true;
        if(previous.GetRootGameObjects().Any(root=>root.activeInHierarchy))
            SessionState.SetString(Key+"error","The actual "+next.name+" portal activated its destination before disabling all source roots.");
    }
    static void BeginOverlap(int chapter)
    {
        destination=chapter;source=SceneManager.GetActiveScene();
        var session=JourneySession.Instance;var previousPlayer=session.Player();
        Check(previousPlayer&&previousPlayer.gameObject.scene==source,"Source player belongs to source scene");
        Move(previousPlayer,new Vector2(0,chapter==2?-33:chapter==3?-30:-46));
        var next=SceneManager.CreateScene(Names[chapter-1]);SceneManager.SetActiveScene(next);
        Check(session.Player()==null,"Empty active "+next.name+" cannot borrow the outgoing player");
        var prefab=Resources.Load<GameObject>(Prefabs[chapter-1]);Check(prefab,"Destination Resources prefab exists");
        var world=UnityEngine.Object.Instantiate(prefab);SceneManager.MoveGameObjectToScene(world,next);
        // No frame is advanced while both physics worlds are active. Build only the real
        // destination's base world, check identity, then retire the source before its Start/UI work.
        var builder=world.GetComponents<MonoBehaviour>().First(component=>component.GetType().GetMethod("BuildWorld")!=null);
        builder.GetType().GetMethod("BuildWorld").Invoke(builder,null);
        var player=PlayerInScene(chapter,next);Check(player,"Destination created its own player");
        Check(previousPlayer.gameObject.activeInHierarchy,"Regression fixture retains the source player for the lookup");
        Check(session.Player()==player,"Active "+next.name+" player wins while outgoing player still exists");
        foreach(var root in source.GetRootGameObjects())root.SetActive(false);
        readyFrame=Time.frameCount+4;stage=1;
    }
    static void VerifyArrival(int chapter)
    {
        var scene=SceneManager.GetActiveScene();var session=JourneySession.Instance;var player=PlayerInScene(chapter,scene);
        Check(scene.name==Names[chapter-1]&&player&&session.Player()==player,"Arrival resolves the current chapter player");
        Check(Vector2.Distance(player.position,Spawn(chapter))<.01f,Names[chapter-1]+" starts at its arrival spawn, not the outgoing exit");
        var exploration=InScene<JourneyWorld>(scene);
        Check(exploration,"Destination owns its exploration world after initialization");
        var ripples=(List<Transform>)typeof(JourneyWorld).GetField("ripples",Hidden).GetValue(exploration);
        var lanterns=(List<SpriteRenderer>)typeof(JourneyWorld).GetField("lanterns",Hidden).GetValue(exploration);
        Check(ripples.All(r=>r&&r.gameObject.scene==scene),"Every live water reflection belongs to its own destination scene before source unload");
        Check(lanterns.All(r=>r&&r.gameObject.scene==scene),"Lantern animation never retains an outgoing-scene renderer");
        var cohesion=exploration.GetComponent<WorldCohesion>();
        var original=(SpriteRenderer[])typeof(WorldCohesion).GetField("original",Hidden).GetValue(cohesion);
        Check(original.All(r=>!r||r.gameObject.scene==scene),"World art only reskins and places its own scene's objects");
        Check(session.Data.chapter==chapter&&session.Data.unlocked>=chapter,"In-memory chapter/unlock follows the destination");
        if(chapter==2)
        {
            var level=InScene<SummerLevel>(scene);var p=level.Progress;
            Check(!p.Introduced&&p.WardCount==0&&!p.Complete&&p.Valid(),"Fresh Summer starts with no completed lessons or guardian");
            Physics2D.SyncTransforms();
            var route=SeasonNavigation.FindPath(player.position,level.workshop.position,new Vector2(-30,-32),new Vector2(30,28));
            Check(route.Count>0,"Arrival has a real collision-aware path to the first workshop lesson");
            Vector2 previous=player.position;
            foreach(var point in route){Check(SeasonNavigation.ClearSegment(previous,point),"Workshop approach does not cross scenery or a locked seal");previous=point;}
            Check(Vector2.Distance(previous,level.workshop.position)<.01f,"Workshop route reaches the actual lesson interaction point");
        }
        else if(chapter==3){var p=InScene<FallLevel>(scene).Progress;Check(!p.Introduced&&p.RuneCount==0&&!p.Complete&&p.Valid(),"Fresh Fall starts with no completed lessons or guardian");}
        else {var p=InScene<WinterLevel>(scene).Progress;Check(!p.Introduced&&!p.WinterComplete&&!p.Complete&&!p.InCave&&p.Valid(),"Fresh Winter starts outside with both bosses unfinished");}
    }
    static void VerifyManualGreeting(int chapter)
    {
        var scene=SceneManager.GetActiveScene();var player=JourneySession.Instance.Player();
        MonoBehaviour level;Transform guide;Func<bool> introduced,open,blocked;Action restore;string initial;
        Func<string> progress;
        if(chapter==2)
        {
            var summer=InScene<SummerLevel>(scene);level=summer;guide=summer.maleWarrior.transform;
            progress=()=>JsonUtility.ToJson(summer.Progress);initial=progress();
            introduced=()=>summer.Progress.Introduced;open=()=>summer.IsDialogueOpen;blocked=()=>summer.InputBlocked;
            restore=()=>summer.RestoreProgress(JsonUtility.FromJson<SummerProgress>(initial));
        }
        else if(chapter==3)
        {
            var fall=InScene<FallLevel>(scene);level=fall;guide=fall.ember.transform;
            progress=()=>JsonUtility.ToJson(fall.Progress);initial=progress();
            introduced=()=>fall.Progress.Introduced;open=()=>fall.IsDialogueOpen;blocked=()=>fall.InputBlocked;
            restore=()=>fall.RestoreProgress(JsonUtility.FromJson<FallProgress>(initial));
        }
        else
        {
            var winter=InScene<WinterLevel>(scene);level=winter;guide=winter.littleMage.transform;
            progress=()=>JsonUtility.ToJson(winter.Progress);initial=progress();
            introduced=()=>winter.Progress.Introduced;open=()=>winter.IsDialogueOpen;blocked=()=>winter.InputBlocked;
            restore=()=>winter.RestoreProgress(JsonUtility.FromJson<WinterProgress>(initial));
        }
        var position=player.position;var body=player.GetComponent<Rigidbody2D>();var velocity=body.linearVelocity;
        var previousKeyboard=Keyboard.current;var keyboard=InputSystem.AddDevice<Keyboard>();keyboard.MakeCurrent();
        Action tick=()=>level.GetType().GetMethod("Update",Hidden).Invoke(level,null);
        Action<InputKey> press=key=>{
            // Each synchronous assertion is a separate logical input frame. Give it a
            // fresh device and restore the Escape-consumption guard after observing it.
            var keyDevice=InputSystem.AddDevice<Keyboard>();keyDevice.MakeCurrent();
            var session=JourneySession.Instance;var resume=typeof(JourneySession).GetField("resumeFrame",Hidden);var before=resume.GetValue(session);
            try
            {
                InputSystem.QueueStateEvent(keyDevice,new KeyboardState(key));InputSystem.Update();
                Check(Keyboard.current==keyDevice&&keyDevice[key].wasPressedThisFrame,"Injected "+key+" edge delivered for "+Names[chapter-1]+"; current="+(Keyboard.current==keyDevice)+", held="+keyDevice[key].isPressed);
                typeof(JourneySession).GetMethod("Update",Hidden).Invoke(session,null);
                if(key==InputKey.Escape)Check(JourneySession.Blocked,"Global Escape consumes the close before the chapter sees that same input");
                tick();
                InputSystem.QueueStateEvent(keyDevice,new KeyboardState());InputSystem.Update();
            }
            finally{resume.SetValue(session,before);InputSystem.RemoveDevice(keyDevice);keyboard.MakeCurrent();}
        };
        try
        {
            Check(!introduced()&&!open()&&!blocked(),Names[chapter-1]+" arrival is silent and permits movement until deliberate interaction");
            Check(level.GetComponentsInChildren<CompanionSpeech>(true).All(speech=>!speech.Visible),"Arrival never starts an automatic companion caption");
            Move(player,(Vector2)guide.position+Vector2.right*10);press(InputKey.E);
            Check(!introduced()&&!open(),"E away from the guide cannot start an arrival conversation");
            Move(player,(Vector2)guide.position+Vector2.right*.8f);tick();
            Check(!introduced()&&!open()&&!blocked(),"Walking beside the guide never automatically opens dialogue");
            int nearest=(int)level.GetType().GetMethod("Nearest",Hidden).Invoke(level,null);
            Check(nearest==0||chapter==2&&nearest==9,"Nearby guide is selected by the actual "+Names[chapter-1]+" interaction resolver: "+nearest);
            press(InputKey.E);
            Check(open()&&blocked()&&!introduced(),"Nearby E opens the greeting; arrival progress waits for dismissal: chapter="+Names[chapter-1]+", nearest="+nearest+", open="+open()+", blocked="+blocked()+", introduced="+introduced()+", sessionBlocked="+JourneySession.Blocked+", transitioning="+level.GetType().GetField("Transitioning").GetValue(level));
            press(InputKey.Enter);
            Check(introduced()&&!open()&&!blocked(),"Enter closes the greeting and returns movement control");
            if(chapter==2){var summer=(SummerLevel)level;Check(summer.maleWarrior.Guidance!=null&&summer.femaleWarrior.Guidance!=null,"Summer escort starts only after the greeting");}
            else if(chapter==3)Check(((FallLevel)level).ember.Guidance!=null,"Fall escort starts only after the greeting");
            press(InputKey.E);Check(open(),"An introduced guide still requires E to explain the next task");
            press(InputKey.Escape);Check(!open()&&!blocked(),"Escape closes a guide conversation without trapping movement");
        }
        finally
        {
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
            InputSystem.RemoveDevice(keyboard);if(previousKeyboard!=null)previousKeyboard.MakeCurrent();
            restore();Move(player,position);body.linearVelocity=velocity;
        }
        Check(progress()==initial,"Conversation checks restore exact fresh progression before the next handover");
    }
    static void BeginPortal(int chapter)
    {
        expectedPortal=chapter;observedPortal=false;destination=chapter;
        var scene=SceneManager.GetActiveScene();MonoBehaviour bridge;string method;
        if(chapter==2)
        {
            var guide=InScene<SpringGuide>(scene);
            guide.RestoreProgress(new SpringState{controlsSeen=true,lessonComplete=true,complete=true,missions=3,bossScore=4,cleared=new[]{true,true,true},studied=new[]{true,true,true},trials=new[]{2,2,2}});
            Move(guide.player,guide.ExitPortal.position);
            bridge=UnityEngine.Object.FindAnyObjectByType<SummerPortalBridge>();method="EnterSummer";
            typeof(SummerPortalBridge).GetField("spring",Hidden).SetValue(bridge,guide);
        }
        else if(chapter==3)
        {
            var level=InScene<SummerLevel>(scene);var progress=new SummerProgress{Introduced=true,GuideArrived=true,WorkshopQuestion=2,GardenQuestion=1,Wards=new[]{true,true,true}};
            Check(progress.BeginBoss(),"Prepared Summer lessons permit its guardian");foreach(var q in SummerContent.Boss)Check(progress.AnswerBoss(q.Correct),"Prepared Summer answer is valid");
            level.RestoreProgress(progress);Move(level.player.transform,level.fallGate.position);
            bridge=UnityEngine.Object.FindAnyObjectByType<FallPortalBridge>();method="EnterFall";
            typeof(FallPortalBridge).GetField("summer",Hidden).SetValue(bridge,level);
        }
        else
        {
            var level=InScene<FallLevel>(scene);var progress=new FallProgress{Introduced=true,GuideArrived=true};
            for(int i=0;i<3;i++){Check(progress.ReadNote(i),"Prepared Fall note is valid");foreach(var q in FallContent.Missions[i])Check(progress.AnswerMission(i,q.Correct),"Prepared Fall answer is valid");}
            Check(progress.BeginBoss(),"Prepared Fall lessons permit its guardian");foreach(var q in FallContent.Boss)Check(progress.AnswerBoss(q.Correct),"Prepared Fall boss answer is valid");
            level.RestoreProgress(progress);Move(level.player.transform,level.winterGate.position);
            bridge=UnityEngine.Object.FindAnyObjectByType<WinterPortalBridge>();method="EnterWinter";
            typeof(WinterPortalBridge).GetField("fall",Hidden).SetValue(bridge,level);
        }
        Check(bridge,"Persistent portal bridge exists");
        bridge.StartCoroutine((IEnumerator)bridge.GetType().GetMethod(method,Hidden).Invoke(bridge,null));
        Check(SeasonTransition.Active,"Actual portal owns the fade while handing off scenes");
        var session=JourneySession.Instance;var data=JsonUtility.ToJson(session.Data);var current=SceneManager.GetActiveScene();
        Check(JourneySession.Blocked,"The portal fade blocks gameplay input");
        session.HandleBack();session.Pause();session.ShowArtifacts();session.LoadChapter(JourneySession.CurrentChapter());
        Check(!session.Paused&&Time.timeScale==1,"Escape, pause and artifacts cannot interrupt the portal fade");
        foreach(string state in new[]{"menu","loading","artifactsOpen"})
            Check(!(bool)typeof(JourneySession).GetField(state,Hidden).GetValue(session),"Fade cannot open a menu or start another chapter load: "+state);
        Check(SceneManager.GetActiveScene()==current&&JsonUtility.ToJson(session.Data)==data,"Ignored fade controls preserve the active scene and in-memory progress");
        readyFrame=Time.frameCount+4;stage=4;
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key+"active",false))return;
        try
        {
            var error=SessionState.GetString(Key+"error","");Check(error=="",error);
            Check(DateTime.UtcNow<DateTime.Parse(SessionState.GetString(Key+"deadline","")).ToUniversalTime(),"Transition checks timed out at stage "+stage);
            Application.runInBackground=true;EditorApplication.QueuePlayerLoopUpdate();SignalTick?.Invoke(null,null);
            if(!EditorApplication.isPlaying||Time.frameCount==lastFrame)return;lastFrame=Time.frameCount;
            JourneySession.TestMode=true;var session=JourneySession.Instance;if(!session)return;
            if(stage==0)
            {
                var scene=SceneManager.GetActiveScene();var guide=InScene<SpringGuide>(scene);
                if(Time.frameCount<8||!guide||!InScene<JourneyWorld>(scene))return;
                Check(!session.Data.started&&session.Data.playerName==""&&session.Data.unlocked==1&&session.Data.ArtifactCount==0&&!session.Data.finished,"Initial Awake ignores legacy saves and creates fresh session data before test setup");
                session.BeginTestProfile();session.Resume();guide.DismissPanel();
                Check(session.Data.unlocked==1&&session.Data.ArtifactCount==0&&!session.Data.finished,"New in-memory run starts clean");
                BeginOverlap(2);
            }
            else if(stage==1&&Time.frameCount>=readyFrame)
            {
                VerifyArrival(destination);VerifyManualGreeting(destination);unloading=SceneManager.UnloadSceneAsync(source);stage=2;
            }
            else if(stage==2&&(unloading==null||unloading.isDone))
            {
                if(destination<4)BeginOverlap(destination+1);
                else
                {
                    session.BeginTestProfile();Check(session.Data.unlocked==1&&session.Data.ArtifactCount==0&&!session.Data.finished,"Another run has no prior unlocks, artifacts or completion");
                    SceneManager.LoadScene("Spring");readyFrame=Time.frameCount+4;stage=3;
                }
            }
            else if(stage==3&&Time.frameCount>=readyFrame&&InScene<JourneyWorld>(SceneManager.GetActiveScene()))BeginPortal(2);
            else if(stage==4&&Time.frameCount>=readyFrame&&observedPortal&&!SeasonTransition.Active)
            {
                VerifyArrival(destination);VerifyManualGreeting(destination);expectedPortal=0;
                Check(SceneManager.sceneCount==1,"Outgoing gameplay scene has unloaded after portal fade");
                if(destination<4)BeginPortal(destination+1);
                else Finish(true,"All three active-scene player lookups, canonical arrival spawns, fresh in-memory progression, explicit E greetings and actual portal handovers passed; source roots retire before destination activation.");
            }
        }
        catch(Exception error){Finish(false,error.ToString());}
    }
    static void Finish(bool passed,string message)
    {
        SessionState.SetBool(Key+"active",false);expectedPortal=0;Time.timeScale=1;
        File.WriteAllText("Logs/TransitionChecks/Results.txt",(passed?"PASS":"FAIL")+"\n"+message+"\nChecks: "+count);
        Debug.Log("TRANSITION_ARRIVAL_"+(passed?"PASS: ":"FAIL: ")+message);EditorApplication.Exit(passed?0:1);
    }
}
#endif
