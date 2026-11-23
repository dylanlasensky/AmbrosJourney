# Teammate intro and outro brief

**Current status, November 22:** the teammate's 40-second intro from PR18 and 20-second outro are supplied and connected. The intro was accidentally replaced in PR19 with the older October still-image sequence; the PR18 source has now been restored unchanged. See [source provenance](IntroVideoSource.md). The direction below is the earlier planning brief, not the duration or implementation of the finished clip.

## Earlier intro direction

Target approximately 20 seconds, 16:9, 1080p or higher. Use a coherent illustrated anime/storybook style with warm painterly detail, soft cinematic lighting and expressive natural motion. Match Ambros's tousled dark hair, black hoodie, age and face to the three original references in `tools/opening-art`. This illustrated cinematic style is distinct from the pixel-art gameplay.

1. **0-6 seconds, `01-home.png`:** Ambros plays a fantasy game on his laptop in the same cozy bedroom. Fingers and screen move; lamp glows and curtains move gently. A restrained camera push approaches over his shoulder as he notices the screen brightening.
2. **6-13 seconds, `02-portal.png`:** keep identity, clothes, desk and room consistent. Green-gold magic swirls out of the laptop. Loose papers move toward it. Ambros reacts, reaches for the desk and is pulled through the screen in one readable action. A brief magical light fills the shot.
3. **13-20 seconds, `03-spring.png`:** he lands on the illustrated forest path, steadies himself, looks around and rises. Clothing settles, blossoms drift and the camera settles toward the trail. End with a clean handoff into Spring.

Audio moves from keyboard/bedroom ambience through a magical whoosh to forest wind/birds. Keep music restrained. Avoid captions, watermarks, photorealistic redesigns, block-art substitutions, flickering faces, teleporting furniture and abrupt unrelated shots.

## Integration contract

Intro: the selected source is `capstone/Assets/Journey/Resources/Journey/Intro.mp4`. Keep its existing `.meta` GUID. `OpeningCinematic` loads `Journey/Intro` natively; Web builds copy the same bytes to `StreamingAssets/JourneyVideos/Intro.mp4`. New Journey plays it after name entry, then loads Spring. Preparation failure retains an Enter fallback.

Outro: `capstone/Assets/Journey/Resources/Journey/Outro.mp4` remains unchanged. Collecting the final scroll opens the certificate. Exit resumes the cave and reveals the home portal; walking through that portal invokes the outro and returns to the main menu. The Web export includes it beside the intro.

Review identity, anatomy, continuity, visible cause/effect and sound before integration. See [Journey completion](JourneyCompletion.md) for runtime behavior and [current design](GameDesign.md) for the completed learning route. Boss and scenery art are separate from these teammate-owned movies. Restoring the intended source does not regenerate or re-encode either film.
