# Ambros' Journey

A top-down pixel-art adventure for middle-school players learning to use AI thoughtfully and safely. Ambros is pulled into a game and travels through four seasons to find a way home. Lessons and responses are authored; there is no live AI service.

**Team:** Dylan Lasensky, Kaley Carrubba, and Kaylie Pasternak · CMS 484 Capstone

## Play the current game

Open the **inner `capstone` folder** in Unity **6000.6.2f1**. Open `Assets/Opening/Intro.unity` and press Play, or use **Capstone > Play From Opening**. The Blossom Valley menu offers name entry, New Journey, Settings and Quit. A completed explorer can reopen their certificate until starting another journey or closing/reloading the game.

Enter an explorer name and choose New Journey to play the supplied intro, then start in Spring. Play in one sitting: progress is not saved between launches or browser refreshes. After viewing the certificate, walk through the home portal to play the supplied outro. Close the conversation at each unlocked seasonal gate, then walk through it to continue.

| Progression | Helper and teaching goal | Encounter and reward |
| --- | --- | --- |
| Spring Valley | Ancient Mage: examples and training limits, humanlike language, verification and privacy | Three notes and six true/false claims; **Blossom Guardian**, 4/5. Apprentice rank; Mage joins. |
| Summer Valley | Married Warriors: useful assistance, independent practice, classroom rules, privacy and real support | Workshop, practice-garden field note and message post; **Suncrest Guardian**, 4/5. Independent Thinker; both Warriors join. |
| Fall / Emberwood | Emberfist: goals, context, audience, format, useful revisions and factual limits | Three notes and seven choices; **Bramble Sentinel**, 4/5. Prompt Pathfinder; Emberfist joins. |
| Winter / Frostveil | Little Mage: original sources, independent evidence, dates, uncertainty and corrections | Two outdoor notes with four choices; **Frostbound Warden**, 4/5. Winter crest; Talk to Little Mage with E for the cave briefing, then walk inside. |
| Echo Cavern finale | Apply AI safety across the whole journey | Echo Archive note and two choices, then **Echo Keeper**, an ancient AI robot: eight new activities, **7/8**. Defeat animation, dropped certificate scroll, E pickup and personalized PDF; Exit returns to the cave, where an animated portal leads to the main menu. |

Spring, Summer and Fall exit portals appear only after their guardian dissolves into pixels and the portal finishes assembling. Winter keeps its natural cave without a portal overlay or building animation; the Warden still dissolves. Talk to Little Mage beside the entrance with E, then walk inside after the victory effect finishes. The final certificate retains Download PDF; Exit/Escape returns to the cave and builds a home portal that leads to the menu.

Every season now has its own animated guardian. Spring has a blossom-covered woodland tortoise, Summer a golden sunflower-maned lion, Winter a snowy ice-armored bear, and the final Keeper is a towering rune-powered machine. Fall's existing autumn stone-and-bramble boss is preserved. The new bosses have two idle poses and separate windup/impact poses tied to answer feedback. [Boss art and animations](docs/Bosses.md)

## Controls and progress

- **WASD / arrows:** move. **E:** interact. **E / Enter:** continue a conversation. **1/2** answers Spring's claims; later choices also use **3** or the mouse.
- **J:** field journal. **Esc** or Pause: pause/menu controls. **E:** collect a nearby artifact. **M:** mute/unmute seasonal audio.
- Finale: select details, match jobs, or drag cards into order. Clicking a card and a slot works too; number keys and Enter provide alternatives. Match rows cycle through options when clicked repeatedly; Check submits the completed activity.
- Pause offers Resume, a confirmed end-run return to the main menu, artifacts, always-on guidance and travel to seasons unlocked during this run.
- Lessons, bosses, artifacts and chapter locations remain in memory during the current run, including season revisits. Ending the run, starting another journey, closing the game or refreshing the page starts over. Previous local saves are ignored and left untouched. There is no Continue or autosave. See [session behavior and browser checks](docs/BrowserSaving.md).
- The HUD explicitly reads **0/12 Artifacts**. Three optional artifacts in each season unlock the **Evidence Keeper** achievement and Evidence Compass collection lesson. The final's evidence topic is also taught in Winter; collecting all artifacts is not required to understand or enter the finale.

