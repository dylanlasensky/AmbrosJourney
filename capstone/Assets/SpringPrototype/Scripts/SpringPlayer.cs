using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class SpringPlayer : MonoBehaviour
{
    public Sprite[] idleFrames;
    public Sprite[] walkFrames;
    public float speed = 4f;
    public SpringGuide guide;
    Rigidbody2D body;
    SpriteRenderer visual;
    Vector2 input;
    float clock;
    public Vector2 spawnPoint = new Vector2(0, -4);
    void Awake() { body = GetComponent<Rigidbody2D>(); visual = GetComponent<SpriteRenderer>(); }
    void Update()
    {
        var k = Keyboard.current;
        input = Vector2.zero;
        if (k != null && (guide == null || !guide.DialogueOpen))
        {
            input.x = ((k.dKey.isPressed || k.rightArrowKey.isPressed) ? 1 : 0) - ((k.aKey.isPressed || k.leftArrowKey.isPressed) ? 1 : 0);
            input.y = ((k.wKey.isPressed || k.upArrowKey.isPressed) ? 1 : 0) - ((k.sKey.isPressed || k.downArrowKey.isPressed) ? 1 : 0);
            input = input.normalized;
            if (k.rKey.wasPressedThisFrame) { body.position = spawnPoint; body.linearVelocity = Vector2.zero; }
        }
        if (Mathf.Abs(input.x) > .01f) visual.flipX = input.x < 0;
        clock += Time.deltaTime;
        var frames = input.sqrMagnitude > .01f ? walkFrames : idleFrames;
        if (frames != null && frames.Length > 0) visual.sprite = frames[(int)(clock * 9) % frames.Length];
        visual.sortingOrder = 100 - Mathf.RoundToInt(transform.position.y * 10);
    }
    void FixedUpdate() { body.linearVelocity = input * speed; }
}
