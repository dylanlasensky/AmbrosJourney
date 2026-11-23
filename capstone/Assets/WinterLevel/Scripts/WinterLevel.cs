using AmbrosJourney.Audio;
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
        public const float GuideTalkRadius=4f;
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
        int dialogVersion;
        AudioSource effects,music;
        AudioClip good,bad,ward,strike;
        Canvas canvas;
        SeasonOverlay presentation;
        SeasonWorldLabel[] lessonLabels;
        FinalActivityPanel finalPanel;
        public WinterCertificateReward CertificateReward {get;private set;}
        SeasonWorldLabel littleMageLabel;
        int nearby=-1;
        string currentTitle;
        public void RestoreProgress(WinterProgress p)
        {
            if(p==null||!p.Valid())throw new ArgumentException("Invalid Winter checkpoint");
            Progress=p;Close();journal.SetActive(false);World.RestoreRegion(p.InCave);if(p.Complete)BeginCertificateReward(false);RefreshWorld();AmbrosJourney.Journey.BossPortalSequence.RestoreCheckpoint();
        }
        void Start()
        {
            foreach(var flag in shields)if(flag)flag.gameObject.SetActive(false);
            winterGate.gameObject.SetActive(false);
            Application.runInBackground=false;CreateUI();CreateAudio();
            mage.follow=player.transform;maleWarrior.follow=mage.transform;femaleWarrior.follow=maleWarrior.transform;ember.follow=femaleWarrior.transform;
            Close();
            Debug.Log("WINTER_READY: Frostveil and Echo Cavern, six evidence challenges and cave guardian.");
        }
        void Welcome()=>Show("Little Mage / Welcome to Frostveil","Some AI claims sound convincing but need checking. Read the Wayfarer's Ledger and Frozen Observatory boards with E. They will help you check sources and compare evidence.\n\nThen face the Warden at the cave. Your field notes stay in J.",()=>{Progress.Introduced=true;Close();});
        public bool DismissPanel()
        {
            if(journal&&journal.activeSelf){journal.SetActive(false);return true;}
            if(dialog&&dialog.activeSelf){Progress.Introduced=true;Close();return true;}
            return false;
        }
        void Update()
        {
            if(AmbrosJourney.Journey.JourneySession.Blocked)return;
            if(!canvas)return;nearby=Nearest();var k=Keyboard.current;
            if(k!=null && !Transitioning)
            {
                if(k.mKey.wasPressedThisFrame){GameAudio.ToggleMute();}
                if(k.escapeKey.wasPressedThisFrame)DismissPanel();
                else if(k.jKey.wasPressedThisFrame && !dialog.activeSelf)ToggleJournal();
                else if(finalPanel&&finalPanel.IsOpen)finalPanel.HandleKeyboard(k);
                else if(dialog.activeSelf)
                {
                    if(actions.Count>0){int c=k.digit1Key.wasPressedThisFrame?0:k.digit2Key.wasPressedThisFrame?1:k.digit3Key.wasPressedThisFrame?2:-1;if(c>=0 && c<actions.Count)actions[c]();}
                    else if((k.eKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame) && next!=null){var n=next;next=null;n();}
                }
                else if(!journal.activeSelf && k.eKey.wasPressedThisFrame){if(CertificateReward&&CertificateReward.CanCollect)CertificateReward.TryCollect();else if(nearby>=0)Interact(nearby);}
            }
            if(Progress.Complete&&!CertificateReward&&!dialog.activeSelf)BeginCertificateReward(true);
            RefreshWorld();
        }
        public bool CanEnterCave=>AmbrosJourney.Journey.JourneySession.CanUsePortals&&!InputBlocked&&AmbrosJourney.Journey.BossPortalSequence.AllowsEntry(caveEntrance)&&Progress.CanEnterCave&&!Progress.InCave&&player&&caveEntrance&&Vector2.Distance(player.transform.position,caveEntrance.position)<1.1f;
        void LateUpdate()
        {
            if(CanEnterCave)World.ChangeRegion(true);
            if(Progress.InCave&&AmbrosJourney.Journey.JourneySession.CanUsePortals&&!InputBlocked&&AmbrosJourney.Journey.BossPortalSequence.AllowsEntry(winterGate)&&Vector2.Distance(player.transform.position,winterGate.position)<1.1f)
                AmbrosJourney.Journey.JourneySession.Instance?.EnterHomePortal();
        }
        public bool GuideNeedsTalk=>!Progress.InCave&&!Progress.Complete&&(!Progress.Introduced||Progress.WinterComplete&&!Progress.CaveEntryExplained);
        void PlaceLittleMage()
        {
            littleMage.follow=Progress.WinterComplete&&Progress.InCave?ember.transform:null;
            if(littleMageLabel)littleMageLabel.enabled=GuideNeedsTalk;
            if(!littleMage.follow)littleMage.transform.position=Progress.WinterComplete?WinterWorld.CaveGuide:WinterWorld.ArrivalGuide;
        }
        int Nearest()
        {
            Transform[] targets={littleMage.transform,stations[0],stations[1],stations[2],guardian.transform,winterGate,caveEntrance,caveExit,winterGuardian.transform};
            if(!Progress.Introduced)return !Progress.InCave&&Vector2.Distance(player.transform.position,littleMage.transform.position)<GuideTalkRadius?0:-1;
            int result=-1;float best=2.8f;for(int i=1;i<targets.Length;i++){if((i==0&&Progress.InCave)||(i==4&&Progress.Complete)||(i==5)||(i==8&&Progress.WinterComplete)||(i==6&&Progress.WinterComplete)||!targets[i].gameObject.activeInHierarchy)continue;float d=Vector2.Distance(player.transform.position,targets[i].position);if(d<best){best=d;result=i;}}if(result<0)foreach(int i in new[]{4,8})if(targets[i].gameObject.activeInHierarchy&&!(i==4&&Progress.Complete)&&!(i==8&&Progress.WinterComplete)&&Vector2.Distance(player.transform.position,targets[i].position)<5.5f){result=i;break;}if(result<0&&GuideNeedsTalk&&Vector2.Distance(player.transform.position,littleMage.transform.position)<GuideTalkRadius)result=0;return result;
        }
        public void Interact(int target)
        {
            if(AmbrosJourney.Journey.JourneySession.Blocked || !Progress.Introduced&&target!=0 || Transitioning || target==8&&Progress.WinterComplete || target==5 || target==0&&!GuideNeedsTalk)return;
            GameAudio.Click();
            if(target==0)
            {
                if(!Progress.Introduced)Welcome();
                else if(Progress.WinterComplete)Show("Little Mage / Echo Cavern","The cave is open. Walk through the entrance when you are ready. Follow the lanterns, find the Echo Archive board, then face the final Keeper. J holds your notes.",()=>{Progress.ReadCaveGuide();Close();AmbrosJourney.Journey.JourneySession.Instance?.CaptureChapter();});
                else Show("Little Mage / Follow the evidence","Read the Ledger and Observatory boards with E, then face the Warden at the cave. Check where an AI claim came from and compare it with reliable evidence.",Close);
            }
            else if(target>=1&&target<=3){if(stations[target-1].gameObject.activeInHierarchy)Mission(target-1);}
            else if(target==4&&Progress.InCave)Boss();
            else if(target==8&&!Progress.InCave)WinterBoss();
            else if(target==5)Show("Winter crest",Progress.Complete?"WINTER COMPLETE / EVIDENCE EXPLORER\nThe Little Mage joins your party. You may explore both maps and review your notes. The final Keeper is defeated. Open Pause to view your AI Learning Certificate.":"Earn the Winter crest, finish the cave lesson and get at least 7/8 right in the Echo Keeper challenge.",Close);
            else if(target==6&&!Progress.InCave&&!Progress.WinterComplete)WinterBoss();
            else if(target==7&&Progress.InCave)Show("Return to Frostveil","Follow the daylight back outside. Your notes, evidence seals and guardian progress are kept.",()=>{Close();World.ChangeRegion(false);});
        }
        void Mission(int mission)
        {
            if(Progress.MissionComplete(mission)){Show(WinterContent.Landmarks[mission]+" / Rune restored",WinterContent.Notes[mission],Close);return;}
            Show(WinterContent.Landmarks[mission]+" / Field note",WinterContent.Notes[mission],()=>{Progress.ReadNote(mission);MissionQuestion(mission);});
        }
        void MissionQuestion(int mission)
        {
            if(Progress.MissionComplete(mission)){effects.PlayOneShot(ward,.6f);SeasonFeedback.Burst(stations[mission].position,new Color(1,.75f,.3f),24);Show("Evidence seal restored / "+Progress.RuneCount+" of 3",WinterContent.Notes[mission]+(Progress.CanFight?"\n\nAll three evidence seals are restored. Follow the arrow to the Echo Keeper.":"\n\nThe journal records this lesson. Follow the guide arrow to another station."),Close);return;}
            var q=WinterContent.Missions[mission][Progress.Steps[mission]];
            Ask(WinterContent.Landmarks[mission]+" / "+(Progress.Steps[mission]+1)+" of "+WinterContent.Missions[mission].Length,q.Prompt,q.Choices,c=>{
                bool right=Progress.AnswerMission(mission,c);Effect(right);
                Show(right?"The evidence holds":"Incorrect / Try again",SeasonOverlay.AnswerFeedback(right,q.Explanation,Progress.Steps[mission],WinterContent.Missions[mission].Length)+(right?"":"\n\nTry again. Your completed choices stay safe."),()=>MissionQuestion(mission));
            });
        }
        void WinterBoss()
        {
            BossBattleMusic.Play(this,music);
            if(Progress.WinterComplete){Show("Frostbound Warden / Restored","The Winter crest is yours. Enter the cave behind me, find the Echo Archive, then face the final Keeper.",Close);return;}
            if(!Progress.CanFightWinter){Show("The Warden guards the entrance","Finish the Wayfarer's Ledger and Frozen Observatory first. Their two evidence seals unlock this challenge.",Close);return;}
            Show("Winter boss / Frostbound Warden","Five claims guard this cave. Score at least 4/5 to earn Winter's crest and enter. Your two outdoor lessons stay complete when you retry.",()=>{Progress.BeginWinterBoss();WinterBossQuestion();});
        }
        void WinterBossQuestion()
        {
            if(Progress.WinterBossFinished)
            {
                if(Progress.WinterComplete){PlaceLittleMage();Show("Winter crest earned / "+Progress.WinterBossScore+" of 5","The Winter crest is yours! Keep checking sources and comparing evidence. The Little Mage has one last tip before you enter the cave.",()=>{Close();AmbrosJourney.Journey.JourneySession.Instance?.CaptureChapter();});}
                else Show("Winter score / "+Progress.WinterBossScore+" of 5","Score 4/5 to pass. Review your field notes or try again; your lessons are safe.",()=>{Progress.BeginWinterBoss();WinterBossQuestion();});
                return;
            }
            var q=WinterContent.Boss[Progress.WinterBossQuestion];
            Ask("Winter Warden / "+(Progress.WinterBossQuestion+1)+" of 5",q.Prompt,q.Choices,c=>{bool right=Progress.AnswerWinterBoss(c);Effect(right);if(right){player.Strike();winterGuardian.Flash(new Color(1,.8f,.3f));}else winterGuardian.Strike();Show(right?"Evidence accepted":"Incorrect / Answer recorded",SeasonOverlay.AnswerFeedback(right,q.Explanation,Progress.WinterBossScore,WinterContent.Boss.Length),WinterBossQuestion);});
        }
        void Boss()
        {
            BossBattleMusic.Play(this,music);
            if(Progress.Complete){Close();BeginCertificateReward(false);return;}
            if(!Progress.CanFight){Show("The final seal is closed","Earn the Winter crest outside, then complete the Echo Archive. All three evidence seals are needed.",Close);return;}
            Show("Final boss / The Echo Keeper","Eight activities put your AI safety skills to work in new situations. Pick details, build a request, match jobs and check evidence. Get at least 7/8 right.\n\nYou can revisit any unlocked season from Pause to find missed artifacts and review notes. Your lessons stay safe on a retry.",()=>{Progress.BeginBoss();BossQuestion();});
        }
        void BossQuestion()
        {
            if(Progress.BossFinished)
            {
                if(Progress.Complete){effects.PlayOneShot(ward,.7f);Close();BeginCertificateReward(true);}
                else Show("Final score / "+Progress.BossScore+" of 8","You need 7/8 to pass. Your crests and collected artifacts are safe. Continue to retry, or close this conversation to review your notes and explore.",()=>{Progress.BeginBoss();BossQuestion();});
                return;
            }
            var q=WinterContent.Final[Progress.BossQuestion];
            Show("Echo Keeper: Activity "+(Progress.BossQuestion+1)+" of 8",q.Prompt,null);
            if(!finalPanel)finalPanel=gameObject.AddComponent<FinalActivityPanel>();
            finalPanel.Present(presentation,q,response=>{
                bool right=Progress.AnswerFinal(response);Effect(right);
                if(Progress.Complete)AmbrosJourney.Journey.JourneySession.Instance?.RecordFinalVictory();
                if(right){player.Strike();guardian.Flash(new Color(1,.8f,.3f));}else guardian.Strike();
                effects.PlayOneShot(strike,.35f);
                Show(right?"That works":"Incorrect / Answer recorded",SeasonOverlay.AnswerFeedback(right,q.Explanation,Progress.BossScore,WinterContent.Final.Length),BossQuestion);
            });
        }
        void BeginCertificateReward(bool animate)
        {
            if(!CertificateReward)CertificateReward=gameObject.AddComponent<WinterCertificateReward>();
            CertificateReward.Begin(this,animate);
        }
        void ShowPages(string[] pages,int index,Action complete)
        {Show("Winter / Little Mage's trail",pages[index],()=>{if(index+1<pages.Length)ShowPages(pages,index+1,complete);else{complete();Close();}});}
        void Effect(bool right)
        {
            effects.PlayOneShot(right?good:bad,.45f);presentation.Feedback(right);
            player.Flash(right?new Color(.65f,1,.65f):new Color(1,.5f,.4f));
            if(right)SeasonFeedback.Burst(player.transform.position,new Color(1,.8f,.3f));
        }
        void Close(){dialogVersion++;BossBattleMusic.Stop(this);presentation.SetFrostNotice(false);if(finalPanel)finalPanel.Hide();dialog.SetActive(false);actions.Clear();next=null;if(music)music.pitch=1;}
        void Show(string title,string body,Action onContinue)
        {
            presentation.SetFrostNotice(false);
            if(finalPanel)finalPanel.Hide();
            dialogVersion++;currentTitle=title;heading.text=title;copy.text=body;dialog.SetActive(true);journal.SetActive(false);next=onContinue==null?null:()=>{GameAudio.Click();onContinue();};actions.Clear();
            foreach(var c in choices)c.gameObject.SetActive(false);
            choices[0].gameObject.SetActive(true);SetButton(choices[0],"Continue [ E ]",()=>{if(next!=null){var n=next;next=null;n();}});
            presentation.Present(false);
        }
        void Ask(string title,string body,string[] options,Action<int> answer)
        {
            Show(title,body,null);int version=dialogVersion;for(int i=0;i<choices.Count;i++){choices[i].gameObject.SetActive(i<options.Length);if(i>=options.Length)continue;int index=i;Action choose=()=>{if(version!=dialogVersion||!dialog.activeSelf||AmbrosJourney.Journey.JourneySession.Blocked)return;dialogVersion++;GameAudio.Click();answer(index);};actions.Add(choose);SetButton(choices[i],(i+1)+".  "+options[i],choose);}
            presentation.Present(true);
        }
        void SetButton(UnityEngine.UI.Button button,string label,Action action)
        {button.GetComponentInChildren<TextMeshProUGUI>().text=label;button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>{GameAudio.Click();action();});}
        void ToggleJournal()
        {
            GameAudio.Click();journal.SetActive(!journal.activeSelf);if(!journal.activeSelf)return;
            journalCopy.text="";
            for(int i=0;i<3;i++)journalCopy.text+=(Progress.Studied[i]?WinterContent.Notes[i]:WinterContent.Landmarks[i]+" / Find and read this station's note.")+"\nProgress: "+Progress.Steps[i]+" / "+WinterContent.Missions[i].Length+"\n\n";
            journalCopy.text+=party.text;
            var titles=new string[3];var notes=new string[3];
            for(int i=0;i<3;i++){titles[i]=WinterContent.Landmarks[i]+"  /  "+Progress.Steps[i]+" of "+WinterContent.Missions[i].Length+" challenges";notes[i]=Progress.Studied[i]?WinterContent.Notes[i].Split('\n')[1]:"Find the field note at this landmark to collect its lesson.";}
            presentation.SetJournal(titles,notes,party.text);
        }
        public Vector2 Destination()
        {
            if(!Progress.Introduced||Progress.WinterComplete&&!Progress.InCave&&!Progress.CaveEntryExplained)return littleMage.transform.position;
            if(!Progress.InCave){for(int i=0;i<2;i++)if(!Progress.MissionComplete(i))return stations[i].position;return Progress.WinterComplete?caveEntrance.position:winterGuardian.transform.position;}
            for(int i=0;i<2;i++)if(!Progress.MissionComplete(i))return caveExit.position;
            if(!Progress.MissionComplete(2))return stations[2].position;
            if(Progress.Complete&&CertificateReward&&CertificateReward.Ready)return CertificateReward.Position;
            return Progress.Complete?(Vector2)winterGate.position:guardian.transform.position;
        }
        void RefreshWorld()
        {
            if(lessonLabels!=null)foreach(var label in lessonLabels)if(label)label.enabled=Progress.Introduced;
            arenaSeal.SetActive(Progress.RuneCount<3);exitSeal.SetActive(!Progress.Complete);
            for(int i=0;i<3;i++)runes[i].color=Progress.MissionComplete(i)?new Color(.6f,1,.85f):new Color(.6f,.75f,1);
            for(int i=0;i<shields.Length;i++)shields[i].enabled=false;
            string quest=Progress.Complete&&Transitioning?"The Echo Keeper is crumbling...":Progress.Complete?(CertificateReward&&CertificateReward.Ready?"Collect the certificate dropped by the Echo Keeper":(AmbrosJourney.Journey.JourneySession.Instance.Data.homePortalOpened?"Walk through the portal to go home":"View your certificate, then choose Exit to open the way home")):!Progress.InCave?(Progress.MissionComplete(0)?Progress.MissionComplete(1)?(Progress.WinterComplete?"Walk into the cave opening":"North ridge / Challenge the Winter Warden"):"Frozen Observatory / Compare evidence":"Wayfarer's Ledger / Check the source"):(Progress.RuneCount==3?"Echo Keeper / Final challenge: 7 of 8":!Progress.MissionComplete(0)||!Progress.MissionComplete(1)?"Return outside for the two field lessons":"Explore the passages to the Echo Archive");
            if(!Progress.Introduced)quest="Press E to talk to the Little Mage by the portal.";
            else if(Progress.WinterComplete&&!Progress.InCave&&!Progress.CaveEntryExplained)quest="Press E to talk to the Little Mage beside the cave.";
            objective.text=(Progress.InCave?"ECHO CAVERN":"FROSTVEIL")+"  |  SEALS "+Progress.RuneCount+" / 3\n"+quest;
            var destination=Destination();if(!InputBlocked)presentation.SetCompass(player.transform.position,destination,destination,false);
            prompt.text=CertificateReward&&CertificateReward.CanCollect&&!InputBlocked?"[ E ]  Pick up your certificate":InputBlocked||nearby<0?"":"[ E ]  "+new[]{"Talk to the Little Mage","Wayfarer's Ledger","Frozen Observatory","Echo Archive","Challenge the Echo Keeper","Winter crest","Enter Echo Cavern","Return outside","Challenge the Winter Warden"}[nearby];
            if(!InputBlocked&&!Progress.InCave&&Progress.CanEnterCave&&nearby<0&&Vector2.Distance(player.transform.position,caveEntrance.position)<3)prompt.text="Walk into the cave opening";
            party.text=Progress.WinterComplete?"PARTY / Ambros + Ancient Mage + Warriors + Emberfist + Little Mage":"PARTY / Ambros + Ancient Mage + Warriors + Emberfist";
        }
        public void RegionChanged(){PlaceLittleMage();}
        void CreateUI()
        {
            var targets=new[]{stations[0],stations[1],stations[2],winterGuardian.transform,guardian.transform};lessonLabels=new SeasonWorldLabel[targets.Length];
            for(int i=0;i<targets.Length;i++)lessonLabels[i]=targets[i].GetComponentInChildren<SeasonWorldLabel>(true);
            littleMageLabel=littleMage.GetComponentInChildren<SeasonWorldLabel>(true);
            var ui=new GameObject("Winter interface");ui.transform.SetParent(transform,false);
            presentation=ui.AddComponent<SeasonOverlay>();presentation.Initialize(font,4);presentation.CaveLighting=World.GetComponent<CaveVisibility>();
            canvas=presentation.Canvas;dialog=presentation.Dialog;journal=presentation.Journal;
            heading=presentation.Heading;copy=presentation.Copy;objective=presentation.Objective;compass=presentation.CompassText;prompt=presentation.Prompt;journalCopy=presentation.JournalCopy;party=presentation.Party;
            choices.AddRange(presentation.Choices);
        }
        void CreateAudio()
        {
            effects = gameObject.AddComponent<AudioSource>(); effects.playOnAwake = false;
            music = gameObject.AddComponent<AudioSource>(); music.playOnAwake = false; music.loop = true; music.volume = .22f;
            good=GameAudio.Clip(SoundCue.Correct);bad=GameAudio.Clip(SoundCue.Incorrect);ward=GameAudio.Clip(SoundCue.Complete);strike=GameAudio.Clip(SoundCue.Impact);
            effects.volume=.6f;GameAudio.Register(effects);GameAudio.Register(music);
            music.clip = Resources.Load<AudioClip>("SeasonMusic/zephiramusic-lofi-winter-610024");
            if (music.clip) music.Play();
            else Debug.LogError("Missing Winter background music.");
        }

    }
}
