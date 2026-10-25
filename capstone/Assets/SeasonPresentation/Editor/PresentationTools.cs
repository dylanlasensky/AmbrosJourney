#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AmbrosJourney.Fall;
using AmbrosJourney.Summer;

namespace AmbrosJourney.Presentation.Editor
{
    public static class PresentationTools
    {
        [MenuItem("Capstone/Presentation/Prepare Mage Walking Frames")]
        public static void PrepareArt()
        {
            const string prefabPath="Assets/FallLevel/Resources/FallLevel.prefab";
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);var existing=prefab.GetComponent<FallWorld>();
            if(existing.artwork.Any(a=>a.key=="MageWalk8"))return;
            const string source="Assets/Animations/Hero pack/64 X 64 Ancient mage 2D assets/Sprites and files/Full Sprite sheet/Walk sprite sheet.png";
            var sheet=new Texture2D(2,2);sheet.LoadImage(File.ReadAllBytes(source));var added=new List<FallArtwork>();
            for(int i=0;i<8;i++)
            {
                string path="Assets/FallLevel/Art/MageWalk"+(i+1)+".png";var t=new Texture2D(64,64,TextureFormat.RGBA32,false);t.SetPixels(sheet.GetPixels(i*64,0,64,64));t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=32;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
                added.Add(new FallArtwork{key="MageWalk"+(i+1),texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path),pixelsPerUnit=32,pivot=new Vector2(.5f,0)});
            }
            UnityEngine.Object.DestroyImmediate(sheet);
            var contents=PrefabUtility.LoadPrefabContents(prefabPath);
            try{var world=contents.GetComponent<FallWorld>();world.artwork=world.artwork.Concat(added).ToArray();PrefabUtility.SaveAsPrefabAsset(contents,prefabPath);}
            finally{PrefabUtility.UnloadPrefabContents(contents);}
            Debug.Log("PRESENTATION_ART_READY: eight original Mage walking frames added to Fall's existing prefab.");
        }
        [MenuItem("Capstone/Presentation/Validate Shared Compass and Trail")]
        public static void ValidateMath()
        {
            var trail=new SeasonTrail();trail.Record(Vector2.zero);for(int i=1;i<=100;i++)trail.Record(Vector2.right*i*.1f);
            Require(Vector2.Distance(trail.Behind(1.8f),new Vector2(8.2f,0))<.03f,"Follower keeps 1.8-unit spacing");
            for(int i=1;i<=50;i++)trail.Record(new Vector2(10,i*.1f));
            Require(Vector2.Distance(trail.Behind(1.8f),new Vector2(10,3.2f))<.03f,"Follower traces the turn");
            Require(Vector2.Distance(trail.Behind(7.2f),new Vector2(7.8f,0))<.04f,"Last follower stays on the older leg of the trail");
            var stationary=trail.Behind(1.8f);for(int i=0;i<100;i++)trail.Record(new Vector2(10,5));Require(Vector2.Distance(stationary,trail.Behind(1.8f))<.001f,"Standing still does not jiggle the trail");
            int revision=trail.Revision;trail.Record(new Vector2(-38,34));Require(trail.Revision>revision&&Vector2.Distance(trail.Behind(1.8f),new Vector2(-39.8f,34))<.02f,"Arrival teleport resets the party trail");
            Debug.Log("PRESENTATION_MATH_PASS: follower spacing, corners, stops and teleport reset.");
        }
        [MenuItem("Capstone/Presentation/Validate Live Compass Routes")]
        public static void ValidateRoutes()
        {
            var fall=UnityEngine.Object.FindAnyObjectByType<FallLevel>();
            var summer=UnityEngine.Object.FindAnyObjectByType<SummerLevel>();
            Require(fall||summer,"Start Summer or Fall first");Physics2D.SyncTransforms();
            Vector2 origin=fall?FallLevel.Spawn:SummerLevel.Spawn;
            Vector2 min=fall?FallWorld.Min:new Vector2(-30,-32),max=fall?FallWorld.Max:new Vector2(30,28);
            var goals=fall?fall.stations.Select(t=>(Vector2)t.position).ToArray():new[]{(Vector2)summer.workshop.position,(Vector2)summer.garden.position,(Vector2)summer.messagePost.position};
            foreach(var goal in goals)
            {
                var route=SeasonNavigation.FindPath(origin,goal,min,max);Require(route.Count>0,"Shared compass reaches "+goal);
                Vector2 previous=origin;foreach(var point in route){Require(SeasonNavigation.ClearSegment(previous,point),"Route avoids static obstacles at "+point);previous=point;}
                var lookahead=new SeasonNavigation(min,max).Next(origin,goal);Require(SeasonNavigation.ClearSegment(origin,lookahead),"Arrow points to a visible route waypoint");origin=goal;
            }
            Debug.Log("PRESENTATION_ROUTES_PASS: shared compass reaches all three landmarks with clear route segments and visible lookahead.");
        }
        public static void Require(bool value,string message){if(!value)throw new InvalidOperationException("PRESENTATION CHECK: "+message);}
    }
}
#endif
