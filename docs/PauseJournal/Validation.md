# Pause journal

The pause screen uses the shared FieldCard parchment and Determination font, with a two-page layout, named notes and chapter tabs. JourneyPauseJournal owns presentation; JourneySession supplies the actions and their eligibility.

Esc/Pause freezes time, input and ordinary audio. Actions are Resume, confirmed End run and return to menu, Artifacts, Music On/Off, unlocked chapters and Certificate when earned. Locked chapters are decorative. Guidance is an always-on status label. There is no save/Continue, reset shortcut or persistent gameplay music button.

Music toggles the application-session preference while leaving the game paused and sound effects separate. New registered music sources inherit it. Artifact Return restores the pause journal; Resume returns gameplay with only Pause and Artifacts on the HUD. Ending a run requires confirmation; cancellation preserves all in-memory data. Chapter revisits restore this run's location and progress.

PauseJournalChecks.Run checks action wiring, music/effects separation, no HUD music action, freeze/resume, artifacts, end-run cancellation, chapter travel, retained position, certificate access, fresh menu, text overflow and viewport bounds. Run it in an isolated project. Current executed results and platform limitations are in [CurrentValidation](../CurrentValidation.md); generated captures live under ignored Logs/PauseJournalChecks.
