// Runs isolated Editor regression checks for moonbranch polish.
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
        static void VisibleArrow(MoonbranchCompass compass)
        {
            var arrow=compass.Needle;
            Check(arrow.sizeDelta.x>=48&&arrow.sizeDelta.x<=56&&Mathf.Abs(arrow.sizeDelta.x-arrow.sizeDelta.y)<.001f,"Readable square direction marker replaces the old tiny arrow");
            var image=arrow.GetComponent<UnityEngine.UI.RawImage>();var texture=image.texture as Texture2D;
            Check(texture&&!image.raycastTarget&&image.color.a>=.99f,"Opaque direction marker cannot intercept interaction clicks");
            var colors=texture.GetPixels().Where(c=>c.a>.99f).ToArray();Check(colors.Length>0,"Arrow has an opaque silhouette");
            float brightest=colors.Max(Luminance),darkest=colors.Min(Luminance);
            Check((brightest+.05f)/(darkest+.05f)>=7,"Bright face and dark outline retain strong internal contrast");
            var overlay=compass.GetComponentInParent<SeasonOverlay>();var labels=overlay.transform.Find("Landmark labels");
            Check(arrow.parent==overlay.transform&&arrow.GetSiblingIndex()>labels.GetSiblingIndex(),"Floating name cards cannot cover the arrow");
            Check(arrow.GetSiblingIndex()<overlay.Dialog.transform.GetSiblingIndex()&&arrow.GetSiblingIndex()<overlay.Journal.transform.GetSiblingIndex(),"Lessons and journal remain above the marker");
        }
        static float Linear(float value)=>value<=.04045f?value/12.92f:Mathf.Pow((value+.055f)/1.055f,2.4f);
        static float Luminance(Color color)=>.2126f*Linear(color.r)+.7152f*Linear(color.g)+.0722f*Linear(color.b);
        public static void Verify(int chapter)
        {
            var compass=UnityEngine.Object.FindAnyObjectByType<MoonbranchCompass>();Check(compass,"Compass exists in season "+chapter);
            VisibleArrow(compass);
            compass.Point(new Vector2(.321f,.731f));
            float before=compass.Needle.localEulerAngles.z;Call(compass,"Update");
            float target=Mathf.Atan2(.731f,.321f)*Mathf.Rad2Deg-90;
            Check(Mathf.Abs(Mathf.DeltaAngle(compass.Needle.localEulerAngles.z,target))<=Mathf.Abs(Mathf.DeltaAngle(before,target))+.01f,"Needle approaches arbitrary non-cardinal heading");
            var scale=compass.Needle.localScale;
            Check(scale.x>=.9999f&&scale.x<=1.0501f&&Mathf.Abs(scale.x-scale.y)<.0001f,"Slow arrow pulse stays small and cannot distort its direction");
            Check(compass.Needle.sizeDelta.x*scale.x<=56,"Visible pulse remains a modest marker, not a large compass");
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
            Debug.Log("MOONBRANCH_POLISH_PASS: season "+chapter+", visible arrow, free angle, keyboard back/focus, scattered party and cached physics.");
        }
    }
}
#endif
