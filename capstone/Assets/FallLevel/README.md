# Fall: Emberwood

Chapter 3 follows Summer. Emberfist teaches how to write useful prompts, inspect the response, and ask for specific changes without assuming that clear instructions guarantee true facts. Educational content is authored; no live AI service is called.

## Play and progression

Use the main menu, finish Summer and walk through its unlocked portal after closing the conversation, or select **Capstone > Fall > Play Fall Only**.

1. Meet Emberfist; Ambros arrives with the Ancient Mage and both Warriors.
2. **Harvest Orchard:** read Say What You Need, then choose a clear goal, relevant background and response format.
3. Cross the north bridge to **Lantern Archive:** read Think About Who It Is For, then choose an audience/scope and a permitted hint that preserves practice.
4. Follow the eastern hedge switchbacks to **Revision Kiln:** read Read It, Then Make a Change, fix a format mismatch, and distinguish following instructions from giving safe factual advice.
5. Seven successful choices restore three runes and open the southern **Bramble Sentinel**. Its five-question challenge requires **4/5 (80%)**; mistakes explain the answer and retries retain the missions.
6. Earn Prompt Pathfinder, recruit Emberfist, close the Winter gate conversation and walk through it to **Frostveil**. Winter and the cave finale are playable.

The 99 x 93-tile map is about 2.5 times Summer's area. Two river crossings, hedge turns and an optional watch-post branch increase navigation complexity. Explorer guidance uses broad bearings for distant objectives; Assisted gives exact route guidance. There are three optional artifacts and no artificial waiting timers.

## Shared systems and ownership

`FallWorld` builds the runtime map/Resources prefab; `FallLevel` owns interactions; `FallProgress` owns guarded lesson and boss rules; `FallPortalBridge` supplies the Summer transition. JourneySession persistently saves partial work, exact positions and artifacts. The shared trail/formation system follows corners and resets the party on return to arrival.

All four seasons use the shared Determination/parchment UI, with copper accents in Fall, answer feedback and seasonal leaves. The existing **Bramble Sentinel artwork, scale and four-pose animation are unchanged by the seasonal boss refresh**. It remains the visual reference for the new Spring, Summer, Winter and final bosses.

Keep `Assets/SummerLevel` and the shared Journey/SeasonPresentation folders intact: Fall reuses their resources. Original Mana Seed, Hero Pack and creature-pack notices still apply. `ArtSources.txt` records crops; the existing generated Sentinel atlas is credited in `docs/JourneyArtPrompts.md`.

**Rebuild Fall Definition** regenerates Fall's art, Resources prefab and scene. Coordinate scene ownership and preserve unsaved work first. It does not rebuild other seasons. Normal gameplay does not read source packs or write asset files.

## Checks

Fall progression checks cover note prerequisites, seven choices, wrong answers, duplicate rewards, all 243 guardian combinations, the 4/5 threshold and retained progress. Live route checks verify station access and locked/unlocked barriers. The connected suite adds real portal transitions, text fitting, saves, artifacts and the mixed finale.

`docs/FallValidation.md` is an older dated snapshot. Use `docs/CurrentValidation.md` and `docs/Testing.md` for the current game. Audience comprehension and timing still need first-time players.
