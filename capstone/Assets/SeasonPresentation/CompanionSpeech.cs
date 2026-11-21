using TMPro;
using UnityEngine;
using AmbrosJourney.Journey;

namespace AmbrosJourney.Presentation
{
    // A guide can speak while the player moves. Captions never own input or lesson state.
    [DefaultExecutionOrder(500)]
    public sealed class CompanionSpeech : MonoBehaviour
    {
        SeasonOverlay owner;
        RectTransform panel;
        TextMeshProUGUI speaker,copy;
        CanvasGroup group;
        float remaining,visibleTime;
        public bool Visible=>panel&&panel.gameObject.activeSelf;
        public float RemainingSeconds=>remaining;
        public static CompanionSpeech Say(SeasonOverlay overlay,string name,string message)
        {
            if(!overlay)return null;
            var speech=overlay.GetComponent<CompanionSpeech>();
            if(!speech){speech=overlay.gameObject.AddComponent<CompanionSpeech>();speech.Initialize(overlay);}
            speech.speaker.text=name+"  /  J: field notes";speech.copy.text=message;
            speech.remaining=Mathf.Clamp(message.Length/14f,8,12);speech.visibleTime=0;
            speech.Advance(0);return speech;
        }
        public static void Clear(SeasonOverlay overlay)
        {
            if(!overlay)return;
            var speech=overlay.GetComponent<CompanionSpeech>();
            if(speech){speech.remaining=0;speech.panel.gameObject.SetActive(false);}
        }
        void Initialize(SeasonOverlay overlay)
        {
            owner=overlay;
            var host=new GameObject("Companion caption",typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(CanvasGroup));
            host.transform.SetParent(overlay.transform,false);panel=host.GetComponent<RectTransform>();
            panel.anchorMin=panel.anchorMax=new Vector2(.5f,0);panel.pivot=new Vector2(.5f,0);
            panel.anchoredPosition=new Vector2(0,86);panel.sizeDelta=new Vector2(700,118);
            group=host.GetComponent<CanvasGroup>();group.interactable=false;group.blocksRaycasts=false;
            var source=overlay.Dialog.transform.Find("Parchment").GetComponent<UnityEngine.UI.Image>();
            var face=host.GetComponent<UnityEngine.UI.Image>();face.sprite=source.sprite;face.type=source.type;face.pixelsPerUnitMultiplier=source.pixelsPerUnitMultiplier;face.color=source.color;face.raycastTarget=false;
            speaker=Label("Companion speaker",new Vector2(18,-12),new Vector2(664,24),16,overlay.Heading);
            copy=Label("Companion guidance",new Vector2(18,-40),new Vector2(664,66),20,overlay.Copy);
            copy.enableAutoSizing=true;copy.fontSizeMin=18;copy.fontSizeMax=20;
            panel.gameObject.SetActive(false);
        }
        TextMeshProUGUI Label(string label,Vector2 position,Vector2 size,float fontSize,TextMeshProUGUI reference)
        {
            var item=new GameObject(label,typeof(RectTransform),typeof(TextMeshProUGUI));item.transform.SetParent(panel,false);
            var text=item.GetComponent<TextMeshProUGUI>();var r=text.rectTransform;r.anchorMin=r.anchorMax=r.pivot=Vector2.up;r.anchoredPosition=position;r.sizeDelta=size;
            text.font=reference.font;text.fontSize=fontSize;text.color=reference.color;text.raycastTarget=false;text.textWrappingMode=TextWrappingModes.Normal;return text;
        }
        public void Advance(float elapsed)
        {
            if(!panel||!owner)return;
            bool hidden=SeasonTransition.Active||JourneySession.Blocked||!owner.Canvas.enabled||owner.Dialog.activeSelf||owner.Journal.activeSelf;
            panel.gameObject.SetActive(!hidden&&remaining>0);
            if(hidden||remaining<=0)return;
            elapsed=Mathf.Max(0,elapsed);remaining=Mathf.Max(0,remaining-elapsed);visibleTime+=elapsed;
            group.alpha=Mathf.Min(Mathf.Clamp01(visibleTime/.15f),Mathf.Clamp01(remaining/.4f));
            if(remaining<=0)panel.gameObject.SetActive(false);
        }
        void LateUpdate()=>Advance(Time.unscaledDeltaTime);
    }
}
