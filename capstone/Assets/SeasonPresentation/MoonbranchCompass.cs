using TMPro;
using UnityEngine;

namespace AmbrosJourney.Presentation
{
    // Stationary seasonal branch frame; only the single-ended moonstone needle rotates.
    public sealed class MoonbranchCompass : MonoBehaviour
    {
        public RectTransform Needle { get; private set; }
        float targetAngle; bool hasHeading;
        Texture2D backing;
        public void Initialize(int season,TMP_FontAsset font)
        {
            backing=new Texture2D(64,64,TextureFormat.RGBA32,false);
            for(int y=0;y<64;y++)for(int x=0;x<64;x++)
            {
                float a=Mathf.Clamp01((30-Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f)))*.5f);
                backing.SetPixel(x,y,new Color(.10f,.075f,.07f,a*.8f));
            }
            backing.Apply();backing.filterMode=FilterMode.Bilinear;
            Image("Quiet backing",backing,88,new Vector2(56,-56));
            var ring=Image("Seasonal moon branches",Resources.Load<Texture2D>("Journey/MoonbranchRings"),112,new Vector2(56,-56));
            int cell=Mathf.Clamp(season-1,0,3);ring.uvRect=new Rect(cell%2*.5f,cell<2?.5f:0,.5f,.5f);
            string[] letters={"N","E","S","W"};
            Vector2[] offsets={new Vector2(0,29),new Vector2(29,0),new Vector2(0,-29),new Vector2(-29,0)};
            for(int i=0;i<4;i++)
            {
                var o=new GameObject(letters[i],typeof(RectTransform),typeof(TextMeshProUGUI));o.transform.SetParent(transform,false);
                var t=o.GetComponent<TextMeshProUGUI>();Center(t.rectTransform,24,new Vector2(56,-56)+offsets[i]);t.font=font;t.fontSize=12;t.text=letters[i];t.alignment=TextAlignmentOptions.Center;t.color=new Color(1,.91f,.69f);t.raycastTarget=false;
            }
            Needle=Image("Moonstone guide / rounded counterweight",Resources.Load<Texture2D>("Journey/MoonbranchNeedle"),84,new Vector2(56,-56)).rectTransform;
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
