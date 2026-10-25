using UnityEngine;

namespace AmbrosJourney.Summer
{
    [RequireComponent(typeof(Camera))]
    public sealed class SummerCamera : MonoBehaviour
    {
        public Transform target;
        public Vector2 worldMin = new Vector2(-30, -32), worldMax = new Vector2(30, 28);
        Camera view;
        void Awake() { view = GetComponent<Camera>(); }
        Vector3 smoothingVelocity;
        void LateUpdate(){AmbrosJourney.Presentation.SeasonCameraMotion.Follow(view,target,worldMin,worldMax,ref smoothingVelocity);}
    }
}
