# Moonbranch compass and world polish

**November 22 session policy:** Persistent progress saving has been removed. New launches and browser refreshes start fresh; unlocked-season revisits retain progress only during the current run. Older save files/storage are ignored and left untouched. Results below are historical evidence for their named builds, including their former save/Continue behavior; they do not verify this change.

The original polish pass was based on `69c2564`. Current behavior below includes the November 21 feedback; the validation section remains evidence from the earlier pass. See [current validation](CurrentValidation.md).

- The active guide is a 52-pixel gold-and-cream, single-headed arrow with an opaque dark outline near Ambros. It rotates continuously toward the next required destination and has a slow 0–5% size pulse. It draws above nearby name cards but below lessons and the journal. It stays available outdoors in Winter and inside Echo Cavern, with no frozen-arrow notice. Direct guidance does not run path searches; the player chooses a walkable route around scenery. Older seasonal compass artwork remains provenance.
- Escape closes the active conversation, journal, artifact collection, settings or reset confirmation. From the earned certificate, Escape and Exit resume the cave and assemble its home portal; walking through that portal opens the main menu. When the certificate is viewed from the main menu, Exit/Escape returns directly to that menu. Artifacts opened from Pause return to Pause; artifacts opened from gameplay return to gameplay. `I` opens artifacts. Menu focus, Tab/Shift-Tab, arrow navigation and Enter support keyboard use alongside the existing mouse buttons and dialogue shortcuts.
- Companion advice and first-station escorts start automatically; arrival captions do not open lessons. Closing Warden victory enables the cave tip and permission without a required NPC conversation. See [companion guidance](CompanionGuidance.md).
- Companions use separate nearby positions around Ambros in open ground, slower arrival movement and staggered resting adjustments. Narrow passages use checked breadcrumbs. Formation queries are cached; checkpoint/region relocations regroup around the leader.
- Ground cover is composed around existing tree clusters with asymmetrical offsets. Paths and collectible approaches stay clear. Fall supplies form orchard and kiln work areas. Summer has a cleared garden plot and short approach. Winter firewood is beside the research shelter. Prop placement considers other decorative footprints as well as paths and collision.

## Earlier validation snapshot

Unity 6000.6.2f1:

- `SeasonIntegrationChecks.Run`: passed four seasons, both Winter bosses, final quiz, all twelve artifacts, save/restore outdoors and in the cave, menus, certificate and reset. Includes updated compass and keyboard regressions in every season.
- `CompanionMovementChecks.Run`: passed live Fall travel, collision-safe movement, nearby resting positions and regrouping after a checkpoint jump across the river.
- `CertificateRewardChecks.Run`: passed the merged final-boss animation, dropped certificate, E pickup, reload recovery, duplicate protection, saved date and PDF export.
- Captured and inspected full outdoor maps, the cave, seasonal HUDs at 1280x720 and 1024x768, companion resting/checkpoint views, and garden/doorstep views. Traversable routes to all authored doorsteps and artifacts are checked in Unity.
- Mac standalone build: succeeded with 0 errors and 3 warnings. Asset integrity: 388 tracked GUIDs passed.
- Transparent PNG checks: empty ring centers and no visible art crossing atlas cell boundaries. Generated art, prompt provenance and Unity metadata are committed together; see `MoonbranchArtwork.md`.

The full-game harness needed its completed-profile fixture updated to include the new certificate pickup state; this changes test setup, not the teammate's reward flow. The companion and certificate checks run separately after the final full-game pass. These are automated local Editor checks and visual capture review, not a claim of exhaustive human playtesting.

Generated captures and builds are under the ignored `capstone/Logs` directory. No intro/outro video changes are included.
