using AmbrosJourney.Audio;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using AmbrosJourney.Presentation;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace AmbrosJourney.Fall
{
    public sealed class FallLevel : MonoBehaviour
    {
        public static readonly Vector2 Spawn=new Vector2(-38,34);
        public FallActor player,mage,maleWarrior,femaleWarrior,ember,guardian;
        public Transform[] stations;
        public Transform winterGate;
        public GameObject arenaSeal,exitSeal;
        public SpriteRenderer[] runes,shields;
        public TMP_FontAsset font;
        public FallProgress Progress=new FallProgress();
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
        SeasonNavigation navigation;
        int nearby=-1;
        string currentTitle;
        public void RestoreProgress(FallProgress p)
        {
            if(p==null||!p.Valid())throw new ArgumentException("Invalid Fall checkpoint");
            Progress=p;if(p.Introduced){Close();RestoreGuide();}journal.SetActive(false);if(p.Complete){ember.Guidance=null;ember.follow=femaleWarrior.transform;}RefreshWorld();
        }
        void Start()
        {
            Application.runInBackground=false;CreateUI();CreateAudio();
            mage.follow=player.transform;maleWarrior.follow=mage.transform;femaleWarrior.follow=maleWarrior.transform;
            Show("Fall / Emberfist's trail","Emberfist: Welcome to Emberwood! Our machines keep getting requests mixed up. Help me work out how to give them clearer instructions.",()=>{BeginGuidance();Close();});
            Debug.Log("FALL_READY: Emberwood, three prompt missions, seven applied choices and five-question guardian.");
        }
        public bool DismissPanel()
        {
            if(journal&&journal.activeSelf){journal.SetActive(false);return true;}
            if(dialog&&dialog.activeSelf){if(!Progress.Introduced)BeginGuidance();Close();return true;}
            return false;
        }
        void Update()
        {
            if(AmbrosJourney.Journey.JourneySession.Blocked)return;
            if(!canvas)return;nearby=Nearest();var k=Keyboard.current;
            if(k!=null && !Transitioning)
            {
                if(k.mKey.wasPressedThisFrame){GameAudio.ToggleMute();}
                if(k.escapeKey.wasPressedThisFrame){if(journal.activeSelf)journal.SetActive(false);else if(dialog.activeSelf && Progress.Introduced)Close();}
                else if(k.jKey.wasPressedThisFrame && !dialog.activeSelf)ToggleJournal();
                else if(dialog.activeSelf)
                {
                    if(actions.Count>0){int c=k.digit1Key.wasPressedThisFrame?0:k.digit2Key.wasPressedThisFrame?1:k.digit3Key.wasPressedThisFrame?2:-1;if(c>=0 && c<actions.Count)actions[c]();}
                    else if((k.eKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame) && next!=null){var n=next;next=null;n();}
                }
                else if(!journal.activeSelf && k.eKey.wasPressedThisFrame && nearby>=0)Interact(nearby);
            }
            UpdateGuidance();
            RefreshWorld();
        }
        public static readonly Vector2 OrchardGuide=new Vector2(-27,9);
        void BeginGuidance()
        {
            Progress.Introduced=true;ember.Guidance=new GuideWalk(OrchardGuide,FallWorld.Min,FallWorld.Max);
        }
        void RestoreGuide()
        {
            if(Progress.GuideArrived||Progress.Studied[0]||Progress.RuneCount>0)
            {Progress.GuideArrived=true;ember.Guidance=null;ember.transform.position=OrchardGuide;}
            else BeginGuidance();
        }
        void UpdateGuidance()
        {
            if(InputBlocked||!Progress.Introduced||Progress.GuideArrived||ember.Guidance==null||!ember.Guidance.Arrived||Vector2.Distance(player.transform.position,stations[0].position)>4)return;
            Progress.GuideArrived=true;ember.Guidance=null;
            ShowPages(new[]{
                "A prompt is what you ask an AI tool to do. Say what you need, give useful background and explain how you want the reply. Then read it and ask for changes if needed. You still have to check the facts.",
                "Start at Harvest Orchard, cross the north bridge to Lantern Archive, then find Revision Kiln. Read each note and fix the requests there. Three runes open the Sentinel's clearing. Get four of five questions right to reach Winter."
            },0,()=>{});
        }
        int Nearest()
        {
            Transform[] targets={ember.transform,stations[0],stations[1],stations[2],guardian.transform,winterGate};
            int result=-1;float best=2.8f;for(int i=1;i<targets.Length;i++){float d=Vector2.Distance(player.transform.position,targets[i].position);if(d<best){best=d;result=i;}}if(result<0&&Vector2.Distance(player.transform.position,ember.transform.position)<2.8f)result=0;return result;
        }
        public void Interact(int target)
        {
            if(!Progress.Introduced || Transitioning)return;
            GameAudio.Click();
            if(target==0)Show("Emberfist / The prompt forge",Progress.Complete?"I will travel with you! In Winter, keep checking what a tool tells you. Your own judgment still matters.":"Read the three station notes, then repair their requests. Follow the paths: the north bridge reaches the archive; its east opening leads into the switchbacks. The lower bridge loops back to the orchard.",Close);
            else if(target>=1 && target<=3)Mission(target-1);
            else if(target==4)Boss();
            else if(target==5)Show("The Winter gate",Progress.Complete?"FALL COMPLETE / PROMPT PATHFINDER\nEmberfist joins your party.\n\nClose this conversation and walk through the unlocked gate to reach Frostveil. Find the Little Mage and discover the cave beyond the snowy ridge.":"Collect the three prompt runes and pass the Sentinel's quiz with at least 4/5. The Winter gate will then open.",Close);
        }
        void Mission(int mission)
        {
            if(Progress.MissionComplete(mission)){Show(FallContent.Landmarks[mission]+" / Rune restored",FallContent.Notes[mission],Close);return;}
            Show(FallContent.Landmarks[mission]+" / Field note",FallContent.Notes[mission],()=>{Progress.ReadNote(mission);MissionQuestion(mission);});
        }
        void MissionQuestion(int mission)
        {
            if(Progress.MissionComplete(mission)){effects.PlayOneShot(ward,.6f);SeasonFeedback.Burst(stations[mission].position,new Color(1,.75f,.3f),24);Show("Prompt rune restored / "+Progress.RuneCount+" of 3",FallContent.Notes[mission]+(Progress.CanFight?"\n\nThe southern arena opens. Follow the compass to the Bramble Sentinel.":"\n\nThe journal records this lesson. Follow the compass to another station."),Close);return;}
            var q=FallContent.Missions[mission][Progress.Steps[mission]];
            Ask(FallContent.Landmarks[mission]+" / "+(Progress.Steps[mission]+1)+" of "+FallContent.Missions[mission].Length,q.Prompt,q.Choices,c=>{
                bool right=Progress.AnswerMission(mission,c);Effect(right);
                Show(right?"The request takes shape":"Revise your approach",q.Explanation+(right?"":"\n\nTry again. Your completed choices stay safe."),()=>MissionQuestion(mission));
            });
        }
        void Boss()
        {
            if(Progress.Complete){Show("Bramble Sentinel / Restored", "Sentinel: A clear request gives me direction, not wisdom.\nEmberfist: Exactly. We still inspect the answer.",Close);return;}
            if(!Progress.CanFight){Show("The rune barrier is sealed","Restore the Orchard, Archive and Kiln runes before challenging the Sentinel.",Close);return;}
            Show("Bramble Sentinel / Five tangled instructions","Sentinel: Let's try a few new requests. Can you spot what each one needs?\n\nGet four of five questions right to open Winter. Your runes stay safe if you need another try.",()=>{Progress.BeginBoss();BossQuestion();});
        }
        void BossQuestion()
        {
            if(Progress.BossFinished)
            {
                if(Progress.Complete){ember.Guidance=null;ember.follow=femaleWarrior.transform;effects.PlayOneShot(ward,.7f);Show("Fall cleared / "+Progress.BossScore+" of 5","The Bramble Sentinel's tangled ward falls away. Emberfist joins your party!\n\nRank earned: PROMPT PATHFINDER\nGive a goal, context and format. Set useful limits, check the response and revise. Never confuse clear wording with guaranteed truth.\n\nThe Winter gate is open.",Close);}
                else Show("Your score / "+Progress.BossScore+" of 5","You need 4/5 (80%). All rune missions remain complete. Review the journal or continue to retry the five questions.",()=>{Progress.BeginBoss();BossQuestion();});
                return;
            }
            var q=FallContent.Boss[Progress.BossQuestion];
            Ask("Sentinel / Question "+(Progress.BossQuestion+1)+" of 5",q.Prompt,q.Choices,c=>{
                bool right=Progress.AnswerBoss(c);Effect(right);if(right){player.Strike();guardian.Flash(new Color(1,.8f,.3f));}else guardian.Strike();effects.PlayOneShot(strike,.35f);
                Show(right?"A tangled seal breaks!":"The Sentinel pushes back",q.Explanation+"\n\nScore: "+Progress.BossScore+" / "+Progress.BossQuestion+" answered. Courage: "+Progress.Courage+" / 3.",BossQuestion);
            });
        }
        void ShowPages(string[] pages,int index,Action complete)
        {Show("Fall / Emberfist's trail",pages[index],()=>{if(index+1<pages.Length)ShowPages(pages,index+1,complete);else{complete();Close();}});}
        void Effect(bool right)
        {
            effects.PlayOneShot(right?good:bad,.45f);presentation.Feedback(right);
            player.Flash(right?new Color(.65f,1,.65f):new Color(1,.5f,.4f));
            if(right)SeasonFeedback.Burst(player.transform.position,new Color(1,.8f,.3f));
        }
        void Close(){dialog.SetActive(false);actions.Clear();next=null;if(music)music.pitch=1;}
        void Show(string title,string body,Action onContinue)
        {
            currentTitle=title;heading.text=title;copy.text=body;dialog.SetActive(true);journal.SetActive(false);next=onContinue==null?null:()=>{GameAudio.Click();onContinue();};actions.Clear();
            foreach(var c in choices)c.gameObject.SetActive(false);
            choices[0].gameObject.SetActive(true);SetButton(choices[0],"Continue [ E ]",()=>{if(next!=null){var n=next;next=null;n();}});
            presentation.Present(false);
        }
        void Ask(string title,string body,string[] options,Action<int> answer)
        {
            Show(title,body,null);for(int i=0;i<choices.Count;i++){choices[i].gameObject.SetActive(i<options.Length);if(i>=options.Length)continue;int index=i;Action choose=()=>{GameAudio.Click();answer(index);};actions.Add(choose);SetButton(choices[i],(i+1)+".  "+options[i],choose);}
            presentation.Present(true);
        }
        void SetButton(UnityEngine.UI.Button button,string label,Action action)
        {button.GetComponentInChildren<TextMeshProUGUI>().text=label;button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>{GameAudio.Click();action();});}
        void ToggleJournal()
        {
            GameAudio.Click();journal.SetActive(!journal.activeSelf);if(!journal.activeSelf)return;
            journalCopy.text="";
            for(int i=0;i<3;i++)journalCopy.text+=(Progress.Studied[i]?FallContent.Notes[i]:FallContent.Landmarks[i]+" / Find and read this station's note.")+"\nProgress: "+Progress.Steps[i]+" / "+FallContent.Missions[i].Length+"\n\n";
            journalCopy.text+=party.text;
            var titles=new string[3];var notes=new string[3];
            for(int i=0;i<3;i++){titles[i]=FallContent.Landmarks[i]+"  /  "+Progress.Steps[i]+" of "+FallContent.Missions[i].Length+" challenges";notes[i]=Progress.Studied[i]?FallContent.Notes[i].Split('\n')[1]:"Find the field note at this landmark to collect its lesson.";}
            presentation.SetJournal(titles,notes,party.text);
        }
        public Vector2 Destination()
        {
            if(!Progress.Introduced)return ember.transform.position;
            for(int i=0;i<3;i++)if(!Progress.MissionComplete(i))return stations[i].position;
            return Progress.Complete?(Vector2)winterGate.position:guardian.transform.position;
        }
        void RefreshWorld()
        {
            arenaSeal.SetActive(!Progress.CanFight);exitSeal.SetActive(!Progress.Complete);
            for(int i=0;i<3;i++)runes[i].color=Progress.MissionComplete(i)?new Color(.6f,1,.7f):new Color(1,.65f,.3f);
            for(int i=0;i<shields.Length;i++)shields[i].enabled=!Progress.Complete && i>=Progress.BossScore;
            string quest=Progress.Complete?"Walk through the unlocked Winter gate":Progress.CanFight?"Challenge the Sentinel / 4 of 5 to pass":!Progress.MissionComplete(0)?"Harvest Orchard / Goal + context + format":!Progress.MissionComplete(1)?"Lantern Archive / Audience + limits":"Revision Kiln / Check + improve";
            objective.text="RUNES "+Progress.RuneCount+" / 3   |   COURAGE "+Progress.Courage+" / 3\n"+quest;
            if(Progress.Introduced&&!Progress.GuideArrived)objective.text="Walk with Emberfist to Harvest Orchard. He will wait for you.";
            var destination=Destination();
            if(!InputBlocked)presentation.SetCompass(player.transform.position,navigation.Next(player.transform.position,destination),destination);
            prompt.text=InputBlocked||nearby<0?"":"[ E ]  "+new[]{"Talk to Emberfist","Harvest Orchard","Lantern Archive","Revision Kiln","Face the Sentinel","Winter gate"}[nearby];
            party.text=Progress.Complete?"PARTY / Ambros + Mage + Warriors + Emberfist":"PARTY / Ambros + Mage + both Warriors";
        }
        void CreateUI()
        {
            var ui=new GameObject("Fall interface");ui.transform.SetParent(transform,false);
            presentation=ui.AddComponent<SeasonOverlay>();presentation.Initialize(font,true);
            canvas=presentation.Canvas;dialog=presentation.Dialog;journal=presentation.Journal;
            heading=presentation.Heading;copy=presentation.Copy;objective=presentation.Objective;compass=presentation.CompassText;prompt=presentation.Prompt;journalCopy=presentation.JournalCopy;party=presentation.Party;
            choices.AddRange(presentation.Choices);
            navigation=new SeasonNavigation(FallWorld.Min,FallWorld.Max);
        }
        void CreateAudio()
        {
            effects = gameObject.AddComponent<AudioSource>(); effects.playOnAwake = false;
            music = gameObject.AddComponent<AudioSource>(); music.playOnAwake = false; music.loop = true; music.volume = .22f;
            good=GameAudio.Clip(SoundCue.Correct);bad=GameAudio.Clip(SoundCue.Incorrect);ward=GameAudio.Clip(SoundCue.Complete);strike=GameAudio.Clip(SoundCue.Impact);
            effects.volume=.6f;GameAudio.Register(effects);GameAudio.Register(music);
            music.clip = Resources.Load<AudioClip>("SeasonMusic/atlasaudio-autumn-background-621525");
            if (music.clip) music.Play();
            else Debug.LogError("Missing Fall background music.");
        }

    }
}
