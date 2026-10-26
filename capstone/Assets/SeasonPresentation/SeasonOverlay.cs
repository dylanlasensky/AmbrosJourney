using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace AmbrosJourney.Presentation
{
    // Spring's 1280x720 layout, rounded parchment and gold compass, shared by later seasons.
    public sealed class SeasonOverlay : MonoBehaviour
    {
        public Canvas Canvas;
        public GameObject Dialog, Journal;
        public TextMeshProUGUI Heading, Copy, Objective, Prompt, JournalCopy, Party, CompassText;
        public readonly List<UnityEngine.UI.Button> Choices = new List<UnityEngine.UI.Button>();
        public Vector2 CompassDelta { get; private set; }
        public float ObjectiveDistance { get; private set; }
        public RectTransform Arrow { get; private set; }
        public bool Asking { get; private set; }
        RectTransform root, dialogueRect, headerRect, bodyRect, objectiveRect, context, controls, compassBox, worldLabels;
        UnityEngine.UI.Image dialogFill;
        TMP_FontAsset font;int season;
        Sprite rounded, arrowSprite;
        Color ink, accent, paper = new Color(.98f,.94f,.81f), border = new Color(.55f,.35f,.22f);
        readonly Dictionary<SeasonWorldLabel,RectTransform> labels = new Dictionary<SeasonWorldLabel,RectTransform>();
        float openedAt, feedbackUntil; bool positive;
        public void Initialize(TMP_FontAsset typeface, bool autumn) => Initialize(typeface, autumn ? 3 : 2);
        public void Initialize(TMP_FontAsset typeface, int chapter)
        {
            season=chapter;bool autumn=chapter==3;
            font=typeface; ink=autumn?new Color(.25f,.20f,.14f):new Color(.19f,.29f,.23f);
            accent=autumn?new Color(.43f,.26f,.16f):new Color(.23f,.39f,.30f);
            if(chapter==4){ink=new Color(.16f,.23f,.30f);accent=new Color(.19f,.32f,.42f);}
            rounded=Rounded(); arrowSprite=ArrowSprite();
            root=gameObject.AddComponent<RectTransform>();Canvas=gameObject.AddComponent<Canvas>();Canvas.renderMode=RenderMode.ScreenSpaceOverlay;Canvas.sortingOrder=100;
            var scaler=gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.screenMatchMode=UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            if(!EventSystem.current){var e=new GameObject("Season EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));e.transform.SetParent(transform,false);}
            var title=Panel("Chapter",transform,Vector2.up,new Rect(24,24,290,83));
            Text("Title",title,new Rect(18,10,260,32),new[]{"Spring Valley","Summer Valley","Emberwood","Winter / Frostveil"}[chapter-1],25,true);
            Text("Chapter",title,new Rect(18,48,260,27),new[]{"01  /  THE AWAKENING","02  /  INDEPENDENCE","03  /  PROMPT FORGE","04  /  SEEK THE EVIDENCE"}[chapter-1],14);
            var tracker=Panel("Objectives",transform,Vector2.one,new Rect(-428,24,404,100));
            Objective=Text("Objective",tracker,new Rect(18,37,365,57),"",19);objectiveRect=Objective.rectTransform;
            // Kept as one text field for the established level contract; split visually in LateUpdate.
            trackerText=Text("Progress",tracker,new Rect(18,10,365,26),"",14);
            compassBox=Panel("Compass",transform,Vector2.one,new Rect(-112,130,88,100));
            var arrowObject=new GameObject("Golden direction arrow",typeof(RectTransform),typeof(UnityEngine.UI.Image));arrowObject.transform.SetParent(compassBox,false);Arrow=arrowObject.GetComponent<RectTransform>();Place(Arrow,Vector2.up,new Rect(27,27,34,34));Arrow.pivot=new Vector2(.5f,.5f);Arrow.anchoredPosition=new Vector2(44,-46);var image=arrowObject.GetComponent<UnityEngine.UI.Image>();image.sprite=arrowSprite;image.raycastTarget=false;
            CompassText=Text("Distance",compassBox,new Rect(8,68,76,25),"",14);CompassText.alignment=TextAlignmentOptions.Center;
            controls=Panel("Controls",transform,Vector2.zero,new Rect(24,-62,780,38));
            Text("Controls",controls,new Rect(16,6,750,27),"WASD / Arrows  Move    E  Interact    1–3  Answer    J  Journal    R  Return",14);
            context=Panel("Interaction prompt",transform,new Vector2(.5f,0),new Rect(-190,-120,380,42));Prompt=Text("Prompt",context,new Rect(18,5,345,32),"",19);
            // Party membership belongs in the journal, rather than floating over the scenery.
            Party=Text("Party",transform,new Rect(0,0,1,1),"",14);Party.gameObject.SetActive(false);
            var labelLayer=new GameObject("Landmark labels",typeof(RectTransform));labelLayer.transform.SetParent(transform,false);worldLabels=labelLayer.GetComponent<RectTransform>();Stretch(worldLabels);
            dialogueRect=Panel("Conversation",transform,new Vector2(.5f,0),new Rect(-450,-355,900,330));Dialog=dialogueRect.gameObject;dialogFill=dialogueRect.Find("Parchment").GetComponent<UnityEngine.UI.Image>();
            Heading=Text("Speaker",dialogueRect,new Rect(24,15,850,38),"",25,true);headerRect=Heading.rectTransform;
            Copy=Text("Dialogue",dialogueRect,new Rect(24,61,850,145),"",19);bodyRect=Copy.rectTransform;
            for(int i=0;i<3;i++)
            {
                var b=new GameObject("Choice "+(i+1),typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button));b.transform.SetParent(dialogueRect,false);var r=b.GetComponent<RectTransform>();Place(r,Vector2.up,new Rect(24,191+i*53,850,46));
                var face=b.GetComponent<UnityEngine.UI.Image>();face.sprite=rounded;face.type=UnityEngine.UI.Image.Type.Sliced;face.color=accent;
                var button=b.GetComponent<UnityEngine.UI.Button>();button.targetGraphic=face;var colors=button.colors;colors.highlightedColor=new Color(1.18f,1.18f,1.18f);colors.pressedColor=new Color(.85f,.85f,.85f);button.colors=colors;button.navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.None};
                var label=Text("Label",r,new Rect(16,4,818,38),"",18);label.color=new Color(1,.96f,.83f);label.alignment=TextAlignmentOptions.MidlineLeft;Choices.Add(button);
            }
            closeHint=Text("Close hint",dialogueRect,new Rect(24,286,560,29),"ESC  Close conversation",14);
            var journalRect=Panel("Journal",transform,new Vector2(.5f,.5f),new Rect(-450,-275,900,550));Journal=journalRect.gameObject;
            Text("Journal title",journalRect,new Rect(25,15,850,40),"Ambros' Field Journal",25,true);
            JournalCopy=Text("Notes",journalRect,new Rect(25,70,850,430),"",19);
            journalParty=Text("Journal party",journalRect,new Rect(25,493,850,24),"",14);
            Text("Journal hint",journalRect,new Rect(25,522,850,24),"J / ESC  Return to the trail",14);
            Dialog.SetActive(false);Journal.SetActive(false);
        }
        TextMeshProUGUI trackerText,closeHint,journalParty;
        readonly List<TextMeshProUGUI> journalTitles=new List<TextMeshProUGUI>(),journalNotes=new List<TextMeshProUGUI>();
        public void SetJournal(string[] titles,string[] notes,string party)
        {
            JournalCopy.gameObject.SetActive(false);journalParty.text=party;
            if(journalTitles.Count==0)
            {
                var panel=Journal.transform;
                for(int i=0;i<3;i++)
                {
                    journalTitles.Add(Text("Lesson title "+i,panel,new Rect(25,70+i*140,850,32),"",25,true));
                    var body=Text("Lesson note "+i,panel,new Rect(25,106+i*140,850,100),"",19);body.enableAutoSizing=true;body.fontSizeMin=17;body.fontSizeMax=19;journalNotes.Add(body);
                }
            }
            for(int i=0;i<3;i++){journalTitles[i].text=titles[i];journalNotes[i].text=notes[i];}
        }
        public void SetCompass(Vector2 player,Vector2 waypoint,Vector2 destination)
        {
            bool assisted=AmbrosJourney.Journey.JourneySession.Instance&&AmbrosJourney.Journey.JourneySession.Instance.Data.assistedGuidance;
            bool distant=Vector2.Distance(player,destination)>14;
            bool whiteout=season==4&&player.x<80&&Mathf.Repeat(Time.time,48)>26;
            Arrow.gameObject.SetActive(assisted||!whiteout);
            if(!assisted&&season>=3&&distant)waypoint=destination;
            CompassDelta=waypoint-player;ObjectiveDistance=Vector2.Distance(player,destination);
            Arrow.localEulerAngles=new Vector3(0,0,Mathf.Atan2(CompassDelta.y,CompassDelta.x)*Mathf.Rad2Deg);
            CompassText.text=!assisted&&whiteout?"Snowfall":!assisted&&season>=3&&distant?"Explore":Mathf.CeilToInt(ObjectiveDistance)+" steps";
        }
        public void Present(bool asking)
        {
            Asking=asking;openedAt=Time.unscaledTime;
            LayoutDialogue();
        }
        public void Feedback(bool right){positive=right;feedbackUntil=Time.unscaledTime+.55f;}
        void LayoutDialogue()
        {
            if(!Copy)return;
            float textHeight=Mathf.Max(Asking?90:64,Copy.GetPreferredValues(Copy.text,850,0).y+8);
            textHeight=Mathf.Min(250,textHeight);
            int count=0;foreach(var b in Choices)if(b.gameObject.activeSelf)count++;
            float choiceHeight=46;
            if(Asking)foreach(var b in Choices)if(b.gameObject.activeSelf)choiceHeight=Mathf.Max(choiceHeight,b.GetComponentInChildren<TextMeshProUGUI>().GetPreferredValues(818,0).y+16);
            float height=Asking?Mathf.Max(300,61+textHeight+14+count*(choiceHeight+7)+40):Mathf.Max(220,61+textHeight+78);
            float top=root.rect.height-25-height;
            Place(dialogueRect,new Vector2(.5f,0),new Rect(-450,-(height+25),900,height));
            Place(bodyRect,Vector2.up,new Rect(24,61,850,textHeight));
            float y=61+textHeight+14;
            foreach(var b in Choices)if(b.gameObject.activeSelf)
            {
                var r=b.GetComponent<RectTransform>();bool continuing=!Asking;
                Place(r,Vector2.up,new Rect(continuing?620:24,continuing?height-58:y,continuing?255:850,continuing?40:choiceHeight));
                var label=b.GetComponentInChildren<TextMeshProUGUI>();Place(label.rectTransform,Vector2.up,new Rect(16,4,continuing?223:818,(continuing?40:choiceHeight)-8));y+=choiceHeight+7;
            }
            Place(closeHint.rectTransform,Vector2.up,new Rect(24,height-44,560,29));
        }
        void LateUpdate()
        {
            if(!root)return;
            string text=Objective.text;int newline=text.IndexOf('\n');
            if(newline>=0){trackerText.text=text.Substring(0,newline);Objective.text=text.Substring(newline+1);}
            bool modal=Dialog.activeSelf||Journal.activeSelf;controls.gameObject.SetActive(!modal);context.gameObject.SetActive(!modal&&!string.IsNullOrEmpty(Prompt.text));compassBox.gameObject.SetActive(!modal);
            if(Dialog.activeSelf)
            {
                LayoutDialogue();float t=Mathf.Clamp01((Time.unscaledTime-openedAt)/.14f);dialogueRect.anchoredPosition+=Vector2.down*(1-t)*12;
                dialogFill.color=Time.unscaledTime<feedbackUntil?Color.Lerp(paper,positive?new Color(.75f,.94f,.69f):new Color(1,.78f,.65f),Mathf.Clamp01((feedbackUntil-Time.unscaledTime)/.55f)*.35f):paper;
            }
            UpdateLabels();
        }
        void UpdateLabels()
        {
            var camera=Camera.main;if(!camera)return;
            var expired=new List<SeasonWorldLabel>();foreach(var pair in labels){if(!pair.Key){Destroy(pair.Value.gameObject);expired.Add(pair.Key);}else pair.Value.gameObject.SetActive(false);}foreach(var marker in expired)labels.Remove(marker);
            foreach(var marker in SeasonWorldLabel.Active)
            {
                if(!marker)continue;
                if(!labels.TryGetValue(marker,out var panel)){panel=Panel(marker.Caption,worldLabels,Vector2.up,new Rect(0,0,270,29));var label=Text("Label",panel,new Rect(10,4,250,25),marker.Caption,14);label.enableAutoSizing=true;label.fontSizeMin=11;label.fontSizeMax=14;labels.Add(marker,panel);}
                var p=camera.WorldToViewportPoint(marker.transform.position);
                bool visible=p.z>0&&p.x>=0&&p.x<=1&&p.y>=0&&p.y<=1&&!Journal.activeSelf&&!Dialog.activeSelf;panel.gameObject.SetActive(visible);
                if(visible)Place(panel,Vector2.up,new Rect(p.x*root.rect.width-135,(1-p.y)*root.rect.height,270,29));
            }
        }
        RectTransform Panel(string name,Transform parent,Vector2 anchor,Rect area)
        {
            var o=new GameObject(name,typeof(RectTransform));o.transform.SetParent(parent,false);var r=o.GetComponent<RectTransform>();Place(r,anchor,area);
            var shadow=Face("Shadow",r,new Color(.12f,.22f,.18f,.7f));Stretch(shadow.rectTransform);shadow.rectTransform.anchoredPosition=new Vector2(3,-5);
            var rim=Face("Border",r,border);Stretch(rim.rectTransform);
            var fill=Face("Parchment",r,paper);Stretch(fill.rectTransform);fill.rectTransform.offsetMin=new Vector2(3,3);fill.rectTransform.offsetMax=new Vector2(-3,-3);
            var rule=Face("Gold inset",r,new Color(.73f,.55f,.30f,.65f));Place(rule.rectTransform,Vector2.up,new Rect(14,6,Mathf.Max(8,area.width-28),2));
            return r;
        }
        UnityEngine.UI.Image Face(string name,Transform parent,Color color)
        {var o=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));o.transform.SetParent(parent,false);var i=o.GetComponent<UnityEngine.UI.Image>();i.sprite=rounded;i.type=UnityEngine.UI.Image.Type.Sliced;i.color=color;i.raycastTarget=false;return i;}
        TextMeshProUGUI Text(string name,Transform parent,Rect r,string value,int size,bool bold=false)
        {
            var o=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));o.transform.SetParent(parent,false);var t=o.GetComponent<TextMeshProUGUI>();Place(t.rectTransform,Vector2.up,r);t.font=font;t.fontSize=size;t.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;t.color=ink;t.text=value;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;t.margin=Vector4.zero;return t;
        }
        static void Place(RectTransform r,Vector2 anchor,Rect rect){r.anchorMin=r.anchorMax=anchor;r.pivot=Vector2.up;r.anchoredPosition=new Vector2(rect.x,-rect.y);r.sizeDelta=rect.size;}
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        public static Sprite Rounded()
        {
            var t=new Texture2D(32,32,TextureFormat.RGBA32,false);for(int y=0;y<32;y++)for(int x=0;x<32;x++){float dx=Mathf.Max(7-x,x-24),dy=Mathf.Max(7-y,y-24);t.SetPixel(x,y,dx>0&&dy>0&&dx*dx+dy*dy>49?Color.clear:Color.white);}t.filterMode=FilterMode.Point;t.Apply();return Sprite.Create(t,new Rect(0,0,32,32),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(8,8,8,8));
        }
        static Sprite ArrowSprite()
        {
            var t=new Texture2D(24,24,TextureFormat.RGBA32,false);for(int y=0;y<24;y++)for(int x=0;x<24;x++)t.SetPixel(x,y,x<=24-Mathf.Abs(y-12)*2?new Color(1,.78f,.29f):Color.clear);t.filterMode=FilterMode.Point;t.Apply();return Sprite.Create(t,new Rect(0,0,24,24),Vector2.one*.5f,100);
        }
        void OnDestroy(){foreach(var s in new[]{rounded,arrowSprite})if(s){Destroy(s.texture);Destroy(s);}}
    }
}