Field notes, challenges, guides and pickups show compact overhead **[E]** hints only within interaction range. Bosses can be approached from up to 5.5 world units away; nearby lesson objects retain priority. NPCs wait for E before opening a conversation. Summer's Warriors and Fall's Emberfist start their escort after that greeting and wait for another E interaction at the destination. Decorative portal, location and route headings are removed; the objective and player arrow identify the next stop. See [companion guidance](docs/CompanionGuidance.md).

Correct responses say **Correct!** and wrong responses say **Incorrect** before their explanation. Field notes let players retry the same question; bosses record each answer once and advance. A failed boss attempt restarts the full quiz. Scores keep the full quiz total throughout an attempt: five for seasonal guardians, eight for the final review, and the actual fixed count for individual lessons. Courage/hearts and their penalties are removed; retries keep completed learning.

Winter's arrival Mage is clear of the portal, and the Warden stands directly in front of the visible cave opening. After the Warden victory, press E beside Little Mage to read the cave briefing. Once the victory effect finishes, walk through the opening to enter Echo Cavern. He follows inside without a dialogue prompt. Continuous ridge barriers stop travel behind the cave. The former frozen pond is now a snowy fir grove with a walking gap. Extended ground and a viewport-aware camera clamp keep the outdoor map edge out of view.

A bright gold-and-cream arrow with a dark outline follows Ambros and rotates smoothly through 360 degrees toward the next required destination. Its broad single head and slow, slight size pulse make the direction easier to notice. It points directly to the objective, so players choose a walkable route around scenery. Guidance stays on throughout Winter, with no frozen-arrow popup. Echo Cavern has a small visibility circle around Ambros and warm light pools at its lanterns; the HUD remains fully readable.

All four seasons, menus, controls, notes and activities use the supplied **Determination** font with the existing point sizes and shared parchment UI. Minor labels are shortened to fit. [Current questions and notes](docs/QuestionRefresh.md)

## Documentation and delivery

- [Current design](docs/GameDesign.md), [runtime flow](docs/JourneyCompletion.md), [curriculum](docs/QuestionRefresh.md)
- [Testing and rebuild instructions](docs/Testing.md), [executed results and limits](docs/CurrentValidation.md)
- [Implemented user feedback](docs/UserTestingFeedback.md), [documentation index](docs/README.md)
- [Sprint reports and team contract](Reports/README.md)
- [Web, macOS and Windows packages](Builds/README.md)
- [Spring](SPRING_PROTOTYPE.md), [Summer](capstone/Assets/SummerLevel/README.md), [Fall](capstone/Assets/FallLevel/README.md), [Winter/cave](capstone/Assets/WinterLevel/README.md)

Music On/Off is in the pause journal and main-menu Settings. There is no music button over gameplay. The preference applies across scenes for the current application session; effects remain separate. M is the existing all-audio mute shortcut.

GitHub Actions runs progression, asset, session-policy, Web-template and cinematic-focus checks. Unity runtime checks and release builds supplement them. Required assets and their .meta files must stay together. Generated caches, raw demo recordings and unpacked players are excluded; selected deliverable archives live under Builds. Existing asset/font/shader license notices are preserved.

## Design targets and remaining work

Spring targets about five minutes and the complete journey roughly thirty minutes; these are unmeasured first-time-player targets. Navigation becomes more complex through Fall and Winter without artificial waiting timers. All content is authored; no live AI model, backend, database or persistent progress save is required.

The team tested individually and with friends/roommates. Middle-school audience testing, measured learning outcomes, low-end hardware profiling and accessibility improvements remain future work. The current itch upload must be replaced with the new Web ZIP and tested on the host; a successful build does not establish that upload's behavior.

Primary deliverables: [Play on itch.io](https://kcarrubba.itch.io/ambros-journey) and [GitHub source](https://github.com/dylanlasensky/AmbrosJourney). Team: Kaley Carrubba, Kaylie Pasternak and Dylan Lasensky, Two Kaylies and a Dylan.
