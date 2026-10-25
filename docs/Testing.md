# Automated checks

GitHub Actions runs **Game checks** for pull requests and pushes to main. It uses read-only repository permissions and does not need Unity credentials or API keys.

- **Lesson and guardian regression tests** compile the actual Summer and Fall progression source with .NET 8. They check all 32 Summer and 243 Fall guardian answer combinations, the 80% pass threshold, prerequisites, wrong-answer retries, privacy selection, stone order, duplicate rewards and progress preservation.
- **Unity asset integrity** checks tracked gameplay metadata, duplicate GUIDs, world artwork references, scene prefab references, Resources portal prefabs, build-scene paths and package JSON.

Run the same checks locally from the repository root:

```sh
dotnet run --project tests/Progression/Progression.csproj --configuration Release
python3 tests/check_assets.py
```

These checks are automated tests, not an AI review or a complete Unity build. They do not compile every Unity script or prove that graphics, audio, navigation and player builds work. A green result does not replace reviewing the diff and playing the changed chapter.

## Unity integration checks

Use Unity 6000.6.2f1 and a disposable checkout. The integration runner enters Play mode, completes Spring, crosses the real Spring/Summer and Summer/Fall portals, exercises modal guards, runs the Summer/Fall interface callbacks and route checks, and verifies Fall completion. It exits the Editor when finished, so do not run it in an Editor containing unsaved work.

```sh
unity open /absolute/path/to/capstone --wait \
  --args '-executeMethod SeasonIntegrationChecks.Run -logFile /tmp/season-integration.log'
```

Read `capstone/Logs/SeasonChecks/Integration.txt` and the log. The result must say PASS; merely starting Unity does not count. The runner fails on logged errors or exceptions and has a three-minute runtime deadline after imports.

To compile a macOS Development player containing the opening and Spring, with Summer and Fall included through their Resources prefabs:

```sh
unity run /absolute/path/to/capstone --timeout 600 \
  -- -executeMethod SeasonIntegrationChecks.Build
```

The app and build summary go under `capstone/Logs/SeasonChecks/`. Unity builds and runtime checks currently run locally with the installed licensed Editor. Hosted Unity builds would require a separately configured Unity licensing setup. No self-hosted runner or paid service is enabled by this workflow.

Before release, also play the standalone game, check audio and multiple screen shapes, and time first-time players. Progress remains session-only.
