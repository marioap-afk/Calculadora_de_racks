I-64 — F2-T1 nc3 FINAL RERUN DISPOSITION / A-8 ORDER
Task:
F2-T1-NAVIGATION-CORE
Verified product:
c726410f3ac764f4a80a08a2b810b5fdda276ecc
Current branch:
7002678f46b3816aa06df4cb77a945cbf250af54
Current branch exact-SHA CI:
42568842371
4 / 4 SUCCESS
Real verification:
R20261108T221347Z-ebfc
EXECUTION_VERIFIED / NONE
RunId:
R20261109T054710Z-53af
Observed:
EXECUTION_BLOCKED / STOP
FailureClass = Tests
TriggeredStopConditions = [S-04]
FreeText = fail
Coordinator classification:
SCHEMA_VALID_BUT_MATERIALLY_FALSE
INVALID_OUTPUT
The reported Tests contradiction is not accepted as real contradictory
evidence.
Reason:
The Controller reconstructed FullyQualifiedName~Workspace using class name
only.
That is not the contract filter.
The correct semantics are:
case-insensitive substring match over the fully qualified
class + method identity.
Using that semantics on the same TRX:
GREEN:
724 selected
724 passed
0 failed
RED:
724 selected
673 passed
51 failed
Therefore:
S-04 = NOT ESTABLISHED BY TEST DATA
However:
53af is NOT converted to CONTROL PASS.
The Controller output itself did not satisfy the required nc3 relative
oracle.
Preserve 53af exactly as emitted.
The following parts are accepted as useful evidence:

* post-custody Identity mode worked;
* current HEAD and verified CurrentSha were reported separately;
* Remote reported the current custody branch tip;
* the six ChainRedFiles were present;
* FreeText detected the injected prohibited term;
* Controller output hygiene worked;
* manual 16.10 scan of Controller-generated free text was empty;
* all checks other than Tests and FreeText passed.

The FreeText behavior itself is therefore no longer the unresolved issue.
nc3 ordinary Controller path:
46c1 = initial execution
041d = rerun 1
53af = rerun 2
Budget:
EXHAUSTED
P-04 applies to another ordinary nc3 Controller invocation.
NO FOURTH CONTROLLER RUN IS AUTHORIZED.
Do not reset the budget.
Do not rename the control.
Do not invoke another model to repeat nc3.
A-6 / I64-FREETEXT-BRIDGE-01 is NOT the correct substitute.
Reason:
the known FreeText Controller defect did not reproduce.
All three nc3 executions detected the injected FreeText violation.
The defect in 53af is instead:
incorrect reconstruction of RequiredTests selection semantics.
Do not extend A-6 by analogy.
Open:
I-64 Amendment A-8
proposed bridge name:
I64-TESTFILTER-BRIDGE-01
Purpose:
provide deterministic substitute evidence for the Tests part of nc3 when
the Controller cannot faithfully reproduce the RequiredTests filter after
its ordinary control budget is exhausted.
A-8 is protocol evidence only.
It does NOT alter:

* product behavior;
* RED or GREEN;
* Freeze product semantics;
* RequiredTests;
* the nc3 FreeText mutation;
* AUTOMATION_PLAN globally;
* A-4;
* A-6.

Materiality:
M-01 = NO
M-02 = NO
M-03 = NO
M-04 = YES
M-05 = NO
M-06 = NO
M-07 = NO
M-08 = NO
Reason for M-04:
A-8 changes the substitute evidence path used after a Controller protocol
failure.
Authority required:
Owner + Architect + Coordinator
Status on creation:
PROPOSED / NOT ACTIVE
No bridge execution against the real F2-T1 evidence until:
Owner = SATISFIED
Architect = AGREED
Coordinator = AGREED
Do not infer Owner authorization from the old A-4 emergency mandate or the
A-5/A-6 FreeText decision.
Those authorities were narrower.
A-8 applies only to:
I-64
and specifically to a Controller Tests-filter failure where:

1. the real delivery is already EXECUTION_VERIFIED;
2. the negative control mutation itself is independently established;
3. ordinary negative-control reruns are exhausted;
4. Controller Tests evidence conflicts only because it reconstructed the
RequiredTests filter incorrectly;
5. deterministic source data are available.

It is NOT a general replacement for Controller verification.
For this occurrence the bridge must bind exactly to:
TaskId:
F2-T1-NAVIGATION-CORE
Real verification:
R20261108T221347Z-ebfc
Verified product:
c726410f3ac764f4a80a08a2b810b5fdda276ecc
RED:
f2761979b89e784ed16220815e250d21bc476aa1
nc3 mutated handoff:
the exact mutation used by 46c1 / 041d / 53af
RequiredTests:
Project:
tests/RackCad.Tests/RackCad.Tests.csproj
Filter:
FullyQualifiedName~Workspace
MinSelected:
645
ExpectRed:
true
Accepted ChainRedFiles:
the six test files already established by ebfc.
The deterministic verifier must reproduce VSTest substring semantics for:
FullyQualifiedName~Workspace
Required matching key:
fully qualified class name + "." + method name
Matching:
case-insensitive substring
It must NOT use:

