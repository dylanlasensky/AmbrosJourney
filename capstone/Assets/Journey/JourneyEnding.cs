using System.Collections;
using UnityEngine;
using UnityEngine.Video;
namespace AmbrosJourney.Journey
{
    public static class JourneyEnding
    {
        // Teammate-owned video: drop Outro.mp4 into Assets/Resources/Journey. Never synthesize a replacement.
        public static IEnumerator Play(JourneySession session)
        {
            session.Pause();var clip=Resources.Load<VideoClip>("Journey/Outro");
            if(!clip){session.Data.outroSeen=false;session.Save();session.ShowCertificate();yield break;}
            var host=new GameObject("Return-home cinematic");var video=host.AddComponent<VideoPlayer>();video.clip=clip;video.playOnAwake=false;video.isLooping=false;video.timeUpdateMode=VideoTimeUpdateMode.UnscaledGameTime;
            video.renderMode=VideoRenderMode.CameraNearPlane;video.targetCamera=Camera.main;video.aspectRatio=VideoAspectRatio.FitInside;video.audioOutputMode=VideoAudioOutputMode.Direct;video.SetDirectAudioVolume(0,.4f);
            bool done=false,failed=false;video.loopPointReached+=_=>done=true;video.errorReceived+=(_,message)=>{failed=true;Debug.LogWarning("Outro playback: "+message);};
            session.SetCinematicVisible(true);AudioListener.pause=false;video.Prepare();float deadline=Time.realtimeSinceStartup+20;
            while(!video.isPrepared&&!failed&&Time.realtimeSinceStartup<deadline)yield return null;
            if(video.isPrepared&&!failed){video.Play();deadline=Time.realtimeSinceStartup+(float)clip.length+15;while(!done&&!failed&&Time.realtimeSinceStartup<deadline)yield return null;}
            Object.Destroy(host);session.SetCinematicVisible(false);session.Data.outroSeen=done;session.Save();session.ShowCertificate();
        }
    }
}
