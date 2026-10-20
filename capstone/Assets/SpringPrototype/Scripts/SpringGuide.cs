using UnityEngine;
using UnityEngine.InputSystem;

// Spring: tutorial -> three short missions -> graded bat boss -> Summer gate.
public class SpringGuide : MonoBehaviour
{
    public Transform player, mage;
    public Texture2D batSheet, guardianSheet;
    public Vector2 StartPosition = new Vector2(-18,16);
    bool explored;
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
    GUIStyle title, text, small, button, panelStyle;
    readonly string[] lessons = {
        "Ambros! Another kid pulled through a computer... I was your age when it happened to me. I have been trapped here so long that everyone calls me the Ancient Mage.",
        "To get home, complete the challenges in Spring, Summer, Fall and Winter. Bring the final item back to Spring. I will teach you what you need to know, then join your party when we clear this season.",
        "AI is software built by people. Many AI systems learn patterns from training examples, such as text or pictures. They use those patterns to make predictions or generate answers. They do not know everything.",
        "Keep personal information out of AI tools. Do not upload private photos, passwords or your address. A service may store your inputs depending on its settings. Ask a trusted adult before sharing anything personal.",
        "Walk with WASD or the arrow keys. Press E near a character to talk. Follow the golden compass through Blossom Grove, Stillwater Lake and the southern ruins. Three confused bats are spreading AI myths. Explore each landmark and answer their challenges. Then face the Thornhide Guardian to open the Summer gate."
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
        Vector2[] positions = { new Vector2(-18,0), new Vector2(10,6), new Vector2(15,-14) };
        for(int i=0;i<3;i++) bats[i]=CreateBat("Misinformation Bat "+(i+1),positions[i],1);
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

    }
    Transform CreateGuardian()
    {
        var o=new GameObject("Thornhide Guardian");o.transform.position=new Vector2(0,-28);
        var r=o.AddComponent<SpriteRenderer>();r.sharedMaterial=mage.GetComponent<SpriteRenderer>().sharedMaterial;
        if(guardianSheet){var frames=new Sprite[4];for(int i=0;i<4;i++)frames[i]=Sprite.Create(guardianSheet,new Rect(64,guardianSheet.height-32*(i+1),32,32),new Vector2(.5f,.1f),8);o.AddComponent<SpringSpriteAnimation>().frames=frames;}
        o.AddComponent<CircleCollider2D>().radius=1;return o.transform;
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
        if(Distance(mage)<distance){best=-1;distance=Distance(mage);}
        for(int i=0;i<3;i++) if(!cleared[i] && Distance(bats[i])<distance){best=i;distance=Distance(bats[i]);}
        if(Distance(boss)<distance){best=3;distance=Distance(boss);}
        if(Distance(gate)<distance)best=4;
        return best;
    }
    void Update()
    {
        if(Vector2.Distance(player.position,StartPosition)>3)explored=true;
        if(SpringComplete && Distance(mage)>15)mage.position=player.position+new Vector3(-2,1,0);
        if(SpringComplete && !DialogueOpen && Distance(mage)>1.6f) mage.position=Vector3.MoveTowards(mage.position,player.position+new Vector3(-1,-.5f,0),2.8f*Time.deltaTime);
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
    Texture2D panelTexture, buttonTexture, arrowTexture;
    Texture2D Rounded(Color c){var t=new Texture2D(32,32);for(int y=0;y<32;y++)for(int x=0;x<32;x++){float dx=Mathf.Max(7-x,x-24),dy=Mathf.Max(7-y,y-24);t.SetPixel(x,y,dx>0&&dy>0&&dx*dx+dy*dy>49?Color.clear:c);}t.filterMode=FilterMode.Point;t.Apply();return t;}
    void Styles()
    {
        if(title!=null)return;
        panelTexture=Rounded(new Color(.98f,.94f,.81f));buttonTexture=Rounded(new Color(.23f,.39f,.30f).linear);panelStyle=new GUIStyle{border=new RectOffset(8,8,8,8)};panelStyle.normal.background=panelTexture;
        arrowTexture=new Texture2D(24,24);for(int y=0;y<24;y++)for(int x=0;x<24;x++)arrowTexture.SetPixel(x,y,x<=24-Mathf.Abs(y-12)*2?new Color(1,.78f,.29f):Color.clear);arrowTexture.Apply();
        title=new GUIStyle(GUI.skin.label){fontSize=25,fontStyle=FontStyle.Bold,wordWrap=true};title.normal.textColor=new Color(.19f,.29f,.23f).linear;
        text=new GUIStyle(GUI.skin.label){fontSize=19,wordWrap=true};text.normal.textColor=new Color(.24f,.32f,.27f).linear;
        small=new GUIStyle(text){fontSize=14};
        button=new GUIStyle(GUI.skin.button){fontSize=18,wordWrap=true,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(16,16,8,8),border=new RectOffset(8,8,8,8)};button.normal.background=buttonTexture;button.normal.textColor=new Color(1,.96f,.83f);button.hover.background=buttonTexture;button.hover.textColor=Color.white;
    }
    void Panel(Rect r){if(Event.current.type!=EventType.Repaint)return;var old=GUI.color;GUI.color=new Color(.12f,.22f,.18f,.7f).linear;panelStyle.Draw(new Rect(r.x+3,r.y+5,r.width,r.height),false,false,false,false);GUI.color=new Color(.55f,.35f,.22f).linear;panelStyle.Draw(r,false,false,false,false);GUI.color=Color.white;panelStyle.Draw(new Rect(r.x+3,r.y+3,r.width-6,r.height-6),false,false,false,false);GUI.color=old;}
    Transform Destination(){if(!LessonComplete)return mage;for(int i=0;i<3;i++)if(!cleared[i])return bats[i];return SpringComplete?gate:boss;}
    string Quest()=>SpringComplete?"Step through the Summer gate":!explored?"Stretch your legs! Follow the compass.":!LessonComplete?"Meet the Mage beside his cottage":MissionsComplete<3?new[]{"Blossom Grove: learn how AI learns","Lakeside: separate people from AI","Southern ruins: protect your privacy"}[NextBat()]:"Head south to challenge Thornhide";
    int NextBat(){for(int i=0;i<3;i++)if(!cleared[i])return i;return 2;}
    void Compass(float w,float h){var dest=Destination();if(!dest||DialogueOpen)return;Vector2 waypoint=dest.position;if(LessonComplete&&MissionsComplete==1){if(player.position.x< -10&&player.position.y> -5)waypoint=new Vector2(-15,-7);else if(player.position.x<2)waypoint=new Vector2(3,-7);}Vector2 delta=waypoint-(Vector2)player.position;var center=new Vector2(w-65,176);Panel(new Rect(w-112,130,88,100));var matrix=GUI.matrix;GUI.matrix=matrix*Matrix4x4.TRS(center,Quaternion.Euler(0,0,-Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg),Vector3.one);GUI.DrawTexture(new Rect(-17,-17,34,34),arrowTexture);GUI.matrix=matrix;GUI.Label(new Rect(w-100,198,78,25),Mathf.CeilToInt(delta.magnitude)+" steps",small);}
    void OnGUI()
    {
        Styles();float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));float w=Screen.width/scale,h=Screen.height/scale;
        Panel(new Rect(24,24,260,83));GUI.Label(new Rect(42,34,240,32),"Spring Valley",title);GUI.Label(new Rect(42,72,240,27),"CHAPTER 01  /  THE AWAKENING",small);
        Panel(new Rect(w-428,24,404,100));GUI.Label(new Rect(w-410,34,365,26),"JOURNAL  "+MissionsComplete+" / 3   |   COURAGE "+hearts+" / 3",small);GUI.Label(new Rect(w-410,61,365,57),Quest(),text);
        Compass(w,h);
        for(int i=0;i<3;i++)if(!cleared[i])WorldLabel(bats[i],"MYTH BAT "+(i+1),w,h);
        if(!SpringComplete)WorldLabel(boss,"THORNHIDE  /  80% TO PASS",w,h);
        WorldLabel(gate,SpringComplete?"SUMMER  /  UNLOCKED":"SUMMER  /  SEALED",w,h);
        if(!DialogueOpen){Panel(new Rect(24,h-62,590,38));GUI.Label(new Rect(40,h-57,565,30),"WASD / Arrows  Move     E  Talk     R  Return to cottage",small);if(Nearby()!=-99){Panel(new Rect(w/2-150,h-120,300,42));GUI.Label(new Rect(w/2-130,h-115,270,34),"[ E ]  Start conversation",text);}return;}
        var box=new Rect(w/2-450,h-355,900,330);Panel(box);
        string heading="Ancient Mage",body="";
        if(target==-1)body=SpringComplete?"You did it, Ambros! I will travel with you. The Summer gate is open beyond the guardian clearing.":LessonComplete?"Find the three myth bats, then face the Thornhide Guardian. Pass its five-question quiz with at least 80%. I will join you after we clear Spring.":lessons[page];
        else if(target<3){heading="Misinformation Bat "+(target+1);body=!LessonComplete?"You are not ready for this challenge. Find the Ancient Mage first.":feedback!=""?feedback:"TRUE OR FALSE?\n\n"+claims[target];}
        else if(target==3){heading="Spring Thornhide Guardian";body=SpringComplete?"Spring cleared! Your grade: "+BossScore+" / 5 ("+(BossScore*20)+"%). You earned Apprentice rank. The Ancient Mage joins your party. The Summer gate is now open.":MissionsComplete<3?"Dispel all three myth bats before challenging the guardian.":bossResult?"Your grade: "+BossScore+" / 5 ("+(BossScore*20)+"%). You need 80% to pass. Review your mistakes and try again. No progress is lost.":feedback!=""?feedback:"QUESTION "+(bossQuestion+1)+" / 5  •  80% TO PASS\n\n"+questions[bossQuestion];}
        else {heading="The Summer Gate";body=SpringComplete?"You have unlocked Summer! The next season teaches when AI is useful and when to do the thinking yourself. This build ends here. Your Spring adventure is complete.":"The path is sealed. Finish the Mage's lessons, defeat the myth bats and score at least 80% against the Thornhide Guardian.";}
        GUI.Label(new Rect(box.x+24,box.y+15,850,38),heading,title);GUI.Label(new Rect(box.x+24,box.y+61,850,145),body,text);
        if(IsQuestion() && feedback==""){for(int i=0;i<2;i++){string label=target<3?(i==0?"True":"False"):answers[bossQuestion,i];if(GUI.Button(new Rect(box.x+24,box.y+191+i*53,850,46),(i+1)+". "+label,button))Answer(i);}}
        else if(GUI.Button(new Rect(box.x+620,box.y+272,255,40),target==3&&bossResult&&!SpringComplete?"Try again [ E ]":"Continue [ E ]",button))Continue();
        GUI.Label(new Rect(box.x+24,box.y+286,560,29),"ESC  Close conversation",small);
    }
    void WorldLabel(Transform t,string label,float w,float h){if(t==null || Camera.main==null)return;var p=Camera.main.WorldToViewportPoint(t.position+Vector3.up*1.8f);if(p.x<0||p.x>1||p.y<0||p.y>1)return;var r=new Rect(p.x*w-135,(1-p.y)*h,270,29);Panel(r);GUI.Label(new Rect(r.x+10,r.y+4,250,25),label,small);}
}
