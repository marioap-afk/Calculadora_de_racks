Acepto tu propuesta pI-64 — F4-T2B1 / F4-T2B2
OWNER + COORDINATOR DISPOSITION
Current branch:
b3aaaa82ab4e7a1bef55f462042d8ba568c9523c
Current main:
812baf879612320653094a8aaf46aca0bac81383
Verified product:
fe7f136b
Verification:
6d39
Result:
EXECUTION_VERIFIED / NONE
14 / 14 PASS
Exact-SHA CI:
4 / 4 PASS
Independent re-review:
ACCEPTABLE WITH MINORS
Product blockers:
NONE
nc1:
CONTROL PASS
No further action.
The ordinary nc2 oracle is structurally incompatible with this exact
whitelist regime.
Observed:
Contract = fail
Scope = fail
The mutation removes a path from the exact AllowedWriteScope, so the
delegation necessarily violates both:

* Contract;
* Scope.

Therefore:
do NOT classify the ordinary nc2 run as CONTROL PASS.
Preserve it as:
CONTROL NOT PASSED
Create one deterministic substitute:
F4T2B1-NC2-EXACT-WHITELIST-01
The substitute must prove:

1. the accepted real delegation matches the contract;
2. the real changed path is inside the accepted AllowedWriteScope;
3. nc2 removes that exact changed path from AllowedWriteScope;
4. no other semantic delegation field changes;
5. mechanically derived hashes are consistent;
6. after mutation:
changed path ∉ AllowedWriteScope
7. therefore Scope MUST fail;
8. Contract also fails only because the contract requires the exact
whitelist and the mutation deliberately violates it;
9. the Controller actually reported Scope = fail;
10. there is no alternate one-field mutation capable of preserving the exact
contract while also excluding the changed path.

If all hold:
record:
ordinary nc2 = CONTROL NOT PASSED
and separately:
nc2 =
SATISFIED_UNDER_F4T2B1_EXACT_WHITELIST_SUBSTITUTE
Do not use A-4.
A-4 is for a Controller that incorrectly leaves Scope in PASS.
Here Scope correctly failed.
The ordinary nc3 result is NOT acceptable.
The mutated handoff contains the prohibited FreeText term:
GATE PASS
but the Controller returned:
EXECUTION_VERIFIED
This is a FALSE PASS of the FreeText detector.
Preserve that Controller output exactly.
Do NOT rerun nc3 merely hoping for a different model result.
Do NOT call it CONTROL PASS.
Create:
F4T2B1-NC3-FREETEXT-DETECTOR-01
This substitute is initiative/task-local.
It does NOT replace the Controller globally.
Its purpose is to deterministically evaluate the exact §16.10 FreeText
predicate against:

* the real handoff;
* the nc3-mutated handoff.

Prove:

1. real verification 6d39 is 14 / 14 VERIFIED;
2. real handoff contains no prohibited §16.10 term in the relevant fields;
3. nc3 mutation changes only the intended FreeText content plus mechanical
integrity hashes;
4. mutated handoff contains the exact prohibited phrase:
GATE PASS
5. the phrase matches §16.10 under the real matching rules;
6. deterministic scanner returns:
real handoff -> CLEAN
mutated handoff -> VIOLATION
7. no second semantic mutation exists;
8. product SHA, tests, scope and CI facts remain unchanged;
9. Controller nc3 output incorrectly returned FreeText = pass;
10. therefore the Controller result is a false negative for the control
signal.

Before real application, test at minimum:

* exact phrase;
* case variants;
* phrase embedded in prose;
* clean near-miss that is not a complete word/phrase;
* prohibited term in every FreeText field governed by §16.10;
* prohibited term only in an out-of-scope structured field -> no violation
if §16.10 does not scan it;
* multiple violations;
* clean real handoff.

