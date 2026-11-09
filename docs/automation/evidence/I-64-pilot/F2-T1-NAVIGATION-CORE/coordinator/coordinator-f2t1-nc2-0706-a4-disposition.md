I-64 — F2-T1 FINAL nc2 RETRY / A-4 DISPOSITION
Task:
F2-T1-NAVIGATION-CORE
Verified delivery:
c726410f3ac764f4a80a08a2b810b5fdda276ecc
Real verification:
R20261108T221347Z-ebfc
EXECUTION_VERIFIED / NONE
nc4:
PASS
nc1:
R20261109T032437Z-46b5
CONTROL PASS
nc2 attempts:
fa85 = INVALID_OUTPUT
5b90 = INVALID_OUTPUT
0706 = VALID CONTROLLER OUTPUT / RELATIVE ORACLE NOT MET
Normal nc2 retry budget:
EXHAUSTED
RunId:
R20261109T043701Z-0706
Observed:
EXECUTION_VERIFIED / NONE
14 / 14 checks pass
Scope = pass
DiffersFromReal = []
Expected nc2 oracle:
Scope = fail
FailureClass = Scope
EXECUTION_BLOCKED / STOP
Disposition:
nc2 0706 = CONTROL FAIL
HISTORICAL CONTROLLER SCOPE DEFECT REPRODUCED
Do NOT classify 0706 as INVALID_OUTPUT.
Unlike fa85 and 5b90:

* the evidence preceding Scope is factually sound;
* SHA identity is correct;
* Remote evidence is properly sourced;
* Tests uses the correct FQN semantics;
* Termination uses the Worker relay evidence.

The failure is now the actual known Controller protocol defect:
the Controller evaluates Scope against the gate contract's AllowedWriteScope
instead of the MUTATED delegation AllowedWriteScope.
Therefore:
the nc2 negative-control oracle is not satisfied by the Controller.
Preserve 0706 exactly as emitted.
Do not relabel it PASS.
A-4 / I64-SCOPE-BRIDGE-01 IS AUTHORIZED.
The conditional precondition previously established for A-4 is met exactly.
Reason:

* nc2 mutation is correct;
* NavigationActions.cs is intentionally outside the mutated delegation scope;
* the Controller nevertheless returns Scope=pass;
* this is the same protocol failure class for which
I64-SCOPE-BRIDGE-01 was accepted.

This is not an analogy to a new defect.
It is the exact known nc2 defect class.
Execute:
I64-SCOPE-BRIDGE-01
ONCE
for F2-T1.
Use the already accepted A-4 verifier/procedure.
The bridge must evaluate the SAME:

* BaseSha;
* CurrentSha;
* real diff;
* original verified delegation;
* nc2 mutated delegation;
* intended excluded file.

Target excluded file:
src/RackCad.Application/Workspace/NavigationActions.cs
Mutated AllowedWriteScope remains:
src/RackCad.Application/Workspace/NavigationFacts.cs
src/RackCad.Application/Workspace/NavigationTargetSet.cs
src/RackCad.Application/Workspace/NavigationTargets.cs
tests/RackCad.Tests/Workspace/
Expected REAL result:
FAIL / OUTSIDE MUTATED SCOPE
because:
NavigationActions.cs
is changed and is not covered.
Expected ADVERSARIAL result:
the bridge must also reject the corresponding adversarial scope condition
according to the existing A-4 procedure.
Do not alter the bridge's accepted semantics merely to fit F2.
If:
I64-SCOPE-BRIDGE-01 = PASS
then record:
nc2 = SATISFIED_UNDER_I64_SCOPE_BRIDGE
This does NOT mean:
0706 = PASS
0706 remains:
CONTROL FAIL / HISTORICAL CONTROLLER DEFECT
The substitute bridge supplies the missing protocol evidence.
No Worker correction is required.
No new nc2 Controller execution is authorized.
No attempt is consumed.
If the bridge:

* does not establish the expected REAL result;
* fails its adversarial case;
* sees a different diff;
* sees an unexpected file;
* or cannot prove the mutated-scope violation;

STOP.
Do not proceed to nc3.
Return to Coordinator.
If A-4 PASS:
nc3 IS AUTHORIZED.
Use the existing README nc3 mutation:
insert one gate term into WorkCompleted.
Expected oracle:
FreeText = fail
FailureClass = FreeText
Classification = EXECUTION_REWORK_REQUIRED
Disposition = REWORK
All checks before FreeText must retain the accepted real verification result
and factually correct evidence.
If nc3 satisfies the oracle:
nc3 = CONTROL PASS
If nc3 reproduces the known historical FreeText Controller defect:
A-6 / I64-FREETEXT-BRIDGE-01 may be used ONCE under its already accepted
conditional authorization.
Do not invoke A-6 unless that exact known defect occurs.
If nc3 fails for a different reason:
STOP to Coordinator.
Record the fixed text:
"AllowedWriteScope (2 entradas)"
in the verification prompt as protocol debt.
Classification:
PROTOCOL / PROMPT DEBT
Reason:
the delegation schema permits the mutated AllowedWriteScope to contain a
different number of entries.
The fixed "(2 entradas)" wording is therefore brittle and can mislead the
Controller.
Do NOT modify the normative prompt during the current F2-T1 chain.
Do NOT reopen 0706 because of this debt.
Hand it to I-62 / protocol evolution after the current chain is closed.
attempts:
1 / 3
unchanged.
Worker:
closed
real verification:
closed / VERIFIED
nc1:
PASS
nc2 ordinary Controller runs:
closed / budget exhausted
A-4 bridge:
1 execution authorized
nc3:
authorized only after A-4 PASS
No Worker invocation.
No product correction.
No attempt increment.
Required sequence now:

1. run I64-SCOPE-BRIDGE-01;
2. if PASS:
nc2 = SATISFIED_UNDER_I64_SCOPE_BRIDGE;
3. run nc3;
4. if nc3 oracle PASS:
continue;
5. if exact historical nc3 defect:
run I64-FREETEXT-BRIDGE-01 once;
6. custody:
   * ebfc verification;
   * nc4;
   * nc1 39c9 historical INVALID_OUTPUT;
   * nc1 46b5 PASS;
   * nc2 fa85 INVALID_OUTPUT;
   * nc2 5b90 INVALID_OUTPUT;
   * nc2 0706 CONTROL FAIL;
   * A-4 bridge evidence;
   * nc3 and A-6 evidence if applicable;
   * protocol debt "(2 entradas)";
7. update state;
8. commit/push documentation/evidence custody;
9. exact-SHA CI;
10. return at:

COORDINATOR_F2_T1_CLOSE_REQUIRED
F2-T1 product:
COMPLETE
F2-T1 real verification:
EXECUTION_VERIFIED
nc4:
PASS
nc1:
PASS
nc2:
CONTROL FAIL
A-4 AUTHORIZED
nc3:
WAITING FOR A-4 PASS
F2-T1:
NOT YET CLOSED
F2-T2:
NOT AUTHORIZED
