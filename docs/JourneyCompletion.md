# Runtime architecture and journey flow

## Entry and state

Open the inner `capstone/` Unity project in **6000.6.2f1**. Start `Assets/Opening/Intro.unity`. The main menu requires an explorer name, shows an AI-content disclaimer, plays the supplied intro and loads Spring. Intro and Spring are the two build scenes; later worlds are instantiated from Resources prefabs through seasonal portal bridges.

`JourneySession` coordinates name entry, menus, pause, current-run chapter data and transitions. `JourneyData` stores identity, unlocked chapters, visited locations, lesson/boss progress, twelve artifacts and certificate completion in memory. Chapter revisits keep this run's state and position. An arrival guard prevents an immediate repeat transition when restoring a location on a portal.

There is no persistent profile, Continue, database, account or live AI API. Refreshing/closing the player, ending the run or starting a new journey starts fresh. Legacy saves are ignored without deletion. After the completed outro, the certificate can be reopened from the menu until a new journey or reload.

## Learning flow

| Chapter | Runtime ownership | Player progression |
| --- | --- | --- |
| Spring | Authored scene, `SpringGuide`, `SpringPlayer` | Controls journal, Ancient Mage, three notes/six Myth Bat claims, guardian 4/5, Summer portal |
| Summer | `SummerWorld`, `SummerLevel`, `SummerProgress`, `SummerPortalBridge` | Warriors, workshop/garden/message missions, three wards, guardian 4/5, Fall portal |
| Fall | `FallWorld`, `FallLevel`, `FallProgress`, `FallPortalBridge` | Emberfist, goal/context/format and revision lessons, three runes, guardian 4/5, Winter portal |
| Winter | `WinterWorld`, `WinterLevel`, `WinterProgress`, `WinterPortalBridge` | Two evidence stations, Warden 4/5, completed outside Little Mage briefing, natural cave entry |
| Echo Cavern | Same Winter instance; `FinalChallenges`, `FinalActivityPanel` | Archive note/two choices, eight cumulative activities, 7/8, certificate scroll, home portal |

Questions are authored content. Progress classes keep grading separate from UI and are linked directly into standalone tests. Wrong lesson answers allow retry; failed bosses retain completed lessons. The final has multi-selection, ordering, prompt assembly, matching and choice activities with click/keyboard alternatives. Correct feedback is explicit and all score denominators stay fixed. [Current curriculum](QuestionRefresh.md).

## Presentation and interaction

`JourneyUI`, `SeasonOverlay` and the baked Determination font provide the shared parchment presentation. `SeasonWorldLabel` shows one nearby overhead interaction card and avoids fixed HUD/player overlap. `CompanionSpeech` and each level's state retire completed NPC prompts and restore them for later briefings. E starts dialogue; Esc closes it. The duplicate bottom prompt and R restart are absent.

`GuideWalk` uses collision-aware companion routes and varied formation spacing. `MoonbranchCompass` points directly at the next objective through all four seasons and the cave. It does not freeze in snow or choose a path around obstacles. `NaturalTrail`, `SeasonalWorldcraft` and `WorldCohesion` supply trails, scenery, reflections and lantern details. Winter's ridge collision prevents walking behind the cave; its visibility mask reveals nearby passages and lantern pools.

`SeasonBossVisual` animates seasonal atlases. `BossPortalSequence`, `PixelBossDissolve` and `PixelPortalReveal` block competing input while victories dissolve and exits assemble. Winter instead keeps its original cave art: no portal overlay, and first entry also requires the completed outside Mage briefing. Final victory drops the certificate scroll. E opens it; Download PDF stays in the viewer; Exit/Escape resumes the cave and builds the home portal. Walking through it plays the original outro and returns to the menu.

## Pause, audio and deployment

Esc/Pause freezes gameplay, animation and ordinary audio. The two-page journal offers Resume, confirmed End run, Artifacts, **Music On/Off**, unlocked chapter travel and an earned Certificate action. Guidance is always on. The gameplay HUD has only Pause and Artifacts actions below the objective box; no music button persists over gameplay. Main-menu Settings also controls music. The music preference lasts for the application session and affects new tracks as well as current music; sound effects remain separate. M mutes all registered audio.

`GameAudio` shares short sound cues, `BossBattleMusic` replaces exploration music during encounters, and the menu uses Spring's track. `JourneyCinematicIsolation` suppresses game UI/cameras/other audio during films. Web builds copy the original MP4s to StreamingAssets, use the responsive custom template and browser PDF bridge. Native PDF export writes to Downloads, with the existing fallback when unavailable. A certificate records participation, not an external qualification.

Release Web/macOS/Windows exports and rebuild instructions are indexed in [Builds](../Builds/README.md). Browser execution/fullscreen/download testing is separate from compilation and packaging. [Testing](Testing.md), [actual results](CurrentValidation.md), [video provenance](IntroVideoSource.md).
