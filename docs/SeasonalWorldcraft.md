# Seasonal scenery and portal placement

The scenery pass was based on `64f380a`, including the sound-effects refresh and open-journal pause screen from PR #13. This page is reconciled with the November 12 feedback changes; older captures remain evidence of the earlier art pass.

## What changed

- Twelve artifacts now have individual silhouettes, materials and seasonal palettes, with matching collection-screen icons. Artifact IDs and save data are unchanged.
- Four generated seasonal portals replace the shared green portal sprite. Summer and Fall arrivals move closer to their paths; Winter's arrival moves onto its diagonal approach. Southern exit gates align with the existing trail centre. Destination labels follow the new artwork height.
- Portal landings join the route at the threshold. The doorway-spur generator now samples cell centres symmetrically instead of biasing paths to the right.
- Spring gets woodland logs, ferns and gardening supplies; Summer gets watering supplies, pollinator hives and shoreline reeds; Fall gets harvest baskets, mushroom stumps and fuel; Winter gets snow-covered expedition supplies, shrubs and stones.
- New props use authored anchors with at most 1.2 units of adjustment. Their bases avoid routes, collision, collectible approaches, existing mission props and one another. Crowded candidates are omitted. Foreground canopies are cleared where they hide landmarks or new details.
- The Summer garden bench moves below the planting beds. Fall's orchard and kiln supply stacks are reduced and spaced. Winter's redundant entrance lantern is removed.
- Echo Cavern uses a modest entrance backed by overlapping snow-covered ridge and foothill pieces, with a snow apron at ground level. Continuous frontage/rear collision now prevents walking behind the ridge. The three-unit doorway automatically enters after the Warden challenge; a separate Little Mage conversation is optional before walking inside. Archive crystals form small wall-side mineral groups instead of a straight row.
- Spring's Mage collider is now a trigger: conversation still uses proximity, but the companion cannot physically block a doorway or be treated as a wall by the compass. A focused probe reproduced a blocked route when the Mage stood at the portal and verified a 67-step route after the fix.

Overhead interaction cards now follow the actual targets, use [E] where required, and avoid duplicates and overlaps. The artifact HUD says 0/12 Artifacts. The compass's first snowy outage has an icicle-edged hint for the next unfinished landmark. [Current feedback record](UserTestingFeedback.md)

## Review and validation

The Unity integration suite checks all seasons and transitions, both Winter bosses, exact outdoor/cave saves, pause/back controls, all twelve pickups, achievement, certificate export and reset. Worldcraft checks additionally verify distinct artifact art, seasonal portal selection, route and collision clearance, canopy visibility, reachable doorsteps/portal thresholds and the Mage-in-doorway regression. Captures include full maps, nine survey views per season, landmarks, portals and both UI aspect ratios.

The keyboard tests now use a dedicated virtual keyboard so a background Editor does not inherit hardware key state. Human playtime, learning outcomes and browser/WebGL performance still require the separate user testing described in the Sprint 3 report.

Artwork, exact generation prompts and placement references: [SeasonalWorldcraft-Art.md](SeasonalWorldcraft-Art.md).
