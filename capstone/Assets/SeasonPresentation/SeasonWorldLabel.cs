using System.Collections.Generic;
using UnityEngine;
namespace AmbrosJourney.Presentation
{
    public sealed class SeasonWorldLabel : MonoBehaviour
    {
        public string Caption;
        public float Width=310;
        SpriteRenderer anchor;
        public bool Anchored=>anchor;
        public bool Visible=>!anchor||anchor.enabled;
        public Vector3 Position=>anchor?new Vector3(anchor.bounds.center.x,anchor.bounds.max.y+.3f,anchor.transform.position.z):transform.position;
        // Keep one caption attached to the thing it describes, including walking guides.
        public static SeasonWorldLabel Attach(Transform target,string caption,float width=310)
        {
            var obj=new GameObject("Interaction label");obj.transform.SetParent(target,false);
            var label=obj.AddComponent<SeasonWorldLabel>();label.Caption=caption;label.Width=width;
            label.anchor=target.GetComponent<SpriteRenderer>();return label;
        }
        public static readonly List<SeasonWorldLabel> Active=new List<SeasonWorldLabel>();
        void OnEnable(){Active.Add(this);}
        void OnDisable(){Active.Remove(this);}
    }
}
