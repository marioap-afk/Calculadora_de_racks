I-64 — F4-T2A
PLANNING ACCEPTANCE DISPOSITION
Current branch:
ed680e1f2eb780b6ead4a4c8a0c06ba17444f0cb
Current main:
812baf879612320653094a8aaf46aca0bac81383
Planning:
R20261111T215420Z-981d
nc4:
6aea
Worker:
NOT YET INVOKED
F4-T2A attempts:
1 / 3
ACCEPT planning 981d.
Authorize the F4-T2A Worker ONCE.
No further planning invocation is required.
Grant exactly ONE planning-output fidelity waiver for:
AcceptanceCriterion 18
Source text:
"conforme a su esquema"
981d text:
"conforme al esquema"
This difference is accepted as:
GRAMMATICAL_NORMALIZATION_ONLY
It does NOT change the criterion's technical meaning.
All actual contract authority collections required by the planning gate are
correct.
Measured:
A1-A8 = 8 PASS
nc4 = expected relative oracle
and byte-identical contract fidelity for:

* Objective;
* 15 invariants;
* AllowedWriteScope, exact elements and order;
* 57 ForbiddenWriteScope entries;
* Authorities;
* RequiredTests;
* StopConditions;
* ExpectedEvidence.

17 of 18 AcceptanceCriteria are also byte-identical.
The only difference is the grammatical replacement:
"su esquema"
->
"el esquema"
No scope, authority, test, behavior or safety requirement changes.
Preserve the strict comparison result:
981d = DIFFERENCE DETECTED
Do NOT rewrite it as byte-identical.
Record separately:
Coordinator waiver:
criterion 18 grammatical normalization accepted.
The strict checker remains correct.
The Coordinator is overriding its disposition for this one known difference.
This waiver applies ONLY to:
F4-T2A
planning 981d
AcceptanceCriterion 18
the exact text pair above.
It does NOT authorize:

* grammatical normalization of invariants;
* omitted list entries;
* reordered collections;
* scope changes;
* paraphrased requirements;
* future planning differences.

Do not generalize this into:
"semantic equivalence is enough".
Preserve:
13b0:
REJECTED_BEFORE_INVOCATION
A5 FAIL
61f3:
REJECTED_BEFORE_INVOCATION
A4 FAIL
981d:
ACCEPTED_BY_COORDINATOR_WITH_ONE_TEXTUAL_WAIVER
These three planning runs demonstrate distinct behaviors:
13b0:
invariant rewritten
61f3:
scope entry omitted
981d:
all contractual collections correct;
one non-contract acceptance sentence grammatically normalized.
Only the third is accepted.
The reduced-scope contract remains authoritative.
ForbiddenWriteScope:
57 entries
AllowedWriteScope:
unchanged exact whitelist
The 82 removed explicit denies remain protected through AllowedWriteScope
enforcement as already proven across the repository path universe.
No further contract revision.
F4-T2A Worker is AUTHORIZED ONCE.
Use the accepted delegation from:
981d
Do NOT manually edit delegation.json.
The Worker receives the delegation exactly as emitted.
The Worker must implement only the contracted F4-T2A responsibilities.
In particular:

* Application-only pilot state;
* Push Back bounded pilot;
* N-2 anti-aliasing protection;
* semantic copy/comparison behavior;
* pending-text state required by the contract;
* exact field set already contracted.

No F4-T2B host work.
No F4-T2C IC-04 host work.
No T3 UI.
No T4 mutation.
The Worker must prove that host/source-owned mutable objects cannot alter:
InitialDraftState
after draft creation.
Required adversarial behavior remains:
mutate/reuse source object after DraftSession creation
->
InitialDraftState unchanged
and dirty semantics remain correct.
Do not weaken this test.
After Worker delivery:

1. exact RED/ GREEN evidence as contracted;
2. exact-SHA CI;
3. direct session code review;
4. independent adversarial review;
5. Controller verification;
6. nc1/nc2/nc3;
7. custody;
8. close F4-T2A only if all required evidence closes.

Given the defects found in prior tasks:
Controller 14/14 is necessary but NOT sufficient.
Independent review remains mandatory.
F4-T2A remains:
attempts = 1 / 3
The three planning runs do not consume product attempts.
Only a product correction after Worker execution increments the task-local
budget.
If the Worker finds a real ambiguity that changes product semantics:
STOP.
Do not reinterpret the contract automatically.
If the question is merely implementation detail within the frozen behavior,
choose the conservative implementation and continue.
F4-T1:
CLOSED / PASS
F4-T2A:
PLANNING ACCEPTED
WORKER AUTHORIZED ONCE
Planning:
981d
nc4:
6aea
attempts:
1 / 3
F4-T2B:
WAITING
F4-T2C:
WAITING
F4-T3/T4:
WAITING
Smoke-2:
PENDING
