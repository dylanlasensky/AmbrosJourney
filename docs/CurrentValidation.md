# Current combined validation

Checked November 20, 2026 using Unity **6000.6.2f1** on macOS, for the second-round feedback changes based on main `865a5c7`. Earlier dated reports describe their original snapshots.

## Passed checks

- **Second-round boss/portal flow:** R no longer moves any player controller. All four seasonal bosses lose their flags and crumble after the results close. Spring, Summer and Fall build their exit portals from pixels; Winter retains its natural cave and requires finishing the Little Mage briefing before automatic entry. Gates reject entry during the effects. The final interactive quiz still drops the certificate; Download PDF works; Exit/Escape resumes the cave and builds the home portal; walking through it opens the menu. Saved completed portals restore without replaying the sequence. See [implementation and reproduction](SecondUserTesting.md).

- **22,895 progression assertions:** all 32 Summer, 243 Fall and 243 outdoor Winter boss answer combinations; all 256 pass/fail combinations of the eight final activities; prerequisites, wrong-answer retries, retained lessons, invalid responses and passing thresholds. Courage/heart fields and penalties have been removed.
- **Save compatibility:** old Spring/Summer/Fall/Winter checkpoints containing the removed fields remain readable. Save round trips, recovery, reset, names and bounds pass. Valid Winter outdoor positions stay exact; old checkpoints behind the newly sealed ridge recover at the cave approach without resetting lessons.
- **Connected Unity playthrough:** Spring > Summer > Fall > Winter using real portals; both Winter bosses; mission/boss retries; menus, pause, exact outdoor/cave saves, revisits, all twelve artifacts, certificate export, reset and a fresh journey.
- **Focused UI/collision checks:** explicit Correct feedback and fixed quiz totals in all chapters and the finale; wrong answers retain the same denominator; no courage/hearts text on screen; 0/12 and 12/12 Artifacts labels fit; real player-body collisions block the ridge sides and rear while leaving the automatic doorway usable; Little Mage has no interaction or label inside the cave; Activity 5 explains repeated clicks and its boxes cycle options; certificate Exit/Escape resumes the cave, forms the home portal, and retains completion; the portal opens the main menu.
- **Render/text checks:** 1280x720 and 1024x768. The artifact button is slightly wider to keep its existing font size. Correctness feedback retains each question's explanation and the established UI styling.
- **Earlier changes in this feedback batch:** Winter's arrival/cave NPC positions and automatic entry; the empty-name gold pulse; one overhead [E] label per interactive target with no duplicate bottom prompt; moving/hidden character label handling; retiring the Garden Warrior's completed dialogue label and remembering it in saves. Focused checks for these changes passed before the general update, and the combined journey still passes.
- **Asset integrity:** tracked gameplay metadata, unique GUIDs, artwork/resource references and build scenes pass. No new package, font, external service or generated-art dependency is required for this feedback batch.
- **Winter compass notice:** the real snowfall trigger, saved once-only flag, four progress-specific hints, rendered icicle mesh, text at both resolutions, Continue/Escape dismissal and normal dialogue restoration pass. The notice does not interrupt pause/conversations and does not appear if Assisted was selected before the freeze, inside the cavern or after journey completion. Once shown, it permanently hides the outdoor arrow until the cave; leaving the cave hides the arrow again.

## Reproduction

[Testing](Testing.md) lists the pure checks and connected Unity suite. Run `GeneralFeedbackChecks.Run` through Unity's `-executeMethod` in a **disposable project copy** with `-batchmode` to reproduce the focused checks. It uses `Logs/SeasonChecks/general-feedback-save.json`, renders review images and writes `Logs/SeasonChecks/GeneralFeedback.txt`, then exits. The method rejects non-batch use so it cannot be started accidentally in a working Editor. `CompassNoticeChecks.Run` independently verifies the real snowfall event and icy panel using its own save and `Logs/SeasonChecks/CompassNotice.txt`.

These results describe automated behavior and render checks. The second-round changes await the requested human playthrough and have no new standalone or WebGL build; the previous successful standalone build remains recorded in the older validation history. Teammate intro/outro clips remain absent; the retained outro player is not wired into the current scroll-pickup path.


## Winter follow-up verification

`WinterRefinementChecks.Run` passed the real snowfall notice, arrow hiding through clear weather/settings/reload, arrow restoration inside the cave and hiding outside again, natural cave with no added portal/build effect, Warden disintegration, required finished Little Mage dialogue (including dismissal/reload cases), automatic entry afterward and old cave-save compatibility. The updated plain-C# suite passed 22,895 progression assertions and legacy save checks.
