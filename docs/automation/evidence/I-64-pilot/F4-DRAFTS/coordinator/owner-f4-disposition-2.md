I-64 — F4 OWNER / COORDINATOR DISPOSITION
Current I-64 branch:
5406579660b8e59ddb593e33e9e840479b3ee00a
Current origin/main:
812baf879612320653094a8aaf46aca0bac81383
Attempt 1:
REWORK
Attempt 2 product:
09955667
Evidence:

* exact-SHA CI 4/4;
* Core 16811 / 16811;
* Workspace filter 1273 / 1273;
* Controller verification:
EXECUTION_VERIFIED
14 / 14;
* nc1, nc2 and nc3:
CONTROL PASS;
* direct review:
no blocking core defect;
* independent review:
previous findings corrected, with N-1 and N-2 remaining.

The previous three serious defects are considered CLOSED:

1. InitialDraftState / CurrentDraftState aliasing;
2. Apply lock released while operation still active;
3. duplicate comparison key capable of false "unchanged".

The previous F4 phase-wide attempt budget is SUPERSEDED.
For the decomposed F4 implementation, attempts are now TASK-LOCAL.
Reason:
F4 was explicitly decomposed into multiple independently contracted,
planned, delegated and verified tasks:

* F4-T1
* F4-T2
* F4-T3
* F4-T4A/T4B as finally contracted

A single three-attempt pool across all of them creates artificial coupling
between independent deliveries and discourages correction of real defects.
New rule for F4:
each separately contracted F4 task begins with:
attempts = 1 / 3
and maintains its own history.
Historical F4-T1 attempts are NOT rewritten.
Therefore:
F4-T1 is currently:
attempt 2 / 3
and may use:
attempt 3 / 3
if required.
When F4-T2 begins:
attempts = 1 / 3
for T2.
Likewise later separately contracted F4 tasks receive their own 1 / 3
budget.
This disposition applies to F4 only.
Do not infer a global AUTOMATION_PLAN change.
N-1 MUST BE CORRECTED BEFORE F4-T1 CLOSE.
Do NOT close T1 with this limitation.
Finding:
the new guard no longer detects uppercase:
BOM
in the member-name position required by INV-F4T1-22.
Although another parent-namespace guard catches some BOM cases, that does
not satisfy the specific T1 guard contract.
This is a contract regression.
Authorize:
F4-T1 attempt 3 / 3
but constrain it to a MINIMAL correction.
Scope of Attempt 3:

* restore case-insensitive/equivalent BOM detection required by
INV-F4T1-22;
* add exact regression tests proving:
BOM
Bom
bom
and the relevant prefix/suffix/member-name cases required by the
contract;
* no unrelated DraftSession redesign;
* no T2 host implementation;
* no T3 UI;
* no T4 mutation host.

If the correction can be made without changing product behavior outside the
guard/test surface, keep it that small.
Required:
RED
GREEN
exact-SHA CI
direct review
independent review
normal Controller verification
normal controls
After PASS:
close F4-T1.
N-2 does NOT block F4-T1 close after N-1 is corrected.
Finding:
InitialDraftState currently retains the object supplied by the future read
host.
Risk:
if a host later retains and mutates that same instance, the draft's initial
reference state could be altered externally.
Disposition:
MANDATORY F4-T2 CONTRACT OBLIGATION.
F4-T2 must establish an ownership boundary such that state entering the
DraftSession cannot subsequently be mutated through a host-held alias.
Acceptable mechanisms include:

* immutable state;
* defensive copy at the port boundary;
* defensive copy inside DraftSession creation;
* another mechanism with equivalent proof.

Required invariant:
after admission/materialization, mutating or reusing any host-side object
cannot change:
InitialDraftState
or silently alter dirty calculation.
Add an adversarial test that deliberately mutates/reuses the source object
after draft creation.
This test must fail before the T2 protection and pass after it.
Decision:
OPTION (b) ACCEPTED, WITH A NARROW PILOT BOUNDARY.
F4 MAY implement a compact Push Back editing surface over the SAME underlying
Push Back editable state.
Purpose:
prove the F4 DraftSession / dirty / stale / Actualizar architecture.
This does NOT constitute full Push Back editor migration.
Requirements:

1. one underlying editable state;
2. no parallel product model;
3. no second persisted authority;
4. closed, explicitly enumerated field set;
5. unsupported fields/features remain residual to F5;
6. no attempt to reproduce the complete legacy editor in F4;
7. the same DraftSession semantics must be capable of supporting the fuller
F5 surface later.

Record this as:
F4-PILOT-PUSHBACK-01
and as a staged implementation clarification to D-15/F5.
Before implementing T2/T3, the contract must enumerate the exact pilot
fields.
Do not let the Worker choose an arbitrary set.
Prefer the smallest coherent group that exercises:

* scalar edit;
* validation;
* dirty;
* pending text;
* stale comparison;
* write/readback.

