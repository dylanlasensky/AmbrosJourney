using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using AmbrosJourney.Summer;
using AmbrosJourney.Fall;
using AmbrosJourney.Presentation.Editor;

// Run in a disposable checkout: enters Play mode, exercises the real portals, then exits.
[InitializeOnLoad]
public static class SeasonIntegrationChecks
{
    const string Key = "SeasonIntegrationChecks.";
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static double readyAt;
    static readonly MethodInfo SignalTick = typeof(EditorApplication).GetMethod("SignalTick", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
    static SeasonIntegrationChecks()
    {
        EditorApplication.update += Tick;
        Application.logMessageReceived += (message, stack, type) =>
        {
            if (SessionState.GetBool(Key + "active", false) &&
                (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
                SessionState.SetString(Key + "error", message);
        };
    }
    public static void Run()
    {
        Directory.CreateDirectory("Logs/SeasonChecks");
        File.WriteAllText("Logs/SeasonChecks/Integration.txt", "RUNNING\n");
        SessionState.SetString(Key + "error", "");
        SessionState.SetInt(Key + "stage", 0);
        SessionState.SetBool(Key + "active", true);
        SessionState.SetString(Key + "deadline", DateTime.UtcNow.AddMinutes(3).ToString("O"));
        SummerToolsValidate();
        FallToolsValidate();
        PresentationTools.ValidateMath();
        EditorSceneManager.OpenScene("Assets/Scenes/Spring.unity");
        EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();
        Application.runInBackground = true;
        EditorApplication.isPlaying = true;
    }
    static void SummerToolsValidate() => AmbrosJourney.Summer.Editor.SummerTools.Validate();
    static void FallToolsValidate() => AmbrosJourney.Fall.Editor.FallTools.Validate();
    static void Check(bool value, string message)
    { if (!value) throw new Exception(message); }
    static void Set(object obj, string name, object value) => obj.GetType().GetField(name, Private).SetValue(obj, value);
    static void Call(object obj, string name, params object[] args) => obj.GetType().GetMethod(name, Private).Invoke(obj, args);
    static void Tick()
    {
        if (!SessionState.GetBool(Key + "active", false)) return;
        try
        {
            string error = SessionState.GetString(Key + "error", "");
            Check(error == "", error);
            Check(DateTime.UtcNow < DateTime.Parse(SessionState.GetString(Key + "deadline", "")).ToUniversalTime(), "Timed out at stage " + SessionState.GetInt(Key + "stage", 0) + "; frame " + Time.frameCount);
            Application.runInBackground = true;
            EditorApplication.QueuePlayerLoopUpdate();
            SignalTick?.Invoke(null, null);
            if (!EditorApplication.isPlaying) return;
            if (readyAt == 0) readyAt = EditorApplication.timeSinceStartup + 3;
            if (EditorApplication.timeSinceStartup < readyAt) return;
            int stage = SessionState.GetInt(Key + "stage", 0);
            if (stage == 0)
            {
                var g = UnityEngine.Object.FindAnyObjectByType<SpringGuide>();
                Check(g, "Spring scene has its guide");
                var gate = GameObject.Find("Summer Gate (unlocks after 80 percent)").transform;
                Check(!SummerPortalBridge.GateAllowsEntry(g, gate.position, gate.position), "Locked Spring gate blocks entry");
                Set(g, "target", -1); for (int i = 0; i < 3; i++) Call(g, "Continue");
                for (int i = 0; i < 3; i++) { Set(g, "target", 5 + i); Call(g, "Continue"); }
                int[][] answers = {new[]{0,0}, new[]{1,0}, new[]{1,0}};
                for (int i = 0; i < 3; i++) { Set(g, "target", i); foreach (int a in answers[i]) { Call(g, "Answer", a); Call(g, "Continue"); } }
                Set(g, "target", 3); Set(g, "bossQuestion", 0); Set(g, "feedback", ""); Set(g, "bossResult", false);
                foreach (int a in new[]{0,0,1,1,0}) { Call(g, "Answer", a); Call(g, "Continue"); }
                Check(!g.SpringComplete && g.BossScore == 3, "Spring 3/5 fails"); Call(g, "Continue");
                foreach (int a in new[]{0,0,1,0,0}) { Call(g, "Answer", a); Call(g, "Continue"); }
                Check(g.SpringComplete && g.BossScore == 4, "Spring 4/5 passes"); Call(g, "Continue");
                Check(!SummerPortalBridge.GateAllowsEntry(g, gate.position + Vector3.up * 5, gate.position), "Spring gate requires proximity");
                typeof(SpringGuide).GetProperty("DialogueOpen").SetValue(g, true);
                Check(!SummerPortalBridge.GateAllowsEntry(g, gate.position, gate.position), "Spring modal blocks transition");
                typeof(SpringGuide).GetProperty("DialogueOpen").SetValue(g, false);
                g.player.position = gate.position; g.player.GetComponent<Rigidbody2D>().position = gate.position;
                Advance(1);
            }
            else if (stage == 1 && SceneManager.GetActiveScene().name == "Summer")
            {
                var l = UnityEngine.Object.FindAnyObjectByType<SummerLevel>();
                Check(l && l.InputBlocked, "Summer loads and shows introduction");
                Check(!UnityEngine.Object.FindAnyObjectByType<SpringGuide>(), "Spring unloads after transition");
                Check(!FallPortalBridge.GateAllowsEntry(l), "Incomplete Summer cannot exit");
                PresentationTools.ValidateRoutes();
                PresentationPlayChecks.Summer();
                Check(File.ReadAllText("Temp/PresentationChecks/SummerPresentationChecks.txt").StartsWith("PASS"), "Summer UI checks pass");
                l.player.transform.position = l.fallGate.position; l.player.GetComponent<Rigidbody2D>().position = l.fallGate.position;
                Check(!FallPortalBridge.GateAllowsEntry(l), "Summer gate conversation blocks exit");
                Call(l, "Close"); Call(l, "ToggleJournal");
                Check(!FallPortalBridge.GateAllowsEntry(l), "Summer journal blocks exit"); Call(l, "ToggleJournal");
                Check(FallPortalBridge.GateAllowsEntry(l), "Completed Summer permits entry");
                Advance(2);
            }
            else if (stage == 2 && SceneManager.GetActiveScene().name == "Fall")
            {
                var l = UnityEngine.Object.FindAnyObjectByType<FallLevel>();
                Check(l && l.InputBlocked, "Fall loads and shows introduction");
                Check(!UnityEngine.Object.FindAnyObjectByType<SummerLevel>(), "Summer unloads after transition");
                AmbrosJourney.Fall.Editor.FallTools.ValidateRoutes();
                PresentationTools.ValidateRoutes();
                PresentationPlayChecks.Fall();
                Check(File.ReadAllText("Temp/PresentationChecks/FallPresentationChecks.txt").StartsWith("PASS"), "Fall UI checks pass");
                Check(l.Progress.Complete && l.ember.follow == l.femaleWarrior.transform, "Fall completed and Emberfist recruited");
                Advance(3);
            }
            else if (stage == 3) Finish(true, "Spring -> Summer -> Fall transitions, modal guards, progression, UI, routes and followers passed.");
        }
        catch (Exception e) { Finish(false, e.ToString()); }
    }
    static void Advance(int stage) { Debug.Log("SEASON_CHECK_STAGE: " + stage); SessionState.SetInt(Key + "stage", stage); readyAt = EditorApplication.timeSinceStartup + 4; }
    static void Finish(bool passed, string message)
    {
        SessionState.SetBool(Key + "active", false);
        File.WriteAllText("Logs/SeasonChecks/Integration.txt", (passed ? "PASS" : "FAIL") + "\n" + message);
        Debug.Log("SEASON_INTEGRATION_" + (passed ? "PASS: " : "FAIL: ") + message);
        EditorApplication.Exit(passed ? 0 : 1);
    }
    public static void Build()
    {
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[]{"Assets/Opening/Intro.unity", "Assets/Scenes/Spring.unity"},
            locationPathName = "Logs/SeasonChecks/AmbrosJourney.app", target = BuildTarget.StandaloneOSX,
            options = BuildOptions.Development
        });
        Directory.CreateDirectory("Logs/SeasonChecks");
        File.WriteAllText("Logs/SeasonChecks/Build.txt", report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings);
        Check(report.summary.result == BuildResult.Succeeded, "Connected game build failed");
    }
}
