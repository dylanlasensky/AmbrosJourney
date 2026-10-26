# Referenced journey artwork

Generated October 26, 2026 using OpenAI image generation. Reference images were the existing `Assets/MapSpring/seasonal sample (spring).png` terrain pack and the previous `Assets/WinterLevel/Art/MountainRidge.png`. The terrain was the style target; the old mountain illustrated the realism mismatch to remove.

Initial prompt requested a transparent four-object atlas: a timber-and-stone ranger cottage, a snowy equivalent, a broad mountain with an integrated cave mouth and lanterns, and a compass learning shrine. Requirements included SNES-style 3/4-view pixel art, dark muted purple outlines, warm ivory highlights, rounded stone matching the terrain cliffs, three or four shades per material, consistent upper-left lighting, no photographic peaks, no text and no external background plates.

The first result did not provide enough gutter between sprites. A second generation edited the atlas into a square two-by-two arrangement with generous transparent padding, preserving the designs. The final imported source is `Assets/Journey/Resources/Journey/WorldAtlas.png`. Runtime sprite bounds are measured against that 1254×1254 source in `JourneyWorld.Atlas`, and normalized to the imported texture size. Point filtering, no mipmaps, transparent edges and uncompressed import preserve the intended pixels.

Original output files remain outside the repository in the generator's output directory. Only the selected final atlas is included. Existing third-party asset licenses and source files remain unchanged.

A second original atlas, `SentinelAtlas.png`, replaces Fall's enlarged 32-pixel placeholder. The prompt used the existing terrain and old Sentinel as references, requesting a stone-and-wood golem with amber eyes, copper autumn leaves, mossy rounded shoulders and a chest rune. Four subtle idle poses share a two-by-two layout, perspective, scale and foot baseline. The imported atlas is capped at 512 pixels with point filtering; the guardian uses all four poses and retains the existing quiz, strike feedback and collision logic.
