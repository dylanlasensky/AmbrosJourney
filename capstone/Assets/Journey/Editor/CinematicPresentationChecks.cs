#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using AmbrosJourney.Journey;

public static class CinematicPresentationChecks
{
    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static void Run()
    {
        var host = new GameObject("Video presentation test");
        var cameraHost = new GameObject("Former film camera");
        var wide = new Texture2D(16, 9);
        var tall = new Texture2D(9, 16);
        var oldTarget = new RenderTexture(640, 480, 0);
        bool originalBackground = Application.runInBackground;
        try
        {
            Application.runInBackground = false;
            var video = host.AddComponent<VideoPlayer>();
            video.playOnAwake = false;
            video.renderMode = VideoRenderMode.CameraNearPlane;
            video.targetCamera = cameraHost.AddComponent<Camera>();
            video.targetTexture = oldTarget;
            var screen = JourneyVideoScreen.Create(host, video);
            Check(Application.runInBackground, "Visible cinematic must keep rendering when the itch fullscreen control takes focus");
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            typeof(JourneyVideoScreen).GetMethod("OnDisable", flags).Invoke(screen, null);
            Check(!Application.runInBackground, "Disabling a cinematic must restore gameplay background policy");
            typeof(JourneyVideoScreen).GetMethod("OnEnable", flags).Invoke(screen, null);
            Check(Application.runInBackground, "Re-enabled cinematic must reacquire rendering");
            Check(video.renderMode == VideoRenderMode.APIOnly && !video.targetCamera && !video.targetTexture,
                "Film must not retain a camera or a screen-sized render target");
            Check(screen.Canvas.renderMode == RenderMode.ScreenSpaceOverlay && screen.Canvas.sortingOrder == 32767,
                "Film must remain above the world independently of its cameras");
            var picture = host.GetComponentInChildren<RawImage>(true);
            var present = typeof(JourneyVideoScreen).GetMethod("PresentFrame", BindingFlags.NonPublic | BindingFlags.Instance);
            // WorldSpace permits deterministic viewport sizes without changing the user's display.
            screen.Canvas.renderMode = RenderMode.WorldSpace;
            var viewport = host.GetComponent<RectTransform>();
            foreach (var size in new[] {new Vector2(1280,720), new Vector2(2560,1600), new Vector2(720,1280), new Vector2(1024,768), new Vector2(1280,720)})
            {
                viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
                present.Invoke(screen, new object[] {wide});
                var frame = picture.rectTransform.rect;
                Check(picture.enabled && picture.texture == wide, "Resize must preserve the current video texture");
                Check(Mathf.Abs(frame.width / frame.height - 16f / 9f) < .001f, "Resize must preserve the entire film's aspect ratio");
                Check(frame.width <= size.x + .01f && frame.height <= size.y + .01f,
                    "Video must stay inside fullscreen, portrait and embedded viewports");
                Check(Mathf.Abs(frame.width-size.x) < .01f || Mathf.Abs(frame.height-size.y) < .01f,
                    "Film must use all available space on at least one axis");
            }
            present.Invoke(screen, new object[] {tall});
            Check(picture.texture == tall && Mathf.Abs(picture.rectTransform.rect.width / picture.rectTransform.rect.height - 9f/16f) < .001f,
                "A decoder texture replacement must bind and refit immediately");
            present.Invoke(screen, new object[] {null});
            Check(!picture.enabled && host.GetComponentInChildren<Image>().color == Color.black,
                "No decoded frame must show the black letterbox, never a white texture");
            typeof(JourneyVideoScreen).GetMethod("OnDestroy", flags).Invoke(screen, null);
            Check(!Application.runInBackground, "Scene teardown must release cinematic background rendering");
            Directory.CreateDirectory("Logs/CinematicChecks");
            File.WriteAllText("Logs/CinematicChecks/Presentation.txt", "PASS: focus-safe render lifetime restored on disable and destruction; camera/target detached; embedded, fullscreen and portrait sizes fit; texture replacement and missing-frame handling passed. Browser fullscreen playback still requires a Web build test.");
            Debug.Log("CINEMATIC_PRESENTATION_PASS");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(host);
            UnityEngine.Object.DestroyImmediate(cameraHost);
            UnityEngine.Object.DestroyImmediate(wide);
            UnityEngine.Object.DestroyImmediate(tall);
            UnityEngine.Object.DestroyImmediate(oldTarget);
            Application.runInBackground = originalBackground;
        }
    }
}
#endif
