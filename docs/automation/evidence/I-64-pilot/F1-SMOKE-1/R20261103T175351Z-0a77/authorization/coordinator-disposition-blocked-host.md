# I-64 — Disposición del Coordinator: BLOCKED_BEFORE_SMOKE1 por estado del host (literal)

Recibida el 2026-11-03 (~17:45Z) por el chat de la sesión responsable, en respuesta al informe de verificación de solo lectura del
mismo día (HEAD `09f2f1c3`, `origin/main` `a140eb3a` con I-65).

```text
I-64 — COORDINATOR DISPOSITION
Your read-only verification report is accepted with one correction.
Current disposition:
BLOCKED_BEFORE_SMOKE1
The blocker is HOST STATE only.
Candidate 09f2f1c3, C1, fixtures and historical Smoke-1 evidence remain valid.
Do NOT merge/rebase main into I-64 yet.
Do NOT start F2.
Do NOT launch AutoCAD.
Do NOT automatically accept the current:
AssemblyMap\RackCad.Plugin
entry as harmless initial state.
It points to the temporary I-65 bundle and T3C does not currently include
AssemblyMap in its BASELINE/POST comparison.
That creates a Candidate-integrity / assembly-resolution risk.
I-64 must not delete or repair the entry itself.
Escalate it to I-52 / I-65 host authority / CAD manager and obtain one of:
A. CLEANED
The owning host procedure removes the I-65 residue and a fresh read-only
recheck confirms it is gone.
Preferred.
or
B. EXPLICITLY ACCEPTED
The host authority explicitly classifies the exact AssemblyMap value as
expected persistent machine state and demonstrates that it cannot redirect
or contaminate the exact I-64 Candidate load.
If B is used, add explicit read-only AssemblyMap capture before and after
Smoke-1.
Do not infer acceptance.
The existing AppDataLow/CoreUser entries may be treated as historical
baseline only if I-52 confirms:

* they pre-exist the new occurrence;
* they are not changed by I-64;
* they do not participate in RackCad assembly loading.

Record them, but do not modify them.
After all I-65 host windows and any authorized recovery:
obtain fresh I-52 / host references for:

* acad.exe = 0;
* active profile;
* exact acad.exe SHA-256;
* TRUSTEDPATHS entry count and full chain SHA-256;
* current SECURELOAD rule/value;
* registry no-drift;
* ApplicationPlugins no-drift;
* ACL no-drift;
* AssemblyMap\RackCad.Plugin state;
* H10 held during I-64.

Only after this is clean/accepted may Smoke-1 preparation continue.
Close:
R20261019T180407Z-1be6
as:
NOT_STARTED / SUPERSEDED
because it has no BASELINE, POST or executed Smoke steps.
It does not count as a Smoke-1 occurrence.
Assign a fresh RunId only after the host recheck is accepted.
Your post-Smoke integration analysis is accepted.
After Smoke-1 closes:

* synchronize I-64 with current origin/main containing I-65;
* resolve the two command-census conflicts;
* expected combined command count = 39;
* revalidate solution/project changes;
* revalidate Plugin and bundle;
* revalidate Workspace/C1 guards;
* revalidate I-65 guards;
* run Core Full, UI Full, Debug builds and exact-SHA CI;
* establish a new Candidate if further host validation is required.

Do not perform this integration before the current Smoke-1 retry.
Return at:
COORDINATOR_SMOKE1_RETRY_AUTHORIZATION_REQUIRED
with:

* fresh I-52/host recheck;
* explicit AssemblyMap disposition;
* AppDataLow disposition;
* H10 hold confirmation;
* new RunId proposal;
* confirmation Candidate 09f2f1c3 remains unchanged;
* confirmation acad.exe = 0.

No AutoCAD until then.
```
