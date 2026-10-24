using UnityEngine;
using UnityEngine.InputSystem;

namespace AmbrosJourney.Summer
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SummerActor : MonoBehaviour
    {
        public Sprite[] idle, walk, attack;
        public bool isPlayer;
        public Transform follow;
        public SummerLevel level;
        SpriteRenderer visual;
        Rigidbody2D body;
        Material animationMaterial;
        Vector2 movement;
        float attackUntil;
        public void Strike() { attackUntil = Time.time + .45f; }
        void Awake() { visual = GetComponent<SpriteRenderer>(); body = GetComponent<Rigidbody2D>(); animationMaterial = visual.material; }
        void Start() { body = GetComponent<Rigidbody2D>(); }
        void Update()
        {
            movement = Vector2.zero;
            bool blocked = level && level.InputBlocked;
            if (isPlayer && !blocked && Keyboard.current != null)
            {
                var k = Keyboard.current;
                movement = new Vector2((k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0),
                    (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0)).normalized;
                if (k.rKey.wasPressedThisFrame && body) body.position = SummerLevel.Spawn;
            }
            else if (follow && !blocked)
            {
                Vector3 target = follow.position + new Vector3(-1.6f, -.7f, 0);
                if (Vector2.Distance(transform.position, target) > 2) { movement = (target - transform.position).normalized; transform.position = Vector3.MoveTowards(transform.position, target, 3.5f * Time.deltaTime); }
            }
            if (movement.x != 0) visual.flipX = movement.x < 0;
            Sprite[] frames = Time.time < attackUntil && attack != null && attack.Length > 0 ? attack : movement.sqrMagnitude > .01f && walk != null && walk.Length > 0 ? walk : idle;
            if (frames != null && frames.Length > 0) { visual.sprite = frames[(int)(Time.time * 9) % frames.Length]; animationMaterial.mainTexture = visual.sprite.texture; }
            visual.sortingOrder = 100 - Mathf.RoundToInt(transform.position.y * 10);
        }
        void FixedUpdate() { if (isPlayer && body) body.linearVelocity = level && level.InputBlocked ? Vector2.zero : movement * 4.6f; }
        void OnDestroy() { if (animationMaterial) Destroy(animationMaterial); }
    }
}
