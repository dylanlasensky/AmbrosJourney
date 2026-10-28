# Ambros' Journey

A top-down pixel-art adventure for middle-school players learning to use AI thoughtfully and safely. Ambros is pulled into a game and travels through four seasons to find a way home. Lessons and responses are authored; there is no live AI service.

**Team:** Dylan Lasensky, Kaley Carrubba, and Kaylie Pasternak · CMS 484 Capstone

## Play the current game

Open the **inner `capstone` folder** in Unity **6000.6.2f1**. Open `Assets/Opening/Intro.unity` and press Play, or use **Capstone > Play From Opening**. The Blossom Valley main menu supports New Journey, Continue, Settings, Quit, and a completed explorer's certificate. New Journey asks for a name and confirms before replacing an existing save.

New Journey enters Spring directly while the teammate's intro video is absent. Optional intro/outro hooks are implemented; this update does not supply replacement videos. Close the conversation at each unlocked seasonal gate, then walk through it to continue.

| Progression | Helper and teaching goal | Encounter and reward |
| --- | --- | --- |
| Spring Valley | Ancient Mage: examples and training limits, humanlike language, verification and privacy | Three notes and six true/false claims; **Blossom Guardian**, 4/5. Apprentice rank; Mage joins. |
| Summer Valley | Married Warriors: useful assistance, independent practice, classroom rules, privacy and real support | Workshop, stone pattern and message post; **Suncrest Guardian**, 4/5. Independent Thinker; both Warriors join. |
| Fall / Emberwood | Emberfist: goals, context, audience, format, useful revisions and factual limits | Three notes and seven choices; **Bramble Sentinel**, 4/5. Prompt Pathfinder; Emberfist joins. |
| Winter / Frostveil | Little Mage: original sources, independent evidence, dates, uncertainty and corrections | Two outdoor notes with four choices; **Frostbound Warden**, 4/5. Winter crest; Little Mage joins and the cave opens. |
| Echo Cavern finale | Apply AI safety across the whole journey | Echo Archive note and two choices, then **Echo Keeper**, an ancient AI robot: eight new activities, **7/8**. Optional outro and personalized PDF certificate. |

Every season now has its own animated guardian. Spring has a blossom-covered woodland tortoise, Summer a golden sunflower-maned lion, Winter a snowy ice-armored bear, and the final Keeper is a towering rune-powered machine. Fall's existing autumn stone-and-bramble boss is preserved. The new bosses have two idle poses and separate windup/impact poses tied to answer feedback. [Boss art and animations](docs/Bosses.md)

## Controls and progress

- **WASD / arrows:** move. **E / Enter:** interact and continue. **1/2** answers Spring's claims; later choices also use **3** or the mouse.
- **J:** field journal. **Esc** or Pause: pause/menu controls. **R:** return to the current region's arrival. **F:** collect a nearby artifact. **M:** mute/unmute seasonal audio in Summer, Fall and Winter.
- Finale: select details, match jobs, or drag cards into order. Clicking a card and a slot works too; number keys and Enter provide alternatives. Check submits the completed activity.
- Pause offers Resume, Save and return, reset with confirmation, artifacts, Explorer/Assisted guidance and travel to unlocked seasons.
- Progress saves locally, including exact chapter/region coordinates, partial lessons, bosses, artifacts and completion. Autosave runs every five seconds during exploration. There is one profile per device with a recovery copy, not cloud accounts.
- Three optional artifacts in each season unlock the **Evidence Keeper** achievement and Evidence Compass collection lesson. The final's evidence topic is also taught in Winter; collecting all artifacts is not required to understand or enter the finale.

All four seasons, menus, controls, notes and activities use the supplied **Determination** font with the existing point sizes and shared parchment UI. Minor labels are shortened to fit. [Current questions and notes](docs/QuestionRefresh.md)

## Documentation and checks

- [Game design and progression](docs/GameDesign.md)
- [Menus, saves, artifacts and teammate video handoff](docs/JourneyCompletion.md)
- [Spring setup](SPRING_PROTOTYPE.md), [Summer](capstone/Assets/SummerLevel/README.md), [Fall](capstone/Assets/FallLevel/README.md), [Winter](capstone/Assets/WinterLevel/README.md)
- [Testing and builds](docs/Testing.md), [latest combined validation](docs/CurrentValidation.md), [documentation index](docs/README.md)

The hosted Game checks workflow runs progression and asset-integrity checks. Unity playthrough, rendering and player builds are additional local checks; a passing source test alone is not a complete release test. Commit required `.meta` files with assets and keep generated caches/builds/logs out of Git. Preserve existing art and font licenses.

## Design targets and remaining work

Spring targets about five minutes; the whole journey targets roughly thirty minutes. These are unmeasured first-time-player targets, not promised durations. Navigation becomes more complex in Fall and Winter without artificial waiting timers. Explorer guidance uses broad bearings at long range in those seasons; Assisted restores exact route guidance, including during Winter snow.

The teammate's final intro/outro clips, broader inventory/customization and audience testing remain outstanding. The implemented finale goes directly to the ending/certificate flow; the older plan to require a playable return to Spring is not currently implemented. User testing should check comprehension, difficulty, pacing, audio balance, screen shapes and standalone behavior on target hardware.

The team's original Fall 2026 milestones remain planning context: Oct 19-23 rough Spring/Summer; Oct 26-30 rough Fall/Winter/finale; Sprint Three due Nov 4; later November polish, audience testing and post-Thanksgiving demo preparation. A milestone date is not evidence of completion.
