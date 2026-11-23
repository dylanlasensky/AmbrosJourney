# Seasonal scenery and portal placement

The scenery pass was based on `64f380a`, including the sound-effects refresh and open-journal pause screen from PR #13. The current behavior below includes the November 22 interaction-label and manual-guidance updates; older captures remain evidence of the earlier art pass.

## What changed

- Twelve artifacts now have individual silhouettes, materials and seasonal palettes, with matching collection-screen icons. Artifact IDs are unchanged, and collection progress belongs to the current run.
- Four generated seasonal portals replace the shared green portal sprite. Summer and Fall arrivals move closer to their paths; Winter's arrival moves onto its diagonal approach. Southern exit gates align with the existing trail centre. Decorative destination labels are removed; the objective HUD explains where to go next.
- Portal landings join the route at the threshold. The doorway-spur generator now samples cell centres symmetrically instead of biasing paths to the right.
- Spring gets woodland logs, ferns and gardening supplies; Summer gets watering supplies, pollinator hives and shoreline reeds; Fall gets harvest baskets, mushroom stumps and fuel; Winter gets snow-covered expedition supplies, shrubs and stones.
- New props use authored anchors with at most 1.2 units of adjustment. Their bases avoid routes, collision, collectible approaches, existing mission props and one another. Crowded candidates are omitted. Foreground canopies are cleared where they hide landmarks or new details.
- The Summer garden bench moves below the planting beds. Fall's orchard and kiln supply stacks are reduced and spaced. Winter's redundant entrance lantern is removed.
- Echo Cavern uses a modest entrance backed by overlapping snow-covered ridge and foothill pieces, with a snow apron at ground level. Continuous frontage/rear collision now prevents walking behind the ridge. The three-unit doorway lets the player walk inside after the Warden challenge and the Little Mage briefing. Press E beside the Mage to start that conversation; it no longer plays automatically. Archive crystals form small wall-side mineral groups instead of a straight row.
- Spring's Mage collider is now a trigger: conversation still uses proximity, but the companion cannot physically block a doorway or be treated as a wall by the compass. A focused probe reproduced a blocked route when the Mage stood at the portal and verified a 67-step route after the fix.

Overhead [E] cards follow actual interaction targets and appear only at their usable distance, with extra reach for bosses and guides. They avoid Ambros and the fixed HUD, and hide during conversations, journals, pause and transitions. Decorative location headers and floating route directions are removed; carved signs remain scenic props. The artifact HUD says 0/12 Artifacts, and pickups show a compact [E] Collect artifact card. The arrow remains available in Winter; the snowy compass outage is retired. [Historical feedback record](UserTestingFeedback.md)

## Review and validation

The Unity integration suite covers all seasons and transitions, both Winter bosses, current-run outdoor/cave revisits, pause/back controls, all twelve pickups, achievement, certificate export and fresh-run resets. There is no persistent progress file or Continue action. Worldcraft checks additionally verify distinct artifact art, seasonal portal selection, route and collision clearance, canopy visibility, reachable doorsteps/portal thresholds and the Mage-in-doorway regression. Captures include full maps, nine survey views per season, landmarks, portals and both UI aspect ratios.

The keyboard tests now use a dedicated virtual keyboard so a background Editor does not inherit hardware key state. Human playtime, learning outcomes and browser/WebGL performance still require the separate user testing described in the Sprint 3 report.

Artwork, exact generation prompts and placement references: [SeasonalWorldcraft-Art.md](SeasonalWorldcraft-Art.md).
