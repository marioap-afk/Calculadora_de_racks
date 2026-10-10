# I-64 — Orden del Coordinator: EMERGENCY PROTOCOL BRIDGE / PHASE B

Transcripción literal de la orden recibida por el chat de la sesión responsable el 2026-10-10 (ejecutada en §35).

```text
I-64 — EMERGENCY PROTOCOL BRIDGE / PHASE B
TARGET_SHA:
51bb9b94d8b98a35cae2f87a5c31f41daa2f159e
Current main:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
A-1:
docs/initiatives/I-64-A-1.md
blob fb6a49c2e8e96689f82a2ad01fda7994fc2397b0
Independent review:
R20261010T012851Z-de0d
CHANGES_REQUIRED / NON_CONFORMING
2 REQUIRED
The two REQUIRED findings are ACCEPTED.
I64-A1-M04:
ACCEPTED REQUIRED.
I64-A1-I61-SUBSTITUTION:
ACCEPTED REQUIRED.
Do NOT edit A-1.
Create A-2.
The Owner's prior order:
"I-64 — EMERGENCY PARALLEL RESUMPTION
AUTHORITATIVE PROTOCOL BRIDGE"
explicitly authorized alternative (B) of I-64's recorded resume condition:
an authoritative amendment applicable only to I-64 allowing a conforming,
independently verifiable substitute for the failed Scope negative control,
without waiting for I-62 integration.
For this emergency path, record that order as the explicit Owner authority
for a UNIT-SCOPED PROTOCOL EXCEPTION.
This is OWNER authority over the exception.
It does NOT:

* modify I-61 normative text;
* create a general exception for another initiative;
* authorize I-62 changes;
* relax Scope;
* turn historical nc2 into PASS.

If an Architect concludes the exception cannot remain strictly unit-local
and would require changing global I-61 semantics, STOP back to Owner /
Coordinator.
Create:
docs/initiatives/I-64-A-2.md
Standard append-only amendment.
Amendment:
A-2
Initiative / Unit:
I-64
Freeze:
commit = 9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311
path = docs/initiatives/I-64-proposal-v5.md
blob = dc1924ff5a73a4e529fbc5f995621d29351a091b
Applies-to:
I-64
Corrects / supersedes for activation:
I-64-A-1
A-1 remains historical and immutable.
Authority:
Owner + Architect + Coordinator
Owner:
SATISFIED by the explicit emergency protocol-bridge order.
Architect:
REQUIRED because M-04 = YES.
Coordinator:
PENDING until Architect review is conforming.
Protocol-owner decision:
SATISFIED only for this I-64 unit-scoped exception by the Owner order above.
No global protocol authority is transferred to I-64.
External authority remains:
README §10 nc2:
Controller executes the mutated delegation and is expected to detect Scope.
AUTOMATION_PLAN §16.9 Scope invariant remains:
git diff --name-only BaseSha..CurrentSha
must be wholly covered by AllowedWriteScope
and must not intersect ForbiddenWriteScope.
A-2 DOES NOT change that invariant.
It changes only how I-64 may independently demonstrate the negative
control when the Controller nc2 mechanism itself was proven unreliable.
For I-64 only:
The historical Controller-based nc2 remains:
FAIL
It is never reclassified.
For closure evidence, a task MAY satisfy the nc2 obligation through
I64-SCOPE-BRIDGE-01 if and only if all of these are true:

1. normal Controller Scope verification on the real delivery is PASS;
2. historical or task-specific Controller nc2 evidence is preserved;
3. ScopeBridge REAL = PASS;
4. ScopeBridge ADVERSARIAL = FAIL;
5. ADVERSARIAL failure is exclusively the expected Scope violation;
6. exact changed path excluded from AllowedWriteScope is identified;
7. verifier identity matches the blob authorized by the applicable A-n;
8. any parse/Git/evidence ambiguity fails closed;
9. nc1, nc3, nc4 and every other applicable I-61 obligation remain unchanged;
10. no budget is reset.

This is a SUBSTITUTE EVIDENCE MECHANISM for the nc2 closure obligation,
not a PASS result for the old nc2 execution.
Set:
M-01 = NO
M-02 = NO
M-03 = NO
M-04 = YES
M-05 = NO
M-06 = NO
M-07 = NO
M-08 = NO
M-04 rationale:
The amendment changes the failure/closure semantics of the delegated
execution protocol as applied to I-64 by permitting a mechanical
fail-closed substitute to satisfy an otherwise mandatory Controller nc2
negative-control obligation.
It does NOT change product failure semantics.
Because M-04 = YES:
Architect + Coordinator agreement are mandatory.
A-2 may adopt the existing A-1 verifier unchanged:
docs/automation/evidence/I-64-protocol-bridge/tools/verify_scope_bridge.py
blob:
093a570f71a359e8e004b8983ea9625e74ebbc87
SHA-256:
327982a9f97335e23afecce00a08f1b1a3e90aef5da7fa087fcd1863878b8bca
Self-test:
23/23 PASS
Do NOT change the verifier unless the Architect identifies a defect.
If it changes:
new blob + repeat self-test + Architect reviews the new exact object.
Run ONE formal independent Architect review of A-2.
This is NOT the previous lightweight conformance review.
Profile:
ARCHITECTURE_REVIEW
Semantic effort:
Deep
Use an eligible measured read-only transport.
Important:
do not silently route down this time.
If there is no eligible measured cell for ARCHITECTURE_REVIEW / Deep:
STOP at:
ARCHITECT_CELL_REQUIRED
and return the eligible-cell facts to Coordinator.
Do NOT invent eligibility.
The review object includes:

* Freeze V5 exact commit/path/blob;
* A-1 exact commit/blob;
* A-2 exact commit/blob;
* verifier exact blob;
* self-test evidence;
* historical nc2 evidence;
* independent review de0d and both REQUIRED;
* Owner emergency order;
* governing LIFECYCLE §6;
* AUTOMATION_PLAN §16;
* README §10.

Architect must answer explicitly:

1. Does A-2 correctly classify M-04 = YES?
2. Is Owner authority sufficient for a unit-scoped I-64 exception without
modifying global I-61 text?
3. Does A-2 preserve the normative Scope invariant exactly?
4. Does ScopeBridge provide evidence at least as fail-closed as nc2 for the
Scope invariant?
5. Can historical nc2 remain FAIL while closure is supplied by independent
substitute evidence without contradiction?
6. Is the exception genuinely Applies-to I-64 only?
7. Is any authority from I-62 required?
8. Does A-2 create any hidden M-01..08 besides M-04?
9. Are the REAL and ADVERSARIAL oracles sufficient?
10. Is there any fail-open path or ambiguity requiring correction?

Required verdict:
AGREED
0 REQUIRED
OPTIONAL findings may be dispositioned later.
If any REQUIRED:
STOP to Coordinator.
Even if Architect AGREES:
DO NOT run ScopeBridge against T1 yet.
Do not rebase yet.
Do not start T2.
Custody the Architect result and stop at:
COORDINATOR_A2_AGREEMENT_REQUIRED
Return:

* A-2 commit/blob;
* Architect RunId;
* exact model/effort/transport;
* Architect verdict;
* REQUIRED count;
* OPTIONAL count;
* CI of A-2/evidence commit;
* whether verifier changed;
* M-01..08.

Do not route down silently.
Return:
ARCHITECT_CELL_REQUIRED
with:

* available measured cells;
* missing capability/effort;
* whether a one-time read-only measurement is technically possible;
* whether it requires Owner authorization under current routing rules.

Do not launch that measurement without authorization.
No:

* ScopeBridge execution against T1;
* T1 product changes;
* old nc2 retry;
* T2 planning or implementation;
* I-61 modification;
* I-62 modification;
* rebase;
* F1 PASS;
* T1 COMPLETE.

Start by writing A-2 and determining the exact eligible Architect cell.
```
