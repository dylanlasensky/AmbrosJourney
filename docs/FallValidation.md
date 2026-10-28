> **Dated validation snapshot.** Counts, captures and limitations below describe the version tested at that time. For the current bosses, Determination UI, mixed finale, persistent saves and latest results, see [current validation](CurrentValidation.md) and [documentation index](README.md).

# Fall validation — October 24, 2026

Verified in the existing Unity 6000.6.2f1 project.

- C# compilation passed with no errors.
- All 243 combinations of the guardian's three-choice answers matched the expected score and 4/5 threshold.
- Introduction and field-note prerequisites, seven mission choices, wrong-answer retries, duplicate rewards, courage recovery and retained mission progress passed.
- Live collider-grid routes reached all three stations. The guardian was unreachable before runes and reachable after unlocking; the Winter gate was blocked before passing and reachable afterward.
- Automated UI playthrough exercised the actual dialogue continuations and answer callbacks: introduction, all stations, wrong answers, a failing 3/5 guardian run, successful 4/5 retry, journal, Emberfist recruitment and Winter endpoint. No TMP text overflow was reported for those states at the tested view size.
- Summer's actual interaction callbacks completed all three wards and its five Orc questions. Incomplete progress, distance from the gate and open dialogue prevented transition. With the completed player at the gate and dialogue closed, the connector loaded Fall, unloaded Summer and displayed Fall's introduction.
- Native Unity Game view checked; E and mouse advanced the introduction, and held D moved Ambros with the camera and companions following.
- Runtime checks after the portal reported zero Unity errors or warnings and an advancing player loop.
- Standalone macOS Development build starting in Summer succeeded with **0 errors**, 3 existing font/shader/platform warnings, approximately 311 MB, in 64 seconds. Its Resources include Fall. Warnings: SummerDefault's internal font name differs from its filename; Intel macOS architecture deprecation; deprecated debug-symbol pragma in the existing TMP shader. No existing shaders, font or platform settings were changed to suppress these.

The build check proves packaging and player compilation; a full manual playthrough of that standalone executable was not performed. Audience comprehension, first-time navigation, pacing and platform-specific user testing remain to be done. At this snapshot, progress was session-only and Winter was still planned. The current journey now includes persistent saves and playable Winter/finale.

The temporary Unity Pipeline connection used to construct and inspect the project was removed from the manifest after validation; Fall has no dependency on it. Existing art/font/render settings that Unity reserialized during the build were restored.

See [map overview](FallMap.png), [arrival view](FallArrival.png), and [implementation notes](../capstone/Assets/FallLevel/README.md).
