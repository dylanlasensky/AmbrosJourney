# Journey completion update

## Play and save

Open `capstone` in Unity **6000.6.2f1**, open `Assets/Opening/Intro.unity`, and press Play. The opening scene now starts with a name-entry menu. Play begins the existing opening clip; its completion loads Spring. Continue skips the opening and restores the last saved location.

Pause is always available on the right side of every season, or with **Esc**. It stops physics, animation and audio, and offers Resume, Save and return to menu, artifacts, guidance, reset, and travel to unlocked seasons. The same profile has one checkpoint per season. Travel lets players recover missed collectibles without restarting lessons. Returning to a checkpoint on a portal does not immediately send the player through it: step away first.

Saves contain the explorer name, exact player coordinates, Winter outdoor/cave region, completed and partial lessons, boss progress, collected artifacts and completion date. Returning from a menu closes a conversation; an unfinished boss encounter can be restarted without losing completed lessons. Save and return captures the current position immediately; normal exploration also autosaves every five seconds. One profile is stored locally on each device, in `Application.persistentDataPath/ambros-journey-v1.json`. There is no cloud account or name transmission. Atomic replacement retains a `.bak` recovery copy. Corrupt, oversized or invalid saves are rejected. Reset has a confirmation screen and removes the primary, backup and pending save, then returns to name entry.

## Exploration and learning

- Spring field note 2 is at **(7, 1)**, on the approach to the lakeside Myth Bat at (10, 6).
- Three artifacts are placed in each season. Approach and press **F**. The HUD and collection screen track all twelve. Completed seasons remain available from Pause, so none is permanently missable.
- Completing the collection awards **Evidence Keeper** and reveals the Evidence Compass lesson: agreement between AI tools is not independent corroboration. The final quiz includes this lesson.
- Fall and Winter use broad bearings at long range in **Explorer** mode. Winter snow periodically obscures the compass outdoors, and snowdrifts partly cover the northern trail. Landmarks, lanterns and signposts provide clues. **Pause → Guidance: Assisted** restores exact distance and route guidance, including during snowfall.
- The **Frostbound Warden** stands immediately outside Echo Cavern. Complete the two outdoor evidence stations, then score **4/5** to earn the Winter crest and recruit Little Mage. The cave stays closed until this encounter is passed.
- Inside the cave, complete the Echo Archive to open the final hall. The **Echo Keeper** asks eight questions spanning all four seasons and the artifact lesson. **7/8** passes. Feedback follows every answer; retries preserve lessons, crests and collectibles.

## Teammate video handoff

No intro or outro video was created or modified by this update.

**Intro:** replace the existing `Assets/Opening/Opening.mp4` while keeping its `.meta`, or assign the new VideoClip to `OpeningCinematic.clip` in `Assets/Opening/Intro.unity`. The menu calls `OpeningCinematic.Begin()` after a valid name is entered. Playback completion loads Spring. A playback failure displays an Enter-to-continue fallback.

**Outro:** add the teammate's clip at **`Assets/Resources/Journey/Outro.mp4`**, commit its `.meta`, and rebuild. The final victory continuation loads `Resources.Load<VideoClip>("Journey/Outro")`, plays it using unscaled time while gameplay is paused, then presents the certificate. A missing/unplayable clip goes directly to the certificate, so an unfinished video does not trap the player. Do not put the outro in the opening folder without also updating this resource path.

The certificate uses the explorer name and completion date. On desktop, Download PDF saves a dated PDF in the user's Downloads folder (persistent data folder if Downloads is unavailable) and opens it in the default viewer. A WebGL download bridge is included, but browser-build behavior has not been tested on this Mac. The certificate is a high-resolution rendered page, preserving the game's text layout; it is not a professional qualification.

## Graphics and animation

The generated atlas uses the existing seasonal terrain pack as its visual reference: rounded violet-gray stone, warm ivory highlights, earthy roofs and clustered pixel shading. It adds warm and snowy ranger cottages, a mountain integrated with the cave mouth, and collectible compass shrines. Existing shelter/cottage visuals use the same family. Fall's enlarged placeholder guardian is replaced by a four-pose stone-and-bramble Sentinel atlas. Additional cottages are placed only where their footprint is clear; paths and mission locations remain usable. Generated files and import metadata live under `Assets/Journey/Resources/Journey`.

Water surfaces have moving highlights, lanterns subtly flicker, and existing blossom/leaf/snow effects continue. Companions retain a safe route spine but use different gaps and slowly varying lateral offsets. Obstacle checks pull the formation back into narrow passages; teleports reset its route.

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

The expanded pure .NET suite covers all 32 Summer, 243 Fall, 243 Winter and 6,561 final-boss answer combinations, plus save round trips, corrupt-primary recovery, reset, bounds and name handling. The Unity integration runner traverses the actual four-season portals, checks mission/boss retries and UI text at 1280×720 and 1024×768, follows routes to every artifact, collects all twelve with F-key input, verifies outdoor and cave checkpoint restoration, exports a personalized PDF, resets the profile, and checks that the opening video plays before fresh Spring.

Run the existing .NET project, `python3 tests/check_assets.py`, and Unity's `SeasonIntegrationChecks.Run`. Runtime captures, test saves and generated sample PDFs are written under `capstone/Logs/SeasonChecks` and are not source assets. `SeasonIntegrationChecks.Build` creates the release Mac application there.
