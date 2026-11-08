# I-64 — Revisión del Coordinator de la comprobación de host posterior a I-65 (literal)

Recibida el 2026-11-08 por el chat de la sesión responsable, que esperaba en COORDINATOR_POST_I65_HOST_SANITY_REVIEW_REQUIRED con
`disposition/return-package.md` (SHA-256 EB82113E…1610). La pegó el Owner.

```text
I-64 — COORDINATOR POST-I65 HOST SANITY REVIEW
RunId:
R20261108T065253Z-aa47
Disposition:
PASS
The post-I65 host sanity checkpoint is accepted.
Synchronized product:
3f5c7014351d1dc59de4eee86a7d1319fbeccc07
Documentary HEAD:
53683249c229329e3f576fc8f9b9919b1755c63f
Loaded RackCad.Plugin.dll SHA-256:
0C0F2FDE0987C489BABAF2ACB9A46936117D044EAF45C706A303E6B5B1C56C0B
Accepted.
Observed:

* one non-persistent Load Once;
* one RackCad.Plugin.dll in the authorized AutoCAD process;
* DLL loaded from the synchronized worktree build;
* exact SHA-256 MATCH.

AUTHORIZED_AUTOCAD_PID:
39252
Observed lifetime:
2026-11-08T07:06:56Z
through
2026-11-08T07:13:05Z
No evidence establishes that a second AutoCAD instance actually existed.
The watcher line:
"WATCH STOP: otra instancia 39252@07:06:56Z"
is classified:
FALSE_POSITIVE_PROCESS_TEARDOWN
Reason:

* the reported PID is the same authorized PID;
* the reported process start time is the same authorized process start time;
* no distinct PID was observed;
* the condition occurred while the authorized process was terminating;
* the reproduced short-lived-process experiment demonstrates that the
process can remain enumerable with PID/start-time identity while its
executable path has already become unavailable.

Therefore:
HOST_ISOLATION_VIOLATION = NO
Do not reclassify aa47 as STOP on account of this watcher line.
Preserve the line and this classification in evidence.
The watcher behavior is a tooling false positive.
It is not a RackCad product defect.
For future use, the helper should distinguish:
same PID + same start time + disappearing executable path during teardown
from:
a genuinely new acad.exe process.
Do not require a product correction in I-64 for this helper behavior.
A tooling improvement may be recorded separately.
PASS.
Observed:

* RACKPANEL is registered;
* command opens the Workspace;
* normal active session state is reached;
* rack selection produces the expected "Un rack" state and RackId;
* Esc returns selection state to "Ninguno";
* no unexpected RackLog ERROR attributable to coexistence with I-65.

This is sufficient for the narrow post-I65 coexistence check.
No C1/C2 Smoke repetition is required.
PASS for the approved non-destructive coexistence check.
Observed result:
"Unsupported. Solo se exporta un rack Cantilever; el elegido es de tipo selective."
Accepted properties:

* RACKRAMPAQUETE is registered;
* command is callable in the combined Plugin;
* Selective context reaches the documented Unsupported path;
* command returns before station/load/folder/package publication;
* no package directory was created;
* no RAM package was written;
* RAM Elements was not launched;
* DBMOD remained 0.

This does not constitute a new I-65 functional validation.
It proves command coexistence only, as intended.
Accepted:

* DBMOD = 0;
* QUIT completed without save prompts;
* drawing SHA-256 values remained unchanged;
* RackLog unchanged;
* no new ERROR;
* no transaction/persistence symptom attributable to the combined product.

PASS.
T3C / host diff:
CLEAN
The two changed AutoCAD state variables are accepted under the existing
allowed host-state rules.
HOST-EXTRAS:
Recent File List movement only.
Classification:
EXPECTED MRU / UI-STATE ACTIVITY
INFORMATIVE
NON-BLOCKING
AssemblyMap / load registration state:
UNCHANGED
No new RackCad registration or persistence was introduced.
Pre-run recheck:
SIN DERIVA
Post-run recheck:
2026-11-08T07:15:39Z
SIN DERIVA
I-52 performed no operation during the host window.
Accepted.
Its stated scope limitations remain exactly as recorded and do not weaken
the aa47 evidence because RackLog, host diff, HOST-EXTRAS and product-specific
state were independently collected by I-64.
The normal POST helper refused to capture because it interpreted the watcher
false-positive line as a STOP.
The session then used a POST variant.
Disposition:
TOOLING DEVIATION = ACCEPTED
The resulting POST is accepted provided the recorded variant differs from the
approved POST procedure only in bypassing the false-positive watcher veto and
does not:

* change capture scope;
* omit required host/profile fields;
* rewrite captured values;
* relax T3C comparison semantics;
* modify host state.

Based on the submitted package, no such semantic change is reported.
Therefore the POST and resulting CLEAN DIFF remain valid evidence.
Preserve:

* the original helper refusal;
* the exact variant used;
* the one recorded code difference;
* this Coordinator acceptance.

Do not rewrite history as though the normal helper had succeeded.
P-01 remains:
CLEARED
Accepted snapshot:
SNAPSHOT D
config.toml SHA-256:
377D0C3C2DA32B0F763501628CF4A4F054FF33C64C40C2D50399801539EAD1E8
Ordered inventory:
104 names
No model invocation occurred during aa47.
Future model invocation remains conditioned on config.toml matching an
accepted governed snapshot at invocation time.
The two conditions from the integration review are now satisfied:

1. P-01 CLEARED
2. POST-I65 HOST SANITY CHECK = PASS

Therefore:
POST-I65 INTEGRATION GATE = CLOSED
The synchronized I-64 + I-65 state is accepted for continuation.
Disposition:
F2 = MAY START
This does NOT automatically authorize arbitrary implementation work.
The session must resume under the existing I-64 workflow / gate rules for F2.
Before any model invocation required by F2:

* verify config.toml still matches an accepted snapshot;
* respect existing protocol budgets and ownership rules.

No additional Smoke-1 or post-I65 AutoCAD check is required unless a future
product change specifically invalidates this evidence.
Custody of aa47 is AUTHORIZED.
Create a documentation/evidence-only custody commit containing:

* authorization;
* exact DLL identity;
* PID/process evidence;
* watcher false-positive evidence;
* reproduced teardown evidence;
* RACKPANEL observation;
* RACKRAMPAQUETE Unsupported observation;
* DBMOD/drawing hashes;
* RackLog result;
* PRE/POST host captures;
* POST variant and its exact deviation record;
* T3C CLEAN result;
* HOST-EXTRAS classification;
* I-52 pre/post rechecks;
* this Coordinator review.

Do not modify product code as part of custody.
Run exact-SHA CI after custody.
F1 = PASS
Smoke-1 = PASS
I-65 synchronization = PASS
Post-I65 host sanity = PASS
P-01 = CLEARED
Integration gate = CLOSED
Next:
custody aa47
→ CI
→ resume I-64 at F2 under the existing workflow
```
