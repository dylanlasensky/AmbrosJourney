using UnityEngine;
using UnityEngine.InputSystem;
public class SpringGuide : MonoBehaviour
{
    public Transform player;
    public Transform mage;
    public bool DialogueOpen { get; private set; }
    public bool LessonComplete { get; private set; }
    int page;
    string feedback = "";
    GUIStyle title, text, small, button;
    bool near => player != null && mage != null && Vector2.Distance(player.position, mage.position) < 2.5f;
    readonly string[] lines = {
        "Ambros! You made it to Spring. I am the Ancient Mage. I was pulled into this world just like you. Learn its lessons and we can find a way home.",
        "Move with WASD or the arrow keys. Walk up to someone and press E to talk. Follow the path through the clearing. Each season will teach you a new AI skill.",
        "Your first lesson: AI learns patterns from examples. It can help explain things and suggest ideas. But a confident answer can still be wrong. Check important information with a reliable source."
    };
    void Update()
    {
        var k = Keyboard.current; if (k == null) return;
        if (k.escapeKey.wasPressedThisFrame) { DialogueOpen = false; return; }
        if (DialogueOpen && !near) DialogueOpen = false;
        if (k.eKey.wasPressedThisFrame || (DialogueOpen && (k.enterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame)))
        {
            if (!DialogueOpen && near) { DialogueOpen = true; page = LessonComplete ? 4 : 0; feedback = ""; }
            else if (DialogueOpen && page < 3) page++;
            else if (DialogueOpen && page == 4) DialogueOpen = false;
        }
        if (DialogueOpen && page == 3)
        {
            if (k.digit1Key.wasPressedThisFrame) Answer(false);
            if (k.digit2Key.wasPressedThisFrame) Answer(true);
        }
    }
    void Answer(bool correct)
    {
        if (correct) { LessonComplete = true; page = 4; feedback = ""; }
        else feedback = "Try again. AI can sound confident even when its answer is wrong.";
    }
    void Styles()
    {
        if (title != null) return;
        title = new GUIStyle(GUI.skin.label) { fontSize = 25, fontStyle = FontStyle.Bold, wordWrap = true }; title.normal.textColor = new Color(.95f,.94f,.85f);
        text = new GUIStyle(GUI.skin.label) { fontSize = 19, wordWrap = true }; text.normal.textColor = new Color(.86f,.9f,.85f);
        small = new GUIStyle(text) { fontSize = 14 }; small.normal.textColor = new Color(.64f,.77f,.68f);
        button = new GUIStyle(GUI.skin.button) { fontSize = 17, wordWrap = true, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(16,16,8,8) };
    }
    void Panel(Rect r)
    {
        var old=GUI.color; GUI.color=new Color(.035f,.075f,.065f,.96f); GUI.DrawTexture(r,Texture2D.whiteTexture); GUI.color=old;
        GUI.color=new Color(.55f,.75f,.47f); GUI.DrawTexture(new Rect(r.x,r.y,3,r.height),Texture2D.whiteTexture); GUI.color=old;
    }
    void OnGUI()
    {
        Styles();
        float scale=Mathf.Min(Screen.width/1280f, Screen.height/720f);
        GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(scale,scale,1));
        float w=Screen.width/scale, h=Screen.height/scale;
        Panel(new Rect(24,24,330,101));
        GUI.Label(new Rect(42,34,300,31),"SPRING  /  THE AWAKENING",small);
        GUI.Label(new Rect(42,61,290,33),"Ambros' Journey",title);
        GUI.Label(new Rect(42,98,300,25),"Tutorial + Level 1 prototype",small);
        Panel(new Rect(w-362,24,338,89));
        GUI.Label(new Rect(w-344,35,300,24),LessonComplete ? "FIRST LESSON COMPLETE" : "YOUR FIRST QUEST",small);
        GUI.Label(new Rect(w-344,62,300,42),LessonComplete ? "Explore the Spring clearing" : "Find the Ancient Mage",text);
        if (!DialogueOpen)
        {
            Panel(new Rect(24,h-66,520,42));
            GUI.Label(new Rect(40,h-57,500,26),"WASD / Arrows  Move     E  Interact     R  Return to start",small);
            if(near) { Panel(new Rect(w/2-170,h-131,340,44)); GUI.Label(new Rect(w/2-153,h-123,310,32),"[ E ]  Talk to the Ancient Mage",text); }
            return;
        }
        float height=page==3 ? 300 : 237;
        Rect box=new Rect(w/2-440,h-height-25,880,height); Panel(box);
        GUI.Label(new Rect(box.x+24,box.y+15,790,37),page==3 ? "A quick check" : "Ancient Mage",title);
        GUI.Label(new Rect(box.x+24,box.y+58,825,116),page<3 ? lines[page] : page==3 ? "An AI gives you an answer that sounds convincing. What should you do?" : "Exactly! Check important claims with a reliable source. You have completed your first lesson. Explore Spring while we prepare the next challenge.",text);
        if(page==3)
        {
            if(GUI.Button(new Rect(box.x+24,box.y+118,832,48),"1. Trust it because it sounds confident",button)) Answer(false);
            if(GUI.Button(new Rect(box.x+24,box.y+174,832,48),"2. Check it against a reliable source",button)) Answer(true);
            GUI.Label(new Rect(box.x+24,box.y+230,820,44),feedback,small);
        }
        else if(GUI.Button(new Rect(box.x+635,box.y+height-59,220,38),page==4 ? "Finish  [ E ]" : "Continue  [ E ]",button)) { if(page==4) DialogueOpen=false; else page++; }
        GUI.Label(new Rect(box.x+24,box.y+height-42,570,29),"ESC  Close conversation",small);
    }
}
