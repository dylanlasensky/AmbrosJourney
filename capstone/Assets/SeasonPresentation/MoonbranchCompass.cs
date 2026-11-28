// Renders the seasonal compass and direct objective arrow with distance feedback.
using TMPro;
using UnityEngine;

namespace AmbrosJourney.Presentation
{
    // A single-headed marker that rotates continuously toward the next required lesson or interaction.
    public sealed class MoonbranchCompass : MonoBehaviour
    {
        public const float MarkerSize=52;
        public RectTransform Needle { get; private set; }
        float targetAngle; bool hasHeading;
        Texture2D backing;
        public void Initialize(int season,TMP_FontAsset font)
        {
            // The same clear gold arrow in every season: a broad head, short flat tail,
            // and an opaque dark outline that stays distinct against snow, grass and cave rock.
            backing=new Texture2D(48,48,TextureFormat.RGBA32,false);
            Color cream=new Color32(255,246,202,255),gold=new Color32(255,204,62,255),outline=new Color32(21,28,43,255);
            for(int y=0;y<48;y++)for(int x=0;x<48;x++)
            {
                Color color=Color.clear;
                if(Silhouette(x,y))color=x<24?cream:gold;
                else
                {
                    bool edge=false;
                    for(int dy=-2;dy<=2&&!edge;dy++)for(int dx=-2;dx<=2;dx++)
                        if(Silhouette(x+dx,y+dy)){edge=true;break;}
                    if(edge)color=outline;
                }
                backing.SetPixel(x,y,color);
            }
            backing.Apply();backing.filterMode=FilterMode.Point;backing.wrapMode=TextureWrapMode.Clamp;
            Needle=Image("Direction arrow",backing,MarkerSize,Vector2.zero).rectTransform;
        }
        static bool Silhouette(int x,int y)
        {
            float dx=Mathf.Abs(x-23.5f);
            return y>=20&&y<=43&&dx<=(44-y)*.75f||y>=6&&y<23&&dx<=4.5f;
        }

        UnityEngine.UI.RawImage Image(string label,Texture texture,float size,Vector2 position)
        {
            var obj=new GameObject(label,typeof(RectTransform),typeof(UnityEngine.UI.RawImage));obj.transform.SetParent(transform,false);
            var image=obj.GetComponent<UnityEngine.UI.RawImage>();Center(image.rectTransform,size,position);image.texture=texture;image.raycastTarget=false;return image;
        }
        static void Center(RectTransform rect,float size,Vector2 position)
        {rect.anchorMin=rect.anchorMax=Vector2.up;rect.pivot=Vector2.one*.5f;rect.sizeDelta=Vector2.one*size;rect.anchoredPosition=position;}
        public void Point(Vector2 direction)
        {
            if(direction.sqrMagnitude<.001f)return;
            targetAngle=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-90;
            if(!hasHeading){Needle.localRotation=Quaternion.Euler(0,0,targetAngle);hasHeading=true;}
        }
        void Update()
        {
            Advance(Time.deltaTime);
            // A slow 0-5% size change draws the eye without flashing or fading the pointer.
            if(Needle)Needle.localScale=Vector3.one*(1.025f+.025f*Mathf.Sin(Time.time*Mathf.PI*.8f));
        }
        public void Advance(float deltaTime)
        {
            if(!hasHeading||!Needle)return;
            float angle=Mathf.LerpAngle(Needle.localEulerAngles.z,targetAngle,1-Mathf.Exp(-14*Mathf.Max(0,deltaTime)));
            Needle.localRotation=Quaternion.Euler(0,0,angle);
        }
        void OnDestroy(){if(backing)Destroy(backing);}
    }
}
