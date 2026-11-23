# Seasonal world cohesion

## Active visual systems

- Winter trees use transparent snow-rooted fir and bare-oak sprites with snowy rocks/shrubs. Cave rocks use neutral stone rather than outdoor snow caps.
- Authored buildings share a timber-and-stone family with four seasonal treatments. Their front steps connect to trails. Roof-adjacent supplies, planters, harvest carts, benches and lanterns give places a purpose.
- Extra generic runtime cottages are removed. Fall retains three distinct landmarks and the optional overlook sits clear of its collectible.
- Trees rooted in trails, dense root clusters, crowded door aprons and dock clutter are cleared. Small decorations reject occupied routes/interaction areas and use a safe nearby position or are omitted.
- Noticeboards, carved signs, workbenches, gardens, lecterns, portals, cave crystals and artifacts share a visual vocabulary. Interaction labels are live UI text, not generated lettering.
- Framed parchment cards appear only near usable interactions: field notes at 2.6 units in Spring or 2.8 elsewhere, guides at 4, bosses at 5.5, and artifacts at 2.3. Each card follows its actual target and says [E]; artifacts use the compact “Collect artifact” prompt. Cave darkness can shorten this range. Cards hide during conversations, journals, pause and scene transitions, and avoid Ambros and the fixed HUD.
- Decorative portal names, building headings and floating trail directions are removed. The physical carved signs remain part of the scenery, while the objective HUD and arrow provide guidance. NPC conversations start with E; cleared-season companions stop advertising a conversation. Water, lanterns, weather and render limits remain active.
- The Blossom Guardian, Suncrest Guardian, Frostbound Warden and robot Echo Keeper now match their environments with separate idle and attack poses. Fall's existing Bramble Sentinel is unchanged. See [Bosses](Bosses.md).

Winter's arrival Mage stands within talking distance of the landing, clear of the portal. The Warden stands in front of the cave opening. The ridge has continuous collision up to the boundaries and a closed rear, with a three-unit walk-in doorway unlocked by Warden victory and the completed Little Mage conversation. Press E beside the Mage for that briefing; it does not play automatically. The cave keeps its original artwork, without a portal overlay or building effect. Its tunnels reveal a small area around Ambros and their lanterns, while the final chamber brightens on entry and stays lit after victory. Winter's arrow remains available throughout; the frozen-arrow notice is retired. Snow and fir groves replace the former outdoor ice patch.

## Ownership and generation

`WorldCohesion` skins existing authored objects and adds contextual furnishings; `NaturalTrail` supplies worn routes. `JourneyWorld` owns artifacts, environmental motion and Fall's existing Sentinel atlas. `SeasonBossVisual` is attached by the Spring/Summer/Winter world setup and owns only the four new boss visuals. Ordinary gameplay loads committed assets; it does not call an image service.

Earlier direction used the official Stardew Valley press imagery for connected building approaches and grouped supplies. Those images were composition references, not imported game art. Generated art used the project's existing terrain/guardian style as reference. More decoration is not always better, so route clearance takes priority.

## Audit scope

The world checks assert authored building counts of 1/2/3/2, routes to every doorstep and Winter tree texture assignment. Whole-map, arrival, doorway and nine-view surveys are saved under ignored `Logs/WorldAudit`. Selected older world captures remain in `docs/WorldScreenshots`; current boss captures are under `docs/BossScreenshots`.

The connected suite covers all seasons, both Winter bosses, mixed finale/retries, pause, current-run chapter revisits, all twelve artifacts, certificate, fresh-run resets and direct Spring entry while the teammate intro is absent. Progress is held in memory for one sitting; returning to the menu ends the run and refreshing the browser starts fresh. See [Testing](Testing.md) and [current results](CurrentValidation.md).

The retired procedural opening is not shipped; the teammate still owns intro/outro clips. Building entrances are scenic landmarks, not newly playable interiors. Existing terrain/character licenses remain in force. User testing and target-laptop performance measurement are still required.
