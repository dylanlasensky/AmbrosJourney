using AmbrosJourney.Audio;
using UnityEngine;
using UnityEngine.InputSystem;
using AmbrosJourney.Presentation;
using TMPro;

// Spring: exploration and field notes -> six applied challenges -> guardian -> Summer.
public class SpringGuide : MonoBehaviour
{
    public Transform player, mage;
    public Texture2D batSheet, guardianSheet;
    public Vector2 StartPosition = new Vector2(-18,16);
    public const float GuideTalkRadius=4f;
    bool explored, controlsSeen, welcomeOpen;
    AmbrosJourney.Journey.JourneyUI welcomeUI;
    public Sprite gateSprite;
    GameObject gateSeal;
    public bool DialogueOpen { get; private set; }
    public bool LessonComplete { get; private set; }
    public bool SpringComplete { get; private set; }
    public int MissionsComplete { get; private set; }
    public int BossScore { get; private set; }
    int page, target = -1, bossQuestion;
    bool bossResult;
    string feedback = "";
    Transform[] bats = new Transform[3];
    Transform boss, gate;
    public Transform Boss=>boss;
    public Transform ExitPortal=>gate;
    public GameObject ExitBarrier=>gateSeal;
    bool[] cleared = new bool[3];
    bool[] studied=new bool[3];
    int[] trials=new int[3];
    Transform[] stones=new Transform[3];
    bool journalOpen;
    readonly string[] locations={"Blossom Grove", "Stillwater Lake", "Whispering Ruins"};
    readonly string[] fieldNotes={
        "AI LEARNS PATTERNS\nMany AI systems learn patterns from examples such as text or pictures. This is called training. A chatbot uses learned patterns to create replies; it does not check every fact before answering. People test AI on new examples to find mistakes.\nRead this, then try the two claims at the nearby bat. J opens your notes.",
        "SOUNDS HUMAN, STILL A TOOL\nA chatbot can sound friendly without having feelings or knowing you. It can also sound sure when it is wrong. Use it for ideas, but check important facts somewhere reliable. Talk to people you trust when you need support.\nTry the two claims at the lakeside bat.",
        "KEEP SOME THINGS PRIVATE\nBefore sending a message, check what someone could learn about you from it. Leave out passwords, your address and private photos. A made-up example often works just as well. If you are unsure what a tool needs, ask a trusted adult.\nTry the two claims at the ruins bat."
    };
    SeasonOverlay presentation;
    SeasonWorldLabel mageLabel;
    readonly string[] lessons = {
        "You're stuck here too? I was about your age when I arrived. Now everyone calls me the Ancient Mage. We need to get through all four seasons to find a way home.",
        "Let's start with what AI can and can't do. Read the notes along the trail, then try the two claims at each bat. You can look back at your notes whenever you need them.",
        "Finish the three lessons, then get at least four of Blossom Guardian's five questions right. I'll come with you after that."
    };
    int QuestionIndex(int mission)=>mission*3+(trials[mission]==0?0:mission==2?1:2);
    readonly string[] claims = {
        "People can train AI systems to find patterns in examples such as text or pictures.",
        "If AI gets every practice example right, it will always get new examples right.",
        "AI can make mistakes on new examples, so people need to test its results.",
        "If a chatbot remembers your name, it cares about you the way a person does.",
        "A detailed chatbot answer is enough to prove a fact.",
        "Before using a surprising AI fact in homework, it helps to check a book or another reliable source.",
        "If a chatbot needs more details for a story, your password is useful context.",
        "A made-up name can work in a story prompt instead of your real name and address.",
        "It's okay to pause and ask an adult before sharing personal details with a tool."
    };
    readonly bool[] truth = { true,false,true,false,false,true,false,true,true };
    readonly string[] explanations = {
        "AI can learn patterns from examples people provide. People also decide what it should learn and check how well it works.",
        "AI can learn patterns that do not work well on new examples. Testing helps people find these mistakes.",
        "A good score on practice examples is not a guarantee. Testing new examples helps show where AI still makes mistakes.",
        "Remembering a name is a feature. It doesn't mean the chatbot has feelings or understands you like a friend would.",
        "Details can sound convincing and still be wrong. Look for evidence that supports the answer.",
        "Checking a source helps you catch mistakes before you use them. Open the source itself, not just the link in an AI answer.",
        "A story doesn't need your password. Give details about the story, not details that let someone into your account.",
        "A made-up example gives the tool something to work with without identifying you.",
        "You can stop and ask for help. You don't have to share something just to keep a conversation going."
    };
    readonly string[] questions = { "What does an AI chatbot use to create a reply?", "An AI chatbot writes a clear, confident answer. Why could it still be wrong?", "A chatbot gives a date you have never heard before. What should you do before using it?", "You ask an AI chatbot for party ideas. Which detail should you leave out?", "A chatbot says, 'I am your best friend.' What should you remember?" };
    readonly string[,] answers = { { "Patterns learned from training examples", "A list of guaranteed facts for every possible question" }, { "It can generate a claim without checking the facts", "Correct grammar guarantees correct facts" }, { "Use it if the answer gives enough detail", "Check a reliable source for that date" }, { "The password for your party-planning account", "The theme and how many ideas you want" }, { "It knows and cares about me like a person does", "Friendly words do not mean it has feelings or knows me" } };
    readonly int[] correct = { 0, 0, 1, 0, 1 };
    readonly string[] reasons = {
        "A chatbot creates replies using patterns learned during training. It does not have a guaranteed correct answer to every question.",
        "A chatbot can generate convincing words without evidence for a claim. Confidence and correct grammar do not prove accuracy.",
        "Check the date in a reliable source before using it. A detailed answer can still contain a mistake.",
        "A password gives access to your account. A party theme is enough context for brainstorming.",
        "Friendly language is something a chatbot can generate. It does not make the tool a person or a replacement for people you trust." };
    public AmbrosJourney.Journey.SpringState CaptureProgress()=>new AmbrosJourney.Journey.SpringState{explored=explored,controlsSeen=controlsSeen,lessonComplete=LessonComplete,complete=SpringComplete,missions=MissionsComplete,bossScore=BossScore,cleared=(bool[])cleared.Clone(),studied=(bool[])studied.Clone(),trials=(int[])trials.Clone()};
    public void RestoreProgress(AmbrosJourney.Journey.SpringState state)
    {
        if(state==null||!state.Valid())throw new System.ArgumentException("Invalid Spring checkpoint");
        controlsSeen=state.controlsSeen||state.explored||state.lessonComplete;welcomeOpen=false;
        if(welcomeUI){Destroy(welcomeUI.gameObject);welcomeUI=null;}
        explored=state.explored;LessonComplete=state.lessonComplete;SpringComplete=state.complete;MissionsComplete=state.missions;BossScore=state.bossScore;cleared=state.cleared;studied=state.studied;trials=state.trials;
        DialogueOpen=journalOpen=false;target=-1;feedback="";bossQuestion=0;bossResult=false;
        for(int i=0;i<3;i++)bats[i].gameObject.SetActive(!cleared[i]);boss.gameObject.SetActive(!SpringComplete);gateSeal.SetActive(!SpringComplete);AmbrosJourney.Journey.BossPortalSequence.RestoreCheckpoint();
    }
    void Start()
    {
        // Conversation uses proximity; companions must never plug a doorway or a compass route.
        foreach(var collider in mage.GetComponents<Collider2D>())collider.isTrigger=true;
        Vector2[] positions = { new Vector2(-18,0), new Vector2(10,6), new Vector2(15,-14) };
        for(int i=0;i<3;i++) bats[i]=CreateBat("Misinformation Bat "+(i+1),positions[i],1);
        Vector2[] notePositions={new Vector2(-21,5),new Vector2(7,1),new Vector2(21,-12)};
        for(int i=0;i<3;i++){var o=new GameObject(locations[i]+" field note");o.transform.position=notePositions[i];var r=o.AddComponent<SpriteRenderer>();if(gateSprite){var t=gateSprite.texture;r.sprite=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,0),t.height/1.8f);}r.sharedMaterial=mage.GetComponent<SpriteRenderer>().sharedMaterial;r.color=new Color(.85f,1,.85f);r.sortingOrder=100-Mathf.RoundToInt(notePositions[i].y*10);stones[i]=o.transform;}
        boss=CreateGuardian();
        var obj=new GameObject("Summer Gate (unlocks after 80 percent)");gate=obj.transform;gate.position=new Vector3(0,-33,0);
        if(gateSprite!=null)
        {
            var r=obj.AddComponent<SpriteRenderer>();
            var texture=gateSprite.texture;r.sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,0),texture.height/4f);
            r.sharedMaterial=new Material(Shader.Find("Sprites/Default"));r.sharedMaterial.mainTexture=texture;r.sortingOrder=430;
            Debug.Log("SEASON_GATE_READY: "+texture.width+"x"+texture.height+" size="+r.bounds.size);
            foreach(float x in new[]{-1.2f,1.2f}) {var side=new GameObject("Stone pillar");side.transform.SetParent(gate);side.transform.localPosition=new Vector3(x,.6f,0);side.AddComponent<BoxCollider2D>().size=new Vector2(.6f,1.2f);}
        }
        gateSeal=new GameObject("Sealed path to Summer");gateSeal.transform.position=new Vector3(0,-32.5f,0);
        var seal=gateSeal.AddComponent<SpriteRenderer>();seal.sprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);seal.sharedMaterial=mage.GetComponent<SpriteRenderer>().sharedMaterial;seal.color=new Color(.16f,.8f,.72f,.25f);seal.sortingOrder=420;gateSeal.transform.localScale=new Vector3(1.6f,1.5f,1);
        gateSeal.AddComponent<BoxCollider2D>();
        gate.gameObject.SetActive(false);
        NaturalTrail.Spring();

    }
    Transform CreateGuardian()
    {
        var o=new GameObject("Blossom Guardian");o.transform.position=new Vector2(0,-28);
        var r=o.AddComponent<SpriteRenderer>();r.sharedMaterial=mage.GetComponent<SpriteRenderer>().sharedMaterial;
        if(guardianSheet){var frames=new Sprite[4];for(int i=0;i<4;i++)frames[i]=Sprite.Create(guardianSheet,new Rect(64,guardianSheet.height-32*(i+1),32,32),new Vector2(.5f,.1f),8);o.AddComponent<SpringSpriteAnimation>().frames=frames;}
        o.AddComponent<CircleCollider2D>().radius=1;SeasonBossVisual.Attach(o,"SpringBlossomAtlas",5.8f);return o.transform;
    }
    Transform CreateBat(string label,Vector2 pos,float size)
    {
        var o=new GameObject(label);o.transform.position=pos;o.transform.localScale=Vector3.one*size;
        var r=o.AddComponent<SpriteRenderer>();r.sharedMaterial=mage.GetComponent<SpriteRenderer>().sharedMaterial;
        if(batSheet!=null) {int count=batSheet.width/64;var frames=new Sprite[count];for(int i=0;i<count;i++)frames[i]=Sprite.Create(batSheet,new Rect(i*64,0,64,64),new Vector2(.5f,.2f),32);o.AddComponent<SpringSpriteAnimation>().frames=frames;}
        var c=o.AddComponent<CircleCollider2D>();c.radius=.3f;
        return o.transform;
    }
    float Distance(Transform t) => t==null || player==null ? float.MaxValue : Vector2.Distance(player.position,t.position);
    int Nearby()
    {
        int best=-99;float distance=2.6f;
        for(int i=0;i<3;i++) if(!cleared[i] && Distance(bats[i])<distance){best=i;distance=Distance(bats[i]);}
        if(!SpringComplete&&boss.gameObject.activeInHierarchy&&Distance(boss)<5.5f&&best==-99){best=3;}
        if(AmbrosJourney.Journey.BossPortalSequence.AllowsEntry(gate)&&Distance(gate)<distance){best=4;distance=Distance(gate);}
        for(int i=0;i<3;i++)if(Distance(stones[i])<distance){best=5+i;distance=Distance(stones[i]);}
        if(best==-99&&!SpringComplete&&Distance(mage)<GuideTalkRadius)best=-1;
        return best;
    }
    public bool DismissPanel(){if(welcomeOpen){CloseWelcome();return true;}if(!DialogueOpen&&!journalOpen)return false;journalOpen=false;DialogueOpen=false;return true;}
    void CloseWelcome()
    {
        controlsSeen=true;welcomeOpen=false;DialogueOpen=false;
        if(welcomeUI){Destroy(welcomeUI.gameObject);welcomeUI=null;}
        var session=AmbrosJourney.Journey.JourneySession.Instance;if(session)session.CaptureChapter();
    }
    readonly AmbrosJourney.Presentation.SeasonTrail companionTrail=new AmbrosJourney.Presentation.SeasonTrail();
    int companionRevision=-1;
    void Update()
    {
        if(AmbrosJourney.Journey.JourneySession.Blocked)return;
        if(!controlsSeen&&!welcomeOpen)
        {
            welcomeOpen=true;DialogueOpen=true;
            var book=new GameObject("Spring controls journal");book.transform.SetParent(transform,false);
            welcomeUI=book.AddComponent<AmbrosJourney.Journey.JourneyUI>();welcomeUI.Initialize();welcomeUI.Canvas.sortingOrder=31001;
            AmbrosJourney.Journey.JourneyWelcome.Draw(welcomeUI,CloseWelcome);
        }
        if(welcomeOpen)
        {
            var keyboard=Keyboard.current;
            if(keyboard!=null&&(keyboard.eKey.wasPressedThisFrame||keyboard.enterKey.wasPressedThisFrame))CloseWelcome();
            return;
        }
        if(Vector2.Distance(player.position,StartPosition)>3)explored=true;
        companionTrail.Record(player.position);
        if(SpringComplete&&!DialogueOpen)
        {
            var destination=companionTrail.Formation(1,companionRevision!=companionTrail.Revision?(Vector2)player.position:(Vector2)mage.position);
            if(companionRevision!=companionTrail.Revision){mage.position=destination;companionRevision=companionTrail.Revision;}
            else if(Vector2.Distance(mage.position,destination)>.22f)
                mage.position=Vector2.MoveTowards(mage.position,destination,Mathf.Lerp(1.5f,5.5f,Mathf.InverseLerp(.3f,4.5f,Vector2.Distance(mage.position,destination)))*Time.deltaTime);
        }
        var k=Keyboard.current;if(k==null)return;
        if(k.mKey.wasPressedThisFrame)GameAudio.ToggleMute();
        if(k.jKey.wasPressedThisFrame && (!DialogueOpen || journalOpen)){GameAudio.Click();journalOpen=!journalOpen;DialogueOpen=journalOpen;return;}
        if(k.escapeKey.wasPressedThisFrame){journalOpen=false;DialogueOpen=false;return;}
        if(journalOpen)return;
        if(k.eKey.wasPressedThisFrame || (DialogueOpen && k.enterKey.wasPressedThisFrame))
        {
            if(!DialogueOpen){target=Nearby();if(target==-99)return;GameAudio.Click();DialogueOpen=true;page=0;feedback="";bossResult=false;bossQuestion=0;if(!SpringComplete)BossScore=0;}
            else Continue();
        }
        if(DialogueOpen && string.IsNullOrEmpty(feedback) && IsQuestion())
        {if(k.digit1Key.wasPressedThisFrame)Answer(0);else if(k.digit2Key.wasPressedThisFrame)Answer(1);}
    }
    bool IsQuestion()=> target>=0 && target<=2 && LessonComplete && studied[target] && !cleared[target] || target==3 && MissionsComplete==3 && !bossResult && !SpringComplete;
    void Continue()
    {
        GameAudio.Click();
        if(welcomeOpen){CloseWelcome();return;}
        if(target>=5){if(LessonComplete)studied[target-5]=true;DialogueOpen=false;return;}
        if(target==-1){if(page<lessons.Length-1 && !LessonComplete)page++;else{LessonComplete=true;DialogueOpen=false;}return;}
        if(target>=0 && target<=2 && feedback!="")
        {if(cleared[target])DialogueOpen=false;else feedback="";return;}
        if(target==3 && feedback!="" && !bossResult){feedback="";bossQuestion++;if(bossQuestion>=questions.Length){bossResult=true;SpringComplete=BossScore>=4;if(SpringComplete){GameAudio.Play(SoundCue.Complete,.65f);}}return;}
        if(target==3 && bossResult && !SpringComplete){bossQuestion=0;BossScore=0;bossResult=false;feedback="";return;}
        if(!IsQuestion() || SpringComplete)DialogueOpen=false;
    }
    void Answer(int selected)
    {
        if(selected<0 || selected>1 || !IsQuestion() || feedback!="")return;
        GameAudio.Click();
        if(target<3)
        {
            int question=QuestionIndex(target);bool right=(selected==0)==truth[question];GameAudio.Play(right?SoundCue.Correct:SoundCue.Incorrect,.45f);
            if(right){trials[target]++;feedback="Correct! "+explanations[question];if(trials[target]==2){cleared[target]=true;MissionsComplete++;GameAudio.Play(SoundCue.Complete,.55f);bats[target].gameObject.SetActive(false);feedback+="\nLesson complete. Your journal has been updated.";}}
            else{feedback="Incorrect. "+explanations[question]+" Try that claim again.";}
            feedback+="\n\n"+trials[target]+" / 2 correct.";
        }
        else {bool right=selected==correct[bossQuestion];GameAudio.Play(right?SoundCue.Correct:SoundCue.Incorrect,.45f);if(right){BossScore++;boss.GetComponent<SeasonBossVisual>().Flash(new Color(1,.85f,.55f));}else boss.GetComponent<SeasonBossVisual>().Strike();feedback=(right?"Correct! ":"Incorrect. ")+reasons[bossQuestion]+"\n\n"+BossScore+" / "+questions.Length+" correct.";}
    }
    Transform Destination(){if(!LessonComplete)return mage;for(int i=0;i<3;i++)if(!cleared[i])return studied[i]?bats[i]:stones[i];return SpringComplete?gate:boss;}
    string Quest()=>SpringComplete?"Step through the Summer gate":!LessonComplete?"Follow the arrow to the Ancient Mage. Press E to talk.":MissionsComplete<3?(studied[NextBat()]?"Follow the arrow to Myth Bat "+(NextBat()+1)+". Press E.":"Follow the arrow to the "+locations[NextBat()]+" field note. Press E."):"Follow the arrow to Blossom Guardian. Press E.";
    int NextBat(){for(int i=0;i<3;i++)if(!cleared[i])return i;return 2;}
    void LateUpdate()
    {
        if(AmbrosJourney.Journey.JourneySession.Blocked)return;
        if(!presentation)
        {
            var ui=new GameObject("Spring interface");ui.transform.SetParent(transform,false);
            presentation=ui.AddComponent<SeasonOverlay>();presentation.Initialize(Resources.Load<TMP_FontAsset>("SummerDefault"),1);
            for(int i=0;i<3;i++){AddLabel(stones[i],"[E] FIELD NOTE / "+locations[i]);AddLabel(bats[i],"[E] MYTH BAT "+(i+1));}
            mageLabel=SeasonWorldLabel.Attach(mage,"[E] Talk",112,GuideTalkRadius);AddLabel(boss,"[E] BLOSSOM GUARDIAN",310,5.5f);
        }
        if(mageLabel)mageLabel.enabled=!SpringComplete;
        presentation.Objective.text="LESSONS "+MissionsComplete+" / 3\n"+Quest();
        int nearby=Nearby();
        presentation.Prompt.text=DialogueOpen||nearby==-99?"":nearby==-1?"[ E ]  Talk to the Ancient Mage":"[ E ]  Explore / interact";
        presentation.Dialog.SetActive(DialogueOpen&&!journalOpen);presentation.Journal.SetActive(journalOpen);
        if(welcomeOpen){presentation.Dialog.SetActive(false);presentation.Prompt.text="";return;}
        if(journalOpen){var titles=new string[3];var notes=new string[3];for(int i=0;i<3;i++){titles[i]=locations[i]+" / "+trials[i]+" of 2 challenges";notes[i]=studied[i]?fieldNotes[i].Split('\n')[1]:"Read this field note with E, then try the nearby myth bat. Complete all three lessons before the Guardian (4 of 5 to pass).";}presentation.SetJournal(titles,notes,SpringComplete?"PARTY / Ambros + Ancient Mage":"PARTY / Ambros");return;}
        var dest=Destination();if(dest&&!DialogueOpen)presentation.SetCompass(player.position,dest.position,dest.position);
        if(DialogueOpen&&target==3)
        {
            var atmosphere=FindAnyObjectByType<SpringAtmosphere>();
            BossBattleMusic.Play(this,atmosphere?atmosphere.GetComponent<AudioSource>():null);
        }
        else BossBattleMusic.Stop(this);
        if(!DialogueOpen)return;
        string heading="Ancient Mage",body="";
        if(target==-1)body=SpringComplete?"You did it, Ambros! I will travel with you. The Summer gate is open beyond the guardian clearing.":LessonComplete?"Find the three myth bats, then face the Blossom Guardian. Pass its five-question quiz with at least 80%. I will join you after we clear Spring.":lessons[page];
        else if(target<3){heading=locations[target]+" / Challenge "+Mathf.Min(2,trials[target]+1)+" of 2";body=!LessonComplete?"Meet the Ancient Mage first.":!studied[target]?"Find the field note near this landmark first. Read it with E, then come back to apply what you learned.":feedback!=""?feedback:"TRUE OR FALSE?\n\n"+claims[QuestionIndex(target)];}
        else if(target==3){heading="Spring Blossom Guardian";body=SpringComplete?"Spring cleared! Your grade: "+BossScore+" / 5 ("+(BossScore*20)+"%). You earned Apprentice rank. The Ancient Mage joins your party. Close this message to watch the Guardian fade and the Summer portal form.":MissionsComplete<3?"Dispel all three myth bats before challenging the guardian.":bossResult?"Your grade: "+BossScore+" / 5 ("+(BossScore*20)+"%). You need 80% to pass. Review your mistakes and try again. No progress is lost.":feedback!=""?feedback:"QUESTION "+(bossQuestion+1)+" / 5  /  80% TO PASS\n\n"+questions[bossQuestion];}
        else if(target>=5){heading=locations[target-5]+" / Field Note";body=LessonComplete?fieldNotes[target-5]:"Visit the Mage at his cottage first. He will help you read these trail notes.";}
        else {heading="The Summer Gate";body=SpringComplete?"You have unlocked Summer! The next season teaches when AI is useful and when to do the thinking yourself. Close this conversation and walk through the unlocked gate to continue your adventure in Summer.":"The path is sealed. Finish the Mage's lessons, defeat the myth bats and score at least 80% against the Blossom Guardian.";}
        presentation.Heading.text=heading;presentation.Copy.text=body;
        bool asking=IsQuestion()&&feedback=="";
        for(int i=0;i<presentation.Choices.Count;i++)
        {
            var b=presentation.Choices[i];b.gameObject.SetActive(i<(asking?2:1));if(!b.gameObject.activeSelf)continue;
            b.onClick.RemoveAllListeners();int answer=i;
            b.GetComponentInChildren<TextMeshProUGUI>().text=asking?(i+1)+". "+(target<3?(i==0?"True":"False"):answers[bossQuestion,i]):"Continue [ E ]";
            if(asking)b.onClick.AddListener(()=>Answer(answer));else b.onClick.AddListener(Continue);
        }
        if(lastBody!=body){presentation.Present(asking);lastBody=body;}
    }
    string lastBody;
    void AddLabel(Transform t,string caption,float width=310,float revealDistance=2.6f){if(t)SeasonWorldLabel.Attach(t,caption,width,revealDistance);}
}
