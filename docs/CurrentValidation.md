# Current combined validation

Checked October 28, 2026 using Unity **6000.6.2f1** on macOS, based on merged commit `206464b` plus the question/font/finale/boss refresh. This is the current result; older reports are dated snapshots.

## Passed checks

- **22,927 progression/save assertions:** all 32 Summer, 243 Fall and 243 outdoor Winter guardian answer combinations; all 256 pass/fail combinations of the eight mixed final activities; prerequisites, invalid/incomplete responses, duplicate selections, retries, thresholds, courage recovery, save recovery, reset and bounded names/positions.
- **Connected Unity playthrough:** Spring > Summer > Fall > Winter through the actual portals, including completion/proximity/modal guards and locked/unlocked physical routes.
- **Seasonal boss art:** Blossom Guardian, Suncrest Guardian, Frostbound Warden and robot Echo Keeper each loaded the correct atlas, had two idle poses and distinct windup/impact poses, accepted the encounter strike signal, and produced four in-world pose renders. Selected idle/impact images are in `BossScreenshots/`.
- **Fall preservation:** the existing Sentinel atlas is byte-for-byte unchanged (Git blob `c254a020c8bc7a7f7fe1d1988b95392da6476271`); Fall's boss-specific world/actor and JourneyWorld art setup are unchanged. Fall question wording is part of the separate lesson refresh.
- **128 Winter/finale UI states:** six evidence choices, incorrect answers and retries, outdoor Warden 3/5 failure and 4/5 pass, cave access/return, all eight new activities, actual uGUI begin-drag/drag/drop/end-drag handlers, a 6/8 final failure and 7/8 success.
- **Text/render checks at 1280x720 and 1024x768:** all seasonal interfaces, new activity types, menus, collection and certificate. Determination retains the existing point sizes; only excessive font leading and cramped copy were adjusted.
- **Journey systems:** menu callbacks, pause, Escape-resume guards, exact outdoor/cave saves and restoration, chapter revisits, valid routes and F-key collection for all twelve artifacts, Evidence Keeper, PDF export, confirmed reset, and fresh Spring startup while the teammate intro is absent.
- **Standalone macOS release build:** succeeded with **0 errors and 3 existing build warnings**. Intro/Spring are build scenes and later chapters, fonts and boss atlases are retained through Resources. The output is separate from the earlier running preview.

The integration profile is explicitly resumed at setup because a background Editor can auto-pause before test mode starts. Tests use their own isolated save path and project copy, so resets/captures/build serialization do not replace the player's saved journey or unrelated live project settings.

## Scope and remaining review

Generated art uses the existing Fall guardian as style reference. Original licensed packs, the player's progress format, scoring thresholds, interaction positions, package manifests and build-scene configuration are retained. No live AI/image service is used during gameplay. The user's supplied Determination font and generated font asset are included with metadata.

These checks establish controlled grading, interaction, rendering and packaging behavior. They do not replace a complete human playthrough of the new standalone build, first-time learning/pacing tests, audio/accessibility review or target-laptop performance measurements. The final teammate intro/outro clips remain absent; the missing-video fallbacks are tested, not a finished teammate video.

Reproduction: [Testing](Testing.md). Current learning copy: [QuestionRefresh](QuestionRefresh.md). Boss sources/prompts: [Bosses](Bosses.md).
