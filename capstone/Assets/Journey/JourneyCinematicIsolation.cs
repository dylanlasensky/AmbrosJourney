using System.Collections.Generic;
using UnityEngine;
namespace AmbrosJourney.Journey
{
    // Captures inactive UI too, so delayed chapter headings cannot appear above the film.
    [DefaultExecutionOrder(32000)]
    public sealed class JourneyCinematicIsolation : MonoBehaviour
    {
        readonly Dictionary<Canvas,bool> canvases=new Dictionary<Canvas,bool>();
        readonly Dictionary<Camera,bool> cameras=new Dictionary<Camera,bool>();
        readonly Dictionary<AudioSource,bool> sources=new Dictionary<AudioSource,bool>();
        Canvas film;AudioSource filmAudio;bool listenerPaused,released;
        public void Begin(Canvas canvas,AudioSource audio)
        {
            film=canvas;filmAudio=audio;listenerPaused=AudioListener.pause;
            SilenceWorld();HideWorld();Canvas.preWillRenderCanvases+=HideWorld;
        }
        void HideWorld()
        {
            if(released)return;
            foreach(var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
            {
                if(canvas==film)continue;
                if(!canvases.ContainsKey(canvas))canvases.Add(canvas,canvas.enabled);
                canvas.enabled=false;
            }
            foreach(var camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include))
            {
                if(!cameras.ContainsKey(camera))cameras.Add(camera,camera.enabled);
                camera.enabled=false;
            }
        }
        void SilenceWorld()
        {
            AudioListener.pause=true;
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include))
            {
                if(source==filmAudio)continue;
                if(!sources.ContainsKey(source))sources.Add(source,source.mute);
                source.mute=true;
            }
        }
        void LateUpdate(){if(!released){HideWorld();SilenceWorld();}}
        public void Restore()
        {
            if(released)return;released=true;Canvas.preWillRenderCanvases-=HideWorld;
            foreach(var item in cameras)if(item.Key)item.Key.enabled=item.Value;
            foreach(var item in canvases)if(item.Key)item.Key.enabled=item.Value;
            foreach(var item in sources)if(item.Key)item.Key.mute=item.Value;
            AudioListener.pause=listenerPaused;
        }
        void OnDestroy()=>Restore();
    }
}
