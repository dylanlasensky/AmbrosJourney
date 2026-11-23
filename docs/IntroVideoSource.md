# Intro video source correction — 22 November 2026

The selected intro is the teammate's **40-second, 1920×1080, 24 FPS** movie added in PR18, commit `e22ff228eb4310cd0af83e941e1e0b9adb5d91f8` by `kayliepast`. That commit documented the original filename as `Ambros-Journey-Intro-40s.mp4`. Its unchanged source contains one audio track and is **29,330,381 bytes**.

Its frames show the study-assistant exchange, Ambros reacting at his laptop, and his arrival beside the Ancient Mage. Source frames at 1, 8, 18, 28 and 36 seconds were inspected during the audit. The original audio track was preserved, not edited or re-recorded.

PR19 commit `deda414f4ed6d67634bd4f17cfcd37fa62fe2022` replaced it with `Assets/Opening/Opening.mp4` from October commit `62196e5`. The resulting 34-second, 1280×720 file was the earlier captioned still-image sequence. Its SHA-256 exactly matched that October asset. This explains why a successful movie-copy check still packaged the wrong intro: it compared the build against an already incorrect source path.

## Selected assets

| Asset | Provenance | SHA-256 |
| --- | --- | --- |
| `capstone/Assets/Journey/Resources/Journey/Intro.mp4` | PR18 teammate intro, restored byte-for-byte | `de4dba62d3607650b8157d0d7f29b027ac2574717bd896d6cd14f691ef4e14cf` |
| `capstone/Assets/Journey/Resources/Journey/Outro.mp4` | Existing PR18 teammate outro, unchanged | `e2a89f9ea854fc0293d8acc25e4b9a8961dcf5271c38d386410bd436c7494a4f` |

The intro's existing `.meta` GUID is retained. `Intro.unity` has no overriding serialized clip, so native playback resolves `Resources.Load<VideoClip>("Journey/Intro")`. Web playback uses `StreamingAssets/JourneyVideos/Intro.mp4`, copied from the same source by `JourneyWebVideoBuild`. No playback logic or authored content needs to change to select the intended movie. The opening setup utility now points at that same source path.

`tests/check_assets.py` checks the chosen source identities as well as normal Unity asset integrity. `JourneyWebBuild` separately checks that exported movies match those sources. A deliberate future replacement must update the provenance and expected identity together. Earlier fullscreen/browser results used the older intro. The restored movie was subsequently checked in the exported game, including fullscreen and keyboard handoff; see [current validation](PresentationClarityValidation.md).
