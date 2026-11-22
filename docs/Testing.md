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
| `WinterRefinementChecks.Run` | `Logs/WinterRefinements/Results.txt` | Always-visible Winter guidance, continuous chapter guidance, centered Warden, automatic cave permission, natural cave entry, visibility circle and lantern light. `CompassNoticeChecks.Run` is a compatibility alias for this suite. |

Both methods reject non-batch use and exit the Editor after their checks. `PASS` in the result file is required; also inspect the fresh log for compile/runtime errors rather than trusting an older result file. The game no longer reads or writes a persistent player profile.

Use the assertion count printed by the current pure suite; older dated counts describe earlier snapshots. Session tests check fresh defaults, distinct data objects for separate runs, same-run lesson/collection state and certificate identity. New launches must ignore existing save files/storage without changing them; verify that with the runtime session tests, not by treating a serialization test as startup coverage. Required asset metadata must be included; no new packages are required.

## Standalone build

```sh
"/Applications/Unity/Hub/Editor/6000.6.2f1/Unity.app/Contents/MacOS/Unity"   -batchmode -projectPath /absolute/path/to/capstone   -executeMethod AmbrosJourney.Presentation.Editor.BossRefreshChecks.Build -quit   -logFile /tmp/ambros-build.log
```

This makes a macOS release player at `Logs/BossChecks/AmbrosJourney-BossPreview.app`; Intro and Spring are build scenes and later worlds/art are retained through Resources. Check `Logs/BossChecks/Build.txt`. It uses a separate output from the earlier preview so building does not overwrite a running copy.

See [current validation](CurrentValidation.md) for results rather than inferring success from a command launch. A full human standalone playthrough, target-hardware measurements, accessibility, audio and first-time learning/pacing tests remain necessary. Hosted Unity builds require a separate licensing setup; no paid service or self-hosted runner is added.


## Second-round boss and portal regression

Run `BossExitChecks.Run` with the same disposable-project batch-mode setup. It writes `Logs/BossExitChecks/Results.txt` and exercises real seasonal boss answer buttons, failed/pass thresholds, progressive boss dust, fully gated portal assembly, automatic transitions, R-key input in all four player controllers, every final activity type, certificate pickup/PDF/Exit, certificate Escape, completed in-memory chapter revisit, and the final walk-home portal. It checks UI at 1280x720 and 1024x768 and uses isolated run data. `SeasonIntegrationChecks.Run` has also been updated to wait across the new multi-frame effects and to expect no interaction with defeated bosses.


For the Winter follow-up, run `WinterRefinementChecks.Run` in the disposable project. It writes `Logs/WinterRefinements/Results.txt` and renders continuous guidance, the natural cave and lantern visibility at both standard test sizes. `BossExitChecks.Run` expects Winter's cave exception and automatic cave permission after the victory message closes. `CompanionGuidanceChecks` and `DirectGuidanceChecks` are included in the connected suite for nonblocking advice, same-run state and direct objective guidance.

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
