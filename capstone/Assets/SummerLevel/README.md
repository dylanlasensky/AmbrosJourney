# Summer Valley: Level 2

The married Warriors teach when AI help is useful, when real people are needed, and how to keep your own practice and judgment. Privacy, classroom rules and human support carry forward from Spring.

## Play and route

Open the inner `capstone` project with Unity **6000.6.2f1**. Use the main menu, the completed Spring portal, or **Capstone > Summer > Play Summer Only**. `SummerWorld` builds the map at runtime from the scene/Resources prefab. Do not rebuild Spring to make Summer.

1. Meet the Warriors; the Ancient Mage arrives with Ambros.
2. Workshop: choose useful festival brainstorming help and ask the builder to inspect a damaged bridge.
3. Training garden: choose a first-step hint, interact with **Sun > Seed > Leaf**, and ask for feedback on your own draft rather than a replacement.
4. Message post: remove the unneeded name, fictional street address and password from Ambros's actual example, then choose real support for a friend. This is an authored fictional message, not collected personal data.
5. Each completed mission awards a ward piece and a field note: **Pick the Right Kind of Help**, **Leave Room to Practice**, **Share Less, Ask People Too**.
6. Three wards open the southern **Suncrest Guardian**, a golden sunflower-maned lion with leaf armor. Score **4/5** on its five questions. Wrong answers explain the issue; retries preserve missions and depleted courage replenishes.
7. Earn Independent Thinker, recruit both Warriors, close the final conversation and walk through the unlocked Fall portal.

Three optional artifacts are available. Progress, partial missions and exact locations are saved by JourneySession and survive chapter revisits. The 5-6 minute first-time pacing estimate remains unmeasured.

## Presentation and assets

All seasons share Determination, parchment panels, question buttons, objective cards, framed labels, a journal and gold compass. Summer uses green accents. Its companions use the shared trail/formation behavior, including the Mage's walking frames. Normal controls, pause, saves and guidance are documented in the repository README and JourneyCompletion.

`SummerLevel` owns interactions; `SummerProgress` owns session state and guarded scoring; `SummerWorld` owns layout; `SummerPortalBridge` handles the Spring connection. The field named `orc` remains for source compatibility, but the active boss, labels and dialogue are Suncrest Guardian. `SeasonBossVisual` uses the new generated atlas and distinct idle/attack poses. Original Hero Pack art remains licensed source/fallback material; it is not the active guardian appearance.

The existing Summer terrain, character packs, supporting props, font/shader licenses and `Art/ASSET_SOURCES.json` retain their original credits. The displayed font is now the supplied Determination, not the older SummerBody font. See `docs/Bosses.md` for new art prompts and ownership.

## Verification

**Capstone > Summer > Validate Summer Progression** checks prerequisites, wrong answers, pattern order, privacy selection, courage recovery and boss thresholds. Hosted tests cover all 32 guardian combinations. The connected Unity checks cover live callbacks, portals, text fitting, boss animation hooks, saves and collection. Run them in an isolated checkout, not over unsaved Editor work.

See `docs/Testing.md`, `docs/QuestionRefresh.md` and `docs/CurrentValidation.md`. First-time comprehension, pacing and audio balance still need people to play the game.
