using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using AmbrosJourney.Presentation;
using UnityEngine.Tilemaps;

namespace AmbrosJourney.Fall
{
    [Serializable]
    public sealed class FallArtwork
    {
        public string key;
        public Texture2D texture;
        public float pixelsPerUnit = 32;
        public Vector2 pivot = new Vector2(.5f, 0);
    }
    // Both Fall.unity and the portal's Resources prefab use this same world definition.
    public sealed class FallWorld : MonoBehaviour
    {
        public FallArtwork[] artwork;
        readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        readonly Dictionary<Texture2D, Material> spriteMaterials = new Dictionary<Texture2D, Material>();
        Material pixelMaterial;
        TMP_FontAsset runtimeFont;
        FallLevel level;
        void Start() { BuildWorld(); }
        Sprite Art(string key)
        {
            if (sprites.TryGetValue(key, out var sprite)) return sprite;
            foreach (var a in artwork) if (a.key == key && a.texture)
            {
                sprite = Sprite.Create(a.texture, new Rect(0, 0, a.texture.width, a.texture.height), a.pivot, a.pixelsPerUnit);
                sprite.name = key; sprites.Add(key, sprite); return sprite;
            }
            throw new InvalidOperationException("Missing Fall artwork: " + key);
        }
        Sprite[] Frames(string prefix, int count)
        { var result = new Sprite[count]; for (int i = 0; i < count; i++) result[i] = Art(prefix + (i + 1)); return result; }
        GameObject ObjectAt(string label, Vector2 p)
        { var o = new GameObject(label); o.transform.SetParent(transform); o.transform.position = p; return o; }
        SpriteRenderer Visual(string label, string art, Vector2 p, float scale = 1)
        {
            var o = ObjectAt(label, p); o.transform.localScale = Vector3.one * scale;
            var r = o.AddComponent<SpriteRenderer>(); r.sprite = Art(art);
            // Distinct texture materials avoid SRP batching different runtime sprites with one texture.
            if (!spriteMaterials.TryGetValue(r.sprite.texture, out var material))
            { material = new Material(pixelMaterial); material.mainTexture = r.sprite.texture; spriteMaterials.Add(r.sprite.texture, material); }
            r.sharedMaterial = material; r.sortingOrder = 100 - Mathf.RoundToInt(p.y * 10); return r;
        }
        void Block(string label, Vector2 p, Vector2 size)
        { var o = ObjectAt(label, p); o.AddComponent<BoxCollider2D>().size = size; }
        void Label(string text, Vector2 p, float width = 8)
        {
            var o=ObjectAt(text,p);o.AddComponent<SeasonWorldLabel>().Caption=text;
        }
        FallActor Actor(string label, string prefix, int count, Vector2 p, string attackPrefix = null, int attackCount = 0, float scale = 1)
        {
            var r = Visual(label, prefix + "1", p, scale); var actor = r.gameObject.AddComponent<FallActor>();
            actor.idle = Frames(prefix, count); actor.level = level;
            if (attackCount > 0) actor.attack = Frames(attackPrefix, attackCount);
            return actor;
        }
        Tilemap Map(string label, int order)
        {
            var o = ObjectAt(label, Vector2.zero); o.AddComponent<Grid>();
            var child = new GameObject(label + " tiles", typeof(Tilemap), typeof(TilemapRenderer)); child.transform.SetParent(o.transform, false);
            var r = child.GetComponent<TilemapRenderer>(); r.sortingOrder = order; r.sharedMaterial = pixelMaterial;
            return child.GetComponent<Tilemap>();
        }
        Tile Tile(string key) { var tile = ScriptableObject.CreateInstance<Tile>(); tile.sprite = Art(key); tile.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.None; return tile; }
        public static readonly Vector2 Min=new Vector2(-49,-48), Max=new Vector2(49,44);
        public static readonly Vector2[] MissionPositions={new Vector2(-29,8),new Vector2(27,26),new Vector2(28,-22)};
        public static readonly Vector2 GuardianPosition=new Vector2(0,-41), GatePosition=new Vector2(0,-46);
        readonly List<Vector2> trail=new List<Vector2>{new Vector2(-38,34),new Vector2(-29,28),new Vector2(-29,8),new Vector2(-16,8),new Vector2(-16,20),new Vector2(10,20),new Vector2(27,26),new Vector2(34,26),new Vector2(34,6),new Vector2(12,6),new Vector2(12,-8),new Vector2(28,-8),new Vector2(28,-22),new Vector2(12,-25),new Vector2(0,-29),new Vector2(0,-41),new Vector2(0,-46)};
        public void BuildWorld()
        {
            if(GetComponent<FallLevel>())return;
            pixelMaterial=new Material(Shader.Find("Sprites/Default"));Resources.Load<Material>("SummerText");runtimeFont=Resources.Load<TMP_FontAsset>("SummerDefault");
            if(!runtimeFont)throw new InvalidOperationException("Fall reuses Summer's font. Keep Assets/SummerLevel/Resources intact.");
            gameObject.AddComponent<FallLeaves>();level=gameObject.AddComponent<FallLevel>();level.font=runtimeFont;
            var ground=Map("Autumn forest floor",-1000);var paths=Map("Emberwood branching trails",-990);var river=Map("Amber river / two crossings",-980);
            var grass=Tile("Grass");var flowers=Tile("Flowers");var dirt=Tile("Path");var water=Tile("Water");
            var rng=new System.Random(31024);
            for(int y=-48;y<=44;y++)for(int x=-49;x<=49;x++)ground.SetTile(new Vector3Int(x,y,0),rng.Next(35)==0?flowers:grass);
            for(int i=1;i<trail.Count;i++)Trail(paths,dirt,trail[i-1],trail[i],1);
            Trail(paths,dirt,new Vector2(-29,8),new Vector2(-29,-14),1);Trail(paths,dirt,new Vector2(-29,-14),new Vector2(12,-14),1);Trail(paths,dirt,new Vector2(12,-14),new Vector2(12,-8),1);
            Trail(paths,dirt,new Vector2(-29,-14),new Vector2(-38,-26),1); // optional outlook branch
            for(int y=-30;y<=43;y++)for(int x=-6;x<=-2;x++)
            {
                bool crossing=Mathf.Abs(y-20)<=2 || Mathf.Abs(y+14)<=2;
                if(crossing){paths.SetTile(new Vector3Int(x,y,0),dirt);if(y==18||y==22||y==-16||y==-12)Visual("Bridge rail","Crate",new Vector2(x+.5f,y+.5f),.45f);}
                else {river.SetTile(new Vector3Int(x,y,0),water);Block("River shoreline",new Vector2(x+.5f,y+.5f),Vector2.one);}
            }
            Hedge(12,-1,31);Hedge(12,37,48); // first passage at x=34
            Hedge(-2,-1,9);Hedge(-2,15,48); // next passage at x=12
            for(int x=-48;x<=48;x+=2)if(Mathf.Abs(x)>3){Visual("Bramble arena perimeter","Rock",new Vector2(x,-34));Block("Arena wall",new Vector2(x,-34),new Vector2(2,1));}
            Block("West boundary",new Vector2(-50,-2),new Vector2(1,95));Block("East boundary",new Vector2(50,-2),new Vector2(1,95));Block("North boundary",new Vector2(0,45),new Vector2(101,1));Block("South boundary",new Vector2(0,-49),new Vector2(101,1));
            for(int i=0;i<480;i++)
            {
                Vector2 pos=new Vector2(rng.Next(-48,49),rng.Next(-31,44));if(!SceneryAllowed(pos))continue;
                var r=Visual("Copper canopy","Tree",pos,.8f+(float)rng.NextDouble()*.2f);var c=r.gameObject.AddComponent<BoxCollider2D>();c.size=new Vector2(.6f,.4f);c.offset=new Vector2(0,.2f);
            }
            for(int i=0;i<95;i++){Vector2 pos=new Vector2(rng.Next(-48,49),rng.Next(-31,44));if(SceneryAllowed(pos))Visual("Autumn undergrowth",i%4==0?"Rock":"Bush",pos,.8f);}
            Visual("Summer arrival portal","Portal",new Vector2(-38,38),1.4f);Label("SUMMER ARRIVAL",new Vector2(-38,41));
            level.player=Actor("Ambros","AmbrosIdle",4,FallLevel.Spawn,"AmbrosAttack",5);level.player.isPlayer=true;level.player.walk=Frames("AmbrosWalk",8);
            var body=level.player.gameObject.AddComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            var capsule=level.player.gameObject.AddComponent<CapsuleCollider2D>();capsule.size=new Vector2(.55f,.38f);capsule.direction=CapsuleDirection2D.Horizontal;capsule.offset=new Vector2(0,.22f);
            level.mage=Actor("Ancient Mage","MageIdle",4,new Vector2(-40,33));level.mage.walk=Frames("MageWalk",8);
            level.maleWarrior=Actor("Married Warrior","MaleIdle",4,new Vector2(-42,33));level.maleWarrior.walk=Frames("MaleWalk",6);
            level.femaleWarrior=Actor("Married Warrior","FemaleIdle",4,new Vector2(-44,33));level.femaleWarrior.walk=Frames("FemaleWalk",6);
            level.ember=Actor("Emberfist Warrior","EmberIdle",4,new Vector2(-29,28));level.ember.walk=Frames("EmberWalk",8);
            Visual("Emberfist's forge","Workbench",new Vector2(-32,29),1.5f);Visual("Forge shelter","Shelter",new Vector2(-34,32),1.3f);Label("EMBERFIST'S FORGE",new Vector2(-29,32));
            level.stations=new Transform[3];level.runes=new SpriteRenderer[3];string[] props={"Noticeboard","Noticeboard","Workbench"};
            for(int i=0;i<3;i++){var pos=MissionPositions[i];level.stations[i]=Visual(FallContent.Landmarks[i],props[i],pos,1.5f).transform;Label(FallContent.Landmarks[i].ToUpper(),pos+new Vector2(0,3.6f),12);level.runes[i]=Visual("Prompt rune "+(i+1),"Ward",pos+new Vector2(2,0));Visual("Instruction engine","Device",pos+new Vector2(-3,1));}
            for(int i=0;i<5;i++)Visual("Harvest supplies","Crate",new Vector2(-35+i*2,4));
            Visual("Archive shelter","Shelter",new Vector2(24,31),1.6f);Visual("Archive scrolls","Letter",new Vector2(30,28));
            for(int i=0;i<3;i++)Visual("Kiln fuel","Crate",new Vector2(33+i*2,-23));
            Sign("ORCHARD  SOUTH",new Vector2(-24,22));Sign("ARCHIVE  EAST / NORTH BRIDGE",new Vector2(-12,22));Sign("KILN  EAST THEN SOUTH",new Vector2(29,16));Sign("KILN  WEST AT NEXT TURN",new Vector2(36,4));Sign("KILN  SOUTH THEN EAST",new Vector2(16,-6));Sign("LOWER BRIDGE  WEST",new Vector2(7,-11));Sign("SENTINEL  SOUTH",new Vector2(5,-28));
            Visual("Quiet overlook","Shelter",new Vector2(-38,-26));Label("OLD WATCH / OPTIONAL DETOUR",new Vector2(-38,-23),14);
            level.guardian=Actor("Bramble Sentinel","SentinelIdle",4,GuardianPosition,"SentinelIdle",4,2.1f);Label("BRAMBLE SENTINEL",GuardianPosition+new Vector2(0,4),13);
            var seal=Visual("Three-rune arena barrier","Barrier",new Vector2(0,-34));seal.transform.localScale=new Vector3(8,2,1);seal.gameObject.AddComponent<BoxCollider2D>();level.arenaSeal=seal.gameObject;
            level.shields=new SpriteRenderer[5];for(int i=0;i<5;i++)level.shields[i]=Visual("Sentinel seal "+(i+1),"Shield",new Vector2(-4+i*2,-42));
            level.winterGate=Visual("Winter portal","Portal",GatePosition,1.5f).transform;level.winterGate.GetComponent<SpriteRenderer>().color=new Color(.65f,.85f,1);Label("WINTER / VERIFICATION",new Vector2(0,-44),14);
            for(int x=-48;x<=48;x+=2)if(Mathf.Abs(x)>3){Visual("Winter approach wall","Rock",new Vector2(x,-44.5f));Block("Winter approach wall",new Vector2(x,-44.5f),new Vector2(2,1));}
            var exit=Visual("Winter seal","Barrier",new Vector2(0,-44.5f));exit.transform.localScale=new Vector3(5,1,1);exit.gameObject.AddComponent<BoxCollider2D>();level.exitSeal=exit.gameObject;
            AmbrosJourney.Presentation.NaturalTrail.Apply(paths,3);
            var camera=ObjectAt("Fall Camera",FallLevel.Spawn);camera.tag="MainCamera";camera.transform.position=new Vector3(FallLevel.Spawn.x,FallLevel.Spawn.y,-10);
            var cam=camera.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=7;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.29f,.19f,.12f);camera.AddComponent<AudioListener>();
            var follow=camera.AddComponent<FallCamera>();follow.target=level.player.transform;follow.worldMin=Min;follow.worldMax=Max;
        }
        void Sign(string text,Vector2 pos){Visual("Trail sign","Noticeboard",pos,.65f);Label(text,pos+Vector2.up*1.7f,14);}
        void Hedge(int y,int from,int to){for(int x=from;x<=to;x+=2){Visual("Copper hedge","Bush",new Vector2(x,y),1.1f);Block("Hedge barrier",new Vector2(x,y+.3f),new Vector2(2.1f,1.2f));}}
        bool SceneryAllowed(Vector2 p)
        {
            if(p.x>-10 && p.x<2)return false;
            if(Mathf.Abs(p.y-12)<4 || Mathf.Abs(p.y+2)<4)return false;
            foreach(var c in MissionPositions)if(Vector2.Distance(p,c)<8)return false;
            if(Vector2.Distance(p,FallLevel.Spawn)<9 || Vector2.Distance(p,new Vector2(-29,28))<8)return false;
            for(int i=1;i<trail.Count;i++)if(SegmentDistance(p,trail[i-1],trail[i])<4.5f)return false;
            if(SegmentDistance(p,new Vector2(-29,8),new Vector2(-29,-14))<4.5f || SegmentDistance(p,new Vector2(-29,-14),new Vector2(12,-14))<4.5f || SegmentDistance(p,new Vector2(12,-14),new Vector2(12,-8))<4.5f || SegmentDistance(p,new Vector2(-29,-14),new Vector2(-38,-26))<4.5f)return false;
            return true;
        }
        static float SegmentDistance(Vector2 p,Vector2 a,Vector2 b){var v=b-a;return Vector2.Distance(p,a+v*Mathf.Clamp01(Vector2.Dot(p-a,v)/v.sqrMagnitude));}
        static void Trail(Tilemap map,Tile tile,Vector2 a,Vector2 b,int width)
        {int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(a,b)*2));for(int i=0;i<=steps;i++){var p=Vector2.Lerp(a,b,(float)i/steps);for(int y=-width;y<=width;y++)for(int x=-width;x<=width;x++)map.SetTile(new Vector3Int(Mathf.RoundToInt(p.x)+x,Mathf.RoundToInt(p.y)+y,0),tile);}}
        // Breadth-first route on the same physics obstacles that stop the player.
        public static List<Vector2> FindPath(Vector2 from,Vector2 to)
        {
            var start=Vector2Int.RoundToInt(from);var goal=Vector2Int.RoundToInt(to);var queue=new Queue<Vector2Int>();var previous=new Dictionary<Vector2Int,Vector2Int>();queue.Enqueue(start);previous[start]=start;
            var dirs=new[]{Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left};
            while(queue.Count>0)
            {
                var c=queue.Dequeue();if(c==goal){var path=new List<Vector2>();while(c!=start){path.Add(c);c=previous[c];}path.Reverse();return path;}
                foreach(var d in dirs){var n=c+d;if(n.x<Min.x+1 || n.x>Max.x-1 || n.y<Min.y+1 || n.y>Max.y-1 || previous.ContainsKey(n))continue;
                    bool blocked=false;foreach(var collider in Physics2D.OverlapCircleAll((Vector2)n+new Vector2(0,.22f),.4f))if(!collider.isTrigger && !collider.attachedRigidbody){blocked=true;break;}
                    if(blocked)continue;previous[n]=c;queue.Enqueue(n);
                }
            }
            return new List<Vector2>();
        }
        List<Vector2> route;Vector2 routeGoal,lastPosition;float nextRoute;
        public Vector2 NextWaypoint(Vector2 from,Vector2 to)
        {
            if(route==null || to!=routeGoal || Time.unscaledTime>nextRoute && Vector2.Distance(from,lastPosition)>1.5f)
            {route=FindPath(from,to);routeGoal=to;lastPosition=from;nextRoute=Time.unscaledTime+1;}
            while(route.Count>0 && Vector2.Distance(from,route[0])<1.3f)route.RemoveAt(0);
            return route.Count>0?route[Mathf.Min(2,route.Count-1)]:to;
        }
        void OnDestroy()
        {
            foreach(var sprite in sprites.Values)if(sprite)Destroy(sprite);foreach(var material in spriteMaterials.Values)if(material)Destroy(material);if(pixelMaterial)Destroy(pixelMaterial);
        }
    }
}
