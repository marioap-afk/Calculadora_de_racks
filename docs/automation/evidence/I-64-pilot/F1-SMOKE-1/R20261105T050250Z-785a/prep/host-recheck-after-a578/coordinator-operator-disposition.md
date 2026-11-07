# I-64 — COORDINATOR OPERATOR DISPOSITION (transcripción literal)

Recibida el 2026-11-05 (~15:45Z) por el chat de la sesión responsable, tras el paquete `return-package.md` (sección 6: el Owner no
puede operar).

```text
I-64 — COORDINATOR OPERATOR DISPOSITION
The Owner cannot personally operate AutoCAD for the next Smoke-1 occurrence.
Do NOT record:
"Confirmo que opero yo"
That would be factually incorrect.
Disposition:
OPERATOR_EXCEPTION = AUTHORIZED
Scope:
ONE next Smoke-1 occurrence only
Authorized AutoCAD operator:
implementation session
Owner:
INFORMED / occurrence authorized
This is an explicit pre-run exception, not an unplanned role deviation.
T3D §1 remains unchanged historically.
The occurrence remains eligible for Smoke-1 PASS only if all host-isolation
conditions below are satisfied.
Before BASELINE:

* acad.exe count = 0;
* RAM Elements count = 0.

After the implementation session opens AutoCAD:

* acad.exe count = exactly 1;
* record PID;
* record executable path;
* record acad.exe SHA-256;
* record process start time.

That PID becomes:
AUTHORIZED_AUTOCAD_PID
All process/module evidence must bind to that PID.
After NETLOAD, prove on AUTHORIZED_AUTOCAD_PID:

* exactly one RackCad.Plugin.dll is loaded;
* exact Candidate build path;
* exact authorized Candidate DLL SHA-256.

At every critical checkpoint:
if acad.exe count != 1
OR AUTHORIZED_AUTOCAD_PID disappears
OR another acad.exe PID appears
=> STOP immediately.
Before POST:

* close AutoCAD normally;
* acad.exe count must return to 0.

Because the implementation session is the authorized operator:

* record every Smoke step and result contemporaneously;
* do not batch undocumented actions;
* preserve screenshots/captures required by T3D;
* do not perform extra exploratory AutoCAD actions outside the accepted
procedure unless the procedure explicitly permits them;
* any necessary deviation returns to the Coordinator before continuing.

The final operator record must state explicitly:
"AutoCAD operated by implementation session under Coordinator
OPERATOR_EXCEPTION for this occurrence."
Do not label it as Owner-operated.
The newly delivered I-52 recheck is accepted.
The correction concerning NETLOAD-history attribution is also accepted:
the cited last-write existed in the pre-BASELINE host capture and therefore
must NOT be attributed to a578.
No additional I-52 response is required for that correction.
Update the new-run package with this operator disposition.
Then return at:
COORDINATOR_SMOKE1_RETRY_AUTHORIZATION_REQUIRED
only if any other authorization item remains unresolved.
If Candidate, DLL, fresh host recheck, RunId, process guard and all other
previous requirements are already complete, this operator exception removes
the remaining operator blocker.
Do not merge/rebase main.
Do not start F2.
```
