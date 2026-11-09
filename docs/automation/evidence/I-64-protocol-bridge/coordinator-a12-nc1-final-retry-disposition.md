I-64 — F2-T2A-NAVIGATION-HOST
nc1 FINAL RETRY DISPOSITION
Verified product:
44a692a4ec386e559cc6d330b3d78b87dbad6581
Real verification:
R20261109T201422Z-89a6
EXECUTION_VERIFIED / NONE
nc1 initial:
R20261109T202056Z-3d86
INVALID_OUTPUT
nc1 retry 1:
R20261109T203840Z-16b6
16b6 is classified as:
SCHEMA_VALID_BUT_ORACLE_INVALID
INVALID_OUTPUT
Reason:
The expected nc1 oracle requires:
Handoff = pass
Identity = fail
FailureClass = Identity
Instead 16b6 produced:
Handoff = fail
Identity = fail
FailureClass = Handoff
The Controller again treated the deliberately false CurrentSha as
contradictory Handoff evidence instead of reserving that repository mismatch
for Identity.
This is the same control-output defect class as 3d86.
It does NOT reopen:

* product;
* Worker;
* real verification;
* Attempt 2.

Preserve 16b6 exactly as emitted.
The initial process reading that temporarily classified two bash.exe
processes as participants is NOT treated as P-02.
Reason:

* they were the session's own measurement command;
* both processes had terminated on re-read;
* the clean second measurement showed 0 participants;
* nc1 was launched only after that clean measurement.

Preserve both readings as evidence.
No protocol defect is attributed to the invocation from this fact.
AUTHORIZE:
ONE SECOND AND FINAL nc1 RETRY
Use:

* same verified inputs;
* same nc1 mutation;
* same verification prompt;
* same Controller cell;
* same model/effort;
* new RunId only.

Do NOT:

* modify the prompt;
* add a Coordinator hint about Handoff vs Identity;
* create analysis.md;
* commit anything;
* move HEAD;
* modify product.

Reason for keeping the prompt unchanged:
the exact prompt has produced a correct nc1 oracle in prior chains, and one
ordinary rerun remains under README section 10.
Required:
Termination = pass
Handoff = pass
Authority = pass
Contract = pass
Identity = fail
FailureClass = Identity
Classification = EXECUTION_BLOCKED
Disposition = STOP
Remote, Scope, CleanTree, Ci, Tests, Trailer, Routing, FreeText and Denials
must remain equal to the accepted real verification, subject only to the
normal relative-oracle rules.
No failure before Identity.
nc1:
initial:
3d86
rerun 1:
16b6
rerun 2:
AUTHORIZED NOW
This is the FINAL ordinary nc1 rerun.
No further ordinary nc1 rerun exists after this invocation.
Product attempts remain:
2 / 3
Control reruns do not consume product attempts.
If the final nc1 retry satisfies the relative oracle:
record:
nc1 = CONTROL PASS
Then proceed to:
nc2
If nc2 closes successfully:
proceed to:
nc3
Each control remains sequential.
Stop immediately on any invalid/oracle-missing output.
If the final nc1 retry again produces:
Handoff = fail
or otherwise misses its relative oracle:
STOP.
Record:
nc1 NORMAL CONTROL PATH EXHAUSTED
P-04 applies.
Do NOT:

* run nc2;
* run nc3;
* alter the prompt;
* run another Controller retry;
* infer a bridge by analogy.

Return to Coordinator with the exact output.
At that point I will decide whether the repeated Handoff/Identity defect
requires an explicit I-64 substitute protocol path or leaves T2A blocked.
A-4:
only for the exact nc2 Scope defect, if reached and reproduced.
A-6:
only for the exact nc3 FreeText defect, if reached and reproduced.
A-11:
consumed and unavailable.
There is currently NO accepted substitute for nc1.
F2-T1:
CLOSED / PASS
F2-T2A Attempt 1:
REWORK
F2-T2A Attempt 2:
EXECUTION_VERIFIED
nc1:
3d86 = INVALID_OUTPUT
16b6 = INVALID_OUTPUT
FINAL RETRY AUTHORIZED
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
