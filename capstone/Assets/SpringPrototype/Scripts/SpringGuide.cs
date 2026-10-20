using UnityEngine;
using UnityEngine.InputSystem;

// Spring: tutorial -> three short missions -> graded bat boss -> Summer gate.
public class SpringGuide : MonoBehaviour
{
    public Transform player, mage;
    public Texture2D batSheet;
    public Sprite gateSprite;
    GameObject gateSeal;
    public bool DialogueOpen { get; private set; }
    public bool LessonComplete { get; private set; }
    public bool SpringComplete { get; private set; }
    public int MissionsComplete { get; private set; }
    public int BossScore { get; private set; }
    int page, target = -1, bossQuestion, hearts = 3;
    bool bossResult;
    string feedback = "";
    Transform[] bats = new Transform[3];
    Transform boss, gate;
    bool[] cleared = new bool[3];
    GUIStyle title, text, small, button;
    readonly string[] lessons = {
        "Ambros! Another kid pulled through a computer... I was your age when it happened to me. I have been trapped here so long that everyone calls me the Ancient Mage.",
        "To get home, complete the challenges in Spring, Summer, Fall and Winter. Bring the final item back to Spring. I will teach you what you need to know, then join your party when we clear this season.",
        "AI is software built by people. Many AI systems learn patterns from training examples, such as text or pictures. They use those patterns to make predictions or generate answers. They do not know everything.",
        "Keep personal information out of AI tools. Do not upload private photos, passwords or your address. A service may store your inputs depending on its settings. Ask a trusted adult before sharing anything personal.",
        "Walk with WASD or the arrow keys. Press E near a character to talk. Three confused bats are spreading AI myths in this clearing. Find them and answer their challenges. Then face the bat guardian to open the Summer gate."
    };
    readonly string[] claims = { "AI is built by people and can learn patterns from examples.", "AI thinks and feels exactly like a human.", "It is safe to give an AI your password or private photos." };
    readonly bool[] truth = { true, false, false };
    readonly string[] explanations = { "Correct. People build and train AI systems using examples. That is how many systems learn patterns.", "Correct. AI can generate humanlike responses. That does not mean it thinks or feels like a person.", "Correct. Keep passwords, private photos and identifying details out of AI tools. Ask a trusted adult when unsure." };
    readonly string[] questions = { "How do many AI systems learn?", "Who builds AI systems?", "Can an AI give a wrong answer?", "Which information should stay private?", "What is a safe way to use AI for school?" };
    readonly string[,] answers = { { "By learning patterns from examples", "By knowing everything from birth" }, { "People who design and train them", "They appear without people" }, { "No. A confident answer is always right", "Yes. Check important claims" }, { "Passwords, your address and private photos", "A made-up story about a dragon" }, { "Let it do all my work and copy it", "Use it to explain a topic, then think for myself" } };
    readonly int[] correct = { 0, 0, 1, 0, 1 };
    readonly string[] reasons = { "AI learns patterns from training examples.", "People design, train and test AI systems.", "AI can make mistakes even when it sounds confident.", "Never share passwords or private personal information.", "Use AI to support your learning. Follow your teacher's rules." };
    void Start()
    {
        Vector2[] positions = { new Vector2(-5,3), new Vector2(5,3), new Vector2(-4,-4) };
        for(int i=0;i<3;i++) bats[i]=CreateBat("Misinformation Bat "+(i+1),positions[i],1);
        boss=CreateBat("Spring Bat Guardian",new Vector2(8,3),1.7f);
        var obj=new GameObject("Summer Gate (unlocks after 80 percent)");gate=obj.transform;gate.position=new Vector3(0,7,0);
        if(gateSprite!=null)
        {
            var r=obj.AddComponent<SpriteRenderer>();
            var texture=gateSprite.texture;r.sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,0),texture.height/4f);
            r.sharedMaterial=new Material(Shader.Find("Sprites/Default"));r.sharedMaterial.mainTexture=texture;r.sortingOrder=30;
            Debug.Log("SEASON_GATE_READY: "+texture.width+"x"+texture.height+" size="+r.bounds.size);
            foreach(float x in new[]{-1.2f,1.2f}) {var side=new GameObject("Stone pillar");side.transform.SetParent(gate);side.transform.localPosition=new Vector3(x,.6f,0);side.AddComponent<BoxCollider2D>().size=new Vector2(.6f,1.2f);}
        }
        gateSeal=new GameObject("Sealed path to Summer");gateSeal.transform.position=new Vector3(0,7.5f,0);
        var seal=gateSeal.AddComponent<SpriteRenderer>();seal.sprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);seal.sharedMaterial=mage.GetComponent<SpriteRenderer>().sharedMaterial;seal.color=new Color(.16f,.8f,.72f,.25f);seal.sortingOrder=20;gateSeal.transform.localScale=new Vector3(1.6f,1.5f,1);
        gateSeal.AddComponent<BoxCollider2D>();

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
        int best=-99;float distance=2.2f;
        if(Distance(mage)<distance){best=-1;distance=Distance(mage);}
        for(int i=0;i<3;i++) if(!cleared[i] && Distance(bats[i])<distance){best=i;distance=Distance(bats[i]);}
        if(Distance(boss)<distance){best=3;distance=Distance(boss);}
        if(Distance(gate)<distance)best=4;
        return best;
    }
    void Update()
    {
        if(SpringComplete && !DialogueOpen && Distance(mage)>1.6f && Distance(mage)<15) mage.position=Vector3.MoveTowards(mage.position,player.position+new Vector3(-1,-.5f,0),2.8f*Time.deltaTime);
        var k=Keyboard.current;if(k==null)return;
        if(k.escapeKey.wasPressedThisFrame){DialogueOpen=false;return;}
        if(k.eKey.wasPressedThisFrame || (DialogueOpen && k.enterKey.wasPressedThisFrame))
        {
            if(!DialogueOpen){target=Nearby();if(target==-99)return;DialogueOpen=true;page=0;feedback="";bossResult=false;bossQuestion=0;if(!SpringComplete)BossScore=0;}
            else Continue();
        }
        if(DialogueOpen && string.IsNullOrEmpty(feedback) && IsQuestion())
        {if(k.digit1Key.wasPressedThisFrame)Answer(0);else if(k.digit2Key.wasPressedThisFrame)Answer(1);}
    }
    bool IsQuestion()=> target>=0 && target<=2 && LessonComplete || target==3 && MissionsComplete==3 && !bossResult && !SpringComplete;
    void Continue()
    {
        if(target==-1){if(page<lessons.Length-1 && !LessonComplete)page++;else{LessonComplete=true;DialogueOpen=false;}return;}
        if(target>=0 && target<=2 && feedback!="")
        {if(cleared[target])DialogueOpen=false;else feedback="";return;}
        if(target==3 && feedback!="" && !bossResult){feedback="";bossQuestion++;if(bossQuestion>=questions.Length){bossResult=true;SpringComplete=BossScore>=4;if(SpringComplete){boss.gameObject.SetActive(false);gateSeal.SetActive(false);}}return;}
        if(target==3 && bossResult && !SpringComplete){bossQuestion=0;BossScore=0;bossResult=false;feedback="";return;}
        if(!IsQuestion() || SpringComplete)DialogueOpen=false;
    }
    void Answer(int selected)
    {
        if(target<3)
        {
            bool right=(selected==0)==truth[target];
            if(right){cleared[target]=true;MissionsComplete++;bats[target].gameObject.SetActive(false);feedback=explanations[target]+" Mission complete!";}
            else{hearts--;feedback="That myth fooled you! "+(hearts<=0?"The Mage restores your three hearts. ":"You lost a heart. ")+"Think about the lesson and try again.";if(hearts<=0)hearts=3;}
        }
        else {bool right=selected==correct[bossQuestion];if(right)BossScore++;feedback=(right?"Correct! ":"Not quite. ")+reasons[bossQuestion];}
    }
    void Styles()
    {
        if(title!=null)return;
        title=new GUIStyle(GUI.skin.label){fontSize=25,fontStyle=FontStyle.Bold,wordWrap=true};title.normal.textColor=new Color(.97f,.94f,.83f);
        text=new GUIStyle(GUI.skin.label){fontSize=19,wordWrap=true};text.normal.textColor=new Color(.9f,.94f,.88f);
        small=new GUIStyle(text){fontSize=14};
        button=new GUIStyle(GUI.skin.button){fontSize=18,wordWrap=true,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(16,16,8,8)};
    }
    void Panel(Rect r){var old=GUI.color;GUI.color=new Color(.025f,.055f,.05f,.97f);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=new Color(.62f,.8f,.48f);GUI.DrawTexture(new Rect(r.x,r.y,3,r.height),Texture2D.whiteTexture);GUI.color=old;}
    string Quest()=>SpringComplete?"Summer unlocked • Mage joined your party":!LessonComplete?"Meet the Ancient Mage on the northern path":MissionsComplete<3?"Dispel the AI myths: "+MissionsComplete+" / 3 bats":"Face the Bat Guardian east of the Mage";
    void OnGUI()
    {
        Styles();float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));float w=Screen.width/scale,h=Screen.height/scale;
        Panel(new Rect(24,24,300,85));GUI.Label(new Rect(42,35,280,32),"AMBROS' JOURNEY",title);GUI.Label(new Rect(42,73,280,27),"SPRING  /  THE AWAKENING",small);
        Panel(new Rect(w-428,24,404,100));GUI.Label(new Rect(w-410,35,365,26),"QUEST  •  HEARTS "+hearts+" / 3",small);GUI.Label(new Rect(w-410,65,365,57),Quest(),text);
        // World labels are drawn above characters; the gate changes color after the boss.
        for(int i=0;i<3;i++)if(!cleared[i])WorldLabel(bats[i],"MYTH BAT "+(i+1),w,h);
        if(!SpringComplete)WorldLabel(boss,"BAT GUARDIAN • 80% TO PASS",w,h);
        WorldLabel(gate,SpringComplete?"SUMMER GATE • UNLOCKED":"SUMMER GATE • LOCKED",w,h);
        if(!DialogueOpen){Panel(new Rect(24,h-62,620,38));GUI.Label(new Rect(40,h-57,600,30),"WASD / Arrows  Move     E  Interact     R  Return to start",small);if(Nearby()!=-99){Panel(new Rect(w/2-180,h-120,360,42));GUI.Label(new Rect(w/2-160,h-115,330,34),"[ E ]  Talk / Challenge / Inspect",text);}return;}
        var box=new Rect(w/2-450,h-355,900,330);Panel(box);
        string heading="Ancient Mage",body="";
        if(target==-1)body=LessonComplete?"Find the three myth bats, then face the Bat Guardian. Pass its five-question quiz with at least 80%. I will join you after we clear Spring.":lessons[page];
        else if(target<3){heading="Misinformation Bat "+(target+1);body=!LessonComplete?"You are not ready for this challenge. Find the Ancient Mage first.":feedback!=""?feedback:"TRUE OR FALSE?\n\n"+claims[target];}
        else if(target==3){heading="Spring Bat Guardian";body=SpringComplete?"Spring cleared! You earned Apprentice rank. The Ancient Mage joins your party. The Summer gate is now open.":MissionsComplete<3?"Dispel all three myth bats before challenging the guardian.":bossResult?"Your grade: "+BossScore+" / 5 ("+(BossScore*20)+"%). You need 80% to pass. Review your mistakes and try again. No progress is lost.":feedback!=""?feedback:"QUESTION "+(bossQuestion+1)+" / 5  •  80% TO PASS\n\n"+questions[bossQuestion];}
        else {heading="The Summer Gate";body=SpringComplete?"You have unlocked Summer! The next season teaches when AI is useful and when to do the thinking yourself. This build ends here. Your Spring adventure is complete.":"The path is sealed. Finish the Mage's lessons, defeat the myth bats and score at least 80% against the Bat Guardian.";}
        GUI.Label(new Rect(box.x+24,box.y+15,850,38),heading,title);GUI.Label(new Rect(box.x+24,box.y+61,850,145),body,text);
        if(IsQuestion() && feedback==""){for(int i=0;i<2;i++){string label=target<3?(i==0?"True":"False"):answers[bossQuestion,i];if(GUI.Button(new Rect(box.x+24,box.y+191+i*53,850,46),(i+1)+". "+label,button))Answer(i);}}
        else if(GUI.Button(new Rect(box.x+620,box.y+272,255,40),target==3&&bossResult&&!SpringComplete?"Try again [ E ]":"Continue [ E ]",button))Continue();
        GUI.Label(new Rect(box.x+24,box.y+286,560,29),"ESC  Close conversation",small);
    }
    void WorldLabel(Transform t,string label,float w,float h){if(t==null || Camera.main==null)return;var p=Camera.main.WorldToViewportPoint(t.position+Vector3.up*1.8f);if(p.x<0||p.x>1||p.y<0||p.y>1)return;GUI.Label(new Rect(p.x*w-150,(1-p.y)*h,300,27),label,small);}
}
