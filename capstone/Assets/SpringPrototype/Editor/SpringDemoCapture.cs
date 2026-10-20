using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor;
using UnityEditor.SceneManagement;

// Editor-only demo: sends normal keyboard input and records the actual Game view.
[InitializeOnLoad]
public static class SpringDemoCapture
{
    static SpringDemoCapture(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("SpringDemoPending",false)){SessionState.SetBool("SpringDemoPending",false);new GameObject("Editor Walkthrough Recorder").AddComponent<SpringWalkthrough>();}};}
    [MenuItem("Capstone/Record Spring Demo")]
    static void Record(){if(EditorApplication.isPlaying)return;if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene("Assets/Scenes/Spring.unity");SessionState.SetBool("SpringDemoPending",true);EditorApplication.isPlaying=true;}
}
public class SpringWalkthrough : MonoBehaviour
{
    bool recording=true;int frame;string folder;RenderTexture scaled;Texture2D pixels;
    IEnumerator Start()
    {
        Application.runInBackground=true;Time.captureFramerate=12;folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../tools/.spring-demo-v3"));Directory.CreateDirectory(folder);
        scaled=new RenderTexture(1280,720,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);StartCoroutine(Capture());
        yield return new WaitForSeconds(2);
        yield return MoveTo(new Vector2(-14,14));yield return KeyPress(Key.E);yield return new WaitForSeconds(3);
        for(int i=0;i<4;i++){yield return KeyPress(Key.E);yield return new WaitForSeconds(3);}
        yield return KeyPress(Key.E);
        yield return MoveTo(new Vector2(-18,6));yield return MoveTo(new Vector2(-18,1.2f));yield return Challenge(Key.Digit1);
        yield return MoveTo(new Vector2(-15,-7));yield return MoveTo(new Vector2(3,-7));yield return MoveTo(new Vector2(10,0));yield return MoveTo(new Vector2(10,4.3f));yield return Challenge(Key.Digit2);
        yield return MoveTo(new Vector2(19,2));yield return MoveTo(new Vector2(21,-7));yield return MoveTo(new Vector2(15,-12.3f));yield return Challenge(Key.Digit2);
        yield return MoveTo(new Vector2(15,-22));yield return MoveTo(new Vector2(0,-24));yield return MoveTo(new Vector2(0,-26.2f));yield return KeyPress(Key.E);
        foreach(Key key in new[]{Key.Digit1,Key.Digit1,Key.Digit2,Key.Digit1,Key.Digit2}){yield return new WaitForSeconds(2.8f);yield return KeyPress(key);yield return new WaitForSeconds(2);yield return KeyPress(Key.E);}
        yield return new WaitForSeconds(3);yield return KeyPress(Key.E);
        yield return MoveTo(new Vector2(0,-31.3f));yield return KeyPress(Key.E);yield return new WaitForSeconds(4);
        recording=false;var guide=Object.FindAnyObjectByType<SpringGuide>();Debug.Log("SPRING_DEMO_RECORDED: frames="+frame+" missions="+guide.MissionsComplete+" complete="+guide.SpringComplete+" path="+folder);
    }
    IEnumerator Challenge(Key key){yield return KeyPress(Key.E);yield return new WaitForSeconds(3);yield return KeyPress(key);yield return new WaitForSeconds(3);yield return KeyPress(Key.E);}
    IEnumerator MoveTo(Vector2 goal){var player=Object.FindAnyObjectByType<SpringPlayer>();float start=Time.time;while(Vector2.Distance(player.transform.position,goal)>.23f){if(Time.time-start>18){Debug.LogError("DEMO_ROUTE_BLOCKED: "+goal+" from "+player.transform.position);recording=false;yield break;}Vector2 d=goal-(Vector2)player.transform.position;var keys=new System.Collections.Generic.List<Key>();if(d.x>.18f)keys.Add(Key.D);if(d.x<-.18f)keys.Add(Key.A);if(d.y>.18f)keys.Add(Key.W);if(d.y<-.18f)keys.Add(Key.S);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(keys.ToArray()));yield return null;}InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());yield return new WaitForSeconds(.25f);}
    IEnumerator KeyPress(Key key,float duration=.12f)
    {
        InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(key));yield return new WaitForSeconds(duration);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());yield return new WaitForSeconds(.15f);
    }
    IEnumerator Capture()
    {
        while(recording)
        {
            yield return new WaitForEndOfFrame();var shot=ScreenCapture.CaptureScreenshotAsTexture();Graphics.Blit(shot,scaled);var old=RenderTexture.active;RenderTexture.active=scaled;pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();RenderTexture.active=old;File.WriteAllBytes(Path.Combine(folder,(frame++).ToString("D5")+".jpg"),pixels.EncodeToJPG(85));Destroy(shot);
        }
    }
    void OnDestroy(){Time.captureFramerate=0;InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());if(scaled!=null){scaled.Release();Destroy(scaled);}if(pixels!=null)Destroy(pixels);}
}
