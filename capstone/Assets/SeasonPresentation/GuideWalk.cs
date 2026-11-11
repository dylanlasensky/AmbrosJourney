using UnityEngine;
namespace AmbrosJourney.Presentation
{
    // A guide leads at walking pace and waits when the player stops to explore.
    public sealed class GuideWalk
    {
        readonly SeasonNavigation navigation;
        readonly Vector2 destination;
        public bool Arrived { get; private set; }
        public GuideWalk(Vector2 destination,Vector2 min,Vector2 max)
        { this.destination=destination;navigation=new SeasonNavigation(min,max); }
        public Vector2 Step(Transform guide,Transform player,bool blocked,float deltaTime=-1)
        {
            if(deltaTime<0)deltaTime=Time.deltaTime;
            if(blocked||Arrived||!player)return Vector2.zero;
            Vector2 before=guide.position;
            if(Vector2.Distance(before,destination)<.15f){Arrived=true;return Vector2.zero;}
            if(Vector2.Distance(before,player.position)>5&&Vector2.Distance(before,destination)<Vector2.Distance(player.position,destination))return Vector2.zero;
            Vector2 next=navigation.Next(before,destination);
            Vector2 after=Vector2.MoveTowards(before,next,3.2f*deltaTime);
            // Never walk through a wall if a route is temporarily unavailable.
            if(!SeasonNavigation.ClearSegment(before,after))return Vector2.zero;
            guide.position=after;
            return (after-before)/Mathf.Max(deltaTime,.001f);
        }
    }
}
