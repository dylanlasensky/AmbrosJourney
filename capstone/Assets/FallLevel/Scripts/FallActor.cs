using UnityEngine;
using UnityEngine.InputSystem;
using AmbrosJourney.Presentation;

namespace AmbrosJourney.Fall
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class FallActor : MonoBehaviour
    {
        public Sprite[] idle,walk,attack;
        public bool isPlayer;
        public Transform follow;
        public FallLevel level;
        readonly SeasonTrail trail=new SeasonTrail();
        SpriteRenderer visual;Rigidbody2D body;Material animationMaterial;
        Vector2 movement;float attackUntil,animationClock,flashUntil;int animationState=-1,trailRevision=-1;
        Color flashColor;
        public void Strike(){attackUntil=Time.time+.45f;}
        public void Flash(Color color){flashColor=color;flashUntil=Time.time+.3f;}
        void Awake(){visual=GetComponent<SpriteRenderer>();body=GetComponent<Rigidbody2D>();animationMaterial=visual.material;}
        void Start(){body=GetComponent<Rigidbody2D>();if(isPlayer)trail.Reset(transform.position);}
        void Update()
        {
            movement=Vector2.zero;bool blocked=level&&level.InputBlocked;
            if(isPlayer)
            {
                trail.Record(transform.position);
                if(!blocked&&Keyboard.current!=null)
                {
                    var k=Keyboard.current;
                    movement=new Vector2((k.dKey.isPressed||k.rightArrowKey.isPressed?1:0)-(k.aKey.isPressed||k.leftArrowKey.isPressed?1:0),(k.wKey.isPressed||k.upArrowKey.isPressed?1:0)-(k.sKey.isPressed||k.downArrowKey.isPressed?1:0)).normalized;
                    if(k.rKey.wasPressedThisFrame&&body){body.position=FallLevel.Spawn;transform.position=FallLevel.Spawn;body.linearVelocity=Vector2.zero;trail.Reset(FallLevel.Spawn);}
                }
            }
            else if(follow&&!blocked)
            {
                int slot=1;var leader=follow.GetComponent<FallActor>();
                while(leader&&leader.follow&&slot<8){leader=leader.follow.GetComponent<FallActor>();slot++;}
                if(leader)
                {
                    leader.trail.Record(leader.transform.position);var destination=leader.trail.Behind(slot*1.8f);
                    Vector2 old=transform.position;
                    if(trailRevision!=leader.trail.Revision){transform.position=destination;trailRevision=leader.trail.Revision;movement=Vector2.zero;}
                    else {transform.position=Vector2.MoveTowards(old,destination,5.8f*Time.deltaTime);movement=((Vector2)transform.position-old)/Mathf.Max(.001f,Time.deltaTime);}
                }
            }
            if(Mathf.Abs(movement.x)>.03f)visual.flipX=movement.x<0;
            float measuredSpeed=isPlayer&&body?body.linearVelocity.magnitude:movement.magnitude;
            int state=Time.time<attackUntil&&attack!=null&&attack.Length>0?2:measuredSpeed>.10f&&walk!=null&&walk.Length>0?1:0;
            if(state!=animationState){animationClock=0;animationState=state;}animationClock+=Time.deltaTime*(state==0?5:state==1?measuredSpeed*1.9f:10);
            var frames=state==2?attack:state==1?walk:idle;
            if(frames!=null&&frames.Length>0){visual.sprite=frames[(int)animationClock%frames.Length];animationMaterial.mainTexture=visual.sprite.texture;}
            visual.color=Time.time<flashUntil?Color.Lerp(Color.white,flashColor,(flashUntil-Time.time)/.3f):Color.white;
            visual.sortingOrder=100-Mathf.RoundToInt(transform.position.y*10);
        }
        void FixedUpdate(){if(isPlayer&&body)body.linearVelocity=level&&level.InputBlocked?Vector2.zero:Vector2.MoveTowards(body.linearVelocity,movement*4.6f,(movement.sqrMagnitude>.01f?42:65)*Time.fixedDeltaTime);}
        void OnDestroy(){if(animationMaterial)Destroy(animationMaterial);}
    }
}
