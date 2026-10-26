# Journey update validation

Verified locally on October 26, 2026 with Unity 6000.6.2f1 on macOS.

- **139,898 progression assertions passed**, covering every answer combination for Summer (32), Fall (243), the outdoor Winter Warden (243), and the cumulative final Keeper (6,561).
- Additional save tests passed: Unicode explorer name, exact coordinates and cave-region round trip, recovery from a truncated primary file, full reset including the backup, invalid coordinate rejection and bounded names.
- The live Unity integration passed the actual Spring → Summer → Fall → Winter portals, existing Summer/Fall regression checks, 96 Winter dialogue/quiz states, both new boss thresholds and retries, cave gating, region round trips and route clearance.
- Every artifact was reached by a valid physics route and collected using queued **F-key input**, with achievement state preserved across chapter travel and menu transitions.
- Menu, pause, journal, dialogue, achievement and certificate screens were checked for TMP overflow at **1280×720** and **1024×768**. PNG output was visually reviewed. The certificate PDF was parsed with pypdf and independently rendered with Poppler; the accented sample name and all printed content were legible.
- Exact outdoor and cave location restoration, paused physics/audio/input, Escape-resume without also dismissing a conversation, reset, name validation, and actual intro playback were tested. The intro's preparation timeout resets after a long menu wait.
- The missing-outro path reaches the certificate without claiming that a video played. A real teammate outro was not available for playback verification.
- A Winter route rebuild measured **1.09–2.29 ms**, with 1,056 reusable-buffer physics queries. One hundred calls inside the sampling interval added **zero** physics queries. The configured render ceiling was at most 60 FPS, and background execution was disabled. These are local measurements, not a thermal test of the teammate's laptop.

Reports and runtime screenshots are under `capstone/Logs/SeasonChecks` (ignored). Selected UI previews are in `docs/JourneyScreenshots`. The game-checks workflow continues to run progression/save tests and tracked Unity asset integrity on pull requests. Unity gameplay/build checks run locally because hosted Unity licensing is not configured.

The desktop build is the validated target. The optional WebGL download bridge has not been tested in a browser build. Team playtesting is still needed for first-time pacing, laptop temperatures and the teammate's finished videos.

The non-development macOS application build succeeded with **zero errors**. Unity reported three build warnings; the log is `/tmp/ambros-journey-build.log`.
