// Assembles a portal from pixel fragments before enabling its completed appearance.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AmbrosJourney.Presentation
{
    // Rebuild the actual portal art in square chunks, from its foundation upward.
    public sealed class PixelPortalReveal : MonoBehaviour
    {
        public Transform Portal {get;private set;}
        public bool Ready {get;private set;}
        public bool Playing {get;private set;}
        public float Progress {get;private set;}
        readonly List<GameObject> pieces=new List<GameObject>();
        readonly List<Sprite> sprites=new List<Sprite>();
        public void Initialize(Transform portal,bool ready)
        {
            Portal=portal;SetReady(ready);
        }
        public void SetReady(bool ready)
        {
            Ready=ready;Progress=ready?1:0;if(Portal)Portal.gameObject.SetActive(ready);
        }
        public IEnumerator Assemble()
        {
            if(Ready||Playing)yield break;
            Playing=true;Portal.gameObject.SetActive(false);
            var visual=Portal.GetComponent<SpriteRenderer>();var sprite=visual.sprite;var rect=sprite.textureRect;
            int cell=Mathf.Max(1,Mathf.CeilToInt(rect.width/16));var homes=new List<Vector3>();var times=new List<float>();
            var colors=sprite.texture.GetPixels32();
            for(int y=0;y<rect.height;y+=cell)for(int x=0;x<rect.width;x+=cell)
            {
                int w=Mathf.Min(cell,(int)rect.width-x),h=Mathf.Min(cell,(int)rect.height-y);bool solid=false;
                for(int dy=0;dy<h&&!solid;dy++)for(int dx=0;dx<w;dx++)if(colors[((int)rect.y+y+dy)*sprite.texture.width+(int)rect.x+x+dx].a>40){solid=true;break;}
                if(!solid)continue;
                var piece=Sprite.Create(sprite.texture,new Rect(rect.x+x,rect.y+y,w,h),Vector2.one*.5f,sprite.pixelsPerUnit,0,SpriteMeshType.FullRect);sprites.Add(piece);
                var host=new GameObject("Forming portal pixels",typeof(SpriteRenderer));host.transform.SetParent(Portal.parent,true);host.transform.localScale=Portal.lossyScale;
                var r=host.GetComponent<SpriteRenderer>();r.sprite=piece;r.sharedMaterial=visual.sharedMaterial;r.sortingOrder=visual.sortingOrder;r.enabled=false;
                var local=new Vector3(sprite.bounds.min.x+(x+w*.5f)/sprite.pixelsPerUnit,sprite.bounds.min.y+(y+h*.5f)/sprite.pixelsPerUnit,0);
                if(visual.flipX)local.x=-local.x;if(visual.flipY)local.y=-local.y;
                homes.Add(Portal.TransformPoint(local));times.Add(y/rect.height*.70f+((x/cell)%3)*.035f);pieces.Add(host);
            }
            float elapsed=0;
            while(elapsed<1.1f)
            {
                elapsed+=Time.deltaTime;Progress=Mathf.Clamp01(elapsed/1.1f);
                for(int i=0;i<pieces.Count;i++)
                {
                    float t=Mathf.Clamp01((elapsed-times[i])/.28f);var r=pieces[i].GetComponent<SpriteRenderer>();r.enabled=elapsed>=times[i];
                    pieces[i].transform.position=homes[i]+Vector3.up*(.6f*(1-t));r.color=new Color(1,1,1,t);
                }
                yield return null;
            }
            Cleanup();Playing=false;SetReady(true);
            SeasonFeedback.Burst(Portal.position+Vector3.up,new Color(.6f,.95f,1),18);
        }
        void Cleanup(){foreach(var p in pieces)if(p)Destroy(p);pieces.Clear();foreach(var s in sprites)if(s)Destroy(s);sprites.Clear();}
        void OnDestroy(){Cleanup();}
    }
}
