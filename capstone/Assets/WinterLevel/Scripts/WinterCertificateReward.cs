using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AmbrosJourney.Journey;
using AmbrosJourney.Presentation;

namespace AmbrosJourney.Winter
{
    // Owned by the Winter level; the dropped item stays with the cavern across region changes.
    public sealed class WinterCertificateReward : MonoBehaviour
    {
        WinterLevel level;
        SpriteRenderer item;
        readonly List<GameObject> dust=new List<GameObject>();
        Texture2D paperTexture;Sprite paperSprite;Material material;
        readonly List<Sprite> fragments=new List<Sprite>();
        public int DetachedPixelCount {get;private set;}
        public int TotalPixelCount {get;private set;}
        public bool Dropping {get;private set;}
        Vector3 restingPosition;
        bool started,claimed;
        Camera focusedCamera;WinterCamera followCamera;float originalSize;Vector3 originalCameraPosition;bool followWasEnabled;
        public bool Ready=>item&&item.gameObject.activeInHierarchy&&!claimed&&!Dropping;
        public bool CanCollect=>Ready&&level.Progress.InCave&&Vector2.Distance(level.player.transform.position,item.transform.position)<2.8f;
        public Vector2 Position=>restingPosition;
        public void Begin(WinterLevel owner,bool animate)
        {
            if(started)return;started=true;level=owner;
            var session=JourneySession.Instance;
            bool alreadyClaimed=session&&session.Data.finished&&!session.Data.certificatePending;
            session?.RecordFinalVictory();
            if(alreadyClaimed){HideBoss();claimed=true;return;}
            if(animate)StartCoroutine(Crumble());else{HideBoss();Drop();}
        }
        void HideBoss()
        {
            level.guardian.enabled=false;
            var animation=level.guardian.GetComponent<SeasonBossVisual>();if(animation)animation.enabled=false;
            foreach(var collider in level.guardian.GetComponents<Collider2D>())collider.enabled=false;
            level.guardian.GetComponent<SpriteRenderer>().enabled=false;
        }
        IEnumerator Crumble()
        {
            level.Transitioning=true;
            var boss=level.guardian;var visual=boss.GetComponent<SpriteRenderer>();var sprite=visual.sprite;
            boss.enabled=false;var animation=boss.GetComponent<SeasonBossVisual>();if(animation)animation.enabled=false;
            foreach(var collider in boss.GetComponents<Collider2D>())collider.enabled=false;
            material=new Material(Shader.Find("Sprites/Default"));
            var origins=new List<Vector3>();var velocities=new List<Vector3>();var releaseTimes=new List<float>();var lifetimes=new List<float>();var spins=new List<float>();var renderers=new List<SpriteRenderer>();
            var bounds=visual.bounds;
            focusedCamera=Camera.main;
            if(focusedCamera)
            {
                followCamera=focusedCamera.GetComponent<WinterCamera>();followWasEnabled=followCamera&&followCamera.enabled;if(followCamera)followCamera.enabled=false;
                originalSize=focusedCamera.orthographicSize;originalCameraPosition=focusedCamera.transform.position;
                float bottom=Mathf.Min(bounds.min.y,level.player.transform.position.y-1),top=bounds.max.y;
                focusedCamera.orthographicSize=Mathf.Max(originalSize,(top-bottom)*.5f+1.8f);
                focusedCamera.transform.position=new Vector3(bounds.center.x,(top+bottom)*.5f,-10);
            }
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
                var host=new GameObject("Falling Keeper dust",typeof(SpriteRenderer));host.transform.SetParent(boss.transform.parent,true);
                var renderer=host.GetComponent<SpriteRenderer>();renderer.sprite=piece;renderer.sharedMaterial=material;renderer.sortingOrder=visual.sortingOrder+1;renderer.enabled=false;
                Vector3 local=new Vector3(sprite.bounds.min.x+(x+width*.5f)/sprite.pixelsPerUnit,sprite.bounds.min.y+(y+height*.5f)/sprite.pixelsPerUnit,0);
                if(visual.flipX)local.x=-local.x;if(visual.flipY)local.y=-local.y;
                Vector3 position=boss.transform.TransformPoint(local);host.transform.position=position;host.transform.localScale=boss.transform.lossyScale;
                float fromTop=1-Mathf.InverseLerp(bounds.min.y,bounds.max.y,position.y);
                dust.Add(host);renderers.Add(renderer);origins.Add(position);
                releaseTimes.Add(.18f+fromTop*.60f+(float)random.NextDouble()*.18f);
                lifetimes.Add(.52f+(float)random.NextDouble()*.16f);
                velocities.Add(new Vector3(((float)random.NextDouble()-.5f)*2.4f,.15f+(float)random.NextDouble()*.9f,0));spins.Add(((float)random.NextDouble()-.5f)*100);
            }
            TotalPixelCount=dust.Count;float elapsed=0;Vector3 bossPosition=boss.transform.position;
            while(elapsed<1.8f)
            {
                elapsed+=Time.deltaTime;DetachedPixelCount=0;
                visual.enabled=elapsed<.18f;
                visual.color=Color.Lerp(Color.white,new Color(.65f,.95f,1),Mathf.Sin(Mathf.Clamp01(elapsed/.18f)*Mathf.PI)*.55f);
                if(visual.enabled)boss.transform.position=bossPosition+Vector3.right*(Mathf.Sin(elapsed*90)*.035f);
                for(int i=0;i<dust.Count;i++)
                {
                    var renderer=renderers[i];renderer.enabled=elapsed>=.18f;
                    float t=elapsed-releaseTimes[i];float collapse=Mathf.SmoothStep(0,.85f,Mathf.InverseLerp(.45f,1.02f,Mathf.Min(elapsed,releaseTimes[i])));
                    var origin=origins[i];origin.y=bounds.min.y+(origin.y-bounds.min.y)*(1-collapse);
                    if(t<0){dust[i].transform.position=origin;continue;}
                    DetachedPixelCount++;
                    dust[i].transform.position=origin+velocities[i]*t+Vector3.down*(5.5f*t*t);
                    dust[i].transform.rotation=Quaternion.Euler(0,0,spins[i]*t);
                    float remaining=1-t/lifetimes[i];renderer.color=new Color(1,1,1,Mathf.Clamp01(remaining*3));
                    if(remaining<=0)renderer.enabled=false;
                }
                yield return null;
            }
            boss.transform.position=bossPosition;HideBoss();
            foreach(var host in dust)if(host)Destroy(host);dust.Clear();
            foreach(var fragment in fragments)if(fragment)Destroy(fragment);fragments.Clear();
            // The reward visibly falls after the last pixels disappear, lands, and bounces once.
            Dropping=true;Drop();float fallTime=0;
            while(fallTime<.68f)
            {
                fallTime+=Time.deltaTime;float height;
                if(fallTime<.38f){float t=fallTime/.38f;height=2.1f*(1-t*t);item.transform.rotation=Quaternion.Euler(0,0,18*(1-t));}
                else{float t=Mathf.Clamp01((fallTime-.38f)/.30f);height=.36f*Mathf.Sin(t*Mathf.PI);item.transform.rotation=Quaternion.identity;}
                item.transform.position=restingPosition+Vector3.up*(.12f+height);yield return null;
            }
            Dropping=false;SeasonFeedback.Burst(restingPosition,new Color(1,.83f,.4f),12);RestoreCamera();level.Transitioning=false;
        }
        void Drop()
        {
            if(item||claimed)return;
            restingPosition=level.guardian.transform.position+Vector3.down*.45f;
            paperTexture=new Texture2D(24,24,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color32[24*24];Color32 paper=new Color32(253,242,203,255),edge=new Color32(174,128,54,255),ink=new Color32(57,89,72,255);
            for(int y=4;y<21;y++)for(int x=4;x<20;x++)pixels[y*24+x]=x==4||x==19||y==4||y==20?edge:paper;
            for(int y=13;y<=17;y+=2)for(int x=7;x<17;x++)pixels[y*24+x]=ink;
            for(int y=7;y<11;y++)for(int x=13;x<17;x++)pixels[y*24+x]=edge;
            pixels[6*24+14]=edge;pixels[6*24+16]=edge;paperTexture.SetPixels32(pixels);paperTexture.Apply();
            paperSprite=Sprite.Create(paperTexture,new Rect(0,0,24,24),Vector2.one*.5f,20);
            var host=new GameObject("Dropped AI safety certificate",typeof(SpriteRenderer));host.transform.SetParent(level.guardian.transform.parent,true);host.transform.position=restingPosition;
            item=host.GetComponent<SpriteRenderer>();item.sprite=paperSprite;item.sortingOrder=110-Mathf.RoundToInt(restingPosition.y*10);
            if(!material)material=new Material(Shader.Find("Sprites/Default"));item.sharedMaterial=material;
            if(!Dropping)SeasonFeedback.Burst(restingPosition,new Color(1,.83f,.4f),18);
        }
        void Update()
        {
            if(Ready)item.transform.position=restingPosition+Vector3.up*(.12f+Mathf.Sin(Time.time*3)*.10f);
        }
        public bool TryCollect()
        {
            var session=JourneySession.Instance;
            if(!CanCollect||level.InputBlocked||!session)return false;
            claimed=true;item.gameObject.SetActive(false);SeasonFeedback.Burst(restingPosition,new Color(1,.85f,.5f),20);
            session.FinishJourney();return true;
        }
        void RestoreCamera()
        {
            if(focusedCamera){focusedCamera.orthographicSize=originalSize;focusedCamera.transform.position=originalCameraPosition;if(followCamera)followCamera.enabled=followWasEnabled;focusedCamera=null;}
        }
        void OnDestroy()
        {
            RestoreCamera();
            foreach(var p in dust)if(p)Destroy(p);if(item)Destroy(item.gameObject);
            if(paperSprite)Destroy(paperSprite);if(paperTexture)Destroy(paperTexture);foreach(var fragment in fragments)if(fragment)Destroy(fragment);if(material)Destroy(material);
        }
    }
}
