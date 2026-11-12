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

## Original audio validation snapshot

Unity 6000.6.2f1 `AmbrosJourney.Journey.Editor.SoundEffectsChecks.Run` passed: eight mono cues, duration and waveform limits, non-clipping headroom, smooth endpoints, four original music mappings, shared-clip ownership, click deduplication, paused-menu playback, and mute inheritance/unmute. The fixture also ran the complete certificate reward regression (pixel breakup, fast scroll drop, pickup, reload recovery, name/date and PDF).

That earlier .NET lesson/guardian pass had 22,927 assertions plus save/name/recovery checks. The current suite has 22,652 progression assertions after removing courage-only checks, plus legacy-save compatibility; see [current validation](../CurrentValidation.md). Audio cues and music are unchanged by the feedback update. Cue files are generated from deterministic synthesis and use no third-party audio samples.
