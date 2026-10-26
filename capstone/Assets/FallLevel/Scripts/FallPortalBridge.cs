using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AmbrosJourney.Fall
{
    // Reuses Summer's runtime scene handoff. Spring and build settings stay untouched.
    public sealed class FallPortalBridge : MonoBehaviour
    {
        bool transitioning;
        AmbrosJourney.Summer.SummerLevel summer;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if(Object.FindAnyObjectByType<FallPortalBridge>())return;
            var host=new GameObject("Fall portal connector (runtime only)");Object.DontDestroyOnLoad(host);host.AddComponent<FallPortalBridge>();
        }
        public static bool GateAllowsEntry(AmbrosJourney.Summer.SummerLevel source)
        {return source && source.Progress.Complete && !source.InputBlocked && source.player && source.fallGate && Vector2.Distance(source.player.transform.position,source.fallGate.position)<1.1f;}
        void LateUpdate()
        {
            if(!AmbrosJourney.Journey.JourneySession.CanUsePortals)return;
            if(transitioning)return;
            if(!summer)summer=Object.FindAnyObjectByType<AmbrosJourney.Summer.SummerLevel>();
            if(GateAllowsEntry(summer))StartCoroutine(EnterFall());
        }
        IEnumerator EnterFall()
        {
            var prefab = Resources.Load<GameObject>("FallLevel");
            if (!prefab) { Debug.LogError("FALL: Missing Assets/FallLevel/Resources/FallLevel.prefab. Restore the FallLevel folder."); enabled = false; yield break; }
            AmbrosJourney.Journey.JourneySession.Instance?.Save();
            transitioning = true; summer.Transitioning=true;
            var source = SceneManager.GetActiveScene();
            var player = summer.player.transform;
            foreach (var motor in player.GetComponents<MonoBehaviour>())
                if (motor.GetType().Name == "SummerActor") motor.enabled = false;
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
            var destination = SceneManager.CreateScene("Fall"); SceneManager.SetActiveScene(destination);
            var world = Object.Instantiate(prefab); world.name = "Emberwood"; SceneManager.MoveGameObjectToScene(world, destination);
            yield return SceneManager.UnloadSceneAsync(source);
            for (float t = 0; t < .6f; t += Time.unscaledDeltaTime) { group.alpha = 1 - t / .6f; yield return null; }
            Object.Destroy(overlay); transitioning = false;
            Debug.Log("FALL_PORTAL_ENTERED: Summer completed; unlocked gate crossed; Fall loaded.");
        }
    }
}
