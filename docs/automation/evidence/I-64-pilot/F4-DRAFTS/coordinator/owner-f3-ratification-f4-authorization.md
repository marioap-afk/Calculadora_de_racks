I-64 — F3 RATIFICATION / F4 AUTHORIZATION
Current I-64 branch:
67ee8e4a801eb98ded494f2e63e7d486857f25c1
Current origin/main:
812baf879612320653094a8aaf46aca0bac81383
F3 — BROWSER SESSION
is:
CLOSED / PASS WITH LIMITATIONS
Accepted product identities:
F3-T1-BROWSER-CORE:
a92039bb
F3-T2-BROWSER-UI:
206e0f7a
Current branch tip:
67ee8e4a...
contains custody/documentation in addition to the verified products.
Do not replace the task product identities with the custody SHA.
Accepted:

* F3-T1 verification complete;
* F3-T2 corrected Attempt 2 verification complete;
* F3-T2 second independent review found no blocking defect/regression;
* nc1/nc2/nc3 closed under the already accepted I-64 control mechanisms;
* exact-SHA CI green;
* scenarios 4, 5 and 6 green;
* K = 1 demonstrated;
* no per-tab AutoCAD subscriptions;
* no per-tab retained AutoCAD objects;
* tab switching writes neither DWG nor selection/view;
* performance characterized without inventing thresholds.

F3 attempts:
2 / 3
F3 attempt history remains phase-local and closed.
Preserve F3-E12 exactly as known limitations.
R-1:
loaded summary does not automatically reread after erase/rename/undo.
Disposition:
ACCEPTED FOR F3.
F4 stale/obsolescence semantics are the natural next authority.
R-2:
transient "command in progress" presentation after document switch.
Disposition:
ACCEPTED.
UX/text cleanup later.
R-3:
keyboard tab activation/fix may lose focus.
Disposition:
ACCEPTED.
F5/F7 UX debt.
R-4:
PICKADD=0 edge case.
Disposition:
OPEN CHARACTERIZATION.
Do not silently change echo semantics in F4 unless required.
F3 did not have a host milestone.
Therefore:
F3 = PASS WITH LIMITATIONS
not:
host-validated browser complete.
Carry forward:

* SN-06c;
* tab-driven portion of SN-10;
* real-host validation that notification handling does not read DWG;
* real-host validation that deleted racks disappear correctly;
* representative production-rack Owner Validation.

These are later evidence obligations.
F3-E02 is RATIFIED.
Snapshot F:
ACCEPTED
Reason:
relative to accepted Snapshot E:

* same 104 keys, same order;
* no added/removed keys;
* six changed values only;
* all six are application-managed installation/runtime paths;
* no model change;
* no effort change;
* no service-tier change;
* no sandbox change;
* no trust-level change;
* no approval/permission/routing change;
* accredited binary unchanged;
* neutral Controller probe passed with effective behavior equal to baseline.

Snapshot F becomes the active config baseline for subsequent I-64 Controller
work.
Any later config hash change:
remeasure against F.
Do not infer acceptance automatically.
IC-04 is ACCEPTED WITH PRECISE WORDING.
The rule is:
AutoCAD event notification handlers themselves MUST NOT read the DWG.
They only enqueue/coalesce hints.
A lightweight summary read may occur AFTER notification processing, as a
separate scheduled/user-visible operation, only when the normal application
context conditions are satisfied:

* correct SessionId/document;
* active document as required;
* quiescent;
* no RackCad modal;
* DocumentLock;
* bounded operation;
* no DBObject/Transaction survives.

The summary read must NOT occur synchronously inside:

* ObjectModified;
* ObjectErased;
* ImpliedSelectionChanged;
* CommandEnded;
* Idle/event callback simply because the callback fired.

If the drain completes and then schedules/materializes the active surface
through the normal read path, that is acceptable.
Therefore:
"after the drain"
is acceptable.
"inside the notification handler"
is not.
Do not describe IC-04 as permission to read from AutoCAD events.
Record as process debt:
An EXECUTION_VERIFIED 14/14 result did NOT establish product correctness for
F3-T2.
Six real defects remained.
Therefore for F4 and later:
Controller verification remains necessary but NOT sufficient.
Every substantial UI/host delivery must also receive:

* direct code review by the session;
* independent adversarial review of the diff;
* host smoke when the gate requires host behavior.

Do not spend an attempt merely because an independent reviewer raises a
non-contractual cosmetic preference.
Do spend/correct when a finding violates Freeze, contract, safety or
observable product behavior.
F4 starts with a fresh phase-local budget:
attempts = 1 / 3
AUTHORIZED.
F3's:
2 / 3
does NOT carry into F4.
Record the new budget before the first F4 Worker delegation.
F4 is the first phase where the Workspace becomes an editing workspace,
rather than principally navigation/browser UI.
Primary objectives from Freeze V5:

* DraftSession;
* StaleBase;
* InitialDraftState;
* CurrentDraftState;
* pending text;
* real dirty semantics;
* lazy draft lifetime across tabs;
* external-change detection;
* conflict/stale behavior;
* explicit Actualizar write boundary;
* session/document isolation;
* close/veto behavior;
* no DWG writes before Actualizar.

This phase is materially higher risk than F3.
Do not combine the whole phase into one giant Worker.
No accepted F4 gate-contract package currently exists.
Prepare one before implementation.
Read and bind at minimum:
Proposal V5:
D-10
D-11
D-12
D-13
D-14
D-17
and all associated invariants / OV scenarios.
Also bind:

