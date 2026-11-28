# Sound effects refresh

Original, short mono PCM cues replace the per-level synthesized pitch sweeps. Generate the source WAVs with `python3 tools/audio/generate_sound_effects.py`.

| Cue | Length | Use |
| --- | --- | --- |
| Click | 30 ms | Menu, settings, dialog, journal and final-activity controls; supported keyboard actions use the same cue. No hover sounds. |
| Correct | 75 ms | Light answer-confirmation tap |
| Incorrect | 95 ms | Low, muted retry tap |
| Complete | 200 ms | Restrained completion accent |
| Impact | 85 ms | Soft guardian impact |
| Pickup | 150 ms | Paper/collectible rustle; artifacts and the certificate scroll |
| Portal | 280 ms | Quiet air cue for travel between seasonal scenes |
| Dust | 550 ms | Dry crumble at the start of the Echo Keeper's disintegration |

`GameAudio` owns shared cue assets and a persistent pair of 2D effect voices. UI/pickup effects remain audible when the game pauses to show a menu or certificate. World effects respect pause. Duplicate click calls within a frame are suppressed; global effect cues have a 40 ms retrigger guard. M mutes registered music/effect sources and new scene sources inherit that state. Level cleanup no longer destroys shared imported clips.

The four seasonal MP3 assets and their music volume/loop/pitch settings are unchanged. Sound cues are preloaded/decompressed short PCM assets, avoiding the latency and unnecessary streaming overhead of using long-song import settings.

## Music and mute controls

Music On/Off appears in the pause journal and main-menu Settings, not on the gameplay HUD. `RegisterMusic` marks exploration, menu, battle and cinematic music sources so new tracks inherit the session preference. The setting resets for a new application session and is not written to disk. Sound effects remain audible with music off; M is the separate all-audio mute shortcut. Main-menu Spring music stops immediately after a valid New Journey action. Encounter music temporarily replaces the seasonal track and restores it when the conversation closes.

## Verification

`SoundEffectsChecks.Run` checks cue duration/waveform limits, shared ownership, clicks, pause and mute inheritance. `PauseJournalChecks.Run` checks the pause-only music action, toggle behavior and separation from effects. These procedures do not establish subjective audio balance; see [current results](../CurrentValidation.md) for executed checks. Cue files use deterministic synthesis without third-party sound samples; existing music credits remain with the project assets.
