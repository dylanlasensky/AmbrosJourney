using UnityEngine;
public class SpringCamera : MonoBehaviour
{
    public Transform target;
    public Vector2 worldMin = new Vector2(-16, -12);
    public Vector2 worldMax = new Vector2(16, 12);
    void LateUpdate()
    {
        if (target == null) return;
        var cam = GetComponent<Camera>();
        float h = cam.orthographicSize, w = h * cam.aspect;
        Vector3 p = target.position;
        p.x = w * 2 >= worldMax.x-worldMin.x ? 0 : Mathf.Clamp(p.x, worldMin.x+w, worldMax.x-w);
        p.y = h * 2 >= worldMax.y-worldMin.y ? 0 : Mathf.Clamp(p.y, worldMin.y+h, worldMax.y-h);
        p.z = -10;
        transform.position = Vector3.Lerp(transform.position, p, 1-Mathf.Exp(-9*Time.deltaTime));
    }
}
