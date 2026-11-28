# Game design and team handoff

This is the current design/implementation handoff. Earlier validation snapshots are consolidated in the current results; see [documentation index](README.md).

## Story and audience

Ambros, represented by the Chibi Assassin character, is pulled into a game while using his laptop. Four seasons teach middle-school players to understand AI, keep their own judgment, ask useful questions and check evidence. The Ancient Mage is another trapped child who has grown old. Summer's married Warriors, Fall's Emberfist and Winter's Little Mage provide the later lessons.

The teammate's original 40-second PR18 intro is restored byte-for-byte; the outro remains unchanged. See [video provenance](IntroVideoSource.md). The main menu loops Spring's music at the same volume and stops it immediately after a valid New Journey click. After name entry, a black screen fades in with an AI disclaimer in Determination, holds for seven seconds, then fades into the intro. The note says: "The opening video and some game artwork were created with AI. As you play, you'll learn how to use AI safely, check its answers and bring your own ideas to life." Final victory crumbles the Keeper and drops an E-pickup certificate scroll. Exit/Escape from the certificate returns to the cave and forms a home portal; walking through it plays the outro, then returns to the main menu. The film uses an aspect-preserving screen overlay, with game overlays and other audio suppressed. Browser fullscreen playback still needs the [release checks](CinematicFullscreenChecks.md).

## Progression contract

| Stage | Teaching and application | Guardian threshold | Companion/reward |
| --- | --- | --- | --- |
| Spring | E-to-talk Mage orientation; three landmark notes; six true/false claims about training, humanlike responses, verification and privacy | Blossom Guardian, 4/5 | Apprentice; Ancient Mage |
| Summer | Useful tool choices; practice-garden field note; permitted draft feedback; privacy in AI prompts and human support | Suncrest Guardian, 4/5 after three wards | Independent Thinker; both Warriors |
| Fall | Goal/context/format; audience and permitted hints; specific revisions and factual limits; seven choices across three notes | Existing Bramble Sentinel, 4/5 after three runes | Prompt Pathfinder; Emberfist |
| Winter outdoors | Original sources, source context, independent evidence and current local information; four choices after two notes | Frostbound Warden, 4/5 | Winter crest; Little Mage; completed E-to-talk cave briefing |
| Cave/finale | Echo Archive uncertainty/correction note and two choices; eight new cumulative AI safety activities | Echo Keeper, 7/8 | Keeper disintegration; dropped scroll and E pickup; personalized PDF; Exit to cave, animated home portal to main menu |

Teach before testing. Spring is approachable; later scenarios require applying concepts and distinguishing plausible shortcuts. Wording is concise and conversational, with no em dashes in runtime copy. Explanations should help the player understand a mistake. Mission retries retain completed choices; failed guardians retain missions. There are no artificial waiting timers.

Correct responses explicitly say Correct! before their explanations. Every displayed quiz score keeps its full total: five for seasonal guardians, eight for the final, and the actual per-lesson count. Courage/hearts and their penalties are removed.

The final uses multi-selection, drag/click prompt assembly, ordering, matching and one new judgment choice. It applies the lessons in unfamiliar situations rather than repeating earlier questions. The AI-agreement lesson is in Winter's ordinary note as well as the optional collection reward, so final access does not depend on finding every artifact.

## Exploration, art and presentation

Spring targets about five minutes within a roughly thirty-minute full journey; Summer's 5-6 minute estimate and all pacing targets remain unmeasured. Fall's 99 x 93 tiles are about 2.5 times Summer's area. Rivers, branching paths and hedge turns make later navigation more complex. Winter adds snow, fir groves, landmark clues and a second map.

Keep routes readable, doorsteps connected and interaction areas clear. The world is whimsical top-down pixel art with natural seasonal scenery and signs of machine takeover. The new Blossom Guardian, Suncrest Guardian and Frostbound Warden use seasonal silhouettes and materials. The final Keeper is a larger ancient AI robot. Fall's existing Sentinel remains unchanged.

All four seasons and menus use Determination, existing point sizes and shared parchment cards. Font leading is normalized without scaling glyphs; short labels are adjusted to fit rather than making the text smaller. A bright gold-and-cream, dark-outlined arrow rotates continuously toward the next required destination in every season. It stays available outdoors in Winter and inside the cave; players choose their walkable route. Guides wait silently for E at arrival. Their conversations pause movement and can be closed with Escape; finishing the Summer/Fall greeting starts their walking escort. Lesson boards require E. After Warden victory, complete the Little Mage conversation with E or Enter before entering the cave; Escape leaves that briefing available to reopen. See [companion guidance](CompanionGuidance.md). Companions follow a safe breadcrumb spine with varied spacing/offsets and obstacle-aware narrowing.

## One-sitting journey

Name entry, pause, confirmed end-run, unlocked-season travel, twelve artifacts, Evidence Keeper and the PDF certificate are implemented. Positions/region, partial lessons, guardians and collectibles remain in memory only for this run. Returning to an unlocked season keeps that run's progress. Closing or refreshing the game, or ending the run, starts fresh; previous saves are ignored without deletion. After the outro, the certificate stays available until a new journey or reload. There are no cloud accounts or live AI calls.

Three artifacts per season encourage revisits. Completing the collection reveals the Evidence Compass lesson about privacy, original sources, independent evidence and uncertainty. These collectibles are optional; the final requires all lessons/crests, not all artifacts.

## Collaboration and outstanding work

Open `capstone/` in Unity 6000.6.2f1. Runtime world builders and Resources prefabs own later seasons; coordinate before rebuilding scenes or assets. Commit required `.meta` files and preserve pack/font licenses. Keep Library, Temp, Logs and unpacked generated players out of source control. Packaged deliverables are indexed in the top-level Builds folder. Legacy `orc` and skeleton fields remain compatibility/source references, not the displayed boss forms.

The original Sprint Three deadline was Nov 4; the original weekly schedule remains planning context in the root README. Final Web/itch video and session-lifecycle checks, audience comprehension/difficulty/pacing tests, and target-hardware/audio checks remain outstanding release evidence. Implemented features are not proof that the game meets learning or performance goals.

Use [Journey completion](JourneyCompletion.md), [current questions](QuestionRefresh.md), [boss artwork](Bosses.md), [testing](Testing.md) and [current validation](CurrentValidation.md).

## November 23 feedback polish

Pause and Artifacts sit side by side immediately below the top-right lessons/objective panel in every season. NPC interaction cards use names and appear only while an unread conversation is available; later briefings can bring them back. Seasonal boss victory messages retain the score/rank and a short lesson takeaway, without instructions describing the dissolve or portal animation. Existing victory animation and portal/cave entry rules remain in place.

## Music preference

Music On/Off is in the pause journal and main-menu Settings. The gameplay HUD has no music button. The preference applies across scenes for this application session, while effects and the separate M all-audio shortcut retain their behavior.
