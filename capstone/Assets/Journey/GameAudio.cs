using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AmbrosJourney.Audio
{
    public enum SoundCue { Click, Correct, Incorrect, Complete, Impact, Pickup, Portal, Dust }
    public static class GameAudio
    {
        static readonly Dictionary<SoundCue,AudioClip> clips=new Dictionary<SoundCue,AudioClip>();
        static readonly List<AudioSource> sources=new List<AudioSource>();
        static readonly Dictionary<SoundCue,float> lastPlayed=new Dictionary<SoundCue,float>();
        static GameObject host;static AudioSource ui,world;static int clickFrame=-1;
        public static bool Muted {get;private set;}
        public static int ClickPlayCount {get;private set;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset(){SceneManager.activeSceneChanged-=SceneChanged;clips.Clear();sources.Clear();lastPlayed.Clear();host=null;ui=world=null;Muted=false;clickFrame=-1;ClickPlayCount=0;}
        public static AudioClip Clip(SoundCue cue)
        {
            if(!clips.TryGetValue(cue,out var clip)||!clip){clip=Resources.Load<AudioClip>("GameSounds/"+cue);clips[cue]=clip;if(!clip)Debug.LogError("Missing game sound: "+cue);}
            return clip;
        }
        static void Ensure()
        {
            if(host)return;
            host=new GameObject("Game sound effects");Object.DontDestroyOnLoad(host);
            ui=host.AddComponent<AudioSource>();ui.playOnAwake=false;ui.spatialBlend=0;ui.volume=.5f;ui.ignoreListenerPause=true;ui.priority=80;
            world=host.AddComponent<AudioSource>();world.playOnAwake=false;world.spatialBlend=0;world.volume=.6f;world.priority=100;
            Register(ui);Register(world);SceneManager.activeSceneChanged-=SceneChanged;SceneManager.activeSceneChanged+=SceneChanged;
        }
        public static void Register(AudioSource source){if(!source)return;source.mute=Muted;if(!sources.Contains(source))sources.Add(source);}
        public static void ToggleMute()=>SetMuted(!Muted);
        public static void SetMuted(bool muted)
        {
            Muted=muted;for(int i=sources.Count-1;i>=0;i--){if(!sources[i])sources.RemoveAt(i);else sources[i].mute=muted;}
        }
        public static void Click()
        {
            if(clickFrame==Time.frameCount)return;clickFrame=Time.frameCount;
            if(Play(SoundCue.Click,.6f))ClickPlayCount++;
        }
        public static bool Play(SoundCue cue,float volume=1)
        {
            if(Muted)return false;Ensure();float now=Time.realtimeSinceStartup;
            if(lastPlayed.TryGetValue(cue,out var before)&&now-before<.04f)return false;
            var clip=Clip(cue);if(!clip)return false;lastPlayed[cue]=now;
            (cue==SoundCue.Click||cue==SoundCue.Pickup?ui:world).PlayOneShot(clip,Mathf.Clamp01(volume));return true;
        }
        static bool Season(string name)=>name=="Spring"||name=="Summer"||name=="Fall"||name=="Winter";
        static void SceneChanged(Scene previous,Scene next){if(Season(previous.name)&&Season(next.name)&&previous.name!=next.name)Play(SoundCue.Portal,.65f);}
    }
}
