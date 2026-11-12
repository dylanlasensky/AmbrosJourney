# Journal pause-menu presentation

The pause screen is presented as an open field journal, using the existing `Journey/FieldCard` parchment texture and Determination game font. It has a bound cover, stitched gutter, seasonal bookmark tabs, a named field-notes page, and a chapter-map page. The paused game remains visible behind a dimmer.

`JourneyPauseJournal` contains only presentation code. `JourneySession.ShowPause` retains the existing callbacks and eligibility rules for Resume, save/return, artifacts, guidance, reset confirmation, earned certificate access, and every unlocked chapter. Locked chapter entries are decorative text, not new buttons. The existing button factory retains click audio, keyboard navigation, and event wiring.

Unity 6000.6.2f1 `AmbrosJourney.Journey.Editor.PauseJournalChecks.Run` passed the journal workflow: pause freezes time/audio/input; Resume resumes; guidance is saved; artifact view and Return / Esc restore the pause journal; reset still requires confirmation and cancellation preserves progress; all four chapter actions load correctly; Winter restores its exact saved position; earned certificate access works; and save/return/Continue retains the profile and artifacts.

Layout checks cover active button hit areas, viewport bounds, text overflow, locked chapters, and a 32-character name. Captures are saved under `Logs/PauseJournalChecks`. The test uses an isolated save and the current Game View size. The current Escape/back handler and keyboard navigation are retained; certificate Return/Escape now opens the main menu, while artifact Return still restores the correct pause/play origin; automated keyboard-event injection in batch mode is not used. Its error collection excludes the known Unity Editor SearchDatabase startup exception, not gameplay exceptions.

The results above are the pause-journal snapshot. The November 12 feedback and current combined results are in [CurrentValidation](../CurrentValidation.md).

Revalidated on November 5, 2026 against main `30ebb2d` using Unity 6000.6.2f1. `PauseJournalChecks.Run` passed with the current artifact Return / Esc behavior. The repository pause-menu screenshot is the previously captured journal preview.
