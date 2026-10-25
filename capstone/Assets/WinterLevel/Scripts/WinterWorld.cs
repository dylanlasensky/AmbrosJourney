using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using AmbrosJourney.Presentation;
using UnityEngine.Tilemaps;

namespace AmbrosJourney.Winter
{
    [Serializable]
    public sealed class WinterArtwork
    {
        public string key;
        public Texture2D texture;
        public float pixelsPerUnit = 32;
        public Vector2 pivot = new Vector2(.5f, 0);
    }
    // Both Winter.unity and the portal's Resources prefab use this same world definition.
    public sealed class WinterWorld : MonoBehaviour
    {
        public WinterArtwork[] artwork;
        readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        readonly Dictionary<Texture2D, Material> spriteMaterials = new Dictionary<Texture2D, Material>();
        Material pixelMaterial;
        TMP_FontAsset runtimeFont;
        WinterLevel level;
        Transform region;
        public GameObject Outdoors,Cavern;
        WinterCamera followCamera;
        void Start() { BuildWorld(); }
        Sprite Art(string key)
        {
            if (sprites.TryGetValue(key, out var sprite)) return sprite;
            foreach (var a in artwork) if (a.key == key && a.texture)
            {
                sprite = Sprite.Create(a.texture, new Rect(0, 0, a.texture.width, a.texture.height), a.pivot, a.pixelsPerUnit);
                sprite.name = key; sprites.Add(key, sprite); return sprite;
            }
            throw new InvalidOperationException("Missing Winter artwork: " + key);
        }
        Sprite[] Frames(string prefix, int count)
        { var result = new Sprite[count]; for (int i = 0; i < count; i++) result[i] = Art(prefix + (i + 1)); return result; }
        GameObject ObjectAt(string label, Vector2 p)
        { var o = new GameObject(label); o.transform.SetParent(region?region:transform); o.transform.position = p; return o; }
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
        WinterActor Actor(string label, string prefix, int count, Vector2 p, string attackPrefix = null, int attackCount = 0, float scale = 1)
        {
            var r = Visual(label, prefix + "1", p, scale); var actor = r.gameObject.AddComponent<WinterActor>();
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
        public static readonly Vector2 Min=new Vector2(-40,-34),Max=new Vector2(40,36);
        public static readonly Vector2 CaveMin=new Vector2(90,-34),CaveMax=new Vector2(150,36);
        public static readonly Vector2 CaveSpawn=new Vector2(100,-26),Entrance=new Vector2(24,27),BossPosition=new Vector2(134,25);
        public static readonly Vector2[] MissionPositions={new Vector2(-26,-6),new Vector2(23,7),new Vector2(103,10)};
        readonly Vector2[] trail={new Vector2(-32,-25),new Vector2(-26,-20),new Vector2(-26,-6),new Vector2(-12,-6),new Vector2(-12,-17),new Vector2(10,-17),new Vector2(10,7),new Vector2(23,7),new Vector2(23,18),new Vector2(8,18),new Vector2(8,23),new Vector2(24,23),Entrance};
        public void BuildWorld()
        {
            if(GetComponent<WinterLevel>())return;
            pixelMaterial=new Material(Shader.Find("Sprites/Default"));Resources.Load<Material>("SummerText");runtimeFont=Resources.Load<TMP_FontAsset>("SummerDefault");
            level=gameObject.AddComponent<WinterLevel>();level.font=runtimeFont;level.World=this;
            Outdoors=new GameObject("Frostveil / outdoor map");Outdoors.transform.SetParent(transform);region=Outdoors.transform;
            var floor=Map("Snow meadow",-1000);var path=Map("Lantern trail",-990);var snow=Tile("SnowGround");var snowDetail=snow;var dirt=Tile("Path");
            var rng=new System.Random(40126);for(int y=-34;y<=36;y++)for(int x=-40;x<=40;x++)floor.SetTile(new Vector3Int(x,y,0),rng.Next(30)==0?snowDetail:snow);
            for(int i=1;i<trail.Length;i++)Trail(path,dirt,trail[i-1],trail[i],1);
            Block("West edge",new Vector2(-41,1),new Vector2(1,72));Block("East edge",new Vector2(41,1),new Vector2(1,72));Block("North edge",new Vector2(0,37),new Vector2(83,1));Block("South edge",new Vector2(0,-35),new Vector2(83,1));
            var lake=Map("Frozen lake / unsafe ice",-980);var ice=Tile("Water");for(int y=-9;y<=8;y++)for(int x=-7;x<=3;x++)if(Mathf.Pow(x+2,2)/36f+Mathf.Pow(y,2)/81f<1){lake.SetTile(new Vector3Int(x,y,0),ice);Block("Thin ice",new Vector2(x+.5f,y+.5f),Vector2.one);}
            for(int i=0;i<330;i++){var p=new Vector2(rng.Next(-38,39),rng.Next(-32,35));if(!SceneryAllowed(p))continue;var v=Visual("Snow fir","Tree",p,.8f+(float)rng.NextDouble()*.25f);var c=v.gameObject.AddComponent<BoxCollider2D>();c.size=new Vector2(.6f,.4f);c.offset=new Vector2(0,.2f);}
            for(int i=0;i<75;i++){var p=new Vector2(rng.Next(-38,39),rng.Next(-32,35));if(SceneryAllowed(p))Visual("Snowbank",i%3==0?"Rock":"Bush",p,.7f);}
            Visual("Autumn arrival","Portal",WinterLevel.Spawn+Vector2.up*3,1.3f);Label("FROSTVEIL / ARRIVAL",WinterLevel.Spawn+Vector2.up*6);
            for(int i=1;i<trail.Length;i+=2){Visual("Warm trail lantern","Lantern",trail[i]+new Vector2(2.4f,1),.75f);}
            level.caveEntrance=ObjectAt("Enter Echo Cavern",Entrance).transform;
            Visual("Frostveil mountain ridge / integrated cave","MountainRidge",Entrance+Vector2.down*1.5f).sortingOrder=-500;Label("ECHO CAVERN / E TO ENTER",Entrance+Vector2.up*3.5f);
            Block("Mountain west shoulder",new Vector2(15,30),new Vector2(14,7));Block("Mountain east shoulder",new Vector2(34,30),new Vector2(14,7));Block("Mountain upper wall",new Vector2(24,33),new Vector2(32,1));
            Label("FOLLOW THE LANTERNS NORTH",new Vector2(23,13));Label("THIN ICE / STAY ON THE TRAIL",new Vector2(-3,-11));
            level.stations=new Transform[3];level.runes=new SpriteRenderer[3];
            for(int i=0;i<2;i++){level.stations[i]=Visual(WinterContent.Landmarks[i],"Noticeboard",MissionPositions[i],1.5f).transform;level.runes[i]=Visual("Evidence seal","Ward",MissionPositions[i]+Vector2.right*2);Label(WinterContent.Landmarks[i].ToUpper(),MissionPositions[i]+Vector2.up*3.5f);Visual("Research shelter","Shelter",MissionPositions[i]+new Vector2(-4,3),1.1f);}
            Cavern=new GameObject("Echo Cavern / interior map");Cavern.transform.SetParent(transform);region=Cavern.transform;
            BuildCave();
            level.caveExit=Visual("Daylight exit","Portal",CaveSpawn+Vector2.down,1.2f).transform;Label("DAYLIGHT / E TO LEAVE",CaveSpawn+Vector2.up*3);
            level.stations[2]=Visual("Echo Archive","Noticeboard",MissionPositions[2],1.5f).transform;level.runes[2]=Visual("Archive evidence seal","Ward",MissionPositions[2]+Vector2.right*2);Label("ECHO ARCHIVE",MissionPositions[2]+Vector2.up*3);
            for(int i=0;i<5;i++)Visual("Archive crystal","Crystal",new Vector2(98+i*2,15),.4f);
            level.guardian=Actor("Frostbound Warden","SkeletonIdle",4,BossPosition,"SkeletonAttack",6,2.2f);Label("FROSTBOUND WARDEN",BossPosition+Vector2.up*5);
            var seal=Visual("Three-seal frozen barrier","Barrier",new Vector2(134,16));seal.transform.localScale=new Vector3(9,1.3f,1);seal.gameObject.AddComponent<BoxCollider2D>();level.arenaSeal=seal.gameObject;
            level.shields=new SpriteRenderer[5];for(int i=0;i<5;i++)level.shields[i]=Visual("Warden ice seal","Shield",BossPosition+new Vector2(-4+i*2,-2));
            level.winterGate=Visual("Winter crest","Ward",new Vector2(140,30),2).transform;Label("WINTER CREST",new Vector2(140,33));
            level.exitSeal=ObjectAt("Crest ward",new Vector2(140,29)); // Interaction is gated by progress; no invisible collision.
            region=null;
            level.player=Actor("Ambros","AmbrosIdle",4,WinterLevel.Spawn,"AmbrosAttack",5);level.player.isPlayer=true;level.player.walk=Frames("AmbrosWalk",8);
            var body=level.player.gameObject.AddComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            var collider=level.player.gameObject.AddComponent<CapsuleCollider2D>();collider.size=new Vector2(.55f,.38f);collider.direction=CapsuleDirection2D.Horizontal;collider.offset=new Vector2(0,.22f);
            level.mage=Actor("Ancient Mage","MageIdle",4,WinterLevel.Spawn+Vector2.left*2);level.mage.walk=Frames("MageWalk",8);
            level.maleWarrior=Actor("Warrior","MaleIdle",4,WinterLevel.Spawn+Vector2.left*3);level.maleWarrior.walk=Frames("MaleWalk",6);
            level.femaleWarrior=Actor("Warrior","FemaleIdle",4,WinterLevel.Spawn+Vector2.left*4);level.femaleWarrior.walk=Frames("FemaleWalk",6);
            level.ember=Actor("Emberfist","EmberIdle",4,WinterLevel.Spawn+Vector2.left*5);level.ember.walk=Frames("EmberWalk",8);
            level.littleMage=Actor("Little Mage","LittleIdle",4,WinterLevel.Spawn+new Vector2(3,2));level.littleMage.walk=Frames("LittleWalk",4);
            AmbrosJourney.Presentation.NaturalTrail.Apply(path,4);
            var camera=ObjectAt("Winter Camera",WinterLevel.Spawn);camera.tag="MainCamera";camera.transform.position=new Vector3(WinterLevel.Spawn.x,WinterLevel.Spawn.y,-10);
            var cam=camera.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=7;cam.backgroundColor=new Color(.07f,.10f,.17f);cam.clearFlags=CameraClearFlags.SolidColor;camera.AddComponent<AudioListener>();followCamera=camera.AddComponent<WinterCamera>();followCamera.target=level.player.transform;followCamera.worldMin=Min;followCamera.worldMax=Max;
            gameObject.AddComponent<WinterSnow>().level=level;Cavern.SetActive(false);
        }
        bool SceneryAllowed(Vector2 p)
        {
            if(p.y>23&&p.x>6)return false;
            if(Vector2.Distance(p,Entrance)<9||Vector2.Distance(p,WinterLevel.Spawn)<9)return false;
            foreach(var m in MissionPositions)if(Vector2.Distance(p,m)<7)return false;
            for(int i=1;i<trail.Length;i++)if(SegmentDistance(p,trail[i-1],trail[i])<4)return false;
            return !(p.x>-10&&p.x<6&&p.y>-12&&p.y<12);
        }
        void BuildCave()
        {
            // Four chambers joined by bent passages; the west archive is a meaningful detour.
            var rooms=new[]{new RectInt(95,-31,16,13),new RectInt(117,-18,23,17),new RectInt(96,3,17,15),new RectInt(123,20,23,14)};
            var passages=new[]{new RectInt(106,-26,24,5),new RectInt(124,-24,5,18),new RectInt(102,-7,24,5),new RectInt(102,-5,5,13),new RectInt(131,-3,7,26)};
            var floor=Map("Cavern stone",-1000);var walls=Map("Faceted ice walls",-900);var stone=Tile("CaveFloor");stone.color=new Color(.68f,.80f,1);var ice=Tile("CaveWall");ice.color=new Color(.72f,.86f,1);
            var cells=new HashSet<Vector2Int>();foreach(var r in rooms)foreach(var p in r.allPositionsWithin)cells.Add(p);foreach(var r in passages)foreach(var p in r.allPositionsWithin)cells.Add(p);
            foreach(var p in cells)floor.SetTile(new Vector3Int(p.x,p.y,0),stone);
            // Sparse mineral debris keeps the central walking routes clear.
            foreach(var p in new[]{new Vector2(97,-29),new Vector2(108,-29),new Vector2(119,-16),new Vector2(137,-15),new Vector2(98,16),new Vector2(110,16),new Vector2(125,31),new Vector2(143,23)}){var v=Visual("Frosted cave boulder","Rock",p,.65f);v.color=new Color(.65f,.8f,1);}
            var boundary=new HashSet<Vector2Int>();foreach(var p in cells)foreach(var d in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right})if(!cells.Contains(p+d))boundary.Add(p+d);
            foreach(var p in boundary){walls.SetTile(new Vector3Int(p.x,p.y,0),ice);Block("Cavern wall",(Vector2)p+Vector2.one*.5f,Vector2.one);}
            foreach(var p in new[]{new Vector2(97,-29),new Vector2(108,-20),new Vector2(119,-15),new Vector2(138,-4),new Vector2(98,5),new Vector2(111,15),new Vector2(125,23),new Vector2(144,31)})Visual("Cyan crystal formation","Crystal",p,.65f);
            foreach(var p in new[]{new Vector2(108,-24),new Vector2(126,-19),new Vector2(119,-5),new Vector2(104,1),new Vector2(135,8),new Vector2(135,21)})Visual("Cave lantern","Lantern",p,.7f);
            Label("ECHO ARCHIVE / WEST PASSAGE",new Vector2(122,-9));Label("WARDEN / NORTHERN HALL",new Vector2(132,2));
        }
        public void ChangeRegion(bool cave)
        {
            if(level.InputBlocked||level.Progress.InCave==cave)return;
            var doorway=cave?level.caveEntrance:level.caveExit;if(Vector2.Distance(level.player.transform.position,doorway.position)>3)return;
            if(cave&&!level.Progress.EnterCave())return;if(!cave)level.Progress.LeaveCave();
            Outdoors.SetActive(!cave);Cavern.SetActive(cave);
            var spawn=cave?CaveSpawn:Entrance+Vector2.down*2;
            level.player.transform.position=spawn;var body=level.player.GetComponent<Rigidbody2D>();body.position=spawn;body.linearVelocity=Vector2.zero;
            var actors=new[]{level.mage,level.maleWarrior,level.femaleWarrior,level.ember,level.littleMage};for(int i=0;i<actors.Length;i++)actors[i].transform.position=spawn+Vector2.left*(.5f+i*.25f);
            followCamera.worldMin=cave?CaveMin:Min;followCamera.worldMax=cave?CaveMax:Max;followCamera.transform.position=new Vector3(spawn.x,spawn.y,-10);level.RegionChanged();Physics2D.SyncTransforms();
            Debug.Log(cave?"WINTER_CAVE_ENTERED":"WINTER_CAVE_EXITED");
        }
        static float SegmentDistance(Vector2 p,Vector2 a,Vector2 b){var v=b-a;return Vector2.Distance(p,a+v*Mathf.Clamp01(Vector2.Dot(p-a,v)/v.sqrMagnitude));}
        static void Trail(Tilemap map,Tile tile,Vector2 a,Vector2 b,int width){int n=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(a,b)*2));for(int i=0;i<=n;i++){var p=Vector2.Lerp(a,b,(float)i/n);for(int y=-width;y<=width;y++)for(int x=-width;x<=width;x++)map.SetTile(new Vector3Int(Mathf.RoundToInt(p.x)+x,Mathf.RoundToInt(p.y)+y,0),tile);}}
        void OnDestroy(){foreach(var s in sprites.Values)if(s)Destroy(s);foreach(var m in spriteMaterials.Values)if(m)Destroy(m);if(pixelMaterial)Destroy(pixelMaterial);}
    }
}
