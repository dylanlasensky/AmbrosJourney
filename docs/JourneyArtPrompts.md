> Asset provenance: these are the original prompts for earlier world assets. Their text is retained as a record, not a current feature list. New boss art is documented in [Bosses](Bosses.md) and `BossGenerationPrompts.json`; all current live text uses Determination.

# Referenced journey artwork

Generated October 26, 2026 using OpenAI image generation. Reference images were the existing `Assets/MapSpring/seasonal sample (spring).png` terrain pack and the previous `Assets/WinterLevel/Art/MountainRidge.png`. The terrain was the style target; the old mountain illustrated the realism mismatch to remove.

Initial prompt requested a transparent four-object atlas: a timber-and-stone ranger cottage, a snowy equivalent, a broad mountain with an integrated cave mouth and lanterns, and a compass learning shrine. Requirements included SNES-style 3/4-view pixel art, dark muted purple outlines, warm ivory highlights, rounded stone matching the terrain cliffs, three or four shades per material, consistent upper-left lighting, no photographic peaks, no text and no external background plates.

The first result did not provide enough gutter between sprites. A second generation edited the atlas into a square two-by-two arrangement with generous transparent padding, preserving the designs. The final imported source is `Assets/Journey/Resources/Journey/WorldAtlas.png`. Runtime sprite bounds are measured against that 1254×1254 source in `JourneyWorld.Atlas`, and normalized to the imported texture size. Point filtering, no mipmaps, transparent edges and uncompressed import preserve the intended pixels.

Original output files remain outside the repository in the generator's output directory. Only the selected final atlas is included. Existing third-party asset licenses and source files remain unchanged.

A second original atlas, `SentinelAtlas.png`, replaces Fall's enlarged 32-pixel placeholder. The prompt used the existing terrain and old Sentinel as references, requesting a stone-and-wood golem with amber eyes, copper autumn leaves, mossy rounded shoulders and a chest rune. Four subtle idle poses share a two-by-two layout, perspective, scale and foot baseline. The imported atlas is capped at 512 pixels with point filtering; the guardian uses all four poses and retains the existing quiz, strike feedback and collision logic.

## Blossom Valley homepage

`MenuLandscape.png` is an image-generation edit of the owner-supplied “Ambros’ Journey_ Blossom Valley.png” reference. The prompt preserves the dark blossom tree, pink flowers, cloaked explorer, sunset mountains, castle, waterfalls and cyan portal; removes all lettering, button frames and ornaments; and reconstructs calm, dark foliage beneath the left menu. All title text, decorative rules and menu actions are rendered by Unity, not baked into the artwork. The landscape uses bilinear filtering, no mipmaps, a 2048-pixel import limit and high-quality compression. Aspect-fill cropping preserves proportions and the left menu's backdrop.

At this asset-generation stage the live title and menu used Spectral Regular by Production Type, distributed under the SIL Open Font License. The source and license remain, but current live text uses the supplied Determination font instead. Source: https://github.com/google/fonts/tree/main/ofl/spectral. The unmodified TTF and its OFL license are included together in `Assets/Journey/Resources/Journey/`.

## Seasonal cohesion assets

Six additional original PNGs were generated on October 26, 2026. The prompts referenced the existing seasonal terrain, requesting a consistent three-quarter top-down pixel-art view, muted dark outlines, upper-left lighting, warm timber, rounded gray stone, clear ground pivots, generous isolated spacing, and no words. Winter prompts explicitly prohibited green grass islands and rectangular ground plates. No third-party game screenshots were used as generated image inputs.

| Resource | Contents and sprite layout |
| --- | --- |
| `WinterNature.png` | Snow fir, bare oak, snow boulder and berry shrub; two columns, top row 66% of source height. |
| `VillageDetails.png` | Noticeboard, sign, workbench, bench, wood rack, harvest cart, planter, well and lantern; three columns with measured boundaries 0/.35/.64/1, rows 0/.39/.665/1 from top. |
| `SeasonCottages.png` | One timber-and-stone cottage in Spring, Summer, Fall and Winter treatments; equal two-by-two cells. |
| `DiscoveryDetails.png` | Stone portal, compass relic, neutral rocks and crystal cluster; two-by-two, split at 55% from left/top. |
| `UtilityDetails.png` | Raised garden, supplies, reading lectern and practice target; equal two-by-two cells. |
| `FieldCard.png` | Blank cream parchment with walnut frame and copper pins, cropped and nine-sliced in `FieldCardSkin`. Text is rendered in Unity. |

`WorldCohesion` trims transparent sprite padding at import-independent runtime coordinates, assigns ground pivots, and sizes each family consistently. World textures use point filtering, no mipmaps, a 1024-pixel import cap and uncompressed alpha; readable pixels support the one-time bounds calculation. Generated art is static texture data, not a live image-generation dependency. The old `WorldAtlas` remains in use for the integrated cave mountain; its former cottage and shrine cells are superseded by the new families.
