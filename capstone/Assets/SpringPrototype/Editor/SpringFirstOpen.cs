using System.IO;
using UnityEditor;
[InitializeOnLoad]
public static class SpringFirstOpen
{
    static SpringFirstOpen()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists("Assets/Scenes/Spring.unity") && !EditorApplication.isPlayingOrWillChangePlaymode)
                SpringSceneBuilder.Build();
        };
    }
}