The pilot exists to prove architecture, not feature completeness.
Record explicitly for F5:
Push Back legacy editor capabilities not represented by the compact F4
pilot remain migration work.
F4 PASS must NOT be described as:
"Push Back editor migrated".
It means:
"Push Back provides the bounded edit pilot used to prove DraftSession and
Actualizar semantics."
ACCEPTED.
Command name:
RACKPANELDESCARTAR
Use only if a command surface is actually required by the contracted close /
recovery path.
Semantics:

* targets the active RackCad Workspace session/draft according to the
contract;
* discards only in-memory panel draft/pending state;
* performs NO authored write;
* cannot affect another document/session;
* does not silently choose a dirty rack if the target is ambiguous.

If ambiguity exists:
fail with a clear message and zero effect.
Do not turn this command into a general editor command API.
ACCEPTED.
For a rack that is:
No editable aquí
F4 shows:

* diagnostic/status;
* reason when available;
* instruction to use RACKEDITAR.

Do NOT launch RACKEDITAR automatically from F4.
"Abrir en editor clásico" remains an F5 obligation under IC-03.
This avoids introducing another command-launch mechanism during the high-risk
F4 mutation work.
NO DISPENSATION.
Smoke-2 must use representative REAL RackCad drawings.
Synthetic-only drawings are not sufficient for F4 close.
Owner/operator participation is expected for this milestone.
The automated session should prepare:

* exact build;
* exact DLL/package;
* procedure;
* expected outcomes;
* evidence forms;
* rollback/recovery instructions.

But F4 may not be finally CLOSED / PASS until the required real-host
observations are made.
Smoke-2 must exercise at minimum:

1. open representative Push Back rack;
2. enter edit intent;
3. edit one pilot field;
4. verify dirty;
5. switch tab and return;
6. verify draft persists;
7. verify DWG remains unchanged before Actualizar;
8. pending invalid text across a non-blockable boundary;
9. blockable invalid transition aborts correctly;
10. explicit Actualizar;
11. reread after write;
12. external authored change before Actualizar;
13. stale/conflict path;
14. cancel conflict without write;
15. successful retry/reload path;
16. document/session mismatch produces zero write;
17. dirty/pending draft causes BeginDocumentClose veto;
18. discard/clean allows document close;
19. Undo after Actualizar;
20. verify one logical AutoCAD Undo step for the explicit update;
21. no cross-rack own-mutation contamination;
22. no event-handler DWG read under IC-04.

Also use the opportunity to repeat:

* SN-06c;
* tab portion of SN-10;
* representative-rack Select / View / Locate;
* document-switch safety.

RATIFIED.
T4B may use:
ExecuteInCommandContextAsync
for the explicit Actualizar mutation.
This is intentionally DIFFERENT from the F2 navigation host clarification.
Reason:
Actualizar is an authored DWG mutation and the Freeze requires the write
boundary to execute through the controlled mutation path.
Limit the command-context exception to the exact mutation adapter/file(s)
authorized by the T4B contract.
Do not generalize it back to navigation.
T4B must prove in real AutoCAD:

* explicit user action only;
* correct document/session;
* quiescent/modal preconditions;
* revalidation immediately before mutation;
* DocumentLock/transaction boundaries as required;
* one mutation operation;
* POST reread;
* own-mutation hints handled correctly;
* no second concurrent Actualizar;
* exactly one logical Undo step;
* failed/stale/conflict paths produce zero authored partial write.

Event callbacks:
do not read DWG.
They enqueue/coalesce hints only.
Any summary/draft reread happens later through an authorized application
context read path.
F4-T2/T4 must preserve and test this distinction.
The already-versioned Attempt-1 relay note that says its first participant
count was 2 is factually incorrect.
Actual first count:
0
Do NOT rewrite historical evidence.
Append a correction note referencing the original artifact and explaining
the template-copy error.
This has no product impact.
Proceed:

1. record this disposition;
2. set F4-T1 attempts = 3 / 3;
3. issue the minimal N-1 correction contract;
4. execute and verify Attempt 3;
5. close F4-T1 if PASS;
6. set F4-T2 attempts = 1 / 3;
7. issue F4-T2 contract including mandatory N-2 anti-aliasing requirement
and F4-PILOT-PUSHBACK-01;
8. continue normal T2 process;
9. then T3 with attempts = 1 / 3;
10. then T4A/T4B according to the final decomposition, each with its own
task-local attempt budget if separately contracted;
11. prepare Smoke-2;
12. STOP for Owner/operator only when real AutoCAD Smoke-2 requires manual
execution or if another material architecture decision appears.

F0:
CLOSED / PASS
F1:
CLOSED / PASS
F2:
CLOSED / PASS WITH LIMITATIONS
F3:
CLOSED / PASS WITH LIMITATIONS
F4-T1:
Attempt 2 VERIFIED
Attempt 3 AUTHORIZED for N-1 only
F4-T2:
WAITING
future budget 1 / 3
F4-T3:
WAITING
F4-T4:
WAITING
Smoke-2:
REQUIRED / REAL DRAWINGS
F5-F7:
PENDING
Owner Validation:
PENDING
