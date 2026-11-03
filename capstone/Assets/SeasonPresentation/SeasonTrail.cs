using System.Collections.Generic;
using UnityEngine;
namespace AmbrosJourney.Presentation
{
    // A safe breadcrumb spine with individual spacing and gentle lateral wandering.
    public sealed class SeasonTrail
    {
        readonly List<Vector2> points=new List<Vector2>();
        Vector2 latest;float lastMoved;
        readonly bool[] wasResting=new bool[9];readonly Vector2[] formation=new Vector2[9];readonly float[] nextFormation=new float[9];
        public int Revision {get;private set;}
        public void Record(Vector2 position)
        {
            if(points.Count==0||Vector2.Distance(latest,position)>8){Reset(position);return;}
            if(Vector2.Distance(points[points.Count-1],position)>=.08f){lastMoved=Time.time;points.Add(position);if(points.Count>500)points.RemoveAt(0);}
            latest=position;
        }
        public void Reset(Vector2 position)
        {
            lastMoved=Time.time;System.Array.Clear(nextFormation,0,nextFormation.Length);points.Clear();int length=100;while(length>0&&!SeasonNavigation.ClearSegment(position,position+Vector2.left*(length*.1f)))length-=10;for(int i=length;i>=0;i--)points.Add(position+Vector2.left*(i*.1f));latest=position;Revision++;
        }
        public Vector2 Formation(int slot,Vector2 current)
        {
            slot=Mathf.Clamp(slot,1,8);
            bool resting=Time.time-lastMoved>1.1f;
            if(Time.time<nextFormation[slot]&&resting==wasResting[slot])return formation[slot];
            wasResting[slot]=resting;
            // Different resting rhythms avoid synchronized orbiting and constant foot shuffling.
            nextFormation[slot]=Time.time+(resting?2.8f+(slot%3)*.7f:.25f);
            float angle=(slot*137.508f+(resting?Mathf.Floor(Time.time/(7+slot)) * 19:0))*Mathf.Deg2Rad;
            float radius=1.75f+(slot%3)*.48f;
            Vector2 offset=latest+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
            if(SeasonNavigation.ClearSegment(latest,offset)&&SeasonNavigation.ClearSegment(current,offset))
                return formation[slot]=offset;
            // Around corners retain a safe breadcrumb target; never cut through a bank or house.
            var center=Behind(1.1f+slot*.55f);
            if(SeasonNavigation.ClearSegment(current,center))return formation[slot]=center;
            for(float distance=0;distance<12;distance+=.7f)
            {
                var point=Behind(distance);
                if(SeasonNavigation.ClearSegment(current,point))return formation[slot]=point;
            }
            return formation[slot]=current;
        }
        public Vector2 Behind(float distance)
        {
            Vector2 end=latest;
            for(int i=points.Count-1;i>=0;i--){float segment=Vector2.Distance(end,points[i]);if(segment>=distance&&segment>.0001f)return Vector2.Lerp(end,points[i],distance/segment);distance-=segment;end=points[i];}
            return end;
        }
    }
}
