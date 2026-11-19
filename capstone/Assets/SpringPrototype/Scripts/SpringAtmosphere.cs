using UnityEngine;
using AmbrosJourney.Audio;
public class SpringAtmosphere : MonoBehaviour
{
    Transform[] petals=new Transform[45], ripples=new Transform[12];
    Vector3[] homes=new Vector3[12];
    static Sprite dot;
    readonly System.Collections.Generic.List<Object> lakeResources=new System.Collections.Generic.List<Object>();
    public static Sprite Pixel {get{if(dot==null)dot=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f,1);return dot;}}
    static GameObject Mark(string name,Color color,int order){var o=new GameObject(name,typeof(SpriteRenderer));var r=o.GetComponent<SpriteRenderer>();r.sprite=Pixel;r.sharedMaterial=new Material(Shader.Find("Sprites/Default"));r.color=color;r.sortingOrder=order;return o;}
    void CreateAudio(){
        var music=gameObject.AddComponent<AudioSource>();
        music.playOnAwake=false;music.loop=true;music.volume=.22f;music.spatialBlend=0;
        music.clip=Resources.Load<AudioClip>("SeasonMusic/andriig-spring-easter-day-music-507171");
        GameAudio.Register(music);if(music.clip)music.Play();else Debug.LogError("Missing Spring background music.");
    }
    void Start(){CreateAudio();
        foreach(var lake in FindObjectsByType<MeshRenderer>())
        {
            if(lake.name!="Stillwater Lake"&&lake.name!="Turquoise shallows"&&lake.name!="Soft shoreline")continue;
            var material=lake.material;lakeResources.Add(material);
            material.color=lake.name=="Soft shoreline"?new Color(.42f,.53f,.31f):lake.name=="Turquoise shallows"?new Color(.30f,.52f,.49f):new Color(.20f,.44f,.48f);
            bool shore=lake.name=="Soft shoreline";
            var texture=new Texture2D(128,128,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Repeat};lakeResources.Add(texture);
            for(int y=0;y<128;y++)for(int x=0;x<128;x++)
            {
                float shade=shore?.78f+Mathf.PerlinNoise(x*.30f,y*.30f)*.40f:.86f+Mathf.PerlinNoise(x*.025f,y*.11f)*.24f;
                if(!shore&&y%13==0&&Mathf.PerlinNoise(x*.09f,y*.2f)>.55f)shade+=.10f;
                texture.SetPixel(x,y,new Color(shade,shade,shade,1));
            }
            texture.Apply();material.mainTexture=texture;
            var filter=lake.GetComponent<MeshFilter>();var mesh=Instantiate(filter.sharedMesh);lakeResources.Add(mesh);
            var vertices=mesh.vertices;var uv=new Vector2[vertices.Length];for(int i=0;i<uv.Length;i++)uv[i]=new Vector2(vertices[i].x/4,vertices[i].y/4);
            mesh.uv=uv;filter.sharedMesh=mesh;
        }
        for(int i=0;i<petals.Length;i++){var o=Mark("Drifting blossom",new Color(1,.76f,.78f,.75f),950);o.transform.SetParent(transform);o.transform.localScale=new Vector3(.10f,.055f,1);petals[i]=o.transform;}
        for(int i=0;i<ripples.Length;i++){var o=Mark("Lake shimmer",new Color(.64f,.83f,.80f,.20f),-940);o.transform.SetParent(transform);float angle=i*2.39996f;float radius=Mathf.Sqrt((i+.5f)/ripples.Length);homes[i]=new Vector3(-2+Mathf.Cos(angle)*radius*4.5f,3+Mathf.Sin(angle)*radius*2.8f,0);ripples[i]=o.transform;}
    }
    void OnDestroy(){foreach(var resource in lakeResources)if(resource)Destroy(resource);}
    void Update(){var cam=Camera.main;if(!cam)return;for(int i=0;i<petals.Length;i++){float x=Mathf.Repeat(i*3.731f+Time.time*.45f,28)-14;float y=Mathf.Repeat(i*2.17f-Time.time*.36f,18)-9;petals[i].position=cam.transform.position+new Vector3(x+Mathf.Sin(Time.time+i)*.3f,y,10);}
        for(int i=0;i<ripples.Length;i++){ripples[i].position=homes[i]+Vector3.right*Mathf.Sin(Time.time*.6f+i)*.10f;ripples[i].localScale=new Vector3(.18f+.16f*(.5f+.5f*Mathf.Sin(Time.time*.6f+i)),.025f,1);}}
}
