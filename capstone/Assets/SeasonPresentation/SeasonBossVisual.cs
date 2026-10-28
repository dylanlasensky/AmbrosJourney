using System;
using UnityEngine;

namespace AmbrosJourney.Presentation
{
    // Seasonal artwork only: lesson state, interaction positions and gate rules stay with each level.
    [DefaultExecutionOrder(10000)]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SeasonBossVisual : MonoBehaviour
    {
        public const float AttackDuration=.55f;
        public Sprite[] IdleFrames {get;private set;}
        public Sprite[] AttackFrames {get;private set;}
        public string ResourceName {get;private set;}
        public float CellWorldWidth {get;private set;}
        public bool PlayingAttack=>Time.time<attackStarted+AttackDuration;
        SpriteRenderer visual;Material material;float attackStarted=-100,flashUntil;Color flashColor;bool flashed;
        Sprite[] frames;
        public static SeasonBossVisual Attach(GameObject target,string resource,float width)
        {
            var texture=Resources.Load<Texture2D>("Journey/Bosses/"+resource);
            if(!texture)throw new InvalidOperationException("Missing seasonal boss atlas: "+resource);
            var boss=target.AddComponent<SeasonBossVisual>();boss.Configure(texture,resource,width);
            return boss;
        }
        void Configure(Texture2D texture,string resource,float width)
        {
            if(texture.width%2!=0||texture.height%2!=0)throw new InvalidOperationException("Boss atlas needs four equal cells");
            ResourceName=resource;CellWorldWidth=width;visual=GetComponent<SpriteRenderer>();
            material=new Material(Shader.Find("Sprites/Default")){mainTexture=texture};visual.sharedMaterial=material;
            transform.localScale=Vector3.one;
            int w=texture.width/2,h=texture.height/2;var pixels=texture.GetPixels32();frames=new Sprite[4];
            for(int i=0;i<4;i++)
            {
                int x=(i%2)*w,y=(i<2?1:0)*h,ground=0;
                // Ignore isolated glow/petal pixels when finding the contact row. Keep X fixed so an extended fist cannot slide the torso.
                for(int row=0;row<h;row++)
                {
                    int solid=0;for(int column=0;column<w;column++)if(pixels[(y+row)*texture.width+x+column].a>128)solid++;
                    if(solid>=Mathf.Max(8,w/50)){ground=row;break;}
                }
                frames[i]=Sprite.Create(texture,new Rect(x,y,w,h),new Vector2(.5f,(ground+.5f)/h),w/width,0,SpriteMeshType.FullRect);
                frames[i].name=resource+" / "+i;
            }
            IdleFrames=new[]{frames[0],frames[1]};AttackFrames=new[]{frames[2],frames[3]};
            var old=GetComponent<SpringSpriteAnimation>();if(old)old.enabled=false;
            var summer=GetComponent<AmbrosJourney.Summer.SummerActor>();if(summer){summer.idle=IdleFrames;summer.attack=AttackFrames;}
            var winter=GetComponent<AmbrosJourney.Winter.WinterActor>();if(winter){winter.idle=IdleFrames;winter.attack=AttackFrames;}
            visual.sprite=IdleFrames[0];
        }
        public void Strike(){attackStarted=Time.time;}
        public void Flash(Color color){flashColor=color;flashUntil=Time.time+.3f;flashed=true;}
        public Sprite SamplePose(float elapsed,bool attack)
        {
            return attack?AttackFrames[elapsed<AttackDuration*.45f?0:1]:IdleFrames[Mathf.FloorToInt(Mathf.Max(0,elapsed)*2.5f)%2];
        }
        void LateUpdate()
        {
            if(!visual||frames==null)return;
            visual.sprite=SamplePose(PlayingAttack?Time.time-attackStarted:Time.time,PlayingAttack);
            material.mainTexture=visual.sprite.texture;visual.sortingOrder=100-Mathf.RoundToInt(transform.position.y*10);
            if(Time.time<flashUntil)visual.color=Color.Lerp(Color.white,flashColor,(flashUntil-Time.time)/.3f);
            else if(flashed){visual.color=Color.white;flashed=false;}
        }
#if UNITY_EDITOR
        // A deterministic preview for atlas review and render checks; runtime uses LateUpdate's scaled clock.
        public void PreviewPose(float elapsed,bool attacking){visual.sprite=SamplePose(elapsed,attacking);material.mainTexture=visual.sprite.texture;}
#endif
        void OnDestroy(){if(frames!=null)foreach(var frame in frames)if(frame)Destroy(frame);if(material)Destroy(material);}
    }
}
