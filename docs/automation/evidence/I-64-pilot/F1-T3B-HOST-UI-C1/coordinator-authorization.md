# I-64 — Disposición del STOP de Smoke-1 y autorización de F1-T3B-HOST-UI-C1 (literal)

Recibida por el chat de la sesión responsable el 2026-10-13, tras la custodia de Smoke-1 (`30d600ad`, CI 38333812962 push 4/4) y la
parada en COORDINATOR_SMOKE1_REVIEW_REQUIRED.

```text
I-64 — Smoke-1 STOP disposition / F1-T3B-HOST-UI-C1 authorized
Smoke occurrence:
`R20261013T011654Z-184b`
Disposition:
STOP CONFIRMED
Cause:
On SM-03, the Workspace `PaletteSet` became unusable/off-screen before the Owner could observe the panel. The Owner correctly refused to deviate from T3D.
This is a product finding against D-01 / initial Workspace usability.
Do not repeat Smoke-1 on Candidate `4d79e910`.
Authorize one narrow product-correction task:
`F1-T3B-HOST-UI-C1`
Scope:

* inspect the public AutoCAD 2025 `PaletteSet` API actually available;
* add deterministic first-creation geometry/docking so the Workspace is visibly usable on first `RACKPANEL`;
* apply that only on initial palette creation, not every `Show()`;
* preserve user movement/docking during the current process;
* preserve no-GUID/no-persistence design;
* no registry/profile/settings/security writes;
* no changes to bridge/session/DWG behavior.

Preferred files:

* `src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs`
* narrowly related T3B Workspace tests/guards only.

Do not broaden scope without Coordinator disposition.
Required RED
Add tests/guards proving at minimum:

* initial placement/docking is explicitly configured;
* repeated `Show()` does not reapply/reset geometry;
* no persistence/GUID/autoload/profile mechanism is introduced;
* existing PaletteSet style, docking capability and `KeepFocus` remain intact.

Verification
After GREEN:

* relevant Workspace/UI focused suite PASS;
* Core Full PASS;
* UI Full PASS;
* Debug UI build PASS;
* Debug Plugin build PASS except already-known warnings;
* exact-SHA CI 4/4.

Historical Smoke-1 STOP remains unchanged.
Do not modify its evidence or relabel SM-03.
Candidate
The correction necessarily creates a new Candidate SHA and new DLL SHA-256.
Candidate `4d79e910` becomes:
`HISTORICAL_SMOKE1_STOP_CANDIDATE`
It must not be reused.
After correction
Prepare a delta close package for `F1-T3B-HOST-UI-C1` showing:

* exact RED SHA;
* exact GREEN/product SHA;
* files changed;
* tests;
* CI;
* confirmation no persistence/security behavior was introduced;
* comparison against the original D-01 requirements;
* preserved Smoke-1 STOP evidence.

Stop at:
`COORDINATOR_T3B_C1_CLOSE_REQUIRED`
Do not run AutoCAD.
Do not request another Smoke-1 occurrence yet.
Do not start F2.
```
