using UnityEngine;
using UnityEngine.Video;

// Name entry -> supplied opening -> loading -> Spring.
[RequireComponent(typeof(VideoPlayer))]
public class OpeningCinematic : MonoBehaviour
{
    public VideoClip clip;
    VideoPlayer video;
    GameObject screen;
    AmbrosJourney.Journey.JourneyVideoScreen presentation;
    bool loading;
    string error;
    float elapsed;
    void Start()
    {
        GetComponent<VideoPlayer>().playOnAwake=false;
        GetComponent<VideoPlayer>().Stop();
        ShowMenu();
    }
    public void Begin()=>PlayOpening();
    void ShowMenu()
    {
        var session=AmbrosJourney.Journey.JourneySession.Instance;
        session.SetCinematicVisible(false);session.OpenMenu(Begin);
    }
    void PlayOpening()
    {
        elapsed=0;error=null;loading=false;
        AmbrosJourney.Journey.JourneySession.Instance.SetCinematicVisible(true);
        video=GetComponent<VideoPlayer>();
#if UNITY_WEBGL && !UNITY_EDITOR
        video.source=VideoSource.Url;video.url=Application.streamingAssetsPath+"/JourneyVideos/Intro.mp4";
#else
        if(!clip)clip=Resources.Load<VideoClip>("Journey/Intro");
        if(!clip){Finished(null);return;}
        video.clip=clip;
#endif
        screen=new GameObject("Opening video screen",typeof(RectTransform));
        presentation=AmbrosJourney.Journey.JourneyVideoScreen.Create(screen,video);
        presentation.SetMessage("AMBROS' JOURNEY\nPreparing the opening...");
        video.aspectRatio=VideoAspectRatio.FitInside;video.playOnAwake=false;video.isLooping=false;video.waitForFirstFrame=true;video.timeUpdateMode=VideoTimeUpdateMode.UnscaledGameTime;
        video.audioOutputMode=VideoAudioOutputMode.Direct;video.SetDirectAudioVolume(0,.4f);
        video.loopPointReached+=Finished;video.errorReceived+=Failed;
        video.prepareCompleted+=Prepared;
        video.Prepare();
    }
    void Prepared(VideoPlayer source){error=null;elapsed=0;presentation.SetMessage(null);source.Play();Debug.Log("OPENING_CINEMATIC_STARTED");}
    void Failed(VideoPlayer source,string message){error="The opening could not play. Press Enter to begin Spring.";Debug.LogWarning("Opening video: "+message);}
    void Finished(VideoPlayer source){if(loading)return;loading=true;Debug.Log("OPENING_CINEMATIC_COMPLETE: loading Spring");if(video){video.loopPointReached-=Finished;video.errorReceived-=Failed;video.prepareCompleted-=Prepared;video.Stop();video=null;}if(presentation)presentation.ReturnFocusAfterPlayback();if(screen){Destroy(screen);screen=null;}var session=AmbrosJourney.Journey.JourneySession.Instance;session.SetCinematicVisible(false);session.LoadChapter(1);}
    void Update()
    {
        if(video==null)return;
        elapsed+=Time.unscaledDeltaTime;
        if(video!=null && !video.isPrepared && elapsed>20 && error==null)error="The opening is taking too long to load. Press Enter to begin Spring.";
        if(error!=null && presentation)presentation.SetMessage(error);
        if(error!=null && UnityEngine.InputSystem.Keyboard.current!=null && UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)Finished(video);
    }
    void OnDestroy(){if(screen)Destroy(screen);if(video!=null){video.loopPointReached-=Finished;video.errorReceived-=Failed;video.prepareCompleted-=Prepared;}}
}
