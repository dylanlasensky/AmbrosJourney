using UnityEngine;
namespace AmbrosJourney.Winter
{
    public sealed class WinterSnow:MonoBehaviour
    {
        public WinterLevel level;
        readonly Transform[] flakes=new Transform[60];Sprite sprite;Material material;
        void Start(){var t=new Texture2D(3,3);for(int y=0;y<3;y++)for(int x=0;x<3;x++)t.SetPixel(x,y,x==1||y==1?Color.white:Color.clear);t.filterMode=FilterMode.Point;t.Apply();sprite=Sprite.Create(t,new Rect(0,0,3,3),Vector2.one*.5f,28);material=new Material(Shader.Find("Sprites/Default"));material.mainTexture=t;for(int i=0;i<flakes.Length;i++){var o=new GameObject("Snowflake");o.transform.SetParent(transform);var r=o.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=material;r.sortingOrder=800;r.color=new Color(.9f,.96f,1,.45f);flakes[i]=o.transform;}}
        void Update(){if(!Camera.main)return;var c=Camera.main.transform.position;for(int i=0;i<flakes.Length;i++){flakes[i].gameObject.SetActive(!level.Progress.InCave);if(level.Progress.InCave)continue;float x=Mathf.Repeat(i*7.371f+Time.time*.3f,28)-14;float y=Mathf.Repeat(i*3.711f-Time.time*(.35f+i%3*.12f),18)-9;flakes[i].position=new Vector3(c.x+x,c.y+y,0);}}
        void OnDestroy(){if(sprite){Destroy(sprite.texture);Destroy(sprite);}if(material)Destroy(material);}
    }
}
