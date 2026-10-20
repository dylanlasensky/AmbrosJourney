using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class SpringSceneBuilder
{
    const string Root="Assets/SpringPrototype";
    const string Hero="Assets/Animations/Hero pack/64 X 64 Chibi assassin/Sprites and files/Individual sprite sheet/";
    const string Mage="Assets/Animations/Hero pack/64 X 64 Ancient mage 2D assets/Sprites and files/Full Sprite sheet/IDLE sprite sheet.png";
    static Material material;
    [MenuItem("Capstone/Build Spring Prototype")]
    public static void Build()
    {
        Directory.CreateDirectory(Root+"/Art"); Directory.CreateDirectory(Root+"/Tiles"); Directory.CreateDirectory(Root+"/Prefabs");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        material=new Material(Shader.Find("Sprites/Default"));
        AssetDatabase.CreateAsset(material,AssetDatabase.GenerateUniqueAssetPath(Root+"/Art/PixelUnlit.mat"));
        var sheet=Readable("Assets/MapSpring/seasonal sample (spring).png");
        Sprite grass=Crop(sheet,"Grass",0,24,8,8,8,new Vector2(.5f,.5f));
        Sprite dirt=Crop(sheet,"Path",64,0,16,16,16,new Vector2(.5f,.5f));
        Sprite flowers=Crop(sheet,"Flowers",0,96,16,16,16,new Vector2(.5f,.5f));
        Sprite tree=Crop(sheet,"SpringTree",176,0,80,112,16,new Vector2(.5f,.08f));
        Sprite rock=Crop(sheet,"Rock",48,224,32,32,16,new Vector2(.5f,.12f));
        Sprite bush=Crop(sheet,"FlowerBush",32,128,32,32,16,new Vector2(.5f,.15f));
        Sprite water=Crop(sheet,"Water",192,212,16,16,16,new Vector2(.5f,.5f));
        var grid=new GameObject("Spring World",typeof(Grid));
        var floor=Map("Grass and Paths",grid.transform,-100);
        var pond=Map("Pond",grid.transform,-90);
        Tile g=TileAsset("Grass",grass), d=TileAsset("Path",dirt), f=TileAsset("Flowers",flowers), wat=TileAsset("Water",water);
        var random=new System.Random(17);
        for(int y=-12;y<12;y++) for(int x=-16;x<16;x++)
        {
            bool path=(Mathf.Abs(x)<=1 && y>=-10 && y<=5) || (y>=2 && y<=4 && x>=-10 && x<=10);
            floor.SetTile(new Vector3Int(x,y,0),path ? d : random.Next(12)==0 ? f : g);
            if(x>=7 && x<=12 && y>=-8 && y<=-5) pond.SetTile(new Vector3Int(x,y,0),wat);
        }
        Collider("Pond boundary",new Vector2(10,-6),new Vector2(6,4));
        var decor=new GameObject("Trees, rocks and flowers");
        Vector2[] trees={new(-13,-9),new(-8,-8),new(-13,-3),new(-9,2),new(-13,7),new(-7,8),new(-2,9),new(4,8),new(10,8),new(14,3),new(14,-3),new(14,-10),new(3,-10)};
        foreach(var p in trees) { var t=Visual("Cherry tree",tree,p,100-Mathf.RoundToInt(p.y*10));t.transform.SetParent(decor.transform); var c=t.AddComponent<BoxCollider2D>();c.size=new Vector2(.8f,.5f);c.offset=new Vector2(0,.1f); }
        Vector2[] rocks={new(-5,-2),new(5,1),new(-11,-5),new(5,-7),new(10,1)};
        foreach(var p in rocks) {var r=Visual("Mossy rock",rock,p,100-Mathf.RoundToInt(p.y*10));r.transform.SetParent(decor.transform);var c=r.AddComponent<BoxCollider2D>();c.size=new Vector2(1.1f,.65f);c.offset=new Vector2(0,.2f);}
        foreach(var p in new Vector2[]{new(-4,5),new(5,5),new(-5,-6),new(4,-3),new(11,-10),new(-10,5)}) Visual("Spring flowers",bush,p,100-Mathf.RoundToInt(p.y*10)).transform.SetParent(decor.transform);
        Collider("West boundary",new Vector2(-16.5f,0),new Vector2(1,25));
        Collider("East boundary",new Vector2(16.5f,0),new Vector2(1,25));
        Collider("South boundary",new Vector2(0,-12.5f),new Vector2(33,1));
        Collider("North boundary",new Vector2(0,12.5f),new Vector2(33,1));
        var idle=HeroFrames("IDLE",4);var walk=HeroFrames("Walk",8);
        var player=Visual("Ambros",idle[0],new Vector2(0,-4),140);
        var body=player.AddComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
        var feet=player.AddComponent<CapsuleCollider2D>();feet.size=new Vector2(.55f,.38f);feet.direction=CapsuleDirection2D.Horizontal;feet.offset=new Vector2(0,.22f);
        var motor=player.AddComponent<SpringPlayer>();motor.idleFrames=idle;motor.walkFrames=walk;
        var mageSheet=Readable(Mage);var mageFrames=new List<Sprite>();
        for(int i=0;i<mageSheet.width/64;i++) mageFrames.Add(Crop(mageSheet,"MageIdle"+i,i*64,0,64,64,32,new Vector2(.5f,0)));
        var mage=Visual("Ancient Mage",mageFrames[0],new Vector2(0,2),80);
        mage.AddComponent<SpringSpriteAnimation>().frames=mageFrames.ToArray();
        var mageFeet=mage.AddComponent<CapsuleCollider2D>();mageFeet.size=new Vector2(.6f,.4f);mageFeet.direction=CapsuleDirection2D.Horizontal;mageFeet.offset=new Vector2(0,.2f);
        var guide=new GameObject("Tutorial and First AI Lesson").AddComponent<SpringGuide>();guide.player=player.transform;guide.mage=mage.transform;motor.guide=guide;
        var camObj=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener),typeof(SpringCamera));camObj.tag="MainCamera";
        var cam=camObj.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=7;cam.backgroundColor=new Color(.10f,.18f,.14f);cam.clearFlags=CameraClearFlags.SolidColor;
        camObj.transform.position=new Vector3(0,-4,-10);camObj.GetComponent<SpringCamera>().target=player.transform;
        PrefabUtility.SaveAsPrefabAsset(player,Root+"/Prefabs/Ambros.prefab");
        PrefabUtility.SaveAsPrefabAsset(mage,Root+"/Prefabs/AncientMage.prefab");
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Spring.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Spring.unity",true)};
        AssetDatabase.SaveAssets();
        Debug.Log("SPRING_PROTOTYPE_READY: Spring scene, Ambros, Mage, tilemaps, collisions and tutorial created.");
    }
    static Tilemap Map(string name,Transform parent,int order)
    {
        var obj=new GameObject(name,typeof(Tilemap),typeof(TilemapRenderer));obj.transform.SetParent(parent);obj.GetComponent<TilemapRenderer>().sortingOrder=order;obj.GetComponent<TilemapRenderer>().sharedMaterial=material;return obj.GetComponent<Tilemap>();
    }
    static Tile TileAsset(string name,Sprite sprite)
    {
        var t=ScriptableObject.CreateInstance<Tile>();t.sprite=sprite;t.colliderType=Tile.ColliderType.None;
        string path=Root+"/Tiles/"+name+".asset";
        var existing=AssetDatabase.LoadAssetAtPath<Tile>(path);
        if(existing!=null){EditorUtility.CopySerialized(t,existing);Object.DestroyImmediate(t);return existing;}
        AssetDatabase.CreateAsset(t,path);return t;
    }
    static Texture2D Readable(string path)
    {
        var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.isReadable=true;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.filterMode=FilterMode.Point;imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static Sprite Crop(Texture2D source,string name,int x,int top,int width,int height,int ppu,Vector2 pivot)
    {
        var image=new Texture2D(width,height,TextureFormat.RGBA32,false);image.SetPixels(source.GetPixels(x,source.height-top-height,width,height));image.Apply();
        string path=Root+"/Art/"+name+".png";File.WriteAllBytes(path,image.EncodeToPNG());Object.DestroyImmediate(image);AssetDatabase.ImportAsset(path);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=ppu;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=9;settings.spritePivot=pivot;importer.SetTextureSettings(settings);importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    static Sprite[] HeroFrames(string name,int count)
    {
        var list=new List<Sprite>();for(int i=1;i<=count;i++){var tex=Readable(Hero+name+i+".png");list.Add(Crop(tex,"Ambros"+name+i,0,0,tex.width,tex.height,32,new Vector2(.5f,0)));}return list.ToArray();
    }
    static GameObject Visual(string name,Sprite sprite,Vector2 position,int order)
    {
        var obj=new GameObject(name,typeof(SpriteRenderer));obj.transform.position=position;var r=obj.GetComponent<SpriteRenderer>();r.sprite=sprite;r.sortingOrder=order;r.sharedMaterial=material;return obj;
    }
    static void Collider(string name,Vector2 p,Vector2 size){var o=new GameObject(name,typeof(BoxCollider2D));o.transform.position=p;o.GetComponent<BoxCollider2D>().size=size;}
}
