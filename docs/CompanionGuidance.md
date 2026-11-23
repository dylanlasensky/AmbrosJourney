# Companion conversations

NPC dialogue opens only after the player approaches and presses **E**. Arriving in a season, walking toward a task or returning to a chapter does not start a speech caption or complete an introduction. The objective and player arrow identify the next guide or lesson.

- Spring starts with the controls card, then directs the player to the Ancient Mage. E opens his three-page introduction; completing it unlocks the field-note challenges.
- Either Summer Warrior can start the greeting beside the arrival portal. Finishing or dismissing that greeting begins their walk to the workshop. Fall uses the same flow with Emberfist and Harvest Orchard. They stay within five world units while escorting and wait if the player stops. Arriving never opens a lesson automatically; E opens the guide's task instructions or the nearby board. Lesson objects take priority over a nearby companion.
- Winter starts with Little Mage beside the portal. After the Warden victory, E beside Little Mage at the cave opens the briefing. Completing it with E, Enter or Continue permits the walk-in entrance. Escape closes it without granting permission, and E can reopen it. Inside, he follows silently without a talk label.

Guide hints appear within four world units, ordinary notes within their 2.6–2.8-unit reach, artifacts within 2.3 and bosses within 5.5. Finished companions hide their talk hints. Decorative location, portal and route titles are removed; physical signs and landmarks remain. Cards still avoid the player and HUD, and hide during conversations, journals, pause and transitions.

E or Enter advances a conversation; Escape closes it. Number keys or mouse clicks choose answers. No unsolicited companion caption is created by any season controller. Same-run chapter revisits retain completed conversations, while refresh/end-run starts fresh.

The updated `CompanionGuidanceChecks`, transition, integration and boss-exit runners exercise these flows. See [presentation clarity validation](PresentationClarityValidation.md) for the current executed checks and their limits. Earlier automatic-caption checks in historical records do not describe this behavior.
