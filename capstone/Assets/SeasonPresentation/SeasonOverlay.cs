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
        public AmbrosJourney.Winter.CaveVisibility CaveLighting;
        public GameObject Dialog, Journal;
        public TextMeshProUGUI Heading, Copy, Objective, Prompt, JournalCopy, Party, CompassText;
        public readonly List<UnityEngine.UI.Button> Choices = new List<UnityEngine.UI.Button>();
        public Vector2 CompassDelta { get; private set; }
        public float ObjectiveDistance { get; private set; }
        public RectTransform Arrow { get; private set; }
        public bool Asking { get; private set; }
        public bool CompassObscured { get; private set; }
        bool frostNotice;FrostNoticeFrame frostFrame;
        static readonly Color FrostPaper=new Color(.89f,.96f,1);
        public static string AnswerFeedback(bool right,string explanation,int score=-1,int total=0)
        {return (right?"Correct! ":"Incorrect. ")+explanation+(score<0?"":"\n\n"+score+" / "+total+" correct.");}
        RectTransform root, dialogueRect, headerRect, bodyRect, objectiveRect, context, controls, compassBox, worldLabels, chapterPanel, objectivePanel;
        UnityEngine.UI.Image dialogFill;
        TMP_FontAsset font;int season;
        Sprite rounded, card;
        MoonbranchCompass moonbranch;
        Color ink, accent, paper = new Color(.98f,.94f,.81f), border = new Color(.55f,.35f,.22f);
        readonly Dictionary<SeasonWorldLabel,RectTransform> labels = new Dictionary<SeasonWorldLabel,RectTransform>();
        readonly List<Rect> labelAreas=new List<Rect>();
        readonly List<SeasonWorldLabel> expiredLabels=new List<SeasonWorldLabel>();
        Transform labelPlayer;SpriteRenderer labelPlayerVisual;
        float openedAt, feedbackUntil; bool positive;
        public void Initialize(TMP_FontAsset typeface, bool autumn) => Initialize(typeface, autumn ? 3 : 2);
        public void Initialize(TMP_FontAsset typeface, int chapter)
        {
            season=chapter;bool autumn=chapter==3;
            font=GameFont.Resolve(typeface); ink=autumn?new Color(.25f,.20f,.14f):new Color(.19f,.29f,.23f);
            accent=autumn?new Color(.43f,.26f,.16f):new Color(.23f,.39f,.30f);
            if(chapter==4){ink=new Color(.16f,.23f,.30f);accent=new Color(.19f,.32f,.42f);}
            rounded=Rounded();card=FieldCardSkin.Create();if(card)paper=Color.white;
            root=gameObject.AddComponent<RectTransform>();Canvas=gameObject.AddComponent<Canvas>();Canvas.renderMode=RenderMode.ScreenSpaceOverlay;Canvas.sortingOrder=100;
            var scaler=gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.screenMatchMode=UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            if(!EventSystem.current){var e=new GameObject("Season EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));e.transform.SetParent(transform,false);}
            var title=Panel("Chapter",transform,Vector2.up,new Rect(24,24,290,83));chapterPanel=title;
            Text("Title",title,new Rect(18,10,260,32),new[]{"Spring Valley","Summer Valley","Emberwood","Frostveil"}[chapter-1],25,true);
            Text("Chapter",title,new Rect(18,48,260,27),new[]{"01  /  THE AWAKENING","02  /  INDEPENDENCE","03  /  PROMPT FORGE","04  /  SEEK THE EVIDENCE"}[chapter-1],14);
            var tracker=Panel("Objectives",transform,Vector2.one,new Rect(-428,24,404,100));objectivePanel=tracker;
            Objective=Text("Objective",tracker,new Rect(18,37,365,57),"",19);objectiveRect=Objective.rectTransform;
            // Kept as one text field for the established level contract; split visually in LateUpdate.
            trackerText=Text("Progress",tracker,new Rect(18,10,365,26),"",14);
            var compassObject=new GameObject("Moonbranch compass",typeof(RectTransform));compassObject.transform.SetParent(transform,false);
            compassBox=compassObject.GetComponent<RectTransform>();Place(compassBox,Vector2.one,new Rect(-136,126,112,126));
            moonbranch=compassObject.AddComponent<MoonbranchCompass>();moonbranch.Initialize(chapter,font);
            Arrow=moonbranch.Needle;Arrow.SetParent(transform,false);Arrow.sizeDelta=Vector2.one*MoonbranchCompass.MarkerSize;
            CompassText=Text("Distance",compassBox,new Rect(0,105,112,24),"",14);CompassText.alignment=TextAlignmentOptions.Center;CompassText.color=new Color(1,.96f,.84f);
            var distanceShadow=CompassText.gameObject.AddComponent<UnityEngine.UI.Shadow>();distanceShadow.effectColor=new Color(.08f,.09f,.13f,.95f);distanceShadow.effectDistance=new Vector2(1,-1);
            controls=Panel("Controls",transform,Vector2.zero,new Rect(24,-62,780,38));
            Text("Controls",controls,new Rect(16,6,750,27),"WASD / Arrows  Move    E  Interact    1-3  Answer    J  Journal    I  Artifacts    Esc  Back",14);
            context=Panel("Interaction prompt",transform,new Vector2(.5f,0),new Rect(-190,-120,380,42));Prompt=Text("Prompt",context,new Rect(18,5,345,32),"",19);
            // Party membership belongs in the journal, rather than floating over the scenery.
            Party=Text("Party",transform,new Rect(0,0,1,1),"",14);Party.gameObject.SetActive(false);
            var labelLayer=new GameObject("Landmark labels",typeof(RectTransform));labelLayer.transform.SetParent(transform,false);worldLabels=labelLayer.GetComponent<RectTransform>();Stretch(worldLabels);
            Arrow.SetSiblingIndex(worldLabels.GetSiblingIndex()+1); // Name cards must not hide the direction marker. Dialogs are created above it.
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
        public void SetCompass(Vector2 player,Vector2 waypoint,Vector2 destination,bool frozen=false)
        {
            CompassObscured=false;
            Arrow.gameObject.SetActive(true);
            // The player guide always points to the required destination. NPC pathfinding is separate.
            CompassDelta=destination-player;ObjectiveDistance=CompassDelta.magnitude;
            moonbranch.Point(CompassDelta);
            CompassText.text=Mathf.CeilToInt(ObjectiveDistance)+" steps";
        }

        public void SetFrostNotice(bool active)
        {
            frostNotice=active;
            if(active&&!frostFrame)
            {
                var obj=new GameObject("Icicle frame",typeof(RectTransform),typeof(FrostNoticeFrame));obj.transform.SetParent(dialogueRect,false);
                Stretch(obj.GetComponent<RectTransform>());frostFrame=obj.GetComponent<FrostNoticeFrame>();frostFrame.raycastTarget=false;
            }
            if(frostFrame)frostFrame.gameObject.SetActive(active);
            dialogFill.sprite=active?rounded:card?card:rounded;dialogFill.color=active?FrostPaper:paper;
            closeHint.text=active?"ESC Close message":"ESC Close conversation";
            LayoutDialogue();
        }
        public void Present(bool asking)
        {
            Asking=asking;openedAt=Time.unscaledTime;
            LayoutDialogue();
        }
        public void Feedback(bool right){positive=right;feedbackUntil=Time.unscaledTime+.55f;}
        RectTransform activityArea;
        float activityHeight;
        public RectTransform BeginActivity(float height)
        {
            EndActivity();activityHeight=height;
            var o=new GameObject("Final activity",typeof(RectTransform));o.transform.SetParent(dialogueRect,false);activityArea=o.GetComponent<RectTransform>();
            foreach(var choice in Choices)choice.gameObject.SetActive(false);
            Present(true);return activityArea;
        }
        public void EndActivity(){activityHeight=0;activityArea=null;}
        void LayoutDialogue()
        {
            if(!Copy)return;
            if(frostNotice)
            {
                float bodyHeight=Mathf.Clamp(Copy.GetPreferredValues(Copy.text,672,0).y+8,86,190),boxHeight=61+bodyHeight+78;
                Place(dialogueRect,new Vector2(.5f,.5f),new Rect(-360,-boxHeight*.5f,720,boxHeight));
                Place(headerRect,Vector2.up,new Rect(24,15,672,38));Place(bodyRect,Vector2.up,new Rect(24,61,672,bodyHeight));
                foreach(var b in Choices)if(b.gameObject.activeSelf)
                {
                    Place(b.GetComponent<RectTransform>(),Vector2.up,new Rect(441,boxHeight-58,255,40));
                    Place(b.GetComponentInChildren<TextMeshProUGUI>().rectTransform,Vector2.up,new Rect(16,4,223,32));
                }
                Place(closeHint.rectTransform,Vector2.up,new Rect(24,boxHeight-44,380,29));return;
            }
            Place(headerRect,Vector2.up,new Rect(24,15,850,38));
            float textHeight=Mathf.Max(Asking?90:64,Copy.GetPreferredValues(Copy.text,850,0).y+8);
            textHeight=Mathf.Min(250,textHeight);
            int count=0;foreach(var b in Choices)if(b.gameObject.activeSelf)count++;
            float choiceHeight=46;
            if(Asking)foreach(var b in Choices)if(b.gameObject.activeSelf)choiceHeight=Mathf.Max(choiceHeight,b.GetComponentInChildren<TextMeshProUGUI>().GetPreferredValues(818,0).y+16);
            float height=Asking?Mathf.Max(300,61+textHeight+14+count*(choiceHeight+7)+40):Mathf.Max(220,61+textHeight+78);
            if(activityHeight>0)height=61+textHeight+14+activityHeight+40;
            Place(dialogueRect,new Vector2(.5f,0),new Rect(-450,-(height+25),900,height));
            Place(bodyRect,Vector2.up,new Rect(24,61,850,textHeight));
            float y=61+textHeight+14;
            if(activityArea)Place(activityArea,Vector2.up,new Rect(24,y,850,activityHeight));
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
            bool modal=Dialog.activeSelf||Journal.activeSelf;controls.gameObject.SetActive(!modal);compassBox.gameObject.SetActive(!modal);
            compassBox.localScale=Vector3.zero;
            var guidePlayer=AmbrosJourney.Journey.JourneySession.Instance?AmbrosJourney.Journey.JourneySession.Instance.Player():null;
            bool guideVisible=!modal&&!AmbrosJourney.Journey.JourneySession.Blocked&&!CompassObscured&&guidePlayer&&ObjectiveDistance>1;
            Arrow.gameObject.SetActive(guideVisible);
            if(guideVisible&&Camera.main){var direction=CompassDelta.normalized;var point=Camera.main.WorldToViewportPoint(guidePlayer.position+(Vector3)(Vector2.up*.8f+direction*1.8f));Arrow.anchorMin=Arrow.anchorMax=Vector2.zero;Arrow.pivot=Vector2.one*.5f;Arrow.anchoredPosition=new Vector2(point.x*root.rect.width,point.y*root.rect.height);}
            context.gameObject.SetActive(false); // Interaction hints belong above their world targets.
            if(Dialog.activeSelf)
            {
                LayoutDialogue();float t=Mathf.Clamp01((Time.unscaledTime-openedAt)/.14f);dialogueRect.anchoredPosition+=Vector2.down*(1-t)*12;
                dialogFill.color=frostNotice?FrostPaper:Time.unscaledTime<feedbackUntil?Color.Lerp(paper,positive?new Color(.75f,.94f,.69f):new Color(1,.78f,.65f),Mathf.Clamp01((feedbackUntil-Time.unscaledTime)/.55f)*.35f):paper;
            }
            UpdateLabels();
        }
        void UpdateLabels()
        {
            var camera=Camera.main;if(!camera)return;
            expiredLabels.Clear();foreach(var pair in labels){if(!pair.Key){Destroy(pair.Value.gameObject);expiredLabels.Add(pair.Key);}else pair.Value.gameObject.SetActive(false);}foreach(var marker in expiredLabels)labels.Remove(marker);
            var player=AmbrosJourney.Journey.JourneySession.Instance?AmbrosJourney.Journey.JourneySession.Instance.Player():null;
            labelAreas.Clear();ReserveHud(chapterPanel);ReserveHud(objectivePanel);ReserveHud(controls);ReservePlayer(camera,player);
            float labelRange=CaveLighting?CaveLighting.LabelRange:8;
            foreach(var marker in SeasonWorldLabel.Active)
            {
                if(!marker||!marker.Visible||player&&Vector2.Distance(marker.transform.position,player.position)>labelRange)continue;
                if(CaveLighting&&!CaveLighting.Reveals(marker.transform.position))continue;
                float width=Mathf.Clamp(marker.Width,80,310);
                if(!labels.TryGetValue(marker,out var panel)){panel=Panel(marker.Caption,worldLabels,Vector2.up,new Rect(0,0,width,44));var label=Text("Label",panel,new Rect(12,8,width-24,30),marker.Caption,14);label.enableAutoSizing=true;label.fontSizeMin=11;label.fontSizeMax=14;labels.Add(marker,panel);}
                var p=camera.WorldToViewportPoint(marker.Position);
                bool visible=p.z>0&&p.x>=0&&p.x<=1&&p.y>=0&&p.y<=1&&!Journal.activeSelf&&!Dialog.activeSelf;panel.gameObject.SetActive(visible);
                if(visible)
                {
                    var area=new Rect(p.x*root.rect.width-width/2,(1-p.y)*root.rect.height-(marker.Anchored?44:0),width,44);
                    // Keep nearby name cards clear of Ambros, fixed HUD panels and one another.
                    if(!TryPlaceLabel(area,root.rect.size,labelAreas,out var placed)){panel.gameObject.SetActive(false);continue;}
                    labelAreas.Add(placed);Place(panel,Vector2.up,placed);
                }
            }
        }
        void ReservePlayer(Camera camera,Transform player)
        {
            if(labelPlayer!=player){labelPlayer=player;labelPlayerVisual=player?player.GetComponent<SpriteRenderer>():null;}
            if(!labelPlayerVisual||!labelPlayerVisual.enabled||!labelPlayerVisual.sprite||!labelPlayerVisual.gameObject.activeInHierarchy)return;
            var bounds=labelPlayerVisual.bounds;var low=new Vector2(float.PositiveInfinity,float.PositiveInfinity);var high=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
            // Project the current animation frame without allocations. Name cards may move;
            // the player's silhouette stays clear, including when the camera reaches a map edge.
            for(int corner=0;corner<4;corner++)
            {
                var point=camera.WorldToViewportPoint(new Vector3((corner&1)==0?bounds.min.x:bounds.max.x,(corner&2)==0?bounds.min.y:bounds.max.y,bounds.center.z));
                if(point.z<=0)return;
                low=Vector2.Min(low,point);high=Vector2.Max(high,point);
            }
            if(high.x<0||low.x>1||high.y<0||low.y>1)return;
            labelAreas.Add(Rect.MinMaxRect(low.x*root.rect.width-8,(1-high.y)*root.rect.height-8,high.x*root.rect.width+8,(1-low.y)*root.rect.height+8));
        }
        void ReserveHud(RectTransform panel)
        {
            if(!panel||!panel.gameObject.activeInHierarchy)return;
            var position=panel.anchoredPosition;
            labelAreas.Add(new Rect(root.rect.width*panel.anchorMin.x+position.x-8,root.rect.height*(1-panel.anchorMin.y)-position.y-8,panel.rect.width+16,panel.rect.height+16));
        }
        static bool TryPlaceLabel(Rect desired,Vector2 size,List<Rect> occupied,out Rect placed)
        {
            const float margin=8,step=48;
            placed=desired;
            if(size.x<desired.width+margin*2||size.y<desired.height+margin*2)return false;
            desired.x=Mathf.Clamp(desired.x,margin,size.x-desired.width-margin);
            desired.y=Mathf.Clamp(desired.y,margin,size.y-desired.height-margin);
            for(int attempt=0;attempt<=8;attempt++)
            {
                var candidate=desired;
                if(attempt>0)candidate.y+=((attempt+1)/2)*step*(attempt%2==1?-1:1);
                if(candidate.y<margin||candidate.yMax>size.y-margin)continue;
                bool overlap=false;foreach(var area in occupied)if(candidate.Overlaps(area)){overlap=true;break;}
                if(!overlap){placed=candidate;return true;}
            }
            return false;
        }
        RectTransform Panel(string name,Transform parent,Vector2 anchor,Rect area)
        {
            var o=new GameObject(name,typeof(RectTransform));o.transform.SetParent(parent,false);var r=o.GetComponent<RectTransform>();Place(r,anchor,area);
            var shadow=Face("Shadow",r,new Color(.12f,.22f,.18f,.7f));Stretch(shadow.rectTransform);shadow.rectTransform.anchoredPosition=new Vector2(3,-5);
            if(card)
            {
                var fill=Face("Parchment",r,Color.white);fill.sprite=card;fill.pixelsPerUnitMultiplier=8;Stretch(fill.rectTransform);
            }
            else
            {
                var rim=Face("Border",r,border);Stretch(rim.rectTransform);
                var fill=Face("Parchment",r,paper);Stretch(fill.rectTransform);fill.rectTransform.offsetMin=new Vector2(3,3);fill.rectTransform.offsetMax=new Vector2(-3,-3);
            }
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
        void OnDestroy(){if(card)Destroy(card);foreach(var s in new[]{rounded})if(s){Destroy(s.texture);Destroy(s);}}
    }
}
