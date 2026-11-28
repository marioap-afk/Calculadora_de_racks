I-64 — F4-T2A
CONTRACT COMPACTION / REISSUE DISPOSITION
Current branch:
0a563cfb78b4294e8440ab881962d40f6e5b0495
Current main:
812baf879612320653094a8aaf46aca0bac81383
Rejected planning runs:
13b0
A5 FAIL
REJECTED_BEFORE_INVOCATION
61f3
A4 FAIL
REJECTED_BEFORE_INVOCATION
Worker:
NEVER INVOKED
F4-T2A attempts:
1 / 3
UNCHANGED
Authorize OPTION 1.
Reissue the F4-T2A contract with:
IDENTICAL PRODUCT SEMANTICS
but a COMPACT ForbiddenWriteScope.
Do NOT change AUTOMATION_PLAN.
Do NOT change the Controller cell.
Do NOT accept 61f3.
Preserve:
R20261111T211626Z-61f3
as:
REJECTED_BEFORE_INVOCATION
P-03
A4 FAIL
Reason:
one ForbiddenWriteScope entry was omitted.
The missing entry:
src/RackCad.Application/Systems/PushBack/PushBackHighEndBeamGeometry.cs
must remain historical evidence of the failed planning output.
Do not repair delegation.json manually.
Before reissuing the contract:
create one docs-only custody commit containing:

* 61f3 planning output;
* byte-fidelity comparison;
* A1-A8 result;
* nc4 e9cb;
* STOP/P-03 classification;
* analysis of the omitted ForbiddenWriteScope entry;
* this Coordinator disposition.

No src/ changes.
No tests/ changes.
That custody commit becomes the new BaseSha / authority basis as required by
the normal workflow.
The reissued contract must preserve EXACTLY:

* Objective;
* product behavior;
* invariants;
* AcceptanceCriteria;
* RequiredTests;
* StopConditions;
* ExpectedEvidence;
* AllowedWriteScope;
* task decomposition;
* N-2 obligation;
* pilot semantics;
* all F4-T2A functional decisions.

Only the representation of:
ForbiddenWriteScope
may change.
Compact ForbiddenWriteScope using directory/prefix entries ONLY where the
entire covered subtree is forbidden.
A prefix is valid only if:
NO AllowedWriteScope path lies beneath it.
Do NOT introduce a prefix that covers an allowed candidate.
For mixed directories such as:
Systems/PushBack/
Workspace/
where allowed paths coexist with forbidden paths:
do NOT forbid the whole directory.
Use the minimum deterministic set of:

* safe forbidden prefixes;
* individual forbidden files only where necessary.

Before issuing the compacted contract, mechanically prove:
For every repository path relevant to the original contract universe:
OriginalForbidden(path)
CompactedForbidden(path)
except that redundant entries may disappear.
At minimum prove:

1. every original forbidden path is still forbidden;
2. every AllowedWriteScope path remains allowed;
3. no newly forbidden path intersects AllowedWriteScope;
4. no previously forbidden src/tests authority becomes writable;
5. the specific omitted path from 61f3 remains forbidden;
6. candidate paths preserve exactly their previous allowed/forbidden status.

Produce a deterministic equivalence report.
If equivalence cannot be proven:
STOP.
Treat the compacted contract as a NEW contract revision.
Do NOT claim byte identity with the previous contract.
Record explicitly:
semantic change:
NONE
representation change:
ForbiddenWriteScope compaction only
Reason:
reduce planning-copy fragility while preserving scope semantics.
After custody and contract reissue:
authorize ONE fresh planning invocation.
This is NOT a third retry of the old contract.
It is the first planning against the new compacted contract revision.
Use:
new RunId
same Controller cell:
gpt-6-luna / high
same routing/effort rules.
Retain the strengthened byte-copy instruction.
Require exact copy for all collections that remain literal authority.
Also retain the deterministic comparison before acceptance.
Do not relax A4 or A5.
Required:
A1-A8 = 8 PASS
No semantic-equivalence waiver.
The delegation must match the NEW contract exactly.
Run nc4 again against the new delegation.
Reason:
new contract revision
+
new delegation.
Required relative oracle remains unchanged.
If:

* contract semantic-equivalence proof PASS;
* planning terminates normally;
* A1-A8 = 8 PASS;
* nc4 satisfies its oracle;
* BaseSha / MainSha valid;
* config baseline accepted;
* tree clean;
* no participant conflict;

then:
F4-T2A Worker is AUTHORIZED ONCE.
No further Owner decision required.
F4-T2A remains:
attempts = 1 / 3
Neither 13b0 nor 61f3 consumed a product attempt because no Worker was
invoked.
Do not increment attempts merely for contract/planning recovery.
If planning against the COMPACTED contract again fails A4/A5 because the
Controller cannot reproduce the authority collections correctly:
STOP.
Do NOT:

* compact further automatically;
* change AUTOMATION_PLAN;
* switch Controller model automatically;
* manually patch delegation.json;
* invoke Worker.

Return to Coordinator.
At that point a protocol-level solution becomes justified.
Option 2 is NOT authorized now.
Mechanically injecting contract collections into delegation.json would alter
the planning protocol and A1-A8 authority model.
That is disproportionate while a simpler contract representation can remove
the observed copy-pressure without reducing safety.
Only reconsider protocol change if the compacted contract still cannot be
planned faithfully.
F4-T1:
CLOSED / PASS
F4-T2A:
CONTRACT REISSUE AUTHORIZED
attempts:
1 / 3
13b0:
REJECTED_BEFORE_INVOCATION
61f3:
REJECTED_BEFORE_INVOCATION
Worker:
NOT INVOKED
F4-T2B:
WAITING
F4-T2C:
WAITING
F4-T3/T4:
WAITING
Smoke-2:
PENDING
