# Winter: Frostveil and Echo Cavern

Open the inner `capstone` project with Unity **6000.6.2f1**. Use **Capstone → Winter → Play Winter Only**, or complete Fall and walk through its unlocked Winter gate. Close the gate conversation first.

Winter has two explorable maps in one session:

- **Frostveil:** a snowy woodland trail around unsafe ice, two evidence stations, the Little Mage, lantern signposts and a cave entrance on the northern ridge.
- **Echo Cavern:** a separate stone interior with connected chambers, a western archive detour, crystal formations and a sealed northern boss hall. Interact with the daylight exit to return outside. Progress survives both directions.

The Little Mage teaches verification: find the original source, compare independent evidence, and admit uncertainty or correct mistakes. Read each of three notes before its two challenges. All three evidence seals unlock the cave boss, the **Frostbound Warden**. Score 4/5 on its five-question challenge to earn the Winter crest and recruit the Little Mage. Mistakes and retries preserve lessons.

The final return-home chapter is not implemented. Winter completion says this explicitly. Progress is still session-only.

## Controls and ownership

WASD/arrows move; E interacts/continues; 1/2/3 or mouse selects answers; J opens the journal; Escape closes it or a conversation after the introduction; R returns to the current map's arrival; M toggles Winter audio.

`WinterWorld` creates both map groups from the same Resources prefab used by `Winter.unity`. Outdoor objects and cave objects are activated separately; the player, party, progress and UI remain alive. `WinterProgress` has no Unity dependency and is exercised by hosted CI. `WinterPortalBridge` follows the existing seasonal handoff convention. `WinterTools` rebuilds the scene/prefab from source assets through the Editor.

## Art

Terrain uses the repository's existing Mana Seed winter sample. Characters reuse the existing Hero Pack (Ambros, Ancient Mage, Warriors, Emberfist, Little Mage and Skeleton); original credits and licenses remain in their asset folders. Winter references shared existing artwork rather than duplicating all prior seasons.

`Art/CaveEntrance.png`, `Art/Crystal.png` and `Art/Lantern.png` are new generated artwork, created with the built-in image-generation tool for this chapter. They are imported with point filtering, transparency, no mipmaps and no texture compression. See `docs/WinterArtPrompts.md` for the art direction and prompts.

The shared `SeasonOverlay` now renders all four seasons. Seasonal accents vary while typography, parchment framing, buttons, objectives, compass and journal use one layout.
