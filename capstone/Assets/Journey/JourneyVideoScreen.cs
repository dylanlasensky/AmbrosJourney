// Presents original intro/outro videos consistently in native players and Web builds.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace AmbrosJourney.Journey
{
    // Present the decoder's own texture. Resizing the browser never replaces a camera target
    // or stretches a screen-sized RenderTexture that was allocated before fullscreen.
    public sealed class JourneyVideoScreen : MonoBehaviour
    {
        VideoPlayer video;
        RawImage picture;
        Text message;
        RectTransform viewport;
        bool initialized, backgroundWasEnabled, backgroundHeld;
#if UNITY_WEBGL && !UNITY_EDITOR
        bool focusReturned;
        [System.Runtime.InteropServices.DllImport("__Internal")]
        static extern int AmbrosReturnCinematicFocus();
#endif
        public Canvas Canvas { get; private set; }
        public void SetPictureOpacity(float alpha){if(picture)picture.color=new Color(1,1,1,Mathf.Clamp01(alpha));}

        public static JourneyVideoScreen Create(GameObject host, VideoPlayer player)
        {
            var screen = host.AddComponent<JourneyVideoScreen>();
            screen.video = player;
            screen.Canvas = host.AddComponent<Canvas>();
            screen.Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            screen.Canvas.sortingOrder = 32767;
            screen.viewport = host.GetComponent<RectTransform>();
            var background = new GameObject("Video letterbox", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(host.transform, false);
            var bounds = background.GetComponent<RectTransform>();
            bounds.anchorMin = Vector2.zero;
            bounds.anchorMax = Vector2.one;
            bounds.offsetMin = bounds.offsetMax = Vector2.zero;
            background.GetComponent<Image>().color = Color.black;
            var frame = new GameObject("Video picture", typeof(RectTransform), typeof(RawImage));
            frame.transform.SetParent(host.transform, false);
            screen.picture = frame.GetComponent<RawImage>();
            screen.picture.raycastTarget = false;
            screen.picture.enabled = false;
            var rect = frame.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            var caption = new GameObject("Video loading message", typeof(RectTransform), typeof(Text));
            caption.transform.SetParent(host.transform, false);
            var captionBounds = caption.GetComponent<RectTransform>();
            captionBounds.anchorMin = new Vector2(.05f, .35f);
            captionBounds.anchorMax = new Vector2(.95f, .65f);
            captionBounds.offsetMin = captionBounds.offsetMax = Vector2.zero;
            screen.message = caption.GetComponent<Text>();
            screen.message.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            screen.message.fontSize = 22;
            screen.message.alignment = TextAnchor.MiddleCenter;
            screen.message.raycastTarget = false;
            screen.message.gameObject.SetActive(false);
            player.renderMode = VideoRenderMode.APIOnly;
            player.targetCamera = null;
            player.targetTexture = null;
            screen.initialized = true;
            screen.KeepRendering();
            return screen;
        }

        public void SetMessage(string text)
        {
            if (!message) return;
            message.text = text ?? string.Empty;
            message.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        // Call before handing the screen to another scene/menu, never from generic teardown.
        public void ReturnFocusAfterPlayback()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!initialized || !backgroundHeld || focusReturned) return;
            focusReturned = true;
            AmbrosReturnCinematicFocus();
#endif
        }

        void KeepRendering()
        {
            if (backgroundHeld) return;
            backgroundWasEnabled = Application.runInBackground;
            backgroundHeld = true;
            // itch's fullscreen button is outside the game iframe. Losing keyboard focus
            // to that visible control must not suspend the video's rendering loop.
            Application.runInBackground = true;
        }
        void RestoreBackgroundPolicy()
        {
            if (!backgroundHeld) return;
            backgroundHeld = false;
            Application.runInBackground = backgroundWasEnabled;
        }
        void OnEnable()
        {
            UnityEngine.Canvas.preWillRenderCanvases -= RefreshFrame;
            UnityEngine.Canvas.preWillRenderCanvases += RefreshFrame;
            if (initialized) KeepRendering();
        }
        void OnDisable()
        {
            UnityEngine.Canvas.preWillRenderCanvases -= RefreshFrame;
            RestoreBackgroundPolicy();
        }
        void OnDestroy()
        {
            UnityEngine.Canvas.preWillRenderCanvases -= RefreshFrame;
            RestoreBackgroundPolicy();
        }
        void LateUpdate() => RefreshFrame();
        void RefreshFrame() => PresentFrame(video ? video.texture : null);

        void PresentFrame(Texture texture)
        {
            if (!picture || !viewport) return;
            // The decoder may replace its texture after a browser graphics-context resize.
            picture.texture = texture;
            picture.enabled = texture && texture.width > 0 && texture.height > 0;
            if (!picture.enabled) return;
            Vector2 size = viewport.rect.size;
            float scale = Mathf.Min(size.x / texture.width, size.y / texture.height);
            picture.rectTransform.sizeDelta = new Vector2(texture.width * scale, texture.height * scale);
        }
    }
}
