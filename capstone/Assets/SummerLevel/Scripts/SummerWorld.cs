using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using AmbrosJourney.Presentation;
using UnityEngine.Tilemaps;

namespace AmbrosJourney.Summer
{
    [Serializable]
    public sealed class SummerArtwork
    {
        public string key;
        public Texture2D texture;
        public float pixelsPerUnit = 32;
        public Vector2 pivot = new Vector2(.5f, 0);
    }
    // Both Summer.unity and the portal's Resources prefab use this same world definition.
    public sealed class SummerWorld : MonoBehaviour
    {
        public SummerArtwork[] artwork;
        readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        readonly Dictionary<Texture2D, Material> spriteMaterials = new Dictionary<Texture2D, Material>();
        Material pixelMaterial;
        TMP_FontAsset runtimeFont;
        SummerLevel level;
        void Start() { BuildWorld(); }
        Sprite Art(string key)
        {
            if (sprites.TryGetValue(key, out var sprite)) return sprite;
            foreach (var a in artwork) if (a.key == key && a.texture)
            {
                sprite = Sprite.Create(a.texture, new Rect(0, 0, a.texture.width, a.texture.height), a.pivot, a.pixelsPerUnit);
                sprite.name = key; sprites.Add(key, sprite); return sprite;
            }
            throw new InvalidOperationException("Missing Summer artwork: " + key);
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
        SummerActor Actor(string label, string prefix, int count, Vector2 p, string attackPrefix = null, int attackCount = 0, float scale = 1)
        {
            var r = Visual(label, prefix + "1", p, scale); var actor = r.gameObject.AddComponent<SummerActor>();
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
        public void BuildWorld()
        {
            if (GetComponent<SummerLevel>()) return;
            pixelMaterial = new Material(Shader.Find("Sprites/Default"));
            // A Resources material retains TMP's shader in player builds, without editing GraphicsSettings.
            Resources.Load<Material>("SummerText");
            runtimeFont = Resources.Load<TMP_FontAsset>("SummerDefault");
            if (!runtimeFont) throw new InvalidOperationException("SummerDefault font could not load. Keep SummerLevel/Resources intact.");
            level = gameObject.AddComponent<SummerLevel>(); level.font = runtimeFont;
            var ground = Map("Summer meadow", -1000); var paths = Map("Village loop trails", -990);
            var grass = Tile("Grass"); var flowers = Tile("Flowers"); var dirt = Tile("Path"); var water = Tile("Water");
            var rng = new System.Random(2210);
            for (int y = -32; y <= 28; y++) for (int x = -30; x <= 30; x++)
                ground.SetTile(new Vector3Int(x, y, 0), rng.Next(22) == 0 ? flowers : grass);
            Vector2[] route = { new Vector2(0,22), new Vector2(-16,14), new Vector2(-18,5), new Vector2(-13,-5), new Vector2(0,-8), new Vector2(16,-4), new Vector2(19,10), new Vector2(13,18), new Vector2(0,22) };
            for (int i = 0; i < route.Length - 1; i++) Trail(paths, dirt, route[i], route[i + 1], 1);
            Trail(paths, dirt, new Vector2(0,-8), new Vector2(0,-30), 2);
            var pond = Map("Summer pond", -980);
            for (int y = 1; y < 10; y++) for (int x = -3; x < 7; x++) if (Mathf.Pow((x-2)/5f,2)+Mathf.Pow((y-5)/4f,2)<1) pond.SetTile(new Vector3Int(x,y,0),water);
            Block("Pond shoreline", new Vector2(2,5), new Vector2(8,6));
            // Keep each path and interaction area clear of randomized scenery.
            for (int i = 0; i < 175; i++)
            {
                Vector2 p = new Vector2(rng.Next(-29,30), rng.Next(-30,28));
                if (!SceneryAllowed(p, route)) continue;
                var tree = Visual("Summer canopy", "Tree", p, .8f + (float)rng.NextDouble() * .22f);
                var feet = tree.gameObject.AddComponent<BoxCollider2D>(); feet.size = new Vector2(.6f,.4f); feet.offset = new Vector2(0,.2f);
            }
            for (int i = 0; i < 65; i++)
            {
                Vector2 p = new Vector2(rng.Next(-28,29), rng.Next(-30,28)); if (!SceneryAllowed(p,route)) continue;
                Visual("Summer undergrowth", i % 3 == 0 ? "Rock" : "Bush", p, .8f);
            }
            Block("West world boundary",new Vector2(-31,0),new Vector2(1,66)); Block("East world boundary",new Vector2(31,0),new Vector2(1,66));
            Block("North world boundary",new Vector2(0,29),new Vector2(63,1)); Block("South world boundary",new Vector2(0,-33),new Vector2(63,1));
            Visual("Spring arrival portal","Portal",new Vector2(0,24),1.3f); Label("SPRING ARRIVAL",new Vector2(0,26));
            // The same extracted Chibi Assassin frames used in Spring, copied without recoloring.
            level.player = Actor("Ambros","AmbrosIdle",4,SummerLevel.Spawn,"AmbrosAttack",5); level.player.isPlayer = true; level.player.walk = Frames("AmbrosWalk",8);
            var body = level.player.gameObject.AddComponent<Rigidbody2D>(); body.gravityScale=0; body.freezeRotation=true; body.interpolation=RigidbodyInterpolation2D.Interpolate; body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            var capsule=level.player.gameObject.AddComponent<CapsuleCollider2D>();capsule.size=new Vector2(.55f,.38f);capsule.direction=CapsuleDirection2D.Horizontal;capsule.offset=new Vector2(0,.22f);
            level.mage=Actor("Ancient Mage","MageIdle",4,new Vector2(-2,22)); level.mage.walk=Frames("MageWalk",8);
            level.maleWarrior=Actor("Warrior - Workshop guide","MaleIdle",4,new Vector2(-14,16)); level.maleWarrior.walk=Frames("MaleWalk",6);
            level.femaleWarrior=Actor("Warrior - Garden guide","FemaleIdle",4,new Vector2(-16,-3)); level.femaleWarrior.walk=Frames("FemaleWalk",6);
            level.orc=Actor("Suncrest Guardian","OrcIdle",5,new Vector2(0,-23),"OrcAttack",6,1.7f);
            SeasonBossVisual.Attach(level.orc.gameObject,"SummerSuncrestAtlas",6f);
            // Readable landmarks with new supporting props; original character art is retained.
            level.workshop=Visual("Village workshop","Workbench",new Vector2(-18,12),1.5f).transform;
            Label("VILLAGE WORKSHOP",new Vector2(-18,16));
            Visual("Festival supplies","Crate",new Vector2(-22,12)); Visual("Repair supplies","Crate",new Vector2(-22,10));
            Visual("Workshop shelter","Shelter",new Vector2(-21,17),1.3f);
            level.garden=Visual("Training garden","Target",new Vector2(-15,-6),1.3f).transform; Label("TRAINING GARDEN",new Vector2(-16,-1));
            for(int i=0;i<4;i++)Visual("Garden bed","GardenBed",new Vector2(-23+i*2,-9));
            level.stones=new Transform[3];
            Vector2[] stonePositions={new Vector2(-21,-4),new Vector2(-18,-8),new Vector2(-23,-8)};
            string[] stoneNames={"SUN","LEAF","SEED"};
            for(int i=0;i<3;i++){level.stones[i]=Visual(stoneNames[i]+" stepping stone","Stone"+i,stonePositions[i],1.2f).transform;Label(stoneNames[i],stonePositions[i]+Vector2.up*1.7f,4);}
            level.messagePost=Visual("Message post","Noticeboard",new Vector2(19,8),1.3f).transform;Label("MESSAGE POST",new Vector2(19,12));
            Visual("Message shelter","Shelter",new Vector2(24,14));Visual("Letters","Letter",new Vector2(17,8));
            // Short ruined bridge is a visual teaching prop, outside the navigable route.
            Visual("Damaged bridge - ask the builder","Bridge",new Vector2(8,5));Block("Broken bridge approach",new Vector2(7,5),new Vector2(2,5));
            level.wardLights=new SpriteRenderer[3]; Vector2[] wp={new Vector2(-16,12),new Vector2(-12,-6),new Vector2(21,8)};
            for(int i=0;i<3;i++)level.wardLights[i]=Visual("Ward piece "+(i+1),"Ward",wp[i]);
            Label("SUNCREST'S CLEARING",new Vector2(0,-16));
            for(int x=-29;x<=29;x+=2) if(Mathf.Abs(x)>3) {Visual("Arena perimeter","Rock",new Vector2(x,-18));Block("Arena wall",new Vector2(x,-18),new Vector2(2,1));}
            var seal=Visual("Village ward barrier","Barrier",new Vector2(0,-18));seal.transform.localScale=new Vector3(8.4f,2,1);seal.gameObject.AddComponent<BoxCollider2D>(); level.arenaSeal=seal.gameObject;
            level.shields=new SpriteRenderer[5];for(int i=0;i<5;i++)level.shields[i]=Visual("Suncrest seal "+(i+1),"Shield",new Vector2(-3+i*1.5f,-24));
            for(int i=0;i<3;i++)Visual("Instruction device","Device",new Vector2(-5+i*5,-26));
            level.fallGate=Visual("Fall portal","Portal",new Vector2(0,-30),1.5f).transform;level.fallGate.GetComponent<SpriteRenderer>().color=new Color(1,.69f,.4f);Label("FALL / PROMPT ENGINEERING",new Vector2(0,-27));
            var exit=Visual("Fall seal","Barrier",new Vector2(0,-28.7f));exit.transform.localScale=new Vector3(5,1,1);exit.gameObject.AddComponent<BoxCollider2D>();level.exitSeal=exit.gameObject;
            AmbrosJourney.Presentation.NaturalTrail.Apply(paths,2);
            var camera=ObjectAt("Summer Camera",new Vector2(0,22));camera.tag="MainCamera";camera.transform.position=new Vector3(0,22,-10);
            var cam=camera.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=7;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.17f,.25f,.15f);camera.AddComponent<AudioListener>();
            var follow=camera.AddComponent<SummerCamera>();follow.target=level.player.transform;
        }
        static void Trail(Tilemap map,Tile tile,Vector2 a,Vector2 b,int width)
        {int steps=Mathf.CeilToInt(Vector2.Distance(a,b)*2);for(int i=0;i<=steps;i++){Vector2 p=Vector2.Lerp(a,b,(float)i/steps);for(int y=-width;y<=width;y++)for(int x=-width;x<=width;x++)map.SetTile(new Vector3Int(Mathf.RoundToInt(p.x)+x,Mathf.RoundToInt(p.y)+y,0),tile);}}
        static bool SceneryAllowed(Vector2 p,Vector2[] route)
        {
            if(p.y< -15 && Mathf.Abs(p.x)<10 || Vector2.Distance(p,new Vector2(2,5))<9) return false;
            Vector2[] clear={new Vector2(0,23),new Vector2(-18,13),new Vector2(-19,-6),new Vector2(20,10)};
            foreach(var c in clear)if(Vector2.Distance(p,c)<7)return false;
            for(int i=0;i<route.Length-1;i++){Vector2 ab=route[i+1]-route[i];float t=Mathf.Clamp01(Vector2.Dot(p-route[i],ab)/ab.sqrMagnitude);if(Vector2.Distance(p,route[i]+ab*t)<4)return false;}
            return true;
        }
        void OnDestroy()
        {
            foreach(var sprite in sprites.Values)if(sprite)Destroy(sprite);
            foreach(var material in spriteMaterials.Values)if(material)Destroy(material);
            if(pixelMaterial)Destroy(pixelMaterial);
            // Font, atlas and material are shared Resources assets and must survive scene unload.
        }
    }
}
