using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using AmbrosJourney.Presentation;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace AmbrosJourney.Winter
{
    public sealed class WinterLevel : MonoBehaviour
    {
        public static readonly Vector2 Spawn=new Vector2(-32,-25);
        public WinterActor player,mage,maleWarrior,femaleWarrior,ember,littleMage,guardian,winterGuardian;
        public Transform[] stations;
        public Transform winterGate,caveEntrance,caveExit;
        public WinterWorld World;
        public Vector2 Arrival=>Progress.InCave?WinterWorld.CaveSpawn:Spawn;
        public GameObject arenaSeal,exitSeal;
        public SpriteRenderer[] runes,shields;
        public TMP_FontAsset font;
        public WinterProgress Progress=new WinterProgress();
        public bool InputBlocked=>AmbrosJourney.Journey.JourneySession.Blocked || !dialog || !journal || dialog.activeSelf || journal.activeSelf || Transitioning;
        public bool Transitioning;
        public bool IsDialogueOpen=>dialog && dialog.activeSelf;
        public string CurrentTitle=>currentTitle;
        GameObject dialog,journal;
        TextMeshProUGUI heading,copy,objective,compass,prompt,journalCopy,party;
        readonly List<UnityEngine.UI.Button> choices=new List<UnityEngine.UI.Button>();
        readonly List<Action> actions=new List<Action>();
        Action next;
        AudioSource effects,music;
        AudioClip good,bad,ward,strike;
        Canvas canvas;
        SeasonOverlay presentation;
        FinalActivityPanel finalPanel;
        SeasonNavigation navigation;
        int nearby=-1;
        string currentTitle;
        public void RestoreProgress(WinterProgress p)
        {
            if(p==null||!p.Valid())throw new ArgumentException("Invalid Winter checkpoint");
            Progress=p;if(p.Introduced)Close();journal.SetActive(false);if(p.WinterComplete){littleMage.follow=ember.transform;}World.RestoreRegion(p.InCave);RefreshWorld();
        }
        void Start()
        {
            Application.runInBackground=false;CreateUI();CreateAudio();
            mage.follow=player.transform;maleWarrior.follow=mage.transform;femaleWarrior.follow=maleWarrior.transform;ember.follow=femaleWarrior.transform;
            ShowPages(new[]{
                "Little Mage: Glad you made it to Frostveil! Some of the information along this trail doesn't add up. Let's check where it came from before we use it.",
                "Start with the Wayfarer's Ledger and Frozen Observatory. The notes will help you check their claims. Follow the lanterns toward the northern ridge when you're ready for the cave.",
                "The Warden outside the cave needs four of five answers right. Then find the Echo Archive inside. The final Keeper has eight new activities about AI safety. Get seven right to open the way home."
            },0,()=>Progress.Introduced=true);
            Debug.Log("WINTER_READY: Frostveil and Echo Cavern, six evidence challenges and cave guardian.");
        }
        void Update()
        {
            if(AmbrosJourney.Journey.JourneySession.Blocked)return;
            if(!canvas)return;nearby=Nearest();var k=Keyboard.current;
            if(k!=null && !Transitioning)
            {
                if(k.mKey.wasPressedThisFrame){music.mute=!music.mute;effects.mute=music.mute;}
                if(k.escapeKey.wasPressedThisFrame){if(journal.activeSelf)journal.SetActive(false);else if(dialog.activeSelf && Progress.Introduced)Close();}
                else if(k.jKey.wasPressedThisFrame && !dialog.activeSelf)ToggleJournal();
                else if(finalPanel&&finalPanel.IsOpen)finalPanel.HandleKeyboard(k);
                else if(dialog.activeSelf)
                {
                    if(actions.Count>0){int c=k.digit1Key.wasPressedThisFrame?0:k.digit2Key.wasPressedThisFrame?1:k.digit3Key.wasPressedThisFrame?2:-1;if(c>=0 && c<actions.Count)actions[c]();}
                    else if((k.eKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame) && next!=null){var n=next;next=null;n();}
                }
                else if(!journal.activeSelf && k.eKey.wasPressedThisFrame && nearby>=0)Interact(nearby);
            }
            RefreshWorld();
        }
        int Nearest()
        {
            Transform[] targets={littleMage.transform,stations[0],stations[1],stations[2],guardian.transform,winterGate,caveEntrance,caveExit,winterGuardian.transform};
            int result=-1;float best=2.8f;for(int i=0;i<targets.Length;i++){if(!targets[i].gameObject.activeInHierarchy)continue;float d=Vector2.Distance(player.transform.position,targets[i].position);if(d<best){best=d;result=i;}}return result;
        }
        public void Interact(int target)
        {
            if(!Progress.Introduced || Transitioning)return;
            if(target==0)Show("Little Mage / Follow the evidence",Progress.Complete?"You earned the Winter crest. Evidence, careful checking and honest uncertainty are stronger than a confident guess.":"Read the ledger in the western clearing and the observatory on the eastern ridge. Follow the lanterns north to the cave. The Winter Warden guards the cave entrance. The final Echo Keeper waits inside.",Close);
            else if(target>=1&&target<=3){if(stations[target-1].gameObject.activeInHierarchy)Mission(target-1);}
            else if(target==4&&Progress.InCave)Boss();
            else if(target==8&&!Progress.InCave)WinterBoss();
            else if(target==5)Show("Winter crest",Progress.Complete?"WINTER COMPLETE / EVIDENCE EXPLORER\nThe Little Mage joins your party. You may explore both maps and review your notes. The final Keeper is defeated. Open Pause to view your AI Learning Certificate.":"Earn the Winter crest, finish the cave lesson and get at least 7/8 right in the Echo Keeper challenge.",Close);
            else if(target==6&&!Progress.InCave&&!Progress.WinterComplete)WinterBoss();
            else if(target==6&&!Progress.InCave)Show("Echo Cavern / Entrance","A lantern glows beside the cave. Explore its branching passages to reach the Echo Archive, then find the Echo Keeper beyond the sealed northern hall. Your outdoor lessons stay with you.",()=>{Close();World.ChangeRegion(true);});
            else if(target==7&&Progress.InCave)Show("Return to Frostveil","Follow the daylight back outside. Your notes, evidence seals and guardian progress are kept.",()=>{Close();World.ChangeRegion(false);});
        }
        void Mission(int mission)
        {
            if(Progress.MissionComplete(mission)){Show(WinterContent.Landmarks[mission]+" / Rune restored",WinterContent.Notes[mission],Close);return;}
            Show(WinterContent.Landmarks[mission]+" / Field note",WinterContent.Notes[mission],()=>{Progress.ReadNote(mission);MissionQuestion(mission);});
        }
        void MissionQuestion(int mission)
        {
            if(Progress.MissionComplete(mission)){effects.PlayOneShot(ward,.6f);SeasonFeedback.Burst(stations[mission].position,new Color(1,.75f,.3f),24);Show("Evidence seal restored / "+Progress.RuneCount+" of 3",WinterContent.Notes[mission]+(Progress.CanFight?"\n\nThe northern cave seal opens. Follow the compass to the Echo Keeper.":"\n\nThe journal records this lesson. Follow the compass to another station."),Close);return;}
            var q=WinterContent.Missions[mission][Progress.Steps[mission]];
            Ask(WinterContent.Landmarks[mission]+" / "+(Progress.Steps[mission]+1)+" of "+WinterContent.Missions[mission].Length,q.Prompt,q.Choices,c=>{
                bool right=Progress.AnswerMission(mission,c);Effect(right);
                Show(right?"The evidence holds":"Check the evidence again",q.Explanation+(right?"":"\n\nTry again. Your completed choices stay safe."),()=>MissionQuestion(mission));
            });
        }
        void WinterBoss()
        {
            if(Progress.WinterComplete){Show("Frostbound Warden / Restored","The Winter crest is yours. Enter the cave behind me, find the Echo Archive, then face the final Keeper.",Close);return;}
            if(!Progress.CanFightWinter){Show("The Warden guards the entrance","Finish the Wayfarer's Ledger and Frozen Observatory first. Their two evidence seals unlock this challenge.",Close);return;}
            Show("Winter boss / Frostbound Warden","Five claims guard this cave. Score at least 4/5 to earn Winter's crest and enter. Your two outdoor lessons stay complete when you retry.",()=>{Progress.BeginWinterBoss();WinterBossQuestion();});
        }
        void WinterBossQuestion()
        {
            if(Progress.WinterBossFinished)
            {
                if(Progress.WinterComplete){littleMage.follow=ember.transform;Show("Winter crest earned / "+Progress.WinterBossScore+" of 5","The Warden steps aside. Little Mage joins you! Explore the cave behind the Warden to find the Echo Archive and your final challenge.",()=>{Close();AmbrosJourney.Journey.JourneySession.Instance?.Save();});}
                else Show("Winter score / "+Progress.WinterBossScore+" of 5","Score 4/5 to pass. Review your field notes or try again; your lessons are safe.",()=>{Progress.BeginWinterBoss();WinterBossQuestion();});
                return;
            }
            var q=WinterContent.Boss[Progress.WinterBossQuestion];
            Ask("Winter Warden / "+(Progress.WinterBossQuestion+1)+" of 5",q.Prompt,q.Choices,c=>{bool right=Progress.AnswerWinterBoss(c);Effect(right);if(right){player.Strike();winterGuardian.Flash(new Color(1,.8f,.3f));}else winterGuardian.Strike();Show(right?"Evidence accepted":"Check that claim",q.Explanation,WinterBossQuestion);});
        }
        void Boss()
        {
            if(Progress.Complete){Show("Final Keeper / Restored","Your way home is open. View your certificate from the pause menu, or keep exploring for artifacts.",()=>{Close();var session=AmbrosJourney.Journey.JourneySession.Instance;if(session){if(session.Data.finished)session.ShowCertificate();else session.FinishJourney();}});return;}
            if(!Progress.CanFight){Show("The final seal is closed","Earn the Winter crest outside, then complete the Echo Archive. All three evidence seals are needed.",Close);return;}
            Show("Final boss / The Echo Keeper","Eight activities put your AI safety skills to work in new situations. Pick details, build a request, match jobs and check evidence. Get at least 7/8 right.\n\nYou can revisit any unlocked season from Pause to find missed artifacts and review notes. Your lessons stay safe on a retry.",()=>{Progress.BeginBoss();BossQuestion();});
        }
        void BossQuestion()
        {
            if(Progress.BossFinished)
            {
                if(Progress.Complete){effects.PlayOneShot(ward,.7f);Show("The way home / "+Progress.BossScore+" of 8","The Echo Keeper's final seal opens. You have learned to question confidently worded claims, protect your privacy, practice your own skills and check evidence.\n\nYour AI Learning Certificate is earned. Continue to return home.",()=>{Close();AmbrosJourney.Journey.JourneySession.Instance?.FinishJourney();});}
                else Show("Final score / "+Progress.BossScore+" of 8","You need 7/8 to pass. Your crests and collected artifacts are safe. Continue to retry, or close this conversation to review your notes and explore.",()=>{Progress.BeginBoss();BossQuestion();});
                return;
            }
            var q=WinterContent.Final[Progress.BossQuestion];
            Show("Echo Keeper: Activity "+(Progress.BossQuestion+1)+" of 8",q.Prompt,null);
            if(!finalPanel)finalPanel=gameObject.AddComponent<FinalActivityPanel>();
            finalPanel.Present(presentation,q,response=>{
                bool right=Progress.AnswerFinal(response);Effect(right);
                if(right){player.Strike();guardian.Flash(new Color(1,.8f,.3f));}else guardian.Strike();
                effects.PlayOneShot(strike,.35f);
                Show(right?"That works":"Let's look at that again",q.Explanation+"\n\nScore: "+Progress.BossScore+" / "+Progress.BossQuestion+" activities.",BossQuestion);
            });
        }
        void ShowPages(string[] pages,int index,Action complete)
        {Show("Winter / Little Mage's trail",pages[index],()=>{if(index+1<pages.Length)ShowPages(pages,index+1,complete);else{complete();Close();}});}
        void Effect(bool right)
        {
            effects.PlayOneShot(right?good:bad,.45f);presentation.Feedback(right);
            player.Flash(right?new Color(.65f,1,.65f):new Color(1,.5f,.4f));
            if(right)SeasonFeedback.Burst(player.transform.position,new Color(1,.8f,.3f));
        }
        void Close(){if(finalPanel)finalPanel.Hide();dialog.SetActive(false);actions.Clear();next=null;if(music)music.pitch=1;}
        void Show(string title,string body,Action onContinue)
        {
            if(finalPanel)finalPanel.Hide();
            currentTitle=title;heading.text=title;copy.text=body;dialog.SetActive(true);journal.SetActive(false);next=onContinue;actions.Clear();
            foreach(var c in choices)c.gameObject.SetActive(false);
            choices[0].gameObject.SetActive(true);SetButton(choices[0],"Continue [ E ]",()=>{if(next!=null){var n=next;next=null;n();}});
            presentation.Present(false);
        }
        void Ask(string title,string body,string[] options,Action<int> answer)
        {
            Show(title,body,null);for(int i=0;i<choices.Count;i++){choices[i].gameObject.SetActive(i<options.Length);if(i>=options.Length)continue;int index=i;Action choose=()=>answer(index);actions.Add(choose);SetButton(choices[i],(i+1)+".  "+options[i],choose);}
            presentation.Present(true);
        }
        void SetButton(UnityEngine.UI.Button button,string label,Action action)
        {button.GetComponentInChildren<TextMeshProUGUI>().text=label;button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>action());}
        void ToggleJournal()
        {
            journal.SetActive(!journal.activeSelf);if(!journal.activeSelf)return;
            journalCopy.text="";
            for(int i=0;i<3;i++)journalCopy.text+=(Progress.Studied[i]?WinterContent.Notes[i]:WinterContent.Landmarks[i]+" / Find and read this station's note.")+"\nProgress: "+Progress.Steps[i]+" / "+WinterContent.Missions[i].Length+"\n\n";
            journalCopy.text+=party.text;
            var titles=new string[3];var notes=new string[3];
            for(int i=0;i<3;i++){titles[i]=WinterContent.Landmarks[i]+"  /  "+Progress.Steps[i]+" of "+WinterContent.Missions[i].Length+" challenges";notes[i]=Progress.Studied[i]?WinterContent.Notes[i].Split('\n')[1]:"Find the field note at this landmark to collect its lesson.";}
            presentation.SetJournal(titles,notes,party.text);
        }
        public Vector2 Destination()
        {
            if(!Progress.Introduced)return littleMage.transform.position;
            if(!Progress.InCave){for(int i=0;i<2;i++)if(!Progress.MissionComplete(i))return stations[i].position;return Progress.WinterComplete?caveEntrance.position:winterGuardian.transform.position;}
            for(int i=0;i<2;i++)if(!Progress.MissionComplete(i))return caveExit.position;
            if(!Progress.MissionComplete(2))return stations[2].position;
            return Progress.Complete?(Vector2)winterGate.position:guardian.transform.position;
        }
        void RefreshWorld()
        {
            arenaSeal.SetActive(Progress.RuneCount<3);exitSeal.SetActive(!Progress.Complete);
            for(int i=0;i<3;i++)runes[i].color=Progress.MissionComplete(i)?new Color(.6f,1,.85f):new Color(.6f,.75f,1);
            for(int i=0;i<shields.Length;i++)shields[i].enabled=!Progress.Complete&&i>=Progress.BossScore;
            string quest=Progress.Complete?"Winter complete / Explore or return outside":!Progress.InCave?(Progress.MissionComplete(0)?Progress.MissionComplete(1)?(Progress.WinterComplete?"Enter the cave behind the Warden":"North ridge / Challenge the Winter Warden"):"Frozen Observatory / Compare evidence":"Wayfarer's Ledger / Check the source"):(Progress.RuneCount==3?"Echo Keeper / Final challenge: 7 of 8":!Progress.MissionComplete(0)||!Progress.MissionComplete(1)?"Return outside for the two field lessons":"Explore the passages to the Echo Archive");
            objective.text=(Progress.InCave?"ECHO CAVERN":"FROSTVEIL")+"  |  SEALS "+Progress.RuneCount+" / 3\n"+quest;
            var destination=Destination();if(!InputBlocked)presentation.SetCompass(player.transform.position,navigation.Next(player.transform.position,destination),destination);
            prompt.text=InputBlocked||nearby<0?"":"[ E ]  "+new[]{"Talk to the Little Mage","Wayfarer's Ledger","Frozen Observatory","Echo Archive","Face the Warden","Winter crest","Enter Echo Cavern","Return outside","Challenge the Winter Warden"}[nearby];
            party.text=Progress.WinterComplete?"PARTY / Ambros + Ancient Mage + Warriors + Emberfist + Little Mage":"PARTY / Ambros + Ancient Mage + Warriors + Emberfist";
        }
        public void RegionChanged(){navigation=new SeasonNavigation(Progress.InCave?WinterWorld.CaveMin:WinterWorld.Min,Progress.InCave?WinterWorld.CaveMax:WinterWorld.Max);}
        void CreateUI()
        {
            var ui=new GameObject("Winter interface");ui.transform.SetParent(transform,false);
            presentation=ui.AddComponent<SeasonOverlay>();presentation.Initialize(font,4);
            canvas=presentation.Canvas;dialog=presentation.Dialog;journal=presentation.Journal;
            heading=presentation.Heading;copy=presentation.Copy;objective=presentation.Objective;compass=presentation.CompassText;prompt=presentation.Prompt;journalCopy=presentation.JournalCopy;party=presentation.Party;
            choices.AddRange(presentation.Choices);
            navigation=new SeasonNavigation(WinterWorld.Min,WinterWorld.Max);
        }
        AudioClip Tone(string name, float duration, float frequency, bool descending = false)
        {
            int rate = 22050; var data = new float[(int)(duration * rate)];
            for (int i = 0; i < data.Length; i++) { float t = (float)i / rate; float f = descending ? frequency * (1 - .5f * t / duration) : frequency * (1 + .3f * t / duration); data[i] = Mathf.Sin(t * f * Mathf.PI * 2) * Mathf.Sin(Mathf.PI * t / duration) * .18f; }
            var clip = AudioClip.Create(name, data.Length, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        void CreateAudio()
        {
            effects = gameObject.AddComponent<AudioSource>(); effects.playOnAwake = false;
            music = gameObject.AddComponent<AudioSource>(); music.playOnAwake = false; music.loop = true; music.volume = .22f;
            good = Tone("Winter positive", .2f, 660); bad = Tone("Winter reconsider", .25f, 210, true); ward = Tone("Ward restored", .8f, 520); strike = Tone("Shield impact", .12f, 100, true);
            music.clip = Resources.Load<AudioClip>("SeasonMusic/zephiramusic-lofi-winter-610024");
            if (music.clip) music.Play();
            else Debug.LogError("Missing Winter background music.");
        }
        void OnDestroy()
        {
            foreach (var c in new[] { good, bad, ward, strike }) if (c) Destroy(c);
        }
    }
}
