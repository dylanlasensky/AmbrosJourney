# Winter: Frostveil and Echo Cavern

Use Unity **6000.6.2f1** with the inner `capstone` project. Start at the main menu, complete Fall and walk through the unlocked Winter gate after closing its conversation, or use **Capstone > Winter > Play Winter Only**.

## Learning route

Little Mage teaches original sources, independent evidence, dates/context, uncertainty and correcting mistakes. The two regions share one session:

1. **Frostveil:** snowy trails around unsafe ice, lantern directions, the Wayfarer's Ledger and Frozen Observatory. Each station teaches a note before two evidence choices.
2. Complete both outdoor seals, then challenge the **Frostbound Warden** outside the cave. This giant white bear-like guardian has icy fists, crystal antlers and snowy fir growth. Its five questions require **4/5**. Passing earns Winter's crest, recruits Little Mage and opens the entrance.
3. **Echo Cavern:** bent passages connect stone chambers, crystals, an archive branch and a sealed northern hall. The Echo Archive teaches uncertainty/corrections before its two choices. You can return outside without losing lessons.
4. All three seals unlock the **Echo Keeper**, a large ancient AI robot with iron/bronze armor, rune circuitry and a cyan core. Eight new AI safety activities review all seasons; **7/8** passes. The activities include privacy selection, prompt assembly, matching, evidence ordering and a new judgment scenario. Dragging has click/number-key alternatives.
5. Victory starts the optional teammate outro, then the personalized PDF certificate. The current implementation does not require walking back to Spring.

Winter's independent-evidence note teaches that AI agreement is not separate evidence. The final can therefore test this idea without requiring the optional twelve-artifact collection. Three artifacts are in Winter, with the third inside the cave.

## Controls and systems

WASD/arrows move; E/Enter interacts/continues; 1/2/3 or mouse selects regular answers; J opens notes; Esc pauses; F collects; R returns to the current region's arrival; M toggles seasonal audio. The final panel explains its own selection, matching and arrangement controls and grades only on Check/Enter.

Explorer guidance uses broad bearings at long range, with periodic outdoor snowfall obscuring the compass. Landmarks, lanterns and signs remain clues. Pause > Guidance: Assisted restores exact guidance. JourneySession saves the exact outdoor/cave location, partial lessons, bosses and artifacts and supports revisits.

`WinterWorld` creates separate outdoor/cave groups from the Resources prefab while keeping player, party and UI alive. `WinterProgress` and `FinalChallenges` are plain C# grading data used by hosted tests. `FinalActivityPanel` renders activities in the existing conversation frame. `SeasonBossVisual` renders the two new boss atlases and their idle/windup/impact animation; existing interaction positions and scoring rules are retained.

## Art, font and checks

Terrain and companions reuse the existing licensed packs; shared Journey scenery provides snow-rooted trees, cottages and cave details. New Warden/Keeper art is generated specifically for this game. Original skeleton frames remain in source references, not as active boss appearances. All game text uses Determination with existing point sizes.

See `docs/Bosses.md` for prompts, `docs/WinterArtPrompts.md` for earlier cave art, `docs/JourneyCompletion.md` for saves/videos, and `docs/Testing.md` / `docs/CurrentValidation.md` for verified scope and limitations.
