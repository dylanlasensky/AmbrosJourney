using UnityEngine;
namespace AmbrosJourney.Presentation
{
    public sealed class SeasonFeedback : MonoBehaviour
    {
        static Sprite pixel;
        SpriteRenderer[] sparks;
        Vector3[] origins,velocity;
        float born;
        public static void Burst(Vector2 position,Color color,int count=14)
        {
            var host=new GameObject("Lesson sparkle");host.transform.position=position;var effect=host.AddComponent<SeasonFeedback>();effect.Initialize(color,count);
        }
        void Initialize(Color color,int count)
        {
            if(!pixel)pixel=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f,1);
            sparks=new SpriteRenderer[count];origins=new Vector3[count];velocity=new Vector3[count];born=Time.time;
            for(int i=0;i<count;i++){var o=new GameObject("Glint",typeof(SpriteRenderer));o.transform.SetParent(transform,false);var r=o.GetComponent<SpriteRenderer>();r.sprite=pixel;r.color=color;r.sortingOrder=1500;o.transform.localScale=new Vector3(.11f,.06f,1);sparks[i]=r;float angle=i*Mathf.PI*2/count;velocity[i]=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*1.8f+Vector3.up*.5f;origins[i]=Vector3.up*.8f;}
        }
        void Update()
        {
            float t=Time.time-born;if(t>1){Destroy(gameObject);return;}
            for(int i=0;i<sparks.Length;i++){sparks[i].transform.localPosition=origins[i]+velocity[i]*t+Vector3.down*t*t*.6f;var c=sparks[i].color;c.a=1-t;sparks[i].color=c;sparks[i].transform.Rotate(0,0,60*Time.deltaTime);}
        }
    }
}
