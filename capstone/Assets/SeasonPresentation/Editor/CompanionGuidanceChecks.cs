// Runs isolated Editor regression checks for companion guidance.
#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using AmbrosJourney.Journey;
using AmbrosJourney.Summer;
using AmbrosJourney.Fall;
using AmbrosJourney.Winter;

namespace AmbrosJourney.Presentation.Editor
{
    // Exercise the actual E/Escape handlers in each disposable chapter, then restore its state.
    public static class CompanionGuidanceChecks
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static void Check(bool value,string why){if(!value)throw new Exception("COMPANION GUIDANCE: "+why);}
        static void Call(object owner,string method,params object[] args)=>owner.GetType().GetMethod(method,Private).Invoke(owner,args);
        static void Move(Transform actor,Vector3 position){actor.position=position;var body=actor.GetComponent<Rigidbody2D>();if(body){body.position=position;body.linearVelocity=Vector2.zero;}Physics2D.SyncTransforms();}
        static void Press(MonoBehaviour owner,Key key)
        {
            var session=JourneySession.Instance;var guard=typeof(JourneySession).GetField("resumeFrame",Private);int previousGuard=(int)guard.GetValue(session);
            var previous=Keyboard.current;var keyboard=InputSystem.AddDevice<Keyboard>();keyboard.MakeCurrent();
            try
            {
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));InputSystem.Update();
                Check(Keyboard.current==keyboard&&keyboard[key].wasPressedThisFrame,"Synthetic input reaches the active keyboard");
                Call(session,"Update");
                if(key==Key.Escape)Check(JourneySession.Blocked,"Global Escape owner protects the rest of the same input frame");
                Call(owner,"Update");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard);if(previous!=null)previous.MakeCurrent();
                // These synchronous checks share one Unity frame. The tested guard must not
                // leak into the next simulated key press or the caller's real input frame.
                guard.SetValue(session,previousGuard);
            }
        }
        static void NoCaption(SeasonOverlay ui)
        {
            var caption=ui.transform.Find("Companion caption");Check(!caption||!caption.gameObject.activeSelf,"Guides never add unsolicited speech captions");
        }
        static void InteractiveName(Transform actor)
        {
            var label=actor.GetComponentInChildren<SeasonWorldLabel>(true);
            Check(label&&label.enabled&&label.Caption.Contains("[E]")&&Mathf.Abs(label.RevealDistance-4)<.001f,"Nearby guide advertises the real E interaction range");
        }
        static void ReadableDialogue(SeasonOverlay ui,bool blocked)
        {
            Check(ui.Dialog.activeSelf&&blocked,"Deliberate conversation pauses character input");NoCaption(ui);
            Canvas.ForceUpdateCanvases();
            foreach(var text in ui.Dialog.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,"Conversation fits: "+text.name);}
        }
        static void StopsPlayer(MonoBehaviour actor)
        {
            var body=actor.GetComponent<Rigidbody2D>();body.linearVelocity=Vector2.right*4;Call(actor,"FixedUpdate");
            Check(body.linearVelocity==Vector2.zero,"A conversation immediately stops existing player velocity");
        }
        public static void Spring(SpringGuide level)
        {
            JourneySession.Instance.Resume();Call(level,"Update");level.DismissPanel();Call(level,"LateUpdate");
            var initial=level.CaptureProgress();Vector3 position=level.player.position;
            try
            {
                level.RestoreProgress(new SpringState{controlsSeen=true});Call(level,"LateUpdate");
                Check(!level.LessonComplete&&!level.DialogueOpen&&level.MissionsComplete==0,"Closing controls or restoring a fresh chapter never completes the Mage conversation");
                var ui=level.GetComponentInChildren<SeasonOverlay>();NoCaption(ui);InteractiveName(level.mage);
                Move(level.player,level.mage.position+Vector3.right*3.5f);
                Check((int)level.GetType().GetMethod("Nearby",Private).Invoke(level,null)==-1,"Mage is reachable with E from a comfortable distance");
                Press(level,Key.E);Call(level,"LateUpdate");ReadableDialogue(ui,level.DialogueOpen);
                Press(level,Key.Escape);Check(!level.DialogueOpen&&!level.LessonComplete,"Escape leaves an unfinished Mage lesson available to reopen");
                Press(level,Key.E);
                for(int page=0;page<3;page++){Call(level,"LateUpdate");ReadableDialogue(ui,level.DialogueOpen);Press(level,Key.E);}
                Check(level.LessonComplete&&!level.DialogueOpen&&level.MissionsComplete==0,"Only the completed deliberate Mage conversation enables the field lessons");
                var introduced=level.CaptureProgress();level.RestoreProgress(introduced);Call(level,"LateUpdate");
                Check(level.LessonComplete&&!level.DialogueOpen,"Current-run restoration keeps the Mage lesson without reopening dialogue");NoCaption(ui);
            }
            finally{level.RestoreProgress(initial);Move(level.player,position);JourneySession.Instance.CaptureChapter();}
        }
        public static void Summer(SummerLevel level)
        {
            var initial=JsonUtility.FromJson<SummerProgress>(JsonUtility.ToJson(level.Progress));var positions=new[]{level.player.transform.position,level.maleWarrior.transform.position,level.femaleWarrior.transform.position};
            try
            {
                Move(level.player.transform,SummerLevel.Spawn);level.RestoreProgress(new SummerProgress());var ui=level.GetComponentInChildren<SeasonOverlay>();
                Check(!level.Progress.Introduced&&!level.InputBlocked&&level.maleWarrior.Guidance==null&&level.femaleWarrior.Guidance==null,"Warriors wait silently at arrival for E");NoCaption(ui);InteractiveName(level.maleWarrior.transform);InteractiveName(level.femaleWarrior.transform);
                Press(level,Key.E);ReadableDialogue(ui,level.InputBlocked);StopsPlayer(level.player);
                Check(!level.Progress.Introduced&&level.maleWarrior.Guidance==null,"Opening greeting does not start walking behind its dialogue");
                Press(level,Key.Escape);Check(level.Progress.Introduced&&!level.InputBlocked&&level.maleWarrior.Guidance!=null,"Closing the deliberate greeting starts the walking escort");
                Press(level,Key.E);Check(!level.IsDialogueOpen&&!level.InputBlocked,"Completed arrival greeting does not reopen during the escort");
                Move(level.player.transform,level.workshop.position);level.Interact(1);Check(level.Progress.GuideArrived,"Reading the workshop retires orientation even before guides arrive");level.DismissPanel();Move(level.player.transform,level.garden.position);Call(level,"RefreshWorld");
                Check(!level.InputBlocked&&!ui.Objective.text.Contains("Walk with"),"Outpacing Warriors cannot leave a stale escort objective");
                for(int i=0;i<1000&&!level.maleWarrior.Guidance.Arrived;i++)level.maleWarrior.Guidance.Step(level.maleWarrior.transform,level.player.transform,false,.1f);
                Check(level.maleWarrior.Guidance.Arrived,"Warrior reaches the visited workshop without making the player return");
                Move(level.player.transform,SummerLevel.Spawn);level.RestoreProgress(new SummerProgress());Press(level,Key.E);Press(level,Key.Enter);
                Move(level.maleWarrior.transform,SummerLevel.WorkshopGuide);Move(level.femaleWarrior.transform,SummerLevel.WorkshopPartner);Move(level.player.transform,level.workshop.position);
                level.maleWarrior.Guidance.Step(level.maleWarrior.transform,level.player.transform,false,.1f);level.femaleWarrior.Guidance.Step(level.femaleWarrior.transform,level.player.transform,false,.1f);Call(level,"UpdateGuidance");
                Check(level.Progress.GuideArrived&&!level.InputBlocked&&level.Progress.WorkshopQuestion==0,"Arrival never opens or answers the workshop");NoCaption(ui);
                Call(level,"RefreshWorld");
                Check(!level.femaleWarrior.GetComponentInChildren<SeasonWorldLabel>(true).enabled,"Following Warrior stops displaying a floating E card after arrival");
                Move(level.player.transform,level.garden.position+Vector3.right*8);Move(level.femaleWarrior.transform,level.player.transform.position);
                Check((int)level.GetType().GetMethod("Nearest",Private).Invoke(level,null)==-1,"Following Warrior cannot keep a talk prompt next to Ambros");level.Interact(9);Check(!level.IsDialogueOpen,"Following Warrior does not reopen task dialogue");
                level.Interact(0);ReadableDialogue(ui,level.InputBlocked);Check(ui.Copy.text.Contains("workshop"),"Stationed Warrior still offers the current task when deliberately asked");level.DismissPanel();
                var arrived=JsonUtility.FromJson<SummerProgress>(JsonUtility.ToJson(level.Progress));level.RestoreProgress(arrived);Check(!level.InputBlocked&&level.maleWarrior.Guidance==null,"Chapter restoration preserves finished escort without reopening dialogue");
                level.Interact(1);Check(level.IsDialogueOpen,"Workshop still opens deliberately with E");level.DismissPanel();NoCaption(ui);
            }
            finally{level.RestoreProgress(initial);Move(level.player.transform,positions[0]);Move(level.maleWarrior.transform,positions[1]);Move(level.femaleWarrior.transform,positions[2]);JourneySession.Instance.CaptureChapter();}
        }
        public static void Fall(FallLevel level)
        {
            var initial=JsonUtility.FromJson<FallProgress>(JsonUtility.ToJson(level.Progress));var positions=new[]{level.player.transform.position,level.ember.transform.position};
            try
            {
                Move(level.player.transform,FallLevel.Spawn);level.RestoreProgress(new FallProgress());var ui=level.GetComponentInChildren<SeasonOverlay>();
                Check(!level.Progress.Introduced&&!level.InputBlocked&&level.ember.Guidance==null,"Emberfist waits silently at the portal for E");NoCaption(ui);InteractiveName(level.ember.transform);
                Press(level,Key.E);ReadableDialogue(ui,level.InputBlocked);StopsPlayer(level.player);Check(level.ember.Guidance==null,"Greeting cannot move Emberfist away from the player");
                Press(level,Key.Enter);Check(level.Progress.Introduced&&!level.InputBlocked&&level.ember.Guidance!=null,"Deliberately finishing greeting begins escort");
                Move(level.player.transform,level.stations[0].position);level.Interact(1);Check(level.Progress.GuideArrived,"Reading the orchard retires orientation before Emberfist arrives");level.DismissPanel();Move(level.player.transform,level.stations[1].position);Call(level,"RefreshWorld");
                Check(!level.InputBlocked&&!ui.Objective.text.Contains("Walk with"),"Outpacing Emberfist cannot leave a stale escort objective");
                for(int i=0;i<1000&&!level.ember.Guidance.Arrived;i++)level.ember.Guidance.Step(level.ember.transform,level.player.transform,false,.1f);
                Check(level.ember.Guidance.Arrived,"Emberfist reaches the visited orchard without making the player return");
                Move(level.player.transform,FallLevel.Spawn);level.RestoreProgress(new FallProgress());Press(level,Key.E);Press(level,Key.Escape);
                Move(level.ember.transform,FallLevel.OrchardGuide);Move(level.player.transform,level.stations[0].position);level.ember.Guidance.Step(level.ember.transform,level.player.transform,false,.1f);Call(level,"UpdateGuidance");
                Check(level.Progress.GuideArrived&&!level.InputBlocked&&!level.Progress.Studied[0],"Orchard arrival never opens or reads a note");NoCaption(ui);
                level.Interact(0);ReadableDialogue(ui,level.InputBlocked);level.DismissPanel();
                var arrived=JsonUtility.FromJson<FallProgress>(JsonUtility.ToJson(level.Progress));level.RestoreProgress(arrived);Check(!level.InputBlocked&&level.ember.Guidance==null,"Fall restoration keeps escort complete and silent");
                level.Interact(1);Check(level.IsDialogueOpen,"Orchard note still uses E");level.DismissPanel();
            }
            finally{level.RestoreProgress(initial);Move(level.player.transform,positions[0]);Move(level.ember.transform,positions[1]);JourneySession.Instance.CaptureChapter();}
        }
        public static void Winter(WinterLevel level)
        {
            var initial=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(level.Progress));Vector3 position=level.player.transform.position;bool wardenActive=level.winterGuardian.gameObject.activeSelf;
            try
            {
                Move(level.player.transform,WinterLevel.Spawn);level.RestoreProgress(new WinterProgress());var ui=level.GetComponentInChildren<SeasonOverlay>();
                Check(!level.Progress.Introduced&&!level.InputBlocked,"Winter arrives silently without a forced greeting");NoCaption(ui);InteractiveName(level.littleMage.transform);
                Press(level,Key.E);ReadableDialogue(ui,level.InputBlocked);StopsPlayer(level.player);Press(level,Key.Enter);
                Check(level.Progress.Introduced&&!level.InputBlocked,"E greeting enables Winter lessons when closed");
                var ready=new WinterProgress{Introduced=true};for(int m=0;m<2;m++){ready.ReadNote(m);foreach(var q in WinterContent.Missions[m])ready.AnswerMission(m,q.Correct);}ready.BeginWinterBoss();foreach(var q in WinterContent.Boss)ready.AnswerWinterBoss(q.Correct);
                level.RestoreProgress(ready);Call(level,"Update");Check(!ready.CaveGuideRead&&!ready.CanEnterCave&&!level.InputBlocked,"Warden victory never silently reads the cave briefing");NoCaption(ui);
                Check(level.Destination()==(Vector2)level.littleMage.transform.position,"Unfinished cave briefing points to the actual guide");
                var entry=JourneySession.Instance.Data.chapters[3];entry.x=19.25f;entry.y=-12.5f;entry.progress=JsonUtility.ToJson(ready);string beforeRestore=entry.progress;
                level.RestoreProgress(JsonUtility.FromJson<WinterProgress>(beforeRestore));
                Check(!level.Progress.CaveGuideRead&&entry.x==19.25f&&entry.y==-12.5f&&entry.progress==beforeRestore,"Restoring pending guide progress neither grants permission nor captures wrong coordinates");
                Move(level.player.transform,level.littleMage.transform.position+Vector3.down*2);Press(level,Key.E);ReadableDialogue(ui,level.InputBlocked);
                Check(!level.Progress.CaveGuideRead,"Opening cave conversation alone does not grant permission");Press(level,Key.Escape);
                Check(!level.Progress.CanEnterCave&&!level.InputBlocked,"Escape closes cave briefing without consuming it");
                Press(level,Key.E);Press(level,Key.Enter);
                Check(level.Progress.CaveGuideRead&&level.Progress.CanEnterCave&&!level.InputBlocked&&level.Progress.Valid(),"Completing the deliberate briefing permits cave entry");
                Check(JsonUtility.FromJson<WinterProgress>(entry.progress).CaveGuideRead&&Mathf.Abs(entry.x-level.player.transform.position.x)<.001f&&Mathf.Abs(entry.y-level.player.transform.position.y)<.001f,"Manual briefing captures the current chapter at the real player position");
                var finished=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(level.Progress));level.RestoreProgress(finished);Check(finished.CaveGuideRead&&finished.CanEnterCave&&!level.IsDialogueOpen,"Same-run restoration retains cave permission without reopening speech");NoCaption(ui);
            }
            finally{level.RestoreProgress(initial);level.winterGuardian.gameObject.SetActive(wardenActive);Move(level.player.transform,position);JourneySession.Instance.CaptureChapter();}
        }
    }
}
#endif
