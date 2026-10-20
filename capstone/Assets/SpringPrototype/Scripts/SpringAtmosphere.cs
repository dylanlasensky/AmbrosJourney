using UnityEngine;
public class SpringAtmosphere : MonoBehaviour
{
    Transform[] petals=new Transform[45], ripples=new Transform[22];
    Vector3[] homes=new Vector3[22];
    static Sprite dot;
    public static Sprite Pixel {get{if(dot==null)dot=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f,1);return dot;}}
    static GameObject Mark(string name,Color color,int order){var o=new GameObject(name,typeof(SpriteRenderer));var r=o.GetComponent<SpriteRenderer>();r.sprite=Pixel;r.sharedMaterial=new Material(Shader.Find("Sprites/Default"));r.color=color;r.sortingOrder=order;return o;}
    void Start(){var rand=new System.Random(14);for(int i=0;i<petals.Length;i++){var o=Mark("Drifting blossom",new Color(1,.76f,.78f,.75f),950);o.transform.SetParent(transform);o.transform.localScale=new Vector3(.10f,.055f,1);petals[i]=o.transform;}
        for(int i=0;i<ripples.Length;i++){var o=Mark("Lake shimmer",new Color(.61f,.92f,.84f,.5f),-940);o.transform.SetParent(transform);homes[i]=new Vector3(-7+(float)rand.NextDouble()*10,0+(float)rand.NextDouble()*6,0);ripples[i]=o.transform;}
    }
    void Update(){var cam=Camera.main;if(!cam)return;for(int i=0;i<petals.Length;i++){float x=Mathf.Repeat(i*3.731f+Time.time*.45f,28)-14;float y=Mathf.Repeat(i*2.17f-Time.time*.36f,18)-9;petals[i].position=cam.transform.position+new Vector3(x+Mathf.Sin(Time.time+i)*.3f,y,10);}
        for(int i=0;i<ripples.Length;i++){ripples[i].position=homes[i]+Vector3.right*Mathf.Sin(Time.time*.6f+i)*.3f;ripples[i].localScale=new Vector3(.3f+.5f*(.5f+.5f*Mathf.Sin(Time.time+i)),.035f,1);}}
}
