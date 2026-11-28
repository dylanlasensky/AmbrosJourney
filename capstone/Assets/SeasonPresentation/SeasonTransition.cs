// Fades between chapters while blocking competing input and portal entries.
using UnityEngine;
namespace AmbrosJourney.Presentation
{
    // Lives on the persistent fade overlay, covering both source and destination scenes.
    public sealed class SeasonTransition : MonoBehaviour
    {
        static int overlays;
        public static bool Active=>overlays>0;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset()=>overlays=0;
        void OnEnable()=>overlays++;
        void OnDisable()=>overlays=Mathf.Max(0,overlays-1);
    }
}
