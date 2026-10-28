#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace AmbrosJourney.Presentation.Editor
{
    public static class QuestionRefreshChecks
    {
        [MenuItem("Capstone/Questions/Prepare Determination Font")]
        public static void PrepareFont()
        {
            const string target="Assets/SeasonPresentation/Resources/Determination.asset";
            var existing=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(target);if(existing){NormalizeLeading(existing);AssetDatabase.SaveAssets();return;}
            var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/Text Font/determination.ttf");
            if(!source)throw new InvalidOperationException("The supplied determination.ttf is missing");
            Directory.CreateDirectory("Assets/SeasonPresentation/Resources");AssetDatabase.Refresh();
            var font=TMP_FontAsset.CreateFontAsset(source,72,7,GlyphRenderMode.SDFAA,2048,2048,AtlasPopulationMode.Dynamic,true);
            font.name="Determination";
            string characters=new string(Enumerable.Range(32,95).Select(c=>(char)c).ToArray())+"éëöäüñÉËÖÄÜÑ·×";
            string missing;font.TryAddCharacters(characters,out missing);
            if(missing.Any(c=>c>=32&&c<=126))throw new InvalidOperationException("Determination is missing required characters: "+missing);
            font.atlasPopulationMode=AtlasPopulationMode.Static;NormalizeLeading(font);
            AssetDatabase.CreateAsset(font,target);
            font.material.name="Determination Material";AssetDatabase.AddObjectToAsset(font.material,font);
            foreach(var atlas in font.atlasTextures){atlas.name="Determination Atlas";AssetDatabase.AddObjectToAsset(atlas,font);}
            EditorUtility.SetDirty(font);AssetDatabase.SaveAssets();
            Debug.Log("DETERMINATION_READY: static font asset; existing UI point sizes retained.");
        }
        static void NormalizeLeading(TMP_FontAsset font)
        {
            // The TTF reserves a large blank band over capitals. Trim that leading, not the glyph scale.
            var face=font.faceInfo;face.scale=1;face.ascentLine=face.capLine+face.pointSize*.08f;
            face.lineHeight=face.ascentLine-face.descentLine;font.faceInfo=face;EditorUtility.SetDirty(font);
        }
        static double startAt;
        public static void Run(){PrepareFont();startAt=EditorApplication.timeSinceStartup+5;EditorApplication.update+=StartWhenReady;}
        static void StartWhenReady()
        {
            if(EditorApplication.timeSinceStartup<startAt||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            EditorApplication.update-=StartWhenReady;SeasonIntegrationChecks.Run();
        }
    }
}
#endif
