#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using AmbrosJourney.Fall;

namespace AmbrosJourney.Winter.Editor
{
    public static class WinterTools
    {
        const string Root="Assets/WinterLevel";
        public const string ScenePath=Root+"/Winter.unity";
        static bool CanChangeScene()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return false;
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty){Debug.LogWarning("Save your current work first. Winter tools leave unsaved scenes alone.");return false;}
            return true;
        }
        [MenuItem("Capstone/Winter/Open Winter Scene")]
        public static void Open(){if(CanChangeScene())EditorSceneManager.OpenScene(ScenePath);}
        [MenuItem("Capstone/Winter/Play Winter Only")]
        public static void Play(){if(CanChangeScene()){EditorSceneManager.OpenScene(ScenePath);EditorApplication.isPlaying=true;}}
        [MenuItem("Capstone/Winter/Rebuild Winter Definition")]
        public static void Build()
        {
            if(!CanChangeScene())return;
            Directory.CreateDirectory(Root+"/Art");Directory.CreateDirectory(Root+"/Resources");AssetDatabase.Refresh();
            var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/FallLevel/Resources/FallLevel.prefab").GetComponent<FallWorld>();
            const string summerPath="Assets/SummerLevel/Resources/SummerLevel.prefab";
            var summerContents=PrefabUtility.LoadPrefabContents(summerPath);
            try{var sw=summerContents.GetComponent<AmbrosJourney.Summer.SummerWorld>();var items=sw.artwork.ToList();foreach(var a in source.artwork.Where(a=>a.key.StartsWith("MageWalk")))if(!items.Any(x=>x.key==a.key))items.Add(new AmbrosJourney.Summer.SummerArtwork{key=a.key,texture=a.texture,pixelsPerUnit=a.pixelsPerUnit,pivot=a.pivot});sw.artwork=items.ToArray();PrefabUtility.SaveAsPrefabAsset(summerContents,summerPath);}finally{PrefabUtility.UnloadPrefabContents(summerContents);}
            var art=source.artwork.Select(a=>new WinterArtwork{key=a.key,texture=a.texture,pixelsPerUnit=a.pixelsPerUnit,pivot=a.pivot}).ToList();
            var winter=Read("Assets/MapWinter/seasonal sample (winter).png");
            Replace(art,Crop(winter,"Grass",0,24,8,8,8,new Vector2(.5f,.5f)));
            Replace(art,Crop(winter,"Path",64,0,16,16,16,new Vector2(.5f,.5f)));
            Replace(art,Crop(winter,"Flowers",0,96,16,16,16,new Vector2(.5f,.5f)));
            Replace(art,Crop(winter,"Tree",176,0,80,112,16,new Vector2(.5f,.08f)));
            Replace(art,Crop(winter,"Rock",48,224,32,32,16,new Vector2(.5f,.12f)));
            Replace(art,Crop(winter,"Bush",32,128,32,32,16,new Vector2(.5f,.15f)));
            Replace(art,Crop(winter,"Water",192,212,16,16,16,new Vector2(.5f,.5f)));
            Replace(art,Crop(winter,"CaveFloor",64,0,16,16,16,new Vector2(.5f,.5f)));
            Replace(art,Crop(winter,"CaveWall",112,80,16,16,16,new Vector2(.5f,.5f)));
            UnityEngine.Object.DestroyImmediate(winter);
            AddSheet(art,"Assets/Animations/Hero pack/64 X 64 Mage 2D assets/Sprites and files/Full Sprite sheet/mage IDLE sprite sheet.png","LittleIdle",4);
            AddSheet(art,"Assets/Animations/Hero pack/64 X 64 Mage 2D assets/Sprites and files/Full Sprite sheet/mage walk sprite sheet.png","LittleWalk",4);
            AddSheet(art,"Assets/Animations/Hero pack/64 X 64 Skeleton 2D assets/Sprites and files/Full Sprite sheet/IDLE sprite sheet.png","SkeletonIdle",4);
            AddSheet(art,"Assets/Animations/Hero pack/64 X 64 Skeleton 2D assets/Sprites and files/Full Sprite sheet/Attack 1 sprite sheet.png","SkeletonAttack",6);
            Generated(art,"CaveEntrance",25.6f, new Vector2(.5f,.08f));Generated(art,"Crystal",32,new Vector2(.5f,.08f));
            Generated(art,"Lantern",64,new Vector2(.5f,.08f));
            Generated(art,"MountainRidge",16,new Vector2(.5f,.10f));Generated(art,"SnowGround",32,new Vector2(.5f,.5f));
            var world=new GameObject("Frostveil");world.AddComponent<WinterWorld>().artwork=art.ToArray();
            PrefabUtility.SaveAsPrefabAsset(world,Root+"/Resources/WinterLevel.prefab");UnityEngine.Object.DestroyImmediate(world);
            
            var previous=SceneManager.GetActiveScene();if(string.IsNullOrEmpty(previous.path)){EditorSceneManager.OpenScene("Assets/Scenes/Spring.unity");previous=SceneManager.GetActiveScene();}var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Resources/WinterLevel.prefab");var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
            EditorSceneManager.SaveScene(scene,ScenePath);EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(previous);AssetDatabase.SaveAssets();
            Debug.Log("WINTER_BUILD_PASS: Frostveil scene and Resources prefab created.");
        }
        static Texture2D Read(string path){var t=new Texture2D(2,2);if(!t.LoadImage(File.ReadAllBytes(path)))throw new InvalidOperationException(path);return t;}
        static WinterArtwork Crop(Texture2D source,string key,int x,int top,int w,int h,float ppu,Vector2 pivot)
        {
            string path=Root+"/Art/"+key+".png";var t=new Texture2D(w,h,TextureFormat.RGBA32,false);t.SetPixels(source.GetPixels(x,source.height-top-h,w,h));t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=ppu;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.SaveAndReimport();
            return new WinterArtwork{key=key,texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path),pixelsPerUnit=ppu,pivot=pivot};
        }
        static void Replace(List<WinterArtwork> art,WinterArtwork entry){art.RemoveAll(a=>a.key==entry.key);art.Add(entry);}
        static void AddSheet(List<WinterArtwork> art,string path,string prefix,int count)
        {var t=Read(path);for(int i=0;i<count;i++)Replace(art,Crop(t,prefix+(i+1),i*64,0,64,64,32,new Vector2(.5f,0)));UnityEngine.Object.DestroyImmediate(t);}
        static void Generated(List<WinterArtwork> art,string key,float ppu,Vector2 pivot)
        {string path=Root+"/Art/"+key+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=ppu;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=key=="MountainRidge"?512:key=="SnowGround"?32:key=="CaveEntrance"?256:128;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.SaveAndReimport();Replace(art,new WinterArtwork{key=key,texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path),pixelsPerUnit=ppu,pivot=pivot});}
    }
}
#endif
