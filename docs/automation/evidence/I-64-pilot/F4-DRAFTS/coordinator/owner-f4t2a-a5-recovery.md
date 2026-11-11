I-64 — F4-T2A
A5 PLANNING RECOVERY DISPOSITION
Current branch:
36214bb820edb6d5e4777d16f28bd5170477e2ce
Current main:
812baf879612320653094a8aaf46aca0bac81383
Planning rejected:
R20261111T204805Z-13b0
Failure:
A5 = FAIL
P-03 STOP
Worker:
NOT INVOKED
F4-T2A attempts:
1 / 3
UNCHANGED
OPTION 1 AUTHORIZED.
Authorize exactly ONE replan of F4-T2A.
This is planning recovery only.
It does NOT consume a product attempt.
13b0 is:
REJECTED_BEFORE_INVOCATION
Reason:
A5 failed because INV-F4T2A-08 was not copied byte-identically from the
contract.
Observed normalization:
"lo leen"
became:
"los leen"
This is a Controller planning-output fidelity defect.
It is NOT:

* a contract defect;
* a scope defect;
* a product defect;
* an implementation defect.

Preserve 13b0 exactly as historical evidence.
Before the new planning invocation:
create a docs/evidence/state custody commit containing:

* 13b0 planning output;
* A1-A8 evaluation;
* nc4 result;
* STOP / P-03 classification;
* analysis.md explaining the byte-fidelity defect;
* this Coordinator disposition.

No product/source/test changes.
That docs-only commit becomes the new:
BaseSha
for the replan.
AuthorityRevision remains the same contract authority unless the custody
mechanics require a newer UNIT_DOC authority that contains this disposition.
Do not change the contract wording.
Use a NEW RunId.
Keep:

* same TaskId;
* same contract;
* same Objective;
* same AllowedWriteScope;
* same ForbiddenWriteScope;
* same RequiredTests;
* same StopConditions;
* same AcceptanceCriteria;
* same invariants.

The prompt must explicitly instruct:
All contract collections that are required to be copied into delegation.json
must be emitted byte-for-byte exactly as loaded.
Do NOT:

* improve grammar;
* normalize singular/plural;
* alter punctuation;
* change accents;
* rewrap wording semantically;
* paraphrase.

Before considering the planning output complete, require a deterministic
comparison between the generated delegation and the contract source.
At minimum compare byte-equivalent values for:

* invariants;
* acceptance criteria;
* required tests;
* stop conditions;
* authorities;
* exact scope collections where A5 requires identity.

The comparison must happen BEFORE delegation acceptance.
If any required copied string differs:
planning output = REJECTED
Do not invoke Worker.
Evaluate the replanned delegation normally.
Required:
A1-A8 = 8 PASS
No exception to A5.
Do not reinterpret grammatical equivalence as byte equivalence.
Run nc4 again.
Reason:
this is a new delegation after the previous real delegation was rejected
before Worker invocation.
Required relative oracle remains the normal one.
nc4 does not consume product attempts.
If and only if:

* replan terminates normally;
* schema valid;
* A1-A8 = 8 PASS;
* byte-fidelity check PASS;
* nc4 PASS under its relative oracle;
* BaseSha = HEAD = remote;
* config baseline accepted;
* no participant/process conflict;
* main-state rules satisfied;
* scope unchanged;

then:
F4-T2A Worker is AUTHORIZED ONCE.
No additional Owner approval is needed.
If the second planning output again fails A5 for any byte-fidelity reason:
STOP.
Do NOT:

* attempt a third planning run;
* rewrite the contract;
* accept semantic equivalence;
* invoke Worker.

Return exact evidence to Coordinator.
F4-T2A remains:
attempts = 1 / 3
The failed planning run does not consume an attempt.
Only a product correction after Worker execution may advance this counter.
F4-T1:
CLOSED / PASS
F4-T2A:
CONTRACT ISSUED
PLANNING 13b0 REJECTED
ONE REPLAN AUTHORIZED
attempts:
1 / 3
Worker:
NOT YET INVOKED
F4-T2B:
WAITING
F4-T2C:
WAITING
F4-T3/T4:
WAITING
Smoke-2:
PENDING
