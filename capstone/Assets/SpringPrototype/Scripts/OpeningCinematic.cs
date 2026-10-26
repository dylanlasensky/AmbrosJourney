using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

// No movement or input is active in the opening scene. Playback ends in Spring.
[RequireComponent(typeof(VideoPlayer))]
public class OpeningCinematic : MonoBehaviour
{
    public VideoClip clip;
    VideoPlayer video;
    bool loading;
    string error;
    float elapsed;
    void Start()
    {
        GetComponent<VideoPlayer>().playOnAwake=false;
        GetComponent<VideoPlayer>().Stop();
        AmbrosJourney.Journey.JourneySession.Instance.OpenMenu(Begin);
    }
    public void Begin()
    {
        elapsed=0;error=null;loading=false;
        video=GetComponent<VideoPlayer>();video.clip=clip;video.renderMode=VideoRenderMode.CameraNearPlane;video.targetCamera=Object.FindAnyObjectByType<Camera>();video.targetCameraAlpha=1;video.aspectRatio=VideoAspectRatio.FitInside;video.playOnAwake=false;video.isLooping=false;video.waitForFirstFrame=true;
        video.audioOutputMode=VideoAudioOutputMode.Direct;video.SetDirectAudioVolume(0,.4f);
        video.loopPointReached+=Finished;video.errorReceived+=Failed;
        video.prepareCompleted+=Prepared;
        if(clip==null)error="Opening video is missing. Press Enter to begin Spring.";else video.Prepare();
    }
    void Prepared(VideoPlayer source){source.Play();Debug.Log("OPENING_CINEMATIC_STARTED");}
    void Failed(VideoPlayer source,string message){error="The opening could not play. Press Enter to begin Spring.";Debug.LogWarning("Opening video: "+message);}
    void Finished(VideoPlayer source){if(loading)return;loading=true;Debug.Log("OPENING_CINEMATIC_COMPLETE: entering Spring");SceneManager.LoadScene("Spring");}
    void Update()
    {
        if(video==null)return;
        elapsed+=Time.unscaledDeltaTime;
        if(video!=null && !video.isPrepared && elapsed>20 && error==null)error="The opening is taking too long to load. Press Enter to begin Spring.";
        if(error!=null && UnityEngine.InputSystem.Keyboard.current!=null && UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)Finished(video);
    }
    void OnGUI()
    {
        if(video==null || (video.isPrepared && error==null))return;
        GUI.color=Color.black;GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;
        var style=new GUIStyle(GUI.skin.label){fontSize=22,alignment=TextAnchor.MiddleCenter,wordWrap=true};GUI.Label(new Rect(30,Screen.height/2-50,Screen.width-60,100),error??"AMBROS' JOURNEY\nPreparing the opening...",style);
    }
    void OnDestroy(){if(video!=null){video.loopPointReached-=Finished;video.errorReceived-=Failed;video.prepareCompleted-=Prepared;}}
}
