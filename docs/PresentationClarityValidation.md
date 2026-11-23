# Presentation clarity and manual conversations — 22 November 2026

## Changes

- Restored the exact 40-second, 1080p teammate intro from PR18. The later October still-image replacement is no longer selected. Both source video hashes are checked in CI; see [provenance](IntroVideoSource.md). The outro is unchanged.
- NPCs wait for E. Spring's Mage introduction is deliberate again; Summer/Fall escorts begin after the greeting, with task dialogue available on E. Winter's Little Mage briefing must be completed before walking into the cave. Escape cancels that briefing without granting access. Followers remain quiet.
- Removed decorative location, portal and route captions. Nearby action hints use the actual interaction anchor/radius: 4 units for guides, 5.5 for bosses, 2.6–2.8 for notes and 2.3 for artifacts. Guide cards now read `[E] Talk`; their names remain in dialogue and objectives. Hints hide during modals and transitions and avoid the player, HUD and the direction arrow, including its rotation and pulse.
- The old browser template forced one rendered pixel per CSS pixel even on Retina. The new responsive buffer uses up to 2× density, capped at 2560×1440 total pixels. A standard-DPI laptop retains native size unless its viewport exceeds that budget. This is bounded high-DPI rendering, not an unlimited 4K mode. Frame limits and background throttling are unchanged.

The installed TextMesh Pro font is an intentional pixel font with a 2048×2048 signed-distance atlas; the UI uses a 1280×720 reference layout, and the render pipeline uses scale 1. The previous small preview and forced DPR1 could soften presentation. The existing art/style is retained.

The resize implementation follows [Unity 6000.6 canvas sizing](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/webgl/develop/canvas-size): CSS fills the iframe and an observer owns the backing size. Fullscreen, resize and hidden zero-size iframe cases are covered by browser helper tests. A visible, loaded canvas regains focus after an actual resize so an outer fullscreen button cannot leave foreground-only Unity on a cleared frame. Other focused controls, hidden tabs, loading/error states and unchanged sizes are excluded. Pointer alignment and real movie playback were checked in the export below.

## Native and source checks

- Pure progression: 22,959 assertions plus fresh-run/session-data checks passed.
- `SeasonIntegrationChecks.Run`: four seasons, both Winter bosses, deliberate guide conversations, escorts, wrong-answer feedback, direct player guidance, label ranges/clearance (including eight arrow angle/pulse cases per scene), movement, all 12 E pickups, current-run chapter/cave revisits, certificate/PDF and end-run/reset passed. Layouts were rendered and checked for TMP overflow at 1280×720 and 1024×768.
- `TransitionArrivalChecks.Run`: all three real portals and held-source overlap fixtures passed. Every arrival has its canonical spawn and fresh progress, remains quiet while walking up, rejects distant E, opens on nearby E, and returns control with Enter/Escape.
- `BossExitChecks.Run`: all four seasonal quiz thresholds, retry/pass flow, dissolves and real portals, final interactive quiz, cave briefing/exit, certificate PDF and native ending/home flow passed.
- Browser helpers: responsive high-DPI budget/aspect, fullscreen resizing, loading/retry and cinematic focus restoration passed. Asset integrity checks passed 442 tracked GUIDs and the intended teammate video identities. The session-only source guard passed 67 runtime files.
- Early new keyboard checks exposed test-fixture focus/dispatch problems: the batch transition runner lacked a focused Game view, and synthetic Escape initially bypassed the session's global input handler. Test-only corrections now assert delivered key edges and the same-frame Escape guard. A later arrow fixture was isolated from unrelated arrival cards while preserving all visibility assertions. The revisit runner now waits for actual chapter-load completion instead of assuming its four-second delay was sufficient. No runtime behavior was relaxed to make the tests pass. The final full integration rerun passed after these corrections.

## Export and live browser verification

Final output: `capstone/Builds/Web/Submission-20261122-reviewed`, made with Unity 6000.6.2f1 in release mode. Build report: success, 0 errors, 6 warnings. The log includes a TextMesh Pro shader deprecation warning. The build report records 113,713,016 bytes; the complete seven-file package is 152,461,322 bytes including the separately streamed movies.

ZIP: `Submission-20261122-reviewed.zip`, 152,044,187 bytes. SHA-256: `da842e8c52595af662c690362da6b8a06bb91b85aa106f4d3c6140d22011bbb5`. ZIP CRC, exact archive/folder contents, gzip streams and both exported movie hashes passed. Generated test exports are ignored by Git.

Local Chrome checks used an iframe wrapper on ports 8771/8772. The initial export exposed a blank gameplay frame when the parent's fullscreen button took focus; the guarded resize-focus correction fixed it. Both movie playback and gameplay/pause then redrew on fullscreen entry without an extra canvas click. The final reviewed export was loaded again for a fresh opening, fullscreen handoff and Spring E conversation. The nearby talk prompt and arrow were both visible after the clearance fix.

- Real Retina display: a 1470×745 CSS viewport used a 2697×1366 backing buffer, within the pixel budget. Embedded 642×540 used 1284×1080. A temporary standard-DPI 1280×720 check used a matching 1280×720 buffer; the viewport override was then removed.
- Menu settings, explorer-name input and buttons remained aligned with mouse input. The movie remained visible on fullscreen entry/exit. The welcome screen handed control to the game, movement worked, the nearby Mage stayed quiet until E, and Escape closed the conversation.
- Source/native tests cover all four seasons; the browser smoke check covers the menu, intro, Spring interaction and fullscreen path. It is not a complete manual four-season browser playthrough.

No itch deployment is part of this change. Local Chrome/native checks do not certify the hosted upload, all browsers, low-end hardware performance or first-time-player comprehension.
