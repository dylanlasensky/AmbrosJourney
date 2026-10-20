using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System.IO;
[InitializeOnLoad]
public static class AnimatedOpeningCapture
{
    static AnimatedOpeningCapture(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("AnimatedOpeningCapture",false)){SessionState.SetBool("AnimatedOpeningCapture",false);new GameObject("Opening Recorder").AddComponent<OpeningFrames>();}};}
    [MenuItem("Capstone/Record Animated Opening")]
    static void Record(){if(EditorApplication.isPlaying)return;var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("Camera",typeof(Camera),typeof(AudioListener));var a=new GameObject("Animated Opening",typeof(AnimatedOpening)).GetComponent<AnimatedOpening>();a.tree=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SpringPrototype/Art/SpringTree.png");a.hero=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SpringPrototype/Art/AmbrosIDLE1.png");a.cabin=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SpringPrototype/Art/MageCabin.png");EditorSceneManager.SaveScene(scene,"Assets/Opening/AnimatedSource.unity");SessionState.SetBool("AnimatedOpeningCapture",true);EditorApplication.isPlaying=true;}
}
public class OpeningFrames:MonoBehaviour
{
    IEnumerator Start(){Application.runInBackground=true;var a=Object.FindAnyObjectByType<AnimatedOpening>();a.manualClock=true;string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../tools/.intro-animated"));Directory.CreateDirectory(dir);var rt=new RenderTexture(1280,720,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);for(int i=0;i<480;i++){a.SetTime(i/24f);yield return new WaitForEndOfFrame();var shot=ScreenCapture.CaptureScreenshotAsTexture();Graphics.Blit(shot,rt);var old=RenderTexture.active;RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();RenderTexture.active=old;File.WriteAllBytes(Path.Combine(dir,i.ToString("D5")+".jpg"),pixels.EncodeToJPG(95));Destroy(shot);}rt.Release();Destroy(rt);Destroy(pixels);Debug.Log("ANIMATED_OPENING_CAPTURED: 480 frames / 20 seconds");EditorApplication.isPlaying=false;}
}
