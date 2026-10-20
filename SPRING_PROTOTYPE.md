# Spring playable prototype

Open the inner `capstone` folder in Unity 6000.6.2f1. Open `Assets/Scenes/Spring.unity` and press Play.

## Controls

- WASD or arrow keys: walk
- E: talk to the Ancient Mage or continue dialogue
- 1 / 2 or the onscreen buttons: answer the first AI question
- Escape: close dialogue
- R: return Ambros to the starting position

The starting clearing contains a grass/path tilemap, cherry trees, flower patches, rocks, a pond, collision boundaries, animated Ambros and Ancient Mage sprites, a following camera, and a short tutorial with a retryable question. Movement pauses while dialogue is open. The lesson completion flag lasts for the current play session.

## Editing

All new scripts and extracted art are under `Assets/SpringPrototype`. Edit the scene normally to adjust layout. Player and Mage prefabs are in `Assets/SpringPrototype/Prefabs`. The scene is generated on the first project open if it does not exist. `Capstone > Build Spring Prototype` rebuilds the prototype scene; save any desired scene edits first because rebuilding replaces that scene's layout.

The environment uses the team's existing Mana Seed Spring sample sheet. Ambros and the Mage use the team's imported character pack. Original source art stays in its existing location. The source art's licenses still apply to all extracted images. The character pack has side-facing animations, so this first top-down prototype uses the same walking animation for vertical movement.

## Next development milestone

Expand the Spring lesson with the misinformation bats. Award progress after the challenge, then unlock the route to Summer. Later work includes the other seasons, saving/loading, inventory, party members, audio, and the final safety quiz.

## Validation

Play-tested in Unity 6000.6.2f1: walking and camera follow, Mage collision, dialogue opening and progression, movement paused during dialogue, wrong-answer retry, correct-answer completion, western boundary and camera clamp, pond collision, and return-to-start. Verified scene/prefab asset references and checked the Editor log for C# compilation and runtime exceptions. Lesson completion is session-only.
