using System.Collections.Generic;
using UnityEngine;
namespace AmbrosJourney.Presentation
{
    public sealed class SeasonWorldLabel : MonoBehaviour
    {
        public string Caption;
        public static readonly List<SeasonWorldLabel> Active=new List<SeasonWorldLabel>();
        void OnEnable(){Active.Add(this);}
        void OnDisable(){Active.Remove(this);}
    }
}
