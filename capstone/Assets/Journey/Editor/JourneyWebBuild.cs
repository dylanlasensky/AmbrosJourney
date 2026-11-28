// Builds and validates QA/release Web players and packages the itch.io ZIP.
#if UNITY_EDITOR
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AmbrosJourney.Journey.Editor
{
    public static class JourneyWebBuild
    {
        [MenuItem("Ambros Journey/Build/Web QA")]
        public static void BuildQA() => Build(true);

        [MenuItem("Ambros Journey/Build/Web Submission")]
        public static void BuildSubmission() => Build(false);

        static void Build(bool development)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new BuildFailedException("Install Web Build Support for Unity " + Application.unityVersion + ".");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                throw new BuildFailedException("Switch to Web before building, or launch the Editor with -buildTarget WebGL.");

            string output = OutputPath(development);
            if (File.Exists(output) || (Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any()))
                throw new BuildFailedException("Choose an empty Web output directory to avoid packaging stale files: " + output);
            if (File.Exists(output + ".zip"))
                throw new BuildFailedException("An archive already exists for this output. Choose a new output directory.");

            string oldTemplate = PlayerSettings.WebGL.template;
            WebGLCompressionFormat oldCompression = PlayerSettings.WebGL.compressionFormat;
            bool oldFallback = PlayerSettings.WebGL.decompressionFallback;
            bool oldDiagnostics = PlayerSettings.WebGL.showDiagnostics;
            try
            {
                PlayerSettings.WebGL.template = "PROJECT:AmbrosJourney";
                PlayerSettings.WebGL.compressionFormat = development ? WebGLCompressionFormat.Disabled : WebGLCompressionFormat.Gzip;
                PlayerSettings.WebGL.decompressionFallback = false;
                PlayerSettings.WebGL.showDiagnostics = false;
                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { "Assets/Opening/Intro.unity", "Assets/Scenes/Spring.unity" },
                    locationPathName = output,
                    target = BuildTarget.WebGL,
                    options = development ? BuildOptions.Development : BuildOptions.None
                });
                if (report.summary.result != BuildResult.Succeeded || report.summary.totalErrors != 0)
                    throw new BuildFailedException("Web build failed: " + report.summary.result + ", errors: " + report.summary.totalErrors);
                if (((report.summary.options & BuildOptions.Development) != 0) != development)
                    throw new BuildFailedException("The generated build does not match the requested QA/submission mode.");

                ValidateFiles(output, !development);
                File.WriteAllText(output + ".build.txt",
                    "Unity: " + Application.unityVersion + "\nMode: " + (development ? "QA (development)" : "Submission (release)") +
                    "\nResult: " + report.summary.result + "\nErrors: " + report.summary.totalErrors +
                    "\nWarnings: " + report.summary.totalWarnings + "\nSize: " + report.summary.totalSize + " bytes\n" +
                    (development ? "Compression: none (development build).\n" : "Compression: gzip; host must serve Content-Encoding: gzip.\n") +
                    "Browser playtesting is a separate check; a successful build does not prove it.\n");
                if (!development)
                    ZipFile.CreateFromDirectory(output, output + ".zip", System.IO.Compression.CompressionLevel.Optimal, false);
                Debug.Log("Web " + (development ? "QA" : "submission") + " export complete: " + output);
            }
            finally
            {
                PlayerSettings.WebGL.template = oldTemplate;
                PlayerSettings.WebGL.compressionFormat = oldCompression;
                PlayerSettings.WebGL.decompressionFallback = oldFallback;
                PlayerSettings.WebGL.showDiagnostics = oldDiagnostics;
            }
        }

        static string OutputPath(bool development)
        {
            string[] args = Environment.GetCommandLineArgs();
            string path = null;
            for (int i = 0; i < args.Length; i++)
                if (args[i] == "-buildOutput")
                {
                    if (i + 1 >= args.Length || args[i + 1].StartsWith("-"))
                        throw new BuildFailedException("-buildOutput needs an output directory.");
                    path = args[++i];
                }
            if (string.IsNullOrWhiteSpace(path))
                path = Path.Combine("Builds", "Web", (development ? "QA-" : "Submission-") + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
            path = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string project = Path.GetFullPath(".");
            if (path == project || path == Path.GetPathRoot(path).TrimEnd(Path.DirectorySeparatorChar))
                throw new BuildFailedException("Build into a separate output directory.");
            foreach (string reserved in new[] { "Assets", "Packages", "ProjectSettings", "Library" })
            {
                string folder = Path.Combine(project, reserved);
                if (path.Equals(folder, StringComparison.OrdinalIgnoreCase) || path.StartsWith(folder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    throw new BuildFailedException("Do not export a build into " + reserved + ".");
            }
            return path;
        }

        static void ValidateFiles(string output, bool submission)
        {
            if (!File.Exists(Path.Combine(output, "index.html")))
                throw new BuildFailedException("Web export has no index.html.");
            string build = Path.Combine(output, "Build");
            // Unity development Web builds are uncompressed, even when release compression is configured.
            string suffix = submission ? ".gz" : "";
            foreach (string pattern in new[] { "*.loader.js", "*.framework.js" + suffix, "*.wasm" + suffix, "*.data" + suffix })
                if (!Directory.Exists(build) || !Directory.EnumerateFiles(build, pattern).Any())
                    throw new BuildFailedException("Web export is missing " + pattern);
            foreach (string name in new[] { "Intro", "Outro" })
            {
                string source = Path.Combine("Assets", "Journey", "Resources", "Journey", name + ".mp4");
                string exported = Path.Combine(output, "StreamingAssets", "JourneyVideos", name + ".mp4");
                if (!File.Exists(exported) || Hash(source) != Hash(exported))
                    throw new BuildFailedException("The original " + name + " video was not packaged correctly.");
            }
            var files = Directory.GetFiles(output, "*", SearchOption.AllDirectories);
            long size = files.Sum(file => new FileInfo(file).Length);
            bool fitsItch = files.Length <= 1000 && size <= 500000000 &&
                files.All(file => new FileInfo(file).Length <= 200000000 && file.Substring(output.Length + 1).Length <= 240);
            if (!fitsItch && submission)
                throw new BuildFailedException("Export exceeds itch.io's default HTML upload limits. Inspect file sizes before packaging.");
        }

        static string Hash(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(stream));
        }
    }
}
#endif
