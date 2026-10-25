# Fall / Emberwood

Chapter 3 follows Summer. Emberfist teaches prompt engineering through three exploration missions, then a five-question Bramble Sentinel challenge. Educational content is authored, with no live AI API. Progress is session-only.

## Play

Use **Capstone → Fall → Play Fall Only** to start directly. For the connected route, complete Summer, close its final conversation and walk through its unlocked Fall portal. The bridge requires Summer completion, no open dialogue/journal, and the player inside the gate. It fades into the Resources prefab in a new runtime scene, following Summer's Spring adapter. No existing scene or build list is replaced.

WASD/arrows move; E/Enter advances interactions; 1/2/3 or mouse selects answers; J opens the journal; Escape closes after the introduction; R returns to arrival without resetting work; M mutes/unmutes this season's audio.

## Learning route

1. Meet Emberfist at the forge. The Ancient Mage and both married Warriors arrive with Ambros.
2. **Harvest Orchard:** read the note, then assemble a prompt by choosing a goal, relevant context and response format.
3. Cross the **north bridge** to the **Lantern Archive:** read the note, then adapt a request for its audience and choose a safe, permitted hint.
4. Navigate the **eastern hedge switchbacks** to the **Revision Kiln:** read the note, identify a response-format mismatch and choose a specific revision, then distinguish formatting from factual safety.
5. Three runes open the southern guardian arena. Answer five three-choice questions; 4/5 or 5/5 passes. Wrong answers give explanations; failed attempts preserve missions. Courage replenishes rather than resetting learning.
6. Earn Prompt Pathfinder rank, recruit Emberfist and open the Winter gate. Winter remains planned; the gate explains the next chapter and currently ends the playable content.

The map spans 99 by 93 ground tiles, compared with Summer's 61 by 61 (about 2.5 times the area). The river has two crossings, hedges force alternating turns, paths branch to an optional watch post, and recognizable stations remain separated. The compass computes a walkable grid route using the same world colliders, rather than pointing straight through obstacles. No timers pad the level.

## Structure and assets

- `FallWorld`: serialized artwork plus deterministic runtime map builder, physics boundaries and compass routing. Same runtime-world/prefab setup as Summer.
- `FallLevel`: interactions, Canvas/TMP UI, journal, compass, authored challenges and original synthesized music/effects, adapted from Summer.
- `FallProgress`: session state, note prerequisites, mission and guardian rules.
- `FallActor` / `FallCamera`: movement and camera conventions with Fall bounds; followers trace arc-length breadcrumbs with stable spacing and independent walk/idle animation.
- `FallPortalBridge`: completion-gated Summer-to-Fall handoff.
- `Editor/FallTools`: scene/prefab/art generation and progression/route checks.

Fall reuses Summer's original Ambros, Mage, Warrior and prop textures, retained TMP font/material, UI and shader setup. Keep `Assets/SummerLevel` intact. Fall's crops come from the existing autumn Mana Seed sample, Emberfist individual idle/walk frames, and the premium creature sheet (group 5, column at x=160). Original asset-pack notices and licenses apply. Crops are saved under `Art` with point filtering and no compression; runtime does not read source packs or write files.

**Rebuild Fall Definition** only regenerates Fall's art, Resources prefab and Fall scene; it declines to switch scenes when there is unsaved work. Coordinate before rebuilding if teammates have edited the generated Fall definition. It does not rebuild Spring or Summer.

## Validation

**Capstone → Fall → Validate Fall Progression** covers introduction/note prerequisites, all seven choices, wrong-answer retries, duplicate rewards, all 243 guardian answer combinations, the 80% threshold, courage recovery and retained missions. `FallTools.ValidateRoutes()` runs in Play mode and checks all station routes and physical guardian/exit barriers before and after unlocking.

Live checks, visuals and transition results are recorded in `docs/FallValidation.md`. Audience testing and timing with first-time players remain necessary; there is no persistent-save contract yet.

## Spring-style presentation

Summer and Fall share `Assets/SeasonPresentation`: Spring-style rounded parchment dialogue, question buttons, chapter/objective cards, framed landmark labels, journal sections, control hints and gold compass. Summer uses green accents and Fall uses copper. Spring itself is unchanged. Correct/incorrect answers tint the popup; Fall also flashes Ambros and emits gold glints for correct answers and completed runes. Autumn leaves mirror Spring’s seasonal particles.

The shared compass uses collision-aware route lookahead and displays the actual distance to the objective, rather than the distance to a nearby routing cell. It hides during dialogue and journals, matching Spring. Fall companions follow the player’s actual path around turns at 1.8-unit gaps, stop without chasing an offset, and reset together when R returns to arrival. The Mage now has eight original walking frames.

Editor checks are under **Capstone → Presentation**. Interface checks require a fresh playing session of the named season. Build and art preparation require Edit mode. See `docs/PresentationValidation.md`.
