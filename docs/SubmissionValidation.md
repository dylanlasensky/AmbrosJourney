# Submission changes and validation — 21 November 2026

This repository update is prepared for PR review. The team will upload the game to itch; no deployment or full manual browser completion is claimed here.

## Changes

- The brighter 52-pixel gold-and-cream arrow has a dark outline, one broad tip and a small, slow pulse. It rotates smoothly toward the next objective directly; the player chooses the walkable route around scenery.
- Overhead name cards reserve clearance around Ambros so nearby labels leave the player sprite visible.
- Guide advice is passive while movement remains available. Summer/Fall escorts wait for the player, and long frames cannot push a leading guide beyond the five-unit waiting radius or sideways across an obstacle.
- Winter's frozen pond is replaced by loose fir groves around a snowy walking gap. Extended ground and viewport-aware camera bounds remove the visible outdoor edge. The existing cave, Warden and final-boss progression remain connected.
- All 52 playable questions/activities, three unused Spring claim strings, 12 note lessons and guide/artifact lessons received an editorial AI-content review. Wording distinguishes training, verification, privacy, useful requests and human responsibility. See the [item-by-item audit](QuestionRefresh.md).
- Films use a screen overlay with aspect-preserving letterboxing. Visible iframe focus loss no longer suspends their picture. Normal completion restores canvas focus before returning to gameplay/menu; the original clips remain intact. Browser loading failures offer retry. See [cinematic checks](CinematicFullscreenChecks.md).
- Performance changes reduce repeated object searches, UI navigation allocations, texture CPU copies and unnecessary effect updates. These are implementation changes, not measured low-end hardware gains.

## Recorded checks

| Check | Result and scope |
|---|---|
| Final native integration | **PASS.** All four seasons, both Winter bosses, menus, save/restore, artifacts, certificate and reset; includes grove placement, camera bounds and guide slow-frame assertions. Report: `capstone/Logs/SeasonChecks/Integration.txt`; run log: `ambros-final-verified-integration.log` (`SEASON_INTEGRATION_PASS` at line 2197). |
| Native visual inspection | Winter arrival and woodland survey captures show the fir grove/walking gap and no exposed outdoor map edge. `SummerInterface1280` shows Ambros unobscured by nearby name cards. |
| Pure progression | **22,959 assertions passed** against the latest source on November 21. This does not replace runtime rendering/input tests. |
| Staged asset integrity | **PASS: 442 tracked GUIDs**, covering gameplay metadata, artwork references, portal resources and build scenes. |
| Browser helper tests | **PASS.** Actual save-bridge reload/reset/denied-storage behavior, template loading/error recovery, and cinematic focus guards. Included in Game checks CI; a local pass is not a hosted-CI result. |
| First release Web export: `Submission-20261121` | **Succeeded: 0 errors, 6 warnings.** ZIP/gzip integrity, generated URLs and original MP4 hashes passed. This build preceded the focus/map follow-ups. |
| First-build Chrome iframe checks | Full approximately 34-second intro remained visible in outer-control fullscreen and reached Spring. Outro played fullscreen above UI and returned to the menu with View Certificate. No game errors observed. |
| First-build browser persistence and certificate | Refresh restored exact position and the first field-note state. PDF download verified: **380,516 bytes**. |
| Second release Web export: `Submission-20261121-final` | **Succeeded: 0 errors, 6 warnings.** Report: `Submission-20261121-final.build.txt`; run log: `ambros-web-final-build.log`. This build includes the focus fix but precedes the last name-card clearance adjustment. |
| Second-build intro keyboard handoff | **PASS in native Chrome.** Entered fullscreen with the outer button during the film; after it completed, Return closed Spring's welcome and S moved Ambros, with no intervening canvas click. |
| Second-build outro keyboard handoff | **PASS in native Chrome.** Restored the local Winter checkpoint, walked into the home portal and entered fullscreen with the outer button during the film. Frames continued to the main menu; Return activated Continue and restored Winter, with no intervening canvas click. |
| Reviewed release Web export: `Submission-20261121-reviewed` | **Succeeded: 0 errors, 6 warnings.** ZIP CRC and contents, all three gzip streams, generated URLs and both original MP4 hashes independently passed. Seven files; ZIP **119,928,550 bytes**. No local test fixtures included. Run log: `ambros-web-reviewed-build.log`. |

Music suppression is covered by native assertions and code inspection. These browser runs did not include a system-audio recording or listening check.

The recurring URP warning names an unsupported/stripped optional FSR upscaling pass. The current pipeline uses render scale 1 and Automatic filtering, and does not enable camera post-processing. URP checks that optional shader while constructing its passes even when FSR is unused. No missing game effect was identified; package shaders/settings were left unchanged.

## Reviewed-export checks

Target export: `capstone/Builds/Web/Submission-20261121-reviewed`, including the last name-card clearance adjustment. The checks above remain scoped to their named builds. Results from this exact export:

- **PASS — reviewed release export:** build report and independent output-integrity checks recorded above.
- **PASS — intro and keyboard handoff:** used the outer fullscreen button during the film. After it completed, native Return closed Spring's welcome and S moved Ambros without a canvas click. A black capture immediately after resize was transient; the next capture showed advancing video without input.
- **PASS — reset persistence:** confirmed Reset Journey in the game, then used the parent page's Refresh Game control. The menu returned with an empty name and no Continue or Certificate option.
- **PASS — checkpoint refresh:** after reading Blossom Grove and pausing with Escape, refreshed the game. Continue returned beside the same board at the same visible location; the note remained read, the HUD and arrow targeted Myth Bat 1, and neither intro nor welcome replayed. Location was checked visually, not by extracting numeric coordinates.
- **Outro scope:** fullscreen picture and no-click menu handoff passed on `Submission-20261121-final` as recorded above. The reviewed export only changes name-card clearance; the unchanged outro was not replayed on this third build. Music isolation remains a native/code assertion, not a listening result.

The pre-outro browser fixture came from a native checkpoint and passed current C# save validators. It tests restoration and movie playback, not completion of every browser lesson. Fixture tools remain outside the repository/build and do not access the real itch save. Local browser tests do not establish performance on a teammate's low-end laptop. Hidden-tab behavior and additional target browsers remain part of the [browser check procedure](BrowserBuildChecks.md). Repeat the critical checks on the actual itch upload after the team publishes it.
