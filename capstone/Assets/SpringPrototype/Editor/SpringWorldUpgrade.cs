using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SpringWorldUpgrade
{
    const string Art="Assets/SpringPrototype/Art/";
    static Material mat;
    static Sprite Load(string n)=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+n+".png");
    static readonly Vector2[] route={new(-18,16),new(-14,12),new(-18,6),new(-18,0),new(-15,-7),new(3,-7),new(10,0),new(10,6),new(19,2),new(21,-7),new(15,-14),new(15,-22),new(0,-24),new(0,-33)};
    public static float TrailDistance(Vector2 p){float d=999;for(int i=1;i<route.Length;i++){var a=route[i-1];var v=route[i]-a;d=Mathf.Min(d,Vector2.Distance(p,a+v*Mathf.Clamp01(Vector2.Dot(p-a,v)/v.sqrMagnitude)));}return d;}
    [MenuItem("Capstone/Upgrade Spring World")]
    public static void Build()
    {
        if(EditorApplication.isPlaying)return;
        EditorSceneManager.OpenScene("Assets/Scenes/Spring.unity");
        foreach(var name in new[]{"Spring World","Trees, rocks and flowers","Pond boundary","West boundary","East boundary","South boundary","North boundary","Spring Valley"}){var o=GameObject.Find(name);if(o)Object.DestroyImmediate(o);}
        mat=AssetDatabase.LoadAssetAtPath<Material>(Art+"PixelUnlit.mat");if(!mat)mat=new Material(Shader.Find("Sprites/Default"));
        var root=new GameObject("Spring Valley");var grid=new GameObject("Meadow floor",typeof(Grid));grid.transform.SetParent(root.transform);
        var go=new GameObject("Hand-laid winding trails",typeof(Tilemap),typeof(TilemapRenderer));go.transform.SetParent(grid.transform);var map=go.GetComponent<Tilemap>();go.GetComponent<TilemapRenderer>().sharedMaterial=mat;go.GetComponent<TilemapRenderer>().sortingOrder=-1000;
        Tile grass=AssetDatabase.LoadAssetAtPath<Tile>("Assets/SpringPrototype/Tiles/Grass.asset"),path=AssetDatabase.LoadAssetAtPath<Tile>("Assets/SpringPrototype/Tiles/Path.asset"),flowers=AssetDatabase.LoadAssetAtPath<Tile>("Assets/SpringPrototype/Tiles/Flowers.asset");
        var rnd=new System.Random(41);
        for(int y=-38;y<26;y++)for(int x=-32;x<32;x++){var p=new Vector2(x+.5f,y+.5f);float d=TrailDistance(p);map.SetTile(new Vector3Int(x,y,0),d<1.25f?path:rnd.Next(23)==0?flowers:grass);if(d<1.25f)map.SetColor(new Vector3Int(x,y,0),new Color(.93f,.92f,.85f));}
        // Irregular continuous shoreline, two depths, animated reflected light.
        Lake(root.transform);
        var tree=Load("SpringTree");var bush=Load("FlowerBush");var rock=Load("Rock");
        var planted=new List<Vector2>();
        for(int i=0;i<360;i++){
            Vector2 p=new Vector2(-31+(float)rnd.NextDouble()*62,-37+(float)rnd.NextDouble()*62);
            if(TrailDistance(p)<4.3f || LakeValue(p)<1.4f || Vector2.Distance(p,new Vector2(-20,13))<6 || Vector2.Distance(p,new Vector2(0,-29))<7)continue;
            bool close=false;foreach(var q in planted)if(Vector2.Distance(p,q)<4)close=true;if(close)continue;planted.Add(p);
            var t=Visual("Blossom canopy",tree,p,root.transform);Solid(t,new Vector2(.75f,.5f),new Vector2(0,.15f));
        }
        for(int i=0;i<150;i++){
            var p=new Vector2(-30+(float)rnd.NextDouble()*60,-36+(float)rnd.NextDouble()*60);if(TrailDistance(p)<2.5f || LakeValue(p)<1.25f || Vector2.Distance(p,new Vector2(0,-29))<5)continue;
            var o=Visual(i%4==0?"Mossy boulder":"Wildflower bed",i%4==0?rock:bush,p,root.transform);if(i%4==0)Solid(o,new Vector2(1,.5f),new Vector2(0,.2f));
        }
        var ci=(TextureImporter)AssetImporter.GetAtPath(Art+"MageCabin.png");if(ci!=null){ci.textureType=TextureImporterType.Sprite;ci.spriteImportMode=SpriteImportMode.Single;ci.maxTextureSize=256;ci.spritePixelsPerUnit=180;ci.filterMode=FilterMode.Point;ci.textureCompression=TextureImporterCompression.Uncompressed;ci.mipmapEnabled=false;var cs=new TextureImporterSettings();ci.ReadTextureSettings(cs);cs.spriteAlignment=9;cs.spritePivot=new Vector2(.5f,0);ci.SetTextureSettings(cs);ci.SaveAndReimport();}
        var cabin=Load("MageCabin");if(cabin){var c=Visual("The Mage's cottage",cabin,new Vector2(-22,12),root.transform);Solid(c,new Vector2(5,2.5f),new Vector2(0,1.5f));}
        // A small dock and resting place along the lake's western bank.
        for(int i=0;i<10;i++)Rect("Dock plank",new Vector2(-10+i*.5f,3),new Vector2(.46f,1.8f),new Color(.54f,.34f,.20f),-850,root.transform);
        foreach(float y in new[]{2f,4f})foreach(float x in new[]{-10f,-6f})Rect("Dock post",new Vector2(x,y),new Vector2(.2f,.35f),new Color(.3f,.2f,.15f),100-Mathf.RoundToInt(y*10),root.transform);
        for(int i=0;i<11;i++){float a=i*Mathf.PI/10;var p=new Vector2(Mathf.Cos(a)*6,-30-Mathf.Sin(a)*5);if(Mathf.Abs(p.x)<2)continue;var o=Visual("Guardian circle stone",rock,p,root.transform);Solid(o,new Vector2(1,.5f),Vector2.zero);}
        foreach(var p in new[]{new Vector2(-14,7),new Vector2(-12,-7),new Vector2(12,2),new Vector2(18,-17),new Vector2(4,-24)}){
            Rect("Trail marker post",p,new Vector2(.15f,1),new Color(.35f,.23f,.15f),100-Mathf.RoundToInt(p.y*10),root.transform);
            Rect("Trail marker",p+Vector2.up*.65f,new Vector2(1.1f,.45f),new Color(.77f,.56f,.31f),110-Mathf.RoundToInt(p.y*10),root.transform);
        }
        Boundary("West",new(-32.5f,-6),new(1,66),root.transform);Boundary("East",new(32.5f,-6),new(1,66),root.transform);Boundary("North",new(0,26.5f),new(66,1),root.transform);Boundary("South",new(0,-38.5f),new(66,1),root.transform);
        var g=Object.FindAnyObjectByType<SpringGuide>();g.player.position=new Vector2(-18,16);g.mage.position=new Vector2(-14,12);g.player.GetComponent<SpringPlayer>().spawnPoint=new Vector2(-18,16);g.player.GetComponent<SpringPlayer>().speed=5;
        string monster="Assets/Animations/Creatures Pack Premium/Sprites/IDLE animation Groupe 4.png";var imp=(TextureImporter)AssetImporter.GetAtPath(monster);imp.filterMode=FilterMode.Point;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.SaveAndReimport();g.guardianSheet=AssetDatabase.LoadAssetAtPath<Texture2D>(monster);
        var camera=Object.FindAnyObjectByType<SpringCamera>();camera.worldMin=new Vector2(-32,-38);camera.worldMax=new Vector2(32,26);camera.GetComponent<Camera>().orthographicSize=7;camera.transform.position=new Vector3(-18,16,-10);
        root.AddComponent<SpringAtmosphere>();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();Debug.Log("SPRING_VALLEY_READY: 64x64 map, lake, cottage, trails and guardian arena.");
    }
    static float LakeValue(Vector2 p){p-=new Vector2(-2,3);return p.x*p.x/49+p.y*p.y/25;}
    static void Lake(Transform parent){
        for(int layer=0;layer<3;layer++){
            var o=new GameObject(layer==0?"Soft shoreline":layer==1?"Turquoise shallows":"Stillwater Lake",typeof(MeshFilter),typeof(MeshRenderer));o.transform.SetParent(parent);o.transform.position=new Vector3(-2,3,0);
            var v=new Vector3[65];var triangles=new int[64*3];float factor=layer==0?1.13f:layer==1?1: .86f;
            for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;float f=factor*(1+.07f*Mathf.Sin(a*3)+.04f*Mathf.Cos(a*7));v[i+1]=new Vector3(Mathf.Cos(a)*7*f,Mathf.Sin(a)*5*f,0);triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=(i+1)%64+1;}
            var mesh=new Mesh();mesh.vertices=v;mesh.triangles=triangles;mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,AssetDatabase.GenerateUniqueAssetPath(Art+"LakeMesh.asset"));o.GetComponent<MeshFilter>().sharedMesh=mesh;var material=new Material(Shader.Find("Sprites/Default"));material.color=layer==0?new Color(.69f,.72f,.39f):layer==1?new Color(.31f,.72f,.64f):new Color(.16f,.49f,.56f);AssetDatabase.CreateAsset(material,AssetDatabase.GenerateUniqueAssetPath(Art+"LakeMaterial.mat"));o.GetComponent<MeshRenderer>().sharedMaterial=material;o.GetComponent<MeshRenderer>().sortingOrder=-950+layer;
            if(layer==1){var c=o.AddComponent<PolygonCollider2D>();var points=new Vector2[64];for(int i=0;i<64;i++)points[i]=v[i+1];c.points=points;}
        }
    }
    static GameObject Visual(string n,Sprite s,Vector2 p,Transform parent){var o=new GameObject(n,typeof(SpriteRenderer));o.transform.SetParent(parent);o.transform.position=p;var r=o.GetComponent<SpriteRenderer>();r.sprite=s;r.sharedMaterial=mat;r.sortingOrder=100-Mathf.RoundToInt(p.y*10);return o;}
    static void Solid(GameObject o,Vector2 size,Vector2 offset){var c=o.AddComponent<BoxCollider2D>();c.size=size;c.offset=offset;}
    static void Boundary(string n,Vector2 p,Vector2 s,Transform root){var o=new GameObject(n+" world edge");o.transform.SetParent(root);o.transform.position=p;Solid(o,s,Vector2.zero);}
    static void Rect(string n,Vector2 p,Vector2 size,Color c,int order,Transform root){var o=new GameObject(n,typeof(SpriteRenderer));o.transform.SetParent(root);o.transform.position=p;o.transform.localScale=size;var r=o.GetComponent<SpriteRenderer>();r.sprite=Load("Grass");r.color=c;r.sharedMaterial=mat;r.sortingOrder=order;}
}
