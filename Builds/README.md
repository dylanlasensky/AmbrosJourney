# Playable build packages

The top-level Builds folder holds the local release exports. Source code remains in the inner capstone project. The Web archive exceeds GitHub's ordinary 100 MB file limit; generated ZIPs and unpacked players are excluded from ordinary Git history. Package identities and sizes are recorded in [manifest.json](manifest.json). Build upload status and verification are recorded in docs/CurrentValidation.md.

| Platform | Local file relative to repository root | How to use |
| --- | --- | --- |
| itch.io / browser | `Builds/Web/AmbrosJourney-Web.zip` | Upload the entire ZIP as an HTML game. index.html is at the root; preserve Build and StreamingAssets. |
| macOS | `Builds/Desktop/macOS/AmbrosJourney-macOS.zip` | Extract the complete .app bundle outside iCloud before opening; this export targets Apple silicon. |
| Windows 64-bit | `Builds/Desktop/Windows/AmbrosJourney-Windows.zip` | Extract the complete folder; launch AmbrosJourney.exe beside its Data folder and DLLs. |

Desktop builds are Unity release exports, not platform-signed installers. Windows execution must be checked on Windows. A successful Web build still requires hosted browser testing after upload. Original cinematic bytes and source asset licenses are preserved.

[Rebuild instructions](../docs/Testing.md) · [Actual validation and delivery status](../docs/CurrentValidation.md) · [Source repository](https://github.com/dylanlasensky/AmbrosJourney) · [itch page](https://kcarrubba.itch.io/ambros-journey)

These ZIPs are currently local downloads. The GitHub plugin used for this update can publish repository files but cannot attach large build archives to releases; no remote build download is implied. The Windows executable is also available locally at `Builds/Desktop/Windows/Player/AmbrosJourney.exe` with its dependencies.
