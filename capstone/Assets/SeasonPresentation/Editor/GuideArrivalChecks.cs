#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;
using AmbrosJourney.Summer;
using AmbrosJourney.Fall;
namespace AmbrosJourney.Presentation.Editor
{
    public static class GuideArrivalChecks
    {
        static void Check(bool condition,string message){if(!condition)throw new Exception("GUIDE ARRIVAL: "+message);}
        static void Call(object target,string method)=>target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,null);
        static void Walk(GuideWalk route,Transform guide,Transform player)
        {
            Check(route!=null,"Introduction starts walking route");
            Vector2 start=guide.position;
            Check(route.Step(guide,player,true,.1f)==Vector2.zero&&(Vector2)guide.position==start,"Pause/dialogue stops guide");
            // Stop the player and verify the guide waits within view.
            for(int i=0;i<100;i++)route.Step(guide,player,false,.05f);
            Check(Vector2.Distance(guide.position,player.position)<5.3f,"Guide waits for player");
            for(int i=0;i<2000&&!route.Arrived;i++)
            {
                player.position=guide.position;player.GetComponent<Rigidbody2D>().position=player.position;
                route.Step(guide,player,false,.05f);
            }
            Check(route.Arrived,"Guide reaches the first task along a collision-free route; stopped at "+guide.position+"; blockers: "+string.Join(", ",System.Array.ConvertAll(Physics2D.OverlapCircleAll((Vector2)guide.position+Vector2.up*.22f,.4f),c=>c.name)));
        }
        public static void Summer(SummerLevel level)
        {
            Check(Vector2.Distance(level.maleWarrior.transform.position,SummerLevel.Spawn)<4&&Vector2.Distance(level.femaleWarrior.transform.position,SummerLevel.Spawn)<4,"Both Warriors welcome player at portal");
            Check(!level.InputBlocked,"Walking phase permits player movement");
            Walk(level.maleWarrior.Guidance,level.maleWarrior.transform,level.player.transform);
            // Walk the second guide with a fresh nearby player for the wait assertion.
            level.player.transform.position=level.femaleWarrior.transform.position;
            Walk(level.femaleWarrior.Guidance,level.femaleWarrior.transform,level.player.transform);
            level.player.transform.position=level.workshop.position;level.player.GetComponent<Rigidbody2D>().position=level.player.transform.position;
            Call(level,"UpdateGuidance");Check(level.Progress.GuideArrived&&level.IsDialogueOpen,"Explanation resumes at workshop");
            var female=level.femaleWarrior.GetComponent<SpriteRenderer>();
            Check(!System.Linq.Enumerable.Any(UnityEngine.Object.FindObjectsByType<SpriteRenderer>(),r=>r.name=="Summer canopy"&&r.sortingOrder>female.sortingOrder&&r.bounds.Intersects(female.bounds)),"Female Warrior visible outside foreground trees");
            Camera.main.transform.position=level.player.transform.position+Vector3.back*10;
            SeasonIntegrationChecks.Capture("SummerGuidesAtWorkshop");
            var restored=JsonUtility.FromJson<SummerProgress>(JsonUtility.ToJson(level.Progress));
            level.RestoreProgress(restored);Check(level.Progress.GuideArrived&&level.maleWarrior.Guidance==null,"Reload keeps completed escort");
            level.player.transform.position=level.workshop.position+Vector3.down;
            level.femaleWarrior.transform.position=level.player.transform.position;
            int nearest=(int)level.GetType().GetMethod("Nearest",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(level,null);
            Check(nearest==1,"Workshop interaction wins over nearby companion");
            level.femaleWarrior.transform.position=SummerLevel.WorkshopPartner;
            // Restore the visible arrival explanation for the button-flow checks.
            level.Progress.GuideArrived=false;level.maleWarrior.Guidance=new GuideWalk(SummerLevel.WorkshopGuide,new Vector2(-30,-32),new Vector2(30,28));level.femaleWarrior.Guidance=new GuideWalk(SummerLevel.WorkshopPartner,new Vector2(-30,-32),new Vector2(30,28));
            level.maleWarrior.Guidance.Step(level.maleWarrior.transform,level.player.transform,false,.05f);level.femaleWarrior.Guidance.Step(level.femaleWarrior.transform,level.player.transform,false,.05f);Call(level,"UpdateGuidance");
        }
        public static void Fall(FallLevel level)
        {
            Check(Vector2.Distance(level.ember.transform.position,FallLevel.Spawn)<4,"Emberfist welcomes player at portal");
            Walk(level.ember.Guidance,level.ember.transform,level.player.transform);
            level.player.transform.position=level.stations[0].position;level.player.GetComponent<Rigidbody2D>().position=level.player.transform.position;
            Call(level,"UpdateGuidance");Check(level.Progress.GuideArrived&&level.IsDialogueOpen,"Explanation resumes at orchard");
            Camera.main.transform.position=level.player.transform.position+Vector3.back*10;
            SeasonIntegrationChecks.Capture("EmberfistAtOrchard");
        }
    }
}
#endif
