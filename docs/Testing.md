# Testing and builds

## Hosted checks

GitHub Actions runs **Game checks** for PRs and pushes to main with read-only permissions and no Unity/API credentials.

- Actual Summer/Fall/Winter progression sources compile with .NET 8. Tests cover all 32 Summer, 243 Fall and 243 outdoor Winter boss answer combinations, plus all **256 pass/fail combinations of the eight mixed final activities**. The old 6,561 three-choice final combinations no longer describe this finale.
- Checks include prerequisites, wrong answers, invalid/incomplete activity responses, duplicate picks, score thresholds, retries, retained missions/crests, courage recovery, save round trips, backup recovery, reset and name/bounds validation.
- Asset integrity checks tracked gameplay `.meta` files, GUID uniqueness, prefab artwork/video references, Resources portals, build scenes and package JSON. New assets and their metadata must both be staged before this check represents the PR.

From the repository root:

```sh
dotnet run --project tests/Progression/Progression.csproj --configuration Release
python3 tests/check_assets.py
```

These checks do not compile every Unity script, render artwork or prove player behavior. They supplement review and playtesting.

## Unity integration and art checks

Use Unity **6000.6.2f1** and an isolated checkout/project copy. The runner enters Play mode and exits the Editor when finished. It exercises resets against a separate test save path. Do not run it in an Editor with unsaved work.

```sh
"/Applications/Unity/Hub/Editor/6000.6.2f1/Unity.app/Contents/MacOS/Unity"   -batchmode -projectPath /absolute/path/to/capstone   -executeMethod AmbrosJourney.Presentation.Editor.BossRefreshChecks.Run   -logFile /tmp/ambros-integration.log
```

The wrapper prepares the baked Determination asset if missing, imports the four boss atlases with alpha/point filtering, waits for Editor startup work, then runs the connected suite. Required assets are committed; ordinary gameplay needs no generation step or extra package.

Coverage includes the actual four-season portals, locked/modal guards, mission/boss retries, new final pointer drag/drop handlers and submission, all four seasonal UIs, menu callbacks, pause, exact outdoor/cave saves, chapter revisits, all twelve F-key pickups, certificate PDF export, reset and fresh Spring startup. Boss checks confirm two idle frames, distinct windup/impact frames, actor-to-visual strike signals and four in-world renders per replaced boss. Fall's existing boss is preserved.

Read `Logs/SeasonChecks/Integration.txt`: it must say **PASS**. The runner has a twelve-minute post-import deadline and fails on errors during the active checks. Text-overflow and render checks cover **1280x720 and 1024x768**. Reports/captures go to ignored `Logs/SeasonChecks` and `Logs/BossChecks`. The world audit also checks building counts, doorstep routes, winter tree art, maps and survey views.

## Standalone build

```sh
"/Applications/Unity/Hub/Editor/6000.6.2f1/Unity.app/Contents/MacOS/Unity"   -batchmode -projectPath /absolute/path/to/capstone   -executeMethod AmbrosJourney.Presentation.Editor.BossRefreshChecks.Build -quit   -logFile /tmp/ambros-build.log
```

This makes a macOS release player at `Logs/BossChecks/AmbrosJourney-BossPreview.app`; Intro and Spring are build scenes and later worlds/art are retained through Resources. Check `Logs/BossChecks/Build.txt`. It uses a separate output from the earlier preview so building does not overwrite a running copy.

See [current validation](CurrentValidation.md) for results rather than inferring success from a command launch. A full human standalone playthrough, target-hardware measurements, accessibility, audio and first-time learning/pacing tests remain necessary. Hosted Unity builds require a separate licensing setup; no paid service or self-hosted runner is added.
