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
        AudioSource effects, music;
        AudioClip good, bad, ward, strike;
        Canvas canvas;
        SeasonOverlay presentation;
        SeasonNavigation navigation;
        int nearby = -1;
        string currentTitle;
        public bool IsDialogueOpen => dialog && dialog.activeSelf;
        public string CurrentTitle => currentTitle;
        public void RestoreProgress(SummerProgress p)
        {
            if(p==null||!p.Valid())throw new ArgumentException("Invalid Summer checkpoint");
            Progress=p;if(p.Introduced)Close();journal.SetActive(false);if(p.Complete){maleWarrior.follow=mage.transform;femaleWarrior.follow=maleWarrior.transform;}RefreshWorld();
        }
        void Start()
        {
            Application.runInBackground = false;
            CreateUI(); CreateAudio();
            mage.follow = player.transform;
            ShowPages(new[] {
                "Warrior: You made it! We've been trying to get the village back on track. A tool can be useful without being right about everything.",
                "Warrior: My spouse and I look after this place. Lately, people have been letting the tools make every decision. Help us work out where they actually help.",
                "Ancient Mage: Try the workshop, garden and message post. Each one has a different problem. Finish all three and the path to the Suncrest Guardian will open."
            }, 0, () => Progress.Introduced = true);
            Debug.Log("SUMMER_READY: three missions, five-question Suncrest Guardian, journal, compass and companions.");
        }
        void Update()
        {
            if(AmbrosJourney.Journey.JourneySession.Blocked)return;
            if (!canvas) return;
            var k = Keyboard.current;
            nearby = Nearest();
            if (k != null && !Transitioning)
            {
                if (k.mKey.wasPressedThisFrame) { music.mute = !music.mute; effects.mute = music.mute; }
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
            RefreshWorld();
        }
        int Nearest()
        {
            Transform[] targets = { maleWarrior.transform, workshop, garden, messagePost, orc.transform, fallGate, stones[0], stones[1], stones[2], femaleWarrior.transform };
            float best = 2.8f; int result = -1;
            for (int i = 0; i < targets.Length; i++)
            {
                float d = Vector2.Distance(player.transform.position, targets[i].position);
                if (d < best) { best = d; result = i; }
            }
            return result;
        }
        public void Interact(int target)
        {
            if (!Progress.Introduced) return;
            switch (target)
            {
                case 0: case 9:
                    Show("The married Warriors", Progress.Complete ? "Our village is safe again. We will travel with you. AI can support your ideas and learning. You are responsible for your choices." : "Restore the workshop, training garden and message post. Our ward will open the way to the Suncrest Guardian. We will guard the village while you work.", Close); break;
                case 1: Workshop(); break;
                case 2: Garden(); break;
                case 3: Message(); break;
                case 4: Boss(); break;
                case 5:
                    Show("The Fall gate", Progress.Complete ? "SUMMER COMPLETE\nRank earned: Independent Thinker\nThe married Warriors join your party.\n\nEmberfist's trail marker: Good help begins with a clear request.\n\nEmberfist awaits in Emberwood. Close this conversation and walk through the unlocked portal to begin Fall." : "The Fall gate is sealed. Restore all three wards and score at least 4/5 against the Suncrest Guardian.", Close); break;
                default: Stone(target - 6); break;
            }
        }
        void Workshop()
        {
            if (Progress.Wards[0]) { Show("Workshop restored", SummerContent.Notes[0], Close); return; }
            Show("Village workshop / Choose the right tool", "Warrior: We've got two jobs to sort out. Think about what a tool can help with and what needs someone here in the village.", WorkshopQuestion);
        }
        void WorkshopQuestion()
        {
            if (Progress.Wards[0]) { Reward(0); return; }
            var q = SummerContent.Workshop[Progress.WorkshopQuestion];
            Ask("Workshop / " + (Progress.WorkshopQuestion + 1) + " of 2", q.Prompt, q.Choices, c => {
                bool right = Progress.AnswerWorkshop(c); Effect(right);
                Show(right ? "Good choice" : "Try a different approach", q.Explanation + (right ? "" : "\nYour completed work is safe. Try again."), WorkshopQuestion);
            });
        }
        void Garden()
        {
            if (Progress.Wards[1]) { Show("Training garden restored", SummerContent.Notes[1], Close); return; }
            if (!Progress.HintChosen)
                Ask("Training garden / Keep your practice", "Warrior: You're stuck on a three-stone pattern. The device can give you a hint or show all the steps. Which help leaves you something to practice?", new[] { "Show me the first step so I can try the rest", "Show me the full pattern so I can follow it" }, c => {
                    bool right = Progress.ChooseHint(c); Effect(right);
                    Show(right ? "A helpful hint" : "Keep something to practice", right ? "Start with SUN. Then follow the garden's clue: sun wakes the SEED; the seed grows into a LEAF.\n\nClose this message. Walk to each stepping stone and press E: Sun, Seed, Leaf. The labels stay visible." : "If the device does every step, you miss the practice. A hint can help you begin while leaving the rest for you to work out.", right ? (Action)Close : Garden);
                });
            else if (!Progress.PatternComplete) Show("Your stepping-stone practice", "Walk to the stones and press E in order: SUN, SEED, LEAF.\nProgress: " + Progress.PatternStep + " / 3.\nA mistake resets only this short pattern. You can retry immediately.", Close);
            else GardenQuestion();
        }
        void Stone(int index)
        {
            if (index < 0 || index > 2) return;
            if (!Progress.HintChosen) { Show("Training stones", "Speak with the Warrior at the training garden first. Choose how you want help with this exercise.", Close); return; }
            if (Progress.PatternComplete) { GardenQuestion(); return; }
            bool right = Progress.StepOnStone(index); Effect(right);
            if (Progress.PatternComplete) Show("You completed the pattern!", "You used a hint and worked out the remaining steps yourself. Now apply that idea to schoolwork.", GardenQuestion);
            else Show(right ? "Step " + Progress.PatternStep + " / 3" : "Try the pattern again", right ? "Good step. Close this message and find the next stone." : "Start with SUN, then SEED, then LEAF. Your other missions are unchanged.", Close);
        }
        void GardenQuestion()
        {
            if (Progress.Wards[1]) { Show("Keep your practice", SummerContent.Notes[1], Close); return; }
            Ask("Training garden / Put learning first", "You have a draft for an assignment. Your teacher allows feedback, but the ideas must be yours. What should you ask for?", new[] { "Point out one unclear part so I can revise it", "Rewrite the draft so I can turn in the improved version" }, c => {
                bool right = Progress.AnswerGarden(c); Effect(right);
                Show(right ? "Your practice matters" : "Keep the thinking", "Use feedback to improve your own work. If the tool rewrites it all, you miss the practice. Follow your teacher's rules about help.", right ? (Action)(() => Reward(1)) : GardenQuestion);
            });
        }
        void Message()
        {
            if (Progress.Wards[2]) { Show("Message post restored", SummerContent.Notes[2], Close); return; }
            if (!Progress.PrivacyComplete)
            {
                var labels = new string[3]; string[] details = { "Ambros", "123 Mystical View Rd", "IWantToGoHome987" };
                for (int i = 0; i < 3; i++) labels[i] = (Progress.RemovedPrivateDetails[i] ? "Removed: " : "Keep out: ") + details[i];
                Ask("Message post / Remove private details", "Read Ambros's made-up message. Pick the details you'd leave out before sending it.\n\nI am Ambros, I live at 123 Mystical View Rd, and my password is IWantToGoHome987. Help me plan a birthday treasure hunt.", labels, c => {
                    Progress.AnswerMessage(c); effects.PlayOneShot(good, .25f);
                    if (Progress.PrivacyComplete) Show("A safer request", "Help me brainstorm a birthday treasure hunt with five clues for a small garden.\n\nThis gives enough context without identifying anyone. Next, consider when a real person can help.", Message);
                    else Message();
                });
            }
            else Ask("Message post / People matter", "Your friend has stopped replying in the group chat. You aren't sure why. What should you do?", new[] { "Paste the chat into AI and ask what my friend is thinking", "Check in with my friend and ask an adult for help if needed" }, c => {
                bool right = Progress.AnswerMessage(c); Effect(right);
                Show(right ? "Ask safely" : "AI cannot know your friend's feelings", "A chat tool doesn't know what your friend is feeling. Talk to them kindly. If you're worried about them, get a trusted adult's help.", right ? (Action)(() => Reward(2)) : Message);
            });
        }
        void Reward(int mission)
        {
            effects.PlayOneShot(ward, .6f);
            Show("Ward piece restored / " + Progress.WardCount + " of 3", SummerContent.Notes[mission] + (Progress.CanFight ? "\n\nThe village ward opens the Suncrest Guardian's clearing. Follow the compass south." : "\n\nYour journal now holds this lesson. Follow the compass to the next ward."), Close);
        }
        void Boss()
        {
            if (Progress.Complete) { Show("The Suncrest Guardian is learning too", "Suncrest Guardian: What if I get it wrong?\nAmbros: Then you practice. You can ask for help without giving up.", Close); return; }
            if (!Progress.CanFight) { Show("The Suncrest Guardian's clearing is sealed", "Restore all three ward pieces first. The journal records your lessons, and the compass points toward your next mission.", Close); return; }
            Show("The Suncrest Guardian / Five shields", "Suncrest Guardian: I've been using these tools for everything. How do you decide when to step in?\n\nWarrior: Let's see what you've learned. Get four of five questions right to open Fall. You can retry without redoing the village jobs.", () => { Progress.BeginBoss(); BossQuestion(); });
        }
        void BossQuestion()
        {
            music.pitch = .78f;
            if (Progress.BossFinished)
            {
                if (Progress.Complete)
                {
                    maleWarrior.follow = mage.transform; femaleWarrior.follow = maleWarrior.transform;
                    effects.PlayOneShot(ward, .7f);
                    Show("Summer cleared / " + Progress.BossScore + " of 5", "Suncrest Guardian: What if I get it wrong?\nAmbros: Then you practice. You can ask for help without giving up.\n\nThe Warriors restore the village ward and join your party.\nRank earned: INDEPENDENT THINKER\n\nAI can support your ideas and learning. You are responsible for your choices.\n\nThe Fall gate is open.", Close);
                }
                else Show("Your score / " + Progress.BossScore + " of 5", "You need 4/5 (80%) to pass. Review the explanations or open your journal. All three missions remain complete.\n\nContinue to try again, or press Escape to return to the village.", () => { Progress.BeginBoss(); BossQuestion(); });
                return;
            }
            var q = SummerContent.Boss[Progress.BossQuestion];
            Ask("Suncrest Guardian / Question " + (Progress.BossQuestion + 1) + " of 5", q.Prompt, q.Choices, c => {
                bool right = Progress.AnswerBoss(c); Effect(right);
                if (right) player.Strike(); else orc.Strike();
                effects.PlayOneShot(strike, .35f);
                Show(right ? "A shield cracks!" : "The Suncrest Guardian attacks", q.Explanation + "\n\nScore: " + Progress.BossScore + " / " + Progress.BossQuestion + " answered. Courage: " + Progress.Courage + " / 3.", BossQuestion);
            });
        }
        void ShowPages(string[] pages, int index, Action complete)
        { Show("Summer / The village ward", pages[index], () => { if (index + 1 < pages.Length) ShowPages(pages, index + 1, complete); else { complete(); Close(); } }); }
        void Effect(bool right) { effects.PlayOneShot(right ? good : bad, .45f); presentation.Feedback(right); }
        void Close() { dialog.SetActive(false); actions.Clear(); next = null; if (music) music.pitch = 1; }
        void Show(string title, string body, Action onContinue)
        {
            currentTitle = title; heading.text = title; copy.text = body; dialog.SetActive(true); journal.SetActive(false);
            next = onContinue; actions.Clear();
            for (int i = 0; i < choices.Count; i++) choices[i].gameObject.SetActive(false);
            var button = choices[0]; button.gameObject.SetActive(true); SetButton(button, "Continue [ E ]", () => { if (next != null) { var n = next; next = null; n(); } });
            presentation.Present(false);
        }
        void Ask(string title, string body, string[] options, Action<int> answer)
        {
            Show(title, body, null); actions.Clear();
            for (int i = 0; i < choices.Count; i++)
            {
                choices[i].gameObject.SetActive(i < options.Length);
                if (i >= options.Length) continue;
                int index = i; Action choose = () => answer(index); actions.Add(choose);
                SetButton(choices[i], (i + 1) + ".  " + options[i], choose);
            }
            presentation.Present(true);
        }
        void SetButton(UnityEngine.UI.Button button, string label, Action action)
        { button.GetComponentInChildren<TextMeshProUGUI>().text = label; button.onClick.RemoveAllListeners(); button.onClick.AddListener(() => action()); }
        void ToggleJournal()
        {
            journal.SetActive(!journal.activeSelf); if (!journal.activeSelf) return;
            journalCopy.text = "";
            for (int i = 0; i < 3; i++) journalCopy.text += (Progress.Wards[i] ? SummerContent.Notes[i] : new[] { "WORKSHOP / Restore this ward to collect its lesson.", "TRAINING GARDEN / Restore this ward to collect its lesson.", "MESSAGE POST / Restore this ward to collect its lesson." }[i]) + "\n\n";
            journalCopy.text += party.text;
            var titles=new string[3];var notes=new string[3];var names=new[]{"Village Workshop","Training Garden","Message Post"};
            for(int i=0;i<3;i++){titles[i]=names[i]+"  /  "+(Progress.Wards[i]?"RESTORED":"NOT YET");notes[i]=Progress.Wards[i]?SummerContent.Notes[i].Split('\n')[1]:"Restore this ward to collect its lesson.";}
            presentation.SetJournal(titles,notes,party.text);
        }
        Vector2 Destination()
        {
            if (!Progress.Introduced) return maleWarrior.transform.position;
            if (!Progress.Wards[0]) return workshop.position;
            if (!Progress.Wards[1]) return Progress.HintChosen && !Progress.PatternComplete ? stones[SummerContent.Pattern[Progress.PatternStep]].position : garden.position;
            if (!Progress.Wards[2]) return messagePost.position;
            return Progress.Complete ? (Vector2)fallGate.position : orc.transform.position;
        }
        void RefreshWorld()
        {
            arenaSeal.SetActive(!Progress.CanFight); exitSeal.SetActive(!Progress.Complete);
            for (int i = 0; i < 3; i++) wardLights[i].color = Progress.Wards[i] ? new Color(.5f, 1, .75f) : new Color(.7f, .55f, .30f);
            for (int i = 0; i < shields.Length; i++) shields[i].enabled = !Progress.Complete && i >= Progress.BossScore;
            objective.text = "WARD " + Progress.WardCount + " / 3   |   COURAGE " + Progress.Courage + " / 3\n" + (Progress.Complete ? "Walk through the Fall gate" : Progress.CanFight ? "Challenge the Suncrest Guardian / 4 of 5 to pass" : !Progress.Wards[0] ? "Workshop / Choose the right tool" : !Progress.Wards[1] ? "Training garden / Keep your practice" : "Message post / Ask safely");
            var destination=Destination();
            if(!InputBlocked)presentation.SetCompass(player.transform.position,navigation.Next(player.transform.position,destination),destination);
            prompt.text=InputBlocked||nearby<0?"":"[ E ]  "+new[]{"Talk to the Warrior","Visit the workshop","Practice in the garden","Read the message post","Face the Suncrest Guardian","Enter the Fall gate","SUN stone","LEAF stone","SEED stone","Talk to the Warrior"}[nearby];
            party.text = Progress.Complete ? "PARTY / Ambros + Mage + both Warriors" : "PARTY / Ambros + Ancient Mage";
        }
        void CreateUI()
        {
            var ui=new GameObject("Summer interface");ui.transform.SetParent(transform,false);
            presentation=ui.AddComponent<SeasonOverlay>();presentation.Initialize(font,false);
            canvas=presentation.Canvas;dialog=presentation.Dialog;journal=presentation.Journal;
            heading=presentation.Heading;copy=presentation.Copy;objective=presentation.Objective;compass=presentation.CompassText;prompt=presentation.Prompt;journalCopy=presentation.JournalCopy;party=presentation.Party;
            choices.AddRange(presentation.Choices);
            navigation=new SeasonNavigation(new Vector2(-30,-32),new Vector2(30,28));
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
            music = gameObject.AddComponent<AudioSource>(); music.playOnAwake = false; music.loop = true; music.volume = .12f;
            good = Tone("Summer positive", .2f, 660); bad = Tone("Summer reconsider", .25f, 210, true); ward = Tone("Ward restored", .8f, 520); strike = Tone("Shield impact", .12f, 100, true);
            int rate = 22050, length = rate * 16; var samples = new float[length]; float[] notes = { 261.63f, 329.63f, 392f, 329.63f, 293.66f, 349.23f, 440f, 392f };
            for (int i = 0; i < length; i++) { float t = (float)i / rate; float beat = t % 2; float envelope = Mathf.Sin(Mathf.PI * beat / 2); samples[i] = .12f * Mathf.Sin(2 * Mathf.PI * notes[(int)(t / 2) % notes.Length] * t) * envelope + .035f * Mathf.Sin(2 * Mathf.PI * 130.815f * t) * Mathf.Sin(Mathf.PI * t / 16); }
            var ambient = AudioClip.Create("Original Summer melody", length, 1, rate, false); ambient.SetData(samples, 0); music.clip = ambient; music.Play();
        }
        void OnDestroy()
        {
            foreach (var c in new[] { good, bad, ward, strike, music ? music.clip : null }) if (c) Destroy(c);
        }
    }
}
