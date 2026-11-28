// Coordinates the in-memory run, menus, pause controls, chapter handoffs and certificate access.
using AmbrosJourney.Audio;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using AmbrosJourney.Summer;
using AmbrosJourney.Fall;
using AmbrosJourney.Winter;
using AmbrosJourney.Presentation;
namespace AmbrosJourney.Journey
{
    [DefaultExecutionOrder(-500)]
    public sealed class JourneySession : MonoBehaviour
    {
        public static JourneySession Instance {get;private set;}
        public static bool Blocked=>SeasonTransition.Active||BossPortalSequence.Busy||Instance&&(Instance.Paused||Instance.loading||Instance.menu||Instance.cinematic||Instance.resumeFrame==Time.frameCount&&Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame);
        public static bool TestMode;
        public static bool CanUsePortals=>!Blocked&&(!Instance||!Instance.portalProtected);
        bool portalProtected;Vector2 protectedPosition;
        Transform cachedPlayer;
        public JourneyData Data {get;private set;}
        public bool Paused {get;private set;}
        Action backAction;bool artifactsOpen;
        AudioSource menuMusic;
        int resumeFrame=-1;JourneyUI ui;bool menu,loading,cinematic,ending;string notice="",nameDraft="";Action beginIntro;TMP_InputField nameInput;
        public static readonly string[] ChapterNames={"Spring Valley","Summer Valley","Emberwood","Frostveil"};
        public const string ArtifactLesson="THE EVIDENCE COMPASS\nBefore sharing, leave out private details, check the original source and look for separate evidence. Say what you still haven't checked.\n\nSeveral AI tools can agree and still be wrong. They may repeat the same mistake, so matching replies don't count as separate checks.";
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetStatics(){Instance=null;TestMode=false;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Install(){if(!Instance){var host=new GameObject("Journey / session and menus");DontDestroyOnLoad(host);host.AddComponent<JourneySession>();}}
#if UNITY_EDITOR
        public void BeginTestProfile(){TestMode=true;Data=new JourneyData{playerName="Zoë Explorer",started=true};}
#endif
        void Awake()
        {
            if(Instance){Destroy(gameObject);return;}Instance=this;Data=new JourneyData();
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
        void SceneChanged(Scene previous,Scene current){cachedPlayer=null;if(!loading&&current.name!="Intro")StartCoroutine(WorldReady(true));}
        IEnumerator WorldReady(bool restore)
        {
            yield return null;yield return null;
            int chapter=CurrentChapter();if(chapter==0)yield break;
            Data.chapter=chapter;Data.unlocked=Math.Max(Data.unlocked,chapter);
            if(restore&&Data.chapters[chapter-1].visited)Restore(chapter);
            if(!SceneComponent<JourneyWorld>()){var world=new GameObject("Journey exploration");world.AddComponent<JourneyWorld>();}
            if(!menu&&!Paused)ShowHUD();
        }
        public static int CurrentChapter(){switch(SceneManager.GetActiveScene().name){case "Spring":return 1;case "Summer":return 2;case "Fall":return 3;case "Winter":return 4;default:return 0;}}
        static T SceneComponent<T>() where T:Component
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if(!root.activeInHierarchy)continue;
                var component=root.GetComponentInChildren<T>();if(component)return component;
            }
            return null;
        }
        public Transform Player()
        {
            var scene=SceneManager.GetActiveScene();
            if(cachedPlayer&&cachedPlayer.gameObject.activeInHierarchy&&cachedPlayer.gameObject.scene==scene)return cachedPlayer;
            cachedPlayer=null;
            switch(CurrentChapter())
            {
                case 1:var spring=SceneComponent<SpringPlayer>();if(spring)cachedPlayer=spring.transform;break;
                case 2:var summer=SceneComponent<SummerLevel>();if(summer&&summer.player)cachedPlayer=summer.player.transform;break;
                case 3:var fall=SceneComponent<FallLevel>();if(fall&&fall.player)cachedPlayer=fall.player.transform;break;
                case 4:var winter=SceneComponent<WinterLevel>();if(winter&&winter.player)cachedPlayer=winter.player.transform;break;
            }
            return cachedPlayer&&cachedPlayer.gameObject.scene==scene?cachedPlayer:null;
        }
        void Update()
        {
            if(portalProtected){var player=Player();if(player&&Vector2.Distance(player.position,protectedPosition)>2)portalProtected=false;}
            if(!cinematic&&!loading&&!SeasonTransition.Active&&Keyboard.current!=null)
            {
                if(Keyboard.current.escapeKey.wasPressedThisFrame){GameAudio.Click();HandleBack();}
                else if(artifactsOpen&&Keyboard.current.spaceKey.wasPressedThisFrame){GameAudio.Click();HandleBack();}
                else if(!menu&&!Paused&&Keyboard.current.iKey.wasPressedThisFrame){GameAudio.Click();ShowArtifacts();}
            }
        }
        // One owner consumes Escape before world scripts, so closing a panel never also pauses.
        public void HandleBack()
        {
            if(cinematic||loading||SeasonTransition.Active||BossPortalSequence.Busy)return;
            if(backAction!=null){var back=backAction;resumeFrame=Time.frameCount;back();return;}
            if(menu)return;
            bool dismissed=false;
            var spring=SceneComponent<SpringGuide>();if(spring)dismissed=spring.DismissPanel();
            var summer=SceneComponent<SummerLevel>();if(summer)dismissed=summer.DismissPanel();
            var fall=SceneComponent<FallLevel>();if(fall)dismissed=fall.DismissPanel();
            var winter=SceneComponent<WinterLevel>();if(winter)dismissed=winter.DismissPanel();
            if(dismissed){resumeFrame=Time.frameCount;return;}
            if(CurrentChapter()>0)Pause();
        }
        void BackToMenu(){menu=true;Paused=false;Time.timeScale=1;AudioListener.pause=false;ShowMenu();}
        // Chapter snapshots live only in memory, for revisiting seasons during this run.
        public bool CaptureChapter()
        {
            if(loading||!Data.started||CurrentChapter()==0)return true;
            int chapter=CurrentChapter();var player=Player();if(!player)return false;
            var entry=Data.chapters[chapter-1];entry.visited=true;entry.x=player.position.x;entry.y=player.position.y;Data.chapter=chapter;Data.unlocked=Math.Max(Data.unlocked,chapter);
            switch(chapter){case 1:entry.progress=JsonUtility.ToJson(SceneComponent<SpringGuide>().CaptureProgress());break;case 2:entry.progress=JsonUtility.ToJson(SceneComponent<SummerLevel>().Progress);break;case 3:entry.progress=JsonUtility.ToJson(SceneComponent<FallLevel>().Progress);break;case 4:var w=SceneComponent<WinterLevel>();entry.inCave=w.Progress.InCave;entry.progress=JsonUtility.ToJson(w.Progress);break;}
            return true;
        }
        void Restore(int chapter)
        {
            var e=Data.chapters[chapter-1];if(!e.visited)return;
            try
            {
                switch(chapter){case 1:SceneComponent<SpringGuide>().RestoreProgress(JsonUtility.FromJson<SpringState>(e.progress));break;case 2:SceneComponent<SummerLevel>().RestoreProgress(JsonUtility.FromJson<SummerProgress>(e.progress));break;case 3:SceneComponent<FallLevel>().RestoreProgress(JsonUtility.FromJson<FallProgress>(e.progress));break;case 4:SceneComponent<WinterLevel>().RestoreProgress(JsonUtility.FromJson<WinterProgress>(e.progress));break;}
                var position=new Vector2(e.x,e.y);
                if(chapter==4&&!SceneComponent<WinterLevel>().Progress.InCave)position=WinterWorld.ConstrainOutdoorCheckpoint(position);
                var p=Player();p.position=position;var body=p.GetComponent<Rigidbody2D>();if(body){body.position=p.position;body.linearVelocity=Vector2.zero;}
                var camera=Camera.main;if(camera)camera.transform.position=new Vector3(position.x,position.y,-10);
                portalProtected=true;protectedPosition=p.position;Physics2D.SyncTransforms();
            }
            catch(Exception error){notice="This chapter's checkpoint could not be restored. Its starting area has been loaded.";Debug.LogWarning("Checkpoint recovery: "+error.Message);}
        }
        public void OpenMenu(Action playIntro)
        {
            beginIntro=playIntro;ending=false;menu=true;Paused=false;Time.timeScale=1;AudioListener.pause=false;ShowMenu();
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
            PlayMenuMusic();
            artifactsOpen=false;backAction=null;ui.KeyboardNavigation=true;
            JourneyMenu.Background(ui);
            nameInput=null;
            JourneyMenu.Text(ui,"Name label","YOUR EXPLORER NAME",new Rect(-554,-43,350,28),16,JourneyMenu.Gold);
            nameInput=ui.NameInput(ui.Root,nameDraft,new Rect(-562,-9,340,48));
            nameInput.GetComponent<UnityEngine.UI.Image>().color=new Color(.025f,.065f,.05f,.87f);
            nameInput.textComponent.color=JourneyMenu.Cream; nameInput.textComponent.font=ui.MenuFont;
            var placeholder=(TMP_Text)nameInput.placeholder;placeholder.font=ui.MenuFont;placeholder.color=JourneyMenu.Muted;
            foreach(var text in new[]{nameInput.textComponent,placeholder})
            {
                JourneyUI.Place(text.rectTransform,new Rect(-155,-23,310,46));text.fontSize=22;
                // Use font-metric centering so TMP places its caret inside the name box.
                text.alignment=TextAlignmentOptions.Left;text.textWrappingMode=TextWrappingModes.NoWrap;
                text.enableAutoSizing=true;text.fontSizeMin=10;text.fontSizeMax=22;
            }
            nameInput.customCaretColor=true;nameInput.caretColor=JourneyMenu.Gold;
            JourneyMenu.Frame(ui,new Rect(-562,-9,340,48));
            JourneyMenu.Button(ui,"New Journey",63,NewJourney,true);
            JourneyMenu.Button(ui,"Settings",132,ShowMenuSettings);
            JourneyMenu.Button(ui,"Quit",186,QuitGame);
            if(Data.finished&&!Data.certificatePending)JourneyMenu.Button(ui,"View Certificate",239,ShowCertificate,height:32);
            if(!string.IsNullOrEmpty(notice)&&!Data.finished)
            {
                var status=JourneyMenu.Text(ui,"Menu status",notice,new Rect(-554,241,425,31),15,JourneyMenu.Muted);
                status.textWrappingMode=TextWrappingModes.Normal;
            }
        }
        void ShowMenuSettings()
        {
            backAction=BackToMenu;
            // Keep a fresh explorer's draft name when they visit settings before playing.
            if(nameInput)nameDraft=JourneyData.CleanName(nameInput.text);
            JourneyMenu.Background(ui);nameInput=null;
            JourneyMenu.Text(ui,"Settings heading","Journey settings",new Rect(-550,-45,420,46),30);
            JourneyMenu.Text(ui,"Guidance status","Trail guidance: always on",new Rect(-534,20,410,40),24,JourneyMenu.Cream);
            var description=JourneyMenu.Text(ui,"Guidance description","The guide arrow stays visible in every season and points toward your next lesson or goal.",new Rect(-534,78,360,65),19,JourneyMenu.Muted);description.textWrappingMode=TextWrappingModes.Normal;
            JourneyMenu.Button(ui,MusicLabel,150,()=>{GameAudio.ToggleMusic();ShowMenuSettings();},false,42);
            JourneyMenu.Button(ui,"Back",210,ShowMenu,true);
        }
        void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#elif UNITY_WEBGL
            notice="You can close this browser tab. A new visit starts a new journey.";ShowMenu();
#else
            Application.Quit();
#endif
        }
        void PlayMenuMusic()
        {
            if(!menuMusic)
            {
                menuMusic=gameObject.AddComponent<AudioSource>();menuMusic.playOnAwake=false;menuMusic.loop=true;menuMusic.volume=.22f;menuMusic.spatialBlend=0;
                menuMusic.clip=Resources.Load<AudioClip>("SeasonMusic/andriig-spring-easter-day-music-507171");GameAudio.RegisterMusic(menuMusic);
            }
            if(menuMusic.clip&&!menuMusic.isPlaying)menuMusic.Play();
        }
        public void NewJourney()
        {
            string name=JourneyData.CleanName(nameInput?nameInput.text:nameDraft);
            if(name.Length==0){notice="Enter an explorer name to start.";ShowMenu();return;}
            var fresh=new JourneyData{playerName=name,started=true,assistedGuidance=Data.assistedGuidance};
            Data=fresh;notice="";portalProtected=false;cachedPlayer=null;
            if(menuMusic)menuMusic.Stop();
            menu=false;ui.Clear();beginIntro?.Invoke();
        }
        public void LoadChapter(int chapter){if(loading||SeasonTransition.Active||!Data.started||chapter<1||chapter>Data.unlocked)return;StartCoroutine(Load(chapter));}
        IEnumerator Load(int chapter)
        {
            loading=true;menu=false;Paused=false;Time.timeScale=1;AudioListener.pause=false;Background("THE JOURNEY CONTINUES",ChapterNames[chapter-1],"Returning to your trail...");
            if(chapter==1)yield return SceneManager.LoadSceneAsync("Spring");
            else
            {
                var prefab=Resources.Load<GameObject>(new[]{"","SummerLevel","FallLevel","WinterLevel"}[chapter-1]);
                if(!prefab){loading=false;notice="This chapter is unavailable. You can keep exploring the current season.";Pause();yield break;}
                var old=SceneManager.GetActiveScene();foreach(var root in old.GetRootGameObjects())root.SetActive(false);
                var scene=SceneManager.CreateScene(new[]{"Spring","Summer","Fall","Winter"}[chapter-1]);SceneManager.SetActiveScene(scene);var world=Instantiate(prefab);SceneManager.MoveGameObjectToScene(world,scene);yield return SceneManager.UnloadSceneAsync(old);
            }
            yield return null;yield return null;Restore(chapter);Data.chapter=chapter;loading=false;yield return WorldReady(false);CaptureChapter();
        }
        public void Pause(){if(menu||loading||cinematic||SeasonTransition.Active||BossPortalSequence.Busy)return;Paused=true;Time.timeScale=0;AudioListener.pause=true;CaptureChapter();ShowPause();}
        public void Resume(){resumeFrame=Time.frameCount;Paused=false;Time.timeScale=1;AudioListener.pause=false;ShowHUD();}
        static string MusicLabel=>"Music: "+(GameAudio.MusicEnabled?"On":"Off");
        void ShowHUD()
        {
            artifactsOpen=false;backAction=null;ui.KeyboardNavigation=false;
            if(UnityEngine.EventSystems.EventSystem.current)UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            ui.Clear();
            var pause=ui.Button(ui.Root,"Pause / Esc",new Rect(470,298,145,38),Pause);var rect=pause.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=Vector2.one;rect.anchoredPosition=new Vector2(-379,-136);
            var b=ui.Button(ui.Root,Data.ArtifactCount+"/"+Data.artifacts.Length+" Artifacts",new Rect(300,298,190,38),ShowArtifacts);var br=b.GetComponent<RectTransform>();br.anchorMin=br.anchorMax=Vector2.one;br.anchoredPosition=new Vector2(-214,-136);

        }
        void ShowPause()
        {
            artifactsOpen=false;backAction=Resume;ui.KeyboardNavigation=true;
            JourneyPauseJournal.Open(ui,Data,notice);
            JourneyPauseJournal.Button(ui,"Resume",new Rect(-510,-68,445,49),Resume,true);
            JourneyPauseJournal.Button(ui,"End run and return to menu",new Rect(-510,-7,445,42),ShowConfirmEndRun);
            JourneyPauseJournal.Button(ui,"Artifacts / "+Data.ArtifactCount+" of 12",new Rect(-510,44,445,42),ShowArtifacts);
            JourneyPauseJournal.Button(ui,MusicLabel,new Rect(-510,95,445,42),()=>{GameAudio.ToggleMusic();ShowPause();});
            ui.Text(ui.Root,"Guidance status","Trail guidance: always on",new Rect(-490,226,405,20),17,new Color(.43f,.40f,.30f));
            if(Data.finished&&!Data.certificatePending)JourneyPauseJournal.Button(ui,"Certificate",new Rect(-510,146,445,42),ShowCertificate);
            for(int i=0;i<Data.unlocked;i++){int c=i+1;JourneyPauseJournal.Chapter(ui,i,ChapterNames[i],()=>{if(CaptureChapter())LoadChapter(c);else ShowPause();},c==Data.chapter);}

        }
        public void ReturnToMenu()
        {
            Data=new JourneyData();nameDraft="";notice="";portalProtected=false;cachedPlayer=null;
            Paused=false;menu=true;Time.timeScale=1;AudioListener.pause=false;SceneManager.LoadScene("Intro");
        }
        void ReturnAfterEnding()
        {
            Data.started=false;nameDraft="";portalProtected=false;cachedPlayer=null;
            Paused=false;menu=true;Time.timeScale=1;AudioListener.pause=false;SceneManager.LoadScene("Intro");
        }
        void ShowConfirmEndRun()
        {
            backAction=ShowPause;
            Background("END THIS RUN?","Return to the main menu?","Your lessons and artifacts last for this run only. Returning to the menu starts your next journey from the beginning.");
            ui.Button(ui.Root,"Keep playing",new Rect(-550,10,510,56),ShowPause);
            ui.Button(ui.Root,"End run / Main menu",new Rect(40,10,510,56),ReturnToMenu);
        }
        public void ShowArtifacts()
        {
            if(SeasonTransition.Active)return;
            bool fromPause=Paused;
            artifactsOpen=true;
            Action returnTo=()=>{artifactsOpen=false;if(menu)BackToMenu();else if(fromPause)ShowPause();else Resume();};
            backAction=returnTo;ui.KeyboardNavigation=true;
            if(!menu){Paused=true;Time.timeScale=0;AudioListener.pause=true;CaptureChapter();}
            Background(Data.ArtifactCount==12?"ACHIEVEMENT / EVIDENCE KEEPER":"LOST ARTIFACTS",Data.ArtifactCount+" of 12 recovered",Data.ArtifactCount==12?"Your collection is complete. Carry its lesson into the final challenge.":"Three artifacts are hidden in each season. Search clearings, shelters and the edges of landmarks.");
            var world=FindAnyObjectByType<WorldCohesion>();
            for(int chapter=0;chapter<4;chapter++)
            {
                float x=-550+chapter*280;
                ui.Text(ui.Root,"Collection "+chapter,ChapterNames[chapter],new Rect(x,-65,260,30),21,JourneyUI.Gold);
                for(int item=0;item<3;item++)
                {
                    int id=chapter*3+item;float y=-28+item*50;
                    if(world)
                    {
                        var icon=ui.Panel(ui.Root,"Relic icon "+id,new Rect(x,y,38,38),Data.artifacts[id]?Color.white:new Color(.45f,.45f,.45f,.6f)).GetComponent<UnityEngine.UI.Image>();
                        icon.sprite=world.ArtifactIcon(id);icon.preserveAspect=true;icon.raycastTarget=false;
                    }
                    ui.Text(ui.Root,"Relic name "+id,(Data.artifacts[id]?"Found · ":"Hidden · ")+JourneyWorld.ArtifactNames[id],new Rect(x+46,y,212,46),17,JourneyUI.Paper);
                }
            }
            ui.Text(ui.Root,"Collection lesson",Data.ArtifactCount==12?ArtifactLesson:"Complete the collection to unlock the Evidence Compass lesson.\nUse the pause menu to revisit earlier seasons during this run.",new Rect(-550,120,1100,140),20,JourneyUI.Paper);
            ui.Button(ui.Root,"Return / Esc / Space",new Rect(-550,270,350,48),returnTo);
        }
        public void Collect(int id){if(id<0||id>=12||Data.artifacts[id])return;Data.artifacts[id]=true;GameAudio.Play(SoundCue.Pickup,.65f);CaptureChapter();ShowArtifacts();}
        public void RecordFinalVictory()
        {
            if(!Data.finished){Data.finished=true;Data.certificatePending=true;Data.completedOn=DateTime.Now.ToString("yyyy-MM-dd");}
            CaptureChapter();
        }
        public void FinishJourney()
        {
            RecordFinalVictory();Data.certificatePending=false;CaptureChapter();ShowCertificate();
        }
        public void SetCinematicVisible(bool playing){cinematic=playing;ui.gameObject.SetActive(!playing);}
        public void ShowCertificate()
        {
            if(!Data.finished||Data.certificatePending)return;
            if(menu&&nameInput)nameDraft=JourneyData.CleanName(nameInput.text);
            backAction=ExitCertificate;ui.KeyboardNavigation=true;
            Paused=true;Time.timeScale=0;AudioListener.pause=true;ui.gameObject.SetActive(true);ui.Clear();
            var backdrop=ui.Panel(ui.Root,"Certificate backdrop",new Rect(-5000,-5000,10000,10000),new Color(.06f,.12f,.11f,.96f));backdrop.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            JourneyCertificateView.Draw(ui,Data,.86f);
            ui.Button(ui.Root,"Download PDF certificate",new Rect(-430,312,410,40),()=>JourneyCertificate.Export(Data,ui));
            ui.Button(ui.Root,"Exit",new Rect(20,312,410,40),ExitCertificate);
        }
        public void EnterHomePortal()
        {
            if(ending||!CanUsePortals||!Data.finished||Data.certificatePending||!Data.homePortalOpened)return;
            ending=true;Paused=true;Time.timeScale=0;AudioListener.pause=true;CaptureChapter();
            StartCoroutine(JourneyEnding.Play(this,ReturnAfterEnding));
        }
        public void ExitCertificate()
        {
            var winter=SceneComponent<WinterLevel>();
            if(menu){BackToMenu();return;}
            if(!winter||!winter.Progress.InCave){Resume();return;}
            Data.homePortalOpened=true;Resume();
            FindAnyObjectByType<BossPortalSequence>()?.RevealHomePortal();CaptureChapter();
        }
        void OnApplicationFocus(bool focused){if(TestMode)return;if(!focused&&!Paused&&Data.started&&!menu&&!loading&&CurrentChapter()>0)Pause();}
        void OnApplicationQuit(){Time.timeScale=1;AudioListener.pause=false;}
        void OnDestroy(){SceneManager.activeSceneChanged-=SceneChanged;if(Instance==this){Instance=null;Time.timeScale=1;AudioListener.pause=false;}}
    }
}
