#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
namespace AmbrosJourney.Presentation.Editor
{
    public static class TrailNavigationChecks
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception("TRAIL_GUIDANCE: "+message);}
        public static void Verify()
        {
            // Clear space with a U-shaped road: a visibility shortcut must not cross its lawn.
            var road=new HashSet<Vector2Int>();
            for(int y=0;y<=8;y++){road.Add(new Vector2Int(-500,-500+y));road.Add(new Vector2Int(-488,-500+y));}
            for(int x=0;x<=12;x++)road.Add(new Vector2Int(-500+x,-492));
            var path=SeasonNavigation.FindPath(new Vector2(-500,-500),new Vector2(-488,-500),new Vector2(-505,-505),new Vector2(-480,-485),road);
            Check(path.Count>0&&path[0].y>-500,"Direction starts along the road, not toward a visible destination across the lawn");
            Check(path.Exists(p=>p.y>=-492),"Route visits the bend before turning to the destination");
            foreach(var p in path)Check(road.Contains(Vector2Int.RoundToInt(p)),"Preferred road remains the cheapest traversable route");
            var blocker=new GameObject("Temporary route blocker");blocker.transform.position=new Vector2(-500,-496);blocker.AddComponent<BoxCollider2D>();Physics2D.SyncTransforms();
            try
            {
                var detour=SeasonNavigation.FindPath(new Vector2(-500,-500),new Vector2(-488,-500),new Vector2(-505,-505),new Vector2(-480,-485),road);
                Check(detour.Count>0&&!detour.Contains(new Vector2(-500,-496)),"A blocked road permits a safe detour");
            }
            finally{UnityEngine.Object.DestroyImmediate(blocker);Physics2D.SyncTransforms();}
            var boss=new GameObject("Temporary solid boss");boss.transform.position=new Vector2(-488,-500);boss.AddComponent<CircleCollider2D>().radius=1;Physics2D.SyncTransforms();
            try
            {
                var from=new Vector2(-500,-500);var target=new Vector2(-488,-500);var min=new Vector2(-505,-505);var max=new Vector2(-480,-485);
                Check(SeasonNavigation.FindPath(from,target,min,max).Count==0,"Strict reachability still rejects a solid goal");
                var guidance=new SeasonNavigation(min,max);var waypoint=guidance.Next(from,target);
                Check(Vector2.Distance(waypoint,from)>.5f&&SeasonNavigation.ClearSegment(from,waypoint),"Solid bosses still receive safe approach guidance");
                int plans=guidance.RebuildCount;guidance.Next(from,target);Check(guidance.RebuildCount==plans,"Repeated guidance samples reuse their route");
            }
            finally{UnityEngine.Object.DestroyImmediate(boss);Physics2D.SyncTransforms();}
            Debug.Log("TRAIL_GUIDANCE_PASS: authored road preference and blocked-road detour.");
        }
    }
}
#endif
