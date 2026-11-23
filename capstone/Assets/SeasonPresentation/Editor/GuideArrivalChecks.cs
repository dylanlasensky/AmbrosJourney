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
        static void SlowFrameWait()
        {
            var player=new GameObject("Guide wait test player");var guide=new GameObject("Guide wait test actor");
            Vector2 origin=new Vector2(-500,-500),destination=origin+Vector2.right*20;
            try
            {
                player.transform.position=origin;
                foreach(float elapsed in new[]{1f/60,1f/15,.33333334f})
                {
                    guide.transform.position=origin+new Vector2(4.8f,0);
                    var route=new GuideWalk(destination,origin-Vector2.one*30,origin+Vector2.one*30);
                    var before=(Vector2)guide.transform.position;
                    route.Step(guide.transform,player.transform,false,elapsed);
                    Check(Vector2.Distance(guide.transform.position,origin)<=5.001f,"A slow frame cannot carry a waiting guide past five units");
                    Check(guide.transform.position.x>before.x&&Mathf.Abs(guide.transform.position.y-before.y)<.001f,"Wait boundary preserves progress along the clear route");
                }
                var catchup=new GuideWalk(destination,origin-Vector2.one*30,origin+Vector2.one*30);
                guide.transform.position=origin-Vector2.right*8;catchup.Step(guide.transform,player.transform,false,.33333334f);
                Check(Vector2.Distance(guide.transform.position,origin)<8,"A guide behind the player can still catch up");
                guide.transform.position=origin+Vector2.right*4.8f;catchup.WaitForPlayer=false;catchup.Step(guide.transform,player.transform,false,.33333334f);
                Check(Vector2.Distance(guide.transform.position,origin)>5.3f,"Visited-station guide can finish walking when waiting is disabled");
            }
            finally{UnityEngine.Object.DestroyImmediate(player);UnityEngine.Object.DestroyImmediate(guide);}
        }
        public static void Summer(SummerLevel level)
        {
            Check(!level.Progress.Introduced&&level.maleWarrior.Guidance==null&&!level.InputBlocked,"Summer waits for a deliberate greeting");
            level.Interact(0);Check(level.IsDialogueOpen&&level.InputBlocked,"E opens Warrior dialogue");level.DismissPanel();
            Check(level.Progress.Introduced&&level.maleWarrior.Guidance!=null,"Closing greeting starts the Summer escort");
            SlowFrameWait();
            Check(Vector2.Distance(level.maleWarrior.transform.position,SummerLevel.Spawn)<5.3f&&Vector2.Distance(level.femaleWarrior.transform.position,SummerLevel.Spawn)<5.3f,"Warriors stay within view of the portal while the player waits: male="+level.maleWarrior.transform.position+", female="+level.femaleWarrior.transform.position+", player="+level.player.transform.position);
            Check(!level.InputBlocked,"Walking phase permits player movement");
            Walk(level.maleWarrior.Guidance,level.maleWarrior.transform,level.player.transform);
            // Walk the second guide with a fresh nearby player for the wait assertion.
            level.player.transform.position=level.femaleWarrior.transform.position;
            Walk(level.femaleWarrior.Guidance,level.femaleWarrior.transform,level.player.transform);
            level.player.transform.position=level.workshop.position;level.player.GetComponent<Rigidbody2D>().position=level.player.transform.position;
            Call(level,"UpdateGuidance");Check(level.Progress.GuideArrived&&!level.IsDialogueOpen&&!level.InputBlocked&&level.Progress.WorkshopQuestion==0,"Workshop arrival stays silent without starting the lesson");
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

        }
        public static void Fall(FallLevel level)
        {
            Check(!level.Progress.Introduced&&level.ember.Guidance==null&&!level.InputBlocked,"Fall waits for a deliberate greeting");
            level.Interact(0);Check(level.IsDialogueOpen&&level.InputBlocked,"E opens Emberfist dialogue");level.DismissPanel();
            Check(level.Progress.Introduced&&level.ember.Guidance!=null,"Closing greeting starts the Fall escort");
            Check(Vector2.Distance(level.ember.transform.position,FallLevel.Spawn)<5.3f,"Emberfist stays within view of the portal while the player waits");
            Walk(level.ember.Guidance,level.ember.transform,level.player.transform);
            level.player.transform.position=level.stations[0].position;level.player.GetComponent<Rigidbody2D>().position=level.player.transform.position;
            Call(level,"UpdateGuidance");Check(level.Progress.GuideArrived&&!level.IsDialogueOpen&&!level.InputBlocked&&!level.Progress.Studied[0],"Orchard arrival stays silent without reading the note");
            Camera.main.transform.position=level.player.transform.position+Vector3.back*10;
            SeasonIntegrationChecks.Capture("EmberfistAtOrchard");
        }
    }
}
#endif
