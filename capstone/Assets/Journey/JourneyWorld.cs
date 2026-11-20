using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using AmbrosJourney.Presentation;
using AmbrosJourney.Winter;
namespace AmbrosJourney.Journey
{
    [DefaultExecutionOrder(-400)]
    public sealed class JourneyWorld : MonoBehaviour
    {
        public static readonly string[] ArtifactNames={"Pattern Seed","Reflection Shard","Privacy Key","Practice Token","Toolmaker's Lens","Safe Message","Purpose Rune","Context Ribbon","Revision Seal","Source Ledger","Witness Crystal","Uncertainty Compass"};
        public static readonly Vector2[] ArtifactPositions={new Vector2(-20,9),new Vector2(6,-5),new Vector2(21,-19),new Vector2(-22,11),new Vector2(17,16),new Vector2(13,-10),new Vector2(-38,-26),new Vector2(-30,26),new Vector2(39,5),new Vector2(-30,-9),new Vector2(28,10),new Vector2(108,12)};
        readonly List<Object> owned=new List<Object>();readonly List<SpriteRenderer> lanterns=new List<SpriteRenderer>();
        readonly List<Transform> ripples=new List<Transform>();readonly List<Vector3> rippleHomes=new List<Vector3>();
        readonly List<GameObject> artifacts=new List<GameObject>();readonly List<Vector3> artifactHomes=new List<Vector3>();
        Material atlasMaterial,pixelMaterial;Sprite pixel;int chapter;WinterLevel winter;SpringGuide spring;AmbrosJourney.Summer.SummerLevel summer;AmbrosJourney.Fall.FallLevel fall;Transform player;
        public List<GameObject> Artifacts=>artifacts;
        Sprite Atlas(int part,float width)
        {
            var texture=Resources.Load<Texture2D>("Journey/WorldAtlas");if(!texture)return null;
            // Measured bounds of the generated atlas; the mountain intentionally spans a wider cell.
            Rect[] bounds={new Rect(34,645,574,497),new Rect(645,645,590,490),new Rect(17,74,648,415),new Rect(689,74,542,406)};
            var b=bounds[part];float sx=texture.width/1254f,sy=texture.height/1254f;b=new Rect(b.x*sx,b.y*sy,b.width*sx,b.height*sy);
            var sprite=Sprite.Create(texture,b,new Vector2(.5f,.08f),b.width/width);owned.Add(sprite);return sprite;
        }
        SpriteRenderer Visual(string name,Vector2 position,Sprite sprite,Transform parent=null)
        {
            var o=new GameObject(name);o.transform.SetParent(parent?parent:transform);o.transform.position=position;var r=o.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=atlasMaterial;r.sortingOrder=100-Mathf.RoundToInt(position.y*10);return r;
        }
        void Start()
        {
            chapter=JourneySession.CurrentChapter();winter=FindAnyObjectByType<WinterLevel>();spring=FindAnyObjectByType<SpringGuide>();summer=FindAnyObjectByType<AmbrosJourney.Summer.SummerLevel>();fall=FindAnyObjectByType<AmbrosJourney.Fall.FallLevel>();player=JourneySession.Instance.Player();if(chapter==0)return;
            var texture=Resources.Load<Texture2D>("Journey/WorldAtlas");atlasMaterial=new Material(Shader.Find("Sprites/Default")){mainTexture=texture};owned.Add(atlasMaterial);
            pixel=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f,1);owned.Add(pixel);pixelMaterial=new Material(Shader.Find("Sprites/Default")){mainTexture=Texture2D.whiteTexture};owned.Add(pixelMaterial);
            foreach(var r in FindObjectsByType<SpriteRenderer>())
            {
                string n=r.name.ToLowerInvariant();
                if(n.Contains("lantern")&&r.sprite)lanterns.Add(r);
            }
            var cohesion=gameObject.AddComponent<WorldCohesion>();cohesion.Build(chapter,winter);
            lanterns.Clear();foreach(var light in FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))if(light.name.ToLowerInvariant().Contains("lantern"))lanterns.Add(light);
            for(int i=0;i<3;i++)
            {
                int id=(chapter-1)*3+i;var position=FindClear(ArtifactPositions[id]);
                Transform parent=winter?(i==2?winter.World.Cavern.transform:winter.World.Outdoors.transform):transform;
                var r=Visual("Artifact / "+ArtifactNames[id],position,null,parent);cohesion.Relic(r,id);cohesion.FrameRelic(r);var label=new GameObject("Artifact caption");label.transform.SetParent(r.transform,false);label.transform.localPosition=new Vector3(0,2,0);label.AddComponent<SeasonWorldLabel>().Caption="[ E ] "+ArtifactNames[id];
                artifacts.Add(r.gameObject);artifactHomes.Add(r.transform.position);
            }
            var sentinel=Resources.Load<Texture2D>("Journey/SentinelAtlas");
            if(fall&&sentinel)
            {
                var frames=new Sprite[4];float w=sentinel.width/2f,h=sentinel.height/2f;
                for(int i=0;i<4;i++){frames[i]=Sprite.Create(sentinel,new Rect((i%2)*w,(i<2?1:0)*h,w,h),new Vector2(.5f,.09f),w/6);owned.Add(frames[i]);}
                fall.guardian.idle=frames;fall.guardian.attack=frames;fall.guardian.transform.localScale=Vector3.one;
            }
            gameObject.AddComponent<BossPortalSequence>().Initialize(chapter,cohesion);
            AddWaterMotion();
            if(chapter>=3)AddClues();
            Physics2D.SyncTransforms();
        }
        static Vector2 FindClear(Vector2 point)
        {
            for(int radius=0;radius<=4;radius++)for(int y=-radius;y<=radius;y++)for(int x=-radius;x<=radius;x++)
            {var p=point+new Vector2(x,y);if(!Physics2D.OverlapCircle(p+Vector2.up*.22f,.7f))return p;}return point;
        }
        void AddClues()
        {
            Vector2[] places=chapter==3?new[]{new Vector2(-29,10),new Vector2(10,20),new Vector2(32,5)}:new[]{new Vector2(-26,-16),new Vector2(18,10),new Vector2(8,22)};
            string[] clues=chapter==3?new[]{"ORCHARD WEST / ARCHIVE ACROSS NORTH BRIDGE","FOLLOW THE RIVER NORTH, THEN TURN EAST","KILN: TAKE THE SOUTHERN SWITCHBACK"}:new[]{"LEDGER NORTH / OBSERVATORY EAST OF THE LAKE","CAVE: LANTERNS LEAD NORTH, THEN WEST","FOLLOW THIS RIDGE EAST TO THE WARDEN"};
            for(int i=0;i<places.Length;i++){var o=new GameObject("Trail directions");o.transform.SetParent(winter?winter.World.Outdoors.transform:transform);o.transform.position=places[i];o.AddComponent<SeasonWorldLabel>().Caption=clues[i];GetComponent<WorldCohesion>().Sign(places[i]-Vector2.up*1.7f);}
        }
        void AddWaterMotion()
        {
            foreach(var map in FindObjectsByType<Tilemap>(FindObjectsInactive.Include))
            {
                string name=map.name.ToLowerInvariant();if(!name.Contains("pond")&&!name.Contains("river")&&!name.Contains("lake"))continue;
                var cells=new HashSet<Vector3Int>();foreach(var cell in map.cellBounds.allPositionsWithin)if(map.HasTile(cell))cells.Add(cell);
                var variants=new Dictionary<int,Tile>();
                foreach(var cell in cells)
                {
                    int mask=(cells.Contains(cell+Vector3Int.left)?0:1)|(cells.Contains(cell+Vector3Int.right)?0:2)|(cells.Contains(cell+Vector3Int.down)?0:4)|(cells.Contains(cell+Vector3Int.up)?0:8);
                    int phaseX=((cell.x%4)+4)%4,phaseY=((cell.y%4)+4)%4;int key=mask+16*(phaseX+4*phaseY);
                    if(!variants.TryGetValue(key,out var tile))
                    {
                        var texture=new Texture2D(32,32,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;owned.Add(texture);
                        Color deep=chapter==4?new Color(.40f,.65f,.73f):new Color(.20f,.44f,.48f);
                        Color shore=chapter==4?new Color(.86f,.93f,.92f):chapter==3?new Color(.58f,.49f,.28f):new Color(.42f,.53f,.31f);
                        for(int y=0;y<32;y++)for(int x=0;x<32;x++)
                        {
                            int edge=32;if((mask&1)!=0)edge=Mathf.Min(edge,x);if((mask&2)!=0)edge=Mathf.Min(edge,31-x);if((mask&4)!=0)edge=Mathf.Min(edge,y);if((mask&8)!=0)edge=Mathf.Min(edge,31-y);
                            int px=x+phaseX*32,py=y+phaseY*32;
                            float grain=Mathf.PerlinNoise(px*.14f,py*.19f)*.07f;
                            float bank=2+Mathf.FloorToInt(Mathf.PerlinNoise(px*.12f,py*.12f)*3);
                            float surface=Mathf.PerlinNoise(px*.025f,py*.11f)*.10f-.05f;
                            Color water=deep+new Color(surface,surface,surface,0);
                            if(chapter!=4&&py%13==0&&Mathf.PerlinNoise(px*.09f,py*.2f)>.55f)water=Color.Lerp(water,new Color(.52f,.70f,.68f),.22f);
                            Color color=edge<bank?shore:Color.Lerp(Color.Lerp(water,shore,.45f),water,Mathf.Clamp01((edge-bank)/9f));
                            texture.SetPixel(x,y,color+new Color(grain,grain,grain,0));
                        }
                        texture.Apply();var sprite=Sprite.Create(texture,new Rect(0,0,32,32),Vector2.one*.5f,32);owned.Add(sprite);
                        tile=ScriptableObject.CreateInstance<Tile>();tile.sprite=sprite;owned.Add(tile);variants[key]=tile;
                    }
                    map.SetTile(cell,tile);
                    // Frozen lakes remain still; reflections stay inside fully surrounded water cells.
                    if(chapter==4||mask!=0||Mathf.Abs(cell.x*13+cell.y*7)%9!=0||ripples.Count>=50)continue;
                    var o=new GameObject("Water / gentle reflection",typeof(SpriteRenderer));o.transform.SetParent(map.transform,false);o.transform.position=map.GetCellCenterWorld(cell);
                    var r=o.GetComponent<SpriteRenderer>();r.sprite=pixel;r.sharedMaterial=pixelMaterial;r.sortingOrder=map.GetComponent<TilemapRenderer>().sortingOrder+1;r.color=new Color(.65f,.83f,.81f,.20f);
                    ripples.Add(o.transform);rippleHomes.Add(o.transform.position);
                }
            }
        }
        void Update()
        {
            var session=JourneySession.Instance;if(!session||chapter==0)return;
            for(int i=0;i<ripples.Count;i++){float wave=Mathf.Sin(Time.time*.8f+i*1.7f);ripples[i].position=rippleHomes[i]+Vector3.right*(wave*.13f);ripples[i].localScale=new Vector3(.15f+.18f*(wave+1),.035f,1);}
            for(int i=0;i<lanterns.Count;i++)if(lanterns[i])lanterns[i].color=Color.Lerp(new Color(1,.85f,.63f),Color.white,.7f+.18f*Mathf.Sin(Time.time*2.2f+i));
            bool interact=!JourneySession.Blocked&&player;
            if(spring&&spring.DialogueOpen)interact=false;
            // Level dialogs can still be active while the persistent menu is closed.
            if(winter&&winter.InputBlocked)interact=false;
            if(summer&&summer.InputBlocked)interact=false;
            if(fall&&fall.InputBlocked)interact=false;
            for(int i=0;i<artifacts.Count;i++)
            {
                int id=(chapter-1)*3+i;var item=artifacts[i];item.SetActive(!session.Data.artifacts[id]);if(!item.activeInHierarchy)continue;
                // Only the collectible floats; buildings and collision stay still.
                item.transform.position=artifactHomes[i]+Vector3.up*(Mathf.Sin(Time.time*1.5f+i)*.045f);
                if(interact&&Vector2.Distance(player.position,artifactHomes[i])<2.3f&&Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame){session.Collect(id);break;}
            }
        }
        void OnDestroy(){foreach(var o in owned)if(o)Destroy(o);}
    }
}
