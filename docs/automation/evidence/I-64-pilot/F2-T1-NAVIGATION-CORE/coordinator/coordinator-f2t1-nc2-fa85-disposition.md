I-64 — F2-T1 NEGATIVE CONTROL nc2 DISPOSITION
Task:
F2-T1-NAVIGATION-CORE
Real verification:
R20261108T221347Z-ebfc
Status:
EXECUTION_VERIFIED / NONE
nc1:
R20261109T032437Z-46b5
Disposition:
CONTROL PASS
nc2:
R20261109T032830Z-fa85
Coordinator disposition:
INVALID_OUTPUT
CONTROL ORACLE NOT ACCEPTED
nc1 46b5 is ACCEPTED.
Expected relative oracle:
Termination = pass
Handoff = pass
Authority = pass
Contract = pass
Identity = fail
FailureClass = Identity
Classification = EXECUTION_BLOCKED
Disposition = STOP
Observed result satisfies the oracle.
The later Ci failure is accepted as a consequence of the synthetic
nonexistent CurrentSha.
Therefore:
nc1 = PASS
No further nc1 execution is authorized.
The nc2 mutation itself is ACCEPTED as correctly constructed.
The mutated AllowedWriteScope excludes exactly:
src/RackCad.Application/Workspace/NavigationActions.cs
while preserving coverage for:
src/RackCad.Application/Workspace/NavigationFacts.cs
src/RackCad.Application/Workspace/NavigationTargetSet.cs
src/RackCad.Application/Workspace/NavigationTargets.cs
tests/RackCad.Tests/Workspace/
No unexpected additional changed file under the removed prefix was found.
The mechanical Scope condition therefore behaves as intended:
Scope = fail
with only:
src/RackCad.Application/Workspace/NavigationActions.cs
outside the mutated scope.
The known historical nc2 protocol defect is NOT the issue here.
A-4 is NOT applicable to fa85.
Although the final Scope conclusion is correct, fa85 contains materially
false evidence in checks that precede Scope.
The Controller transcribed the real CurrentSha:
c726410f3ac764f4a80a08a2b810b5fdda276ecc
as the malformed value:
c726410f3ac764f4a80a08b5fdda276ecc
The malformed value:

* has 34 characters;
* does not occur in the inputs;
* was used in Controller Git commands;
* caused Git failures;
* was nevertheless reported in Identity/Remote Evidence as though Git had
returned it successfully.

Examples preserved by the session include claims equivalent to:

* rev-parse HEAD returned the malformed SHA;
* rev-parse origin branch returned the malformed SHA;
* relay LsRemoteSha equals the malformed CurrentSha.

Those claims are factually false.
A negative control is valid only if:

1. the deliberately mutated check fails as intended; and
2. every earlier check retains the Result and evidentiary validity of the
accepted real verification.

Here:
Scope is the intended mutation.
Identity and Remote precede Scope.
Their Results happen to be true for the actual repository, but their
Evidence contains fabricated/incorrect command results.
Therefore the relative oracle is not trustworthy.
Classification:
SCHEMA_VALID_BUT_MATERIALLY_FALSE
INVALID_OUTPUT
Do not rewrite fa85.
Do not call it CONTROL PASS.
The repeated display-name counting in fa85 is also incorrect.
The accepted semantics remain:
FullyQualifiedName~Workspace
case-insensitive
over fully qualified class/method identity.
Reference counts remain:
GREEN:
724 / 724 / 0
RED:
724 / 673 / 51
The display-name count of 727 / 676 is not authoritative.
This defect alone would already require correction of the evidence method,
but the earlier Identity/Remote evidence defect is sufficient to invalidate
fa85.
ONE nc2 retry is AUTHORIZED.
This is a CONTROL-phase INVALID_OUTPUT recovery.
It:

* does not consume attempts;
* does not reopen the Worker;
* does not change ebfc;
* does not change the delegation;
* does not change RED or GREEN;
* does not invoke A-4.

Use:
same verified delivery
same delegation mutation
same excluded file
same launcher
new RunId
The mutation must remain byte-equivalent in meaning to fa85.
Expected relative oracle:
Termination = same Result as ebfc
Handoff = same Result as ebfc
Authority = same Result as ebfc
Contract = same Result as ebfc
Identity = same Result as ebfc
Remote = same Result as ebfc
Scope = fail
FailureClass = Scope
Classification = EXECUTION_BLOCKED
Disposition = STOP
All checks preceding Scope must contain factually correct evidence.
For the nc2 retry:
do not manually transcribe CurrentSha into shell commands.
Use the CurrentSha directly from the verified delegation/relay input or an
exact variable populated from that source.
Before invocation mechanically assert:
length(CurrentSha) = 40
and:
CurrentSha =
c726410f3ac764f4a80a08a2b810b5fdda276ecc
Any generated Git command containing another value:
STOP BEFORE INVOCATION.
The Controller must not infer, shorten or reconstruct SHA text.
If live git ls-remote cannot reach GitHub during the control:
do not fabricate a value.
Use only the already captured RemoteFacts from the accepted ebfc relay where
the protocol allows that evidence source, and label it as relay evidence.
If the check cannot be established from an allowed source:
BLOCKED
is preferable to invented evidence.
Continue using:
-c allow_login_shell=false
for the nc2 retry and, if nc2 passes, nc3.
This remains the accepted per-invocation tooling control.
No persistent profile or config mutation is authorized.
nc3 is NOT authorized yet.
Only after:
nc2 retry = CONTROL PASS
may the session launch nc3.
Expected nc3 oracle remains:
FreeText = fail
FailureClass = FreeText
Classification = EXECUTION_REWORK_REQUIRED
Disposition = REWORK
All prior checks must remain equal to ebfc.
A-4:
NOT APPLICABLE to fa85.
The historical nc2 Scope-defect signature did not occur.
A-6 remains conditional only on the previously defined nc3 FreeText protocol
defect.
Do not invoke either bridge now.
attempts:
1 / 3
unchanged.
nc1:
valid oracle output = 1 / 1
closed
nc2:
fa85 = INVALID_OUTPUT
valid oracle outputs = 0 / 1
Authorized:
one nc2 retry
If that retry again contains false evidence or misses the oracle:
STOP to Coordinator.
Do not automatically spend another retry.
nc3:
not launched
Worker GREEN:
c726410f3ac764f4a80a08a2b810b5fdda276ecc
Real verification:
EXECUTION_VERIFIED
nc4:
PASS
nc1:
PASS
nc2:
PENDING VALID RETRY
nc3:
NOT RUN
F2-T1 close:
PENDING NEGATIVE CONTROLS
F2-T2:
NOT AUTHORIZED
Run exactly one nc2 retry.
If:
nc2 retry = CONTROL PASS
then proceed to nc3.
If:
nc2 retry != CONTROL PASS
STOP immediately and return to Coordinator.
Do not launch nc3 after a failed nc2 oracle.
