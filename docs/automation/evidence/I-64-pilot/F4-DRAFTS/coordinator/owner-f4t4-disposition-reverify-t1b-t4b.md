I-64 — F4-T4
OWNER / COORDINATOR DISPOSITION
Current I-64 branch:
c3ae1c970c908ef541840fc583680b86376cae0b
Current main:
812baf879612320653094a8aaf46aca0bac81383
======================================================================

1. F4-T4A2
======================================================================

Decision:
OPTION (a)
Authorize ONE Controller reverification with a clarification note.
Do NOT:

* dispense the failed verification;
* spend Attempt 3;
* modify product;
* modify tests;
* invoke another Worker.

Reason:
Attempt 2 product is already supported by:

* GREEN ae0a7b40;
* exact-SHA CI 4 / 4;
* direct review with no blocking/important defects;
* independent re-review ACCEPTABLE WITH MINORS;
* contract explicitly allowing the M-3 deviation.

The Controller STOP came from a misreading of the contract, not from a
product failure.
Provide the Controller a narrow clarification that:
INV-F4T4A2-05 / the Attempt-2 contract explicitly declares the relevant
deviation.
The clarification must:

* quote the exact contract authority;
* explain why the observed behavior is permitted;
* change no product fact;
* change no test fact;
* change no scope;
* change no RED/GREEN evidence.

Run ONE reverification.
If:
EXECUTION_VERIFIED / 14 of 14
then run nc1-nc3 and close T4A2 normally.
If the reverification fails for another reason:
STOP.
Do not consume Attempt 3 automatically.
Decision:
AUTHORIZE EMISSION OF T1B PACKAGE v4
subject to the interpretations below.
T1B is CHARACTERIZATION ONLY.
Its objective is to freeze the currently observed behavior of the legacy
Push Back editor before T4B/T4C depend on its write-facing seams.
It is NOT an editor redesign.
AUTHORIZED.
Because characterization tests should pass on the real current product,
T1B may use a special RED:
RED =
new permanent characterization tests
+
temporary, explicitly enumerated product mutations that make those tests
fail.
GREEN =
revert ONLY those intentional mutations.
Required final GREEN condition:
the product tree for the mutated production paths must be byte-identical to
the T1B BaseSha.
The characterization tests remain.
This is the intended proof that the tests actually discriminate behavior.
Every RED mutation must be:

* explicitly enumerated in the contract;
* mechanically reversible;
* limited to the exact behavior under characterization;
* incapable of opening uncontrolled UI in CI;
* incapable of writing unexpected user/local data.

The GREEN must prove:
Base product bytes
final GREEN product bytes
for every intentionally mutated production file.
Any residual product delta:
FAIL.
AUTHORIZED.
The characterization boundary may include the measured transitive
dependencies of the Push Back classic editor.
Use the measured closure from the package, not an inferred whole-UI closure.
Do not expand into unrelated editors merely because they share generic
helpers.
AUTHORIZED WITH CONTAINMENT.
The test may instantiate/close the real header configurator if that is the
only reliable way to prove the behavior.
But:

* use an isolated temporary test location;
* never use a user's real preference/config file;
* prove cleanup;
* no modal UI may remain alive after the test;
* no persistent state may escape the test sandbox.

If the current implementation necessarily writes a file:
redirect that file to an isolated fixture/temp location.
Do not accept writes into the real application/user configuration.
Characterization may freeze odd existing behaviors as debt.
Do NOT "fix while characterizing":

* Enter behavior;
* Escape/close behavior;
* Insertar varias always enabled;
* cancel-after-confirm oddities;
* header configurator applying on close;
* other measured legacy behavior.

Record them as legacy behavior.
Changing them belongs to a separate product decision.
Decision:
DO NOT EMIT v1.
Continue with the redesigned v2 and its re-review.
The v1 review correctly found architectural blockers.
Decision:
USE A PILOT-SPECIFIC WRITE PATH
rather than forcing the pilot through the existing I-55 mutation seam.
Reason:
the F4 Push Back pilot has different requirements:

