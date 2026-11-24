using System.Collections;
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
    AmbrosJourney.Journey.JourneyUI disclaimer;
    bool beginning;
    public const string DisclaimerText="The opening video and some game artwork were created with AI. As you play, you'll learn how to use AI safely, check its answers and bring your own ideas to life.";
    void Start()
    {
        GetComponent<VideoPlayer>().playOnAwake=false;
        GetComponent<VideoPlayer>().Stop();
        ShowMenu();
    }
    public void Begin()
    {
        if(beginning)return;
        beginning=true;
        AmbrosJourney.Journey.JourneySession.Instance.SetCinematicVisible(true);
        StartCoroutine(ShowDisclaimer());
    }
    IEnumerator ShowDisclaimer()
    {
        var host=new GameObject("AI disclaimer");
        disclaimer=host.AddComponent<AmbrosJourney.Journey.JourneyUI>();disclaimer.Initialize();
        disclaimer.KeyboardNavigation=false;disclaimer.Canvas.sortingOrder=32767;
        var background=disclaimer.Panel(disclaimer.Root,"Black background",new Rect(-640,-360,1280,720),Color.black);
        background.anchorMin=Vector2.zero;background.anchorMax=Vector2.one;background.offsetMin=background.offsetMax=Vector2.zero;
        var group=host.AddComponent<CanvasGroup>();group.alpha=0;
        var title=disclaimer.Text(disclaimer.Root,"Disclaimer title","A NOTE ABOUT AI",new Rect(-450,-120,900,50),32,AmbrosJourney.Journey.JourneyMenu.Gold);
        title.alignment=TMPro.TextAlignmentOptions.Center;
        var body=disclaimer.Text(disclaimer.Root,"Disclaimer message",DisclaimerText,new Rect(-450,-40,900,180),26,AmbrosJourney.Journey.JourneyMenu.Cream);
        body.alignment=TMPro.TextAlignmentOptions.Center;
        for(float time=0;time<.5f;time+=Time.unscaledDeltaTime){group.alpha=Mathf.Clamp01(time/.5f);yield return null;}
        group.alpha=1;
        yield return new WaitForSecondsRealtime(7);
        for(float time=0;time<.5f;time+=Time.unscaledDeltaTime)
        {
            float alpha=1-Mathf.Clamp01(time/.5f);title.alpha=body.alpha=alpha;yield return null;
        }
        // The video screen supplies its own black backdrop while the decoder prepares.
        PlayOpening();Destroy(host);disclaimer=null;
    }
    IEnumerator FadeOpening()
    {
        for(float time=0;time<.65f;time+=Time.unscaledDeltaTime)
        {
            if(!presentation)yield break;
            presentation.SetPictureOpacity(Mathf.Clamp01(time/.65f));yield return null;
        }
        if(presentation)presentation.SetPictureOpacity(1);
    }
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
        presentation.SetPictureOpacity(0);
        presentation.SetMessage("AMBROS' JOURNEY\nPreparing the opening...");
        video.aspectRatio=VideoAspectRatio.FitInside;video.playOnAwake=false;video.isLooping=false;video.waitForFirstFrame=true;video.timeUpdateMode=VideoTimeUpdateMode.UnscaledGameTime;
        video.audioOutputMode=VideoAudioOutputMode.Direct;video.SetDirectAudioVolume(0,.4f);video.SetDirectAudioMute(0,AmbrosJourney.Audio.GameAudio.Muted||!AmbrosJourney.Audio.GameAudio.MusicEnabled);
        video.loopPointReached+=Finished;video.errorReceived+=Failed;
        video.prepareCompleted+=Prepared;
        video.Prepare();
    }
    void Prepared(VideoPlayer source){error=null;elapsed=0;presentation.SetMessage(null);source.Play();StartCoroutine(FadeOpening());Debug.Log("OPENING_CINEMATIC_STARTED");}
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
    void OnDestroy(){if(disclaimer)Destroy(disclaimer.gameObject);if(screen)Destroy(screen);if(video!=null){video.loopPointReached-=Finished;video.errorReceived-=Failed;video.prepareCompleted-=Prepared;}}
}
