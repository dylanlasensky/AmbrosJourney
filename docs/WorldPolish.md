# World and movement polish

## Current environment

Deterministic worn trails replace square road strips while preserving route/collision layouts. Their irregular pixel contours, quiet grain and sparse stones keep scenery readable. Winter's cave mouth sits inside a continuous snowy mountain ridge; snow-rooted fir/oak sprites and low-contrast snow tie it into the forest. The current tree family is `WinterNature`; earlier rejected variants are provenance, not active replacements.

Seasonal cottages share timber-and-stone construction. Authored landmarks have connected doorstep trails, clear approaches and grouped supplies. Extra generic cottages and props that crowded routes were removed. Fall retains its forge, archive and optional overlook. Water highlights, lantern flicker and blossom/leaf/snow effects remain active. [World cohesion](WorldCohesion.md) explains the object families.

## Movement and guidance

Players accelerate to walking speed and brake quickly. Dialogue stops movement immediately. Walk timing follows measured speed so pushing into a wall does not imply full walking. Rigidbody interpolation and shared damped cameras smooth normal movement; long teleports snap and reset damping.

Companions use a recorded safe route spine with varied gaps and small lateral offsets; obstacle checks narrow the formation. Teleports reset the trail without a false high-speed animation step. Explorer guidance gives broad bearings for distant Fall/Winter objectives and can be obscured by Winter snow; Assisted restores exact cached route guidance.

## Bosses and UI

Spring, Summer and Winter now have distinct animated seasonal guardians, and the final Echo Keeper is a larger ancient AI machine. Fall's autumn Sentinel remains the reference and is unchanged. New art is transparent, point-filtered and grounded per frame; attacks have separate one-shot windup/impact poses. Boss visual changes do not move interaction points or change score/gate rules. [Boss artwork](Bosses.md)

All live text uses Determination with the shared parchment layout and existing point sizes. New final activities stay inside the conversation frame. Cramped labels are shortened; font leading is normalized without shrinking glyphs.

## References and verification

Earlier art-direction notes cited the official Stardew Valley and Eastward galleries for coherent scenery groups, quiet ground and open walking space. No art from those games is imported. These are reference decisions from earlier work, not new research claims.

Current automated checks cover actual controller acceleration/braking, modal stops, interpolation, camera resets, real seasonal portals, region routes, new boss poses/strike hooks, mixed final activities, UI fitting, saves, collection and certificate export. Results and build scope are in [CurrentValidation](CurrentValidation.md). Controlled checks do not establish subjective movement feel, audience pacing or laptop thermal performance.
