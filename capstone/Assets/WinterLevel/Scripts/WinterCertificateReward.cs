// Spawns the earned certificate scroll and opens its reward view on interaction.
using AmbrosJourney.Audio;
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
        Texture2D paperTexture;Sprite paperSprite;Material material;
        public int DetachedPixelCount {get;private set;}
        public int TotalPixelCount {get;private set;}
        public bool Dropping {get;private set;}
        public float AnimationElapsed {get;private set;}
        public const float DropDelay=.72f,FallDuration=.20f,BounceDuration=.12f;
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
            PixelBossDissolve.StopBoss(level.guardian.gameObject);
            level.guardian.enabled=false;
            var animation=level.guardian.GetComponent<SeasonBossVisual>();if(animation)animation.enabled=false;
            foreach(var collider in level.guardian.GetComponents<Collider2D>())collider.enabled=false;
            level.guardian.GetComponent<SpriteRenderer>().enabled=false;
        }
        IEnumerator Crumble()
        {
            level.Transitioning=true;GameAudio.Play(SoundCue.Dust,.55f);
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
            var crumble=gameObject.AddComponent<PixelBossDissolve>();
            yield return crumble.Play(visual,elapsed=>{
                AnimationElapsed=elapsed;DetachedPixelCount=crumble.DetachedPixelCount;TotalPixelCount=crumble.TotalPixelCount;
                if(!item&&elapsed>=DropDelay){Dropping=true;Drop();}
                if(item&&Dropping)AnimateDrop(elapsed-DropDelay);
            });
            HideBoss();Destroy(crumble);
            if(Dropping)AnimateDrop(FallDuration+BounceDuration);
            RestoreCamera();level.Transitioning=false;
        }
        void AnimateDrop(float elapsed)
        {
            float height;
            if(elapsed<FallDuration){float t=elapsed/FallDuration;height=1.45f*(1-t*t);item.transform.rotation=Quaternion.Euler(0,0,12*(1-t));}
            else{float t=Mathf.Clamp01((elapsed-FallDuration)/BounceDuration);height=.16f*Mathf.Sin(t*Mathf.PI);item.transform.rotation=Quaternion.identity;}
            item.transform.position=restingPosition+Vector3.up*(.12f+height);
            if(elapsed>=FallDuration+BounceDuration){Dropping=false;SeasonFeedback.Burst(restingPosition,new Color(1,.83f,.4f),12);}
        }
        void Drop()
        {
            if(item||claimed)return;
            restingPosition=level.guardian.transform.position+Vector3.down*.45f;
            paperTexture=new Texture2D(32,32,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            // Original pixel-art rolled parchment: shaded curls, cream highlights and a red ribbon.
            string[] rows={
                ".........ppppp..............",
                "........pHHHhpp.............",
                "......ppPHHHhPpp............",
                ".....pbPPHHhhPPp............",
                "....pbccPPHhhPPPp...........",
                "...pbcccbPPHhhPPPp..........",
                "...pbcccbbPPHhhPPPp.........",
                "...pbbccbbbPPHhhPPp.........",
                "....pbbbbbbPPPHhhPRR........",
                ".....pbbbbPPPPHhPRRRR.......",
                "......ppPPPPPPPRRrRRR.......",
                "........pPPPPPRRrrRRRR......",
                ".........pPPPRRrrRRRRPp.....",
                "..........pPRRrrRRRPPPPp....",
                "...........RRrrRRRPPHPPPp...",
                "..........RRrrRRRPPHHPPPpp..",
                ".........RRrrRRRPPHHPPPbbbp.",
                "........RRrrRRRPPHHPPPbcccbbp",
                ".......RRrrRRRPPHHPPPbccccbbp",
                ".......RRRRRRPPHHPPPbccbccbp.",
                "........RRRRPPHHPPPbccPPccbp.",
                "..........pPPHHPPPbccPPccbp..",
                "...........pPHHPPPbccccbbp...",
                "............ppPPPpbbbbbp....",
                "..............pppbbbbp......",
                "................ppppp......."
            };
            var colors=new Dictionary<char,Color32>{
                {'p',new Color32(193,112,75,255)},{'P',new Color32(246,166,106,255)},
                {'H',new Color32(249,223,164,255)},{'h',new Color32(235,191,126,255)},
                {'b',new Color32(147,78,53,255)},{'c',new Color32(214,136,83,255)},
                {'R',new Color32(201,30,35,255)},{'r',new Color32(255,65,64,255)}};
            var pixels=new Color32[32*32];
            for(int y=0;y<rows.Length;y++)for(int x=0;x<rows[y].Length;x++)if(colors.TryGetValue(rows[y][x],out var color))pixels[(29-y)*32+x+1]=color;
            paperTexture.SetPixels32(pixels);paperTexture.Apply();
            paperSprite=Sprite.Create(paperTexture,new Rect(0,0,32,32),Vector2.one*.5f,24);
            var host=new GameObject("Dropped AI safety certificate",typeof(SpriteRenderer));host.transform.SetParent(level.guardian.transform.parent,true);host.transform.position=restingPosition;
            item=host.GetComponent<SpriteRenderer>();item.sprite=paperSprite;SeasonWorldLabel.Attach(host.transform,"[E] PICK UP CERTIFICATE");item.sortingOrder=110-Mathf.RoundToInt(restingPosition.y*10);
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
            claimed=true;GameAudio.Play(SoundCue.Pickup,.65f);item.gameObject.SetActive(false);SeasonFeedback.Burst(restingPosition,new Color(1,.85f,.5f),20);
            session.FinishJourney();return true;
        }
        void RestoreCamera()
        {
            if(focusedCamera){focusedCamera.orthographicSize=originalSize;focusedCamera.transform.position=originalCameraPosition;if(followCamera)followCamera.enabled=followWasEnabled;focusedCamera=null;}
        }
        void OnDestroy()
        {
            RestoreCamera();
            if(item)Destroy(item.gameObject);
            if(paperSprite)Destroy(paperSprite);if(paperTexture)Destroy(paperTexture);if(material)Destroy(material);
        }
    }
}
