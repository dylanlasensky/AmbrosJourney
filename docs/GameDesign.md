# Game design and team handoff

This is the current design/implementation handoff. Dated validation reports remain evidence of their own snapshots; see [documentation index](README.md).

## Story and audience

Ambros, represented by the Chibi Assassin character, is pulled into a game while using his laptop. Four seasons teach middle-school players to understand AI, keep their own judgment, ask useful questions and check evidence. The Ancient Mage is another trapped child who has grown old. Summer's married Warriors, Fall's Emberfist and Winter's Little Mage provide the later lessons.

The teammate owns the final intro/outro videos. The generated placeholder intro and its tools were retired. Until the intro is supplied, name entry leads straight into Spring. The current final victory goes to the outro/certificate flow; the earlier proposal for a required interactive return to Spring is not implemented.

## Progression contract

| Stage | Teaching and application | Guardian threshold | Companion/reward |
| --- | --- | --- | --- |
| Spring | Meet Mage; three landmark notes; six true/false claims about training, humanlike responses, verification and privacy | Blossom Guardian, 4/5 | Apprentice; Ancient Mage |
| Summer | Useful tool choices; hint then Sun > Seed > Leaf practice; permitted draft feedback; fictional privacy redaction and human support | Suncrest Guardian, 4/5 after three wards | Independent Thinker; both Warriors |
| Fall | Goal/context/format; audience and permitted hints; specific revisions and factual limits; seven choices across three notes | Existing Bramble Sentinel, 4/5 after three runes | Prompt Pathfinder; Emberfist |
| Winter outdoors | Original sources, source context, independent evidence and current local information; four choices after two notes | Frostbound Warden, 4/5 | Winter crest; Little Mage; cave access |
| Cave/finale | Echo Archive uncertainty/correction note and two choices; eight new cumulative AI safety activities | Echo Keeper, 7/8 | Journey complete; optional outro; personalized PDF |

Teach before testing. Spring is approachable; later scenarios require applying concepts and distinguishing plausible shortcuts. Wording is concise and conversational, with no em dashes in runtime copy. Explanations should help the player understand a mistake. Mission retries retain completed choices; failed guardians retain missions; depleted courage replenishes. There are no artificial waiting timers.

The final uses multi-selection, drag/click prompt assembly, ordering, matching and one new judgment choice. It applies the lessons in unfamiliar situations rather than repeating earlier questions. The AI-agreement lesson is in Winter's ordinary note as well as the optional collection reward, so final access does not depend on finding every artifact.

## Exploration, art and presentation

Spring targets about five minutes within a roughly thirty-minute full journey; Summer's 5-6 minute estimate and all pacing targets remain unmeasured. Fall's 99 x 93 tiles are about 2.5 times Summer's area. Rivers, branching paths and hedge turns make later navigation more complex. Winter adds snow, unsafe ice, landmark clues and a second map.

Keep routes readable, doorsteps connected and interaction areas clear. The world is whimsical top-down pixel art with natural seasonal scenery and signs of machine takeover. The new Blossom Guardian, Suncrest Guardian and Frostbound Warden use seasonal silhouettes and materials. The final Keeper is a larger ancient AI robot. Fall's existing Sentinel remains unchanged.

All four seasons and menus use Determination, existing point sizes and shared parchment cards. Font leading is normalized without scaling glyphs; short labels are adjusted to fit rather than making the text smaller. Explorer guidance uses broad bearings in distant Fall/Winter areas and Winter whiteouts; Assisted restores exact route guidance. Companions follow a safe breadcrumb spine with varied spacing/offsets and obstacle-aware narrowing.

## Persistent journey

Name entry, Continue, pause, save/return, confirmed reset, unlocked-season travel, twelve artifacts, Evidence Keeper and the PDF certificate are implemented. Saves retain exact positions/region, partial lessons, guardians and collectibles locally with a recovery copy. There are no cloud accounts or live AI calls. One profile is stored per device.

Three artifacts per season encourage revisits. Completing the collection reveals the Evidence Compass lesson about privacy, original sources, independent evidence and uncertainty. These collectibles are optional; the final requires all lessons/crests, not all artifacts.

## Collaboration and outstanding work

Open `capstone/` in Unity 6000.6.2f1. Runtime world builders and Resources prefabs own later seasons; coordinate before rebuilding scenes or assets. Commit required `.meta` files and preserve pack/font licenses. Keep Library, Temp, Logs and player builds out of source control. Legacy `orc` and skeleton fields remain compatibility/source references, not the displayed boss forms.

Sprint Three is due Nov 4; the original weekly schedule remains planning context in the root README. The teammate's clips, audience comprehension/difficulty/pacing tests, target-hardware/audio checks, and broader inventory/customization remain outstanding. Implemented features are not proof that the game meets learning or performance goals.

Use [Journey completion](JourneyCompletion.md), [current questions](QuestionRefresh.md), [boss artwork](Bosses.md), [testing](Testing.md) and [current validation](CurrentValidation.md).
