using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using AmbrosJourney.Winter;

namespace AmbrosJourney.Presentation
{
    // Uses the existing conversation frame, font sizes and colors. Nothing is graded until Check.
    public sealed class FinalActivityPanel : MonoBehaviour
    {
        SeasonOverlay overlay;FinalChallenge challenge;Action<int[]> submitted;
        RectTransform area;TMP_Text instruction;int selected=-1;int[] response;
        readonly HashSet<int> picked=new HashSet<int>();
        readonly List<UnityEngine.UI.Button> cards=new List<UnityEngine.UI.Button>(),slots=new List<UnityEngine.UI.Button>();
        UnityEngine.UI.Button check;
        public bool IsOpen=>area&&area.gameObject.activeSelf;
        public int[] Response=>challenge.Kind==FinalActivityKind.SelectMany?picked.OrderBy(x=>x).ToArray():(int[])response.Clone();
        bool Available=>IsOpen&&!AmbrosJourney.Journey.JourneySession.Blocked;
        public void Present(SeasonOverlay ui,FinalChallenge task,Action<int[]> onSubmit)
        {
            Hide();overlay=ui;challenge=task;submitted=onSubmit;picked.Clear();cards.Clear();slots.Clear();selected=-1;
            response=Enumerable.Repeat(-1,task.Kind==FinalActivityKind.Choose?1:task.Kind==FinalActivityKind.SelectMany?0:task.Expected.Length).ToArray();
            area=overlay.BeginActivity(350);instruction=Label("Instructions",task.Instruction,new Rect(0,0,850,42),19);
            if(task.Kind==FinalActivityKind.Arrange)
            {
                for(int i=0;i<task.Choices.Length;i++)
                {
                    int index=i;var b=Button("Card "+i,task.Choices[i],new Rect(0,48+i*55,407,48),()=>Select(index));cards.Add(b);
                    b.gameObject.AddComponent<FinalDragCard>().Initialize(this,index);
                    int slot=i;var target=Button("Space "+i,(i+1)+". Drop a card here",new Rect(429,48+i*55,421,48),()=>Place(slot));slots.Add(target);
                    target.gameObject.AddComponent<FinalDropSlot>().Initialize(this,slot);
                }
            }
            else if(task.Kind==FinalActivityKind.Match)
            {
                for(int i=0;i<task.Choices.Length;i++)
                {
                    int index=i;Label("Situation "+i,task.Choices[i],new Rect(0,48+i*73,407,65),18);
                    cards.Add(Button("Match "+i,"Choose a step",new Rect(429,48+i*73,421,65),()=>Select(index)));
                }
            }
            else
            {
                float height=task.Choices.Length>4?43:52;
                for(int i=0;i<task.Choices.Length;i++){int index=i;cards.Add(Button("Select "+i,task.Choices[i],new Rect(0,48+i*(height+5),850,height),()=>Select(index)));}
            }
            check=Button("Check activity","Check answer",new Rect(620,300,230,40),Submit);
            Label("Activity hint",task.Kind==FinalActivityKind.Arrange?"1-4: pick a card, then a space. Backspace: clear.":"Number keys change your choices. Enter: check.",new Rect(0,302,604,44),14);
            Refresh();
        }
        public void Select(int index)
        {
            if(!Available||index<0||index>=challenge.Choices.Length)return;
            if(challenge.Kind==FinalActivityKind.Arrange)selected=index;
            else if(challenge.Kind==FinalActivityKind.Match)response[index]=(response[index]+1)%challenge.Categories.Length;
            else if(challenge.Kind==FinalActivityKind.SelectMany){if(!picked.Add(index))picked.Remove(index);}
            else response[0]=index;
            Refresh();
        }
        public void Place(int slot)
        {
            if(!Available||challenge.Kind!=FinalActivityKind.Arrange||selected<0||slot<0||slot>=response.Length)return;
            int old=Array.IndexOf(response,selected);if(old>=0)response[old]=response[slot];response[slot]=selected;selected=-1;Refresh();
        }
        public void Drop(int card,int slot){Select(card);Place(slot);}
        public void ClearArrangement(){if(!Available)return;for(int i=0;i<response.Length;i++)response[i]=-1;selected=-1;Refresh();}
        public void Submit()
        {
            if(!Available)return;bool right;var values=Response;
            if(!challenge.TryEvaluate(values,out right)){instruction.text="Finish your choices before checking.";return;}
            var callback=submitted;submitted=null;Hide();callback?.Invoke(values);
        }
        public void HandleKeyboard(Keyboard k)
        {
            if(!Available||k==null)return;int index=k.digit1Key.wasPressedThisFrame?0:k.digit2Key.wasPressedThisFrame?1:k.digit3Key.wasPressedThisFrame?2:k.digit4Key.wasPressedThisFrame?3:k.digit5Key.wasPressedThisFrame?4:-1;
            if(index>=0){if(challenge.Kind==FinalActivityKind.Arrange&&selected>=0)Place(index);else Select(index);}
            if(k.backspaceKey.wasPressedThisFrame&&challenge.Kind==FinalActivityKind.Arrange)ClearArrangement();
            if(k.enterKey.wasPressedThisFrame)Submit();
        }
        void Refresh()
        {
            for(int i=0;i<cards.Count;i++)
            {
                string label=challenge.Choices[i];bool marked=false;
                if(challenge.Kind==FinalActivityKind.Match){label=response[i]<0?"Choose a step":challenge.Categories[response[i]];marked=response[i]>=0;}
                else if(challenge.Kind==FinalActivityKind.Arrange)marked=selected==i||Array.IndexOf(response,i)>=0;
                else {marked=challenge.Kind==FinalActivityKind.Choose?response[0]==i:picked.Contains(i);label=(marked?"[x] ":"[ ] ")+label;}
                cards[i].GetComponentInChildren<TMP_Text>().text=label;cards[i].GetComponent<UnityEngine.UI.Image>().color=marked?new Color(.33f,.47f,.39f):new Color(.19f,.32f,.42f);
            }
            for(int i=0;i<slots.Count;i++)slots[i].GetComponentInChildren<TMP_Text>().text=(i+1)+". "+(response[i]<0?"Drop a card here":challenge.Choices[response[i]]);
            bool ignored;check.interactable=challenge.TryEvaluate(Response,out ignored);
        }
        TMP_Text Label(string name,string text,Rect bounds,int size)
        {
            var o=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));o.transform.SetParent(area,false);var t=o.GetComponent<TextMeshProUGUI>();Position(t.rectTransform,bounds);t.font=overlay.Copy.font;t.fontSize=size;t.color=overlay.Copy.color;t.text=text;t.richText=false;t.textWrappingMode=TextWrappingModes.Normal;t.raycastTarget=false;return t;
        }
        UnityEngine.UI.Button Button(string name,string text,Rect bounds,Action action)
        {
            var o=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button));o.transform.SetParent(area,false);Position(o.GetComponent<RectTransform>(),bounds);
            var face=o.GetComponent<UnityEngine.UI.Image>();face.sprite=overlay.Choices[0].GetComponent<UnityEngine.UI.Image>().sprite;face.type=UnityEngine.UI.Image.Type.Sliced;face.color=new Color(.19f,.32f,.42f);face.raycastTarget=true;
            var button=o.GetComponent<UnityEngine.UI.Button>();button.targetGraphic=face;button.onClick.AddListener(()=>action());button.navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.None};
            var label=Label(name+" label",text,new Rect(10,3,bounds.width-20,bounds.height-6),18);label.transform.SetParent(o.transform,false);Position(label.rectTransform,new Rect(10,3,bounds.width-20,bounds.height-6));label.color=new Color(1,.96f,.83f);return button;
        }
        static void Position(RectTransform rect,Rect bounds){rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.up;rect.anchoredPosition=new Vector2(bounds.x,-bounds.y);rect.sizeDelta=bounds.size;}
        public void Hide(){if(area){var old=area;area=null;old.gameObject.SetActive(false);old.SetParent(null,false);Destroy(old.gameObject);}if(overlay)overlay.EndActivity();}
    }
    public sealed class FinalDragCard : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        FinalActivityPanel owner;int index;Vector2 home;CanvasGroup group;RectTransform rect;int sibling;bool dragging;
        public void Initialize(FinalActivityPanel panel,int card){owner=panel;index=card;rect=(RectTransform)transform;group=gameObject.AddComponent<CanvasGroup>();}
        public void OnBeginDrag(PointerEventData e){if(AmbrosJourney.Journey.JourneySession.Blocked)return;home=rect.anchoredPosition;sibling=rect.GetSiblingIndex();rect.SetAsLastSibling();dragging=true;group.blocksRaycasts=false;owner.Select(index);}
        public void OnDrag(PointerEventData e){if(!dragging||AmbrosJourney.Journey.JourneySession.Blocked)return;rect.anchoredPosition+=e.delta/GetComponentInParent<Canvas>().scaleFactor;}
        public void OnEndDrag(PointerEventData e){if(!dragging)return;rect.anchoredPosition=home;rect.SetSiblingIndex(sibling);dragging=false;group.blocksRaycasts=true;}
    }
    public sealed class FinalDropSlot : MonoBehaviour,IDropHandler
    {
        FinalActivityPanel owner;int slot;
        public void Initialize(FinalActivityPanel panel,int space){owner=panel;slot=space;}
        public void OnDrop(PointerEventData e){if(e.pointerDrag){var card=e.pointerDrag.GetComponent<FinalDragCard>();if(card)owner.Place(slot);}}
    }
}
