> Current handoff. For the current feedback changes see [User testing](UserTestingFeedback.md). For boss, font and question sources see [Bosses](Bosses.md), [Question refresh](QuestionRefresh.md) and [current validation](CurrentValidation.md).

# Journey completion update

## Play and save

Open `capstone` in Unity **6000.6.2f1**, open `Assets/Opening/Intro.unity`, and press Play. The opening scene starts with a name-entry menu; its empty name field pulses gold until a nonblank name is entered. New Journey loads Spring immediately while the teammate-owned intro is absent. Continue skips the opening and restores the last saved location.

Pause is always available on the right side of every season, or with **Esc**. It stops physics, animation and audio, and offers Resume, Save and return to menu, artifacts, guidance, reset, and travel to unlocked seasons. The same profile has one checkpoint per season. Travel lets players recover missed collectibles without restarting lessons. Returning to a checkpoint on a portal does not immediately send the player through it: step away first.

Saves contain the explorer name, exact player coordinates, Winter outdoor/cave region, completed and partial lessons, boss progress, collected artifacts and completion date. Returning from a menu closes a conversation; an unfinished boss encounter can be restarted without losing completed lessons. Save and return captures the current position immediately; normal exploration also autosaves every five seconds. One profile is stored locally on each device, in `Application.persistentDataPath/ambros-journey-v1.json`. There is no cloud account or name transmission. Atomic replacement retains a `.bak` recovery copy. Corrupt, oversized or invalid saves are rejected. Reset has a confirmation screen and removes the primary, backup and pending save, then returns to name entry.

## Exploration and learning

- Spring field note 2 is at **(7, 1)**, on the approach to the lakeside Myth Bat at (10, 6).
- Three artifacts are placed in each season. Approach and press **F**. The HUD reads 0/12 Artifacts and the collection screen tracks all twelve. Completed seasons remain available from Pause, so none is permanently missable.
- Completing the collection awards **Evidence Keeper** and reveals the Evidence Compass lesson: agreement between AI tools is not independent corroboration. Winter also teaches this idea before the final activities; collection completion is optional.
- Fall and Winter use broad bearings at long range in **Explorer** mode. Winter snow periodically obscures the compass outdoors, and snowdrifts partly cover the northern trail. Landmarks, lanterns and signposts provide clues. The first outage shows a compact icicle-edged notice naming the next unfinished landmark; its once-only flag is saved. The existing temporary snowfall cycle is retained. **Pause → Guidance: Assisted** restores exact distance and route guidance, including during snowfall.
- The **Frostbound Warden** stands to the right of Echo Cavern, leaving the opening visible. Complete the two outdoor evidence stations, then score **4/5** to earn the Winter crest and recruit Little Mage. Little Mage then waits beside the cave for a separate briefing. Walking into the unlocked opening enters automatically; no E-to-enter dialogue is required. Ridge barriers block travel behind the cave, and older saves there recover at the approach.
- Inside the cave, Little Mage follows without a dialogue interaction or overhead E card. Complete the Echo Archive to open the final hall. The **Echo Keeper** presents eight new AI safety activities spanning all four seasons: selecting private details, arranging a prompt, matching tasks to help and checking evidence. **7/8** passes. Dragging has click and keyboard alternatives. The independent-evidence lesson is taught in Winter, so it does not depend on collecting optional artifacts. Correct feedback says Correct! and each score denominator stays fixed at the full quiz length. Courage/hearts are removed. Retries preserve lessons, crests and collectibles.

## Teammate video handoff

The retired placeholder intro and its generation tools have been removed. New videos belong to the teammate.

**Intro:** add the teammate's clip at `Assets/Resources/Journey/Intro.mp4` and commit its `.meta`, or assign their VideoClip to `OpeningCinematic.clip` in `Assets/Opening/Intro.unity`. With neither supplied, New Journey enters Spring immediately. There is no placeholder or missing-video screen. A supplied clip still transitions to Spring when it ends and retains an Enter-to-continue fallback if playback fails.

**Outro:** `JourneyEnding.Play` retains a player for `Assets/Resources/Journey/Outro.mp4`, with unscaled playback and missing/failed-video fallback. It is currently not invoked by `FinishJourney`: the active flow crumbles the Keeper, drops the scroll, and opens the certificate on E pickup. Adding a clip alone does not activate an outro. The teammate must connect the helper to the agreed pickup/ending handoff and revalidate that flow. Do not change the resource path accidentally.

The certificate uses the saved explorer name and original completion date. Return and Escape open the main menu rather than resuming the finished world; completed explorers can reopen the certificate there. On desktop, Download PDF saves a dated PDF in the user's Downloads folder (persistent data folder if Downloads is unavailable) and opens it in the default viewer. A WebGL download bridge is included, but browser-build behavior has not been tested on this Mac. The certificate is a high-resolution rendered page, preserving the game's text layout; it is not a professional qualification.

