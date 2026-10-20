# Opening and Spring Valley

This local review build follows Sprint Two's Ambros / Ancient Mage / AI-literacy adventure. The opening follows the updated brief: Ambros plays a game on his laptop, gets pulled inside, and must beat the seasons to get home. No live AI service is used.

## Current build
- Temporary twenty-second original 2D animation (rejected visual style, replacement pending): furnished bedroom, typing character, growing portal, animated suction and tunnel, landing in Spring. Includes an original music and effects cue. The video automatically loads Spring, with no player action during playback.
- Spring spans 64 × 64 world units. Winding trails connect the Mage's cottage, Blossom Grove, Stillwater Lake, southern ruins, and a separate guardian clearing. Grass, blossoms, trees, bushes, rocks, a dock, trail posts and drifting petals furnish the map.
- A parchment journal, contextual interaction prompts and rotating compass guide exploration. The compass routes the second mission around the lake.
- Three field-note stops teach the material before six applied true/false challenges. Press J to review the collected notes. The lessons cover AI training, humanlike responses, and privacy. Wrong answers provide retries; three courage points replenish on depletion.
- Thornhide uses a separate animated creature from the existing Creatures Pack. Its five-question quiz requires 4/5 to open the Summer gate behind the arena. Failure preserves lesson progress. The Mage joins after success.
- The generated woodland cottage and earlier season gate supplement the original asset packs.

## Review and reproduction
Open the inner `capstone` project with Unity 6000.6.2f1. Use the Capstone menu:
- **Play From Opening** / **Play Spring Only**: review the game.
- **Upgrade Spring World**: rebuild the expanded map in Spring (replaces that generated layout). Then run **Polish Cottage and Learning Trail** to apply the enlarged cottage, connected approach and revised spawn.
- **Validate Spring Progression**: checks field-note completion, wrong-answer retry, all six challenges, 60% failure, 80% success, distinct boss art and expanded bounds.
- **Record Spring Demo**: editor-only walkthrough using ordinary Input System keyboard events. Captures the real Game view at 12 fps into `tools/.spring-demo-v5`; encode with ffmpeg.
- **Record Animated Opening**: renders 480 animation frames from `AnimatedSource.unity` into `tools/.intro-animated`. Run `python3 tools/render_animated_opening.py /path/to/ffmpeg` to encode the 20-second video and original sound cue.

`AnimatedOpening.cs` retains the editable animation source. The older keyframe renderer and art remain as earlier source material; they are no longer used by the opening video.

## Limits
Summer, Fall and Winter are not playable yet. Saves, inventory, character customization and the eventual completion certificate remain future work. Movement uses the original character pack's side-facing frames. The opening is stylized 2D animation, not a rendered 3D film. No changes are pushed until the demo is approved.

## Pacing target
Spring targets approximately 5 minutes for a first-time player reading the field notes, considering six applied claims, exploring and completing the five-question guardian quiz. This is a design estimate, not a measured usability-test result. Fast repeat play and the demonstration are shorter. No artificial waiting is required. Spring is the shorter introductory chapter. Reserve the remaining roughly twenty-five minutes for the later seasons and finale.
