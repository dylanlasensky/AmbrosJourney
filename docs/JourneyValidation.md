> **Dated validation snapshot.** Counts, captures and limitations below describe the version tested at that time. For the current bosses, Determination UI, mixed finale, persistent saves and latest results, see [current validation](CurrentValidation.md) and [documentation index](README.md).

# Journey update validation

Verified locally on October 26, 2026 with Unity 6000.6.2f1 on macOS.

- **139,898 progression assertions passed**, covering every answer combination for Summer (32), Fall (243), the outdoor Winter Warden (243), and the cumulative final Keeper (6,561).
- Additional save tests passed: Unicode explorer name, exact coordinates and cave-region round trip, recovery from a truncated primary file, full reset including the backup, invalid coordinate rejection and bounded names.
- The live Unity integration passed the actual Spring → Summer → Fall → Winter portals, existing Summer/Fall regression checks, 96 Winter dialogue/quiz states, both new boss thresholds and retries, cave gating, region round trips and route clearance.
- Every artifact was reached by a valid physics route and collected using queued **F-key input**, with achievement state preserved across chapter travel and menu transitions.
- Menu, pause, journal, dialogue, achievement and certificate screens were checked for TMP overflow at **1280×720** and **1024×768**. PNG output was visually reviewed. The certificate PDF was parsed with pypdf and independently rendered with Poppler; the accented sample name and all printed content were legible.
- Exact outdoor and cave location restoration, paused physics/audio/input, Escape-resume without also dismissing a conversation, reset, name validation, and the former intro playback were tested in PR #6. The world-cohesion update removes that clip and tests direct entry to Spring instead.
- The missing-outro path reaches the certificate without claiming that a video played. A real teammate outro was not available for playback verification.
- A Winter route rebuild measured **1.09–2.29 ms**, with 1,056 reusable-buffer physics queries. One hundred calls inside the sampling interval added **zero** physics queries. The configured render ceiling was at most 60 FPS, and background execution was disabled. These are local measurements, not a thermal test of the teammate's laptop.

Reports and runtime screenshots are under `capstone/Logs/SeasonChecks` (ignored). Selected UI previews are in `docs/JourneyScreenshots`. The game-checks workflow continues to run progression/save tests and tracked Unity asset integrity on pull requests. Unity gameplay/build checks run locally because hosted Unity licensing is not configured.

The desktop build is the validated target. The optional WebGL download bridge has not been tested in a browser build. Team playtesting is still needed for first-time pacing, laptop temperatures and the teammate's finished videos.

The non-development macOS application build succeeded with **zero errors**. Unity reported three build warnings; the log is `/tmp/ambros-journey-build.log`.

## Reference homepage revision (October 26, 2026)

Re-ran the complete Unity integration suite after replacing the homepage with the supplied blossom-valley composition. The final run passed all four seasons, both Winter bosses, artifacts, exact-location recovery, reset, the then-existing intro playback and PDF export. New coverage exercises the actual Continue/New Journey/Settings/Back button callbacks, correct saved-season labeling, reset cancellation, guidance persistence to disk, draft-name retention, and an intentionally unwritable save destination. A failed initial save leaves the profile unstarted and the name field available.

Fresh, returning, settings and completed-game menus passed TMP overflow checks and rendered screenshots at 1280×720 and 1024×768. The compact certificate action is checked for overlap with Quit. Visually reviewed the paintings, aspect-fill crop, title, live controls, input and gold frames. The illustrated background is static and compressed; existing frame limits remain in effect.

## World cohesion update

The updated Unity integration suite passed the four-season journey, both Winter bosses, artifact achievement, PDF certificate, exact-location saves and reset. It now asserts that no placeholder intro clip ships and that New Journey loads Spring without a video error screen. The new map audit checks all eight authored building approaches and Winter's snow-tree sprites, and captures each whole map plus local survey views for visual review. See `WorldCohesion.md` for the placement decisions and review images.

The standalone macOS build succeeded with zero errors. Three remaining build warnings concern an existing navigation API deprecation, TextMeshPro's bundled shader pragma, and unavailable optional Unity Cloud symbol upload. The new object searches use the current Unity overload. The local asset integrity check passes with 357 tracked GUIDs, including a new check for unresolved serialized video clips. GitHub CI also runs the independent progression tests; this does not replace the local Unity gameplay and visual checks.

Manual standalone smoke check: the Blossom homepage loaded, Continue restored the existing Spring save with the new art and cards, movement responded, and Escape opened the pause/save screen. The player log had no exceptions or missing-resource errors during this check. The saved journey was preserved.
