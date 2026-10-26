using System.Collections.Generic;
using UnityEngine;
namespace AmbrosJourney.Presentation
{
    // A* is bounded to map cells. Queries reuse buffers, and the HUD samples a route five times per second.
    public sealed class SeasonNavigation
    {
        readonly Vector2 min,max;List<Vector2> path;Vector2 goal,lastOrigin,cached,checkedFrom;float nextPlan,nextCheck;
        public static int PhysicsQueryCount {get;private set;}
        public int RebuildCount {get;private set;}
        static readonly RaycastHit2D[] hits=new RaycastHit2D[32];
        static readonly Collider2D[] overlaps=new Collider2D[32];
        static readonly ContactFilter2D filter=new ContactFilter2D().NoFilter();
        public SeasonNavigation(Vector2 min,Vector2 max){this.min=min;this.max=max;}
        public Vector2 Next(Vector2 from,Vector2 destination)
        {
            if(Time.unscaledTime<nextCheck&&destination==goal&&(from-checkedFrom).sqrMagnitude<4)return cached;
            nextCheck=Time.unscaledTime+.2f;checkedFrom=from;
            if(ClearSegment(from,destination)){goal=destination;return cached=destination;}
            if(path==null||destination!=goal||(from-lastOrigin).sqrMagnitude>64||Time.unscaledTime>nextPlan&&(from-lastOrigin).sqrMagnitude>9)
            {path=FindPath(from,destination,min,max);goal=destination;lastOrigin=from;nextPlan=Time.unscaledTime+1.5f;RebuildCount++;}
            while(path.Count>0&&Vector2.Distance(from,path[0])<1.2f)path.RemoveAt(0);
            if(path.Count==0)return cached=destination;
            int visible=0;for(int i=1;i<path.Count&&i<40;i+=3){if(!ClearSegment(from,path[i]))break;visible=i;}return cached=path[visible];
        }
        public static bool ClearSegment(Vector2 from,Vector2 to)
        {
            var d=to-from;if(d.sqrMagnitude<.001f)return true;
            PhysicsQueryCount++;int count=Physics2D.CircleCast(from+new Vector2(0,.22f),.35f,d.normalized,filter,hits,d.magnitude);
            if(count==hits.Length)return false;
            for(int i=0;i<count;i++)if(hits[i].collider&&!hits[i].collider.isTrigger&&!hits[i].collider.attachedRigidbody)return false;
            return true;
        }
        static bool Blocked(Vector2Int p)
        {
            PhysicsQueryCount++;int count=Physics2D.OverlapCircle((Vector2)p+new Vector2(0,.22f),.4f,filter,overlaps);
            if(count==overlaps.Length)return true;
            for(int i=0;i<count;i++)if(!overlaps[i].isTrigger&&!overlaps[i].attachedRigidbody)return true;return false;
        }
        struct Node{public Vector2Int point;public int priority,cost;public Node(Vector2Int p,int f,int g){point=p;priority=f;cost=g;}}
        sealed class Heap
        {
            readonly List<Node> nodes=new List<Node>();public int Count=>nodes.Count;
            public void Push(Node node){nodes.Add(node);int i=nodes.Count-1;while(i>0){int parent=(i-1)/2;if(nodes[parent].priority<=node.priority)break;nodes[i]=nodes[parent];i=parent;}nodes[i]=node;}
            public Node Pop(){var first=nodes[0];var last=nodes[nodes.Count-1];nodes.RemoveAt(nodes.Count-1);if(nodes.Count==0)return first;int i=0;while(i*2+1<nodes.Count){int c=i*2+1;if(c+1<nodes.Count&&nodes[c+1].priority<nodes[c].priority)c++;if(last.priority<=nodes[c].priority)break;nodes[i]=nodes[c];i=c;}nodes[i]=last;return first;}
        }
        static int Distance(Vector2Int a,Vector2Int b)=>Mathf.Abs(a.x-b.x)+Mathf.Abs(a.y-b.y);
        public static List<Vector2> FindPath(Vector2 from,Vector2 to,Vector2 min,Vector2 max)
        {
            var start=Vector2Int.RoundToInt(from);var goal=Vector2Int.RoundToInt(to);var open=new Heap();var previous=new Dictionary<Vector2Int,Vector2Int>();var costs=new Dictionary<Vector2Int,int>();var blocked=new Dictionary<Vector2Int,bool>();
            open.Push(new Node(start,Distance(start,goal),0));costs[start]=0;previous[start]=start;
            var directions=new[]{Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left};int expanded=0;
            int budget=Mathf.Min(16000,Mathf.CeilToInt((max.x-min.x)*(max.y-min.y)));
            while(open.Count>0&&expanded++<budget)
            {
                var node=open.Pop();var c=node.point;if(costs[c]!=node.cost)continue;
                if(c==goal){var result=new List<Vector2>();while(c!=start){result.Add(c);c=previous[c];}result.Reverse();return result;}
                foreach(var d in directions){var n=c+d;if(n.x<min.x+1||n.x>max.x-1||n.y<min.y+1||n.y>max.y-1)continue;
                    int cost=node.cost+1;if(costs.TryGetValue(n,out int old)&&old<=cost)continue;
                    if(!blocked.TryGetValue(n,out bool wall)){wall=Blocked(n);blocked[n]=wall;}if(wall)continue;
                    costs[n]=cost;previous[n]=c;open.Push(new Node(n,cost+Distance(n,goal),cost));
                }
            }
            return new List<Vector2>();
        }
    }
}
