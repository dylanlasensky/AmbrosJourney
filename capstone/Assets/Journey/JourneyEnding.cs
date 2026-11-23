using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
namespace AmbrosJourney.Journey
{
    public static class JourneyEnding
    {
        // The final portal replaces the entire game view and soundtrack with the supplied video.
        public static IEnumerator Play(JourneySession session,Action finished)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            var clip=Resources.Load<VideoClip>("Journey/Outro");
            if(!clip){session.Data.outroSeen=false;session.CaptureChapter();finished();yield break;}
#endif
            bool done=false,failed=false,cleaned=false,returningNormally=false;
            var host=new GameObject("Return-home cinematic");
            var audio=host.AddComponent<AudioSource>();audio.playOnAwake=false;audio.spatialBlend=0;audio.ignoreListenerPause=true;audio.volume=.4f;audio.mute=AmbrosJourney.Audio.GameAudio.Muted;
            var video=host.AddComponent<VideoPlayer>();
#if UNITY_WEBGL && !UNITY_EDITOR
            video.source=VideoSource.Url;video.url=Application.streamingAssetsPath+"/JourneyVideos/Outro.mp4";
#else
            video.clip=clip;
#endif
            video.playOnAwake=false;video.isLooping=false;video.waitForFirstFrame=true;video.timeUpdateMode=VideoTimeUpdateMode.UnscaledGameTime;
            var screen=JourneyVideoScreen.Create(host,video);
#if UNITY_WEBGL && !UNITY_EDITOR
            // Web video ignores AudioSource.volume; Direct honors the film level and music mute choice.
            video.audioOutputMode=VideoAudioOutputMode.Direct;video.controlledAudioTrackCount=1;
            video.EnableAudioTrack(0,true);video.SetDirectAudioVolume(0,.4f);video.SetDirectAudioMute(0,AmbrosJourney.Audio.GameAudio.Muted);
#else
            video.audioOutputMode=VideoAudioOutputMode.AudioSource;video.controlledAudioTrackCount=clip?clip.audioTrackCount:(ushort)1;
            for(ushort track=0;track<video.controlledAudioTrackCount;track++){video.EnableAudioTrack(track,track==0);if(track==0)video.SetTargetAudioSource(track,audio);}
#endif
            video.loopPointReached+=_=>done=true;video.errorReceived+=(_,message)=>{failed=true;Debug.LogWarning("Outro playback: "+message);};
            session.SetCinematicVisible(true);
            var isolation=host.AddComponent<JourneyCinematicIsolation>();isolation.Begin(screen.Canvas,audio);
            try
            {
                video.Prepare();float deadline=Time.realtimeSinceStartup+20;
                while(video&&!video.isPrepared&&!failed&&Time.realtimeSinceStartup<deadline)yield return null;
                if(video&&video.isPrepared&&!failed)
                {
                    // Some browsers report unknown duration until playback begins.
                    double duration=video.length;
                    float timeout=duration>0&&!double.IsNaN(duration)&&!double.IsInfinity(duration)?Mathf.Clamp((float)duration,1,600)+15:615;
                    video.Play();deadline=Time.realtimeSinceStartup+timeout;
                    while(video&&!done&&!failed&&Time.realtimeSinceStartup<deadline)yield return null;
                }
                returningNormally=true;
            }
            finally
            {
                // Scene loading can dispose this coroutine again while its completion callback runs.
                if(!cleaned)
                {
                    cleaned=true;
                    if(returningNormally&&screen)screen.ReturnFocusAfterPlayback();
                    if(video)video.Stop();if(host)UnityEngine.Object.Destroy(host);
                    if(isolation)isolation.Restore();
                    if(session)session.SetCinematicVisible(false);
                }
            }
            if(!session)yield break;
            session.Data.outroSeen=done;session.CaptureChapter();finished();
        }
    }
}