* scoped primitive write operations;
* Application-owned orchestration/classification;
* explicit failure at unit vs commit level;
* stale/conflict guarantees;
* POST reread;
* testability without AutoCAD.

Trying to force this through the existing coarse I-55 seam would either:

* move too much policy into Plugin;
* weaken testability;
* or change an already integrated write authority used by legacy Insertar /
RACKEDITAR.

D-13 already permits a panel-specific path.
Because the pilot-specific path is new:
OV-RED / prior physical validation of the I-55 path does NOT validate this
new path.
Therefore:
Smoke-2 becomes the first physical validation of the T4B mutation path.
This must be explicit in the package.
Application owns:

* stale decision;
* conflict classification;
* mutation sequencing;
* failure classification;
* POST interpretation.

Plugin owns primitive AutoCAD operations only.
Do NOT let Plugin decide business policy.
The redesigned run-scoped primitive port is accepted in principle.
Requirements:

* supplied per Actualizar execution;
* not retained as long-lived mutable host state;
* scope ends on every success/failure path;
* no operation escapes the run;
* no DBObject/ObjectId/Transaction crosses into Application;
* one transaction/mutation boundary as finally contracted.

AUTHORIZED if and only if:

* behavior of Insertar remains identical;
* source guard proves the moved logic is equivalent;
* characterization T1B protects the relevant existing behavior;
* no caller-visible semantics change.

Treat as refactor, not new authority.
If T1B reveals a behavior difference:
STOP and reconcile before T4B.
Decision:
LOG ONLY
unless the purge failure makes authored state correctness uncertain.
If purge is best-effort cleanup with no returned status today:
do not invent a false success/failure contract around it.
But if failure can leave the rack partially or incorrectly authored:
it must instead fail the mutation.
The package must prove which category applies.
Clarification:
T4B owns the authored mutation semantics and implementation.
T4C may later wrap/call T4B through:

* ExecuteInCommandContextAsync;
* DocumentLock;
* host command-context integration.

T4C must NOT implement a second authored mutation algorithm.
Therefore:
T4B = write implementation/semantics
T4C = execution-context integration
No contradiction.
The redesigned path must distinguish:
A. failure before commit
-> zero authored result
B. commit failure / uncertain persistence
-> state UNKNOWN / ERROR
-> do NOT claim rollback unless AutoCAD guarantees it
C. successful commit
-> POST reread required
Do not convert commit uncertainty into ordinary failure or success.
Successful mutation is not complete until POST reread succeeds sufficiently
to establish the authored result.
If write succeeds but POST reread fails:
use the already established AppliedWithWarnings / unknown-post semantics
defined by the current DraftApplyOutcome model.
Do not silently mark the draft clean based only on Commit() returning.
Proceed in this order:

1. record this disposition;
2. perform the single T4A2 reverification;
3. if VERIFIED:
nc1-nc3
custody
close T4A2;
4. emit and execute T1B v4;
5. use T1B results to validate the remaining T4B assumptions;
6. finish independent review of T4B v2;
7. incorporate all blocking/important findings;
8. only then emit T4B.

Do NOT emit T4B before T1B closes.
Also record the pending F4-E51 documentation note in the next useful
documentation commit.
Do not rewrite F4-E50.
F4-T3:
COMPLETE
F4-T4A1:
CLOSED / PASS WITH LIMITATIONS
F4-T4A2:
PRODUCT ATTEMPT 2 COMPLETE
ONE REVERIFICATION AUTHORIZED
F4-T1B:
PACKAGE v4 AUTHORIZED FOR EMISSION
F4-T4B:
PACKAGE v2 UNDER REVIEW
NOT AUTHORIZED FOR EMISSION YET
F4-T4C:
WAITING
Smoke-2:
MANDATORY
F5-F7:
PENDING
