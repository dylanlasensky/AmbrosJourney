# Cinematic fullscreen checks

The original teammate intro/outro play through JourneyVideoScreen with aspect-preserving letterboxing. JourneyCinematicIsolation suppresses other cameras, canvases and audio while a film is active. Video playback temporarily keeps rendering during outer-iframe fullscreen focus changes, then restores gameplay's background-pause policy. Completion requests focus for the visible game canvas without stealing focus from hidden pages or browser form controls.

## Repeatable regression procedure

1. Export a release Web build using the original video identities in [IntroVideoSource](IntroVideoSource.md). Serve over HTTP with gzip headers and MP4 range support.
2. In an iframe, start a named journey. While the intro is moving, use the parent's fullscreen button without clicking the canvas afterward. Resize, exit and re-enter fullscreen. Check continuous picture/sound and the handoff to Spring keyboard movement.
3. Complete the certificate/home-portal flow and repeat during the outro. Confirm no game overlays or exploration music cover the film and that the main menu works afterward.
4. Test tab hiding/return, paused gameplay, missing-media fallback and a fresh run after reload. Repeat on the uploaded itch build; local/native passes do not prove hosted browser behavior.

CinematicPresentationChecks.Run checks video texture/layout and lifetime behavior; CinematicIsolationChecks.Run checks UI/audio isolation; tests/check_cinematic_focus.cjs exercises browser focus guards with mocks. Their scope differs from watching/listening to the actual hosted release. See [current results](CurrentValidation.md) and [Web build procedure](BrowserBuildChecks.md). Earlier browser captures may show the retired placeholder film and are not evidence for new exports.
