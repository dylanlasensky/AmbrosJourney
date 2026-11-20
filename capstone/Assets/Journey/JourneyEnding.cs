using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
namespace AmbrosJourney.Journey
{
    public static class JourneyEnding
    {
        // The final portal replaces the entire game view and soundtrack with the supplied video.
        public static IEnumerator Play(JourneySession session,Action finished)
        {
            var clip=Resources.Load<VideoClip>("Journey/Outro");
            if(!clip){session.Data.outroSeen=false;session.Save();finished();yield break;}
            var cameras=UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            var cameraStates=cameras.Select(camera=>camera.enabled).ToArray();
            var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            var canvasStates=canvases.Select(canvas=>canvas.enabled).ToArray();
            var sources=UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            var muteStates=sources.Select(source=>source.mute).ToArray();
            bool pausedAudio=AudioListener.pause,done=false,failed=false,cleaned=false;
            var host=new GameObject("Return-home cinematic");
            var canvas=host.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32767;
            var imageObject=new GameObject("Full-screen ending video",typeof(RectTransform),typeof(UnityEngine.UI.RawImage));imageObject.transform.SetParent(host.transform,false);
            var rect=imageObject.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            var target=new RenderTexture(Mathf.Max(1,Screen.width),Mathf.Max(1,Screen.height),0);target.Create();
            var active=RenderTexture.active;RenderTexture.active=target;GL.Clear(true,true,Color.black);RenderTexture.active=active;
            var image=imageObject.GetComponent<UnityEngine.UI.RawImage>();image.texture=target;image.raycastTarget=false;
            var audio=host.AddComponent<AudioSource>();audio.playOnAwake=false;audio.spatialBlend=0;audio.ignoreListenerPause=true;audio.volume=.4f;audio.mute=AmbrosJourney.Audio.GameAudio.Muted;
            var video=host.AddComponent<VideoPlayer>();video.clip=clip;video.playOnAwake=false;video.isLooping=false;video.waitForFirstFrame=true;video.timeUpdateMode=VideoTimeUpdateMode.UnscaledGameTime;
            video.renderMode=VideoRenderMode.RenderTexture;video.targetTexture=target;video.aspectRatio=VideoAspectRatio.FitOutside;
            video.audioOutputMode=VideoAudioOutputMode.AudioSource;video.controlledAudioTrackCount=clip.audioTrackCount;
            for(ushort track=0;track<clip.audioTrackCount;track++){video.EnableAudioTrack(track,track==0);if(track==0)video.SetTargetAudioSource(track,audio);}
            video.loopPointReached+=_=>done=true;video.errorReceived+=(_,message)=>{failed=true;Debug.LogWarning("Outro playback: "+message);};
            session.SetCinematicVisible(true);
            foreach(var camera in cameras)camera.enabled=false;
            foreach(var gameCanvas in canvases)gameCanvas.enabled=false;
            foreach(var source in sources)source.mute=true;
            AudioListener.pause=true;
            try
            {
                video.Prepare();float deadline=Time.realtimeSinceStartup+20;
                while(video&&!video.isPrepared&&!failed&&Time.realtimeSinceStartup<deadline)yield return null;
                if(video&&video.isPrepared&&!failed){video.Play();deadline=Time.realtimeSinceStartup+(float)clip.length+15;while(video&&!done&&!failed&&Time.realtimeSinceStartup<deadline)yield return null;}
            }
            finally
            {
                // Scene loading can dispose this coroutine again while its completion callback runs.
                if(!cleaned)
                {
                    cleaned=true;
                    if(video)video.Stop();if(image)image.texture=null;
                    if(target){target.Release();UnityEngine.Object.Destroy(target);}if(host)UnityEngine.Object.Destroy(host);
                    for(int i=0;i<cameras.Length;i++)if(cameras[i])cameras[i].enabled=cameraStates[i];
                    for(int i=0;i<canvases.Length;i++)if(canvases[i])canvases[i].enabled=canvasStates[i];
                    for(int i=0;i<sources.Length;i++)if(sources[i])sources[i].mute=muteStates[i];
                    AudioListener.pause=pausedAudio;
                    if(session)session.SetCinematicVisible(false);
                }
            }
            if(!session||!video)yield break;
            session.Data.outroSeen=done;session.Save();finished();
        }
    }
}
