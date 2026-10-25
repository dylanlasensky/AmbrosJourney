using UnityEngine;
namespace AmbrosJourney.Presentation
{
    // Same subtle camera-relative drift as Spring's blossom atmosphere, in autumn colors.
    public sealed class FallLeaves : MonoBehaviour
    {
        readonly Transform[] leaves=new Transform[32];
        Sprite leaf;
        void Start()
        {
            var texture=new Texture2D(5,3,TextureFormat.RGBA32,false);for(int y=0;y<3;y++)for(int x=0;x<5;x++)texture.SetPixel(x,y,(x==0||x==4)&&(y==0||y==2)?Color.clear:Color.white);texture.filterMode=FilterMode.Point;texture.Apply();leaf=Sprite.Create(texture,new Rect(0,0,5,3),Vector2.one*.5f,32);
            for(int i=0;i<leaves.Length;i++){var o=new GameObject("Drifting autumn leaf",typeof(SpriteRenderer));o.transform.SetParent(transform,false);var r=o.GetComponent<SpriteRenderer>();r.sprite=leaf;r.color=i%2==0?new Color(1,.65f,.20f,.7f):new Color(.84f,.34f,.18f,.7f);r.sortingOrder=950;leaves[i]=o.transform;}
        }
        void Update()
        {
            var camera=Camera.main;if(!camera)return;float h=camera.orthographicSize+2,w=h*camera.aspect;
            for(int i=0;i<leaves.Length;i++){float x=Mathf.Repeat(i*3.731f+Time.time*.45f,w*2)-w,y=Mathf.Repeat(i*2.17f-Time.time*.36f,h*2)-h;leaves[i].position=new Vector3(camera.transform.position.x+x+Mathf.Sin(Time.time+i)*.3f,camera.transform.position.y+y,0);leaves[i].rotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time+i)*35);}
        }
        void OnDestroy(){if(leaf){Destroy(leaf.texture);Destroy(leaf);}}
    }
}
