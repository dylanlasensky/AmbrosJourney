# Current validation: 27 November 2026

Validated on macOS with Unity **6000.6.2f1**, against main **97c7cbb** plus the pause-music, comments and documentation update described in this commit. The isolated build copy used the current repository inputs; source projects and unrelated local generated files were not reset.

## Passed checks

| Check | Result and scope |
| --- | --- |
| Standalone .NET progression | **22,959 assertions passed**; all 32 Summer, 243 Fall, 243 Winter boss outcomes and 256 mixed-final pass/fail combinations, plus independent current-run data, names, bounds and certificate identity. |
| PauseJournalChecks.Run | **PAUSE_JOURNAL_PASS**; no music action over gameplay or after Resume, working pause music toggle, independent sound effects, pause freeze, layout, long/accented names, artifacts, end-run cancellation, unlocked chapters, retained Winter location and certificate access. |
| SeasonIntegrationChecks.Run | **SEASON_INTEGRATION_PASS**; four seasons, both Winter bosses, real portals, question UI, retries, companion conversations, maps, chapter/cave revisits, twelve artifacts, menus, certificate export, end-run and a new named journey. Winter checks inspected 125 UI states. |
| Asset integrity | **444 tracked GUIDs passed**, including the new desktop build helper metadata, required Resources/artwork/portals, build scenes and original video identities. |
| Session-only source guard | **67 runtime files passed**; no progress-storage capabilities, with certificate PDF export retained. |
| Web template and cinematic focus | Both Node checks passed: bounded Retina sizing, resize/fullscreen handling, loading/error flow, visible-canvas focus restoration, hidden-page/form-control preservation and denied-focus fallback. These use source/mocks, not a running WebAssembly player. |
| Comment-only audit | Existing authored C# changes outside JourneySession and the three updated Editor checks contain no executable changes. The new desktop build helper is Editor-only. |
| Report integrity | All seven supplied PDFs copied byte-for-byte into Reports; no byte-identical duplicates were present elsewhere among the tracked documents. |

## Test maintenance

The first connected run exposed Unity's known startup SearchDatabase indexing exception. The integration runner now applies the same narrow stack-specific exclusion already used by PauseJournalChecks; other gameplay exceptions still fail the run. A subsequent run exposed stale fixture answers for Spring's revised claim mapping and an expectation that Summer's completed arrival greeting could be repeated. The checks now read the active Spring claim mapping and assert that the completed Summer greeting stays closed. The complete updated suite then passed. No gameplay rule was changed to satisfy those outdated expectations.

## Release builds

All three exports succeeded with **zero build errors**:

| Platform | Build report | Additional verification |
| --- | --- | --- |
| macOS | Release, 254,660,015 bytes; 2 build warnings | Apple silicon arm64. Release player remained running in a 12-second startup smoke check with no logged gameplay exception. Applied and verified a local ad-hoc signature. This is not a notarized universal installer. |
| Windows | Release, 281,756,622 bytes; 1 build warning | Windows x86_64 Mono. Complete EXE/Data/DLL/Mono dependencies packaged; not executed on Windows from this Mac. |
| Web | Release, 113,717,470 bytes in Unity's build report; 6 build warnings | Gzip custom template; separate original MP4s retained. ZIP integrity/root layout and movie hashes checked. Unity's report does not include every separately copied streaming file. |

Build warnings include existing Editor API deprecations and renderer/shader warnings. Compilation/export succeeded; these results do not claim warning-free source. Local build summaries/logs retain the exact messages.

The command tool placed provenance inside the Mac app root after export. It was moved outside the bundle before ad-hoc signing. The Mac ZIP is made from the verified source app outside iCloud, avoiding Finder metadata that iCloud adds to an unpacked bundle. Windows debug-backup directories explicitly marked DoNotShip are excluded. ZIP byte counts and SHA-256 identities are in [Builds/manifest.json](../Builds/manifest.json).

## Delivery and limits

Source, reports and maintained documentation are published through the signed-in GitHub connector. All three packages are saved locally under the top-level Builds folder. Large archives are excluded from ordinary Git history because each exceeds GitHub's 100 MB file limit. The available GitHub connector does not provide release-attachment uploads; no uploaded GitHub build release or replacement itch upload is claimed by this update.

The current packages still need the team's hosted itch playthrough, including films/fullscreen, fresh starts after refresh and PDF download. Windows execution, Intel Mac support, target-audience comprehension, accessibility, pacing, low-end hardware and subjective audio balance are not established by these checks. The team has tested individually and with friends/roommates; no middle-school testing or measured learning outcomes are claimed. Use [Testing](Testing.md) and [browser procedure](BrowserBuildChecks.md) to repeat the checks. Submitted sprint PDFs and old artwork captures remain historical records rather than current feature specifications.
