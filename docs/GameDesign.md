# Game design and team handoff

Read this alongside the README before extending Ambros' Journey. Planned features are not all implemented.

## Sprint context

Early repository work established the Unity project, character and animation packs, and seasonal environment assets. Map art is a foundation, not evidence that every season is playable.

Sprint Two established the four-season educational adventure. Ambros uses the Chibi Assassin character. The Ancient Mage is another child who has been trapped so long that he has grown old. Summer's married Warriors teach overreliance, Fall's Emberfist Warrior teaches prompt engineering, and Winter's Little Mage teaches verification. Companions join after their seasons. The eventual final AI Safety challenge, return to Spring with an item obtained in Winter, and completion certificate resolve the wider adventure.

The current direction updates that plan:

- Ambros starts by playing a game on his laptop at home and is pulled inside. The roughly twenty-second opening is noninteractive and hands control to Spring.
- Spring targets five minutes within a roughly thirty-minute full game. Do not pad playtime with timers or repetitive dialogue.
- Exploration and field notes teach concepts before bats test them. A distinct seasonal guardian checks understanding.
- Spring's guardian is Thornhide, a separate large creature rather than another bat.
- The expanded map has an enlarged cottage, connected steps and approach, open woodland, a lake and dock, winding routes, and a southern arena with the gate behind the boss.

Sprint Three is due November 4. The current schedule covers rough versions of all levels, then finalizing Spring and Summer. Later work finishes the remaining seasons and finale, includes audience testing, and prepares the post-Thanksgiving demo. See the README for the team's weekly dates. This is a design handoff, not a replacement for original sprint submissions or proof that every planned deliverable is complete.

## Spring progression contract

1. Arrive near the cottage and meet the Ancient Mage for the three-page introduction.
2. Explore three landmark field-note stops covering training from examples, humanlike responses and verification, and privacy/safe use.
3. Visit three spaced-out misinformation bats. Each has two true/false questions; collect the relevant note first. The compass and prompts guide exploration.
4. Review collected notes with J. Wrong answers provide feedback and retries. Courage can deplete and replenish without erasing lesson progress.
5. Complete Thornhide's five-question quiz. Four correct answers unlock the Summer gate. A lower score permits another attempt while preserving completed lessons.
6. The Mage joins and the gate opens. Close the conversation and walk through the unlocked gate: the runtime Summer portal adapter loads playable Summer.

Test route clarity and comprehension with first-time players. The recorded demo is shorter than the target because it follows a known route and advances text quickly. Keep teaching copy concise and age-appropriate. Teach players to check claims and protect personal information. Use authored lessons rather than a live model service.

## Art, layout, and UI

Preserve the original pixel-art characters and seasonal environment identity. Match new assets in palette, scale, perspective, and readability. Detail should distinguish places without crowding paths. Keep cottage steps usable, shoreline boundaries clear, and the boss approach readable. Avoid placing all enemies at spawn or reducing the route to a simple T-shaped junction.

Use a cartoony fantasy UI with legible text, contextual E prompts, a journal, and clear objectives. Directional guidance should encourage movement between lessons. The guardian must look different from the bats.

Intro and outro videos are owned by the teammate. The retired procedural intro and its generator have been removed. The game starts directly in Spring until a teammate clip is supplied; see `JourneyCompletion.md` for the optional video hooks.

## Implementation and collaboration

| Location | Purpose |
| --- | --- |
| `capstone/` | Unity project root; open this folder in Unity Hub |
| `capstone/Assets/SpringPrototype/Scripts/` | Player, camera, dialogue, lessons, UI, progression, atmosphere |
| `capstone/Assets/SpringPrototype/Editor/` | Scene builders, polish tools, validation, demo capture |
| `capstone/Assets/Resources/Journey/Intro.mp4` | Optional teammate-owned intro (not supplied) |
| `tools/opening-art/` | Original illustrated opening references |
| `docs/SpringDemo.mp4` | Recorded gameplay preview |
| `SPRING_PROTOTYPE.md` | Reproduction and validation instructions |

World rebuild commands replace generated layout content. Coordinate before running them and apply the cottage/learning-trail polish step afterward. Extend the existing game rather than creating an unrelated prototype.

Commit `.meta` files with new assets so references survive another teammate's checkout. Do not commit Library, Temp, Logs, local capture frames, or editor caches. Agree on scene ownership before simultaneous edits and review scene changes carefully. Preserve source-pack notices and licensing.

## Validation and remaining work

Spring validation covers field-note completion, wrong-answer retries, six bat challenges, a failing 60% guardian result, a passing 80% result, distinct guardian art, and expanded bounds. The gameplay recording exercises the playable route. These checks do not replace a clean build or audience testing.

Spring, Summer, Fall and Winter are playable. The journey update adds the main menu, pause/return/reset, persistent chapter checkpoints, an artifact collection, cumulative final challenge and PDF certificate. The teammate owns the final intro/outro videos. Character customization and broader inventory remain future work; audience testing is still needed. Current movement uses side-facing character frames.


## Summer → Fall progression

Summer's three wards and 4/5 Orc pass are unchanged. Close its final conversation and walk through the Fall portal. The add-only Fall adapter loads the Resources world prefab into a runtime scene, using Summer's existing transition convention.

Fall follows the authored prompt-engineering plan: Emberfist introduces goal/context/format, audience and limits, then checking and revising. Three field-note stations teach before seven applied choices. All three runes unlock the Bramble Sentinel; 4/5 passes its five three-choice questions, recruits Emberfist and opens the Winter gate. Winter now continues through Frostveil and Echo Cavern: two outdoor evidence stations, a five-question Warden outside the cave requiring 4/5, the cave archive, and an eight-question cumulative Echo Keeper requiring 7/8. Final victory connects to the teammate outro and PDF certificate.

Emberwood's 99 × 93 tiles are about 2.5 times Summer's area. Two river crossings, eastern hedge switchbacks and an optional watch-post branch increase navigation difficulty. The compass follows physics-valid routes. Keep the same movement, dialogue, journal, courage recovery and persistent progression conventions. See `capstone/Assets/FallLevel/README.md` for scene ownership, rebuilding, art sources and validation.
