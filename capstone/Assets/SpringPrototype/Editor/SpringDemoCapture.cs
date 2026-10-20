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
        Application.runInBackground=true;folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../tools/.spring-demo-final"));Directory.CreateDirectory(folder);
        scaled=new RenderTexture(1280,720,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);StartCoroutine(Capture());
        yield return new WaitForSeconds(2);
        yield return KeyPress(Key.W,1f);yield return KeyPress(Key.E);yield return new WaitForSeconds(3);
        for(int i=0;i<4;i++){yield return KeyPress(Key.E);yield return new WaitForSeconds(3);}
        yield return KeyPress(Key.E);yield return KeyPress(Key.A,1.25f);yield return KeyPress(Key.W,.6f);yield return KeyPress(Key.E);yield return new WaitForSeconds(2);yield return KeyPress(Key.Digit1);yield return new WaitForSeconds(2);yield return KeyPress(Key.E);
        yield return KeyPress(Key.W,.3f);yield return KeyPress(Key.D,2.1f);yield return KeyPress(Key.E);yield return new WaitForSeconds(2);yield return KeyPress(Key.Digit2);yield return new WaitForSeconds(2);yield return KeyPress(Key.E);
        yield return KeyPress(Key.R);yield return KeyPress(Key.A,.65f);yield return KeyPress(Key.E);yield return new WaitForSeconds(2);yield return KeyPress(Key.Digit2);yield return new WaitForSeconds(2);yield return KeyPress(Key.E);
        yield return KeyPress(Key.R);yield return KeyPress(Key.W,1);yield return KeyPress(Key.D,1.75f);yield return KeyPress(Key.W,.4f);yield return KeyPress(Key.E);
        foreach(Key key in new[]{Key.Digit1,Key.Digit1,Key.Digit2,Key.Digit1,Key.Digit2}){yield return new WaitForSeconds(2);yield return KeyPress(key);yield return new WaitForSeconds(1.7f);yield return KeyPress(Key.E);}
        yield return new WaitForSeconds(3);yield return KeyPress(Key.E);
        yield return KeyPress(Key.W,.8f);yield return KeyPress(Key.A,1.75f);yield return KeyPress(Key.W,.2f);yield return KeyPress(Key.E);yield return new WaitForSeconds(4);
        recording=false;var guide=Object.FindAnyObjectByType<SpringGuide>();Debug.Log("SPRING_DEMO_RECORDED: frames="+frame+" missions="+guide.MissionsComplete+" complete="+guide.SpringComplete+" path="+folder);
    }
    IEnumerator KeyPress(Key key,float duration=.12f)
    {
        InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(key));yield return new WaitForSeconds(duration);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());yield return new WaitForSeconds(.15f);
    }
    IEnumerator Capture()
    {
        while(recording)
        {
            yield return new WaitForEndOfFrame();var shot=ScreenCapture.CaptureScreenshotAsTexture();Graphics.Blit(shot,scaled);var old=RenderTexture.active;RenderTexture.active=scaled;pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();RenderTexture.active=old;File.WriteAllBytes(Path.Combine(folder,(frame++).ToString("D5")+".jpg"),pixels.EncodeToJPG(85));Destroy(shot);yield return new WaitForSeconds(.1666667f);
        }
    }
    void OnDestroy(){InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());if(scaled!=null){scaled.Release();Destroy(scaled);}if(pixels!=null)Destroy(pixels);}
}
