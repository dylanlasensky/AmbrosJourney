#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace AmbrosJourney.Journey.Editor
{
    public sealed class WorldcraftImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.EndsWith("/SeasonalRelics.png")&&!assetPath.EndsWith("/SeasonalDetails.png")&&!assetPath.EndsWith("/WinterRidge.png")&&!assetPath.EndsWith("/SeasonPortals.png"))return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Default;
            importer.isReadable=true;importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false;importer.filterMode=FilterMode.Point;
            importer.wrapMode=TextureWrapMode.Clamp;importer.maxTextureSize=2048;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
        }
    }
}
#endif
