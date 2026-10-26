using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using AmbrosJourney.Summer;
using AmbrosJourney.Fall;
using AmbrosJourney.Winter;
namespace AmbrosJourney.Journey
{
    [DefaultExecutionOrder(-500)]
    public sealed class JourneySession : MonoBehaviour
    {
        public static JourneySession Instance {get;private set;}
        public static bool Blocked=>Instance&&(Instance.Paused||Instance.loading||Instance.menu||Instance.cinematic||Instance.resumeFrame==Time.frameCount&&Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame);
        public static bool TestMode;
        public static bool CanUsePortals=>!Blocked&&(!Instance||!Instance.portalProtected);
        bool portalProtected;Vector2 protectedPosition;
        public static string SavePathOverride;
        public static string SavePath=>SavePathOverride??Path.Combine(Application.persistentDataPath,"ambros-journey-v1.json");
        public JourneyData Data {get;private set;}
        public bool Paused {get;private set;}
        int resumeFrame=-1;JourneyUI ui;bool menu,loading,cinematic;float nextSave;string notice="";Action beginIntro;TMP_InputField nameInput;
        public static readonly string[] ChapterNames={"Spring Valley","Summer Valley","Emberwood","Frostveil"};
        public const string ArtifactLesson="THE EVIDENCE COMPASS\nPause before sharing. Protect private details. Trace a claim to its original source, compare independent evidence, and explain what you still do not know.\n\nEven unanimous AI answers are not independent evidence: several tools can repeat the same mistake. This is the collection's final-boss lesson.";
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetStatics(){Instance=null;TestMode=false;SavePathOverride=null;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Install(){if(!Instance){var host=new GameObject("Journey / save and menus");DontDestroyOnLoad(host);host.AddComponent<JourneySession>();}}
        static bool ValidSave(JourneyData data)
        {
            if(!data.Valid())return false;
            for(int i=0;i<4;i++)if(data.chapters[i].visited)
            {
                string json=data.chapters[i].progress;
                switch(i){case 0:if(!(JsonUtility.FromJson<SpringState>(json)?.Valid()??false))return false;break;case 1:if(!(JsonUtility.FromJson<SummerProgress>(json)?.Valid()??false))return false;break;case 2:if(!(JsonUtility.FromJson<FallProgress>(json)?.Valid()??false))return false;break;case 3:if(!(JsonUtility.FromJson<WinterProgress>(json)?.Valid()??false))return false;break;}
            }
            return true;
        }
#if UNITY_EDITOR
        public void BeginTestProfile(){TestMode=true;Data=new JourneyData{playerName="Zoë Explorer",started=true};}
#endif
        void Awake()
        {
            if(Instance){Destroy(gameObject);return;}Instance=this;Data=JourneyStorage.Read(SavePath,JsonUtility.FromJson<JourneyData>,ValidSave)??new JourneyData();
            var canvas=new GameObject("Journey interface");canvas.transform.SetParent(transform,false);ui=canvas.AddComponent<JourneyUI>();ui.Initialize();
            ApplyPerformance();SceneManager.activeSceneChanged+=SceneChanged;
            if(SceneManager.GetActiveScene().name!="Intro")StartCoroutine(WorldReady(false));
        }
        public static void ApplyPerformance()
        {
            // Hardware-paced, never above 60 Hz on common 60/90/120/144/165/240 Hz displays.
            double refresh=Screen.currentResolution.refreshRateRatio.value;
            int divisor=Mathf.CeilToInt((float)refresh/60f);
            QualitySettings.vSyncCount=refresh>=30&&divisor>=1&&divisor<=4?divisor:0;
            Application.targetFrameRate=60;Application.runInBackground=false;
        }
        void SceneChanged(Scene previous,Scene current){if(!loading&&current.name!="Intro")StartCoroutine(WorldReady(true));}
        IEnumerator WorldReady(bool restore)
        {
            yield return null;yield return null;
            int chapter=CurrentChapter();if(chapter==0)yield break;
            Data.chapter=chapter;Data.unlocked=Math.Max(Data.unlocked,chapter);
            if(restore&&Data.chapters[chapter-1].visited)Restore(chapter);
            if(!FindAnyObjectByType<JourneyWorld>()){var world=new GameObject("Journey exploration");world.AddComponent<JourneyWorld>();}
            if(!menu&&!Paused)ShowHUD();nextSave=Time.unscaledTime+5;
        }
        public static int CurrentChapter(){switch(SceneManager.GetActiveScene().name){case "Spring":return 1;case "Summer":return 2;case "Fall":return 3;case "Winter":return 4;default:return 0;}}
        public Transform Player()
        {
            var spring=FindAnyObjectByType<SpringPlayer>();if(spring)return spring.transform;
            var summer=FindAnyObjectByType<SummerLevel>();if(summer)return summer.player.transform;
            var fall=FindAnyObjectByType<FallLevel>();if(fall)return fall.player.transform;
            var winter=FindAnyObjectByType<WinterLevel>();return winter?winter.player.transform:null;
        }
        void Update()
        {
            if(portalProtected&&Player()&&Vector2.Distance(Player().position,protectedPosition)>2)portalProtected=false;
            if(!cinematic&&!menu&&!loading&&CurrentChapter()>0&&Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){if(Paused)Resume();else Pause();}
            if(!TestMode&&Data.started&&!Blocked&&Time.unscaledTime>=nextSave){Save();nextSave=Time.unscaledTime+5;}
        }
        public bool Save()
        {
            if(loading||!Data.started||CurrentChapter()==0)return true;
            int chapter=CurrentChapter();var player=Player();if(!player)return false;
            var entry=Data.chapters[chapter-1];entry.visited=true;entry.x=player.position.x;entry.y=player.position.y;Data.chapter=chapter;Data.unlocked=Math.Max(Data.unlocked,chapter);
            switch(chapter){case 1:entry.progress=JsonUtility.ToJson(FindAnyObjectByType<SpringGuide>().CaptureProgress());break;case 2:entry.progress=JsonUtility.ToJson(FindAnyObjectByType<SummerLevel>().Progress);break;case 3:entry.progress=JsonUtility.ToJson(FindAnyObjectByType<FallLevel>().Progress);break;case 4:var w=FindAnyObjectByType<WinterLevel>();entry.inCave=w.Progress.InCave;entry.progress=JsonUtility.ToJson(w.Progress);break;}
            try{JourneyStorage.Write(SavePath,JsonUtility.ToJson(Data));notice="Progress saved on this device.";return true;}catch(Exception e)when(e is IOException||e is UnauthorizedAccessException){notice="Could not save. Your session is still open. Free some disk space and try again.";Debug.LogWarning("Journey save failed: "+e.Message);return false;}
        }
        void Restore(int chapter)
        {
            var e=Data.chapters[chapter-1];if(!e.visited)return;
            try
            {
                switch(chapter){case 1:FindAnyObjectByType<SpringGuide>().RestoreProgress(JsonUtility.FromJson<SpringState>(e.progress));break;case 2:FindAnyObjectByType<SummerLevel>().RestoreProgress(JsonUtility.FromJson<SummerProgress>(e.progress));break;case 3:FindAnyObjectByType<FallLevel>().RestoreProgress(JsonUtility.FromJson<FallProgress>(e.progress));break;case 4:FindAnyObjectByType<WinterLevel>().RestoreProgress(JsonUtility.FromJson<WinterProgress>(e.progress));break;}
                var p=Player();p.position=new Vector3(e.x,e.y,0);var body=p.GetComponent<Rigidbody2D>();if(body){body.position=p.position;body.linearVelocity=Vector2.zero;}
                var camera=Camera.main;if(camera)camera.transform.position=new Vector3(e.x,e.y,-10);
                portalProtected=true;protectedPosition=p.position;Physics2D.SyncTransforms();
            }
            catch(Exception error){notice="This chapter's checkpoint could not be restored. Its starting area has been loaded.";Debug.LogWarning("Checkpoint recovery: "+error.Message);}
        }
        public void OpenMenu(Action playIntro)
        {
            beginIntro=playIntro;menu=true;Paused=false;Time.timeScale=1;AudioListener.pause=false;ShowMenu();
        }
        void Background(string eyebrow,string title,string subtitle)
        {
            ui.Clear();ui.Panel(ui.Root,"Backdrop",new Rect(-1800,-1100,3600,2200),JourneyUI.Green);
            ui.Panel(ui.Root,"Gold line",new Rect(-550,-245,1100,3),JourneyUI.Gold);
            ui.Text(ui.Root,"Eyebrow",eyebrow,new Rect(-550,-310,1100,32),19,JourneyUI.Gold);
            var titleText=ui.Text(ui.Root,"Title",title,new Rect(-550,-230,1100,75),48,JourneyUI.Paper);titleText.enableAutoSizing=true;titleText.fontSizeMin=28;titleText.fontSizeMax=48;
            ui.Text(ui.Root,"Subtitle",subtitle,new Rect(-550,-150,1100,65),22,JourneyUI.Paper);
        }
        void ShowMenu()
        {
            Background("AMBROS' JOURNEY  /  FOUR SEASONS OF DISCOVERY","A small explorer. A wiser world.","Learn to question, create and check the evidence. Your journey stays saved on this device.");
            var card=ui.Panel(ui.Root,"Explorer card",new Rect(-550,-57,520,355),JourneyUI.Paper);
            ui.Text(card,"Name label","EXPLORER NAME",new Rect(-230,-155,460,30),16);
            nameInput=ui.NameInput(card,Data.playerName,new Rect(-230,-116,460,56));
            if(Data.started)
            {
                nameInput.interactable=false;
                ui.Button(card,"Continue / "+ChapterNames[Data.chapter-1],new Rect(-230,-38,460,54),()=>LoadChapter(Data.chapter));
                ui.Button(card,"Reset journey",new Rect(-230,30,460,48),ConfirmReset);
                if(Data.finished)ui.Button(card,"View certificate",new Rect(-230,90,460,48),()=>ShowCertificate());
            }
            else ui.Button(card,"Play / Begin in Spring",new Rect(-230,-38,460,54),NewJourney);
            ui.Text(card,"Save status",notice,new Rect(-230,146,460,42),15);
            var atlas=Resources.Load<Texture2D>("Journey/WorldAtlas");
            if(atlas){var art=new GameObject("Season illustration",typeof(RectTransform),typeof(UnityEngine.UI.RawImage));art.transform.SetParent(ui.Root,false);JourneyUI.Place(art.GetComponent<RectTransform>(),new Rect(95,-70,365,280));var image=art.GetComponent<UnityEngine.UI.RawImage>();image.texture=atlas;image.uvRect=new Rect(34f/1254,645f/1254,574f/1254,497f/1254);image.raycastTarget=false;}
            ui.Text(ui.Root,"Chapter trail","SPRING  >  SUMMER  >  FALL  >  WINTER\n\n12 lost artifacts. One evidence compass.\nA final challenge, and your way home.",new Rect(45,220,510,105),19,JourneyUI.Paper);
        }
        public void NewJourney()
        {
            string name=JourneyData.CleanName(nameInput?nameInput.text:Data.playerName);
            if(name.Length==0){notice="Enter an explorer name to start.";ShowMenu();return;}
            Data=new JourneyData{playerName=name,started=true};
            try{JourneyStorage.Write(SavePath,JsonUtility.ToJson(Data));}catch(Exception e)when(e is IOException||e is UnauthorizedAccessException){notice="Could not create your save. Check available disk space.";ShowMenu();return;}
            menu=false;ui.Clear();beginIntro?.Invoke();
        }
        public void LoadChapter(int chapter){if(loading||chapter<1||chapter>Data.unlocked)return;StartCoroutine(Load(chapter));}
        IEnumerator Load(int chapter)
        {
            loading=true;menu=false;Paused=false;Time.timeScale=1;AudioListener.pause=false;Background("THE JOURNEY CONTINUES",ChapterNames[chapter-1],"Returning to your trail...");
            if(chapter==1)yield return SceneManager.LoadSceneAsync("Spring");
            else
            {
                var prefab=Resources.Load<GameObject>(new[]{"","SummerLevel","FallLevel","WinterLevel"}[chapter-1]);
                if(!prefab){loading=false;notice="This chapter is unavailable. Your save has been kept.";menu=true;ShowMenu();yield break;}
                var old=SceneManager.GetActiveScene();foreach(var root in old.GetRootGameObjects())root.SetActive(false);
                var scene=SceneManager.CreateScene(new[]{"Spring","Summer","Fall","Winter"}[chapter-1]);SceneManager.SetActiveScene(scene);var world=Instantiate(prefab);SceneManager.MoveGameObjectToScene(world,scene);yield return SceneManager.UnloadSceneAsync(old);
            }
            yield return null;yield return null;Restore(chapter);Data.chapter=chapter;loading=false;yield return WorldReady(false);Save();
        }
        public void Pause(){if(menu||loading)return;Paused=true;Time.timeScale=0;AudioListener.pause=true;Save();ShowPause();}
        public void Resume(){resumeFrame=Time.frameCount;Paused=false;Time.timeScale=1;AudioListener.pause=false;ShowHUD();}
        void ShowHUD()
        {
            ui.Clear();var pause=ui.Button(ui.Root,"Pause / Esc",new Rect(470,298,145,38),Pause);var rect=pause.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=Vector2.one;rect.anchoredPosition=new Vector2(-169,-245);
            var b=ui.Button(ui.Root,"Artifacts "+Data.ArtifactCount+" / 12",new Rect(300,298,170,38),ShowArtifacts);var br=b.GetComponent<RectTransform>();br.anchorMin=br.anchorMax=Vector2.one;br.anchoredPosition=new Vector2(-194,-291);
        }
        void ShowPause()
        {
            Background("JOURNEY PAUSED",Data.playerName+"'s field journal",notice);
            ui.Button(ui.Root,"Resume",new Rect(-550,-55,320,52),Resume);
            ui.Button(ui.Root,"Save and return to menu",new Rect(-550,11,320,52),ReturnToMenu);
            ui.Button(ui.Root,"Artifacts / "+Data.ArtifactCount+" of 12",new Rect(-550,77,320,52),ShowArtifacts);
            ui.Button(ui.Root,"Guidance: "+(Data.assistedGuidance?"Assisted":"Explorer"),new Rect(-550,143,320,52),()=>{Data.assistedGuidance=!Data.assistedGuidance;Save();ShowPause();});
            ui.Button(ui.Root,"Reset journey",new Rect(-550,275,320,45),ConfirmReset);
            if(Data.finished)ui.Button(ui.Root,"Certificate",new Rect(-550,209,320,52),ShowCertificate);
            ui.Text(ui.Root,"Travel heading","REVISIT AN UNLOCKED SEASON",new Rect(-140,-55,660,35),20,JourneyUI.Gold);
            ui.Text(ui.Root,"Travel explanation","Each season remembers your location and lessons. Return for missed artifacts whenever you like.",new Rect(-140,-10,660,70),20,JourneyUI.Paper);
            for(int i=0;i<Data.unlocked;i++){int c=i+1;ui.Button(ui.Root,ChapterNames[i],new Rect(-140,72+i*53,660,44),()=>{if(Save())LoadChapter(c);else ShowPause();});}
        }
        public void ReturnToMenu(){if(!Save()){ShowPause();return;}Paused=false;Time.timeScale=1;AudioListener.pause=false;SceneManager.LoadScene("Intro");}
        void ConfirmReset()
        {
            Background("START AGAIN?","Reset this explorer's journey?","This removes saved locations, lessons, artifacts and the achievement for this profile.");
            ui.Button(ui.Root,"Keep my progress",new Rect(-550,10,510,56),()=>{if(menu)ShowMenu();else ShowPause();});
            ui.Button(ui.Root,"Reset / Start from the beginning",new Rect(40,10,510,56),()=>{
                try{JourneyStorage.Reset(SavePath);Data=new JourneyData();Paused=false;Time.timeScale=1;AudioListener.pause=false;if(menu)ShowMenu();else SceneManager.LoadScene("Intro");}
                catch(Exception e)when(e is IOException||e is UnauthorizedAccessException){notice="Reset failed. Your current session is still available.";if(menu)ShowMenu();else ShowPause();}
            });
        }
        public void ShowArtifacts()
        {
            if(!menu){Paused=true;Time.timeScale=0;AudioListener.pause=true;Save();}
            Background(Data.ArtifactCount==12?"ACHIEVEMENT / EVIDENCE KEEPER":"LOST ARTIFACTS",Data.ArtifactCount+" of 12 recovered",Data.ArtifactCount==12?"The compass is complete. Carry its lesson into the final challenge.":"Three artifacts are hidden in each season. Search clearings, shelters and the edges of landmarks.");
            for(int i=0;i<4;i++)ui.Text(ui.Root,"Collection "+i,ChapterNames[i]+"\n"+ArtifactRow(i),new Rect(-550+i*280,-55,260,110),19,JourneyUI.Paper);
            ui.Text(ui.Root,"Collection lesson",Data.ArtifactCount==12?ArtifactLesson:"Complete the collection to unlock the Evidence Compass lesson.\nUse the pause menu to revisit earlier seasons. Nothing is permanently missable.",new Rect(-550,75,1100,160),22,JourneyUI.Paper);
            ui.Button(ui.Root,"Return",new Rect(-550,270,300,48),()=>{if(menu)ShowMenu();else Resume();});
        }
        string ArtifactRow(int chapter){string s="";for(int i=chapter*3;i<chapter*3+3;i++)s+=(Data.artifacts[i]?"Found: ":"Missing: ")+JourneyWorld.ArtifactNames[i]+"\n";return s;}
        public void Collect(int id){if(id<0||id>=12||Data.artifacts[id])return;Data.artifacts[id]=true;Save();ShowArtifacts();}
        public void FinishJourney(){Data.finished=true;Data.completedOn=DateTime.Now.ToString("yyyy-MM-dd");Save();StartCoroutine(JourneyEnding.Play(this));}
        public void SetCinematicVisible(bool playing){cinematic=playing;ui.gameObject.SetActive(!playing);}
        public void ShowCertificate(){Paused=true;Time.timeScale=0;AudioListener.pause=true;Background("JOURNEY COMPLETE","AI Learning Certificate",Data.playerName+" completed the four seasons and the cumulative final challenge.");ui.Text(ui.Root,"Certificate preview","AMBROS' JOURNEY\n\nAwarded to "+Data.playerName+"\nAI foundations / independence / responsible prompts / evidence checking\n\nCompleted "+Data.completedOn+"\nAn in-game learning achievement, not a professional qualification.",new Rect(-550,-40,1100,240),25,JourneyUI.Paper);ui.Button(ui.Root,"Download PDF certificate",new Rect(-550,235,520,54),()=>JourneyCertificate.Export(Data,ui));ui.Button(ui.Root,"Return",new Rect(30,235,520,54),()=>{if(menu)ShowMenu();else Resume();});}
        void OnApplicationFocus(bool focused){if(TestMode)return;if(!focused&&!Paused&&Data.started&&!menu&&!loading&&CurrentChapter()>0)Pause();}
        void OnApplicationPause(bool paused){if(paused&&!TestMode)Save();}
        void OnApplicationQuit(){Save();Time.timeScale=1;AudioListener.pause=false;}
        void OnDestroy(){SceneManager.activeSceneChanged-=SceneChanged;if(Instance==this){Instance=null;Time.timeScale=1;AudioListener.pause=false;}}
    }
}
