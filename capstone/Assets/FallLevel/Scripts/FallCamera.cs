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
        void LateUpdate()
        {
            if (!target) return;
            float h = view.orthographicSize, w = h * view.aspect;
            var p = target.position;
            p.x = w * 2 >= worldMax.x - worldMin.x ? (worldMin.x + worldMax.x) / 2 : Mathf.Clamp(p.x, worldMin.x + w, worldMax.x - w);
            p.y = h * 2 >= worldMax.y - worldMin.y ? (worldMin.y + worldMax.y) / 2 : Mathf.Clamp(p.y, worldMin.y + h, worldMax.y - h);
            p.z = -10;
            transform.position = Vector3.Lerp(transform.position, p, 1 - Mathf.Exp(-9 * Time.deltaTime));
        }
    }
}
