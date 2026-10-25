using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace AmbrosJourney.Presentation
{
    // Visual-only trail surface. The authored route and its collision geometry remain authoritative.
    // A sampled contour replaces square tile edges, with deterministic stones and worn soil.
    public sealed class NaturalTrail : MonoBehaviour
    {
        const int Pixels=16, Chunk=16;
        readonly List<Object> owned=new List<Object>();
        public static void Apply(Tilemap map,int season,TileBase fill=null,bool onlyPath=false)
        {
            var cells=new HashSet<Vector2Int>();
            foreach(var p in map.cellBounds.allPositionsWithin)
            {
                var tile=map.GetTile(p);if(!tile)continue;
                if(onlyPath&&!tile.name.ToLowerInvariant().Contains("path")&&!(tile is Tile t&&t.sprite&&t.sprite.name.ToLowerInvariant().Contains("path")))continue;
                cells.Add(new Vector2Int(p.x,p.y));
            }
            if(cells.Count==0)return;
            var host=new GameObject("Natural trail / blended shoulders");host.transform.SetParent(map.transform,false);var surface=host.AddComponent<NaturalTrail>();
            surface.Build(cells,season,map.GetComponent<TilemapRenderer>().sortingOrder+1);
            foreach(var p in cells)map.SetTile(new Vector3Int(p.x,p.y,0),fill);
        }
        public static void Spring()
        {
            foreach(var map in Object.FindObjectsByType<Tilemap>())
            {
                if(!map.name.Contains("Hand-laid"))continue;
                TileBase grass=null;foreach(var p in map.cellBounds.allPositionsWithin){var t=map.GetTile(p);if(t&&t.name.ToLowerInvariant().Contains("grass")){grass=t;break;}}
                Apply(map,1,grass,true);
            }
        }
        static float Noise(int x,int y){unchecked{uint h=(uint)(x*374761393+y*668265263);h=(h^(h>>13))*1274126177;return (h&65535)/65535f;}}
        static float Coverage(HashSet<Vector2Int> cells,float x,float y)
        {
            x-=.5f;y-=.5f;int ix=Mathf.FloorToInt(x),iy=Mathf.FloorToInt(y);float fx=x-ix,fy=y-iy;
            float a=cells.Contains(new Vector2Int(ix,iy))?1:0,b=cells.Contains(new Vector2Int(ix+1,iy))?1:0,c=cells.Contains(new Vector2Int(ix,iy+1))?1:0,d=cells.Contains(new Vector2Int(ix+1,iy+1))?1:0;
            return Mathf.Lerp(Mathf.Lerp(a,b,fx),Mathf.Lerp(c,d,fx),fy);
        }
        void Build(HashSet<Vector2Int> cells,int season,int order)
        {
            var chunks=new HashSet<Vector2Int>();foreach(var p in cells)for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++)chunks.Add(new Vector2Int(Mathf.FloorToInt((p.x+x)/(float)Chunk),Mathf.FloorToInt((p.y+y)/(float)Chunk)));
            Color soil=season==4?new Color(.64f,.73f,.80f):season==3?new Color(.53f,.40f,.29f):new Color(.62f,.51f,.34f);
            foreach(var chunk in chunks)
            {
                int size=Chunk*Pixels;var colors=new Color32[size*size];bool visible=false;
                for(int py=0;py<size;py++)for(int px=0;px<size;px++)
                {
                    int wx=chunk.x*size+px,wy=chunk.y*size+py;float x=(wx+.5f)/Pixels,y=(wy+.5f)/Pixels;
                    float coverage=Coverage(cells,x,y);float edge=.48f+.055f*Mathf.Sin(x*2.3f+y*.8f)+.045f*Mathf.Sin(y*3.1f-x*.6f);
                    float noise=Noise(wx,wy);float alpha=Mathf.Clamp01((coverage-edge)*8+.48f);
                    // Pixel dither only at the soil/grass boundary: no blurred or tiled square border.
                    if(alpha<=0||alpha<noise)continue;visible=true;
                    float grain=(Noise(wx/3,wy/3)-.5f)*.075f+(noise-.5f)*.025f;
                    var color=soil+new Color(grain,grain,grain,0);
                    if(coverage<.67f)color=Color.Lerp(color,season==4?new Color(.84f,.89f,.93f):new Color(.38f,.40f,.24f),.38f);
                    if(Noise(wx/2,wy/2)>.982f)color=Color.Lerp(color,season==4?new Color(.86f,.91f,.96f):new Color(.76f,.68f,.51f),.55f);
                    color.a=1;colors[py*size+px]=color;
                }
                if(!visible)continue;
                var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;texture.SetPixels32(colors);texture.Apply();owned.Add(texture);
                var sprite=Sprite.Create(texture,new Rect(0,0,size,size),Vector2.zero,Pixels);owned.Add(sprite);
                var part=new GameObject("Worn trail patch");part.transform.SetParent(transform,false);part.transform.localPosition=new Vector3(chunk.x*Chunk,chunk.y*Chunk,0);
                var renderer=part.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=order;var material=new Material(Shader.Find("Sprites/Default"));material.mainTexture=texture;renderer.sharedMaterial=material;owned.Add(material);
            }
        }
        void OnDestroy(){foreach(var item in owned)if(item)Destroy(item);}
    }
}
