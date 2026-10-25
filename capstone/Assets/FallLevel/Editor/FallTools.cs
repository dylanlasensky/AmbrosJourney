#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using AmbrosJourney.Summer;

namespace AmbrosJourney.Fall.Editor
{
    public static class FallTools
    {
        const string Root="Assets/FallLevel";
        public const string ScenePath=Root+"/Fall.unity";
        static bool CanChangeScene()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return false;
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty){Debug.LogWarning("Save your current work first. Fall tools leave unsaved scenes alone.");return false;}
            return true;
        }
        [MenuItem("Capstone/Fall/Open Fall Scene")]
        public static void Open(){if(CanChangeScene())EditorSceneManager.OpenScene(ScenePath);}
        [MenuItem("Capstone/Fall/Play Fall Only")]
        public static void Play(){if(CanChangeScene()){EditorSceneManager.OpenScene(ScenePath);EditorApplication.isPlaying=true;}}
        [MenuItem("Capstone/Fall/Rebuild Fall Definition")]
        public static void Build()
        {
            if(!CanChangeScene())return;
            Directory.CreateDirectory(Root+"/Art");Directory.CreateDirectory(Root+"/Resources");AssetDatabase.Refresh();
            var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SummerLevel/Resources/SummerLevel.prefab").GetComponent<SummerWorld>();
            var art=source.artwork.Select(a=>new FallArtwork{key=a.key,texture=a.texture,pixelsPerUnit=a.pixelsPerUnit,pivot=a.pivot}).ToList();
            var autumn=Read("Assets/MapFall/seasonal sample (autumn).png");
            Replace(art,Crop(autumn,"Grass",0,24,8,8,8,new Vector2(.5f,.5f)));
            Replace(art,Crop(autumn,"Path",64,0,16,16,16,new Vector2(.5f,.5f)));
            Replace(art,Crop(autumn,"Flowers",0,96,16,16,16,new Vector2(.5f,.5f)));
            Replace(art,Crop(autumn,"Tree",176,0,80,112,16,new Vector2(.5f,.08f)));
            Replace(art,Crop(autumn,"Rock",48,224,32,32,16,new Vector2(.5f,.12f)));
            Replace(art,Crop(autumn,"Bush",32,128,32,32,16,new Vector2(.5f,.15f)));
            Replace(art,Crop(autumn,"Water",192,212,16,16,16,new Vector2(.5f,.5f)));UnityEngine.Object.DestroyImmediate(autumn);
            const string ember="Assets/Animations/Hero pack/64 X 64 EmberFist warrior/Sprites and files/Individual sprite sheet/";
            for(int i=1;i<=4;i++){var t=Read(ember+"IDLE"+i+".png");art.Add(Crop(t,"EmberIdle"+i,0,0,64,64,32,new Vector2(.5f,0)));UnityEngine.Object.DestroyImmediate(t);}
            for(int i=1;i<=8;i++){var t=Read(ember+"Walk"+i+".png");art.Add(Crop(t,"EmberWalk"+i,0,0,64,64,32,new Vector2(.5f,0)));UnityEngine.Object.DestroyImmediate(t);}
            var sentinel=Read("Assets/Animations/Creatures Pack Premium/Sprites/IDLE animation Groupe 5.png");
            for(int i=0;i<4;i++)art.Add(Crop(sentinel,"SentinelIdle"+(i+1),160,i*32,32,32,8,new Vector2(.5f,.1f)));
            UnityEngine.Object.DestroyImmediate(sentinel);
            var world=new GameObject("Emberwood");world.AddComponent<FallWorld>().artwork=art.ToArray();
            PrefabUtility.SaveAsPrefabAsset(world,Root+"/Resources/FallLevel.prefab");UnityEngine.Object.DestroyImmediate(world);
            AmbrosJourney.Presentation.Editor.PresentationTools.PrepareArt();
            var previous=SceneManager.GetActiveScene();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Resources/FallLevel.prefab");var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
            EditorSceneManager.SaveScene(scene,ScenePath);EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(previous);AssetDatabase.SaveAssets();
            Debug.Log("FALL_BUILD_PASS: Fall scene and Resources prefab created using Summer conventions and original autumn/Emberfist art.");
        }
        static Texture2D Read(string path){var t=new Texture2D(2,2);if(!t.LoadImage(File.ReadAllBytes(path)))throw new InvalidOperationException(path);return t;}
        static FallArtwork Crop(Texture2D source,string key,int x,int top,int w,int h,float ppu,Vector2 pivot)
        {
            string path=Root+"/Art/"+key+".png";var t=new Texture2D(w,h,TextureFormat.RGBA32,false);t.SetPixels(source.GetPixels(x,source.height-top-h,w,h));t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=ppu;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.SaveAndReimport();
            return new FallArtwork{key=key,texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path),pixelsPerUnit=ppu,pivot=pivot};
        }
        static void Replace(List<FallArtwork> art,FallArtwork entry){art.RemoveAll(a=>a.key==entry.key);art.Add(entry);}
        [MenuItem("Capstone/Fall/Validate Fall Progression")]
        public static void Validate()
        {
            var p=new FallProgress();Require(!p.BeginBoss()&&!p.AnswerBoss(0),"Guardian locked before missions");Require(!p.ReadNote(0)&&!p.AnswerMission(0,1),"Introduction required");p.Introduced=true;
            for(int m=0;m<3;m++)
            {
                Require(!p.AnswerMission(m,FallContent.Missions[m][0].Correct),"Note required");p.ReadNote(m);
                foreach(var q in FallContent.Missions[m]){int old=p.Steps[m];Require(!p.AnswerMission(m,(q.Correct+1)%3)&&p.Steps[m]==old,"Mistakes retry without losing steps");Require(p.AnswerMission(m,q.Correct),"Mission step");}
                Require(p.MissionComplete(m),"Rune earned");Require(!p.AnswerMission(m,0),"No duplicate reward");
            }
            Require(p.CanFight,"All runes required");
            for(int mask=0;mask<243;mask++)
            {
                p.Complete=false;p.BeginBoss();int bits=mask,score=0;
                for(int q=0;q<5;q++){int choice=bits%3;bits/=3;if(choice==FallContent.Boss[q].Correct)score++;p.AnswerBoss(choice);}
                Require(p.BossFinished&&p.BossScore==score&&p.Complete==(score>=4),"All 243 guardian combinations");Require(p.RuneCount==3,"Retry preserves runes");
            }
            p.Complete=false;p.BeginBoss();for(int i=0;i<5;i++)p.AnswerBoss(i<4?FallContent.Boss[i].Correct:(FallContent.Boss[i].Correct+1)%3);
            Require(p.Complete&&!p.BeginBoss()&&!p.AnswerBoss(0),"80 percent final success");for(int i=0;i<12;i++)p.LoseCourage();Require(p.Courage>0&&p.RuneCount==3,"Courage recovery");
            Require(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Resources/FallLevel.prefab"),"Portal prefab present");
            Debug.Log("FALL_VALIDATION_PASS: note prerequisites, seven challenges, retries, 243 guardian outcomes, 80% pass and progress preservation.");
        }
        public static void ValidateRoutes()
        {
            var level=UnityEngine.Object.FindAnyObjectByType<FallLevel>();Require(level,"Fall must be running");Physics2D.SyncTransforms();
            foreach(var pos in FallWorld.MissionPositions)Require(FallWorld.FindPath(FallLevel.Spawn,pos).Count>0,"Station reachable from arrival");
            Require(FallWorld.FindPath(FallLevel.Spawn,FallWorld.GuardianPosition).Count==0,"Arena sealed before runes");
            level.arenaSeal.SetActive(false);Physics2D.SyncTransforms();Require(FallWorld.FindPath(FallLevel.Spawn,FallWorld.GuardianPosition).Count>0,"Guardian reachable after runes");
            Require(FallWorld.FindPath(FallWorld.GuardianPosition,FallWorld.GatePosition).Count==0,"Winter sealed before pass");level.exitSeal.SetActive(false);Physics2D.SyncTransforms();Require(FallWorld.FindPath(FallWorld.GuardianPosition,FallWorld.GatePosition).Count>0,"Winter reachable after pass");
            level.arenaSeal.SetActive(!level.Progress.CanFight);level.exitSeal.SetActive(!level.Progress.Complete);
            Debug.Log("FALL_ROUTES_PASS: every station reachable, guardian and Winter physically gated, both unlock routes reachable.");
        }
        static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException("FALL VALIDATION: "+message);}
    }
}
#endif
