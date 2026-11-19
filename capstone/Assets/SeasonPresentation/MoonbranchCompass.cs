using TMPro;
using UnityEngine;

namespace AmbrosJourney.Presentation
{
    // A single-headed marker that rotates continuously toward the next route waypoint.
    public sealed class MoonbranchCompass : MonoBehaviour
    {
        public RectTransform Needle { get; private set; }
        float targetAngle; bool hasHeading;
        Texture2D backing;
        public void Initialize(int season,TMP_FontAsset font)
        {
            backing=new Texture2D(32,32,TextureFormat.RGBA32,false);
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)
            {
                float dx=Mathf.Abs(x-15.5f);bool head=y>=13&&dx<(31-y)*.72f;bool shaft=y>=3&&y<18&&dx<3;
                bool rim=(y>=12&&dx<(32-y)*.72f+1)||(y>=2&&y<19&&dx<4);
                backing.SetPixel(x,y,head||shaft?new Color(1,.86f,.36f):rim?new Color(.12f,.15f,.18f):Color.clear);
            }
            backing.Apply();backing.filterMode=FilterMode.Bilinear;
            Needle=Image("Direction arrow",backing,38,Vector2.zero).rectTransform;
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
        void Update()=>Advance(Time.deltaTime);
        public void Advance(float deltaTime)
        {
            if(!hasHeading||!Needle)return;
            float angle=Mathf.LerpAngle(Needle.localEulerAngles.z,targetAngle,1-Mathf.Exp(-14*Mathf.Max(0,deltaTime)));
            Needle.localRotation=Quaternion.Euler(0,0,angle);
        }
        void OnDestroy(){if(backing)Destroy(backing);}
    }
}
