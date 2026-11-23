# Documentation index

## Current implementation

- [Repository overview](../README.md): play, controls, feature status and remaining work.
- [November 22 session and transition validation](SessionOnlyValidation.md): current behavior, native regressions and exact release-browser checks.
- [November 21 submission validation](SubmissionValidation.md): historical native/browser results by build and delivery limits; its save behavior predates the session-only change.
- [Game design](GameDesign.md): story, learning goals, rewards and progression contracts.
- [Journey completion](JourneyCompletion.md): menu, current-run progress, artifacts, guidance, performance and teammate video hooks.
- [Presentation clarity checks](PresentationClarityValidation.md): restored teammate intro, deliberate NPC interactions, nearby labels and bounded Retina rendering.
- [Companion guidance](CompanionGuidance.md): E-to-talk conversations, deliberate escort greetings, quiet arrivals and manual cave briefing.
- [Cinematic fullscreen checks](CinematicFullscreenChecks.md): video presentation and the required browser focus/fullscreen checks.
- [Web export and browser checks](BrowserBuildChecks.md): QA/submission builds, packaging and the browser release procedure.
- [Session behavior](BrowserSaving.md): one-sitting play, fresh starts on reload and untouched legacy saves.
- [Question refresh](QuestionRefresh.md): current notes, questions, correct answers and final activity structure.
- [Bosses](Bosses.md): active seasonal forms, frame setup, generated-art prompts and review images.
- [Testing](Testing.md) and [dated validation record](CurrentValidation.md): commands, recorded results and limits.
- [Spring](../SPRING_PROTOTYPE.md), [Summer](../capstone/Assets/SummerLevel/README.md), [Fall](../capstone/Assets/FallLevel/README.md), [Winter](../capstone/Assets/WinterLevel/README.md): chapter ownership and setup.
- [Arrow and world polish](MoonbranchPolish.md), [certificate reward](CertificateReward/Validation.md), [pause journal](PauseJournal/Validation.md), and [audio](Audio/SoundEffects.md): feature notes and earlier validation.
- [Intro brief](IntroVideoBrief.md): teammate-owned story/video requirements.

## Asset provenance and dated evidence

[JourneyArtPrompts](JourneyArtPrompts.md) and [WinterArtPrompts](WinterArtPrompts.md) preserve the prompts used for earlier assets; new boss prompts are in [Bosses](Bosses.md) and `BossGenerationPrompts.json`. Preserve original third-party pack/font credits alongside them.

[World cohesion](WorldCohesion.md), [world polish](WorldPolish.md), [Second-round changes](SecondUserTesting.md), [first-player guidance](FirstPlayerGuidance.md), [November 12 feedback](UserTestingFeedback.md), [seasonal worldcraft](SeasonalWorldcraft.md), [FallValidation](FallValidation.md), [PresentationValidation](PresentationValidation.md), [SeasonReview](SeasonReview.md), [JourneyValidation](JourneyValidation.md), and [QuestionRefreshValidation](QuestionRefreshValidation.md) are dated evidence, not current feature specifications. Their counts/captures describe those earlier snapshots. For current behavior use the repository overview and companion guidance. The frozen-arrow rule is superseded. November 22 restores deliberate NPC conversations and the Little Mage cave briefing; earlier automatic-advice results are historical. Persistent saves and Continue were removed on November 22; current-run chapter revisits remain. Retaining older records does not restore their former saving, placeholder intro or multiple-choice finale.
