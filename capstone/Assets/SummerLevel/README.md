# Summer Valley — Level 2

Summer teaches choosing appropriate AI help, preserving learning practice, following classroom rules, privacy, and seeking support from people. This implements the three-mission and Orc breakdown discussed for Sprint Three. The 5–6 minute pacing is a target awaiting first-time user testing.

## Play

Open the inner `capstone` folder in Unity **6000.6.2f1**. Select **Capstone → Summer → Play Summer Only**, or open `Assets/SummerLevel/Summer.unity` and press Play. The map is assembled at runtime from the scene's `SummerWorld` component, so the Edit-mode hierarchy initially shows the world definition. Do not run the original Spring rebuild tools to create Summer.

For the connected route, start the existing opening/Spring normally, complete Spring, close its final conversation, then **walk through the existing unlocked Summer portal**. `SummerPortalBridge` installs itself at runtime, checks `SpringComplete`, and transitions to the new Summer Resources prefab. It does not replace Spring's dialogue or edit its scene. Summer is loaded into a runtime scene without modifying the existing build scene list. The old Spring gate dialogue still says the old prototype ends there; close it and walk through the gate to continue.

The resource prefab is included automatically in player builds. No Pipeline package or editor-only loader is required by Summer. Runtime progress is session-only, like the existing prototype.

## Route and controls

- WASD / arrows: move. E: interact or continue. 1/2/3: select an answer. Mouse buttons also select answers.
- J: journal when dialogue is closed. Escape: close journal/dialogue after the introduction.
- R: return to Summer arrival without resetting progress. M: mute/unmute Summer music and effects.
- Meet the married Warriors and arrive with the Ancient Mage.
- Workshop, northwest: festival brainstorming and damaged-bridge decisions, with explanations and retries.
- Training garden, southwest: choose a hint; walk to Sun → Seed → Leaf stones, pressing E at each; answer the classroom-practice choice.
- Message post, northeast: remove all three fictional private details, then choose real human support for an upset friend.
- Collect three ward pieces; the compass and journal guide the route. The ward opens the southern Orc arena.
- Answer the five Orc questions. A 3/5 score fails; 4/5 or 5/5 passes. Feedback appears after every answer. Retries preserve the missions; depleted courage replenishes.
- Pass to earn Independent Thinker rank, recruit both Warriors, restore the ward, and open Fall. The Fall portal introduces prompt engineering; no playable Fall is fabricated.

## Assets and implementation

All level additions are under `Assets/SummerLevel/`. Existing files are untouched, including Spring, the intro, package manifests, and project/build settings.

The Ambros idle/walk textures are pixel-identical copies of Spring's extracted frames. Mage, married Warrior and Orc frames come from the existing character packs. Summer ground and foliage are cropped from the existing Summer pack. New supporting pixel props include the workshop, stepping stones, message post, ward, shields and machinery. Preserve the original asset-pack notices and licenses. The copied TMP shader and its dependencies include Unity's uGUI license in `Shaders/`. The new Resources folder includes a static Liberation Sans font, its OFL license, a TMP settings asset, and a material retaining the shader for builds. No pre-existing TMP settings asset was present or edited.

`SummerWorld` owns layout/art references. `SummerLevel` owns interactions, responsive Canvas/TMP UI and original synthesized music/effects. `SummerProgress` owns session state and pass/retry rules. `SummerActor` animates movement, attacks and companions. `SummerPortalBridge` provides the add-only Spring connection. These scripts do not write files during gameplay.

## Checks

Use **Capstone → Summer → Validate Summer Progression** to check lesson prerequisites, wrong-answer retries, stone order, private-field selection, courage recovery, boss thresholds and the Resources prefab.

The level was also checked against all 32 combinations of Orc answers, including retry and progress preservation. `Art/ASSET_SOURCES.json` records the included artwork. All assets needed to play are included; no generation script is required.

Before calling the level final, play the full Spring-to-Summer route, inspect UI at the target window size, listen to the audio, and time first-time players. Validate clean standalone builds on the team's intended platforms. The existing repository has no persistent save contract; Summer does not add one.

## Standalone compatibility

Summer has its own camera and an optional runtime Spring adapter. It compiles and plays in older checkouts without Spring scripts; the portal connection activates when the newer Spring scene is present.
