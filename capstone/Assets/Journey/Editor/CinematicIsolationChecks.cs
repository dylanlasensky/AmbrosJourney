// Runs isolated Editor regression checks for cinematic isolation.
#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using AmbrosJourney.Journey;
public static class CinematicIsolationChecks
{
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        var film=new GameObject("Test film");var world=new GameObject("Test world");var late=new GameObject("Delayed title");
        bool paused=AudioListener.pause;
        try
        {
            var canvas=film.AddComponent<Canvas>();var audio=film.AddComponent<AudioSource>();
            var worldCanvas=world.AddComponent<Canvas>();var camera=world.AddComponent<Camera>();var music=world.AddComponent<AudioSource>();
            world.SetActive(false);
            var isolation=film.AddComponent<JourneyCinematicIsolation>();isolation.Begin(canvas,audio);
            world.SetActive(true);Check(!worldCanvas.enabled&&!camera.enabled&&music.mute,"Inactive world is isolated when activated later");
            var lateCanvas=late.AddComponent<Canvas>();var lateAudio=late.AddComponent<AudioSource>();lateAudio.ignoreListenerPause=true;
            typeof(JourneyCinematicIsolation).GetMethod("LateUpdate",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(isolation,null);
            Check(!lateCanvas.enabled&&lateAudio.mute,"New overlays and pause-exempt audio remain hidden/muted");
            Check(canvas.enabled&&!audio.mute&&AudioListener.pause,"Film owns picture and sound");
            isolation.Restore();Check(worldCanvas.enabled&&camera.enabled&&!music.mute&&lateCanvas.enabled&&!lateAudio.mute,"Original states restored");
            isolation.Restore();Check(AudioListener.pause==paused,"Cleanup is idempotent");
            Directory.CreateDirectory("Logs/CinematicChecks");File.WriteAllText("Logs/CinematicChecks/Results.txt","PASS: inactive and newly-created overlays/audio suppressed, film retained, original state restored.");
            Debug.Log("CINEMATIC_ISOLATION_PASS");
        }
        finally{UnityEngine.Object.DestroyImmediate(film);UnityEngine.Object.DestroyImmediate(world);UnityEngine.Object.DestroyImmediate(late);AudioListener.pause=paused;}
    }
}
#endif
