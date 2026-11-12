# World and movement polish

## Current environment

Deterministic worn trails replace square road strips while preserving route/collision layouts. Their irregular pixel contours, quiet grain and sparse stones keep scenery readable. Winter's cave mouth sits inside a continuous snowy mountain ridge. The frontage now connects to the map boundaries with a rear barrier, leaving a three-unit walk-in doorway and no route behind the artwork. The Warden stands to the side, Little Mage briefs players outside, and old checkpoints in the newly blocked area recover at the approach; snow-rooted fir/oak sprites and low-contrast snow tie it into the forest. The current tree family is `WinterNature`; earlier rejected variants are provenance, not active replacements.

Seasonal cottages share timber-and-stone construction. Authored landmarks have connected doorstep trails, clear approaches and grouped supplies. Extra generic cottages and props that crowded routes were removed. Fall retains its forge, archive and optional overlook. Water highlights, lantern flicker and blossom/leaf/snow effects remain active. [World cohesion](WorldCohesion.md) explains the object families.

## Movement and guidance

Players accelerate to walking speed and brake quickly. Dialogue stops movement immediately. Walk timing follows measured speed so pushing into a wall does not imply full walking. Rigidbody interpolation and shared damped cameras smooth normal movement; long teleports snap and reset damping.

Companions use a recorded safe route spine with varied gaps and small lateral offsets; obstacle checks narrow the formation. Teleports reset the trail without a false high-speed animation step. Explorer guidance gives broad bearings for distant Fall/Winter objectives and can be obscured by Winter snow; Assisted restores exact cached route guidance. The first Explorer-mode whiteout opens an icicle-edged notice naming the next unfinished Winter landmark; it is saved once and waits for other conversations. The snowfall remains temporary and does not obscure the cave compass.

## Bosses and UI

Spring, Summer and Winter now have distinct animated seasonal guardians, and the final Echo Keeper is a larger ancient AI machine. Fall's autumn Sentinel remains the reference and is unchanged. New art is transparent, point-filtered and grounded per frame; attacks have separate one-shot windup/impact poses. The November 12 feedback pass moves only Winter's outdoor Warden interaction point beside the cave; passing scores and gate rules remain. [Boss artwork](Bosses.md)

All live text uses Determination with the shared parchment layout and existing point sizes. New final activities stay inside the conversation frame. Cramped labels are shortened; font leading is normalized without shrinking glyphs. One overhead [E] card identifies each interactive helper/lesson object, while artifacts retain [F]. Moving labels follow sprites and avoid overlap; the duplicate bottom interaction popup is removed. Correct responses say Correct!, quiz denominators stay fixed and courage/hearts are removed. The HUD names the twelve Artifacts; its counter is slightly wider to retain its text size.

## References and verification

Earlier art-direction notes cited the official Stardew Valley and Eastward galleries for coherent scenery groups, quiet ground and open walking space. No art from those games is imported. These are reference decisions from earlier work, not new research claims.

Current automated checks cover actual controller acceleration/braking, modal stops, interpolation, camera resets, real seasonal portals, region routes, new boss poses/strike hooks, mixed final activities, UI fitting, saves, collection and certificate export. Results and build scope are in [CurrentValidation](CurrentValidation.md). Controlled checks do not establish subjective movement feel, audience pacing or laptop thermal performance.
