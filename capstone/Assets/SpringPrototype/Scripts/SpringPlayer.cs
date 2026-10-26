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
    float clock;int animationState=-1;
    public Vector2 spawnPoint = new Vector2(0, -4);
    void Awake() { body = GetComponent<Rigidbody2D>(); body.interpolation=RigidbodyInterpolation2D.Interpolate; visual = GetComponent<SpriteRenderer>(); }
    void Update()
    {
        var k = Keyboard.current;
        input = Vector2.zero;
        if (!AmbrosJourney.Journey.JourneySession.Blocked && k != null && (guide == null || !guide.DialogueOpen))
        {
            input.x = ((k.dKey.isPressed || k.rightArrowKey.isPressed) ? 1 : 0) - ((k.aKey.isPressed || k.leftArrowKey.isPressed) ? 1 : 0);
            input.y = ((k.wKey.isPressed || k.upArrowKey.isPressed) ? 1 : 0) - ((k.sKey.isPressed || k.downArrowKey.isPressed) ? 1 : 0);
            input = input.normalized;
            if (k.rKey.wasPressedThisFrame) { body.position = spawnPoint; body.linearVelocity = Vector2.zero; }
        }
        if (Mathf.Abs(input.x) > .01f) visual.flipX = input.x < 0;
        float actualSpeed=body.linearVelocity.magnitude;int state=actualSpeed>.10f?1:0;if(state!=animationState){clock=0;animationState=state;}clock+=Time.deltaTime*(state==1?actualSpeed*1.9f:5);
        var frames = state==1 ? walkFrames : idleFrames;
        if (frames != null && frames.Length > 0) visual.sprite = frames[(int)clock % frames.Length];
        visual.sortingOrder = 100 - Mathf.RoundToInt(transform.position.y * 10);
    }
    void FixedUpdate() { body.linearVelocity=AmbrosJourney.Journey.JourneySession.Blocked||guide&&guide.DialogueOpen?Vector2.zero:Vector2.MoveTowards(body.linearVelocity,input*speed,(input.sqrMagnitude>.01f?42:65)*Time.fixedDeltaTime); }
}
