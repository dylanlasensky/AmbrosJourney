# Browser export and playtesting

Use the project's exact Unity version, 6000.6.2f1, with its matching Web Build Support module. An older editor's Web module cannot substitute for it. Close other Editor instances for this project before a batch build.

The editor methods `AmbrosJourney.Journey.Editor.JourneyWebBuild.BuildQA` and `BuildSubmission` build Intro followed by Spring. QA enables Unity development diagnostics and exports uncompressed files; submission uses release build options and writes a ZIP beside the exported folder. Neither mode adds gameplay shortcuts; both use the same one-sitting session behavior. Both use the same responsive template and original MP4s. The methods reject a nonempty output directory, honor `-buildOutput`, check the build report, and verify that the videos match their source hashes. Submission also checks itch.io's default file count, path length and size limits before making the ZIP, with `index.html` at its root.

Preferred CLI (from the repository root):

```sh
unity build capstone --target WebGL --execute-method AmbrosJourney.Journey.Editor.JourneyWebBuild.BuildQA --output capstone/Builds/Web/QA
unity build capstone --target WebGL --execute-method AmbrosJourney.Journey.Editor.JourneyWebBuild.BuildSubmission --output capstone/Builds/Web/Submission
```

Choose a new output directory for each build. If the CLI cannot launch the Editor, use the installed matching Editor executable with `-batchmode -quit -projectPath <absolute-project-path> -buildTarget WebGL -executeMethod <method-above> -buildOutput <absolute-output-folder> -logFile <absolute-log-file>`. The execute method must run after Web is the active build target. In the Editor UI, switch to Web and choose **Ambros Journey → Build → Web QA / Web Submission**. The build methods temporarily select the custom template and mode-specific compression settings, then restore the editor's prior settings. For an ordinary manual Build button export, choose the **AmbrosJourney** Web template explicitly.

The canvas fills its iframe at embedded and fullscreen sizes. A `ResizeObserver` and resize/fullscreen listeners update its backing buffer, with Unity's automatic canvas matching disabled. Rendering follows display density up to 2× and a total budget of 2560 × 1440 pixels, so Retina text is sharper without an unbounded 4K/5K buffer. Standard-DPI displays are not supersampled; very large views may render below native resolution to stay within the budget. Current heap settings remain unchanged until browser measurements justify a change. Submission gzip uses native browser decoding; the host must send `Content-Encoding: gzip` and the proper content type. itch.io does this for gzip exports. Opening `index.html` as a local file or serving `.gz` files without those headers is not a valid test.

## Browser checks before submission

Serve the build over HTTP with MP4 range support; the submission build also needs gzip headers for its compressed files. Put it in an iframe whose parent runs on a different localhost port. The parent's fullscreen button should request fullscreen for the iframe wrapper without refocusing the game. This reproduces the focus boundary of itch's outer fullscreen control while keeping test browser state separate from the real itch origin. Do not clear the user's normal browser storage.

1. Start a new local test run. While the intro is visibly moving, press the parent's fullscreen button. Confirm picture, sound and captions continue without clicking the game afterward. Exit fullscreen, resize, and repeat. Play the intro through to Spring.
2. Verify keyboard movement and interaction, all four seasons, direct arrow guidance, boss questions, manual E-to-talk companion conversations and the cave. Verify mouse clicks still hit the intended buttons before and after embedded/fullscreen resizing, including a Retina display where available. Test keyboard pause/resume after a fullscreen change. Confirm returning from a hidden browser tab is usable; browsers may throttle hidden tabs.
3. Make lesson/artifact progress, refresh the iframe, and confirm the game starts fresh with no resumed run. Existing legacy checkpoints must be ignored. Start another journey and confirm the previous run's lessons, artifacts and completion do not carry over. Pause/resume must retain the current run while the page stays open.
4. Finish the final boss and repeat the fullscreen test during the outro. Confirm only the film's audio plays, no game titles/popups cover it, and the certificate flow works afterward. Gameplay should resume its normal background-pause behavior after films.
5. Repeat critical checks on the release export, not only the development build. Check the browser console for errors and network requests for missing files. Inspect the ZIP's root before the team uploads it, then repeat intro/outro fullscreen and fresh-start-after-refresh behavior on the uploaded itch version.

`tests/check_web_template.cjs` checks template configuration, bounded Retina sizing, resize events and loading/error behavior; it does not execute WebAssembly or prove film playback. Native tests and a successful Web export also do not replace these browser checks. Record the actual build and browser results separately; this document is a procedure, not a claim that they passed.

Sources: [Unity canvas sizing](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/webgl/develop/canvas-size), [Unity Web template configuration](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/webgl/building-distribution/templates/web-templates-build-configuration), [Unity compression and deployment](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/webgl/building-distribution/deploying), [Unity development-build compression](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/webgl/building-distribution/distributionsize-codestripping), [Unity Web memory](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/webgl/develop/memory), and [itch HTML game hosting, fullscreen and upload limits](https://itch.io/docs/creators/html5).

Packaged exports are stored at the repository top level under [Builds](../Builds/README.md). Rebuild using a fresh empty output directory; do not overwrite an active export or copy only the HTML file.
