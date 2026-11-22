> Handoff updated November 22, 2026 for one-sitting play. See [companion guidance](CompanionGuidance.md), [cinematic fullscreen checks](CinematicFullscreenChecks.md) and [Web export/browser checks](BrowserBuildChecks.md). Earlier validation describes its dated snapshot; this update does not claim a new browser pass.

# Journey completion update

## Play in one sitting

Open `capstone` in Unity **6000.6.2f1**, open `Assets/Opening/Intro.unity`, and press Play. Enter an explorer name and choose New Journey. The supplied intro plays, then Spring starts fresh. There is no Continue option or persistent progress saving.

Pause is always available on the right side of every season, or with **Esc**. It stops physics, animation and audio, and offers Resume, confirmed end-run, artifacts, guidance, and travel to seasons unlocked during this run. Revisits retain this run's lessons and collected artifacts. Returning to a chapter position on a portal does not immediately send the player through it: step away first.

The current run holds the explorer name, chapter locations, Winter region, partial lessons, bosses, artifacts and completion date in memory. Closing or refreshing the game, ending the run or beginning a new journey clears that progress. Existing save files and browser storage are ignored and left untouched. There is no autosave, background checkpoint or cloud account. After completing the outro, the earned certificate remains available from the menu until a new journey or reload; a downloaded PDF remains the player's file.

## Exploration and learning

- Spring field note 2 is at **(7, 1)**, on the approach to the lakeside Myth Bat at (10, 6).
- Three artifacts are placed in each season. Approach and press **E**. The HUD reads 0/12 Artifacts and the collection screen tracks all twelve. Completed seasons remain available from Pause during this run, so missed artifacts can still be collected before ending it.
- Completing the collection awards **Evidence Keeper** and reveals the Evidence Compass lesson: agreement between AI tools is not independent corroboration. Winter also teaches this idea before the final activities; collection completion is optional.
- A bright gold-and-cream arrow with a dark outline rotates smoothly toward the next required destination. It remains active throughout Winter, including during snowstorms and cave travel. Players use the terrain to choose a walkable route; the arrow does not select path waypoints or disappear in snow.
- The **Frostbound Warden** stands directly in front of Echo Cavern. Complete the two outdoor evidence stations, then score **4/5** to earn the Winter crest and recruit Little Mage. Closing the victory message automatically enables cave guidance; the Mage's tip does not stop movement or require E. The Warden disintegrates, and the player can walk through the natural doorway once the victory sequence finishes. Caves remain accessible when revisited during this run. Ridge barriers block travel behind the cave.
- Inside the cave, Little Mage follows without a dialogue interaction or overhead E card. Complete the Echo Archive to open the final hall. The **Echo Keeper** presents eight new AI safety activities spanning all four seasons: selecting private details, arranging a prompt, matching tasks to help and checking evidence. **7/8** passes. Dragging has click and keyboard alternatives. The independent-evidence lesson is taught in Winter, so it does not depend on collecting optional artifacts. Correct feedback says Correct! and each score denominator stays fixed at the full quiz length. Courage/hearts are removed. Retries preserve lessons, crests and collectibles.

## Intro and outro videos

The supplied October opening is retained as `Assets/Journey/Resources/Journey/Intro.mp4`. The name-entry menu opens first. Every New Journey plays the 34-second opening, followed by loading and fresh Spring.

`Ambros-Journey-Outro-20s.mp4` is included as `Assets/Journey/Resources/Journey/Outro.mp4`. Collecting the certificate opens it first. Closing it with Exit or Esc resumes the cave and reveals the home portal. Walking through that portal plays the outro, then returns to the main menu. The video fits the viewport with aspect-preserving letterboxing, with all game cameras and UI disabled. All game audio sources are muted and the listener is paused; a dedicated video-audio source is the only sound allowed through. The screen stays black while the first video frame prepares. Download PDF keeps the certificate open. A successfully watched outro is recorded in the current run. Reopening or closing the certificate never starts the video. Video preparation failures retain a way back to the menu or gameplay.

The certificate uses this run's explorer name and original completion date. Exit and Escape resume the cave and assemble a home portal; walking through it opens the main menu. From the main-menu certificate viewer, Exit/Escape returns directly to that menu; completed explorers can reopen the certificate there. On desktop, Download PDF saves a dated PDF in the user's Downloads folder (persistent data folder if Downloads is unavailable) and opens it in the default viewer. The existing WebGL download bridge supports browser PDF downloads. This feedback batch verified the Unity flow and desktop PDF export; it does not include a new browser build. The certificate is a high-resolution rendered page, preserving the game's text layout; it is not a professional qualification.

## Interaction cards

Each interactive helper or lesson object uses one overhead [E] card; artifacts use [E]. Cards follow the current sprite bounds, avoid overlapping neighboring cards and hide during conversations/journals. Automatic portal destinations do not imply an E requirement. The redundant bottom interaction card is disabled. The Garden Warrior stops advertising her finished dialogue, including during same-run chapter revisits; other helpers and lesson targets keep their established interactions.

## Graphics and animation

