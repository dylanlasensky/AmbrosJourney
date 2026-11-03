#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AmbrosJourney.Fall;
using AmbrosJourney.Presentation;
namespace AmbrosJourney.Journey.Editor
{
    [InitializeOnLoad]
    public static class CompanionMovementChecks
    {
        const string Key="Moonbranch.CompanionCheck";
        static FallLevel level;static List<Vector2> path;static Vector2[] previous;
        static double ready,deadline;static float settledAt;static int frames,phase;
        static readonly MethodInfo Signal=typeof(EditorApplication).GetMethod("SignalTick",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
        static CompanionMovementChecks(){EditorApplication.update+=Tick;}
        public static void Run()
        {
            SessionState.SetBool(Key,true);EditorSceneManager.OpenScene("Assets/FallLevel/Fall.unity");
            EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();
            EditorApplication.isPlaying=true;deadline=EditorApplication.timeSinceStartup+120;
        }
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
        static void Tick()
        {
            if(!SessionState.GetBool(Key,false))return;
            try
            {
                JourneySession.TestMode=true;JourneySession.SavePathOverride=Path.GetFullPath("Logs/SeasonChecks/companion-save.json");
                Application.runInBackground=true;EditorApplication.QueuePlayerLoopUpdate();Signal?.Invoke(null,null);
                if(deadline==0)deadline=EditorApplication.timeSinceStartup+120;
                Require(EditorApplication.timeSinceStartup<deadline,"Companion check timed out");
                if(!EditorApplication.isPlaying)return;
                if(ready==0){ready=EditorApplication.timeSinceStartup+3;return;}if(EditorApplication.timeSinceStartup<ready)return;
                if(!level)
                {
                    level=UnityEngine.Object.FindAnyObjectByType<FallLevel>();Require(level,"Fall loaded");
                    JourneySession.Instance.BeginTestProfile();JourneySession.Instance.Resume();level.DismissPanel();
                    path=SeasonNavigation.FindPath(level.player.transform.position,FallWorld.MissionPositions[0],FallWorld.Min,FallWorld.Max);
                    Require(path.Count>0,"Walk route exists");previous=new Vector2[3];for(int i=0;i<3;i++)previous[i]=Party()[i].transform.position;
                }
                if(Time.frameCount==frames)return;frames=Time.frameCount;
                var party=Party();
                for(int i=0;i<3;i++)
                {
                    Vector2 p=party[i].transform.position;
                    if(phase!=2&&Vector2.Distance(previous[i],p)<4)Require(SeasonNavigation.ClearSegment(previous[i],p),"Companion crossed scenery: "+i);
                    previous[i]=p;
                }
                if(path.Count>0)
                {
                    Vector2 current=level.player.transform.position,next=Vector2.MoveTowards(current,path[0],4.6f*Mathf.Min(Time.deltaTime,.05f));
                    level.player.transform.position=next;level.player.GetComponent<Rigidbody2D>().position=next;
                    if(Vector2.Distance(next,path[0])<.1f)path.RemoveAt(0);
                }
                else
                {
                    if(settledAt==0)settledAt=Time.time+6;if(Time.time<settledAt)return;
                    foreach(var actor in party)Require(Vector2.Distance(actor.transform.position,level.player.transform.position)<5,"Companion catches up and stays nearby");
                    if(phase==0)
                    {
                        Directory.CreateDirectory("Logs/SeasonChecks");SeasonIntegrationChecks.Capture("CompanionsResting");
                        // A restored checkpoint across the river must relocate the party safely.
                        Vector2 destination=new Vector2(27,26);level.player.transform.position=destination;level.player.GetComponent<Rigidbody2D>().position=destination;
                        phase=2;settledAt=Time.time+3;return;
                    }
                    SeasonIntegrationChecks.Capture("CompanionsCheckpoint");
                    File.WriteAllText("Logs/SeasonChecks/Companions.txt","PASS: live travel, collision-safe steps, catch-up, idle grouping and checkpoint relocation. Frames: "+frames);
                    Debug.Log("COMPANION_MOVEMENT_PASS: live travel, safe steps, nearby rest and checkpoint relocation");Finish(0);
                }
            }
            catch(Exception e){Debug.LogError(e);Finish(1);}
        }
        static FallActor[] Party()=>new[]{level.mage,level.maleWarrior,level.femaleWarrior};
        static void Finish(int code){SessionState.SetBool(Key,false);EditorApplication.Exit(code);}
    }
}
#endif