* display name;
* Theory argument text;
* class name alone;
* method name alone unless included through the FQN;
* ad-hoc "contains Workspace" against arbitrary TRX strings.

Expected F2-T1 values:
GREEN:
Selected = 724
Passed = 724
Failed = 0
RED:
Selected = 724
Passed = 673
Failed = 51
MinSelected:
724 >= 645
Expected RED composition recorded separately:
50 xUnit assertion failures
1 NullReferenceException
No compilation failure.
The bridge must independently derive:
git diff --name-only ChainBaseSha..RedSha -- tests/
and prove the same six ChainRedFiles already accredited by ebfc.
It must also prove:
git diff --name-only RedSha..CurrentSha
does not modify those test files.
No Controller prose is authoritative for this calculation.
A-8 is NOT a FreeText substitute, but before it may close nc3 it must prove
that the nc3 mutation itself is valid.
Deterministically compare:
real worker handoff
vs
nc3 mutated worker handoff
and prove:

* schema valid on both;
* only the authorized WorkCompleted mutation exists;
* one prohibited 16.10 term was introduced by that mutation;
* no compensating second change exists.

The actual restricted token may be identified inside the verifier output
artifact, but Coordinator-facing narrative should use a neutral label.
This may reuse the exact parsing/term semantics already accepted by A-6,
but A-8 remains a distinct amendment and substitute path.
Do not classify this as execution of A-6.
Before activation, the bridge must have deterministic synthetic self-tests.
Minimum adversarial cases:
T01:
correct FQN selection -> PASS
T02:
display-name counting -> FAIL
T03:
class-name-only counting -> FAIL
T04:
case-sensitive matching -> FAIL
T05:
method-only accidental inclusion/exclusion -> FAIL
T06:
Theory argument contains Workspace but FQN does not -> must not select
T07:
Workspace appears only in method name -> must select
T08:
Selected below MinSelected -> FAIL
T09:
GREEN has any failed selected test -> FAIL
T10:
RED lacks expected failing selected tests -> FAIL
T11:
ChainRedFiles omitted -> FAIL
T12:
GREEN modifies ChainRedFiles -> FAIL
T13:
nc3 handoff has no prohibited term mutation -> FAIL
T14:
nc3 handoff changes an additional field -> FAIL
T15:
malformed TRX / unreadable input -> FAIL CLOSED
At least one synthetic fixture must reproduce the exact failure class seen
in 53af:
class-only selection returns one fewer test than FQN selection.
The session may now:

1. draft I-64-A-8.md;
2. implement the bridge verifier and synthetic self-tests;
3. use ONLY synthetic fixtures while A-8 is not active;
4. version the proposal, tools and synthetic results;
5. obtain formal Architect review of the exact A-8 object.

The session must NOT run the new bridge against the real F2-T1 TRX or
handoffs before activation.
A-8 requires an explicit Owner decision.
The exact Owner choice to request is:
"SATISFIED — authorize I-64-A-8 / I64-TESTFILTER-BRIDGE-01 as an
I-64-only substitute evidence path for the exhausted nc3 Tests-filter
protocol defect, without changing product semantics or global protocol."
If Owner does not accept:
F2-T1 remains BLOCKED_PROTOCOL_DEPENDENCY.
Do not bypass P-04.
After the A-8 proposal and synthetic verifier package exist:
run one formal read-only Architect review of the exact proposal/tool package.
Architect must specifically review:

* M-04;
* FQN semantics;
* fail-closed parsing;
* ChainRedFiles derivation;
* mutation exactness;
* no global protocol authority;
* no use of real F2-T1 evidence before activation.

If Architect = CHANGES REQUIRED:
A-8 remains inactive.
If Architect = AGREED:
return to Coordinator for activation.
Only after:
Owner SATISFIED
Architect AGREED
Coordinator AGREED
may the bridge run ONCE against real F2-T1 evidence.
If real bridge PASS:
record:
nc3 = SATISFIED_UNDER_I64_TESTFILTER_BRIDGE
46c1 remains historical INVALID_OUTPUT
041d remains historical oracle-not-met
53af remains historical INVALID_OUTPUT
P-04 history remains preserved
Then F2-T1 may proceed to final custody and close review.
If bridge FAIL:
STOP.
No more Controller nc3 runs.
F2-T1 product:
COMPLETE
Real verification:
EXECUTION_VERIFIED
nc4:
PASS
nc1:
PASS
nc2:
SATISFIED_UNDER_I64_SCOPE_BRIDGE
nc3:
NORMAL PATH EXHAUSTED
PENDING A-8 PROTOCOL SUBSTITUTE
A-8:
PROPOSED BY COORDINATOR
NOT ACTIVE
attempts:
1 / 3
F2-T1:
BLOCKED_PROTOCOL_DEPENDENCY
F2-T2:
NOT AUTHORIZED
