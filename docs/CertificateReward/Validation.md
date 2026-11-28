# Certificate reward flow

Passing the eight final Echo Keeper activities with at least 7/8 dissolves the robot and drops the certificate scroll. The player approaches and presses E to open the personalized Determination-font certificate.

Download PDF keeps the certificate view open. Native export saves the rendered PDF to Downloads (with the existing fallback if unavailable) and opens it in the default viewer; Web uses the browser download bridge. Name and completion date come from the current run.

Exit/Escape resumes the cave and assembles the home portal. Walking through it plays the supplied outro, then returns to the main menu. Opening/closing the certificate alone does not replay the film. From the main-menu viewer, Exit returns to that menu. The earned certificate remains available until a new run or reload; a downloaded PDF is the player's own file.

There is no cave Mage conversation, persistent game save or certificate-based qualification. The reward follows successful completion of authored learning activities.

Repeatable checks: `CertificateRewardChecks.Run`, `BossExitChecks.Run` and the connected integration suite in an isolated project. See [testing](../Testing.md) and [current results](../CurrentValidation.md) for what has actually been run on this revision. Earlier capture files are historical art evidence.
