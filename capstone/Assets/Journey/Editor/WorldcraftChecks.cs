#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using AmbrosJourney.Presentation;
using AmbrosJourney.Winter;
namespace AmbrosJourney.Journey.Editor
{
    public static class WorldcraftChecks
    {
        static void Check(bool condition,string reason){if(!condition)throw new Exception("WORLDCRAFT: "+reason);}
        public static void Verify(int chapter)
        {
            Check(!UnityEngine.Object.FindObjectsByType<SpriteRenderer>().Any(r=>r.sprite&&r.sprite.name=="Letter"),"No decorative envelopes resemble an interaction");
            var session=JourneySession.Instance;var player=session.Player();var saved=player.position;
            object controller;Transform boss;string method;int expected;
            var winterCheck=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();bool wasCave=winterCheck&&winterCheck.Progress.InCave;if(winterCheck){winterCheck.Progress.InCave=false;winterCheck.World.RestoreRegion(false);}
            if(chapter==1){controller=UnityEngine.Object.FindAnyObjectByType<SpringGuide>();boss=(Transform)typeof(SpringGuide).GetField("boss",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(controller);method="Nearby";expected=3;}
            else if(chapter==2){var level=UnityEngine.Object.FindAnyObjectByType<AmbrosJourney.Summer.SummerLevel>();controller=level;boss=level.orc.transform;method="Nearest";expected=4;}
            else if(chapter==3){var level=UnityEngine.Object.FindAnyObjectByType<AmbrosJourney.Fall.FallLevel>();controller=level;boss=level.guardian.transform;method="Nearest";expected=4;}
            else{var level=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();controller=level;boss=level.winterGuardian.transform;method="Nearest";expected=8;}
            try{player.position=boss.position+(chapter==4?Vector3.down:Vector3.left)*4.5f;int target=(int)controller.GetType().GetMethod(method,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(controller,null);Check(boss.gameObject.activeInHierarchy?target==expected:target!=expected,"Active boss is reachable; defeated boss has no E interaction in season "+chapter);}
            finally{player.position=saved;if(winterCheck){winterCheck.Progress.InCave=wasCave;winterCheck.World.RestoreRegion(wasCave);}}
            if(winterCheck)
            {
                bool complete=winterCheck.Progress.Complete;
                try
                {
                    winterCheck.Progress.InCave=true;winterCheck.Progress.Complete=complete;winterCheck.World.RestoreRegion(true);
                    player.position=winterCheck.guardian.transform.position+Vector3.down*4.5f;
                    int target=(int)typeof(WinterLevel).GetMethod("Nearest",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(winterCheck,null);
                    Check(complete?target!=4:target==4,"Active final boss is reachable; defeated final boss has no interaction");
                }
                finally{player.position=saved;winterCheck.Progress.InCave=wasCave;winterCheck.Progress.Complete=complete;winterCheck.World.RestoreRegion(wasCave);}
            }
            var overlay=UnityEngine.Object.FindAnyObjectByType<SeasonOverlay>();var camera=Camera.main;var oldCamera=camera.transform.position;
            camera.transform.position=player.position+Vector3.back*10;
            overlay.SetCompass(player.position,(Vector2)player.position+new Vector2(4,3),(Vector2)player.position+new Vector2(8,6));
            overlay.GetComponentInChildren<MoonbranchCompass>(true).Advance(1);
            typeof(SeasonOverlay).GetMethod("LateUpdate",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(overlay,null);
            SeasonIntegrationChecks.Capture("PlayerArrow"+chapter);camera.transform.position=oldCamera;
            var cohesion=UnityEngine.Object.FindAnyObjectByType<WorldCohesion>();
            var world=UnityEngine.Object.FindAnyObjectByType<JourneyWorld>();
            Check(cohesion.SeasonalDetails.Count>=4,"At least four authored seasonal vignettes, season "+chapter+": "+cohesion.SeasonalDetails.Count);
            var portals=UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include).Where(r=>r.sprite&&r.sprite.texture.name=="SeasonPortals").ToArray();
            Check(portals.Length>0,"Seasonal portal artwork exists");
            foreach(var portal in portals)
            {
                Check(portal.sprite.name=="SeasonPortals / "+(chapter-1),"Portal matches the surrounding season");
                Check(!UnityEngine.Object.FindObjectsByType<SpriteRenderer>().Any(r=>(r.name=="Snow fir"||r.name=="Copper canopy"||r.name=="Summer canopy"||r.name=="Blossom canopy")&&r.sortingOrder>portal.sortingOrder&&r.bounds.Intersects(portal.bounds)),"Portal visible beyond foreground canopies");
            }
            foreach(var threshold in cohesion.PortalThresholds)Check(UnityEngine.Object.FindObjectsByType<NaturalTrail>().Any(t=>t.Near(threshold,0)),"Portal threshold connects to a path");
            WorldCohesionChecks.Capture("Portal"+chapter,portals[0].transform.position+Vector3.up*1.5f,5,false);
            Check(world.Artifacts.Select(a=>a.GetComponent<SpriteRenderer>().sprite.rect).Distinct().Count()==3,"Three distinct collectible silhouettes");
            foreach(var artifact in world.Artifacts)
            {
                var sprite=artifact.GetComponent<SpriteRenderer>().sprite;
                Check(sprite.texture.name=="SeasonalRelics","New artifact artwork used");
                Check(sprite.bounds.size.x<1.3f&&sprite.bounds.size.y<2,"Artifacts stay at readable gameplay scale");
            }
            foreach(var detail in cohesion.SeasonalDetails)
            {
                Check(!UnityEngine.Object.FindObjectsByType<SpriteRenderer>().Any(r=>(r.name=="Snow fir"||r.name=="Copper canopy"||r.name=="Summer canopy"||r.name=="Blossom canopy")&&r.sortingOrder>detail.sortingOrder&&r.bounds.Intersects(detail.bounds)),"No foreground canopy hides decoration: "+detail.name);
                if(chapter==2&&detail.name.Contains("hive"))Check(detail.transform.position.y>-17.5f,"Garden hives stay north of the arena boundary");
                Check(!Physics2D.OverlapBox((Vector2)detail.transform.position+Vector2.up*.25f,new Vector2(detail.bounds.size.x+.2f,.7f),0),"Decoration base outside collision: "+detail.name);
                Check(!UnityEngine.Object.FindObjectsByType<NaturalTrail>().Any(t=>t.Near(detail.transform.position,detail.bounds.size.x*.5f+.3f)),"Decoration clear of route: "+detail.name);
            }
            if(chapter==1)
            {
                var guide=UnityEngine.Object.FindAnyObjectByType<SpringGuide>();var originalPosition=guide.mage.position;
                try
                {
                    guide.mage.position=cohesion.PortalThresholds[0];Physics2D.SyncTransforms();
                    Check(guide.mage.GetComponents<Collider2D>().All(c=>c.isTrigger),"Spring companion cannot physically block the player");
                    Check(SeasonNavigation.FindPath(new Vector2(-18,16),cohesion.PortalThresholds[0],new Vector2(-32,-38),new Vector2(32,26)).Count>0,"Mage standing in portal cannot block navigation");
                }
                finally{guide.mage.position=originalPosition;Physics2D.SyncTransforms();}
            }
            foreach(var detail in cohesion.SeasonalDetails.Take(3))WorldCohesionChecks.Capture("Worldcraft"+chapter+"_"+detail.name.Replace("/","-"),detail.transform.position+Vector3.up,5,false);
            WorldCohesionChecks.Capture("Relics"+chapter,world.Artifacts[0].transform.position,5,false);
            if(chapter==4)
            {
                Check(cohesion.RidgePieces.Count==7,"Modular mountain replaces giant cave painting");
                var winter=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();
                Check(SeasonNavigation.FindPath(WinterLevel.Spawn,WinterWorld.Entrance,WinterWorld.Min,WinterWorld.Max).Count>0,"Outdoor cave entrance reachable");
                Check(!Physics2D.OverlapCircle(WinterWorld.Entrance,.5f),"Doorway remains open");
                WorldCohesionChecks.Capture("WinterRidge",WinterWorld.Entrance+Vector2.up*2,10,false);
            }
            Debug.Log("WORLDCRAFT_PASS: season "+chapter+", "+cohesion.SeasonalDetails.Count+" placed details, distinct relics, clear approaches.");
        }
    }
}
#endif
