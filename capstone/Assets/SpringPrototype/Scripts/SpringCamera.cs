using UnityEngine;
public class SpringCamera : MonoBehaviour
{
    public Transform target;
    public Vector2 worldMin = new Vector2(-16, -12);
    public Vector2 worldMax = new Vector2(16, 12);
    Camera view;Vector3 smoothingVelocity;
    void Awake(){view=GetComponent<Camera>();}
    void LateUpdate(){AmbrosJourney.Presentation.SeasonCameraMotion.Follow(view,target,worldMin,worldMax,ref smoothingVelocity);}
}
