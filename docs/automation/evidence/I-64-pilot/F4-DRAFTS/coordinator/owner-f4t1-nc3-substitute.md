I-64 — F4-T1-DRAFT-CORE
nc3 CONTROL SUBSTITUTE DISPOSITION
Current branch:
61d83788e7fa8decd861626b7e3ca89c8f23b9b4
Verified product:
ce73ceb7
Reverification:
1f4f
EXECUTION_VERIFIED / NONE
14 / 14 PASS
nc1:
bc22
CONTROL PASS
nc2:
SATISFIED_UNDER_F4T1_SINGLE_PATH_SCOPE_SUBSTITUTE
nc3:
2f8e
CONTROL NOT PASSED
OPTION 1 AUTHORIZED.
Do NOT execute nc3 again.
Create an I-64 / F4-T1-only deterministic substitute:
F4T1-NC3-FREETEXT-SUBSTITUTE-01
Purpose:
prove the intended nc3 FreeText property while preserving 2f8e exactly as
historical evidence.
2f8e already demonstrates the substantive nc3 condition:

* every check except FreeText matches the real verification;
* FreeText = fail;
* the injected prohibited term is detected.

The expected control oracle differs only in final classification:
expected:
EXECUTION_REWORK_REQUIRED / REWORK
observed:
EXECUTION_BLOCKED / STOP
The escalation is caused by two control-construction inputs:

1. reissue-record.json states that the reissued real handoff contains no
§16.10 terms;
2. the inherited reverification authority states that if the reverification
fails any check, STOP.

Both statements are true for the REAL reverification.
They become contradictory only because nc3 deliberately mutates FreeText.
Therefore they are control-harness constraints, not evidence that FreeText
detection failed.
Preserve:
2f8e = CONTROL NOT PASSED
Do NOT rewrite it as:
CONTROL PASS
Do NOT alter its Controller output.
Do NOT remove the S-04 history.
The substitute satisfies nc3 separately.
The deterministic substitute must establish ALL of the following:

1. real verification 1f4f is:
EXECUTION_VERIFIED / NONE
14 / 14 PASS;
2. nc3 uses the accepted real verification inputs plus the intended nc3
FreeText mutation and mechanically derived hash changes;
3. the intended prohibited §16.10 term is present in the mutated free-text
field;
4. the real handoff does NOT contain that term;
5. FreeText in 2f8e = fail;
6. every check BEFORE FreeText has the same Result as 1f4f;
7. every check AFTER FreeText has the same Result as 1f4f;
8. no product SHA changed;
9. no RED/GREEN/test/CI fact changed;
10. the only semantic handoff mutation is the intended nc3 FreeText mutation;
11. all hash changes are mechanical consequences of the mutated handoff;
12. reissue-record.json differs from the nc3 occurrence only because it
describes the REAL reissued handoff, not a negative-control mutation;
13. the inherited "if this reverification fails any check: STOP" clause
belongs to real reverification recovery semantics and is not part of the
intended nc3 FreeText oracle;
14. without those two auxiliary contradictions, the result vector corresponds
exactly to:
FreeText = fail
FailureClass = FreeText
EXECUTION_REWORK_REQUIRED / REWORK.

Prove deterministically that the nc3 mutated handoff differs from the real
reissued handoff only in:

* the intended FreeText field;
* mechanically derived hashes / integrity fields required by that byte
change.

No second semantic mutation is allowed.
If any other semantic field changes:
SUBSTITUTE FAIL.
Do NOT modify reissue-record.json for history.
The substitute must instead distinguish:
REAL HANDOFF CLAIM
from:
NEGATIVE CONTROL MUTATION.
The statement:
"real reissued handoff contains no §16.10 term"
remains TRUE.
nc3 intentionally creates a synthetic handoff that violates that property.
That contradiction must be identified as expected control construction, not
silently erased.
Do NOT rewrite the prior Coordinator authority.
The substitute must show that the STOP clause was written for failure of the
REAL second reverification.
1f4f did not fail.
nc3 is a later negative control designed to fail FreeText deliberately.
Therefore applying that reverification STOP clause to the negative control
changes the control's final classification without changing the signal under
test.
Record this distinction explicitly.
Before applying the substitute to 2f8e, create deterministic synthetic tests
at minimum:
A.
only intended FreeText mutation, correct vector
-> PASS
B.
FreeText does not fail
-> FAIL
C.
another earlier check differs from real verification
-> FAIL
D.
another later check differs
-> FAIL
E.
second semantic handoff field changed
-> FAIL
F.
product SHA changed
-> FAIL
G.
real handoff already contains prohibited term
-> FAIL
H.
hash mutation does not match handoff bytes
-> FAIL
I.
reissue-record contradiction absent but result still STOP for another reason
-> FAIL
J.
FailureClass is not FreeText
-> FAIL
K.
FreeText term is not one of §16.10
-> FAIL
L.
real verification is not 14 / 14 VERIFIED
-> FAIL
Synthetic suite must be deterministic.
After self-tests pass:
apply the substitute once to:
2f8e
If all required facts pass, record:
2f8e = CONTROL NOT PASSED
and separately:
nc3 =
SATISFIED_UNDER_F4T1_FREETEXT_SUBSTITUTE
Do NOT call 2f8e a normal CONTROL PASS.
Do NOT:

* rerun nc3;
* remove reissue-record.json;
* remove Coordinator authority;
* alter the real verification input set;
* sanitize 2f8e;
* create another handoff;
* create Attempt 4.

The purpose of this substitute is to preserve all evidence while isolating
the actual property under test.
If the substitute passes:
F4-T1 = CLOSED / PASS
Final control state:
nc1:
CONTROL PASS
nc2:
SATISFIED_UNDER_F4T1_SINGLE_PATH_SCOPE_SUBSTITUTE
nc3:
SATISFIED_UNDER_F4T1_FREETEXT_SUBSTITUTE
Preserve all ordinary-control outputs exactly.
After nc3 substitute PASS:
custody at minimum:

* Snapshot G probe/acceptance evidence;
* handoff reissue;
* ce94;
* 4bb6;
* 1f4f;
* bc22;
* 50da;
* nc2 substitute and self-tests;
* 2f8e;
* nc3 substitute and self-tests;
* post-custody Identity authorities;
* narrow §16.8 Tests exception;
* final F4-T1 close;
* all relevant Coordinator dispositions.

Then push and obtain exact-SHA CI on the custody tip.
After F4-T1 closure:
start:
F4-T2 — DRAFT READ / ADMISSION HOST
attempts:
1 / 3
Mandatory inherited requirements:

1. N-2 anti-aliasing;
2. host-side state cannot mutate InitialDraftState after draft creation;
3. adversarial source-object mutation test;
4. one logical read produces:
StaleBase
admission
InitialDraftState;
5. RackSiblingMembership admission;
6. no DWG write;
7. IC-04 remains binding:
event handlers enqueue only;
8. F4-PILOT-PUSHBACK-01 remains the bounded pilot;
9. exact pilot field set must be contracted before Worker implementation.

F4-T1 product:
VERIFIED
Product:
ce73ceb7
Reverification:
1f4f = VERIFIED
nc1:
PASS
nc2:
SATISFIED UNDER SUBSTITUTE
nc3:
SUBSTITUTE AUTHORIZED
F4-T1:
PENDING SUBSTITUTE + CUSTODY
F4-T2:
WAITING
attempts 1 / 3
