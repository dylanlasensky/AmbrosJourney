using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AmbrosJourney.Summer
{
    // Installs itself at runtime, leaving Spring.unity, SpringGuide.cs and build settings intact.
    public sealed class SummerPortalBridge : MonoBehaviour
    {
        bool transitioning;
        MonoBehaviour spring;
        Transform gate;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (Object.FindAnyObjectByType<SummerPortalBridge>()) return;
            var host = new GameObject("Summer portal connector (runtime only)");
            Object.DontDestroyOnLoad(host); host.AddComponent<SummerPortalBridge>();
        }
        // Optional adapter: Summer also compiles in the team's older checkout without Spring scripts.
        static bool Flag(MonoBehaviour guide, string property)
        { return guide && guide.GetType().GetProperty(property)?.GetValue(guide) is bool value && value; }
        static Transform Player(MonoBehaviour guide)
        { return guide ? guide.GetType().GetField("player")?.GetValue(guide) as Transform : null; }
        public static bool GateAllowsEntry(MonoBehaviour guide, Vector2 playerPosition, Vector2 gatePosition)
        { return guide && Vector2.Distance(playerPosition, gatePosition) < 1.1f && Flag(guide, "SpringComplete") && !Flag(guide, "DialogueOpen"); }
        void LateUpdate()
        {
            if(!AmbrosJourney.Journey.JourneySession.CanUsePortals)return;
            if (transitioning || SceneManager.GetActiveScene().name != "Spring") return;
            if (!spring)
                foreach (var behaviour in Object.FindObjectsByType<MonoBehaviour>())
                    if (behaviour.GetType().Name == "SpringGuide") { spring = behaviour; break; }
            if (!gate) { var found = GameObject.Find("Summer Gate (unlocks after 80 percent)"); if (found) gate = found.transform; }
            var player = Player(spring);
            if (player && gate && GateAllowsEntry(spring, player.position, gate.position))
                StartCoroutine(EnterSummer());
        }
        IEnumerator EnterSummer()
        {
            var prefab = Resources.Load<GameObject>("SummerLevel");
            if (!prefab) { Debug.LogError("SUMMER: Missing Assets/SummerLevel/Resources/SummerLevel.prefab. Restore the new SummerLevel folder."); enabled = false; yield break; }
            AmbrosJourney.Journey.JourneySession.Instance?.Save();
            transitioning = true;
            var source = SceneManager.GetActiveScene();
            var player = Player(spring);
            foreach (var motor in player.GetComponents<MonoBehaviour>())
                if (motor.GetType().Name == "SpringPlayer") motor.enabled = false;
            var body = player.GetComponent<Rigidbody2D>(); if (body) body.linearVelocity = Vector2.zero;
            var overlay = new GameObject("Season transition", typeof(Canvas), typeof(CanvasGroup));
            overlay.transform.SetParent(transform);
            overlay.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            overlay.GetComponent<Canvas>().sortingOrder = 30000;
            var image = new GameObject("Fade", typeof(RectTransform), typeof(UnityEngine.UI.Image)); image.transform.SetParent(overlay.transform, false);
            var rect = image.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            image.GetComponent<UnityEngine.UI.Image>().color = new Color(.09f, .13f, .10f);
            var group = overlay.GetComponent<CanvasGroup>();
            for (float t = 0; t < .5f; t += Time.unscaledDeltaTime) { group.alpha = t / .5f; yield return null; }
            group.alpha = 1;
            foreach (var root in source.GetRootGameObjects())
            {
                foreach (var listener in root.GetComponentsInChildren<AudioListener>()) listener.enabled = false;
                foreach (var camera in root.GetComponentsInChildren<Camera>()) camera.enabled = false;
            }
            var destination = SceneManager.CreateScene("Summer"); SceneManager.SetActiveScene(destination);
            var world = Object.Instantiate(prefab); world.name = "Summer Valley"; SceneManager.MoveGameObjectToScene(world, destination);
            yield return SceneManager.UnloadSceneAsync(source);
            for (float t = 0; t < .6f; t += Time.unscaledDeltaTime) { group.alpha = 1 - t / .6f; yield return null; }
            Object.Destroy(overlay); transitioning = false;
            Debug.Log("SUMMER_PORTAL_ENTERED: Spring completed; existing gate crossed; Summer loaded.");
        }
    }
}
