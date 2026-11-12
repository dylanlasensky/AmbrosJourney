# Teammate intro and outro brief

**Current status:** the teammate's final clips are not supplied. The game is playable: New Journey enters Spring directly, and final victory crumbles the Keeper and drops a scroll whose E pickup opens the certificate. Return/Escape then opens the main menu. The generated pixel intro and its generator were retired. This brief remains the intended illustrated story direction, not a claim that the film exists.

## Intro direction

Target approximately 20 seconds, 16:9, 1080p or higher. Use a coherent illustrated anime/storybook style with warm painterly detail, soft cinematic lighting and expressive natural motion. Match Ambros's tousled dark hair, black hoodie, age and face to the three original references in `tools/opening-art`. This illustrated cinematic style is distinct from the pixel-art gameplay.

1. **0-6 seconds, `01-home.png`:** Ambros plays a fantasy game on his laptop in the same cozy bedroom. Fingers and screen move; lamp glows and curtains move gently. A restrained camera push approaches over his shoulder as he notices the screen brightening.
2. **6-13 seconds, `02-portal.png`:** keep identity, clothes, desk and room consistent. Green-gold magic swirls out of the laptop. Loose papers move toward it. Ambros reacts, reaches for the desk and is pulled through the screen in one readable action. A brief magical light fills the shot.
3. **13-20 seconds, `03-spring.png`:** he lands on the illustrated forest path, steadies himself, looks around and rises. Clothing settles, blossoms drift and the camera settles toward the trail. End with a clean handoff into Spring.

Audio moves from keyboard/bedroom ambience through a magical whoosh to forest wind/birds. Keep music restrained. Avoid captions, watermarks, photorealistic redesigns, block-art substitutions, flickering faces, teleporting furniture and abrupt unrelated shots.

## Integration contract

Intro: provide `capstone/Assets/Resources/Journey/Intro.mp4` with its `.meta`, or assign a VideoClip to `OpeningCinematic.clip` in `Assets/Opening/Intro.unity`. The existing loader hands control to Spring when playback ends; absent clips start Spring directly and failed playback has an Enter fallback.

Outro: provide `capstone/Assets/Resources/Journey/Outro.mp4` with its `.meta`. The retained `JourneyEnding.Play` helper can play that clip and fall back to the certificate if it is missing or fails. The current `FinishJourney` pickup path opens the certificate directly and does not invoke this helper. Adding the clip alone will not play it: wire the agreed teammate handoff into that path and revalidate collection/Return behavior. The older required walk back to Spring is not implemented; the clip can resolve the return-home story.

Review identity, anatomy, continuity, visible cause/effect and sound before integration. See [Journey completion](JourneyCompletion.md) for runtime behavior and [current design](GameDesign.md) for the completed learning route. Boss and scenery art are already integrated; no replacement video is supplied by the feedback update.
