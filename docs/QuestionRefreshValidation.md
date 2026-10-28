> **Dated validation snapshot.** Counts, captures and limitations below describe the version tested at that time. For the current bosses, Determination UI, mixed finale, persistent saves and latest results, see [current validation](CurrentValidation.md) and [documentation index](README.md).

# Question and font refresh validation

Checked October 28, 2026 against the merged game at 206464b, using Unity 6000.6.2f1. Changes are on the local feature/question-refresh branch.

- Pure progression and save checks: **22,927 assertions passed**. Includes all 32 Summer, 243 Fall, 243 Winter boss answer combinations and all 256 pass/fail combinations for the eight final activities. Invalid and incomplete responses, duplicate selections, retries, courage recovery, retained crests and save recovery are covered.
- Connected Spring > Summer > Fall > Winter playthrough passed with the real interactions and portal transitions.
- Winter checks inspected **128 UI states**, including every new final activity, a failed 6/8 final attempt, a successful 7/8 retry and actual uGUI begin-drag/drag/drop/end-drag event handlers. Matching, multi-selection, arrangement and single-choice activities submit through the real UI.
- Render/text-overflow checks passed at **1280x720 and 1024x768**, including final activities, all four seasonal interfaces, menus, collection screens and the certificate. Existing point sizes and boxes were retained. Font leading was normalized without scaling glyphs; cramped labels were shortened.
- The complete macOS release build succeeded with **0 errors and 3 existing font/shader/platform warnings**. The baked Determination atlas and new activity scripts are included.
- Menu, exact outdoor/cave save restoration, pause, chapter revisits, all twelve artifact pickups, certificate PDF export, full reset and fresh Spring startup passed. Tests use their own isolated save path.
- The requested fictional Ambros privacy message is present exactly. Runtime copy has no em dashes. New final situations do not reuse earlier question prompts. Winter teaches the AI-agreement evidence lesson before the finale, independently of optional collectibles.

The tests ran in an isolated project copy so automated resets and capture/build operations did not change the player's saved journey. Images and runtime logs are under question-refresh/verification/Logs/SeasonChecks in the working workspace. A human playthrough remains useful for judging wording and challenge level.
