using AmbrosJourney.Audio;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using AmbrosJourney.Presentation;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace AmbrosJourney.Summer
{
    public sealed class SummerLevel : MonoBehaviour
    {
        public static readonly Vector2 Spawn = new Vector2(0, 22);
        public SummerActor player, mage, maleWarrior, femaleWarrior, orc;
        public Transform workshop, garden, messagePost, fallGate;
        public Transform[] stones;
        public GameObject arenaSeal, exitSeal;
        public SpriteRenderer[] wardLights, shields;
        public TMP_FontAsset font;
        public SummerProgress Progress = new SummerProgress();
        public bool Transitioning;
        public bool InputBlocked => AmbrosJourney.Journey.JourneySession.Blocked ||  Transitioning || !dialog || !journal || dialog.activeSelf || journal.activeSelf;
        GameObject dialog, journal;
        TextMeshProUGUI heading, copy, objective, compass, prompt, journalCopy, party;
        readonly List<UnityEngine.UI.Button> choices = new List<UnityEngine.UI.Button>();
        readonly List<Action> actions = new List<Action>();
        Action next;
        int dialogVersion;
        AudioSource effects, music;
        AudioClip good, bad, ward, strike;
        Canvas canvas;
        SeasonOverlay presentation;
        bool readingGardenWarrior;
        SeasonWorldLabel gardenWarriorLabel;
        bool GardenWarriorHasMore=>!Progress.GardenWarriorRead&&!Progress.GuideArrived&&!Progress.Wards[1];
        int nearby = -1;
        string currentTitle;
        public bool IsDialogueOpen => dialog && dialog.activeSelf;
        public string CurrentTitle => currentTitle;
        public void RestoreProgress(SummerProgress p)
        {
            if(p==null||!p.Valid())throw new ArgumentException("Invalid Summer checkpoint");
            readingGardenWarrior=false;CompanionSpeech.Clear(presentation);Progress=p;Close();journal.SetActive(false);if(p.Introduced)RestoreGuides();else{BeginGuidance();Welcome();}if(p.Complete){maleWarrior.follow=mage.transform;femaleWarrior.follow=maleWarrior.transform;}RefreshWorld();AmbrosJourney.Journey.BossPortalSequence.RestoreCheckpoint();
        }
        void Start()
        {
            foreach(var flag in shields)if(flag)flag.gameObject.SetActive(false);
            fallGate.gameObject.SetActive(false);
            Application.runInBackground = false;
            CreateUI(); CreateAudio();
            mage.follow = player.transform;
            Close();BeginGuidance();Welcome();
            Debug.Log("SUMMER_READY: three missions, five-question Suncrest Guardian, journal, compass and companions.");
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
            if (!canvas) return;
            var k = Keyboard.current;
            nearby = Nearest();
            if (k != null && !Transitioning)
            {
                if (k.mKey.wasPressedThisFrame) { GameAudio.ToggleMute(); }
                if (k.escapeKey.wasPressedThisFrame)
                {
                    if (journal.activeSelf) journal.SetActive(false);
                    else if (dialog.activeSelf && Progress.Introduced) Close();
                }
                else if (k.jKey.wasPressedThisFrame && !dialog.activeSelf) ToggleJournal();
                else if (dialog.activeSelf)
                {
                    if (actions.Count > 0)
                    {
                        int selected = k.digit1Key.wasPressedThisFrame ? 0 : k.digit2Key.wasPressedThisFrame ? 1 : k.digit3Key.wasPressedThisFrame ? 2 : -1;
                        if (selected >= 0 && selected < actions.Count) actions[selected]();
                    }
                    else if ((k.eKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame) && next != null) { var n = next; next = null; n(); }
                }
                else if (!journal.activeSelf && k.eKey.wasPressedThisFrame && nearby >= 0) Interact(nearby);
            }
            UpdateGuidance();
            RefreshWorld();
        }
        void Welcome()=>CompanionSpeech.Say(presentation,"The Warriors","Welcome to Summer! AI can help without making every decision for us. Come to the workshop; we'll walk with you. Read the lesson boards with E.");
        public static readonly Vector2 WorkshopGuide=new Vector2(-16,11), WorkshopPartner=new Vector2(-17,10);
        void BeginGuidance()
        {
            Progress.Introduced=true;
            maleWarrior.Guidance=new GuideWalk(WorkshopGuide,new Vector2(-30,-32),new Vector2(30,28));
            femaleWarrior.Guidance=new GuideWalk(WorkshopPartner,new Vector2(-30,-32),new Vector2(30,28));
        }
        void RestoreGuides()
        {
            if(Progress.GuideArrived||Progress.WorkshopQuestion>0||Progress.HintChosen||Progress.WardCount>0)
            {
                Progress.GuideArrived=true;maleWarrior.Guidance=null;femaleWarrior.Guidance=null;
                maleWarrior.transform.position=WorkshopGuide;femaleWarrior.transform.position=WorkshopPartner;
                femaleWarrior.follow=mage.transform;
            }
            else BeginGuidance();
        }
        void UpdateGuidance()
        {
            if(InputBlocked||!Progress.Introduced||maleWarrior.Guidance==null||femaleWarrior.Guidance==null)return;
            if(!maleWarrior.Guidance.Arrived||!femaleWarrior.Guidance.Arrived||!Progress.GuideArrived&&Vector2.Distance(player.transform.position,workshop.position)>4)return;
            bool announce=!Progress.GuideArrived;
            Progress.GuideArrived=true;maleWarrior.Guidance=null;femaleWarrior.Guidance=null;femaleWarrior.follow=mage.transform;
            if(announce)CompanionSpeech.Say(presentation,"The Warriors","Here is the workshop. Press E at its board when you are ready. Finish the workshop, garden and message post to open the Guardian's clearing. J holds your notes.");
        }
        int Nearest()
        {
            Transform[] targets = { maleWarrior.transform, workshop, garden, messagePost, orc.transform, fallGate, null, null, null, femaleWarrior.transform };
            float best = 2.8f; int result = -1;
            for (int i = 1; i < targets.Length-1; i++)
            {
                if(!targets[i]||!targets[i].gameObject.activeInHierarchy || i==4&&Progress.Complete)continue;
                float d = Vector2.Distance(player.transform.position, targets[i].position);
                if (d < best) { best = d; result = i; }
            }
            // Nearby lesson objects take priority over a companion standing beside Ambros.
            if(result<0&&!Progress.Complete&&Vector2.Distance(player.transform.position,orc.transform.position)<5.5f)result=4;
            return result;
        }
        public void Interact(int target)
        {
            if (AmbrosJourney.Journey.JourneySession.Blocked || target==4&&Progress.Complete || target==5&&!AmbrosJourney.Journey.BossPortalSequence.AllowsEntry(fallGate) || !Progress.Introduced||target==9&&!GardenWarriorHasMore) return;
            GameAudio.Click();
            switch (target)
            {
                case 0: case 9:
                    CompanionSpeech.Say(presentation,"The Warriors",Progress.Complete?"We will travel with you. AI can support your ideas, but your choices are still yours.":"Read the workshop, garden and message-post boards with E. Finish all three lessons to reach the Suncrest Guardian.");break;
                case 1: Workshop(); break;
                case 2: Garden(); break;
                case 3: Message(); break;
                case 4: Boss(); break;
                case 5:
                    Show("The Fall gate", Progress.Complete ? "SUMMER COMPLETE\nRank earned: Independent Thinker\nThe married Warriors join your party.\n\nEmberfist's trail marker: Good help begins with a clear request.\n\nEmberfist awaits in Emberwood. Close this conversation and walk through the unlocked portal to begin Fall." : "The Fall gate is sealed. Restore all three wards and score at least 4/5 against the Suncrest Guardian.", Close); break;
                default: break;
            }
        }
        void Workshop()
        {
            Progress.GuideArrived=true;
            if(maleWarrior.Guidance!=null)maleWarrior.Guidance.WaitForPlayer=false;
            if(femaleWarrior.Guidance!=null)femaleWarrior.Guidance.WaitForPlayer=false;
            if (Progress.Wards[0]) { Show("Workshop restored", SummerContent.Notes[0], Close); return; }
            Show("Village workshop / Choose the right tool", "Warrior: We've got two jobs to sort out. Think about what a tool can help with and what needs someone here in the village.", WorkshopQuestion);
        }
        void WorkshopQuestion()
        {
            if (Progress.Wards[0]) { Reward(0); return; }
            var q = SummerContent.Workshop[Progress.WorkshopQuestion];
            Ask("Workshop / " + (Progress.WorkshopQuestion + 1) + " of 2", q.Prompt, q.Choices, c => {
                bool right = Progress.AnswerWorkshop(c); Effect(right);
                Show(right ? "Good choice" : "Incorrect / Try again", SeasonOverlay.AnswerFeedback(right,q.Explanation,Progress.WorkshopQuestion,SummerContent.Workshop.Length) + (right ? "" : "\nYour completed work is safe. Try again."), WorkshopQuestion);
            });
        }
        void Garden()=>GardenQuestion();
        void GardenQuestion()
        {
            if (Progress.Wards[1]) { Show("Keep your practice", SummerContent.Notes[1], Close); return; }
            Ask("Garden field note / Put learning first", "You wrote your own paragraph. Your teacher allows AI feedback but not AI writing. Which request helps you practice?", new[] { "Point out one unclear part so I can revise it", "Rewrite the draft so I can turn in the improved version" }, c => {
                bool right = Progress.AnswerGarden(c); Effect(right);
                Show(right ? "Your practice matters" : "Incorrect / Keep the thinking", SeasonOverlay.AnswerFeedback(right,"Use feedback to improve your own work. If the tool rewrites it all, you miss the practice. Follow your teacher's rules about help."), right ? (Action)(() => Reward(1)) : GardenQuestion);
            });
        }
        void Message()
        {
            if (Progress.Wards[2]) { Show("Message post restored", SummerContent.Notes[2], Close); return; }
            if (!Progress.PrivacyComplete)
            {
                var q=SummerContent.Message[0];
                Ask("Message post / Keep private details out",q.Prompt,q.Choices,c=>{
                    bool right=Progress.AnswerMessage(c);Effect(right);
                    Show(right?"A safer AI request":"Incorrect / Keep private details out",SeasonOverlay.AnswerFeedback(right,q.Explanation)+(right?"":"\n\nTry this question again."),Message);
                });
            }
            else
            {
                var q=SummerContent.Message[1];
                Ask("Message post / Choose human help",q.Prompt,q.Choices,c=>{
                    bool right=Progress.AnswerMessage(c);Effect(right);
                    Show(right?"People provide real support":"Incorrect / Ask someone you trust",SeasonOverlay.AnswerFeedback(right,q.Explanation)+(right?"":"\n\nTry this question again."),right?(Action)(()=>Reward(2)):Message);
                });
            }
        }

        void Reward(int mission)
        {
            effects.PlayOneShot(ward, .6f);
            Show("Ward piece restored / " + Progress.WardCount + " of 3", SummerContent.Notes[mission] + (Progress.CanFight ? "\n\nThe village ward opens the Suncrest Guardian's clearing. Follow the guide arrow south." : "\n\nYour journal now holds this lesson. Follow the guide arrow to the next ward."), Close);
        }
        void Boss()
        {
            BossBattleMusic.Play(this,music);
            if (Progress.Complete) { Show("The Suncrest Guardian is learning too", "Suncrest Guardian: What if I get it wrong?\nAmbros: Then you practice. You can ask for help without giving up.", Close); return; }
            if (!Progress.CanFight) { Show("The Suncrest Guardian's clearing is sealed", "Restore all three ward pieces first. The journal records your lessons, and the arrow points toward your next mission.", Close); return; }
            Show("The Suncrest Guardian / Five questions", "Suncrest Guardian: I've been using these tools for everything. How do you decide when to step in?\n\nWarrior: Let's see what you've learned. Get four of five questions right to open Fall. You can retry without redoing the village jobs.", () => { Progress.BeginBoss(); BossQuestion(); });
        }
        void BossQuestion()
        {
            if (Progress.BossFinished)
            {
                if (Progress.Complete)
                {
                    maleWarrior.Guidance=null;femaleWarrior.Guidance=null;maleWarrior.follow = mage.transform; femaleWarrior.follow = maleWarrior.transform;
                    effects.PlayOneShot(ward, .7f);
                    Show("Summer cleared / " + Progress.BossScore + " of 5", "Suncrest Guardian: What if I get it wrong?\nAmbros: Then you practice. You can ask for help without giving up.\n\nThe Warriors restore the village ward and join your party.\nRank earned: INDEPENDENT THINKER\n\nAI can support your ideas and learning. You are responsible for your choices.\n\nClose this message to see the Guardian fade and the Fall portal form.", Close);
                }
                else Show("Your score / " + Progress.BossScore + " of 5", "You need 4/5 (80%) to pass. Review the explanations or open your journal. All three missions remain complete.\n\nContinue to try again, or press Escape to return to the village.", () => { Progress.BeginBoss(); BossQuestion(); });
                return;
            }
            var q = SummerContent.Boss[Progress.BossQuestion];
            Ask("Suncrest Guardian / Question " + (Progress.BossQuestion + 1) + " of 5", q.Prompt, q.Choices, c => {
                bool right = Progress.AnswerBoss(c); Effect(right);
                if (right) player.Strike(); else orc.Strike();
                effects.PlayOneShot(strike, .35f);
                Show(right ? "Good answer!" : "Incorrect / The Guardian attacks", SeasonOverlay.AnswerFeedback(right,q.Explanation,Progress.BossScore,SummerContent.Boss.Length), BossQuestion);
            });
        }
        void ShowPages(string[] pages, int index, Action complete)
        { Show("Summer / The village ward", pages[index], () => { if (index + 1 < pages.Length) ShowPages(pages, index + 1, complete); else { complete(); Close(); } }); }
        void Effect(bool right) { effects.PlayOneShot(right ? good : bad, .45f); presentation.Feedback(right); }
        void Close() { dialogVersion++;BossBattleMusic.Stop(this); if(readingGardenWarrior){Progress.GardenWarriorRead=true;readingGardenWarrior=false;}dialog.SetActive(false); actions.Clear(); next = null; if (music) music.pitch = 1; }
        void Show(string title, string body, Action onContinue)
        {
            dialogVersion++;currentTitle = title; heading.text = title; copy.text = body; dialog.SetActive(true); journal.SetActive(false);
            next = onContinue==null?null:()=>{GameAudio.Click();onContinue();}; actions.Clear();
            for (int i = 0; i < choices.Count; i++) choices[i].gameObject.SetActive(false);
            var button = choices[0]; button.gameObject.SetActive(true); SetButton(button, "Continue [ E ]", () => { if (next != null) { var n = next; next = null; n(); } });
            presentation.Present(false);
        }
        void Ask(string title, string body, string[] options, Action<int> answer)
        {
            Show(title, body, null); actions.Clear();int version=dialogVersion;
            for (int i = 0; i < choices.Count; i++)
            {
                choices[i].gameObject.SetActive(i < options.Length);
                if (i >= options.Length) continue;
                int index = i; Action choose = () => {if(version!=dialogVersion||!dialog.activeSelf||AmbrosJourney.Journey.JourneySession.Blocked)return;dialogVersion++;GameAudio.Click();answer(index);}; actions.Add(choose);
                SetButton(choices[i], (i + 1) + ".  " + options[i], choose);
            }
            presentation.Present(true);
        }
        void SetButton(UnityEngine.UI.Button button, string label, Action action)
        { button.GetComponentInChildren<TextMeshProUGUI>().text = label; button.onClick.RemoveAllListeners(); button.onClick.AddListener(() => {GameAudio.Click();action();}); }
        void ToggleJournal()
        {
            GameAudio.Click();journal.SetActive(!journal.activeSelf); if (!journal.activeSelf) return;
            journalCopy.text = "";
            for (int i = 0; i < 3; i++) journalCopy.text += (Progress.Wards[i] ? SummerContent.Notes[i] : new[] { "WORKSHOP / Restore this ward to collect its lesson.", "TRAINING GARDEN / Restore this ward to collect its lesson.", "MESSAGE POST / Restore this ward to collect its lesson." }[i]) + "\n\n";
            journalCopy.text += party.text;
            var titles=new string[3];var notes=new string[3];var names=new[]{"Village Workshop","Training Garden","Message Post"};
            for(int i=0;i<3;i++){titles[i]=names[i]+"  /  "+(Progress.Wards[i]?"RESTORED":"NOT YET");notes[i]=SummerContent.Notes[i].Split('\n')[1];}
            presentation.SetJournal(titles,notes,party.text);
        }
        Vector2 Destination()
        {
            if (!Progress.Introduced) return maleWarrior.transform.position;
            if (!Progress.Wards[0]) return workshop.position;
            if (!Progress.Wards[1]) return garden.position;
            if (!Progress.Wards[2]) return messagePost.position;
            return Progress.Complete ? (Vector2)fallGate.position : orc.transform.position;
        }
        void RefreshWorld()
        {
            if(gardenWarriorLabel)gardenWarriorLabel.enabled=!Progress.Complete;
            arenaSeal.SetActive(!Progress.CanFight); exitSeal.SetActive(!Progress.Complete);
            for (int i = 0; i < 3; i++) wardLights[i].color = Progress.Wards[i] ? new Color(.5f, 1, .75f) : new Color(.7f, .55f, .30f);
            for (int i = 0; i < shields.Length; i++) shields[i].enabled=false;
            objective.text = "WARD " + Progress.WardCount + " / 3\n" + (Progress.Complete ? "Walk through the Fall gate" : Progress.CanFight ? "Challenge the Suncrest Guardian / 4 of 5 to pass" : !Progress.Wards[0] ? "Workshop / Choose the right tool" : !Progress.Wards[1] ? "Training garden / Keep your practice" : "Message post / Ask safely");
            if(Progress.Introduced&&!Progress.GuideArrived)objective.text="Walk with the Warriors to the workshop. They will wait for you.";
            var destination=Destination();
            if(!InputBlocked)presentation.SetCompass(player.transform.position,destination,destination);
            prompt.text=InputBlocked||nearby<0?"":"[ E ]  "+new[]{"Talk to the Warrior","Visit the workshop","Read the garden field note","Read the message post","Face the Suncrest Guardian","Enter the Fall gate","","","","Talk to the Warrior"}[nearby];
            party.text = Progress.Complete ? "PARTY / Ambros + Mage + both Warriors" : "PARTY / Ambros + Ancient Mage";
        }
        void CreateUI()
        {
            gardenWarriorLabel=femaleWarrior.GetComponentInChildren<SeasonWorldLabel>(true);
            var ui=new GameObject("Summer interface");ui.transform.SetParent(transform,false);
            presentation=ui.AddComponent<SeasonOverlay>();presentation.Initialize(font,false);
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
            music.clip = Resources.Load<AudioClip>("SeasonMusic/alex-morgan-soft-chill-background-music-548644");
            if (music.clip) music.Play();
            else Debug.LogError("Missing Summer background music.");
        }

    }
}
