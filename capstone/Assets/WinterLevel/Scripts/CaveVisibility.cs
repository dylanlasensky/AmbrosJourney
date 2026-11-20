using UnityEngine;
using UnityEngine.UI;

namespace AmbrosJourney.Winter
{
    // One unlit overlay keeps the cave readable without adding real-time lights to every sprite.
    [DefaultExecutionOrder(200)]
    public sealed class CaveVisibility : MonoBehaviour
    {
        public const float PlayerRadius=3.2f, LanternRadius=2.5f;
        WinterLevel level;
        Camera view;
        Canvas veil;
        Material material;
        readonly Vector4[] lights=new Vector4[8];
        public bool IsVisible=>veil&&veil.enabled;
        public int LanternCount {get;private set;}
        public void Initialize(WinterLevel owner,Camera camera,Vector2[] lanterns)
        {
            level=owner;view=camera;
            var shader=Resources.Load<Shader>("CaveVisibility");
            if(!shader){Debug.LogError("Missing cave visibility shader.");return;}
            material=new Material(shader);
            LanternCount=Mathf.Min(lanterns.Length,lights.Length);
            for(int i=0;i<LanternCount;i++)lights[i]=new Vector4(lanterns[i].x,lanterns[i].y+.65f,LanternRadius,0);
            material.SetVectorArray("_Lanterns",lights);material.SetInt("_LanternCount",LanternCount);
            var root=new GameObject("Cave darkness",typeof(RectTransform),typeof(Canvas));root.transform.SetParent(transform,false);
            veil=root.GetComponent<Canvas>();veil.renderMode=RenderMode.ScreenSpaceOverlay;veil.sortingOrder=90;veil.enabled=false;
            var panel=new GameObject("Lantern-lit visibility",typeof(RectTransform),typeof(RawImage));panel.transform.SetParent(root.transform,false);
            var rect=panel.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            var image=panel.GetComponent<RawImage>();image.texture=Texture2D.whiteTexture;image.material=material;image.raycastTarget=false;
        }
        public bool Reveals(Vector2 point)
        {
            if(!level||!level.Progress.InCave)return true;
            if(Vector2.Distance(point,(Vector2)level.player.transform.position+Vector2.up*.7f)<PlayerRadius)return true;
            for(int i=0;i<LanternCount;i++)if(Vector2.Distance(point,(Vector2)lights[i])<LanternRadius*.8f)return true;
            return false;
        }
        void LateUpdate()
        {
            if(!veil||!level||!view)return;
            veil.enabled=level.Progress.InCave&&view.enabled;
            if(!veil.enabled)return;
            var low=view.ViewportToWorldPoint(new Vector3(0,0,-view.transform.position.z));
            var high=view.ViewportToWorldPoint(new Vector3(1,1,-view.transform.position.z));
            material.SetVector("_WorldRect",new Vector4(low.x,low.y,high.x-low.x,high.y-low.y));
            var p=level.player.transform.position;material.SetVector("_Player",new Vector4(p.x,p.y+.7f,PlayerRadius,0));
        }
        void OnDestroy(){if(material)Destroy(material);}
    }
}
