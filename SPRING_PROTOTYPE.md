# Spring Valley: implementation and review

Spring is the introductory chapter of the connected four-season game. The Ancient Mage teaches how AI uses examples, why humanlike language is not human understanding, and how to check claims and protect privacy. He was another child trapped in the game and has grown old there.

## Start and progression

Use the main menu through `Assets/Opening/Intro.unity`, **Capstone > Play From Opening**, or **Play Spring Only** for a direct chapter preview. New Journey starts Spring immediately until the teammate supplies the optional intro.

1. Meet the Mage beside his cottage for the three-page introduction.
2. Read the field notes at Blossom Grove, Stillwater Lake and Whispering Ruins.
3. Apply each note to two true/false claims from its Myth Bat. Six claims are active; the source retains three unused alternatives.
4. Use J to review notes. Mistakes explain the answer and allow retries; depleted courage replenishes without erasing lessons.
5. Pass the **Blossom Guardian** with at least **4/5 (80%)**. This blossom-covered moss/woodland tortoise has separate idle, windup and impact poses. It replaces the old Thornhide creature visually and in displayed names.
6. Earn Apprentice rank, recruit the Ancient Mage, close the gate conversation and walk through the unlocked Summer portal.

The 64 x 64-unit valley includes the Mage's cottage, winding woodland trails, a lake and dock, three landmark lessons and a separate southern guardian clearing. The lakeside note is at (7, 1), on the approach to its bat. Three optional artifacts support later collection completion.

## Shared systems

Spring uses the same Canvas/TMP parchment UI and Determination font as later chapters. The gold compass points to the next note, bat or guardian, with the existing lake detour. JourneySession adds pause, exact-location saves, artifacts, chapter revisits and certificate access. Progress is persistent, not session-only.

`SpringGuide` owns lesson/guardian rules; `SeasonBossVisual` supplies the new guardian's runtime artwork and one-shot attack. Existing bat art and licensed source-pack files remain. The guardian's previous texture assignment is retained as a source reference; active artwork comes from `Resources/Journey/Bosses/SpringBlossomAtlas`.

## Tools and limits

**Validate Spring Progression** covers prerequisites, six claims, wrong-answer retries, a 3/5 guardian failure and a 4/5 success. The connected integration suite additionally checks real portals, menus, saves, text fitting and the new boss poses.

World rebuilding commands replace generated scene content. Coordinate scene ownership before using Upgrade Spring World or the cottage/trail polish commands. Do not use a Spring rebuild to generate another season. The retired generated opening and `AnimatedOpening.cs` are no longer shipped; old demo footage is historical, not a current visual reference.

Spring's approximate five-minute first-time pacing is a target awaiting audience testing. Movement retains side-facing character frames. The optional teammate intro/outro remains separate from gameplay.

See [current design](docs/GameDesign.md), [questions](docs/QuestionRefresh.md), [boss artwork](docs/Bosses.md) and [validation](docs/CurrentValidation.md).
