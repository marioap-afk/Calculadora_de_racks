# I-64 — Decisión del Owner: A-5 / recuperación del control FreeText (I64-FREETEXT-BRIDGE-01)

Transcripción literal recibida por el canal del Owner (chat de la sesión responsable) el 2026-10-11, tras la parada
OWNER_DECISION_REQUIRED fijada por la orden del Coordinator «AUTHORITATIVE FREETEXT CONTROL RECOVERY»
(`coordinator-order-a5-freetext.md`).

```text
OWNER DECISION — I-64 / A-5 / FREETEXT CONTROL RECOVERY
For I-64 only, I authorize a unit-scoped mechanical substitute for the FreeText negative-control obligation after all of the following facts have been established:

* the real F1-T2-BRIDGE delivery was independently EXECUTION_VERIFIED / NONE;
* all permitted model-based nc3 executions were exhausted;
* the model consistently detected the injected FreeText violation correctly;
* FailureClass was consistently FreeText;
* the remaining verification checks preserved the expected relationship to the real verification;
* the model repeatedly transformed the synthetic REWORK result into STOP by applying the real task's correction/attempt limits;
* nc3 therefore cannot complete its required oracle through the currently authorized model-based mechanism.

I authorize creation of:
I-64-A-5
and of a deterministic, fail-closed mechanical substitute:
I64-FREETEXT-BRIDGE-01
subject to Architect and Coordinator approval before use.
This authority:

* applies ONLY to I-64;
* applies only to the FreeText negative-control obligation described by A-5;
* does NOT modify the normative text of I-61;
* does NOT make any historical nc3 execution PASS;
* does NOT waive or weaken FreeText enforcement;
* does NOT authorize accepting only the discriminating portion of an otherwise non-conforming nc3 result;
* does NOT apply to Identity, Scope, or any other negative control;
* does NOT reset attempts, correction budgets, invocation budgets, or other exhausted limits;
* does NOT authorize another Worker execution;
* does NOT authorize product changes;
* does NOT authorize F1-T3-HOST;
* does NOT authorize a global protocol exception for another initiative;
* expires for I-64 when an integrated authoritative protocol revision supersedes this unit-scoped exception.

The substitute must mechanically prove, at minimum, that:

1. the real verified handoff is clean under the complete FreeText rule of AUTOMATION_PLAN §16.10;
2. the mutated handoff differs from the real handoff only by the authorized nc3 mutation;
3. the mutation introduces a forbidden FreeText term in a governed field;
4. the exact T2 historical mutation is the append of " GATE PASS." to WorkCompleted;
5. all identity, Git-related, file-list and non-FreeText fields remain unchanged;
6. malformed, ambiguous or incomplete evidence fails closed;
7. the authoritative §16.10 term set and matching semantics are used in full, not a reduced local interpretation.

Authority sequence:
Owner = SATISFIED by this decision.
Architect = REQUIRED because M-04 = YES.
Coordinator = REQUIRED after Architect AGREED / 0 REQUIRED.
Until those approvals exist:

* A-5 is not active;
* I64-FREETEXT-BRIDGE-01 must not be executed against T2;
* T2 remains protocol-incomplete;
* F1-T3-HOST remains blocked.

Proceed with:
A-5
→ mechanical verifier
→ reproducible self-tests
→ exact commit + CI
→ independent ARCHITECTURE_REVIEW / Deep
→ stop at COORDINATOR_A5_AGREEMENT_REQUIRED.
```