## Interaction cards

Each interactive helper or lesson object uses one overhead [E] card; artifacts use [F]. Cards follow the current sprite bounds, avoid overlapping neighboring cards and hide during conversations/journals. Automatic portal destinations do not imply an E requirement. The redundant bottom interaction card is disabled. The Garden Warrior stops advertising her finished dialogue, including after a save reload; other helpers and lesson targets keep their established interactions.

## Graphics and animation

The generated atlas uses the existing seasonal terrain pack as its visual reference: rounded violet-gray stone, warm ivory highlights, earthy roofs and clustered pixel shading. It adds warm and snowy ranger cottages, a mountain integrated with the cave mouth, and collectible compass shrines. Existing shelter/cottage visuals use the same family. Fall's enlarged placeholder guardian is replaced by a four-pose stone-and-bramble Sentinel atlas. Only authored landmark buildings remain; their front steps connect to trails. Seasonal cottage variants, snow-rooted trees, carved signs and grouped local furnishings replace generic extra-house placement. Generated files and import metadata live under `Assets/Journey/Resources/Journey`.

Water surfaces have moving highlights, lanterns subtly flicker, and existing blossom/leaf/snow effects continue. All UI now uses the supplied Determination font through a baked TMP asset, including menu, notes, choices, controls, collection and certificate. Existing point sizes remain; font leading is normalized and cramped labels are shortened.

Spring, Summer and Winter have new seasonal guardian atlases; the final Echo Keeper is a larger ancient AI robot. Two idle poses and distinct one-shot windup/impact poses respond to encounter events. Fall's existing Sentinel is unchanged. See `Bosses.md`.

Companions retain a safe route spine but use different gaps and slowly varying lateral offsets. Obstacle checks pull the formation back into narrow passages; teleports reset its route.

## Performance changes

- Hardware VSync divides common display refresh rates down to at most 60 FPS: 60→60, 90→45, 120→60, 144→48, 165→55, 240→60. Other refresh configurations fall back to a 60 FPS software target. This follows Unity's preference for hardware frame pacing on desktop.
- Background running is disabled. Losing focus pauses active gameplay and saves the current location.
- The route compass samples at five updates per second. Bounded A* replaces breadth-first exploration; each search caches checked cells and uses reusable physics-result buffers. Formation clearance is sampled, rather than allocating cast results every frame.
- Builds open in a 1280×720 window and use a non-development release build. Unity Editor testing still has Editor overhead.

These changes address identifiable unnecessary work. They do not establish the temperature on the teammate's laptop; a standalone build should still be compared on that hardware.

## Research and asset choices

- [A Short Hike](https://ashorthike.com/): marked routes, backcountry exploration and optional discoveries informed the landmark/collectible loop.
- [TUNIC](https://tunicgame.com/): exploration, lost knowledge and staged discoveries informed the final-cave structure. No game art was copied.
- [Unity frame-rate documentation](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Application-targetFrameRate.html): desktop VSync and target-frame-rate behavior.
- [Kenney Tiny Town](https://kenney.nl/assets/tiny-town) and [Tiny Dungeon](https://kenney.nl/assets/tiny-dungeon): reviewed as CC0 alternatives. Their 16×16 style is simpler than the existing terrain; this update uses referenced original artwork and the team's existing pack to preserve visual consistency. No additional third-party pack was imported.

## Verification

The expanded pure .NET suite covers all 32 Summer, 243 Fall, 243 Winter and 256 final-activity pass/fail combinations, plus save round trips, corrupt-primary recovery, reset, bounds and name handling. The Unity integration runner traverses the actual four-season portals, checks mission/boss retries and UI text at 1280×720 and 1024×768, follows routes to every artifact, collects all twelve with F-key input, verifies outdoor and cave checkpoint restoration, exports a personalized PDF, resets the profile, and checks fresh Spring starts directly when the teammate intro is absent.

Run the existing .NET project, `python3 tests/check_assets.py`, and Unity's `SeasonIntegrationChecks.Run`. Runtime captures, test saves and generated sample PDFs are written under `capstone/Logs/SeasonChecks` and are not source assets. `SeasonIntegrationChecks.Build` creates the release Mac application there.

## Homepage

The blossom-valley homepage follows the supplied visual reference. Returning explorers see Continue and their actual saved season. New Journey asks before resetting an existing save; fresh explorers are guided to the glowing empty name field before entering Spring (or a teammate-supplied intro). Settings changes Explorer/Assisted guidance, persists that choice for existing saves, and retains a draft name before a new game. Quit exits the standalone game; a browser build instead explains that the tab can be closed. Completed explorers can reopen their certificate from the menu. The intro hook and retained, currently unwired outro helper remain teammate-owned.
