using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

[InitializeOnLoad]
public static class OpeningSetup
{
    const string Opening="Assets/Opening/Opening.mp4";
    static OpeningSetup(){EditorApplication.delayCall+=AutoSetup;}
    static void AutoSetup(){if(!EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(Opening) && !File.Exists("Assets/Opening/Intro.unity"))Build();}
    [MenuItem("Capstone/Set Up Opening and Spring Missions")]
    public static void Build()
    {
        if(EditorApplication.isPlaying){Debug.LogWarning("Stop Play mode before setup.");return;}
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        var spring=EditorSceneManager.OpenScene("Assets/Scenes/Spring.unity");
        var guide=Object.FindAnyObjectByType<SpringGuide>();
        guide.batSheet=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Animations/Hero pack/64 X 64 Bat 2D assets/Sprites and files/Full Sprite sheet/Fly sprite sheet.png");
        var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/SpringPrototype/Art/SeasonGate.png");
        if(importer!=null){importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.maxTextureSize=256;importer.spritePixelsPerUnit=64;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=9;settings.spritePivot=new Vector2(.5f,0);importer.SetTextureSettings(settings);importer.SaveAndReimport();guide.gateSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/SpringPrototype/Art/SeasonGate.png");}
        EditorUtility.SetDirty(guide);EditorSceneManager.SaveScene(spring);
        var intro=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var cam=new GameObject("Opening Camera",typeof(Camera),typeof(AudioListener));cam.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;cam.GetComponent<Camera>().backgroundColor=Color.black;
        var playback=new GameObject("Automatic Opening",typeof(VideoPlayer),typeof(OpeningCinematic));playback.GetComponent<OpeningCinematic>().clip=AssetDatabase.LoadAssetAtPath<VideoClip>(Opening);
        EditorSceneManager.SaveScene(intro,"Assets/Opening/Intro.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Opening/Intro.unity",true),new EditorBuildSettingsScene("Assets/Scenes/Spring.unity",true)};
        AssetDatabase.SaveAssets();Debug.Log("OPENING_AND_SPRING_READY");
    }
    [MenuItem("Capstone/Play From Opening")]
    public static void PlayOpening(){if(EditorApplication.isPlaying)return; if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene("Assets/Opening/Intro.unity");EditorApplication.isPlaying=true;}
    [MenuItem("Capstone/Play Spring Only")]
    public static void PlaySpring(){if(EditorApplication.isPlaying)return; if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene("Assets/Scenes/Spring.unity");EditorApplication.isPlaying=true;}
}
