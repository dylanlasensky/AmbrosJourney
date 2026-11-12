# First-player guidance fixes

## Player-facing changes

- Spring's Mage refers to the compass. Interactive labels in every season show `[E]`, and the Blossom Guardian label shows `[E]` without a score requirement in its name.
- Space and Escape both leave the artifact collection screen. Returning from the pause menu keeps the game paused; opening the collection during play returns to play.
- Both Summer Warriors greet Ambros beside the arrival portal. They lead toward the workshop, wait if Ambros falls behind, and continue the existing explanation at the task. The Garden Warrior then follows the party; her repeated conversation and E label retire once her guidance/read dialogue is finished or the garden is complete. The marked garden target and stones still teach the lesson.
- Emberfist greets Ambros near Fall's arrival portal and leads him to Harvest Orchard before continuing the existing explanation.
- Escort movement pauses with dialogue and menus. It uses collision-aware navigation and saves whether the arrival explanation has started. Existing progressed saves skip the new escort.
- Summer's training target has an interaction label. Sun, Seed and Leaf use narrow labels; the message post says “Ambros’”. Nearby task objects take priority over talking to a companion.
- Decorative letters and unused instruction books/devices are removed. Several outlying baskets, supplies and a sled become seasonal ferns, mushrooms or snow-covered rocks. Settlement furnishings and real lesson objects remain.

- All interactive helpers, notes and challenges use a single overhead [E] card. Artifacts retain [F]; automatic portals remain walk-through destinations. The bottom duplicate interaction card is hidden, while controls remain.
- Winter's Mage is clear of the arrival portal; the Warden leaves the cave visible. After the Warden, Little Mage offers the cave briefing outside, the doorway enters automatically, and his cave interaction is disabled.
- Empty name entry pulses gold until filled. Correct answers explicitly say Correct!, quiz totals are fixed, courage/hearts are removed, and the counter says 0/12 Artifacts.
- The first Explorer whiteout opens an icy notice with the next landmark; the existing temporary weather, Assisted setting and cave guidance remain. Certificate Return/Escape leads to the main menu.

## Validation

The integration suite exercises both escort routes, pause/wait behavior, nearby speakers, continuation at the first task, collection Space/Escape controls and the existing four-season progression. Screenshots include Summer's workshop and Fall's orchard. Full run and standalone build results are recorded in `capstone/Logs/SeasonChecks/` locally.

These are automated checks and screenshot review, not a replacement for a first-time middle-school playtest.
