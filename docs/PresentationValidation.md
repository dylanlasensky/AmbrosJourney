> **Dated validation snapshot.** Counts, captures and limitations below describe the version tested at that time. For the current bosses, Determination UI, mixed finale, persistent saves and latest results, see [current validation](CurrentValidation.md) and [documentation index](README.md).

# Seasonal presentation validation — October 24, 2026

Verified in the existing Unity 6000.6.2f1 project. Spring remains the reference and its source/scenes are unchanged.

- Summer: 37 dialogue/question/journal states checked through the existing continuation and visible answer-button callbacks. Includes the three-option privacy exercise and completed Orc progression. No active TMP text overflow after shortening the chapter caption.
- Fall: 53 states checked, including every mission, incorrect-answer explanations, all three-choice layouts, a 3/5 guardian failure followed by a 4/5 pass, recruitment and the three-section journal. No active TMP text overflow at the tested Game view size.
- Shared compass: all four cardinal arrow rotations and actual objective distance verified independently of route lookahead. Live collider checks in both seasons reached all three landmarks, checked every path segment against obstacles, and confirmed a visible lookahead waypoint.
- Follower math: 1.8-unit spacing, a right-angle turn, fourth-companion position on the earlier path segment, stationary stability and teleport reset passed. Native movement checks in Fall showed the group following horizontal/vertical movement around a corner. All four companions have walking frames; Mage frames were extracted from the existing pack.
- Native Unity checks exercised E and mouse continuation and inspected the seasonal parchment overlays. Fall now includes popup motion, answer color feedback, player flashes, gold success/rune glints and camera-relative falling leaves.
- Standalone macOS Development build starting in Summer succeeded in 22.4 seconds with **0 errors** and 3 existing font/shader/platform warnings. Fall and shared presentation are retained through Resources. Full manual playthrough of this standalone executable was not performed.
- No added Unity packages. Existing font/render/project settings reserialized by the build were backed up and restored. That presentation pass retained the then-current session-only progress and seasonal rules; persistent saves were added afterward.

Checks can be rerun from **Capstone → Presentation**. Start a fresh Summer or Fall session before that season’s interface check. Live route checks run in either playing season; build/art preparation runs in Edit mode. Results are written to the project’s `Temp/PresentationChecks`. Saved interface results accompany this document.

These checks cover the tested Editor view size, not every display aspect ratio or first-time-player experience. Use Play Fall Only for a clean preview; use Play From Opening for the connected game.
