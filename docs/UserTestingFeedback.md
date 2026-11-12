# User-testing feedback: November 12, 2026

## General game changes

1. Courage and hearts are removed from gameplay, question feedback, trackers and progress data. Wrong answers explain the issue and allow retries without a penalty meter. Existing saves are compatible.
2. The HUD counter reads **0/12 Artifacts**, using the actual twelve-item collection. Its full-count form fits at the original point size.
3. Correct responses explicitly say **Correct!** before the explanation across all chapters and final activity types.
4. Quiz scores use the full question count throughout an attempt: five for seasonal guardians, eight for the finale, and the actual fixed count for each lesson. An incorrect answer changes no denominator.
5. Continuous Winter ridge barriers prevent walking around or behind the cave artwork. The intended doorway stays open and enters automatically after the Warden is passed. Older saves behind the ridge recover at the approach.
6. Little Mage still accompanies the party in the cavern, but has no dialogue interaction or [E] caption there. His outdoor dialogue stays available outside.
7. The certificate's Return action and Escape lead to the main menu, retaining the completed journey and certificate.
8. Final Activity 5 says: “Click each answer box repeatedly to see all options. Fill every row, then check.”

## Earlier changes in this same feedback batch

Winter's arrival Mage is clear of its portal; the Warden stands beside the visible cave opening; Little Mage waits beside the cave after the Warden and explains it separately from the walk-in transition. The main menu's empty-name field pulses gold until a name is entered. Interactive objects and helpers use overhead [E] captions; the duplicate bottom interaction card is hidden. Nearby captions avoid overlap, and the Garden Warrior's finished dialogue stops advertising a repeat interaction.

[Current validation](CurrentValidation.md) records the combined checks. The authored question content, four-season order, guardian passing scores, existing artwork and player font remain in place.

## Winter compass notice

The first heavy-snow compass outage in Explorer mode opens an icy popup: “Uh oh! Your compass has frozen!” It directs the player toward the next unfinished landmark using lanterns and trail signs. Its pixel icicles are native uGUI geometry. The message appears once per saved journey, waits for conversations to finish, and supports Continue/E/Enter or Escape. The existing temporary snowfall behavior, Assisted setting and functioning cave compass are retained.

## Documentation handoff

The root/chapter guides, design, journey/save/certificate handoff, guidance/world notes, video brief and testing instructions describe the current implementation. Earlier validation counts and original art prompts are preserved as dated evidence/provenance. The current certificate pickup opens the certificate directly; the retained outro helper needs an explicit teammate integration. No new video, font, package or generated-art asset is introduced by this feedback work.
