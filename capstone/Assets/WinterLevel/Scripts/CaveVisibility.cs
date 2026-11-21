using UnityEngine;
using UnityEngine.UI;

namespace AmbrosJourney.Winter
{
    // One unlit overlay keeps the cave readable without adding real-time lights to every sprite.
    [DefaultExecutionOrder(200)]
    public sealed class CaveVisibility : MonoBehaviour
    {
        public const float PlayerRadius=3.2f, LanternRadius=2.5f;
        static readonly int WorldRectId=Shader.PropertyToID("_WorldRect"),PlayerId=Shader.PropertyToID("_Player"),ChamberLightId=Shader.PropertyToID("_ChamberLight");
        Vector4 lastWorldRect,lastPlayer;
        float lastChamberLight=-1;
        WinterLevel level;
        Camera view;
        Canvas veil;
        Material material;
        readonly Vector4[] lights=new Vector4[8];
        public bool IsVisible=>veil&&veil.enabled;
        public int LanternCount {get;private set;}
        public float ChamberLight
        {
            get
            {
                if(!level||!level.player||!level.Progress.InCave)return 0;
                if(level.Progress.Complete)return 1;
                var p=(Vector2)level.player.transform.position;
                // Derive the entrance fade from position so pause and saved games restore it exactly.
                return InChamber(p)?Mathf.SmoothStep(0,1,(p.y-WinterWorld.BossChamber.yMin)/1.25f):0;
            }
        }
        public float LabelRange=>level&&level.Progress.InCave&&!(ChamberLight>=.5f&&InChamber(level.player.transform.position))?3.8f:8;
        public static bool InChamber(Vector2 point)
        {
            var room=WinterWorld.BossChamber;
            return point.x>=room.xMin&&point.x<room.xMax&&point.y>=room.yMin&&point.y<room.yMax;
        }
        public void Initialize(WinterLevel owner,Camera camera,Vector2[] lanterns)
        {
            level=owner;view=camera;
            var shader=Resources.Load<Shader>("CaveVisibility");
            if(!shader){Debug.LogError("Missing cave visibility shader.");return;}
            material=new Material(shader);
            var room=WinterWorld.BossChamber;material.SetVector("_Chamber",new Vector4(room.center.x,room.center.y,room.width*.5f,room.height*.5f));
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
            if(ChamberLight>=.5f&&InChamber(point))return true;
            if(Vector2.Distance(point,(Vector2)level.player.transform.position+Vector2.up*.7f)<PlayerRadius)return true;
            for(int i=0;i<LanternCount;i++)if(Vector2.Distance(point,(Vector2)lights[i])<LanternRadius*.8f)return true;
            return false;
        }
        void LateUpdate()
        {
            if(!veil||!level||!view)return;
            bool visible=level.Progress.InCave&&view.enabled;if(veil.enabled!=visible)veil.enabled=visible;
            if(!veil.enabled)return;
            float chamberLight=ChamberLight;if(lastChamberLight!=chamberLight){material.SetFloat(ChamberLightId,chamberLight);lastChamberLight=chamberLight;}
            var low=view.ViewportToWorldPoint(new Vector3(0,0,-view.transform.position.z));
            var high=view.ViewportToWorldPoint(new Vector3(1,1,-view.transform.position.z));
            var worldRect=new Vector4(low.x,low.y,high.x-low.x,high.y-low.y);
            if(lastWorldRect!=worldRect){material.SetVector(WorldRectId,worldRect);lastWorldRect=worldRect;}
            var p=level.player.transform.position;var player=new Vector4(p.x,p.y+.7f,PlayerRadius,0);
            if(lastPlayer!=player){material.SetVector(PlayerId,player);lastPlayer=player;}
        }
        void OnDestroy(){if(material)Destroy(material);}
    }
}
