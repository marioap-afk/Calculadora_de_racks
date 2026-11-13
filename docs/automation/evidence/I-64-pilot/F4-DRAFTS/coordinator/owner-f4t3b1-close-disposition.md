I-64 — F4-T3B1 / F4-T3B2
OWNER + COORDINATOR DISPOSITION
Current branch:
1a9445bdc08019f64acc0e256906e9d327310c6a
Current main:
812baf879612320653094a8aaf46aca0bac81383
Verified product:
1708e764
Attempt:
2 / 3
Controller verification:
EXECUTION_VERIFIED
14 / 14 PASS
Exact-SHA CI:
4 / 4 PASS
Independent re-review:
ACCEPTABLE WITH MINORS
0 blockers
0 important findings
The keyboard blocker from Attempt 1 is CLOSED.
nc2 remains:
SATISFIED UNDER THE ALREADY PREPARED DETERMINISTIC SCOPE SUBSTITUTE
Preserve the ordinary Controller output.
Do not reinterpret it as a normal CONTROL PASS.
Authorize proposal (a).
Create and apply:
F4T3B1-NC1-IDENTITY-SIGNAL-01
This is a NEW task-local substitute.
Do NOT claim it is the same signature as the previously accepted nc1
substitutes.
Reason:
this occurrence is different:

* mutated CurrentSha is false;
* Controller rejects the mutated delivery;
* but Identity itself is reported PASS.

The existing substitute correctly rejected this mode.
Therefore this decision explicitly authorizes a stronger independent
identity signal for this occurrence.
The substitute must prove at minimum:

1. the real verification is 14 / 14 VERIFIED;
2. the nc1 mutation changes CurrentSha to the bound nonexistent SHA;
3. no second semantic mutation exists except mechanically derived integrity
fields;
4. mutated CurrentSha != real verified CurrentSha;
5. mutated CurrentSha != HEAD;
6. mutated CurrentSha != remote product branch;
7. mutated CurrentSha does not exist in the repository object database;
8. BaseSha / RedSha / real CurrentSha relationships remain valid;
9. the Controller did NOT verify the mutated occurrence;
10. the Controller output contains evidence referencing the false CurrentSha
and the real repository identity;
11. the mutated occurrence ends BLOCKED / STOP or otherwise non-VERIFIED;
12. the deterministic identity predicate returns FAIL for the mutated SHA.

The substitute must FAIL if:

* false SHA exists;
* false SHA equals HEAD;
* second semantic mutation exists;
* Controller actually VERIFIED the mutated delivery;
* repository identity cannot be established.

If the prepared synthetic suite remains deterministic and the real
application passes:
preserve ordinary nc1 as:
CONTROL NOT PASSED
and record separately:
nc1 =
SATISFIED_UNDER_F4T3B1_IDENTITY_SIGNAL
Record:
PD-F4-05
Controller may report Identity PASS for a deliberately false CurrentSha
while still rejecting the delivery elsewhere.
Do not change AUTOMATION_PLAN globally.
Authorize proposal (b).
Apply the prepared deterministic FreeText detector:
F4T3B1-NC3-FREETEXT-DETECTOR-01
The substitute must prove:

* real handoff is clean;
* nc3 mutation injects the exact prohibited §16.10 term;
* no second semantic mutation exists;
* deterministic scanner reports VIOLATION;
* Controller's FreeText signal also detects the violation where observed;
* unrelated Ci/Trailer variation is not caused by the nc3 mutation.

Preserve the ordinary nc3 output as:
CONTROL NOT PASSED
Do not call it normal CONTROL PASS.
Record:
PD-F4-06
Controller negative controls may vary unrelated Ci / Trailer results relative
to the same real verification.
This variation must NOT be used to weaken Ci or Trailer in real product
verification.
It is protocol debt only.
If both prepared substitutes PASS on the real occurrences:
F4-T3B1 = CLOSED / PASS WITH LIMITATIONS
Final control state:
nc1:
SATISFIED_UNDER_F4T3B1_IDENTITY_SIGNAL
nc2:
SATISFIED_UNDER_DETERMINISTIC_SCOPE_SUBSTITUTE
nc3:
SATISFIED_UNDER_F4T3B1_FREETEXT_DETECTOR
Preserve every ordinary Controller result exactly.
Approve proposal (c).
Carry:
N-1 -> F4-T3B2
N-3 -> F4-T3B2
N-2:
DECLARED LIMITATION
N-2 is the Space-key behavior identified by the independent review.
Do not spend Attempt 3 solely on these minors.
The task closes on Attempt 2 if the control substitutes pass.
The prepared T3B2 package v3 may proceed after T3B1 closure.
Before emission:
incorporate this disposition and explicitly bind N-1 / N-3.
No further Owner decision is required merely to emit the package.
F4-T3B2 starts with:
attempts = 1 / 3
The prepared scope is accepted:

* inline close of dirty/pending tabs:
Discard / Cancel;
* clean tab closes without prompting;
* discard action for Error-state field;
* document-level draft discard with TOCTOU recheck;
* close-veto notice;
* draft-loss notice;
* retain active tab when selection changes while unapplied text exists;
* inherited T3B1 minors where owned by T3B2.

Accept for contract treatment:

1. a tab without a draft may close without calling DiscardRack.

This is acceptable if there is no in-memory draft/pending state to discard
and there is zero authored write.

2. close-veto notice currently names only the panel path.

T3C MUST add the command path once RACKPANELDESCARTAR exists.

3. GetRacksAtRisk failure can currently be indistinguishable from
"nothing at risk".

This is NOT accepted as final behavior.
T3C must harden it so failure/unknown cannot masquerade as a clean result.
Proceed:

1. apply nc1 substitute to real occurrence;
2. apply nc3 substitute to real occurrence;
3. if both PASS, record T3B1 close;
4. custody closure and substitutes;
5. update package v3 with this disposition;
6. run final deterministic package validation;
7. emit T3B2 contract;
8. planning / A1-A8 / nc4;
9. Worker;
10. CI;
11. direct review;
12. independent review;
13. Controller verification;
14. nc1-nc3;
15. custody / close.

Keep the established F4 rule:
one reinforced replan is allowed if the first planning output fails only
literal collection fidelity.
No product attempt consumed.
A second planning fidelity failure:
STOP to Coordinator.
After T3B2 closes:
prepare F4-T3C.
T3C owns at minimum:

* RACKPANELDESCARTAR command exposure;
* notice text including command path;
* hardening of GetRacksAtRisk failure/unknown;
* remaining T3 modal/focus integration;
* carried minor limitations assigned to T3C.

Do not start F4-T4 before T3 is closed.
F4-T3A:
CLOSED / PASS WITH LIMITATIONS
F4-T3B1:
PRODUCT VERIFIED
CONTROL SUBSTITUTES AUTHORIZED
CLOSE PENDING SUBSTITUTE PASS
F4-T3B2:
PACKAGE v3 PREPARED
AUTHORIZED AFTER T3B1 CLOSE
F4-T3C:
WAITING
F4-T4:
WAITING
Smoke-2:
PENDING
F5-F7:
PENDING
