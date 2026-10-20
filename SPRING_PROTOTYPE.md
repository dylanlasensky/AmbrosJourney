# Intro and Spring implementation

Start in `Assets/Opening/Intro.unity`, followed automatically by `Assets/Scenes/Spring.unity`. The Capstone menu can play either scene.

## Implemented

- Original 34-second storybook opening MP4 with generated keyframes, gentle camera movement, captions and an original synthesized ambient cue. Ambros is at his laptop, gets pulled inside, and arrives in Spring. The opening has no player actions.
- Spring clearing with tilemaps, trees, rocks, water, boundaries, animated Ambros, camera follow and NPC interaction.
- Ancient Mage backstory and tutorial on AI basics, training examples and private data.
- Three true/false myth-bat missions with immediate feedback, heart damage for wrong answers and retries.
- Five-question Bat Guardian quiz. Four correct answers (80%) unlock the Summer gate, remove the guardian and add the Mage as a following companion. Failed attempts can be retried without repeating missions.
- Original mossy stone seasonal gate. Summer is unlocked as a progression flag; no Summer gameplay is included.

## Sprint Two alignment

The source is `SprintTwo (1).pdf`, pages 3–5, read October 20, 2026. Ambros is a middle schooler trapped through his computer. Spring contains the tutorial and first AI lesson, taught by the Ancient Mage (another child who grew old in the game). Misinformation bats use true/false challenges. Future seasons teach appropriate AI use, prompting and verification. Winter eventually drops an item to bring back to Spring for the return home. The planned final challenge focuses on AI safety. No live AI API is used.

The latest direction uses a game on the laptop for the opening, replacing the notebook's earlier Claude-question setup. A larger bat guardian and 80% seasonal pass threshold implement the requested final boss gate.

## Controls and editing

WASD / arrows move. E interacts or continues. 1 / 2 select an answer. Escape closes dialogue. R returns to spawn. No movement is accepted while dialogue is open. Progress is session-only.

Scripts and extracted art: `Assets/SpringPrototype`. Intro: `Assets/Opening`. Existing environment/character licenses still apply. The gate and cinematic artwork were generated for this project. Keyframes and the renderer are retained under `tools/opening-art` and `tools/render_opening.py`. Characters use side-facing art for all movement directions.

`Capstone > Set Up Opening and Spring Missions` wires scene references and build order. `Build Spring Prototype` replaces the original layout, so save desired scene edits before rebuilding. `Validate Spring Progression` tests the tutorial, bat retries, all missions, a failing boss grade and an 80% passing grade. `Record Spring Demo` sends ordinary keyboard inputs and records the actual Game view for a review video.

Summer, Fall, Winter, saving/loading, inventory, customization and the final certificate remain future work.
