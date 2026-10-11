# I-64 — Revisión del Coordinator de F1-T3C-PROFILE-DOC

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-11, tras el commit 9af9ac75
(CI 38158957028 4/4) y la parada en COORDINATOR_T3C_REVIEW_REQUIRED.

```text
I-64 — COORDINATOR T3C REVIEW
TARGET SHA:
9af9ac75c0103270116a21c0bf504b2386770446
origin/main:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
Coordinator disposition:
PASS
Record:
F1-T3C-PROFILE-DOC =
COMPLETE
Profile-diff procedure =
APPROVED FOR FUTURE SMOKE-1 USE
No AutoCAD execution occurred.
No profile / registry / TRUSTEDPATHS / SECURELOAD / ACL mutation occurred.
A written Smoke-1 operator procedure IS required before F1 PASS.
Create a separate documentary task:
F1-T3D-SMOKE-PROCEDURE
It is:
session-owned
documentary
no product Worker
no AutoCAD execution
no host mutation
It must translate the frozen S1-01..S1-12 sequence into an exact operator
procedure.
The procedure must exist before F1 gate close.
Smoke-1 itself is executed only AFTER F1 PASS.
ACCEPTED.
Smoke-1 runtime evidence uses a transient run directory such as:
artifacts/orchestration/I-64/F1-SMOKE-1/<RunId>/
Final custody follows the established T3A/T3B pattern under:
docs/automation/evidence/I-64-pilot/F1-SMOKE-1/
Custody must include:

* manifest with original SHA-256;
* BASELINE canonical capture;
* POST canonical capture;
* profile diff;
* console/operator records;
* I-52 RELEASE and acknowledgement;
* Owner/Coordinator dispositions;
* redactions.

The clear local companion files are NEVER versioned.
Only their SHA-256 is recorded.
KEEP CONSERVATIVE REPORTING.
Existing RackCad.UI.* key present in BASELINE:
placement/value changes may be ALLOWED under the existing rule.
New RackCad.UI.* key appearing only in POST:
REPORT
not automatically ALLOWED.
For the Workspace Palette:
if I-52 acknowledgement explicitly records that its state key may appear:
REPORT + PaletteKeyAcceptance=ACCEPTED
=> REPORTED_ACCEPTED
Otherwise:
REPORT_REQUIRED_STOP
Do not silently normalize new persistence as expected behavior.
A one-time non-persistent DLL load MAY be used only if explicitly authorized
by the applicable I-52 host RELEASE / CAD-manager acknowledgement.
Required characteristics:

* exact Candidate DLL;
* no permanent TRUSTEDPATHS addition;
* no SECURELOAD change;
* no new profile;
* no registry autoload;
* no ApplicationPlugins bundle/autoload;
* no persistent security change.

BASELINE/POST must prove that.
Forbidden:

* "trust this location permanently";
* add candidate folder to TRUSTEDPATHS;
* change/disable SECURELOAD;
* create another profile;
* register RackCad persistently to bypass the prompt.

If the load cannot proceed without one of those changes:
STOP HOST-TO-CONFIRM / STOP-06.
Return to I-52 / CAD manager.
Keep rule precedence:
INVALID
→ FORBIDDEN
→ REPORT
→ ALLOWED
R-A-03 is a residual classification only.
It does NOT grant general authority to ignore arbitrary future profile
changes.
Any sensitive/specific rule wins first.
In the next documentary commit:
record T3C review = PASS.
Advance:
last_evidence_commit =
9af9ac75c0103270116a21c0bf504b2386770446
No product changes.
Prepare the exact operator procedure for Smoke-1.
Basis:

* Freeze V5 S1-01..S1-12;
* T3C profile-diff procedure;
* current verified F1 product;
* I-52 host rules;
* Owner Validation boundaries.

Must specify:

* prerequisites;
* Candidate identity;
* exact DLL SHA;
* CI requirement;
* I-52 RELEASE requirement;
* BASELINE capture;
* AutoCAD startup/load path;
* RACKPANEL actions;
* multi-document actions;
* focus checks;
* classic-command coexistence;
* hide/show;
* active-document switching;
* failure/containment checks;
* profile POST capture;
* DIFF;
* disposition;
* evidence/custody;
* rollback/STOP behavior.

Do not execute any step.
Prepare F1 gate-close package.
It must map every frozen F1 closing invariant to exact evidence from:
T1
T2
T3A
T3B
T3C
T3D
Include exact product SHAs, CI runs and Coordinator close reviews.
Do not declare F1 PASS yourself.
Stop at:
COORDINATOR_F1_GATE_CLOSE_REQUIRED
Do not:

* run AutoCAD;
* execute Smoke-1;
* request Owner Validation yet;
* change profile/security;
* touch src/ or tests/;
* start F2;
* declare F1 PASS.

Proceed with T3D documentary preparation, then the F1 gate-close package.
```
