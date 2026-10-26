# Seasonal world cohesion

The retired generated opening video and its capture tools are removed. New Journey enters Spring immediately while the teammate's intro is absent. Optional teammate video integration remains at `Assets/Resources/Journey/Intro.mp4`; no replacement intro or outro was generated.

## Art and placement

- Winter trees now use transparent snow-rooted fir and bare-oak sprites, with matching snowy rocks and shrubs. Cave rocks use neutral stone rather than outdoor snow caps.
- Authored buildings share a timber-and-stone cottage design with four seasonal treatments. Extra runtime cottages are removed. Fall retains three distinct landmarks: the forge, archive, and distant optional overlook. The overlook moves off its collectible.
- Every cottage has a connected doorstep trail and a clear entrance. Roof-adjacent wood supplies, garden boxes, harvest carts, benches, and lanterns establish a purpose for the surrounding space.
- Trees rooted in trails are removed; dense root clusters are thinned. Scenery around door approaches and Spring's dock is cleared. Small additions reject occupied positions, important interaction areas, and walking paths, using a nearby safe spot or omitting the prop.
- Noticeboards, carved signs, workbenches, garden beds, supplies, lecterns, targets, portals, crystals, and artifacts use a shared visual vocabulary. Readable labels remain live text, not generated lettering.
- All four seasons use the same framed parchment cards. Nearby world labels appear within eight world units, reducing distant text clutter. Existing water movement, lantern flicker, seasonal weather, pause behavior, and frame limits remain active.

## Reference decisions

The official [Stardew Valley press kit](https://www.stardewvalley.net/press/) farm and Night Market screenshots informed building groups, open door aprons, paths leading to useful destinations, and contextual supplies. Those images were reviewed for composition only; no Stardew artwork was imported. Generated assets used the project's existing seasonal terrain as the art reference. More decoration is not always better: a prop is omitted when it crowds a route or interaction.

## Verification

The Unity integration suite covers the complete four-season route, both Winter bosses, failure/retry flows, menu callbacks, pause, exact outdoor and cave save restoration, all twelve artifact pickups, achievement, certificate export, reset, and direct entry to Spring without a placeholder video.

The additional world audit asserts the expected authored building counts (1/2/3/2), computes a reachable route to every doorstep, verifies Winter trees use the snow atlas, and renders all outdoor maps, the cave, every doorstep, arrival points, and a nine-view survey per season. These outputs are in ignored `capstone/Logs/WorldAudit/`; selected review captures are in `docs/WorldScreenshots/`.

This is a visual and functional polish pass, not a claim that every asset has been replaced or that laptop thermal behavior has been measured. Existing licensed character art, terrain, and gameplay barriers remain. Building entrances are scenic landmarks; this change does not add house interiors.
