// Copies the original cinematic files into Web StreamingAssets after a successful build.
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
// Unity Web does not support embedded VideoClips; package the team's original MP4s as URLs.
public sealed class JourneyWebVideoBuild : IPostprocessBuildWithReport
{
    public int callbackOrder=>100;
    public void OnPostprocessBuild(BuildReport report)
    {
        if(report.summary.platform!=BuildTarget.WebGL)return;
        string destination=Path.Combine(report.summary.outputPath,"StreamingAssets","JourneyVideos");
        Directory.CreateDirectory(destination);
        foreach(string name in new[]{"Intro","Outro"})
        {
            string source=Path.Combine("Assets","Journey","Resources","Journey",name+".mp4");
            if(!File.Exists(source))throw new BuildFailedException("Missing journey video: "+source);
            File.Copy(source,Path.Combine(destination,name+".mp4"),true);
        }
    }
}
#endif
