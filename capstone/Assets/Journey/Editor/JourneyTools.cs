#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace AmbrosJourney.Journey.Editor
{
    public static class JourneyTools
    {
        [MenuItem("Capstone/Journey/Prepare Art and Player")]
        public static void Prepare()
        {
            RemoveRetiredOpening();
            foreach(string name in new[]{"WinterNature","VillageDetails","SeasonCottages","FieldCard","DiscoveryDetails","UtilityDetails"})
            {
                string file="Assets/Journey/Resources/Journey/"+name+".png";
                AssetDatabase.ImportAsset(file,ImportAssetOptions.ForceSynchronousImport);
                var art=(TextureImporter)AssetImporter.GetAtPath(file);art.textureType=TextureImporterType.Default;art.filterMode=FilterMode.Point;art.mipmapEnabled=false;art.alphaIsTransparency=true;art.isReadable=true;art.maxTextureSize=1024;art.textureCompression=TextureImporterCompression.Uncompressed;art.SaveAndReimport();
            }
            const string menuPath="Assets/Journey/Resources/Journey/MenuLandscape.png";
            AssetDatabase.ImportAsset(menuPath,ImportAssetOptions.ForceSynchronousImport);
            var menu=(TextureImporter)AssetImporter.GetAtPath(menuPath);menu.textureType=TextureImporterType.Default;menu.filterMode=FilterMode.Bilinear;menu.mipmapEnabled=false;menu.wrapMode=TextureWrapMode.Clamp;menu.maxTextureSize=2048;menu.textureCompression=TextureImporterCompression.CompressedHQ;menu.SaveAndReimport();
            const string path="Assets/Journey/Resources/Journey/WorldAtlas.png";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            const string sentinelPath="Assets/Journey/Resources/Journey/SentinelAtlas.png";AssetDatabase.ImportAsset(sentinelPath,ImportAssetOptions.ForceSynchronousImport);
            var sentinel=(TextureImporter)AssetImporter.GetAtPath(sentinelPath);sentinel.textureType=TextureImporterType.Default;sentinel.filterMode=FilterMode.Point;sentinel.mipmapEnabled=false;sentinel.alphaIsTransparency=true;sentinel.maxTextureSize=512;sentinel.textureCompression=TextureImporterCompression.Uncompressed;sentinel.SaveAndReimport();
            PlayerSettings.runInBackground=false;PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            AssetDatabase.SaveAssets();Debug.Log("JOURNEY_PREPARED");
        }
        static void RemoveRetiredOpening()
        {
            const string scenePath="Assets/Opening/Intro.unity";
            if(!System.IO.File.ReadAllText(scenePath).Contains("d0648432c3a0e40de8f6f27bce8a3cbf"))return;
            var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            var opening=Object.FindAnyObjectByType<OpeningCinematic>();opening.clip=null;opening.GetComponent<UnityEngine.Video.VideoPlayer>().clip=null;
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        }
    }
}
#endif
