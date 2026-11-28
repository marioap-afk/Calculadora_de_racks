# I-64 — Autorización de Smoke-1 completa, RunId R20261107T171052Z-6862 (literal)

Recibida el 2026-11-07 por el chat de la sesión responsable, que esperaba en COORDINATOR_SMOKE1_RETRY_AUTHORIZATION_REQUIRED. El paquete era
`F1-SMOKE-1-RETRY-PREP/post-785a/return-package.md` (SHA-256 6F5CD621…D299). La autorización asigna el RunId que la sesión propuso a las 17:10:52Z.

```text
I-64 — COORDINATOR FULL SMOKE-1 AUTHORIZATION
RunId:
R20261107T171052Z-6862
Disposition:
AUTHORIZED — FULL NEW SMOKE
Candidate:
cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6
Exact RackCad.Plugin.dll SHA-256:
3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0
Current branch HEAD:
1917bc19e11e0fa09f4484956794825ba960098a
HEAD is documentary custody only and is NOT the Candidate.
Reuse of Candidate cc7503e3 without recompilation is AUTHORIZED.
Fresh host state is ACCEPTED:
acad.exe = 0
RAM Elements = 0
Session recheck = no material drift.
I-52 recheck = SIN DERIVA.
The I-52 reply read from its session transcript is acceptable for this
pre-run package because:

* the literal reply was preserved;
* its provenance is recorded;
* no contradictory direct-channel reply exists;
* I-52 states it will not touch the host during the occurrence.

I-52 ACK/RELEASE is not required.
HOST_RELEASE = GRANTED FOR I-64 / 6862.
The Recent File List movement of Drawing D remains:
EXPECTED MRU / UI-STATE REORDERING
NON-BLOCKING.
The existing TEMP .sv$ files are pre-run host artifacts and are not in
themselves blocking. Do not use, modify or treat them as product evidence.
ONE non-persistent NETLOAD is AUTHORIZED.
Use:
Load Once
Authorized DLL only:
3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0
Do not rebuild or recompile.
After NETLOAD, inspect the AUTHORIZED_AUTOCAD_PID and require:

* exactly one RackCad.Plugin.dll;
* exact Candidate build path;
* exact authorized DLL SHA-256.

Before BASELINE:
acad.exe count = 0
RAM Elements count = 0
After AutoCAD opens:
acad.exe count = exactly 1
Record:
AUTHORIZED_AUTOCAD_PID
acad.exe path
acad.exe SHA-256
process start UTC
At every critical checkpoint:

* acad.exe count = 1;
* sole PID = AUTHORIZED_AUTOCAD_PID.

Any second acad.exe or ambiguous PID:
STOP.
After AutoCAD closes:
acad.exe count = 0
before completing POST evidence.
Stage A — before SM-00a:
Verify:

* AutoCAD monitor assignment;
* Claude/chat window is on another monitor;
* intended capture regions;
* screenshot mechanism;
* obstruction-detection mechanism.

A deliberate synthetic obstruction test is allowed.
Stage B — SM-03:
Once the actual RackCad palette exists, verify the real capture target:

* complete palette title/header visible;
* status text visible;
* read/write controls visible;
* lower content visible;
* relevant command line visible where required;
* no unrelated/private window overlaps the evidence region.

If Stage B fails:
PAUSE.
Window arrangement may be corrected without altering drawing state.
Do not proceed until a clean real-palette capture is demonstrated.
For every critical screenshot in this run:
open/review it immediately after capture.
Do not defer visual verification until custody.
Primary AutoCAD operator:
implementation session
This is an explicit Coordinator exception for 6862.
The implementation session may also execute:

* preflight;
* BASELINE;
* HOST-EXTRAS;
* process/module inspection;
* POST;
* T3C comparison;
* evidence capture.

Exception inside that exception:
SM-29 middle-button drag ONLY:
Owner performs the physical middle-button drag.
Record:
OWNER_MANUAL_GESTURE_EXCEPTION — SM-29 only
The implementation session records VIEWCTR before and after.
The Owner must perform only the requested drag and no additional AutoCAD
action.
The implementation session resumes operation afterward.
S1-02 must be actually observed in this occurrence.
For the middle-button drag:

1. record VIEWCTR before;
2. ask Owner to perform the middle-button pan;
3. record VIEWCTR after;
4. confirm the view center changed;
5. capture the visual result as required by the procedure.

Do not infer PASS merely because other navigation actions pass.
The prohibition on Ctrl+V applies to entering AutoCAD command-line text.
It does NOT remove the T3D SM-14c test of Ctrl+V while focus is inside the
Workspace field/control.
SM-14c remains mandatory.
Before sending Ctrl+V:

* visibly establish focus inside the palette/control;
* record the focused state.

Send Ctrl+V once.
Expected:
the action remains inside the palette and does not reach AutoCAD.
If AutoCAD starts PASTECLIP, the command line reacts, DBMOD changes, or
focus attribution is ambiguous:
STOP immediately.
Do not attempt a recovery continuation.
For normal command-line input elsewhere in the run:
type text directly;
do not use Ctrl+V.
Additional read-only DBMOD checks are AUTHORIZED.
They are diagnostic guard points and do not change T3D semantics.
At minimum record DBMOD:

* after Drawing A is opened;
* before SM-14a;
* after SM-14e;
* immediately before SM-14h;
* immediately after SM-14h;
* before SM-18;
* after SM-18.

Preserve the distinction between:
view-state bits
and:
drawing-modified bit 1.
If bit 1 becomes set before the authoritative SM-18 assertion for any
unrelated operator/tooling reason:
STOP.
Do not continue another compromised no-write proof.
Do not repair, undo or reset DBMOD merely to make the test pass.
This is a FULL Smoke-1.
No prior occurrence exempts any mandatory step.
Pay particular attention to:
SM-03:
clean authoritative C1 capture.
SM-13:
command-active presentation and no drain/read during LINE.
SM-17:
clean authoritative dock-right / float / hide-show evidence.
SM-18:
clean authoritative drawing-write assertion on A.
SM-26:
clean authoritative "Sin documento" capture.
SM-27b:
clean authoritative Start-state capture with no stale session.
SM-29:
actual Owner middle-button drag observed.
SM-34:
clean authoritative RACKEDITAR command-active capture.
SM-35:
clean B write assertion.
SM-39:
no unexpected RackLog error, no DocumentActivated NullReferenceException,
no transaction-leak diagnostic.
SM-40 / SM-41:
normal close and byte-identical drawing files.
Use:
new BASELINE
new POST
new HOST-EXTRAS BEFORE/AFTER
new operator record
SECURELOAD:
fresh read-only Owner/CAD-manager value immediately before BASELINE;
T3C BASELINE must independently read the same value.
Mismatch:
STOP before AutoCAD.
After the occurrence:

* close AutoCAD normally;
* require acad.exe = 0;
* POST;
* T3C DIFF;
* HOST-EXTRAS;
* fresh I-52 read-only recheck if reachable, or preserve its returned
transcript response with provenance if the same channel condition recurs.

At the end present the completed operator record to the Owner.
Owner signature is required.
It confirms the record, not PASS.
The implementation session must not self-declare Smoke-1 PASS.
Return at:
COORDINATOR_SMOKE1_REVIEW_REQUIRED
with the complete run evidence and proposed disposition.
Do NOT merge/rebase main.
Do NOT start F2.
Do NOT change product.
```
