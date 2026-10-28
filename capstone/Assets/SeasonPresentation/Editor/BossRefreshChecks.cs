#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using AmbrosJourney.Presentation;
namespace AmbrosJourney.Presentation.Editor
{
    public static class BossRefreshChecks
    {
        [MenuItem("Capstone/Bosses/Prepare Seasonal Artwork")]
        public static void Prepare()
        {
            QuestionRefreshChecks.PrepareFont();
            foreach(string name in new[]{"SpringBlossomAtlas","SummerSuncrestAtlas","WinterFrostboundAtlas","FinalEchoKeeperAtlas"})
            {
                string path="Assets/Journey/Resources/Journey/Bosses/"+name+".png";
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;
                importer.alphaIsTransparency=true;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.isReadable=true;
                importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();Debug.Log("SEASONAL_BOSS_ART_READY: four transparent point-filtered atlases.");
        }
        public static void Run(){Prepare();QuestionRefreshChecks.Run();}
        public static void Build()
        {
            var report=UnityEditor.BuildPipeline.BuildPlayer(new UnityEditor.BuildPlayerOptions{scenes=new[]{"Assets/Opening/Intro.unity","Assets/Scenes/Spring.unity"},locationPathName="Logs/BossChecks/AmbrosJourney-BossPreview.app",target=UnityEditor.BuildTarget.StandaloneOSX,options=UnityEditor.BuildOptions.None});
            Directory.CreateDirectory("Logs/BossChecks");File.WriteAllText("Logs/BossChecks/Build.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Seasonal boss player build failed");
        }
        public static void CheckAndCapture(SeasonBossVisual boss,string name)
        {
            if(!boss||boss.IdleFrames.Length!=2||boss.AttackFrames.Length!=2)throw new Exception("Boss needs idle and attack poses: "+name);
            if(boss.SamplePose(0,false)==boss.SamplePose(.5f,false)||boss.SamplePose(.05f,true)==boss.SamplePose(.4f,true))throw new Exception("Animation poses must differ: "+name);
            if(!boss.GetComponent<SpriteRenderer>().sharedMaterial.mainTexture.name.Contains(boss.ResourceName))throw new Exception("Wrong boss material: "+name);
            var actor=boss.GetComponent<AmbrosJourney.Summer.SummerActor>();var winter=boss.GetComponent<AmbrosJourney.Winter.WinterActor>();
            if(actor)actor.Strike();else if(winter)winter.Strike();else boss.Strike();
            if(!boss.PlayingAttack)throw new Exception("Strike signal did not reach boss artwork: "+name);
            Directory.CreateDirectory("Logs/BossChecks");var camera=Camera.main;var position=camera.transform.position;float oldSize=camera.orthographicSize,aspect=camera.aspect;
            var oldTarget=camera.targetTexture;var active=RenderTexture.active;var canvases=UnityEngine.Object.FindObjectsByType<Canvas>();var enabled=canvases.Select(c=>c.enabled).ToArray();
            var rt=new RenderTexture(1280,720,24);var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try
            {
                foreach(var canvas in canvases)canvas.enabled=false;
                camera.transform.position=boss.transform.position+new Vector3(0,boss.CellWorldWidth*.35f,-10);camera.orthographicSize=boss.CellWorldWidth*.7f;camera.aspect=1280f/720;camera.targetTexture=rt;
                for(int pose=0;pose<4;pose++)
                {
                    boss.PreviewPose(pose%2==0?.05f:.45f,pose>=2);camera.Render();RenderTexture.active=rt;
                    pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();File.WriteAllBytes("Logs/BossChecks/"+name+"-"+pose+".png",pixels.EncodeToPNG());
                }
            }
            finally{camera.transform.position=position;camera.orthographicSize=oldSize;camera.aspect=aspect;camera.targetTexture=oldTarget;RenderTexture.active=active;for(int i=0;i<canvases.Length;i++)canvases[i].enabled=enabled[i];UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(pixels);}
            Debug.Log("SEASONAL_BOSS_PASS: "+name+", two idle poses, distinct windup/impact and strike signal; four in-world captures.");
        }
    }
}
#endif
