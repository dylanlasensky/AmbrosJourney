# Browser sessions and the supplied videos

The game is designed to be completed in one sitting. Progress belongs to the running game only. Refreshing the page, closing the game or starting a new journey begins again; earlier browser or native checkpoints are not loaded. The game no longer writes progress to localStorage or a save file. Existing old browser records can remain untouched because the game does not use them.

Pause keeps the current run in memory while the game remains open. Download the earned certificate before closing or refreshing the game. The PDF download is a player-requested export, not a progress save.

Unity Web does not support embedded VideoClips. `JourneyWebVideoBuild` copies the team's unchanged Intro.mp4 and Outro.mp4 into the built game's `StreamingAssets/JourneyVideos` folder after a Web build. The Web player uses those relative URLs; native builds still use the original VideoClips. Do not delete StreamingAssets when zipping for itch. The final film suppresses inactive/delayed game canvases, cameras, and every non-film audio source until playback ends.

## Validation procedure

- `node tests/check_web_template.cjs` checks the loader and presentation template; `node tests/check_cinematic_focus.cjs` checks cinematic keyboard-focus guards.
- `CinematicIsolationChecks.Run` checks inactive/new canvases and audio plus restoration in Unity.
- Build Web and confirm both MP4s are packaged. Serve over HTTP, begin a named run, make lesson progress, then refresh. Confirm a fresh menu and a fresh Spring journey without restored lessons, artifacts or completion. Repeat with old checkpoint records already present; they must be ignored. Do not clear unrelated browser data.
- Play the final film with music enabled and verify only film audio/picture remain. Test fullscreen and keyboard handoff on the actual uploaded itch page using the [browser procedure](BrowserBuildChecks.md).

References: [Unity Web video support](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/webgl/develop/video), [itch HTML uploads](https://itch.io/docs/creators/html5).
