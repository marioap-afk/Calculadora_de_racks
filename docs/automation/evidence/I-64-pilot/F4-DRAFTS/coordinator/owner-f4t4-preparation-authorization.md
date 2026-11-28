Ratificación del cierre de F4-T3 y autorización de preparación de F4-T4 del Owner y del Coordinator (2026-11-13, tras la custodia `7297e1f1`). Texto pegado por el Owner en la sesión, literal:

I-64 — F4-T3 CLOSE RATIFICATION / F4-T4 PREPARATION AUTHORIZATION
Current I-64 branch:
7297e1f11378461b860b99023d19dfab1398fdc7
Current main:
812baf879612320653094a8aaf46aca0bac81383
F4-T3 is:
COMPLETE AT SUBTASK LEVEL
Closed:
F4-T3A
F4-T3B1
F4-T3B2
F4-T3C1
F4-T3C2
T3C2 product:
4e34bb56
T3C2 custody:
7297e1f1
Do not reopen T3 merely because T4 later exercises its interfaces.
Record the missing readable custody note in the NEXT useful documentation
commit.
Do NOT rewrite F4-E50.
State only:

* the auxiliary Worker script containing a local path was excluded from
public custody;
* only its SHA-256 was retained;
* synthetic test paths were redacted according to the custody rules;
* no personal/local path was published.

This is documentation only.
Authorize PREPARATION of F4-T4 now.
Do NOT emit a monolithic T4 contract yet.
First map and review the mutation architecture.
F4-T4 is the highest-risk part of F4 because it crosses the authored DWG
write boundary.
Prepare T4 as AT LEAST two independently contracted tasks:
F4-T4A — PREPARE / STALE / CONFLICT
Owns:

* authoritative reread before update;
* StaleBase comparison;
* external-change detection;
* conflict classification;
* "Comprobar ahora";
* reload/rebase/keep-draft policy where already frozen;
* validation immediately before mutation;
* building an immutable mutation plan;
* proving zero authored write in PREPARE.

No DWG mutation.
F4-T4B — MUTATE / POST / UNDO
Owns:

* explicit Actualizar;
* ExecuteInCommandContextAsync;
* DocumentLock;
* transaction/mutation boundary;
* integrated I-55 membership authority;
* applying ONLY pilot-owned changes;
* no stale write;
* atomicity;
* own-mutation scope;
* POST reread;
* draft baseline reset after success;
* failure semantics;
* exactly one logical AutoCAD Undo step.

This is the only F4 task allowed to perform authored writes.
If repository mapping shows that host/command wiring, Undo integration, or
post-write reconciliation makes T4B too broad for one Worker:
split a third task:
F4-T4C — HOST / UNDO / POST-INTEGRATION
but only if the code evidence justifies it.
Do not create T4C merely to gain attempt budget.
Each separately contracted T4 task starts with:
attempts = 1 / 3
This follows the accepted task-local F4 budget model.
Do not treat T4A + T4B as one shared pool.
PREPARE must be read-only.
It may produce:

* authoritative current facts;
* StaleBase comparison;
* conflict result;
* immutable mutation intent/plan.

It must NOT:

* mutate DWG;
* update block definitions;
* commit draft state as if a write succeeded;
* clear dirty merely because PREPARE passed.

T4A must clearly separate:
StaleBase
from:
CurrentDraftState
and:
current authoritative DWG reread.
Conflict/stale detection must compare authored facts, not raw serialized
formatting where the Freeze says semantic comparison applies.
No external authored change may be overwritten silently.
The Push Back pilot still edits only its contracted field set.
For authored data the pilot does NOT own:
use the authoritative current base according to the already accepted F4
rules.
Do NOT rebuild the outgoing authored state from stale draft copies of
non-pilot fields.
This is especially important for the prior ComposeForWrite finding.
Use the integrated existing authorities.
Do NOT invent new rack-membership/write authority.
In particular preserve:

* RackSiblingMembership / integrated I-55 authority;
* existing identity/view authorities;
* NavigationTargets only for navigation, not as write membership authority.

