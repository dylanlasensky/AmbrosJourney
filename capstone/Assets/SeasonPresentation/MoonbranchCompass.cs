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
            // An ivory spearhead, dark indigo edge and seasonal jewel read as one clear tip.
            // Pixel construction matches the game's sprites and stays legible on snow and grass.
            backing=new Texture2D(48,48,TextureFormat.RGBA32,false);
            Color jewel=new[]{new Color(.77f,.48f,.61f),new Color(.40f,.67f,.65f),new Color(.77f,.48f,.29f),new Color(.50f,.67f,.84f)}[Mathf.Clamp(season-1,0,3)];
            for(int y=0;y<48;y++)for(int x=0;x<48;x++)
            {
                float dx=Mathf.Abs(x-23.5f);
                bool head=y>=19&&y<=43&&dx<(44-y)*.55f;
                bool rim=y>=17&&y<=45&&dx<(46-y)*.55f+1;
                bool stem=y>=8&&y<24&&dx<2.5f;
                bool stemRim=y>=7&&y<25&&dx<4;
                // Curving silver branch feathers at the base, not a second needle.
                float branchX=5+(19-y)*.48f;
                bool branch=y>=8&&y<=19&&Mathf.Abs(dx-branchX)<1.4f;
                bool gem=Mathf.Abs(x-23.5f)+Mathf.Abs(y-19)<5;
                Color color=Color.clear;
                if(rim||stemRim)color=new Color(.13f,.17f,.24f);
                if(head||stem)color=x<24?new Color(.96f,.91f,.76f):new Color(.65f,.66f,.69f);
                if(branch)color=new Color(.73f,.70f,.59f);
                if(gem)color=jewel;
                backing.SetPixel(x,y,color);
            }
            backing.Apply();backing.filterMode=FilterMode.Point;
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
