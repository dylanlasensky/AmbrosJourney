// Moves companions along collision-aware routes and pauses them during conversations.
using UnityEngine;
namespace AmbrosJourney.Presentation
{
    // A guide leads at walking pace and waits when the player stops to explore.
    public sealed class GuideWalk
    {
        readonly SeasonNavigation navigation;
        readonly Vector2 destination;
        public bool Arrived { get; private set; }
        public bool WaitForPlayer { get; set; }=true;
        public GuideWalk(Vector2 destination,Vector2 min,Vector2 max)
        { this.destination=destination;navigation=new SeasonNavigation(min,max); }
        public Vector2 Step(Transform guide,Transform player,bool blocked,float deltaTime=-1)
        {
            if(deltaTime<0)deltaTime=Time.deltaTime;
            if(blocked||Arrived||!player)return Vector2.zero;
            Vector2 before=guide.position;
            if(Vector2.Distance(before,destination)<.15f){Arrived=true;return Vector2.zero;}
            Vector2 fromPlayer=before-(Vector2)player.position;
            bool leading=WaitForPlayer&&Vector2.Distance(before,destination)<Vector2.Distance(player.position,destination);
            if(leading&&fromPlayer.sqrMagnitude>25)return Vector2.zero;
            Vector2 next=navigation.Next(before,destination);
            Vector2 after=Vector2.MoveTowards(before,next,3.2f*deltaTime);
            // A long frame must not step beyond the waiting radius. Clip along the route
            // segment, rather than pulling the guide sideways across nearby obstacles.
            if(leading&&(after-(Vector2)player.position).sqrMagnitude>25)
            {
                Vector2 step=after-before;float lengthSquared=step.sqrMagnitude;
                if(lengthSquared>0)
                {
                    float projection=Vector2.Dot(fromPlayer,step);
                    float fraction=(Mathf.Sqrt(Mathf.Max(0,projection*projection+lengthSquared*(25-fromPlayer.sqrMagnitude)))-projection)/lengthSquared;
                    after=before+step*Mathf.Clamp01(fraction);
                }
            }
            // Never walk through a wall if a route is temporarily unavailable.
            if(!SeasonNavigation.ClearSegment(before,after))return Vector2.zero;
            guide.position=after;
            return (after-before)/Mathf.Max(deltaTime,.001f);
        }
    }
}
