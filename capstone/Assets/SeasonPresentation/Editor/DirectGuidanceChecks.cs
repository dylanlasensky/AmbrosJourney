#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;
using AmbrosJourney.Journey;
using AmbrosJourney.Summer;
using AmbrosJourney.Fall;
using AmbrosJourney.Winter;

namespace AmbrosJourney.Presentation.Editor
{
    public static class DirectGuidanceChecks
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        static void Check(bool value,string message)
        { if(!value)throw new Exception("DIRECT_GUIDANCE: "+message); }
        static object Call(object target,string method)
            => target.GetType().GetMethod(method,Private|BindingFlags.Public).Invoke(target,null);

        public static void Verify(int chapter)
        {
            var session=JourneySession.Instance;
            Check(session&&JourneySession.CurrentChapter()==chapter&&!JourneySession.Blocked,"Expected an unblocked active chapter");
            MonoBehaviour owner=chapter==1?(MonoBehaviour)UnityEngine.Object.FindAnyObjectByType<SpringGuide>():
                chapter==2?UnityEngine.Object.FindAnyObjectByType<SummerLevel>():
                chapter==3?UnityEngine.Object.FindAnyObjectByType<FallLevel>():UnityEngine.Object.FindAnyObjectByType<WinterLevel>();
            Check(owner,"Season controller exists");
            string refresh=chapter==1?"LateUpdate":"RefreshWorld";
            var player=session.Player();Check(player,"Player exists");
            var overlay=owner.GetComponentInChildren<SeasonOverlay>(true);Check(overlay,"Season overlay exists");
            var needle=overlay.GetComponentInChildren<MoonbranchCompass>(true);Check(needle,"Season marker exists");
            var targetAngle=typeof(MoonbranchCompass).GetField("targetAngle",Private);
            var hasHeading=typeof(MoonbranchCompass).GetField("hasHeading",Private);
            var body=player.GetComponent<Rigidbody2D>();
            Vector3 oldPosition=player.position;
            Vector2 oldBodyPosition=body?body.position:Vector2.zero,oldVelocity=body?body.linearVelocity:Vector2.zero;
            bool dialog=overlay.Dialog.activeSelf,journal=overlay.Journal.activeSelf;
            var arrow=overlay.Arrow;
            Quaternion oldRotation=arrow.localRotation;
            Vector2 oldAnchored=arrow.anchoredPosition,oldMin=arrow.anchorMin,oldMax=arrow.anchorMax,oldPivot=arrow.pivot;
            bool oldActive=arrow.gameObject.activeSelf;
            object oldTarget=targetAngle.GetValue(needle),oldHeading=hasHeading.GetValue(needle);
            try
            {
                Check(!dialog&&!journal,"Guidance is checked during exploration, not by bypassing a dialog");
                int queries=SeasonNavigation.PhysicsQueryCount;
                // Invoke each real season's HUD update with a moving player. NPC navigation is
                // separate and intentionally not counted as part of these player-arrow calls.
                foreach(var offset in new[]{Vector2.zero,new Vector2(2.35f,-1.7f),new Vector2(-3.15f,2.2f)})
                {
                    player.position=oldPosition+(Vector3)offset;
                    if(body)body.position=player.position;
                    Call(owner,refresh);
                    object destination=Call(owner,"Destination");
                    Vector2 goal=destination is Transform transform?(Vector2)transform.position:(Vector2)destination;
                    Check((overlay.CompassDelta-(goal-(Vector2)player.position)).sqrMagnitude<.000001f,
                        "Chapter "+chapter+" points directly to its current required destination after player movement");
                }
                // Deliberately disagree with the legacy waypoint: a route bend must not steer
                // this destination marker, and headings must not snap to eight directions.
                foreach(float degrees in new[]{17.25f,113.7f,241.2f,359.2f})
                {
                    Vector2 direction=new Vector2(Mathf.Cos(degrees*Mathf.Deg2Rad),Mathf.Sin(degrees*Mathf.Deg2Rad));
                    Vector2 origin=player.position,goal=origin+direction*12.5f;
                    overlay.SetCompass(origin,origin-direction*20,goal,true);
                    needle.Advance(1);
                    Call(overlay,"LateUpdate");
                    Check((overlay.CompassDelta-direction*12.5f).sqrMagnitude<.000001f,"Legacy waypoint and frozen flags cannot redirect/hide the destination");
                    Check(Mathf.Abs(overlay.ObjectiveDistance-12.5f)<.001f&&!overlay.CompassObscured&&arrow.gameObject.activeSelf,"Distance and always-visible guidance remain valid");
                    Vector2 facing=arrow.localRotation*Vector3.up;
                    Check(Vector2.Dot(facing.normalized,direction)>.99999f,"Arrow resolves continuous heading "+degrees+" degrees");
                    var camera=Camera.main;Check(camera,"Guidance camera exists");
                    var root=overlay.GetComponent<RectTransform>();
                    var center=camera.WorldToViewportPoint(player.position+Vector3.up*.8f);
                    var ahead=camera.WorldToViewportPoint(player.position+Vector3.up*.8f+(Vector3)direction*1.8f);
                    Vector2 centerPixels=new Vector2(center.x*root.rect.width,center.y*root.rect.height);
                    Vector2 expectedOffset=new Vector2((ahead.x-center.x)*root.rect.width,(ahead.y-center.y)*root.rect.height);
                    Check(expectedOffset.sqrMagnitude>1&&(arrow.anchoredPosition-centerPixels-expectedOffset).sqrMagnitude<.01f,
                        "Marker orbits the current player on the destination side");
                }
                overlay.SetCompass(player.position,player.position,player.position);
                Call(overlay,"LateUpdate");
                Check(overlay.ObjectiveDistance==0&&!arrow.gameObject.activeSelf&&!float.IsNaN(arrow.localEulerAngles.z),"Arrival hides the marker without an invalid rotation");
                Check(SeasonNavigation.PhysicsQueryCount==queries,"Player guidance updates perform zero route physics queries");
                Debug.Log("DIRECT_GUIDANCE_PASS: chapter "+chapter+"; live destination, moving player, contradictory waypoint, continuous rotation, arrival and zero route queries.");
            }
            finally
            {
                if(body){body.position=oldBodyPosition;body.linearVelocity=oldVelocity;}
                player.position=oldPosition;
                overlay.Dialog.SetActive(dialog);overlay.Journal.SetActive(journal);
                Call(owner,refresh);Call(overlay,"LateUpdate");
                targetAngle.SetValue(needle,oldTarget);hasHeading.SetValue(needle,oldHeading);
                arrow.anchorMin=oldMin;arrow.anchorMax=oldMax;arrow.pivot=oldPivot;arrow.anchoredPosition=oldAnchored;arrow.localRotation=oldRotation;arrow.gameObject.SetActive(oldActive);
                Physics2D.SyncTransforms();
            }
        }
    }
}
#endif
