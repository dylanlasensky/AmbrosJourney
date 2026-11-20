using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AmbrosJourney.Presentation
{
    // Texture-backed chunks preserve each boss silhouette, then collapse from top to bottom.
    public sealed class PixelBossDissolve : MonoBehaviour
    {
        readonly List<GameObject> dust=new List<GameObject>();
        readonly List<Sprite> fragments=new List<Sprite>();
        Material material;
        public int DetachedPixelCount {get;private set;}
        public int TotalPixelCount {get;private set;}
        public float AnimationElapsed {get;private set;}
        public static void StopBoss(GameObject boss)
        {
            foreach(var motor in boss.GetComponents<MonoBehaviour>())
                if(motor is SeasonBossVisual || motor is SpringSpriteAnimation || motor is AmbrosJourney.Summer.SummerActor || motor is AmbrosJourney.Fall.FallActor || motor is AmbrosJourney.Winter.WinterActor)motor.enabled=false;
            foreach(var collider in boss.GetComponents<Collider2D>())collider.enabled=false;
            foreach(var label in boss.GetComponentsInChildren<SeasonWorldLabel>(true))label.enabled=false;
        }
        public IEnumerator Play(SpriteRenderer visual,Action<float> tick=null)
        {
            var seasonal=visual.GetComponent<SeasonBossVisual>();
            if(seasonal&&seasonal.enabled)visual.sprite=seasonal.SamplePose(Time.time,false);
            StopBoss(visual.gameObject);var sprite=visual.sprite;var bounds=visual.bounds;
            material=new Material(Shader.Find("Sprites/Default"));
            var origins=new List<Vector3>();var velocities=new List<Vector3>();var releaseTimes=new List<float>();var lifetimes=new List<float>();var spins=new List<float>();var renderers=new List<SpriteRenderer>();
            var random=new System.Random(40);
            Rect region=sprite.textureRect;Color32[] pixels=sprite.texture.GetPixels32();const int cell=12;
            // Texture-backed fragments initially reconstruct the exact boss. Each chunk then leaves
            // the silhouette individually: top details first, followed by the collapsing body.
            for(int y=0;y<(int)region.height;y+=cell)for(int x=0;x<(int)region.width;x+=cell)
            {
                int width=Mathf.Min(cell,(int)region.width-x),height=Mathf.Min(cell,(int)region.height-y);bool solid=false;
                for(int dy=0;dy<height&&!solid;dy++)for(int dx=0;dx<width;dx++)
                    if(pixels[((int)region.y+y+dy)*sprite.texture.width+(int)region.x+x+dx].a>40){solid=true;break;}
                if(!solid)continue;
                var piece=Sprite.Create(sprite.texture,new Rect(region.x+x,region.y+y,width,height),Vector2.one*.5f,sprite.pixelsPerUnit,0,SpriteMeshType.FullRect);fragments.Add(piece);
                var host=new GameObject("Falling Keeper dust",typeof(SpriteRenderer));host.transform.SetParent(visual.transform.parent,true);
                var renderer=host.GetComponent<SpriteRenderer>();renderer.sprite=piece;renderer.sharedMaterial=material;renderer.sortingOrder=visual.sortingOrder+1;renderer.enabled=false;
                Vector3 local=new Vector3(sprite.bounds.min.x+(x+width*.5f)/sprite.pixelsPerUnit,sprite.bounds.min.y+(y+height*.5f)/sprite.pixelsPerUnit,0);
                if(visual.flipX)local.x=-local.x;if(visual.flipY)local.y=-local.y;
                Vector3 position=visual.transform.TransformPoint(local);host.transform.position=position;host.transform.localScale=visual.transform.lossyScale;
                float fromTop=1-Mathf.InverseLerp(bounds.min.y,bounds.max.y,position.y);
                dust.Add(host);renderers.Add(renderer);origins.Add(position);
                releaseTimes.Add(.18f+fromTop*.42f+(float)random.NextDouble()*.12f);
                lifetimes.Add(.40f+(float)random.NextDouble()*.12f);
                velocities.Add(new Vector3(((float)random.NextDouble()-.5f)*2.4f,.15f+(float)random.NextDouble()*.9f,0));spins.Add(((float)random.NextDouble()-.5f)*100);
            }
            TotalPixelCount=dust.Count;float elapsed=0;Vector3 bossPosition=visual.transform.position;
            while(elapsed<1.30f)
            {
                elapsed+=Time.deltaTime;AnimationElapsed=elapsed;DetachedPixelCount=0;
                visual.enabled=elapsed<.18f;
                visual.color=Color.Lerp(Color.white,new Color(.65f,.95f,1),Mathf.Sin(Mathf.Clamp01(elapsed/.18f)*Mathf.PI)*.55f);
                if(visual.enabled)visual.transform.position=bossPosition+Vector3.right*(Mathf.Sin(elapsed*90)*.035f);
                for(int i=0;i<dust.Count;i++)
                {
                    var renderer=renderers[i];renderer.enabled=elapsed>=.18f;
                    float t=elapsed-releaseTimes[i];float collapse=Mathf.SmoothStep(0,.85f,Mathf.InverseLerp(.30f,.75f,Mathf.Min(elapsed,releaseTimes[i])));
                    var origin=origins[i];origin.y=bounds.min.y+(origin.y-bounds.min.y)*(1-collapse);
                    if(t<0){dust[i].transform.position=origin;continue;}
                    DetachedPixelCount++;
                    dust[i].transform.position=origin+velocities[i]*t+Vector3.down*(5.5f*t*t);
                    dust[i].transform.rotation=Quaternion.Euler(0,0,spins[i]*t);
                    float remaining=1-t/lifetimes[i];renderer.color=new Color(1,1,1,Mathf.Clamp01(remaining*3));
                    if(remaining<=0)renderer.enabled=false;
                }
                tick?.Invoke(elapsed);
                yield return null;
            }
            visual.transform.position=bossPosition;visual.enabled=false;
            foreach(var host in dust)if(host)Destroy(host);dust.Clear();
            foreach(var fragment in fragments)if(fragment)Destroy(fragment);fragments.Clear();
        }
        void OnDestroy()
        {
            foreach(var host in dust)if(host)Destroy(host);
            foreach(var fragment in fragments)if(fragment)Destroy(fragment);
            if(material)Destroy(material);
        }
    }
}