The generated atlas uses the existing seasonal terrain pack as its visual reference: rounded violet-gray stone, warm ivory highlights, earthy roofs and clustered pixel shading. It adds warm and snowy ranger cottages, a mountain integrated with the cave mouth, and collectible compass shrines. Existing shelter/cottage visuals use the same family. Fall's enlarged placeholder guardian is replaced by a four-pose stone-and-bramble Sentinel atlas. Only authored landmark buildings remain; their front steps connect to trails. Seasonal cottage variants, snow-rooted trees, carved signs and grouped local furnishings replace generic extra-house placement. Generated files and import metadata live under `Assets/Journey/Resources/Journey`.

Water surfaces have moving highlights, lanterns subtly flicker, and existing blossom/leaf/snow effects continue. All UI now uses the supplied Determination font through a baked TMP asset, including menu, notes, choices, controls, collection and certificate. Existing point sizes remain; font leading is normalized and cramped labels are shortened.

Spring, Summer and Winter have new seasonal guardian atlases; the final Echo Keeper is a larger ancient AI robot. Two idle poses and distinct one-shot windup/impact poses respond to encounter events. Fall's existing Sentinel is unchanged. See `Bosses.md`.

Companions retain a safe route spine but use different gaps and slowly varying lateral offsets. Obstacle checks pull the formation back into narrow passages; teleports reset its route.

## Performance changes

- Hardware VSync divides common display refresh rates down to at most 60 FPS: 60→60, 90→45, 120→60, 144→48, 165→55, 240→60. Other refresh configurations fall back to a 60 FPS software target. This follows Unity's preference for hardware frame pacing on desktop.
- Background running is disabled during ordinary gameplay. Video playback temporarily keeps rendering through a fullscreen focus change, then restores the previous background setting.
- The player arrow points directly to the objective and does no route searches. Companion escorts still use bounded, cached route checks; formation clearance is sampled rather than allocating cast results every frame.
- Builds open in a 1280×720 window and use a non-development release build. Unity Editor testing still has Editor overhead.

These changes address identifiable unnecessary work. They do not establish the temperature on the teammate's laptop; a standalone build should still be compared on that hardware.

## Research and asset choices

- [A Short Hike](https://ashorthike.com/): marked routes, backcountry exploration and optional discoveries informed the landmark/collectible loop.
- [TUNIC](https://tunicgame.com/): exploration, lost knowledge and staged discoveries informed the final-cave structure. No game art was copied.
- [Unity frame-rate documentation](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Application-targetFrameRate.html): desktop VSync and target-frame-rate behavior.
- [Kenney Tiny Town](https://kenney.nl/assets/tiny-town) and [Tiny Dungeon](https://kenney.nl/assets/tiny-dungeon): reviewed as CC0 alternatives. Their 16×16 style is simpler than the existing terrain; this update uses referenced original artwork and the team's existing pack to preserve visual consistency. No additional third-party pack was imported.

## Verification

The pure .NET suite covers all 32 Summer, 243 Fall, 243 Winter and 256 final-activity pass/fail combinations, plus clean independent runs, in-memory chapter data, bounds and name handling. The Unity integration suite must cover real seasonal portals, quiz retries, UI fitting, pause/end-run confirmation, same-run revisits, all twelve E-key pickups, certificate export and a clean New Journey. `TransitionArrivalChecks.Run` separately checks active-scene player selection and canonical arrivals without borrowing the outgoing player.

Run the existing .NET project, `python3 tests/check_assets.py`, and Unity's `SeasonIntegrationChecks.Run`. Runtime captures and generated sample PDFs are written under `capstone/Logs/SeasonChecks` and are not source assets. `SeasonIntegrationChecks.Build` creates the release Mac application there.

## Homepage

The blossom-valley homepage follows the supplied visual reference. New journeys begin at the name field, then play the supplied intro and enter Spring. Settings explains always-on guidance. There is no returning-player Continue button or saved-season card. Ending an unfinished run requires confirmation. Completed explorers can reopen their certificate until a new journey or reload. Quit exits a standalone player; a browser build explains that the tab can be closed. The supplied intro/outro clips remain teammate-owned.


See [second-round feedback changes](SecondUserTesting.md) for the removed R shortcut, shared boss crumble, animated seasonal exits and cave-to-menu home portal.

## Spring controls journal

First-time explorers see a large two-page controls book at Spring spawn, matching the pause journal frame. Pixel key diagrams show WASD, arrow keys, E, 1/2, J, I and Esc. The guide explains the arrow beside the player and directs them to the Blossom Grove field note. Closing the controls automatically starts the Mage orientation caption; no second conversation is required. E, Enter, the button or Esc dismisses it; the current-run dismissal flag prevents repeats on chapter revisits.

## Boss encounter music

The supplied `atlasaudio-battle-drums-636771.mp3` loops during the Spring Blossom Guardian, Summer Suncrest Guardian, Fall Sentinel, Winter Frostbound Warden and Echo Keeper conversations. It temporarily pauses the seasonal background track and resumes it when the conversation closes. Global mute and pause apply to the battle track. It uses the same streaming import settings as the other seasonal music.

Focused Unity checks verified all five conversations, looping, background-music replacement/restoration, mute and pause/resume. A separate ending check verified certificate close → home portal reveal → physical portal entry → final video → main menu.
