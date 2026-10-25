using System.Collections.Generic;
using UnityEngine;
namespace AmbrosJourney.Presentation
{
    // Arc-length breadcrumbs: every companion traces the player's actual turns at a fixed gap.
    public sealed class SeasonTrail
    {
        readonly List<Vector2> points=new List<Vector2>();
        Vector2 latest;
        public int Revision {get;private set;}
        public void Record(Vector2 position)
        {
            if(points.Count==0||Vector2.Distance(latest,position)>8){Reset(position);return;}
            if(Vector2.Distance(points[points.Count-1],position)>=.08f){points.Add(position);if(points.Count>500)points.RemoveAt(0);}
            latest=position;
        }
        public void Reset(Vector2 position)
        {
            points.Clear();for(int i=100;i>=0;i--)points.Add(position+Vector2.left*(i*.1f));latest=position;Revision++;
        }
        public Vector2 Behind(float distance)
        {
            Vector2 end=latest;
            for(int i=points.Count-1;i>=0;i--){float segment=Vector2.Distance(end,points[i]);if(segment>=distance&&segment>.0001f)return Vector2.Lerp(end,points[i],distance/segment);distance-=segment;end=points[i];}
            return end;
        }
    }
}
