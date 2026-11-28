I-64 — F2-T2A-NAVIGATION-HOST
nc1 DISPOSITION
Attempt 2 product:
VERIFIED
Verification:
R20261109T201422Z-89a6
Result:
EXECUTION_VERIFIED / NONE
VerifiedSha:
44a692a4ec386e559cc6d330b3d78b87dbad6581
nc1:
R20261109T202056Z-3d86
3d86 is classified as:
SCHEMA_VALID_BUT_ORACLE_INVALID
INVALID_OUTPUT
Reason:
The nc1 mutation is intended to test:
Identity
but the Controller marked:
Handoff = fail
Identity = fail
and chose:
FailureClass = Handoff
The expected relative oracle requires all checks before Identity to remain
equal to the accepted real verification.
That did not happen.
The overall BLOCKED / STOP result is not enough.
Therefore:
nc1 3d86 = INVALID_OUTPUT
It is NOT:
CONTROL PASS
product defect
Worker defect
attempt consumption
Preserve 3d86 exactly as emitted.
ONE nc1 retry is AUTHORIZED.
Use:

* same verified product;
* same real verification inputs;
* same nc1 mutation;
* same verification prompt;
* only a new RunId.

Do not modify product.
Do not invoke Worker.
Do not consume an attempt.
The retry must satisfy:
Termination = pass
Handoff = pass
Authority = pass
Contract = pass
Identity = fail
FailureClass = Identity
Classification = EXECUTION_BLOCKED
Disposition = STOP
All checks after Identity must remain equal to the accepted real
verification unless the protocol explicitly allows otherwise.
No cascade into Handoff.
For this negative control:
Handoff validates the structure and internal identity of the handoff record
as a handoff artifact.
It must NOT fail merely because the deliberately mutated CurrentSha no longer
matches repository state.
That mismatch belongs to:
Identity
for nc1.
This is the same defect class previously observed in historical controls.
Do not change the mutation.
Do not weaken Identity.
CONFIRMED.
If the nc1 retry satisfies its relative oracle:
nc1 = CONTROL PASS
Then proceed normally to:
nc2
and after nc2 closes:
nc3
Each control must stop immediately if its oracle is not met.
Do not continue past an invalid control output.
A-4:
available only if nc2 reproduces the exact accepted historical Scope defect
under the already defined conditions.
A-6:
available only if nc3 reproduces the exact accepted historical FreeText
defect under its existing conditions.
A-11:
NOT AVAILABLE
Its single F2-T1 application was consumed.
No new bridge is inferred for T2A.
README §10 allows:
initial control invocation
plus
up to two reruns for invalid/transport control output.
For nc1:
initial:
3d86
valid reruns used:
0
Therefore:
retry budget remaining after this authorization:
2 protocol reruns total available,
but authorize ONLY ONE now.
If the next output again misses the oracle:
STOP to Coordinator.
Do not automatically spend the second retry.
Attempt 2 product remains:
IMPLEMENTATION_COMPLETE
GREEN:
44a692a4ec386e559cc6d330b3d78b87dbad6581
Real verification:
EXECUTION_VERIFIED
This nc1 defect does not reopen product verification.
F2-T1:
CLOSED / PASS
F2-T2A Attempt 1:
REWORK
F2-T2A Attempt 2:
EXECUTION_VERIFIED
nc1:
3d86 INVALID_OUTPUT
ONE RETRY AUTHORIZED
nc2:
WAITING
nc3:
WAITING
attempts:
2 / 3
F2-T2B:
WAITING
Smoke-NAV:
PENDING
