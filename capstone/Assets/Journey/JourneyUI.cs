using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
namespace AmbrosJourney.Journey
{
    public sealed class JourneyUI : MonoBehaviour
    {
        Sprite rounded;
        public Canvas Canvas;public RectTransform Root;public TMP_FontAsset Font;
        public static readonly Color Ink=new Color(.16f,.24f,.25f),Gold=new Color(.80f,.63f,.32f),Paper=new Color(.97f,.93f,.82f),Green=new Color(.16f,.30f,.28f);
        public void Initialize()
        {
            rounded=AmbrosJourney.Presentation.SeasonOverlay.Rounded();Root=gameObject.AddComponent<RectTransform>();Canvas=gameObject.AddComponent<Canvas>();Canvas.renderMode=RenderMode.ScreenSpaceOverlay;Canvas.sortingOrder=31000;
            var scaler=gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.screenMatchMode=UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();Font=Resources.Load<TMP_FontAsset>("SummerDefault");
            if(!EventSystem.current){var e=new GameObject("Journey EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));e.transform.SetParent(transform,false);}
        }
        public RectTransform Panel(Transform parent,string name,Rect bounds,Color color)
        {
            var obj=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));obj.transform.SetParent(parent,false);var r=obj.GetComponent<RectTransform>();Place(r,bounds);obj.GetComponent<UnityEngine.UI.Image>().color=color;return r;
        }
        public static void Place(RectTransform r,Rect b){r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(b.x,-b.y);r.sizeDelta=b.size;}
        public TextMeshProUGUI Text(Transform parent,string name,string text,Rect b,float size=22,Color? color=null)
        {
            var obj=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));obj.transform.SetParent(parent,false);var t=obj.GetComponent<TextMeshProUGUI>();Place(t.rectTransform,b);t.font=Font;t.text=text;t.fontSize=size;t.color=color??Ink;t.richText=false;t.textWrappingMode=TextWrappingModes.Normal;t.raycastTarget=false;return t;
        }
        public UnityEngine.UI.Button Button(Transform parent,string label,Rect b,Action action)
        {
            var r=Panel(parent,label,b,new Color(.23f,.40f,.36f));
            var face=r.GetComponent<UnityEngine.UI.Image>();face.sprite=rounded;face.type=UnityEngine.UI.Image.Type.Sliced;var outline=r.gameObject.AddComponent<UnityEngine.UI.Outline>();outline.effectColor=Gold;outline.effectDistance=new Vector2(1,-1);var button=r.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=r.GetComponent<UnityEngine.UI.Image>();button.onClick.AddListener(()=>action());
            var colors=button.colors;colors.highlightedColor=new Color(1.25f,1.18f,.94f);colors.selectedColor=colors.highlightedColor;button.colors=colors;
            var t=Text(r,"Label",label,new Rect(-b.width/2+12,-b.height/2+2,b.width-24,b.height-4),19,Paper);t.alignment=TextAlignmentOptions.Center;return button;
        }
        public TMP_InputField NameInput(Transform parent,string initial,Rect b)
        {
            var r=Panel(parent,"Player name",b,Color.white);var field=r.gameObject.AddComponent<TMP_InputField>();
            var t=Text(r,"Name",initial,new Rect(-b.width/2+15,-b.height/2+9,b.width-30,b.height-18),24);t.raycastTarget=true;
            var placeholder=Text(r,"Placeholder","Your explorer name",new Rect(-b.width/2+15,-b.height/2+9,b.width-30,b.height-18),24,new Color(.4f,.47f,.45f));
            field.textViewport=r;field.textComponent=t;field.placeholder=placeholder;field.characterLimit=32;field.lineType=TMP_InputField.LineType.SingleLine;field.text=initial;return field;
        }
        void OnDestroy(){if(rounded){Destroy(rounded.texture);Destroy(rounded);}}
        public void Clear(){for(int i=transform.childCount-1;i>=0;i--){var c=transform.GetChild(i);if(!c.GetComponent<EventSystem>()){c.gameObject.SetActive(false);Destroy(c.gameObject);}}}
    }
}
