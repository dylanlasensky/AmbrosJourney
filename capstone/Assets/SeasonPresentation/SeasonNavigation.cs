using System.Collections.Generic;
using UnityEngine;
namespace AmbrosJourney.Presentation
{
    // Compass follows useful corners, while its distance remains the distance to the objective.
    public sealed class SeasonNavigation
    {
        readonly Vector2 min,max;
        List<Vector2> path;
        Vector2 goal,lastOrigin;
        float nextPlan;
        public int RebuildCount {get;private set;}
        public SeasonNavigation(Vector2 min,Vector2 max){this.min=min;this.max=max;}
        public Vector2 Next(Vector2 from,Vector2 destination)
        {
            if(ClearSegment(from,destination))return destination;
            if(path==null || destination!=goal || Vector2.Distance(from,lastOrigin)>8 || Time.unscaledTime>nextPlan&&Vector2.Distance(from,lastOrigin)>2)
            {path=FindPath(from,destination,min,max);goal=destination;lastOrigin=from;nextPlan=Time.unscaledTime+.8f;RebuildCount++;}
            while(path.Count>0&&Vector2.Distance(from,path[0])<1.2f)path.RemoveAt(0);
            if(path.Count==0)return destination;
            int visible=0;for(int i=1;i<path.Count&&i<40;i++){if(!ClearSegment(from,path[i]))break;visible=i;}
            return path[visible];
        }
        public static bool ClearSegment(Vector2 from,Vector2 to)
        {
            var delta=to-from;if(delta.sqrMagnitude<.001f)return true;
            foreach(var hit in Physics2D.CircleCastAll(from+new Vector2(0,.22f),.35f,delta.normalized,delta.magnitude))
                if(hit.collider&&!hit.collider.isTrigger&&!hit.collider.attachedRigidbody)return false;
            return true;
        }
        public static List<Vector2> FindPath(Vector2 from,Vector2 to,Vector2 min,Vector2 max)
        {
            var start=Vector2Int.RoundToInt(from);var goal=Vector2Int.RoundToInt(to);var queue=new Queue<Vector2Int>();var previous=new Dictionary<Vector2Int,Vector2Int>();queue.Enqueue(start);previous[start]=start;
            var directions=new[]{Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left};
            while(queue.Count>0)
            {
                var c=queue.Dequeue();if(c==goal){var result=new List<Vector2>();while(c!=start){result.Add(c);c=previous[c];}result.Reverse();return result;}
                foreach(var d in directions){var n=c+d;if(n.x<min.x+1||n.x>max.x-1||n.y<min.y+1||n.y>max.y-1||previous.ContainsKey(n))continue;
                    bool blocked=false;foreach(var obstacle in Physics2D.OverlapCircleAll((Vector2)n+new Vector2(0,.22f),.4f))if(!obstacle.isTrigger&&!obstacle.attachedRigidbody){blocked=true;break;}
                    if(blocked)continue;previous[n]=c;queue.Enqueue(n);
                }
            }
            return new List<Vector2>();
        }
    }
}
