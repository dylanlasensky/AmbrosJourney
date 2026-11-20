# Browser saves and the supplied videos

Browser builds save the explorer name, unlocked seasons, per-season checkpoint, artifacts and completion to a small `localStorage` record. The last checkpoint is retained as a recovery copy. Saving completes synchronously, once a second while exploring and at existing milestones. Visibility/page-hide events also request a final snapshot; a forced process termination can still lose the last second of movement. Continue restores the checkpoint after a normal refresh.

Saves belong to the same browser profile and game origin. They are not a cloud account and do not transfer between devices. Private browsing may discard them when the private session closes; clearing site data removes them. If a browser blocks storage or runs out of its quota, the game reports that it could not save. Existing in-memory play is kept. Allow storage for the itch page before starting a new journey. Native saves keep their existing filesystem/backup behavior.

The stable game-specific key is `ambros-journey:checkpoint:v1`. Reset writes an empty reset marker so a previous filesystem checkpoint cannot reappear. Valid legacy filesystem checkpoints can still be read before the first browser save. An itch upload that changes the browser origin or partition can change which storage is available; verify the uploaded version on the actual page.

Unity Web does not support embedded VideoClips. `JourneyWebVideoBuild` copies the team's unchanged Intro.mp4 and Outro.mp4 into the built game's `StreamingAssets/JourneyVideos` folder after a Web build. The Web player uses those relative URLs; native builds still use the original VideoClips. Do not delete StreamingAssets when zipping for itch. The final film suppresses inactive/delayed game canvases, cameras, and every non-film audio source until playback ends.

Validation:
- `node tests/check_browser_save.cjs` exercises the actual JavaScript bridge with a new runtime across reload, recovery, Unicode, reset, corruption and denied/quota failures.
- `CinematicIsolationChecks.Run` checks inactive/new canvases and audio plus restoration in Unity.
- Before upload: build Web, confirm both MP4s are in StreamingAssets/JourneyVideos, serve over HTTP, start a named journey, move/collect, wait a second, refresh, Continue, and compare name, season, position and artifacts. Repeat on the final itch page, then test Reset followed by refresh. Play the final film with music enabled and verify only film audio/picture remain.

References: [Unity persistentDataPath](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/application/persistentdatapath), [Unity Web video support](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/webgl/develop/video), [Unity JavaScript interop](https://docs.unity3d.com/6000.0/Documentation/Manual/web-interacting-browser-js-to-unity.html), [itch HTML uploads](https://itch.io/docs/creators/html5), [MDN localStorage](https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage).