* integrated I-55 mutation authorities;
* ADR-0029;
* ADR-0032;
* current Workspace/session/browser contracts;
* current F2 navigation host clarification;
* F3 BrowserSession behavior.

Do not reconstruct F4 from memory.
Prepare the package with a decomposition close to:
F4-T1 — DRAFT CORE
Pure Application layer.
Responsibilities:

* DraftSession state machine;
* InitialDraftState;
* CurrentDraftState;
* pending text model;
* Dirty calculation;
* state transitions:
Clean
Dirty
PossiblyStale
Conflict
Applying
Unknown/Error as Freeze requires;
* per-(SessionId,RackId) registry;
* persistence across tab materialization;
* close/discard policy in memory.

No AutoCAD.
No WPF.
No DWG writes.
F4-T2 — DRAFT READ / ADMISSION HOST
Plugin/Application boundary.
Responsibilities:

* one logical read for edit intention;
* StaleBase capture;
* InitialDraftState materialization from the same logical read;
* RackSiblingMembership admission;
* source-envelope change between preview and edit intention;
* stale-base rereads;
* "Check now";
* clean lazy reload;
* exact session/document validation.

Reads only.
No authored mutation yet.
F4-T3 — EDITING UI / PENDING TEXT
Workspace UI.
Responsibilities:

* first field interaction fixes preview;
* pending text;
* blocked vs non-blockable internal commit boundaries;
* dirty marker;
* tab switching behavior;
* rematerialization of K=1 surface;
* validation behavior;
* close Dirty: Discard / Cancel;
* pending text survives non-blockable boundaries.

Still no DWG write.
F4-T4 — ACTUALIZAR / STALE / MUTATION HOST
Highest-risk task.
Responsibilities:

* explicit Actualizar only;
* PREPARE / re-read / stale comparison;
* conflict decision;
* integrated I-55 membership authority;
* correct mutation authority;
* own-mutation scope;
* POST reread;
* zero stale write;
* session validation immediately before mutation;
* document lock / required execution context;
* failure semantics;
* authored write only here.

This task MUST receive:

* exact RED/GREEN;
* Controller;
* negative controls;
* direct review;
* independent adversarial review;
* real AutoCAD host smoke.

Do not start this task until T1-T3 are closed.
F4 must NOT become F5.
Do not migrate every legacy editor yet.
A minimal supported edit path sufficient to prove DraftSession and
Actualizar semantics is acceptable if that is what the Freeze/gate defines.
Editor-family migration belongs to F5.
Do not add F6 global inventory/browser responsibilities.
The F4 contract package must make these explicit and testable:

1. Dirty is NOT raw JSON inequality.
2. Dirty =
CurrentDraftState != InitialDraftState
OR pending text exists.
3. Preview alone does NOT create a DraftSession/StaleBase.
4. First edit intention performs one logical read that yields:
StaleBase
admission
InitialDraftState
5. No authored write occurs before explicit Actualizar.
6. Tab changes do not write DWG.
7. Closing/discarding a draft does not write DWG.
8. A non-blockable boundary preserves pending invalid text instead of
silently committing/repairing it.
9. A blockable boundary may abort if pending input is invalid.
10. StaleBase is used only for external-change detection.
11. A stale check rereads authoritative authored data.
12. External changes can never be overwritten without detection.
13. Session A can never write document B.
14. Document destruction retires the session/drafts.
15. Dirty/pending drafts participate in BeginDocumentClose veto semantics.
16. own-mutation hints cannot falsely mark the same operation as external.
17. POST state is established by reread, not by assuming the write succeeded.
18. all DB objects and transactions die inside the operation.

Unlike F3, F4 must not close solely on unit/UI tests.
Before F4 close, exercise in real AutoCAD at minimum:

* open a real/representative rack;
* enter edit intent;
* modify without Actualizar;
* switch tab and return;
* verify no DWG mutation;
* invalid pending text over a non-blockable boundary;
* explicit Actualizar;
* external modification before Actualizar;
* stale/conflict path;
* document/session mismatch safety;
* close document with dirty/pending draft and verify veto;
* successful close after discard/clean;
* Undo/Redo interaction where relevant.

Prefer representative RackCad racks, not only synthetic DXF.
Proceed now with:

1. record this disposition;
2. set F4 attempts = 1 / 3;
3. prepare the F4 gate-contract package from the actual authorities;
4. perform independent contract review before issuing it;
5. issue F4-T1 contract;
6. execute/verify/close T1;
7. then T2;
8. then T3;
9. then T4;
10. perform required real-host Smoke;
11. close F4 only if all gate obligations are evidenced.

You may adjust task boundaries if the repository evidence shows a cleaner
decomposition, but do not collapse mutation into the early pure/core tasks.
Do not request Owner approval merely for ordinary progress inside the
authorized F4 scope.
Return if:

* F4 requires changing frozen authored semantics;
* a required editor capability cannot be represented without pulling F5
materially into F4;
* product attempts exhaust;
* a real AutoCAD safety invariant fails;
* external change detection cannot be made reliable;
* BeginDocumentClose protection conflicts with the actual host;
* or a material new architecture decision appears.

F0:
CLOSED / PASS
F1:
CLOSED / PASS
F2:
CLOSED / PASS WITH LIMITATIONS
F3:
CLOSED / PASS WITH LIMITATIONS
F4:
AUTHORIZED
attempts 1 / 3
contract package to be prepared
F5-F7:
PENDING
Smoke-2:
PENDING
Owner Validation:
PENDING
