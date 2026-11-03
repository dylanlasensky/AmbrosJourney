#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using AmbrosJourney.Audio;

namespace AmbrosJourney.Journey.Editor
{
    public static class SoundEffectsChecks
    {
        const string Pending="SoundEffectsChecks.Pending";
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        public static void Run()
        {
            foreach(SoundCue cue in Enum.GetValues(typeof(SoundCue)))
            {
                string path="Assets/Audio/Resources/GameSounds/"+cue+".wav";
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(AudioImporter)AssetImporter.GetAtPath(path);var settings=importer.defaultSampleSettings;
                settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;settings.preloadAudioData=true;
                importer.defaultSampleSettings=settings;importer.loadInBackground=false;importer.forceToMono=true;importer.SaveAndReimport();
            }
            SessionState.SetBool(Pending,true);CertificateRewardChecks.Run();
        }
        static void Check(bool value,string why){if(!value)throw new Exception("SOUND CHECK: "+why);}
        public static void InspectIfRequested()
        {
            if(!SessionState.GetBool(Pending,false))return;SessionState.SetBool(Pending,false);
            foreach(SoundCue cue in Enum.GetValues(typeof(SoundCue)))
            {
                var clip=GameAudio.Clip(cue);Check(clip&&clip.channels==1&&clip.length<=.56f&&clip.loadType==AudioClipLoadType.DecompressOnLoad,"Short mono preloaded cue: "+cue);
                var data=new float[clip.samples];Check(clip.GetData(data,0),"Readable PCM "+cue);float peak=0;double energy=0;
                foreach(float sample in data){peak=Mathf.Max(peak,Mathf.Abs(sample));energy+=sample*sample;}
                Check(peak>.10f&&peak<=.45f&&energy>0,"Non-silent cue with headroom: "+cue);
                Check(Mathf.Abs(data[0])<.002f&&Mathf.Abs(data[data.Length-1])<.002f,"Smooth cue endpoints: "+cue);
            }
            Check(GameAudio.Clip(SoundCue.Click).length<.04f,"Simple click is under 40 milliseconds");
            CheckLevel(typeof(SpringAtmosphere),"andriig-spring-easter-day-music-507171");
            CheckLevel(typeof(AmbrosJourney.Summer.SummerLevel),"alex-morgan-soft-chill-background-music-548644");
            CheckLevel(typeof(AmbrosJourney.Fall.FallLevel),"atlasaudio-autumn-background-621525");
            CheckLevel(typeof(AmbrosJourney.Winter.WinterLevel),"zephiramusic-lofi-winter-610024");
            var probe=new GameObject("Sound click check");var ui=probe.AddComponent<JourneyUI>();ui.Initialize();int actions=0;
            var button=ui.Button(ui.Root,"Click check",new Rect(0,0,100,40),()=>actions++);int before=GameAudio.ClickPlayCount;
            button.onClick.Invoke();button.onClick.Invoke();Check(actions==2&&GameAudio.ClickPlayCount==before+1,"UI invokes action while duplicate click audio is suppressed");
            var host=GameObject.Find("Game sound effects");var voices=host.GetComponents<AudioSource>();
            bool paused=AudioListener.pause;AudioListener.pause=true;
            Check(GameAudio.Play(SoundCue.Pickup,.1f)&&voices[0].ignoreListenerPause&&voices[0].isPlaying,"UI/pickup cues remain audible in pause and certificate menus");AudioListener.pause=paused;
            Check(!voices[1].ignoreListenerPause,"World effects respect pause");
            GameAudio.SetMuted(true);Check(voices[0].mute&&voices[1].mute&&!GameAudio.Play(SoundCue.Click),"Mute silences all effect channels");
            var registered=probe.AddComponent<AudioSource>();GameAudio.Register(registered);Check(registered.mute,"New scene sources inherit mute");
            GameAudio.SetMuted(false);Check(!voices[0].mute&&!voices[1].mute&&!registered.mute,"Unmute restores registered channels");
            UnityEngine.Object.Destroy(probe);
            Debug.Log("SOUND_EFFECTS_PASS: eight short PCM cues, click under 40ms, waveform headroom/endpoints, four season music mappings, shared clip ownership, UI click deduplication, paused-menu audio and mute persistence.");
        }
        static void CheckLevel(Type type,string musicName)
        {
            var root=new GameObject("Season sound check");root.SetActive(false);var level=root.AddComponent(type);
            type.GetMethod("CreateAudio",Hidden).Invoke(level,null);
            var sources=root.GetComponents<AudioSource>();bool music=false;
            foreach(var source in sources)if(source.clip){Check(source.clip.name==musicName&&source.loop&&source.pitch==1,"Original music preserved: "+type.Name);music=true;}
            Check(music,"Music assigned: "+type.Name);
            var positive=type.GetField("good",Hidden);
            if(positive!=null)Check((AudioClip)positive.GetValue(level)==GameAudio.Clip(SoundCue.Correct),"Shared feedback cue: "+type.Name);
            var shared=GameAudio.Clip(SoundCue.Correct);UnityEngine.Object.DestroyImmediate(root);Check(shared==GameAudio.Clip(SoundCue.Correct),"Level cleanup preserves shared clips");
        }
    }
}
#endif
