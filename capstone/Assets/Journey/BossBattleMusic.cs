using UnityEngine;
namespace AmbrosJourney.Audio
{
    // One looping encounter track replaces the level music until the conversation closes.
    public sealed class BossBattleMusic : MonoBehaviour
    {
        AudioSource battle,background;bool resumeBackground;
        public static void Play(MonoBehaviour owner,AudioSource levelMusic)
        {
            var music=owner.GetComponent<BossBattleMusic>();
            if(!music)music=owner.gameObject.AddComponent<BossBattleMusic>();
            music.Begin(levelMusic);
        }
        public static void Stop(MonoBehaviour owner)
        {
            var music=owner.GetComponent<BossBattleMusic>();if(music)music.End();
        }
        void Begin(AudioSource levelMusic)
        {
            if(battle&&battle.isPlaying)return;
            var clip=Resources.Load<AudioClip>("SeasonMusic/atlasaudio-battle-drums-636771");if(!clip)return;
            if(!battle){battle=gameObject.AddComponent<AudioSource>();battle.playOnAwake=false;battle.loop=true;battle.spatialBlend=0;battle.volume=.22f;GameAudio.RegisterMusic(battle);}
            background=levelMusic;resumeBackground=background&&background.isPlaying;
            if(resumeBackground)background.Pause();
            battle.clip=clip;battle.Play();
        }
        void End()
        {
            if(battle)battle.Stop();
            if(background&&resumeBackground)background.UnPause();
            background=null;resumeBackground=false;
        }
        void OnDisable()=>End();
        void OnDestroy()=>End();
    }
}