For Actualizar:
ExecuteInCommandContextAsync
is RATIFIED and REQUIRED where the current Freeze/host contract calls for
command-context authored mutation.
This differs intentionally from F2 navigation.
Limit the exception to exact T4 mutation adapter files.
Required T4B oracle:
Either the intended authored mutation commits successfully as one logical
operation,
or:
zero partial authored result remains.
A failure halfway through must not leave a half-updated rack.
The lock already proved in T1 must remain effective.
No second Actualizar may start while one is Applying.
Every exit path must release the operation exactly once.
Do not allow Error/Discard paths to release it early.
POST must be established by rereading authored state.
Do NOT assume:
"the write call returned, therefore state is correct."
After success:

* reread;
* establish new StaleBase;
* establish new InitialDraftState;
* align CurrentDraftState;
* clear pending text as appropriate;
* Dirty becomes false only from the verified post-state.

Events produced by RackCad's own update must not appear as an external
conflict against the same operation.
Own-mutation scope must remain:

* session-bound;
* rack/member-bound;
* active only for the mutation window;
* released on all exit paths.

Other racks must not be masked.
T4B / Smoke-2 must prove:
one explicit Actualizar
produces exactly:
ONE logical AutoCAD Undo step.
Undo must restore the authored rack to the pre-update state.
Redo should restore the post-update state if the host supports the normal
AutoCAD behavior.
Do not claim Undo compliance from unit tests alone.
Carry from T3C1:
ErrorReason must only be populated/presented in Error state.
Make this a declared T4 contract change, not an accidental cleanup.
Carry the 8 T3C2 minors into T4 contract review.
Prioritize the two important guard-quality items:

* Sí/No confirmation guard must detect inverted implementation;
* Authorize guard must detect inverted/incorrect authorization behavior.

Fix them where T4 touches those guard surfaces.
Do not spend an entire T4 correction attempt only on comments or cosmetic
documentation.
T4 cannot close on automated tests alone.
Before F4 closes, Smoke-2 in real AutoCAD is mandatory.
Use representative real RackCad drawings, not synthetic-only fixtures.
At minimum:

1. open representative Push Back rack;
2. enter editing;
3. change AnnotationScale;
4. verify dirty;
5. switch tabs and return;
6. verify no DWG change before Actualizar;
7. invalid pending text survives a non-blockable boundary;
8. blockable invalid transition aborts;
9. explicit Actualizar;
10. authored drawing actually changes correctly;
11. POST reread produces clean draft;
12. external authored change before Actualizar;
13. stale/conflict detected;
14. cancel conflict -> zero write;
15. reload/keep path as frozen;
16. document mismatch -> zero write;
17. dirty close veto;
18. comparator failure -> conservative veto;
19. explicit discard allows cleanup;
20. exactly one Undo step;
21. Undo restores pre-update authored state;
22. Redo restores post-update state if normal AutoCAD behavior allows;
23. own-mutation events do not create false conflict;
24. another rack is not masked by own-mutation scope;
25. CLOSE / CLOSEALL / QUIT / tab / API lifecycle observations assigned from
T2B2B;
26. SN-06c;
27. tab-driven SN-10;
28. representative-rack Select / View / Locate;
29. session/document switch safety;
30. IC-04 timing: no DWG read inside event/Idle callback stack.

Proceed now:

1. record this disposition;
2. map actual mutation/read/write code;
3. map I-55 write authorities;
4. map current Push Back legacy update path;
5. map Undo/transaction behavior;
6. map PREPARE/MUTATE/POST candidate seams;
7. prepare T4 package with proposed decomposition;
8. run independent adversarial review;
9. apply blocking/important findings;
10. only then emit T4A.

You do NOT need Owner approval merely to prepare/review the package.
Return to Owner only if package review shows that:

* the frozen write authority must change;
* T4 cannot be separated safely into read-only PREPARE and mutation;
* the pilot cannot write without pulling substantial F5 migration into F4;
* Undo requires a material host architecture change;
* the legacy Push Back path has semantics that conflict with the accepted
pilot model;
* or another material product decision appears.

Otherwise:
emit T4A and continue normally.
F4-T3:
COMPLETE
F4-T4:
PREPARATION AUTHORIZED
F4-T4A:
PACKAGE TO PREPARE
F4-T4B:
WAITING
Smoke-2:
MANDATORY
F5-F7:
PENDING
