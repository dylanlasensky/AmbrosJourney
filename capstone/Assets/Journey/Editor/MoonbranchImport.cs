#if UNITY_EDITOR
using UnityEditor;
namespace AmbrosJourney.Journey.Editor
{
    public sealed class MoonbranchImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.Contains("/Journey/Moonbranch"))return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Default;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
            importer.filterMode=UnityEngine.FilterMode.Bilinear;
            importer.wrapMode=UnityEngine.TextureWrapMode.Clamp;
            importer.maxTextureSize=1024;importer.textureCompression=TextureImporterCompression.Uncompressed;
        }
    }
}
#endif
