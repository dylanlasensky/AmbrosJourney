#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AmbrosJourney.Summer.Editor
{
    public static class SummerTools
    {
        const string ScenePath = "Assets/SummerLevel/Summer.unity";
        [MenuItem("Capstone/Summer/Play Summer Only")]
        public static void Play()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            foreach (var scene in EnumerableScenes())
                if (scene.isDirty) { Debug.LogWarning("Save your current work before opening Summer. Summer tools do not save or edit existing scenes."); return; }
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.isPlaying = true;
        }
        [MenuItem("Capstone/Summer/Open Summer Scene")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            foreach (var scene in EnumerableScenes())
                if (scene.isDirty) { Debug.LogWarning("Save your current work first. Existing scenes are left untouched."); return; }
            EditorSceneManager.OpenScene(ScenePath);
        }
        static System.Collections.Generic.IEnumerable<UnityEngine.SceneManagement.Scene> EnumerableScenes()
        { for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++) yield return UnityEngine.SceneManagement.SceneManager.GetSceneAt(i); }

        [MenuItem("Capstone/Summer/Validate Summer Progression")]
        public static void Validate()
        {
            var p = new SummerProgress();
            Require(!p.BeginBoss(), "Orc locked before introduction");
            Require(!p.AnswerWorkshop(0), "Workshop waits for introduction");
            p.Introduced = true;
            Require(!p.AnswerWorkshop(1) && p.WorkshopQuestion == 0, "Wrong workshop answer retries");
            Require(p.AnswerWorkshop(0) && p.AnswerWorkshop(1) && p.Wards[0], "Workshop complete");
            Require(!p.ChooseHint(1), "Copying is explained before practice");
            Require(p.ChooseHint(0), "Hint chosen");
            Require(!p.StepOnStone(1) && p.PatternStep == 0, "Pattern mistake retries");
            foreach (int stone in SummerContent.Pattern) Require(p.StepOnStone(stone), "Pattern step");
            Require(!p.AnswerGarden(1), "Garden misconception retries"); Require(p.AnswerGarden(0), "Practice ward earned");
            Require(!p.BeginBoss(), "All three wards required");
            p.AnswerMessage(0); p.AnswerMessage(0); Require(!p.PrivacyComplete, "Duplicate removal cannot finish privacy");
            p.AnswerMessage(1); p.AnswerMessage(2); Require(p.PrivacyComplete, "All private details removed");
            Require(!p.AnswerMessage(0), "Friendship misconception retries"); Require(p.AnswerMessage(1), "Message ward earned");
            Require(p.WardCount == 3 && p.BeginBoss(), "Orc opens after missions");
            for (int i = 0; i < 5; i++) p.AnswerBoss(i < 3 ? SummerContent.Boss[i].Correct : 1-SummerContent.Boss[i].Correct);
            Require(p.BossFinished && !p.Complete && p.BossScore == 3, "60 percent fails");
            Require(p.WardCount == 3 && p.BeginBoss(), "Retry preserves all missions");
            for (int i = 0; i < 5; i++) p.AnswerBoss(i < 4 ? SummerContent.Boss[i].Correct : 1-SummerContent.Boss[i].Correct);
            Require(p.Complete && p.BossScore == 4 && !p.BeginBoss(), "80 percent passes and is final");
            for (int i = 0; i < 9; i++) p.LoseCourage();
            Require(p.Courage > 0 && p.WardCount == 3 && p.Complete, "Courage recovery preserves progress");
            Require(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SummerLevel/Resources/SummerLevel.prefab"), "Portal prefab present");
            Debug.Log("SUMMER_VALIDATION_PASS: lesson gates, pattern retry, privacy removal, 60% failure, 80% success and progress preservation.");
        }
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException("SUMMER VALIDATION: " + message); }
    }
}
#endif
