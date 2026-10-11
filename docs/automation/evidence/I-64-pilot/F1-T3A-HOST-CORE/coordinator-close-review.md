# I-64 — Revisión del Coordinator del cierre de F1-T3A-HOST-CORE

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-11, tras el cierre documental 7869e5fa
(CI 38120061176 4/4).

```text
I-64 — COORDINATOR T3A CLOSE REVIEW
TARGET SHA:
7869e5fa15b2174565709837a1623caf1fa081d7
origin/main:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
Coordinator disposition:
PASS
Record:
F1-T3A-HOST-CORE =
COMPLETE_UNDER_I64_SUBSTITUTE_PROTOCOL_EVIDENCE
ProductStatus:
EXECUTION_VERIFIED / NONE
ProductVerifiedSha:
ba52b3b983fb7d2a02393df643e8323b125b3d03
ProtocolClose:
SATISFIED UNDER A-4 + A-6
Final controls:
nc1 =
PASS
Controller nc2 =
CONTROL NOT PASSED
I64-SCOPE-BRIDGE-01 =
PASS
Controller nc3 =
NOT PASSED / P-04
I64-FREETEXT-BRIDGE-01 =
PASS
nc4 =
PASS
Do not rerun any T3A control.
Do not reopen T3A product.
The T3A nc3 classification is ACCEPTED.
The session should have returned the repeated T2 failure mode to the
Coordinator before applying the same INVALID_OUTPUT classification, but this
procedural deviation does not invalidate the resulting protocol close.
Reason:
A-6 is active for I-64 and its preconditions are satisfied:

* real product independently EXECUTION_VERIFIED;
* all permitted nc3 executions exhausted;
* FreeText detected consistently;
* synthetic REWORK transformed into STOP by real-task attempt/correction
rules;
* historical nc3 outputs preserved as NOT PASSED / P-04;
* I64-FREETEXT-BRIDGE-01 passed.

Preserve the deviation in evidence.
Do not relabel any nc3 execution PASS.
AuthorityRevision typo "...8354":
NON-BLOCKING CONTROLLER TEXTUAL ERROR.
Preserve it.
Do not modify the Controller output.
Verification stderr:
"failed to record rollout items"
NON-BLOCKING CLI AUXILIARY ERROR because:

* process exit = 0;
* structured verification output is valid;
* verification completed;
* no missing evidence required for the disposition;
* no Git/product side effect resulted.

Preserve stderr verbatim.
Line-count difference:
NON-BLOCKING REPORTING CONVENTION.
Use git diff --numstat as canonical file line delta when a numeric delta is
required.
In the next documentary/authority commit set:
last_evidence_commit =
7869e5fa15b2174565709837a1623caf1fa081d7
Record Coordinator T3A close review = PASS.
No product changes in this correction.
T3B is now authorized for CONTRACT PREPARATION ONLY.
Do NOT invoke Planning or Worker yet.
Derive the exact T3B contract from the CURRENT repository after T3A.
T3B conceptual objective:

* exactly one PaletteSet per process;
* lazy creation from the T3A controller/RACKPANEL path;
* one RackCad.UI root UserControl via AddVisual;
* view-model/state presentation;
* active-document/session presentation;
* empty state with no document;
* hidden/visible behavior through the T3A port/controller;
* focus policy at the host/UI boundary;
* panel-originated exception containment;
* Error state;
* local WPF resource merging;
* presentation of memory-only instrumentation;
* no authored writes;
* no navigation;
* no AutoCAD.Internal.

Before proposing T3B scope, inspect the actual T3A product at:
ba52b3b983fb7d2a02393df643e8323b125b3d03
Do not rely on the old monolithic T3 package assumptions.
Determine exactly:

* public T3A host/port/controller surface;
* existing filenames/namespaces;
* which Plugin file can own PaletteSet;
* how UI can be instantiated without adding AutoCAD references to RackCad.UI;
* existing WPF resource conventions;
* UI boundary guards;
* window/control census guards;
* any command/source guard affected by T3B;
* how controller lifecycle can create/show/hide the host without modifying
T3A files unnecessarily.

If T3B cannot be implemented without reopening T3A product files:
STOP Coordinator before contract issuance.
Do not silently expand scope.
Re-evaluate routing from the actual derived scope.
Expected classification:
Implementacion transversal a capas (corta)
Deep
Candidate Worker:
claude-sonnet-5-5 / high
But do not assume eligibility solely from T3A.
Freshly check routing/model catalog and current measured capability.
If derived T3B becomes Long-horizon:
STOP Coordinator.
Do not reset initiative attempts.
Propose for T3B:
Attempt = 3
AttemptsRemaining = 0
CorrectionsAuthorized = false
Propose invocation budget per TaskId using the same bounded model as T3A,
but return it for Coordinator issuance.
Any future valid REWORK requiring Worker correction remains S-11 unless new
authority explicitly changes that.
T3B is a new TaskId.
ChainRedSha starts null.
A real behavioral RED will be mandatory.
Derive exact UI tests from the actual surface.
Expected areas include:

* root view-model presents empty/document/session states;
* five selection contexts render deterministically;
* panel Error state on contained exception;
* host-visible state does not create DWG work by itself;
* resources resolve without App.xaml;
* UI contains no Autodesk references;
* PaletteSet source structure obeys host constraints.

Do not invent artificial failing tests.
No AutoCAD execution during T3B implementation.
Static compilation/source guards are allowed.
Smoke-1 remains separate.
Do not:

* launch acad.exe;
* load RackCad DLL into AutoCAD;
* modify profile;
* modify registry;
* modify ACL;
* modify TRUSTEDPATHS;
* modify SECURELOAD.

Freshly fetch all relevant branches before contract proposal.
Current observed I-65 during T3A close review:
16fa9e7649e139798d41188db5bbe8b3be94e086
Do not consume unmerged I-65 code/rules.
I-52 host RELEASE is still irrelevant to static T3B implementation but is
required later for Smoke-1.
I-62 protocol debt remains external to product.
F1-T3C-PROFILE-DOC remains:
RESERVED / NOT AUTHORIZED
Do not create it yet.
After:

1. versioning this T3A close decision and state correction;
2. exact CI 4/4;
3. fresh repository/T3A surface inspection;
4. fresh routing and branch overlap checks;
5. preparation of a T3B gate-contract package marked NOT EMITTED;

STOP at:
COORDINATOR_T3B_GATE_CONTRACT_REQUIRED
Return:

* documentary decision commit + exact CI;
* current HEAD/main;
* fresh I-52/I-65/I-62 tips and overlap;
* T3B objective;
* exact AllowedWriteScope;
* exact ForbiddenWriteScope;
* whether any T3A file must be reopened;
* proposed invariants;
* RequiredTests and expected RED;
* routing dimensions/class;
* proposed Worker/Controller cells;
* budget;
* Attempt/AttemptsRemaining;
* unresolved decisions.

Do NOT invoke T3B Planning.
Do NOT invoke T3B Worker.
```
