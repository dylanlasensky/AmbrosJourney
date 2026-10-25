# Ambros' Journey

[Automated checks and local validation](docs/Testing.md)

A Unity 2D adventure that teaches middle-school players how to use AI safely. Ambros is playing a game on his laptop at home when he is pulled into its world. To get home, he must explore four seasons, learn from their inhabitants, and pass each guardian's quiz before facing a final AI Safety challenge.

**Team:** Dylan Lasensky, Kaley Carrubba, and Kaylie Pasternak · CMS 484 Capstone

## Current build

**Spring Valley is playable:** an expanded woodland map, enlarged cottage with a connected entrance path, lake and dock, winding trails, spaced-out encounters, three field-note stops, six myth-bat questions, a journal, directional compass, and a separate Thornhide Guardian arena. Scoring **4/5 (80%)** on the guardian quiz opens the Summer gate and adds the Ancient Mage to the party.

**Summer Valley** adds three ward missions about tool choice, practice and safe requests. **Fall / Emberwood** adds a 99 × 93-tile autumn map, two river crossings, hedge switchbacks, seven prompt-engineering choices, a journal, route-aware compass and a five-question guardian. Pass with **4/5 (80%)** to recruit Emberfist and unlock the Winter gate. See [Fall implementation notes](capstone/Assets/FallLevel/README.md) and [validation](docs/FallValidation.md).

Spring targets **about five minutes** for a first-time player; the full game targets **about thirty minutes**. These are design targets pending user testing. The [gameplay demo](docs/SpringDemo.mp4) follows a faster, familiar route.

The opening automatically transitions into Spring, but its current 20-second animation is **temporary**. The final opening should match the original illustrated Ambros references and remains unfinished. All four seasons are playable. Walk through each unlocked seasonal gate after closing its conversation: Spring → Summer → Fall → Winter. Winter includes Frostveil outdoors and the explorable Echo Cavern, where the Frostbound Warden guards the Winter crest. The finale remains planned. Progress lasts only for the current session; main-menu, pause-menu, and persistent-save features are not yet complete.

See [Winter setup and art credits](capstone/Assets/WinterLevel/README.md). All four seasons now share the same parchment UI, dialogue buttons, objective cards and journal.

## Game direction

| Chapter | Learning focus and cast | Status |
| --- | --- | --- |
| Level 1 — Spring | Ancient Mage introduces AI basics; misinformation bats test understanding; Thornhide guards the exit. | Playable; polish and user testing remain |
| Level 2 — Summer | Married Warriors teach the risks of overreliance on AI; an Orc encounter reinforces the lesson. | Playable; connects to Fall |
| Level 3 — Fall | Emberfist teaches prompt engineering across Emberwood: three rune missions and the Bramble Sentinel. | Playable; user testing remains |
| Level 4 — Winter | Little Mage teaches verification across Frostveil and Echo Cavern; the Frostbound Warden tests evidence-checking. | Playable; two maps, cave boss and Winter crest |
| Final level | Cumulative AI Safety quiz and return-to-Spring escape resolution. | Planned |

Each season should teach through short exploration missions before testing the player. Companions join after their season. The intended ending uses an item obtained in Winter to return home from Spring; a completion certificate is a future goal. Educational interactions use authored content, without a live AI API.

The gameplay aesthetic is whimsical top-down pixel art: seasonal nature, fantasy characters, and signs of an AI/cyborg takeover. Build on the existing character and environment packs. Leave generous walking space, add meaningful scenery, connect paths to entrances, and spread lessons across recognizable landmarks. Use clear on-screen objectives and a playful, readable UI. Bosses should look visibly larger and different from ordinary enemies. The opening has its own illustrated storybook style, with continuous action rather than captioned still images.

See [Game Design and Team Handoff](docs/GameDesign.md) for progression rules, sprint context, and collaboration guidance, and the [intro brief](docs/IntroVideoBrief.md) for opening shots and references.

## Timeline — Fall 2026

These are team targets, not claims that scheduled work is already finished.

| Week | Dates | Goals |
| --- | --- | --- |
| 1 | Oct 19–23 | Rough drafts of Levels 1 and 2; main menu; pause and return-to-menu buttons |
| 2 | Oct 26–30 | Rough drafts of Levels 3 and 4; rough final AI Safety quiz |
| 3 | Nov 2–6 | **Sprint 3 due Nov 4**; finalize Levels 1 and 2 |
| 4 | Nov 9–13 | Finalize Levels 3, 4, and the final level; user testing with friends and the target audience |
| 5 | Nov 16–20 | Finishing touches; prepare for the demo after Thanksgiving |

## Open and play

1. Clone this repository and open the **inner `capstone` folder** in Unity Hub with **Unity 6000.6.2f1**. Let asset imports finish.
2. Select **Capstone → Play From Opening** or **Capstone → Play Spring Only**; use **Capstone → Summer → Play Summer Only** or **Capstone → Fall → Play Fall Only** to start later chapters directly in the Editor. **Capstone → Winter → Play Winter Only** opens Frostveil.
3. Move with **WASD / arrow keys**, interact with **E**, answer with **1 / 2** in Spring or **1 / 2 / 3** in Summer/Fall, open the journal with **J**, close dialogue with **Escape**, or return to the cottage with **R**.

Use **Capstone → Validate Spring Progression** for the progression check. [Spring implementation notes](SPRING_PROTOTYPE.md) describe scene rebuilding, demo recording, and known limitations. Coordinate scene edits, commit asset `.meta` files with assets, keep Unity-generated caches out of version control, and preserve asset-pack licenses and credits.
