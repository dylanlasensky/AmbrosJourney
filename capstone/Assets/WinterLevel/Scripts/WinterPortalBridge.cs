using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AmbrosJourney.Winter
{
    // Reuses Summer's runtime scene handoff. Spring and build settings stay untouched.
    public sealed class WinterPortalBridge : MonoBehaviour
    {
        bool transitioning;
        AmbrosJourney.Fall.FallLevel fall;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if(Object.FindAnyObjectByType<WinterPortalBridge>())return;
            var host=new GameObject("Winter portal connector (runtime only)");Object.DontDestroyOnLoad(host);host.AddComponent<WinterPortalBridge>();
        }
        public static bool GateAllowsEntry(AmbrosJourney.Fall.FallLevel source)
        {return source && source.Progress.Complete && !source.InputBlocked && source.player && source.winterGate && Vector2.Distance(source.player.transform.position,source.winterGate.position)<1.1f;}
        void LateUpdate()
        {
            if(!AmbrosJourney.Journey.JourneySession.CanUsePortals)return;
            if(transitioning)return;
            if(!fall)fall=Object.FindAnyObjectByType<AmbrosJourney.Fall.FallLevel>();
            if(GateAllowsEntry(fall))StartCoroutine(EnterWinter());
        }
        IEnumerator EnterWinter()
        {
            var prefab = Resources.Load<GameObject>("WinterLevel");
            if (!prefab) { Debug.LogError("WINTER: Missing Assets/WinterLevel/Resources/WinterLevel.prefab. Restore the WinterLevel folder."); enabled = false; yield break; }
            AmbrosJourney.Journey.JourneySession.Instance?.Save();
            transitioning = true; fall.Transitioning=true;
            var source = SceneManager.GetActiveScene();
            var player = fall.player.transform;
            foreach (var motor in player.GetComponents<MonoBehaviour>())
                if (motor.GetType().Name == "FallActor") motor.enabled = false;
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
            var destination = SceneManager.CreateScene("Winter"); SceneManager.SetActiveScene(destination);
            var world = Object.Instantiate(prefab); world.name = "Frostveil"; SceneManager.MoveGameObjectToScene(world, destination);
            yield return SceneManager.UnloadSceneAsync(source);
            for (float t = 0; t < .6f; t += Time.unscaledDeltaTime) { group.alpha = 1 - t / .6f; yield return null; }
            Object.Destroy(overlay); transitioning = false;
            Debug.Log("WINTER_PORTAL_ENTERED: Fall completed; Winter loaded.");
        }
    }
}
