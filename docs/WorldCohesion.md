# Seasonal world cohesion

## Active visual systems

- Winter trees use transparent snow-rooted fir and bare-oak sprites with snowy rocks/shrubs. Cave rocks use neutral stone rather than outdoor snow caps.
- Authored buildings share a timber-and-stone family with four seasonal treatments. Their front steps connect to trails. Roof-adjacent supplies, planters, harvest carts, benches and lanterns give places a purpose.
- Extra generic runtime cottages are removed. Fall retains three distinct landmarks and the optional overlook sits clear of its collectible.
- Trees rooted in trails, dense root clusters, crowded door aprons and dock clutter are cleared. Small decorations reject occupied routes/interaction areas and use a safe nearby position or are omitted.
- Noticeboards, carved signs, workbenches, gardens, lecterns, targets, portals, crystals and artifacts share a visual vocabulary. Labels are live Determination text, not generated lettering.
- All seasons use framed parchment cards; nearby labels appear within eight world units and hide during conversations/journals. Interactive targets use one attached overhead [E] card, artifacts use [F], and automatic portals keep walk-through labels. Cards follow moving sprites, avoid overlap, and replace the old bottom interaction popup. Finished Garden Warrior dialogue no longer advertises an interaction. Water, lanterns, weather, pause behavior and render limits remain active.
- The Blossom Guardian, Suncrest Guardian, Frostbound Warden and robot Echo Keeper now match their environments with separate idle and attack poses. Fall's existing Bramble Sentinel is unchanged. See [Bosses](Bosses.md).

Winter's arrival Mage is clear of its portal; the Warden is beside the visible cave opening. The ridge has continuous collision up to the boundaries and a closed rear, with a three-unit automatic doorway. The first outdoor Explorer whiteout uses an icy, progress-aware notice; its icicles are native UI geometry rather than a new texture asset.

## Ownership and generation

`WorldCohesion` skins existing authored objects and adds contextual furnishings; `NaturalTrail` supplies worn routes. `JourneyWorld` owns artifacts, environmental motion and Fall's existing Sentinel atlas. `SeasonBossVisual` is attached by the Spring/Summer/Winter world setup and owns only the four new boss visuals. Ordinary gameplay loads committed assets; it does not call an image service.

Earlier direction used the official Stardew Valley press imagery for connected building approaches and grouped supplies. Those images were composition references, not imported game art. Generated art used the project's existing terrain/guardian style as reference. More decoration is not always better, so route clearance takes priority.

## Audit scope

The world checks assert authored building counts of 1/2/3/2, routes to every doorstep and Winter tree texture assignment. Whole-map, arrival, doorway and nine-view surveys are saved under ignored `Logs/WorldAudit`. Selected older world captures remain in `docs/WorldScreenshots`; current boss captures are under `docs/BossScreenshots`.

The connected suite checks all seasons, both Winter bosses, mixed finale/retries, pause, exact-location saves, all twelve artifacts, certificate, reset and direct Spring entry while the teammate intro is absent. See [Testing](Testing.md) and [current results](CurrentValidation.md).

The retired procedural opening is not shipped; the teammate still owns intro/outro clips. Building entrances are scenic landmarks, not newly playable interiors. Existing terrain/character licenses remain in force. User testing and target-laptop performance measurement are still required.
