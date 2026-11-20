# Moonbranch compass and world polish

The original polish pass was based on `69c2564`. Current behavior below includes the November 20 feedback; the validation section remains evidence from the earlier pass. See [current validation](CurrentValidation.md).

- The active guide is a small gold, single-headed pixel arrow near Ambros, rotating continuously toward the next route waypoint. The older seasonal compass rings/needle artwork remains provenance, not the visible HUD. The first Explorer whiteout opens a saved, once-only icicle-edged notice naming the next unfinished Winter landmark. After the notice, the outdoor arrow remains frozen through clear weather, guidance changes and reloads. It works inside Echo Cavern and hides again outside. Assisted selected before the notice prevents the initial freeze. The notice waits for dialogue and does not appear in the cave.
- Escape closes the active conversation, journal, artifact collection, settings or reset confirmation. From the earned certificate, Escape and Exit resume the cave and assemble its home portal; walking through that portal opens the main menu. When the certificate is viewed from the main menu, Exit/Escape returns directly to that menu. Artifacts opened from Pause return to Pause; artifacts opened from gameplay return to gameplay. `I` opens artifacts. Menu focus, Tab/Shift-Tab, arrow navigation and Enter support keyboard use alongside the existing mouse buttons and dialogue shortcuts.
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
