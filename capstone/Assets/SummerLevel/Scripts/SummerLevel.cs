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
        public bool InputBlocked => Transitioning || !dialog || !journal || dialog.activeSelf || journal.activeSelf;
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
        void Start()
        {
            Application.runInBackground = true;
            CreateUI(); CreateAudio();
            mage.follow = player.transform;
            ShowPages(new[] {
                "Warrior: You made it through Spring! We could use someone who knows that a confident answer is not always a correct one.",
                "Warrior: My spouse and I have been defending this village. We have tools for almost everything. The trouble started when people stopped deciding how to use them.",
                "Ancient Mage: A useful tool should help you grow. Restore three ward pieces: choose the right tool, keep your practice, and ask safely. Then face the Orc together."
            }, 0, () => Progress.Introduced = true);
            Debug.Log("SUMMER_READY: three missions, five-question Orc, journal, compass and companions.");
        }
        void Update()
        {
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
                    Show("The married Warriors", Progress.Complete ? "Our village is safe again. We will travel with you. AI can support your ideas and learning. You are responsible for your choices." : "Restore the workshop, training garden and message post. Our ward will open the way to the Orc. We will guard the village while you work.", Close); break;
                case 1: Workshop(); break;
                case 2: Garden(); break;
                case 3: Message(); break;
                case 4: Boss(); break;
                case 5:
                    Show("The Fall gate", Progress.Complete ? "SUMMER COMPLETE\nRank earned: Independent Thinker\nThe married Warriors join your party.\n\nEmberfist's trail marker: Good help begins with a clear request.\n\nEmberfist awaits in Emberwood. Close this conversation and walk through the unlocked portal to begin Fall." : "The Fall gate is sealed. Restore all three wards and score at least 4/5 against the Orc.", Close); break;
                default: Stone(target - 6); break;
            }
        }
        void Workshop()
        {
            if (Progress.Wards[0]) { Show("Workshop restored", SummerContent.Notes[0], Close); return; }
            Show("Village workshop / Choose the right tool", "Warrior: Sometimes AI can suggest ideas or explain something unfamiliar. Sometimes you need to inspect the problem yourself, or ask someone who knows what they are doing.", WorkshopQuestion);
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
                Ask("Training garden / Keep your practice", "The training device offers to do every exercise for you.\n\nWarrior: If a machine does every practice swing, your sword arm gets no practice. Learning works better when you take part.\n\nYou are stuck on a three-stone pattern. What help should you ask for?", new[] { "Give me a hint about the first step", "Complete the whole exercise for me" }, c => {
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
            Ask("Training garden / Put learning first", "A student has an assignment to show their own understanding. What should they do?", new[] { "Try it, then ask for an explanation or permitted feedback", "Submit an AI answer they cannot explain" }, c => {
                bool right = Progress.AnswerGarden(c); Effect(right);
                Show(right ? "Your practice matters" : "Keep the thinking", "Keep the thinking and follow your teacher's rules. Overreliance can mean fewer opportunities to practice skills; AI does not literally remove your brain.", right ? (Action)(() => Reward(1)) : GardenQuestion);
            });
        }
        void Message()
        {
            if (Progress.Wards[2]) { Show("Message post restored", SummerContent.Notes[2], Close); return; }
            if (!Progress.PrivacyComplete)
            {
                var labels = new string[3]; string[] details = { "FULL NAME", "HOME ADDRESS", "PASSWORD" };
                for (int i = 0; i < 3; i++) labels[i] = (Progress.RemovedPrivateDetails[i] ? "Removed: " : "Remove: ") + details[i];
                Ask("Message post / Remove private details", "This example uses placeholders, not real personal information:\n\nI am [FULL NAME], I live at [HOME ADDRESS], and my password is [PASSWORD]. Help me plan a birthday treasure hunt.\n\nSelect all three private details to remove. The treasure-hunt request can stay.", labels, c => {
                    Progress.AnswerMessage(c); effects.PlayOneShot(good, .25f);
                    if (Progress.PrivacyComplete) Show("A safer request", "Help me brainstorm a birthday treasure hunt with five clues for a small garden.\n\nThis gives enough context without identifying anyone. Next, consider when a real person can help.", Message);
                    else Message();
                });
            }
            else Ask("Message post / People matter", "Your friend seems upset. What should you do?", new[] { "Ask AI to decide exactly what my friend feels", "Talk kindly with my friend; get a trusted adult's help if needed" }, c => {
                bool right = Progress.AnswerMessage(c); Effect(right);
                Show(right ? "Ask safely" : "AI cannot know your friend's feelings", "Real conversations and trusted people matter. AI cannot know exactly how another person feels or replace personal support.", right ? (Action)(() => Reward(2)) : Message);
            });
        }
        void Reward(int mission)
        {
            effects.PlayOneShot(ward, .6f);
            Show("Ward piece restored / " + Progress.WardCount + " of 3", SummerContent.Notes[mission] + (Progress.CanFight ? "\n\nThe village ward opens the Orc's clearing. Follow the compass south." : "\n\nYour journal now holds this lesson. Follow the compass to the next ward."), Close);
        }
        void Boss()
        {
            if (Progress.Complete) { Show("The Orc is learning too", "Orc: What if I get it wrong?\nAmbros: Then you practice. You can ask for help without giving up.", Close); return; }
            if (!Progress.CanFight) { Show("The Orc's clearing is sealed", "Restore all three ward pieces first. The journal records your lessons, and the compass points toward your next mission.", Close); return; }
            Show("The Orc / Five shields", "Orc: Why think when a machine can choose everything? Hand over your thinking, and I will handle the rest!\n\nWarrior: Tools can help us. We still make the decisions.\n\nAnswer five questions. Score 4/5 to open Fall. Wrong answers give feedback; retries preserve your missions.", () => { Progress.BeginBoss(); BossQuestion(); });
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
                    Show("Summer cleared / " + Progress.BossScore + " of 5", "Orc: What if I get it wrong?\nAmbros: Then you practice. You can ask for help without giving up.\n\nThe Warriors restore the village ward and join your party.\nRank earned: INDEPENDENT THINKER\n\nAI can support your ideas and learning. You are responsible for your choices.\n\nThe Fall gate is open.", Close);
                }
                else Show("Your score / " + Progress.BossScore + " of 5", "You need 4/5 (80%) to pass. Review the explanations or open your journal. All three missions remain complete.\n\nContinue to try again, or press Escape to return to the village.", () => { Progress.BeginBoss(); BossQuestion(); });
                return;
            }
            var q = SummerContent.Boss[Progress.BossQuestion];
            Ask("Orc / Question " + (Progress.BossQuestion + 1) + " of 5", q.Prompt, q.Choices, c => {
                bool right = Progress.AnswerBoss(c); Effect(right);
                if (right) player.Strike(); else orc.Strike();
                effects.PlayOneShot(strike, .35f);
                Show(right ? "A shield cracks!" : "The Orc attacks", q.Explanation + "\n\nScore: " + Progress.BossScore + " / " + Progress.BossQuestion + " answered. Courage: " + Progress.Courage + " / 3.", BossQuestion);
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
            objective.text = "WARD " + Progress.WardCount + " / 3   |   COURAGE " + Progress.Courage + " / 3\n" + (Progress.Complete ? "Walk through the Fall gate" : Progress.CanFight ? "Challenge the Orc / 4 of 5 to pass" : !Progress.Wards[0] ? "Workshop / Choose the right tool" : !Progress.Wards[1] ? "Training garden / Keep your practice" : "Message post / Ask safely");
            var destination=Destination();
            if(!InputBlocked)presentation.SetCompass(player.transform.position,navigation.Next(player.transform.position,destination),destination);
            prompt.text=InputBlocked||nearby<0?"":"[ E ]  "+new[]{"Talk to the Warrior","Visit the workshop","Practice in the garden","Read the message post","Face the Orc","Enter the Fall gate","SUN stone","LEAF stone","SEED stone","Talk to the Warrior"}[nearby];
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
