// Applies shared camera feedback without replacing each season's follow controller.
using UnityEngine;
namespace AmbrosJourney.Presentation
{
    public static class SeasonCameraMotion
    {
        // Frame-rate independent damping, modest directional lead, and instant arrival resets.
        public static void Follow(Camera view,Transform target,Vector2 min,Vector2 max,ref Vector3 velocity,Vector2 lookOffset=default)
        {
            if(!view||!target)return;
            float h=view.orthographicSize,w=h*view.aspect;var body=target.GetComponent<Rigidbody2D>();
            Vector3 goal=target.position+(Vector3)lookOffset;if(body)goal+=(Vector3)Vector2.ClampMagnitude(body.linearVelocity*.12f,.65f);
            goal.x=w*2>=max.x-min.x?(min.x+max.x)*.5f:Mathf.Clamp(goal.x,min.x+w,max.x-w);
            goal.y=h*2>=max.y-min.y?(min.y+max.y)*.5f:Mathf.Clamp(goal.y,min.y+h,max.y-h);goal.z=-10;
            if(Vector2.Distance(view.transform.position,target.position)>12){view.transform.position=goal;velocity=Vector3.zero;return;}
            var position=Time.deltaTime>0?Vector3.SmoothDamp(view.transform.position,goal,ref velocity,.12f,100,Time.deltaTime):view.transform.position;
            // Resizing can put the existing camera outside the new limits. Damping only the goal
            // would expose the map edge while it catches up; constrain the rendered position too.
            var bounded=position;
            bounded.x=w*2>=max.x-min.x?(min.x+max.x)*.5f:Mathf.Clamp(position.x,min.x+w,max.x-w);
            bounded.y=h*2>=max.y-min.y?(min.y+max.y)*.5f:Mathf.Clamp(position.y,min.y+h,max.y-h);
            if(bounded.x!=position.x)velocity.x=0;if(bounded.y!=position.y)velocity.y=0;
            view.transform.position=bounded;
        }
    }
}
