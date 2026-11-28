// Loads and shares the baked Determination TMP font across runtime UI.
using TMPro;
using UnityEngine;
namespace AmbrosJourney.Presentation
{
    public static class GameFont
    {
        static TMP_FontAsset determination;
        // Loads and shares the baked Determination TMP font across runtime UI.
        public static TMP_FontAsset Resolve(TMP_FontAsset fallback=null)
        {
            if(!determination)determination=Resources.Load<TMP_FontAsset>("Determination");
            return determination?determination:fallback?fallback:Resources.Load<TMP_FontAsset>("SummerDefault");
        }
    }
}
