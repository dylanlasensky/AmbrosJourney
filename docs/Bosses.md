# Seasonal guardians and the final AI Keeper

Four new original sprite atlases bring the remaining bosses into the same visual family as Fall. **Fall's Bramble Sentinel texture, four-pose animation and scale are unchanged.** Boss locations, learning prerequisites, save state and passing scores are preserved.

| Encounter | Active design | Animation and threshold |
| --- | --- | --- |
| Spring: **Blossom Guardian** | Mossy woodland tortoise with bark/ivory plates, fresh leaves and a pink blossom canopy. Replaces the old displayed Thornhide name. | Two idle poses, raised-paw windup and forward stamp; 4/5. |
| Summer: **Suncrest Guardian** | Golden sunflower-maned lion with green leaf armor, bronze paws and an amber sun core. Replaces the displayed Orc. | Two idle poses, raised-paw windup and swipe/stamp; 4/5. |
| Fall: **Bramble Sentinel** | Existing stone/wood autumn guardian with copper leaves and amber rune. | Existing four-pose loop and feedback preserved; 4/5. |
| Winter: **Frostbound Warden** | Large snowy bear-like golem with icy fists, crystal antlers and frosted fir growth. | Two idle poses, frost-fist windup and impact; 4/5 before cave entry. |
| Final: **Echo Keeper** | Towering ancient AI robot with dark iron/bronze plates, copper joints, violet rune circuitry and a cyan thinking core. Larger than the seasonal guardians. | Two idle poses, mechanical windup and heavy punch; eight safety activities, 7/8. |

## Source and integration

Final PNGs are committed under `capstone/Assets/Journey/Resources/Journey/Bosses/`:

- `SpringBlossomAtlas.png`
- `SummerSuncrestAtlas.png`
- `WinterFrostboundAtlas.png`
- `FinalEchoKeeperAtlas.png`

Each original is a transparent **1254 x 1254**, equal **2 x 2** atlas. Reading left-to-right, top-to-bottom: idle A, idle B, attack windup, attack impact. The generation request targeted 1024 pixels, but the returned originals are 1254; the runtime uses actual dimensions rather than assuming the requested size.

`SeasonBossVisual` slices equal cells once, anchors their ground contact from opaque foot rows while keeping horizontal pivots fixed, and gives each boss its own material. Idle frames loop; strikes play one non-looping windup/impact sequence using scaled game time so pause freezes them. Existing answer callbacks trigger the animation. Spring also flashes its boss on a correct answer; Summer/Winter retain their established player/boss feedback. Sorting stays grounded at the actor position. The final cell width is 8.6 world units, compared with 5.8/6.0/6.2 for the new seasonal bosses; transparent padding makes visible bounds smaller than cell width.

World setup attaches the visual at the existing encounter position. Legacy Hero Pack `Orc`/`Skeleton` frame references and the `SummerLevel.orc` field remain for compatibility; they are not the rendered boss forms. Original licensed source packs are not altered or removed.

## Generation record

Generated with the built-in image-generation tool, using the existing Fall `SentinelAtlas.png` as a **style reference**, not an edit target. Prompts requested chunky shaded pixel art, the same elevated RPG view, complete bodies, consistent scale/foot anchors, true alpha, separate idle/windup/impact poses, no text and no ground plates. [Exact prompt set](BossGenerationPrompts.json)

The first Summer result let a tail cross the cell boundary. A targeted edit curled that tail inward and removed pixels intruding into the adjacent cell, preserving the four characters/poses. The corrected original is the imported Summer source. No Python image editing, external stock boss pack, new Unity package or runtime image-generation dependency was introduced.

Texture imports use Default, point filtering, no mipmaps, uncompressed alpha, readable pixels and NPOT scaling disabled. This preserves the exact grid and supports the one-time contact-row calculation.

## Review captures

| Boss | Idle in its world | Attack impact |
| --- | --- | --- |
| Spring | [Idle](BossScreenshots/SpringBlossom-0.png) | [Impact](BossScreenshots/SpringBlossom-3.png) |
| Summer | [Idle](BossScreenshots/SummerSuncrest-0.png) | [Impact](BossScreenshots/SummerSuncrest-3.png) |
| Winter | [Idle](BossScreenshots/WinterFrostbound-0.png) | [Impact](BossScreenshots/WinterFrostbound-3.png) |
| Final robot | [Idle](BossScreenshots/FinalEchoKeeper-0.png) | [Impact](BossScreenshots/FinalEchoKeeper-3.png) |

These are actual Unity renders. Automated art checks confirm frame assignment and strike signals; people should still judge motion, intimidation, world scale and readability in play. [Current validation](CurrentValidation.md)
