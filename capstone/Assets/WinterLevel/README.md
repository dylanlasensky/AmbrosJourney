# Winter: Frostveil and Echo Cavern

Use Unity **6000.6.2f1** with the inner `capstone` project. Start at the main menu, complete Fall and walk through the unlocked Winter gate after closing its conversation, or use **Capstone > Winter > Play Winter Only**.

## Learning route

Little Mage teaches original sources, independent evidence, dates/context, uncertainty and correcting mistakes. The two regions share one session:

1. **Frostveil:** snowy trails between fir groves, lantern directions, the Wayfarer's Ledger and Frozen Observatory. Each station teaches a note before two evidence choices.
2. Complete both outdoor seals, then challenge the **Frostbound Warden** outside the cave. This giant white bear-like guardian has icy fists, crystal antlers and snowy fir growth. Its five questions require **4/5**. Passing earns Winter's crest and disintegrates the Warden. The original cave artwork stays; no portal overlay or portal-building animation is added. Close the victory message, approach Little Mage and press E to finish his outside cave briefing. Only then walk into the doorway automatically; Escape leaves the briefing unfinished. He follows the party inside.
3. **Echo Cavern:** bent passages connect stone chambers, crystals, an archive branch and a sealed northern hall. The Echo Archive teaches uncertainty/corrections before its two choices. Little Mage follows the party inside but has no conversation or [E] label there. Use the [E] return portal to go outside without losing lessons.
4. All three seals unlock the **Echo Keeper**, a large ancient AI robot with iron/bronze armor, rune circuitry and a cyan core. Eight new AI safety activities review all seasons; **7/8** passes. The activities include privacy selection, prompt assembly, matching, evidence ordering and a new judgment scenario. Dragging has click/number-key alternatives.
5. Victory crumbles the Keeper and drops a certificate scroll. Approach and press E to collect it, then view/download the personalized PDF. Exit or Escape resumes the cave and assembles a home portal; walking through it plays the supplied outro and returns to the menu. The certificate remains available until a new journey or reload. The current implementation does not require walking back to Spring.

Winter's independent-evidence note teaches that AI agreement is not separate evidence. The final can therefore test this idea without requiring the optional twelve-artifact collection. Three artifacts are in Winter, with the third inside the cave.

## Controls and systems

WASD/arrows move; E/Enter interacts/continues; 1/2/3 or mouse selects regular answers; J opens notes; Esc pauses; E collects; M toggles all audio; Music On/Off is in the pause journal. The final panel explains its own selection, matching and arrangement controls and grades only on Check/Enter.

The gold-and-cream arrow stays visible throughout Winter and points directly to the next objective. The cave uses a small player visibility circle and lantern pools; the final boss chamber brightens as the player enters. JourneySession retains locations, lessons, bosses and artifacts only during this run, including revisits. Closing or refreshing the game starts fresh.

`WinterWorld` creates separate outdoor/cave groups from the Resources prefab while keeping player, party and UI alive. `WinterProgress` and `FinalChallenges` are plain C# grading data used by hosted tests. `FinalActivityPanel` renders activities in the existing conversation frame. `SeasonBossVisual` renders the two new boss atlases and their idle/windup/impact animation; the Warden stands directly in front of the cave opening. Little Mage starts clear of the arrival portal. Scoring rules are retained.

The ridge frontage connects to the map boundaries and a rear barrier closes the space behind the entrance. The intended doorway is reachable and opens automatically after Warden victory and the completed outside Little Mage conversation.

## Art, font and checks

Terrain and companions reuse the existing licensed packs; shared Journey scenery provides snow-rooted trees, cottages and cave details. New Warden/Keeper art is generated specifically for this game. Original skeleton frames remain in source references, not as active boss appearances. All game text uses Determination with existing point sizes. Interactive notes, helpers, bosses, the outdoor-return portal and the certificate scroll use one overhead [E] card; artifacts use [E]. The duplicate bottom prompt is hidden. Correct feedback explicitly says Correct!, and all displayed score denominators remain fixed. Courage/hearts are removed.

See `docs/Bosses.md` for prompts, `docs/WinterArtPrompts.md` for earlier cave art, `docs/JourneyCompletion.md` for session behavior/videos, and `docs/Testing.md` / `docs/CurrentValidation.md` for verified scope and limitations.


See [second-round feedback changes](../../../docs/UserTestingFeedback.md) for the removed R shortcut, shared boss crumble, animated seasonal exits and cave-to-menu home portal.
