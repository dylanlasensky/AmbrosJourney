using UnityEngine;

namespace AmbrosJourney.Fall
{
    [RequireComponent(typeof(Camera))]
    public sealed class FallCamera : MonoBehaviour
    {
        public Transform target;
        public Vector2 worldMin = new Vector2(-49, -48), worldMax = new Vector2(49, 44);
        Camera view;
        void Awake() { view = GetComponent<Camera>(); }
        Vector3 smoothingVelocity;
        void LateUpdate(){AmbrosJourney.Presentation.SeasonCameraMotion.Follow(view,target,worldMin,worldMax,ref smoothingVelocity);}
    }
}
