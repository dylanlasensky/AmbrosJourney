#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using AmbrosJourney.Presentation;
namespace AmbrosJourney.Journey.Editor
{
    public static class MoonbranchPolishChecks
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception("MOONBRANCH: "+message);}
        static void Call(object target,string method)=>target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,null);
        public static void Verify(int chapter)
        {
            var compass=UnityEngine.Object.FindAnyObjectByType<MoonbranchCompass>();Check(compass,"Compass exists in season "+chapter);
            var ring=compass.transform.Find("Seasonal moon branches").GetComponent<UnityEngine.UI.RawImage>();
            Check(ring.texture&&ring.uvRect.x==((chapter-1)%2)*.5f&&ring.uvRect.y==(chapter<=2?.5f:0),"Correct seasonal frame");
            Check(compass.Needle.GetComponent<UnityEngine.UI.RawImage>().texture,"Needle texture loaded");
            compass.Point(new Vector2(.321f,.731f));
            float before=compass.Needle.localEulerAngles.z;Call(compass,"Update");
            float target=Mathf.Atan2(.731f,.321f)*Mathf.Rad2Deg-90;
            Check(Mathf.Abs(Mathf.DeltaAngle(compass.Needle.localEulerAngles.z,target))<=Mathf.Abs(Mathf.DeltaAngle(before,target))+.01f,"Needle approaches arbitrary non-cardinal heading");
            var session=JourneySession.Instance;session.Pause();session.ShowArtifacts();session.HandleBack();
            Check(session.Paused,"Artifact panel returns to its pause parent");session.HandleBack();Check(!session.Paused,"Second back resumes gameplay");
            session.ShowArtifacts();var ui=session.GetComponentInChildren<JourneyUI>();Call(ui,"LateUpdate");
            Check(EventSystem.current.currentSelectedGameObject&&EventSystem.current.currentSelectedGameObject.transform.IsChildOf(ui.transform),"Panel receives keyboard focus");
            InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.Escape));InputSystem.Update();Call(session,"Update");
            Check(!session.Paused&&!AudioListener.pause&&Time.timeScale==1,"Escape dismisses artifact panel");
            InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.Update();
            // A clear synthetic space proves party slots occupy multiple sides instead of a train.
            var trail=new SeasonTrail();var origin=new Vector2(-500,-500);trail.Record(origin);
            var positions=Enumerable.Range(1,5).Select(i=>trail.Formation(i,origin)).ToArray();
            Check(positions.All(p=>Vector2.Distance(origin,p)<3.1f),"All companions stay close to leader");
            Check(positions.Any(p=>p.x<origin.x)&&positions.Any(p=>p.x>origin.x)&&positions.Any(p=>p.y>origin.y),"Companions spread around leader");
            for(int i=0;i<positions.Length;i++)for(int j=i+1;j<positions.Length;j++)Check(Vector2.Distance(positions[i],positions[j])>.8f,"Separate resting positions");
            int queries=SeasonNavigation.PhysicsQueryCount;for(int i=0;i<100;i++)trail.Formation(1,origin);Check(SeasonNavigation.PhysicsQueryCount==queries,"Formation queries are throttled");
            Debug.Log("MOONBRANCH_POLISH_PASS: season "+chapter+", frame, free angle, keyboard back/focus, scattered party and cached physics.");
        }
    }
}
#endif
