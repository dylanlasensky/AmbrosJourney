// Builds release desktop players from the same entry scenes used by the Web export.
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AmbrosJourney.Journey.Editor
{
    public static class JourneyDesktopBuild
    {
        [MenuItem("Ambros Journey/Build/macOS Submission")]
        public static void BuildMac() => Build(BuildTarget.StandaloneOSX, "AmbrosJourney.app");
        [MenuItem("Ambros Journey/Build/Windows Submission")]
        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "AmbrosJourney.exe");

        static void Build(BuildTarget target, string playerName)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, target))
                throw new BuildFailedException("Install " + target + " support for Unity " + Application.unityVersion + ".");
            if (EditorUserBuildSettings.activeBuildTarget != target)
                throw new BuildFailedException("Switch the active build target to " + target + " first.");
            string[] args = Environment.GetCommandLineArgs();
            int flag = Array.IndexOf(args, "-buildOutput");
            string output = flag >= 0 && flag + 1 < args.Length ? args[flag + 1] : Path.Combine("Builds", target.ToString(), DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"), playerName);
            output = Path.GetFullPath(output);
            if (!output.EndsWith(Path.GetExtension(playerName), StringComparison.OrdinalIgnoreCase))
                throw new BuildFailedException("Output must end with " + Path.GetExtension(playerName) + ".");
            string folder = Path.GetDirectoryName(output);
            string project = Path.GetFullPath(".");
            // Refuse project/asset locations and populated destinations so another export cannot be overwritten.
            if (folder == Path.GetPathRoot(folder) || folder == project || new[] { "Assets", "Packages", "ProjectSettings", "Library" }.Any(name => folder.Equals(Path.Combine(project, name), StringComparison.OrdinalIgnoreCase) || folder.StartsWith(Path.Combine(project, name) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)))
                throw new BuildFailedException("Choose a separate desktop output directory.");
            if (Directory.Exists(folder) && Directory.EnumerateFileSystemEntries(folder).Any())
                throw new BuildFailedException("Desktop output directory must be empty: " + folder);
            Directory.CreateDirectory(folder);
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Opening/Intro.unity", "Assets/Scenes/Spring.unity" },
                locationPathName = output,
                target = target,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded || report.summary.totalErrors != 0 || (!File.Exists(output) && !Directory.Exists(output)))
                throw new BuildFailedException("Desktop build failed: " + report.summary.result + ", errors: " + report.summary.totalErrors);
            File.WriteAllText(folder + ".build.txt", "Unity: " + Application.unityVersion + "\nTarget: " + target + "\nResult: " + report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings + "\nSize: " + report.summary.totalSize + " bytes\nRelease build; platform playtesting is a separate check.\n");
            Debug.Log("Desktop export complete: " + output);
        }
    }
}
#endif
