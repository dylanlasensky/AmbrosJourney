using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AmbrosJourney.Presentation;
using AmbrosJourney.Winter;

namespace AmbrosJourney.Journey
{
    // One art vocabulary and authored doorstep routes. No randomly added houses.
    public sealed class WorldCohesion : MonoBehaviour
    {
        readonly List<Object> owned=new List<Object>();
        readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        readonly Dictionary<Texture,Material> materials=new Dictionary<Texture,Material>();
        public readonly List<Vector2> Doorsteps=new List<Vector2>();
        public int Buildings {get;private set;}
        int season;Transform outside;SpriteRenderer[] original;
        static bool Tree(SpriteRenderer r)=>r.name=="Snow fir"||r.name=="Copper canopy"||r.name=="Summer canopy"||r.name=="Blossom canopy";
        static bool Loose(SpriteRenderer r)=>Tree(r)||r.name=="Snowbank"||r.name=="Autumn undergrowth"||r.name=="Summer undergrowth"||r.name=="Wildflower bed"||r.name=="Mossy boulder";
        Sprite Art(string atlas,int index,float width)
        {
            string key=atlas+index+":"+width;if(sprites.TryGetValue(key,out var existing))return existing;
            var texture=Resources.Load<Texture2D>("Journey/"+atlas);if(!texture)throw new System.InvalidOperationException("Missing cohesion atlas: "+atlas);
            int columns=atlas=="VillageDetails"?3:2;
            float[] xs=columns==3?new[]{0f,.35f,.64f,1f}:atlas=="DiscoveryDetails"?new[]{0f,.55f,1f}:new[]{0f,.5f,1f};
            float[] ys=atlas=="WinterNature"?new[]{0f,.66f,1f}:columns==3?new[]{0f,.39f,.665f,1f}:atlas=="DiscoveryDetails"?new[]{0f,.55f,1f}:new[]{0f,.5f,1f};
            int column=index%columns,row=index/columns;
            int left=Mathf.RoundToInt(xs[column]*texture.width),right=Mathf.RoundToInt(xs[column+1]*texture.width);
            int bottom=Mathf.RoundToInt((1-ys[row+1])*texture.height),top=Mathf.RoundToInt((1-ys[row])*texture.height);
            // Trim transparent padding once, so every root and doorstep has an honest ground pivot.
            var pixels=texture.GetPixels32();int x0=right,y0=top,x1=left,y1=bottom;
            for(int y=bottom;y<top;y++)for(int x=left;x<right;x++)if(pixels[y*texture.width+x].a>100){x0=Mathf.Min(x0,x);x1=Mathf.Max(x1,x);y0=Mathf.Min(y0,y);y1=Mathf.Max(y1,y);}
            if(x0>x1||y0>y1)throw new System.InvalidOperationException("Empty cohesion sprite: "+key);
            var rect=new Rect(x0,y0,x1-x0+1,y1-y0+1);
            var sprite=Sprite.Create(texture,rect,new Vector2(.5f,0),rect.width/width);sprite.name=atlas+" / "+index;sprites[key]=sprite;owned.Add(sprite);return sprite;
        }
        void Skin(SpriteRenderer renderer,string atlas,int index,float width)
        {
            renderer.sprite=Art(atlas,index,width);renderer.color=Color.white;renderer.transform.localScale=Vector3.one;
            if(!materials.TryGetValue(renderer.sprite.texture,out var material)){material=new Material(Shader.Find("Sprites/Default")){mainTexture=renderer.sprite.texture};materials[renderer.sprite.texture]=material;owned.Add(material);}
            renderer.sharedMaterial=material;renderer.sortingOrder=100-Mathf.RoundToInt(renderer.transform.position.y*10);
        }
        SpriteRenderer Prop(string label,int index,Vector2 position,float width,Transform parent=null)
        {
            Vector2[][] keepClear={new[]{new Vector2(-18,16),new Vector2(-14,12),new Vector2(-21,5),new Vector2(7,1),new Vector2(21,-12)},new[]{new Vector2(0,22),new Vector2(-18,12),new Vector2(-15,-6),new Vector2(19,8)},new[]{new Vector2(-38,34),new Vector2(-29,28),new Vector2(-29,8),new Vector2(27,26),new Vector2(28,-22)},new[]{WinterLevel.Spawn,new Vector2(-26,-6),new Vector2(23,7),WinterWorld.Entrance}};
            var trails=FindObjectsByType<NaturalTrail>();
            Vector2 chosen=position;bool found=false;
            foreach(var offset in new[]{Vector2.zero,Vector2.left*1.6f,Vector2.right*1.6f,Vector2.up*1.6f,Vector2.down*1.6f})
            {
                var candidate=position+offset;
                if(keepClear[season-1].Any(p=>Vector2.Distance(p,candidate)<1.8f))continue;
                if(index!=1&&trails.Any(t=>t.Near(candidate,0)))continue;
                if(Physics2D.OverlapCircle(candidate+Vector2.up*.2f,.45f))continue;
                chosen=candidate;found=true;break;
            }
            if(!found)return null;
            position=chosen;
            var obj=new GameObject(label,typeof(SpriteRenderer));obj.transform.SetParent(parent?parent:outside,false);obj.transform.position=position;
            var r=obj.GetComponent<SpriteRenderer>();Skin(r,"VillageDetails",index,width);return r;
        }
        public void Relic(SpriteRenderer renderer)=>Skin(renderer,"DiscoveryDetails",1,1.35f);
        public void Sign(Vector2 position)=>Prop("Carved trail sign",1,position,1.25f);
        public void Build(int chapter,WinterLevel winter)
        {
            season=chapter;outside=winter?winter.World.Outdoors.transform:transform;
            original=FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include);
            foreach(var r in original)
            {
                string name=r.name.ToLowerInvariant();
                if(name.Contains("shelter")||name.Contains("cottage")||r.sprite&&r.sprite.name=="Shelter")Building(r);
                else if(name=="trail sign")Skin(r,"VillageDetails",1,1.25f);
                else if(name.Contains("field note")||name=="message post"||name=="echo archive"||name=="source ledger"||name=="observatory"||r.sprite&&r.sprite.name=="Noticeboard")Skin(r,"VillageDetails",0,2.15f);
                else if(name.Contains("forge")&&name!="forge shelter"||name=="village workshop"||r.sprite&&r.sprite.name=="Workbench")Skin(r,"VillageDetails",2,2.7f);
                else if(name.Contains("lantern"))Skin(r,"VillageDetails",8,1.05f);
                if(r.sprite&&r.sprite.name=="Crate"&&!name.Contains("rail"))Skin(r,"UtilityDetails",1,1.4f);
                else if(r.sprite&&r.sprite.name=="GardenBed")Skin(r,"UtilityDetails",0,1.9f);
                else if(r.sprite&&r.sprite.name=="Target")Skin(r,"UtilityDetails",3,1.5f);
                else if(r.sprite&&r.sprite.name=="Device")Skin(r,"UtilityDetails",2,1.3f);
                else if(r.sprite&&r.sprite.name=="Ward")Skin(r,"DiscoveryDetails",3,.65f);
                if(name.Contains("portal")||name=="daylight exit"||name=="autumn arrival"||name.StartsWith("summer gate"))Skin(r,"DiscoveryDetails",0,2.8f);
                else if(name.Contains("crystal")&&r.sprite)Skin(r,"DiscoveryDetails",3,1.3f);
                else if(chapter!=4&&r.sprite&&r.sprite.name=="Rock")Skin(r,"DiscoveryDetails",2,1.6f);
                if(chapter==4)
                {
                    if(Tree(r))Skin(r,"WinterNature",Mathf.Abs(Mathf.RoundToInt(r.transform.position.x*3+r.transform.position.y))%4==0?1:0,2.8f);
                    else if(name=="snowbank")Skin(r,"WinterNature",r.sprite.name=="Rock"?2:3,1.35f);
                    else if(name=="frosted cave boulder")Skin(r,"DiscoveryDetails",2,1.4f);
                }
            }
            var trails=FindObjectsByType<NaturalTrail>();
            foreach(var r in original)if(r&&Loose(r)&&trails.Any(t=>t.Near(r.transform.position,Tree(r)?1.5f:.5f)))Remove(r.gameObject);
            ThinScenery();
            // Replace the old painted-rectangle Spring posts with the same carved sign family.
            foreach(var r in original)if(r&&r.name=="Trail marker post"){Sign(r.transform.position);Remove(r.gameObject);}else if(r&&r.name=="Trail marker")Remove(r.gameObject);
            if(chapter==1){ClearRect(new Rect(-14,-3,9,12));Prop("Lakeside bench",3,new Vector2(-11,5.3f),2.2f);Prop("Dock lantern",8,new Vector2(-10,3.8f),1);}
            if(chapter==2){Prop("Garden tool bench",3,new Vector2(-24,-12),2);Prop("Village water well",7,new Vector2(22,20),2.2f);ClearSegment(new Vector2(22,19.5f),new Vector2(15,18));NaturalTrail.Spur(outside,season,new Vector2(22,19.5f),new Vector2(19,19.5f),new Vector2(15,18));}
            if(chapter==3){Prop("Orchard harvest cart",5,new Vector2(-34,2),2.5f);Prop("Archive reading bench",3,new Vector2(31,29),2.2f);}
            if(chapter==4){Prop("Winter fuel reserve",4,new Vector2(30,20),1.7f);Prop("Cave entrance lantern",8,new Vector2(27,23),1.2f);}
            Physics2D.SyncTransforms();
        }
        void Building(SpriteRenderer renderer)
        {
            // Move the overlook off its artifact and give it an actual front approach.
            if(renderer.name=="Quiet overlook")renderer.transform.position=new Vector2(-41,-23);
            var p=(Vector2)renderer.transform.position;
            Skin(renderer,"SeasonCottages",season-1,6.2f);Buildings++;
            var collider=renderer.GetComponent<BoxCollider2D>();if(!collider)collider=renderer.gameObject.AddComponent<BoxCollider2D>();collider.size=new Vector2(4.6f,2.4f);collider.offset=new Vector2(0,2.0f);
            ClearRect(new Rect(p.x-4,p.y-2,8,8));
            Vector2 end;
            switch(season)
            {
                case 1:end=new Vector2(-17,10);break;
                case 2:end=p.x<0?new Vector2(-18,12):new Vector2(19,10);break;
                case 3:end=renderer.name=="Quiet overlook"?new Vector2(-38,-26):p.x<0?new Vector2(-29,28):new Vector2(27,26);break;
                default:end=p.x<0?new Vector2(-26,-6):new Vector2(23,7);break;
            }
            Vector2 door=p+Vector2.down*.35f,bend=new Vector2(end.x,door.y);Doorsteps.Add(door);
            ClearSegment(door,bend);ClearSegment(bend,end);NaturalTrail.Spur(outside,season,door,bend,end);
            Prop("Rest stop bench",3,p+new Vector2(-4,-1.5f),1.8f);
            Prop("Sheltered firewood",4,p+new Vector2(3.5f,1),1.5f);
            Prop("Porch lantern",8,p+new Vector2(2.8f,1.1f),.85f);
            if(season<3)Prop("Tended window garden",6,p+new Vector2(-3.7f,1.3f),1.4f);
            if(season==3)Prop("Harvest delivery",5,p+new Vector2(4.2f,3.5f),1.8f);
        }
        void ThinScenery()
        {
            var planted=new List<Vector2>();
            foreach(var r in original.Where(r=>r&&r.gameObject.activeInHierarchy&&Tree(r)).OrderBy(r=>r.transform.position.x).ThenBy(r=>r.transform.position.y))
            {
                var p=(Vector2)r.transform.position;
                if(planted.Any(q=>Vector2.Distance(p,q)<2.7f)){Remove(r.gameObject);continue;}
                planted.Add(p);
            }
        }
        void ClearRect(Rect area){foreach(var r in original)if(r&&Loose(r)&&area.Contains(r.transform.position))Remove(r.gameObject);}
        void ClearSegment(Vector2 a,Vector2 b)
        {
            Vector2 d=b-a;
            foreach(var r in original)if(r&&Loose(r))
            {
                Vector2 p=r.transform.position;float t=d.sqrMagnitude<.01f?0:Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);
                if(Vector2.Distance(p,a+d*t)<2)Remove(r.gameObject);
            }
        }
        static void Remove(GameObject obj){obj.SetActive(false);Destroy(obj);}
        void OnDestroy(){foreach(var obj in owned)if(obj)Destroy(obj);}
    }
}
