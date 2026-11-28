// Follows the Winter player within the season's world bounds.
using UnityEngine;

namespace AmbrosJourney.Winter
{
    [RequireComponent(typeof(Camera))]
    public sealed class WinterCamera : MonoBehaviour
    {
        public Transform target;
        public Vector2 worldMin = new Vector2(-49, -48), worldMax = new Vector2(49, 44);
        Camera view;float normalSize;
        void Awake() { view = GetComponent<Camera>();normalSize=view.orthographicSize; }
        Vector3 smoothingVelocity;
        void LateUpdate()
        {
            if(!target)return;
            var p=(Vector2)target.position;var room=WinterWorld.BossChamber;
            float framing=CaveVisibility.InChamber(p)?Mathf.SmoothStep(0,1,(p.y-room.yMin)/1.25f):0;
            // Show the Keeper's whole silhouette and name card, while retaining the tunnel's close view.
            view.orthographicSize=Mathf.Lerp(normalSize,9,framing);
            AmbrosJourney.Presentation.SeasonCameraMotion.Follow(view,target,worldMin,worldMax,ref smoothingVelocity,(room.center-p)*framing);
        }
    }
}
