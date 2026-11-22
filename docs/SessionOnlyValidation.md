# Session-only play and season handoffs — 22 November 2026

This update starts every launch and browser refresh with a new journey. Old save files and browser records are ignored and left untouched. The native player save present during validation retained its original SHA-256 hash throughout the checks.

## Behavior

- The main menu has name entry, New Journey, Settings and Quit. Continue, saved-season cards, autosave notices and the storage bridge are removed.
- Pause/Resume keeps the active run. Unlocked seasons can still be revisited during that run for missed artifacts. Ending the run asks for confirmation and clears lessons, locations, artifacts and completion before returning to name entry.
- After the final outro, the earned certificate can be reopened until a new journey or reload. Drafting a new explorer name cannot alter the earned certificate's name/date. PDF downloads remain supported.
- Player lookup selects only the active season's player. Portal fades retire the outgoing scene before activating the destination and block pause, artifacts and competing chapter loads during the handoff.
- Water reflections, lanterns, artwork, trail placement and portal captions use their owning scene. Holding an inactive outgoing scene during testing exposed effects parented to the wrong map and a destroyed-transform error; the ownership fix eliminates that failure.

The transition code could select Spring's departing player while Summer was active, allowing the old save system to associate the Spring exit with Summer. The teammate's actual checkpoint was not available, so this is a confirmed faulty code path and matching regression case, not a claim to have inspected their device.

## Validation on this source

| Check | Result |
| --- | --- |
| Pure progression and session data | PASS: 22,959 progression assertions, plus independent-run defaults, in-memory chapter retention, certificate identity and name/bounds checks. |
| `TransitionArrivalChecks.Run` | PASS: fresh startup before test-profile setup; three held-source overlaps and all three real portal handoffs; canonical spawns; Summer workshop route; current-scene effects; fade input guards. `Logs/TransitionChecks/Results.txt`. |
| `SeasonIntegrationChecks.Run` | PASS: four seasons, both Winter bosses, actual lesson/question flows, guidance/movement, same-run chapter/cave revisits, all twelve E-key artifacts, achievement, PDF export, end-run cancellation/reset and fresh named Spring. Render/text checks at 1280×720 and 1024×768. `Logs/SeasonChecks/Integration.txt`. |
| `BossExitChecks.Run` | PASS: guardian failure/pass thresholds, dissolves, portal assembly, real transitions, final interactive activities, certificate pickup/export, cave Exit/Escape, full native outro and completion menu. New-name draft and Settings do not rename the prior certificate. `Logs/BossExitChecks/Results.txt`. |
| `PauseJournalChecks.Run` | PASS: long/accented names, pause/audio/input, artifact journal, cancellation preserving complete current-run data, all four chapter actions, certificate, and a clean menu after confirming End run. |
| Asset integrity | PASS: 442 tracked GUIDs, gameplay metadata, artwork references, portal resources and build scenes. |
| Storage architecture boundary | PASS: 67 runtime source files; no progress-storage capabilities; the requested PDF export is allowed. This source check does not substitute for runtime tests. |
| Browser helpers | PASS: Web template configuration/loading/error recovery and cinematic focus guards. |

All native test processes exited successfully. Runtime tests were followed by the release build below. The CLI launcher stalled before starting the editor; the installed matching Unity editor successfully performed the export directly. Generated importer/render-setting changes were excluded, preserving the checkout's pre-existing unrelated edits.

## Exact browser export

`capstone/Builds/Web/Submission-20261122-session` uses Unity 6000.6.2f1, release options and gzip. Export: **Succeeded, 0 errors, 6 warnings**. Seven packaged files; ZIP size **119,929,342 bytes**. Independent ZIP CRC/content, all gzip streams and both original video hashes passed. No test profile or test shortcut is packaged.

Chrome checks used the exact export on a separate local iframe origin:

- Before replacement, the older build recognized the disposable trapped-Summer profile and displayed Continue. The new build ignored that same browser record and displayed empty name entry with no Continue.
- New Journey played the original intro. Picture remained visible after using the outer page's fullscreen control. Return closed the Spring welcome and keyboard movement worked afterward.
- E collected Pattern Seed, displaying 1 of 12. Space closed the journal. Pause and cancelling End run retained that artifact.
- Refresh returned to empty name entry with no Continue. A second named run began in Spring with no artifacts and locked later seasons.
- Confirming End run returned to a clean main menu. No browser console errors were recorded during these checks.

The existing optional URP FSR shader warning remains; no missing game effect was observed in these checks. Browser-extension warnings are separate from the game. This export was not uploaded to itch, and this browser run did not repeat the full four-season playthrough, outro or PDF download; those flows have native coverage above. Low-end hardware performance was not benchmarked. The team should repeat fresh-start and fullscreen checks after uploading their chosen build.
