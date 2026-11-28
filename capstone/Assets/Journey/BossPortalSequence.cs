// Sequences boss dissolution and gated exits, retaining Winter's natural cave doorway.
using System.Collections;
using UnityEngine;
using AmbrosJourney.Audio;
using AmbrosJourney.Presentation;
using AmbrosJourney.Summer;
using AmbrosJourney.Fall;
using AmbrosJourney.Winter;
namespace AmbrosJourney.Journey
{
    // One shared victory sequence; lesson/scoring rules remain in each original level.
    public sealed class BossPortalSequence : MonoBehaviour
    {
        public static bool Busy {get;private set;}
        public PixelPortalReveal Exit {get;private set;}
        public PixelPortalReveal Home {get;private set;}
        public PixelBossDissolve Dissolve {get;private set;}
        SpringGuide spring;SummerLevel summer;FallLevel fall;WinterLevel winter;
        GameObject boss;Transform player;bool finished,running;
        Camera cameraView;float cameraSize;Vector3 cameraPosition;Behaviour cameraFollow;bool followEnabled;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetStatics(){Busy=false;}
        public void Initialize(int chapter,WorldCohesion art)
        {
            spring=JourneyWorld.FindInScene<SpringGuide>(this);summer=JourneyWorld.FindInScene<SummerLevel>(this);fall=JourneyWorld.FindInScene<FallLevel>(this);winter=JourneyWorld.FindInScene<WinterLevel>(this);
            player=JourneySession.Instance.Player();Transform portal;
            if(chapter==1){boss=spring.Boss.gameObject;portal=spring.ExitPortal;}
            else if(chapter==2){boss=summer.orc.gameObject;portal=summer.fallGate;RemoveFlags(summer.shields);summer.shields=new SpriteRenderer[0];}
            else if(chapter==3){boss=fall.guardian.gameObject;portal=fall.winterGate;RemoveFlags(fall.shields);fall.shields=new SpriteRenderer[0];}
            else
            {
                boss=winter.winterGuardian.gameObject;portal=winter.caveEntrance;
                // Winter uses its existing natural cave doorway, without an added portal sprite.
                RemoveFlags(winter.shields);winter.shields=new SpriteRenderer[0];
                winter.winterGate.position=WinterWorld.BossPosition+new Vector2(5,-1);
                art.Portal(winter.winterGate,4);
                foreach(var label in winter.winterGate.GetComponentsInChildren<SeasonWorldLabel>(true))label.Caption="HOME / WALK THROUGH";
                Home=gameObject.AddComponent<PixelPortalReveal>();Home.Initialize(winter.winterGate,JourneySession.Instance.Data.homePortalOpened);
            }
            // Move standalone destination captions under their portal so they appear together.
            string caption=chapter==2?"FALL / PROMPT ENGINEERING":chapter==3?"WINTER / VERIFICATION":chapter==4?"ECHO CAVERN / WALK INSIDE":null;
            if(caption!=null)foreach(var label in FindObjectsByType<SeasonWorldLabel>(FindObjectsInactive.Include))
                if(label.gameObject.scene==gameObject.scene&&label.Caption==caption){label.transform.SetParent(portal,true);var visual=portal.GetComponent<SpriteRenderer>();if(visual)label.transform.position=portal.position+Vector3.up*(visual.bounds.size.y+.4f);}
            Exit=gameObject.AddComponent<PixelPortalReveal>();finished=Complete;Exit.Initialize(portal,finished);
            if(finished){boss.SetActive(false);if(spring)spring.ExitBarrier.SetActive(false);}
        }
        public static void RestoreCheckpoint()
        {
            var sequence=FindAnyObjectByType<BossPortalSequence>();
            if(!sequence||!sequence.Exit||sequence.running)return;
            sequence.finished=sequence.Complete;sequence.Exit.SetReady(sequence.finished);
            if(sequence.finished){sequence.boss.SetActive(false);if(sequence.spring)sequence.spring.ExitBarrier.SetActive(false);}
            if(sequence.Home)sequence.Home.SetReady(JourneySession.Instance.Data.homePortalOpened);
        }
        static void RemoveFlags(SpriteRenderer[] flags){foreach(var flag in flags)if(flag){flag.gameObject.SetActive(false);Destroy(flag.gameObject);}}
        bool Complete=>spring?spring.SpringComplete:summer?summer.Progress.Complete:fall?fall.Progress.Complete:winter&&winter.Progress.WinterComplete;
        bool Dialogue=>spring?spring.DialogueOpen:summer?summer.IsDialogueOpen:fall?fall.IsDialogueOpen:winter&&winter.IsDialogueOpen;
        void Update()
        {
            if(!Exit||finished||running||!Complete||Dialogue||JourneySession.Blocked)return;
            StartCoroutine(Victory());
        }
        public static bool AllowsEntry(Transform portal)
        {
            if(!portal)return false;
            foreach(var sequence in FindObjectsByType<BossPortalSequence>())
                if(sequence.Exit&&sequence.Exit.Portal==portal)return sequence.Exit.Ready;
                else if(sequence.Home&&sequence.Home.Portal==portal)return sequence.Home.Ready;
            return false;
        }
        public static bool AllowsEntry(Vector2 position)
        {
            foreach(var sequence in FindObjectsByType<BossPortalSequence>())
                if(sequence.Exit&&Vector2.Distance(sequence.Exit.Portal.position,position)<.1f)return sequence.Exit.Ready;
            return false;
        }
        IEnumerator Victory()
        {
            running=true;Busy=true;JourneySession.Instance?.SetCinematicVisible(true);
            var body=player.GetComponent<Rigidbody2D>();if(body)body.linearVelocity=Vector2.zero;
            Focus(boss.GetComponent<SpriteRenderer>().bounds,Exit.Portal.position);
            GameAudio.Play(SoundCue.Dust,.55f);
            Dissolve=gameObject.AddComponent<PixelBossDissolve>();yield return Dissolve.Play(boss.GetComponent<SpriteRenderer>());
            boss.SetActive(false);
            if(winter)Exit.SetReady(true); // Keep the natural cave; no portal-building effect here.
            else{GameAudio.Play(SoundCue.Portal,.6f);yield return Exit.Assemble();}
            if(spring)spring.ExitBarrier.SetActive(false);
            RestoreCamera();finished=true;running=false;Busy=false;JourneySession.Instance?.SetCinematicVisible(false);JourneySession.Instance?.CaptureChapter();
        }
        public void RevealHomePortal()
        {
            if(Home&&!Home.Ready&&!Home.Playing&&!running)StartCoroutine(OpenHome());
        }
        IEnumerator OpenHome()
        {
            running=true;Busy=true;JourneySession.Instance?.SetCinematicVisible(true);
            var body=player.GetComponent<Rigidbody2D>();if(body)body.linearVelocity=Vector2.zero;
            Focus(new Bounds(player.position,Vector3.one*2),Home.Portal.position);
            GameAudio.Play(SoundCue.Portal,.6f);yield return Home.Assemble();
            RestoreCamera();running=false;Busy=false;JourneySession.Instance?.SetCinematicVisible(false);JourneySession.Instance?.CaptureChapter();
        }
        void Focus(Bounds bossBounds,Vector3 portal)
        {
            cameraView=Camera.main;if(!cameraView)return;
            cameraPosition=cameraView.transform.position;cameraSize=cameraView.orthographicSize;
            foreach(var behaviour in cameraView.GetComponents<Behaviour>())
                if(behaviour.GetType().Name is "SpringCamera" or "SummerCamera" or "FallCamera" or "WinterCamera"){cameraFollow=behaviour;followEnabled=behaviour.enabled;behaviour.enabled=false;break;}
            var bounds=bossBounds;bounds.Encapsulate(player.position);bounds.Encapsulate(portal+Vector3.up*4);bounds.Encapsulate(portal);
            cameraView.orthographicSize=Mathf.Max(cameraSize,bounds.size.y*.5f+1.5f,bounds.size.x/(2*cameraView.aspect)+1.5f);
            cameraView.transform.position=new Vector3(bounds.center.x,bounds.center.y,-10);
        }
        void RestoreCamera()
        {
            if(cameraView){cameraView.orthographicSize=cameraSize;cameraView.transform.position=cameraPosition;cameraView=null;}
            if(cameraFollow)cameraFollow.enabled=followEnabled;
        }
        void OnDestroy(){RestoreCamera();if(running){Busy=false;JourneySession.Instance?.SetCinematicVisible(false);}}
    }
}
