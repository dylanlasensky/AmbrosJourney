#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace AmbrosJourney.Presentation.Editor
{
    public static class PresentationBuildCheck
    {
        [MenuItem("Capstone/Presentation/Build macOS Validation Player")]
        public static void Build()
        {
            if(EditorApplication.isPlaying){Debug.LogWarning("Stop Play mode before building.");return;}
            PresentationTools.PrepareArt();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/SummerLevel/Summer.unity"},locationPathName=Path.GetFullPath("Temp/PresentationChecks/AmbrosPresentationCheck.app"),target=BuildTarget.StandaloneOSX,options=BuildOptions.Development});
            Directory.CreateDirectory("Temp/PresentationChecks");
            File.WriteAllText("Temp/PresentationChecks/Build.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\nSeconds: "+report.summary.totalTime.TotalSeconds);
            if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Presentation player build failed");
            Debug.Log("PRESENTATION_BUILD_PASS: "+report.summary.totalErrors+" errors; "+report.summary.totalWarnings+" warnings.");
        }
    }
}
#endif
