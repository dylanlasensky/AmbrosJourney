# Implemented user-testing feedback

This maintained summary replaces the scattered dated feedback and validation notes. The original submitted sprint documents remain unchanged under [Reports](../Reports/README.md). Current verification and its limits are recorded in [CurrentValidation](CurrentValidation.md).

## Interaction and readability

- Determination is shared across menus, questions, notes, controls and certificate. Existing UI point sizes and parchment frames are retained.
- One nearby overhead `[E] NAME` card identifies an available NPC conversation. Completed conversations hide their cards until another briefing becomes available. Cave Little Mage has no conversation. Field notes/artifacts have their own contextual cards; the redundant bottom interaction prompt is removed.
- Pause and Artifacts sit immediately below the top-right objective/lesson panel. The artifact counter explicitly says `Artifacts`.
- Correct responses start with `Correct!`; explanations remain after both outcomes. Quiz totals remain fixed at the full question count. Courage/hearts and R-to-restart are removed.
- The name field glows on the main menu. The caret alignment fix is retained. A Determination-font AI-content disclaimer fades in before the original intro.

## Learning, navigation and encounters

- Questions use conversational middle-school language, progressively applying AI foundations, responsible use, useful prompts and source verification. The finale uses eight mixed activities rather than repeating seasonal questions. Matching explains how to cycle choices. [Curriculum](QuestionRefresh.md).
- The gold objective arrow stays available through all seasons, including Winter and Echo Cavern. The old frozen-compass rule and guidance modes are retired.
- Companions use spaced, collision-aware movement. Conversations begin deliberately with E; escorts wait at their destinations.
- Seasonal bosses match their setting and animate. Defeated guardians dissolve into pixels; Spring, Summer and Fall then assemble their exit portals. Flags/shields are removed. Victory dialogue gives a concise takeaway instead of explaining the animation.
- Winter retains the natural cave entrance, continuous ridge/rear collision and no extra portal overlay. After Warden victory, the player must finish Little Mage's outside briefing before walking through the doorway. Arrival Little Mage is clear of the landing portal.
- Scene-local players and canonical spawns prevent the Spring-to-Summer handoff from borrowing the outgoing player's position. Same-run revisits retain chapter progress.

## Completion, audio and browser delivery

- Final victory dissolves Echo Keeper and drops an E-pickup certificate scroll. Download PDF keeps the certificate open. Exit/Escape returns to the cave and reveals the home portal. Walking through it plays the supplied outro and returns to the menu.
- Main-menu Spring music stops when a valid journey starts. Seasonal and battle music use the existing session-wide preference. **Music On/Off is available in the pause journal and main-menu Settings, with no persistent gameplay music button.** It does not mute sound effects; M remains the separate all-audio mute shortcut.
- Intro/outro originals are preserved. Web packaging uses the responsive template, bounded Retina rendering, cinematic focus handling and browser PDF download bridge.
- Runs are intentionally in-memory only. There is no Continue/autosave/account/backend. Ending a run, closing or refreshing starts fresh; old storage is ignored without deletion. The certificate remains accessible after completion until a new run or reload.

The team repeatedly tested individually and gathered feedback from friends and roommates. No middle-school user testing or measured learning outcome is claimed. That remains future work.
