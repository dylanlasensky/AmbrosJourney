# Testing and builds

## Hosted checks

GitHub Actions runs **Game checks** for PRs and pushes to main with read-only permissions and no Unity/API credentials.

- Actual Summer/Fall/Winter progression sources compile with .NET 8. Tests cover all 32 Summer, 243 Fall and 243 outdoor Winter boss answer combinations, plus all **256 pass/fail combinations of the eight mixed final activities**. The old 6,561 three-choice final combinations no longer describe this finale.
- Checks include prerequisites, wrong answers, invalid/incomplete activity responses, duplicate picks, score thresholds, retries, retained missions/crests, fresh independent runs, in-memory chapter retention, completion data and name/bounds validation.
- Asset integrity checks tracked gameplay `.meta` files, GUID uniqueness, prefab artwork/video references, Resources portals, build scenes and package JSON. New assets and their metadata must both be staged before this check represents the PR.
- The session-only architecture check rejects durable storage capabilities and legacy save interfaces in runtime C# and authored Web scripts. The reviewed certificate PDF export is the only permitted native file-write exception. This source check does not establish browser refresh behavior or prove that existing storage is untouched at runtime.

From the repository root:

```sh
dotnet run --project tests/Progression/Progression.csproj --configuration Release
python3 tests/check_assets.py
python3 tests/check_session_only.py
node tests/check_web_template.cjs
node tests/check_cinematic_focus.cjs
```

These checks do not compile every Unity script, render artwork or prove player behavior. They supplement review and playtesting.

## Unity integration and art checks

Use Unity **6000.6.2f1** and an isolated checkout/project copy. The runner enters Play mode and exits the Editor when finished. It exercises isolated in-memory runs; it must not read, overwrite or delete a player's old save files. Do not run it in an Editor with unsaved work.

```sh
"/Applications/Unity/Hub/Editor/6000.6.2f1/Unity.app/Contents/MacOS/Unity"   -batchmode -projectPath /absolute/path/to/capstone   -executeMethod AmbrosJourney.Presentation.Editor.BossRefreshChecks.Run   -logFile /tmp/ambros-integration.log
```

The wrapper prepares the baked Determination asset if missing, imports the four boss atlases with alpha/point filtering, waits for Editor startup work, then runs the connected suite. Required assets are committed; ordinary gameplay needs no generation step or extra package.

Coverage includes the actual four-season portals, locked/modal guards, mission/boss retries, new final pointer drag/drop handlers and submission, all four seasonal UIs, menu callbacks, pause, same-run outdoor/cave state, chapter revisits, all twelve E-key pickups, certificate PDF export, reset and fresh Spring startup. Boss checks confirm two idle frames, distinct windup/impact frames, actor-to-visual strike signals and four in-world renders per replaced boss. Fall's existing boss is preserved.

Read `Logs/SeasonChecks/Integration.txt`: it must say **PASS**. The runner has a twelve-minute post-import deadline and fails on errors during the active checks. Text-overflow and render checks cover **1280x720 and 1024x768**. Reports/captures go to ignored `Logs/SeasonChecks` and `Logs/BossChecks`. The world audit also checks building counts, doorstep routes, winter tree art, maps and survey views.

## Focused user-feedback checks

Use the same disposable copy and batch-mode invocation above, replacing `-executeMethod` with one of:

| Method | Result | Coverage |
| --- | --- | --- |
| `GeneralFeedbackChecks.Run` | `Logs/SeasonChecks/GeneralFeedback.txt` | Correct explanations and fixed score totals across four chapters/finale; no courage text; artifact counter at empty/full count; actual body collisions and doorway access; ridge access; no cave Mage interaction; Activity 5 cycling; certificate Exit/Escape to cave, then home portal to menu. |
| `WinterRefinementChecks.Run` | `Logs/WinterRefinements/Results.txt` | Always-visible Winter guidance, continuous chapter guidance, centered Warden, deliberate E-to-talk cave permission, natural cave entry, visibility circle and lantern light. `CompassNoticeChecks.Run` is a compatibility alias for this suite. |

Both methods reject non-batch use and exit the Editor after their checks. `PASS` in the result file is required; also inspect the fresh log for compile/runtime errors rather than trusting an older result file. The game no longer reads or writes a persistent player profile.

Use the assertion count printed by the current pure suite; older dated counts describe earlier snapshots. Session tests check fresh defaults, distinct data objects for separate runs, same-run lesson/collection state and certificate identity. New launches must ignore existing save files/storage without changing them; verify that with the runtime session tests, not by treating a serialization test as startup coverage. Required asset metadata must be included; no new packages are required.

## Release exports

Use a fresh output directory for each build. The top-level [Builds guide](../Builds/README.md) lists packaged deliverables and extraction instructions. Keep the project on Unity 6000.6.2f1 with matching Web and Windows Mono support installed.

