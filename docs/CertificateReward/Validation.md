# Winter certificate reward

After passing the final Echo Keeper challenge (7/8), the completion date is recorded once. Continuing past the final feedback animates the Keeper crumbling into sprite-colored dust, with temporary camera framing. A small bobbing certificate appears where it stood. Approach and press E to collect it and open the approved four-season certificate, populated with the saved player name and original completion date. The same layout is used for the PDF download.

`certificatePending` is an optional version-1 save field: older completed saves keep access to their certificate. New victories retain the uncollected item across reloads; claimed items do not respawn. Long names shrink to fit, and certificate text treats the saved name as plain text. The existing quiz content and pass threshold are unchanged.

## Validation

- Unity 6000.6.2f1: `AmbrosJourney.Journey.Editor.CertificateRewardChecks.Run` passed. The fixture uses an isolated test save under `Logs/CertificateChecks`.
- The Play-mode fixture confirms advancing frames, boss alpha/dust animation, item appearance, restored movement, out-of-range rejection, E-key pickup, modal display, duplicate rejection, reload recovery before and after collection, preserved completion date, accented/long name fit, and a generated PDF.
- The saved date is deliberately changed to a historical test date to confirm that reopening/reloading never replaces it with today's date.
- Screenshots and the exported PDF were visually inspected. The PDF keeps the certificate's 10:7 proportions.
- .NET lesson/guardian regression suite passed 22,927 assertions, plus save/name/position/recovery checks including certificate state and date.
- Unity asset integrity and whitespace checks passed.
- The fixture excludes the confirmed Unity Editor SearchDatabase startup exception from gameplay error collection; it does not ignore gameplay exceptions.

Run the Unity fixture in batch mode without `-quit` (the fixture exits after its final check). The extra Game View size is used only for deterministic 1280x720 screenshots.
