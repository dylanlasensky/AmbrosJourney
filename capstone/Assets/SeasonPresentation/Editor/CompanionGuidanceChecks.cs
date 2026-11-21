#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using AmbrosJourney.Journey;
using AmbrosJourney.Summer;
using AmbrosJourney.Fall;
using AmbrosJourney.Winter;

namespace AmbrosJourney.Presentation.Editor
{
    // Invoke each method after its fresh scene has loaded in the disposable playthrough.
    public static class CompanionGuidanceChecks
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static void Check(bool value,string why){if(!value)throw new Exception("COMPANION GUIDANCE: "+why);}
        static void Call(object owner,string method,params object[] args)=>owner.GetType().GetMethod(method,Private).Invoke(owner,args);
        static void Move(Transform actor,Vector3 position){actor.position=position;var body=actor.GetComponent<Rigidbody2D>();if(body)body.position=position;Physics2D.SyncTransforms();}
        static void PassiveNames(params Transform[] actors)
        {
            foreach(var actor in actors)foreach(var label in actor.GetComponentsInChildren<SeasonWorldLabel>(true))
                Check(!label.Caption.Contains("[E]")&&!label.Caption.Contains("[ E ]"),"Passive guide has no interaction label: "+label.Caption);
        }
        static void FitCaption(SeasonOverlay ui)
        {
            var speech=ui.GetComponent<CompanionSpeech>();Check(speech,"Actual companion guidance exists");speech.Advance(.2f);Check(speech.Visible,"Actual companion guidance is visible");
            Canvas.ForceUpdateCanvases();foreach(var text in ui.transform.Find("Companion caption").GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,"Actual caption fits: "+text.text);}
        }
        static void Caption(SeasonOverlay ui,Func<bool> blocked)
        {
            int canvases=UnityEngine.Object.FindObjectsByType<Canvas>().Length;
            var speech=CompanionSpeech.Say(ui,"Guide","Read the next field note with E. You can keep walking while I speak. J opens your notes.");
            speech.Advance(.2f);Check(speech.Visible&&!blocked(),"Caption does not block movement");
            Check(UnityEngine.Object.FindObjectsByType<Canvas>().Length==canvases,"Caption reuses existing canvas");
            var panel=ui.transform.Find("Companion caption");
            Check(panel.GetComponentsInChildren<UnityEngine.UI.Graphic>().All(g=>!g.raycastTarget),"Caption never intercepts mouse clicks");
            Canvas.ForceUpdateCanvases();foreach(var text in panel.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,"Caption text fits: "+text.text);}
            float left=speech.RemainingSeconds;
            ui.Dialog.SetActive(true);speech.Advance(5);Check(!speech.Visible&&speech.RemainingSeconds==left,"Lesson hides caption and preserves reading time");ui.Dialog.SetActive(false);
            ui.Journal.SetActive(true);speech.Advance(5);Check(!speech.Visible&&speech.RemainingSeconds==left,"Journal hides caption and preserves reading time");ui.Journal.SetActive(false);
            var session=JourneySession.Instance;session.Pause();speech.Advance(5);Check(!speech.Visible&&speech.RemainingSeconds==left,"Pause hides caption and preserves reading time");session.Resume();
            session.SetCinematicVisible(true);speech.Advance(5);Check(!speech.Visible&&speech.RemainingSeconds==left,"Video hides caption and preserves reading time");session.SetCinematicVisible(false);
            speech.Advance(1);Check(speech.Visible&&speech.RemainingSeconds<left,"Caption resumes after overlays close");
            var transition=new GameObject("Test season fade",typeof(SeasonTransition));
            try
            {
                left=speech.RemainingSeconds;speech.Advance(5);Check(!speech.Visible&&speech.RemainingSeconds==left,"Source caption waits during the season fade");
                speech=CompanionSpeech.Say(ui,"New season guide","Welcome! Read the first field note with E when the trail appears.");left=speech.RemainingSeconds;
                speech.Advance(5);Check(!speech.Visible&&speech.RemainingSeconds==left,"New destination caption keeps its full reading time during the fade");
            }
            finally{UnityEngine.Object.DestroyImmediate(transition);}
            speech.Advance(.2f);Check(speech.Visible,"Destination caption appears after fade ends");speech.Advance(20);Check(!speech.Visible,"Caption expires without requiring input");
        }
        public static void Spring(SpringGuide level)
        {
            var session=JourneySession.Instance;session.Resume();Call(level,"Update");level.DismissPanel();Call(level,"LateUpdate");
            Check(level.LessonComplete&&!level.DialogueOpen,"Closing controls starts Spring without a Mage conversation");FitCaption(level.GetComponentInChildren<SeasonOverlay>());
            var initial=level.CaptureProgress();Vector3 position=level.player.position;
            try
            {
                level.RestoreProgress(new SpringState{controlsSeen=true});Call(level,"LateUpdate");
                Check(level.LessonComplete&&!level.DialogueOpen&&level.MissionsComplete==0,"Old save before Mage keeps lesson stations available without granting missions");
                Check(level.CaptureProgress().Valid(),"Migrated Spring checkpoint remains valid");
                Move(level.player,level.mage.position);Check((int)level.GetType().GetMethod("Nearby",Private).Invoke(level,null)!=-1,"Mage does not consume E");
                PassiveNames(level.mage);Caption(level.GetComponentInChildren<SeasonOverlay>(),()=>level.DialogueOpen||JourneySession.Blocked);
            }
            finally{level.RestoreProgress(initial);Move(level.player,position);JourneySession.Instance.Save();}
        }
        public static void Summer(SummerLevel level)
        {
            var initial=JsonUtility.FromJson<SummerProgress>(JsonUtility.ToJson(level.Progress));var positions=new[]{level.player.transform.position,level.maleWarrior.transform.position,level.femaleWarrior.transform.position};
            try
            {
                level.RestoreProgress(new SummerProgress());
                Check(level.Progress.Introduced&&!level.InputBlocked&&level.maleWarrior.Guidance!=null,"Summer starts escort without modal conversation");FitCaption(level.GetComponentInChildren<SeasonOverlay>());
                Move(level.player.transform,level.workshop.position);level.Interact(1);Check(level.Progress.GuideArrived,"Reading the workshop completes orientation even before the guides arrive");level.DismissPanel();Move(level.player.transform,level.garden.position);Call(level,"RefreshWorld");
                Check(!level.InputBlocked&&!level.GetComponentInChildren<SeasonOverlay>().Objective.text.Contains("Walk with"),"Outpacing Warriors cannot leave a stale escort objective");
                for(int i=0;i<1000&&!level.maleWarrior.Guidance.Arrived;i++)level.maleWarrior.Guidance.Step(level.maleWarrior.transform,level.player.transform,false,.1f);
                Check(level.maleWarrior.Guidance.Arrived,"Warrior finishes walking to the visited workshop without making the player return");
                level.RestoreProgress(new SummerProgress());
                Move(level.maleWarrior.transform,SummerLevel.WorkshopGuide);Move(level.femaleWarrior.transform,SummerLevel.WorkshopPartner);Move(level.player.transform,level.workshop.position);
                level.maleWarrior.Guidance.Step(level.maleWarrior.transform,level.player.transform,false,.1f);level.femaleWarrior.Guidance.Step(level.femaleWarrior.transform,level.player.transform,false,.1f);Call(level,"UpdateGuidance");
                Check(level.Progress.GuideArrived&&!level.InputBlocked&&level.Progress.WorkshopQuestion==0,"Arrival tip does not open or answer the workshop");FitCaption(level.GetComponentInChildren<SeasonOverlay>());
                var arrived=JsonUtility.FromJson<SummerProgress>(JsonUtility.ToJson(level.Progress));level.RestoreProgress(arrived);Check(!level.InputBlocked&&level.maleWarrior.Guidance==null,"Reload preserves finished escort without reopening dialogue");
                level.Interact(1);Check(level.IsDialogueOpen,"Workshop still opens deliberately with E");level.DismissPanel();
                PassiveNames(level.maleWarrior.transform,level.femaleWarrior.transform);Caption(level.GetComponentInChildren<SeasonOverlay>(),()=>level.InputBlocked);
            }
            finally{level.RestoreProgress(initial);Move(level.player.transform,positions[0]);Move(level.maleWarrior.transform,positions[1]);Move(level.femaleWarrior.transform,positions[2]);JourneySession.Instance.Save();}
        }
        public static void Fall(FallLevel level)
        {
            var initial=JsonUtility.FromJson<FallProgress>(JsonUtility.ToJson(level.Progress));var positions=new[]{level.player.transform.position,level.ember.transform.position};
            try
            {
                level.RestoreProgress(new FallProgress());Check(level.Progress.Introduced&&!level.InputBlocked&&level.ember.Guidance!=null,"Fall starts escort without modal conversation");FitCaption(level.GetComponentInChildren<SeasonOverlay>());
                Move(level.player.transform,level.stations[0].position);level.Interact(1);Check(level.Progress.GuideArrived,"Reading the orchard completes orientation even before Emberfist arrives");level.DismissPanel();Move(level.player.transform,level.stations[1].position);Call(level,"RefreshWorld");
                Check(!level.InputBlocked&&!level.GetComponentInChildren<SeasonOverlay>().Objective.text.Contains("Walk with"),"Outpacing Emberfist cannot leave a stale escort objective");
                for(int i=0;i<1000&&!level.ember.Guidance.Arrived;i++)level.ember.Guidance.Step(level.ember.transform,level.player.transform,false,.1f);
                Check(level.ember.Guidance.Arrived,"Emberfist finishes walking to the visited orchard without making the player return");
                level.RestoreProgress(new FallProgress());
                Move(level.ember.transform,FallLevel.OrchardGuide);Move(level.player.transform,level.stations[0].position);level.ember.Guidance.Step(level.ember.transform,level.player.transform,false,.1f);Call(level,"UpdateGuidance");
                Check(level.Progress.GuideArrived&&!level.InputBlocked&&!level.Progress.Studied[0],"Orchard arrival never auto-opens or reads a note");FitCaption(level.GetComponentInChildren<SeasonOverlay>());
                var arrived=JsonUtility.FromJson<FallProgress>(JsonUtility.ToJson(level.Progress));level.RestoreProgress(arrived);Check(!level.InputBlocked&&level.ember.Guidance==null,"Fall reload keeps escort complete");
                level.Interact(1);Check(level.IsDialogueOpen,"Orchard note still uses E");level.DismissPanel();
                PassiveNames(level.ember.transform);Caption(level.GetComponentInChildren<SeasonOverlay>(),()=>level.InputBlocked);
            }
            finally{level.RestoreProgress(initial);Move(level.player.transform,positions[0]);Move(level.ember.transform,positions[1]);JourneySession.Instance.Save();}
        }
        public static void Winter(WinterLevel level)
        {
            var initial=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(level.Progress));Vector3 position=level.player.transform.position;bool wardenActive=level.winterGuardian.gameObject.activeSelf;
            try
            {
                level.RestoreProgress(new WinterProgress());Check(level.Progress.Introduced&&!level.InputBlocked,"Winter starts with passive orientation");FitCaption(level.GetComponentInChildren<SeasonOverlay>());
                var ready=new WinterProgress{Introduced=true};for(int m=0;m<2;m++){ready.ReadNote(m);foreach(var q in WinterContent.Missions[m])ready.AnswerMission(m,q.Correct);}ready.BeginWinterBoss();foreach(var q in WinterContent.Boss)ready.AnswerWinterBoss(q.Correct);
                Check(!ready.CanEnterCave,"Legacy completed-Warden save starts without guide flag");
                level.Progress=ready;Call(level,"Show","Winter crest","Victory",(Action)(()=>{}));Call(level,"OfferCaveGuidance");Check(!ready.CaveGuideRead,"Cave hint waits while victory feedback is open");
                level.DismissPanel();Check(ready.CaveGuideRead&&ready.CanEnterCave&&!level.InputBlocked,"Closing victory automatically unlocks cave guidance");FitCaption(level.GetComponentInChildren<SeasonOverlay>());
                var oldSave=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(ready));oldSave.CaveGuideRead=false;
                var entry=JourneySession.Instance.Data.chapters[3];entry.x=19.25f;entry.y=-12.5f;entry.progress=JsonUtility.ToJson(oldSave);string beforeRestore=entry.progress;
                level.RestoreProgress(oldSave);
                Check(!oldSave.CaveGuideRead&&entry.x==19.25f&&entry.y==-12.5f&&entry.progress==beforeRestore,"Restoring progress cannot mutate or save cave guidance before saved coordinates are restored");
                Move(level.player.transform,new Vector2(entry.x,entry.y));Call(level,"OfferCaveGuidance");
                Check(level.Progress.CanEnterCave&&level.Progress.Valid()&&entry.x==19.25f&&entry.y==-12.5f,"Deferred cave guidance preserves the restored position");
                Check(JsonUtility.FromJson<WinterProgress>(entry.progress).CaveGuideRead,"Deferred guidance is saved after position restoration");
                var saved=JsonUtility.FromJson<WinterProgress>(JsonUtility.ToJson(level.Progress));level.RestoreProgress(saved);Check(saved.CaveGuideRead&&saved.CanEnterCave,"Automatic guide flag survives save and reload");
                PassiveNames(level.littleMage.transform);Caption(level.GetComponentInChildren<SeasonOverlay>(),()=>level.InputBlocked);
            }
            finally{level.RestoreProgress(initial);level.winterGuardian.gameObject.SetActive(wardenActive);Move(level.player.transform,position);JourneySession.Instance.Save();}
        }
    }
}
#endif
