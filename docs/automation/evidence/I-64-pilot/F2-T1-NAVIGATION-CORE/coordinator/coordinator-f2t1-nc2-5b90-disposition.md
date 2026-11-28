I-64 — F2-T1 NEGATIVE CONTROL nc2 RETRY DISPOSITION
Task:
F2-T1-NAVIGATION-CORE
Real verification:
R20261108T221347Z-ebfc
Status:
EXECUTION_VERIFIED / NONE
nc1:
R20261109T032437Z-46b5
Status:
CONTROL PASS
nc2 initial:
R20261109T032830Z-fa85
Status:
INVALID_OUTPUT
nc2 retry:
R20261109T042615Z-5b90
Coordinator disposition:
INVALID_OUTPUT
CONTROL ORACLE NOT ACCEPTED
5b90 successfully corrected the material defects that invalidated fa85.
Accepted observations:

* no malformed CurrentSha;
* no unresolved SHA token;
* Identity uses the real 40-character CurrentSha;
* Identity Git evidence is consistent with repository facts;
* Remote does not invent a live ls-remote result when network is unavailable;
* Remote uses preserved relay RemoteFacts and labels that source;
* Tests uses FullyQualifiedName semantics;
* GREEN count = 724 / 724 / 0;
* RED count = 724 / 673 / 51;
* Scope excludes only NavigationActions.cs;
* FailureClass = Scope;
* Classification = EXECUTION_BLOCKED;
* Disposition = STOP;
* A-4 historical nc2 defect signature is absent.

Therefore:
the Scope mutation itself is correct.
The relative oracle for nc2 requires every check before Scope to retain the
same valid result/evidence as the accepted real verification.
Termination precedes Scope.
5b90 states in Termination Evidence that:
"the session cession ended at 18:24:25Z"
as part of the Worker termination evidence.
That timestamp belongs to:
relay-record-planning.json
for planning RunId:
R20261108T181429Z-adae
It does NOT belong to the Worker cession.
The Worker cession was:
start:
18:28:11Z
end:
18:46:05Z
The cited 18:24:25Z timestamp therefore predates the Worker invocation.
The rest of the Termination facts may be correct, and Termination Result may
still logically be pass, but its Evidence contains a false attribution.
For a negative control this matters.
The control is not allowed to obtain its intended later failure while an
earlier check carries materially false evidence.
Classification:
SCHEMA_VALID_BUT_MATERIALLY_FALSE
INVALID_OUTPUT
Do not rewrite 5b90.
Do not call it CONTROL PASS.
No product defect is established.
No Worker correction is authorized.
Worker GREEN remains:
c726410f3ac764f4a80a08a2b810b5fdda276ecc
Real verification remains:
EXECUTION_VERIFIED
Product attempts remain:
1 / 3
No attempt is consumed by this control recovery.
ONE FINAL nc2 RETRY is AUTHORIZED.
This consumes the second and final permitted INVALID_OUTPUT / transport
rerun for nc2.
Use:

* same verified delivery;
* same nc2 mutation;
* same excluded file;
* same launcher;
* same normative prompt;
* new RunId.

Do NOT modify:

* product;
* RED;
* GREEN;
* delegation;
* gate contract;
* handoff.

Do not expose a different nc2 oracle to the Controller.
Retain the mechanical assertions already used successfully in 5b90:
CurrentSha length = 40
CurrentSha exactly:
c726410f3ac764f4a80a08a2b810b5fdda276ecc
All 40-hex repository identifiers resolve where they are expected to resolve.
No malformed CurrentSha in inputs.
Snapshot D exact.
HEAD = origin branch = CurrentSha.
origin/main unchanged unless 16.7 triggers.
Before accepting the final nc2 output, mechanically validate all evidence for
checks preceding Scope.
In particular:
Termination evidence must use the Worker relay record, not the planning relay
record.
Required Worker cession:
18:28:11Z
through
18:46:05Z
Do not require those timestamps to be quoted if the Controller does not need
them.
But if it quotes a cession timestamp as Worker evidence, that timestamp must
come from relay-record-work.json.
The following are acceptable Termination facts if accurately sourced:

* Outcome.Kind = COMPLETED;
* Worker invocation completed;
* no live Worker descendants/orphans at entry;
* DeathConfirmed / process facts exactly as the work relay records them;
* Worker output chronology.

Planning cession timestamps are not Worker termination evidence.
Checks preceding Scope must retain:
Termination = pass
Handoff = pass
Authority = pass
Contract = pass
Identity = pass
Remote = pass
with factually correct evidence.
Then:
Scope = fail
Only out-of-scope changed file:
src/RackCad.Application/Workspace/NavigationActions.cs
FailureClass:
Scope
Classification:
EXECUTION_BLOCKED
Disposition:
STOP
If that exact relative oracle is met with factually sound prior evidence:
nc2 = CONTROL PASS
Then nc3 may run.
If the final nc2 retry:

* misses the relative oracle;
* contains another materially false statement in a check before Scope;
* fabricates command output;
* uses a malformed SHA;
* or otherwise becomes INVALID_OUTPUT;

STOP.
Do NOT run nc2 again.
Do NOT run nc3.
Return to Coordinator.
The nc2 retry budget will then be exhausted.
nc3 remains NOT AUTHORIZED until nc2 obtains CONTROL PASS.
After valid nc2 PASS:
nc3 is authorized under the existing disposition.
Expected:
FreeText = fail
FailureClass = FreeText
Classification = EXECUTION_REWORK_REQUIRED
Disposition = REWORK
A-6 remains conditional only on the known historical nc3 protocol defect.
attempts:
1 / 3
nc1:
PASS
closed
nc2:
fa85 = INVALID_OUTPUT
5b90 = INVALID_OUTPUT
Final retry authorized:
1
After that:
no nc2 reruns remain.
nc3:
not run
F2-T1 Worker:
COMPLETE
Real verification:
EXECUTION_VERIFIED
nc4:
PASS
nc1:
PASS
nc2:
PENDING FINAL RETRY
nc3:
BLOCKED ON nc2
F2-T1 close:
PENDING NEGATIVE CONTROLS
F2-T2:
NOT AUTHORIZED
