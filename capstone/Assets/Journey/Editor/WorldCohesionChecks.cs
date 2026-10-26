#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using AmbrosJourney.Presentation;
using AmbrosJourney.Winter;
namespace AmbrosJourney.Journey.Editor
{
    public static class WorldCohesionChecks
    {
        static void Require(bool value,string reason){if(!value)throw new Exception("WORLD COHESION: "+reason);}
        public static void CheckAndCapture(int chapter)
        {
            var world=UnityEngine.Object.FindAnyObjectByType<WorldCohesion>();Require(world,"Cohesion pass exists");
            Require(world.Buildings==new[]{1,2,3,2}[chapter-1],"Expected authored building count in season "+chapter+", got "+world.Buildings);
            var min=new[]{new Vector2(-32,-38),new Vector2(-30,-32),new Vector2(-49,-48),WinterWorld.Min}[chapter-1];
            var max=new[]{new Vector2(32,26),new Vector2(30,28),new Vector2(49,44),WinterWorld.Max}[chapter-1];
            var start=new[]{new Vector2(-18,16),new Vector2(0,22),new Vector2(-38,34),WinterLevel.Spawn}[chapter-1];
            var winter=UnityEngine.Object.FindAnyObjectByType<WinterLevel>();bool inCave=winter&&winter.Progress.InCave;if(winter)winter.World.RestoreRegion(false);
            try
            {
                Physics2D.SyncTransforms();
                foreach(var door in world.Doorsteps)Require(SeasonNavigation.FindPath(start,door,min,max).Count>0,"Reachable doorstep "+door+" in "+chapter);
                var trees=UnityEngine.Object.FindObjectsByType<SpriteRenderer>().Where(r=>r.name=="Snow fir").ToArray();
                if(chapter==4){Require(trees.Length>15,"Winter retains forest");foreach(var tree in trees)Require(tree.sprite.texture.name=="WinterNature","Winter roots use snow-compatible transparent art");}
                Capture("Map"+chapter,(min+max)*.5f,Mathf.Max(max.x-min.x,max.y-min.y)*.5f+2,true);
                foreach(var door in world.Doorsteps)Capture("Door"+chapter+"_"+Mathf.RoundToInt(door.x),door+Vector2.up*2,7,false);
                Capture("Arrival"+chapter,start,7,false);
                // Full-map coverage in overlapping views, including quiet corners away from missions.
                for(int y=0;y<3;y++)for(int x=0;x<3;x++)Capture("Survey"+chapter+"_"+x+"_"+y,new Vector2(Mathf.Lerp(min.x+10,max.x-10,x/2f),Mathf.Lerp(min.y+7,max.y-7,y/2f)),12,false);
                if(winter){winter.World.RestoreRegion(true);Capture("Map4Cave",new Vector2(120,1),37,true);Capture("CaveArchive",WinterWorld.MissionPositions[2],7,false);}
            }
            finally{if(winter)winter.World.RestoreRegion(inCave);}
            Debug.Log("WORLD_COHESION_PASS: season "+chapter+", "+world.Buildings+" buildings, all doorstep routes, full map and nine survey views.");
        }
        static void Capture(string name,Vector2 center,float size,bool square)
        {
            Directory.CreateDirectory("Logs/WorldAudit");var camera=Camera.main;var oldPosition=camera.transform.position;float oldSize=camera.orthographicSize,oldAspect=camera.aspect;var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
            var canvases=UnityEngine.Object.FindObjectsByType<Canvas>();var enabled=canvases.Select(c=>c.enabled).ToArray();
            int width=square?2048:1280,height=square?2048:720;var rt=new RenderTexture(width,height,24);var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                foreach(var canvas in canvases)canvas.enabled=false;
                camera.transform.position=new Vector3(center.x,center.y,-10);camera.orthographicSize=size;camera.aspect=(float)width/height;camera.targetTexture=rt;camera.Render();
                RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes("Logs/WorldAudit/"+name+".png",texture.EncodeToPNG());
            }
            finally{camera.transform.position=oldPosition;camera.orthographicSize=oldSize;camera.aspect=oldAspect;camera.targetTexture=oldTarget;RenderTexture.active=oldActive;for(int i=0;i<canvases.Length;i++)canvases[i].enabled=enabled[i];UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);}
        }
    }
}
#endif
