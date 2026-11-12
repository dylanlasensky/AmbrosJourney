# Current combined validation

Checked November 12, 2026 using Unity **6000.6.2f1** on macOS, for the user-testing feedback branch based on main `2fd73ea`. Earlier dated reports describe their original snapshots.

## Passed checks

- **22,652 progression assertions:** all 32 Summer, 243 Fall and 243 outdoor Winter boss answer combinations; all 256 pass/fail combinations of the eight final activities; prerequisites, wrong-answer retries, retained lessons, invalid responses and passing thresholds. Courage/heart fields and penalties have been removed.
- **Save compatibility:** old Spring/Summer/Fall/Winter checkpoints containing the removed fields remain readable. Save round trips, recovery, reset, names and bounds pass. Valid Winter outdoor positions stay exact; old checkpoints behind the newly sealed ridge recover at the cave approach without resetting lessons.
- **Connected Unity playthrough:** Spring > Summer > Fall > Winter using real portals; both Winter bosses; mission/boss retries; menus, pause, exact outdoor/cave saves, revisits, all twelve artifacts, certificate export, reset and a fresh journey.
- **Focused UI/collision checks:** explicit Correct feedback and fixed quiz totals in all chapters and the finale; wrong answers retain the same denominator; no courage/hearts text on screen; 0/12 and 12/12 Artifacts labels fit; real player-body collisions block the ridge sides and rear while leaving the automatic doorway usable; Little Mage has no interaction or label inside the cave; Activity 5 explains repeated clicks and its boxes cycle options; certificate Return and Escape open the main menu and retain completion.
- **Render/text checks:** 1280x720 and 1024x768. The artifact button is slightly wider to keep its existing font size. Correctness feedback retains each question's explanation and the established UI styling.
- **Earlier changes in this feedback batch:** Winter's arrival/cave NPC positions and automatic entry; the empty-name gold pulse; one overhead [E] label per interactive target with no duplicate bottom prompt; moving/hidden character label handling; retiring the Garden Warrior's completed dialogue label and remembering it in saves. Focused checks for these changes passed before the general update, and the combined journey still passes.
- **Asset integrity:** tracked gameplay metadata, unique GUIDs, artwork/resource references and build scenes pass. No new package, font, external service or generated-art dependency is required for this feedback batch.
- **Winter compass notice:** the real snowfall trigger, saved once-only flag, four progress-specific hints, rendered icicle mesh, text at both resolutions, Continue/Escape dismissal and normal dialogue restoration pass. The notice does not interrupt pause/conversations and does not appear for Assisted guidance, inside the cavern or after journey completion.

## Reproduction

[Testing](Testing.md) lists the pure checks and connected Unity suite. Run `GeneralFeedbackChecks.Run` through Unity's `-executeMethod` in a **disposable project copy** with `-batchmode` to reproduce the focused checks. It uses `Logs/SeasonChecks/general-feedback-save.json`, renders review images and writes `Logs/SeasonChecks/GeneralFeedback.txt`, then exits. The method rejects non-batch use so it cannot be started accidentally in a working Editor. `CompassNoticeChecks.Run` independently verifies the real snowfall event and icy panel using its own save and `Logs/SeasonChecks/CompassNotice.txt`.

These results describe automated behavior and render checks. The new feedback batch has not had a complete human playthrough or a new standalone build; the previous successful standalone build remains recorded in the older validation history. Teammate intro/outro clips remain absent; the retained outro player is not wired into the current scroll-pickup path.
