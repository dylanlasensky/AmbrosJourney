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
        Texture2D paperTexture;Sprite paperSprite,pixelSprite;Material material;
        Vector3 restingPosition;
        bool started,claimed;
        Camera focusedCamera;WinterCamera followCamera;float originalSize;Vector3 originalCameraPosition;bool followWasEnabled;
        public bool Ready=>item&&item.gameObject.activeInHierarchy&&!claimed;
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
            material=new Material(Shader.Find("Sprites/Default"));pixelSprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f,1);
            var origins=new List<Vector3>();var velocities=new List<Vector3>();var colors=new List<Color>();
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
            // Sample the boss's actual atlas so the falling dust retains its colors.
            Rect region=sprite.textureRect;Color32[] pixels=sprite.texture.GetPixels32();int columns=24,rows=28;
            for(int y=0;y<rows;y++)for(int x=0;x<columns;x++)
            {
                int px=Mathf.Clamp((int)(region.x+(x+.5f)*region.width/columns),0,sprite.texture.width-1);
                int py=Mathf.Clamp((int)(region.y+(y+.5f)*region.height/rows),0,sprite.texture.height-1);
                Color color=pixels[py*sprite.texture.width+px];if(color.a<.5f||random.NextDouble()>.45)continue;
                var p=new GameObject("Falling Keeper dust",typeof(SpriteRenderer));p.transform.SetParent(boss.transform.parent,true);
                var r=p.GetComponent<SpriteRenderer>();r.sprite=pixelSprite;r.sharedMaterial=material;r.sortingOrder=visual.sortingOrder+1;r.color=new Color(color.r,color.g,color.b,0);
                Vector3 position=new Vector3(Mathf.Lerp(bounds.min.x,bounds.max.x,(x+.5f)/columns),Mathf.Lerp(bounds.min.y,bounds.max.y,(y+.5f)/rows),0);
                p.transform.position=position;p.transform.localScale=Vector3.one*(.06f+(float)random.NextDouble()*.10f);
                dust.Add(p);origins.Add(position);velocities.Add(new Vector3(((float)random.NextDouble()-.5f)*1.5f,.2f+(float)random.NextDouble()*.7f,0));colors.Add(color);
            }
            var originalScale=boss.transform.localScale;float elapsed=0;
            while(elapsed<2.6f)
            {
                elapsed+=Time.deltaTime;float fade=Mathf.Clamp01(elapsed/1.5f);
                visual.color=new Color(1,1,1,1-fade);boss.transform.localScale=new Vector3(originalScale.x*(1+fade*.08f),originalScale.y*(1-fade*.7f),originalScale.z);
                for(int i=0;i<dust.Count;i++)
                {
                    float t=Mathf.Max(0,elapsed-.2f);var position=origins[i]+velocities[i]*t+Vector3.down*t*t*1.2f;
                    position.y=Mathf.Max(bounds.min.y+.04f,position.y);dust[i].transform.position=position;
                    var color=colors[i];color.a=Mathf.Min(Mathf.Clamp01(elapsed*3),Mathf.Clamp01((2.6f-elapsed)/.9f));dust[i].GetComponent<SpriteRenderer>().color=color;
                }
                yield return null;
            }
            HideBoss();foreach(var p in dust)if(p)Destroy(p);dust.Clear();Drop();RestoreCamera();level.Transitioning=false;
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
            SeasonFeedback.Burst(restingPosition,new Color(1,.83f,.4f),18);
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
            if(paperSprite)Destroy(paperSprite);if(paperTexture)Destroy(paperTexture);if(pixelSprite)Destroy(pixelSprite);if(material)Destroy(material);
        }
    }
}
