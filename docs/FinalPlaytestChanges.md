# Final playtest changes

This pass implements the team's November playtest feedback. It preserves the existing story, supplied intro/outro videos, and saved progression format.

| Requested change | Implementation |
| --- | --- |
| Replace the bright yellow arrow | `MoonbranchCompass` draws an ivory/silver directional marker with an indigo outline and a seasonal jewel. It still rotates smoothly through 360 degrees near the player. |
| Follow the route along paths | `SeasonNavigation` gives authored `NaturalTrail` cells priority, respects solid obstacles, and points to nearby route waypoints instead of cutting across bends. Collidable destinations use a reachable approach point. |
| Keep guidance visible in Winter | `WinterLevel` no longer hides the arrow or shows the frozen-guidance message; the settings explanation reflects continuous guidance. |
| Put the polar bear in front of the cave | `WinterWorld.WardenPosition` centers the Frostbound Warden on the cave approach. |
| Remove the cave direction bubble | The outdoor “Lanterns lead north then west” label is removed. |
| Keep small ground crystals in the cave only | Outdoor crystal props beside field notes and elsewhere are removed from seasonal world construction and decorative placement. Cave mineral formations remain. |
| Restrict cave sight and light the lanterns | `CaveVisibility` supplies a small, soft-edged player visibility circle and warm pools around cave lanterns. The HUD remains readable. |
| Keep browser progress after refresh | `JourneyStorage` and `JourneyPersistence.jslib` save a synchronous browser checkpoint with a recovery copy, one-second exploration autosaves, and page-hide snapshots. Reset prevents old filesystem saves from returning. |
| Stop music during the outro | `JourneyCinematicIsolation` mutes every non-film audio source, including inactive/new sources, while film audio stays available. |
| Put the outro above all titles and popups | The film has a dedicated full-screen overlay; game canvases and cameras are suppressed throughout playback and restored during cleanup. |
| Replace Summer's target with a note board | `SummerWorld` creates a field-note board in the practice garden and removes the nearby crystal decoration. |
| Improve the lesson by Summer's house | `SummerContent.Message` asks about protecting private details in AI prompts and using real human support rather than having AI guess someone's feelings. |
| Give clear incorrect-answer feedback | All seasonal field notes and bosses explicitly say “Incorrect.” Field-note activities retain the same question until correct. Boss answers are recorded once and advance; a failed attempt restarts the full boss quiz. |

## Verification record

The browser save bridge regression test passed for checkpoint reload, Unicode, recovery copy, corruption, reset, unavailable/quota-limited storage, size bounds, and lifecycle callbacks. GitHub CI includes this test.

Verified locally on Unity 6000.6.2f1, 20 November 2026:

- Four-season integration passed: both Winter bosses, lesson progression, menus, exact outdoor/cave saves, all artifacts, certificate and reset.
- Winter refinements passed: permanent guidance, centered Warden, old-save compatibility, cave entry, visibility circle, six lantern pools and daylight restoration.
- Targeted feedback suite passed, including wrong-answer retry and stale/double-callback rejection.
- Cinematic isolation passed for inactive and newly created overlays/audio and cleanup restoration.
- Pure progression suite passed 22,959 assertions. Browser bridge tests passed. Asset integrity passed with 430 tracked GUIDs.
- Mac player build: Succeeded, Errors: 0, Warnings: 2.
- Inspected 1280x720 and 1024x768 runtime captures. Cave darkness leaves HUD readable; the new ivory marker has a dark outline and distinct tip. The screenshots and detailed local reports are in `capstone/Logs/SeasonChecks` (generated, not committed).

Visual review of touched surfaces: palette 4/5, style 3/5, outline/readability 4/5, player silhouette 4/5, UI 4/5, feedback 4/5. The authored sprite styles remain unchanged; this pass improves guidance, cave visibility and interaction clarity.

These results do not substitute for the final Web/itch smoke test below.

Useful checks: `TrailNavigationChecks`, `WinterRefinementChecks`, `GeneralFeedbackChecks`, `CinematicIsolationChecks`, and the existing season integration, boss exit, journey/save, and progression suites. The exact executed suites and results belong in the release evidence.

## Browser release gate

The locally installed Unity 6000.6.2f1 has Mac build support but no Web build module. A Web build and a live itch.io refresh test have therefore not been verified in this pass. Install matching Web support on the release machine before exporting.

The Web post-build step packages the teammates' unchanged MP4 files in `StreamingAssets/JourneyVideos`, and Web playback uses those URLs because Unity Web cannot play embedded `VideoClip` assets. Include that entire folder in the upload ZIP.

Before submission, on the actual itch page:

1. Start a named journey, move, collect an artifact, wait at least one second, refresh, and choose Continue. Check name, season, location, and artifact count.
2. Repeat from the Winter cave; verify the cave state and restricted sight return correctly.
3. Reset the journey, refresh, and confirm the previous name, lessons, and artifacts do not return.
4. Give a wrong field-note answer and retry it. Give a wrong boss answer and verify it cannot be retried within that attempt; fail and retake the quiz.
5. Complete the home-portal ending with music enabled. Confirm only the film is visible/audible, then confirm return to the menu and certificate access.

Browser checkpoints belong to the same browser profile and game origin, not an online account. Private browsing may discard them; clearing site data removes them. Storage blocking/quota failure displays a save warning. A forced browser/process shutdown can lose the last second of movement. An upload that changes the game's origin or browser storage partition needs another save check.

## References

- [Unity Web video support](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/webgl/develop/video): URL playback and Web audio behavior.
- [Unity persistentDataPath](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/application/persistentdatapath): platform-specific save location.
- [Unity JavaScript plugin interop](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/webgl/develop/interactingwithbrowserscripting/web-interacting-browser-js-to-unity): browser storage bridge and callbacks.
- [MDN localStorage](https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage): persistence, origin scope, and private-browsing restrictions.
- [itch.io HTML upload guide](https://itch.io/docs/creators/html5): iframe hosting, ZIP contents, and relative asset paths.