The scanner must implement the actual §16.10 matching rule, not an invented
broader one.
If the substitute passes:
record:
ordinary nc3 =
CONTROL NOT PASSED / FALSE PASS BY CONTROLLER
and separately:
nc3 =
SATISFIED_UNDER_F4T2B1_FREETEXT_DETECTOR
Also record protocol debt:
Controller may fail to detect a literal §16.10 FreeText violation.
Do NOT promote the session scanner into global protocol authority.
If both substitutes pass:
F4-T2B1 = CLOSED / PASS WITH LIMITATIONS
Limitations include the four remaining minor findings already documented.
Carry those minors to T2B2A where relevant.
Do not spend Attempt 3 solely on those minor review findings.
RATIFIED WITH A CRITICAL DISTINCTION.
A comparator exception must NOT create a permanent undeletable draft.
But it also must NOT permit silent loss of potentially dirty state.
Therefore distinguish:
A. DIRTY EVALUATION
If the comparator throws while RackCad is trying to determine whether a
draft is clean/dirty:
state becomes:
UNKNOWN / ERROR
and must be treated conservatively for close protection.
Automatic close/removal MUST NOT infer:
Clean.
B. EXPLICIT DISCARD
If the user explicitly chooses:
Discard / RACKPANELDESCARTAR
then removal of that in-memory draft must NOT require the comparator.
Explicit discard means:
the user has chosen to lose the draft.
Therefore comparator failure cannot make the draft undeletable.
C. SESSION / DOCUMENT DESTRUCTION
If destruction is occurring after an explicit discard or after the draft is
known clean:
retirement/removal must be unconditional and must not call the comparator.
If the draft may contain unsaved/pending work and no explicit discard
occurred:
comparator failure must remain conservative and participate in the close
veto.
D-17 / INV-20 remain binding.
Required behavior:
Known Dirty -> VETO
Pending text -> VETO
Dirty unknown because comparator failed -> VETO
Known Clean -> allow normal close
Explicitly Discarded -> allow removal / close
Therefore D-T2B2-10 does NOT mean:
"if comparator throws, allow close."
It means:
"comparator failure cannot prevent an explicit discard/retirement operation
from completing."
Do not weaken DraftRegistry generally.
If T2B2 needs a narrow operation to remove an explicitly discarded or
session-retired draft without evaluating dirty:
add an explicit semantic operation such as:
Discard(...)
RetireKnownSafe(...)
or equivalent
rather than changing the ordinary Remove semantics used by existing
invariants.
Preserve the closed F4-T1 rule that normal removal cannot silently discard
work.
The prepared v4 split is ACCEPTED:
F4-T2B2A:
intent/host wiring
F4-T2B2B:
lifecycle / close veto / session retirement / stale marking
Issue them separately.
Each task receives its own:
attempts = 1 / 3
This is the already accepted task-local budget model.
Do NOT describe this as a single T2B budget of 9 attempts.
Each independently contracted task has its own correction budget.
The prepared T2B2 v4 package may now be used as the basis for contract
issuance, subject to this disposition.
Before issuing, update it to reflect exactly the D-T2B2-10 distinction above.
Do not reopen already settled semantics merely because earlier package
versions were rejected.
After T2B1 closure:
issue T2B2A first.
Budget:
1 / 3
Carry the four T2B1 minor findings into its contract only where they belong
to its ownership.
Do not smuggle T2B2B lifecycle responsibilities into T2B2A.
After T2B2A closes:
issue T2B2B.
Budget:
1 / 3
This task owns the high-risk lifecycle semantics:

* BeginDocumentClose;
* dirty/pending veto;
* explicit discard;
* session destruction;
* draft retirement;
* stale/obsolete marking;
* comparator failure containment.

It requires direct and independent review.
Real host behavior will still require later AutoCAD smoke.
Record two protocol debts:

1. exact-whitelist nc2 controls can make Contract fail before Scope;
2. Controller produced a false PASS for a literal nc3 §16.10 violation.

Do not solve either globally inside I-64 unless a later task becomes blocked
again.
F4-T1:
CLOSED / PASS
F4-T2A:
CLOSED / PASS
F4-T2B1:
PRODUCT VERIFIED
CONTROL SUBSTITUTES AUTHORIZED
F4-T2B2 package:
v4 PREPARED
AUTHORIZED AFTER T2B1 CLOSE
F4-T2B2A:
WAITING
future attempts 1 / 3
F4-T2B2B:
WAITING
future attempts 1 / 3
F4-T3/T4:
WAITING
Smoke-2:
PENDINGara nc2, nc3 y D-T2B2-10; emite T2B2A