```sh
unity build capstone --target StandaloneOSX --execute-method AmbrosJourney.Journey.Editor.JourneyDesktopBuild.BuildMac --output /absolute/empty/macOS/AmbrosJourney.app
unity build capstone --target StandaloneWindows64 --execute-method AmbrosJourney.Journey.Editor.JourneyDesktopBuild.BuildWindows --output /absolute/empty/Windows/AmbrosJourney.exe
unity build capstone --target WebGL --execute-method AmbrosJourney.Journey.Editor.JourneyWebBuild.BuildSubmission --output /absolute/empty/Web/AmbrosJourney-Web
```

Desktop export uses release options and the Intro/Spring build scenes. Keep the Windows .exe beside its Data folder, UnityPlayer.dll and Mono dependencies; distribute the complete folder. Preserve the macOS .app bundle with an archive tool that retains executable permissions. Web export automatically makes a ZIP with index.html at its root, verifies original MP4 hashes and default itch size/file limits. Build summaries report errors, warnings and byte sizes. The methods reject populated destinations rather than mixing stale outputs.

A successful desktop export does not establish Windows execution on a Mac or platform signing/notarization. Test on the target OS. A browser release must also pass the [hosted Web procedure](BrowserBuildChecks.md); no paid build service or extra gameplay package is required.

## Pause music regression

Run `AmbrosJourney.Journey.Editor.PauseJournalChecks.Run` in the isolated project using the batch-mode pattern above, without -quit because the runner exits when finished. Require PAUSE_JOURNAL_PASS in the fresh Editor log. It checks no music control on the gameplay HUD (including after Resume), pause Music On/Off behavior, independent effects, layout, end-run cancellation, artifacts, unlocked chapter travel and certificate access.

## Second-round boss and portal regression

Run `BossExitChecks.Run` with the same disposable-project batch-mode setup. It writes `Logs/BossExitChecks/Results.txt` and exercises real seasonal boss answer buttons, failed/pass thresholds, progressive boss dust, fully gated portal assembly, automatic transitions, R-key input in all four player controllers, every final activity type, certificate pickup/PDF/Exit, certificate Escape, completed in-memory chapter revisit, and the final walk-home portal. It checks UI at 1280x720 and 1024x768 and uses isolated run data. `SeasonIntegrationChecks.Run` has also been updated to wait across the new multi-frame effects and to expect no interaction with defeated bosses.


For the Winter follow-up, run `WinterRefinementChecks.Run` in the disposable project. It writes `Logs/WinterRefinements/Results.txt` and renders continuous guidance, the natural cave and lantern visibility at both standard test sizes. `BossExitChecks.Run` expects Winter's natural cave doorway and the completed Little Mage conversation after Warden victory. `CompanionGuidanceChecks` exercises silent arrivals, actual E/Enter/Escape input, paused conversation movement, deliberate escort starts and same-run restoration. `DirectGuidanceChecks` covers direct objective guidance. Both are included in the connected suite.

For QA/release Web exports and actual browser testing, use [BrowserBuildChecks](BrowserBuildChecks.md) and [CinematicFullscreenChecks](CinematicFullscreenChecks.md). Source checks, native playthroughs and a successful export do not establish browser playback or a correct session lifecycle on itch.

## Fresh-run and transition regression

Run `TransitionArrivalChecks.Run` in the same isolated batch-mode project. Its report is `Logs/TransitionChecks/Results.txt`; require a fresh **PASS** and inspect the current log for errors. The runner checks:

- Fresh startup before installing its test profile, and clean data for a subsequent new run.
- An empty new scene cannot borrow the outgoing player; the destination prefab owns its player even while the old player still exists.
- All three actual portal handoffs retire source roots, unload the old scene and arrive at canonical spawn locations with fresh chapter progress.
- Water reflections, lantern animation and world-art processing reference only their own scene during overlapping scene lifetimes.
- Portal fades reject Escape, pause, artifact panels and competing chapter loads without changing progress.
- Summer arrival has a collision-aware route to the first workshop, with each route segment clear of scenery and closed seals.

Separately verify that startup ignores existing legacy save data without changing it, closing/refreshing begins a fresh run, end-run confirmation can be cancelled, and the completed certificate remains available until the next run. A source guard or clean-start fixture does not replace testing with existing browser storage. Record fresh release results separately from these instructions.

For a Mac export made with the command tool, keep its `unity-build.provenance.json` beside rather than inside the .app bundle. Apply a local ad-hoc signature with `codesign --force --deep --sign - /path/AmbrosJourney.app`, then verify with `codesign --verify --deep --strict /path/AmbrosJourney.app`. Archive that verified bundle with `ditto -c -k --sequesterRsrc --keepParent /path/AmbrosJourney.app /path/AmbrosJourney-macOS.zip`. This does not notarize it or add Intel support; the delivered build targets Apple silicon. Keep a signed bundle outside iCloud during packaging, since iCloud adds Finder metadata to unpacked app bundles. Windows archives must exclude Unity's `BackUpThisFolder_ButDontShipItWithYourGame` directory while retaining the player dependencies.
